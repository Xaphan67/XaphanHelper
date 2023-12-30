using System;
using System.Collections.Generic;
using Celeste.Mod.XaphanHelper.Cutscenes;
using Celeste.Mod.XaphanHelper.UI_Elements;
using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste.Mod.XaphanHelper.Managers
{
    public static class WarpManager
    {
        public static bool IsUnlocked(string warpId) => UnlockedWarps.Contains(warpId);
        public static bool ActivateWarp(string warpId) => UnlockedWarps.Add(warpId);
        public static bool DeactivateWarp(string warpId) => UnlockedWarps.Remove(warpId);

        private static bool UseTempWarps => XaphanModule.PlayerHasGolden;
        private static HashSet<string> UnlockedWarps => !UseTempWarps ? XaphanModule.ModSaveData.UnlockedWarps : XaphanModule.ModSaveData.GoldenStrawberryUnlockedWarps;

        public static string GetWarpId(Level level, int warpIndex)
        {
            string levelSet = level.Session.Area.LevelSet;
            int chapterIndex = level.Session.Area.ChapterIndex == -1 ? 0 : level.Session.Area.ChapterIndex;
            MapData mapData = AreaData.Areas[level.Session.Area.ID].Mode[0].MapData;
            string room = level.Session.Level;
            string warpSuffix = warpIndex != 0 ? "_" + warpIndex : "";
            return $"{levelSet}_Ch{chapterIndex}_{room}{warpSuffix}";
        }

        public static List<WarpInfo> GetWarpTargets(int areaId)
        {
            List<WarpInfo> warps = new();
            List<string> invalidWarps = new();

            AreaKey area = new(areaId);
            int index = area.ChapterIndex == -1 ? 0 : area.ChapterIndex;
            MapData mapData = AreaData.Areas[areaId].Mode[0].MapData;
            string start = $"{area.LevelSet}_Ch{index}_";

            foreach (string warpId in UnlockedWarps)
            {
                // Warp name format: $"{levelSet}_Ch{chapterIndex}_{room}{warpSuffix}, where the warp suffix is optional
                // With the current warp naming scheme, we can't easily tell if there's a warp suffix or if the room name just has an underscore in it.
                // So, we assume there is none unless the room name can't be found.
                if (warpId.StartsWith(start))
                {
                    int warpIndex = 0;
                    string room = warpId.Substring(start.Length);
                    bool roomFound = mapData.Get(room) != null;

                    if (!roomFound)
                    {
                        int i = room.LastIndexOf('_');
                        if (i != -1 && i < room.Length - 1)
                        {
                            warpIndex = int.Parse(room.Substring(i + 1));
                            room = room.Substring(0, i);
                            roomFound = mapData.Get(room) != null;
                        }
                    }

                    if (!roomFound)
                    {
                        Logger.Log(LogLevel.Warn, "XaphanHelper/WarpManager", $"Could not find a matching room for {warpId}, removing...");
                        invalidWarps.Add(warpId);
                        continue;
                    }

                    bool warpFound = false;
                    foreach (EntityData warpStation in mapData.Get(room).GetEntityDatas("XaphanHelper/WarpStation"))
                    {
                        if (warpStation.Int("index") == warpIndex)
                        {
                            string dialogKey = $"{area.LevelSet}_Warp_{warpId.Substring(area.LevelSet.Length + 1)}";
                            warps.Add(new WarpInfo(warpId, dialogKey, areaId, room, warpStation.Position));
                            warpFound = true;
                            break;
                        }
                    }

                    if (!warpFound)
                    {
                        Logger.Log(LogLevel.Warn, "XaphanHelper/WarpManager", $"Could not find a matching warp station for {warpId}, removing...");
                        invalidWarps.Add(warpId);
                    }
                }
            }

            foreach (string warp in invalidWarps)
            {
                UnlockedWarps.Remove(warp);
            }

            warps.Sort((warpA, warpB) => warpA.ID.CompareTo(warpB.ID));
            return warps;
        }

        public static void Teleport(WarpInfo warp, string wipeType, float wipeDuration)
        {
            if (Engine.Scene is Level level && level.Tracker.GetEntity<Player>() is Player player)
            {
                int currentAreaId = level.Session.Area.ID;
                if (warp.AreaId == currentAreaId)
                {
                    MapData mapData = AreaData.Areas[level.Session.Area.ID].Mode[0].MapData;
                    level.Add(new TeleportCutscene(player, warp.Room, warp.Position, 0, 0, true, 0f, wipeType, wipeDuration));
                }
                else
                {
                    XaphanModule.ModSaveData.DestinationRoom = warp.Room;
                    XaphanModule.ModSaveData.Spawn = warp.Position;
                    XaphanModule.ModSaveData.Wipe = wipeType;
                    XaphanModule.ModSaveData.WipeDuration = wipeDuration;

                    ScreenWipe wipe = null;
                    if (typeof(Celeste).Assembly.GetType($"Celeste.{wipeType}Wipe") is Type type)
                    {
                        wipe = (ScreenWipe)Activator.CreateInstance(type, new object[] {
                            level, false, new Action(() => TeleportToChapter(warp.AreaId))
                        });
                    }
                    else
                    {
                        wipe = new FadeWipe(level, false, new Action(() => TeleportToChapter(warp.AreaId)));
                    }

                    wipe.Duration = Math.Min(1.35f, wipeDuration);
                }
            }
        }

        private static void TeleportToChapter(int areaId)
        {
            if (Engine.Scene is Level level)
            {
                XaphanModule.ShowUI = false;
                if (level.Tracker.GetEntity<CountdownDisplay>() is CountdownDisplay timerDisplay && timerDisplay.SaveTimer)
                {
                    AreaKey area = level.Session.Area;
                    XaphanModule.ModSaveData.CountdownCurrentTime = timerDisplay.PausedTimer;
                    XaphanModule.ModSaveData.CountdownShake = timerDisplay.Shake;
                    XaphanModule.ModSaveData.CountdownExplode = timerDisplay.Explode;
                    if (XaphanModule.ModSaveData.CountdownStartChapter == -1)
                    {
                        XaphanModule.ModSaveData.CountdownStartChapter = area.ChapterIndex == -1 ? 0 : area.ChapterIndex;
                    }
                    XaphanModule.ModSaveData.CountdownStartRoom = timerDisplay.startRoom;
                    XaphanModule.ModSaveData.CountdownSpawn = timerDisplay.SpawnPosition;
                    XaphanModule.ModSaveData.CountdownHideFlag = timerDisplay.hideFlag;
                    XaphanModule.ModSaveData.CountdownEventsFlags = timerDisplay.rawEventsFlags;
                }
                if (XaphanModule.useMergeChaptersController)
                {
                    long currentTime = level.Session.Time;
                    LevelEnter.Go(new Session(new AreaKey(areaId))
                    {
                        Time = currentTime,
                        DoNotLoad = XaphanModule.ModSaveData.SavedNoLoadEntities[level.Session.Area.LevelSet],
                        Strawberries = XaphanModule.ModSaveData.SavedSessionStrawberries[level.Session.Area.LevelSet]
                    }
                    , fromSaveData: false);
                }
                else
                {
                    LevelEnter.Go(new Session(new AreaKey(areaId)), fromSaveData: false);
                }
            }
        }
    }

    public struct WarpInfo
    {
        public string ID;
        public string DialogKey;
        public int AreaId;
        public string Room;
        public Vector2 Position;

        public WarpInfo(string id, string dialogKey, int areaId, string room, Vector2 position)
        {
            ID = id;
            DialogKey = dialogKey;
            AreaId = areaId;
            Room = room;
            Position = position;
        }
    }
}
