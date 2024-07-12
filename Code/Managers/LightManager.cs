using System.Collections;
using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste.Mod.XaphanHelper.Managers
{
    [Tracked(true)]
    class LightManager : Entity
    {
        public bool MainFlagState;

        public bool ForcedFlagState;

        public float ForcedFlagTimer;

        public Coroutine ForceFlagRoutine = new();

        public bool wasSwitched;

        public bool flagState;

        public Vector2? startSpawnPoint;

        public LightManager()
        {
            Tag = Tags.Persistent | Tags.TransitionUpdate;
        }

        public static void Load()
        {
            Everest.Events.Level.OnLoadLevel += modOnLevelLoad;
            On.Celeste.ChangeRespawnTrigger.OnEnter += onChangeRespawnTriggerOnEnter;
        }

        public static void Unload()
        {
            Everest.Events.Level.OnLoadLevel -= modOnLevelLoad;
            On.Celeste.ChangeRespawnTrigger.OnEnter -= onChangeRespawnTriggerOnEnter;
        }

        private static void modOnLevelLoad(Level level, Player.IntroTypes playerIntro, bool isFromLoader)
        {
            if (level.Tracker.GetEntities<LightManager>().Count == 0)
            level.Add(new LightManager());
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
                    manager.flagState = session.GetFlag("XaphanHelper_LightMode");
                    int chapterIndex = self.SceneAs<Level>().Session.Area.ChapterIndex;
                    self.SceneAs<Level>().Session.SetFlag("Ch" + chapterIndex + "_XaphanHelper_LightMode_true", false);
                    self.SceneAs<Level>().Session.SetFlag("Ch" + chapterIndex + "_XaphanHelper_LightMode_false", false);
                }
            }
        }

        public override void Added(Scene scene)
        {
            base.Added(scene);
            int chapterIndex = SceneAs<Level>().Session.Area.ChapterIndex;
            if (SceneAs<Level>().Session.GetFlag("Ch" + chapterIndex + "_XaphanHelper_LightMode_true"))
            {
                flagState = true;
                SceneAs<Level>().Session.SetFlag("XaphanHelper_LightMode", true);
            }
            else if (SceneAs<Level>().Session.GetFlag("Ch" + chapterIndex + "_XaphanHelper_LightMode_false"))
            {
                flagState = false;
                SceneAs<Level>().Session.SetFlag("XaphanHelper_LightMode", false);
            }
            else
            {
                flagState = SceneAs<Level>().Session.GetFlag("XaphanHelper_LightMode");
                SceneAs<Level>().Session.SetFlag("Ch" + chapterIndex + "_XaphanHelper_LightMode_true", false);
                SceneAs<Level>().Session.SetFlag("Ch" + chapterIndex + "_XaphanHelper_LightMode_false", false);
                SceneAs<Level>().Session.SetFlag("Ch" + chapterIndex + "_XaphanHelper_LightMode_" + (flagState ? "true" : "false"), true);
            }
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
            }
        }

        public override void Update()
        {
            base.Update();
            if (SceneAs<Level>().Transitioning && wasSwitched)
            {
                wasSwitched = false;
                flagState = SceneAs<Level>().Session.GetFlag("XaphanHelper_LightMode");
                int chapterIndex = SceneAs<Level>().Session.Area.ChapterIndex;
                SceneAs<Level>().Session.SetFlag("Ch" + chapterIndex + "_XaphanHelper_LightMode_true", false);
                SceneAs<Level>().Session.SetFlag("Ch" + chapterIndex + "_XaphanHelper_LightMode_false", false);
            }
            if (!ForceFlagRoutine.Active)
            {
                SceneAs<Level>().Session.SetFlag("XaphanHelper_LightMode", MainFlagState);
            }
        }
    }
}
