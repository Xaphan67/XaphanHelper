using System.Collections;
using System.Collections.Generic;
using Celeste.Mod.XaphanHelper.Data;
using Microsoft.Xna.Framework;

namespace Celeste.Mod.XaphanHelper
{
    public class StatsFlags
    {
        public class StrawberryData
        {
            public AreaKey AreaKey;

            public EntityID StrawberryID;

            public StrawberryData(AreaKey areaKey, EntityID strawberryID)
            {
                AreaKey = areaKey;
                StrawberryID = strawberryID;
            }
        }

        public static List<InGameMapRoomControllerData> RoomControllerData = new();

        public static List<InGameMapTilesControllerData> TilesControllerData = new();

        public static List<InGameMapEntitiesData> EntitiesData = new();

        public static int maxChapters;

        public static bool hasInterlude;

        public static int[] CurrentTiles;

        public static int[] TotalTiles;

        public static Dictionary<int, int>[] CurrentSubAreaTiles;

        public static Dictionary<int, int>[] TotalSubAreaTiles;

        public static int[] CurrentEnergyTanks;

        public static int[] TotalEnergyTanks;

        public static int[] CurrentFireRateModules;

        public static int[] TotalFireRateModules;

        public static int[] CurrentMissiles;

        public static int[] TotalMissiles;

        public static int[] CurrentSuperMissiles;

        public static int[] TotalSuperMissiles;

        public static int[] CurrentStrawberries;

        public static int[] TotalStrawberries;

        public static Dictionary<int, int>[] CurrentSubAreaStrawberries;

        public static Dictionary<int, int>[] TotalSubAreaStrawberries;

        public static bool[,] GoldensBerries;

        public static HashSet<StrawberryData> Strawberries = new();

        public static HashSet<StrawberryData> AlreadyCollectedStrawberries = new();

        public static int TotalASideHearts;

        public static int heartCount;

        public static bool[] ASideHearts;

        public static bool[] BSideHearts;

        public static bool[] Cassettes;

        public static int cassetteCount;

        public static bool useStatsFlagsController;

        public static bool initialized;

        public static bool fixedAchievements;

        public static void Load()
        {
            Everest.Events.Level.OnEnter += onLevelEnter;
            Everest.Events.Level.OnExit += onLevelExit;
            On.Celeste.Level.Update += onLevelUpdate;
            On.Celeste.Strawberry.CollectRoutine += onStrawberryCollectRoutine;
            On.Celeste.HeartGem.CollectRoutine += onHeartGemCollectRoutine;
            On.Celeste.Cassette.CollectRoutine += OnCassetteCollectRoutine;
        }

        public static void Unload()
        {
            Everest.Events.Level.OnEnter -= onLevelEnter;
            Everest.Events.Level.OnExit -= onLevelExit;
            On.Celeste.Level.Update -= onLevelUpdate;
            On.Celeste.Strawberry.CollectRoutine -= onStrawberryCollectRoutine;
            On.Celeste.HeartGem.CollectRoutine -= onHeartGemCollectRoutine;
            On.Celeste.Cassette.CollectRoutine -= OnCassetteCollectRoutine;
        }

        private static void CheckStatsFlagsController(MapData MapData)
        {
            useStatsFlagsController = false;
            foreach (LevelData level in MapData.Levels)
            {
                foreach (EntityData entity in level.Entities)
                {
                    if (entity.Name == "XaphanHelper/SetStatsFlagsController")
                    {
                        useStatsFlagsController = true;
                        break;
                    }
                }
            }
        }

        private static void onLevelEnter(Session session, bool fromSaveData)
        {
            fixedAchievements = false;
            CheckStatsFlagsController(AreaData.Areas[(SaveData.Instance.GetLevelSetStats().AreaOffset)].Mode[0].MapData);
            if (useStatsFlagsController)
            {
                GetStats(session);
                initialized = true;
            }
        }

        private static void onLevelExit(Level level, LevelExit exit, LevelExit.Mode mode, Session session, HiresSnow snow)
        {
            ResetStats();
        }

        public static void GetStats(Session session)
        {
            hasInterlude = false;
            string Prefix = session.Area.GetLevelSet();
            maxChapters = SaveData.Instance.GetLevelSetStats().Areas.Count;
            for (int i = 0; i < maxChapters; i++)
            {
                if (AreaData.Areas[(SaveData.Instance.GetLevelSetStats().AreaOffset + i)].Interlude)
                {
                    hasInterlude = true;
                    break;
                }
            }
            if (!hasInterlude)
            {
                maxChapters++;
            }
            AlreadyCollectedStrawberries.Clear();
            CurrentTiles = new int[maxChapters];
            TotalTiles = new int[maxChapters];
            CurrentSubAreaTiles = new Dictionary<int, int>[maxChapters];
            TotalSubAreaTiles = new Dictionary<int, int>[maxChapters];
            CurrentEnergyTanks = new int[maxChapters];
            TotalEnergyTanks = new int[maxChapters];
            CurrentFireRateModules = new int[maxChapters];
            TotalFireRateModules = new int[maxChapters];
            CurrentMissiles = new int[maxChapters];
            TotalMissiles = new int[maxChapters];
            CurrentSuperMissiles = new int[maxChapters];
            TotalSuperMissiles = new int[maxChapters];
            ASideHearts = new bool[maxChapters];
            BSideHearts = new bool[maxChapters];
            Cassettes = new bool[maxChapters];
            CurrentStrawberries = new int[maxChapters];
            TotalStrawberries = new int[maxChapters];
            GoldensBerries = new bool[maxChapters, 3];
            CurrentSubAreaStrawberries = new Dictionary<int, int>[maxChapters];
            TotalSubAreaStrawberries = new Dictionary<int, int>[maxChapters];
            heartCount = 0;
            cassetteCount = 0;
            TotalASideHearts = SaveData.Instance.GetLevelSetStatsFor(SaveData.Instance.GetLevelSet()).MaxHeartGems;
            for (int i = !hasInterlude ? 1 : 0; i < maxChapters; i++)
            {
                MapData MapData = AreaData.Areas[(SaveData.Instance.GetLevelSetStats().AreaOffset + i - (!hasInterlude ? 1 : 0))].Mode[0].MapData;
                RoomControllerData.Clear();
                TilesControllerData.Clear();
                EntitiesData.Clear();
                Strawberries.Clear();
                GetRoomsControllers(MapData);
                GetTilesControllers(MapData);
                GetEntities(MapData);
                CurrentTiles[i] = getCurrentMapTiles(Prefix, i);
                TotalTiles[i] = getTotalMapTiles();
                CurrentSubAreaTiles[i] = getSubAreaTiles(Prefix, i);
                TotalSubAreaTiles[i] = getSubAreaTiles(Prefix, i, true);
                CurrentEnergyTanks[i] = getCurrentEnergyTanks(Prefix, i);
                TotalEnergyTanks[i] = getTotalEnergyTanks();
                CurrentFireRateModules[i] = getCurrentFireRateModules(Prefix, i);
                TotalFireRateModules[i] = getTotalFireRateModules();
                CurrentMissiles[i] = getCurrentMissiles(Prefix, i);
                TotalMissiles[i] = getTotalMissiles();
                CurrentSuperMissiles[i] = getCurrentSuperMissiles(Prefix, i);
                TotalSuperMissiles[i] = getTotalSuperMissiles();
                GetStrawberries(MapData);
                GetAlreadyCollectedStrawberries(MapData);
                CurrentSubAreaStrawberries[i] = getSubAreaStrawberries(Prefix, i);
                TotalSubAreaStrawberries[i] = getSubAreaStrawberries(Prefix, i, true);
                for (int j = 0; j <= 2; j++)
                {
                    if (AreaData.Areas[(SaveData.Instance.GetLevelSetStats().AreaOffset + i - (!hasInterlude ? 1 : 0))].HasMode((AreaMode)j))
                    {
                        MapData ModeMapData = AreaData.Areas[(SaveData.Instance.GetLevelSetStats().AreaOffset + i - (!hasInterlude ? 1 : 0))].Mode[j].MapData;
                        GetGoldenBerries(i, ModeMapData, j);
                    }
                }
                GetCurrentItems(i);
            }
        }

        public static void ResetStats()
        {
            maxChapters = 0;
            hasInterlude = false;
            if (XaphanModule.useIngameMap)
            {
                CurrentTiles = null;
                TotalTiles = null;
            }
            ASideHearts = null;
            BSideHearts = null;
            Cassettes = null;
            CurrentStrawberries = null;
            TotalStrawberries = null;
            GoldensBerries = null;
            heartCount = 0;
            cassetteCount = 0;
            RoomControllerData.Clear();
            TilesControllerData.Clear();
            EntitiesData.Clear();
            CurrentTiles = null;
            TotalTiles = null;
            initialized = false;
        }

        public static void RemoveCompletedAchievementsIfNoLongerComplete(Session session)
        {
            fixedAchievements = true;
            List<AchievementData> achievements = Achievements.GenerateAchievementsList(session);
            HashSet<string> IDsToRemove = new();
            foreach (AchievementData achievement in achievements)
            {
                if (achievement.AchievementID.Contains("map") || // In-game Map achievements
                    achievement.AchievementID.Contains("strwb") || // Strawberries achievements
                    achievement.AchievementID.Contains("tank") || // Energy Tanks achievements
                    achievement.AchievementID.Contains("dfrm") || // Drone Fire Rate Modules achievements
                    achievement.AchievementID.Contains("dmiss") || // Drone Missiles Modules achievements
                    achievement.AchievementID.Contains("cass") || // Cassettes achievements
                    achievement.AchievementID.Contains("heart") || // Blue Hearts achievements
                    achievement.AchievementID.Contains("items") || // All Items achievement
                    achievement.AchievementID.Contains("golden") // Golden Strawberries achievements
                    )
                {
                    if (XaphanModule.ModSaveData.Achievements.Contains(achievement.AchievementID) && !session.GetFlag(achievement.Flag))
                    {
                        IDsToRemove.Add(achievement.AchievementID);
                    }
                }

            }
            foreach (string id in IDsToRemove)
            {
                XaphanModule.ModSaveData.Achievements.Remove(id);
            }
        }

        private static void onLevelUpdate(On.Celeste.Level.orig_Update orig, Level self)
        {
            orig(self);
            if (useStatsFlagsController && initialized)
            {
                string Prefix = self.Session.Area.GetLevelSet();
                if (CurrentTiles != null && TotalTiles != null && XaphanModule.useIngameMap)
                {
                    int chaptersFullyExplored = 0;
                    for (int i = !hasInterlude ? 1 : 0; i < maxChapters; i++)
                    {
                        if (CurrentTiles[i] == TotalTiles[i])
                        {
                            self.Session.SetFlag("XaphanHelper_StatFlag_MapCh" + i);
                            chaptersFullyExplored++;
                        }
                    }
                    if (chaptersFullyExplored == (!hasInterlude ? maxChapters - 1 : maxChapters))
                    {
                        self.Session.SetFlag("XaphanHelper_StatFlag_Map");
                    }
                }
                if (CurrentSubAreaTiles != null && TotalSubAreaTiles != null && XaphanModule.useIngameMap)
                {
                    for (int i = !hasInterlude ? 1 : 0; i < maxChapters; i++)
                    {
                        foreach (int subAreIndex in CurrentSubAreaTiles[i].Keys)
                        {
                            if (CurrentSubAreaTiles[i][subAreIndex] == TotalSubAreaTiles[i][subAreIndex])
                            {
                                if (CurrentSubAreaTiles[i].TryGetValue(subAreIndex, out int currentTileValue) == TotalSubAreaTiles[i].TryGetValue(subAreIndex, out int totalTileValue))
                                {
                                    self.Session.SetFlag("XaphanHelper_StatFlag_MapCh" + i + "-" + subAreIndex);
                                }
                            }
                            CurrentSubAreaTiles[i].TryGetValue(subAreIndex, out int currentTileValue2);
                            if (currentTileValue2 > 0)
                            {
                                self.Session.SetFlag("XaphanHelper_StatFlag_MapCh" + i + "-" + subAreIndex + "-Visited");
                            }
                        }
                    }
                }
                if (CurrentEnergyTanks != null && TotalEnergyTanks != null)
                {
                    int ChaptersAllEnergyTanks = 0;
                    for (int i = !hasInterlude ? 1 : 0; i < maxChapters; i++)
                    {
                        if (CurrentEnergyTanks[i] == TotalEnergyTanks[i])
                        {
                            self.Session.SetFlag("XaphanHelper_StatFlag_EnergyTanksCh" + i);
                            ChaptersAllEnergyTanks++;
                        }
                        if (CurrentEnergyTanks[i] > 0)
                        {
                            self.Session.SetFlag("XaphanHelper_StatFlag_EnergyTank");
                        }
                    }
                    if (ChaptersAllEnergyTanks == (!hasInterlude ? maxChapters - 1 : maxChapters))
                    {
                        self.Session.SetFlag("XaphanHelper_StatFlag_EnergyTanks");
                    }

                }
                if (CurrentFireRateModules != null && TotalFireRateModules != null)
                {
                    int ChaptersAllFireRateModules = 0;
                    for (int i = !hasInterlude ? 1 : 0; i < maxChapters; i++)
                    {
                        if (CurrentFireRateModules[i] == TotalFireRateModules[i])
                        {
                            self.Session.SetFlag("XaphanHelper_StatFlag_FireRateModulesCh" + i);
                            ChaptersAllFireRateModules++;
                        }
                        if (CurrentFireRateModules[i] > 0)
                        {
                            self.Session.SetFlag("XaphanHelper_StatFlag_FireRateModule");
                        }
                    }
                    if (ChaptersAllFireRateModules == (!hasInterlude ? maxChapters - 1 : maxChapters))
                    {
                        self.Session.SetFlag("XaphanHelper_StatFlag_FireRateModules");
                    }
                }
                if (CurrentMissiles != null && TotalMissiles != null)
                {
                    int ChaptersAllMissiles = 0;
                    for (int i = !hasInterlude ? 1 : 0; i < maxChapters; i++)
                    {
                        if (CurrentMissiles[i] == TotalMissiles[i])
                        {
                            self.Session.SetFlag("XaphanHelper_StatFlag_MissilesCh" + i);
                            ChaptersAllMissiles++;
                        }
                        if (CurrentMissiles[i] > 0)
                        {
                            self.Session.SetFlag("XaphanHelper_StatFlag_Missile");
                        }
                    }
                    if (ChaptersAllMissiles == (!hasInterlude ? maxChapters - 1 : maxChapters))
                    {
                        self.Session.SetFlag("XaphanHelper_StatFlag_Missiles");
                    }
                }
                if (CurrentSuperMissiles != null && TotalSuperMissiles != null)
                {
                    int ChaptersAllSuperMissiles = 0;
                    for (int i = !hasInterlude ? 1 : 0; i < maxChapters; i++)
                    {
                        if (CurrentSuperMissiles[i] == TotalSuperMissiles[i])
                        {
                            self.Session.SetFlag("XaphanHelper_StatFlag_SuperMissilesCh" + i);
                            ChaptersAllSuperMissiles++;
                        }
                        if (CurrentSuperMissiles[i] > 0)
                        {
                            self.Session.SetFlag("XaphanHelper_StatFlag_SuperMissile");
                        }
                    }
                    if (ChaptersAllSuperMissiles == (!hasInterlude ? maxChapters - 1 : maxChapters))
                    {
                        self.Session.SetFlag("XaphanHelper_StatFlag_SuperMissiles");
                    }
                }
                if (CurrentStrawberries != null && TotalStrawberries != null)
                {
                    int ChaptersAllStrawberies = 0;
                    for (int i = !hasInterlude ? 1 : 0; i < maxChapters; i++)
                    {
                        if (CurrentStrawberries[i] == TotalStrawberries[i])
                        {
                            self.Session.SetFlag("XaphanHelper_StatFlag_StrawberriesCh" + i);
                            ChaptersAllStrawberies++;
                        }
                        if (CurrentStrawberries[i] > 0)
                        {
                            self.Session.SetFlag("XaphanHelper_StatFlag_Strawberry");
                        }
                    }
                    if (ChaptersAllStrawberies == (!hasInterlude ? maxChapters - 1 : maxChapters))
                    {
                        self.Session.SetFlag("XaphanHelper_StatFlag_Strawberries");
                    }
                }
                if (CurrentSubAreaStrawberries != null && TotalSubAreaStrawberries != null && XaphanModule.useIngameMap)
                {
                    for (int i = !hasInterlude ? 1 : 0; i < maxChapters; i++)
                    {
                        foreach (int subAreIndex in CurrentSubAreaStrawberries[i].Keys)
                        {
                            if (CurrentSubAreaStrawberries[i][subAreIndex] == TotalSubAreaStrawberries[i][subAreIndex])
                            {
                                if (CurrentSubAreaStrawberries[i].TryGetValue(subAreIndex, out int currentStrawberriesValue) == TotalSubAreaStrawberries[i].TryGetValue(subAreIndex, out int totalStrawberriesValue))
                                {
                                    self.Session.SetFlag("XaphanHelper_StatFlag_StrawberriesCh" + i + "-" + subAreIndex);
                                }
                            }
                        }
                    }
                }
                if (heartCount == TotalASideHearts)
                {
                    self.Session.SetFlag("XaphanHelper_StatFlag_Hearts");
                }
                if (cassetteCount == SaveData.Instance.GetLevelSetStatsFor(SaveData.Instance.GetLevelSet()).MaxCassettes)
                {
                    self.Session.SetFlag("XaphanHelper_StatFlag_Cassettes");
                }
                for (int i = !hasInterlude ? 1 : 0; i < maxChapters; i++)
                {
                    // Other flags

                    if (Cassettes != null)
                    {
                        if (Cassettes[i] == true)
                        {
                            self.Session.SetFlag("XaphanHelper_StatFlag_CassetteCh" + i);
                        }
                    }
                    if (ASideHearts != null)
                    {
                        if (ASideHearts[i] == true)
                        {
                            self.Session.SetFlag("XaphanHelper_StatFlag_HeartCh" + i);
                        }
                    }
                    if (BSideHearts != null)
                    {
                        if (BSideHearts[i] == true)
                        {
                            self.Session.SetFlag("XaphanHelper_StatFlag_BSideCh" + i);
                        }
                    }
                    if (GoldensBerries != null)
                    {
                        for (int j = 0; j < 3; j++)
                        {
                            if (GoldensBerries[i, j])
                            {
                                self.Session.SetFlag("XaphanHelper_StatFlag_GoldenCh" + i + "-" + j);
                            }
                        }
                    }

                    // SoCM only flags

                    if (Prefix == "Xaphan/0")
                    {
                        if (XaphanModule.ModSaveData.SavedFlags.Contains(Prefix + "_Ch" + i + "_Gem_Collected"))
                        {
                            self.Session.SetFlag("XaphanHelper_StatFlag_GemCh" + i);
                        }
                        if (XaphanModule.ModSaveData.SavedFlags.Contains(Prefix + "_Ch" + i + "_Boss_Defeated"))
                        {
                            self.Session.SetFlag("XaphanHelper_StatFlag_BossCh" + i);
                        }
                        if (XaphanModule.ModSaveData.SavedFlags.Contains(Prefix + "_Ch" + i + "_Boss_Defeated_CM"))
                        {
                            self.Session.SetFlag("XaphanHelper_StatFlag_BossCMCh" + i);
                        }
                        if (XaphanModule.ModSaveData.SavedFlags.Contains(Prefix + "_Ch" + i + "_Bubbledoor_Red"))
                        {
                            self.Session.SetFlag("XaphanHelper_StatFlag_LockRedCh" + i);
                        }
                        if (XaphanModule.ModSaveData.SavedFlags.Contains(Prefix + "_Ch" + i + "_Bubbledoor_Green"))
                        {
                            self.Session.SetFlag("XaphanHelper_StatFlag_LockGreenCh" + i);
                        }
                        if (XaphanModule.ModSaveData.SavedFlags.Contains(Prefix + "_Ch" + i + "_Bubbledoor_Yellow"))
                        {
                            self.Session.SetFlag("XaphanHelper_StatFlag_LockYellowCh" + i);
                        }

                        // Chapter Specific

                        if (XaphanModule.ModSaveData.SavedFlags.Contains(Prefix + "_Ch2_Temple_Activated"))
                        {
                            self.Session.SetFlag("XaphanHelper_StatFlag_TempleCh2");
                        }

                        // Golden

                        if (XaphanModule.ModSaveData.SavedFlags.Contains(Prefix + "_Ch5_SoCMGolden"))
                        {
                            self.Session.SetFlag("XaphanHelper_StatFlag_SoCMGolden");
                        }
                    }
                }
                if (self.Session.Area.Mode == 0 && self.Session.Area.LevelSet == "Xaphan/0" && !fixedAchievements)
                {
                    RemoveCompletedAchievementsIfNoLongerComplete(self.Session);
                }
            }
        }

        private static IEnumerator onStrawberryCollectRoutine(On.Celeste.Strawberry.orig_CollectRoutine orig, Strawberry self, int collectIndex)
        {
            if (useStatsFlagsController)
            {
                int chapterIndex = self.SceneAs<Level>().Session.Area.ChapterIndex;
                int mode = (int)self.SceneAs<Level>().Session.Area.Mode;
                bool strawberryAlreadyCollected = false;
                foreach (StrawberryData strawberryData in AlreadyCollectedStrawberries)
                {
                    if (strawberryData.AreaKey == self.SceneAs<Level>().Session.Area && strawberryData.StrawberryID.Level == self.ID.Level && strawberryData.StrawberryID.ID == self.ID.ID)
                    {
                        strawberryAlreadyCollected = true;
                        break;
                    }
                }
                if (!strawberryAlreadyCollected)
                {
                    if (!self.Golden)
                    {
                        if (CurrentStrawberries != null)
                        {
                            if (chapterIndex == -1)
                            {
                                chapterIndex = 0;
                            }
                            CurrentStrawberries[chapterIndex] += 1;
                        }
                        if (CurrentSubAreaStrawberries != null && TotalSubAreaStrawberries != null && Strawberries != null)
                        {
                            foreach (StrawberryData strawberry in Strawberries)
                            {
                                if (strawberry.StrawberryID.Level == self.ID.Level && strawberry.StrawberryID.ID == self.ID.ID)
                                {
                                    foreach (InGameMapRoomControllerData roomControllerData in RoomControllerData)
                                    {
                                        if (roomControllerData.Room == strawberry.StrawberryID.Level)
                                        {
                                            CurrentSubAreaStrawberries[chapterIndex][roomControllerData.SubAreaIndex] += 1;
                                        }
                                    }
                                }
                            }
                        }
                    }
                    else
                    {
                        if (GoldensBerries != null)
                        {
                            if (chapterIndex == -1)
                            {
                                chapterIndex = 0;
                            }
                            GoldensBerries[chapterIndex, mode] = true;
                        }
                    }
                }
            }
            yield return new SwapImmediately(orig(self, collectIndex));
        }

        private static IEnumerator onHeartGemCollectRoutine(On.Celeste.HeartGem.orig_CollectRoutine orig, HeartGem self, Player player)
        {
            if (useStatsFlagsController)
            {
                int chapterIndex = self.SceneAs<Level>().Session.Area.ChapterIndex;
                if (self.SceneAs<Level>().Session.Area.Mode == 0 && !SaveData.Instance.Areas_Safe[self.SceneAs<Level>().Session.Area.ID].Modes[0].HeartGem)
                {
                    heartCount += 1;
                }
                if (self.SceneAs<Level>().Session.Area.Mode == 0 && ASideHearts != null)
                {
                    ASideHearts[chapterIndex] = true;
                }
                if ((int)self.SceneAs<Level>().Session.Area.Mode == 1 && BSideHearts != null)
                {
                    BSideHearts[chapterIndex] = true;
                }
            }
            yield return new SwapImmediately(orig(self, player));
        }

        private static IEnumerator OnCassetteCollectRoutine(On.Celeste.Cassette.orig_CollectRoutine orig, Cassette self, Player player)
        {
            if (useStatsFlagsController)
            {
                int chapterIndex = self.SceneAs<Level>().Session.Area.ChapterIndex;
                if (!SaveData.Instance.Areas_Safe[self.SceneAs<Level>().Session.Area.ID].Cassette)
                {
                    cassetteCount += 1;
                }
                if (Cassettes != null)
                {
                    Cassettes[chapterIndex] = true;
                }
            }
            yield return new SwapImmediately(orig(self, player));

        }

        private static void GetRoomsControllers(MapData MapData)
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


        private static void GetTilesControllers(MapData MapData)
        {
            foreach (LevelData level in MapData.Levels)
            {
                foreach (EntityData entity in level.Entities)
                {
                    if (entity.Name == "XaphanHelper/InGameMapTilesController")
                    {
                        TilesControllerData.Add(new InGameMapTilesControllerData(0, level.Name, entity.Attr("tile0Cords"), entity.Attr("tile0"), entity.Attr("tile1Cords"), entity.Attr("tile1"), entity.Attr("tile2Cords"), entity.Attr("tile2"),
                            entity.Attr("tile3Cords"), entity.Attr("tile3"), entity.Attr("tile4Cords"), entity.Attr("tile4"), entity.Attr("tile5Cords"), entity.Attr("tile5"), entity.Attr("tile6Cords"), entity.Attr("tile6"),
                            entity.Attr("tile7Cords"), entity.Attr("tile7"), entity.Attr("tile8Cords"), entity.Attr("tile8"), entity.Attr("tile9Cords"), entity.Attr("tile9"), entity.Attr("display")));
                    }
                }
            }
        }

        private static void GetEntities(MapData MapData)
        {
            foreach (LevelData level in MapData.Levels)
            {
                foreach (EntityData entity in level.Entities)
                {
                    if (entity.Name == "XaphanHelper/CustomFollower")
                    {
                        string str = entity.Attr("type").Replace(" ", "");
                        string type = (char.ToLower(str[0]) + str.Substring(1));
                        EntitiesData.Add(new InGameMapEntitiesData(0, level.Name, level, type, new Vector2(entity.Position.X, entity.Position.Y), Vector2.Zero, MapData.Area, entity.ID));
                    }
                }
            }
        }

        private static void GetStrawberries(MapData MapData)
        {
            foreach (LevelData level in MapData.Levels)
            {
                foreach (EntityData entity in level.Entities)
                {
                    if (StrawberryRegistry.TrackableContains(entity.Name))
                    {
                        Strawberries.Add(new StrawberryData(MapData.Area, new EntityID(entity.Level.Name, entity.ID)));
                    }
                }
            }
        }

        private static void GetAlreadyCollectedStrawberries(MapData MapData)
        {
            foreach (LevelData level in MapData.Levels)
            {
                foreach (EntityData entity in level.Entities)
                {
                    if (SaveData.Instance.CheckStrawberry(MapData.Area, new EntityID(entity.Level.Name, entity.ID)))
                    {
                        AlreadyCollectedStrawberries.Add(new StrawberryData(MapData.Area, new EntityID(entity.Level.Name, entity.ID)));
                    }
                }
            }
        }

        private static void GetGoldenBerries(int chapterIndex, MapData MapData, int Mode)
        {
            string Prefix = SaveData.Instance.GetLevelSetStats().Name;
            foreach (AreaStats item in SaveData.Instance.Areas_Safe)
            {
                if (item.GetLevelSet() == Prefix && item.ID == SaveData.Instance.GetLevelSetStats().AreaOffset + chapterIndex - (!hasInterlude ? 1 : 0))
                {
                    foreach (EntityData goldenberry in MapData.Goldenberries)
                    {
                        EntityID goldenID = new(goldenberry.Level.Name, goldenberry.ID);
                        if (SaveData.Instance.Areas_Safe[item.ID].Modes[Mode].Strawberries.Contains(goldenID))
                        {
                            GoldensBerries[chapterIndex, Mode] = true;
                        }
                    }
                }
            }
        }

        private static void GetCurrentItems(int chapterIndex)
        {
            string Prefix = SaveData.Instance.GetLevelSetStats().Name;
            foreach (AreaStats item in SaveData.Instance.Areas_Safe)
            {
                if (item.GetLevelSet() == Prefix && item.ID == SaveData.Instance.GetLevelSetStats().AreaOffset + chapterIndex - (!hasInterlude ? 1 : 0))
                {
                    int strawberryCount = 0;
                    if (item.Modes[0].TotalStrawberries > 0 || item.TotalStrawberries > 0)
                    {
                        strawberryCount = item.TotalStrawberries;
                    }
                    if (GoldensBerries[chapterIndex, 0])
                    {
                        strawberryCount--;
                    }
                    CurrentStrawberries[chapterIndex] = strawberryCount;
                    TotalStrawberries[chapterIndex] = AreaData.Areas[SaveData.Instance.GetLevelSetStats().AreaOffset + chapterIndex - (!hasInterlude ? 1 : 0)].Mode[0].TotalStrawberries;
                    if (item.Modes[0].HeartGem)
                    {
                        heartCount += 1;
                        ASideHearts[chapterIndex] = true;
                    }
                    AreaData area = AreaData.Areas[(SaveData.Instance.GetLevelSetStats().AreaOffset + chapterIndex - (!hasInterlude ? 1 : 0))];
                    if (area.HasMode(AreaMode.BSide))
                    {
                        TotalASideHearts--;
                    }
                    if (area.HasMode(AreaMode.CSide))
                    {
                        TotalASideHearts--;
                    }
                    if (item.Modes[1].HeartGem)
                    {
                        BSideHearts[chapterIndex] = true;
                    }
                    if (item.Cassette)
                    {
                        cassetteCount += 1;
                        Cassettes[chapterIndex] = true;
                    }
                }
            }
        }

        public static int getCurrentMapTiles(string prefix, int chapterIndex)
        {
            int currentTiles = 0;
            foreach (InGameMapTilesControllerData tilesControllerData in TilesControllerData)
            {
                for (int i = 0; i <= 9; i++)
                {
                    string tile = tilesControllerData.GetTile(i);
                    if (tile != "None" && tile != "ElevatorShaft" && !tile.Contains("Arrow") && !tile.Contains("Connection"))
                    {
                        if (XaphanModule.ModSaveData.VisitedRoomsTiles.Contains(prefix + "/Ch" + chapterIndex + "/" + tilesControllerData.Room + "-" + tilesControllerData.GetTileCords(i)))
                        {
                            currentTiles++;
                        }
                    }
                }
            }
            return currentTiles;
        }

        public static int getTotalMapTiles()
        {
            int totalTiles = 0;
            foreach (InGameMapTilesControllerData tilesControllerData in TilesControllerData)
            {
                for (int i = 0; i <= 9; i++)
                {
                    string tile = tilesControllerData.GetTile(i);
                    if (tile != "None" && tile != "ElevatorShaft" && !tile.Contains("Arrow") && !tile.Contains("Connection"))
                    {
                        totalTiles++;
                    }
                }
            }
            return totalTiles;
        }

        public static int getCurrentEnergyTanks(string prefix, int chapterIndex)
        {
            int currentEnergyTanks = 0;
            foreach (InGameMapEntitiesData entityData in EntitiesData)
            {
                if (entityData.Type == "energyTank")
                {
                    if (XaphanModule.ModSaveData.StaminaUpgrades.Contains(prefix + "_Ch" + chapterIndex + "_" + entityData.Room + ":" + entityData.ID))
                    {
                        currentEnergyTanks++;
                    }
                }
            }
            return currentEnergyTanks;
        }

        public static int getTotalEnergyTanks()
        {
            int totalEnergyTanks = 0;
            foreach (InGameMapEntitiesData entityData in EntitiesData)
            {
                if (entityData.Type == "energyTank")
                {
                    totalEnergyTanks++;
                }
            }
            return totalEnergyTanks;
        }

        public static int getCurrentFireRateModules(string prefix, int chapterIndex)
        {
            int currentFireRateModules = 0;
            foreach (InGameMapEntitiesData entityData in EntitiesData)
            {
                if (entityData.Type == "fireRateModule")
                {
                    if (XaphanModule.ModSaveData.DroneFireRateUpgrades.Contains(prefix + "_Ch" + chapterIndex + "_" + entityData.Room + ":" + entityData.ID))
                    {
                        currentFireRateModules++;
                    }
                }
            }
            return currentFireRateModules;
        }

        public static int getTotalFireRateModules()
        {
            int totalFireRateModules = 0;
            foreach (InGameMapEntitiesData entityData in EntitiesData)
            {
                if (entityData.Type == "fireRateModule")
                {
                    totalFireRateModules++;
                }
            }
            return totalFireRateModules;
        }

        public static int getCurrentMissiles(string prefix, int chapterIndex)
        {
            int currentMissiles = 0;
            foreach (InGameMapEntitiesData entityData in EntitiesData)
            {
                if (entityData.Type == "missile")
                {
                    if (XaphanModule.ModSaveData.DroneMissilesUpgrades.Contains(prefix + "_Ch" + chapterIndex + "_" + entityData.Room + ":" + entityData.ID))
                    {
                        currentMissiles++;
                    }
                }
            }
            return currentMissiles;
        }

        public static int getTotalMissiles()
        {
            int totalMissiles = 0;
            foreach (InGameMapEntitiesData entityData in EntitiesData)
            {
                if (entityData.Type == "missile")
                {
                    totalMissiles++;
                }
            }
            return totalMissiles;
        }

        public static int getCurrentSuperMissiles(string prefix, int chapterIndex)
        {
            int currentSuperMissiles = 0;
            foreach (InGameMapEntitiesData entityData in EntitiesData)
            {
                if (entityData.Type == "superMissile")
                {
                    if (XaphanModule.ModSaveData.DroneSuperMissilesUpgrades.Contains(prefix + "_Ch" + chapterIndex + "_" + entityData.Room + ":" + entityData.ID))
                    {
                        currentSuperMissiles++;
                    }
                }
            }
            return currentSuperMissiles;
        }

        public static int getTotalSuperMissiles()
        {
            int totalSuperMissiles = 0;
            foreach (InGameMapEntitiesData entityData in EntitiesData)
            {
                if (entityData.Type == "superMissile")
                {
                    totalSuperMissiles++;
                }
            }
            return totalSuperMissiles;
        }

        public static Dictionary<int, int> getSubAreaTiles(string prefix, int chapterIndex, bool total = false)
        {
            Dictionary<int, int> subAreaTiles = new();
            foreach (InGameMapRoomControllerData roomControllerData in RoomControllerData)
            {
                int tiles = 0;
                foreach (InGameMapTilesControllerData tilesControllerData in TilesControllerData)
                {
                    if (tilesControllerData.Room == roomControllerData.Room)
                    {
                        for (int i = 0; i <= 9; i++)
                        {
                            string tile = tilesControllerData.GetTile(i);
                            if (tile != "None" && tile != "ElevatorShaft" && !tile.Contains("Arrow") && !tile.Contains("Connection"))
                            {
                                if (total)
                                {
                                    tiles++;
                                }
                                else if (XaphanModule.ModSaveData.VisitedRoomsTiles.Contains(prefix + "/Ch" + chapterIndex + "/" + tilesControllerData.Room + "-" + tilesControllerData.GetTileCords(i)))
                                {
                                    tiles++;
                                }
                            }

                        }
                    }
                }
                if (subAreaTiles.ContainsKey(roomControllerData.SubAreaIndex))
                {
                    subAreaTiles[roomControllerData.SubAreaIndex] += tiles;
                }
                else
                {
                    subAreaTiles.Add(roomControllerData.SubAreaIndex, tiles);
                }
            }
            return subAreaTiles;
        }

        public static Dictionary<int, int> getSubAreaStrawberries(string prefix, int chapterIndex, bool total = false)
        {
            Dictionary<int, int> subAreaStrawberries = new();
            foreach (InGameMapRoomControllerData roomControllerData in RoomControllerData)
            {
                int strawberries = 0;
                foreach (StrawberryData strawberryData in Strawberries)
                {
                    if (strawberryData.StrawberryID.Level == roomControllerData.Room)
                    {
                        if (total)
                        {
                            strawberries++;
                        }
                        else if (SaveData.Instance.CheckStrawberry(strawberryData.AreaKey, strawberryData.StrawberryID))
                        {
                            strawberries++;
                        }
                    }
                }
                if (subAreaStrawberries.ContainsKey(roomControllerData.SubAreaIndex))
                {
                    subAreaStrawberries[roomControllerData.SubAreaIndex] += strawberries;
                }
                else
                {
                    subAreaStrawberries.Add(roomControllerData.SubAreaIndex, strawberries);
                }
            }
            return subAreaStrawberries;
        }
    }
}
