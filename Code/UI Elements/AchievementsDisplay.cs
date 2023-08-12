using System.Collections;
using Celeste.Mod.XaphanHelper.Data;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Monocle;
using System;
using System.Linq;

namespace Celeste.Mod.XaphanHelper.UI_Elements
{
    class AchievementsDisplay : Entity
    {
        [Tracked(true)]
        public class MedalsDisplay : Entity
        {
            public float width = 550f;

            public float height = 150f;

            public MTexture medalIcon;

            public string medals;

            public Color color;

            List<AchievementData> AchievementsList;

            public MedalsDisplay(Level level, Vector2 position, List<AchievementData> data) : base(position)
            {
                Tag = Tags.HUD;
                medalIcon = GFX.Gui["achievements/medal"];
                AchievementsList = data;
                Depth = -10001;
            }

            public override void Added(Scene scene)
            {
                base.Added(scene);
                int medals = 0;
                int totalMedals = 0;
                foreach (AchievementData achievement in AchievementsList)
                {
                    if (SceneAs<Level>().Session.GetFlag(achievement.Flag))
                    {
                        medals += achievement.Medals;
                    }
                    totalMedals += achievement.Medals;
                }
                this.medals = $"{medals} / {totalMedals}";
                color = medals.ToString() == totalMedals.ToString() ? Color.Gold : Color.White;
            }

            public override void Render()
            {
                base.Render();
                float lenght = ActiveFont.Measure(medals).X * 1.5f;
                float totalWidth = lenght + 25f + medalIcon.Width;
                float adjust = (width - totalWidth) / 2;
                ActiveFont.DrawOutline(medals, Position + new Vector2(adjust, height / 2), new Vector2(0f, 0.5f), Vector2.One * 1.5f, color, 2f, Color.Black);
                medalIcon.Draw(Position + new Vector2(lenght + 25f + adjust, height / 2 - medalIcon.Height / 2), new Vector2(0.5f));
            }
        }

        [Tracked(true)]
        public class CategoryDisplay : Entity
        {
            public int ID;

            public float width = 550f;

            public float height = 85f;

            private string Name;

            private string Completed;

            private AchievementsScreen AchievementsScreen;

            public bool Selected;

            private bool ShowArrow;

            public bool Locked;

            private bool AllAchievements;

            private float selectedAlpha = 0;

            private int alphaStatus = 0;

            public CategoryDisplay(Level level, Vector2 position, int id, string name, List<AchievementData> data, bool noDialog = false) : base(position)
            {
                Tag = Tags.HUD;
                AchievementsScreen = level.Tracker.GetEntity<AchievementsScreen>();
                ID = id;
                Name = noDialog ? name : Dialog.Clean(name);

                int completedAchievements = 0;
                int totalAchievements = 0;
                int hiddenAchievements = 0;
                foreach (AchievementData achievement in data)
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
                }
                string HiddenAchievements = $" (+{hiddenAchievements} {Dialog.Clean("XaphanHelper_UI_Hidden")})";
                Completed = $"{completedAchievements} / {totalAchievements} {Dialog.Clean("XaphanHelper_UI_Completed")}" + (hiddenAchievements > 0 ? HiddenAchievements : "");
                AllAchievements = completedAchievements == (totalAchievements + hiddenAchievements);
                Locked = Name == "???";
                Depth = -10001;
            }

            public override void Update()
            {
                base.Update();
                if (AchievementsScreen.categorySelection == ID)
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
                    if (AchievementsScreen.previousCategorySelection == ID)
                    {
                        ShowArrow = true;
                    }
                }
            }

            public override void Render()
            {
                base.Render();
                MTexture mTexture = GFX.Gui["towerarrow"];
                float lenght = ActiveFont.Measure(Name).X * 0.8f;
                if (Selected)
                {
                    Draw.Rect(Position, width, height, Color.Yellow * selectedAlpha);
                }
                else if (AchievementsScreen.previousCategorySelection == ID)
                {
                    Draw.Rect(Position, width, height, Color.DarkGreen * 0.7f);
                }
                ActiveFont.DrawOutline(Name, Position + new Vector2(20f + lenght / 2f, Locked ? height / 2 : 26f), new Vector2(0.5f, 0.5f), Vector2.One * 0.8f, Name != "???" ? (AllAchievements ? Color.Gold : Color.White) : Color.Gray, 2f, Color.Black);
                if (!Locked)
                {
                    ActiveFont.DrawOutline(Completed, Position + new Vector2(20f, 45f), Vector2.Zero, Vector2.One * 0.5f, Color.Gray, 2f, Color.Black);
                }
                if ((ShowArrow || Selected) && !Locked)
                {
                    mTexture.DrawCentered(Position + new Vector2(width - 35f, height / 2f), AllAchievements ? Color.Gold : Color.White, 0.8f, (float)Math.PI);
                }
            }
        }

        [Tracked(true)]
        public class AchievementDisplay : Entity
        {
            public float width = 1010f;

            public float height = 147f;

            public int ID;

            public MTexture icon;

            public MTexture medalIcon;

            public string name;

            public string description;

            public string completion;

            public string medals;

            public float completionPercent;

            private AchievementsScreen AchievementsScreen;

            public bool Selected;

            public bool Locked;

            private float selectedAlpha = 0;

            private int alphaStatus = 0;

            public AchievementDisplay(Level level, Vector2 position, int id, AchievementData data, List<AchievementData> achievements) : base(position)
            {
                Tag = Tags.HUD;
                AchievementsScreen = level.Tracker.GetEntity<AchievementsScreen>();
                ID = id;
                if (data.ReqID != null && !XaphanModule.ModSaveData.Achievements.Contains(data.ReqID))
                {
                    Locked = true;
                }
                name = Dialog.Clean(data.Name);
                if (!Locked)
                {
                    icon = GFX.Gui[data.Icon];
                    medalIcon = GFX.Gui["achievements/medal"];
                    description = Dialog.Clean(data.Description);
                    medals = data.Medals.ToString();
                    completionPercent = data.CurrentValue * 100 / data.MaxValue;
                    completion = $"{Dialog.Clean("XaphanHelper_UI_Objective")} {data.CurrentValue} / {data.MaxValue} ({completionPercent}%)";
                }
                else
                {
                    icon = GFX.Gui["achievements/lockIcon"];
                    description = Dialog.Clean("XaphanHelper_UI_LockedAchievementDesc");
                    completion = $"{Dialog.Clean("XaphanHelper_UI_AchievementToUnlock")} {Dialog.Clean(achievements.Find(achievement => achievement.AchievementID == data.ReqID).Name)}";
                }
                Depth = -10001;
            }

            public override void Update()
            {
                base.Update();
                if (AchievementsScreen.achievementSelection == ID)
                {
                    Selected = true;
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
                }
            }

            public override void Render()
            {
                base.Render();
                if (Position.Y >= 245f && Position.Y <= 833f)
                {
                    Draw.Rect(Position + Vector2.UnitY, width * completionPercent / 100, height - 2, Color.DarkGreen * 0.7f);
                    if (Selected)
                    {
                        Draw.Rect(Position, width, 5f, Color.Yellow * selectedAlpha);
                        Draw.Rect(Position + Vector2.UnitY * 5f, 5f, height - 10f, Color.Yellow * selectedAlpha);
                        Draw.Rect(Position + Vector2.UnitY * (height - 5f), width, 5f, Color.Yellow * selectedAlpha);
                        Draw.Rect(Position + new Vector2(width - 5f, 5f), 5f, height - 10f, Color.Yellow * selectedAlpha);
                    }
                    icon.Draw(Position + Vector2.One * 6, Vector2.Zero, Color.White, 0.9f);
                    float lenght = ActiveFont.Measure(name).X * 0.75f;
                    float descHeight = ActiveFont.Measure(description).Y * 0.5f;
                    ActiveFont.DrawOutline(name, Position + new Vector2(167f + lenght / 2 - 10, 55f - descHeight / 2), new Vector2(0.5f, 0.5f), Vector2.One * 0.75f, completionPercent == 100 ? Color.Gold : Color.White, 2f, Color.Black);
                    ActiveFont.DrawOutline(description, Position + new Vector2(158f, 75f - descHeight / 2), Vector2.Zero, Vector2.One * 0.5f, Color.Gray, 2f, Color.Black);
                    ActiveFont.DrawOutline(completion, Position + new Vector2(158f, 75f + descHeight / 2), Vector2.Zero, Vector2.One * 0.5f, Color.Gray, 2f, Color.Black);
                    if (medalIcon != null)
                    {
                        medalIcon.Draw(Position + new Vector2(width - 115f, 9f));
                        lenght = ActiveFont.Measure(medals).X;
                        ActiveFont.DrawOutline(medals, Position + new Vector2(width - 72.5f - lenght / 2, 106f), new Vector2(0f, 0.5f), Vector2.One, Color.White, 2f, Color.Black);
                    }
                }
            }
        }

        public class LockedDisplay : Entity
        {
            public float width = 1010f;

            public float height = 735f;

            public string title;

            public string description;

            public MTexture lockIcon;

            public LockedDisplay(Level level, Vector2 position) : base(position)
            {
                Tag = Tags.HUD;
                lockIcon = GFX.Gui["achievements/lock"];
                title = Dialog.Clean("XaphanHelper_UI_Achievements_Locked");
                description = Dialog.Clean("XaphanHelper_UI_Achievements_Locked_Description");
                Depth = -10001;
            }

            public override void Render()
            {
                base.Render();
                ActiveFont.DrawOutline(title, Position + new Vector2(width / 2, height / 2 - 200f), new Vector2(0.5f), Vector2.One, Color.Red, 2f, Color.Black);
                lockIcon.Draw(Position + new Vector2(width / 2 - lockIcon.Width / 2, height / 2 - lockIcon.Height / 2), new Vector2(0.5f));
                ActiveFont.DrawOutline(description, Position + new Vector2(width / 2, height / 2 + 200f), new Vector2(0.5f), Vector2.One * 0.8f, Color.Gray, 2f, Color.Black);
            }
        }

        private Level level;
        
        public List<AchievementData> AchievementsData;

        private MedalsDisplay medalDisplay;

        private LockedDisplay lockedDisplay;

        public AchievementsDisplay(Level level)
        {
            this.level = level;
            Tag = Tags.HUD;
            Depth = -10001;
        }

        public override void Added(Scene scene)
        {
            base.Added(scene);
            AchievementsData = Achievements.GenerateAchievementsList(level.Session);
        }

        public IEnumerator GennerateAchievementsDisplay()
        {
            Scene.Add(medalDisplay = new MedalsDisplay(level, new Vector2(155f, 245f), AchievementsData));
            Scene.Add(new CategoryDisplay(level, new Vector2(155f, 470f), 0, "XaphanHelper_UI_General", AchievementsData.FindAll(achievement => achievement.CategoryID == 0)));
            bool visitedChapter = XaphanModule.ModSaveData.VisitedChapters.Contains("Xaphan/0_Ch1_0");
            Scene.Add(new CategoryDisplay(level, new Vector2(155f, 555f), 1, visitedChapter ? "Xaphan_0_1_AncientRuins" : "???", AchievementsData.FindAll(achievement => achievement.CategoryID == 1), visitedChapter ? false : true));
            visitedChapter = XaphanModule.ModSaveData.VisitedChapters.Contains("Xaphan/0_Ch2_0");
            Scene.Add(new CategoryDisplay(level, new Vector2(155f, 640f), 2, visitedChapter ? "Xaphan_0_2_ForgottenAbyss" : "???", AchievementsData.FindAll(achievement => achievement.CategoryID == 2), visitedChapter ? false : true));
            visitedChapter = XaphanModule.ModSaveData.VisitedChapters.Contains("Xaphan/0_Ch3_0");
            Scene.Add(new CategoryDisplay(level, new Vector2(155f, 725f), 3, visitedChapter ? "Xaphan_0_3_ExoticUndergrowdth" : "???", AchievementsData.FindAll(achievement => achievement.CategoryID == 3), visitedChapter ? false : true));
            visitedChapter = XaphanModule.ModSaveData.VisitedChapters.Contains("Xaphan/0_Ch4_0");
            Scene.Add(new CategoryDisplay(level, new Vector2(155f, 810f), 4, visitedChapter ? "Xaphan_0_4_BlazingGorge" : "???", AchievementsData.FindAll(achievement => achievement.CategoryID == 4), visitedChapter ? false : true));
            visitedChapter = XaphanModule.ModSaveData.VisitedChapters.Contains("Xaphan/0_Ch5_0");
            Scene.Add(new CategoryDisplay(level, new Vector2(155f, 895f), 5, visitedChapter ? "Xaphan_0_5_SubterraneanTerminal" : "???", AchievementsData.FindAll(achievement => achievement.CategoryID == 5), visitedChapter ? false : true));

            GenerateAchievementsList(0);

            yield return null;
        }

        public void GenerateAchievementsList(int categoryID)
        {
            if (lockedDisplay != null)
            {
                lockedDisplay.RemoveSelf();
                lockedDisplay = null;
            }
            foreach (AchievementDisplay display in level.Tracker.GetEntities<AchievementDisplay>())
            {
                display.RemoveSelf();
            }
            int YPos = 0;
            int ID = 0;
            bool locked = false;
            foreach (CategoryDisplay categoryDisplay in level.Tracker.GetEntities<CategoryDisplay>())
            {
                if (categoryDisplay.ID == categoryID)
                {
                    locked = categoryDisplay.Locked;
                }
            }
            if (!locked)
            {
                AchievementsData = AchievementsData.OrderByDescending(achievement => !(!string.IsNullOrEmpty(achievement.ReqID) && !XaphanModule.ModSaveData.Achievements.Contains(achievement.ReqID))).ToList();
                foreach (AchievementData achievement in AchievementsData)
                {
                    if (achievement.CategoryID == categoryID && (achievement.Hidden ? achievement.CurrentValue > 0 : true))
                    {
                        Scene.Add(new AchievementDisplay(level, new Vector2(755f, 245f + YPos), ID, achievement, AchievementsData));
                        YPos += 147;
                        ID++;
                    }
                }
            }
            else
            {
                Scene.Add(lockedDisplay = new LockedDisplay(level, new Vector2(755f, 245f)));
            }
        }

        public override void Removed(Scene scene)
        {
            base.Removed(scene);
            foreach (CategoryDisplay display in level.Tracker.GetEntities<CategoryDisplay>())
            {
                display.RemoveSelf();
            }
            foreach (AchievementDisplay display in level.Tracker.GetEntities<AchievementDisplay>())
            {
                display.RemoveSelf();
            }
            medalDisplay.RemoveSelf();
            if (lockedDisplay != null)
            {
                lockedDisplay.RemoveSelf();
            }
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

            SectionName = Dialog.Clean("XaphanHelper_UI_Medals");
            SectionPosition = new Vector2(430f, 225f);
            SectionMaxItems = 1;
            ActiveFont.DrawOutline(SectionName, Position + SectionPosition, new Vector2(0.5f, 0.5f), Vector2.One * 1f, Color.Yellow, 2f, Color.Black);
            SectionTitleLenght = ActiveFont.Measure(SectionName).X;
            SectionTitleHeight = ActiveFont.Measure(SectionName).Y;
            Draw.Rect(Position + SectionPosition + new Vector2(SectionTitleLenght / 2 + 10, -4), 275f - (SectionTitleLenght / 2 + 10) + 15, 8f, Color.White);
            Draw.Rect(Position + SectionPosition + new Vector2(-275f - 15, -4), 275f - (SectionTitleLenght / 2 + 10) + 15, 8f, Color.White);
            Draw.Rect(Position + SectionPosition + new Vector2(SectionTitleLenght / 2 + 10 + 275f - (SectionTitleLenght / 2 + 10) + 5, -4), 10f, SectionMaxItems * 150f + SectionTitleHeight / 2 + 4, Color.White);
            Draw.Rect(Position + SectionPosition + new Vector2(-275f - 15, -4), 10f, SectionMaxItems * 150f + SectionTitleHeight / 2 + 4, Color.White);
            Draw.Rect(Position + SectionPosition + new Vector2(-275f - 15, SectionMaxItems * 150f + SectionTitleHeight / 2 - 8), 580f, 8f, Color.White);

            SectionName = Dialog.Clean("XaphanHelper_UI_Categories");
            SectionPosition = new Vector2(430f, 450f);
            SectionMaxItems = 6;
            ActiveFont.DrawOutline(SectionName, Position + SectionPosition, new Vector2(0.5f, 0.5f), Vector2.One * 1f, Color.Yellow, 2f, Color.Black);
            SectionTitleLenght = ActiveFont.Measure(SectionName).X;
            SectionTitleHeight = ActiveFont.Measure(SectionName).Y;
            Draw.Rect(Position + SectionPosition + new Vector2(SectionTitleLenght / 2 + 10, -4), 275f - (SectionTitleLenght / 2 + 10) + 15, 8f, Color.White);
            Draw.Rect(Position + SectionPosition + new Vector2(-275f - 15, -4), 275f - (SectionTitleLenght / 2 + 10) + 15, 8f, Color.White);
            Draw.Rect(Position + SectionPosition + new Vector2(SectionTitleLenght / 2 + 10 + 275f - (SectionTitleLenght / 2 + 10) + 5, -4), 10f, SectionMaxItems * 85f + SectionTitleHeight / 2 + 4, Color.White);
            Draw.Rect(Position + SectionPosition + new Vector2(-275f - 15, -4), 10f, SectionMaxItems * 85f + SectionTitleHeight / 2 + 4, Color.White);
            Draw.Rect(Position + SectionPosition + new Vector2(-275f - 15, SectionMaxItems * 85f + SectionTitleHeight / 2 - 8), 580f, 8f, Color.White);

            SectionName = Dialog.Clean("XaphanHelper_UI_Achievements").ToUpper();
            SectionPosition = new Vector2(1030f, 225f);
            SectionMaxItems = 5;
            ActiveFont.DrawOutline(SectionName, Position + SectionPosition + new Vector2(230f, 0f), new Vector2(0.5f, 0.5f), Vector2.One * 1f, Color.Yellow, 2f, Color.Black);
            SectionTitleLenght = ActiveFont.Measure(SectionName).X;
            SectionTitleHeight = ActiveFont.Measure(SectionName).Y;
            Draw.Rect(Position + SectionPosition + new Vector2(SectionTitleLenght / 2 + 10, -4) + new Vector2(230f, 0f), 505f - (SectionTitleLenght / 2 + 10) + 15, 8f, Color.White);
            Draw.Rect(Position + SectionPosition + new Vector2(-505f - 15, -4) + new Vector2(230f, 0f), 505f - (SectionTitleLenght / 2 + 10) + 15, 8f, Color.White);
            Draw.Rect(Position + SectionPosition + new Vector2(SectionTitleLenght / 2 + 10 + 505f - (SectionTitleLenght / 2 + 10) + 5, -4) + new Vector2(230f, 0f), 10f, SectionMaxItems * 147f + SectionTitleHeight / 2 + 4, Color.White);
            Draw.Rect(Position + SectionPosition + new Vector2(-505f - 15, -4) + new Vector2(230f, 0f), 10f, SectionMaxItems * 147f + SectionTitleHeight / 2 + 4, Color.White);
            Draw.Rect(Position + SectionPosition + new Vector2(-505f - 15, SectionMaxItems * 147f + SectionTitleHeight / 2 - 8) + new Vector2(230f, 0f), 1040f, 8f, Color.White);

        }
    }
}
