using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Reflection;
using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste.Mod.XaphanHelper.Hooks
{
    internal class MergedChaptersGoldenStrawberry
    {
        public static bool ResetFlags;

        public static bool Grabbed;

        public static int ID;

        public static string StartRoom;

        public static Vector2? StartSpawn;

        private static FieldInfo Strawberry_wiggler = typeof(Strawberry).GetField("wiggler", BindingFlags.Instance | BindingFlags.NonPublic);

        public static void Load()
        {
            On.Celeste.Strawberry.OnPlayer += onStrawberryOnPlayer;
            On.Celeste.Strawberry.CollectRoutine += onStrawberryCollectRoutine;
            On.Celeste.Strawberry.Update += onStrawberryUpdate;
            On.Celeste.Player.Die += onPlayerDie;
            On.Celeste.PlayerDeadBody.End += onPlayerDeadBodyEnd;
            Everest.Events.Level.OnExit += onLevelExit;
            On.Celeste.Level.Update += onLevelUpdate;
            On.Celeste.Level.RegisterAreaComplete += onLevelRegisterAreaComplete;
        }

        private static void onLevelUpdate(On.Celeste.Level.orig_Update orig, Level self)
        {
            orig(self);
            if (!XaphanModule.PlayerHasGolden)
            {
                // Restaure timer from aborted golden attempt
                if (XaphanModule.ModSaveData.PreGoldenTimer != 0 && XaphanModule.ModSaveData.GoldenStartChapter != -999)
                {
                    int curentChapter = self.Session.Area.ChapterIndex == -1 ? 0 : self.Session.Area.ChapterIndex;
                    if (curentChapter == XaphanModule.ModSaveData.GoldenStartChapter)
                    {
                        self.Session.Time += XaphanModule.ModSaveData.PreGoldenTimer;
                        XaphanModule.ModSaveData.PreGoldenTimer = 0;
                        XaphanModule.ModSaveData.GoldenStartChapter = -999;
                    }
                }

                // Restaure no load entities from aborted golden attempt
                if (XaphanModule.ModSaveData.PreGoldenDoNotLoad.Count > 0)
                {
                    foreach (EntityID entity in XaphanModule.ModSaveData.PreGoldenDoNotLoad)
                    {
                        XaphanModule.ModSaveData.SavedNoLoadEntities[self.Session.Area.LevelSet].Add(entity);
                    }
                    foreach (EntityID entity in XaphanModule.ModSaveData.SavedNoLoadEntities[self.Session.Area.LevelSet])
                    {
                        self.Session.DoNotLoad.Add(entity);
                    }
                    XaphanModule.ModSaveData.PreGoldenDoNotLoad.Clear();
                }

                // Restaure flags from aborted golden attempt
                if (XaphanModule.ModSaveData.PreGoldenFlags.Count > 0)
                {
                    foreach (string flag in self.Session.Flags)
                    {
                        if (!flag.Contains("XaphanHelper_") && flag != "SoCM_startedGame")
                        {
                            XaphanModule.ModSaveData.PreGoldenFlags.Add(flag);
                        }
                    }
                    foreach (string flag in XaphanModule.ModSaveData.PreGoldenFlags)
                    {
                        self.Session.SetFlag(flag, true);
                    }
                    XaphanModule.ModSaveData.PreGoldenFlags.Clear();
                }

                // Restaure saved flags from aborted golden attempt
                if (XaphanModule.ModSaveData.PreGoldenSavedFlags.Count > 0)
                {
                    foreach (string flag in XaphanModule.ModSaveData.SavedFlags)
                    {
                        if (flag.Contains(self.Session.Area.LevelSet) && flag != self.Session.Area.LevelSet + "_Can_Open_Map" && !flag.Contains("_MapShard"))
                        {
                            XaphanModule.ModSaveData.PreGoldenSavedFlags.Add(flag);
                        }
                    }
                    foreach (string flag in XaphanModule.ModSaveData.PreGoldenSavedFlags)
                    {
                        XaphanModule.ModSaveData.SavedFlags.Add(flag);
                    }
                    XaphanModule.ModSaveData.PreGoldenSavedFlags.Clear();
                }

                // Restaure global flags from aborted golden attempt
                if (XaphanModule.ModSaveData.PreGoldenGlobalFlags.Count > 0)
                {
                    foreach (string flag in XaphanModule.ModSaveData.GlobalFlags)
                    {
                        if (flag.Contains(self.Session.Area.LevelSet))
                        {
                            XaphanModule.ModSaveData.PreGoldenGlobalFlags.Add(flag);
                        }
                    }
                    foreach (string flag in XaphanModule.ModSaveData.PreGoldenGlobalFlags)
                    {
                        XaphanModule.ModSaveData.GlobalFlags.Add(flag);
                    }
                    XaphanModule.ModSaveData.PreGoldenGlobalFlags.Clear();
                }
            }
        }

        public static void Unload()
        {
            On.Celeste.Strawberry.OnPlayer -= onStrawberryOnPlayer;
            On.Celeste.Strawberry.CollectRoutine -= onStrawberryCollectRoutine;
            On.Celeste.Strawberry.Update -= onStrawberryUpdate;
            On.Celeste.Player.Die -= onPlayerDie;
            On.Celeste.PlayerDeadBody.End -= onPlayerDeadBodyEnd;
            Everest.Events.Level.OnExit -= onLevelExit;
            On.Celeste.Level.Update -= onLevelUpdate;
            On.Celeste.Level.RegisterAreaComplete -= onLevelRegisterAreaComplete;
        }

        private static void onStrawberryOnPlayer(On.Celeste.Strawberry.orig_OnPlayer orig, Strawberry self, Player player)
        {
            Level level = player.SceneAs<Level>();
            if (level.Session.Area.Mode == AreaMode.Normal)
            {
                if (self.Golden)
                {
                    if (XaphanModule.useMergeChaptersController && !Grabbed)
                    {
                        Grabbed = true;
                        XaphanModule.ModSaveData.GoldenStartChapter = level.Session.Area.ChapterIndex == -1 ? 0 : level.Session.Area.ChapterIndex;
                        StartRoom = level.Session.Level;
                        StartSpawn = level.Session.RespawnPoint - new Vector2(level.Bounds.Left, level.Bounds.Top);
                        ID = self.ID.ID;
                        XaphanModule.ModSaveData.GoldenStrawberryUnlockedWarps.Clear();
                        if (self.SceneAs<Level>().Session.Area.LevelSet == "Xaphan/0")
                        {
                            XaphanModule.ModSaveData.GoldenStrawberryUnlockedWarps.Add($"{self.SceneAs<Level>().Session.Area.LevelSet}_Ch0_A-W0");
                        }
                        if (XaphanModule.useUpgrades)
                        {
                            XaphanModule.ModSaveData.GoldenStrawberryStaminaUpgrades.Clear();
                            XaphanModule.ModSaveData.GoldenStrawberryDroneMissilesUpgrades.Clear();
                            XaphanModule.ModSaveData.GoldenStrawberryDroneSuperMissilesUpgrades.Clear();
                            XaphanModule.ModSaveData.GoldenStrawberryDroneFireRateUpgrades.Clear();
                        }
                        XaphanModule.ModSaveData.PreGoldenTimer = level.Session.Time;
                        if (XaphanModule.ModSaveData.SavedNoLoadEntities.ContainsKey(level.Session.Area.LevelSet))
                        {
                            foreach (EntityID entity in XaphanModule.ModSaveData.SavedNoLoadEntities[level.Session.Area.LevelSet])
                            {
                                XaphanModule.ModSaveData.PreGoldenDoNotLoad.Add(entity);
                            }
                        }
                        foreach (string flag in level.Session.Flags)
                        {
                            if (!flag.Contains("XaphanHelper_") && flag != "SoCM_startedGame")
                            {
                                XaphanModule.ModSaveData.PreGoldenFlags.Add(flag);
                            }
                        }
                        foreach (string flag in XaphanModule.ModSaveData.SavedFlags)
                        {
                            if (flag.Contains(level.Session.Area.LevelSet) && flag != level.Session.Area.LevelSet + "_Can_Open_Map" && !flag.Contains("_MapShard"))
                            {
                                XaphanModule.ModSaveData.PreGoldenSavedFlags.Add(flag);
                            }
                        }
                        foreach (string flag in XaphanModule.ModSaveData.GlobalFlags)
                        {
                            if (flag.Contains(level.Session.Area.LevelSet))
                            {
                                XaphanModule.ModSaveData.PreGoldenGlobalFlags.Add(flag);
                            }
                        }
                        if (XaphanModule.ModSaveData.SavedNoLoadEntities.ContainsKey(level.Session.Area.LevelSet))
                        {
                            XaphanModule.ModSaveData.SavedNoLoadEntities[level.Session.Area.LevelSet].Clear();
                        }
                        level.Session.Time = 0;
                        foreach (string flag in level.Session.Flags)
                        {
                            if (!flag.Contains("XaphanHelper_") && flag != "SoCM_startedGame")
                            {
                                level.Session.SetFlag(flag, false);
                            }
                        }
                        HashSet<string> flagsToRemove = new();
                        foreach (string flag in XaphanModule.ModSaveData.SavedFlags)
                        {
                            if (flag.Contains(level.Session.Area.LevelSet) && flag != level.Session.Area.LevelSet + "_Can_Open_Map" && !flag.Contains("_MapShard"))
                            {
                                flagsToRemove.Add(flag);
                            }
                        }
                        foreach (string flag in flagsToRemove)
                        {
                            XaphanModule.ModSaveData.SavedFlags.Remove(flag);
                        }
                        flagsToRemove.Clear();
                        foreach (string flag in XaphanModule.ModSaveData.GlobalFlags)
                        {
                            if (flag.Contains(level.Session.Area.LevelSet))
                            {
                                flagsToRemove.Add(flag);
                            }
                        }
                        foreach (string flag in flagsToRemove)
                        {
                            XaphanModule.ModSaveData.GlobalFlags.Remove(flag);
                        }
                        ResetFlags = true;
                        Audio.Play(SaveData.Instance.CheckStrawberry(self.ID) ? "event:/game/general/strawberry_blue_touch" : "event:/game/general/strawberry_touch", self.Position);
                    }
                    if (Grabbed)
                    {
                        if (self.Follower.Leader != null)
                        {
                            return;
                        }
                        self.ReturnHomeWhenLost = true;
                        self.SceneAs<Level>().Session.GrabbedGolden = true;
                        player.Leader.GainFollower(self.Follower);
                        Wiggler wiggler = (Wiggler)Strawberry_wiggler.GetValue(self);
                        wiggler.Start();
                        self.Depth = -1000000;
                    }
                    if (self.SceneAs<Level>().Session.Area.LevelSet == "Xaphan/0")
                    {
                        self.SceneAs<Level>().Session.SetFlag("SoCM-CarryGolden", true);
                    }
                }
            }
            orig(self, player);
        }

        private static IEnumerator onStrawberryCollectRoutine(On.Celeste.Strawberry.orig_CollectRoutine orig, Strawberry self, int collectIndex)
        {
            Level level = self.SceneAs<Level>();
            if (level.Session.Area.Mode == AreaMode.Normal)
            {
                if (self.Golden && XaphanModule.useMergeChaptersController)
                {
                    string Prefix = self.SceneAs<Level>().Session.Area.LevelSet;
                    XaphanModule.ModSaveData.SavedFlags.Add(Prefix + "_GoldenStrawberryGet");
                    self.SceneAs<Level>().Session.Time += XaphanModule.ModSaveData.PreGoldenTimer;
                    XaphanModule.ModSaveData.PreGoldenTimer = 0;
                    MergeFlagsAndEntities(self.SceneAs<Level>());
                }
                yield return new SwapImmediately(orig(self, collectIndex));
                if (self.Golden && XaphanModule.useMergeChaptersController)
                {
                    self.SceneAs<Level>().Session.DoNotLoad.Remove(new EntityID(StartRoom, ID));
                }
            }
            else
            {
                yield return new SwapImmediately(orig(self, collectIndex));
            }
        }

        public static void onStrawberryUpdate(On.Celeste.Strawberry.orig_Update orig, Strawberry self)
        {
            if (XaphanModule.useMergeChaptersController && self.Golden && self.Follower.Leader == null && self.GetType().ToString() == "Celeste.Strawberry")
            {
                if (PlayerHasGolden(self.SceneAs<Level>()))
                {
                    self.RemoveSelf();
                    return;
                }
            }
            orig(self);
        }

        private static bool PlayerHasGolden(Scene scene)
        {
            foreach (Strawberry item in scene.Entities.FindAll<Strawberry>())
            {
                if (item.Golden && item.Follower.Leader != null && item.GetType().ToString() == "Celeste.Strawberry")
                {
                    return true;
                }
            }
            return false;
        }

        private static PlayerDeadBody onPlayerDie(On.Celeste.Player.orig_Die orig, Player self, Vector2 direction, bool evenIfInvincible, bool registerDeathInStats)
        {
            if (evenIfInvincible && self.SceneAs<Level>().Session.Area.Mode == 0)
            {
                Grabbed = false;
            }
            return orig(self, direction, evenIfInvincible, registerDeathInStats);
        }

        public static void ResetProgression(Level level)
        {
            List<string> ToRemove = new();
            if (level.Session.Area.LevelSet == "Xaphan/0")
            {
                level.Session.SetFlag("SoCM-CarryGolden", false);
            }
            MergeFlagsAndEntities(level);
            level.Session.GrabbedGolden = false;
            XaphanModule.PlayerHasGolden = false;
            Grabbed = false;
            XaphanModule.ModSaveData.GoldenStartChapter = -999;
            StartRoom = "";
            StartSpawn = Vector2.Zero;
            level.Session.Time += XaphanModule.ModSaveData.PreGoldenTimer;
            XaphanModule.ModSaveData.PreGoldenTimer = 0;
        }

        private static void onPlayerDeadBodyEnd(On.Celeste.PlayerDeadBody.orig_End orig, PlayerDeadBody self)
        {
            Level level = self.SceneAs<Level>();
            if (level.Session.Area.Mode == AreaMode.Normal)
            {
                if (XaphanModule.useMergeChaptersController && XaphanModule.ModSaveData.GoldenStartChapter != -999)
                {
                    if (level.Session.Area.ChapterIndex != XaphanModule.ModSaveData.GoldenStartChapter || (level.Session.Area.ChapterIndex == XaphanModule.ModSaveData.GoldenStartChapter && level.Session.Level != StartRoom))
                    {
                        self.DeathAction = delegate
                        {
                            level.Session.GrabbedGolden = false;
                            AreaKey area = level.Session.Area;
                            int currentChapter = area.ChapterIndex == -1 ? 0 : area.ChapterIndex;
                            XaphanModule.ModSaveData.DestinationRoom = StartRoom;
                            XaphanModule.ModSaveData.Spawn = (Vector2)StartSpawn;
                            XaphanModule.ModSaveData.Wipe = "Fade";
                            XaphanModule.ModSaveData.WipeDuration = 0.35f;
                            int chapterOffset = XaphanModule.ModSaveData.GoldenStartChapter - currentChapter;
                            int currentChapterID = area.ID;

                            XaphanModule.PlayerHasGolden = false;
                            Grabbed = false;
                            XaphanModule.ModSaveData.GoldenStartChapter = -999;
                            StartRoom = "";
                            StartSpawn = Vector2.Zero;
                            MergeFlagsAndEntities(level);
                            LevelEnter.Go(new Session(new AreaKey(currentChapterID + chapterOffset))
                            {
                                Time = XaphanModule.ModSaveData.PreGoldenTimer + level.Session.Time,
                                DoNotLoad = XaphanModule.ModSaveData.SavedNoLoadEntities.ContainsKey(level.Session.Area.LevelSet) ? XaphanModule.ModSaveData.SavedNoLoadEntities[level.Session.Area.LevelSet] : new HashSet<EntityID>(),
                                Strawberries = XaphanModule.ModSaveData.SavedSessionStrawberries.ContainsKey(level.Session.Area.LevelSet) ? XaphanModule.ModSaveData.SavedSessionStrawberries[level.Session.Area.LevelSet] : new HashSet<EntityID>()
                            }, fromSaveData: false);
                            XaphanModule.ModSaveData.PreGoldenTimer = 0;
                        };
                    }
                }
            }
            orig(self);
        }

        private static void onLevelExit(Level level, LevelExit exit, LevelExit.Mode mode, Session session, HiresSnow snow)
        {
            Grabbed = false;
            XaphanModule.ModSaveData.GoldenStartChapter = -999;
            StartRoom = "";
            StartSpawn = Vector2.Zero;
        }

        private static void onLevelRegisterAreaComplete(On.Celeste.Level.orig_RegisterAreaComplete orig, Level self)
        {
            if (XaphanModule.useMergeChaptersController)
            {
                if (self.Completed)
                {
                    return;
                }
                Player entity = self.Tracker.GetEntity<Player>();
                if (entity != null)
                {
                    List<IStrawberry> list = new();
                    ReadOnlyCollection<Type> berryTypes = StrawberryRegistry.GetBerryTypes();
                    foreach (Follower follower in entity.Leader.Followers)
                    {
                        if (berryTypes.Contains(follower.Entity.GetType()) && follower.Entity is IStrawberry)
                        {
                            bool skip = false;
                            if (follower.Entity is Strawberry)
                            {
                                Strawberry berry = (Strawberry)follower.Entity;
                                if (berry.Golden)
                                {
                                    skip = true;
                                }
                            }
                            if (!skip)
                            {
                                list.Add(follower.Entity as IStrawberry);
                            }
                        }
                    }
                    foreach (IStrawberry item in list)
                    {
                        item.OnCollect();
                    }
                }
                self.Completed = true;
                SaveData.Instance.RegisterCompletion(self.Session);
            }
            else
            {
                orig(self);
            }
        }

        public static void MergeFlagsAndEntities(Level level)
        {
            if (XaphanModule.ModSaveData.SavedNoLoadEntities.ContainsKey(level.Session.Area.LevelSet))
            {
                foreach (EntityID entity in XaphanModule.ModSaveData.PreGoldenDoNotLoad)
                {
                    XaphanModule.ModSaveData.SavedNoLoadEntities[level.Session.Area.LevelSet].Add(entity);
                }
                foreach (EntityID entity in XaphanModule.ModSaveData.SavedNoLoadEntities[level.Session.Area.LevelSet])
                {
                    level.Session.DoNotLoad.Add(entity);
                }
            }

            foreach (string flag in level.Session.Flags)
            {
                if (!flag.Contains("XaphanHelper_") && flag != "SoCM_startedGame")
                {
                    XaphanModule.ModSaveData.PreGoldenFlags.Add(flag);
                }
            }
            foreach (string flag in XaphanModule.ModSaveData.PreGoldenFlags)
            {
                level.Session.SetFlag(flag, true);
            }

            foreach (string flag in XaphanModule.ModSaveData.SavedFlags)
            {
                if (flag.Contains(level.Session.Area.LevelSet) && flag != level.Session.Area.LevelSet + "_Can_Open_Map" && !flag.Contains("_MapShard"))
                {
                    XaphanModule.ModSaveData.PreGoldenSavedFlags.Add(flag);
                }
            }
            foreach (string flag in XaphanModule.ModSaveData.PreGoldenSavedFlags)
            {
                XaphanModule.ModSaveData.SavedFlags.Add(flag);
            }

            foreach (string flag in XaphanModule.ModSaveData.GlobalFlags)
            {
                if (flag.Contains(level.Session.Area.LevelSet))
                {
                    XaphanModule.ModSaveData.PreGoldenGlobalFlags.Add(flag);
                }
            }
            foreach (string flag in XaphanModule.ModSaveData.PreGoldenGlobalFlags)
            {
                XaphanModule.ModSaveData.GlobalFlags.Add(flag);
            }
            XaphanModule.ModSaveData.PreGoldenDoNotLoad.Clear();
            XaphanModule.ModSaveData.PreGoldenFlags.Clear();
            XaphanModule.ModSaveData.PreGoldenSavedFlags.Clear();
            XaphanModule.ModSaveData.PreGoldenGlobalFlags.Clear();
        }
    }
}
