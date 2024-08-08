using System;
using System.Collections;
using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste.Mod.XaphanHelper.Entities
{
    [Tracked(true)]
    [CustomEntity("XaphanHelper/FlagDashSwitch")]
    class FlagDashSwitch : Solid
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

        private bool pressed;

        public bool wasPressed;

        private Vector2 pressDirection;

        private float speedY;

        private float speedX;

        private float startY;

        private float startX;

        public bool persistent;

        private bool playerWasOn;

        private Sprite sprite;

        private string spriteName;

        public string flag;

        private bool canSwapFlag;

        private string forceInactiveFlag;

        public string mode;

        public bool flagState;

        public bool registerInSaveData;

        public bool saveDataOnlyAfterCheckpoint;

        public Hitbox originHitBox;

        public string particleColor1;

        public string particleColor2;

        public ParticleType P_PressA;

        public ParticleType P_PressB;

        private StaticMover staticMover;

        private Vector2 spriteOffset;

        private static bool haveGolden;

        public Color EnabledColor = Color.White;

        public Color DisabledColor = Color.White;

        public bool VisibleWhenDisabled;

        public bool FlagRegiseredInSaveData()
        {
            Session session = SceneAs<Level>().Session;
            string Prefix = session.Area.LevelSet;
            int chapterIndex = session.Area.ChapterIndex;
            return XaphanModule.ModSaveData.SavedFlags.Contains(Prefix + "_Ch" + chapterIndex + "_" + flag + (haveGolden ? "_GoldenStrawberry" : ""));
        }

        public Vector2? startSpawnPoint;

        private bool inWall;

        private Tween tween;

        public FlagDashSwitch(EntityData data, Vector2 offset, EntityID eid) : base(data.Position + offset, data.Width, data.Height, safe: true)
        {
            Tag = Tags.TransitionUpdate;
            side = data.Enum<Sides>("side");
            persistent = data.Bool("persistent");
            spriteName = data.Attr("spriteName");
            flag = data.Attr("flag");
            canSwapFlag = data.Bool("canSwapFlag");
            forceInactiveFlag = data.Attr("forceInactiveFlag");
            mode = data.Attr("mode", "SetTrue");
            registerInSaveData = data.Bool("registerInSaveData");
            saveDataOnlyAfterCheckpoint = data.Bool("saveDataOnlyAfterCheckpoint");
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
            if (canSwapFlag)
            {
                persistent = false;
            }
            Add(sprite = GFX.SpriteBank.Create("dashSwitch_" + spriteName));
            sprite.Play("idle");
            if (side == Sides.Up || side == Sides.Down)
            {
                Collider.Width = 16f;
                Collider.Height = inWall ? 4f : 8f;
                if (canSwapFlag)
                {
                    originHitBox = new Hitbox(16f, inWall ? 4f : 8f);
                }
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
                        Add(new LedgeBlocker());
                    }
                    pressedTarget = Position + Vector2.UnitX * (inWall ? 4f : 8f);
                    pressDirection = Vector2.UnitX;
                    startX = X;
                    break;
                case Sides.Left:
                    staticMover.SolidChecker = ((Solid s) => CollideCheckOutside(s, Position - Vector2.UnitX * 2f));
                    sprite.Position = new Vector2(inWall ? -4f : 0f, 8f);
                    sprite.Rotation = (float)Math.PI;
                    if (inWall)
                    {
                        Add(new LedgeBlocker());
                    }
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
            staticMover.OnEnable = onEnable;
            staticMover.OnDisable = onDisable;
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
            On.Celeste.ChangeRespawnTrigger.OnEnter += onChangeRespawnTriggerOnEnter;
        }

        public static void Unload()
        {
            On.Celeste.Glider.OnCollideH -= onGliderCollideH;
            On.Celeste.Seeker.SlammedIntoWall -= onSeekerSlammedIntoWall;
            On.Celeste.TheoCrystal.OnCollideH -= onTheoCrystalCollideH;
            On.Celeste.TheoCrystal.OnCollideV -= onTheoCrystalCollideV;
            On.Celeste.ChangeRespawnTrigger.OnEnter -= onChangeRespawnTriggerOnEnter;
        }

        private static void onGliderCollideH(On.Celeste.Glider.orig_OnCollideH orig, Glider self, CollisionData data)
        {
            if (data.Hit is FlagDashSwitch)
            {
                (data.Hit as FlagDashSwitch).OnDashCollide(null, Vector2.UnitX * Math.Sign(self.Speed.X));
            }
            orig(self, data);
        }

        private static void onSeekerSlammedIntoWall(On.Celeste.Seeker.orig_SlammedIntoWall orig, Seeker self, CollisionData data)
        {
            if (data.Hit is FlagDashSwitch)
            {
                (data.Hit as FlagDashSwitch).OnDashCollide(null, Vector2.UnitX * Math.Sign(self.Speed.X));
            }
            orig(self, data);
        }

        private static void onTheoCrystalCollideH(On.Celeste.TheoCrystal.orig_OnCollideH orig, TheoCrystal self, CollisionData data)
        {
            if (data.Hit is FlagDashSwitch)
            {
                (data.Hit as FlagDashSwitch).OnDashCollide(null, Vector2.UnitX * Math.Sign(self.Speed.X));
            }
            orig(self, data);
        }

        private static void onTheoCrystalCollideV(On.Celeste.TheoCrystal.orig_OnCollideV orig, TheoCrystal self, CollisionData data)
        {
            if (data.Hit is FlagDashSwitch)
            {
                (data.Hit as FlagDashSwitch).OnDashCollide(null, Vector2.UnitY * Math.Sign(self.Speed.Y));
            }
            orig(self, data);
        }

        private static void onChangeRespawnTriggerOnEnter(On.Celeste.ChangeRespawnTrigger.orig_OnEnter orig, ChangeRespawnTrigger self, Player player)
        {
            bool onSolid = true;
            Vector2 point = self.Target + Vector2.UnitY * -4f;
            Session session = self.SceneAs<Level>().Session;
            if (self.Scene.CollideCheck<Solid>(point))
            {
                onSolid = self.Scene.CollideCheck<FloatySpaceBlock>(point);
            }
            if (onSolid && (!session.RespawnPoint.HasValue || session.RespawnPoint.Value != self.Target))
            {
                foreach (FlagDashSwitch flagDashSwitch in self.SceneAs<Level>().Tracker.GetEntities<FlagDashSwitch>())
                {
                    if (!string.IsNullOrEmpty(flagDashSwitch.flag))
                    {
                        flagDashSwitch.startSpawnPoint = session.RespawnPoint;
                        flagDashSwitch.flagState = session.GetFlag(flagDashSwitch.flag);
                        int chapterIndex = self.SceneAs<Level>().Session.Area.ChapterIndex;
                        self.SceneAs<Level>().Session.SetFlag("Ch" + chapterIndex + "_" + flagDashSwitch.flag + "_true", false);
                        self.SceneAs<Level>().Session.SetFlag("Ch" + chapterIndex + "_" + flagDashSwitch.flag + "_false", false);
                        if (flagDashSwitch.wasPressed && flagDashSwitch.registerInSaveData && flagDashSwitch.saveDataOnlyAfterCheckpoint)
                        {
                            string Prefix = self.SceneAs<Level>().Session.Area.LevelSet;
                            if (!XaphanModule.ModSaveData.SavedFlags.Contains(Prefix + "_Ch" + chapterIndex + "_" + flagDashSwitch.flag))
                            {
                                XaphanModule.ModSaveData.SavedFlags.Add(Prefix + "_Ch" + chapterIndex + "_" + flagDashSwitch.flag);
                            }
                            if (haveGolden)
                            {
                                if (!XaphanModule.ModSaveData.SavedFlags.Contains(Prefix + "_Ch" + chapterIndex + "_" + flagDashSwitch.flag + "_GoldenStrawberry"))
                                {
                                    XaphanModule.ModSaveData.SavedFlags.Add(Prefix + "_Ch" + chapterIndex + "_" + flagDashSwitch.flag + "_GoldenStrawberry");
                                }
                            }
                        }
                    }
                }
            }
            orig(self, player);
        }

        public override void Added(Scene scene)
        {
            base.Added(scene);
            int chapterIndex = SceneAs<Level>().Session.Area.ChapterIndex;
            if (SceneAs<Level>().Session.GetFlag("Ch" + chapterIndex + "_" + flag + "_true"))
            {
                flagState = true;
                SceneAs<Level>().Session.SetFlag(flag, true);
            }
            else if (SceneAs<Level>().Session.GetFlag("Ch" + chapterIndex + "_" + flag + "_false"))
            {
                flagState = false;
                SceneAs<Level>().Session.SetFlag(flag, false);
            }
            else
            {
                flagState = SceneAs<Level>().Session.GetFlag(flag);
                SceneAs<Level>().Session.SetFlag("Ch" + chapterIndex + "_" + flag + "_true", false);
                SceneAs<Level>().Session.SetFlag("Ch" + chapterIndex + "_" + flag + "_false", false);
                SceneAs<Level>().Session.SetFlag("Ch" + chapterIndex + "_" + flag + "_" + (flagState ? "true" : "false"), true);
            }
            if (SceneAs<Level>().Session.GetFlag(flag) && canSwapFlag)
            {
                pressed = false;
            }
        }

        public override void Awake(Scene scene)
        {
            base.Awake(scene);
            foreach (Strawberry item in Scene.Entities.FindAll<Strawberry>())
            {
                if (item.Golden && item.Follower.Leader != null)
                {
                    haveGolden = true;
                    break;
                }
            }
        }

        public override void Update()
        {
            base.Update();
            if (!string.IsNullOrEmpty(forceInactiveFlag) && SceneAs<Level>().Session.GetFlag(forceInactiveFlag))
            {
                sprite.Rate = 0f;
            }
            else
            {
                sprite.Rate = 1f;
            }
            if (!haveGolden)
            {
                if ((((mode == "SetTrue" && SceneAs<Level>().Session.GetFlag(flag)) || (mode == "SetFalse" && !SceneAs<Level>().Session.GetFlag(flag))) || FlagRegiseredInSaveData()) && !canSwapFlag)
                {
                    sprite.Play("pushed");
                    Position = pressedTarget;
                    Position -= pressDirection * 2f;
                    pressed = true;
                }
            }
            else
            {
                if (((mode == "SetTrue" && SceneAs<Level>().Session.GetFlag(flag)) || (mode == "SetFalse" && !SceneAs<Level>().Session.GetFlag(flag))) && !canSwapFlag)
                {
                    sprite.Play("pushed");
                    Position = pressedTarget;
                    Position -= pressDirection * 2f;
                    pressed = true;
                }
            }
            if (inWall && (side == Sides.Left || side == Sides.Right))
            {
                DisplacePlayerOnTop();
            }
            if (SceneAs<Level>().Transitioning && wasPressed)
            {
                flagState = SceneAs<Level>().Session.GetFlag(flag);
                int chapterIndex = SceneAs<Level>().Session.Area.ChapterIndex;
                SceneAs<Level>().Session.SetFlag("Ch" + chapterIndex + "_" + flag + "_true", false);
                SceneAs<Level>().Session.SetFlag("Ch" + chapterIndex + "_" + flag + "_false", false);
                if (registerInSaveData && saveDataOnlyAfterCheckpoint)
                {
                    string Prefix = SceneAs<Level>().Session.Area.LevelSet;
                    if (!XaphanModule.ModSaveData.SavedFlags.Contains(Prefix + "_Ch" + chapterIndex + "_" + flag))
                    {
                        XaphanModule.ModSaveData.SavedFlags.Add(Prefix + "_Ch" + chapterIndex + "_" + flag);
                    }
                    if (haveGolden)
                    {
                        if (!XaphanModule.ModSaveData.SavedFlags.Contains(Prefix + "_Ch" + chapterIndex + "_" + flag + "_GoldenStrawberry"))
                        {
                            XaphanModule.ModSaveData.SavedFlags.Add(Prefix + "_Ch" + chapterIndex + "_" + flag + "_GoldenStrawberry");
                        }
                    }
                }
            }
            if (pressed)
            {
                Collidable = false;
                Player player = SceneAs<Level>().Tracker.GetEntity<Player>();
                if (player != null && canSwapFlag && !CollideCheck(player))
                {
                    if (side == Sides.Left)
                    {
                        Collider.Width = inWall ? 4f : 8f;
                    }
                    else if (side == Sides.Right)
                    {
                        Collider.Position += new Vector2(8f, 0f);
                        Collider.Width = inWall ? 4f : 8f;
                    }
                    else if (side == Sides.Down)
                    {
                        Collider.Height = inWall ? 4f : 8f;
                    }
                    ResetSwitch();
                }
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

        public IEnumerator MoveSwitch(Vector2 direction)
        {
            sprite.Play("push");
            pressed = true;
            tween = Tween.Create(Tween.TweenMode.Oneshot, Ease.CubeOut, 0.2f, start: true);
            tween.OnUpdate = delegate (Tween t)
            {
                MoveTo(Vector2.Lerp(Position, pressedTarget, t.Eased));
            };
            Add(tween);
            if (canSwapFlag)
            {
                if (side == Sides.Left)
                {
                    Collider.Width = 16f;
                }
                else if (side == Sides.Right)
                {
                    Collider.Position -= new Vector2(8f, 0f);
                    Collider.Width = 16f;
                }
                else if (side == Sides.Down)
                {
                    Collider.Height = 16f;
                }
            }
            Position -= pressDirection * 2f;
            yield return null;
        }

        public DashCollisionResults OnDashed(Player player, Vector2 direction)
        {
            if (!pressed && direction == pressDirection && (string.IsNullOrEmpty(forceInactiveFlag) || (!string.IsNullOrEmpty(forceInactiveFlag) && !SceneAs<Level>().Session.GetFlag(forceInactiveFlag))))
            {
                startSpawnPoint = SceneAs<Level>().Session.RespawnPoint;
                Input.Rumble(RumbleStrength.Medium, RumbleLength.Medium);
                Audio.Play("event:/game/05_mirror_temple/button_activate", Position);
                sprite.Play("push");
                pressed = true;
                wasPressed = true;
                tween = Tween.Create(Tween.TweenMode.Oneshot, Ease.CubeOut, 0.2f, start: true);
                tween.OnUpdate = delegate (Tween t)
                {
                    MoveTo(Vector2.Lerp(Position, pressedTarget, t.Eased));
                };
                Add(tween);
                Collidable = false;
                if (canSwapFlag)
                {
                    if (side == Sides.Left)
                    {
                        Collider.Width = 16f;
                    }
                    else if (side == Sides.Right)
                    {
                        Collider.Position -= new Vector2(8f, 0f);
                        Collider.Width = 16f;
                    }
                    else if (side == Sides.Down)
                    {
                        Collider.Height = 16f;
                    }
                }
                Position -= pressDirection * 2f;
                SceneAs<Level>().ParticlesFG.Emit(P_PressA, 10, Position + sprite.Position, direction.Perpendicular() * 6f, sprite.Rotation - (float)Math.PI);
                SceneAs<Level>().ParticlesFG.Emit(P_PressB, 4, Position + sprite.Position, direction.Perpendicular() * 6f, sprite.Rotation - (float)Math.PI);
                if (canSwapFlag || mode == "SetInverted")
                {
                    SceneAs<Level>().Session.SetFlag(flag, !SceneAs<Level>().Session.GetFlag(flag));
                    foreach (FlagDashSwitch dashSwitch in SceneAs<Level>().Tracker.GetEntities<FlagDashSwitch>())
                    {
                        if (dashSwitch != this && dashSwitch.pressed && dashSwitch.flag == flag)
                        {
                            if (dashSwitch.mode == "SetInverted" || (dashSwitch.mode == "SetFalse" && SceneAs<Level>().Session.GetFlag(flag)) || (dashSwitch.mode == "SetTrue" && !SceneAs<Level>().Session.GetFlag(flag)))
                                dashSwitch.ResetSwitch();
                        }
                    }
                }
                else
                {
                    if (mode == "SetTrue")
                    {
                        SceneAs<Level>().Session.SetFlag(flag, true);
                        if (registerInSaveData && !saveDataOnlyAfterCheckpoint)
                        {
                            string Prefix = SceneAs<Level>().Session.Area.LevelSet;
                            int chapterIndex = SceneAs<Level>().Session.Area.ChapterIndex;
                            if (!XaphanModule.ModSaveData.SavedFlags.Contains(Prefix + "_Ch" + chapterIndex + "_" + flag))
                            {
                                XaphanModule.ModSaveData.SavedFlags.Add(Prefix + "_Ch" + chapterIndex + "_" + flag);
                            }
                            if (haveGolden)
                            {
                                if (!XaphanModule.ModSaveData.SavedFlags.Contains(Prefix + "_Ch" + chapterIndex + "_" + flag + "_GoldenStrawberry"))
                                {
                                    XaphanModule.ModSaveData.SavedFlags.Add(Prefix + "_Ch" + chapterIndex + "_" + flag + "_GoldenStrawberry");
                                }
                            }
                        }
                        foreach (FlagDashSwitch dashSwitch in SceneAs<Level>().Tracker.GetEntities<FlagDashSwitch>())
                        {
                            if (dashSwitch != this && dashSwitch.pressed && dashSwitch.flag == flag && (dashSwitch.mode == "SetFalse" || dashSwitch.mode == "SetInverted"))
                            {
                                dashSwitch.ResetSwitch();
                            }
                            if (dashSwitch != this && dashSwitch.flag == flag && dashSwitch.mode == "SetTrue")
                            {
                                Add(new Coroutine(dashSwitch.MoveSwitch(dashSwitch.pressDirection)));
                            }
                        }
                    }
                    else if (mode == "SetFalse" && !FlagRegiseredInSaveData())
                    {
                        SceneAs<Level>().Session.SetFlag(flag, false);
                        foreach (FlagDashSwitch dashSwitch in SceneAs<Level>().Tracker.GetEntities<FlagDashSwitch>())
                        {
                            if (dashSwitch != this && dashSwitch.pressed && dashSwitch.flag == flag && (dashSwitch.mode == "SetTrue" || dashSwitch.mode == "SetInverted"))
                            {
                                dashSwitch.ResetSwitch();
                            }
                            if (dashSwitch != this && dashSwitch.flag == flag && dashSwitch.mode == "SetFalse")
                            {
                                Add(new Coroutine(dashSwitch.MoveSwitch(dashSwitch.pressDirection)));
                            }
                        }
                    }
                }
            }
            return DashCollisionResults.NormalCollision;
        }

        public void AutoPress()
        {
            Collidable = false;
            pressed = true;
            sprite.Play("push");
        }

        public void ResetSwitch(bool silent = false)
        {
            Collidable = true;
            pressed = false;
            sprite.Play("idle");
            if (!SceneAs<Level>().Transitioning && !silent)
            {
                Audio.Play("event:/game/05_mirror_temple/button_return", Position);
            }
        }

        public void SetSwitchColor(Color color)
        {
            foreach (Component component in Components)
            {
                if (component is Sprite sprite)
                {
                    sprite.Color = color;
                }
            }
        }

        private void onEnable()
        {
            Visible = (Collidable = true);
            SetSwitchColor(EnabledColor);
        }

        private void onDisable()
        {
            Collidable = false;
            if (VisibleWhenDisabled)
            {
                SetSwitchColor(DisabledColor);
                return;
            }
            Visible = false;
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