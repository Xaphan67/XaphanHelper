using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Celeste.Mod.UI;
using FMOD.Studio;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Monocle;

namespace Celeste.Mod.XaphanHelper.UI_Elements
{
    public class CustomOuiChapterPanel : Oui
    {
        private class Option
        {
            public string Label;

            public string ID;

            public MTexture Icon;

            public MTexture Bg = GFX.Gui["areaselect/tab"];

            public Color BgColor = Calc.HexToColor("3c6180");

            public float Pop;

            public bool Large = true;

            public int Siblings;

            public float Slide;

            public float Appear = 1f;

            public float IconEase = 1f;

            public bool Appeared;

            public float Faded;

            public float CheckpointSlideOut;

            public string CheckpointLevelName;

            public float CheckpointRotation;

            public Vector2 CheckpointOffset;

            public int ChapterIndex;

            public float Scale
            {
                get
                {
                    if (Siblings < 5)
                    {
                        return 1f;
                    }
                    return 0.8f;
                }
            }

            public bool OnTopOfUI => Pop > 0.5f;

            public void SlideTowards(int i, int count, bool snap)
            {
                float num = count / 2f - 0.5f;
                float num2 = i - num;
                if (snap)
                {
                    Slide = num2;
                }
                else
                {
                    Slide = Calc.Approach(Slide, num2, Engine.DeltaTime * 4f);
                }
            }

            public Vector2 GetRenderPosition(Vector2 center)
            {
                float num = (Large ? 170 : 130) * Scale;
                if (Siblings > 0 && num * Siblings > 750f)
                {
                    num = 750 / Siblings;
                }
                Vector2 result = center + new Vector2(Slide * num, (float)Math.Sin(Pop * (float)Math.PI) * 70f - Pop * 12f);
                result.Y += (1f - Ease.CubeOut(Appear)) * -200f;
                result.Y -= (1f - Scale) * 80f;
                return result;
            }

            public void Render(Vector2 center, bool selected, Wiggler wiggler, Wiggler appearWiggler)
            {
                float num = Scale + (selected ? (wiggler.Value * 0.25f) : 0f) + (Appeared ? (appearWiggler.Value * 0.25f) : 0f);
                Vector2 renderPosition = GetRenderPosition(center);
                Color color = Color.Lerp(BgColor, Color.Black, (1f - Pop) * 0.6f);
                Bg.DrawCentered(renderPosition + new Vector2(0f, 10f), color, (Appeared ? Scale : num) * new Vector2(Large ? 1f : 0.9f, 1f));
                if (IconEase > 0f)
                {
                    float num2 = Ease.CubeIn(IconEase);
                    Color color2 = Color.Lerp(Color.White, Color.Black, Faded * 0.6f) * num2;
                    Icon.DrawCentered(renderPosition, color2, (Bg.Width - 50) / (float)Icon.Width * num * (2.5f - num2 * 1.5f));
                }
            }
        }

        public AreaKey Area;

        public AreaStats RealStats;

        public AreaStats DisplayedStats;

        public AreaData Data;

        public Overworld.StartMode OverworldStartMode;

        public bool EnteringChapter;

        public const int ContentOffsetX = 440;

        public const int NoStatsHeight = 300;

        public const int StatsHeight = 540;

        public const int CheckpointsHeight = 730;

        private bool initialized;

        private string chapter = "";

        private bool selectingMode = true;

        private float height;

        private bool resizing;

        private Wiggler wiggler;

        private Wiggler modeAppearWiggler;

        private MTexture card = new();

        private Vector2 contentOffset;

        private float spotlightRadius;

        private float spotlightAlpha;

        private Vector2 spotlightPosition;

        private AreaCompleteTitle remixUnlockText;

        private StrawberriesCounter strawberries = new(true, 0, 0, true);

        private CollectableCounter cassettes = new(true, 0, "collectables/XaphanHelper/cassette", 0, true);

        private CollectableCounter heartsASide = new(true, 0, "collectables/XaphanHelper/heart", 0, true);

        private CollectableCounter heartsBSide = new(true, 0, "collectables/XaphanHelper/heartb", 0, true);

        private CollectableCounter heartsCSide = new(true, 0, "collectables/XaphanHelper/heartc", 0, true);

        private Vector2 strawberriesOffset;

        private Vector2 cassettesOffset;

        private Vector2 heartsASideOffset;

        private Vector2 heartsBSideOffset;

        private Vector2 heartsCSideOffset;

        private DeathsCounter deathsASide = new(AreaMode.Normal, true, 0);

        private DeathsCounter deathsBSide = new(AreaMode.BSide, true, 0);

        private DeathsCounter deathsCSide = new(AreaMode.CSide, true, 0);

        private Vector2 deathsASideOffset;

        private Vector2 deathsBSideOffset;

        private Vector2 deathsCSideOffset;

        private int checkpoint;

        private List<Option> modes = new();

        private List<Option> checkpoints = new();

        private EventInstance bSideUnlockSfx;

        private bool instantClose;

        public Vector2 OpenPosition => new(1070f, 100f);

        public Vector2 ClosePosition => new(2220f, 100f);

        public Vector2 IconOffset => new(690f, 86f);

        private Vector2 OptionsRenderPosition => Position + new Vector2(contentOffset.X, 128f + height);

        public Dictionary<int, int> chapterCheckpoints = new();

        private bool Leaving;

        private int option
        {
            get
            {
                if (!selectingMode)
                {
                    return checkpoint;
                }
                return (int)Area.Mode;
            }
            set
            {
                if (selectingMode)
                {
                    Area.Mode = (AreaMode)value;
                }
                else
                {
                    checkpoint = value;
                }
            }
        }

        private List<Option> options
        {
            get
            {
                if (!selectingMode)
                {
                    return checkpoints;
                }
                return modes;
            }
        }

        public CustomOuiChapterPanel()
        {
            Position = new Vector2(10000, 0);
            Add(strawberries);
            Add(cassettes);
            Add(heartsASide);
            Add(heartsBSide);
            Add(heartsCSide);
            Add(deathsASide);
            Add(deathsBSide);
            Add(deathsCSide);
            deathsASide.CanWiggle = false;
            deathsBSide.CanWiggle = false;
            deathsCSide.CanWiggle = false;
            strawberries.CanWiggle = false;
            strawberries.OverworldSfx = true;
            cassettes.CanWiggle = false;
            cassettes.OverworldSfx = true;
            heartsASide.CanWiggle = false;
            heartsASide.OverworldSfx = true;
            heartsBSide.CanWiggle = false;
            heartsBSide.OverworldSfx = true;
            heartsCSide.CanWiggle = false;
            heartsCSide.OverworldSfx = true;
            Add(wiggler = Wiggler.Create(0.4f, 4f));
            Add(modeAppearWiggler = Wiggler.Create(0.4f, 4f));
        }

        public override IEnumerator Enter(Oui from)
        {
            Leaving = false;
            if (instantClose || !XaphanModule.useMergeChaptersController)
            {
                return EnterClose(from);
            }
            return orig_Enter(from);
        }

        private void Reset()
        {
            Area = SaveData.Instance.LastArea_Safe;
            if (XaphanModule.MergeChaptersControllerKeepPrologue)
            {
                Area.ID -= 1;
                if (Area.ID > SaveData.Instance.LevelSetStats.AreaOffset)
                {
                    Area.ID = SaveData.Instance.LevelSetStats.AreaOffset;
                }
            }
            else
            {
                Area.ID = SaveData.Instance.LevelSetStats.AreaOffset;
            }
            Data = AreaData.Areas[Area.ID + (XaphanModule.MergeChaptersControllerKeepPrologue ? 1 : 0)];
            RealStats = SaveData.Instance.Areas_Safe[Area.ID];
            if (SaveData.Instance.CurrentSession_Safe != null && SaveData.Instance.CurrentSession_Safe.OldStats != null && SaveData.Instance.CurrentSession_Safe.Area.ID == Area.ID)
            {
                DisplayedStats = SaveData.Instance.CurrentSession_Safe.OldStats;
                SaveData.Instance.CurrentSession_Safe = null;
            }
            else
            {
                DisplayedStats = RealStats;
            }
            height = GetModeHeight();
            modes.Clear();
            bool flag = false;
            /*if (!Data.Interlude_Safe && Data.HasMode(AreaMode.BSide) && (DisplayedStats.Cassette || ((SaveData.Instance.DebugMode || SaveData.Instance.CheatMode) && DisplayedStats.Cassette == RealStats.Cassette)))
            {
                flag = true;
            }
            bool num = !Data.Interlude_Safe && Data.HasMode(AreaMode.CSide) && SaveData.Instance.UnlockedModes >= 3 && Celeste.PlayMode != Celeste.PlayModes.Event;*/
            modes.Add(new Option
            {
                Label = Dialog.Clean(Data.Interlude_Safe ? "FILE_BEGIN" : "overworld_normal").ToUpper(),
                Icon = GFX.Gui["menu/play"],
                ID = "A"
            });
            if (flag)
            {
                AddRemixButton();
            }
            /*if (num)
            {
                modes.Add(new Option
                {
                    Label = Dialog.Clean("overworld_remix2"),
                    Icon = GFX.Gui["menu/rmx2"],
                    ID = "C"
                });
            }*/
            selectingMode = true;
            UpdateStats(wiggle: false);
            SetStatsPosition(approach: false);
            for (int i = 0; i < options.Count; i++)
            {
                options[i].SlideTowards(i, options.Count, snap: true);
            }
            chapter = Dialog.Get("area_chapter").Replace("{x}", Area.ChapterIndex.ToString().PadLeft(2));
            contentOffset = new Vector2(440f, 120f);
            initialized = true;
        }

        private int GetModeHeight()
        {
            int statLines = 0;
            if (strawberries.Visible || cassettes.Visible)
            {
                statLines++;
            }
            if (heartsASide.Visible || heartsBSide.Visible || heartsCSide.Visible)
            {
                statLines++;
            }
            if (deathsASide.Visible || deathsBSide.Visible || deathsCSide.Visible)
            {
                statLines++;
            }
            if (statLines == 1)
            {
                return 442;
            }
            else if (statLines == 2)
            {
                return 532;
            }
            else if (statLines == 3)
            {
                return 617;
            }
            return 300;
        }

        private Option AddRemixButton()
        {
            Option option = new()
            {
                Label = Dialog.Clean("overworld_remix"),
                Icon = GFX.Gui["menu/remix"],
                ID = "B"
            };
            modes.Insert(1, option);
            return option;
        }

        private FieldInfo OuiChapterSelect_icons = typeof(OuiChapterSelect).GetField("icons", BindingFlags.Instance | BindingFlags.NonPublic);

        private FieldInfo OuiChapterSelect_inputDelay = typeof(OuiChapterSelect).GetField("inputDelay", BindingFlags.Instance | BindingFlags.NonPublic);

        public override IEnumerator Leave(Oui next)
        {
            Leaving = true;
            OuiChapterSelect uI = Overworld.GetUI<OuiChapterSelect>();
            /*foreach (OuiChapterSelectIcon icon in (List<OuiChapterSelectIcon>)OuiChapterSelect_icons.GetValue(uI))
            {
                if (icon.IsSelected)
                {
                    icon.Unselect();
                }
            }*/
            OuiChapterSelect_inputDelay.SetValue(uI, 0.6f);
            Overworld.Mountain.EaseCamera(Area.ID, Data.MountainZoom, 1f, true);
            Add(new Coroutine(EaseOut()));
            //yield return 0.6f;
            yield break;
        }

        public IEnumerator EaseOut(bool removeChildren = true)
        {
            for (float p = 0f; p < 1f; p += Engine.DeltaTime * 4f)
            {
                Position = OpenPosition + (ClosePosition - OpenPosition) * Ease.CubeIn(p);
                yield return null;
            }
            if (!Selected)
            {
                Visible = false;
            }
        }

        public void Start(int chapterIndex, string checkpoint = null)
        {
            Focused = false;
            Audio.Play("event:/ui/world_map/chapter/checkpoint_start");
            if (checkpoint != null)
            {
                string[] str = checkpoint.Split('|');
                checkpoint = str[1];
            }
            Add(new Coroutine(StartRoutine(chapterIndex, checkpoint)));
        }

        private IEnumerator StartRoutine(int chapterIndex, string checkpoint = null)
        {
            EnteringChapter = true;
            Overworld.Maddy.Hide(down: false);
            Overworld.Mountain.EaseCamera(Area.ID, Data.MountainZoom, 1f, true);
            Add(new Coroutine(EaseOut(removeChildren: false)));
            yield return 0.2f;
            ScreenWipe.WipeColor = Color.Black;
            AreaData.Get(Area).Wipe(Overworld, false, null);
            Audio.SetMusic(null);
            Audio.SetAmbience(null);
            yield return 0.5f;
            LevelEnter.Go(new Session(new AreaKey(SaveData.Instance.LevelSetStats.AreaOffset + ((XaphanModule.MergeChaptersControllerKeepPrologue && chapterIndex == 0) ? 1 : 0) + chapterIndex, Area.Mode), checkpoint)
            {
                Time = XaphanModule.ModSaveData.SavedTime.ContainsKey(SaveData.Instance.LevelSetStats.Name) ? XaphanModule.ModSaveData.SavedTime[SaveData.Instance.LevelSetStats.Name] : 0L,
                DoNotLoad = XaphanModule.ModSaveData.SavedNoLoadEntities.ContainsKey(SaveData.Instance.LevelSetStats.Name) ? XaphanModule.ModSaveData.SavedNoLoadEntities[SaveData.Instance.LevelSetStats.Name] : new HashSet<EntityID>(),
                Strawberries = XaphanModule.ModSaveData.SavedSessionStrawberries.ContainsKey(SaveData.Instance.LevelSetStats.Name) ? XaphanModule.ModSaveData.SavedSessionStrawberries[SaveData.Instance.LevelSetStats.Name] : new HashSet<EntityID>()
            }, fromSaveData: false);
        }

        private void Swap()
        {
            Focused = false;
            Overworld.ShowInputUI = !selectingMode;
            Add(new Coroutine(SwapRoutine()));
        }

        private IEnumerator SwapRoutine()
        {
            float fromHeight = height;
            int toHeight = selectingMode ? 730 : GetModeHeight();
            resizing = true;
            PlayExpandSfx(fromHeight, toHeight);
            float offset = 800f;
            for (float p2 = 0f; p2 < 1f; p2 += Engine.DeltaTime * 4f)
            {
                yield return null;
                contentOffset.X = 440f + offset * Ease.CubeIn(p2);
                height = MathHelper.Lerp(fromHeight, toHeight, Ease.CubeOut(p2 * 0.5f));
            }
            selectingMode = !selectingMode;
            if (!selectingMode)
            {
                HashSet<string> hashSet = _GetCheckpoints(SaveData.Instance, Area);
                int siblings = hashSet.Count + 1;
                checkpoints.Clear();
                checkpoints.Add(new Option
                {
                    Label = Dialog.Clean("overworld_start"),
                    BgColor = Calc.HexToColor("eabe26"),
                    Icon = GFX.Gui["areaselect/startpoint"],
                    CheckpointLevelName = null,
                    CheckpointRotation = Calc.Random.Choose(-1, 1) * Calc.Random.Range(0.05f, 0.2f),
                    CheckpointOffset = new Vector2(Calc.Random.Range(-16, 16), Calc.Random.Range(-16, 16)),
                    Large = false,
                    Siblings = siblings
                });
                foreach (string item in hashSet)
                {
                    checkpoints.Add(new Option
                    {
                        Label = GetCheckpointName(Area, item),
                        Icon = GFX.Gui["areaselect/checkpoint"],
                        CheckpointLevelName = item,
                        CheckpointRotation = Calc.Random.Choose(-1, 1) * Calc.Random.Range(0.05f, 0.2f),
                        CheckpointOffset = new Vector2(Calc.Random.Range(-16, 16), Calc.Random.Range(-16, 16)),
                        Large = false,
                        Siblings = siblings,
                        ChapterIndex = int.Parse(item.Split('|')[2])
                    });
                }
                if (!RealStats.Modes[(int)Area.Mode].Completed && !SaveData.Instance.DebugMode && !SaveData.Instance.CheatMode)
                {
                    option = checkpoints.Count - 1;
                    for (int i = 0; i < checkpoints.Count - 1; i++)
                    {
                        options[i].CheckpointSlideOut = 1f;
                    }
                }
                else
                {
                    option = 0;
                }
                for (int j = 0; j < options.Count; j++)
                {
                    options[j].SlideTowards(j, options.Count, snap: true);
                }
            }
            options[option].Pop = 1f;
            for (float p2 = 0f; p2 < 1f; p2 += Engine.DeltaTime * 4f)
            {
                yield return null;
                height = MathHelper.Lerp(fromHeight, toHeight, Ease.CubeOut(Math.Min(1f, 0.5f + p2 * 0.5f)));
                contentOffset.X = 440f + offset * (1f - Ease.CubeOut(p2));
            }
            contentOffset.X = 440f;
            height = toHeight;
            Focused = true;
            resizing = false;
        }

        public string GetCheckpointName(AreaKey area, string level)
        {
            if (!string.IsNullOrEmpty(level))
            {
                string[] str = level.Split('|');
                string sid = str[0];
                string levelName = str[1];
                if (!string.IsNullOrEmpty(sid))
                {
                    area = (AreaData.Get(sid)?.ToKey(area.Mode) ?? area);
                    level = levelName;
                }
            }
            if (string.IsNullOrEmpty(level))
            {
                return "START";
            }
            CheckpointData checkpoint = AreaData.GetCheckpoint(area, level);
            if (checkpoint != null)
            {
                return Dialog.Clean(checkpoint.Name);
            }
            else
            {
                return Dialog.Clean(area.SID + "_" + level);
            }
        }

        public override void Update()
        {
            if (XaphanModule.ModSaveData != null)
            {
                XaphanModule.ModSaveData.LoadedPlayer = false;
            }
            if (Selected && Focused && Input.QuickRestart.Pressed)
            {
                Overworld.Goto<OuiChapterSelect>();
                Overworld.Goto<OuiMapSearch>();
            }
            else if (instantClose)
            {
                Overworld.Goto<OuiChapterSelect>();
                Visible = false;
                instantClose = false;
            }
            else
            {
                if (!initialized)
                {
                    return;
                }
                base.Update();
                if (Visible && !Leaving)
                {
                    OuiChapterSelect uI = Overworld.GetUI<OuiChapterSelect>();
                    foreach (OuiChapterSelectIcon icon in (List<OuiChapterSelectIcon>)OuiChapterSelect_icons.GetValue(uI))
                    {
                        icon.Visible = true;
                    }
                }
                for (int i = 0; i < options.Count; i++)
                {
                    Option option = options[i];
                    option.Pop = Calc.Approach(option.Pop, (this.option == i) ? 1f : 0f, Engine.DeltaTime * 4f);
                    option.Appear = Calc.Approach(option.Appear, 1f, Engine.DeltaTime * 3f);
                    option.CheckpointSlideOut = Calc.Approach(option.CheckpointSlideOut, (this.option > i) ? 1 : 0, Engine.DeltaTime * 4f);
                    option.Faded = Calc.Approach(option.Faded, (this.option != i && !option.Appeared) ? 1 : 0, Engine.DeltaTime * 4f);
                    option.SlideTowards(i, options.Count, snap: false);
                }
                if (selectingMode && !resizing)
                {
                    height = Calc.Approach(height, GetModeHeight(), Engine.DeltaTime * 1600f);
                }
                if (Selected && Focused)
                {
                    if (Input.MenuLeft.Pressed && option > 0)
                    {
                        Audio.Play("event:/ui/world_map/chapter/tab_roll_left");
                        this.option--;
                        wiggler.Start();
                        if (selectingMode)
                        {
                            UpdateStats();
                            PlayExpandSfx(height, GetModeHeight());
                        }
                        else
                        {
                            Audio.Play("event:/ui/world_map/chapter/checkpoint_photo_add");
                        }
                    }
                    else if (Input.MenuRight.Pressed && option + 1 < options.Count)
                    {
                        Audio.Play("event:/ui/world_map/chapter/tab_roll_right");
                        this.option++;
                        wiggler.Start();
                        if (selectingMode)
                        {
                            UpdateStats();
                            PlayExpandSfx(height, GetModeHeight());
                        }
                        else
                        {
                            Audio.Play("event:/ui/world_map/chapter/checkpoint_photo_remove");
                        }
                    }
                    else if (Input.MenuConfirm.Pressed)
                    {
                        if (selectingMode)
                        {
                            if (XaphanModule.MergeChaptersControllerMode != "Classic")
                            {
                                Start(0);
                            }
                            else
                            {
                                bool FoundCheckpoints = false;
                                for (int i = 0; i <= SaveData.Instance.LevelSetStats.MaxArea; i++)
                                {
                                    if (SaveData.Instance.FoundAnyCheckpoints(new AreaKey(SaveData.Instance.LevelSetStats.AreaOffset + i)) || XaphanModule.ModSaveData.Checkpoints.Contains(SaveData.Instance.LevelSetStats.Name + "|" + i))
                                    {
                                        FoundCheckpoints = true;
                                        if (XaphanModule.MergeChaptersControllerKeepPrologue && i == 1 && !SaveData.Instance.FoundAnyCheckpoints(new AreaKey(SaveData.Instance.LevelSetStats.AreaOffset + i)) && XaphanModule.ModSaveData.Checkpoints.Contains(SaveData.Instance.LevelSetStats.Name + "|" + i))
                                        {
                                            FoundCheckpoints = false;
                                        }
                                    }
                                }
                                if (FoundCheckpoints)
                                {
                                    Audio.Play("event:/ui/world_map/chapter/level_select");
                                    Swap();
                                }
                                else
                                {
                                    Start(XaphanModule.MergeChaptersControllerKeepPrologue ? 1 : 0);
                                }
                            }
                        }
                        else
                        {
                            Start(options[option].ChapterIndex, options[option].CheckpointLevelName);
                        }
                    }
                    else if (Input.MenuCancel.Pressed)
                    {
                        if (selectingMode)
                        {
                            Audio.Play("event:/ui/world_map/chapter/back");
                            Overworld.Goto<OuiChapterSelect>();
                        }
                        else
                        {
                            Audio.Play("event:/ui/world_map/chapter/checkpoint_back");
                            Swap();
                        }
                    }
                }
                SetStatsPosition(approach: true);
            }
        }

        public override void Render()
        {
            if (!initialized)
            {
                return;
            }
            Vector2 optionsRenderPosition = OptionsRenderPosition;
            for (int i = 0; i < options.Count; i++)
            {
                if (!options[i].OnTopOfUI)
                {
                    options[i].Render(optionsRenderPosition, option == i, wiggler, modeAppearWiggler);
                }
            }
            bool flag = false;
            if (XaphanModule.ModSaveData.SavedFlags.Contains(Area.LevelSet + "_GoldenStrawberryGet"))
            {
                flag = true;
            }
            MTexture mTexture = GFX.Gui[(!flag) ? _ModCardTexture("areaselect/cardtop") : _ModCardTexture("areaselect/cardtop_golden")];
            mTexture.Draw(Position + new Vector2(0f, -32f));
            MTexture mTexture2 = GFX.Gui[(!flag) ? _ModCardTexture("areaselect/card") : _ModCardTexture("areaselect/card_golden")];
            card = mTexture2.GetSubtexture(0, mTexture2.Height - (int)height, mTexture2.Width, (int)height, card);
            card.Draw(Position + new Vector2(0f, -32 + mTexture.Height));
            for (int j = 0; j < options.Count; j++)
            {
                if (options[j].OnTopOfUI)
                {
                    options[j].Render(optionsRenderPosition, option == j, wiggler, modeAppearWiggler);
                }
            }
            ActiveFont.Draw(options[option].Label, optionsRenderPosition + new Vector2(0f, -140f), Vector2.One * 0.5f, Vector2.One * (1f + wiggler.Value * 0.1f), Color.Black * 0.8f);
            if (selectingMode)
            {
                strawberries.Position = contentOffset + new Vector2(0f, 130f) + strawberriesOffset;
                cassettes.Position = contentOffset + new Vector2(0, 130f) + cassettesOffset;
                heartsASide.Position = contentOffset + new Vector2(0, 210f) + heartsASideOffset;
                heartsBSide.Position = contentOffset + new Vector2(0, 210f) + heartsBSideOffset;
                heartsCSide.Position = contentOffset + new Vector2(0, 210f) + heartsCSideOffset;
                deathsASide.Position = contentOffset + new Vector2(0, 290f) + deathsASideOffset;
                deathsBSide.Position = contentOffset + new Vector2(0, 290f) + deathsBSideOffset;
                deathsCSide.Position = contentOffset + new Vector2(0, 290f) + deathsCSideOffset;
                base.Render();
            }
            if (!selectingMode)
            {
                Vector2 center = Position + new Vector2(contentOffset.X, 340f);
                chapterCheckpoints.Clear();
                int CurrentChapter = 0;
                int totalCurrentChapterCheckpoints = 0;
                for (int i = 0; i <= options.Count() - 1; i++)
                {
                    if (options[i].ChapterIndex == CurrentChapter)
                    {
                        totalCurrentChapterCheckpoints++;
                    }
                    else
                    {
                        if (!chapterCheckpoints.ContainsKey(CurrentChapter))
                        {
                            if (CurrentChapter == 0)
                            {
                                totalCurrentChapterCheckpoints -= 1;
                            }
                            chapterCheckpoints.Add(CurrentChapter, totalCurrentChapterCheckpoints);
                            CurrentChapter++;
                            totalCurrentChapterCheckpoints = 1;
                        }
                    }
                }
                if (!chapterCheckpoints.ContainsKey(CurrentChapter))
                {
                    chapterCheckpoints.Add(CurrentChapter, totalCurrentChapterCheckpoints);
                }
                int CurrentDrawChapter = chapterCheckpoints.Last().Key;
                for (int num = options.Count - 1; num >= 0; num--)
                {
                    string[] str = null;
                    if (options[num].CheckpointLevelName != null)
                    {
                        str = options[num].CheckpointLevelName.Split('|');
                    }
                    string FixedCheckpointName = str != null ? str[1] : options[num].CheckpointLevelName;
                    if (options[num].ChapterIndex == CurrentDrawChapter)
                    {
                        DrawCheckpoint(center, options[num], AreaData.GetCheckpointID(new AreaKey(SaveData.Instance.LevelSetStats.AreaOffset + options[num].ChapterIndex, Area.Mode), FixedCheckpointName) + 1, options[num].ChapterIndex);
                    }
                    else
                    {
                        CurrentDrawChapter--;
                        DrawCheckpoint(center, options[num], AreaData.GetCheckpointID(new AreaKey(SaveData.Instance.LevelSetStats.AreaOffset + options[num].ChapterIndex, Area.Mode), FixedCheckpointName) + 1, options[num].ChapterIndex);
                    }
                }
            }
            GFX.Gui["areaselect/title"].Draw(Position + new Vector2(-60f, 0f), Vector2.Zero, Data.TitleBaseColor);
            GFX.Gui["areaselect/accent"].Draw(Position + new Vector2(-60f, 0f), Vector2.Zero, Data.TitleAccentColor);
            string text = ActiveFont.FontSize.AutoNewline(Dialog.Clean(SaveData.Instance.LevelSetStats.Name), 600);
            ActiveFont.Draw(text, Position + IconOffset + new Vector2(-100 - ActiveFont.Measure(text).X / 2, 0f), new Vector2(0.5f, 0.5f), Vector2.One * 1f, Data.TitleTextColor * 0.8f);
            if (spotlightAlpha > 0f)
            {
                HiresRenderer.EndRender();
                SpotlightWipe.DrawSpotlight(spotlightPosition, spotlightRadius, Color.Black * spotlightAlpha);
                HiresRenderer.BeginRender();
            }
        }

        private void DrawCheckpoint(Vector2 center, Option option, int checkpointIndex, int chapterIndex)
        {
            MTexture checkpointPreview = GetCheckpointPreview(Area, option.CheckpointLevelName, chapterIndex);
            MTexture mTexture = MTN.Checkpoints["polaroid"];
            float checkpointRotation = option.CheckpointRotation;
            Vector2 vector = center + option.CheckpointOffset;
            vector += Vector2.UnitX * 800f * Ease.CubeIn(option.CheckpointSlideOut);
            mTexture.DrawCentered(vector, Color.White, 0.75f, checkpointRotation);
            MTexture mTexture2 = GFX.Gui["collectables/strawberry"];
            if (checkpointPreview != null)
            {
                Vector2 scale = Vector2.One * 0.75f;
                if (SaveData.Instance.Assists.MirrorMode)
                {
                    scale.X = 0f - scale.X;
                }
                scale *= 720f / checkpointPreview.Width;
                HiresRenderer.EndRender();
                HiresRenderer.BeginRender(BlendState.AlphaBlend, SamplerState.PointClamp);
                checkpointPreview.DrawCentered(vector, Color.White, scale, checkpointRotation);
                HiresRenderer.EndRender();
                HiresRenderer.BeginRender();
            }
            int mode = (int)Area.Mode;
            RealStats = SaveData.Instance.Areas_Safe[Area.ID + chapterIndex + ((XaphanModule.MergeChaptersControllerKeepPrologue && chapterIndex == 0) ? 1 : 0)];
            if (!RealStats.Modes[mode].Completed && !SaveData.Instance.CheatMode && !SaveData.Instance.DebugMode)
            {
                return;
            }
            Vector2 vec = new(300f, 220f);
            vec = vector + vec.Rotate(checkpointRotation);
            int num = 0;
            AreaData areaData = AreaData.Areas[Area.ID + chapterIndex + ((XaphanModule.MergeChaptersControllerKeepPrologue && chapterIndex == 0) ? 1 : 0)];
            num = ((checkpointIndex != 0) ? areaData.Mode[mode].Checkpoints[checkpointIndex - 1].Strawberries : areaData.Mode[mode].StartStrawberries);
            bool[] array = new bool[num];
            foreach (EntityID strawberry in RealStats.Modes[mode].Strawberries)
            {
                for (int i = 0; i < num; i++)
                {
                    EntityData entityData = areaData.Mode[mode].StrawberriesByCheckpoint[checkpointIndex, i];
                    if (entityData != null && entityData.Level.Name == strawberry.Level && entityData.ID == strawberry.ID)
                    {
                        array[i] = true;
                    }
                }
            }
            Vector2 value = Calc.AngleToVector(checkpointRotation, 1f);
            Vector2 vector2 = vec - value * num * 44f;
            if (Area.Mode == AreaMode.Normal && areaData.CassetteCheckpointIndex == checkpointIndex)
            {
                Vector2 position = vector2 - value * 60f;
                if (RealStats.Cassette)
                {
                    MTN.Journal["cassette"].DrawCentered(position, Color.White, 1f, checkpointRotation);
                }
                else
                {
                    MTN.Journal["cassette_outline"].DrawCentered(position, Color.DarkGray, 1f, checkpointRotation);
                }
            }
            for (int j = 0; j < num; j++)
            {
                mTexture2.DrawCentered(vector2, array[j] ? Color.White : (Color.Black * 0.3f), 0.5f, checkpointRotation);
                vector2 += value * 44f;
            }
        }

        private void UpdateStats(bool wiggle = true, bool? overrideStrawberryWiggle = null, bool? overrideDeathWiggle = null, bool? overrideHeartWiggle = null)
        {
            LevelSetStats levelSetStats = SaveData.Instance.LevelSetStats;
            AreaModeStats areaModeStats = DisplayedStats.Modes[(int)Area.Mode];
            AreaData areaData = AreaData.Get(Area);
            strawberries.Visible = (levelSetStats.TotalStrawberries != 0);
            strawberries.Amount = levelSetStats.TotalStrawberries;
            strawberries.OutOf = levelSetStats.MaxStrawberries;
            strawberries.Golden = (Area.Mode != AreaMode.Normal);
            cassettes.Visible = (levelSetStats.TotalCassettes != 0);
            cassettes.Amount = levelSetStats.TotalCassettes;
            cassettes.OutOf = levelSetStats.MaxCassettes;
            int ASideDeaths = 0;
            int BSideDeaths = 0;
            int CSideDeaths = 0;
            int ASideHearts = 0;
            int BSideHearts = 0;
            int CSideHearts = 0;
            int ASideTotalHearts = 0;
            int BSideTotalHearts = 0;
            int CSideTotalHearts = 0;
            foreach (AreaStats areaStats in SaveData.Instance.Areas_Safe)
            {
                if (areaStats.LevelSet == Area.LevelSet && (XaphanModule.MergeChaptersControllerKeepPrologue ? areaStats.ID != SaveData.Instance.LevelSetStats.AreaOffset : true))
                {
                    for (int i = 0; i < 3; i++)
                    {
                        AreaData areaData2 = AreaData.Get(areaStats.ID_Safe);
                        if (areaData2.HasMode((AreaMode)i))
                        {
                            if (i == 0)
                            {
                                ASideDeaths += areaStats.Modes[i].Deaths;
                                if (areaStats.Modes[i].HeartGem)
                                {
                                    ASideHearts++;
                                }
                            }
                            if (i == 1)
                            {
                                BSideDeaths += areaStats.Modes[i].Deaths;
                                if (areaStats.Modes[i].HeartGem)
                                {
                                    BSideHearts++;
                                }
                                BSideTotalHearts++;
                            }
                            if (i == 2)
                            {
                                CSideDeaths += areaStats.Modes[i].Deaths;
                                if (areaStats.Modes[i].HeartGem)
                                {
                                    CSideHearts++;
                                }
                                CSideTotalHearts++;
                            }
                        }
                    }
                }
            }
            ASideTotalHearts = levelSetStats.MaxHeartGems - BSideTotalHearts - CSideTotalHearts;
            heartsASide.Visible = (ASideHearts != 0);
            heartsASide.Amount = ASideHearts;
            heartsASide.OutOf = ASideTotalHearts;
            heartsBSide.Visible = (BSideHearts != 0);
            heartsBSide.Amount = BSideHearts;
            heartsBSide.OutOf = BSideTotalHearts;
            heartsCSide.Visible = (CSideHearts != 0);
            heartsCSide.Amount = CSideHearts;
            heartsCSide.OutOf = CSideTotalHearts;
            deathsASide.Visible = (ASideDeaths != 0);
            deathsASide.Amount = ASideDeaths;
            deathsBSide.Visible = (BSideDeaths != 0);
            deathsBSide.Amount = BSideDeaths;
            deathsCSide.Visible = (CSideDeaths != 0);
            deathsCSide.Amount = CSideDeaths;
        }

        private void SetStatsPosition(bool approach)
        {
            bool hasBerriesOrCassettes = false;
            bool hasAnyHearts = false;
            if (strawberries.Visible || cassettes.Visible)
            {
                hasBerriesOrCassettes = true;
            }
            if (heartsASide.Visible || heartsBSide.Visible || heartsCSide.Visible)
            {
                hasAnyHearts = true;
            }
            strawberriesOffset = Approach(strawberriesOffset, new Vector2(cassettes.Visible ? -125 : 0, 0), !approach);
            cassettesOffset = Approach(cassettesOffset, new Vector2(strawberries.Visible ? 125 : 0, 0), !approach);
            heartsASideOffset = Approach(heartsASideOffset, new Vector2(((heartsBSide.Visible && !heartsCSide.Visible) || (!heartsBSide.Visible && heartsCSide.Visible)) ? -125 : ((heartsBSide.Visible && heartsCSide.Visible) ? -250 : 0), hasBerriesOrCassettes ? 0 : -80), !approach);
            heartsBSideOffset = Approach(heartsBSideOffset, new Vector2((heartsASide.Visible && !heartsCSide.Visible) ? 125 : ((!heartsASide.Visible && heartsCSide.Visible) ? -125 : 0), hasBerriesOrCassettes ? 0 : -80), !approach);
            heartsCSideOffset = Approach(heartsCSideOffset, new Vector2(((heartsASide.Visible && !heartsBSide.Visible) || (!heartsASide.Visible && heartsBSide.Visible)) ? 125 : ((heartsASide.Visible && heartsBSide.Visible) ? 250 : 0), hasBerriesOrCassettes ? 0 : -80), !approach);
            deathsASideOffset = Approach(deathsASideOffset, new Vector2(((deathsBSide.Visible && !deathsCSide.Visible) || (!deathsBSide.Visible && deathsCSide.Visible)) ? -125 : ((deathsBSide.Visible && deathsCSide.Visible) ? -250 : 0), (hasBerriesOrCassettes && hasAnyHearts) ? 0 : (((hasBerriesOrCassettes && !hasAnyHearts) || (!hasBerriesOrCassettes && hasAnyHearts)) ? -80 : -167)), !approach);
            deathsBSideOffset = Approach(deathsBSideOffset, new Vector2((deathsASide.Visible && !deathsCSide.Visible) ? 125 : ((!deathsASide.Visible && deathsCSide.Visible) ? -125 : 0), (hasBerriesOrCassettes && hasAnyHearts) ? 0 : (((hasBerriesOrCassettes && !hasAnyHearts) || (!hasBerriesOrCassettes && hasAnyHearts)) ? -80 : -167)), !approach);
            deathsCSideOffset = Approach(deathsCSideOffset, new Vector2(((deathsASide.Visible && !deathsBSide.Visible) || (!deathsASide.Visible && deathsBSide.Visible)) ? 125 : ((deathsASide.Visible && deathsBSide.Visible) ? 250 : 0), (hasBerriesOrCassettes && hasAnyHearts) ? 0 : (((hasBerriesOrCassettes && !hasAnyHearts) || (!hasBerriesOrCassettes && hasAnyHearts)) ? -80 : -167)), !approach);
        }

        private Vector2 Approach(Vector2 from, Vector2 to, bool snap)
        {
            if (snap)
            {
                return to;
            }
            return from += (to - from) * (1f - (float)Math.Pow(0.0010000000474974513, Engine.DeltaTime));
        }

        private IEnumerator IncrementStatsDisplay(AreaModeStats modeStats, AreaModeStats newModeStats, bool doStrawberries, bool doRemixUnlock)
        {
            if (doStrawberries)
            {
                strawberries.CanWiggle = true;
                strawberries.Visible = true;
                while (newModeStats.TotalStrawberries > modeStats.TotalStrawberries)
                {
                    int num = newModeStats.TotalStrawberries - modeStats.TotalStrawberries;
                    if (num < 3)
                    {
                        yield return 0.3f;
                    }
                    else if (num < 8)
                    {
                        yield return 0.2f;
                    }
                    else
                    {
                        yield return 0.1f;
                        modeStats.TotalStrawberries++;
                    }
                    modeStats.TotalStrawberries++;
                    strawberries.Amount = modeStats.TotalStrawberries;
                    Input.Rumble(RumbleStrength.Light, RumbleLength.Short);
                }
                strawberries.CanWiggle = false;
                yield return 0.5f;
                if (newModeStats.Completed && !modeStats.Completed && Area.Mode == AreaMode.Normal)
                {
                    yield return 0.25f;
                    Audio.Play((strawberries.Amount >= Data.Mode[0].TotalStrawberries) ? "event:/ui/postgame/strawberry_total_all" : "event:/ui/postgame/strawberry_total");
                    strawberries.OutOf = Data.Mode[0].TotalStrawberries;
                    strawberries.ShowOutOf = true;
                    strawberries.Wiggle();
                    modeStats.Completed = true;
                    Input.Rumble(RumbleStrength.Light, RumbleLength.Medium);
                    yield return 0.6f;
                }
            }
            if (doRemixUnlock)
            {
                bSideUnlockSfx = Audio.Play("event:/ui/postgame/unlock_bside");
                Option o = AddRemixButton();
                o.Appear = 0f;
                o.IconEase = 0f;
                o.Appeared = true;
                yield return 0.5f;
                spotlightPosition = o.GetRenderPosition(OptionsRenderPosition);
                for (float t2 = 0f; t2 < 1f; t2 += Engine.DeltaTime / 0.5f)
                {
                    spotlightAlpha = t2 * 0.9f;
                    spotlightRadius = 128f * Ease.CubeOut(t2);
                    yield return null;
                }
                yield return 0.3f;
                while ((o.IconEase += Engine.DeltaTime * 2f) < 1f)
                {
                    yield return null;
                }
                o.IconEase = 1f;
                modeAppearWiggler.Start();
                Input.Rumble(RumbleStrength.Medium, RumbleLength.Medium);
                remixUnlockText = new AreaCompleteTitle(spotlightPosition + new Vector2(0f, 80f), Dialog.Clean("OVERWORLD_REMIX_UNLOCKED"), 1f);
                remixUnlockText.Tag = Tags.HUD;
                Overworld.Add(remixUnlockText);
                yield return 1.5f;
                for (float t2 = 0f; t2 < 1f; t2 += Engine.DeltaTime / 0.5f)
                {
                    spotlightAlpha = (1f - t2) * 0.5f;
                    spotlightRadius = 128f + 128f * Ease.CubeOut(t2);
                    remixUnlockText.Alpha = 1f - Ease.CubeOut(t2);
                    yield return null;
                }
                remixUnlockText.RemoveSelf();
                remixUnlockText = null;
                o.Appeared = false;
            }
        }

        public IEnumerator IncrementStats(bool shouldAdvance)
        {
            Focused = false;
            Overworld.ShowInputUI = false;
            if (Data.Interlude_Safe)
            {
                if (shouldAdvance && OverworldStartMode == Overworld.StartMode.AreaComplete)
                {
                    yield return 1.2f;
                    Overworld.Goto<OuiChapterSelect>().AdvanceToNext();
                }
                else
                {
                    Focused = true;
                }
                yield return null;
                yield break;
            }
            AreaData data = Data;
            AreaStats displayedStats = DisplayedStats;
            AreaStats areaStats = SaveData.Instance.Areas_Safe[data.ID];
            AreaModeStats modeStats = displayedStats.Modes[(int)Area.Mode];
            AreaModeStats newModeStats = areaStats.Modes[(int)Area.Mode];
            bool doStrawberries = newModeStats.TotalStrawberries > modeStats.TotalStrawberries;
            bool doHeartGem = newModeStats.HeartGem && !modeStats.HeartGem;
            bool doDeaths = newModeStats.Deaths > modeStats.Deaths && (Area.Mode != 0 || newModeStats.Completed);
            bool doRemixUnlock = Area.Mode == AreaMode.Normal && data.HasMode(AreaMode.BSide) && areaStats.Cassette && !displayedStats.Cassette;
            if (doStrawberries | doHeartGem | doDeaths | doRemixUnlock)
            {
                yield return 0.8f;
            }
            bool skipped = false;
            Coroutine routine = new(IncrementStatsDisplay(modeStats, newModeStats, doStrawberries, doRemixUnlock));
            Add(routine);
            yield return null;
            while (!routine.Finished)
            {
                if (MInput.GamePads[0].Pressed(Buttons.Start) || MInput.Keyboard.Pressed(Keys.Enter))
                {
                    routine.Active = false;
                    routine.RemoveSelf();
                    skipped = true;
                    Audio.Stop(bSideUnlockSfx);
                    Audio.Play("event:/new_content/ui/skip_all");
                    break;
                }
                yield return null;
            }
            if (skipped && doRemixUnlock)
            {
                spotlightAlpha = 0f;
                spotlightRadius = 0f;
                if (remixUnlockText != null)
                {
                    remixUnlockText.RemoveSelf();
                    remixUnlockText = null;
                }
                if (modes.Count <= 1 || modes[1].ID != "B")
                {
                    AddRemixButton();
                }
                else
                {
                    Option option = modes[1];
                    option.IconEase = 1f;
                    option.Appear = 1f;
                    option.Appeared = false;
                }
            }
            DisplayedStats = RealStats;
            if (skipped)
            {
                doStrawberries = (doStrawberries && modeStats.TotalStrawberries != newModeStats.TotalStrawberries);
                doDeaths &= (modeStats.Deaths != newModeStats.Deaths);
                UpdateStats(true, doStrawberries, doDeaths, doHeartGem);
            }
            yield return null;
            if (shouldAdvance && OverworldStartMode == Overworld.StartMode.AreaComplete)
            {
                if ((!doDeaths && !doStrawberries && !doHeartGem) || Settings.Instance.SpeedrunClock != 0)
                {
                    yield return 1.2f;
                }
                Overworld.Goto<OuiChapterSelect>().AdvanceToNext();
            }
            else
            {
                Focused = true;
                Overworld.ShowInputUI = true;
            }
        }

        private float HandleDeathTick(int oldDeaths, int newDeaths, out int add)
        {
            int num = newDeaths - oldDeaths;
            if (num < 3)
            {
                add = 1;
                return 0.3f;
            }
            if (num < 8)
            {
                add = 2;
                return 0.2f;
            }
            if (num < 30)
            {
                add = 5;
                return 0.1f;
            }
            if (num < 100)
            {
                add = 10;
                return 0.1f;
            }
            if (num < 1000)
            {
                add = 25;
                return 0.1f;
            }
            add = 100;
            return 0.1f;
        }

        private void PlayExpandSfx(float currentHeight, float nextHeight)
        {
            if (nextHeight > currentHeight)
            {
                Audio.Play("event:/ui/world_map/chapter/pane_expand");
            }
            else if (nextHeight < currentHeight)
            {
                Audio.Play("event:/ui/world_map/chapter/pane_contract");
            }
        }

        public static string GetCheckpointPreviewName(AreaKey area, string level)
        {
            return _GetCheckpointPreviewName(area, level);
        }

        private MTexture GetCheckpointPreview(AreaKey area, string level, int chapterIndex = -1)
        {
            string checkpointPreviewName = GetCheckpointPreviewName(area, level);
            if (chapterIndex != -1 && checkpointPreviewName.Contains(AreaData.Areas[area.ID + chapterIndex].Mode[(int)area.Mode].MapData.StartLevel().Name) && (XaphanModule.ModSaveData.Checkpoints.Contains(SaveData.Instance.LevelSetStats.Name + "|" + chapterIndex) || SaveData.Instance.DebugMode || SaveData.Instance.CheatMode))
            {
                checkpointPreviewName = checkpointPreviewName.Replace(AreaData.Areas[area.ID + chapterIndex].Mode[(int)area.Mode].MapData.StartLevel().Name, "start");
            }
            if (area.ID == SaveData.Instance.LevelSetStats.AreaOffset && chapterIndex == 0 && XaphanModule.MergeChaptersControllerKeepPrologue)
            {
                checkpointPreviewName = checkpointPreviewName.Replace(AreaData.Areas[area.ID + chapterIndex].Name, AreaData.Areas[area.ID + chapterIndex + 1].Name);
                checkpointPreviewName = checkpointPreviewName.Replace(AreaData.Areas[area.ID + chapterIndex].Mode[(int)area.Mode].MapData.StartLevel().Name, "start");
            }
            if (MTN.Checkpoints.Has(checkpointPreviewName))
            {
                return MTN.Checkpoints[checkpointPreviewName];
            }
            return null;
        }

        internal static string _GetCheckpointPreviewName(AreaKey area, string level)
        {
            int num = level?.IndexOf('|') ?? (-1);
            if (num >= 0)
            {
                string[] str = level?.Split('|');
                level = str[0] + "|" + str[1];
                area = (AreaData.Get(level.Substring(0, num))?.ToKey(area.Mode) ?? area);
                level = level.Substring(num + 1);
            }
            string text = area.ToString();
            if (area.LevelSet != "Celeste")
            {
                text = area.GetSID();
            }
            if (level != null)
            {
                text = text + "_" + level;
            }
            if (MTN.Checkpoints.Has(text))
            {
                return text;
            }
            return string.Format("{0}/{1}/{2}", area.GetSID(), (char)(65 + area.Mode), level ?? "start");
        }

        public override bool IsStart(Overworld overworld, Overworld.StartMode start)
        {
            if ((start == Overworld.StartMode.AreaComplete || start == Overworld.StartMode.AreaQuit) && ((XaphanModule.useMergeChaptersController && !XaphanModule.MergeChaptersControllerKeepPrologue) || (XaphanModule.useMergeChaptersController && XaphanModule.MergeChaptersControllerKeepPrologue && SaveData.Instance.CurrentSession_Safe != null && SaveData.Instance.CurrentSession_Safe.Area.ChapterIndex > 0)))
            {
                bool shouldAdvance = start == Overworld.StartMode.AreaComplete && (Celeste.PlayMode == Celeste.PlayModes.Event || (SaveData.Instance.CurrentSession_Safe != null && SaveData.Instance.CurrentSession_Safe.ShouldAdvance));
                Position = OpenPosition;
                Reset();
                Add(new Coroutine(IncrementStats(shouldAdvance)));
                overworld.ShowInputUI = false;
                overworld.Mountain.SnapState(Data.MountainState);
                overworld.Mountain.SnapCamera(Area.ID, Data.MountainZoom);
                overworld.Mountain.EaseCamera(Area.ID, Data.MountainSelect, 1f, true);
                OverworldStartMode = start;
                return true;
            }
            Position = ClosePosition;
            return false;
        }

        public IEnumerator orig_Enter(Oui from)
        {
            Visible = true;
            Area.Mode = AreaMode.Normal;
            Reset();
            Overworld.Mountain.EaseCamera(Area.ID, Data.MountainSelect, null, true);
            for (float p = 0f; p < 1f; p += Engine.DeltaTime * 4f)
            {
                yield return null;
                Position = ClosePosition + (OpenPosition - ClosePosition) * Ease.CubeOut(p);
            }
            Position = OpenPosition;
            yield return null;
        }

        private IEnumerator EnterClose(Oui from)
        {
            Overworld.Goto<OuiChapterSelect>();
            Visible = false;
            instantClose = false;
            yield break;
        }

        private static HashSet<string> _GetCheckpoints(SaveData save, AreaKey area)
        {
            if (Celeste.PlayMode == Celeste.PlayModes.Event)
            {
                return new HashSet<string>();
            }
            AreaData areaData = AreaData.Areas[area.ID];
            ModeProperties mode = areaData.Mode[(int)area.Mode];
            HashSet<string> hashSet;
            hashSet = new HashSet<string>();
            int MaxAreas = save.LevelSetStats.MaxArea;
            for (int i = 0; i <= MaxAreas; i++)
            {
                HashSet<string> checkpointList = save.GetCheckpoints(new AreaKey(SaveData.Instance.LevelSetStats.AreaOffset + i));
                areaData = AreaData.Areas[area.ID + i];
                mode = areaData.Mode[(int)area.Mode];
                if (mode.Checkpoints != null)
                {
                    CheckpointData[] array = mode.Checkpoints;
                    if (i > 0 && (XaphanModule.ModSaveData.Checkpoints.Contains(SaveData.Instance.LevelSetStats.Name + "|" + i) || SaveData.Instance.DebugMode || SaveData.Instance.CheatMode))
                    {
                        if (!XaphanModule.MergeChaptersControllerKeepPrologue || (XaphanModule.MergeChaptersControllerKeepPrologue && i > 1))
                        {
                            hashSet.Add(areaData.SID + "|" + mode.MapData.StartLevel().Name + "|" + i);
                        }
                    }
                    foreach (CheckpointData checkpointData in array)
                    {
                        if (checkpointList.Contains(checkpointData.Level))
                        {
                            hashSet.Add((AreaData.Get(checkpointData.Area) ?? areaData).SID + "|" + checkpointData.Level + "|" + i);
                        }
                    }
                }
            }
            return hashSet;
        }

        private string _ModCardTexture(string textureName)
        {
            string name = AreaData.Areas[Area.ID + (XaphanModule.MergeChaptersControllerKeepPrologue ? 1 : 0)].Name;
            string text = textureName.Replace("areaselect/card", "areaselect/" + name + "_card");
            if (GFX.Gui.Has(text))
            {
                textureName = text;
                return textureName;
            }
            string str = SaveData.Instance?.LevelSet ?? "Celeste";
            string text2 = textureName.Replace("areaselect/", "areaselect/" + str + "/");
            if (GFX.Gui.Has(text2))
            {
                textureName = text2;
                return textureName;
            }
            return textureName;
        }
    }
}
