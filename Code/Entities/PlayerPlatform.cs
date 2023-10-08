using System;
using System.Collections;
using Celeste.Mod.XaphanHelper.Controllers;
using Celeste.Mod.XaphanHelper.UI_Elements;
using Celeste.Mod.XaphanHelper.Upgrades;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Monocle;

namespace Celeste.Mod.XaphanHelper.Entities
{
    [Tracked(true)]
    class PlayerPlatform : Solid
    {

        private Vector2 StartPosition;

        private Vector2 EndPosition;

        public string Side;

        public bool Gentle;

        private bool CanSlide;

        public bool UpsideDown;

        public bool StickyDash;

        public bool CanJumpThrough;

        private int SlopeHeight;

        public float platfromWidth;

        public float slopeTop;

        public bool AffectPlayerSpeed;

        public bool Sliding;

        public bool preventCollision;
        public string PlayerPose = "";

        public PlayerPlatform(Vector2 position, int width, bool gentle, string side, int soundIndex, int slopeHeight, bool canSlide, float top, bool affectPlayerSpeed, bool upsideDown = false, bool stickyDash = false, bool canJumpThrough = false) : base(position, width, 4, true)
        {
            AllowStaticMovers = false;
            Gentle = gentle;
            Side = side;
            UpsideDown = upsideDown;
            Collider = new Hitbox(width, 4, Gentle ? (Side == "Left" ? 0 : -8) : 0, UpsideDown ? 4 : 8);
            SurfaceSoundIndex = soundIndex;
            SlopeHeight = slopeHeight;
            platfromWidth = width;
            CanSlide = canSlide;
            slopeTop = top;
            AffectPlayerSpeed = affectPlayerSpeed;
            StickyDash = stickyDash;
            CanJumpThrough = canJumpThrough;
        }

        public static void Load()
        {
            On.Celeste.Solid.MoveVExact += OnSolidMoveVExact;
            On.Celeste.Solid.Update += OnSolidUpdate;

            On.Monocle.Sprite.Play += PlayerSpritePlayHook;
        }

        public static void Unload()
        {
            On.Celeste.Solid.MoveVExact -= OnSolidMoveVExact;
            On.Celeste.Solid.Update -= OnSolidUpdate;

            On.Monocle.Sprite.Play -= PlayerSpritePlayHook;
        }
        private static void PlayerSpritePlayHook(On.Monocle.Sprite.orig_Play orig, Sprite self, string id, bool restart = false, bool randomizeFrame = false) {

            if (self.Entity is Player player && player != null && player.StateMachine.State != Player.StDash) {
                foreach (PlayerPlatform platform in self.SceneAs<Level>().Tracker.GetEntities<PlayerPlatform>()) {
                    if (platform.PlayerPose != "" && platform.Active) {
                        id = platform.PlayerPose;
                        break;
                    }
                }
            }
            orig(self, id, restart, randomizeFrame);
        }

        private static void OnSolidMoveVExact(On.Celeste.Solid.orig_MoveVExact orig, Solid self, int move)
        {
            if (self.GetType() == typeof(PlayerPlatform))
            {
                PlayerPlatform platform = (PlayerPlatform)self;
                Player player = self.Scene.Tracker.GetEntity<Player>();
                if (!platform.UpsideDown)
                {
                    if (player != null)
                    {
                        if (move < 0)
                        {
                            if (player.IsRiding(self))
                            {
                                self.Collidable = false;
                                if (player.TreatNaive)
                                {
                                    player.NaiveMove(Vector2.UnitY * move);
                                }
                                else
                                {
                                    player.MoveVExact(move);
                                }
                                self.Collidable = true;
                            }
                            else if (!player.TreatNaive && self.CollideCheck(player, self.Position + Vector2.UnitY * move) && !self.CollideCheck(player))
                            {
                                self.Collidable = false;
                                player.MoveVExact((int)(self.Top + move - player.Bottom));
                                self.Collidable = true;
                            }
                        }
                        else
                        {
                            if ((player.IsRiding(self) && (platform.StickyDash || player.StateMachine.State != 2)))
                            {
                                self.Collidable = false;
                                if (player.TreatNaive)
                                {
                                    player.NaiveMove(Vector2.UnitY * move);
                                }
                                else
                                {
                                    player.MoveVExact(move);
                                }
                                self.Collidable = true;
                            }
                        }
                    }
                    self.Y += move;
                    self.MoveStaticMovers(Vector2.UnitY * move);
                }
                else
                {
                    orig(self, move);
                }
            }
            else
            {
                orig(self, move);
            }
        }

        private static void OnSolidUpdate(On.Celeste.Solid.orig_Update orig, Solid self)
        {
            foreach (PlayerPlatform platform in self.Scene.Tracker.GetEntities<PlayerPlatform>())
            {
                if (platform.InView())
                {
                    if ((!platform.HasPlayerRider() && platform.CollideFirst<Player>(platform.Position + Vector2.UnitY) == null && !platform.UpsideDown) || platform.preventCollision)
                    {
                        platform.Collidable = false;
                    }
                }
            }
            orig(self);
        }

        public override void Added(Scene scene)
        {
            base.Added(scene);
            StartPosition = Position;
        }

        public override void Update()
        {
            base.Update();
            PlayerPose = "";
            Player player = SceneAs<Level>().Tracker.GetEntity<Player>();
            Drone drone = SceneAs<Level>().Tracker.GetEntity<Drone>();

            if (drone != null)
            {
                if (drone.FakePlayer != null && !drone.dead)
                {
                    if (drone.FakePlayer.Sprite.Rate == 2)
                    {
                        Collidable = false;
                    }
                }
            }
            if (player != null)
            {

                if (Sliding && (player.Sprite.CurrentAnimationID != "duck" || player.Speed.X == 0 || Input.Jump.Pressed))
                {
                    Sliding = false;
                }
                if (player.Right <= Left - 16 || player.Left >= Right + 16)
                {
                    Position = StartPosition;
                }
                else
                {
                    if (player.Right <= Left || player.Left >= Right)
                    {
                        Position = StartPosition;
                    }
                    if (!UpsideDown)
                    {
                        if (Position.Y > StartPosition.Y)
                        {
                            Position.Y = StartPosition.Y;
                        }
                    }
                    else
                    {
                        if (Position.Y < StartPosition.Y)
                        {
                            Position.Y = StartPosition.Y;
                        }
                    }
                    if ((player.Sprite.Rate != 2 || (player.Sprite.Rate == 2 && player.Sprite.CurrentAnimationID == "wakeUp")))
                    {
                        if (!UpsideDown)
                        {
                            if (XaphanModule.PlayerIsControllingRemoteDrone() && CollideCheck(player))
                            {
                                player.MoveToY(player.Position.Y - 1);
                            }
                            if (player.Bottom > StartPosition.Y + 4)
                            {
                                Collidable = false;
                            }
                            else
                            {
                                SetCollision(player);
                            }


                            if (Side == "Left")
                            {
                                if (CanSlide && drone == null && player.IsRiding(this) && Input.MoveY == 1 && Input.MoveX != -1 && player.Left >= Left && !XaphanModule.UIOpened)
                                {
                                    Sliding = true;
                                    PlayerPose = "XaphanHelper_slopeSlide";

                                    if (player.Facing != Facings.Right)
                                    {
                                        player.Facing = Facings.Right;
                                    }
                                    if (player.Speed.X <= 250f - (Gentle ? 15f : 18f))
                                    {
                                        player.Speed.X += Gentle ? 15f : 18f;
                                    }
                                    else
                                    {
                                        player.Speed.X = 250f;
                                    }
                                }
                                if (player.BottomCenter.X < Right + 16 && Position.Y >= StartPosition.Y - 8 * SlopeHeight - 4)
                                {
                                    EndPosition = new Vector2(StartPosition.X, StartPosition.Y - (Right - (Gentle ? -4 : 0) - player.BottomCenter.X + (((XaphanModule.useMetroidGameplay && MetroidGameplayController.Shinesparking) || (!XaphanModule.useMetroidGameplay && SceneAs<Level>().Session.GetFlag("Xaphan_Helper_Shinesparking"))) ? 16f : 4f)) / (Gentle ? 2 : 1));
                                    Add(new Coroutine(MoveSlope()));
                                }
                            }
                            else if (Side == "Right")
                            {
                                if (CanSlide && drone == null && player.IsRiding(this) && Input.MoveY == 1 && Input.MoveX != 1 && player.Right <= Right && !XaphanModule.UIOpened)
                                {
                                    Sliding = true;
                                    PlayerPose = "XaphanHelper_slopeSlide";

                                    if (player.Facing != Facings.Left)
                                    {
                                        player.Facing = Facings.Left;
                                    }
                                    if (player.Speed.X >= -250f + (Gentle ? 15f : 18f))
                                    {
                                        player.Speed.X -= Gentle ? 15f : 18f;
                                    }
                                    else
                                    {
                                        player.Speed.X = -250f;
                                    }
                                }
                                if (player.BottomCenter.X > Left - 16 && Position.Y >= StartPosition.Y - 8 * SlopeHeight - 4)
                                {
                                    EndPosition = new Vector2(StartPosition.X, StartPosition.Y + (Left + (Gentle ? -4 : 0) - player.BottomCenter.X - (((XaphanModule.useMetroidGameplay && MetroidGameplayController.Shinesparking) || (!XaphanModule.useMetroidGameplay && SceneAs<Level>().Session.GetFlag("Xaphan_Helper_Shinesparking"))) ? 16f : 4f)) / (Gentle ? 2 : 1));
                                    Add(new Coroutine(MoveSlope()));
                                }
                            }
                        }
                        else if (!SceneAs<Level>().Session.GetFlag("Xaphan_Helper_Ceiling"))
                        {
                            if (XaphanModule.PlayerIsControllingRemoteDrone() && CollideCheck(player))
                            {
                                player.MoveToY(player.Position.Y + 1);
                            }
                            if (player.Top < StartPosition.Y + 12)
                            {
                                Collidable = false;
                            }
                            else
                            {
                                SetCollision(player);
                            }
                            if (Side == "Left")
                            {
                                if (player.BottomCenter.X < Right && player.BottomCenter.X > Left + 7 && Position.Y >= StartPosition.Y - 8 * SlopeHeight - 4)
                                {
                                    EndPosition = new Vector2(StartPosition.X, StartPosition.Y - (Right - (Gentle ? -4 : 0) - player.BottomCenter.X + (((XaphanModule.useMetroidGameplay && MetroidGameplayController.Shinesparking) || (!XaphanModule.useMetroidGameplay && SceneAs<Level>().Session.GetFlag("Xaphan_Helper_Shinesparking"))) ? (Gentle ? 16f : 8f) : (Gentle ? 8f : 4f))) / (Gentle ? 2 : 1) * -1 - (Gentle ? 2 : 0));
                                    Add(new Coroutine(MoveSlope()));
                                }
                                if (player.BottomCenter.X < Left + 7)
                                {
                                    Position.Y = StartPosition.Y + SlopeHeight * 8 + 4;
                                }
                            }
                            else if (Side == "Right")
                            {
                                if (player.BottomCenter.X > Left && player.BottomCenter.X < Right - 7 && Position.Y >= StartPosition.Y - 8 * SlopeHeight - 4)
                                {
                                    EndPosition = new Vector2(StartPosition.X, StartPosition.Y + (Left + (Gentle ? -4 : 0) - player.BottomCenter.X - (((XaphanModule.useMetroidGameplay && MetroidGameplayController.Shinesparking) || (!XaphanModule.useMetroidGameplay && SceneAs<Level>().Session.GetFlag("Xaphan_Helper_Shinesparking"))) ? (Gentle ? 16f : 8f) : (Gentle ? 8f : 4f))) / (Gentle ? 2 : 1) * -1 - (Gentle ? 2 : 0));
                                    Add(new Coroutine(MoveSlope()));
                                }
                                if (player.BottomCenter.X > Right - 7)
                                {
                                    Position.Y = StartPosition.Y + SlopeHeight * 8 + 4;
                                }

                            }
                        }
                    }
                    if (Collidable && !SceneAs<Level>().Session.GetFlag("Xaphan_Helper_Ceiling"))
                    {
                        if (!UpsideDown && CollideCheck<Player>())
                        {
                            player.Position -= Vector2.UnitY;
                        }
                        if (UpsideDown && CollideCheck<Player>())
                        {
                            player.Position += Vector2.UnitY;
                        }
                    }
                }
            }
        }

        public IEnumerator MoveSlope()
        {
            if (!SceneAs<Level>().Session.GetFlag("Xaphan_Helper_Ceiling"))
            {
                MoveToY(Math.Max(EndPosition.Y, StartPosition.Y - 8 * SlopeHeight - 4), 0);
            }

            yield return null;
        }

        public void TurnOffCollision(bool state)
        {
            preventCollision = state;
        }

        public bool InView()
        {
            Camera camera = (base.Scene as Level).Camera;
            if (base.X > camera.X - 16f && base.Y > camera.Y - 16f && base.X < camera.X + 320f + 16f)
            {
                return base.Y < camera.Y + 180f + 16f;
            }
            return false;
        }

        public void SetCollision(Player player)
        {
            if (!preventCollision)
            {
                if (player != null)
                {
                    if (!UpsideDown)
                    {
                        if (CanJumpThrough)
                        {
                            if (player.Bottom <= Top + 2)
                            {
                                Collidable = true;
                            }
                            else
                            {
                                Collidable = false;
                            }
                        }
                        else
                        {
                            Collidable = true;
                        }
                    }
                    else
                    {
                        if (CanJumpThrough)
                        {
                            if (player.Top >= Bottom)
                            {
                                Collidable = true;
                            }
                            else
                            {
                                Collidable = false;
                            }
                        }
                        else
                        {
                            Collidable = true;
                        }
                    }
                }
                else
                {
                    Collidable = false;
                }
            }
            else
            {
                Collidable = false;
            }
        }

        // Remove debug render

        public override void DebugRender(Camera camera)
        {

        }
    }
}