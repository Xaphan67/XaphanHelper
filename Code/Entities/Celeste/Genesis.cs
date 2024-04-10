using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Celeste.Mod.Entities;
using Celeste.Mod.XaphanHelper.Upgrades;
using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste.Mod.XaphanHelper.Entities
{
    [Tracked(true)]
    [CustomEntity("XaphanHelper/Genesis")]
    public class Genesis : Actor
    {
        private enum Facings
        {
            Left,
            Right
        }

        private Facings Facing;

        private bool ShouldSwitchFacing;

        private Coroutine Routine = new();

        private Sprite Sprite;

        public int Health;

        public Vector2 Speed;

        private bool MidAir;

        private bool noFlip;

        private Collision onCollideV;

        private float CannotLeapDelay;

        private float CannotDashDelay;

        private bool IsSlashingBomb;

        public bool playerHasMoved;

        public Genesis(EntityData data, Vector2 offset) : base(data.Position + offset)
        {
            Collider = new Hitbox(44, 23, 2, 1);
            Add(Sprite = new Sprite(GFX.Game, "characters/Xaphan/Genesis/"));
            Sprite.AddLoop("idle", "stand", 0.08f);
            Sprite.Add("walk", "walk", 0.08f);
            Sprite.Add("turn", "turn", 0.08f);
            Sprite.Add("leapUp", "leap", 0f, 0);
            Sprite.Add("dash", "leap", 0.08f);
            Sprite.Add("slash", "slash", 0.12f , 1, 0, 1);
            Sprite.Play("idle");
            Facing = Facings.Right;
            Health = 15;
            onCollideV = OnCollideV;
        }

        public override void Added(Scene scene)
        {
            base.Added(scene);
            Add(new Coroutine(SequenceRoutine()));
        }

        public override void Update()
        {
            base.Update();
            Player player = SceneAs<Level>().Tracker.GetEntity<Player>();
            if (!playerHasMoved && player != null && player.Speed != Vector2.Zero)
            {
                playerHasMoved = true;
            }
            if (ShouldSwitchFacing)
            {
                Facing = Facing == Facings.Right ? Facings.Left : Facings.Right;
                Sprite.FlipX = Facing == Facings.Left;
                ShouldSwitchFacing = false;
            }
            MoveH(Speed.X * Engine.DeltaTime, null);
            MoveV(Speed.Y * Engine.DeltaTime, onCollideV);
        }

        public void SetHealth(int health)
        {
            Health = health;
        }

        public IEnumerator SequenceRoutine()
        {
            yield return IddleRoutine();
            CannotLeapDelay = 2.5f;
            while (Health > 0)
            {
                Player player = SceneAs<Level>().Tracker.GetEntity<Player>();
                if (player != null && !Routine.Active && !MidAir && Health > 0)
                {
                    if ((Facing == Facings.Right && player.Center.X > Center.X) || (Facing == Facings.Left && player.Center.X < Center.X)) // If player is in front of Genesis
                    {
                        if (!Sprite.FlipY) // If Genesis is on ground
                        {
                            if (Calc.Random.Next(1, 101) >= 60 && CannotLeapDelay <= 0f)
                            {
                                Add(Routine = new Coroutine(LeapRoutine()));
                            }
                            /*else if (Math.Abs(player.Center.X - Center.X) >= 80 && Math.Abs(player.Center.X - Center.X) < 120 && CannotShootDelay <= 0f)
                            {
                                Add(Routine = new Coroutine(ShootRoutine()));
                            }
                            else if ((SceneAs<Level>().Session.GetFlag("boss_Challenge_Mode") ? Math.Abs(player.Center.X - Center.X) >= 80 : Math.Abs(player.Center.X - Center.X) >= 120 && Health < 10f) && CannotSwipeDelay <= 0f)
                            {
                                Add(Routine = new Coroutine(SwipeRoutine()));
                            }*/
                            else // If genesis is on the ceiling
                            {
                                Add(Routine = new Coroutine(WalkRoutine(player)));
                            }
                        }
                        else
                        {
                            if (Math.Abs(player.Center.X - Center.X) < 120 && CannotDashDelay <= 0f)
                            {
                                Add(Routine = new Coroutine(DashRoutine(player)));
                            }
                            else
                            {
                                Add(Routine = new Coroutine(WalkRoutine(player)));
                            }
                        }
                    }
                    else
                    {
                        Add(Routine = new Coroutine(TurnRoutine()));
                    }
                }
                if (CollideCheck<Bomb>(new Vector2(Position.X, Position.Y + 4f)) && !IsSlashingBomb)
                {
                    Bomb bomb = SceneAs<Level>().Tracker.GetNearestEntity<Bomb>(Facing == Facings.Right ? CenterRight : CenterLeft);
                    if (bomb != null && !bomb.Hold.IsHeld && !bomb.explode && Facing == Facings.Right ? bomb.Right > Right - 12f : bomb.Left < Left + 12f)
                    {
                        IsSlashingBomb = true;
                        if (Routine.Active)
                        {
                            Routine.Cancel();
                        }
                        Add(Routine = new Coroutine(SlashRoutine(bomb)));
                    }
                }
                if (CannotLeapDelay > 0)
                {
                    CannotLeapDelay -= Engine.DeltaTime;
                }
                if (CannotDashDelay > 0)
                {
                    CannotDashDelay -= Engine.DeltaTime;
                }
                yield return null;
            }
            if (Routine.Active)
            {
                Routine.Cancel();
            }
        }

        public IEnumerator LeapRoutine()
        {
            Sprite.Position = new Vector2(0, - 8f);
            Sprite.Play("leapUp");
            Speed.X = 0f;
            Speed.Y = -325f;
            MidAir = true;
            while (MidAir)
            {
                yield return null;
            }
        }

        public IEnumerator SlashRoutine(Bomb bomb)
        {
            Sprite.Position = new Vector2(-4f, -8f);
            Speed.X = 0f;
            Sprite.Play("slash");
            bool throwLeft = bomb.Center.X < Center.X;
            bomb.Speed = new Vector2(throwLeft ? -200f : 200f, -150f);
            yield return Sprite.CurrentAnimationTotalFrames * 0.12f + 0.1f;
            IsSlashingBomb = false;
        }

        public IEnumerator DashRoutine(Player player)
        {
            Sprite.Position = new Vector2(0, -8f);
            Sprite.Play("dash");
            Sprite.FlipY = false;
            noFlip = true;
            Speed = new Vector2(player.Center.X - Center.X + (Facing == Facings.Right ? 75 : -75), 275f);
            MidAir = true;
            while (MidAir)
            {
                yield return null;
            }
            Speed.X = 0f;
            yield return IddleRoutine();
        }

        public IEnumerator WalkRoutine(Player player)
        {
            Sprite.Position = Vector2.Zero;
            Sprite.Play("walk");
            bool walkLeft = player.Center.X < Center.X;
            float speed = Health >= 10 ? 75f : Health >= 7f ? 100f : Health >= 4f ? 125f : 150f;
            Sprite.Rate = Health >= 10 ? 1f : Health >= 7f ? 1.33f : Health >= 4f ? 1.66f : 2f;
            Speed.X = speed * (walkLeft ? -1 : 1);
            float walkDuration = Sprite.CurrentAnimationTotalFrames * 0.08f / Sprite.Rate;
            yield return walkDuration;
            Speed.X = 0f;
            Sprite.Rate = 1f;
        }

        public IEnumerator TurnRoutine()
        {
            Sprite.Position = Vector2.Zero;
            Sprite.Play("turn");
            float turnDuration = Sprite.CurrentAnimationTotalFrames * 0.08f;
            yield return turnDuration;
            yield return IddleRoutine(true);
        }

        public IEnumerator IddleRoutine(bool flip = false, bool playIddleAnim = true)
        {
            Sprite.Position = Vector2.Zero;
            ShouldSwitchFacing = flip;
            if (playIddleAnim)
            {
                Sprite.Play("idle");
            }
            if (flip)
            {
                yield return null;
            }
            else
            {
                if (!SceneAs<Level>().Session.GetFlag("boss_Challenge_Mode"))
                {
                    yield return Health >= 10 ? 0.5f : Health >= 7f ? 0.3f : Health >= 4f ? 0.15f : 0.05f;
                }
                else
                {
                    yield return Health >= 10 ? 0.3f : Health >= 7f ? 0.15f : Health >= 4f ? 0.05f : 0f;
                }
            }
        }

        private void OnCollideV(CollisionData data)
        {
            if (MidAir)
            {
                if (!noFlip)
                {
                    Sprite.FlipY = true;
                }
                Speed.Y = 0f;
                CannotLeapDelay = 3f;
                CannotDashDelay = 1.5f;
                MidAir = false;
                noFlip = false;
            }
        }
    }
}
