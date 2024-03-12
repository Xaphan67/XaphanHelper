using Celeste.Mod.XaphanHelper.Data;
using Microsoft.Xna.Framework;
using Monocle;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Xml.Linq;

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

            public string Name;

            public string Description;

            public string Picture;

            private string Discovered;

            private LorebookScreen LorebookScreen;

            public bool Selected;

            private bool ShowArrow;

            public bool Locked;

            private bool AllLogs;

            private float selectedAlpha = 0;

            private int alphaStatus = 0;

            public CategoryDisplay(Level level, Vector2 position, int id, string name, List<LorebookData> data, string description, string picture, bool noDialog = false) : base(position)
            {
                Tag = Tags.HUD;
                LorebookScreen = level.Tracker.GetEntity<LorebookScreen>();
                ID = id;
                Name = noDialog ? name : Dialog.Clean(name);

                int discoveredLogs = 0;
                int totalLogs = 0;
                foreach (LorebookData entry in data)
                {
                    if (level.Session.GetFlag("LorebookEntry_" + entry.EntryID))
                    {
                        discoveredLogs++;
                    }
                    totalLogs++;
                }
                Description = description;
                Picture = picture;
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
                    else
                    {
                        ShowArrow = false;
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

        [Tracked(true)]
        public class EntryDisplay : Entity
        {
            public int ID;

            public float width = 550f;

            public float height = 50f;

            public bool Locked;

            public string Name;

            public string Text;

            public string Picture;

            private Sprite Sprite;

            private LorebookScreen LorebookScreen;

            public bool Selected;

            private float selectedAlpha = 0;

            private int alphaStatus = 0;

            private float scale = 0.85f;

            public EntryDisplay(Level level, Vector2 position, int id, string name, string text, string picture, bool noDialog = false) : base(position)
            {
                Tag = Tags.HUD;
                ID = id;
                Name = noDialog ? name : Dialog.Clean(name);
                Text = text;
                Picture = picture;
                Sprite = new Sprite(GFX.Gui, "lorebook/");
                Sprite.AddLoop("new", "new", 0.05f);
                Sprite.Play("new");
                Sprite.Position = position + Vector2.UnitX * 12f;
                LorebookScreen = level.Tracker.GetEntity<LorebookScreen>();
                Locked = Name == "???";
                Depth = -10001;
            }

            public override void Update()
            {
                base.Update();
                if (LorebookScreen.entrySelection == ID)
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
                float lenght = ActiveFont.Measure(Name).X * scale;
                bool smallText = false;
                if (lenght > 500f)
                {
                    lenght = ActiveFont.Measure(Name).X * (scale - 0.1f);
                    smallText = true;
                }
                if (Selected)
                {
                    Draw.Rect(Position, width, height, Color.Yellow * selectedAlpha);
                }
                Sprite.Render();
                ActiveFont.DrawOutline(Name, Position + new Vector2(60 + lenght / 2 - 10, height / 2), new Vector2(0.5f, 0.5f), Vector2.One * (smallText ? 0.75f : scale), Locked ? Color.Gray : Color.White, 2f, Color.Black);
            }
        }

        [Tracked(true)]
        public class EntryInfo : Entity
        {
            private string Name;

            public FancyText.Text Text;

            public string Picture;

            public EntryInfo(Vector2 position) : base(position)
            {
                Tag = Tags.HUD;
                Depth = -10001;
            }

            public override void Update()
            {
                bool showEntryInfo = true;
                foreach (CategoryDisplay category in SceneAs<Level>().Tracker.GetEntities<CategoryDisplay>())
                {
                    if (category.Selected)
                    {
                        showEntryInfo = false;
                        Name = category.Name;
                        Text = FancyText.Parse(Dialog.Clean(category.Description), 850, 4);
                        Picture = category.Picture;
                        break;
                    }
                }
                if (showEntryInfo)
                {
                    foreach (EntryDisplay display in SceneAs<Level>().Tracker.GetEntities<EntryDisplay>())
                    {
                        if (display.Selected)
                        {
                            if (display.Name != "???")
                            {
                                Name = display.Name;
                                Text = FancyText.Parse(Dialog.Clean(display.Text), 1150, 4);
                                Picture = display.Picture;
                                break;
                            }
                            else
                            {
                                Name = null;
                                Text = FancyText.Parse(Dialog.Clean("XaphanHelper_UI_LorebookEntry_Locked"), 500, 4);
                                break;
                            }
                        }
                        Text = null;
                        Picture = "";
                    }
                }
            }

            public override void Render()
            {
                base.Render();
                ActiveFont.DrawOutline(Name, Position, new Vector2(0.5f, 0.5f), Vector2.One * 1f, Color.Gold, 2f, Color.Black);
                if (Text != null)
                {
                    Text.DrawJustifyPerLine(Position + Vector2.UnitY * 25f, new Vector2(0.5f, 0f), Vector2.One * 0.85f, 1f);
                }
            }
        }

        Level level;

        public List<LorebookData> LorebookEntriesData;

        private EntryInfo Info;

        public LorebookDisplay(Level level)
        {
            this.level = level;
            Tag = Tags.HUD;
            Depth = -10001;
        }

        public override void Added(Scene scene)
        {
            base.Added(scene);
            SceneAs<Level>().Add(Info = new EntryInfo(new Vector2(1256f, 685f)));
            LorebookEntriesData = LorebookEntries.GenerateLorebookEntriesDataList(level.Session);
        }

        public IEnumerator GenerateLorebookDisplay()
        {
            Scene.Add(new CategoryDisplay(level, new Vector2(155f, 245f), 0, "XaphanHelper_UI_Locations", LorebookEntriesData.FindAll(entry => entry.CategoryID == 0), "XaphanHelper_UI_Locations_Desc", "lorebook/Xaphan/Locations"));
            Scene.Add(new CategoryDisplay(level, new Vector2(557f, 245f), 1, "XaphanHelper_UI_Equipement", LorebookEntriesData.FindAll(entry => entry.CategoryID == 1), "XaphanHelper_UI_Equipement_Desc", "lorebook/Xaphan/Equipement"));
            Scene.Add(new CategoryDisplay(level, new Vector2(960f, 245f), 2, "XaphanHelper_UI_Bestiary", LorebookEntriesData.FindAll(entry => entry.CategoryID == 2), "XaphanHelper_UI_Bestiary_Desc", "lorebook/Xaphan/Bestiary"));
            Scene.Add(new CategoryDisplay(level, new Vector2(1362f, 245f), 3, "XaphanHelper_UI_Adventure", LorebookEntriesData.FindAll(entry => entry.CategoryID == 3), "XaphanHelper_UI_Adventure_Desc", "lorebook/Xaphan/Adventure"));

            GenerateEntryList(0);

            yield return null;
        }

        public void GenerateEntryList(int categoryID)
        {
            /*if (lockedDisplay != null)
            {
                lockedDisplay.RemoveSelf();
                lockedDisplay = null;
            }*/
            foreach (EntryDisplay display in level.Tracker.GetEntities<EntryDisplay>())
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
                foreach (LorebookData entry in LorebookEntriesData)
                {
                    if (entry.CategoryID == categoryID)
                    {
                        bool unlocked = SceneAs<Level>().Session.GetFlag("LorebookEntry_" + entry.EntryID);
                        Scene.Add(new EntryDisplay(level, new Vector2(155f, 481f + YPos), ID, unlocked ? entry.Name : "???", entry.Text, entry.Picture, !unlocked));
                        YPos += 50;
                        ID++;
                    }
                }
            }
        }

        public override void Removed(Scene scene)
        {
            base.Removed(scene);
            foreach (CategoryDisplay display in level.Tracker.GetEntities<CategoryDisplay>())
            {
                display.RemoveSelf();
            }
            foreach (EntryDisplay display in level.Tracker.GetEntities<EntryDisplay>())
            {
                display.RemoveSelf();
            }
            Info.RemoveSelf();
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

            SectionName = Dialog.Clean("XaphanHelper_UI_Entries");
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
