using System;
using System.Collections;
using System.Collections.Generic;
using Celeste.Mod.XaphanHelper.Data;
using Celeste.Mod.XaphanHelper.Upgrades;
using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste.Mod.XaphanHelper.UI_Elements
{

    [Tracked(true)]
    public class MapDisplay : Entity
    {
        private Level level;

        public MapData MapData;

        public InGameMapControllerData InGameMapControllerData;

        public List<string> HeatedRooms = new();

        public List<InGameMapRoomControllerData> RoomControllerData = new();

        public List<InGameMapRoomAdjustControllerData> RoomAdjustControllerData = new();

        public List<InGameMapImageControllerData> ImageControllerData = new();

        public List<InGameMapTilesControllerData> TilesControllerData = new();

        public static List<InGameMapTilesControllerData> OldTilesControllerData = new();

        public List<InGameMapSubAreaControllerData> SubAreaControllerData = new();

        public List<InGameMapHintControllerData> HintControllerData = new();

        public List<InGameMapEntitiesData> EntitiesData = new();

        public List<InGameMapIconsData> Icons = new();

        public List<InGameMapTilesData> TilesImage = new();

        public List<InGameMapEntrancesData> Entrances = new();

        public HashSet<string> UnexploredRooms = new();

        public HashSet<string> ExtraUnexploredRooms = new();

        public bool currentRoomIndicator;

        public string[] unexploredRooms;

        public string currentRoom;

        public string mode;

        public string Prefix;

        public int chapterIndex;

        public bool MapCollected;

        public bool HeartCollected;

        public bool CassetteCollected;

        public bool BossDefeated;

        public bool BossDefeatedCM;

        public bool HideIndicator;

        public bool useHints;

        public bool ShowHints;

        public Vector2 MapPosition;

        public Vector2 currentRoomJustify;

        public Vector2 currentRoomPosition;

        public Vector2 currentMapPosition;

        public Vector2 worldmapPosition;

        public float MapWidth;

        public float MapHeight;

        public float TmpMapWidth;

        public float TmpMapHeight;

        public float MapLeft;

        public float MapTop;

        public float MostLeftRoomX;

        public float MostTopRoomY;

        public float MostRightRoomX;

        public float MostBottomRoomY;

        public float BeforeHintsMapWidth;

        public float BeforeHintsMapHeight;

        public float BeforeHintsMostLeftRoomX;

        public float BeforeHintsMostTopRoomY;

        public float BeforeHintsMostRightRoomX;

        public float BeforeHintsMostBottomRoomY;

        public float AfterHintsMapWidth;

        public float AfterHintsMapHeight;

        public float AfterHintsMostLeftRoomX;

        public float AfterHintsMostTopRoomY;

        public float AfterHintsMostRightRoomX;

        public float AfterHintsMostBottomRoomY;

        public string MostLeftRoom;

        public string MostTopRoom;

        public Color ExploredRoomColor;

        public Color UnexploredRoomColor;

        public Color SecretRoomColor;

        public Color HeatedRoomColor;

        public Color RoomBorderColor;

        public Color ElevatorColor;

        public Color GridColor;

        public float Opacity;

        public Rectangle Grid;

        public Vector2 playerPosition;

        public bool UnexploredRoomsRevealed = false;

        public bool Display;

        public int ScreenTilesX;

        public int ScreenTilesY;

        public int ExtraUnexploredRoomsCount = 0;

        public Sprite HintTargetSprite;

        public Sprite HintArrowSprite;

        public string SubAreaName;

        public static bool ForceRevealUnexploredRooms;

        public bool NoGrid;

        public MapDisplay(Level level, string mode, int chapter = -1, bool noGrid = false, bool noIndicator = false)
        {
            NoGrid = noGrid;
            ScreenTilesX = 320;
            ScreenTilesY = 184;
            AreaKey area = level.Session.Area;
            Prefix = level.Session.Area.LevelSet;
            this.mode = mode;
            currentRoom = level.Session.Level;
            if (chapter == -1)
            {
                chapterIndex = area.ChapterIndex == -1 ? 0 : area.ChapterIndex;
            }
            else
            {
                chapterIndex = chapter;
            }
            MapData = AreaData.Areas[area.ID - (area.ChapterIndex == -1 ? 0 : area.ChapterIndex) + chapterIndex].Mode[(int)area.Mode].MapData;
            InGameMapControllerData = null;
            GetInGameMapController();
            if (chapter == -1)
            {
                worldmapPosition = Vector2.Zero;
            }
            else
            {
                worldmapPosition = new Vector2(InGameMapControllerData.WorldmapOffsetX * 40, InGameMapControllerData.WorldmapOffsetY * 40);
            }
            Tag = mode == "minimap" ? (Tags.HUD | Tags.Persistent | Tags.PauseUpdate) : (Tags.HUD);
            this.level = level;
            Add(HintTargetSprite = new Sprite(GFX.Gui, "maps/"));
            HintTargetSprite.AddLoop("hint", "hint", 0.15f);
            HintTargetSprite.Play("hint");
            HintTargetSprite.Visible = false;
            Add(HintArrowSprite = new Sprite(GFX.Gui, "maps/"));
            HintArrowSprite.AddLoop("hintArrow", "hintArrow", 0.15f);
            HintArrowSprite.Play("hintArrow");
            HintArrowSprite.Visible = false;
            RoomControllerData.Clear();
            RoomAdjustControllerData.Clear();
            HeatedRooms.Clear();
            ImageControllerData.Clear();
            TilesControllerData.Clear();
            OldTilesControllerData.Clear();
            EntitiesData.Clear();
            GetRoomControllers();
            GetRoomAdjustControllers();
            GetHeatedRooms();
            GetTilesControllers();
            GetImageControllers();
            GetHintControllers();
            GetChapterUnexploredRooms();
            GetSubAreaControllers();
            GridColor = Calc.HexToColor(InGameMapControllerData.GridColor);
            foreach (string savedFlag in XaphanModule.ModSaveData.SavedFlags)
            {
                if (savedFlag.Contains(Prefix + "_Ch" + chapterIndex + "_MapShard"))
                {
                    MapCollected = true;
                }
            }
            HeartCollected = SaveData.Instance.Areas_Safe[area.ID].Modes[(int)area.Mode].HeartGem;
            CassetteCollected = SaveData.Instance.Areas_Safe[area.ID].Cassette;
            BossDefeated = XaphanModule.ModSaveData.SavedFlags.Contains(Prefix + "_Ch" + chapterIndex + "_Boss_Defeated");
            BossDefeatedCM = XaphanModule.ModSaveData.SavedFlags.Contains(Prefix + "_Ch" + chapterIndex + "_Boss_Defeated_CM");
            GetEntities();
            HideIndicator = noIndicator;
            SetGrid();
            Depth = -10001 - (NoGrid ? 1 : 0);
        }

        public static void Load()
        {
            On.Celeste.Strawberry.CollectRoutine += modStrawberryCollectRoutine;
            On.Celeste.Cassette.Removed += modCassetteRemoved;
            On.Celeste.HeartGem.EndCutscene += modHeartGemEndCutscene;
        }

        public static void Unload()
        {
            On.Celeste.Strawberry.CollectRoutine -= modStrawberryCollectRoutine;
            On.Celeste.Cassette.Removed -= modCassetteRemoved;
            On.Celeste.HeartGem.EndCutscene -= modHeartGemEndCutscene;
        }

        private static IEnumerator modStrawberryCollectRoutine(On.Celeste.Strawberry.orig_CollectRoutine orig, Strawberry self, int collectIndex)
        {
            UpdateIcons(self);
            yield return new SwapImmediately(orig(self, collectIndex));
        }

        private static void modCassetteRemoved(On.Celeste.Cassette.orig_Removed orig, Cassette self, Scene scene)
        {
            UpdateIcons(self);
            orig(self, scene);
        }

        private static void modHeartGemEndCutscene(On.Celeste.HeartGem.orig_EndCutscene orig, HeartGem self)
        {
            UpdateIcons(self);
            orig(self);
        }

        public void useHintsCheck(Level level)
        {
            AreaKey area = level.Session.Area;
            MapData MapData = AreaData.Areas[area.ID].Mode[(int)area.Mode].MapData;
            foreach (LevelData levelData in MapData.Levels)
            {
                foreach (EntityData entity in levelData.Entities)
                {
                    if (entity.Name == "XaphanHelper/InGameMapHintController")
                    {
                        useHints = true;
                        break;
                    }
                }
                if (useHints)
                {
                    break;
                }
            }
        }

        public static void UpdateIcons(Entity entity)
        {
            if (XaphanModule.ModSettings.ShowMiniMap)
            {
                MapDisplay mapDisplay = entity.SceneAs<Level>().Tracker.GetEntity<MapDisplay>();
                if (mapDisplay != null)
                {
                    mapDisplay.GenerateIcons();
                }
            }
        }

        public void SetGrid()
        {
            if (mode == "map")
            {
                Grid = new Rectangle(100, 180, 1720, 840);
            }
            else if (mode == "warp")
            {
                Grid = new Rectangle(100, 180, 920, 840);
            }
            else if (mode == "minimap")
            {
                Grid = new Rectangle(1700, 20, 200, 120);
            }
            MapPosition = new Vector2(Grid.Width / 2 + Grid.X, Grid.Height / 2 + Grid.Y);
        }

        public void UpdateMap(int chapter, string room, int index)
        {
            AreaKey area = level.Session.Area;
            HintTargetSprite = null;
            HintArrowSprite = null;
            Add(HintTargetSprite = new Sprite(GFX.Gui, "maps/"));
            HintTargetSprite.AddLoop("hint", "hint", 0.15f);
            HintTargetSprite.Play("hint");
            HintTargetSprite.Visible = false;
            Add(HintArrowSprite = new Sprite(GFX.Gui, "maps/"));
            HintArrowSprite.AddLoop("hintArrow", "hintArrow", 0.15f);
            HintArrowSprite.Play("hintArrow");
            HintArrowSprite.Visible = false;
            Icons.Clear();
            TilesImage.Clear();
            Entrances.Clear();
            chapterIndex = chapter;
            currentRoom = room;
            MapData = AreaData.Areas[area.ID - (area.ChapterIndex == -1 ? 0 : area.ChapterIndex) + chapter].Mode[(int)area.Mode].MapData;
            UnexploredRooms.Clear();
            InGameMapControllerData = null;
            RoomControllerData.Clear();
            RoomAdjustControllerData.Clear();
            HeatedRooms.Clear();
            ImageControllerData.Clear();
            HintControllerData.Clear();
            TilesControllerData.Clear();
            OldTilesControllerData.Clear();
            EntitiesData.Clear();
            SubAreaControllerData.Clear();
            GetInGameMapController();
            GetRoomControllers();
            GetRoomAdjustControllers();
            GetHeatedRooms();
            GetTilesControllers();
            GetImageControllers();
            GetHintControllers();
            GetChapterUnexploredRooms();
            GetSubAreaControllers();
            foreach (string savedFlag in XaphanModule.ModSaveData.SavedFlags)
            {
                if (savedFlag.Contains(Prefix + "_Ch" + chapter + "_MapShard"))
                {
                    MapCollected = true;
                }
            }
            HeartCollected = SaveData.Instance.Areas_Safe[area.ID - (area.ChapterIndex == -1 ? 0 : area.ChapterIndex) + chapter].Modes[(int)area.Mode].HeartGem;
            CassetteCollected = SaveData.Instance.Areas_Safe[area.ID - (area.ChapterIndex == -1 ? 0 : area.ChapterIndex) + chapter].Cassette;
            BossDefeated = XaphanModule.ModSaveData.SavedFlags.Contains(Prefix + "_Ch" + chapterIndex + "_Boss_Defeated");
            BossDefeatedCM = XaphanModule.ModSaveData.SavedFlags.Contains(Prefix + "_Ch" + chapterIndex + "_Boss_Defeated_CM");
            GetEntities();
            SetCurrentRoomCoordinates(GetRoomOffset(MapData, currentRoom, index));
            Add(new Coroutine(GenerateMap()));
        }

        public void UpdateExtraRooms()
        {
            ExtraUnexploredRoomsCount = ExtraUnexploredRooms.Count;
            GetChapterUnexploredRooms();
            GenerateTiles();
            GenerateEntrances();
        }

        public Vector2 GetRoomOffset(MapData mapData, string room, int index)
        {
            if (mode == "warp" && level.Tracker.GetEntity<WarpScreen>() is WarpScreen warpScreen)
            {
                Vector2 pos = warpScreen.SelectedWarp.Position;
                return new Vector2((float)Math.Floor(pos.X / ScreenTilesX), (float)Math.Floor(pos.Y / ScreenTilesY)) * 40;
            }
            foreach (LevelData levelData in mapData.Levels)
            {
                if (levelData.Name == room)
                {
                    foreach (EntityData entity in levelData.Entities)
                    {
                        if (entity.Name == "XaphanHelper/WarpStation" && entity.Int("index") == index)
                        {
                            return new Vector2((float)Math.Floor(entity.Position.X / ScreenTilesX), (float)Math.Floor(entity.Position.Y / ScreenTilesY)) * 40;
                        }
                    }
                }
            }
            return Vector2.Zero;
        }

        private void GetInGameMapController()
        {
            foreach (LevelData level in MapData.Levels)
            {
                foreach (EntityData entity in level.Entities)
                {
                    if (entity.Name == "XaphanHelper/InGameMapController")
                    {
                        InGameMapControllerData = new InGameMapControllerData(entity.Attr("exploredRoomColor", "D83890"), entity.Attr("unexploredRoomColor", "000080"), entity.Attr("secretRoomColor", "057A0C"),
                            entity.Attr("heatedRoomColor", "FF650D"), entity.Attr("roomBorderColor", "FFFFFF"), entity.Attr("elevatorColor", "F80000"), entity.Attr("gridColor", "262626"), entity.Bool("revealUnexploredRooms", false), entity.Bool("hideIconsInUnexploredRooms", false), entity.Attr("showProgress", "Always"), entity.Bool("hideMapProgress", false), entity.Bool("hideStrawberryProgress", false), entity.Bool("hideMoonberryProgress", false), entity.Bool("hideUpgradeProgress", false), entity.Bool("hideHeartProgress", false), entity.Bool("hideCassetteProgress", false), entity.Attr("customCollectablesProgress"), entity.Attr("secretsCustomCollectablesProgress"), entity.Int("worldmapOffsetX"), entity.Int("worldmapOffsetY"));
                        break;
                    }
                }
            }
        }

        private void GetSubAreaControllers()
        {
            foreach (LevelData level in MapData.Levels)
            {
                foreach (EntityData entity in level.Entities)
                {
                    if (entity.Name == "XaphanHelper/InGameMapSubAreaController")
                    {
                        SubAreaControllerData.Add(new InGameMapSubAreaControllerData(entity.Attr("exploredRoomColor", "D83890"), entity.Attr("unexploredRoomColor", "000080"), entity.Attr("secretRoomColor", "057A0C"),
                            entity.Attr("heatedRoomColor", "FF650D"), entity.Attr("roomBorderColor", "FFFFFF"), entity.Attr("elevatorColor", "F80000"), entity.Attr("subAreaName"), entity.Int("subAreaIndex", 0)));
                    }
                }
            }
        }

        private void GetRoomControllers()
        {
            foreach (LevelData level in MapData.Levels)
            {
                foreach (EntityData entity in level.Entities)
                {
                    if (entity.Name == "XaphanHelper/InGameMapRoomController")
                    {
                        RoomControllerData.Add(new InGameMapRoomControllerData(level.Name, entity.Bool("showUnexplored"), entity.Int("mapShardIndex", 0), entity.Bool("secret"), entity.Attr("entrance0Position"), entity.Attr("entrance0Cords"), entity.Attr("entrance1Position"),
                            entity.Attr("entrance1Cords"), entity.Attr("entrance2Position"), entity.Attr("entrance2Cords"), entity.Attr("entrance3Position"), entity.Attr("entrance3Cords"), entity.Attr("entrance4Position"),
                            entity.Attr("entrance4Cords"), entity.Attr("entrance5Position"), entity.Attr("entrance5Cords"), entity.Attr("entrance6Position"), entity.Attr("entrance6Cords"), entity.Attr("entrance7Position"),
                            entity.Attr("entrance7Cords"), entity.Attr("entrance8Position"), entity.Attr("entrance8Cords"), entity.Attr("entrance9Position"), entity.Attr("entrance9Cords"), entity.Int("entrance0Offset"),
                            entity.Int("entrance1Offset"), entity.Int("entrance2Offset"), entity.Int("entrance3Offset"), entity.Int("entrance4Offset"), entity.Int("entrance5Offset"), entity.Int("entrance6Offset"),
                            entity.Int("entrance7Offset"), entity.Int("entrance8Offset"), entity.Int("entrance9Offset"), entity.Int("subAreaIndex", 0)));
                        break;
                    }
                }
            }
        }

        private void GetRoomAdjustControllers()
        {
            foreach (LevelData level in MapData.Levels)
            {
                foreach (EntityData entity in level.Entities)
                {
                    if (entity.Name == "XaphanHelper/InGameMapRoomAdjustController")
                    {
                        RoomAdjustControllerData.Add(new InGameMapRoomAdjustControllerData(level.Name, entity.Int("positionX"), entity.Int("positionY"), entity.Int("sizeX"), entity.Int("sizeX"), entity.Attr("hiddenTiles"), entity.Bool("removeEntrance0"),
                            entity.Bool("removeEntrance1"), entity.Bool("removeEntrance2"), entity.Bool("removeEntrance3"), entity.Bool("removeEntrance4"), entity.Bool("removeEntrance5"), entity.Bool("removeEntrance6"),
                            entity.Bool("removeEntrance7"), entity.Bool("removeEntrance8"), entity.Bool("removeEntrance9"), entity.Bool("ignoreIcons")));
                    }
                }
            }
        }

        private void GetTilesControllers()
        {
            foreach (LevelData level in MapData.Levels)
            {
                foreach (EntityData entity in level.Entities)
                {
                    if (entity.Name == "XaphanHelper/InGameMapTilesController")
                    {
                        TilesControllerData.Add(new InGameMapTilesControllerData(chapterIndex, level.Name, entity.Attr("tile0Cords"), entity.Attr("tile0"), entity.Attr("tile1Cords"), entity.Attr("tile1"), entity.Attr("tile2Cords"), entity.Attr("tile2"),
                            entity.Attr("tile3Cords"), entity.Attr("tile3"), entity.Attr("tile4Cords"), entity.Attr("tile4"), entity.Attr("tile5Cords"), entity.Attr("tile5"), entity.Attr("tile6Cords"), entity.Attr("tile6"),
                            entity.Attr("tile7Cords"), entity.Attr("tile7"), entity.Attr("tile8Cords"), entity.Attr("tile8"), entity.Attr("tile9Cords"), entity.Attr("tile9"), entity.Attr("display")));
                        OldTilesControllerData.Add(new InGameMapTilesControllerData(chapterIndex, level.Name, entity.Attr("tile0Cords"), entity.Attr("tile0"), entity.Attr("tile1Cords"), entity.Attr("tile1"), entity.Attr("tile2Cords"), entity.Attr("tile2"),
                            entity.Attr("tile3Cords"), entity.Attr("tile3"), entity.Attr("tile4Cords"), entity.Attr("tile4"), entity.Attr("tile5Cords"), entity.Attr("tile5"), entity.Attr("tile6Cords"), entity.Attr("tile6"),
                            entity.Attr("tile7Cords"), entity.Attr("tile7"), entity.Attr("tile8Cords"), entity.Attr("tile8"), entity.Attr("tile9Cords"), entity.Attr("tile9"), entity.Attr("display")));
                    }
                }
            }
        }

        private void GetImageControllers()
        {
            foreach (LevelData level in MapData.Levels)
            {
                foreach (EntityData entity in level.Entities)
                {
                    if (entity.Name == "XaphanHelper/InGameMapImageController")
                    {
                        ImageControllerData.Add(new InGameMapImageControllerData(chapterIndex, level.Name, entity.Attr("imagePath"), entity.Attr("color")));
                    }
                }
            }
        }

        private void GetHintControllers()
        {
            foreach (LevelData level in MapData.Levels)
            {
                foreach (EntityData entity in level.Entities)
                {
                    if (entity.Name == "XaphanHelper/InGameMapHintController")
                    {
                        HintControllerData.Add(new InGameMapHintControllerData(chapterIndex, level.Name, entity.Attr("tileCords"), entity.Attr("displayFlags").Split(','), entity.Attr("hideFlag"), entity.Attr("type"), entity.Attr("direction"), entity.Bool("removeWhenReachedByPlayer")));
                    }
                }
            }
        }

        private void GetEntities()
        {
            foreach (LevelData level in MapData.Levels)
            {
                foreach (EntityData entity in level.Entities)
                {
                    if (StrawberryRegistry.TrackableContains(entity.Name) && entity.Bool("moon") == false)
                    {
                        EntitiesData.Add(new InGameMapEntitiesData(chapterIndex, level.Name, level, "strawberry", new Vector2(entity.Position.X, entity.Position.Y), new Vector2((float)Math.Floor(entity.Position.X / ScreenTilesX), (float)Math.Floor(entity.Position.Y / ScreenTilesY)), MapData.Area, entity.ID));
                    }
                    else if (StrawberryRegistry.TrackableContains(entity.Name) && entity.Bool("moon") == true)
                    {
                        EntitiesData.Add(new InGameMapEntitiesData(chapterIndex, level.Name, level, "moonberry", new Vector2(entity.Position.X, entity.Position.Y), new Vector2((float)Math.Floor(entity.Position.X / ScreenTilesX), (float)Math.Floor(entity.Position.Y / ScreenTilesY)), MapData.Area, entity.ID));
                    }
                    else if ((entity.Name == "blackGem" || entity.Name == "XaphanHelper/Ch2HeartController" || entity.Name == "MaxHelpingHand/ReskinnableCrystalHeart") && IsInBounds(level, new Vector2(entity.Position.X, entity.Position.Y)))
                    {
                        EntitiesData.Add(new InGameMapEntitiesData(chapterIndex, level.Name, level, "heart", new Vector2(entity.Position.X, entity.Position.Y), new Vector2((float)Math.Floor(entity.Position.X / ScreenTilesX), (float)Math.Floor(entity.Position.Y / ScreenTilesY))));
                    }
                    else if (entity.Name == "XaphanHelper/UpgradeCollectable" && (entity.Attr("upgrade") == "Map" || entity.Attr("upgrade") == "MapShard"))
                    {
                        EntitiesData.Add(new InGameMapEntitiesData(chapterIndex, level.Name, level, "map", new Vector2(entity.Position.X, entity.Position.Y), new Vector2((float)Math.Floor(entity.Position.X / ScreenTilesX), (float)Math.Floor(entity.Position.Y / ScreenTilesY)), upgradeCollectableUpgrade: entity.Attr("upgrade"), upgradeCollectableMapShardIndex: entity.Int("mapShardIndex")));
                    }
                    else if (entity.Name == "cassette")
                    {
                        EntitiesData.Add(new InGameMapEntitiesData(chapterIndex, level.Name, level, "cassette", new Vector2(entity.Position.X, entity.Position.Y), new Vector2((float)Math.Floor(entity.Position.X / ScreenTilesX), (float)Math.Floor(entity.Position.Y / ScreenTilesY))));
                    }
                    else if (entity.Name == "XaphanHelper/CustomFollower")
                    {
                        string str = entity.Attr("type").Replace(" ", "");
                        string type = (char.ToLower(str[0]) + str.Substring(1));
                        EntitiesData.Add(new InGameMapEntitiesData(chapterIndex, level.Name, level, type, new Vector2(entity.Position.X, entity.Position.Y), new Vector2((float)Math.Floor(entity.Position.X / ScreenTilesX), (float)Math.Floor(entity.Position.Y / ScreenTilesY)), MapData.Area, entity.ID));
                    }
                    else if (entity.Name == "XaphanHelper/WarpStation")
                    {
                        EntitiesData.Add(new InGameMapEntitiesData(chapterIndex, level.Name, level, "warp", new Vector2(entity.Position.X, entity.Position.Y), new Vector2((float)Math.Floor(entity.Position.X / ScreenTilesX), (float)Math.Floor(entity.Position.Y / ScreenTilesY))));
                    }
                    else if (entity.Name == "XaphanHelper/UpgradeCollectable" && (entity.Attr("upgrade") != "Map" && entity.Attr("upgrade") != "MapShard"))
                    {
                        EntitiesData.Add(new InGameMapEntitiesData(chapterIndex, level.Name, level, "upgrade", new Vector2(entity.Position.X, entity.Position.Y), new Vector2((float)Math.Floor(entity.Position.X / ScreenTilesX), (float)Math.Floor(entity.Position.Y / ScreenTilesY)), upgradeCollectableUpgrade: entity.Attr("upgrade")));
                    }
                    else if (entity.Name == "XaphanHelper/CustomFinalBoss" || entity.Name == "XaphanHelper/Torizo" || entity.Name == "XaphanHelper/AncientGuardian" || entity.Name == "XaphanHelper/Genesis")
                    {
                        EntitiesData.Add(new InGameMapEntitiesData(chapterIndex, level.Name, level, "boss", new Vector2(entity.Position.X, entity.Position.Y), new Vector2((float)Math.Floor(entity.Position.X / ScreenTilesX), (float)Math.Floor(entity.Position.Y / ScreenTilesY))));
                    }
                    else if (entity.Name == "XaphanHelper/BubbleDoor")
                    {
                        EntitiesData.Add(new InGameMapEntitiesData(chapterIndex, level.Name, level, "bubbleDoor/" + entity.Attr("side") + "-" + entity.Attr("color"), new Vector2(entity.Position.X, entity.Position.Y), new Vector2((float)Math.Floor(entity.Position.X / ScreenTilesX), (float)Math.Floor(entity.Position.Y / ScreenTilesY))));
                    }
                    else if (entity.Name == "XaphanHelper/CustomCollectable" && !string.IsNullOrEmpty(entity.Attr("mapIcon")))
                    {
                        EntitiesData.Add(new InGameMapEntitiesData(chapterIndex, level.Name, level, "customCollectable", new Vector2(entity.Position.X, entity.Position.Y), new Vector2((float)Math.Floor(entity.Position.X / ScreenTilesX), (float)Math.Floor(entity.Position.Y / ScreenTilesY)), customCollectableMapIcon: entity.Attr("mapIcon"), customCollectableFlag: entity.Attr("flag")));
                    }
                    else if (entity.Name == "XaphanHelper/CollectableDoor")
                    {
                        EntitiesData.Add(new InGameMapEntitiesData(chapterIndex, level.Name, level, "collectableDoor/" + (entity.Attr("mode").Contains("Heart") ? "heart" : (entity.Attr("mode").Contains("Strawberries") ? "strawberry" : (entity.Attr("mode").Contains("Golden") ? "goldenStrawberry" : (entity.Attr("mode").Contains("Cassette") ? "cassette" : "flag")))),
                        new Vector2(entity.Position.X, entity.Position.Y), new Vector2((float)Math.Floor(entity.Position.X / ScreenTilesX), (float)Math.Floor(entity.Position.Y / ScreenTilesY)), collectableDoorFlagID: entity.Level.Name + "_" + entity.ID, collectableDoorInteriorColor: entity.Attr("interiorColor"), collectableDoorEdgesColor: entity.Attr("edgesColor"), collectableDoorOrientation: entity.Attr("orientation"), collectableDoorMapIcon: entity.Attr("mapIcon"), collectableDoorRequires: entity.Int("requires")));
                    }
                    else if (entity.Name == "XaphanHelper/PipeGate")
                    {
                        EntitiesData.Add(new InGameMapEntitiesData(chapterIndex, level.Name, level, "pipeGate" + entity.Attr("direction"), new Vector2(entity.Position.X, entity.Position.Y), new Vector2((float)Math.Floor(entity.Position.X / ScreenTilesX), (float)Math.Floor(entity.Position.Y / ScreenTilesY))));
                    }
                    else if (entity.Name == "XaphanHelper/BigScreen" && entity.Bool("noInteract") == false)
                    {
                        EntitiesData.Add(new InGameMapEntitiesData(chapterIndex, level.Name, level, "bigScreen", new Vector2(entity.Position.X, entity.Position.Y), new Vector2((float)Math.Floor(entity.Position.X / ScreenTilesX), (float)Math.Floor(entity.Position.Y / ScreenTilesY))));
                    }
                }
            }
        }

        public void GetHeatedRooms()
        {
            foreach (LevelData level in MapData.Levels)
            {
                foreach (EntityData entity in level.Entities)
                {
                    if (entity.Name == "XaphanHelper/HeatController")
                    {
                        HeatedRooms.Add(level.Name);
                        break;
                    }
                }
            }
        }

        public Vector2 CalcRoomPosition(Vector2 Room, Vector2 currentRoomPosition, Vector2 currentRoomJustify, Vector2 worldmapPosition)
        {
            int PosXResult;
            float PosXFinal = 0;
            int PosXInt = Math.DivRem((int)Room.X, ScreenTilesX, out PosXResult);
            PosXFinal = PosXInt * ScreenTilesX + ((PosXResult > -ScreenTilesX / 2 && PosXResult < ScreenTilesX / 2) ? 0 : PosXResult);
            int PosYResult;
            float PosYFinal = 0;
            int PosYInt = Math.DivRem((int)Room.Y, ScreenTilesY, out PosYResult);
            PosYFinal = PosYInt * ScreenTilesY + ((PosYResult >= -ScreenTilesY / 2 && PosYResult <= ScreenTilesY / 2) ? 0 : PosYResult);
            return Position = MapPosition + new Vector2((float)Math.Floor(PosXFinal / ScreenTilesX) * 40, (float)Math.Round(PosYFinal / ScreenTilesY, 0, MidpointRounding.AwayFromZero) * 40) - ((mode == "map") ? currentMapPosition : currentRoomPosition) - currentRoomJustify + worldmapPosition;
        }

        public void SetCurrentMapCoordinates()
        {
            if (!NoGrid)
            {
                float OriginMapX = MostLeftRoomX + MapWidth / 2;
                float OriginMapY = MostTopRoomY + MapHeight / 2;
                currentMapPosition = new Vector2((float)Math.Floor(OriginMapX / ScreenTilesX) * 40, (float)Math.Ceiling(OriginMapY / ScreenTilesY) * 40);
                float ConvertedMapWidth = (float)Math.Floor(MapWidth / ScreenTilesX);
                if (ConvertedMapWidth / 40 % 2 != 0)
                {
                    currentRoomJustify.X = 20;
                }
                else
                {
                    currentRoomJustify.X = -20;
                }
                float ConvertedMapHeight = (float)Math.Round(MapHeight / ScreenTilesY, 0, MidpointRounding.AwayFromZero);
                if (ConvertedMapHeight / (ScreenTilesY / 8) % 2 != 0)
                {
                    currentRoomJustify.Y = -20;
                }
                else
                {
                    currentRoomJustify.Y = 20;
                }
            }
        }

        public void GetMapSize()
        {
            string[] baseStr;
            string[] str;
            Vector2 MostRightRoomSize;
            Vector2 MostBottomRoomSize;
            MostLeftRoomX = 1000000f;
            MostTopRoomY = 1000000f;
            MostRightRoomX = -1000000f;
            MostBottomRoomY = -1000000f;
            BeforeHintsMostLeftRoomX = 1000000f;
            BeforeHintsMostTopRoomY = 1000000f;
            BeforeHintsMostRightRoomX = -1000000f;
            BeforeHintsMostBottomRoomY = -1000000f;
            AfterHintsMostLeftRoomX = 1000000f;
            AfterHintsMostTopRoomY = 1000000f;
            AfterHintsMostRightRoomX = -1000000f;
            AfterHintsMostBottomRoomY = -1000000f;
            if (NoGrid)
            {
                foreach (InGameMapRoomControllerData room in RoomControllerData)
                {
                    float AdjustX = 0;
                    float AdjustY = 0;
                    if (roomUseTilesController(room.Room))
                    {
                        List<Vector2> TilesPosition = GetTilesPosition(room.Room);
                        float MostLeftTileX = 1000f;
                        float MostTopTileY = 1000f;
                        float MostRightTileX = -1000f;
                        float MostBottomTileY = -1000f;
                        for (int i = 0; i <= TilesPosition.Count - 1; i++)
                        {
                            if (TilesPosition[i].X < MostLeftTileX)
                            {
                                MostLeftTileX = TilesPosition[i].X;
                            }
                            if (TilesPosition[i].Y < MostTopTileY)
                            {
                                MostTopTileY = TilesPosition[i].Y;
                            }
                            if (TilesPosition[i].X + 1 > MostRightTileX)
                            {
                                MostRightTileX = TilesPosition[i].X + 1;
                            }
                            if (TilesPosition[i].Y + 1 > MostBottomTileY)
                            {
                                MostBottomTileY = TilesPosition[i].Y + 1;
                            }
                        }
                        MostLeftTileX *= ScreenTilesX;
                        MostTopTileY *= ScreenTilesY;
                        MostRightTileX *= ScreenTilesX;
                        MostBottomTileY *= ScreenTilesY;
                        if (roomIsAdjusted(room.Room))
                        {
                            AdjustX = GetAdjustedPosition(room.Room).X;
                            AdjustY = GetAdjustedPosition(room.Room).Y;
                        }
                        if (GetRoomPosition(room.Room).X + AdjustX + MostLeftTileX < MostLeftRoomX)
                        {
                            MostLeftRoomX = GetRoomPosition(room.Room).X + AdjustX + MostLeftTileX;
                            MostLeftRoom = room.Room;
                        }
                        if (GetRoomPosition(room.Room).Y + AdjustY + MostTopTileY < MostTopRoomY)
                        {
                            MostTopRoomY = GetRoomPosition(room.Room).Y + AdjustY + MostTopTileY;
                            MostTopRoom = room.Room;
                        }
                        if (GetRoomPosition(room.Room).X + AdjustX + MostRightTileX > MostRightRoomX)
                        {
                            MostRightRoomX = GetRoomPosition(room.Room).X + AdjustX + MostRightTileX;
                        }
                        if (GetRoomPosition(room.Room).Y + AdjustY + MostBottomTileY > MostBottomRoomY)
                        {
                            MostBottomRoomY = GetRoomPosition(room.Room).Y + AdjustY + MostBottomTileY;
                        }
                    }
                    else
                    {
                        if (roomIsAdjusted(room.Room))
                        {
                            AdjustX = GetAdjustedPosition(room.Room).X;
                            AdjustY = GetAdjustedPosition(room.Room).Y;
                        }
                        if (GetRoomPosition(room.Room).X + AdjustX < MostLeftRoomX)
                        {
                            MostLeftRoomX = GetRoomPosition(room.Room).X + AdjustX;
                            MostLeftRoom = room.Room;
                        }
                        if (GetRoomPosition(room.Room).Y + AdjustY < MostTopRoomY)
                        {
                            MostTopRoomY = GetRoomPosition(room.Room).Y + AdjustY;
                            MostTopRoom = room.Room;
                        }
                        MostRightRoomSize = GetRoomSize(room.Room);
                        if (GetRoomPosition(room.Room).X + AdjustX + MostRightRoomSize.X * 8 > MostRightRoomX)
                        {
                            MostRightRoomX = GetRoomPosition(room.Room).X + AdjustX + MostRightRoomSize.X * 8;
                        }
                        MostBottomRoomSize = GetRoomSize(room.Room);
                        if (GetRoomPosition(room.Room).Y + AdjustY + (MostBottomRoomSize.Y / 40 * (ScreenTilesY / 8)) * 8 > MostBottomRoomY)
                        {
                            MostBottomRoomY = GetRoomPosition(room.Room).Y + AdjustY + (MostBottomRoomSize.Y / 40 * (ScreenTilesY / 8)) * 8;
                        }
                    }
                }
                TmpMapWidth = MostRightRoomX - MostLeftRoomX;
                TmpMapHeight = MostBottomRoomY - MostTopRoomY;
                float TmpOriginMapX = MostLeftRoomX + TmpMapWidth / 2;
                float TmpOriginMapY = MostTopRoomY + TmpMapHeight / 2;
                currentMapPosition = new Vector2((float)Math.Floor(TmpOriginMapX / ScreenTilesX) * 40, (float)Math.Ceiling(TmpOriginMapY / ScreenTilesY) * 40);
                /*MostLeftRoomX = 1000000f;
                MostTopRoomY = 1000000f;
                MostRightRoomX = -1000000f;
                MostBottomRoomY = -1000000f;*/
            }
            else
            {
                foreach (string visitedRoom in XaphanModule.ModSaveData.VisitedRooms)
                {
                    str = visitedRoom.Split('/');
                    if (str[2] == "Ch" + chapterIndex)
                    {
                        float AdjustX = 0;
                        float AdjustY = 0;
                        if (roomUseTilesController(str[3]))
                        {
                            List<Vector2> TilesPosition = GetTilesPosition(str[3]);
                            float MostLeftTileX = 1000f;
                            float MostTopTileY = 1000f;
                            float MostRightTileX = -1000f;
                            float MostBottomTileY = -1000f;
                            for (int i = 0; i <= TilesPosition.Count - 1; i++)
                            {
                                if (XaphanModule.ModSaveData.VisitedRoomsTiles.Contains(Prefix + "/Ch" + chapterIndex + "/" + str[3] + "-" + TilesPosition[i].X + "-" + TilesPosition[i].Y))
                                {
                                    if (TilesPosition[i].X < MostLeftTileX)
                                    {
                                        MostLeftTileX = TilesPosition[i].X;
                                    }
                                    if (TilesPosition[i].Y < MostTopTileY)
                                    {
                                        MostTopTileY = TilesPosition[i].Y;
                                    }
                                    if (TilesPosition[i].X + 1 > MostRightTileX)
                                    {
                                        MostRightTileX = TilesPosition[i].X + 1;
                                    }
                                    if (TilesPosition[i].Y + 1 > MostBottomTileY)
                                    {
                                        MostBottomTileY = TilesPosition[i].Y + 1;
                                    }
                                }
                            }
                            MostLeftTileX *= ScreenTilesX;
                            MostTopTileY *= ScreenTilesY;
                            MostRightTileX *= ScreenTilesX;
                            MostBottomTileY *= ScreenTilesY;
                            if (roomIsAdjusted(str[3]))
                            {
                                AdjustX = GetAdjustedPosition(str[3]).X;
                                AdjustY = GetAdjustedPosition(str[3]).Y;
                            }
                            if (GetRoomPosition(str[3]).X + AdjustX + MostLeftTileX < MostLeftRoomX)
                            {
                                MostLeftRoomX = GetRoomPosition(str[3]).X + AdjustX + MostLeftTileX;
                                MostLeftRoom = str[3];
                            }
                            if (GetRoomPosition(str[3]).Y + AdjustY + MostTopTileY < MostTopRoomY)
                            {
                                MostTopRoomY = GetRoomPosition(str[3]).Y + AdjustY + MostTopTileY;
                                MostTopRoom = str[3];
                            }
                            if (GetRoomPosition(str[3]).X + AdjustX + MostRightTileX > MostRightRoomX)
                            {
                                MostRightRoomX = GetRoomPosition(str[3]).X + AdjustX + MostRightTileX;
                            }
                            if (GetRoomPosition(str[3]).Y + AdjustY + MostBottomTileY > MostBottomRoomY)
                            {
                                MostBottomRoomY = GetRoomPosition(str[3]).Y + AdjustY + MostBottomTileY;
                            }
                        }
                        else
                        {
                            if (roomIsAdjusted(str[3]))
                            {
                                AdjustX = GetAdjustedPosition(str[3]).X;
                                AdjustY = GetAdjustedPosition(str[3]).Y;
                            }
                            if (GetRoomPosition(str[3]).X + AdjustX < MostLeftRoomX)
                            {
                                MostLeftRoomX = GetRoomPosition(str[3]).X + AdjustX;
                                MostLeftRoom = str[3];
                            }
                            if (GetRoomPosition(str[3]).Y + AdjustY < MostTopRoomY)
                            {
                                MostTopRoomY = GetRoomPosition(str[3]).Y + AdjustY;
                                MostTopRoom = str[3];
                            }
                            MostRightRoomSize = GetRoomSize(str[3]);
                            if (GetRoomPosition(str[3]).X + AdjustX + MostRightRoomSize.X * 8 > MostRightRoomX)
                            {
                                MostRightRoomX = GetRoomPosition(str[3]).X + AdjustX + MostRightRoomSize.X * 8;
                            }
                            MostBottomRoomSize = GetRoomSize(str[3]);
                            if (GetRoomPosition(str[3]).Y + AdjustY + (MostBottomRoomSize.Y / 40 * (ScreenTilesY / 8)) * 8 > MostBottomRoomY)
                            {
                                MostBottomRoomY = GetRoomPosition(str[3]).Y + AdjustY + (MostBottomRoomSize.Y / 40 * (ScreenTilesY / 8)) * 8;
                            }
                        }
                    }
                }
                List<int> mapShards = GetUnlockedMapShards();
                if (MapCollected || RevealUnexploredRooms())
                {
                    foreach (string unexploredRoom in UnexploredRooms)
                    {
                        baseStr = unexploredRoom.Split(':');
                        str = baseStr[0].Split('/');
                        foreach (int mapshard in mapShards)
                        {
                            if (str[0] == "Ch" + chapterIndex && int.Parse(baseStr[1]) == mapshard)
                            {
                                float AdjustX = 0;
                                float AdjustY = 0;
                                if (roomUseTilesController(str[1]))
                                {
                                    List<Vector2> TilesPosition = GetTilesPosition(str[1]);
                                    float MostLeftTileX = 1000f;
                                    float MostTopTileY = 1000f;
                                    float MostRightTileX = -1000f;
                                    float MostBottomTileY = -1000f;
                                    bool skipTile = false;
                                    for (int i = 0; i <= TilesPosition.Count - 1; i++)
                                    {
                                        string[] tiles = null;
                                        if (roomHasAdjustController(str[1]))
                                        {
                                            tiles = GetHiddenTiles(str[1]).Split(',');
                                            foreach (string tile in tiles)
                                            {
                                                if (!string.IsNullOrEmpty(tile))
                                                {
                                                    string[] str2 = tile.Split('-');
                                                    int tileX = int.Parse(str2[0]);
                                                    int tileY = int.Parse(str2[1]);
                                                    if (tileX == TilesPosition[i].X && tileY == TilesPosition[i].Y)
                                                    {
                                                        skipTile = true;
                                                    }
                                                }
                                            }
                                        }
                                        if (skipTile)
                                        {
                                            continue;
                                        }
                                        if (TilesPosition[i].X < MostLeftTileX)
                                        {
                                            MostLeftTileX = TilesPosition[i].X;
                                        }
                                        if (TilesPosition[i].Y < MostTopTileY)
                                        {
                                            MostTopTileY = TilesPosition[i].Y;
                                        }
                                        if (TilesPosition[i].X + 1 > MostRightTileX)
                                        {
                                            MostRightTileX = TilesPosition[i].X + 1;
                                        }
                                        if (TilesPosition[i].Y + 1 > MostBottomTileY)
                                        {
                                            MostBottomTileY = TilesPosition[i].Y + 1;
                                        }
                                    }
                                    MostLeftTileX *= ScreenTilesX;
                                    MostTopTileY *= ScreenTilesY;
                                    MostRightTileX *= ScreenTilesX;
                                    MostBottomTileY *= ScreenTilesY;
                                    if (roomIsAdjusted(str[1]))
                                    {
                                        AdjustX = GetAdjustedPosition(str[1]).X;
                                        AdjustY = GetAdjustedPosition(str[1]).Y;
                                    }
                                    if (GetRoomPosition(str[1]).X + AdjustX + MostLeftTileX < MostLeftRoomX)
                                    {
                                        MostLeftRoomX = GetRoomPosition(str[1]).X + AdjustX + MostLeftTileX;
                                        MostLeftRoom = str[1];
                                    }
                                    if (GetRoomPosition(str[1]).Y + AdjustY + MostTopTileY < MostTopRoomY)
                                    {
                                        MostTopRoomY = GetRoomPosition(str[1]).Y + AdjustY + MostTopTileY;
                                        MostTopRoom = str[1];
                                    }
                                    if (GetRoomPosition(str[1]).X + AdjustX + MostRightTileX > MostRightRoomX)
                                    {
                                        MostRightRoomX = GetRoomPosition(str[1]).X + AdjustX + MostRightTileX;
                                    }
                                    if (GetRoomPosition(str[1]).Y + AdjustY + MostBottomTileY > MostBottomRoomY)
                                    {
                                        MostBottomRoomY = GetRoomPosition(str[1]).Y + AdjustY + MostBottomTileY;
                                    }
                                }
                                else
                                {
                                    if (roomIsAdjusted(str[1]))
                                    {
                                        AdjustX = GetAdjustedPosition(str[1]).X;
                                        AdjustY = GetAdjustedPosition(str[1]).Y;
                                    }
                                    if (GetRoomPosition(str[1]).X + AdjustX < MostLeftRoomX)
                                    {
                                        MostLeftRoomX = GetRoomPosition(str[1]).X + AdjustX;
                                        MostLeftRoom = str[1];
                                    }
                                    if (GetRoomPosition(str[1]).Y + AdjustY < MostTopRoomY)
                                    {
                                        MostTopRoomY = GetRoomPosition(str[1]).Y + AdjustY;
                                        MostTopRoom = str[1];
                                    }
                                    MostRightRoomSize = GetRoomSize(str[1]);
                                    if (GetRoomPosition(str[1]).X + AdjustX + MostRightRoomSize.X * 8 > MostRightRoomX)
                                    {
                                        MostRightRoomX = GetRoomPosition(str[1]).X + AdjustX + MostRightRoomSize.X * 8;
                                    }
                                    MostBottomRoomSize = GetRoomSize(str[1]);
                                    if (GetRoomPosition(str[1]).Y + AdjustY + (MostBottomRoomSize.Y / 40 * (ScreenTilesY / 8)) * 8 > MostBottomRoomY)
                                    {
                                        MostBottomRoomY = GetRoomPosition(str[1]).Y + AdjustY + (MostBottomRoomSize.Y / 40 * (ScreenTilesY / 8)) * 8;
                                    }
                                }
                            }
                        }
                    }
                }
                List<string> SkippedRooms = new();
                if (NoGrid)
                {
                    foreach (InGameMapImageControllerData data in ImageControllerData)
                    {
                        foreach (string unexploredRoom in ExtraUnexploredRooms)
                        {
                            str = unexploredRoom.Split('/');
                            if (data.Room == str[1] && data.ChapterIndex == chapterIndex)
                            {
                                SkippedRooms.Add(unexploredRoom);
                            }
                        }
                    }
                }
                foreach (string unexploredRoom in ExtraUnexploredRooms)
                {
                    if (!SkippedRooms.Contains(unexploredRoom))
                    {
                        str = unexploredRoom.Split('/');
                        if (str[0] == "Ch" + chapterIndex)
                        {
                            float AdjustX = 0;
                            float AdjustY = 0;
                            if (roomUseTilesController(str[1]))
                            {
                                List<Vector2> TilesPosition = GetTilesPosition(str[1]);
                                float MostLeftTileX = 1000f;
                                float MostTopTileY = 1000f;
                                float MostRightTileX = -1000f;
                                float MostBottomTileY = -1000f;
                                bool skipTile = false;
                                for (int i = 0; i <= TilesPosition.Count - 1; i++)
                                {
                                    string[] tiles = null;
                                    if (roomHasAdjustController(str[1]))
                                    {
                                        tiles = GetHiddenTiles(str[1]).Split(',');
                                        foreach (string tile in tiles)
                                        {
                                            if (!string.IsNullOrEmpty(tile))
                                            {
                                                string[] str2 = tile.Split('-');
                                                int tileX = int.Parse(str2[0]);
                                                int tileY = int.Parse(str2[1]);
                                                if (tileX == TilesPosition[i].X && tileY == TilesPosition[i].Y)
                                                {
                                                    skipTile = true;
                                                }
                                            }
                                        }
                                    }
                                    if (skipTile)
                                    {
                                        continue;
                                    }
                                    if (TilesPosition[i].X < MostLeftTileX)
                                    {
                                        MostLeftTileX = TilesPosition[i].X;
                                    }
                                    if (TilesPosition[i].Y < MostTopTileY)
                                    {
                                        MostTopTileY = TilesPosition[i].Y;
                                    }
                                    if (TilesPosition[i].X + 1 > MostRightTileX)
                                    {
                                        MostRightTileX = TilesPosition[i].X + 1;
                                    }
                                    if (TilesPosition[i].Y + 1 > MostBottomTileY)
                                    {
                                        MostBottomTileY = TilesPosition[i].Y + 1;
                                    }
                                }
                                MostLeftTileX *= ScreenTilesX;
                                MostTopTileY *= ScreenTilesY;
                                MostRightTileX *= ScreenTilesX;
                                MostBottomTileY *= ScreenTilesY;
                                if (roomIsAdjusted(str[1]))
                                {
                                    AdjustX = GetAdjustedPosition(str[1]).X;
                                    AdjustY = GetAdjustedPosition(str[1]).Y;
                                }
                                if (GetRoomPosition(str[1]).X + AdjustX + MostLeftTileX < MostLeftRoomX)
                                {
                                    MostLeftRoomX = GetRoomPosition(str[1]).X + AdjustX + MostLeftTileX;
                                    MostLeftRoom = str[1];
                                }
                                if (GetRoomPosition(str[1]).Y + AdjustY + MostTopTileY < MostTopRoomY)
                                {
                                    MostTopRoomY = GetRoomPosition(str[1]).Y + AdjustY + MostTopTileY;
                                    MostTopRoom = str[1];
                                }
                                if (GetRoomPosition(str[1]).X + AdjustX + MostRightTileX > MostRightRoomX)
                                {
                                    MostRightRoomX = GetRoomPosition(str[1]).X + AdjustX + MostRightTileX;
                                }
                                if (GetRoomPosition(str[1]).Y + AdjustY + MostBottomTileY > MostBottomRoomY)
                                {
                                    MostBottomRoomY = GetRoomPosition(str[1]).Y + AdjustY + MostBottomTileY;
                                }
                            }
                            else
                            {
                                if (roomIsAdjusted(str[1]))
                                {
                                    AdjustX = GetAdjustedPosition(str[1]).X;
                                    AdjustY = GetAdjustedPosition(str[1]).Y;
                                }
                                if (GetRoomPosition(str[1]).X + AdjustX < MostLeftRoomX)
                                {
                                    MostLeftRoomX = GetRoomPosition(str[1]).X + AdjustX;
                                    MostLeftRoom = str[1];
                                }
                                if (GetRoomPosition(str[1]).Y + AdjustY < MostTopRoomY)
                                {
                                    MostTopRoomY = GetRoomPosition(str[1]).Y + AdjustY;
                                    MostTopRoom = str[1];
                                }
                                MostRightRoomSize = GetRoomSize(str[1]);
                                if (GetRoomPosition(str[1]).X + AdjustX + MostRightRoomSize.X * 8 > MostRightRoomX)
                                {
                                    MostRightRoomX = GetRoomPosition(str[1]).X + AdjustX + MostRightRoomSize.X * 8;
                                }
                                MostBottomRoomSize = GetRoomSize(str[1]);
                                if (GetRoomPosition(str[1]).Y + AdjustY + (MostBottomRoomSize.Y / 40 * (ScreenTilesY / 8)) * 8 > MostBottomRoomY)
                                {
                                    MostBottomRoomY = GetRoomPosition(str[1]).Y + AdjustY + (MostBottomRoomSize.Y / 40 * (ScreenTilesY / 8)) * 8;
                                }
                            }
                        }
                    }
                }
                BeforeHintsMapWidth = MostRightRoomX - MostLeftRoomX;
                BeforeHintsMapHeight = MostBottomRoomY - MostTopRoomY;
                BeforeHintsMostLeftRoomX = MostLeftRoomX;
                BeforeHintsMostTopRoomY = MostTopRoomY;
                BeforeHintsMostRightRoomX = MostRightRoomX;
                BeforeHintsMostBottomRoomY = MostBottomRoomY;
                if (ShowHints)
                {
                    foreach (InGameMapHintControllerData hint in HintControllerData)
                    {
                        if (hint.ChapterIndex == chapterIndex)
                        {
                            bool AllFlagsTrue = true;
                            foreach (string flag in hint.DisplayFlags)
                            {
                                bool flagCheck = level.Session.GetFlag(flag) ? true : level.Session.GetFlag("Ch" + chapterIndex + "_" + flag);
                                if (!string.IsNullOrEmpty(flag) && !flagCheck)
                                {
                                    AllFlagsTrue = false;
                                    break;
                                }
                            }
                            if (AllFlagsTrue && !level.Session.GetFlag(hint.HideFlag))
                            {
                                float AdjustX = 0;
                                float AdjustY = 0;

                                if (roomIsAdjusted(hint.Room))
                                {
                                    AdjustX = GetAdjustedPosition(hint.Room).X;
                                    AdjustY = GetAdjustedPosition(hint.Room).Y;
                                }

                                if (GetRoomPosition(hint.Room).X + AdjustX + hint.TileCordX * ScreenTilesX < MostLeftRoomX)
                                {
                                    MostLeftRoomX = GetRoomPosition(hint.Room).X + AdjustX + hint.TileCordX * ScreenTilesX;
                                    MostLeftRoom = hint.Room;
                                }
                                if (GetRoomPosition(hint.Room).Y + AdjustY + hint.TileCordY * ScreenTilesY < MostTopRoomY)
                                {
                                    MostTopRoomY = GetRoomPosition(hint.Room).Y + AdjustY + hint.TileCordY * ScreenTilesY;
                                    MostTopRoom = hint.Room;
                                }
                                if (GetRoomPosition(hint.Room).X + AdjustX + (1 + hint.TileCordX) * ScreenTilesX > MostRightRoomX)
                                {
                                    MostRightRoomX = GetRoomPosition(hint.Room).X + AdjustX + (1 + hint.TileCordX) * ScreenTilesX;
                                }
                                if (GetRoomPosition(hint.Room).Y + AdjustY + (1 + hint.TileCordY) * ScreenTilesY > MostBottomRoomY)
                                {
                                    MostBottomRoomY = GetRoomPosition(hint.Room).Y + AdjustY + (1 + hint.TileCordY) * ScreenTilesY;
                                }
                            }
                        }
                    }
                }
                AfterHintsMapWidth = MostRightRoomX - MostLeftRoomX;
                AfterHintsMapHeight = MostBottomRoomY - MostTopRoomY;
                AfterHintsMostLeftRoomX = MostLeftRoomX;
                AfterHintsMostTopRoomY = MostTopRoomY;
                AfterHintsMostRightRoomX = MostRightRoomX;
                AfterHintsMostBottomRoomY = MostBottomRoomY;
            }
            MapWidth = MostRightRoomX - MostLeftRoomX;
            MapHeight = MostBottomRoomY - MostTopRoomY;
            MapLeft = CalcRoomPosition(GetRoomPosition(MostLeftRoom) + (roomIsAdjusted(MostLeftRoom) ? GetAdjustedPosition(MostLeftRoom) : Vector2.Zero), currentRoomPosition, currentRoomJustify, worldmapPosition).X;
            MapTop = CalcRoomPosition(GetRoomPosition(MostTopRoom) + (roomIsAdjusted(MostTopRoom) ? GetAdjustedPosition(MostTopRoom) : Vector2.Zero), currentRoomPosition, currentRoomJustify, worldmapPosition).Y;
        }

        public void SetCurrentRoomCoordinates(Vector2 offset)
        {
            currentRoomJustify = new Vector2(20, 20);
            float AdjustX = roomIsAdjusted(currentRoom) ? GetAdjustedPosition(currentRoom).X / ScreenTilesX : 0;
            float AdjustY = roomIsAdjusted(currentRoom) ? GetAdjustedPosition(currentRoom).Y / ScreenTilesY : 0;
            currentRoomPosition = new Vector2((GetRoomPosition(currentRoom).X < 0 ? ((float)Math.Round((GetRoomPosition(currentRoom).X) / ScreenTilesX, 0, MidpointRounding.AwayFromZero) + AdjustX) : ((float)Math.Floor((GetRoomPosition(currentRoom).X) / ScreenTilesX) + AdjustX)) * 40, ((float)Math.Round((GetRoomPosition(currentRoom).Y) / ScreenTilesY, 0, MidpointRounding.AwayFromZero) + AdjustY) * 40) + offset;
        }

        public List<int> GetMapShards()
        {
            List<int> mapShards = new();
            foreach (InGameMapRoomControllerData inGameMapRoomControllerData in RoomControllerData)
            {
                if (!mapShards.Contains(inGameMapRoomControllerData.MapShardIndex))
                {
                    mapShards.Add(inGameMapRoomControllerData.MapShardIndex);
                }
            }
            return mapShards;
        }

        public List<int> GetUnlockedMapShards()
        {
            List<int> mapShards = new();
            foreach (string savedFlag in XaphanModule.ModSaveData.SavedFlags)
            {
                if (savedFlag.Contains(Prefix + "_Ch" + chapterIndex + "_MapShard"))
                {
                    mapShards.Add(0);
                }
                if (savedFlag.Contains(Prefix + "_Ch" + chapterIndex + "_MapShard_"))
                {
                    string[] str = savedFlag.Split('_');
                    mapShards.Add(int.Parse(str[3]));
                }
            }
            if (RevealUnexploredRooms())
            {
                foreach (int mapShard in GetMapShards())
                {
                    mapShards.Add(mapShard);
                }
            }
            return mapShards;
        }

        public void GenerateTiles()
        {
            TilesImage.Clear();
            List<InGameMapTilesData> ConnectionTiles = new();
            List<int> mapShards = GetUnlockedMapShards();
            foreach (LevelData level in MapData.Levels)
            {
                getMapColors(level.Name);
                if (XaphanModule.ModSaveData.VisitedRooms.Contains(Prefix + "/Ch" + chapterIndex + "/" + level.Name) || (ExtraUnexploredRooms.Contains("Ch" + chapterIndex + "/" + level.Name)) || ForceRevealUnexploredRooms)
                {
                    List<InGameMapTilesData> ToDelete = new();
                    List<string> TilesTypes = GetTilesType(level.Name);
                    List<Vector2> TilesPosition = GetTilesPosition(level.Name);
                    string[] EntrancesTypes = GetEntrancesType(level.Name);
                    Vector2[] EntrancesPosition = GetEntrancesPosition(level.Name);
                    Color color = Color.Transparent;
                    for (int i = 0; i <= TilesTypes.Count - 1; i++)
                    {
                        if (XaphanModule.ModSaveData.VisitedRoomsTiles.Contains(Prefix + "/Ch" + chapterIndex + "/" + level.Name + "-" + TilesPosition[i].X + "-" + TilesPosition[i].Y) || ForceRevealUnexploredRooms)
                        {
                            if (HeatedRooms.Contains(level.Name))
                            {
                                color = HeatedRoomColor;
                            }
                            else
                            {
                                if (roomIsSecret(level.Name))
                                {
                                    color = SecretRoomColor;
                                }
                                else
                                {
                                    color = ExploredRoomColor;
                                }
                            }
                        }
                        else if (ExtraUnexploredRooms.Contains("Ch" + chapterIndex + "/" + level.Name))
                        {
                            color = UnexploredRoomColor;
                        }
                        else
                        {
                            continue;
                        }
                        if (roomHasAdjustController(level.Name))
                        {
                            bool hideTile = false;
                            string[] tiles = GetHiddenTiles(level.Name).Split(',');
                            foreach (string tile in tiles)
                            {
                                if (!string.IsNullOrEmpty(tile))
                                {
                                    string[] str = tile.Split('-');
                                    int tileX = int.Parse(str[0]);
                                    int tileY = int.Parse(str[1]);
                                    if (tileX == TilesPosition[i].X && tileY == TilesPosition[i].Y)
                                    {
                                        if (color == UnexploredRoomColor)
                                        {
                                            hideTile = true;
                                        }
                                        else
                                        {
                                            color = SecretRoomColor;
                                        }
                                    }
                                }
                            }
                            if (hideTile)
                            {
                                continue;
                            }
                        }
                        if (!string.IsNullOrEmpty(TilesTypes[i]) && TilesTypes[i] != "None")
                        {
                            string BG_Pattern = "Full";
                            Color BG_Color = color;
                            string Elevator_BG = null;
                            Color Elevator_Color = Color.Transparent;
                            string Entrance = null;
                            if (TilesTypes[i] == "UpperLeftSlopeCorner" || TilesTypes[i] == "UpperRightSlopeCorner" || TilesTypes[i] == "LowerLeftSlopeCorner" || TilesTypes[i] == "LowerRightSlopeCorner" || TilesTypes[i] == "UpperLeftSlightSlopeCornerStart" ||
                                TilesTypes[i] == "UpperLeftSlightSlopeCornerEnd" || TilesTypes[i] == "UpperRightSlightSlopeCornerStart" || TilesTypes[i] == "UpperRightSlightSlopeCornerEnd" || TilesTypes[i] == "LowerLeftSlightSlopeCornerStart" || TilesTypes[i] == "LowerLeftSlightSlopeCornerEnd" ||
                                TilesTypes[i] == "LowerRightSlightSlopeCornerStart" || TilesTypes[i] == "LowerRightSlightSlopeCornerEnd" || TilesTypes[i] == "UpperLeftHalfSlopeCorner" || TilesTypes[i] == "UpperRightHalfSlopeCorner" || TilesTypes[i] == "LowerLeftHalfSlopeCorner" ||
                                TilesTypes[i] == "LowerRightHalfSlopeCorner")
                            {
                                BG_Pattern = TilesTypes[i];
                            }
                            else if (TilesTypes[i] == "ElevatorShaft")
                            {
                                BG_Pattern = null;
                                BG_Color = Color.Transparent;
                                Elevator_BG = TilesTypes[i];
                                Elevator_Color = ElevatorColor;
                            }
                            else if (TilesTypes[i] == "ElevatorUpAllWalls" || TilesTypes[i] == "ElevatorDownAllWalls" || TilesTypes[i] == "ElevatorUpSingleHorizontalLeftEdge" || TilesTypes[i] == "ElevatorUpSingleHorizontalRightEdge" || TilesTypes[i] == "ElevatorDownSingleHorizontalLeftEdge" || TilesTypes[i] == "ElevatorDownSingleHorizontalRightEdge")
                            {
                                Elevator_BG = "Elevator";
                                Elevator_Color = ElevatorColor;
                            }
                            else if (TilesTypes[i] == "UpArrow" || TilesTypes[i] == "DownArrow" || TilesTypes[i] == "LeftArrow" || TilesTypes[i] == "RightArrow" || TilesTypes[i] == "HorizontalAreaConnection" || TilesTypes[i] == "VerticalAreaConnection")
                            {
                                BG_Pattern = null;
                                BG_Color = Color.Transparent;
                            }
                            bool N = false;
                            bool S = false;
                            bool E = false;
                            bool W = false;
                            for (int j = 0; j <= EntrancesPosition.GetLength(0) - 1; j++)
                            {
                                if (TilesPosition[i] == EntrancesPosition[j])
                                {
                                    if (EntrancesTypes[j] == "Top")
                                    {
                                        N = true;
                                    }
                                    if (EntrancesTypes[j] == "Bottom")
                                    {
                                        S = true;
                                    }
                                    if (EntrancesTypes[j] == "Right")
                                    {
                                        E = true;
                                    }
                                    if (EntrancesTypes[j] == "Left")
                                    {
                                        W = true;
                                    }
                                }
                            }
                            Entrance = (N ? "N" : "") + (S ? "S" : "") + (E ? "E" : "") + (W ? "W" : "");
                            if (!string.IsNullOrEmpty(Entrance))
                            {
                                Entrance = "-" + Entrance;
                            }
                            foreach (InGameMapTilesData tile in TilesImage)
                            {
                                if (tile.Room == level.Name && tile.Position == Vector2.One + TilesPosition[i] * 40)
                                {
                                    if (tile.BackgroundColor != color)
                                    {
                                        ToDelete.Add(tile);
                                    }
                                }
                            }
                            foreach (InGameMapTilesData tile in ToDelete)
                            {
                                TilesImage.Remove(tile);
                            }
                            if (TilesTypes.Contains("HorizontalAreaConnection") || TilesTypes.Contains("VerticalAreaConnection"))
                            {
                                ConnectionTiles.Add(new InGameMapTilesData(level.Name, TilesTypes[i] == "Middle" ? null : new Image(GFX.Gui["maps/tiles/borders/" + TilesTypes[i] + Entrance]), Vector2.One + TilesPosition[i] * 40, RoomBorderColor, BG_Pattern, BG_Color, Elevator_BG, Elevator_Color));
                            }
                            else if (!TilesImage.Contains(new InGameMapTilesData(level.Name, TilesTypes[i] == "Middle" ? null : new Image(GFX.Gui["maps/tiles/borders/" + TilesTypes[i] + Entrance]), Vector2.One + TilesPosition[i] * 40, RoomBorderColor, BG_Pattern, BG_Color, Elevator_BG, Elevator_Color)))
                            {
                                TilesImage.Add(new InGameMapTilesData(level.Name, TilesTypes[i] == "Middle" ? null : new Image(GFX.Gui["maps/tiles/borders/" + TilesTypes[i] + Entrance]), Vector2.One + TilesPosition[i] * 40, RoomBorderColor, BG_Pattern, BG_Color, Elevator_BG, Elevator_Color));
                            }
                        }
                    }
                }
                foreach (int mapShard in mapShards)
                {
                    if ((UnexploredRooms.Contains("Ch" + chapterIndex + "/" + level.Name + ":" + mapShard) && (MapCollected || RevealUnexploredRooms())))
                    {
                        List<InGameMapTilesData> ToDelete = new();
                        List<string> TilesTypes = GetTilesType(level.Name);
                        List<Vector2> TilesPosition = GetTilesPosition(level.Name);
                        string[] EntrancesTypes = GetEntrancesType(level.Name);
                        Vector2[] EntrancesPosition = GetEntrancesPosition(level.Name);
                        Color color = Color.Transparent;
                        for (int i = 0; i <= TilesTypes.Count - 1; i++)
                        {
                            if (!XaphanModule.ModSaveData.VisitedRoomsTiles.Contains(Prefix + "/Ch" + chapterIndex + "/" + level.Name + "-" + TilesPosition[i].X + "-" + TilesPosition[i].Y))
                            {
                                if (UnexploredRooms.Contains("Ch" + chapterIndex + "/" + level.Name + ":" + mapShard))
                                {
                                    if (MapCollected || RevealUnexploredRooms())
                                    {
                                        color = UnexploredRoomColor;
                                    }
                                    else
                                    {
                                        continue;
                                    }
                                }
                                else
                                {
                                    continue;
                                }
                            }
                            else
                            {
                                continue;
                            }
                            if (roomHasAdjustController(level.Name))
                            {
                                bool hideTile = false;
                                string[] tiles = GetHiddenTiles(level.Name).Split(',');
                                foreach (string tile in tiles)
                                {
                                    if (!string.IsNullOrEmpty(tile))
                                    {
                                        string[] str = tile.Split('-');
                                        int tileX = int.Parse(str[0]);
                                        int tileY = int.Parse(str[1]);
                                        if (tileX == TilesPosition[i].X && tileY == TilesPosition[i].Y)
                                        {
                                            if (color == UnexploredRoomColor)
                                            {
                                                hideTile = true;
                                            }
                                            else
                                            {
                                                color = SecretRoomColor;
                                            }
                                        }
                                    }
                                }
                                if (hideTile)
                                {
                                    continue;
                                }
                            }
                            if (!string.IsNullOrEmpty(TilesTypes[i]) && TilesTypes[i] != "None")
                            {
                                string BG_Pattern = "Full";
                                Color BG_Color = color;
                                string Elevator_BG = null;
                                Color Elevator_Color = Color.Transparent;
                                string Entrance = null;
                                if (TilesTypes[i] == "UpperLeftSlopeCorner" || TilesTypes[i] == "UpperRightSlopeCorner" || TilesTypes[i] == "LowerLeftSlopeCorner" || TilesTypes[i] == "LowerRightSlopeCorner" || TilesTypes[i] == "UpperLeftSlightSlopeCornerStart" ||
                                    TilesTypes[i] == "UpperLeftSlightSlopeCornerEnd" || TilesTypes[i] == "UpperRightSlightSlopeCornerStart" || TilesTypes[i] == "UpperRightSlightSlopeCornerEnd" || TilesTypes[i] == "LowerLeftSlightSlopeCornerStart" || TilesTypes[i] == "LowerLeftSlightSlopeCornerEnd" ||
                                    TilesTypes[i] == "LowerRightSlightSlopeCornerStart" || TilesTypes[i] == "LowerRightSlightSlopeCornerEnd" || TilesTypes[i] == "UpperLeftHalfSlopeCorner" || TilesTypes[i] == "UpperRightHalfSlopeCorner" || TilesTypes[i] == "LowerLeftHalfSlopeCorner" ||
                                    TilesTypes[i] == "LowerRightHalfSlopeCorner")
                                {
                                    BG_Pattern = TilesTypes[i];
                                }
                                else if (TilesTypes[i] == "ElevatorShaft")
                                {
                                    BG_Pattern = null;
                                    BG_Color = Color.Transparent;
                                    Elevator_BG = TilesTypes[i];
                                    Elevator_Color = ElevatorColor;
                                }
                                else if (TilesTypes[i] == "ElevatorUpAllWalls" || TilesTypes[i] == "ElevatorDownAllWalls" || TilesTypes[i] == "ElevatorUpSingleHorizontalLeftEdge" || TilesTypes[i] == "ElevatorUpSingleHorizontalRightEdge" || TilesTypes[i] == "ElevatorDownSingleHorizontalLeftEdge" || TilesTypes[i] == "ElevatorDownSingleHorizontalRightEdge")
                                {
                                    Elevator_BG = "Elevator";
                                    Elevator_Color = ElevatorColor;
                                }
                                else if (TilesTypes[i] == "UpArrow" || TilesTypes[i] == "DownArrow" || TilesTypes[i] == "LeftArrow" || TilesTypes[i] == "RightArrow" || TilesTypes[i] == "HorizontalAreaConnection" || TilesTypes[i] == "VerticalAreaConnection")
                                {
                                    BG_Pattern = null;
                                    BG_Color = Color.Transparent;
                                }
                                bool N = false;
                                bool S = false;
                                bool E = false;
                                bool W = false;
                                for (int j = 0; j <= EntrancesPosition.GetLength(0) - 1; j++)
                                {
                                    if (TilesPosition[i] == EntrancesPosition[j])
                                    {
                                        if (EntrancesTypes[j] == "Top")
                                        {
                                            N = true;
                                        }
                                        if (EntrancesTypes[j] == "Bottom")
                                        {
                                            S = true;
                                        }
                                        if (EntrancesTypes[j] == "Right")
                                        {
                                            E = true;
                                        }
                                        if (EntrancesTypes[j] == "Left")
                                        {
                                            W = true;
                                        }
                                    }
                                }
                                Entrance = (N ? "N" : "") + (S ? "S" : "") + (E ? "E" : "") + (W ? "W" : "");
                                if (!string.IsNullOrEmpty(Entrance))
                                {
                                    Entrance = "-" + Entrance;
                                }
                                foreach (InGameMapTilesData tile in TilesImage)
                                {
                                    if (tile.Room == level.Name && tile.Position == Vector2.One + TilesPosition[i] * 40)
                                    {
                                        if (tile.BackgroundColor != color)
                                        {
                                            ToDelete.Add(tile);
                                        }
                                    }
                                }
                                foreach (InGameMapTilesData tile in ToDelete)
                                {
                                    TilesImage.Remove(tile);
                                }
                                if (TilesTypes.Contains("HorizontalAreaConnection") || TilesTypes.Contains("VerticalAreaConnection"))
                                {
                                    ConnectionTiles.Add(new InGameMapTilesData(level.Name, TilesTypes[i] == "Middle" ? null : new Image(GFX.Gui["maps/tiles/borders/" + TilesTypes[i] + Entrance]), Vector2.One + TilesPosition[i] * 40, RoomBorderColor, BG_Pattern, BG_Color, Elevator_BG, Elevator_Color));
                                }
                                else if (!TilesImage.Contains(new InGameMapTilesData(level.Name, TilesTypes[i] == "Middle" ? null : new Image(GFX.Gui["maps/tiles/borders/" + TilesTypes[i] + Entrance]), Vector2.One + TilesPosition[i] * 40, RoomBorderColor, BG_Pattern, BG_Color, Elevator_BG, Elevator_Color)))
                                {
                                    TilesImage.Add(new InGameMapTilesData(level.Name, TilesTypes[i] == "Middle" ? null : new Image(GFX.Gui["maps/tiles/borders/" + TilesTypes[i] + Entrance]), Vector2.One + TilesPosition[i] * 40, RoomBorderColor, BG_Pattern, BG_Color, Elevator_BG, Elevator_Color));
                                }
                            }
                        }
                    }
                }
            }
            ConnectionTiles.AddRange(TilesImage);
            TilesImage = ConnectionTiles;
        }

        public void GenerateEntrances() // Only if not using a TilesController
        {
            Entrances.Clear();
            foreach (LevelData level in MapData.Levels)
            {
                if (XaphanModule.ModSaveData.VisitedRooms.Contains(Prefix + "/Ch" + chapterIndex + "/" + level.Name) || (UnexploredRooms.Contains("Ch" + chapterIndex + "/" + level.Name) && (MapCollected || RevealUnexploredRooms())))
                {
                    // Generate entrances infos

                    Color color = Color.Transparent;
                    string[] EntrancesTypes = GetEntrancesType(level.Name);
                    Vector2[] EntrancesPosition = GetEntrancesPosition(level.Name);
                    for (int i = 0; i <= EntrancesTypes.GetLength(0) - 1; i++)
                    {
                        if (XaphanModule.ModSaveData.VisitedRooms.Contains(Prefix + "/Ch" + chapterIndex + "/" + level.Name))
                        {
                            if (HeatedRooms.Contains(level.Name))
                            {
                                color = HeatedRoomColor;
                            }
                            else
                            {
                                if (roomIsSecret(level.Name))
                                {
                                    color = SecretRoomColor;
                                }
                                else
                                {
                                    color = ExploredRoomColor;
                                }
                            }
                        }
                        else
                        {
                            if (UnexploredRooms.Contains("Ch" + chapterIndex + "/" + level.Name))
                            {
                                if (MapCollected || RevealUnexploredRooms())
                                {
                                    color = UnexploredRoomColor;
                                }
                                else
                                {
                                    continue;
                                }
                            }
                            else
                            {
                                continue;
                            }
                        }
                        if (roomHasAdjustController(level.Name))
                        {
                            string[] tiles = GetHiddenTiles(level.Name).Split(',');
                            foreach (string tile in tiles)
                            {
                                if (!string.IsNullOrEmpty(tile))
                                {
                                    string[] str = tile.Split('-');
                                    int tileX = int.Parse(str[0]);
                                    int tileY = int.Parse(str[1]);
                                    if (tileX == EntrancesPosition[i].X && tileY == EntrancesPosition[i].Y)
                                    {
                                        if (color != UnexploredRoomColor)
                                        {
                                            color = SecretRoomColor;
                                        }
                                    }
                                }
                            }
                        }
                        if (EntrancesTypes[i] == "Left")
                        {
                            if (!Entrances.Contains(new InGameMapEntrancesData(level.Name, new Vector2(1, 16) + EntrancesPosition[i] * 40, 6, 10, color)))
                            {
                                Entrances.Add(new InGameMapEntrancesData(level.Name, new Vector2(1, 16) + EntrancesPosition[i] * 40, 6, 10, color));
                            }
                        }
                        else if (EntrancesTypes[i] == "Right")
                        {
                            if (!Entrances.Contains(new InGameMapEntrancesData(level.Name, new Vector2(35, 16) + EntrancesPosition[i] * 40, 6, 10, color)))
                            {
                                Entrances.Add(new InGameMapEntrancesData(level.Name, new Vector2(35, 16) + EntrancesPosition[i] * 40, 6, 10, color));
                            }
                        }
                        else if (EntrancesTypes[i] == "Top")
                        {
                            if (!Entrances.Contains(new InGameMapEntrancesData(level.Name, new Vector2(16, 1) + EntrancesPosition[i] * 40, 10, 6, color)))
                            {
                                Entrances.Add(new InGameMapEntrancesData(level.Name, new Vector2(16, 1) + EntrancesPosition[i] * 40, 10, 6, color));
                            }
                        }
                        else if (EntrancesTypes[i] == "Bottom")
                        {
                            if (!Entrances.Contains(new InGameMapEntrancesData(level.Name, new Vector2(16, 35) + EntrancesPosition[i] * 40, 10, 6, color)))
                            {
                                Entrances.Add(new InGameMapEntrancesData(level.Name, new Vector2(16, 35) + EntrancesPosition[i] * 40, 10, 6, color));
                            }
                        }
                    }
                }
            }
        }

        public void GenerateIcons()
        {
            Icons.Clear();
            List<int> mapShards = GetUnlockedMapShards();
            foreach (InGameMapEntitiesData entity in EntitiesData)
            {
                if (XaphanModule.ModSaveData.VisitedRooms.Contains(Prefix + "/Ch" + chapterIndex + "/" + entity.Room) || ForceRevealUnexploredRooms)
                {
                    bool TilesControllerCheck = IsInBounds(entity.LevelData, entity.Position) && (roomUseTilesController(entity.Room) ? XaphanModule.ModSaveData.VisitedRoomsTiles.Contains(Prefix + "/Ch" + chapterIndex + "/" + entity.Room + "-" + entity.MapTilesPosition.X + "-" + entity.MapTilesPosition.Y) : true);
                    if (TilesControllerCheck || (IsInBounds(entity.LevelData, entity.Position) && ForceRevealUnexploredRooms))
                    {
                        if (entity.Type == "strawberry")
                        {
                            Icons.Add(new InGameMapIconsData("strawberry", entity.Room, Vector2.One + entity.MapTilesPosition * 40, SaveData.Instance.CheckStrawberry(entity.StrawberryArea, new EntityID(entity.Room, entity.ID))));
                        }
                        else if (entity.Type == "moonberry")
                        {
                            Icons.Add(new InGameMapIconsData("moonberry", entity.Room, Vector2.One + entity.MapTilesPosition * 40, SaveData.Instance.CheckStrawberry(entity.StrawberryArea, new EntityID(entity.Room, entity.ID))));
                        }
                        else if (entity.Type == "heart")
                        {
                            Icons.Add(new InGameMapIconsData("heart", entity.Room, Vector2.One + entity.MapTilesPosition * 40, HeartCollected));
                        }
                        else if (entity.Type == "map")
                        {
                            Icons.Add(new InGameMapIconsData("map", entity.Room, Vector2.One + entity.MapTilesPosition * 40, XaphanModule.ModSaveData.SavedFlags.Contains(Prefix + "_Ch" + chapterIndex + "_MapShard" + (entity.UpgradeCollectableMapShardIndex == 0 ? "" : "_" + entity.UpgradeCollectableMapShardIndex))));
                        }
                        else if (entity.Type == "cassette")
                        {
                            Icons.Add(new InGameMapIconsData("cassette", entity.Room, Vector2.One + entity.MapTilesPosition * 40, CassetteCollected));
                        }
                        else if (entity.Type == "energyTank")
                        {
                            Icons.Add(new InGameMapIconsData("energyTank", entity.Room, Vector2.One + entity.MapTilesPosition * 40, XaphanModule.ModSaveData.StaminaUpgrades.Contains(Prefix + "_Ch" + chapterIndex + "_" + entity.Room + ":" + entity.ID)));
                        }
                        else if (entity.Type == "missile")
                        {
                            Icons.Add(new InGameMapIconsData("missile", entity.Room, Vector2.One + entity.MapTilesPosition * 40, XaphanModule.ModSaveData.DroneMissilesUpgrades.Contains(Prefix + "_Ch" + chapterIndex + "_" + entity.Room + ":" + entity.ID)));
                        }
                        else if (entity.Type == "superMissile")
                        {
                            Icons.Add(new InGameMapIconsData("superMissile", entity.Room, Vector2.One + entity.MapTilesPosition * 40, XaphanModule.ModSaveData.DroneSuperMissilesUpgrades.Contains(Prefix + "_Ch" + chapterIndex + "_" + entity.Room + ":" + entity.ID)));
                        }
                        else if (entity.Type == "fireRateModule")
                        {
                            Icons.Add(new InGameMapIconsData("fireRateModule", entity.Room, Vector2.One + entity.MapTilesPosition * 40, XaphanModule.ModSaveData.DroneFireRateUpgrades.Contains(Prefix + "_Ch" + chapterIndex + "_" + entity.Room + ":" + entity.ID)));
                        }
                        else if (entity.Type == "warp")
                        {
                            Icons.Add(new InGameMapIconsData("warp", entity.Room, Vector2.One + entity.MapTilesPosition * 40, false));
                        }
                        else if (entity.Type == "upgrade")
                        {
                            Icons.Add(new InGameMapIconsData("upgrade", entity.Room, Vector2.One + entity.MapTilesPosition * 40, XaphanModule.ModSaveData.SavedFlags.Contains(Prefix + "_Upgrade_" + entity.UpgradeCollectableUpgrade)));
                        }
                        else if (entity.Type == "boss")
                        {
                            Icons.Add(new InGameMapIconsData("boss", entity.Room, Vector2.One + entity.MapTilesPosition * 40, BossDefeated));
                        }
                        else if (entity.Type.Contains("bubbleDoor") && GetEntrancesType(entity.Room)[Array.FindIndex(GetEntrancesPosition(entity.Room), x => x == new Vector2((float)Math.Floor(entity.Position.X / ScreenTilesX), (float)Math.Floor(entity.Position.Y / ScreenTilesY)))] != "None" && (ForceRevealUnexploredRooms || XaphanModule.ModSaveData.VisitedRoomsTiles.Contains(Prefix + "/Ch" + chapterIndex + "/" + entity.Room + "-" + (float)Math.Floor(entity.Position.X / ScreenTilesX) + "-" + (float)Math.Floor(entity.Position.Y / ScreenTilesY))))
                        {
                            Icons.Add(new InGameMapIconsData(entity.Type, entity.Room, Vector2.One + entity.MapTilesPosition * 40, false));
                        }
                        else if (entity.Type == "customCollectable")
                        {
                            Icons.Add(new InGameMapIconsData(Prefix + "/" + entity.CustomCollectableMapIcon, entity.Room, Vector2.One + entity.MapTilesPosition * 40, XaphanModule.ModSaveData.SavedFlags.Contains(Prefix + "_Ch" + chapterIndex + "_" + entity.CustomCollectableFlag)));
                        }
                        else if (entity.Type.Contains("collectableDoor"))
                        {
                            Icons.Add(new InGameMapIconsData("collectableDoor/collectableDoorBG" + (entity.CollectableDoorOrientation == "Horizontal" ? "H" : ""), entity.Room, Vector2.One + entity.MapTilesPosition * 40, level.Session.GetFlag("XaphanHelper_Opened_Collectable_Door_" + entity.CollectableDoorFlagID), entity.CollectableDoorInteriorColor));
                            Icons.Add(new InGameMapIconsData("collectableDoor/collectableDoorBorder" + (entity.CollectableDoorOrientation == "Horizontal" ? "H" : ""), entity.Room, Vector2.One + entity.MapTilesPosition * 40, level.Session.GetFlag("XaphanHelper_Opened_Collectable_Door_" + entity.CollectableDoorFlagID), entity.CollectableDoorEdgesColor));
                            Vector2 iconPosition = new(10f, (entity.Type == "collectableDoor/goldenStrawberry" || level.Session.GetFlag("XaphanHelper_Opened_Collectable_Door_" + entity.CollectableDoorFlagID)) ? 10f : 4f);
                            if (!string.IsNullOrEmpty(entity.CollectableDoorMapIcon))
                            {
                                Icons.Add(new InGameMapIconsData(Prefix + "/" + entity.CollectableDoorMapIcon, entity.Room, Vector2.One + entity.MapTilesPosition * 40 + iconPosition, false));
                            }
                            else
                            {
                                Icons.Add(new InGameMapIconsData(entity.Type, entity.Room, Vector2.One + entity.MapTilesPosition * 40 + iconPosition, false));
                            }
                            if (entity.Type != "collectableDoor/goldenStrawberry" && !level.Session.GetFlag("XaphanHelper_Opened_Collectable_Door_" + entity.CollectableDoorFlagID))
                            {
                                string requiresStr = entity.CollectableDoorRequires.ToString();
                                int requiresStrTotalChars = requiresStr.Length;
                                int padding = requiresStrTotalChars * 5;
                                for (int i = 0; i < requiresStr.Length; i++)
                                {
                                    Icons.Add(new InGameMapIconsData("collectableDoor/" + requiresStr[i], entity.Room, Vector2.One + entity.MapTilesPosition * 40 + new Vector2(20f - padding, 24f), false));
                                    padding -= 10;
                                }
                            }
                        }
                        else if (entity.Type.Contains("pipeGate"))
                        {
                            Icons.Add(new InGameMapIconsData("pipeGate" + entity.Type.Remove(0, 8), entity.Room, Vector2.One + entity.MapTilesPosition * 40, false));
                        }
                        else if (entity.Type.Contains("bigScreen"))
                        {
                            Icons.Add(new InGameMapIconsData("bigScreen", entity.Room, Vector2.One + entity.MapTilesPosition * 40, false));
                        }
                    }
                }
                if (!InGameMapControllerData.HideIconsInUnexploredRooms)
                {
                    foreach (int mapShard in mapShards)
                    {
                        if ((UnexploredRooms.Contains("Ch" + chapterIndex + "/" + entity.Room + ":" + mapShard) && (MapCollected || RevealUnexploredRooms())))
                        {
                            bool TilesControllerCheck = IsInBounds(entity.LevelData, entity.Position) && (roomUseTilesController(entity.Room) ? (roomIsUnexplored(entity.Room) && (MapCollected || RevealUnexploredRooms())) : true);
                            if (TilesControllerCheck)
                            {
                                if (entity.Type == "strawberry")
                                {
                                    Icons.Add(new InGameMapIconsData("strawberry", entity.Room, Vector2.One + entity.MapTilesPosition * 40, SaveData.Instance.CheckStrawberry(entity.StrawberryArea, new EntityID(entity.Room, entity.ID))));
                                }
                                else if (entity.Type == "heart")
                                {
                                    Icons.Add(new InGameMapIconsData("heart", entity.Room, Vector2.One + entity.MapTilesPosition * 40, HeartCollected));
                                }
                                else if (entity.Type == "map")
                                {
                                    Icons.Add(new InGameMapIconsData("map", entity.Room, Vector2.One + entity.MapTilesPosition * 40, XaphanModule.ModSaveData.SavedFlags.Contains(Prefix + "_Ch" + chapterIndex + "_MapShard" + (entity.UpgradeCollectableMapShardIndex == 0 ? "" : "_" + entity.UpgradeCollectableMapShardIndex))));
                                }
                                else if (entity.Type == "cassette")
                                {
                                    Icons.Add(new InGameMapIconsData("cassette", entity.Room, Vector2.One + entity.MapTilesPosition * 40, CassetteCollected));
                                }
                                else if (entity.Type == "energyTank")
                                {
                                    Icons.Add(new InGameMapIconsData("energyTank", entity.Room, Vector2.One + entity.MapTilesPosition * 40, XaphanModule.ModSaveData.StaminaUpgrades.Contains(Prefix + "_Ch" + chapterIndex + "_" + entity.Room + ":" + entity.ID)));
                                }
                                else if (entity.Type == "missile")
                                {
                                    Icons.Add(new InGameMapIconsData("missile", entity.Room, Vector2.One + entity.MapTilesPosition * 40, XaphanModule.ModSaveData.DroneMissilesUpgrades.Contains(Prefix + "_Ch" + chapterIndex + "_" + entity.Room + ":" + entity.ID)));
                                }
                                else if (entity.Type == "superMissile")
                                {
                                    Icons.Add(new InGameMapIconsData("superMissile", entity.Room, Vector2.One + entity.MapTilesPosition * 40, XaphanModule.ModSaveData.DroneSuperMissilesUpgrades.Contains(Prefix + "_Ch" + chapterIndex + "_" + entity.Room + ":" + entity.ID)));
                                }
                                else if (entity.Type == "fireRateModule")
                                {
                                    Icons.Add(new InGameMapIconsData("fireRateModule", entity.Room, Vector2.One + entity.MapTilesPosition * 40, XaphanModule.ModSaveData.DroneFireRateUpgrades.Contains(Prefix + "_Ch" + chapterIndex + "_" + entity.Room + ":" + entity.ID)));
                                }
                                else if (entity.Type == "warp")
                                {
                                    Icons.Add(new InGameMapIconsData("warp", entity.Room, Vector2.One + entity.MapTilesPosition * 40, false));
                                }
                                else if (entity.Type == "upgrade")
                                {
                                    Icons.Add(new InGameMapIconsData("upgrade", entity.Room, Vector2.One + entity.MapTilesPosition * 40, XaphanModule.ModSaveData.SavedFlags.Contains(Prefix + "_Upgrade_" + entity.UpgradeCollectableUpgrade)));
                                }
                                else if (entity.Type == "boss")
                                {
                                    Icons.Add(new InGameMapIconsData("boss", entity.Room, Vector2.One + entity.MapTilesPosition * 40, BossDefeated));
                                }
                                else if (entity.Type.Contains("bubbleDoor") && GetEntrancesType(entity.Room)[Array.FindIndex(GetEntrancesPosition(entity.Room), x => x == new Vector2((float)Math.Floor(entity.Position.X / ScreenTilesX), (float)Math.Floor(entity.Position.Y / ScreenTilesY)))] != "None" && XaphanModule.ModSaveData.VisitedRoomsTiles.Contains(Prefix + "/Ch" + chapterIndex + "/" + entity.Room + "-" + (float)Math.Floor(entity.Position.X / ScreenTilesX) + "-" + (float)Math.Floor(entity.Position.Y / ScreenTilesY)))
                                {
                                    Icons.Add(new InGameMapIconsData(entity.Type, entity.Room, Vector2.One + entity.MapTilesPosition * 40, false));
                                }
                                else if (entity.Type == "customCollectable")
                                {
                                    Icons.Add(new InGameMapIconsData(Prefix + "/" + entity.CustomCollectableMapIcon, entity.Room, Vector2.One + entity.MapTilesPosition * 40, XaphanModule.ModSaveData.SavedFlags.Contains(Prefix + "_Ch" + chapterIndex + "_" + entity.CustomCollectableFlag)));
                                }
                                else if (entity.Type.Contains("collectableDoor"))
                                {
                                    Icons.Add(new InGameMapIconsData("collectableDoor/collectableDoorBG" + (entity.CollectableDoorOrientation == "Horizontal" ? "H" : ""), entity.Room, Vector2.One + entity.MapTilesPosition * 40, level.Session.GetFlag("XaphanHelper_Opened_Collectable_Door_" + entity.CollectableDoorFlagID), color: entity.CollectableDoorInteriorColor));
                                    Icons.Add(new InGameMapIconsData("collectableDoor/collectableDoorBorder" + (entity.CollectableDoorOrientation == "Horizontal" ? "H" : ""), entity.Room, Vector2.One + entity.MapTilesPosition * 40, level.Session.GetFlag("XaphanHelper_Opened_Collectable_Door_" + entity.CollectableDoorFlagID), color: entity.CollectableDoorEdgesColor));
                                    if (!string.IsNullOrEmpty(entity.CollectableDoorMapIcon))
                                    {
                                        Icons.Add(new InGameMapIconsData(Prefix + "/" + entity.CollectableDoorMapIcon, entity.Room, Vector2.One + entity.MapTilesPosition * 40, false));
                                    }
                                    else
                                    {
                                        Icons.Add(new InGameMapIconsData(entity.Type, entity.Room, Vector2.One + entity.MapTilesPosition * 40, false));
                                    }
                                }
                                else if (entity.Type.Contains("pipeGate"))
                                {
                                    Icons.Add(new InGameMapIconsData("pipeGate" + entity.Type.Remove(0, 8), entity.Room, Vector2.One + entity.MapTilesPosition * 40, false));
                                }
                                else if (entity.Type.Contains("bigScreen"))
                                {
                                    Icons.Add(new InGameMapIconsData("bigScreen", entity.Room, Vector2.One + entity.MapTilesPosition * 40, false));
                                }
                            }
                        }
                    }
                }
            }
            if (mode == "map")
            {
                if (playerPosition.Y == -1)
                {
                    playerPosition.Y = 0;
                }
                string playerIcon = !XaphanModule.useMetroidGameplay ? "player" : "samus";
                if (GravityJacket.Active(level))
                {
                    playerIcon += "_gravity";
                }
                else if (VariaJacket.Active(level))
                {
                    playerIcon += "_varia";
                }
                Icons.Add(new InGameMapIconsData(playerIcon, currentRoom, Vector2.One + playerPosition * 40, false));
                if (!XaphanModule.useMetroidGameplay)
                {
                    Icons.Add(new InGameMapIconsData("player_hair", currentRoom, Vector2.One + playerPosition * 40, false));
                }
            }
        }

        public IEnumerator GenerateMap()
        {
            if (mode == "map")
            {
                GetMapSize();
                SetCurrentMapCoordinates();
            }
            if (mode == "minimap")
            {
                SetCurrentRoomCoordinates(Vector2.Zero);
            }
            GenerateTiles();
            GenerateEntrances();
            GenerateIcons();
            yield return null;
            Display = true;
        }

        public bool IsInBounds(LevelData level, Vector2 position)
        {
            return level.Bounds.Contains((int)position.X + level.Bounds.Left, (int)position.Y + level.Bounds.Top);
        }

        public void GetChapterUnexploredRooms()
        {
            foreach (InGameMapRoomControllerData roomControllerData in RoomControllerData)
            {
                if (roomControllerData.ShowUnexplored)
                {
                    UnexploredRooms.Add("Ch" + chapterIndex + "/" + roomControllerData.Room + ":" + roomControllerData.MapShardIndex);
                }
            }
            foreach (string room in XaphanModule.ModSaveData.ExtraUnexploredRooms)
            {
                if (room.Contains(Prefix + "/Ch" + chapterIndex + "/"))
                {
                    string[] str = room.Split('/');
                    ExtraUnexploredRooms.Add("Ch" + chapterIndex + "/" + str[3]);
                }
            }
        }

        public bool RevealUnexploredRooms()
        {
            return InGameMapControllerData.RevealUnexploredRooms || ForceRevealUnexploredRooms;
        }

        public Vector2 GetRoomSize(string room)
        {
            Vector2 roomSize = Vector2.Zero;
            foreach (LevelData level in MapData.Levels)
            {
                if (level.Name == room)
                {
                    roomSize = new Vector2(level.Bounds.Width / (ScreenTilesX / 8) * 40 / 8, (level.Bounds.Height == 180 ? ScreenTilesY : level.Bounds.Height) / (ScreenTilesY / 8) * 40 / 8);
                    break;
                }
            }
            return roomSize;
        }

        public Vector2 ConvertRoomSizeToMapSize(Vector2 roomSize)
        {
            return new Vector2((float)Math.Round(roomSize.X / 40, MidpointRounding.AwayFromZero), (float)Math.Round(roomSize.Y / 40, MidpointRounding.AwayFromZero)) * 40;
        }

        public Vector2 GetRoomPosition(string room)
        {
            Vector2 roomPosition = Vector2.Zero;
            foreach (LevelData level in MapData.Levels)
            {
                if (level.Name == room)
                {
                    roomPosition = new Vector2(level.Bounds.X, level.Bounds.Y);
                    break;
                }
            }
            return roomPosition;
        }

        public void adjustMapPosition(Vector2 Offset)
        {
            MapPosition += Offset;
        }

        public void setMapPosition(Vector2 Offset)
        {
            MapPosition = new Vector2(Grid.Width / 2 + Grid.X, Grid.Height / 2 + Grid.Y) + Offset;
        }

        public void getMapColors(string room)
        {
            int SubAreaIndex = 0;
            foreach (InGameMapRoomControllerData roomControllerData in RoomControllerData)
            {
                if (roomControllerData.Room == room)
                {
                    SubAreaIndex = roomControllerData.SubAreaIndex;
                    break;
                }
            }
            foreach (InGameMapSubAreaControllerData subAreaControllerData in SubAreaControllerData)
            {
                if (subAreaControllerData.SubAreaIndex == SubAreaIndex)
                {
                    ExploredRoomColor = Calc.HexToColor(subAreaControllerData.ExploredRoomColor);
                    UnexploredRoomColor = Calc.HexToColor(subAreaControllerData.UnexploredRoomColor);
                    SecretRoomColor = Calc.HexToColor(subAreaControllerData.SecretRoomColor);
                    HeatedRoomColor = Calc.HexToColor(subAreaControllerData.HeatedRoomColor);
                    RoomBorderColor = Calc.HexToColor(subAreaControllerData.RoomBorderColor);
                    ElevatorColor = Calc.HexToColor(subAreaControllerData.ElevatorColor);
                    return;
                }
            }
            ExploredRoomColor = Calc.HexToColor(InGameMapControllerData.ExploredRoomColor);
            UnexploredRoomColor = Calc.HexToColor(InGameMapControllerData.UnexploredRoomColor);
            SecretRoomColor = Calc.HexToColor(InGameMapControllerData.SecretRoomColor);
            HeatedRoomColor = Calc.HexToColor(InGameMapControllerData.HeatedRoomColor);
            RoomBorderColor = Calc.HexToColor(InGameMapControllerData.RoomBorderColor);
            ElevatorColor = Calc.HexToColor(InGameMapControllerData.ElevatorColor);
        }

        public bool roomIsSecret(string room)
        {
            foreach (InGameMapRoomControllerData roomControllerData in RoomControllerData)
            {
                if (roomControllerData.Room == room)
                {
                    return roomControllerData.Secret;
                }
            }
            return false;
        }

        public bool roomIsUnexplored(string room)
        {
            foreach (InGameMapRoomControllerData roomControllerData in RoomControllerData)
            {
                if (roomControllerData.Room == room)
                {
                    return roomControllerData.ShowUnexplored;
                }
            }
            return false;
        }

        public Dictionary<string, int> GetRoomEntrances(string room)
        {
            Dictionary<string, int> Entrances = new();
            foreach (InGameMapRoomControllerData roomControllerData in RoomControllerData)
            {
                if (roomControllerData.Room == room)
                {
                    bool removeEntrance0 = false;
                    bool removeEntrance1 = false;
                    bool removeEntrance2 = false;
                    bool removeEntrance3 = false;
                    bool removeEntrance4 = false;
                    bool removeEntrance5 = false;
                    bool removeEntrance6 = false;
                    bool removeEntrance7 = false;
                    bool removeEntrance8 = false;
                    bool removeEntrance9 = false;
                    if (roomIsAdjusted(room))
                    {
                        foreach (InGameMapRoomAdjustControllerData roomAdjustControllerData in RoomAdjustControllerData)
                        {
                            if (roomAdjustControllerData.Room == room)
                            {
                                removeEntrance0 = roomAdjustControllerData.RemoveEntrance0;
                                removeEntrance1 = roomAdjustControllerData.RemoveEntrance1;
                                removeEntrance2 = roomAdjustControllerData.RemoveEntrance2;
                                removeEntrance3 = roomAdjustControllerData.RemoveEntrance3;
                                removeEntrance4 = roomAdjustControllerData.RemoveEntrance4;
                                removeEntrance5 = roomAdjustControllerData.RemoveEntrance5;
                                removeEntrance6 = roomAdjustControllerData.RemoveEntrance6;
                                removeEntrance7 = roomAdjustControllerData.RemoveEntrance7;
                                removeEntrance8 = roomAdjustControllerData.RemoveEntrance8;
                                removeEntrance9 = roomAdjustControllerData.RemoveEntrance9;
                                break;
                            }
                        }
                    }
                    if (!removeEntrance0)
                    {
                        Entrances.Add("0-" + roomControllerData.Entrance0Position, roomControllerData.Entrance0Offset);
                    }
                    if (!removeEntrance1)
                    {
                        Entrances.Add("1-" + roomControllerData.Entrance1Position, roomControllerData.Entrance1Offset);
                    }
                    if (!removeEntrance2)
                    {
                        Entrances.Add("2-" + roomControllerData.Entrance2Position, roomControllerData.Entrance2Offset);
                    }
                    if (!removeEntrance3)
                    {
                        Entrances.Add("3-" + roomControllerData.Entrance3Position, roomControllerData.Entrance3Offset);
                    }
                    if (!removeEntrance4)
                    {
                        Entrances.Add("4-" + roomControllerData.Entrance4Position, roomControllerData.Entrance4Offset);
                    }
                    if (!removeEntrance5)
                    {
                        Entrances.Add("5-" + roomControllerData.Entrance5Position, roomControllerData.Entrance5Offset);
                    }
                    if (!removeEntrance6)
                    {
                        Entrances.Add("6-" + roomControllerData.Entrance6Position, roomControllerData.Entrance6Offset);
                    }
                    if (!removeEntrance7)
                    {
                        Entrances.Add("7-" + roomControllerData.Entrance7Position, roomControllerData.Entrance7Offset);
                    }
                    if (!removeEntrance8)
                    {
                        Entrances.Add("8-" + roomControllerData.Entrance8Position, roomControllerData.Entrance8Offset);
                    }
                    if (!removeEntrance9)
                    {
                        Entrances.Add("9-" + roomControllerData.Entrance9Position, roomControllerData.Entrance9Offset);
                    }
                }
            }
            return Entrances;
        }

        public Vector2[] GetEntrancesPosition(string room)
        {
            Vector2[] entrancePosition = new Vector2[10];
            foreach (InGameMapRoomControllerData roomControllerData in RoomControllerData)
            {
                if (roomControllerData.Room == room)
                {
                    for (int i = 0; i <= 9; i++)
                    {
                        string tileCords = roomControllerData.GetEntranceCordsField(i);
                        if (!string.IsNullOrEmpty(tileCords)) // Map use current version of InGameMapRoomController
                        {
                            string[] str = tileCords.Split('-');
                            entrancePosition[i] = new Vector2(int.Parse(str[0]), int.Parse(str[1]));
                        }
                        else // Map use old version of InGameMapRoomController
                        {
                            int oldTileCord = roomControllerData.GetEntranceOffsetField(i);
                            string[] oldEntranceTypes = GetEntrancesType(room);
                            Vector2 roomSize = ConvertRoomSizeToMapSize(GetRoomSize(room));
                            if (oldEntranceTypes[i] == "Left")
                            {
                                entrancePosition[i] = new Vector2(0, oldTileCord);
                            }
                            else if (oldEntranceTypes[i] == "Top")
                            {
                                entrancePosition[i] = new Vector2(oldTileCord, 0);
                            }
                            else
                            {
                                if (oldEntranceTypes[i] == "Right")
                                {
                                    entrancePosition[i] = new Vector2(roomSize.X / 40 - 1, oldTileCord);
                                }
                                else if (oldEntranceTypes[i] == "Bottom")
                                {
                                    entrancePosition[i] = new Vector2(oldTileCord, roomSize.Y / 40 - 1);
                                }
                            }
                        }
                    }
                    break;
                }
            }
            return entrancePosition;
        }

        public string[] GetEntrancesType(string room)
        {
            string[] entranceType = new string[10];
            foreach (InGameMapRoomControllerData roomControllerData in RoomControllerData)
            {
                if (roomControllerData.Room == room)
                {
                    if (roomIsAdjusted(room))
                    {
                        foreach (InGameMapRoomAdjustControllerData roomAdjustControllerData in RoomAdjustControllerData)
                        {
                            if (roomAdjustControllerData.Room == room)
                            {
                                for (int i = 0; i <= 9; i++)
                                {
                                    if (roomAdjustControllerData.GetRemoveEntranceField(i))
                                    {
                                        entranceType[i] = "None";
                                    }
                                    else
                                    {
                                        entranceType[i] = roomControllerData.GetEntrancePositionField(i);
                                    }
                                }
                                break;
                            }
                        }
                    }
                    else
                    {
                        for (int i = 0; i <= 9; i++)
                        {
                            entranceType[i] = roomControllerData.GetEntrancePositionField(i);
                        }
                    }
                    break;
                }
            }
            return entranceType;
        }

        public bool roomHasAdjustController(string room)
        {
            foreach (InGameMapRoomAdjustControllerData roomAdjustControllerData in RoomAdjustControllerData)
            {
                if (roomAdjustControllerData.Room == room)
                {
                    return true;
                }
            }
            return false;
        }

        public bool roomIsAdjusted(string room)
        {
            if (roomHasAdjustController(room))
            {
                if (level.Session.GetFlag("Ignore_Room_Adjust_Ch" + chapterIndex + "_" + room) || XaphanModule.ModSaveData.SavedFlags.Contains(Prefix + "_Ignore_Room_Adjust_Ch" + chapterIndex + "_" + room))
                {
                    return false;
                }
                return true;
            }
            return false;
        }

        public Vector2 GetAdjustedPosition(string room)
        {
            Vector2 adjustedPosition = Vector2.Zero;
            foreach (InGameMapRoomAdjustControllerData roomAdjustControllerData in RoomAdjustControllerData)
            {
                if (roomAdjustControllerData.Room == room)
                {
                    adjustedPosition = new Vector2(roomAdjustControllerData.PositionX * ScreenTilesX, roomAdjustControllerData.PositionY * ScreenTilesY);
                }
            }
            return adjustedPosition;
        }

        public Vector2 GetAdjustedSize(string room)
        {
            Vector2 adjustedSize = Vector2.Zero;
            foreach (InGameMapRoomAdjustControllerData roomAdjustControllerData in RoomAdjustControllerData)
            {
                if (roomAdjustControllerData.Room == room)
                {
                    adjustedSize = new Vector2(roomAdjustControllerData.SizeX * 40, roomAdjustControllerData.SizeY * 40);
                }
            }
            return adjustedSize;
        }

        public bool ignoreAdjustIcons(string room)
        {
            foreach (InGameMapRoomAdjustControllerData roomAdjustControllerData in RoomAdjustControllerData)
            {
                if (roomAdjustControllerData.Room == room)
                {
                    return roomAdjustControllerData.IgonreIcons;
                }
            }
            return false;
        }

        public string GetHiddenTiles(string room)
        {
            string hiddenTiles = null;
            foreach (InGameMapRoomAdjustControllerData roomAdjustControllerData in RoomAdjustControllerData)
            {
                if (roomAdjustControllerData.Room == room)
                {
                    hiddenTiles = roomAdjustControllerData.HiddenTiles;
                }
            }
            return hiddenTiles;
        }

        public bool roomUseTilesController(string room)
        {
            foreach (InGameMapTilesControllerData tilesControllerData in TilesControllerData)
            {
                if (tilesControllerData.Room == room)
                {
                    return true;
                }
            }
            return false;
        }

        public List<Vector2> GetTilesPosition(string room)
        {
            List<Vector2> tilePosition = new();
            int totalEntities = 0;
            foreach (InGameMapTilesControllerData tilesControllerData in TilesControllerData)
            {
                if (tilesControllerData.Room == room && (NoGrid ? tilesControllerData.Display != "Area map only" : tilesControllerData.Display != "World map only"))
                {
                    for (int i = 0; i <= 9; i++)
                    {
                        string tileCords = tilesControllerData.GetTileCords(i);
                        string[] str = tileCords.Split('-');
                        tilePosition.Add(new Vector2(int.Parse(str[0]), int.Parse(str[1])));
                    }
                    totalEntities++;
                }
            }
            return tilePosition;
        }

        public List<string> GetTilesType(string room)
        {
            List<string> tileType = new();
            int totalEntities = 0;
            foreach (InGameMapTilesControllerData tilesControllerData in TilesControllerData)
            {
                if (tilesControllerData.Room == room && (NoGrid ? tilesControllerData.Display != "Area map only" : tilesControllerData.Display != "World map only"))
                {
                    for (int i = 0; i <= 9; i++)
                    {
                        tileType.Add(tilesControllerData.GetTile(i));
                    }
                    totalEntities++;
                }
            }
            return tileType;
        }

        public bool isNotVisibleOnScreen(Vector2 RoomPosition, Vector2 ObjectPosition)
        {
            if (!Grid.Contains((int)RoomPosition.X + (int)ObjectPosition.X, (int)RoomPosition.Y + (int)ObjectPosition.Y))
            {
                return true;
            }
            return false;
        }

        public override void Added(Scene scene)
        {
            base.Added(scene);
            if (!HideIndicator)
            {
                if (mode == "map")
                {
                    Player player = level.Tracker.GetEntity<Player>();
                    playerPosition = new Vector2(Math.Min((float)Math.Floor((player.Center.X - level.Bounds.X) / ScreenTilesX), (float)Math.Round(level.Bounds.Width / (float)ScreenTilesX, MidpointRounding.AwayFromZero) - 1), Math.Min((float)Math.Floor((player.Center.Y - level.Bounds.Y) / ScreenTilesY), (float)Math.Round(level.Bounds.Height / (float)ScreenTilesY, MidpointRounding.AwayFromZero) + 1));
                    GetMapSize();
                    SetCurrentMapCoordinates();
                }
                if (mode == "minimap")
                {
                    SetCurrentRoomCoordinates(Vector2.Zero);
                }
            }
            if (!XaphanModule.ModSaveData.ShowHints.ContainsKey(Prefix))
            {
                XaphanModule.ModSaveData.ShowHints[Prefix] = false;
            }
            useHintsCheck(level);
            if (useHints && XaphanModule.ModSaveData.ShowHints[Prefix])
            {
                ShowHints = true;
            }
            if (mode == "warp")
            {
                Player player = level.Tracker.GetEntity<Player>();
                playerPosition = new Vector2(Math.Min((float)Math.Floor((player.Center.X - level.Bounds.X) / ScreenTilesX), (float)Math.Round(level.Bounds.Width / (float)ScreenTilesX, MidpointRounding.AwayFromZero) - 1), Math.Min((float)Math.Floor((player.Center.Y - level.Bounds.Y) / ScreenTilesY), (float)Math.Round(level.Bounds.Height / (float)ScreenTilesY, MidpointRounding.AwayFromZero) + 1));
            }
        }

        public override void Update()
        {
            base.Update();
            if (mode != "minimap")
            {
                Opacity = 1;
            }
            else
            {
                if (useHints && XaphanModule.ModSaveData.ShowHints[Prefix])
                {
                    ShowHints = true;
                }
                else
                {
                    ShowHints = false;
                }
            }
            Player player = level.Tracker.GetEntity<Player>();
            if (player != null)
            {
                if (mode == "minimap")
                {
                    AreaKey area = level.Session.Area;
                    MapData = AreaData.Areas[area.ID].Mode[(int)area.Mode].MapData;
                    if (!MapCollected)
                    {
                        MapCollected = XaphanModule.ModSaveData.SavedFlags.Contains(Prefix + "_Ch" + chapterIndex + "_MapShard");
                    }
                    if (!HeartCollected)
                    {
                        HeartCollected = SaveData.Instance.Areas_Safe[area.ID].Modes[(int)area.Mode].HeartGem;
                    }
                    if (!CassetteCollected)
                    {
                        CassetteCollected = SaveData.Instance.Areas_Safe[area.ID].Cassette;
                    }
                    if (!BossDefeated)
                    {
                        BossDefeated = XaphanModule.ModSaveData.SavedFlags.Contains(Prefix + "_Ch" + chapterIndex + "_Boss_Defeated");
                    }
                    if (!BossDefeatedCM)
                    {
                        BossDefeatedCM = XaphanModule.ModSaveData.SavedFlags.Contains(Prefix + "_Ch" + chapterIndex + "_Boss_Defeated_CM");
                    }
                    if ((MapCollected || RevealUnexploredRooms()) && !UnexploredRoomsRevealed)
                    {
                        GetChapterUnexploredRooms();
                        GenerateTiles();
                        GenerateEntrances();
                        UnexploredRoomsRevealed = true;
                    }
                }
                playerPosition = new Vector2(Math.Min((float)Math.Floor((player.Center.X - level.Bounds.X) / ScreenTilesX), (float)Math.Round(level.Bounds.Width / (float)ScreenTilesX, MidpointRounding.AwayFromZero) - 1), Math.Min((float)Math.Floor((player.Center.Y - level.Bounds.Y) / ScreenTilesY), (float)Math.Round(level.Bounds.Height / (float)ScreenTilesY, MidpointRounding.AwayFromZero) + 1));
                if (playerPosition.Y == -1)
                {
                    playerPosition.Y = 0;
                }
                if (currentRoom == level.Session.Level)
                {
                    if (mode == "minimap")
                    {
                        setMapPosition(playerPosition * -40);
                    }
                }
            }
            if (Engine.Scene.OnRawInterval(0.3f))
            {
                currentRoomIndicator = !currentRoomIndicator;
            }
        }

        public override void Render()
        {
            base.Render();
            // Draw Grid

            if (!NoGrid)
            {
                Draw.Rect(Grid.X, Grid.Y, Grid.Width, Grid.Height, Color.Black * 0.85f * Opacity);
                for (int i = 0; i <= Grid.Height / 40 - 1; i++)
                {
                    for (int j = 0; j <= Grid.Width / 40 * 5 - 1; j++)
                    {
                        Draw.Rect(new Vector2(Grid.X + j * 8, Grid.Y + i * 40), 4, 4, GridColor * 0.5f * Opacity);
                    }
                    for (int j = 0; j <= 3; j++)
                    {
                        for (int k = 0; k <= Grid.Width / 40 - 1; k++)
                        {
                            Draw.Rect(new Vector2(Grid.X + k * 40, Grid.Y + 8 + j * 8 + i * 40), 4, 4, GridColor * 0.5f * Opacity);
                        }
                    }
                }
            }

            if (Display)
            {

                // Draw content

                // If room is not using a TilesController

                // Unexplored rooms

                if (MapCollected || RevealUnexploredRooms())
                {
                    foreach (string unexploredRoom in UnexploredRooms)
                    {
                        string[] baseStr = unexploredRoom.Split(':');
                        string[] str = baseStr[0].Split('/');
                        if (str[0] == "Ch" + chapterIndex)
                        {
                            if (!roomUseTilesController(str[1]))
                            {
                                Vector2 unexploredRoomPosition = CalcRoomPosition(GetRoomPosition(str[1]) + (roomIsAdjusted(str[1]) ? GetAdjustedPosition(str[1]) : Vector2.Zero), currentRoomPosition, currentRoomJustify, worldmapPosition);
                                Vector2 unexploredRoomSize = ConvertRoomSizeToMapSize(GetRoomSize(str[1]) + (roomIsAdjusted(str[1]) ? GetAdjustedSize(str[1]) : Vector2.Zero));
                                Draw.Rect(unexploredRoomPosition + Vector2.One, unexploredRoomSize.X, unexploredRoomSize.Y, UnexploredRoomColor * Opacity);
                                Draw.HollowRect(unexploredRoomPosition + Vector2.One, unexploredRoomSize.X, unexploredRoomSize.Y, RoomBorderColor * Opacity);
                                Draw.HollowRect(unexploredRoomPosition + new Vector2(2, 2), unexploredRoomSize.X - 2, unexploredRoomSize.Y - 2, RoomBorderColor * Opacity);
                                Draw.HollowRect(unexploredRoomPosition + new Vector2(3, 3), unexploredRoomSize.X - 4, unexploredRoomSize.Y - 4, RoomBorderColor * Opacity);
                                Draw.HollowRect(unexploredRoomPosition + new Vector2(4, 4), unexploredRoomSize.X - 6, unexploredRoomSize.Y - 6, RoomBorderColor * Opacity);
                            }
                        }
                    }
                }

                // Delete visted rooms if they do not exist anymore

                HashSet<string> DeleteRoomsList = new();
                foreach (string visitedRoom in XaphanModule.ModSaveData.VisitedRooms)
                {
                    string[] str = visitedRoom.Split('/');
                    if (visitedRoom.Contains(Prefix) && str[2] == "Ch" + chapterIndex)
                    {
                        bool roomExist = false;
                        foreach (LevelData levelData in MapData.Levels)
                        {
                            if (levelData.Name == str[3])
                            {
                                roomExist = true;
                                break;
                            }
                        }
                        if (!roomExist)
                        {
                            DeleteRoomsList.Add(visitedRoom);
                        }
                    }
                }
                foreach (string room in DeleteRoomsList)
                {
                    XaphanModule.ModSaveData.VisitedRooms.Remove(room);
                }

                // Visited rooms

                foreach (string visitedRoom in XaphanModule.ModSaveData.VisitedRooms)
                {
                    string[] str = visitedRoom.Split('/');
                    if (visitedRoom.Contains(Prefix) && str[2] == "Ch" + chapterIndex)
                    {
                        if (!roomUseTilesController(str[3]))
                        {
                            Vector2 visitedRoomPosition = CalcRoomPosition(GetRoomPosition(str[3]) + (roomIsAdjusted(str[3]) ? GetAdjustedPosition(str[3]) : Vector2.Zero), currentRoomPosition, currentRoomJustify, worldmapPosition);
                            Vector2 visitedRoomSize = ConvertRoomSizeToMapSize(GetRoomSize(str[3]) + (roomIsAdjusted(str[3]) ? GetAdjustedSize(str[3]) : Vector2.Zero));
                            Draw.Rect(visitedRoomPosition + Vector2.One, visitedRoomSize.X, visitedRoomSize.Y, HeatedRooms.Contains(str[3]) ? HeatedRoomColor * Opacity : (roomIsSecret(str[3]) ? SecretRoomColor * Opacity : ExploredRoomColor * Opacity));
                            Draw.HollowRect(visitedRoomPosition + Vector2.One, visitedRoomSize.X, visitedRoomSize.Y, RoomBorderColor * Opacity);
                            Draw.HollowRect(visitedRoomPosition + new Vector2(2, 2), visitedRoomSize.X - 2, visitedRoomSize.Y - 2, RoomBorderColor * Opacity);
                            Draw.HollowRect(visitedRoomPosition + new Vector2(3, 3), visitedRoomSize.X - 4, visitedRoomSize.Y - 4, RoomBorderColor * Opacity);
                            Draw.HollowRect(visitedRoomPosition + new Vector2(4, 4), visitedRoomSize.X - 6, visitedRoomSize.Y - 6, RoomBorderColor * Opacity);
                        }
                    }
                }

                // Entrances (only if the room is not using a TilesController)

                foreach (InGameMapEntrancesData entrance in Entrances)
                {
                    if (!roomUseTilesController(entrance.Room))
                    {
                        Vector2 RoomPosition = CalcRoomPosition(GetRoomPosition(entrance.Room) + (roomIsAdjusted(entrance.Room) ? GetAdjustedPosition(entrance.Room) : Vector2.Zero), currentRoomPosition, currentRoomJustify, worldmapPosition);
                        if (isNotVisibleOnScreen(RoomPosition, entrance.Position))
                        {
                            continue;
                        }
                        Draw.Rect(RoomPosition + entrance.Position, entrance.Width, entrance.Height, entrance.Color * Opacity);
                    }
                }

                // Tiles (only if the room is using a TilesController)

                foreach (InGameMapTilesData tile in TilesImage)
                {
                    if (roomUseTilesController(tile.Room))
                    {
                        Vector2 RoomPosition = CalcRoomPosition(GetRoomPosition(tile.Room) + (roomIsAdjusted(tile.Room) ? GetAdjustedPosition(tile.Room) : Vector2.Zero), currentRoomPosition, currentRoomJustify, worldmapPosition);
                        if (isNotVisibleOnScreen(RoomPosition, tile.Position))
                        {
                            continue;
                        }
                        if (tile.BackgroundPattern == "Full")
                        {
                            Draw.Rect(RoomPosition + tile.Position, 40, 40, tile.BackgroundColor * Opacity);
                        }
                        else if (tile.BackgroundPattern != null)
                        {
                            Image TileBGImage = new(GFX.Gui["maps/tiles/background/" + tile.BackgroundPattern]);
                            TileBGImage.Position = RoomPosition + tile.Position;
                            TileBGImage.Color = tile.BackgroundColor * Opacity;
                            TileBGImage.Render();
                        }
                        if (tile.ElevatorPattern != null)
                        {
                            Image TileElevatorImage = new(GFX.Gui["maps/tiles/background/" + tile.ElevatorPattern]);
                            TileElevatorImage.Position = RoomPosition + tile.Position;
                            TileElevatorImage.Color = tile.ElevatorColor * Opacity;
                            TileElevatorImage.Render();
                        }
                        if (tile.Image != null)
                        {
                            Image TileBorderImage = tile.Image;
                            TileBorderImage.Position = RoomPosition + tile.Position;
                            TileBorderImage.Color = tile.RoomBorderColor * Opacity;
                            TileBorderImage.Render();
                        }
                    }
                }

                // Icons

                foreach (InGameMapIconsData icon in Icons)
                {
                    Vector2 RoomPosition = CalcRoomPosition(GetRoomPosition(icon.Room) + (roomIsAdjusted(icon.Room) ? GetAdjustedPosition(icon.Room) : Vector2.Zero), currentRoomPosition, currentRoomJustify, worldmapPosition);
                    Vector2 IconPosition = icon.Position;
                    if (isNotVisibleOnScreen(RoomPosition, IconPosition))
                    {
                        continue;
                    }
                    Image iconImage = null;
                    string path = icon.Type == "boss" ? "maps/" + Prefix + "/" : "maps/";
                    if (path.Contains("bubbleDoor") || path.Contains("pipeGate") || path.Contains("bigScreen"))
                    {
                        path = icon.Type;
                    }
                    if (icon.Checkmark)
                    {
                        iconImage = new Image(GFX.Gui[path + icon.Type + (icon.Type == "boss" ? (BossDefeatedCM ? "DefeatedCM" : "Defeated") : (icon.Type.Contains("collectableDoor") ? "Opened" : "Collected"))]);
                    }
                    else
                    {
                        iconImage = new Image(GFX.Gui[path + icon.Type]);
                    }
                    if (icon.Color != "FFFFFF")
                    {
                        iconImage.Color = Calc.HexToColor(icon.Color);
                    }
                    iconImage.Color *= Opacity;
                    iconImage.Position = RoomPosition + icon.Position;
                    if (icon.Type.Contains("player") || icon.Type.Contains("samus"))
                    {
                        if (!HideIndicator)
                        {
                            iconImage.Color = icon.Type.Contains("hair") ? (level.Tracker.GetEntity<Player>() != null ? level.Tracker.GetEntity<Player>().Hair.Color * Opacity : Color.White * Opacity) : Color.White * Opacity;
                            if (currentRoomIndicator)
                            {
                                iconImage.Render();
                            }
                        }
                    }
                    else
                    {
                        iconImage.Render();
                    }
                }

                // Images

                if (mode != "minimap" && !NoGrid)
                {
                    foreach (InGameMapImageControllerData image in ImageControllerData)
                    {
                        List<int> mapShards = GetUnlockedMapShards();
                        if (XaphanModule.ModSaveData.VisitedRooms.Contains(Prefix + "/Ch" + chapterIndex + "/" + image.Room) || (ExtraUnexploredRooms.Contains("Ch" + chapterIndex + "/" + image.Room)) || RevealUnexploredRooms())
                        {
                            Vector2 RoomPosition = CalcRoomPosition(GetRoomPosition(image.Room) + (roomIsAdjusted(image.Room) ? GetAdjustedPosition(image.Room) : Vector2.Zero), currentRoomPosition, currentRoomJustify, worldmapPosition);
                            Image Image = new(GFX.Gui["maps/" + Prefix + "/areas/" + image.ImagePath + (Settings.Instance.Language == "french" ? "-" + Settings.Instance.Language : "")]);
                            Image.Color = Calc.HexToColor(string.IsNullOrEmpty(image.Color) ? "FFFFFF" : image.Color);
                            Image.Position = RoomPosition;
                            Image.Render();
                        }
                        foreach (int mapShard in mapShards)
                        {
                            if (!XaphanModule.ModSaveData.VisitedRooms.Contains(Prefix + "/Ch" + chapterIndex + "/" + image.Room) && (((UnexploredRooms.Contains("Ch" + chapterIndex + "/" + image.Room + ":" + mapShard) && MapCollected))))
                            {
                                Vector2 RoomPosition = CalcRoomPosition(GetRoomPosition(image.Room) + (roomIsAdjusted(image.Room) ? GetAdjustedPosition(image.Room) : Vector2.Zero), currentRoomPosition, currentRoomJustify, worldmapPosition);
                                Image Image = new(GFX.Gui["maps/" + Prefix + "/areas/" + image.ImagePath + (Settings.Instance.Language == "french" ? "-" + Settings.Instance.Language : "")]);
                                Image.Color = Calc.HexToColor(string.IsNullOrEmpty(image.Color) ? "FFFFFF" : image.Color);
                                Image.Position = RoomPosition;
                                Image.Render();
                            }
                        }
                    }
                }

                // Hints

                if (ShowHints && HintTargetSprite != null && HintArrowSprite != null)
                {
                    foreach (InGameMapHintControllerData hint in HintControllerData)
                    {
                        if (level.Session.Level != hint.Room || (level.Session.Level == hint.Room && playerPosition != new Vector2(hint.TileCordX, hint.TileCordY)))
                        {
                            if (!hint.RemoveWhenReachedByPlayer || (hint.RemoveWhenReachedByPlayer && !XaphanModule.ModSaveData.VisitedRoomsTiles.Contains(Prefix + "/Ch" + chapterIndex + "/" + hint.Room + "-" + hint.TileCordX + "-" + hint.TileCordY)))
                            {
                                bool AllFlagsTrue = true;
                                foreach (string flag in hint.DisplayFlags)
                                {
                                    bool flagCheck = level.Session.GetFlag(flag) ? true : level.Session.GetFlag("Ch" + chapterIndex + "_" + flag);
                                    if (!string.IsNullOrEmpty(flag) && !flagCheck)
                                    {
                                        AllFlagsTrue = false;
                                        break;
                                    }
                                }
                                if (AllFlagsTrue && (!level.Session.GetFlag(hint.HideFlag) && !level.Session.GetFlag("Ch" + chapterIndex + "_" + hint.HideFlag)))
                                {
                                    Vector2 RoomPosition = CalcRoomPosition(GetRoomPosition(hint.Room) + (roomIsAdjusted(hint.Room) ? GetAdjustedPosition(hint.Room) : Vector2.Zero), currentRoomPosition, currentRoomJustify, worldmapPosition);
                                    if (hint.Type == "Arrow" && mode != "minimap")
                                    {
                                        if (isNotVisibleOnScreen(RoomPosition, HintArrowSprite.Position))
                                        {
                                            continue;
                                        }
                                        HintArrowSprite.Play("hintArrow");
                                        if (hint.Direction == "Up" || hint.Direction == "Down")
                                        {
                                            HintArrowSprite.Position = Vector2.One + new Vector2(hint.TileCordX * 40, hint.TileCordY * 40) + Vector2.UnitY * -10;
                                            if (hint.Direction == "Down")
                                            {
                                                HintArrowSprite.FlipY = true;
                                            }
                                        }
                                        else
                                        {
                                            if (hint.Direction == "Left")
                                            {
                                                HintArrowSprite.Position = Vector2.One + new Vector2(hint.TileCordX * 40, hint.TileCordY * 40) + new Vector2(-10f, 40f);
                                                HintArrowSprite.Rotation = -(float)Math.PI / 2f;
                                            }
                                            else if (hint.Direction == "Right")
                                            {
                                                HintArrowSprite.Position = Vector2.One + new Vector2(hint.TileCordX * 40, hint.TileCordY * 40) + new Vector2(50f, 0f);
                                                HintArrowSprite.Rotation = (float)Math.PI / 2f;
                                            }
                                        }
                                        HintArrowSprite.Color = Color.White * Opacity;
                                        HintArrowSprite.Render();
                                    }
                                    else if (hint.Type == "Target")
                                    {
                                        if (isNotVisibleOnScreen(RoomPosition, HintTargetSprite.Position))
                                        {
                                            continue;
                                        }
                                        HintTargetSprite.Play("hint");
                                        HintTargetSprite.Position = Vector2.One + new Vector2(hint.TileCordX * 40, hint.TileCordY * 40);
                                        HintTargetSprite.Color = Color.White * Opacity;
                                        HintTargetSprite.Render();
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }

        // Backward compatibility

        public static void RestaureExploredTiles(string prefix, int chapterIndex, Level level)
        {
            AreaKey area = level.Session.Area;
            MapData MapData2 = AreaData.Areas[area.ID].Mode[(int)area.Mode].MapData;
            foreach (string visitedRoom in XaphanModule.ModSaveData.VisitedRooms)
            {
                if (visitedRoom.Contains(prefix + "/Ch" + chapterIndex + "/"))
                {
                    string[] str = visitedRoom.Split('/');
                    Vector2[] tilePosition = new Vector2[100];
                    int totalEntities = 0;
                    foreach (InGameMapTilesControllerData tilesControllerData in OldTilesControllerData)
                    {
                        if (tilesControllerData.Room == str[3])
                        {
                            for (int i = 0; i <= 9; i++)
                            {
                                string tileCords = tilesControllerData.GetTileCords(i);
                                string[] str2 = tileCords.Split('-');
                                tilePosition[totalEntities * 10 + i] = new Vector2(int.Parse(str2[0]), int.Parse(str2[1]));
                            }
                            totalEntities++;
                        }
                    }
                    foreach (Vector2 position in tilePosition)
                    {
                        if (!XaphanModule.ModSaveData.VisitedRoomsTiles.Contains(prefix + "/Ch" + chapterIndex + "/" + str[3] + "-" + position.X + "-" + position.Y))
                        {
                            XaphanModule.ModSaveData.VisitedRoomsTiles.Add(prefix + "/Ch" + chapterIndex + "/" + str[3] + "-" + position.X + "-" + position.Y);
                        }
                    }
                }
            }
        }
    }
}