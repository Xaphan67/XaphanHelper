using System;
using System.Collections;
using System.Collections.Generic;
using Celeste.Mod.XaphanHelper.Data;
using Microsoft.Xna.Framework;
using Monocle;

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

            private string Discovered;

            private LorebookScreen LorebookScreen;

            public bool Selected;

            private bool ShowArrow;

            public bool Locked;

            private bool AllLogs;

            private float selectedAlpha = 0;

            private int alphaStatus = 0;

            private Sprite Sprite;

            private int discoveredLogs;

            public int readLogs;

            public CategoryDisplay(Level level, Vector2 position, int id, string name, List<LorebookData> data, string description, bool noDialog = false) : base(position)
            {
                Tag = Tags.HUD;
                Sprite = new Sprite(GFX.Gui, "lorebook/");
                Sprite.AddLoop("new", "new", 0.05f);
                Sprite.Play("new");
                LorebookScreen = level.Tracker.GetEntity<LorebookScreen>();
                ID = id;
                Name = noDialog ? name : Dialog.Clean(name);
                int totalLogs = 0;
                foreach (LorebookData entry in data)
                {
                    if (XaphanModule.ModSaveData.LorebookEntries.Contains(entry.EntryID) || level.Session.GetFlag(entry.Flag))
                    {
                        discoveredLogs++;
                    }
                    if (XaphanModule.ModSaveData.LorebookEntriesRead.Contains(entry.EntryID))
                    {
                        readLogs++;
                    }
                    if (entry.Picture != null)
                    {
                        totalLogs++;
                    }
                }
                Description = description;
                Discovered = $"{discoveredLogs} / {totalLogs} {Dialog.Clean("XaphanHelper_UI_Discovered")}";
                AllLogs = discoveredLogs == totalLogs;
                Locked = Name.Contains("?");
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
                    foreach (EntryInfo info in SceneAs<Level>().Tracker.GetEntities<EntryInfo>())
                    {
                        if (info.textSfx.Playing)
                        {
                            info.textSfx.Stop();
                            info.textSfx.Param("end", 1f);
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
                float paddingLeft = 0f;
                if (readLogs < discoveredLogs && !Locked)
                {
                    float nameWidth = ActiveFont.Measure(Name).X;
                    paddingLeft = Sprite.Width;
                    Sprite.RenderPosition = Position + new Vector2(width / 2 - nameWidth / 2 + paddingLeft / 2 - 5f, 41f - Sprite.Height / 2);
                    Sprite.Render();
                }
                ActiveFont.DrawOutline(Name, Position + new Vector2(width / 2 + paddingLeft / 2, Locked ? height / 2 : 41f), new Vector2(0.5f, 0.5f), Vector2.One * 0.8f, !Name.Contains("?") ? (AllLogs ? Color.Gold : Color.White) : Color.Gray, 2f, Color.Black);
                if (!Locked)
                {
                    ActiveFont.DrawOutline(Discovered, Position + new Vector2(width / 2, 60f), Vector2.UnitX * 0.5f, Vector2.One * 0.5f, Color.Gray, 2f, Color.Black);
                }
                if ((ShowArrow || Selected) && !Locked)
                {
                    mTexture.DrawCentered(Position + new Vector2(width / 2, height - 40f), AllLogs ? Color.Gold : Color.White, 0.8f, (float)-Math.PI / 2);
                }
            }
        }

        [Tracked(true)]
        public class EntryDisplay : Entity
        {
            public int ID;

            public string entryID;

            public int categoryID;

            public bool isSubCategory;

            public float width = 550f;

            public float height = 50f;

            public bool Locked;

            public string Flag;

            public string Name;

            public string Text;

            public string Picture;

            private Sprite Sprite;

            private LorebookScreen LorebookScreen;

            public bool Selected;

            private float selectedAlpha = 0;

            private int alphaStatus = 0;

            private float scale = 0.65f;

            private Coroutine readRoutine = new();

            public EntryDisplay(Level level, Vector2 position, int id, string entryID, string flag, string name, int categoryID, string text, string picture, bool noDialog = false) : base(position)
            {
                Tag = Tags.HUD;
                ID = id;
                this.entryID = entryID;
                this.categoryID = categoryID;
                Flag = flag;
                Name = noDialog ? name : Dialog.Clean(name);
                isSubCategory = picture == null;
                Text = text;
                Picture = picture;
                Sprite = new Sprite(GFX.Gui, "lorebook/");
                Sprite.AddLoop("new", "new", 0.05f);
                Sprite.Play("new");
                LorebookScreen = level.Tracker.GetEntity<LorebookScreen>();
                Locked = Name.Contains("?") && !isSubCategory;
                Depth = -10001;
            }

            public override void Update()
            {
                base.Update();
                if (LorebookScreen.entrySelection == ID)
                {
                    Selected = true;
                    if (!readRoutine.Active && !XaphanModule.ModSaveData.LorebookEntriesRead.Contains(entryID) && !Name.Contains("?"))
                    {
                        Add(readRoutine = new Coroutine(MarkAsReaded()));
                    }
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

            public IEnumerator MarkAsReaded()
            {
                float timer = 2f;
                while (timer > 0f && Selected)
                {
                    timer -= Engine.DeltaTime;
                    yield return null;
                }
                if (timer <= 0)
                {
                    XaphanModule.ModSaveData.LorebookEntriesRead.Add(entryID);
                    foreach (CategoryDisplay display in SceneAs<Level>().Tracker.GetEntities<CategoryDisplay>())
                    {
                        if (display.ID == categoryID)
                        {
                            display.readLogs++;
                            break;
                        }
                    }
                }
            }

            public override void Render()
            {
                base.Render();
                if (Position.Y >= 460f && Position.Y <= 933f)
                {
                    string name = Name.ToUpper();
                    float lenght = ActiveFont.Measure(name).X * scale;
                    if (Selected)
                    {
                        Draw.Rect(Position, width, height, Color.Yellow * selectedAlpha);
                    }
                    float paddingLeft = 0f;
                    if ((XaphanModule.ModSaveData.LorebookEntries.Contains(entryID) || SceneAs<Level>().Session.GetFlag(Flag)) && !XaphanModule.ModSaveData.LorebookEntriesRead.Contains(entryID))
                    {
                        Sprite.RenderPosition = Position + Vector2.UnitX * 12f;
                        Sprite.Render();
                        paddingLeft = 40f;
                    }
                    ActiveFont.DrawOutline(name, Position + new Vector2(20 + paddingLeft + lenght / 2 - 10, height / 2), new Vector2(0.5f, 0.5f), Vector2.One * scale, Locked ? Color.Gray : isSubCategory ? Calc.HexToColor("AA00AA") : Color.White, 2f, Color.Black);
                }
            }
        }

        [Tracked(true)]
        public class EntryInfo : Entity
        {
            private string Name;

            public FancyText.Text Text;

            public string Picture;

            private Sprite Sprite;

            private float scale = 0.85f;

            private bool showEntryInfo;

            public Coroutine TextRoutine = new();

            private int index = 0;

            private int currentDisplayID;

            private int previousDisplayID;

            public SoundSource textSfx;

            public EntryInfo(Vector2 position) : base(position)
            {
                Tag = Tags.HUD;
                Depth = -10001;
                Add(textSfx = new SoundSource());
            }

            public override void Update()
            {
                showEntryInfo = true;
                foreach (CategoryDisplay category in SceneAs<Level>().Tracker.GetEntities<CategoryDisplay>())
                {
                    if (category.Selected)
                    {
                        Visible = true;
                        showEntryInfo = false;
                        Name = category.Name;
                        Text = FancyText.Parse(Dialog.Clean(category.Description), 850, 4);
                        Picture = null;
                        Sprite = null;
                        if (TextRoutine.Active)
                        {
                            TextRoutine.Cancel();
                        }
                        break;
                    }
                }
                if (showEntryInfo)
                {
                    previousDisplayID = currentDisplayID;
                    foreach (EntryDisplay display in SceneAs<Level>().Tracker.GetEntities<EntryDisplay>())
                    {
                        if (display.Selected)
                        {
                            currentDisplayID = display.ID;
                            if (!display.Name.Contains("?"))
                            {
                                Name = display.Name;
                                Text = FancyText.Parse(Dialog.Get(display.Text.Trim()), 1390, 5);
                                Picture = display.Picture;
                                Sprite = new Sprite(GFX.Gui, Picture);
                                Sprite.AddLoop("picture", "", 0.05f);
                                Sprite.Play("picture");
                                scale = 0.7f;
                                break;
                            }
                            else
                            {
                                Name = null;
                                Text = FancyText.Parse(Dialog.Clean("XaphanHelper_UI_LorebookEntry_Locked"), 500, 4);
                                Picture = null;
                                Sprite = null;
                                scale = 0.85f;
                                break;
                            }
                        }
                        Text = null;
                        Picture = null;
                        Sprite = null;
                    }
                    if (TextRoutine.Active && currentDisplayID != previousDisplayID)
                    {
                        TextRoutine.Cancel();
                    }
                    if (!TextRoutine.Active)
                    {
                        Add(TextRoutine = new Coroutine(DisplayTextRoutine()));
                    }
                }
                if (TextRoutine != null && Text != null)
                {
                    TextRoutine.Update();
                }
            }

            private IEnumerator DisplayTextRoutine()
            {
                index = 0;
                float num = 0f;
                textSfx.Play("event:/ui/game/memorial_text_loop");
                textSfx.Param("end", 0f);
                Visible = true;
                while (index < Text.Nodes.Count)
                {
                    if (Text.Nodes[index] is FancyText.Char)
                    {
                        num += (Text.Nodes[index] as FancyText.Char).Delay / 4;
                    }
                    index++;
                    if (num > 0.008f)
                    {
                        yield return num;
                        num = 0f;
                    }
                }
                textSfx.Stop();
                textSfx.Param("end", 1f);
                while (currentDisplayID == previousDisplayID)
                {
                    yield return null;
                }
            }

            public override void Render()
            {
                base.Render();
                if (Sprite != null)
                {
                    Sprite.RenderPosition = Position - new Vector2(Sprite.Width / 2, Sprite.Height / 2 + 110f);
                    Sprite.Render();
                }
                ActiveFont.DrawOutline(Name, Position, new Vector2(0.5f, 0.5f), Vector2.One * 1f, Color.Gold, 2f, Color.Black);
                if (Text != null)
                {
                    if (TextRoutine.Active)
                    {
                        Text.DrawJustifyPerLine(Position + Vector2.UnitY * 25f - Vector2.UnitY * (Picture == null && showEntryInfo ? 60f : 0), new Vector2(0.5f, 0f), Vector2.One * scale, 1f, 0, index);
                    }
                    else
                    {
                        Text.DrawJustifyPerLine(Position + Vector2.UnitY * 25f - Vector2.UnitY * (Picture == null && showEntryInfo ? 60f : 0), new Vector2(0.5f, 0f), Vector2.One * scale, 1f);
                    }
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
            SceneAs<Level>().Add(Info = new EntryInfo(new Vector2(1260f, 685f)));
            LorebookEntriesData = LorebookEntries.GenerateLorebookEntriesDataList();
        }

        public IEnumerator GenerateLorebookDisplay()
        {
            Scene.Add(new CategoryDisplay(level, new Vector2(355f, 245f), 0, "XaphanHelper_UI_Locations", LorebookEntriesData.FindAll(entry => entry.CategoryID == 0), "XaphanHelper_UI_Locations_Desc"));
            Scene.Add(new CategoryDisplay(level, new Vector2(760f, 245f), 1, "XaphanHelper_UI_Equipment", LorebookEntriesData.FindAll(entry => entry.CategoryID == 1), "XaphanHelper_UI_Equipment_Desc"));
            Scene.Add(new CategoryDisplay(level, new Vector2(1165f, 245f), 2, "XaphanHelper_UI_Adventure", LorebookEntriesData.FindAll(entry => entry.CategoryID == 2), "XaphanHelper_UI_Adventure_Desc"));

            GenerateEntryList(0);

            yield return null;
        }

        public void GenerateEntryList(int categoryID)
        {
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
                        string categoryName = "";
                        bool unlocked = SceneAs<Level>().Session.GetFlag(entry.Flag) || XaphanModule.ModSaveData.LorebookEntries.Contains(entry.EntryID) || entry.Picture == null;
                        bool lockedSubCategory = false;
                        if (unlocked || entry.Picture == null)
                        {
                            if (entry.Picture == null)
                            {
                                foreach (LorebookData entry2 in LorebookEntriesData)
                                {
                                    if (entry2.SubCategoryID == entry.EntryID && XaphanModule.ModSaveData.LorebookEntries.Contains(entry2.EntryID))
                                    {
                                        categoryName = entry.Name;
                                        break;
                                    }
                                }
                                if (string.IsNullOrEmpty(categoryName))
                                {
                                    categoryName = ConvertNameToHiddenName(entry.Name, true);
                                    lockedSubCategory = true;
                                }
                            }
                            else
                            {
                                categoryName = entry.Name;
                            }
                        }
                        else
                        {
                            categoryName = ConvertNameToHiddenName(entry.Name);
                        }
                        Scene.Add(new EntryDisplay(level, new Vector2(155f, 481f + YPos), ID, entry.EntryID, entry.Flag, categoryName, categoryID, entry.Text, entry.Picture, !unlocked || lockedSubCategory));
                        YPos += 50;
                        ID++;
                    }
                }
            }
        }

        private string ConvertNameToHiddenName(string name, bool isCategory = false)
        {
            string origName = Dialog.Clean(name);
            string hiddenName = "";
            foreach (char c in origName)
            {
                if (c != ' ' && (isCategory ? c != '-' : true))
                {
                    hiddenName += '?';
                }
                else if (isCategory && c == '-')
                {
                    hiddenName += '-';
                }
                else
                {
                    hiddenName += ' ';
                }
            }
            return hiddenName;
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
            Draw.Rect(Position + SectionPosition + new Vector2(SectionTitleLenght / 2 + 10, -4), 605f - (SectionTitleLenght / 2 + 10) + 15, 8f, Color.White);
            Draw.Rect(Position + SectionPosition + new Vector2(-605f - 15, -4), 605f - (SectionTitleLenght / 2 + 10) + 15, 8f, Color.White);
            Draw.Rect(Position + SectionPosition + new Vector2(SectionTitleLenght / 2 + 10 + 605f - (SectionTitleLenght / 2 + 10) + 5, -4), 10f, SectionMaxItems * 155f + SectionTitleHeight / 2 + 4, Color.White);
            Draw.Rect(Position + SectionPosition + new Vector2(-605f - 15, -4), 10f, SectionMaxItems * 155f + SectionTitleHeight / 2 + 4, Color.White);
            Draw.Rect(Position + SectionPosition + new Vector2(-605f - 15, SectionMaxItems * 155f + SectionTitleHeight / 2 - 8), 1240f, 8f, Color.White);

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
