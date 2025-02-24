using System;
using Celeste.Mod.Entities;
using Celeste.Mod.XaphanHelper.Entities;
using Microsoft.Xna.Framework;
using Mono.Cecil.Cil;
using Monocle;
using MonoMod.Cil;

namespace Celeste.Mod.XaphanHelper.Enemies
{
    [Tracked(true)]
    [CustomEntity("XaphanHelper/Skultera")]
    public class Skultera : Enemy
    {
        public class CollideDetector : Entity
        {
            private struct Particle
            {
                public Vector2 Position;

                public float Percent;

                public float Duration;

                public Vector2 Direction;

                public float Speed;

                public float Spin;

                public int Color;
            }

            private static readonly Color[] _colors =
            {
                Calc.HexToColor("808080"),
                Calc.HexToColor("545151"),
                Calc.HexToColor("ada5a5")
            };

            private enum Direction
            {
                Up,
                Down,
                Left,
                Right
            }

            public Skultera Skultera;

            public Vector2 Offset;

            private float colliderWidth;

            private float colliderHeight;

            private float colliderLeft;

            private float colliderTop;

            public bool DetectedPlayer;

            private Particle[] particles;

            private Vector2 scale = Vector2.One;

            private Vector2 actualSuctionSpeed;

            public CollideDetector(Skultera skultera) : base(skultera.Position)
            {
                Skultera = skultera;
                if (Skultera.direction == Skultera.Direction.Left)
                {
                    Offset = new Vector2(0f, 2f);
                }
                else if(Skultera.direction == Skultera.Direction.Right)
                {
                    Offset = new Vector2(10f, 2f);
                }
                else if (Skultera.direction == Skultera.Direction.Up)
                {
                    Offset = new Vector2(6f, 0f);
                }
                else if (Skultera.direction == Skultera.Direction.Down)
                {
                    Offset = new Vector2(6f, 8f);
                }
                Position = Skultera.Position + Offset;
                Depth = Skultera.Depth + 1;
            }

            private void Reset(int i, float p)
            {
                particles[i].Percent = p;
                particles[i].Position = new Vector2(Calc.Random.Range(0f, Collider.Width), Calc.Random.Range(0f, Collider.Height));
                particles[i].Speed = Calc.Random.Range(4, 14);
                particles[i].Spin = Calc.Random.Range(0.25f, (float)Math.PI * 6f);
                particles[i].Duration = Calc.Random.Range(1f, 4f);
                particles[i].Direction = Calc.AngleToVector(Calc.Random.NextFloat((float)Math.PI * 2f), 1f);
                particles[i].Color = Calc.Random.Next(_colors.Length);
            }

            public override void Added(Scene scene)
            {
                base.Added(scene);
                if (Skultera.direction == Skultera.Direction.Left)
                {
                    Collider = new Hitbox(Skultera.maxRange * 8, 16f, -Skultera.maxRange * 8, -5f);
                }
                else if (Skultera.direction == Skultera.Direction.Right)
                {
                    Collider = new Hitbox(Skultera.maxRange * 8, 16f, -2f, -5f);
                }
                else if (Skultera.direction == Skultera.Direction.Up)
                {
                    Collider = new Hitbox(16f, Skultera.maxRange * 8, -9f, -Skultera.maxRange * 8);
                }
                else if (Skultera.direction == Skultera.Direction.Down)
                {
                    Collider = new Hitbox(16f, Skultera.maxRange * 8, -9f, 0f);
                }
                int particlecount = (int)Collider.Width * (int)Collider.Height / 150;
                particles = new Particle[particlecount];
                for (int i = 0; i < particles.Length; i++)
                {
                    Reset(i, Calc.Random.NextFloat(0.7f));
                }

                PositionParticles();
            }

            public override void Update()
            {
                base.Update();
                foreach (PlayerPlatform plateform in SceneAs<Level>().Tracker.GetEntities<PlayerPlatform>())
                {
                    plateform.Collidable = false;
                }
                base.Update();
                Position = Skultera.Position + Offset;
                bool horizontal = Skultera.direction == Skultera.Direction.Left || Skultera.direction == Skultera.Direction.Right;
                bool vertical = Skultera.direction == Skultera.Direction.Up || Skultera.direction == Skultera.Direction.Down;
                actualSuctionSpeed = new Vector2(horizontal ? Skultera.suctionStrength * Skultera.percent * (Skultera.direction == Skultera.Direction.Right ? -1 : 1) : 0f, vertical ? Skultera.suctionStrength * Skultera.percent * (Skultera.direction == Skultera.Direction.Down ? -1 : 1) : 0f);
                if (Skultera.direction == Skultera.Direction.Left)
                {
                    if (CollideCheck<Solid>() && !CollideCheck<PlayerPlatform>())
                    {
                        while (CollideCheck<Solid>())
                        {
                            Collider.Left += 1;
                            Collider.Width -= 1;
                            colliderLeft = Collider.Left;
                            colliderWidth = Collider.Width;
                        }
                    }
                    else
                    {
                        if (!CollideCheck<Solid>(Position - Vector2.UnitX))
                        {
                            while (!CollideCheck<Solid>(Position - Vector2.UnitX) && Collider.Width < (Skultera.maxRange * 8))
                            {
                                Collider.Left -= 1;
                                Collider.Width += 1;
                                colliderLeft = Collider.Left;
                                colliderWidth = Collider.Width;
                            }
                        }
                    }
                    if (CollideCheck<PlayerPlatform>())
                    {
                        Collider.Left = colliderLeft;
                        Collider.Width = colliderWidth;
                    }
                    if (Collider.Width < (Skultera.maxRange * 8))
                    {
                        Collider.Left -= 4;
                        Collider.Width += 4;
                    }
                }
                else if (Skultera.direction == Skultera.Direction.Right)
                {
                    if (CollideCheck<Solid>() && !CollideCheck<PlayerPlatform>())
                    {
                        while (CollideCheck<Solid>())
                        {
                            Collider.Width -= 1;
                            colliderWidth = Collider.Width;
                        }
                    }
                    else
                    {
                        if (!CollideCheck<Solid>(Position + Vector2.UnitX))
                        {
                            while (!CollideCheck<Solid>(Position + Vector2.UnitX) && Collider.Width < (Skultera.maxRange * 8))
                            {
                                Collider.Width += 1;
                                colliderWidth = Collider.Width;
                            }
                        }
                    }
                    if (CollideCheck<PlayerPlatform>())
                    {
                        Collider.Width = colliderWidth;
                    }
                    if (Collider.Width < (Skultera.maxRange * 8))
                    {
                        Collider.Width += 4;
                    }
                }
                else if (Skultera.direction == Skultera.Direction.Up)
                {
                    if (CollideCheck<Solid>() && !CollideCheck<PlayerPlatform>())
                    {
                        while (CollideCheck<Solid>())
                        {
                            Collider.Top += 1;
                            Collider.Height -= 1;
                            colliderTop = Collider.Top;
                            colliderHeight = Collider.Height;
                        }
                    }
                    else
                    {
                        if (!CollideCheck<Solid>(Position - Vector2.UnitX))
                        {
                            while (!CollideCheck<Solid>(Position - Vector2.UnitX) && Collider.Height < (Skultera.maxRange * 8))
                            {
                                Collider.Top -= 1;
                                Collider.Height += 1;
                                colliderTop = Collider.Top;
                                colliderHeight = Collider.Height;
                            }
                        }
                    }
                    if (CollideCheck<PlayerPlatform>())
                    {
                        Collider.Top = colliderTop;
                        Collider.Height = colliderHeight;
                    }
                    if (Collider.Height < (Skultera.maxRange * 8))
                    {
                        Collider.Top -= 4;
                        Collider.Height += 4;
                    }
                }
                else if (Skultera.direction == Skultera.Direction.Down)
                {
                    if (CollideCheck<Solid>() && !CollideCheck<PlayerPlatform>())
                    {
                        while (CollideCheck<Solid>())
                        {
                            Collider.Height -= 1;
                            colliderHeight = Collider.Height;
                        }
                    }
                    else
                    {
                        if (!CollideCheck<Solid>(Position + Vector2.UnitX))
                        {
                            while (!CollideCheck<Solid>(Position + Vector2.UnitX) && Collider.Height < (Skultera.maxRange * 8))
                            {
                                Collider.Height += 1;
                                colliderHeight = Collider.Height;
                            }
                        }
                    }
                    if (CollideCheck<PlayerPlatform>())
                    {
                        Collider.Height = colliderHeight;
                    }
                    if (Collider.Height < (Skultera.maxRange * 8))
                    {
                        Collider.Height += 4;
                    }
                }

                PositionParticles();
                DetectedPlayer = CollideCheck<Player>();
                foreach (PlayerPlatform plateform in SceneAs<Level>().Tracker.GetEntities<PlayerPlatform>())
                {
                    plateform.Collidable = true;
                }
            }

            public override void DebugRender(Camera camera)
            {

            }

            public override void Render()
            {
                for (int i = 0; i < particles.Length; i++)
                {
                    
                    Vector2 particlePosition = default(Vector2);
                    particlePosition.X = mod(particles[i].Position.X, Collider.Width);
                    particlePosition.Y = mod(particles[i].Position.Y, Collider.Height);
                    float percent = particles[i].Percent;
                    float num = 0f;
                    num = ((!(percent < 0.7f)) ? Calc.ClampedMap(percent, 0.7f, 1f, 1f, 0f) : Calc.ClampedMap(percent, 0f, 0.3f));
                    if (actualSuctionSpeed != Vector2.Zero)
                    {
                        Vector2 adjust = Vector2.Zero;
                        if (Skultera.direction == Skultera.Direction.Left || Skultera.direction == Skultera.Direction.Right)
                        {
                            adjust = Skultera.direction == Skultera.Direction.Left ? new Vector2(Collider.Width, 5f) : new Vector2(4f, 5f);
                        }
                        else
                        {
                            adjust = Skultera.direction == Skultera.Direction.Down ? new Vector2(6f, 3f) : new Vector2(6f, Collider.Height);
                        }
                        Draw.Rect(particlePosition + Position - adjust, scale.X, scale.Y, _colors[particles[i].Color] * num);
                    }
                }
            }

            private void PositionParticles()
            {
                bool num = actualSuctionSpeed.Y == 0f;
                Vector2 zero = Vector2.Zero;
                if (num)
                {
                    scale.X = Math.Max(1f, Math.Abs(actualSuctionSpeed.X) / 40f);
                    scale.Y = 1f;
                    zero = new Vector2(actualSuctionSpeed.X, 0f);
                }
                else
                {
                    scale.X = 1f;
                    scale.Y = Math.Max(1f, Math.Abs(actualSuctionSpeed.Y) / 40f);
                    zero = new Vector2(0f, actualSuctionSpeed.Y);
                }
                for (int i = 0; i < particles.Length; i++)
                {
                    if (particles[i].Percent >= 1f)
                    {
                        Reset(i, 0f);
                    }
                    particles[i].Percent += Engine.DeltaTime / particles[i].Duration;
                    particles[i].Position += (particles[i].Direction * particles[i].Speed + zero) * Engine.DeltaTime;
                    particles[i].Direction.Rotate(particles[i].Spin * Engine.DeltaTime);
                }
            }

            private float mod(float x, float m)
            {
                return (x % m + m) % m;
            }
        }

        private enum Direction
        {
            Up,
            Down,
            Left,
            Right
        }

        private Direction direction;

        public CollideDetector CollideDetect;

        private Sprite Body;

        public float maxRange;

        public float suctionStrength;

        private bool sucking;

        public Skultera(EntityData data, Vector2 offset) : base(data, offset)
        {
            maxRange = data.Float("maxRange", 14f);
            direction = data.Enum("direction", Direction.Left);
            suctionStrength = data.Float("suctionStrength", 150f);
            Body = new Sprite(GFX.Game, "enemies/Xaphan/Skultera/");
            Body.Add("idle", "idle", 0f);
            Body.Add("active", "active", 0.12f);
            Body.Add("activeStop", "active", 0.12f, 2, 1, 0);
            Body.Play("idle");
            Body.CenterOrigin();
            switch (direction)
            {
                case Direction.Left:
                    Collider = new Hitbox(11f, 22f, -3f, -11f);
                    Body.Position.X = -2f;
                    break;
                case Direction.Right:
                    Collider = new Hitbox(11f, 22f, 0f, -11f);
                    Body.FlipX = true;
                    Body.Position.X = 2f;
                    break;
                case Direction.Up:
                    Collider = new Hitbox(22f, 11f, -11f, -3f);
                    Body.Rotation = (float)Math.PI / 2;
                    Body.FlipY = true;
                    Body.Position.Y = -2f;
                    break;
                case Direction.Down:
                    Collider = new Hitbox(22f, 11f, -11f, 0f);
                    Body.Rotation = -(float)Math.PI / 2;
                    Body.Position.Y = 2f;
                    break;
            }
            Body.Position += Vector2.One * 4;
            sprites.Add(Body);
            foreach (Sprite sprite in sprites)
            {
                Add(sprite);
            }
        }

        public override void Added(Scene scene)
        {
            base.Added(scene);
            SceneAs<Level>().Add(CollideDetect = new CollideDetector(this));
        }

        float percent = 0f;

        public override void Update()
        {
            base.Update();
            Player player = SceneAs<Level>().Tracker.GetEntity<Player>();
            if (player != null && CollideDetect.DetectedPlayer)
            {
                if (!Body.CurrentAnimationID.Contains("active") && !sucking)
                {
                    Audio.Play("event:/game/05_mirror_temple/seeker_aggro", Position);
                    sucking = true;
                    Body.Play("active");
                }
                percent = Calc.Approach(percent, 1f, Engine.DeltaTime / 0.5f);
                if (player.StateMachine.State != Player.StRedDash && player.StateMachine.State != Player.StDash && player.StateMachine.State != Player.StClimb && !player.DashAttacking)
                {
                    bool attemptToGrabSolid = (player.Facing == Facings.Left && player.CollideCheck<Solid>(player.Position - Vector2.UnitX * 3) || player.Facing == Facings.Right && player.CollideCheck<Solid>(player.Position + Vector2.UnitX * 3)) && Input.GrabCheck;
                    if (direction == Direction.Left || direction == Direction.Right)
                    {
                        if (!attemptToGrabSolid)
                        {
                            player.Speed = Vector2.Zero;
                            player.MoveH(suctionStrength * (direction == Direction.Right ? -1 : 1) * Engine.DeltaTime * Ease.CubeInOut(percent));
                            player.MoveTowardsY(CollideDetect.Center.Y + 10f, 22.5f * Engine.DeltaTime);
                        }
                    }
                    else if (direction == Direction.Up || direction == Direction.Down)
                    {
                        if (!attemptToGrabSolid)
                        {
                            player.Speed = Vector2.Zero;
                            player.MoveV(suctionStrength * (direction == Direction.Down ? -1 : 1) * Engine.DeltaTime * Ease.CubeInOut(percent));
                            player.MoveTowardsX(CollideDetect.Center.X, 22.5f * Engine.DeltaTime);
                        }
                    }
                }
            }
            else
            {
                if (Body.CurrentAnimationID != "activeStop" && Body.CurrentAnimationID != "idle")
                {
                    Body.Play("activeStop");
                    Body.OnLastFrame = delegate
                    {
                        Body.Play("idle");
                        sucking = false;
                    };
                }
                percent = Calc.Approach(percent, 0f, Engine.DeltaTime / 0.5f);
            }
        }
    }
}
