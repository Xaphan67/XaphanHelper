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

        public string SubArea;

        public BigTitle BigTitle;

        public BigTitle SubAreaName;

        public SwitchUIPrompt prompt;

        public bool promptChoice;

        private Wiggler statusWiggle;

        private Wiggler closeWiggle;

        private Wiggler hintWiggle;

        private Wiggler progressWiggle;

        private Wiggler worldMapWiggle;

        private float statusWiggleDelay;

        private float closeWiggleDelay;

        private float hintWiggleDelay;

        private float progressWiggleDelay;

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

        public MapProgressDisplay MapProgressDisplay;

        public MapProgressDisplay WorldMapProgressDisplay;

        public bool HasWorldMap;

        public MapScreen(Level level, bool fromStatus)
        {
            this.level = level;
            this.fromStatus = fromStatus;
            Tag = Tags.HUD;
            Add(statusWiggle = Wiggler.Create(0.4f, 4f));
            Add(closeWiggle = Wiggler.Create(0.4f, 4f));
            Add(hintWiggle = Wiggler.Create(0.4f, 4f));
            Add(progressWiggle = Wiggler.Create(0.4f, 4f));
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
                if (XaphanSettings.MapScreenShowMapOrWorldMap.Pressed && mapDisplay != null && HasWorldMap)
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
                        WorldMapProgressDisplay.Visible = true;
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
                        WorldMapProgressDisplay.Visible = false;
                        CalcMoves(MapOffset.X > 0 ? (int)MapOffset.X / 40 : 0, MapOffset.X < 0 ? Math.Abs((int)MapOffset.X / 40) : 0, MapOffset.Y > 0 ? (int)MapOffset.Y / 40 : 0, MapOffset.Y < 0 ? Math.Abs((int)MapOffset.Y / 40) : 0);
                    }
                }
                if (XaphanSettings.MapScreenShowHints.Pressed && mapDisplay != null)
                {
                    if (mapDisplay.useHints)
                    {
                        mapDisplay.ShowHints = !mapDisplay.ShowHints;
                        if (XaphanModule.ModSaveData.ShowHints[mapDisplay.Prefix])
                        {
                            Audio.Play("event:/ui/main/button_back");
                            XaphanModule.ModSaveData.ShowHints[mapDisplay.Prefix] = false;
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
                        }
                        else
                        {
                            Audio.Play("event:/ui/main/message_confirm");
                            XaphanModule.ModSaveData.ShowHints[mapDisplay.Prefix] = true;
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
                        }
                    }
                    foreach (MapDisplay display in worldMapMapDisplays)
                    {
                        if (display.useHints)
                        {
                            display.ShowHints = !display.ShowHints;
                        }
                    }
                }
                if (XaphanModule.useUpgrades && !XaphanModule.PlayerIsControllingRemoteDrone())
                {
                    if (Input.Pause.Pressed && statusWiggleDelay <= 0f && switchTimer <= 0)
                    {
                        statusWiggle.Start();
                        statusWiggleDelay = 0.5f;
                    }
                }
                if (mapDisplay != null && mapDisplay.useHints)
                {
                    if (XaphanSettings.MapScreenShowHints.Pressed && hintWiggleDelay <= 0f)
                    {
                        hintWiggle.Start();
                        hintWiggleDelay = 0.5f;
                    }
                }
                if (mapDisplay != null && MapProgressDisplay != null)
                {
                    if (XaphanSettings.MapScreenShowProgressDisplay.Pressed && progressWiggleDelay <= 0f)
                    {
                        progressWiggle.Start();
                        progressWiggleDelay = 0.5f;
                    }
                }
                if (XaphanSettings.MapScreenShowMapOrWorldMap.Pressed && worldMapWiggleDelay <= 0f)
                {
                    worldMapWiggle.Start();
                    worldMapWiggleDelay = 0.5f;
                }
                statusWiggleDelay -= Engine.DeltaTime;
                hintWiggleDelay -= Engine.DeltaTime;
                progressWiggleDelay -= Engine.DeltaTime;
                worldMapWiggleDelay -= Engine.DeltaTime;
            }
            if (Input.MenuCancel.Pressed && closeWiggleDelay <= 0f)
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
                Vector2 CurrentRoomPosition = mapDisplay.CalcRoomPosition(mapDisplay.GetRoomPosition(mapDisplay.currentRoom) + (mapDisplay.roomIsAdjusted(mapDisplay.currentRoom) ? mapDisplay.GetAdjustedPosition(mapDisplay.currentRoom) : Vector2.Zero), mapDisplay.currentRoomPosition, mapDisplay.currentRoomJustify, mapDisplay.worldmapPosition);
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
                    CurrentRoomPosition = displayWithIndicator.CalcRoomPosition(displayWithIndicator.GetRoomPosition(displayWithIndicator.currentRoom) + (displayWithIndicator.roomIsAdjusted(displayWithIndicator.currentRoom) ? displayWithIndicator.GetAdjustedPosition(displayWithIndicator.currentRoom) : Vector2.Zero), displayWithIndicator.currentRoomPosition, displayWithIndicator.currentRoomJustify, displayWithIndicator.worldmapPosition);
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
                    worldMapMapDisplays.Add(new MapDisplay(level, "worldmap", chapter, true, chapter == currentChapter ? false : true) { Visible = false });
                }
            }
            if (worldMapMapDisplays.Count > 1)
            {
                HasWorldMap = true;
                foreach (MapDisplay display in worldMapMapDisplays)
                {
                    Scene.Add(display);
                    yield return display.GenerateMap();
                }
                Vector2 ProgressPosition = new(worldMapMapDisplays[0].Grid.X + 18f, worldMapMapDisplays[0].Grid.Y);
                List<InGameMapControllerData> InGameMapControllerDatas = new();
                foreach (MapDisplay display in worldMapMapDisplays)
                {
                    InGameMapControllerDatas.Add(display.InGameMapControllerData);
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
                    WorldMapInGameMapSubAreaControllerData.AddRange(display.SubAreaControllerData);
                    WorldMapInGameMapRoomControllerData.AddRange(display.RoomControllerData);
                    WorldMapInGameMapTilesControllerData.AddRange(display.TilesControllerData);
                    WorldMapInGameMapEntitiesData.AddRange(display.EntitiesData);
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
        }

        private IEnumerator MapRoutine(Level level)
        {
            Player player = Scene.Tracker.GetEntity<Player>();
            mapDisplay = new MapDisplay(level, "map");
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
            while (!Input.ESC.Pressed && !Input.MenuCancel.Pressed && !XaphanSettings.OpenMap.Pressed && player != null)
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
                else if (mode == "map")
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
                if (Input.Pause.Pressed && XaphanModule.useUpgrades && switchTimer <= 0 && !XaphanModule.PlayerIsControllingRemoteDrone() && !XaphanModule.DisableStatusScreen)
                {
                    if (SceneAs<Level>().Session.Area.LevelSet == "Xaphan/0")
                    {
                        if (prompt == null)
                        {
                            Scene.Add(prompt = new SwitchUIPrompt(Vector2.Zero, 1));
                        }
                    }
                    else
                    {
                        Add(new Coroutine(TransitionToStatusScreen()));
                    }
                }
                yield return null;
            }
            Audio.Play("event:/ui/game/unpause");
            Add(new Coroutine(TransitionToGame()));
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
                Draw.Rect(new Vector2(90, 172), 1740, 8, Color.White);
                Draw.Rect(new Vector2(90, 180), 10, 840, Color.White);
                Draw.Rect(new Vector2(1820, 180), 10, 840, Color.White);
                Draw.Rect(new Vector2(90, 1020), 1740, 8, Color.White);
                MTexture mTexture = GFX.Gui["towerarrow"];
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
                float inputEase = 0f;
                inputEase = Calc.Approach(inputEase, 1, Engine.DeltaTime * 4f);
                if (inputEase > 0f)
                {
                    float scale = 0.5f;
                    string label = Dialog.Clean("XaphanHelper_UI_close");
                    string label2 = SceneAs<Level>().Session.Area.LevelSet != "Xaphan/0" ? Dialog.Clean("XaphanHelper_UI_abilities") : Dialog.Clean("XaphanHelper_UI_menu");
                    string label3 = Dialog.Clean("XaphanHelper_UI_showhints");
                    string label4 = Dialog.Clean("XaphanHelper_UI_hidehints");
                    string label5 = Dialog.Clean("XaphanHelper_UI_progress_area");
                    string label6 = Dialog.Clean("XaphanHelper_UI_progress_subarea");
                    string label7 = Dialog.Clean("XaphanHelper_UI_showProgress");
                    string label8 = Dialog.Clean("XaphanHelper_UI_changeProgress");
                    string label9 = Dialog.Clean("XaphanHelper_UI_hideProgress");
                    string label10 = Dialog.Clean("XaphanHelper_UI_progress_allareas");
                    string label11 = Dialog.Clean("XaphanHelper_UI_showWorldMap");
                    string label12 = Dialog.Clean("XaphanHelper_UI_showAreaMap");
                    float num = ButtonUI.Width(label, Input.MenuCancel);
                    float num2 = ButtonUI.Width(label2, Input.Pause);
                    float num3 = ButtonBindingButtonUI.Width(label3, XaphanSettings.MapScreenShowHints);
                    float num4 = ButtonBindingButtonUI.Width(label4, XaphanSettings.MapScreenShowHints);
                    float num5 = ButtonBindingButtonUI.Width(mode == "map" ? label11 : label12, XaphanSettings.MapScreenShowMapOrWorldMap);
                    Vector2 position = new(1830f, 1055f);
                    ButtonUI.Render(position, label, Input.MenuCancel, scale, 1f, closeWiggle.Value * 0.05f);
                    position.X -= num / 2 + 32;
                    if (XaphanModule.useUpgrades && !XaphanModule.PlayerIsControllingRemoteDrone() && !XaphanModule.DisableStatusScreen)
                    {
                        ButtonUI.Render(position, label2, Input.Pause, scale, 1f, statusWiggle.Value * 0.05f);
                        position.X -= num2 / 2 + 32;
                    }
                    if (mapDisplay != null && mapDisplay.useHints)
                    {
                        ButtonBindingButtonUI.Render(position, !mapDisplay.ShowHints ? label3 : label4, XaphanSettings.MapScreenShowHints, scale, 1f, hintWiggle.Value * 0.05f);
                        position.X -= (!mapDisplay.ShowHints ? num3 : num4) / 2 + 32;
                    }
                    if (HasWorldMap)
                    {
                        ButtonBindingButtonUI.Render(position, mode == "map" ? label11 : label12, XaphanSettings.MapScreenShowMapOrWorldMap, scale, 1f, worldMapWiggle.Value * 0.05f);
                        position.X -= num5 / 2 + 32;
                    }
                    if (MapProgressDisplay != null && MapProgressDisplay.Visible && !MapProgressDisplay.Hidden)
                    {
                        string progressDisplayStatus = MapProgressDisplay.mode == 0 ? (MapProgressDisplay.getSubAreaIndex() == -1 || MapProgressDisplay.SubAreaControllerData.Count == 1 ? label9 : label8) : MapProgressDisplay.mode == 1 ? label9 : label7;
                        ButtonBindingButtonUI.Render(position, progressDisplayStatus, XaphanSettings.MapScreenShowProgressDisplay, scale, 1f, progressWiggle.Value * 0.05f);
                        if (MapProgressDisplay.mode != 2)
                        {
                            string progressDisplayMode = MapProgressDisplay.mode == 0 ? label5 : label6;
                            float progressDisplayWidth = ActiveFont.Measure(progressDisplayMode).X;
                            ActiveFont.Draw(progressDisplayMode, new Vector2(90 + progressDisplayWidth / 4, position.Y), new Vector2(0.5f), new Vector2(scale), Color.White);
                        }
                    }
                    if (WorldMapProgressDisplay != null && WorldMapProgressDisplay.Visible && !WorldMapProgressDisplay.Hidden)
                    {
                        string progressDisplayStatus = WorldMapProgressDisplay.mode == 0 ? label9 : label7;
                        ButtonBindingButtonUI.Render(position, progressDisplayStatus, XaphanSettings.MapScreenShowProgressDisplay, scale, 1f, progressWiggle.Value * 0.05f);
                        if (WorldMapProgressDisplay.mode != 2)
                        {
                            float progressDisplayWidth = ActiveFont.Measure(label10).X;
                            ActiveFont.Draw(label10, new Vector2(90 + progressDisplayWidth / 4, position.Y), new Vector2(0.5f), new Vector2(scale), Color.White);
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

        public string GetSubAreaName()
        {
            string subAreaName = "";
            int CurrentSubAreaIndex = 0;
            foreach (InGameMapRoomControllerData inGameMapRoomControllerData in mapDisplay.RoomControllerData)
            {
                if (inGameMapRoomControllerData.Room == mapDisplay.currentRoom)
                {
                    CurrentSubAreaIndex = inGameMapRoomControllerData.SubAreaIndex;
                    break;
                }
            }
            foreach (InGameMapSubAreaControllerData inGameMapSubAreaControllerData in mapDisplay.SubAreaControllerData)
            {
                if (inGameMapSubAreaControllerData.SubAreaIndex == CurrentSubAreaIndex)
                {
                    subAreaName = inGameMapSubAreaControllerData.SubAreaName;
                    break;
                }
            }
            return subAreaName;
        }
    }
}
