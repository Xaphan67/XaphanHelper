using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Celeste.Mod.XaphanHelper.Data;
using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste.Mod.XaphanHelper.UI_Elements
{
    [Tracked(true)]
    public class MapScreen : Entity
    {
        public class SelectMarkerPrompt : Entity
        {
            private class MarkerSelect : Entity
            {
                public int ID;

                public float width = 100f;

                public float height = 75f;

                private Sprite sprite;

                public bool Selected;

                private float selectedAlpha = 0;

                private int alphaStatus = 0;

                SelectMarkerPrompt prompt;

                public MarkerSelect(Vector2 position, int id, SelectMarkerPrompt prompt) : base(position - new Vector2(50f, 37.5f))
                {
                    Tag = Tags.HUD;
                    ID = id;
                    Add(sprite = new Sprite(GFX.Gui, "maps/keys/"));
                    sprite.Position = new Vector2(18f, 7.5f);
                    sprite.AddLoop("marker", "marker", 0.08f);
                    switch (ID)
                    {
                        case 0:
                            break;
                        case 1:
                            sprite.Color = Calc.HexToColor("4E8BFF");
                            break;
                        case 2:
                            sprite.Color = Calc.HexToColor("F80000");
                            break;
                        case 3:
                            sprite.Color = Calc.HexToColor("88F205");
                            break;
                        case 4:
                            sprite.Color = Calc.HexToColor("F8B000");
                            break;
                        case 5:
                            sprite.Color = Calc.HexToColor("7800F8");
                            break;
                    }
                    sprite.Play("marker");
                    this.prompt = prompt;
                    Depth = prompt.Depth;
                }

                public override void Update()
                {
                    base.Update();
                    if (prompt.Selection == ID)
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
                    if (prompt.drawContent)
                    {
                        if (Selected)
                        {
                            Draw.Rect(Position, width, height, Color.Yellow * selectedAlpha);
                        }
                        sprite.Render();
                    }
                }
            }

            private float height;

            private float width;

            public bool drawContent;

            public bool open;

            public int maxSelection;

            public int Selection = -1;

            public Vector2 PromptPos;

            public Coroutine OpenRoutine = new();

            private List<MarkerSelect> MarkerSelects = new();

            public SelectMarkerPrompt(Vector2 position, int selection) : base(position)
            {
                Tag = (Tags.HUD | Tags.Persistent);
                Depth = -10003;
                maxSelection = 5;
                width = (maxSelection + 1) * 100 + 50;
                Selection = selection;
            }

            public override void Added(Scene scene)
            {
                base.Added(scene);
                OpenPrompt();
            }

            public override void Removed(Scene scene)
            {
                base.Removed(scene);
                foreach (MarkerSelect markerSelect in MarkerSelects)
                {
                    markerSelect.RemoveSelf();
                }
            }

            public void OpenPrompt()
            {
                open = true;
                Add(new Coroutine(Open()));
            }

            public IEnumerator Open()
            {
                while (height < 200)
                {
                    height += Engine.DeltaTime * 1200;
                    yield return null;
                }
                height = 200;
                AddSelections();
                drawContent = true;
            }

            public void AddSelections()
            {
                for (int i = 0; i <= maxSelection; i++)
                {
                    MarkerSelect select = new(PromptPos + new Vector2(75f + 100f * i, 130f), i, this);
                    MarkerSelects.Add(select);
                    SceneAs<Level>().Add(select);
                }
            }

            public void ClosePrompt()
            {
                Add(new Coroutine(Close()));
            }

            public IEnumerator Close()
            {
                drawContent = false;
                while (height > 1)
                {
                    height -= Engine.DeltaTime * 1200;
                    yield return null;
                }
                open = false;
                RemoveSelf();
            }

            public override void Update()
            {
                base.Update();
                PromptPos = new Vector2(Engine.Width / 2 - width / 2, (Engine.Height / 2 - 39) + 200 / 2 - height / 2);
            }

            public override void Render()
            {
                Draw.Rect(PromptPos.X, PromptPos.Y, width, height, Color.Black);
                Draw.Rect(PromptPos.X - 5, PromptPos.Y - 5, width + 10, 10, Color.White);
                Draw.Rect(PromptPos.X - 5, PromptPos.Y - 5, 10, height + 10, Color.White);
                Draw.Rect(PromptPos.X - 5, PromptPos.Y - 5 + height, width + 10, 10, Color.White);
                Draw.Rect(PromptPos.X - 5 + width, PromptPos.Y - 5, 10, height + 10, Color.White);
                if (drawContent)
                {
                    ActiveFont.Draw(Dialog.Clean("XaphanHelper_UI_markers_select"), new Vector2(PromptPos.X + width / 2, PromptPos.Y + 50f), new Vector2(0.5f), Vector2.One, Color.White);
                }
            }
        }

        protected XaphanModuleSettings XaphanSettings => XaphanModule.ModSettings;

        private Level level;

        private MapDisplay mapDisplay;

        private List<MapDisplay> worldMapMapDisplays = new();

        private bool NoInput;

        public float LeftMoves;

        public float RightMoves;

        public float UpMoves;

        public float DownMoves;

        public string Title;

        public int SubAreaIndex;

        public string SubArea;

        public BigTitle BigTitle;

        public BigTitle SubAreaName;

        public SwitchUIPrompt prompt;

        public SelectMarkerPrompt markerPrompt;

        public TextMenu displayMenu;

        public bool promptChoice;

        private Wiggler statusWiggle;

        private Wiggler closeWiggle;

        private Wiggler displayWiggle;

        private Wiggler worldMapWiggle;

        private float statusWiggleDelay;

        private float closeWiggleDelay;

        private float displayWiggleDelay;

        private float worldMapWiggleDelay;

        private bool fromStatus;

        private float switchTimer;

        public string mode;

        private float worldMapWidth;

        private float worldMapHeight;

        public Vector2 MapOffset;

        public Vector2 WorldMapOffset;

        public bool hasInterlude;

        public int maxChapters;

        private Vector2 MapAutoMoves = new();

        public MapProgressDisplay MapProgressDisplay;

        public MapProgressDisplay WorldMapProgressDisplay;

        public bool HasWorldMap;

        public bool registerMarker;

        private bool CanSwitchToWorldMap;

        public MapScreen(Level level, bool fromStatus)
        {
            this.level = level;
            this.fromStatus = fromStatus;
            Tag = Tags.HUD;
            Add(statusWiggle = Wiggler.Create(0.4f, 4f));
            Add(closeWiggle = Wiggler.Create(0.4f, 4f));
            Add(displayWiggle = Wiggler.Create(0.4f, 4f));
            Add(worldMapWiggle = Wiggler.Create(0.4f, 4f));
            mode = "map";
            Depth = -10003;
        }

        public override void Added(Scene scene)
        {
            base.Added(scene);
            XaphanModule.UIOpened = true;
            Level level = Scene as Level;
            level.PauseLock = true;
            level.Session.SetFlag("Map_Opened", true);
            if (!fromStatus)
            {
                Audio.Play("event:/ui/game/pause");
            }
            Add(new Coroutine(TransitionToMap(level)));
        }

        public override void Update()
        {
            if (XaphanModule.ShowUI)
            {
                foreach (Player player in SceneAs<Level>().Tracker.GetEntities<Player>())
                {
                    player.StateMachine.State = Player.StDummy;
                    player.DummyAutoAnimate = false;
                }
            }
            if (prompt == null)
            {
                if (XaphanSettings.MapScreenShowMapOrWorldMap.Pressed && mapDisplay != null && CanSwitchToWorldMap && HasWorldMap && !mapDisplay.MarkerSelectionMode)
                {
                    if (mode == "map")
                    {
                        Audio.Play("event:/ui/main/message_confirm");
                        mapDisplay.Display = false;
                        MapProgressDisplay.Visible = false;
                        mode = "worldmap";
                        foreach (MapDisplay display in worldMapMapDisplays)
                        {
                            display.Visible = true;
                        }
                        if (WorldMapProgressDisplay != null)
                        {
                            WorldMapProgressDisplay.Visible = true;
                        }
                        CalcMoves(WorldMapOffset.X > 0 ? (int)WorldMapOffset.X / 40 : 0, WorldMapOffset.X < 0 ? Math.Abs((int)WorldMapOffset.X / 40) : 0, WorldMapOffset.Y > 0 ? (int)WorldMapOffset.Y / 40 : 0, WorldMapOffset.Y < 0 ? Math.Abs((int)WorldMapOffset.Y / 40) : 0);
                    }
                    else
                    {
                        Audio.Play("event:/ui/main/button_back");
                        mapDisplay.Display = true;
                        MapProgressDisplay.Visible = true;
                        mode = "map";
                        foreach (MapDisplay display in worldMapMapDisplays)
                        {
                            display.Visible = false;
                        }
                        if (WorldMapProgressDisplay != null)
                        {
                            WorldMapProgressDisplay.Visible = false;
                        }
                        CalcMoves(MapOffset.X > 0 ? (int)MapOffset.X / 40 : 0, MapOffset.X < 0 ? Math.Abs((int)MapOffset.X / 40) : 0, MapOffset.Y > 0 ? (int)MapOffset.Y / 40 : 0, MapOffset.Y < 0 ? Math.Abs((int)MapOffset.Y / 40) : 0);
                    }
                }
                if (mapDisplay != null)
                {
                    if (mapDisplay.useHints)
                    {
                        if (XaphanModule.ModSaveData.ShowHints[mapDisplay.Prefix] && !mapDisplay.ShowHints)
                        {
                            mapDisplay.ShowHints = true;
                            mapDisplay.MapWidth = mapDisplay.BeforeHintsMapWidth;
                            mapDisplay.MapHeight = mapDisplay.BeforeHintsMapHeight;
                            mapDisplay.MostLeftRoomX = mapDisplay.BeforeHintsMostLeftRoomX;
                            mapDisplay.MostTopRoomY = mapDisplay.BeforeHintsMostTopRoomY;
                            mapDisplay.MostRightRoomX = mapDisplay.BeforeHintsMostRightRoomX;
                            mapDisplay.MostBottomRoomY = mapDisplay.BeforeHintsMostBottomRoomY;
                            mapDisplay.SetCurrentMapCoordinates();
                            MoveMapX((int)-MapOffset.X, silence: true);
                            MoveMapY((int)-MapOffset.Y, silence: true);
                            if (mode == "map")
                            {
                                CalcMoves();
                            }
                            foreach (MapDisplay display in worldMapMapDisplays)
                            {
                                if (display.useHints && !display.ConnectionTilesOnly)
                                {
                                    display.ShowHints = true;
                                }
                            }
                        }
                        else if (!XaphanModule.ModSaveData.ShowHints[mapDisplay.Prefix] && mapDisplay.ShowHints)
                        {
                            mapDisplay.ShowHints = false;
                            mapDisplay.MapWidth = mapDisplay.AfterHintsMapWidth;
                            mapDisplay.MapHeight = mapDisplay.AfterHintsMapHeight;
                            mapDisplay.MostLeftRoomX = mapDisplay.AfterHintsMostLeftRoomX;
                            mapDisplay.MostTopRoomY = mapDisplay.AfterHintsMostTopRoomY;
                            mapDisplay.MostRightRoomX = mapDisplay.AfterHintsMostRightRoomX;
                            mapDisplay.MostBottomRoomY = mapDisplay.AfterHintsMostBottomRoomY;
                            mapDisplay.GetMapSize();
                            mapDisplay.SetCurrentMapCoordinates();
                            MoveMapX((int)-MapOffset.X, silence: true);
                            MoveMapY((int)-MapOffset.Y, silence: true);
                            if (mode == "map")
                            {
                                CalcMoves();
                            }
                            foreach (MapDisplay display in worldMapMapDisplays)
                            {
                                if (display.useHints && !display.ConnectionTilesOnly)
                                {
                                    display.ShowHints = false;
                                }
                            }
                        }
                    }
                }
                if (XaphanModule.useUpgrades && !XaphanModule.PlayerIsControllingRemoteDrone() && displayMenu == null)
                {
                    if (Input.Pause.Pressed && statusWiggleDelay <= 0f && switchTimer <= 0 && (mapDisplay != null ? !mapDisplay.MarkerSelectionMode : true))
                    {
                        statusWiggle.Start();
                        statusWiggleDelay = 0.5f;
                    }
                }
                if (mapDisplay != null && mapDisplay.useHints && displayMenu == null)
                {
                    if (XaphanSettings.MapScreenDisplayOptions.Pressed && displayWiggleDelay <= 0f && (mapDisplay != null ? !mapDisplay.MarkerSelectionMode : true))
                    {
                        displayWiggle.Start();
                        displayWiggleDelay = 0.5f;
                    }
                }
                if (XaphanSettings.MapScreenShowMapOrWorldMap.Pressed && CanSwitchToWorldMap && worldMapWiggleDelay <= 0f && (mapDisplay != null ? !mapDisplay.MarkerSelectionMode : true))
                {
                    worldMapWiggle.Start();
                    worldMapWiggleDelay = 0.5f;
                }
                statusWiggleDelay -= Engine.DeltaTime;
                displayWiggleDelay -= Engine.DeltaTime;
                worldMapWiggleDelay -= Engine.DeltaTime;
            }
            if (Input.MenuCancel.Pressed && closeWiggleDelay <= 0f && displayMenu == null && (mapDisplay != null ? !mapDisplay.MarkerSelectionMode : true))
            {
                closeWiggle.Start();
                closeWiggleDelay = 0.5f;
            }
            closeWiggleDelay -= Engine.DeltaTime;
            base.Update();
        }

        private IEnumerator TransitionToMap(Level level)
        {
            float duration = 0.5f;
            Add(new Coroutine(WorldMapRoutine(level)));
            if (!fromStatus)
            {
                FadeWipe Wipe = new(SceneAs<Level>(), false)
                {
                    Duration = duration
                };
                SceneAs<Level>().Add(Wipe);
                duration = duration - 0.25f;
                while (duration > 0f)
                {
                    yield return null;
                    duration -= Engine.DeltaTime;
                }
            }
            CountdownDisplay timerDisplay = SceneAs<Level>().Tracker.GetEntity<CountdownDisplay>();
            if (timerDisplay != null)
            {
                timerDisplay.StopTimer(true, false);
            }
            XaphanModule.ShowUI = true;
            duration = 0.25f;
            FadeWipe Wipe2 = new(SceneAs<Level>(), true)
            {
                Duration = duration
            };
            Add(new Coroutine(MapRoutine(level)));
            switchTimer = 0.35f;
            while (switchTimer > 0f)
            {
                yield return null;
                switchTimer -= Engine.DeltaTime;
            }
        }

        private IEnumerator TransitionToGame()
        {
            float duration = 0.5f;
            FadeWipe Wipe = new(SceneAs<Level>(), false)
            {
                Duration = duration
            };
            SceneAs<Level>().Add(Wipe);
            duration = duration - 0.25f;
            while (duration > 0f)
            {
                yield return null;
                duration -= Engine.DeltaTime;
            }
            CountdownDisplay timerDisplay = SceneAs<Level>().Tracker.GetEntity<CountdownDisplay>();
            if (timerDisplay != null)
            {
                timerDisplay.StopTimer(false, true);
            }
            BagDisplay bagDisplay = SceneAs<Level>().Tracker.GetEntity<BagDisplay>();
            if (bagDisplay != null && !XaphanModule.PlayerIsControllingRemoteDrone())
            {
                if (!bagDisplay.CheckIfUpgradeIsActive(bagDisplay.currentSelection))
                {
                    bagDisplay.SetToFirstActiveUpgrade();
                }
            }
            XaphanModule.ShowUI = false;
            duration = 0.25f;
            Wipe = new FadeWipe(SceneAs<Level>(), true)
            {
                Duration = duration
            };
            Add(new Coroutine(CloseMap(false)));
        }

        private IEnumerator TransitionToStatusScreen()
        {
            if (!NoInput)
            {
                NoInput = true;
                Player player = Scene.Tracker.GetEntity<Player>();
                float duration = 0.5f;
                FadeWipe Wipe = new(SceneAs<Level>(), false)
                {
                    Duration = duration
                };
                SceneAs<Level>().Add(Wipe);
                duration = duration - 0.25f;
                while (duration > 0f)
                {
                    yield return null;
                    duration -= Engine.DeltaTime;
                }
                Add(new Coroutine(CloseMap(true)));
                level.Add(new StatusScreen(level, true));
            }
        }

        private IEnumerator TransitionToAchievementsScreen()
        {
            if (!NoInput)
            {
                NoInput = true;
                Player player = Scene.Tracker.GetEntity<Player>();
                float duration = 0.5f;
                FadeWipe Wipe = new(SceneAs<Level>(), false)
                {
                    Duration = duration
                };
                SceneAs<Level>().Add(Wipe);
                duration = duration - 0.25f;
                while (duration > 0f)
                {
                    yield return null;
                    duration -= Engine.DeltaTime;
                }
                Add(new Coroutine(CloseMap(true)));
                level.Add(new AchievementsScreen(level));
            }
        }

        private IEnumerator TransitionToLorebookScreen()
        {
            if (!NoInput)
            {
                NoInput = true;
                Player player = Scene.Tracker.GetEntity<Player>();
                float duration = 0.5f;
                FadeWipe Wipe = new(SceneAs<Level>(), false)
                {
                    Duration = duration
                };
                SceneAs<Level>().Add(Wipe);
                duration = duration - 0.25f;
                while (duration > 0f)
                {
                    yield return null;
                    duration -= Engine.DeltaTime;
                }
                Add(new Coroutine(CloseMap(true)));
                level.Add(new LorebookScreen(level));
            }
        }

        public void CalcMoves(int previousLeftMoves = 0, int previousRightMoves = 0, int previousUpMoves = 0, int previousDownMoves = 0)
        {
            AreaKey area = level.Session.Area;
            int chapterIndex = area.ChapterIndex == -1 ? 0 : area.ChapterIndex;
            if (mode == "map")
            {
                float MaxXMove = (float)Math.Floor((mapDisplay.MapWidth / 8 - 1640) / 40);
                if (MaxXMove % 2 != 0)
                {
                    LeftMoves = (float)Math.Ceiling(MaxXMove / 2);
                    RightMoves = (float)Math.Floor(MaxXMove / 2);
                }
                else
                {
                    LeftMoves = MaxXMove / 2;
                    RightMoves = MaxXMove / 2;
                }

                float MaxYMove = (float)Math.Floor((mapDisplay.MapHeight / 8 - 437) / 23);
                if (MaxYMove % 2 != 0)
                {
                    UpMoves = (float)Math.Floor(MaxYMove / 2);
                    DownMoves = (float)Math.Ceiling(MaxYMove / 2);
                }
                else
                {
                    UpMoves = MaxYMove / 2;
                    DownMoves = MaxYMove / 2;
                }
                Vector2 CurrentRoomPosition = mapDisplay.CalcRoomPosition(mapDisplay.RoomData[mapDisplay.currentRoom].Position + (mapDisplay.roomIsAdjusted(mapDisplay.currentRoom) ? mapDisplay.GetAdjustedPosition(mapDisplay.currentRoom) : Vector2.Zero), mapDisplay.currentRoomPosition, mapDisplay.currentRoomJustify, mapDisplay.worldmapPosition);
                Vector2 PlayerIconPosition = Vector2.One + mapDisplay.playerPosition * 40;

                if (previousLeftMoves != 0 || previousRightMoves != 0 || previousUpMoves != 0 || previousDownMoves != 0)
                {
                    if (previousLeftMoves != 0)
                    {
                        LeftMoves -= previousLeftMoves;
                        RightMoves += previousLeftMoves;
                    }
                    if (previousRightMoves != 0)
                    {
                        LeftMoves += previousRightMoves;
                        RightMoves -= previousRightMoves;
                    }
                    if (previousUpMoves != 0)
                    {
                        UpMoves -= previousUpMoves;
                        DownMoves += previousUpMoves;
                    }
                    if (previousDownMoves != 0)
                    {
                        UpMoves += previousDownMoves;
                        DownMoves -= previousDownMoves;
                    }
                }
                else if (mapDisplay.isNotVisibleOnScreen(CurrentRoomPosition, PlayerIconPosition))
                {
                    if ((CurrentRoomPosition.Y + PlayerIconPosition.Y) < mapDisplay.Grid.Top)
                    {
                        int YAutoMoves = Math.Abs(((int)(CurrentRoomPosition.Y + PlayerIconPosition.Y) - mapDisplay.Grid.Top - 1) / 40);
                        UpMoves -= YAutoMoves;
                        DownMoves += YAutoMoves;
                        MoveMapY(YAutoMoves * 40, silence: true);
                    }
                    if ((CurrentRoomPosition.Y + PlayerIconPosition.Y) > mapDisplay.Grid.Bottom)
                    {
                        int YAutoMoves = ((int)(CurrentRoomPosition.Y + PlayerIconPosition.Y) - mapDisplay.Grid.Bottom - 1) / 40 + 2;
                        UpMoves += YAutoMoves;
                        DownMoves -= YAutoMoves;
                        MoveMapY(YAutoMoves * -40, silence: true);
                    }
                    if ((CurrentRoomPosition.X + PlayerIconPosition.X) < mapDisplay.Grid.Left)
                    {
                        int XAutoMoves = Math.Abs(((int)(CurrentRoomPosition.X + PlayerIconPosition.X) - mapDisplay.Grid.Left - 1) / 40) + 1;
                        LeftMoves -= XAutoMoves;
                        RightMoves += XAutoMoves;
                        MoveMapX(XAutoMoves * 40, silence: true);
                    }
                    if ((CurrentRoomPosition.X + PlayerIconPosition.X) > mapDisplay.Grid.Right)
                    {
                        int XAutoMoves = ((int)(CurrentRoomPosition.X + PlayerIconPosition.X) - mapDisplay.Grid.Right - 1) / 40 + 2;
                        LeftMoves += XAutoMoves;
                        RightMoves -= XAutoMoves;
                        MoveMapX(XAutoMoves * -40, silence: true);
                    }
                }
            }
            else
            {
                float MaxXMove = (float)Math.Floor((worldMapWidth - 1640) / 40);
                if (MaxXMove % 2 != 0)
                {
                    LeftMoves = (float)Math.Ceiling(MaxXMove / 2);
                    RightMoves = (float)Math.Floor(MaxXMove / 2);
                }
                else
                {
                    LeftMoves = MaxXMove / 2;
                    RightMoves = MaxXMove / 2;
                }

                float MaxYMove = (float)Math.Floor((worldMapHeight - 760) / 40);
                if (MaxYMove % 2 != 0)
                {
                    UpMoves = (float)Math.Floor(MaxYMove / 2);
                    DownMoves = (float)Math.Ceiling(MaxYMove / 2);
                }
                else
                {
                    UpMoves = MaxYMove / 2;
                    DownMoves = MaxYMove / 2;
                }
                Vector2 CurrentRoomPosition = Vector2.Zero;
                Vector2 PlayerIconPosition = Vector2.Zero;
                MapDisplay displayWithIndicator = new(level, "empty");
                foreach (MapDisplay display in level.Tracker.GetEntities<MapDisplay>())
                {
                    if (!display.HideIndicator && display.NoGrid && display.Visible)
                    {
                        displayWithIndicator = display;
                        break;
                    }
                }
                if (displayWithIndicator.mode != "empty")
                {
                    CurrentRoomPosition = displayWithIndicator.CalcRoomPosition(displayWithIndicator.RoomData[displayWithIndicator.currentRoom].Position + (displayWithIndicator.roomIsAdjusted(displayWithIndicator.currentRoom) ? displayWithIndicator.GetAdjustedPosition(displayWithIndicator.currentRoom) : Vector2.Zero), displayWithIndicator.currentRoomPosition, displayWithIndicator.currentRoomJustify, displayWithIndicator.worldmapPosition);
                    PlayerIconPosition = Vector2.One + displayWithIndicator.playerPosition * 40;
                }
                else
                {
                    displayWithIndicator = null;
                }
                if (previousLeftMoves != 0 || previousRightMoves != 0 || previousUpMoves != 0 || previousDownMoves != 0)
                {
                    if (previousLeftMoves != 0)
                    {
                        LeftMoves -= previousLeftMoves;
                        RightMoves += previousLeftMoves;
                    }
                    if (previousRightMoves != 0)
                    {
                        LeftMoves += previousRightMoves;
                        RightMoves -= previousRightMoves;
                    }
                    if (previousUpMoves != 0)
                    {
                        UpMoves -= previousUpMoves;
                        DownMoves += previousUpMoves;
                    }
                    if (previousDownMoves != 0)
                    {
                        UpMoves += previousDownMoves;
                        DownMoves -= previousDownMoves;
                    }
                }
                else
                {
                    Vector2 Movement = new(941 - (CurrentRoomPosition.X + PlayerIconPosition.X), 581 - (CurrentRoomPosition.Y + PlayerIconPosition.Y));
                    int XAutoMoves = (int)Math.Abs(Movement.X) / 40;
                    int YAutoMoves = (int)Math.Abs(Movement.Y) / 40;
                    if (LeftMoves > 0 || RightMoves > 0)
                    {
                        if (Movement.X < 0)
                        {
                            if (RightMoves - XAutoMoves < 0)
                            {
                                int extraMoves = (int)Math.Abs(RightMoves - XAutoMoves);
                                XAutoMoves -= extraMoves;
                            }
                            LeftMoves += XAutoMoves;
                            RightMoves -= XAutoMoves;
                            MoveMapX(XAutoMoves * -40, true, true);
                        }
                        else if (Movement.X > 0)
                        {
                            if (LeftMoves - XAutoMoves < 0)
                            {
                                int extraMoves = (int)Math.Abs(LeftMoves - XAutoMoves);
                                XAutoMoves -= extraMoves;
                            }
                            LeftMoves -= XAutoMoves;
                            RightMoves += XAutoMoves;
                            MoveMapX(XAutoMoves * 40, true, true);
                        }
                    }
                    if (UpMoves > 0 || DownMoves > 0)
                    {
                        if (Movement.Y < 0)
                        {
                            if (DownMoves - YAutoMoves < 0)
                            {
                                int extraMoves = (int)Math.Abs(DownMoves - YAutoMoves);
                                YAutoMoves -= extraMoves;
                            }
                            UpMoves += YAutoMoves;
                            DownMoves -= YAutoMoves;
                            MoveMapY(YAutoMoves * -40, true, true);
                        }
                        else if (Movement.Y > 0)
                        {
                            if (UpMoves - YAutoMoves < 0)
                            {
                                int extraMoves = (int)Math.Abs(UpMoves - YAutoMoves);
                                YAutoMoves -= extraMoves;
                            }
                            UpMoves -= YAutoMoves;
                            DownMoves += YAutoMoves;
                            MoveMapY(YAutoMoves * 40, true, true);
                        }
                    }
                }
            }
        }

        private IEnumerator WorldMapRoutine(Level level)
        {
            worldMapMapDisplays.Clear();
            hasInterlude = false;
            maxChapters = SaveData.Instance.LevelSetStats.Areas.Count;
            for (int i = 0; i < maxChapters; i++)
            {
                if (AreaData.Areas[(SaveData.Instance.LevelSetStats.AreaOffset + i)].Interlude)
                {
                    hasInterlude = true;
                    break;
                }
            }
            if (!hasInterlude)
            {
                maxChapters++;
            }
            int currentChapter = level.Session.Area.ChapterIndex == -1 ? 0 : level.Session.Area.ChapterIndex;
            AreaKey area = level.Session.Area;
            List<MapDisplay> ConnectionTilesDisplays = new();
            List<MapDisplay> MainTilesDisplays = new();
            for (int chapter = !hasInterlude ? 1 : 0; chapter < maxChapters; chapter++)
            {
                bool hasController = false;
                MapData MapData = AreaData.Areas[area.ID - (area.ChapterIndex == -1 ? 0 : area.ChapterIndex) + chapter].Mode[(int)area.Mode].MapData;
                foreach (LevelData levelData in MapData.Levels)
                {
                    foreach (EntityData entity in levelData.Entities)
                    {
                        if (entity.Name == "XaphanHelper/InGameMapController")
                        {
                            hasController = true;
                            break;
                        }
                    }
                }
                if (hasController)
                {
                    ConnectionTilesDisplays.Add(new MapDisplay(level, "worldmap", chapter, true, chapter == currentChapter ? false : true) { Visible = false, ConnectionTilesOnly = true });
                    MainTilesDisplays.Add(new MapDisplay(level, "worldmap", chapter, true, chapter == currentChapter ? false : true) { Visible = false });
                }
            }
            foreach (MapDisplay mapDisplay in ConnectionTilesDisplays)
            {
                worldMapMapDisplays.Add(mapDisplay);
            }
            foreach (MapDisplay mapDisplay in MainTilesDisplays)
            {
                worldMapMapDisplays.Add(mapDisplay);
            }
            if (worldMapMapDisplays.Count > 2)
            {
                HasWorldMap = true;
                foreach (MapDisplay display in worldMapMapDisplays)
                {
                    Scene.Add(display);
                    yield return display.GenerateMap(display.ConnectionTilesOnly);
                }
                Vector2 ProgressPosition = new(worldMapMapDisplays[0].Grid.X + 18f, worldMapMapDisplays[0].Grid.Y);
                List<InGameMapControllerData> InGameMapControllerDatas = new();
                foreach (MapDisplay display in worldMapMapDisplays)
                {
                    if (!display.ConnectionTilesOnly)
                    {
                        InGameMapControllerDatas.Add(display.InGameMapControllerData);
                    }
                }
                string WorldMapShowProgress = "Always";
                bool WorldMapHideMapProgress = false;
                bool WorldMapHideStrawberryProgress = false;
                bool WorldMapHideMoonberryProgress = false;
                bool WorldMapHideUpgradeProgress = false;
                bool WorldMapHideHeartProgress = false;
                bool WorldMapHideCassetteProgress = false;
                string WorldMapCustomCollectablesProgress = "";
                string WorldMapSecretsCustomCollectablesProgress = "";
                foreach (InGameMapControllerData data in InGameMapControllerDatas)
                {
                    if (data.ShowProgress != "Always")
                    {
                        if (data.ShowProgress == "AfterChapterComplete" || data.ShowProgress == "AfterCampaignComplete")
                        {
                            WorldMapShowProgress = "AfterCampaignComplete";
                        }
                        else
                        {
                            WorldMapShowProgress = "Never";
                        }
                    }
                    if (data.HideMapProgress == true)
                    {
                        WorldMapHideMapProgress = true;
                    }
                    if (data.HideStrawberryProgress == true)
                    {
                        WorldMapHideStrawberryProgress = true;
                    }
                    if (data.HideMoonberryProgress == true)
                    {
                        WorldMapHideMoonberryProgress = true;
                    }
                    if (data.HideUpgradeProgress == true)
                    {
                        WorldMapHideUpgradeProgress = true;
                    }
                    if (data.HideHeartProgress == true)
                    {
                        WorldMapHideHeartProgress = true;
                    }
                    if (data.HideCassetteProgress == true)
                    {
                        WorldMapHideCassetteProgress = true;
                    }
                    if (!string.IsNullOrEmpty(data.CustomCollectablesProgress))
                    {
                        WorldMapCustomCollectablesProgress += "," + data.CustomCollectablesProgress;
                    }
                    if (!string.IsNullOrEmpty(data.SecretsCustomCollectablesProgress))
                    {
                        WorldMapSecretsCustomCollectablesProgress += "," + data.SecretsCustomCollectablesProgress;
                    }
                }
                InGameMapControllerData WorldMapInGameMapControllerData = new("FFFFFF", "FFFFFF", "FFFFFF", "FFFFFF", "FFFFFF", "FFFFFF", "FFFFFF", false, false,
                    WorldMapShowProgress, WorldMapHideMapProgress, WorldMapHideStrawberryProgress, WorldMapHideMoonberryProgress, WorldMapHideUpgradeProgress, WorldMapHideHeartProgress, WorldMapHideCassetteProgress,
                    WorldMapCustomCollectablesProgress, WorldMapSecretsCustomCollectablesProgress, 0, 0);
                List<InGameMapSubAreaControllerData> WorldMapInGameMapSubAreaControllerData = new();
                List<InGameMapRoomControllerData> WorldMapInGameMapRoomControllerData = new();
                List<InGameMapTilesControllerData> WorldMapInGameMapTilesControllerData = new();
                List<InGameMapEntitiesData> WorldMapInGameMapEntitiesData = new();
                foreach (MapDisplay display in worldMapMapDisplays)
                {
                    if (!display.ConnectionTilesOnly)
                    {
                        WorldMapInGameMapSubAreaControllerData.AddRange(display.SubAreaControllerData);
                        WorldMapInGameMapRoomControllerData.AddRange(display.RoomControllerData);
                        WorldMapInGameMapTilesControllerData.AddRange(display.TilesControllerData);
                        WorldMapInGameMapEntitiesData.AddRange(display.EntitiesData);
                    }
                }
                WorldMapInGameMapSubAreaControllerData.Distinct().ToList();
                WorldMapInGameMapRoomControllerData.Distinct().ToList();
                WorldMapInGameMapTilesControllerData.Distinct().ToList();
                WorldMapInGameMapEntitiesData.Distinct().ToList();
                if (WorldMapInGameMapControllerData.ShowProgress != "Never")
                {
                    if (WorldMapProgressDisplay != null)
                    {
                        WorldMapProgressDisplay.RemoveSelf();
                    }
                    level.Add(WorldMapProgressDisplay = new MapProgressDisplay(ProgressPosition, level, WorldMapInGameMapControllerData, WorldMapInGameMapSubAreaControllerData, WorldMapInGameMapRoomControllerData, WorldMapInGameMapTilesControllerData, WorldMapInGameMapEntitiesData, -1, worldMapMapDisplays[0].currentRoom) { Visible = false });
                }
                float MostLeftDisplay = 1000000f;
                float MostTopDisplay = 1000000f;
                float MostRightDisplay = -1000000f;
                float MostBottomDisplay = -1000000f;
                foreach (MapDisplay display in worldMapMapDisplays)
                {
                    if (display.MapLeft < MostLeftDisplay)
                    {
                        MostLeftDisplay = display.MapLeft;
                    }
                    if (display.MapTop < MostTopDisplay)
                    {
                        MostTopDisplay = display.MapTop;
                    }
                    if (display.MapLeft + display.MapWidth / 320 * 40 > MostRightDisplay)
                    {
                        MostRightDisplay = display.MapLeft + display.MapWidth / 320 * 40;
                    }
                    if (display.MapTop + display.MapHeight / 184 * 40 > MostBottomDisplay)
                    {
                        MostBottomDisplay = display.MapTop + display.MapHeight / 184 * 40;
                    }
                }
                worldMapWidth = MostRightDisplay - MostLeftDisplay;
                worldMapHeight = MostBottomDisplay - MostTopDisplay;
                Vector2 displaysCenter = new(worldMapWidth / 2, worldMapHeight / 2);
                displaysCenter += new Vector2(MostLeftDisplay, MostTopDisplay);
                while (mapDisplay == null)
                {
                    yield return null;
                }
                Vector2 displayJustify = new(mapDisplay.MapPosition.X - displaysCenter.X, mapDisplay.MapPosition.Y - displaysCenter.Y);
                foreach (MapDisplay display in level.Tracker.GetEntities<MapDisplay>())
                {
                    if (display.NoGrid)
                    {
                        display.MapPosition += displayJustify;
                        Vector2 displayAdjust = Vector2.Zero;
                        display.MapPosition.X = (float)Math.Ceiling(display.MapPosition.X / 40) * 40;
                        if ((display.MapPosition.X - 100) / 40 % 2 != 0)
                        {
                            displayAdjust.X = -20f;
                        }
                        display.MapPosition.Y = (float)Math.Floor(display.MapPosition.Y / 40) * 40;
                        if ((display.MapPosition.Y - 180) / 40 % 2 != 0)
                        {
                            displayAdjust.Y = 20f;
                        }
                        display.MapPosition += displayAdjust;
                    }
                }
            }
            CanSwitchToWorldMap = true;
        }

        private IEnumerator MapRoutine(Level level)
        {
            Player player = Scene.Tracker.GetEntity<Player>();
            mapDisplay = new MapDisplay(level, "map", mapScreen: this);
            Scene.Add(mapDisplay);
            yield return mapDisplay.GenerateMap();
            if (mapDisplay.InGameMapControllerData.ShowProgress != "Never")
            {
                if (MapProgressDisplay != null)
                {
                    MapProgressDisplay.RemoveSelf();
                }
                level.Add(MapProgressDisplay = new MapProgressDisplay(new Vector2(mapDisplay.Grid.X + 18f, mapDisplay.Grid.Y), level, mapDisplay.InGameMapControllerData, mapDisplay.SubAreaControllerData, mapDisplay.RoomControllerData, mapDisplay.TilesControllerData, mapDisplay.EntitiesData, mapDisplay.chapterIndex, mapDisplay.currentRoom));
            }
            Title = GetSpecifiedMapName();
            SubAreaIndex = GetSubAreaIndex();
            SubArea = GetSubAreaName();
            Scene.Add(BigTitle = new BigTitle(Title, new Vector2(960, string.IsNullOrEmpty(SubArea) ? 80 : 60)));
            if (!string.IsNullOrEmpty(SubArea))
            {
                Scene.Add(SubAreaName = new BigTitle(SubArea, new Vector2(960, 135), false, 0.8f, "XaphanHelper_UI_Currently_In"));
            }
            CalcMoves();
            while (switchTimer > 0)
            {
                yield return null;
            }
            while (!Input.ESC.Pressed && (!Input.MenuCancel.Pressed || (Input.MenuCancel.Pressed && (displayMenu != null || mapDisplay.MarkerSelectionMode))) && !XaphanSettings.OpenMap.Pressed && player != null)
            {
                if (prompt != null)
                {
                    if (!prompt.open)
                    {
                        prompt = null;
                        promptChoice = false;
                    }
                    else
                    {
                        if (Input.MenuLeft.Pressed && prompt.Selection > 0)
                        {
                            prompt.Selection--;
                        }
                        if (Input.MenuRight.Pressed && prompt.Selection < prompt.maxSelection)
                        {
                            prompt.Selection++;
                        }
                        if ((Input.MenuConfirm.Pressed || Input.Pause.Pressed) && prompt.drawContent && !promptChoice)
                        {
                            promptChoice = true;
                            if (prompt.Selection == 0)
                            {
                                Add(new Coroutine(TransitionToStatusScreen()));
                            }
                            else if (prompt.Selection == 2)
                            {
                                Add(new Coroutine(TransitionToAchievementsScreen()));
                            }
                            else if (prompt.Selection == 3)
                            {
                                Add(new Coroutine(TransitionToLorebookScreen()));
                            }
                            prompt.ClosePrompt();
                        }
                    }
                }
                else if (displayMenu == null && (mapDisplay != null ? !mapDisplay.MarkerSelectionMode : true))
                {
                    if (mode == "map")
                    {
                        if (mapDisplay.MapWidth / 8 > 1640)
                        {
                            if (Input.MenuLeft.Pressed && LeftMoves > 0)
                            {
                                int direction = 40;
                                MoveMapX(direction);
                                LeftMoves -= 1;
                                RightMoves += 1;
                            }
                            if (Input.MenuRight.Pressed && RightMoves > 0)
                            {
                                int direction = -40;
                                MoveMapX(direction);
                                LeftMoves += 1;
                                RightMoves -= 1;
                            }
                        }
                        if (mapDisplay.MapHeight / 8 > 437)
                        {
                            if (Input.MenuUp.Pressed && UpMoves > 0)
                            {
                                int direction = 40;
                                MoveMapY(direction);
                                UpMoves -= 1;
                                DownMoves += 1;
                            }
                            if (Input.MenuDown.Pressed && DownMoves > 0)
                            {
                                int direction = -40;
                                MoveMapY(direction);
                                UpMoves += 1;
                                DownMoves -= 1;
                            }
                        }
                    }
                    else
                    {
                        if (worldMapWidth > 1640)
                        {
                            if (Input.MenuLeft.Pressed && LeftMoves > 0)
                            {
                                int direction = 40;
                                MoveMapX(direction, true);
                                LeftMoves -= 1;
                                RightMoves += 1;
                            }
                            if (Input.MenuRight.Pressed && RightMoves > 0)
                            {
                                int direction = -40;
                                MoveMapX(direction, true);
                                LeftMoves += 1;
                                RightMoves -= 1;
                            }
                        }
                        if (worldMapHeight > 760)
                        {
                            if (Input.MenuUp.Pressed && UpMoves > 0)
                            {
                                int direction = 40;
                                MoveMapY(direction, true);
                                UpMoves -= 1;
                                DownMoves += 1;
                            }
                            if (Input.MenuDown.Pressed && DownMoves > 0)
                            {
                                int direction = -40;
                                MoveMapY(direction, true);
                                UpMoves += 1;
                                DownMoves -= 1;
                            }
                        }
                    }
                }
                if (Input.Pause.Pressed && XaphanModule.useUpgrades && switchTimer <= 0 && !XaphanModule.PlayerIsControllingRemoteDrone() && !XaphanModule.DisableStatusScreen)
                {
                    if (SceneAs<Level>().Session.Area.LevelSet == "Xaphan/0")
                    {
                        if (prompt == null && displayMenu == null && (mapDisplay != null ? !mapDisplay.MarkerSelectionMode : true))
                        {
                            Scene.Add(prompt = new SwitchUIPrompt(Vector2.Zero, 1));
                        }
                    }
                    else
                    {
                        Add(new Coroutine(TransitionToStatusScreen()));
                    }
                }
                if (XaphanSettings.MapScreenDisplayOptions.Pressed)
                {
                    if (prompt == null && displayMenu == null && (mapDisplay != null ? !mapDisplay.MarkerSelectionMode : true))
                    {
                        Audio.Play("event:/ui/main/message_confirm");
                        Scene.Add(displayMenu = new TextMenu());
                        displayMenu.Depth = -10003;
                        displayMenu.AutoScroll = false;
                        displayMenu.Position = new Vector2(Engine.Width / 2f, Engine.Height / 2f + 60f);
                        displayMenu.Add(new TextMenu.Header(Dialog.Clean("XaphanHelper_UI_displayOptions").ToUpper()));
                        displayMenu.Add(new TextMenu.Slider(Dialog.Clean("XaphanHelper_UI_progress"), (int i) => i switch
                        {
                            0 => Dialog.Clean("OPTIONS_OFF"),
                            1 => Dialog.Clean("XaphanHelper_UI_progress_subarea"),
                            _ => Dialog.Clean("XaphanHelper_UI_progress_area"),
                        }, 0, 2, XaphanModule.ModSaveData.ProgressMode[mapDisplay.Prefix]).Change(delegate (int i)
                        {
                            XaphanModule.ModSaveData.ProgressMode[mapDisplay.Prefix] = i;
                        }));
                        displayMenu.Add(new TextMenu.OnOff(Dialog.Clean("XaphanHelper_UI_showmarkers"), XaphanModule.ModSaveData.ShowMarkers[mapDisplay.Prefix]).Change(delegate (bool b)
                        {
                            XaphanModule.ModSaveData.ShowMarkers[mapDisplay.Prefix] = b;
                        }));
                        if (mapDisplay.useHints)
                        {
                            displayMenu.Add(new TextMenu.OnOff(Dialog.Clean("XaphanHelper_UI_showhints"), XaphanModule.ModSaveData.ShowHints[mapDisplay.Prefix]).Change(delegate (bool b)
                            {
                                XaphanModule.ModSaveData.ShowHints[mapDisplay.Prefix] = b;
                            }));
                        }
                        displayMenu.OnCancel = displayMenu.OnClose = CloseDisplaymenu;
                        while (!displayMenu.Visible)
                        {
                            yield return null;
                        }
                    }
                }
                registerMarker = false;
                if (XaphanSettings.MapScreenPlaceMarker.Pressed && markerPrompt == null)
                {
                    if (!mapDisplay.MarkerSelectionMode)
                    {
                        if (prompt == null && displayMenu == null && mode == "map")
                        {
                            Audio.Play("event:/ui/main/message_confirm");
                            Vector2 CurrentRoomPosition = mapDisplay.CalcRoomPosition(mapDisplay.RoomData[mapDisplay.currentRoom].Position + (mapDisplay.roomIsAdjusted(mapDisplay.currentRoom) ? mapDisplay.GetAdjustedPosition(mapDisplay.currentRoom) : Vector2.Zero), mapDisplay.currentRoomPosition, mapDisplay.currentRoomJustify, mapDisplay.worldmapPosition);
                            Vector2 PlayerIconPosition = Vector2.One + mapDisplay.playerPosition * 40;
                            if (mapDisplay.isNotVisibleOnScreen(CurrentRoomPosition, PlayerIconPosition))
                            {
                                if ((CurrentRoomPosition.Y + PlayerIconPosition.Y) < mapDisplay.Grid.Top)
                                {
                                    int YAutoMoves = Math.Abs(((int)(CurrentRoomPosition.Y + PlayerIconPosition.Y) - mapDisplay.Grid.Top - 1) / 40);
                                    MapAutoMoves.Y = YAutoMoves;
                                    MoveMapY(YAutoMoves * 40, silence: true);
                                }
                                if ((CurrentRoomPosition.Y + PlayerIconPosition.Y) > mapDisplay.Grid.Bottom)
                                {
                                    int YAutoMoves = ((int)(CurrentRoomPosition.Y + PlayerIconPosition.Y) - mapDisplay.Grid.Bottom - 1) / 40 + 2;
                                    MapAutoMoves.Y = -YAutoMoves;
                                    MoveMapY(YAutoMoves * -40, silence: true);
                                }
                                if ((CurrentRoomPosition.X + PlayerIconPosition.X) < mapDisplay.Grid.Left)
                                {
                                    int XAutoMoves = Math.Abs(((int)(CurrentRoomPosition.X + PlayerIconPosition.X) - mapDisplay.Grid.Left - 1) / 40) + 1;
                                    MapAutoMoves.X = XAutoMoves;
                                    MoveMapX(XAutoMoves * 40, silence: true);
                                }
                                if ((CurrentRoomPosition.X + PlayerIconPosition.X) > mapDisplay.Grid.Right)
                                {
                                    int XAutoMoves = ((int)(CurrentRoomPosition.X + PlayerIconPosition.X) - mapDisplay.Grid.Right - 1) / 40 + 2;
                                    MapAutoMoves.X = -XAutoMoves;
                                    MoveMapX(XAutoMoves * -40, silence: true);
                                }
                            }
                            mapDisplay.EnterMarkerMode();
                        }
                    }
                    else
                    {
                        string markerFull = mapDisplay.chapterIndex + ":" + mapDisplay.currentRoom + ":" + mapDisplay.markerSelector.TilePosition.X + ":" + mapDisplay.markerSelector.TilePosition.Y; // Marker to register or remove
                        HashSet<string> markers = XaphanModule.ModSaveData.Markers.ContainsKey(mapDisplay.Prefix) ? XaphanModule.ModSaveData.Markers[mapDisplay.Prefix] : null;
                        if (markers != null)
                        {
                            if (mapDisplay.markerSelector.HoverExistingMarker) // Current marker is already registered. Remove it.
                            {
                                string markerInHashSet = null;
                                foreach (string marker in markers)
                                {
                                    if (marker.Contains(mapDisplay.markerSelector.HoverExistingMarkerRoom + ":" + mapDisplay.markerSelector.HoverExistingMarkerPosition.X + ":" + mapDisplay.markerSelector.HoverExistingMarkerPosition.Y))
                                    {
                                        markerInHashSet = marker;
                                        break;
                                    }
                                }
                                Audio.Play("event:/ui/main/button_select");
                                XaphanModule.ModSaveData.Markers[mapDisplay.Prefix].Remove(markerInHashSet);
                                registerMarker = true;
                            }
                            else // Add current marker
                            {
                                yield return AddMarker(markerFull);
                            }
                        }
                        else
                        {
                            XaphanModule.ModSaveData.Markers.Add(mapDisplay.Prefix, new HashSet<string>());
                            yield return AddMarker(markerFull);
                        }
                        mapDisplay.GenerateMarkers();
                        foreach (MapDisplay display in worldMapMapDisplays)
                        {
                            display.GenerateMarkers();
                        }
                    }
                }
                if ((Input.MenuCancel.Pressed && mapDisplay.MarkerSelectionMode && markerPrompt == null) || registerMarker)
                {
                    if (!registerMarker)
                    {
                        Audio.Play("event:/ui/main/button_back");
                    }
                    mapDisplay.EnterMarkerMode();
                    if (!mapDisplay.MarkerSelectionMode && MapAutoMoves != Vector2.Zero)
                    {
                        MoveMapY((int)MapAutoMoves.Y * -40, silence: true);
                        MoveMapX((int)MapAutoMoves.X * -40, silence: true);
                        MapAutoMoves = Vector2.Zero;
                    }
                }
                if (mapDisplay.markerSelector != null)
                {
                    mapDisplay.markerSelector.Focused = true;
                }
                yield return null;
            }
            Audio.Play("event:/ui/game/unpause");
            Add(new Coroutine(TransitionToGame()));
        }

        private IEnumerator AddMarker(string marker)
        {
            Audio.Play("event:/ui/main/button_toggle_on");
            mapDisplay.markerSelector.Focused = false;
            Scene.Add(markerPrompt = new SelectMarkerPrompt(Vector2.Zero, 0));
            int type = 0;
            bool canceled = false;
            while (!Input.MenuConfirm.Pressed && !canceled)
            {
                if (Input.MenuLeft.Pressed && markerPrompt.Selection > 0)
                {
                    markerPrompt.Selection--;
                }
                if (Input.MenuRight.Pressed && markerPrompt.Selection < markerPrompt.maxSelection)
                {
                    markerPrompt.Selection++;
                }
                if (Input.MenuCancel.Pressed)
                {
                    canceled = true;
                }
                yield return null;
            }
            markerPrompt.ClosePrompt();
            if (canceled)
            {
                Audio.Play("event:/ui/main/button_toggle_off");
                markerPrompt = null;
                yield break;
            }
            Audio.Play("event:/ui/main/button_select");
            type = markerPrompt.Selection;
            yield return 0.05f;
            markerPrompt = null;
            XaphanModule.ModSaveData.Markers[mapDisplay.Prefix].Add(marker + ":" + type);
            registerMarker = true;
        }

        private void CloseDisplaymenu()
        {
            if (displayMenu != null)
            {
                Audio.Play("event:/ui/main/button_back");
                displayMenu.Focused = displayMenu.Visible = false;
                displayMenu.RemoveSelf();
                displayMenu = null;
            }
        }

        public void MoveMapX(int direction, bool worldmap = false, bool silence = false)
        {
            if (!worldmap)
            {
                MapOffset.X += direction;
                mapDisplay.adjustMapPosition(new Vector2(direction, 0));
            }
            else
            {
                WorldMapOffset.X += direction;
                foreach (MapDisplay display in worldMapMapDisplays)
                {
                    display.adjustMapPosition(new Vector2(direction, 0));
                }
            }
            if (!silence)
            {
                Audio.Play("event:/ui/main/rollover_up");
            }
        }

        public void MoveMapY(int direction, bool worldmap = false, bool silence = false)
        {
            if (!worldmap)
            {
                MapOffset.Y += direction;
                mapDisplay.adjustMapPosition(new Vector2(0, direction));
            }
            else
            {
                WorldMapOffset.Y += direction;
                foreach (MapDisplay display in worldMapMapDisplays)
                {
                    display.adjustMapPosition(new Vector2(0, direction));
                }
            }
            if (!silence)
            {
                Audio.Play("event:/ui/main/rollover_up");
            }
        }

        private IEnumerator CloseMap(bool switchtoStatus)
        {
            Level level = Scene as Level;
            Player player = Scene.Tracker.GetEntity<Player>();
            level.Remove(BigTitle);
            level.Remove(prompt);
            level.Remove(displayMenu);
            level.Remove(mapDisplay.markerSelector);
            if (SubAreaName != null)
            {
                level.Remove(SubAreaName);
            }
            level.Remove(mapDisplay);
            foreach (MapDisplay display in worldMapMapDisplays)
            {
                level.Remove(display);
            }
            if (MapProgressDisplay != null)
            {
                level.Remove(MapProgressDisplay);
            }
            if (WorldMapProgressDisplay != null)
            {
                level.Remove(WorldMapProgressDisplay);
            }
            if (!switchtoStatus)
            {
                if (player != null)
                {
                    player.StateMachine.State = Player.StNormal;
                    player.DummyAutoAnimate = true;
                }
                level.PauseLock = false;
                yield return 0.1f;
                level.Session.SetFlag("Map_Opened", false);
            }
            XaphanModule.UIOpened = false;
            RemoveSelf();
        }

        public override void Render()
        {
            base.Render();
            if (XaphanModule.ShowUI)
            {
                Draw.Rect(new Vector2(-10, -10), 1940, 182, Color.Black);
                Draw.Rect(new Vector2(-10, 172), 100, 856, Color.Black);
                Draw.Rect(new Vector2(1830, 172), 100, 856, Color.Black);
                Draw.Rect(new Vector2(-10, 1028), 1940, 62, Color.Black);
                if (displayMenu != null)
                {
                    Draw.Rect(new Vector2(-10, -10), 1940, 1100, Color.Black * 0.75f);
                }
                Draw.Rect(new Vector2(90, 172), 1740, 8, Color.White);
                Draw.Rect(new Vector2(90, 180), 10, 840, Color.White);
                Draw.Rect(new Vector2(1820, 180), 10, 840, Color.White);
                Draw.Rect(new Vector2(90, 1020), 1740, 8, Color.White);
                MTexture mTexture = GFX.Gui["towerarrow"];
                if (displayMenu == null && (mapDisplay != null ? !mapDisplay.MarkerSelectionMode : true))
                {
                    if (UpMoves > 0)
                    {
                        mTexture.DrawCentered(new Vector2(960f, 175f), Color.White, 1f, (float)Math.PI / 2f);
                    }
                    if (DownMoves > 0)
                    {
                        mTexture.DrawCentered(new Vector2(960f, 1024f), Color.White, 1f, 4.712389f);
                    }
                    if (LeftMoves > 0)
                    {
                        mTexture.DrawCentered(new Vector2(94f, 600f), Color.White);
                    }
                    if (RightMoves > 0)
                    {
                        mTexture.DrawCentered(new Vector2(1824f, 600f), Color.White, 1f, (float)Math.PI);
                    }
                }
                float inputEase = 0f;
                inputEase = Calc.Approach(inputEase, 1, Engine.DeltaTime * 4f);
                if (inputEase > 0f)
                {
                    float scale = 0.5f;
                    if (mapDisplay != null)
                    {
                        if (!mapDisplay.MarkerSelectionMode)
                        {
                            string label = Dialog.Clean("XaphanHelper_UI_close");
                            string label2 = SceneAs<Level>().Session.Area.LevelSet != "Xaphan/0" ? Dialog.Clean("XaphanHelper_UI_abilities") : Dialog.Clean("XaphanHelper_UI_menu");
                            string label3 = Dialog.Clean("XaphanHelper_UI_displayOptions");
                            string label4 = Dialog.Clean("XaphanHelper_UI_progres_display");
                            string label5 = Dialog.Clean("XaphanHelper_UI_progress_area");
                            string label6 = Dialog.Clean("XaphanHelper_UI_progress_subarea");
                            string label7 = Dialog.Clean("XaphanHelper_UI_progress_allareas");
                            string label8 = Dialog.Clean("XaphanHelper_UI_showWorldMap");
                            string label9 = Dialog.Clean("XaphanHelper_UI_showAreaMap");
                            string label10 = Dialog.Clean("XaphanHelper_UI_markers");
                            float num = ButtonUI.Width(label, Input.MenuCancel);
                            float num2 = ButtonUI.Width(label2, Input.Pause);
                            float num3 = ButtonBindingButtonUI.Width(label3, XaphanSettings.MapScreenDisplayOptions);
                            float num4 = ButtonBindingButtonUI.Width(mode == "map" ? label8 : label9, XaphanSettings.MapScreenShowMapOrWorldMap);
                            float num5 = ButtonBindingButtonUI.Width(label10, XaphanSettings.MapScreenPlaceMarker);
                            Vector2 position = new(1830f, 1055f);
                            ButtonUI.Render(position, label, Input.MenuCancel, scale, 1f, closeWiggle.Value * 0.05f);
                            position.X -= num / 2 + 32;
                            if (XaphanModule.useUpgrades && !XaphanModule.PlayerIsControllingRemoteDrone() && !XaphanModule.DisableStatusScreen)
                            {
                                ButtonUI.Render(position, label2, Input.Pause, scale, 1f, statusWiggle.Value * 0.05f);
                                position.X -= num2 / 2 + 32;
                            }
                            ButtonBindingButtonUI.Render(position, label3, XaphanSettings.MapScreenDisplayOptions, scale, 1f, displayWiggle.Value * 0.05f);
                            position.X -= num3 / 2 + 32;
                            if (mode == "map")
                            {
                                ButtonBindingButtonUI.Render(position, label10, XaphanSettings.MapScreenPlaceMarker, scale, 1f);
                                position.X -= num5 / 2 + 32;
                            }
                            if (HasWorldMap)
                            {
                                ButtonBindingButtonUI.Render(position, mode == "map" ? label8 : label9, XaphanSettings.MapScreenShowMapOrWorldMap, scale, 1f, worldMapWiggle.Value * 0.05f);
                                position.X -= num4 / 2 + 32;
                            }
                            if (MapProgressDisplay != null && MapProgressDisplay.Visible && !MapProgressDisplay.Hidden)
                            {
                                if (MapProgressDisplay.mode != 0)
                                {
                                    string progressDisplayMode = label4 + " " + (MapProgressDisplay.mode == 2 ? label5 : label6);
                                    float progressDisplayWidth = ActiveFont.Measure(progressDisplayMode).X;
                                    ActiveFont.Draw(progressDisplayMode, new Vector2(90 + progressDisplayWidth / 4, position.Y), new Vector2(0.5f), new Vector2(scale), Color.White);
                                }
                            }
                            if (WorldMapProgressDisplay != null && WorldMapProgressDisplay.Visible && !WorldMapProgressDisplay.Hidden)
                            {
                                string progressDisplayStatus = label4 + " " + label7;
                                if (WorldMapProgressDisplay.mode != 0)
                                {
                                    float progressDisplayWidth = ActiveFont.Measure(progressDisplayStatus).X;
                                    ActiveFont.Draw(progressDisplayStatus, new Vector2(90 + progressDisplayWidth / 4, position.Y), new Vector2(0.5f), new Vector2(scale), Color.White);
                                }
                            }
                        }
                        else
                        {
                            string label = Dialog.Clean("KEY_CONFIG_CANCEL");
                            string label2 = Dialog.Clean("XaphanHelper_UI_markers_place");
                            string label3 = Dialog.Clean("XaphanHelper_UI_markers_remove");
                            float num = ButtonUI.Width(label, Input.MenuCancel);
                            float num2 = ButtonBindingButtonUI.Width(label2, XaphanSettings.MapScreenPlaceMarker);
                            float num3 = ButtonBindingButtonUI.Width(label3, XaphanSettings.MapScreenPlaceMarker);
                            Vector2 position = new(1830f, 1055f);
                            ButtonUI.Render(position, label, Input.MenuCancel, scale, 1f, closeWiggle.Value * 0.05f);
                            position.X -= num / 2 + 32;
                            ButtonBindingButtonUI.Render(position, mapDisplay.markerSelector.HoverExistingMarker ? label3 : label2, XaphanSettings.MapScreenPlaceMarker, scale, 1f, displayWiggle.Value * 0.05f);
                        }
                    }
                }
            }
        }

        public string GetSpecifiedMapName()
        {
            string mapName = "";
            AreaKey area = level.Session.Area;
            MapData MapData = AreaData.Areas[area.ID].Mode[(int)area.Mode].MapData;
            foreach (LevelData levelData in MapData.Levels)
            {
                foreach (EntityData entity in levelData.Entities)
                {
                    if (entity.Name == "XaphanHelper/InGameMapController")
                    {
                        mapName = entity.Attr("mapName");
                        break;
                    }
                }
            }
            if (string.IsNullOrEmpty(mapName))
            {
                mapName = AreaData.Get(level.Session.Area).SID;
            }
            return mapName;
        }

        public int GetSubAreaIndex()
        {
            int CurrentSubAreaIndex = 0;
            foreach (InGameMapRoomControllerData inGameMapRoomControllerData in mapDisplay.RoomControllerData)
            {
                if (inGameMapRoomControllerData.Room == mapDisplay.currentRoom)
                {
                    CurrentSubAreaIndex = inGameMapRoomControllerData.SubAreaIndex;
                    break;
                }
            }
            return CurrentSubAreaIndex;
        }

        public string GetSubAreaName()
        {
            string subAreaName = "";
            foreach (InGameMapSubAreaControllerData inGameMapSubAreaControllerData in mapDisplay.SubAreaControllerData)
            {
                if (inGameMapSubAreaControllerData.SubAreaIndex == SubAreaIndex)
                {
                    subAreaName = inGameMapSubAreaControllerData.SubAreaName;
                    break;
                }
            }
            return subAreaName;
        }
    }
}
