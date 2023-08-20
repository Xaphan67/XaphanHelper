using System;
using Celeste.Mod.Entities;
using Celeste.Mod.XaphanHelper.Managers;
using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste.Mod.XaphanHelper.Entities
{
    [Tracked(true)]
    [CustomEntity("XaphanHelper/TimedDashSwitch")]
    class TimedDashSwitch : Solid
    {
        public enum Sides
        {
            Up,
            Down,
            Left,
            Right
        }

        private Sides side;

        private Vector2 pressedTarget;

        public bool pressed;

        private Vector2 pressDirection;

        private float speedY;

        private float speedX;

        private float startY;

        private float startX;

        private bool playerWasOn;

        private Sprite sprite;

        private string spriteName;

        private int timer;

        private string mode;

        private string TickingType;

        private string flag;

        public string particleColor1;

        public string particleColor2;

        public ParticleType P_PressA;

        public ParticleType P_PressB;

        private StaticMover staticMover;

        private Vector2 spriteOffset;

        private bool inWall;

        public TimedDashSwitch(EntityData data, Vector2 offset) : base(data.Position + offset, data.Width, data.Height, safe: true)
        {
            side = data.Enum<Sides>("side");
            spriteName = data.Attr("spriteName");
            timer = data.Int("timer", 10);
            mode = data.Attr("mode").ToLower();
            TickingType = data.Attr("tickingType").ToLower();
            flag = data.Attr("flag");
            particleColor1 = data.Attr("particleColor1");
            particleColor2 = data.Attr("particleColor2");
            inWall = data.Bool("inWall");
            if (string.IsNullOrEmpty(particleColor1))
            {
                particleColor1 = "99e550";
            }
            if (string.IsNullOrEmpty(particleColor2))
            {
                particleColor2 = "d9ffb5";
            }
            P_PressA = new ParticleType
            {
                Color = Calc.HexToColor(particleColor1),
                Color2 = Calc.HexToColor(particleColor2),
                ColorMode = ParticleType.ColorModes.Blink,
                Size = 1f,
                SizeRange = 0f,
                SpeedMin = 60f,
                SpeedMax = 80f,
                LifeMin = 0.8f,
                LifeMax = 1.2f,
                DirectionRange = 0.6981317f,
                SpeedMultiplier = 0.2f
            };
            P_PressB = new ParticleType(P_PressA)
            {
                SpeedMin = 100f,
                SpeedMax = 110f,
                DirectionRange = 0.34906584f
            };
            if (timer < 3)
            {
                timer = 3;
            }
            if (string.IsNullOrEmpty(mode))
            {
                mode = "add";
            }
            Add(sprite = GFX.SpriteBank.Create("dashSwitch_" + spriteName));
            sprite.Play("idle");
            if (side == Sides.Up || side == Sides.Down)
            {
                Collider.Width = 16f;
                Collider.Height = inWall ? 4f : 8f;
            }
            else
            {
                Collider.Width = inWall ? 4f : 8f;
                Collider.Height = 16f;
            }
            staticMover = new StaticMover();
            switch (side)
            {
                case Sides.Up:
                    staticMover.SolidChecker = ((Solid s) => CollideCheckOutside(s, Position + Vector2.UnitY * 4f));
                    sprite.Position = new Vector2(8f, 8f);
                    sprite.Rotation = (float)Math.PI / 2f;
                    if (inWall)
                    {
                        Position += Vector2.UnitY * 4f;
                        Add(new ClimbBlocker(edge: true));
                    }
                    pressedTarget = Position + Vector2.UnitY * (inWall ? 4f : 8f);
                    pressDirection = Vector2.UnitY;
                    startY = Y;
                    break;
                case Sides.Down:
                    staticMover.SolidChecker = ((Solid s) => CollideCheckOutside(s, Position - Vector2.UnitY * 4f));
                    sprite.Position = new Vector2(8f, inWall ? -4f : 0f);
                    sprite.Rotation = -(float)Math.PI / 2f;
                    if (inWall)
                    {
                        Add(new ClimbBlocker(edge: true));
                    }
                    pressedTarget = Position + Vector2.UnitY * -(inWall ? 4f : 8f);
                    pressDirection = -Vector2.UnitY;
                    startY = Y;
                    break;
                case Sides.Right:
                    staticMover.SolidChecker = ((Solid s) => CollideCheckOutside(s, Position + Vector2.UnitX * 2f));
                    sprite.Position = new Vector2(8f, 8f);
                    sprite.Rotation = 0f;
                    if (inWall)
                    {
                        Position += Vector2.UnitX * 4f;
                    }
                    pressedTarget = Position + Vector2.UnitX * (inWall ? 4f : 8f);
                    pressDirection = Vector2.UnitX;
                    startX = X;
                    break;
                case Sides.Left:
                    staticMover.SolidChecker = ((Solid s) => CollideCheckOutside(s, Position - Vector2.UnitX * 2f));
                    sprite.Position = new Vector2(inWall ? -4f : 0f, 8f);
                    sprite.Rotation = (float)Math.PI;
                    pressedTarget = Position + Vector2.UnitX * -(inWall ? 4f : 8f);
                    pressDirection = -Vector2.UnitX;
                    startX = X;
                    break;
            }
            OnDashCollide = OnDashed;
            staticMover.OnAttach = delegate (Platform p)
            {
                Depth = p.Depth + 1;
            };
            staticMover.OnShake = onShake;
            staticMover.OnMove = onMove;
            Add(staticMover);
        }

        public static void Load()
        {
            On.Celeste.Glider.OnCollideH += onGliderCollideH;
            On.Celeste.Seeker.SlammedIntoWall += onSeekerSlammedIntoWall;
            On.Celeste.TheoCrystal.OnCollideH += onTheoCrystalCollideH;
            On.Celeste.TheoCrystal.OnCollideV += onTheoCrystalCollideV;
        }

        public static void Unload()
        {
            On.Celeste.Glider.OnCollideH -= onGliderCollideH;
            On.Celeste.Seeker.SlammedIntoWall -= onSeekerSlammedIntoWall;
            On.Celeste.TheoCrystal.OnCollideH -= onTheoCrystalCollideH;
            On.Celeste.TheoCrystal.OnCollideV -= onTheoCrystalCollideV;
        }

        private static void onGliderCollideH(On.Celeste.Glider.orig_OnCollideH orig, Glider self, CollisionData data)
        {
            if (data.Hit is TimedDashSwitch)
            {
                (data.Hit as TimedDashSwitch).OnDashCollide(null, Vector2.UnitX * Math.Sign(self.Speed.X));
            }
            orig(self, data);
        }

        private static void onSeekerSlammedIntoWall(On.Celeste.Seeker.orig_SlammedIntoWall orig, Seeker self, CollisionData data)
        {
            if (data.Hit is TimedDashSwitch)
            {
                (data.Hit as TimedDashSwitch).OnDashCollide(null, Vector2.UnitX * Math.Sign(self.Speed.X));
            }
            orig(self, data);
        }

        private static void onTheoCrystalCollideH(On.Celeste.TheoCrystal.orig_OnCollideH orig, TheoCrystal self, CollisionData data)
        {
            if (data.Hit is TimedDashSwitch)
            {
                (data.Hit as TimedDashSwitch).OnDashCollide(null, Vector2.UnitX * Math.Sign(self.Speed.X));
            }
            orig(self, data);
        }

        private static void onTheoCrystalCollideV(On.Celeste.TheoCrystal.orig_OnCollideV orig, TheoCrystal self, CollisionData data)
        {
            if (data.Hit is TimedDashSwitch)
            {
                (data.Hit as TimedDashSwitch).OnDashCollide(null, Vector2.UnitY * Math.Sign(self.Speed.Y));
            }
            orig(self, data);
        }

        public override void Added(Scene scene)
        {
            base.Added(scene);
            if (!string.IsNullOrEmpty(flag))
            {
                SceneAs<Level>().Session.SetFlag(flag, false);
            }
        }

        public override void Update()
        {
            base.Update();
            if (inWall && (side == Sides.Left || side == Sides.Right))
            {
                DisplacePlayerOnTop();
            }
            if (pressed)
            {
                Collidable = false;
                return;
            }
            Player playerOnTop = GetPlayerOnTop();
            if (side == Sides.Up && playerOnTop != null)
            {
                if (playerOnTop.Holding != null)
                {
                    OnDashed(playerOnTop, Vector2.UnitY);
                }
                else
                {
                    if (speedY < 0f)
                    {
                        speedY = 0f;
                    }
                    Collider.Height = 4f;
                    speedY = Calc.Approach(speedY, 70f, 200f * Engine.DeltaTime);
                    MoveTowardsY(startY + 2f, speedY * Engine.DeltaTime);
                    if (!playerWasOn)
                    {
                        Audio.Play("event:/game/05_mirror_temple/button_depress", Position);
                    }
                }
            }
            else if (side == Sides.Down || side == Sides.Up)
            {
                if (speedY > 0f)
                {
                    speedY = 0f;
                }
                speedY = Calc.Approach(speedY, -150f, 200f * Engine.DeltaTime);
                MoveTowardsY(startY, (0f - speedY) * Engine.DeltaTime);
                if (playerWasOn && side != Sides.Down)
                {
                    Audio.Play("event:/game/05_mirror_temple/button_return", Position);
                }
            }
            else if (side == Sides.Left)
            {
                if (speedX > 0f)
                {
                    speedX = 0f;
                }
                speedX = Calc.Approach(speedX, -150f, 200f * Engine.DeltaTime);
                MoveTowardsX(startX, (0f - speedX) * Engine.DeltaTime);
            }
            else if (side == Sides.Right)
            {
                if (speedX < 0f)
                {
                    speedX = 0f;
                }
                speedX = Calc.Approach(speedX, 70f, 200f * Engine.DeltaTime);
                MoveTowardsX(startX, speedX * Engine.DeltaTime);
            }
            playerWasOn = (playerOnTop != null);
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
                if (side == Sides.Left)
                {
                    if (player.Left >= Left)
                    {
                        player.Left = Right;
                        player.Y += 1f;
                    }
                }
                else if (player.Right <= Right)
                {
                    player.Right = Left;
                    player.Y += 1f;
                }
            }
        }

        public DashCollisionResults OnDashed(Player player, Vector2 direction)
        {
            if (!pressed && direction == pressDirection)
            {
                Input.Rumble(RumbleStrength.Medium, RumbleLength.Medium);
                Audio.Play("event:/game/05_mirror_temple/button_activate", Position);
                sprite.Play("push");
                pressed = true;
                MoveTo(pressedTarget);
                Collidable = false;
                Position -= pressDirection * 2f;
                SceneAs<Level>().ParticlesFG.Emit(P_PressA, 10, Position + sprite.Position, direction.Perpendicular() * 6f, sprite.Rotation - (float)Math.PI);
                SceneAs<Level>().ParticlesFG.Emit(P_PressB, 4, Position + sprite.Position, direction.Perpendicular() * 6f, sprite.Rotation - (float)Math.PI);
                TimeManager manager = SceneAs<Level>().Tracker.GetEntity<TimeManager>();
                if (manager != null)
                {
                    if (mode == "add")
                    {
                        Scene.Add(new CustomFollower.CustomFollowerCollectText(player.TopCenter + new Vector2(0f, -15f), 0, 2, "+ " + timer + " " + Dialog.Clean("XaphanHelper_Collect_Time" + (timer == 1 ? "_One" : "")), 0.75f));
                        manager.AddTime(timer);
                    }
                    else
                    {
                        manager.SetTime(timer);
                    }
                }
                else
                {
                    SceneAs<Level>().Add(new TimeManager(timer, TickingType, flag));
                }
            }
            return DashCollisionResults.NormalCollision;
        }

        public void ResetSwitch()
        {
            Collidable = true;
            pressed = false;
            sprite.Play("idle");
            Audio.Play("event:/game/05_mirror_temple/button_return", Position);
        }

        private void onShake(Vector2 amount)
        {
            spriteOffset += amount;
        }

        private void onMove(Vector2 amount)
        {
            startY += amount.Y;
            startX += amount.X;
            Vector2 vector = pressedTarget;
            pressedTarget = vector + amount;
            if (Collidable)
            {
                MoveV(amount.Y);
                MoveH(amount.X);
            }
            else
            {
                Position += amount;
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