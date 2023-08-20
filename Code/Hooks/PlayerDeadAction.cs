using Celeste.Mod.XaphanHelper.Entities;
using Monocle;

namespace Celeste.Mod.XaphanHelper.Hooks
{
    internal class PlayerDeadAction
    {
        public static void Load()
        {
            On.Celeste.PlayerDeadBody.End += onPlayerDeaDBodyEnd;
        }

        public static void Unload()
        {
            On.Celeste.PlayerDeadBody.End -= onPlayerDeaDBodyEnd;
        }

        private static void onPlayerDeaDBodyEnd(On.Celeste.PlayerDeadBody.orig_End orig, PlayerDeadBody self)
        {
            if ((self.SceneAs<Level>().Tracker.GetEntities<FlagDashSwitch>().Count > 0 || self.SceneAs<Level>().Tracker.GetEntities<DroneSwitch>().Count > 0) && !self.SceneAs<Level>().Session.GrabbedGolden)
            {
                self.DeathAction = DeathAction;
            }
            XaphanModule.ModSession.NoRespawnIds.Clear();
            orig(self);
        }

        public static void DeathAction()
        {
            if (Engine.Scene is Level)
            {
                Level level = (Level)Engine.Scene;
                int chapterIndex = level.Session.Area.ChapterIndex;
                foreach (FlagDashSwitch flagSwitch in level.Tracker.GetEntities<FlagDashSwitch>())
                {
                    level.Session.SetFlag("Ch" + chapterIndex + "_" + flagSwitch.flag + "_true", false);
                    level.Session.SetFlag("Ch" + chapterIndex + "_" + flagSwitch.flag + "_false", false);
                    if (!flagSwitch.persistent && !flagSwitch.FlagRegiseredInSaveData() && flagSwitch.startSpawnPoint == level.Session.RespawnPoint)
                    {
                        if (flagSwitch.flagState)
                        {
                            level.Session.SetFlag(flagSwitch.flag, true);
                        }
                        else
                        {
                            level.Session.SetFlag(flagSwitch.flag, false);
                        }
                    }
                }
                level.Reload();
            }
        }
    }
}
