using System;
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

        public Coroutine PopupRoutine = new();

        private MTexture Icon;

        private MTexture MedalIcon;

        private MTexture buttonTexture;

        VirtualButton Button = new();

        private string Name;

        private string Description;

        public LorebookData LorebookData;

        private string MedalsValue;

        private float alpha;

        private bool renderAchievement;

        public bool renderLorebook;

        private float delayTimer;

        public Popup()
        {
            Tag = (Tags.HUD | Tags.Persistent | Tags.PauseUpdate | Tags.TransitionUpdate);
            Position = new Vector2(0f, Engine.Height - 149f);
            MedalIcon = GFX.Gui["achievements/medal"];
            ButtonBinding Control = XaphanModule.ModSettings.OpenMap;
            Button.Binding = Control.Binding;
            buttonTexture = Input.GuiButton(Button, "controls/keyboard/oemquestion");
            Visible = false;
            Depth = -1000000;
        }

        public override void Added(Scene scene)
        {
            base.Added(scene);
            achievements = Achievements.GenerateAchievementsList(SceneAs<Level>().Session);
            lorebookEntries = LorebookEntries.GenerateLorebookEntriesDataList();
        }

        public override void Update()
        {
            base.Update();
            Level level = SceneAs<Level>();
            Player player = level.Tracker.GetEntity<Player>();
            if (level.Transitioning || (player != null && player.StateMachine.State == Player.StDummy))
            {
                delayTimer = 0.1f;
            }
            else if (delayTimer > 0f)
            {
                delayTimer -= Engine.DeltaTime;
            }
            if (!PopupRoutine.Active && delayTimer <= 0f && XaphanModule.ModSaveData.CanDisplayPopups)
            {
                foreach (AchievementData achievement in achievements)
                {
                    if (level.Session.GetFlag(achievement.Flag) && !XaphanModule.ModSaveData.Achievements.Contains(achievement.AchievementID))
                    {
                        XaphanModule.ModSaveData.Achievements.Add(achievement.AchievementID);
                        if (XaphanModule.ModSettings.ShowAchievementsPopups)
                        {
                            Add(PopupRoutine = new Coroutine(DisplayAchievementPopup(achievement)));
                        }
                        break;
                    }
                }
                if (XaphanModule.SoCMVersion >= new Version(3, 0, 0))
                {
                    if (!PopupRoutine.Active)
                    {
                        foreach (LorebookData entry in lorebookEntries)
                        {
                            if (level.Session.GetFlag(entry.Flag) && !XaphanModule.ModSaveData.LorebookEntries.Contains(entry.EntryID))
                            {
                                XaphanModule.ModSaveData.LorebookEntries.Add(entry.EntryID);
                                if (XaphanModule.ModSettings.ShowLorebookPopups)
                                {
                                    Add(PopupRoutine = new Coroutine(DisplayLorebookPopup(entry)));
                                }
                                break;
                            }
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
            LorebookData = data;
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
                position.Y += 20f;
            }
            Draw.Rect(position, renderLorebook ? 450f : 750f, renderLorebook ? 129f : 149f, Color.Black * alpha);
            Draw.Rect(position, renderLorebook ? 450f : 750f, 5f, Color.Gold * alpha);
            Draw.Rect(position + Vector2.UnitY * 5f, 5f, renderLorebook ? 119f : 139f, Color.Gold * alpha);
            Draw.Rect(position + new Vector2(renderLorebook ? 445f : 745f, 5f), 5f, renderLorebook ? 119f : 139f, Color.Gold * alpha);
            Draw.Rect(position + Vector2.UnitY * (renderLorebook ? 124f : 144f), renderLorebook ? 450f : 750f, 5f, Color.Gold * alpha);
            Icon.Draw(position + Vector2.One * 7f + (renderLorebook ? new Vector2(20f, 15f) : Vector2.Zero), Vector2.Zero, Color.White * alpha, 0.9f);
            if (renderLorebook)
            {
                float loreLength = ActiveFont.Measure(Dialog.Clean("XaphanHelper_UI_LorebookNewEntry")).X * 0.6f;
                float nameLenght = ActiveFont.Measure(Name).X * 0.4f;
                float infoPressLength = ActiveFont.Measure(Dialog.Clean("XaphanHelper_Press")).X * 0.4f;
                float infoOpenLength = ActiveFont.Measure(Dialog.Clean("XaphanHelper_UI_LorebookOpen")).X * 0.4f;
                ActiveFont.DrawOutline(Dialog.Clean("XaphanHelper_UI_LorebookNewEntry"), position + new Vector2(114f + loreLength / 2 - 10, 35f), new Vector2(0.5f, 0.5f), Vector2.One * 0.6f, Color.White * alpha, 2f, Color.Black * alpha);
                ActiveFont.DrawOutline(Name, position + new Vector2(114f + nameLenght / 2 - 10, 65f), new Vector2(0.5f, 0.5f), Vector2.One * 0.4f, Color.Gray * alpha, 2f, Color.Black * alpha);
                if (buttonTexture != null)
                {
                    float x = 27f;
                    float buttonWidth = buttonTexture.Width * 0.4f;
                    ActiveFont.DrawOutline(Dialog.Clean("XaphanHelper_Press"), position + new Vector2(x, 95f), new Vector2(0f, 0.5f), Vector2.One * 0.4f, Color.Gray * alpha, 2f, Color.Black * alpha);
                    x += infoPressLength + 5f;
                    buttonTexture.DrawCentered(position + new Vector2(x + buttonWidth / 2, 95f), Color.White * alpha, 0.4f);
                    x += buttonWidth + 5f;
                    ActiveFont.DrawOutline(Dialog.Clean("XaphanHelper_UI_LorebookOpen"), position + new Vector2(x, 95f), new Vector2(0f, 0.5f), Vector2.One * 0.4f, Color.Gray * alpha, 2f, Color.Black * alpha);
                }
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
