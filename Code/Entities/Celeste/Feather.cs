using System;
using System.Reflection;
using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste.Mod.XaphanHelper.Entities
{
    [Tracked(true)]
    public class Feather : Actor
    {
        private static FieldInfo PlayerMinHoldTimer = typeof(Player).GetField("minHoldTimer", BindingFlags.Instance | BindingFlags.NonPublic);

        private ParticleType P_Glow;

        public Vector2 Speed;

        public Holdable Hold;

        private Level level;

        private bool destroyed;

        private Sprite sprite;

        private Wiggler wiggler;

        private SineWave platformSine;

        private bool canGoUp;

        private bool hasBeenHolded;

        private Player player;

        public Feather(Vector2 position, Player player) : base(position)
        {
            this.player = player;
            Collider = new Hitbox(8f, 10f, -4f, -10f);
            Add(sprite = new Sprite(GFX.Game, "upgrades/GoldenFeather/"));
            sprite.AddLoop("held", "held", 1f);
            sprite.Add("disapear", "disapear", 0.05f);
            sprite.Justify = new Vector2(0.49f, 0.58f);
            sprite.Play("held");
            Add(wiggler = Wiggler.Create(0.25f, 4f));
            Depth = 5;
            Add(Hold = new Holdable(0.3f));
            Hold.PickupCollider = new Hitbox(20f, 22f, -10f, -16f);
            Hold.SlowFall = true;
            Hold.SlowRun = false;
            Hold.OnPickup = OnPickup;
            Hold.OnRelease = OnRelease;
            Hold.SpeedGetter = (() => Speed);
            platformSine = new SineWave(0.3f, 0f);
            Add(platformSine);
            Add(new WindMover(WindMode));
            P_Glow = new ParticleType
            {
                SpeedMin = 16f,
                SpeedMax = 32f,
                DirectionRange = (float)Math.PI * 2f,
                LifeMin = 0.4f,
                LifeMax = 0.8f,
                Size = 1f,
                FadeMode = ParticleType.FadeModes.Late,
                Color = Calc.HexToColor("FFD65C"),
                Color2 = Calc.HexToColor("FF8000"),
                ColorMode = ParticleType.ColorModes.Blink
            };
        }

        public override void Added(Scene scene)
        {
            base.Added(scene);
            if (player != null && player.Holding != Hold)
            {
                player.Holding = Hold;
                Hold.Pickup(player);
                PlayerMinHoldTimer.SetValue(player, 0.35f);
                player.StateMachine.State = Player.StPickup;
            }
            level = SceneAs<Level>();
        }

        public override void Update()
        {
            if (((Hold.IsHeld && Hold.Holder.OnGround()) || CollideCheck<Liquid>() || CollideCheck<Waterfall.WaterfallSection>() || (hasBeenHolded && !Hold.IsHeld)) && !destroyed)
            {
                Destroy();
            }
            if (Hold.Holder != null && Hold.Holder.Facing == Facings.Left)
            {
                sprite.FlipX = true;
            }
            else
            {
                sprite.FlipX = false;
            }
            if (Scene.OnInterval(0.05f))
            {
                level.Particles.Emit(P_Glow, 1, Center, new Vector2(5f, 4f));
            }
            float target = (!Hold.IsHeld) ? 0f : ((!Hold.Holder.OnGround()) ? Calc.ClampedMap(Hold.Holder.Speed.X, -300f, 300f, (float)Math.PI / 3f, -(float)Math.PI / 3f) : Calc.ClampedMap(Hold.Holder.Speed.X, -300f, 300f, 0.6981317f, -0.6981317f));
            sprite.Rotation = Calc.Approach(sprite.Rotation, target, (float)Math.PI * Engine.DeltaTime);
            base.Update();
            if (!destroyed)
            {
                foreach (SeekerBarrier entity in Scene.Tracker.GetEntities<SeekerBarrier>())
                {
                    entity.Collidable = true;
                    bool num = CollideCheck(entity);
                    entity.Collidable = false;
                    if (num)
                    {
                        Destroy();
                        return;
                    }
                }
                if (Hold.IsHeld)
                {
                    hasBeenHolded = true;
                    if (Hold.Holder.Speed.Y > 0)
                    {
                        canGoUp = true;
                    }
                    if (Hold.Holder.Speed.Y < 0 && !canGoUp)
                    {
                        Hold.Holder.Speed.Y = 0;
                        canGoUp = true;
                    }
                }
                if (Hold.ShouldHaveGravity)
                {
                    float num2 = 200f;
                    if (Speed.Y >= -30f)
                    {
                        num2 *= 0.5f;
                    }
                    float num3 = (Speed.Y < 0f) ? 40f : 10f;
                    Speed.X = Calc.Approach(Speed.X, 0f, num3 * Engine.DeltaTime);
                    if (level.Wind.Y < 0f)
                    {
                        Speed.Y = Calc.Approach(Speed.Y, 0f, num2 * Engine.DeltaTime);
                    }
                    else
                    {
                        Speed.Y = Calc.Approach(Speed.Y, 30f, num2 * Engine.DeltaTime);
                    }
                }
                MoveH(Speed.X * Engine.DeltaTime);
                MoveV(Speed.Y * Engine.DeltaTime);
                CollisionData data;
                if (Left < level.Bounds.Left)
                {
                    Left = level.Bounds.Left;
                    data = new CollisionData
                    {
                        Direction = -Vector2.UnitX
                    };
                }
                else if (Right > level.Bounds.Right)
                {
                    Right = level.Bounds.Right;
                    data = new CollisionData
                    {
                        Direction = Vector2.UnitX
                    };
                }
                if (Top > (level.Bounds.Bottom + 16))
                {
                    RemoveSelf();
                    return;
                }
                Hold.CheckAgainstColliders();
                Vector2 one = Vector2.One;
                if (!Hold.IsHeld)
                {
                    if (level.Wind.Y < 0f)
                    {
                        PlayOpen();
                    }
                }
                else if (Hold.Holder.Speed.Y > 20f || level.Wind.Y < 0f)
                {
                    PlayOpen();
                    if (Input.GliderMoveY.Value > 0)
                    {
                        one.X = 0.7f;
                        one.Y = 1.4f;
                    }
                    else if (Input.GliderMoveY.Value < 0)
                    {
                        one.X = 1.2f;
                        one.Y = 0.8f;
                    }
                    Input.Rumble(RumbleStrength.Climb, RumbleLength.Short);
                }
                sprite.Scale.Y = Calc.Approach(sprite.Scale.Y, one.Y, Engine.DeltaTime * 2f);
                sprite.Scale.X = Calc.Approach(sprite.Scale.X, Math.Sign(sprite.Scale.X) * one.X, Engine.DeltaTime * 2f);
            }
            else
            {
                Position += Speed * Engine.DeltaTime;
            }
        }

        private void PlayOpen()
        {
            if (sprite.CurrentAnimationID != "held")
            {
                sprite.Play("held");
                sprite.Scale = new Vector2(1.5f, 0.6f);
                if (Hold.IsHeld)
                {
                    Input.Rumble(RumbleStrength.Medium, RumbleLength.Short);
                }
            }
        }

        public override void Render()
        {
            if (!destroyed)
            {
                sprite.DrawSimpleOutline();
            }
            base.Render();
        }

        private void WindMode(Vector2 wind)
        {
            if (!Hold.IsHeld)
            {
                if (wind.X != 0f)
                {
                    MoveH(wind.X * 0.5f);
                }
                if (wind.Y != 0f)
                {
                    MoveV(wind.Y);
                }
            }
        }

        private void OnPickup()
        {
            AllowPushing = false;
            Speed = Vector2.Zero;
            AddTag(Tags.Persistent);
            AllowPushing = false;
        }

        private void OnRelease(Vector2 force)
        {
            if (!destroyed)
            {
                Destroy();
            }
            AllowPushing = true;
        }

        private void Destroy()
        {
            destroyed = true;
            if (Hold.IsHeld)
            {
                Hold.Holder.Drop();
            }
            Collidable = false;
            Speed = Vector2.Zero;
            Input.Rumble(RumbleStrength.Medium, RumbleLength.Medium);
            Audio.Play("event:/game/xaphan/golden_feather_disapear", Position);
            sprite.Play("disapear");
            sprite.OnLastFrame += onLastFrame;
        }

        private void onLastFrame(string s)
        {
            RemoveSelf();
        }

        protected override void OnSquish(CollisionData data)
        {
            if (!TrySquishWiggle(data, 3, 3))
            {
                RemoveSelf();
            }
        }
    }
}