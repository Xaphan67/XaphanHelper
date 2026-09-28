using Monocle;
using Microsoft.Xna.Framework;
using Celeste.Mod.Entities;
using System.Collections;
using Celeste.Mod.XaphanHelper.Colliders;
using System.Reflection;
using System;
using Celeste.Mod.XaphanHelper.Enemies;

namespace Celeste.Mod.XaphanHelper.Entities
{
    [Tracked(true)]
    [CustomEntity("XaphanHelper/ExplosiveBoulder")]
    class ExplosiveBoulder : Actor
    {
        private static MethodInfo Spring_BounceAnimate = typeof(Spring).GetMethod("BounceAnimate", BindingFlags.Instance | BindingFlags.NonPublic);

        private FieldInfo Player_launchApproachX = typeof(Player).GetField("launchApproachX", BindingFlags.Instance | BindingFlags.NonPublic);

        private FieldInfo Player_dashCooldownTimer = typeof(Player).GetField("dashCooldownTimer", BindingFlags.Instance | BindingFlags.NonPublic);

        private Sprite Sprite;

        private string Directory;

        public Vector2 Speed;

        private Collision onCollideH;

        private Collision onCollideV;

        private bool Gravity;

        private bool explode;

        private VertexLight light;

        private BloomPoint bloom;

        private ParticleType P_Explode;

        public ParticleType P_Steam;

        private float BounceForce;

        private bool RefillJump;

        private float DashCooldown;

        private Vector2 imageOffset;

        private float ImmunityTimer;

        public ExplosiveBoulder(EntityData data, Vector2 offset) : base(data.Position + offset)
        {
            Collider = new ColliderList(new Hitbox(14, 24, -7, -12), new Hitbox(24, 14, -12, -7), new Hitbox(20, 20, -10, -10));
            Directory = data.Attr("directory");
            Gravity = data.Bool("gravity");
            BounceForce = data.Float("bounceForce", 280f);
            RefillJump = data.Bool("refillJump", false);
            DashCooldown = data.Float("dashCooldown", 0.2f);
            if (string.IsNullOrEmpty(Directory))
            {
                Directory = "objects/XaphanHelper/ExplosiveBoulder";
            }
            Add(new PlayerCollider(onPlayer, new Circle(12f)));
            Add(new EnemyCollider(onEnemy, new Circle(12f)));
            Add(new SpringCollider(onSpring, new Circle(12f)));
            Add(new WeaponCollider(HitByBeam, HitByMissile, new Circle(12f)));
            Add(Sprite = new Sprite(GFX.Game, Directory + "/"));
            Sprite.AddLoop("idleA", "boulder", 0.12f, 0, 1, 2, 3, 4, 3, 2, 1);
            Sprite.AddLoop("idleB", "boulder", 0.12f, 5, 6, 7, 8, 9, 8, 7, 6);
            Sprite.AddLoop("explode", "explode", 0.08f);
            Sprite.CenterOrigin();
            Sprite.Position += new Vector2(20);
            Sprite.Play("idle" + (Calc.Random.Next(2) == 1 ? "A" : "B"));
            onCollideH = OnCollideH;
            onCollideV = OnCollideV;
            Add(new Coroutine(GravityRoutine()));
            P_Explode = new ParticleType
            {
                Color = Calc.HexToColor("F8C820"),
                Color2 = Calc.HexToColor("C83010"),
                ColorMode = ParticleType.ColorModes.Blink,
                FadeMode = ParticleType.FadeModes.Late,
                Size = 1f,
                LifeMin = 0.2f,
                LifeMax = 0.6f,
                SpeedMin = 15f,
                SpeedMax = 75f,
                SpeedMultiplier = 0.3f,
                DirectionRange = (float)Math.PI / 3f
            };
            P_Steam = new ParticleType(ParticleTypes.Steam)
            {
                LifeMin = 1f,
                LifeMax = 2f,
            };
            Depth = -8499;
        }

        public static void Load()
        {
            On.Celeste.Actor.MoveHExact += OnActorMoveH;
            On.Celeste.Actor.MoveVExact += OnActorMoveV;
        }

        public static void Unload()
        {
            On.Celeste.Actor.MoveHExact -= OnActorMoveH;
            On.Celeste.Actor.MoveVExact -= OnActorMoveV;
        }

        private static bool OnActorMoveH(On.Celeste.Actor.orig_MoveHExact orig, Actor self, int moveH, Collision onCollide, Solid pusher)
        {
            if (self.Scene != null && self.GetType() == typeof(ExplosiveBoulder))
            {
                ExplosiveBoulder boulder = self as ExplosiveBoulder;
                foreach (CustomSpinner.Filler filler in self.SceneAs<Level>().Tracker.GetEntities<CustomSpinner.Filler>())
                {
                    if (boulder.CollideCheck(filler))
                    {
                        filler.RemoveSelf();
                    }
                }
            }
            return orig(self, moveH, onCollide, pusher);
        }

        private static bool OnActorMoveV(On.Celeste.Actor.orig_MoveVExact orig, Actor self, int moveV, Collision onCollide, Solid pusher)
        {
            if (self.Scene != null && self.GetType() == typeof(ExplosiveBoulder))
            {
                ExplosiveBoulder boulder = self as ExplosiveBoulder;
                foreach (CustomSpinner.Filler filler in self.SceneAs<Level>().Tracker.GetEntities<CustomSpinner.Filler>())
                {
                    if (boulder.CollideCheck(filler))
                    {
                        filler.RemoveSelf();
                    }
                }
            }
            return orig(self, moveV, onCollide, pusher);
        }

        public override void Added(Scene scene)
        {
            base.Added(scene);
            Add(light = new VertexLight(Color.OrangeRed, 1, 24, 40));
            Add(bloom = new BloomPoint(1f, 16f));
        }

        public override void Update()
        {
            base.Update();
            if (SceneAs<Level>().CollideCheck<Solid>(Position + new Vector2(0, Height + 1)))
            {
                Solid solid = SceneAs<Level>().CollideFirst<Solid>(Position + new Vector2(0, Height + 1));
                if (solid != null)
                {
                    imageOffset = solid.Shake;
                }
            }
            MoveH(Speed.X * Engine.DeltaTime, onCollideH);
            MoveV(Speed.Y * Engine.DeltaTime, onCollideV);
            if (Scene.OnInterval(0.25f))
            {
                int count = (int)Math.Floor(Width * Height / 256f);
                SceneAs<Level>().ParticlesFG.Emit(P_Steam, count, Center, new Vector2(Width / 2, Height / 2), -(float)Math.PI / 2f);
            }
            if (CollideCheck<ExplosiveBoulder>() || CollideCheck<CrystalStaticSpinner>() || CollideCheck<CustomSpinner>())
            {
                Explode();
            }
        }

        private void onPlayer(Player player)
        {
            if (!explode)
            {
                if (player.StateMachine.State != Player.StDash && !player.DashAttacking)
                {
                    player.Die((player.Position - Position).SafeNormalize());
                }
                else
                {
                    Vector2 vector2 = ExplodeLaunch(player, Position, false);
                    SceneAs<Level>().DirectionalShake(vector2, 0.15f);
                    SceneAs<Level>().Displacement.AddBurst(Center, 0.3f, 8f, 32f, 0.8f);
                    SceneAs<Level>().Particles.Emit(Bumper.P_Launch, 12, Center + vector2 * 12f, Vector2.One * 3f, vector2.Angle());
                    Explode();
                }
            }
        }

        public Vector2 ExplodeLaunch(Player player, Vector2 from, bool snapUp = true, bool sidesOnly = false)
        {
            Input.Rumble(RumbleStrength.Strong, RumbleLength.Medium);
            Celeste.Freeze(0.1f);
            Player_launchApproachX.SetValue(player, null);
            Vector2 vector = (player.Center - from).SafeNormalize(-Vector2.UnitY);
            float num = Vector2.Dot(vector, Vector2.UnitY);
            if (snapUp && num <= -0.7f)
            {
                vector.X = 0f;
                vector.Y = -1f;
            }
            else if (num <= 0.65f && num >= -0.55f)
            {
                vector.Y = 0f;
                vector.X = Math.Sign(vector.X);
            }

            if (sidesOnly && vector.X != 0f)
            {
                vector.Y = 0f;
                vector.X = Math.Sign(vector.X);
            }

            player.Speed = BounceForce * vector;
            if (player.Speed.Y <= 50f)
            {
                player.Speed.Y = Math.Min(-150f, player.Speed.Y);
                player.AutoJump = true;
            }

            SlashFx.Burst(player.Center, player.Speed.Angle());
            if (!player.Inventory.NoRefills)
            {
                if (!RefillJump)
                {
                    XaphanModule.refillJumps = false;
                }
                player.RefillDash();
            }

            player.RefillStamina();
            Player_dashCooldownTimer.SetValue(player, DashCooldown);
            player.StateMachine.State = 7;
            return vector;
        }

        private void onEnemy(Enemy enemy)
        {
            Explode();
            enemy.Die();
        }

        private void onSpring(Spring spring)
        {
            if (!explode)
            {
                Gravity = true;
                if (spring.Orientation == Spring.Orientations.Floor)
                {
                    Spring_BounceAnimate.Invoke(spring, null);
                    Bottom = spring.Top;
                    Push(new Vector2(100, -235), Speed.X < 0 ? -Vector2.UnitX : (Speed.X > 0 ? Vector2.UnitX : Vector2.Zero));
                }
                else if (spring.Orientation == Spring.Orientations.WallLeft)
                {
                    Spring_BounceAnimate.Invoke(spring, null);
                    Push(new Vector2(200, -150), Vector2.UnitX);
                }
                else if (spring.Orientation == Spring.Orientations.WallRight)
                {
                    Spring_BounceAnimate.Invoke(spring, null);
                    Push(new Vector2(200, -150), -Vector2.UnitX);
                }
            }
        }

        private void HitByBeam(Beam beam)
        {
            beam.CollideImmune(beam.Direction);
        }

        private void HitByMissile(Missile missile)
        {
            if (!explode)
            {
                Gravity = true;
                missile.CollideSolid(missile.Direction);
                if (missile.SuperMissile)
                {
                    Explode();
                }
                else
                {
                    foreach (Entity entity in Scene.Tracker.GetEntities<CustomSpinner.Filler>())
                    {
                        CustomSpinner.Filler filler = (CustomSpinner.Filler)entity;
                        if (CollideCheck(filler))
                        {
                            filler.RemoveSelf();
                        }
                    }
                    if (missile.Direction.Y == 0)
                    {
                        Push(new Vector2(200, -100), missile.Direction);
                    }
                    else
                    {
                        Push(new Vector2(100, -235), Speed.X < 0 ? -Vector2.UnitX : (Speed.X > 0 ? Vector2.UnitX : Vector2.Zero));
                    }
                }
            }
        }

        private void OnCollideH(CollisionData data)
        {
            Explode();
        }

        private void OnCollideV(CollisionData data)
        {
            if (ImmunityTimer <= 0)
            {
                Explode();
            }
        }

        protected override void OnSquish(CollisionData data)
        {
            Explode();
        }

        public void HitSpinner(Entity spinner)
        {
            Explode();
        }

        public void Explode()
        {
            if (!explode)
            {
                foreach (Entity entity in Scene.Tracker.GetEntities<CustomSpinner.Filler>())
                {
                    CustomSpinner.Filler filler = (CustomSpinner.Filler)entity;
                    if (CollideCheck(filler))
                    {
                        filler.RemoveSelf();
                    }
                }
                AllowPushing = false;
                explode = true;
                light.Visible = false;
                bloom.Visible = false;
                Speed = Vector2.Zero;
                Sprite.Position -= new Vector2(20);
                Audio.Play("event:/new_content/game/10_farewell/puffer_splode", Position);
                Sprite.Play("explode", false);
                Sprite.OnLastFrame = delegate
                {
                    Visible = false;
                    RemoveSelf();
                };
                Collider = new Circle(24f);
                foreach (CrystalStaticSpinner crystalSpinner in Scene.Tracker.GetEntities<CrystalStaticSpinner>())
                {
                    if (CollideCheck(crystalSpinner))
                    {
                        crystalSpinner.Destroy();
                    }
                }
                foreach (CustomSpinner spinner in Scene.Tracker.GetEntities<CustomSpinner>())
                {
                    if (spinner.CanDestroy)
                    {
                        if (CollideCheck(spinner))
                        {
                            spinner.Destroy();
                        }
                    }
                }
                for (float num = 0f; num < (float)Math.PI * 4f; num += 0.17453292f)
                {
                    Vector2 position = Center + Calc.AngleToVector(num + Calc.Random.Range(-(float)Math.PI / 90f, (float)Math.PI / 90f), Calc.Random.Range(12, 18));
                    SceneAs<Level>().Particles.Emit(P_Explode, position, num);
                }
            }
        }

        public void Push(Vector2 force, Vector2 direction)
        {
            Speed = new Vector2(force.X * direction.X, force.Y);
        }

        public IEnumerator GravityRoutine()
        {
            while (!Gravity || OnGround())
            {
                yield return null;
            }
            ImmunityTimer = 0.15f;
            while (!explode && !OnGround())
            {
                Speed.Y = Calc.Approach(Speed.Y, 150f, 800f * Engine.DeltaTime);
                ImmunityTimer -= Engine.DeltaTime;
                yield return null;
            }
            Add(new Coroutine(GravityRoutine()));
        }

        public override void Render()
        {
            Vector2 position = Sprite.Position;
            Sprite.Position += imageOffset;
            Sprite.DrawOutline();
            base.Render();
            Sprite.Position = position;
        }
    }
}
