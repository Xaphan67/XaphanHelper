using Celeste.Mod.XaphanHelper.Data;
using Microsoft.Xna.Framework;
using Monocle;
using System;
using System.Collections;

namespace Celeste.Mod.XaphanHelper.UI_Elements
{
    public class LorebookDisplay : Entity
    {
        [Tracked(true)]
        public class CategoryDisplay : Entity
        {
            public int ID;

            public float width = 400f;

            public float height = 155f;

            private string Name;

            private string Discovered;

            private LorebookScreen LorebookScreen;

            public bool Selected;

            private bool ShowArrow;

            public bool Locked;

            private bool AllLogs;

            private float selectedAlpha = 0;

            private int alphaStatus = 0;

            public CategoryDisplay(Level level, Vector2 position, int id, string name, bool noDialog = false) : base(position)
            {
                Tag = Tags.HUD;
                LorebookScreen = level.Tracker.GetEntity<LorebookScreen>();
                ID = id;
                Name = noDialog ? name : Dialog.Clean(name);

                int discoveredLogs = 0;
                int totalLogs = 0;
                /*foreach (AchievementData achievement in data)
                {
                    if (level.Session.GetFlag(achievement.Flag))
                    {
                        completedAchievements++;
                    }
                    if (achievement.Hidden && achievement.CurrentValue == 0)
                    {
                        hiddenAchievements++;
                        totalAchievements--;
                    }
                    totalAchievements++;
                }*/
                Discovered = $"{discoveredLogs} / {totalLogs} {Dialog.Clean("XaphanHelper_UI_Discovered")}";
                AllLogs = discoveredLogs == totalLogs;
                Locked = Name == "???";
                Depth = -10001;
            }

            public override void Update()
            {
                base.Update();
                if (LorebookScreen.categorySelection == ID)
                {
                    Selected = true;
                    ShowArrow = false;
                    if (alphaStatus == 0 || (alphaStatus == 1 && selectedAlpha != 0.9f))
                    {
                        alphaStatus = 1;
                        selectedAlpha = Calc.Approach(selectedAlpha, 0.9f, Engine.DeltaTime);
                        if (selectedAlpha == 0.9f)
                        {
                            alphaStatus = 2;
                        }
                    }
                    if (alphaStatus == 2 && selectedAlpha != 0.1f)
                    {
                        selectedAlpha = Calc.Approach(selectedAlpha, 0.1f, Engine.DeltaTime);
                        if (selectedAlpha == 0.1f)
                        {
                            alphaStatus = 1;
                        }
                    }
                }
                else
                {
                    Selected = false;
                    if (LorebookScreen.previousCategorySelection == ID)
                    {
                        ShowArrow = true;
                    }
                }
            }

            public override void Render()
            {
                base.Render();
                MTexture mTexture = GFX.Gui["towerarrow"];
                if (Selected)
                {
                    Draw.Rect(Position, width, height, Color.Yellow * selectedAlpha);
                }
                else if (LorebookScreen.previousCategorySelection == ID)
                {
                    Draw.Rect(Position, width, height, Color.DarkGreen * 0.7f);
                }
                ActiveFont.DrawOutline(Name, Position + new Vector2(width / 2, Locked ? height / 2 : 41f), new Vector2(0.5f, 0.5f), Vector2.One * 0.8f, Name != "???" ? (AllLogs ? Color.Gold : Color.White) : Color.Gray, 2f, Color.Black);
                if (!Locked)
                {
                    ActiveFont.DrawOutline(Discovered, Position + new Vector2(width / 2, 60f), Vector2.UnitX * 0.5f, Vector2.One * 0.5f, Color.Gray, 2f, Color.Black);
                }
                if ((ShowArrow || Selected) && !Locked)
                {
                    mTexture.DrawCentered(Position + new Vector2(width /2, height - 40f), AllLogs ? Color.Gold : Color.White, 0.8f, (float)-Math.PI / 2);
                }
            }
        }

        Level level;

        public LorebookDisplay(Level level)
        {
            this.level = level;
            Tag = Tags.HUD;
            Depth = -10001;
        }

        public IEnumerator GenerateLorebookDisplay()
        {
            Scene.Add(new CategoryDisplay(level, new Vector2(155f, 245f), 0, "XaphanHelper_UI_Locations"));
            Scene.Add(new CategoryDisplay(level, new Vector2(557f, 245f), 1, "XaphanHelper_UI_Equipement"));
            Scene.Add(new CategoryDisplay(level, new Vector2(960f, 245f), 2, "XaphanHelper_UI_Bestiary"));
            Scene.Add(new CategoryDisplay(level, new Vector2(1362f, 245f), 3, "XaphanHelper_UI_Adventure"));
            yield return null;
        }

        public override void Removed(Scene scene)
        {
            base.Removed(scene);
            foreach (CategoryDisplay display in level.Tracker.GetEntities<CategoryDisplay>())
            {
                display.RemoveSelf();
            }
            /*foreach (AchievementDisplay display in level.Tracker.GetEntities<AchievementDisplay>())
            {
                display.RemoveSelf();
            }
            medalDisplay.RemoveSelf();
            if (lockedDisplay != null)
            {
                lockedDisplay.RemoveSelf();
            }*/
        }

        public override void Render()
        {
            base.Render();
            Level level = Scene as Level;
            if (level != null && (level.FrozenOrPaused || level.RetryPlayerCorpse != null || level.SkippingCutscene))
            {
                return;
            }
            Draw.Rect(new Vector2(100, 180), 1720, 840, Color.Black * 0.85f);
            float SectionTitleLenght;
            float SectionTitleHeight;
            string SectionName;
            Vector2 SectionPosition;
            int SectionMaxItems;

            SectionName = Dialog.Clean("XaphanHelper_UI_Categories");
            SectionPosition = new Vector2(960f, 225f);
            SectionMaxItems = 1;
            ActiveFont.DrawOutline(SectionName, Position + SectionPosition, new Vector2(0.5f, 0.5f), Vector2.One * 1f, Color.Yellow, 2f, Color.Black);
            SectionTitleLenght = ActiveFont.Measure(SectionName).X;
            SectionTitleHeight = ActiveFont.Measure(SectionName).Y;
            Draw.Rect(Position + SectionPosition + new Vector2(SectionTitleLenght / 2 + 10, -4), 805f - (SectionTitleLenght / 2 + 10) + 15, 8f, Color.White);
            Draw.Rect(Position + SectionPosition + new Vector2(-805f - 15, -4), 805f - (SectionTitleLenght / 2 + 10) + 15, 8f, Color.White);
            Draw.Rect(Position + SectionPosition + new Vector2(SectionTitleLenght / 2 + 10 + 805f - (SectionTitleLenght / 2 + 10) + 5, -4), 10f, SectionMaxItems * 155f + SectionTitleHeight / 2 + 4, Color.White);
            Draw.Rect(Position + SectionPosition + new Vector2(-805f - 15, -4), 10f, SectionMaxItems * 155f + SectionTitleHeight / 2 + 4, Color.White);
            Draw.Rect(Position + SectionPosition + new Vector2(-805f - 15, SectionMaxItems * 155f + SectionTitleHeight / 2 - 8), 1640f, 8f, Color.White);

            SectionName = Dialog.Clean("XaphanHelper_UI_Logs");
            SectionPosition = new Vector2(430f, 460f);
            SectionMaxItems = 10;
            ActiveFont.DrawOutline(SectionName, Position + SectionPosition, new Vector2(0.5f, 0.5f), Vector2.One * 1f, Color.Yellow, 2f, Color.Black);
            SectionTitleLenght = ActiveFont.Measure(SectionName).X;
            SectionTitleHeight = ActiveFont.Measure(SectionName).Y;
            Draw.Rect(Position + SectionPosition + new Vector2(SectionTitleLenght / 2 + 10, -4), 275f - (SectionTitleLenght / 2 + 10) + 15, 8f, Color.White);
            Draw.Rect(Position + SectionPosition + new Vector2(-275f - 15, -4), 275f - (SectionTitleLenght / 2 + 10) + 15, 8f, Color.White);
            Draw.Rect(Position + SectionPosition + new Vector2(SectionTitleLenght / 2 + 10 + 275f - (SectionTitleLenght / 2 + 10) + 5, -4), 10f, SectionMaxItems * 50f + SectionTitleHeight / 2 + 4, Color.White);
            Draw.Rect(Position + SectionPosition + new Vector2(-275f - 15, -4), 10f, SectionMaxItems * 50f + SectionTitleHeight / 2 + 4, Color.White);
            Draw.Rect(Position + SectionPosition + new Vector2(-275f - 15, SectionMaxItems * 50f + SectionTitleHeight / 2 - 8), 580f, 8f, Color.White);

            SectionName = Dialog.Clean("XaphanHelper_UI_Informations");
            SectionPosition = new Vector2(1030f, 460f);
            SectionMaxItems = 1;
            ActiveFont.DrawOutline(SectionName, Position + SectionPosition + new Vector2(230f, 0f), new Vector2(0.5f, 0.5f), Vector2.One * 1f, Color.Yellow, 2f, Color.Black);
            SectionTitleLenght = ActiveFont.Measure(SectionName).X;
            SectionTitleHeight = ActiveFont.Measure(SectionName).Y;
            Draw.Rect(Position + SectionPosition + new Vector2(SectionTitleLenght / 2 + 10, -4) + new Vector2(230f, 0f), 505f - (SectionTitleLenght / 2 + 10) + 15, 8f, Color.White);
            Draw.Rect(Position + SectionPosition + new Vector2(-505f - 15, -4) + new Vector2(230f, 0f), 505f - (SectionTitleLenght / 2 + 10) + 15, 8f, Color.White);
            Draw.Rect(Position + SectionPosition + new Vector2(SectionTitleLenght / 2 + 10 + 505f - (SectionTitleLenght / 2 + 10) + 5, -4) + new Vector2(230f, 0f), 10f, SectionMaxItems * 500f + SectionTitleHeight / 2 + 4, Color.White);
            Draw.Rect(Position + SectionPosition + new Vector2(-505f - 15, -4) + new Vector2(230f, 0f), 10f, SectionMaxItems * 500f + SectionTitleHeight / 2 + 4, Color.White);
            Draw.Rect(Position + SectionPosition + new Vector2(-505f - 15, SectionMaxItems * 500f + SectionTitleHeight / 2 - 8) + new Vector2(230f, 0f), 1040f, 8f, Color.White);

        }
    }
}
