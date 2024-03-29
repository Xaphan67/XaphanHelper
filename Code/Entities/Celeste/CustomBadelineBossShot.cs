﻿using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste.Mod.XaphanHelper.Entities
{
    [Pooled]
    [Tracked(false)]
    public class CustomBadelineBossShot : Entity
    {
        public enum ShotPatterns
        {
            Single,
            Double,
            Triple
        }

        public static ParticleType P_Trail = new();

        private const float MoveSpeed = 100f;

        private const float CantKillTime = 0.15f;

        private const float AppearTime = 0.1f;

        private CustomBadelineBoss boss;

        private Level level;

        private Vector2 speed;

        private float particleDir;

        private Vector2 anchor;

        private Vector2 perp;

        private Player target;

        private Vector2 targetPt;

        private float angleOffset;

        private bool dead;

        private float cantKillTimer;

        private float appearTimer;

        private bool hasBeenInCamera;

        private SineWave sine;

        private float sineMult;

        private Sprite sprite;

        private bool outline;

        public CustomBadelineBossShot() : base(Vector2.Zero)
        {
            Add(sprite = GFX.SpriteBank.Create("badeline_projectile"));
            Collider = new Hitbox(4f, 4f, -2f, -2f);
            Add(new PlayerCollider(OnPlayer));
            Depth = -1000000;
            Add(sine = new SineWave(1.4f, 0f));
        }

        public CustomBadelineBossShot Init(CustomBadelineBoss boss, Player target, string shotTrailParticleColor1, string shotTrailParticleColor2, float angleOffset = 0f, bool outline = true)
        {
            this.boss = boss;
            this.outline = outline;
            P_Trail = new ParticleType
            {
                Size = 1f,
                Color = Calc.HexToColor(shotTrailParticleColor1),
                Color2 = Calc.HexToColor(shotTrailParticleColor2),
                ColorMode = ParticleType.ColorModes.Blink,
                FadeMode = ParticleType.FadeModes.Late,
                SpeedMin = 10f,
                SpeedMax = 30f,
                DirectionRange = 0.6981317f,
                LifeMin = 0.3f,
                LifeMax = 0.6f
            };
            anchor = (Position = boss.Center);
            this.target = target;
            this.angleOffset = angleOffset;
            dead = (hasBeenInCamera = false);
            cantKillTimer = 0.15f;
            appearTimer = 0.1f;
            sine.Reset();
            sineMult = 0f;
            sprite.Play("charge", restart: true);
            InitSpeed();
            return this;
        }

        public CustomBadelineBossShot InitAt(CustomBadelineBoss boss, Vector2 target, string shotTrailParticleColor1, string shotTrailParticleColor2, bool outline = true)
        {
            this.boss = boss;
            this.outline = outline;
            P_Trail = new ParticleType
            {
                Size = 1f,
                Color = Calc.HexToColor(shotTrailParticleColor1),
                Color2 = Calc.HexToColor(shotTrailParticleColor2),
                ColorMode = ParticleType.ColorModes.Blink,
                FadeMode = ParticleType.FadeModes.Late,
                SpeedMin = 10f,
                SpeedMax = 30f,
                DirectionRange = 0.6981317f,
                LifeMin = 0.3f,
                LifeMax = 0.6f
            };
            anchor = (Position = boss.Center);
            this.target = null;
            angleOffset = 0f;
            targetPt = target;
            dead = (hasBeenInCamera = false);
            cantKillTimer = 0.15f;
            appearTimer = 0.1f;
            sine.Reset();
            sineMult = 0f;
            sprite.Play("charge", restart: true);
            InitSpeed();
            return this;
        }

        private void InitSpeed()
        {
            if (target != null)
            {
                speed = (target.Center - base.Center).SafeNormalize(100f);
            }
            else
            {
                speed = (targetPt - base.Center).SafeNormalize(100f);
            }
            if (angleOffset != 0f)
            {
                speed = speed.Rotate(angleOffset);
            }
            perp = speed.Perpendicular().SafeNormalize();
            particleDir = (-speed).Angle();
        }

        public override void Added(Scene scene)
        {
            base.Added(scene);
            level = SceneAs<Level>();
            if (boss.Moving)
            {
                RemoveSelf();
            }
        }

        public override void Removed(Scene scene)
        {
            base.Removed(scene);
            level = null;
        }

        public override void Update()
        {
            base.Update();
            if (appearTimer > 0f)
            {
                Position = (anchor = boss.ShotOrigin);
                appearTimer -= Engine.DeltaTime;
                return;
            }
            if (cantKillTimer > 0f)
            {
                cantKillTimer -= Engine.DeltaTime;
            }
            anchor += speed * Engine.DeltaTime;
            Position = anchor + perp * sineMult * sine.Value * 3f;
            sineMult = Calc.Approach(sineMult, 1f, 2f * Engine.DeltaTime);
            if (!dead)
            {
                bool flag = level.IsInCamera(Position, 8f);
                if (flag && !hasBeenInCamera)
                {
                    hasBeenInCamera = true;
                }
                else if (!flag && hasBeenInCamera)
                {
                    Destroy();
                }
                if (base.Scene.OnInterval(0.04f))
                {
                    level.ParticlesFG.Emit(P_Trail, 1, base.Center, Vector2.One * 2f, particleDir);
                }
            }
        }

        public override void Render()
        {
            if (outline)
            {
                sprite.DrawOutline(Color.Black);
            }
            sprite.Render();
            base.Render();
        }

        public void Destroy()
        {
            dead = true;
            RemoveSelf();
        }

        private void OnPlayer(Player player)
        {
            if (!dead)
            {
                if (cantKillTimer > 0f)
                {
                    Destroy();
                }
                else
                {
                    player.Die((player.Center - Position).SafeNormalize());
                }
            }
        }
    }
}