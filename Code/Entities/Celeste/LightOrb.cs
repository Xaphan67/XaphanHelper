using System;
using Celeste.Mod.Entities;
using Celeste.Mod.XaphanHelper.Managers;
using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste.Mod.XaphanHelper.Entities
{
    [Tracked(true)]
    [CustomEntity("XaphanHelper/LightOrb")]
    class LightOrb : Entity
    {
        private Sprite Sprite;

        private bool PlayerOnTop;

        private bool Temporary;

        private float Timer;

        private LightManager Manager;

        private float Cooldown;

        private ParticleType switchParticles;

        public LightOrb(EntityData data, Vector2 offset) : base(data.Position + offset)
        {
            Tag = Tags.TransitionUpdate;
            Temporary = data.Bool("temporary", false);
            Timer = data.Float("time", 3f);
            Add(Sprite = new Sprite(GFX.Game, "objects/XaphanHelper/LightOrb/"));
            Add(new PlayerCollider(onPlayer, Collider));
            Sprite.AddLoop("light", "light", 0.08f);
            Sprite.AddLoop("dark", "dark", 0.08f);
            Sprite.AddLoop("light-small", "light-small", 0.08f);
            Sprite.AddLoop("dark-small", "dark-small", 0.08f);
            Sprite.CenterOrigin();
            Collider = new Circle(Temporary ? 5f : 9f);
            if (Temporary)
            {
                Sprite.Position += Vector2.One * 4f;
            }
        }

        public override void Added(Scene scene)
        {
            base.Added(scene);
            LightManager manager = SceneAs<Level>().Tracker.GetEntity<LightManager>();
            if(manager != null)
            {
                Manager = manager;
            }
            switchParticles = new ParticleType
            {
                Size = 1f,
                Color = Color.Transparent,
                DirectionRange = (float)Math.PI / 30f,
                LifeMin = 0.6f,
                LifeMax = 1f,
                SpeedMin = 40f,
                SpeedMax = 50f,
                SpeedMultiplier = 0.25f,
                FadeMode = ParticleType.FadeModes.Late
            };
        }

        private void onPlayer(Player player)
        {
            if (!PlayerOnTop && Cooldown <= 0f)
            {
                Cooldown = 1f;
                PlayerOnTop = true;
                if (Manager != null)
                {
                    Audio.Play("event:/game/05_mirror_temple/torch_activate");
                    SceneAs<Level>().Displacement.AddBurst(Position, 0.4f, 8f, Temporary ? 20f : 24f, 0.5f, Ease.QuadOut, Ease.QuadOut);
                    if (Temporary)
                    {
                        if (Manager.ForceModeRoutine.Active)
                        {
                            switchParticles.Color = Calc.HexToColor(Manager.TemporaryMode == XaphanModuleSession.LightModes.Light ? "FCF859" : "81729F");
                        }
                        else
                        {
                            switchParticles.Color = Calc.HexToColor(Manager.MainMode == XaphanModuleSession.LightModes.Light ? "81729F" : "FCF859");
                        }
                    }
                    else
                    {
                        switchParticles.Color = Calc.HexToColor(Manager.MainMode == XaphanModuleSession.LightModes.Light ? "81729F" : "FCF859");
                    }
                    for (int i = 0; i < 360; i += 30)
                    {
                        SceneAs<Level>().ParticlesFG.Emit(switchParticles, 1, Center, Vector2.One * 2f, i * ((float)Math.PI / 180f));
                    }
                    if (Temporary)
                    {
                        if (Manager.ForceModeRoutine.Active)
                        {
                            Manager.ForceTemporaryMode(Manager.TemporaryMode, Timer);
                        }
                        else
                        {
                            Manager.ForceTemporaryMode(XaphanModule.ModSession.LightMode == XaphanModuleSession.LightModes.Light ? XaphanModuleSession.LightModes.Dark : XaphanModuleSession.LightModes.Light, Timer);
                        }
                    }
                    else
                    {
                        Manager.SetMainMode(Manager.MainMode == XaphanModuleSession.LightModes.Light ? XaphanModuleSession.LightModes.Dark : XaphanModuleSession.LightModes.Light);
                    }
                }
            }
        }

        public override void Update()
        {
            base.Update();
            if (SceneAs<Level>().Tracker.GetEntity<Player>() != null && !SceneAs<Level>().Tracker.GetEntity<Player>().Dead)
            {
                if (Cooldown > 0)
                {
                    Cooldown -= Engine.DeltaTime;
                }
                if (Manager == null)
                {
                    Manager = SceneAs<Level>().Tracker.GetEntity<LightManager>();
                }
                if (Temporary)
                {
                    if (Manager.ForceModeRoutine.Active)
                    {
                        Sprite.Play((Manager.TemporaryMode == XaphanModuleSession.LightModes.Light ? "light-small" : "dark-small"));
                    }
                    else
                    {
                        Sprite.Play((Manager.MainMode == XaphanModuleSession.LightModes.Light ? "dark-small" : "light-small"));
                    }
                }
                else
                {
                    Sprite.Play(Manager.MainMode == XaphanModuleSession.LightModes.Light ? "light" : "dark");
                }

                if (CollideFirst<Player>() == null && PlayerOnTop)
                {
                    PlayerOnTop = false;
                }
            }
        }
    }
}
