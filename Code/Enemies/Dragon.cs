using System.Collections;
using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste.Mod.XaphanHelper.Enemies
{
    [CustomEntity("XaphanHelper/Dragon")]
    public class Dragon : Enemy
    {
        [Tracked(true)]
        private class DragonFireball : Actor
        {
            Sprite Sprite;

            private PlayerCollider pc;

            public Vector2 Speed;

            public DragonFireball(Vector2 offset, Vector2 speed, bool toLeft) : base(offset)
            {
                Add(pc = new PlayerCollider(onCollidePlayer, new Circle(2f)));
                Add(Sprite = new Sprite(GFX.Game, "enemies/Xaphan/Dragon/"));
                Sprite.Origin = Vector2.One * 4;
                Sprite.AddLoop("fireball", "fireball", 0.06f);
                Sprite.Play("fireball");
                Speed = new Vector2(speed.X * (toLeft ? -1 : 1), speed.Y); // for random : new Vector2((speed.X + Calc.Random.Next(-15, 16)) * (toLeft ? -1 : 1), speed.Y + Calc.Random.Next(-15, 16));
                Add(new Coroutine(GravityRoutine()));
                Depth = 1;
            }

            public override void Added(Scene scene)
            {
                base.Added(scene);
                Audio.Play("event:/game/xaphan/torizo_fireball", Position);
            }

            public override void Update()
            {
                base.Update();
                MoveH(Speed.X * Engine.DeltaTime);
                MoveV(Speed.Y * Engine.DeltaTime);
                if (Center.X > SceneAs<Level>().Bounds.Right || Center.X < SceneAs<Level>().Bounds.Left || Center.Y > SceneAs<Level>().Bounds.Bottom)
                {
                    RemoveSelf();
                }
            }

            public IEnumerator GravityRoutine()
            {
                while (Speed.Y <= 250f)
                {
                    Speed.Y += 4f;
                    yield return null;
                }
            }

            private void onCollidePlayer(Player player)
            {
                player.Die((player.Position - Position).SafeNormalize());
            }

            public override void Render()
            {
                Sprite.DrawOutline();
                base.Render();
            }
        }

        private enum Facings
        {
            Left,
            Right
        }

        private Facings Facing;

        Sprite Body;

        Sprite Head;

        Coroutine MainRoutine = new();

        private float initialDelay;

        int StartHeight;

        public Vector2 Speed;

        private float IdleTimer;

        private int Fireballs;

        private float FireballTimer;

        public Dragon(EntityData data, Vector2 offset) : base(data, offset + Vector2.UnitY * 16)
        {
            initialDelay = data.Float("initialDelay");
            Collider = new Hitbox(13f, 34f, 3f, 3f);
            Health = 20;
            Damage = 20;
            pc.Collider = new Hitbox(13f, 34f, 3f, 3f);
            bc.Collider = new Hitbox(13f, 34f, 3f, 3f);
            Body = new Sprite(GFX.Game, "enemies/Xaphan/Dragon/");
            Body.AddLoop("body", "body", 0.1f);
            Body.Position += new Vector2(0f, 12f);
            Body.Play("body");
            sprites.Add(Body);
            Head = new Sprite(GFX.Game, "enemies/Xaphan/Dragon/");
            Head.Add("idle", "head", 0f, 0);
            Head.Add("shoot", "head", 0.08f, 1, 2, 3, 2, 1);
            Head.Play("idle");
            sprites.Add(Head);
            foreach (Sprite sprite in sprites)
            {
                Add(sprite);
            }
            StartHeight = (int)Position.Y;
            FireballTimer = data.Float("fireballTimer", 0.7f);
            Fireballs = data.Int("fireballs", 3);
            IdleTimer = data.Float("idleTimer", 2f);
            Add(MainRoutine = new Coroutine(Routine()));
        }

        public override void Update()
        {
            base.Update();
            Player player = SceneAs<Level>().Tracker.GetEntity<Player>();
            if (player != null && !Freezed)
            {
                foreach (Sprite sprite in sprites)
                {
                    sprite.FlipX = player.Center.X <= Center.X;
                }
                if (player.Center.X <= Center.X)
                {
                    Facing = Facings.Left;
                }
                else
                {
                    Facing = Facings.Right;
                }
            }
            NaiveMove(Vector2.UnitY * Speed.Y * Engine.DeltaTime);
            if (MainRoutine != null)
            {
                if (Freezed)
                {
                    MainRoutine.Active = false;
                    Speed.Y = 0f;
                }
                else
                {
                    MainRoutine.Active = true;
                }
            }
        }

        public IEnumerator Routine()
        {
            yield return initialDelay;
            while (true)
            {
                // Rise for 2 tiles

                while (Position.Y > StartHeight - 16)
                {
                    Speed.Y = -40f;
                    yield return null;
                }
                Speed.Y = 0f;

                // Wait

                yield return 0.4f;

                // Shoot fireballs

                for (int i = 0; i < Fireballs; i++)
                {
                    yield return ShootFireballRoutine();
                }

                // Wait

                yield return 0.3f;

                Head.Play("idle");

                yield return 0.7f;

                // Go back to starting position

                while (Position.Y < StartHeight)
                {
                    Speed.Y = 40f;
                    yield return null;
                }
                Speed.Y = 0f;

                // Wait

                yield return IdleTimer;
            }
        }

        public IEnumerator ShootFireballRoutine()
        {
            Head.Play("shoot");
            float timer = FireballTimer;
            float animationTime = 0.08f * Head.CurrentAnimationTotalFrames;
            yield return animationTime;
            timer -= animationTime;
            SceneAs<Level>().Add(new DragonFireball(new Vector2((Facing == Facings.Left ? TopLeft.X - 3f : TopRight.X + 3f), Top - 3f), new Vector2(110f, -140f), Facing == Facings.Left));
            yield return timer;
        }
    }
}
