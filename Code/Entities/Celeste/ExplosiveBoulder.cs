using Monocle;
using Microsoft.Xna.Framework;
using Celeste.Mod.Entities;
using System.Collections;
using Celeste.Mod.XaphanHelper.Colliders;
using System.Reflection;
using System;

namespace Celeste.Mod.XaphanHelper.Entities
{
    [Tracked(true)]
    [CustomEntity("XaphanHelper/ExplosiveBoulder")]
    class ExplosiveBoulder : Actor
    {
        private static MethodInfo Spring_BounceAnimate = typeof(Spring).GetMethod("BounceAnimate", BindingFlags.Instance | BindingFlags.NonPublic);

        private Sprite Sprite;

        private string Directory;

        public Vector2 Speed;

        private Collision onCollide;

        private bool Gravity;

        private bool explode;

        private VertexLight light;

        private BloomPoint bloom;

        private ParticleType P_Explode;

        public ExplosiveBoulder(EntityData data, Vector2 offset) : base(data.Position + offset)
        {
            Collider = new Circle(12f);
            Directory = data.Attr("directory");
            Gravity = data.Bool("gravity");
            if (string.IsNullOrEmpty(Directory))
            {
                Directory = "objects/XaphanHelper/ExplosiveBoulder";
            }
            Add(new PlayerCollider(onPlayer, Collider));
            Add(new SpringCollider(onSpring, Collider));
            Add(Sprite = new Sprite(GFX.Game, Directory + "/"));
            Sprite.AddLoop("idleA", "boulder", 0.08f, 0);
            Sprite.AddLoop("idleB", "boulder", 0.08f, 1);
            Sprite.AddLoop("explode", "explode", 0.08f);
            Sprite.CenterOrigin();
            Sprite.Position += new Vector2(20);
            Sprite.Play("idle" + (Calc.Random.Next(2) == 1 ? "A" : "B"));
            onCollide = OnCollide;
            Add(new Coroutine(GravityRoutine()));
            Add(light = new VertexLight(Center, Color.Red, 1f, 16, 32));
            Add(bloom = new BloomPoint(0.5f, 16f));
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
        }

        public override void Update()
        {
            base.Update();
            MoveH(Speed.X * Engine.DeltaTime, onCollide);
            MoveV(Speed.Y * Engine.DeltaTime, onCollide);
            if (CollideCheck<ExplosiveBoulder>() || CollideCheck<CrystalStaticSpinner>() || CollideCheck<CustomSpinner>() || (Scene.CollideCheck<Solid>(new Rectangle((int)(Position.X - Width / 2), (int)(Position.Y - Height / 2), (int)Width, (int)Height)) && Speed.Y > 0))
            {
                Explode();
            }
        }

        private void onPlayer(Player player)
        {
            if (player.StateMachine.State != Player.StDash && !player.DashAttacking)
            {
                player.Die((player.Position - Position).SafeNormalize());
            }
            else
            {
                Vector2 vector2 = player.ExplodeLaunch(Position, false);
                SceneAs<Level>().DirectionalShake(vector2, 0.15f);
                SceneAs<Level>().Displacement.AddBurst(Center, 0.3f, 8f, 32f, 0.8f);
                SceneAs<Level>().Particles.Emit(Bumper.P_Launch, 12, Center + vector2 * 12f, Vector2.One * 3f, vector2.Angle());
                Explode();
            }
        }

        private void onSpring(Spring spring)
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
                Push(new Vector2(200, -180), Vector2.UnitX);
            }
            else if (spring.Orientation == Spring.Orientations.WallRight)
            {
                Spring_BounceAnimate.Invoke(spring, null);
                Push(new Vector2(200, -180), -Vector2.UnitX);
            }
        }

        private void OnCollide(CollisionData data)
        {
            Explode();
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
                foreach (Entity entity in Scene.Tracker.GetEntities<CrystalStaticSpinner>())
                {
                    CrystalStaticSpinner crystalSpinner = (CrystalStaticSpinner)entity;
                    if (CollideCheck(crystalSpinner))
                    {
                        crystalSpinner.Destroy();
                    }
                }
                foreach (Entity entity in Scene.Tracker.GetEntities<CustomSpinner>())
                {
                    CustomSpinner spinner = (CustomSpinner)entity;
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
            while (!Gravity || Scene.CollideCheck<Solid>(new Rectangle((int)(Position.X - Width / 2), (int)(Position.Y - Height / 2) + 1, (int)Width, (int)Height)))
            {
                yield return null;
            }
            while (!explode)
            {
                Speed.Y = Calc.Approach(Speed.Y, 150f, 800f * Engine.DeltaTime);
                yield return null;
            }
        }

        public override void Render()
        {
            Sprite.DrawOutline();
            base.Render();
        }
    }
}
