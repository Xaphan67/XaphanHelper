using System;
using System.Collections;
using Celeste.Mod.Entities;
using Celeste.Mod.XaphanHelper.Colliders;
using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste.Mod.XaphanHelper.Entities
{
    [Tracked(true)]
    [CustomEntity("XaphanHelper/Genesis")]
    public class Genesis : Actor
    {
        [Tracked(true)]
        private class GenesisAcid : Actor
        {
            Sprite Sprite;

            private PlayerCollider pc;

            public Vector2 Speed;

            private Collision onCollide;

            public GenesisAcid(Vector2 offset, Vector2 speed, bool toLeft, bool upsideDown = false) : base(offset)
            {
                Add(pc = new PlayerCollider(onCollidePlayer, Collider));
                Collider = new Hitbox(4, 4, upsideDown ? (toLeft ? -8 : -1) : (toLeft ? -8 : -1), upsideDown ? (toLeft ? 2 : -1) : -1);
                Add(Sprite = new Sprite(GFX.Game, "characters/Xaphan/Genesis/"));
                Sprite.Origin = new Vector2(toLeft ? 4f : 10f, 3f);
                Sprite.AddLoop("acid", "acid", 0.08f);
                Sprite.Add("explode", "acidExplode", 0.04f);
                Sprite.Play("acid");
                Speed = new Vector2((speed.X + Calc.Random.Next(-15, 16)) * (toLeft ? -1 : 1), speed.Y + Calc.Random.Next(-15, 16));
                Add(new Coroutine(GravityRoutine()));
                onCollide = OnCollide;
                Depth = 1;
            }

            public override void Update()
            {
                base.Update();
                if (Sprite != null && Sprite.CurrentAnimationID != "explode")
                {
                    Sprite.Rotation = (float)Math.Atan2(Center.Y + Speed.Y - Center.Y, Center.X + Speed.X - Center.X);
                }
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
                Sprite.Origin = Vector2.Zero;
                Sprite.Rotation = 0f;
                Sprite.Position = new Vector2(-17f, -13f);
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

        private class GenesisSlashEffect : Entity
        {
            private Sprite Sprite;

            private float alpha;

            private PlayerCollider pc;

            public GenesisSlashEffect(Vector2 offset, bool flip) : base(offset)
            {
                Collider = new Hitbox(24, 26, 2 , 4);
                Add(pc = new PlayerCollider(onCollidePlayer, Collider));
                Add(Sprite = new Sprite(GFX.Game, "characters/Xaphan/Genesis/"));
                Sprite.AddLoop("effect", "slashEffect", 0.08f);
                Sprite.Play("effect");
                Sprite.FlipX = flip;
                alpha = 1f;
                Add(new Coroutine(LifeRoutine()));
            }

            private void onCollidePlayer(Player player)
            {
                player.Die((player.Position - Position).SafeNormalize());
            }

            private IEnumerator LifeRoutine()
            {
                while (alpha > 0) { 
                    alpha -= Engine.DeltaTime * 2;
                    yield return null;
                }
                RemoveSelf();
            }

            public override void Render()
            {
                base.Render();
                Sprite.Color = Color.White * alpha;
                Sprite.Render();
            }
        }

        private enum Facings
        {
            Left,
            Right
        }

        private Facings Facing;

        private BombCollider bc;

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

        private float CannotShootDelay;

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
            Sprite.Add("roar", "roar", 0.08f);
            Sprite.Add("roarEnd", "roar", 0.08f, 1, 0);
            Sprite.Play("idle");
            Facing = Facings.Right;
            Health = 15;
            onCollideV = OnCollideV;
            Add(bc = new BombCollider(onBomb, new Hitbox(10, 15)));
        }

        public override void Added(Scene scene)
        {
            base.Added(scene);
            Add(new Coroutine(SequenceRoutine()));
        }

        private void onBomb(Bomb bomb)
        {
            if (Collidable && !IsSlashingBomb)
            {
                if (bomb != null && !bomb.Hold.IsHeld && !bomb.explode)
                {
                    IsSlashingBomb = true;
                    if (Routine.Active)
                    {
                        Routine.Cancel();
                    }
                    Player player = SceneAs<Level>().Tracker.GetEntity<Player>();
                    Add(Routine = new Coroutine(SlashRoutine(bomb, player)));
                }
            }
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
            bc.Collider.Position = new Vector2(Facing == Facings.Right ? 34 : 4, !Sprite.FlipY ? 4 : 6);
            Collidable = !MidAir && Sprite.CurrentAnimationID != "turn";
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
                            else if (Math.Abs(player.Center.X - Center.X) >= 80 && Math.Abs(player.Center.X - Center.X) < 160 && CannotShootDelay <= 0f)
                            {
                                Add(Routine = new Coroutine(ShootRoutine(player)));
                            }
                            else
                            {
                                Add(Routine = new Coroutine(WalkRoutine(player)));
                            }
                        }
                        else // If genesis is on the ceiling
                        {
                            if (Math.Abs(player.Center.X - Center.X) >= 40 && Math.Abs(player.Center.X - Center.X) < 120 && CannotDashDelay <= 0f)
                            {
                                Add(Routine = new Coroutine(DashRoutine(player)));
                            }
                            else if (Math.Abs(player.Center.X - Center.X) >= 80 && Math.Abs(player.Center.X - Center.X) < 160 && CannotShootDelay <= 0f)
                            {
                                Add(Routine = new Coroutine(ShootRoutine(player, true)));
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
                if (player != null && (Facing == Facings.Right ? player.Left >= Right - 16f && player.Left <= Right + 8f : player.Right <= Left + 16f && player.Right >= Left - 8f) && player.Top >= Top && player.Bottom <= Bottom && !Sprite.FlipY && !IsSlashingBomb)
                {
                    IsSlashingBomb = true;
                    if (Routine.Active)
                    {
                        Routine.Cancel();
                    }
                    Add(Routine = new Coroutine(SlashRoutine(null, player)));
                }
                if (CannotLeapDelay > 0)
                {
                    CannotLeapDelay -= Engine.DeltaTime;
                }
                if (CannotDashDelay > 0)
                {
                    CannotDashDelay -= Engine.DeltaTime;
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
        }

        public IEnumerator LeapRoutine()
        {
            Sprite.Position = new Vector2(0, - 8f);
            Sprite.Play("leapUp");
            Speed.X = 0f;
            Speed.Y = -325f;
            MidAir = true;
            Audio.Play("event:/game/xaphan/genesis_jump", Position);
            while (MidAir)
            {
                yield return null;
            }
        }

        public IEnumerator ShootRoutine(Player player, bool bellow = false)
        {
            Sprite.Position = new Vector2(0f, bellow ? 0 : -8f);
            Speed.X = 0f;
            Sprite.Play("roar");
            yield return Sprite.CurrentAnimationTotalFrames * 0.08f;
            float shootDuration = 1.2f;
            Audio.Play("event:/game/xaphan/genesis_spit", Position);
            while (shootDuration > 0)
            {
                shootDuration -= Engine.DeltaTime;
                if (SceneAs<Level>().OnInterval(0.06f))
                {
                    SceneAs<Level>().Add(new GenesisAcid(new Vector2(Position.X + (Facing == Facings.Right ? 40 : 8), Position.Y + (bellow ? 23 : 1)), new Vector2(Calc.Random.Next(SceneAs<Level>().Session.GetFlag("boss_Challenge_Mode") ? 60 : 110, SceneAs<Level>().Session.GetFlag("boss_Challenge_Mode") ? 260 : 210), bellow ? 75f : -50f), Facing == Facings.Left, bellow));
                }
                yield return null;
            }
            Sprite.Play("roarEnd");
            yield return Sprite.CurrentAnimationTotalFrames * 0.08f;
            CannotShootDelay = 5f;
        }

        public IEnumerator DashRoutine(Player player)
        {
            Sprite.Position = new Vector2(0, -8f);
            Sprite.Play("dash");
            Sprite.FlipY = false;
            noFlip = true;
            Speed = new Vector2(player.Center.X - Center.X + (Facing == Facings.Right ? 48 : -48), 275f);
            MidAir = true;
            Audio.Play("event:/game/xaphan/genesis_growl", Position);
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

        public IEnumerator SlashRoutine(Bomb bomb, Player player)
        {
            Sprite.Position = new Vector2(-4f, -8f);
            Speed.X = 0f;
            Sprite.Play("slash");
            Audio.Play("event:/game/xaphan/genesis_swing", Position);
            SceneAs<Level>().Add(new GenesisSlashEffect(Position + new Vector2(Facing == Facings.Right ? 30f : -10f, -6f), Facing == Facings.Left));
            if (bomb != null)
            {
                yield return 0.08f;
                bool throwLeft = bomb.Center.X < Center.X;
                bomb.Speed = new Vector2(throwLeft ? -250f : 250f, -150f);
            }
            yield return Sprite.CurrentAnimationTotalFrames * 0.12f + 0.1f;
            IsSlashingBomb = false;
            int num = Calc.Random.Next(1, 101);
            if (num <= 50 || player == null)
            {
                yield return LeapRoutine();
            }
            else
            {
                yield return ShootRoutine(player);
            }
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
