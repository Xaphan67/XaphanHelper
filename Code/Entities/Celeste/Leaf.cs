using Microsoft.Xna.Framework;
using System;
using Celeste.Mod.Entities;
using Monocle;

namespace Celeste.Mod.XaphanHelper.Entities
{
    [CustomEntity("XaphanHelper/Leaf")]
    class Leaf : JumpThru
    {
        private Sprite sprite;

        private Wiggler wiggler;

        private ParticleType useParticles;

        private ParticleType appearParticles;

        private SoundSource sfx;

        private bool waiting = true;

        private float speed;

        private Vector2 startPos;

        private float respawnTimer;

        private bool returning;

        private float timer;

        private Vector2 scale;

        private bool canRumble;

        private bool triggered;

        private string flag;

        private float respawnTime;

        public Leaf(EntityData data, Vector2 offset) : base(data.Position + offset, 22, safe: false)
        {
            flag = data.Attr("flag");
            respawnTime = data.Float("respawnTime", 2.5f);
            startPos = Position;
            Collider.Position.X = -11f;
            timer = Calc.Random.NextFloat() * 4f;
            Add(wiggler = Wiggler.Create(0.3f, 4f));
            useParticles = new ParticleType
            {
                Source = GFX.Game["particles/cloud"],
                Color = Calc.HexToColor("273600"),
                FadeMode = ParticleType.FadeModes.None,
                LifeMin = 0.25f,
                LifeMax = 0.3f,
                Size = 0.7f,
                SizeRange = 0.25f,
                ScaleOut = true,
                Direction = 4.712389f,
                DirectionRange = 0.17453292f,
                SpeedMin = 10f,
                SpeedMax = 20f,
                SpeedMultiplier = 0.01f,
                Acceleration = new Vector2(0f, 90f)
            };
            appearParticles = new ParticleType
            {
                Size = 1f,
                Color = Calc.HexToColor("47550D"),
                DirectionRange = (float)Math.PI / 30f,
                LifeMin = 0.6f,
                LifeMax = 1f,
                SpeedMin = 40f,
                SpeedMax = 50f,
                SpeedMultiplier = 0.25f,
                FadeMode = ParticleType.FadeModes.Late
            };
            SurfaceSoundIndex = 4;
            Add(new LightOcclude(0.2f));
            scale = Vector2.One;
            Add(sfx = new SoundSource());
        }

        public override void Added(Scene scene)
        {
            base.Added(scene);
            Add(sprite = new Sprite(GFX.Game, "objects/XaphanHelper/Leaf/"));
            sprite.AddLoop("idle", "leaf", 0);
            sprite.Add("spawn", "leaf", 0.03f, 9, 8, 7, 6, 5, 4, 3, 2, 1, 0);
            sprite.Add("fade", "leaf", 0.02f, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10);
            sprite.Origin = new Vector2(sprite.Width / 2f, 6f);
            sprite.OnFrameChange = delegate (string s)
            {
                if (s == "spawn" && sprite.CurrentAnimationFrame == 6)
                {
                    wiggler.Start();
                }
            };
            sprite.Play("idle");
        }

        public override void Update()
        {
            base.Update();
            appearParticles.Color = (!string.IsNullOrEmpty(flag) && SceneAs<Level>().Session.GetFlag(flag)) ? Calc.HexToColor("47550D") : Calc.HexToColor("3B0D01");
            if (sprite.CurrentAnimationID == "fade")
            {
                scale.X = Calc.Approach(scale.X, 1.2f, 1f * Engine.DeltaTime);
                scale.Y = Calc.Approach(scale.Y, 1.4f, 1f * Engine.DeltaTime);
            }
            else
            {
                scale.X = Calc.Approach(scale.X, 1f, 1f * Engine.DeltaTime);
                scale.Y = Calc.Approach(scale.Y, 1f, 1f * Engine.DeltaTime);
            }
            if (!string.IsNullOrEmpty(flag) && SceneAs<Level>().Session.GetFlag(flag))
            {
                sprite.Color = Color.White;
                triggered = false;
                timer += Engine.DeltaTime;
                if (GetPlayerRider() != null)
                {
                    sprite.Position = Vector2.Zero;
                }
                else
                {
                    sprite.Position = Calc.Approach(sprite.Position, new Vector2(0f, (float)Math.Sin(timer * 2f)), Engine.DeltaTime * 4f);
                }
                if (respawnTimer > 0f)
                {
                    respawnTimer -= Engine.DeltaTime;
                    if (respawnTimer <= 0f)
                    {
                        waiting = true;
                        Y = startPos.Y;
                        speed = 0f;
                        scale = Vector2.One;
                        Collidable = true;
                        sprite.Play("spawn");
                        for (int i = 0; i < 360; i += 30)
                        {
                            SceneAs<Level>().ParticlesBG.Emit(appearParticles, 1, Center, Vector2.One * 2f, i * ((float)Math.PI / 180f));
                        }
                        sfx.Play("event:/game/04_cliffside/cloud_pink_reappear");
                    }
                    return;
                }
                if (waiting)
                {
                    Player playerRider = GetPlayerRider();
                    if (playerRider != null && playerRider.Speed.Y >= 0f)
                    {
                        canRumble = true;
                        speed = 180f;
                        scale = new Vector2(1.3f, 0.7f);
                        waiting = false;
                        Audio.Play("event:/game/04_cliffside/cloud_pink_boost", Position);

                    }
                    return;
                }
                if (returning)
                {
                    speed = Calc.Approach(speed, 180f, 600f * Engine.DeltaTime);
                    MoveTowardsY(startPos.Y, speed * Engine.DeltaTime);
                    if (ExactPosition.Y == startPos.Y)
                    {
                        returning = false;
                        waiting = true;
                        speed = 0f;
                    }
                    return;
                }
                if (Collidable && !HasPlayerRider())
                {
                    Collidable = false;
                    sprite.Play("fade");
                }
                if (speed < 0f && canRumble)
                {
                    canRumble = false;
                    if (HasPlayerRider())
                    {
                        Input.Rumble(RumbleStrength.Medium, RumbleLength.Medium);
                    }
                }
                if (speed < 0f && Scene.OnInterval(0.02f))
                {
                    (Scene as Level).ParticlesBG.Emit(useParticles, 1, Position + new Vector2(0f, 2f), new Vector2(base.Collider.Width / 2f, 1f), (float)Math.PI / 2f);
                }
                if (speed < 0f)
                {
                    sprite.Scale.Y = Calc.Approach(sprite.Scale.Y, 0f, Engine.DeltaTime * 4f);
                }
                if (Y >= startPos.Y)
                {
                    speed -= 1200f * Engine.DeltaTime;
                }
                else
                {
                    speed += 1200f * Engine.DeltaTime;
                    if (speed >= -100f)
                    {
                        Player playerRider = GetPlayerRider();
                        if (playerRider != null && playerRider.Speed.Y >= 0f)
                        {
                            playerRider.Speed.Y = -200f;
                        }
                        Collidable = false;
                        sprite.Play("fade");
                        respawnTimer = respawnTime;
                    }
                }
                float num = speed;
                if (num < 0f)
                {
                    num = -220f;
                }
                MoveV(speed * Engine.DeltaTime, num);
            }
            else
            {
                sprite.Color = Color.OrangeRed;
                Player playerRider = GetPlayerRider();
                if (playerRider != null || triggered)
                {
                    if (!triggered)
                    {
                        Audio.Play("event:/game/xaphan/leaf", Position);
                    }
                    triggered = true;
                    speed = Calc.Approach(speed, 35f, 600f * Engine.DeltaTime);
                    Vector2 vector = startPos - -Vector2.UnitY * 16f;
                    Vector2 vector2 = Calc.Approach(ExactPosition, vector, speed * 0.333f * Engine.DeltaTime);
                    Vector2 liftSpeed = (vector2 - ExactPosition).SafeNormalize(speed * 0.333f);
                    liftSpeed.X *= 0.75f;
                    MoveTo(vector2, liftSpeed);
                }
                if (respawnTimer > 0f)
                {
                    respawnTimer -= Engine.DeltaTime;
                    if (respawnTimer <= 0f)
                    {
                        triggered = false;
                        waiting = true;
                        Y = startPos.Y;
                        speed = 0f;
                        scale = Vector2.One;
                        Collidable = true;
                        sprite.Play("spawn");
                        for (int i = 0; i < 360; i += 30)
                        {
                            SceneAs<Level>().ParticlesBG.Emit(appearParticles, 1, Center, Vector2.One * 2f, i * ((float)Math.PI / 180f));
                        }
                        sfx.Play("event:/game/04_cliffside/cloud_pink_reappear");
                    }
                    return;
                }
                if (Y >= startPos.Y + 8)
                {
                    Collidable = false;
                    sprite.Play("fade");
                    respawnTimer = respawnTime;
                }
            }
        }

        public override void Render()
        {
            Vector2 vector = scale;
            vector *= 1f + 0.1f * wiggler.Value;
            sprite.Scale = vector;
            base.Render();
        }
    }
}
