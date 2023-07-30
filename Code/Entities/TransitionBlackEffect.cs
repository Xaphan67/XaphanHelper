using System;
using System.Collections;
using System.Linq;
using System.Reflection;
using Celeste.Mod.XaphanHelper.Effects;
using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste.Mod.XaphanHelper.Entities
{
    [Tracked(true)]
    public class TransitionBlackEffect : Entity
    {
        private Camera camera = new();

        private static float alpha;

        private bool immediate;

        private static FieldInfo LevelTransition = typeof(Level).GetField("transition", BindingFlags.Instance | BindingFlags.NonPublic);

        public TransitionBlackEffect(bool immediate = false) : base()
        {
            Tag = (Tags.Persistent | Tags.TransitionUpdate);
            Depth = -89990;
            alpha = immediate ? 1f : 0f;
            this.immediate = immediate;
        }

        public static void Load()
        {
            On.Celeste.Level.TransitionRoutine += OnLevelTransitionRoutine;
            On.Celeste.TalkComponent.TalkComponentUI.Update += OnTalkComponentTalkComponentUIUpdate;
            On.Celeste.TalkComponent.TalkComponentUI.ctor += OnTalkComponentTalkComponentUICtor;
            On.Celeste.Booster.ctor_Vector2_bool += OnBoosterCtor;
            On.Celeste.Booster.Update += OnBoosterUpdate;
        }

        public static void Unload()
        {
            On.Celeste.Level.TransitionRoutine -= OnLevelTransitionRoutine;
            On.Celeste.TalkComponent.TalkComponentUI.Update -= OnTalkComponentTalkComponentUIUpdate;
            On.Celeste.TalkComponent.TalkComponentUI.ctor -= OnTalkComponentTalkComponentUICtor;
            On.Celeste.Booster.ctor_Vector2_bool -= OnBoosterCtor;
            On.Celeste.Booster.Update -= OnBoosterUpdate;

        }

        private static IEnumerator OnLevelTransitionRoutine(On.Celeste.Level.orig_TransitionRoutine orig, Level self, LevelData next, Vector2 direction)
        {
            if (self.Session.Area.LevelSet == "Xaphan/0" && XaphanModule.SoCMVersion >= new Version(3, 0, 0))
            {
                self.Add(new TransitionBlackEffect());
                yield return 0.35f;
                yield return new SwapImmediately(orig(self, next, direction));
                LevelTransition.SetValue(self, new Coroutine(TranstionRoutine(self)));
            }
            else
            {
                yield return new SwapImmediately(orig(self, next, direction));
            }
        }

        private static void OnTalkComponentTalkComponentUICtor(On.Celeste.TalkComponent.TalkComponentUI.orig_ctor orig, TalkComponent.TalkComponentUI self, TalkComponent handler)
        {
            orig(self, handler);
            if (SaveData.Instance.CurrentSession_Safe.Area.LevelSet == "Xaphan/0" && XaphanModule.SoCMVersion >= new Version(3, 0, 0))
            {
                self.AddTag(Tags.TransitionUpdate);
            }
        }

        private static void OnTalkComponentTalkComponentUIUpdate(On.Celeste.TalkComponent.TalkComponentUI.orig_Update orig, TalkComponent.TalkComponentUI self)
        {
            if (self.SceneAs<Level>().Session.Area.LevelSet == "Xaphan/0" && XaphanModule.SoCMVersion >= new Version(3, 0, 0))
            {
                if (self.SceneAs<Level>().Tracker.GetEntities<TransitionBlackEffect>().Count() != 0)
                {
                    self.Visible = false;
                }
                else
                {
                    self.Visible = true;
                }
            }
            orig(self);
        }

        private static void OnBoosterUpdate(On.Celeste.Booster.orig_Update orig, Booster self)
        {
            if (self.SceneAs<Level>().Session.Area.LevelSet == "Xaphan/0" && XaphanModule.SoCMVersion >= new Version(3, 0, 0))
            {
                if (self.SceneAs<Level>().Transitioning && self.BoostingPlayer)
                {
                    self.Depth = -89992;
                }
                else
                {
                    self.Depth = -8500;
                }
            }
            orig(self);
        }

        private static void OnBoosterCtor(On.Celeste.Booster.orig_ctor_Vector2_bool orig, Booster self, Vector2 position, bool red)
        {
            orig(self, position, red);
            if (SaveData.Instance.CurrentSession_Safe.Area.LevelSet == "Xaphan/0" && XaphanModule.SoCMVersion >= new Version(3, 0, 0))
            {
                self.AddTag(Tags.TransitionUpdate);
            }
        }


        private static IEnumerator TranstionRoutine(Level level)
        {
            yield return 0.35f;
            LevelTransition.SetValue(level, null);
        }

        public override void Added(Scene scene)
        {
            base.Added(scene);
            camera = SceneAs<Level>().Camera;
            if (!immediate)
            {
                Add(new Coroutine(FadeRoutine()));
            }
        }

        private IEnumerator FadeRoutine()
        {
            Player player = SceneAs<Level>().Tracker.GetEntity<Player>();
            Drone drone = SceneAs<Level>().Tracker.GetEntity<Drone>();
            if (player != null)
            {
                if (SceneAs<Level>().Transitioning)
                {
                    player.Depth = Depth - 1;
                }
            }
            if (drone != null)
            {
                drone.Depth = Depth - 1;
            }
            float fadeTimer = 0.35f;
            while (fadeTimer > 0)
            {
                fadeTimer -= Engine.DeltaTime;
                alpha += Engine.DeltaTime * 2.86f;
                alpha = Math.Min(1f, alpha);
                foreach (Backdrop backdrop in SceneAs<Level>().Foreground.Backdrops)
                {
                    if (backdrop is HeatParticles)
                    {
                        backdrop.Color.A = (byte)((Math.Max(0, 1 - alpha)) * 255);
                    }
                    else
                    {
                        backdrop.FadeAlphaMultiplier = (Math.Max(0, 1 - alpha));
                    }
                }
                yield return null;
            }
            yield return 0.65f;
            fadeTimer = 0.35f;
            while (fadeTimer > 0)
            {
                fadeTimer -= Engine.DeltaTime;
                alpha -= Engine.DeltaTime * 2.86f;
                alpha = Math.Max(0f, alpha);
                foreach (Backdrop backdrop in SceneAs<Level>().Foreground.Backdrops)
                {
                    if (backdrop is HeatParticles)
                    {
                        backdrop.Color.A = (byte)((Math.Max(0, 1 - alpha)) * 255);
                    }
                    else
                    {
                        backdrop.FadeAlphaMultiplier = (Math.Max(0, 1 - alpha));
                    }
                }
                yield return null;
            }
            if (player != null)
            {
                player.Depth = 0;
            }
            if (drone != null)
            {
                drone.Depth = 0;
            }
            RemoveSelf();
        }

        public override void Render()
        {
            base.Render();
            Draw.Rect(camera.X - 10, camera.Y - 10, Engine.Width + 20, Engine.Height + 20, Color.Black * alpha);
        }
    }
}
