using Monocle;
using Microsoft.Xna.Framework;
using System.Reflection;
using Celeste.Mod.XaphanHelper.Cutscenes;

namespace Celeste.Mod.XaphanHelper.Hooks
{
    internal class MergedChaptersGoldenStrawberry
    {
        public static bool Grabbed;

        public static int ID;

        public static int StartChapter = -999;

        public static string StartRoom;

        public static Vector2? StartSpawn;

        private static FieldInfo Strawberry_wiggler = typeof(Strawberry).GetField("wiggler", BindingFlags.Instance | BindingFlags.NonPublic);

        public static void Load()
        {
            On.Celeste.Strawberry.OnPlayer += onStrawberryOnPlayer;
            On.Celeste.Player.Die += onPlayerDie;
            On.Celeste.PlayerDeadBody.End += onPlayerDeadBodyEnd;
        }

        public static void Unload()
        {
            On.Celeste.Strawberry.OnPlayer -= onStrawberryOnPlayer;
            On.Celeste.Player.Die += onPlayerDie;
            On.Celeste.PlayerDeadBody.End += onPlayerDeadBodyEnd;
        }

        private static void onStrawberryOnPlayer(On.Celeste.Strawberry.orig_OnPlayer orig, Strawberry self, Player player)
        {
            if (XaphanModule.useMergeChaptersController && self.Golden && !Grabbed)
            {
                Grabbed = true;
                Level level = player.SceneAs<Level>();
                StartChapter = level.Session.Area.ChapterIndex == -1 ? 0 : level.Session.Area.ChapterIndex;
                StartRoom = level.Session.Level;
                StartSpawn = level.Session.RespawnPoint - new Vector2(level.Bounds.Left, level.Bounds.Top);
                ID = self.ID.ID;
                XaphanModule.ModSaveData.GoldenStrawberryUnlockedWarps.Clear();
                XaphanModule.ModSaveData.GoldenStrawberryStaminaUpgrades.Clear();
                XaphanModule.ModSaveData.GoldenStrawberryDroneMissilesUpgrades.Clear();
                XaphanModule.ModSaveData.GoldenStrawberryDroneSuperMissilesUpgrades.Clear();
                XaphanModule.ModSaveData.GoldenStrawberryDroneFireRateUpgrades.Clear();
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
            orig(self, player);
        }

        private static PlayerDeadBody onPlayerDie(On.Celeste.Player.orig_Die orig, Player self, Vector2 direction, bool evenIfInvincible, bool registerDeathInStats)
        {
            Grabbed = false;
            return orig(self, direction, evenIfInvincible, registerDeathInStats);
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

                        StartChapter = -999;
                        StartRoom = "";
                        StartSpawn = Vector2.Zero;

                        LevelEnter.Go(new Session(new AreaKey(currentChapterID + chapterOffset)), fromSaveData: false);
                    };
                }
            }
            orig(self);
        }
    }
}
