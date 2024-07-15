using System;
using System.Collections;
using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste.Mod.XaphanHelper.Entities
{
    [Tracked(true)]
    [CustomEntity("XaphanHelper/VineHead")]
    class VineHead : Actor
    {
        private enum Facings
        {
            Up,
            Down,
            Left,
            Right
        }

        private Facings Facing;

        private Vector2[] nodes;

        private Vector2 PreviousDirection;

        private string directory;

        private Sprite Sprite;

        private Coroutine SequenceRoutine = new();

        private string flag;

        private bool SwapDirection;

        private bool JustSwapped;

        private bool CanBounce = true;

        private bool BouncePlayer;

        private bool StopMoving;

        public int ID;

        private Vector2 Scale;

        private float GrowSpeed;

        private float PauseTime;

        public VineHead(EntityData data, Vector2 offset) : base(data.Position + offset)
        {
            Tag = Tags.TransitionUpdate;
            ID = data.ID;
            GrowSpeed = data.Float("growSpeed", 50f);
            PauseTime = data.Float("pauseTime", 3f);
            flag = data.Attr("flag");
            Collider = new Hitbox(8f, 8f);
            Add(new PlayerCollider(onPlayer, new Circle(8, 4, 4)));
            Add(new PlayerCollider(onBounce, new Circle(6, 4, 4)));
            nodes = data.NodesWithPosition(offset);
            directory = data.Attr("directory");
            if (string.IsNullOrEmpty(directory))
            {
                directory = "objects/XaphanHelper/Vine";
            }
            Add(Sprite = new Sprite(GFX.Game, directory + "/"));
            Sprite.Add("idle", "head", 0.08f, 0);
            Sprite.Add("open", "head", 0.08f, 1, 2, 3);
            Sprite.Add("close", "head", 0.08f, 3, 2, 1, 0);
            Sprite.CenterOrigin();
            Sprite.Position += new Vector2(4);
            Sprite.Play("idle");
            Scale = Vector2.One;
        }

        private void onPlayer(Player player)
        {
            if (((Facing == Facings.Up && player.Bottom > Top + 4f) || (Facing == Facings.Down && player.Top < Bottom - 4f) || (Facing == Facings.Left && player.Right > Left + 4f) || (Facing == Facings.Right && player.Left < Right - 4f) || !CanBounce) && !BouncePlayer)
            {
                player.Die((player.Position - Position).SafeNormalize());
            }
        }

        private void onBounce(Player player)
        {
            if (!BouncePlayer)
            {
                BouncePlayer = true;
                bool wasDashing = false;
                if (player.StateMachine.State == Player.StDash || player.DashAttacking)
                {
                    wasDashing = true;
                }
                player.StateMachine.State = 0;
                player.Speed = Vector2.Zero;
                Vector2 speed = new Vector2();
                Scale = new Vector2(1.3f, 0.7f);
                if (Facing == Facings.Up)
                {
                    speed = Vector2.UnitY * (wasDashing ? -350f : -225f);
                }
                else if (Facing == Facings.Down)
                {

                    player.MoveTowardsY(BottomCenter.Y + 16f, 50);
                    speed = Vector2.UnitY * (wasDashing ? 300f : 175f);
                }
                else if (Facing == Facings.Left)
                {
                    player.MoveTowardsX(CenterLeft.X - 8f, 50);
                    player.MoveTowardsY(CenterLeft.Y, 50);
                    speed = new Vector2(wasDashing ? -325f : -225f, -150f);
                }
                else if (Facing == Facings.Right)
                {
                    player.MoveTowardsX(CenterRight.X + 8f, 50);
                    player.MoveTowardsY(CenterRight.Y, 50);
                    speed = new Vector2(wasDashing ? 325f : 225f, -150f);
                }
                player.Speed = speed;
                player.RefillDash();
                player.RefillStamina();
                player.Sprite.Scale = new Vector2(0.6f, 1.4f);
                Audio.Play("event:/game/general/thing_booped", Position);
                Add(new Coroutine(BounceCooldownRoutine()));
            }
            else if (Facing == Facings.Down)
            {
                bool wasDashing = false;
                if (player.StateMachine.State == Player.StDash || player.DashAttacking)
                {
                    wasDashing = true;
                }
                player.Speed = Vector2.UnitY * (wasDashing ? 300f : 175f);
            }
        }

        private IEnumerator BounceCooldownRoutine()
        {
            StopMoving = true;
            yield return 0.1f;
            Sprite.Play("open");
            CanBounce = false;
            yield return 0.3f;
            BouncePlayer = false;
            yield return PauseTime - 0.3f - 0.32f;
            Sprite.Play("close");
            yield return 0.32f;
            StopMoving = false;
            CanBounce = true;
        }

        public override void Awake(Scene scene)
        {
            base.Awake(scene);
            if ((!string.IsNullOrEmpty(flag) && SceneAs<Level>().Session.GetFlag(flag)) || XaphanModule.ModSession.LightMode == XaphanModuleSession.LightModes.Light)
            {
                SwapDirection = true;
                Position = nodes[1];
            }
            if (Scene.CollideCheck<VinePath>(new Rectangle((int)X, (int)Y + 8, 1, 1)))
            {
                Sprite.Rotation = Position == nodes[0] ? (float)Math.PI : 0;
            }
            if (Scene.CollideCheck<VinePath>(new Rectangle((int)X + 8, (int)Y, 1, 1)))
            {
                Sprite.Rotation = Position == nodes[0] ? (float)Math.PI / 2 : -(float)Math.PI / 2;
            }
            if (Scene.CollideCheck<VinePath>(new Rectangle((int)X - 8, (int)Y, 1, 1)))
            {
                Sprite.Rotation = Position == nodes[0] ? -(float)Math.PI / 2 : (float)Math.PI / 2;
            }
            if (Scene.CollideCheck<VinePath>(new Rectangle((int)X, (int)Y - 8, 1, 1)))
            {
                Sprite.Rotation = Position == nodes[0] ? 0 : (float)Math.PI;
            }
        }

        public override void Update()
        {
            base.Update();
            if (SceneAs<Level>().Tracker.GetEntity<Player>() != null && !SceneAs<Level>().Tracker.GetEntity<Player>().Dead)
            {
                Scale.X = Calc.Approach(Scale.X, 1f, 1f * Engine.DeltaTime);
                Scale.Y = Calc.Approach(Scale.Y, 1f, 1f * Engine.DeltaTime);
                if (!StopMoving)
                {
                    if (((!string.IsNullOrEmpty(flag) && SceneAs<Level>().Session.GetFlag(flag)) || XaphanModule.ModSession.LightMode == XaphanModuleSession.LightModes.Light) && Position == nodes[1])
                    {
                        foreach (VinePath.VinePathSection section in SceneAs<Level>().Tracker.GetEntities<VinePath.VinePathSection>())
                        {
                            if (section.ID == ID)
                            {
                                section.SetGrownSprite(true);
                            }
                        }
                    }

                    if ((((!string.IsNullOrEmpty(flag) && SceneAs<Level>().Session.GetFlag(flag)) || XaphanModule.ModSession.LightMode == XaphanModuleSession.LightModes.Light) && !SwapDirection) || (((!string.IsNullOrEmpty(flag) && !SceneAs<Level>().Session.GetFlag(flag)) || XaphanModule.ModSession.LightMode != XaphanModuleSession.LightModes.Light) && SwapDirection))
                    {
                        SwapDirection = !SwapDirection;
                        if (SequenceRoutine.Active)
                        {
                            SequenceRoutine.Cancel();
                        }
                        JustSwapped = true;
                    }

                    // At start and flag not set -> Wait
                    if (Position == nodes[0] && ((!string.IsNullOrEmpty(flag) && !SceneAs<Level>().Session.GetFlag(flag)) || XaphanModule.ModSession.LightMode != XaphanModuleSession.LightModes.Light))
                    {
                        if (SequenceRoutine.Active)
                        {
                            SequenceRoutine.Cancel();
                        }
                    }

                    else
                    // Not at end and flag set -> Expand
                    if (Position != nodes[1] && ((!string.IsNullOrEmpty(flag) && SceneAs<Level>().Session.GetFlag(flag)) || XaphanModule.ModSession.LightMode == XaphanModuleSession.LightModes.Light))
                    {
                        if (!SequenceRoutine.Active)
                        {
                            if (JustSwapped)
                            {
                                JustSwapped = false;
                                PreviousDirection = -PreviousDirection;
                            }
                            StartSequence();
                        }
                        foreach (VinePath.VinePathSection pathSection in SceneAs<Level>().Tracker.GetEntities<VinePath.VinePathSection>())
                        {
                            if (Left == pathSection.Left && Right == pathSection.Right && Top == pathSection.Top && Bottom == pathSection.Bottom)
                            {
                                pathSection.SetGrownSprite(true);
                            }
                        }
                    }

                    else
                    // Not at start and flag not set -> Retract
                    if (Position != nodes[0] && ((!string.IsNullOrEmpty(flag) && !SceneAs<Level>().Session.GetFlag(flag)) || XaphanModule.ModSession.LightMode != XaphanModuleSession.LightModes.Light))
                    {
                        if (!SequenceRoutine.Active)
                        {
                            if (JustSwapped)
                            {
                                JustSwapped = false;
                                Add(SequenceRoutine = new Coroutine(GrowRoutine(PreviousDirection, true)));
                            }
                            else
                            {
                                StartSequence(true);
                            }
                        }
                        foreach (VinePath.VinePathSection pathSection in SceneAs<Level>().Tracker.GetEntities<VinePath.VinePathSection>())
                        {
                            if (Left == pathSection.Left && Right == pathSection.Right && Top == pathSection.Top && Bottom == pathSection.Bottom)
                            {
                                pathSection.SetGrownSprite(false);
                            }
                        }
                    }

                    else
                    // At end and flag set -> Wait
                    if (Position == nodes[1] && ((!string.IsNullOrEmpty(flag) && SceneAs<Level>().Session.GetFlag(flag)) || XaphanModule.ModSession.LightMode == XaphanModuleSession.LightModes.Light))
                    {
                        if (SequenceRoutine.Active)
                        {
                            SequenceRoutine.Cancel();
                        }
                    }
                }
                else
                {
                    if (SequenceRoutine.Active)
                    {
                        SequenceRoutine.Cancel();
                    }
                }

                if (Sprite.Rotation == -(float)Math.PI / 2)
                {
                    Facing = Facings.Left;
                }
                else if (Sprite.Rotation == (float)Math.PI / 2)
                {
                    Facing = Facings.Right;
                }
                else if (Sprite.Rotation == (float)Math.PI)
                {
                    Facing = Facings.Down;
                }
                else if (Sprite.Rotation == 0)
                {
                    Facing = Facings.Up;
                }
            }
            else
            {
                if (SequenceRoutine.Active)
                {
                    SequenceRoutine.Cancel();
                }
            }
        }

        private void StartSequence(bool reverseSprite = false)
        {
            if (!SequenceRoutine.Active)
            {
                if (!MoveHCheck(1) && PreviousDirection != Vector2.UnitX)
                {
                    Add(SequenceRoutine = new Coroutine(GrowRoutine(Vector2.UnitX, reverseSprite)));
                }
                else if (!MoveVCheck(1) && PreviousDirection != Vector2.UnitY)
                {
                    Add(SequenceRoutine = new Coroutine(GrowRoutine(Vector2.UnitY, reverseSprite)));
                }
                else if (!MoveHCheck(-1) && PreviousDirection != -Vector2.UnitX)
                {
                    Add(SequenceRoutine = new Coroutine(GrowRoutine(-Vector2.UnitX, reverseSprite)));
                }
                else if (!MoveVCheck(-1) && PreviousDirection != -Vector2.UnitY)
                {
                    Add(SequenceRoutine = new Coroutine(GrowRoutine(-Vector2.UnitY, reverseSprite)));
                }
            }
        }

        private IEnumerator GrowRoutine(Vector2 direction, bool reverseSprite = false)
        {
            if (direction == Vector2.Zero)
            {
                yield break;
            }
            else
            {
                if (direction.X > 0)
                {
                    Sprite.Rotation = reverseSprite ? -(float)Math.PI / 2 : (float)Math.PI / 2;
                }
                else if (direction.X < 0)
                {
                    Sprite.Rotation = reverseSprite ? (float)Math.PI / 2 : -(float)Math.PI / 2;
                }
                else if (direction.Y > 0)
                {
                    Sprite.Rotation = reverseSprite ? 0 : (float)Math.PI;
                }
                else if (direction.Y < 0)
                {
                    Sprite.Rotation = reverseSprite ? (float)Math.PI : 0;
                }
            }
            PreviousDirection = -direction;
            while (true)
            {
                bool flag = (direction.X == 0f) ? MoveVCheck(direction.Y) : MoveHCheck(direction.X);
                if (flag)
                {
                    break;
                }
                if (direction.X != 0)
                {
                    MoveH(direction.X * GrowSpeed * Engine.DeltaTime);
                    yield return null;
                }
                if (direction.Y != 0f)
                {
                    MoveV(direction.Y * GrowSpeed * Engine.DeltaTime);
                    yield return null;
                }
            }
            if (direction.X != 0f)
            {
                CorrectionH(direction.X);
            }
            if (direction.Y != 0f)
            {
                CorrectionV(direction.Y);
            }
        }

        private bool MoveHCheck(float direction)
        {
            if (CollideCheck<VinePath>())
            {
                if ((!Scene.CollideCheck<VinePath>(new Rectangle((int)(X - 1f), (int)Y, 1, 1)) || !Scene.CollideCheck<VinePath>(new Rectangle((int)(X - 1f), (int)(Y + Height - 1), 1, 1)) || CollideCheck<Solid>(Position - Vector2.UnitX)) && direction == -1)
                {
                    return true;
                }
                if ((!Scene.CollideCheck<VinePath>(new Rectangle((int)(X + Width - 1f + 1f), (int)Y, 1, 1)) || !Scene.CollideCheck<VinePath>(new Rectangle((int)(X + Width - 1f + 1f), (int)(Y + Height - 1), 1, 1)) || CollideCheck<Solid>(Position + Vector2.UnitX)) && direction == 1)
                {
                    return true;
                }
                return false;
            }
            return true;
        }

        private bool MoveVCheck(float direction)
        {
            if (CollideCheck<VinePath>())
            {
                if ((!Scene.CollideCheck<VinePath>(new Rectangle((int)X, (int)(Y - 1f), 1, 1)) || !Scene.CollideCheck<VinePath>(new Rectangle((int)(X + Width - 1), (int)(Y - 1f), 1, 1)) || CollideCheck<Solid>(Position - Vector2.UnitY)) && direction == -1)
                {
                    return true;
                }
                if ((!Scene.CollideCheck<VinePath>(new Rectangle((int)X, (int)(Y + Height - 1f + 1f), 1, 1)) || !Scene.CollideCheck<VinePath>(new Rectangle((int)(X + Width - 1), (int)(Y + Height - 1f + 1f), 1, 1)) || CollideCheck<Solid>(Position + Vector2.UnitY)) && direction == 1)
                {
                    return true;
                }
                return false;
            }
            return true;
        }

        private void CorrectionH(float direction)
        {
            if (Scene.CollideCheck<VinePath>(new Rectangle((int)(X - 1f), (int)Y, 1, 1)) && Scene.CollideCheck<VinePath>(new Rectangle((int)(X - 1f), (int)(Y + Height - 1), 1, 1)) && !CollideCheck<Solid>(Position - Vector2.UnitX) && direction == -1)
            {
                MoveHExact(-1);
                CorrectionH(direction);
            }
            if (Scene.CollideCheck<VinePath>(new Rectangle((int)(X + Width), (int)Y, 1, 1)) && Scene.CollideCheck<VinePath>(new Rectangle((int)(X + Width), (int)(Y + Height - 1), 1, 1)) && !CollideCheck<Solid>(Position + Vector2.UnitX) && direction == 1)
            {
                MoveHExact(1);
                CorrectionH(direction);
            }
        }

        private void CorrectionV(float direction)
        {
            if (Scene.CollideCheck<VinePath>(new Rectangle((int)X, (int)(Y - 1f), 1, 1)) && Scene.CollideCheck<VinePath>(new Rectangle((int)(X + Width - 1), (int)(Y - 1f), 1, 1)) && !CollideCheck<Solid>(Position - Vector2.UnitY) && direction == -1)
            {
                MoveVExact(-1);
                CorrectionV(direction);
            }
            if (Scene.CollideCheck<VinePath>(new Rectangle((int)X, (int)(Y + Height), 1, 1)) && Scene.CollideCheck<VinePath>(new Rectangle((int)(X + Width - 1f), (int)(Y + Height), 1, 1)) && !CollideCheck<Solid>(Position + Vector2.UnitY) && direction == 1)
            {
                MoveVExact(1);
                CorrectionV(direction);
            }
        }

        public override void Render()
        {
            Sprite.Scale = Scale;
            base.Render();
        }
    }
}
