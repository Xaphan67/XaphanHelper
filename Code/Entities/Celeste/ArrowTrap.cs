using System;
using System.Collections;
using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste.Mod.XaphanHelper.Entities
{
    [Tracked(true)]
    [CustomEntity("XaphanHelper/ArrowTrap")]
    class ArrowTrap : Solid
    {
        public class CollideDetector : Entity
        {
            public ArrowTrap ArrowTrap;

            public Vector2 Offset;

            private float colliderWidth;

            private float colliderHeight;

            private float colliderLeft;

            private float colliderTop;

            public bool DetectedPlayer;

            public CollideDetector(ArrowTrap trap) : base(trap.Position)
            {
                ArrowTrap = trap;
                if (ArrowTrap.side == "Left")
                {
                    Offset = new Vector2(5f, 6f);
                }
                else if (ArrowTrap.side == "Right")
                {
                    Offset = new Vector2(3f, 6f);
                }
                else if (ArrowTrap.side == "Top")
                {
                    Offset = new Vector2(6f, 5f);
                }
                else if (ArrowTrap.side == "Bottom")
                {
                    Offset = new Vector2(6f, 3f);
                }
                Position = ArrowTrap.Position + Offset;
            }

            public override void Added(Scene scene)
            {
                base.Added(scene);
                if (ArrowTrap.side == "Left")
                {
                    Collider = new Hitbox(Right - SceneAs<Level>().Bounds.Left, 4f, -(Right - SceneAs<Level>().Bounds.Left), 0f);
                }
                else if (ArrowTrap.side == "Right")
                {
                    Collider = new Hitbox(SceneAs<Level>().Bounds.Right - Left, 4f);
                }
                else if (ArrowTrap.side == "Top")
                {
                    Collider = new Hitbox(4f, Bottom - SceneAs<Level>().Bounds.Top, 0f, -(Bottom - SceneAs<Level>().Bounds.Top));
                }
                else if (ArrowTrap.side == "Bottom")
                {
                    Collider = new Hitbox(4f, SceneAs<Level>().Bounds.Bottom - Top);
                }
            }

            public override void Update()
            {
                base.Update();
                foreach (PlayerPlatform plateform in SceneAs<Level>().Tracker.GetEntities<PlayerPlatform>())
                {
                    plateform.Collidable = false;
                }
                base.Update();
                Position = ArrowTrap.Position + Offset;
                if (ArrowTrap.side == "Left")
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
                            while (!CollideCheck<Solid>(Position - Vector2.UnitX) && Collider.Width < (Right - SceneAs<Level>().Bounds.Left))
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
                    if (Collider.Width < (Right - SceneAs<Level>().Bounds.Left))
                    {
                        Collider.Left -= 4;
                        Collider.Width += 4;
                    }
                }
                else if (ArrowTrap.side == "Right")
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
                            while (!CollideCheck<Solid>(Position + Vector2.UnitX) && Collider.Width < SceneAs<Level>().Bounds.Right - Left)
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
                    if (Collider.Width < SceneAs<Level>().Bounds.Right - Left)
                    {
                        Collider.Width += 4;
                    }
                }
                else if (ArrowTrap.side == "Top")
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
                            while (!CollideCheck<Solid>(Position - Vector2.UnitX) && Collider.Height < Bottom - SceneAs<Level>().Bounds.Top)
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
                    if (Collider.Height < Bottom - SceneAs<Level>().Bounds.Top)
                    {
                        Collider.Top -= 4;
                        Collider.Height += 4;
                    }
                }
                else if (ArrowTrap.side == "Bottom")
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
                            while (!CollideCheck<Solid>(Position + Vector2.UnitX) && Collider.Height < SceneAs<Level>().Bounds.Bottom - Top)
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
                    if (Collider.Height < SceneAs<Level>().Bounds.Bottom - Top)
                    {
                        Collider.Height += 4;
                    }
                }
                Player player = SceneAs<Level>().Tracker.GetEntity<Player>();
                if (player != null)
                {
                    DetectedPlayer = player.CollideCheck(this) ? true : false;
                }
                foreach (PlayerPlatform plateform in SceneAs<Level>().Tracker.GetEntities<PlayerPlatform>())
                {
                    plateform.Collidable = true;
                }
            }

            public override void DebugRender(Camera camera)
            {

            }
        }
        
        public string side;

        Sprite sprite;

        PlayerCollider pc;

        public Arrow nextArrow;

        public float mainCooldown;

        public CollideDetector CollideDetect;

        private StaticMover staticMover;

        public string mode;

        public float cooldown;

        public float initialDelay;

        public string flag;

        public bool onlyOnce;

        public bool active;

        public Vector2 spriteOffset;

        public Coroutine SequenceRoutine = new();

        public ArrowTrap(EntityData data, Vector2 position) : base(data.Position + position, data.Width, data.Height, false)
        {
            mode = data.Attr("mode", "Triggered");
            side = data.Attr("side", "Right");
            cooldown = data.Float("cooldown", 1f);
            initialDelay = data.Float("initialDelay", 1f);
            flag = data.Attr("flag");
            onlyOnce = data.Bool("onlyOnce");
            Add(sprite = new Sprite(GFX.Game, "objects/XaphanHelper/ArrowTrap" + "/"));
            sprite.AddLoop("idle", "idle", 0.08f);
            sprite.Origin = new Vector2(sprite.Width / 2, sprite.Height / 2);
            sprite.Play("idle");
            Add(new ClimbBlocker(true));
            staticMover = new StaticMover();
            staticMover.OnAttach = delegate (Platform p)
            {
                Depth = p.Depth + 1;
            };
            staticMover.OnMove = OnMove;
            staticMover.OnShake = onShake;
            staticMover.SolidChecker = IsRiding;
            staticMover.JumpThruChecker = IsRiding;
            if (side == "Right")
            {
                Collider = new Hitbox(3f, 10f, 0f, 3f);
                Add(pc = new PlayerCollider(OnPlayer, new Hitbox(3f, 8f, 3f, 4f)));
                sprite.Position = new Vector2(4f, 8f);
            }
            else if (side == "Left")
            {
                Collider = new Hitbox(3f, 10f, 5f, 3f);
                Add(pc = new PlayerCollider(OnPlayer, new Hitbox(3f, 8f, 2f, 4f)));
                sprite.Position = new Vector2(4f, 8f);
                sprite.FlipX = true;
            }
            else if (side == "Top")
            {
                Collider = new Hitbox(10f, 3f, 3f, 5f);
                Add(pc = new PlayerCollider(OnPlayer, new Hitbox(8f, 3f, 4f, 2f)));
                sprite.Position = new Vector2(8f, 4f);
                sprite.Rotation = -(float)Math.PI / 2f;
            }
            else if (side == "Bottom")
            {
                Collider = new Hitbox(10f, 3f, 3f, 0f);
                Add(pc = new PlayerCollider(OnPlayer, new Hitbox(8f, 3f, 4f, 3f)));
                sprite.Position = new Vector2(8f, 4f);
                sprite.Rotation = -(float)Math.PI / 2f;
                sprite.FlipX = true;
            }
            Add(staticMover);
        }

        private bool IsRiding(Solid solid)
        {
            if (solid.GetType() != typeof(Arrow))
            {
                if (side == "Right")
                {
                    return CollideCheck(solid, Position - Vector2.UnitX);
                }
                else if (side == "Left")
                {
                    return CollideCheck(solid, Position + Vector2.UnitX);
                }
                else if (side == "Top")
                {
                    return CollideCheck(solid, Position + Vector2.UnitY);
                }
                else if (side == "Bottom")
                {
                    return CollideCheck(solid, Position - Vector2.UnitY);
                }
            }
            return false;
        }

        private bool IsRiding(JumpThru jumpThru)
        {
            if (side == "Right")
            {
                return CollideCheck(jumpThru, Position - Vector2.UnitX);
            }
            else if (side == "Left")
            {
                return CollideCheck(jumpThru, Position + Vector2.UnitX);
            }
            else if (side == "Top")
            {
                return CollideCheck(jumpThru, Position + Vector2.UnitY);
            }
            else if (side == "Bottom")
            {
                return CollideCheck(jumpThru, Position - Vector2.UnitY);
            }
            return false;
        }

        public override void Added(Scene scene)
        {
            base.Added(scene);
            SceneAs<Level>().Add(CollideDetect = new CollideDetector(this));
            if (side == "Right" || side == "Left")
            {
                Scene.Add(nextArrow = new Arrow(new EntityData(), Position + new Vector2(-11, 4), this, side)
                {
                    canShoot = true
                });
            }
            else
            {
                Scene.Add(nextArrow = new Arrow(new EntityData(), Position + new Vector2(4, -11), this, side)
                {
                    canShoot = true
                });
            }
        }

        private void OnMove(Vector2 amount)
        {
            Position += amount;
            if (nextArrow != null)
            {
                nextArrow.Position += amount;
            }
        }

        private void onShake(Vector2 amount)
        {
            spriteOffset += amount;
        }

        public override void Update()
        {
            base.Update();
            if (mainCooldown > 0)
            {
                mainCooldown -= Engine.DeltaTime;
            }
            DisplacePlayerOnTop();
            if (string.IsNullOrEmpty(flag) || SceneAs<Level>().Session.GetFlag(flag) && active)
            {
                Player player = SceneAs<Level>().Tracker.GetEntity<Player>();
                if (player != null)
                {
                    if (mode == "Triggered")
                    {
                        if (CollideDetect.DetectedPlayer && nextArrow != null && nextArrow.canShoot && mainCooldown <= 0f)
                        {
                            Add(new Coroutine(nextArrow.Shoot()));
                            nextArrow = null;
                            if (!onlyOnce)
                            {
                                Add(new Coroutine(ReloadArrow()));
                            }
                            else
                            {
                                active = false;
                            }
                        }
                    }
                    else if (mode == "Automatic" && nextArrow != null && nextArrow.canShoot && !SequenceRoutine.Active)
                    {
                        Add(SequenceRoutine = new Coroutine(Sequence()));
                    }
                }
            }
        }

        public IEnumerator Sequence()
        {
            yield return initialDelay;
            while (string.IsNullOrEmpty(flag) || SceneAs<Level>().Session.GetFlag(flag))
            {
                Add(new Coroutine(nextArrow.Shoot()));
                nextArrow = null;
                Add(new Coroutine(ReloadArrow()));
                yield return cooldown;
            }
        }

        public IEnumerator ReloadArrow()
        {
            float cooldown = 0.2f;
            while (cooldown > 0)
            {
                cooldown -= Engine.DeltaTime;
                yield return null;
            }
            mainCooldown = mode == "Triggered" ? this.cooldown - 0.2f : 0f;
            while (mainCooldown > 0.5f)
            {
                mainCooldown -= Engine.DeltaTime;
                yield return null;
            }
            if (side == "Right")
            {
                Scene.Add(nextArrow = new Arrow(new EntityData(), Position + new Vector2(-13, 4), this, side));
            }
            else if (side == "Left")
            {
                Scene.Add(nextArrow = new Arrow(new EntityData(), Position + new Vector2(-9, 4), this, side));
            }
            else if (side == "Top")
            {
                Scene.Add(nextArrow = new Arrow(new EntityData(), Position + new Vector2(4, 1), this, side));
            }
            else if (side == "Bottom")
            {
                Scene.Add(nextArrow = new Arrow(new EntityData(), Position + new Vector2(4, -23), this, side));
            }
            Add(new Coroutine(nextArrow.Reload()));
            while (mainCooldown > 0f)
            {
                mainCooldown -= Engine.DeltaTime;
                yield return null;
            }
            mainCooldown = 0f;
        }

        public void OnPlayer(Player player)
        {
            player.Die(new Vector2(0f, -1f));
        }

        private void DisplacePlayerOnTop()
        {
            if (!HasPlayerOnTop())
            {
                return;
            }
            Player player = GetPlayerOnTop();
            if (player == null)
            {
                return;
            }
            else if (player.Bottom == Top && player.Speed.Y >= 0)
            {
                if (player.Left >= Left)
                {
                    player.Left = Right;
                    player.Y += 1f;
                }
                if (player.Right <= Right)
                {
                    player.Right = Left;
                    player.Y += 1f;
                }
            }
        }

        public override void Render()
        {
            Vector2 position = Position;
            Position += spriteOffset;
            base.Render();
            Position = position;
        }
    }
}
