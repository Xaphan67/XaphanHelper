using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using Celeste.Mod.Entities;
using Celeste.Mod.Meta;
using Celeste.Mod.XaphanHelper.Data;
using Celeste.Mod.XaphanHelper.UI_Elements;
using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste.Mod.XaphanHelper.Controllers
{
    [Tracked(true)]
    [CustomEntity("XaphanHelper/CustomEndScreenController")]
    class CustomEndScreenController : Entity
    {
        private static FieldInfo LevelExit_mode = typeof(LevelExit).GetField("mode", BindingFlags.Instance | BindingFlags.NonPublic);

        private static FieldInfo LevelExit_completeLoaded = typeof(LevelExit).GetField("completeLoaded", BindingFlags.Instance | BindingFlags.NonPublic);

        private static FieldInfo LevelExit_timer = typeof(LevelExit).GetField("timer", BindingFlags.Instance | BindingFlags.NonPublic);

        private static FieldInfo LevelExit_session = typeof(LevelExit).GetField("session", BindingFlags.Instance | BindingFlags.NonPublic);

        private static List<CustomEndScreenControllerData> CustomEndScreenControllerData = new();

        public CustomEndScreenController(EntityData data, Vector2 offset) : base(data.Position + offset)
        {

        }

        public static void Load()
        {
            On.Celeste.LevelExit.Begin += modLevelExitBegin;
            On.Celeste.Level.CompleteArea_bool_bool_bool += modLevelCompleteArea;
        }

        public static void Unload()
        {
            On.Celeste.LevelExit.Begin -= modLevelExitBegin;
            On.Celeste.Level.CompleteArea_bool_bool_bool -= modLevelCompleteArea;
        }

        private static void modLevelExitBegin(On.Celeste.LevelExit.orig_Begin orig, LevelExit self)
        {
            Session session = (Session)LevelExit_session.GetValue(self);
            if ((LevelExit.Mode)LevelExit_mode.GetValue(self) == LevelExit.Mode.Completed && CustomEndScreenControllerData.Count > 0)
            {
                StatsFlags.GetStats(session);
                CustomEndScreenControllerData SelectedScreenData = new()
                {
                    Priority = -1
                };
                foreach (CustomEndScreenControllerData data in CustomEndScreenControllerData)
                {
                    if ((!string.IsNullOrEmpty(data.RequiredFlags) ? CheckFlags(session, data.RequiredFlags) : true) && (data.RequiredTime != 0 ? CheckTime(data.RequirementsCheck, session, data.RequiredTime) : true) && GetStrawberries(data.RequirementsCheck, session) >= data.RequiredStrawberries && GetItemsPercent(data.RequirementsCheck, session) >= data.RequiredItemPercent && GetMapPercent(data.RequirementsCheck, session) >= data.RequiredMapPercent)
                    {
                        SelectedScreenData = data;
                        break;
                    }
                }
                if (SelectedScreenData.Priority == -1)
                {
                    orig(self);
                }
                else
                {
                    SaveLoadIcon.Show(self);
                    HiresSnow snow = new();
                    snow.Direction = new Vector2(0f, 16f);
                    snow.Reset();
                    AreaData areaData = AreaData.Get(session);
                    Atlas completeAtlas;
                    MapMetaCompleteScreen completeMeta;
                    if (string.IsNullOrEmpty(SelectedScreenData.Atlas))
                    {
                        completeAtlas = Atlas.FromAtlas(Path.Combine("Graphics", "Atlases", "EndScreens/XaphanHelper"), Atlas.AtlasDataFormat.PackerNoAtlas);
                    }
                    else
                    {
                        if (SelectedScreenData.Atlas.EndsWith("/"))
                        {
                            SelectedScreenData.Atlas = SelectedScreenData.Atlas.Remove(SelectedScreenData.Atlas.Length - 1);
                        }
                        completeAtlas = Atlas.FromAtlas(Path.Combine("Graphics", "Atlases", SelectedScreenData.Atlas), Atlas.AtlasDataFormat.PackerNoAtlas);
                    }
                    completeMeta = new MapMetaCompleteScreen();
                    completeMeta.StartArray = new float[] { 0.0f, 0.0f };
                    completeMeta.CenterArray = new float[] { 0.0f, 0.0f };
                    completeMeta.OffsetArray = new float[] { 0.0f, 0.0f };
                    string[] images = string.IsNullOrEmpty(SelectedScreenData.Atlas) ? new string[0] : SelectedScreenData.Images.Split(',');
                    completeMeta.Layers = new MapMetaCompleteScreenLayer[images.GetLength(0) + (SelectedScreenData.ShowTitle ? 1 : 0)];
                    for (int i = 0; i < images.GetLength(0); i++)
                    {
                        MapMetaCompleteScreenLayer layer = new();
                        layer.Type = "layer";
                        layer.Images = new string[1];
                        layer.Images[0] = images[i];
                        layer.PositionArray = new float[] { 0.0f, 0.0f };
                        layer.ScrollArray = new float[] { 0.0f };
                        completeMeta.Layers[i] = layer;
                    }
                    if (SelectedScreenData.ShowTitle)
                    {
                        MapMetaCompleteScreenLayer layer = new();
                        layer.Type = "ui";
                        completeMeta.Layers[images.GetLength(0)] = layer;
                    }
                    LevelExit_completeLoaded.SetValue(self, true);
                    if (!string.IsNullOrEmpty(SelectedScreenData.Music))
                    {
                        Audio.SetMusic(SelectedScreenData.Music);
                    }
                    else
                    {
                        if (session.Area.Mode != 0)
                        {
                            Audio.SetMusic("event:/music/menu/complete_bside");
                        }
                        else
                        {
                            Audio.SetMusic("event:/music/menu/complete_area");
                        }
                    }
                    Audio.SetAmbience(null);
                    Entity entity;
                    self.Add(entity = new Entity());
                    entity.Add(new Coroutine(CustomEndScreenRoutine(self, session, completeAtlas, snow, completeMeta, SelectedScreenData.ShowTime ? GetTime(SelectedScreenData.RequirementsCheck, session) : "", SelectedScreenData.ShowStrawberries ? GetStrawberries(SelectedScreenData.RequirementsCheck, session) : -1, SelectedScreenData.ShowStrawberries ? GetMaxStrawberries(SelectedScreenData.RequirementsCheck, session) : -1, SelectedScreenData.ShowItemPercent ? GetItemsPercent(SelectedScreenData.RequirementsCheck, session) : -1, SelectedScreenData.ShowMapPercent ? GetMapPercent(SelectedScreenData.RequirementsCheck, session) : -1, SelectedScreenData)));
                    self.Add(snow);
                    new FadeWipe(self, wipeIn: true);
                    Stats.Store();
                    self.RendererList.UpdateLists();
                    StatsFlags.ResetStats();
                }
            }
            else
            {
                orig(self);
            }
        }

        private static ScreenWipe modLevelCompleteArea(On.Celeste.Level.orig_CompleteArea_bool_bool_bool orig, Level self, bool spotlightWipe, bool skipScreenWipe, bool skipCompleteScreen)
        {
            GetCustomEndScreenControllerData(self.Session);
            if (CustomEndScreenControllerData.Count > 0)
            {
                self.RegisterAreaComplete();
                self.PauseLock = true;
                Action action = delegate
                {
                    Engine.Scene = new LevelExit(LevelExit.Mode.Completed, self.Session);
                };
                if (!self.SkippingCutscene && !skipScreenWipe)
                {
                    if (spotlightWipe)
                    {
                        Player entity = self.Tracker.GetEntity<Player>();
                        if (entity != null)
                        {
                            SpotlightWipe.FocusPoint = entity.Position - self.Camera.Position - new Vector2(0f, 8f);
                        }
                        return new SpotlightWipe(self, false, action);
                    }
                    return new FadeWipe(self, false, action);
                }
                Audio.BusStopAll("bus:/gameplay_sfx", immediate: true);
                action();
                return null;
            }
            else
            {
                CustomEndScreenControllerData.Clear();
                return orig(self, spotlightWipe, skipScreenWipe, skipCompleteScreen);
            }
        }

        private static IEnumerator CustomEndScreenRoutine(LevelExit levelExit, Session session, Atlas completeAtlas, HiresSnow snow, MapMetaCompleteScreen completeMeta, string clearTime, int strawberries, int maxStrawberries, float itemPercent, float mapPercent, CustomEndScreenControllerData screenData)
        {
            UserIO.SaveHandler(file: true, settings: true);
            while (UserIO.Saving)
            {
                yield return null;
            }
            while (!(bool)LevelExit_completeLoaded.GetValue(levelExit))
            {
                yield return null;
            }
            while (SaveLoadIcon.OnScreen)
            {
                yield return null;
            }
            while ((float)LevelExit_timer.GetValue(levelExit) < 3.3f)
            {
                yield return null;
            }
            Audio.SetMusicParam("end", 1f);
            Engine.Scene = new CustomEndScreen(session, completeAtlas, snow, completeMeta, clearTime, strawberries, maxStrawberries, itemPercent, mapPercent, screenData);
            CustomEndScreenControllerData.Clear();
        }

        private static void GetCustomEndScreenControllerData(Session session)
        {
            AreaKey area = session.Area;
            MapData MapData = AreaData.Areas[area.ID].Mode[(int)area.Mode].MapData;
            foreach (LevelData levelData in MapData.Levels)
            {
                foreach (EntityData entity in levelData.Entities)
                {
                    if (entity.Name == "XaphanHelper/CustomEndScreenController")
                    {
                        CustomEndScreenControllerData.Add(new CustomEndScreenControllerData(entity.Attr("atlas"), entity.Attr("images"), entity.Attr("title"), entity.Bool("showTitle"), entity.Attr("subText1"), entity.Attr("subText1Color"), entity.Attr("subText2"), entity.Attr("subText2Color"), entity.Attr("music"), entity.Bool("hideVanillaTimer"), entity.Int("requiredTime"), entity.Bool("showTime"), entity.Int("requiredStrawberries"), entity.Bool("showStrawberries"), entity.Attr("strawberriesColor"), entity.Attr("strawberriesMaxColor"), entity.Int("requiredItemPercent"), entity.Bool("showItemPercent"), entity.Attr("itemPercentColor"), entity.Attr("itemPercentMaxColor"), entity.Int("requiredMapPercent"), entity.Bool("showMapPercent"), entity.Attr("mapPercentColor"), entity.Attr("mapPercentMaxColor"), entity.Attr("requiredFlags"), entity.Attr("requirementsCheck"), entity.Int("priority")));
                    }
                }
            }
            CustomEndScreenControllerData.Sort();
        }

        private static bool CheckFlags(Session session, string flags)
        {
            bool allFlagsTrue = true;
            string[] flagsToCheck = flags.Split(',');
            foreach (string flag in flagsToCheck)
            {
                if (!session.GetFlag(flag))
                {
                    allFlagsTrue = false;
                    break;
                }
            }
            return allFlagsTrue;
        }

        private static bool CheckTime(string requirementsCheck, Session session, long requiredTime)
        {
            long time = 0;
            if (requirementsCheck == "Chapter" || XaphanModule.useMergeChaptersController)
            {
                time = session.Time;
            }
            else
            {
                LevelSetStats stats = SaveData.Instance.LevelSetStats;
                foreach (AreaStats areaStat in stats.Areas)
                {
                    time += areaStat.TotalTimePlayed;
                }
            }
            return (time / 10000) <= requiredTime * 1000;
        }

        private static string GetTime(string requirementsCheck, Session session)
        {
            long time = 0;
            if (requirementsCheck == "Chapter")
            {
                time = session.Time;
            }
            else
            {
                LevelSetStats stats = SaveData.Instance.LevelSetStats;
                foreach (AreaStats areaStat in stats.Areas)
                {
                    time += areaStat.TotalTimePlayed;
                }
            }
            return TimeSpan.FromTicks(session.Time).ShortGameplayFormat();
        }

        public static int GetStrawberries(string requirementsCheck, Session session)
        {
            int Strawberries = 0;
            if (requirementsCheck == "Chapter")
            {
                int chapterIndex = session.Area.ChapterIndex == -1 ? 0 : session.Area.ChapterIndex;
                Strawberries = StatsFlags.CurrentStrawberries[chapterIndex];
            }
            else
            {
                foreach (int strawberries in StatsFlags.CurrentStrawberries)
                {
                    Strawberries += strawberries;
                }
            }
            return Strawberries;
        }

        public static int GetMaxStrawberries(string requirementsCheck, Session session)
        {
            int maxStrawberries = 0;
            if (requirementsCheck == "Chapter")
            {
                int chapterIndex = session.Area.ChapterIndex == -1 ? 0 : session.Area.ChapterIndex;
                maxStrawberries = StatsFlags.TotalStrawberries[chapterIndex];
            }
            else
            {
                foreach (int strawberries in StatsFlags.TotalStrawberries)
                {
                    maxStrawberries += strawberries;
                }
            }
            return maxStrawberries;
        }

        public static float GetItemsPercent(string requirementsCheck, Session session)
        {
            int CurrentStrawberries = 0;
            int TotalStrawberries = 0;
            int CurrentHearts = 0;
            int CurrentCassettes = 0;
            int TotalHearts = 0;
            int TotalCassettes = 0;
            if (requirementsCheck == "Chapter")
            {
                int chapterIndex = session.Area.ChapterIndex == -1 ? 0 : session.Area.ChapterIndex;
                CurrentStrawberries = StatsFlags.CurrentStrawberries[chapterIndex];
                TotalStrawberries = StatsFlags.TotalStrawberries[chapterIndex];
                CurrentHearts = session.HeartGem ? 1 : 0;
                CurrentCassettes = session.Cassette ? 1 : 0;
                TotalHearts = AreaData.Areas[session.Area.ID].Mode[0].MapData.DetectedHeartGem ? 1 : 0;
                TotalCassettes = AreaData.Areas[session.Area.ID].Mode[0].MapData.DetectedCassette ? 1 : 0;
            }
            else
            {
                foreach (int currentStrawberries in StatsFlags.CurrentStrawberries)
                {
                    CurrentStrawberries += currentStrawberries;
                }
                foreach (int totalStrawberries in StatsFlags.TotalStrawberries)
                {
                    TotalStrawberries += totalStrawberries;
                }
                CurrentHearts = StatsFlags.heartCount;
                CurrentCassettes = StatsFlags.cassetteCount;
                TotalHearts = StatsFlags.TotalASideHearts;
                TotalCassettes = SaveData.Instance.GetLevelSetStatsFor(SaveData.Instance.LevelSet).MaxCassettes;
            }
            if ((TotalStrawberries + TotalHearts + TotalCassettes) != 0)
            {
                return (CurrentStrawberries + CurrentHearts + CurrentCassettes) * 100 / (TotalStrawberries + TotalHearts + TotalCassettes);
            }
            else
            {
                return 0;
            }
        }

        public static float GetMapPercent(string requirementsCheck, Session session)
        {
            int CurrentTiles = 0;
            int TotalTiles = 0;
            if (requirementsCheck == "Chapter")
            {
                int chapterIndex = session.Area.ChapterIndex == -1 ? 0 : session.Area.ChapterIndex;
                CurrentTiles = StatsFlags.CurrentTiles[chapterIndex];
                TotalTiles = StatsFlags.TotalTiles[chapterIndex];
            }
            else
            {
                foreach (int currentTiles in StatsFlags.CurrentTiles)
                {
                    CurrentTiles += currentTiles;
                }
                foreach (int totalTiles in StatsFlags.TotalTiles)
                {
                    TotalTiles += totalTiles;
                }
            }
            if (TotalTiles != 0)
            {
                return CurrentTiles * 100 / TotalTiles;
            }
            else
            {
                return 0;
            }
        }
    }
}
