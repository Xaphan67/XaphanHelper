using System;
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

        public bool MainFlagState;

        public bool ForcedFlagState;

        public float ForcedFlagTimer;

        public Coroutine ForceFlagRoutine = new();

        public bool wasSwitched;

        public Vector2? startSpawnPoint;

        public LightManager()
        {
            Tag = Tags.Persistent | Tags.TransitionUpdate;
            TransitionListener transitionListener = new();
            transitionListener.OnOut = OnTransitionOut;
            Add(transitionListener);
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
                if (manager.ForceFlagRoutine.Active)
                {
                    manager.ForceFlagRoutine.Cancel();
                }
                XaphanModule.ModSession.LightMode = manager.RespawnMode;
                manager.SetMainFlag(manager.RespawnMode == XaphanModuleSession.LightModes.Light);
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
                    manager.startSpawnPoint = session.RespawnPoint;
                    manager.RespawnMode = XaphanModule.ModSession.LightMode;
                }
            }
        }

        private void OnTransitionOut(float percent)
        {
            if (ForceFlagRoutine.Active)
            {
                ForceFlagRoutine.Cancel();
            }
        }

        public override void Added(Scene scene)
        {
            base.Added(scene);
            RespawnMode = XaphanModule.ModSession.LightMode;
            MainFlagState = SceneAs<Level>().Session.GetFlag("XaphanHelper_LightMode");
        }

        public void SetMainFlag(bool state)
        {
            MainFlagState = state;
            wasSwitched = true;
            startSpawnPoint = SceneAs<Level>().Session.RespawnPoint;
        }

        public void ForceTemporaryFlag(bool state, float time)
        {
            ForcedFlagState = state;
            ForcedFlagTimer = time;
            if (!ForceFlagRoutine.Active)
            {
                Add(ForceFlagRoutine = new Coroutine(FlagRoutine()));
            }
            wasSwitched = true;
            startSpawnPoint = SceneAs<Level>().Session.RespawnPoint;
        }

        private IEnumerator FlagRoutine()
        {
            while (ForcedFlagTimer > 0)
            {
                ForcedFlagTimer -= Engine.DeltaTime;
                yield return null;
                SceneAs<Level>().Session.SetFlag("XaphanHelper_LightMode", ForcedFlagState);
                XaphanModule.ModSession.LightMode = ForcedFlagState ? XaphanModuleSession.LightModes.Light : XaphanModuleSession.LightModes.Dark;
            }
        }

        public override void Update()
        {
            base.Update();
            if (XaphanModule.ModSaveData.LightMode != XaphanModuleSession.LightModes.None && (XaphanModule.useMergeChaptersController ? SceneAs<Level>().Session.Level == XaphanModule.ModSaveData.SavedRoom[SceneAs<Level>().Session.Area.LevelSet] : true) && (SceneAs<Level>().Session.Area.GetSID().Contains("Xaphan/0") ? SceneAs<Level>().Session.Area.GetSID() != "Xaphan/0/0-Prologue" : true))
            {
                XaphanModule.ModSession.LightMode = XaphanModule.ModSaveData.LightMode;
                XaphanModule.ModSaveData.LightMode = XaphanModuleSession.LightModes.None;
                MainFlagState = XaphanModule.ModSession.LightMode == XaphanModuleSession.LightModes.Light;
                SceneAs<Level>().Session.SetFlag("XaphanHelper_LightMode", MainFlagState);
            }
            if (SceneAs<Level>().Transitioning)
            {
                if (XaphanModule.ModSession.LightMode == XaphanModuleSession.LightModes.Light && SceneAs<Level>().Lighting.Alpha != SceneAs<Level>().BaseLightingAlpha)
                {
                    SceneAs<Level>().Lighting.Alpha = SceneAs<Level>().BaseLightingAlpha;
                }
                else if (XaphanModule.ModSession.LightMode == XaphanModuleSession.LightModes.Dark && SceneAs<Level>().Lighting.Alpha != SceneAs<Level>().BaseLightingAlpha + 0.25f)
                {
                    SceneAs<Level>().Lighting.Alpha = SceneAs<Level>().BaseLightingAlpha + 0.25f;
                }
                if (wasSwitched)
                {
                    RespawnMode = XaphanModule.ModSession.LightMode;
                    wasSwitched = false;
                }
            }
            if (!ForceFlagRoutine.Active && SceneAs<Level>().Tracker.GetEntities<LightOrb>().Count > 0)
            {
                SceneAs<Level>().Session.SetFlag("XaphanHelper_LightMode", MainFlagState);
                XaphanModule.ModSession.LightMode = MainFlagState ? XaphanModuleSession.LightModes.Light : XaphanModuleSession.LightModes.Dark;
            }
            if (SceneAs<Level>().Tracker.GetEntity<Player>() != null)
            {
                if (XaphanModule.ModSession.LightMode == XaphanModuleSession.LightModes.Light)
                {
                    SceneAs<Level>().Lighting.Alpha = Calc.Approach(SceneAs<Level>().Lighting.Alpha, SceneAs<Level>().BaseLightingAlpha, Engine.DeltaTime);
                }
                else if (XaphanModule.ModSession.LightMode == XaphanModuleSession.LightModes.Dark)
                {
                    SceneAs<Level>().Lighting.Alpha = Calc.Approach(SceneAs<Level>().Lighting.Alpha, SceneAs<Level>().BaseLightingAlpha + 0.25f, Engine.DeltaTime);
                }
            }
        }
    }
}
