using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Celeste.Mod.Entities;
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

        public bool playerHasMoved;

        public Genesis(EntityData data, Vector2 offset) : base(data.Position + offset)
        {
            Collider = new Hitbox(44, 23, 2, 1);
            Add(Sprite = new Sprite(GFX.Game, "characters/Xaphan/Genesis/"));
            Sprite.AddLoop("idle", "stand", 0.08f);
            Sprite.Add("walk", "walk", 0.08f);
            Sprite.Add("turn", "turn", 0.08f);
            Sprite.Play("idle");
            Facing = Facings.Right;
            Health = 15;
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
            MoveV(Speed.Y * Engine.DeltaTime, null);
        }

        public void SetHealth(int health)
        {
            Health = health;
        }

        public IEnumerator SequenceRoutine()
        {
            yield return IddleRoutine();
            while (Health > 0)
            {
                Player player = SceneAs<Level>().Tracker.GetEntity<Player>();
                if (player != null && !Routine.Active /*&& !MidAir*/ && Health > 0)
                {
                    if ((Facing == Facings.Right && player.Center.X > Center.X) || (Facing == Facings.Left && player.Center.X < Center.X)) // If player is in front of Genesis
                    {
                        /*if (SceneAs<Level>().Session.GetFlag("boss_Challenge_Mode") ? MustJumpAway : Math.Abs(player.Center.X - Center.X) < 80 && CannotJumpDelay <= 0f)
                        {
                            MustJumpAway = false;
                            Add(Routine = new Coroutine(JumpBackRoutine()));
                        }
                        else if (Math.Abs(player.Center.X - Center.X) < 80 && SceneAs<Level>().Session.GetFlag("boss_Challenge_Mode") && CannotShootDelay <= 0f)
                        {
                            Add(Routine = new Coroutine(ShootRoutine(true)));
                        }
                        else if (Math.Abs(player.Center.X - Center.X) >= 80 && Math.Abs(player.Center.X - Center.X) < 120 && CannotShootDelay <= 0f)
                        {
                            Add(Routine = new Coroutine(ShootRoutine()));
                        }
                        else if ((SceneAs<Level>().Session.GetFlag("boss_Challenge_Mode") ? Math.Abs(player.Center.X - Center.X) >= 80 : Math.Abs(player.Center.X - Center.X) >= 120 && Health < 10f) && CannotSwipeDelay <= 0f)
                        {
                            Add(Routine = new Coroutine(SwipeRoutine()));
                        }
                        else*/
                        {
                            Add(Routine = new Coroutine(WalkRoutine(player)));
                        }
                    }
                    else
                    {
                        Add(Routine = new Coroutine(TurnRoutine()));
                    }
                }
            yield return null;
            }
        }

        public IEnumerator WalkRoutine(Player player)
        {
            Sprite.Play("walk");
            bool walkLeft = player.Center.X < Center.X;
            float speed = Health >= 10 ? 45f : Health >= 7f ? 60f : Health >= 4f ? 75f : 90f;
            Sprite.Rate = Health >= 10 ? 1f : Health >= 7f ? 1.33f : Health >= 4f ? 1.66f : 2f;
            Speed.X = speed * (walkLeft ? -1 : 1);
            float walkDuration = Sprite.CurrentAnimationTotalFrames * 0.08f / Sprite.Rate;
            yield return walkDuration;
            Speed.X = 0f;
            Sprite.Rate = 1f;
        }

        public IEnumerator TurnRoutine()
        {
            Sprite.Play("turn");
            float turnDuration = Sprite.CurrentAnimationTotalFrames * 0.08f;
            yield return turnDuration;
            yield return IddleRoutine(true);
        }

        public IEnumerator IddleRoutine(bool flip = false, bool playIddleAnim = true)
        {
            ShouldSwitchFacing = flip;
            if (playIddleAnim)
            {
                Sprite.Play("idle");
            }
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
}
