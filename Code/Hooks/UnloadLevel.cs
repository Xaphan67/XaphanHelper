using Celeste.Mod.XaphanHelper.Entities;
using Celeste.Mod.XaphanHelper.Managers;

namespace Celeste.Mod.XaphanHelper.Hooks
{
    internal class UnloadLevel
    {
        public static void Load()
        {
            On.Celeste.Level.UnloadLevel += onLevelUnloadLevel;
        }

        public static void Unload()
        {
            On.Celeste.Level.UnloadLevel -= onLevelUnloadLevel;
        }

        private static void onLevelUnloadLevel(On.Celeste.Level.orig_UnloadLevel orig, Level self)
        {
            Player player = self.Tracker.GetEntity<Player>();
            if (self.Session.Area.LevelSet != "Celeste" && (player == null || player.Dead))
            {
                if ((self.Tracker.GetEntities<FlagDashSwitch>().Count > 0 || self.Tracker.GetEntities<Detonator>().Count > 0 || self.Tracker.GetEntities<BombSwitch>().Count > 0 || self.Tracker.GetEntity<LightManager>() != null) && !self.Session.GrabbedGolden)
                {
                    int chapterIndex = self.Session.Area.ChapterIndex;
                    foreach (FlagDashSwitch flagSwitch in self.Tracker.GetEntities<FlagDashSwitch>())
                    {
                        self.Session.SetFlag("Ch" + chapterIndex + "_" + flagSwitch.flag + "_true", false);
                        self.Session.SetFlag("Ch" + chapterIndex + "_" + flagSwitch.flag + "_false", false);
                        if (!flagSwitch.persistent && !flagSwitch.FlagRegiseredInSaveData() && flagSwitch.startSpawnPoint == self.Session.RespawnPoint)
                        {
                            self.Session.SetFlag(flagSwitch.flag, flagSwitch.flagState);
                        }
                    }
                    foreach (Detonator detonator in self.Tracker.GetEntities<Detonator>())
                    {
                        if (!detonator.FlagRegiseredInSaveData())
                        {
                            self.Session.SetFlag(detonator.flag, false);
                        }
                    }
                    foreach (BombSwitch bombSwitch in self.Tracker.GetEntities<BombSwitch>())
                    {
                        self.Session.SetFlag("Ch" + chapterIndex + "_" + bombSwitch.flag + "_true", false);
                        self.Session.SetFlag("Ch" + chapterIndex + "_" + bombSwitch.flag + "_false", false);
                        if (!bombSwitch.FlagRegiseredInSaveData() && bombSwitch.startSpawnPoint == self.Session.RespawnPoint)
                        {
                            self.Session.SetFlag(bombSwitch.flag, bombSwitch.flagState);
                        }
                    }
                    foreach (LightManager manager in self.Tracker.GetEntities<LightManager>())
                    {
                        manager.TemporaryModeTimer = 0f;
                        if (manager.ForceModeRoutine.Active)
                        {
                            manager.ForceModeRoutine.Cancel();
                            manager.TemporaryMode = XaphanModuleSession.LightModes.None;
                        }
                        XaphanModule.ModSession.LightMode = manager.RespawnMode;
                        manager.SetMainMode(manager.RespawnMode);
                    }
                }
                XaphanModule.ModSession.NoRespawnIds.Clear();
            }
            orig(self);
        }
    }
}
