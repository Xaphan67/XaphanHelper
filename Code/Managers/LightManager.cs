using System.Collections;
using Celeste.Mod.XaphanHelper.Entities;
using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste.Mod.XaphanHelper.Managers
{
    [Tracked(true)]
    class LightManager : Entity
    {
        public XaphanModuleSession.LightModes RespawnMode;

        public XaphanModuleSession.LightModes MainMode;

        public XaphanModuleSession.LightModes TemporaryMode;

        public float TemporaryModeTimer;

        public Coroutine ForceModeRoutine = new();

        public LightManager()
        {
            Tag = Tags.Global | Tags.TransitionUpdate;
            RespawnMode = XaphanModuleSession.LightModes.None;
            MainMode = XaphanModuleSession.LightModes.None;
            TemporaryMode = XaphanModuleSession.LightModes.None;
        }

        public static void Load()
        {
            Everest.Events.Level.OnLoadLevel += modOnLevelLoad;
            Everest.Events.Player.OnDie += modOnPlayerDie;
            On.Celeste.ChangeRespawnTrigger.OnEnter += onChangeRespawnTriggerOnEnter;
        }

        public static void Unload()
        {
            Everest.Events.Level.OnLoadLevel -= modOnLevelLoad;
            Everest.Events.Player.OnDie -= modOnPlayerDie;
            On.Celeste.ChangeRespawnTrigger.OnEnter -= onChangeRespawnTriggerOnEnter;
        }

        private static void modOnPlayerDie(Player player)
        {
            LightManager manager = player.SceneAs<Level>().Tracker.GetEntity<LightManager>();
            if (manager != null)
            {
                if (manager.ForceModeRoutine.Active)
                {
                    manager.ForceModeRoutine.Cancel();
                    manager.TemporaryMode = XaphanModuleSession.LightModes.None;
                }
                XaphanModule.ModSession.LightMode = manager.RespawnMode;
                manager.SetMainMode(manager.RespawnMode);
            }
        }

        private static void modOnLevelLoad(Level level, Player.IntroTypes playerIntro, bool isFromLoader)
        {
            if (level.Tracker.GetEntities<LightManager>().Count == 0)
            {
                level.Add(new LightManager());
            }
        }

        private static void onChangeRespawnTriggerOnEnter(On.Celeste.ChangeRespawnTrigger.orig_OnEnter orig, ChangeRespawnTrigger self, Player player)
        {
            orig(self, player);
            bool onSolid = true;
            Vector2 point = self.Target + Vector2.UnitY * -4f;
            Session session = self.SceneAs<Level>().Session;
            if (self.Scene.CollideCheck<Solid>(point))
            {
                onSolid = self.Scene.CollideCheck<FloatySpaceBlock>(point);
            }
            if (onSolid && (!session.RespawnPoint.HasValue || session.RespawnPoint.Value != self.Target))
            {
                foreach (LightManager manager in self.SceneAs<Level>().Tracker.GetEntities<LightManager>())
                {
                    manager.RespawnMode = manager.MainMode;
                }
            }
        }

        public override void Added(Scene scene)
        {
            base.Added(scene);
            MainMode = RespawnMode = XaphanModule.ModSession.LightMode;
        }

        public void SetMainMode(XaphanModuleSession.LightModes mode)
        {
            MainMode = mode;
        }

        public void ForceTemporaryMode(XaphanModuleSession.LightModes mode, float time)
        {
            TemporaryModeTimer = time;
            if (!ForceModeRoutine.Active)
            {
                Add(ForceModeRoutine = new Coroutine(TemporaryModeRoutine(mode)));
            }
        }

        private IEnumerator TemporaryModeRoutine(XaphanModuleSession.LightModes mode)
        {
            TemporaryMode = mode;
            while (TemporaryModeTimer > 0)
            {
                TemporaryModeTimer -= Engine.DeltaTime;
                yield return null;
            }
            TemporaryMode = XaphanModuleSession.LightModes.None;
        }

        public override void Update()
        {
            base.Update();
            if (XaphanModule.ModSaveData.LightMode != XaphanModuleSession.LightModes.None && (XaphanModule.useMergeChaptersController ? SceneAs<Level>().Session.Level == XaphanModule.ModSaveData.SavedRoom[SceneAs<Level>().Session.Area.LevelSet] : true) && (SceneAs<Level>().Session.Area.GetSID().Contains("Xaphan/0") ? SceneAs<Level>().Session.Area.GetSID() != "Xaphan/0/0-Prologue" : true))
            {
                MainMode = RespawnMode = XaphanModule.ModSaveData.LightMode;
                XaphanModule.ModSaveData.LightMode = XaphanModuleSession.LightModes.None;
            }
            if (MainMode != XaphanModuleSession.LightModes.None || TemporaryMode != XaphanModuleSession.LightModes.None)
            {
                if (TemporaryMode != XaphanModuleSession.LightModes.None)
                {
                    XaphanModule.ModSession.LightMode = TemporaryMode;
                }
                else
                {
                    XaphanModule.ModSession.LightMode = MainMode;
                }
            }
            else
            {
                if (SceneAs<Level>().Tracker.GetEntities<LightOrb>().Count > 0 && MainMode == XaphanModuleSession.LightModes.None)
                {
                    MainMode = RespawnMode = XaphanModuleSession.LightModes.Dark;
                }
            }
            if (SceneAs<Level>().Transitioning)
            {
                if (XaphanModule.ModSession.LightMode == XaphanModuleSession.LightModes.Light && SceneAs<Level>().Lighting.Alpha != SceneAs<Level>().BaseLightingAlpha)
                {
                    SceneAs<Level>().Lighting.Alpha = SceneAs<Level>().BaseLightingAlpha + 0.25f;
                }
                else if (XaphanModule.ModSession.LightMode == XaphanModuleSession.LightModes.Dark && SceneAs<Level>().Lighting.Alpha != SceneAs<Level>().BaseLightingAlpha + 0.25f)
                {
                    SceneAs<Level>().Lighting.Alpha = SceneAs<Level>().BaseLightingAlpha + 0.4f;
                }
                RespawnMode = MainMode;
            }
            if (SceneAs<Level>().Tracker.GetEntity<Player>() != null)
            {
                if (XaphanModule.ModSession.LightMode == XaphanModuleSession.LightModes.Light)
                {
                    SceneAs<Level>().Lighting.Alpha = Calc.Approach(SceneAs<Level>().Lighting.Alpha, SceneAs<Level>().BaseLightingAlpha + 0.25f, Engine.DeltaTime);
                }
                else if (XaphanModule.ModSession.LightMode == XaphanModuleSession.LightModes.Dark)
                {
                    SceneAs<Level>().Lighting.Alpha = Calc.Approach(SceneAs<Level>().Lighting.Alpha, SceneAs<Level>().BaseLightingAlpha + 0.4f, Engine.DeltaTime);
                }
            }
        }
    }
}
