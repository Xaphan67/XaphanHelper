using System.Collections;
using System.Collections.Generic;
using Celeste.Mod.XaphanHelper.Data;
using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste.Mod.XaphanHelper.UI_Elements
{
    [Tracked(true)]
    public class Popup : Entity
    {
        private List<AchievementData> achievements;

        private List<LorebookData> lorebookEntries;

        private Coroutine PopupRoutine = new();

        private MTexture Icon;

        private MTexture MedalIcon;

        private string Name;

        private string Description;

        private string MedalsValue;

        private float alpha;

        private bool renderAchievement;

        private bool renderLorebook;

        public Popup()
        {
            Tag = (Tags.HUD | Tags.Persistent | Tags.PauseUpdate | Tags.TransitionUpdate);
            Position = new Vector2(0f, Engine.Height - 149f);
            MedalIcon = GFX.Gui["achievements/medal"];
            Visible = false;
            Depth = -1000000;
        }

        public override void Added(Scene scene)
        {
            base.Added(scene);
            achievements = Achievements.GenerateAchievementsList(SceneAs<Level>().Session);
            lorebookEntries = LorebookEntries.GenerateLorebookEntriesDataList(SceneAs<Level>().Session);
        }

        public override void Update()
        {
            base.Update();
            if (!PopupRoutine.Active && XaphanModule.ModSaveData.CanDisplayPopups)
            {
                foreach (AchievementData achievement in achievements)
                {
                    if (SceneAs<Level>().Session.GetFlag(achievement.Flag) && !XaphanModule.ModSaveData.Achievements.Contains(achievement.AchievementID))
                    {
                        XaphanModule.ModSaveData.Achievements.Add(achievement.AchievementID);
                        Add(PopupRoutine = new Coroutine(DisplayAchievementPopup(achievement)));
                        break;
                    }
                }
                if (!PopupRoutine.Active)
                {
                    foreach (LorebookData entry in lorebookEntries)
                    {
                        if (SceneAs<Level>().Session.GetFlag(entry.Flag) && !XaphanModule.ModSaveData.LorebookEntries.Contains(entry.EntryID))
                        {
                            XaphanModule.ModSaveData.LorebookEntries.Add(entry.EntryID);
                            Add(PopupRoutine = new Coroutine(DisplayLorebookPopup(entry)));
                            break;
                        }
                    }
                }
            }
        }

        private IEnumerator DisplayAchievementPopup(AchievementData data)
        {
            renderAchievement = true;
            renderLorebook = false;
            Icon = GFX.Gui[data.Icon];
            Name = Dialog.Clean(data.Name);
            Description = Dialog.Clean(data.Description);
            MedalsValue = "+ " + data.Medals.ToString();
            Audio.Play("event:/game/02_old_site/theoselfie_photo_filter");
            float popupTime = 5f;
            while (popupTime > 0)
            {
                if (popupTime <= 1f)
                {
                    alpha = popupTime;
                }
                else if (popupTime >= 4f)
                {
                    alpha = 5f - popupTime;
                }
                Visible = true;
                popupTime -= Engine.DeltaTime;
                yield return null;
            }
            Visible = false;
        }

        private IEnumerator DisplayLorebookPopup(LorebookData data)
        {
            renderAchievement = false;
            renderLorebook = true;
            Icon = GFX.Gui["common/lorebookScreen"];
            Name = Dialog.Clean(data.Name);
            Audio.Play("event:/game/02_old_site/theoselfie_photo_filter");
            float popupTime = 5f;
            while (popupTime > 0)
            {
                if (popupTime <= 1f)
                {
                    alpha = popupTime;
                }
                else if (popupTime >= 4f)
                {
                    alpha = 5f - popupTime;
                }
                Visible = true;
                popupTime -= Engine.DeltaTime;
                yield return null;
            }
            Visible = false;
        }

        public override void Render()
        {
            base.Render();
            Vector2 position = Position;
            if (renderLorebook)
            {
                position.Y += 49f;
            }
            Draw.Rect(position, renderLorebook ? 450f : 750f, renderLorebook ? 100f : 149f, Color.Black * alpha);
            Draw.Rect(position, renderLorebook ? 450f : 750f, 5f, Color.Gold * alpha);
            Draw.Rect(position + Vector2.UnitY * 5f, 5f, renderLorebook ? 90f : 139f, Color.Gold * alpha);
            Draw.Rect(position + new Vector2(renderLorebook ? 445f : 745f, 5f), 5f, renderLorebook ? 90f : 139f, Color.Gold * alpha);
            Draw.Rect(position + Vector2.UnitY * (renderLorebook ? 95f : 144f), renderLorebook ? 450f : 750f, 5f, Color.Gold * alpha);
            Icon.Draw(position + Vector2.One * 7f + (renderLorebook ? Vector2.One * 20f : Vector2.Zero), Vector2.Zero, Color.White * alpha, 0.9f);
            if (renderLorebook)
            {
                float loreLength = ActiveFont.Measure(Dialog.Clean("XaphanHelper_UI_LorebookNewEntry")).X * 0.6f;
                float nameLenght = ActiveFont.Measure(Name).X * 0.4f;
                ActiveFont.DrawOutline(Dialog.Clean("XaphanHelper_UI_LorebookNewEntry"), position + new Vector2(114f + loreLength / 2 - 10, 35f), new Vector2(0.5f, 0.5f), Vector2.One * 0.6f, Color.White * alpha, 2f, Color.Black * alpha);
                ActiveFont.DrawOutline(Name, position + new Vector2(114f + nameLenght / 2 - 10, 65f), new Vector2(0.5f, 0.5f), Vector2.One * 0.4f, Color.Gray * alpha, 2f, Color.Black * alpha);
            }
            if (renderAchievement)
            {
                float lenght = ActiveFont.Measure(Name).X * 0.6f;
                float descHeight = ActiveFont.Measure(Description).Y * 0.4f;
                ActiveFont.DrawOutline(Name, position + new Vector2(167f + lenght / 2 - 10, 50f - descHeight / 2), new Vector2(0.5f, 0.5f), Vector2.One * 0.6f, Color.White * alpha, 2f, Color.Black * alpha);
                ActiveFont.DrawOutline(Description, position + new Vector2(158f, 70f - descHeight / 2), Vector2.Zero, Vector2.One * 0.4f, Color.Gray * alpha, 2f, Color.Black * alpha);
                ActiveFont.DrawOutline(MedalsValue, position + new Vector2(158f, 70f + descHeight / 2 + 5f), Vector2.Zero, Vector2.One * 0.5f, Color.Gold * alpha, 2f, Color.Black * alpha);
                lenght = ActiveFont.Measure(MedalsValue).X * 0.5f;
                MedalIcon.Draw(position + new Vector2(159f + lenght + 10f, 70f + descHeight / 2), Vector2.Zero, Color.White * alpha, 0.35f);
            }
        }
    }
}
