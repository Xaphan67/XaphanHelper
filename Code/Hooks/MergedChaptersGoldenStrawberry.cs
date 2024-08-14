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

        public static int StartChapter = -999;

        public static string StartRoom;

        public static Vector2? StartSpawn;

        private static FieldInfo Strawberry_wiggler = typeof(Strawberry).GetField("wiggler", BindingFlags.Instance | BindingFlags.NonPublic);

        public static void Load()
        {
            On.Celeste.Strawberry.OnPlayer += onStrawberryOnPlayer;
            On.Celeste.Strawberry.CollectRoutine += onStrawberryCollectRoutine;
            On.Celeste.Player.Die += onPlayerDie;
            On.Celeste.PlayerDeadBody.End += onPlayerDeadBodyEnd;
            Everest.Events.Level.OnExit += onLevelExit;
            On.Celeste.Level.RegisterAreaComplete += onLevelRegisterAreaComplete;
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

        private static IEnumerator onStrawberryCollectRoutine(On.Celeste.Strawberry.orig_CollectRoutine orig, Strawberry self, int collectIndex)
        {
            if (self.Golden && XaphanModule.useMergeChaptersController)
            {
                string Prefix = self.SceneAs<Level>().Session.Area.LevelSet;
                XaphanModule.ModSaveData.SavedFlags.Add(Prefix + "_GoldenStrawberryGet");
                self.SceneAs<Level>().Session.Time += XaphanModule.ModSaveData.PreGoldenTimer;
            }
            yield return new SwapImmediately(orig(self, collectIndex));
        }

        public static void Unload()
        {
            On.Celeste.Strawberry.OnPlayer -= onStrawberryOnPlayer;
            On.Celeste.Player.Die -= onPlayerDie;
            On.Celeste.PlayerDeadBody.End -= onPlayerDeadBodyEnd;
            Everest.Events.Level.OnExit -= onLevelExit;
            On.Celeste.Level.RegisterAreaComplete -= onLevelRegisterAreaComplete;
        }

        private static void onStrawberryOnPlayer(On.Celeste.Strawberry.orig_OnPlayer orig, Strawberry self, Player player)
        {
            Level level = player.SceneAs<Level>();
            if (XaphanModule.useMergeChaptersController && self.Golden && !Grabbed)
            {
                Grabbed = true;
                ResetFlags = true;
                StartChapter = level.Session.Area.ChapterIndex == -1 ? 0 : level.Session.Area.ChapterIndex;
                StartRoom = level.Session.Level;
                StartSpawn = level.Session.RespawnPoint - new Vector2(level.Bounds.Left, level.Bounds.Top);
                ID = self.ID.ID;
                XaphanModule.ModSaveData.GoldenStrawberryUnlockedWarps.Clear();
                if (XaphanModule.useUpgrades)
                {
                    XaphanModule.ModSaveData.GoldenStrawberryStaminaUpgrades.Clear();
                    XaphanModule.ModSaveData.GoldenStrawberryDroneMissilesUpgrades.Clear();
                    XaphanModule.ModSaveData.GoldenStrawberryDroneSuperMissilesUpgrades.Clear();
                    XaphanModule.ModSaveData.GoldenStrawberryDroneFireRateUpgrades.Clear();
                }
                XaphanModule.ModSaveData.PreGoldenTimer = level.Session.Time;
                XaphanModule.ModSaveData.PreGoldenDoNotLoad.Clear();
                foreach (EntityID entity in level.Session.DoNotLoad)
                {
                    XaphanModule.ModSaveData.PreGoldenDoNotLoad.Add(entity);
                }
                level.Session.Time = 0;
                level.Session.DoNotLoad.Clear();
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
            orig(self, player);
        }

        private static PlayerDeadBody onPlayerDie(On.Celeste.Player.orig_Die orig, Player self, Vector2 direction, bool evenIfInvincible, bool registerDeathInStats)
        {
            if (evenIfInvincible)
            {
                Grabbed = false;
                ResetProgression(self.SceneAs<Level>());
            }
            return orig(self, direction, evenIfInvincible, registerDeathInStats);
        }

        public static void ResetProgression(Level level, bool fullReset = false)
        {
            List<string> ToRemove = new();
            foreach (string flag in XaphanModule.ModSaveData.SavedFlags)
            {
                if (flag.Contains(level.Session.Area.LevelSet) && flag.Contains("_GoldenStrawberry"))
                {
                    ToRemove.Add(flag);
                }
            }
            foreach (string flag in ToRemove)
            {
                XaphanModule.ModSaveData.SavedFlags.Remove(flag);
            }
            if (level.Session.Area.LevelSet == "Xaphan/0")
            {
                level.Session.SetFlag("SoCM-CarryGolden", false);
            }
            if (fullReset)
            {
                level.Session.GrabbedGolden = false;
                XaphanModule.PlayerHasGolden = false;
                Grabbed = false;
                StartChapter = -999;
                StartRoom = "";
                StartSpawn = Vector2.Zero;
                level.Session.Time += XaphanModule.ModSaveData.PreGoldenTimer;
                foreach (EntityID entity in level.Session.DoNotLoad)
                {
                    XaphanModule.ModSaveData.PreGoldenDoNotLoad.Add(entity);
                }
                foreach (EntityID entity in XaphanModule.ModSaveData.PreGoldenDoNotLoad)
                {
                    level.Session.DoNotLoad.Add(entity);
                }
                XaphanModule.ModSaveData.PreGoldenDoNotLoad.Clear();
                XaphanModule.ModSaveData.PreGoldenTimer = 0;
            }
        }

        private static void onPlayerDeadBodyEnd(On.Celeste.PlayerDeadBody.orig_End orig, PlayerDeadBody self)
        {
            Level level = self.SceneAs<Level>();
            if (XaphanModule.useMergeChaptersController && StartChapter != -999)
            {
                if (level.Session.Area.ChapterIndex != StartChapter || (level.Session.Area.ChapterIndex == StartChapter && level.Session.Level != StartRoom))
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
                        int chapterOffset = StartChapter - currentChapter;
                        int currentChapterID = area.ID;

                        XaphanModule.PlayerHasGolden = false;
                        Grabbed = false;
                        StartChapter = -999;
                        StartRoom = "";
                        StartSpawn = Vector2.Zero;

                        foreach (EntityID entity in level.Session.DoNotLoad)
                        {
                            XaphanModule.ModSaveData.PreGoldenDoNotLoad.Add(entity);
                        }
                        LevelEnter.Go(new Session(new AreaKey(currentChapterID + chapterOffset))
                        {
                            Time = XaphanModule.ModSaveData.PreGoldenTimer + level.Session.Time,
                            DoNotLoad = XaphanModule.ModSaveData.PreGoldenDoNotLoad,
                            Strawberries = XaphanModule.ModSaveData.SavedSessionStrawberries.ContainsKey(level.Session.Area.LevelSet) ? XaphanModule.ModSaveData.SavedSessionStrawberries[level.Session.Area.LevelSet] : new HashSet<EntityID>()
                        }, fromSaveData: false);
                        XaphanModule.ModSaveData.PreGoldenTimer = 0;
                    };
                }
            }
            orig(self);
        }

        private static void onLevelExit(Level level, LevelExit exit, LevelExit.Mode mode, Session session, HiresSnow snow)
        {
            Grabbed = false;
            StartChapter = -999;
            StartRoom = "";
            StartSpawn = Vector2.Zero;
        }
    }
}
