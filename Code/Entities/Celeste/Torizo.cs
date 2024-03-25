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
    [CustomEntity("XaphanHelper/Torizo")]
    public class Torizo : Actor
    {
        private class TorizoFireball : Actor
        {
            Sprite Sprite;

            private PlayerCollider pc;

            public Vector2 Speed;

            private Collision onCollide;

            public TorizoFireball(Vector2 offset, Vector2 speed, bool toLeft) : base(offset)
            {
                Add(pc = new PlayerCollider(onCollidePlayer, new Circle(4f)));
                Collider = new Hitbox(2, 2);
                Add(Sprite = new Sprite(GFX.Game, "characters/Xaphan/Torizo/"));
                Sprite.Origin = Vector2.One * 8;
                Sprite.AddLoop("fireball", "fireball", 0.08f);
                Sprite.Add("explode", "fireballExplode", 0.08f);
                Sprite.Play("fireball");
                Speed = new Vector2((speed.X + Calc.Random.Next(-15, 16)) * (toLeft ? -1 : 1), speed.Y + Calc.Random.Next(-15, 16));
                Add(new Coroutine(GravityRoutine()));
                onCollide = OnCollide;
                Depth = 1;
            }

            public override void Update()
            {
                base.Update();
                Sprite.Rotation = (float)Math.Atan2(Center.Y + Speed.Y - Center.Y, Center.X + Speed.X - Center.X);
                MoveH(Speed.X * Engine.DeltaTime, onCollide);
                MoveV(Speed.Y * Engine.DeltaTime, onCollide);
            }

            public IEnumerator GravityRoutine()
            {
                while (Speed.Y <= 250f)
                {
                    Speed.Y += 4f;
                    yield return null;
                }
            }

            private void OnCollide(CollisionData data)
            {
                Collidable = false;
                Speed = Vector2.Zero;
                Sprite.Play("explode");
                Sprite.OnLastFrame = onLastFrame;
            }

            private void onCollidePlayer(Player player)
            {
                player.Die((player.Position - Position).SafeNormalize());
            }

            private void onLastFrame(string s)
            {
                if (Sprite.CurrentAnimationID == "explode")
                {
                    RemoveSelf();
                }
            }
        }

        private class TorizoWave : Actor
        {
            Sprite Sprite;

            private PlayerCollider pc;

            public Vector2 Speed;

            private float MaxXSpeed;

            private float Lifetime = 5f;

            public TorizoWave(Vector2 offset, Vector2 speed, bool toLeft) : base(offset)
            {
                Add(pc = new PlayerCollider(onCollidePlayer, new Hitbox(13, 20, -5, -6)));
                Add(Sprite = new Sprite(GFX.Game, "characters/Xaphan/Torizo/"));
                Sprite.Origin = Vector2.One * 8;
                Sprite.FlipX = toLeft;
                Sprite.AddLoop("wave", "wave", 0.04f);
                Sprite.Play("wave");
                Sprite.Scale = Vector2.One * 0.3f;
                MaxXSpeed = speed.X;
                Speed = new Vector2(0 * (toLeft ? -1 : 1), speed.Y);
                Add(new Coroutine(SpeedRoutine(toLeft)));
                Add(new Coroutine(SpriteRoutine()));
                Add(new Coroutine(LifeRoutine()));
            }

            public override void Update()
            {
                base.Update();
                MoveH(Speed.X * Engine.DeltaTime);
                MoveV(Speed.Y * Engine.DeltaTime);
            }

            public IEnumerator SpeedRoutine(bool toLeft)
            {
                while (Speed.X < MaxXSpeed)
                {
                    Speed.X += 3f * (toLeft ? -1 : 1);
                    yield return null;
                }
                Speed.X = MaxXSpeed;
            }

            public IEnumerator SpriteRoutine()
            {
                while (Sprite.Scale.X < 1 && Sprite.Scale.Y < 1)
                {
                    Sprite.Scale.X += Engine.DeltaTime;
                    Sprite.Scale.Y += Engine.DeltaTime;
                    yield return null;
                }
                Sprite.Scale = Vector2.One;
            }

            public IEnumerator LifeRoutine()
            {
                yield return Lifetime;
                RemoveSelf();
            }

            private void onCollidePlayer(Player player)
            {
                player.Die((player.Position - Position).SafeNormalize());
            }
        }

        private enum Facings
        {
            Left,
            Right
        }

        private PlayerCollider pc;

        public ColliderList colliders;

        private InvisibleBarrier shield;

        private Facings Facing;

        private bool ShouldSwitchFacing;

        private Coroutine Routine = new();

        private Sprite Sprite;

        public bool Activated;

        private bool Defeated;

        public int Health;

        private float InvincibilityDelay;

        private bool Flashing;

        public Vector2 Speed;

        private bool MidAir;

        private Collision onCollideV;

        private float CannotJumpDelay;

        private float CannotSwipeDelay;

        private float CannotShootDelay;

        public Torizo(EntityData data, Vector2 offset) : base(data.Position + offset)
        {
            Add(Sprite = new Sprite(GFX.Game, "characters/Xaphan/Torizo/"));
            Sprite.Add("sit", "standUp", 0f, 0);
            Sprite.Add("standUp", "standUp", 0.08f, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11);
            Sprite.Add("idle", "walk", 0f, 0);
            Sprite.Add("walk", "walk", 0.08f, 0, 1, 2, 3, 4, 5, 6, 7);
            Sprite.Add("walk2", "walk", 0.08f, 8, 9, 10 ,11, 12, 13, 14);
            Sprite.Add("jumpStart", "jump", 0.08f, 0, 1, 2, 3, 4, 5);
            Sprite.Add("jumpEnd", "jump", 0.08f, 6, 7, 8, 9, 10, 11, 12, 13);
            Sprite.Add("swipe", "swipe", 0.08f);
            Sprite.Add("shoot", "shoot", 0.08f);
            Sprite.Add("shootReverse", "shoot", 0.08f, 5, 4, 3, 2, 1, 0);
            Sprite.Add("turn", "turn", 0.08f);
            Sprite.Add("kneel", "kneel", 0.08f);
            Sprite.Play("sit");
            Facing = Facings.Right;
            Health = 15;
            onCollideV = OnCollideV;
        }

        public override void Added(Scene scene)
        {
            base.Added(scene);
            Sprite.OnFrameChange = delegate (string anim)
            {
                if (Scene != null)
                {
                    int currentAnimationFrame = Sprite.CurrentAnimationFrame;
                    if ((anim.Equals("walk") && (currentAnimationFrame == 7)) || (anim.Equals("walk2") && (currentAnimationFrame == 6)))
                    {
                        SceneAs<Level>().Shake(0.3f);
                        Input.Rumble(RumbleStrength.Medium, RumbleLength.Short);
                    }
                }
            };
            Add(new Coroutine(SequenceRoutine()));
            Add(new Coroutine(InvincibilityRoutine()));
            Add(new Coroutine(GravityRoutine()));
        }

        public override void Update()
        {
            List<Entity> crumbleBlocks = Scene.Tracker.GetEntities<CustomCrumbleBlock>().ToList();
            if (shield != null)
            {
                shield.Collidable = false;
            }
            foreach (CustomCrumbleBlock crumbleBlock in crumbleBlocks)
            {
                crumbleBlock.Collidable = false;
            }
            base.Update();
            if (ShouldSwitchFacing)
            {
                Facing = Facing == Facings.Right ? Facings.Left : Facings.Right;
                Sprite.FlipX = Facing == Facings.Left;
                ShouldSwitchFacing = false;
            }
            if (Activated && pc == null)
            {
                Collider = new Hitbox(24, 80, 32, 16);
                colliders = new ColliderList(new Hitbox(8, 32, Position.X + 40, Position.Y + 24), new Hitbox(8, 8, Position.X + 48, Position.Y + 16));
                Add(pc = new PlayerCollider(onCollidePlayer, new Hitbox(24, 52, 32, 16)));
                SceneAs<Level>().Add(shield = new InvisibleBarrier(Position + new Vector2(33, 19), 6, 37));
            }
            if (Flashing)
            {
                Sprite.Color = Color.Red;
            }
            else
            {
                Sprite.Color = Color.White;
            }
            if (Activated && !Defeated)
            {
                Speed.Y = Calc.Approach(Speed.Y, 200f, 800f * Engine.DeltaTime);
                MoveH(Speed.X * Engine.DeltaTime, null);
                MoveV(Speed.Y * Engine.DeltaTime, onCollideV);
                foreach (Collider collider in colliders.colliders)
                {
                    if (collider.Height == 32)
                    {
                        collider.Position.X = Position.X + 40;
                        collider.Position.Y = Position.Y + 24;
                    }
                    else
                    {
                        if (Facing == Facings.Right)
                        {
                            if (Sprite.CurrentAnimationID == "turn")
                            {
                                if (Sprite.CurrentAnimationFrame == 0)
                                {
                                    collider.Position.X = Position.X + 48;
                                }
                                else if (Sprite.CurrentAnimationFrame <= 3)
                                {
                                    collider.Position.X = Position.X + 40;
                                }
                                else
                                {
                                    collider.Position.X = Position.X + 32;
                                }
                            }
                            else
                            {
                                collider.Position.X = Position.X + 48;
                            }
                        }
                        else
                        {
                            if (Sprite.CurrentAnimationID == "turn")
                            {
                                if (Sprite.CurrentAnimationFrame == 0)
                                {
                                    collider.Position.X = Position.X + 32;
                                }
                                else if (Sprite.CurrentAnimationFrame <= 3)
                                {
                                    collider.Position.X = Position.X + 40;
                                }
                                else
                                {
                                    collider.Position.X = Position.X + 48;
                                }
                            }
                            else
                            {
                                collider.Position.X = Position.X + 32;
                            }
                        }
                        collider.Position.Y = Position.Y + 16;
                    }
                }
                if (shield != null)
                {
                    if (Facing == Facings.Right)
                    {
                        if (Sprite.CurrentAnimationID == "turn")
                        {
                            if (Sprite.CurrentAnimationFrame == 0)
                            {
                                shield.Position.X = Position.X + 33;
                            }
                            else if (Sprite.CurrentAnimationFrame <= 3)
                            {
                                shield.Position.X = Position.X + 41;
                            }
                            else
                            {
                                shield.Position.X = Position.X + 49;
                            }
                        }
                        else
                        {
                            shield.Position.X = Position.X + 33;
                        }
                        
                    }
                    else
                    {
                        if (Sprite.CurrentAnimationID == "turn")
                        {
                            if (Sprite.CurrentAnimationFrame == 0)
                            {
                                shield.Position.X = Position.X + 49;
                            }
                            else if (Sprite.CurrentAnimationFrame <= 3)
                            {
                                shield.Position.X = Position.X + 41;
                            }
                            else
                            {
                                shield.Position.X = Position.X + 33;
                            }
                        }
                        else
                        {
                            shield.Position.X = Position.X + 49;
                        }
                        
                    }
                    shield.Position.Y = Position.Y + 19;
                }
            }
            if (Health <= 0)
            {
                Defeated = true;
                Collider = null;
                colliders = null;
                Remove(pc);
                shield.RemoveSelf();
            }
            foreach (CustomCrumbleBlock crumbleBlock in crumbleBlocks)
            {
                crumbleBlock.Collidable = crumbleBlock.Destroyed ? false : true;
            }
            if (shield != null)
            {
                shield.Collidable = true;
            }
        }

        public IEnumerator SequenceRoutine()
        {
            yield return 1f;
            StandUp();
            while (!Activated)
            {
                yield return null;
            }
            while (Health > 0)
            {
                Player player = SceneAs<Level>().Tracker.GetEntity<Player>();
                if (player != null && !Routine.Active && !MidAir && Health > 0)
                {
                    if ((Facing == Facings.Right && player.Center.X > Center.X) || (Facing == Facings.Left && player.Center.X < Center.X)) // If player is in front of Torizo
                    {
                        if (Math.Abs(player.Center.X - Center.X) < 80 && CannotJumpDelay <= 0f)
                        {
                            Add(Routine = new Coroutine(JumpBackRoutine()));
                        }
                        else if (Math.Abs(player.Center.X - Center.X) >= 80 && Math.Abs(player.Center.X - Center.X) < 120 && CannotShootDelay <= 0f)
                        {
                            Add(Routine = new Coroutine(ShootRoutine()));
                        }
                        else if (Math.Abs(player.Center.X - Center.X) >= 120 && Health < 10f && CannotSwipeDelay <= 0f)
                        {
                            Add(Routine = new Coroutine(SwipeRoutine()));
                        }
                        else
                        {
                            Add(Routine = new Coroutine(WalkRoutine(player)));
                        }
                    }
                    else
                    {
                        Add(Routine = new Coroutine(TurnRoutine()));
                    }
                }
                if (CannotJumpDelay > 0)
                {
                    CannotJumpDelay -= Engine.DeltaTime;
                }
                if (CannotSwipeDelay > 0)
                {
                    CannotSwipeDelay -= Engine.DeltaTime;
                }
                if (CannotShootDelay > 0)
                {
                    CannotShootDelay -= Engine.DeltaTime;
                }
                yield return null;
            }
            if (Routine.Active)
            {
                Routine.Cancel();
            }
            Add(Routine = new Coroutine(KneelRoutine()));
        }

        public IEnumerator JumpBackRoutine()
        {
            CannotJumpDelay = 5f;
            Sprite.Play("jumpStart");
            Speed.Y = -225f;
            Speed.X = 260f * (Facing == Facings.Right ? -1 : 1);
            MidAir = true;
            while (MidAir)
            {
                if (SceneAs<Level>().OnInterval(0.06f) && Sprite.CurrentAnimationFrame >= 2)
                {
                    SceneAs<Level>().Add(new TorizoFireball(new Vector2(Position.X + (Facing == Facings.Right ? 56 : 24), Position.Y + 24), new Vector2(75f, -125f), Facing == Facings.Left));
                }
                yield return null;
            }
            Sprite.Play("jumpEnd");
            yield return Sprite.CurrentAnimationTotalFrames * 0.08f;
            yield return IddleRoutine();
        }

        public IEnumerator SwipeRoutine()
        {
            CannotSwipeDelay = 5f;
            Sprite.Position = new Vector2(Facing == Facings.Right ? 23 : -7, -8);
            Sprite.Play("swipe");
            float swipeDuration = Sprite.CurrentAnimationTotalFrames * 0.08f;
            float waveYPos = 0f;
            while (swipeDuration > 0)
            {
                swipeDuration -= Engine.DeltaTime;
                if (SceneAs<Level>().OnInterval(0.4f))
                {
                    SceneAs<Level>().Add(new TorizoWave(new Vector2(Position.X + (Facing == Facings.Right ? 64 : 24), Position.Y + 24 + waveYPos * 24), new Vector2(175f, 0f), Facing == Facings.Left));
                    waveYPos += 1f;
                    if (waveYPos > 1f)
                    {
                        waveYPos = 0f;
                    }
                }
                yield return null;
            }
            yield return IddleRoutine();
        }

        public IEnumerator ShootRoutine()
        {
            Sprite.Position = new Vector2(Facing == Facings.Right ? 21 : 11, 8);
            CannotShootDelay = 5f;
            Sprite.Play("shoot");
            float shootAnimDuration = Sprite.CurrentAnimationTotalFrames * 0.08f;
            yield return shootAnimDuration;
            float shootDuration = 1f;
            while (shootDuration > 0)
            {
                shootDuration -= Engine.DeltaTime;
                if (SceneAs<Level>().OnInterval(0.06f))
                {
                    SceneAs<Level>().Add(new TorizoFireball(new Vector2(Position.X + (Facing == Facings.Right ? 56 : 24), Position.Y + 32), new Vector2(Calc.Random.Next(110, 210), 0f), Facing == Facings.Left));
                }
                yield return null;
            }
            Sprite.Play("shootReverse");
            yield return shootAnimDuration;
            yield return IddleRoutine();
        }

        public IEnumerator WalkRoutine(Player player)
        {
            Sprite.Position = new Vector2(Facing == Facings.Right ? 21 : 11, 0);
            Sprite.Play("walk");
            bool walkLeft = player.Center.X < Center.X;
            float speed = Health >= 10 ? 45f : Health >= 7f ? 60f : Health >= 4f ? 75f : 90f;
            Sprite.Rate = Health >= 10 ? 1f : Health >= 7f ? 1.33f : Health >= 4f ? 1.66f : 2f;
            Speed.X = speed * (walkLeft ? -1 : 1);
            float walkDuration = Sprite.CurrentAnimationTotalFrames * 0.08f / Sprite.Rate;
            yield return walkDuration;
            Speed.X = 0f;
            Sprite.Rate = 1f;
            yield return IddleRoutine(playIddleAnim: false);
            Sprite.Play("walk2");
            Sprite.Rate = Health >= 10 ? 1f : Health >= 7f ? 1.33f : Health >= 4f ? 1.66f : 2f;
            Speed.X = speed * (walkLeft ? -1 : 1);
            walkDuration = Sprite.CurrentAnimationTotalFrames * 0.08f / Sprite.Rate;
            yield return walkDuration;
            Speed.X = 0f;
            Sprite.Rate = 1f;
            yield return IddleRoutine();
        }

        public IEnumerator TurnRoutine()
        {
            Sprite.Position = new Vector2(16, 0);
            Sprite.Play("turn");
            float turnDuration = Sprite.CurrentAnimationTotalFrames * 0.08f;
            yield return turnDuration;
            yield return IddleRoutine(true);
        }

        public IEnumerator KneelRoutine()
        {
            Sprite.Stop();
            yield return 0.75f;
            Sprite.Position = new Vector2(Facing == Facings.Right ? 21 : 11, 16);
            Sprite.Play("kneel");
        }

        public IEnumerator IddleRoutine(bool flip = false, bool playIddleAnim = true)
        {
            Sprite.Position = new Vector2(Facing == Facings.Right ? 21 : 11, 0);
            ShouldSwitchFacing = flip;
            if (flip)
            {
                Sprite.Position = new Vector2(Facing == Facings.Right ? 11 : 21, 0);
            }
            if (playIddleAnim)
            {
                Sprite.Play("idle");
            }
            yield return Health >= 10 ? 0.5f : Health >= 7f ? 0.3f : Health >= 4f ? 0.15f : 0.05f;
        }

        public IEnumerator InvincibilityRoutine()
        {
            while (Health > 0)
            {
                while (InvincibilityDelay > 0)
                {
                    if (Scene.OnRawInterval(0.06f))
                    {
                        Flashing = !Flashing;
                    }
                    InvincibilityDelay -= Engine.DeltaTime;
                    yield return null;
                }
                Flashing = false;
                yield return null;
            }
        }

        public IEnumerator GravityRoutine()
        {
            while (Health > 0)
            {
                while (MidAir)
                {
                    Speed.Y += 1f;
                    yield return null;
                }
                yield return null;
            }
        }

        public void StandUp()
        {
            Sprite.Play("standUp");
            Sprite.OnLastFrame = onLastFrame;
        }

        public void GetHit()
        {
            if (Health > 0 && InvincibilityDelay <= 0)
            {
                Health -= 1;
                InvincibilityDelay = 0.75f;
            }
        }

        private void onCollidePlayer(Player player)
        {
            player.Die((player.Position - Position).SafeNormalize());
        }

        private void onLastFrame(string s)
        {
            if (Sprite.LastAnimationID == "standUp")
            {
                Activated = true;
                Sprite.OnLastFrame = null;
            }
        }

        private void OnCollideV(CollisionData data)
        {
            if (MidAir)
            {
                Speed.X = 0f;
                SceneAs<Level>().Shake(0.6f);
                Input.Rumble(RumbleStrength.Strong, RumbleLength.Medium);
                MidAir = false;
            }
        }

        public override void DebugRender(Camera camera)
        {
            base.DebugRender(camera);
            if (colliders != null)
            {
                foreach (Collider collider in colliders.colliders)
                {
                    Draw.HollowRect(collider, Color.Red);
                }
            }
        }
    }
}
