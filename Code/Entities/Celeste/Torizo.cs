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

            public override void Added(Scene scene)
            {
                base.Added(scene);
                Audio.Play("event:/game/xaphan/torizo_fireball", Position);
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
                int soundChance = Calc.Random.Next(1, 4);
                if (soundChance == 3)
                {
                    Audio.Play("event:/game/xaphan/torizo_fireball_explode", Position);
                }
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

            public override void Added(Scene scene)
            {
                base.Added(scene);
                Audio.Play("event:/game/xaphan/torizo_swipe", Position);
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

        [Pooled]
        private class TorizoDebris : Actor
        {
            private Image sprite;

            private Vector2 speed;

            private bool shaking;

            private bool firstHit;

            private float alpha;

            private Collision onCollideH;

            private Collision onCollideV;

            private float spin;

            private float lifeTimer;

            private string directory;

            public TorizoDebris() : base(Vector2.Zero)
            {
                Tag = Tags.TransitionUpdate;
                Collider = new Hitbox(4f, 4f, -2f, -2f);

                onCollideH = delegate
                {
                    speed.X = (0f - speed.X) * 0.5f;
                };
                onCollideV = delegate
                {
                    if (firstHit || speed.Y > 50f)
                    {
                        Audio.Play("event:/game/06_reflection/fall_spike_smash", Position, "debris_velocity", Calc.ClampedMap(speed.Y, 0f, 600f));
                    }
                    if (speed.Y > 0f && speed.Y < 40f)
                    {
                        speed.Y = 0f;
                    }
                    else
                    {
                        speed.Y = (0f - speed.Y) * 0.25f;
                    }
                    firstHit = false;
                };
            }

            protected override void OnSquish(CollisionData data)
            {
            }

            public TorizoDebris Init(Vector2 position, Vector2 center)
            {
                Collidable = true;
                Position = position;
                speed = ((position - center).SafeNormalize(60f + Calc.Random.NextFloat(60f)) + Vector2.UnitY * -150f) * new Vector2(2f, 1f);
                directory = "characters/Xaphan/Torizo/debris";
                Add(sprite = new Image(Calc.Random.Choose(GFX.Game.GetAtlasSubtextures(directory))));
                sprite.CenterOrigin();
                sprite.FlipX = Calc.Random.Chance(0.5f);
                sprite.Position = Vector2.Zero;
                sprite.Rotation = Calc.Random.NextAngle();
                shaking = false;
                sprite.Scale.X = 1f;
                sprite.Scale.Y = 1f;
                sprite.Color = Color.White;
                alpha = 1f;
                firstHit = false;
                spin = Calc.Random.Range(3.49065852f, 10.4719753f) * Calc.Random.Choose(1, -1);
                lifeTimer = Calc.Random.Range(0.6f, 2.6f);
                return this;
            }

            public override void Update()
            {
                base.Update();
                if (Collidable)
                {
                    speed.X = Calc.Approach(speed.X, 0f, Engine.DeltaTime * 100f);
                    if (!OnGround())
                    {
                        speed.Y += 400f * Engine.DeltaTime;
                    }
                    MoveH(speed.X * Engine.DeltaTime, onCollideH);
                    MoveV(speed.Y * Engine.DeltaTime, onCollideV);
                }
                if (shaking && Scene.OnInterval(0.05f))
                {
                    sprite.X = -1 + Calc.Random.Next(3);
                    sprite.Y = -1 + Calc.Random.Next(3);
                }
                if ((Scene as Level).Transitioning)
                {
                    alpha = Calc.Approach(alpha, 0f, Engine.DeltaTime * 4f);
                    sprite.Color = Color.White * alpha;
                }
                sprite.Rotation += spin * Calc.ClampedMap(Math.Abs(speed.Y), 50f, 150f) * Engine.DeltaTime;
                if (lifeTimer > 0f)
                {
                    lifeTimer -= Engine.DeltaTime;
                }
                else if (alpha > 0f)
                {
                    alpha -= 4f * Engine.DeltaTime;
                    if (alpha <= 0f)
                    {
                        RemoveSelf();
                    }
                }
                sprite.Color = Color.White  * alpha;
            }
        }

        private enum Facings
        {
            Left,
            Right
        }

        private Vector2 OrigPosition;

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

        public bool playerHasMoved;

        public bool ForcedDestroy;

        private bool MustJumpAway;

        public Torizo(EntityData data, Vector2 offset) : base(data.Position + offset)
        {
            OrigPosition = Position;
            Collider = new Hitbox(24, 80, 32, 16);
            Add(Sprite = new Sprite(GFX.Game, "characters/Xaphan/Torizo/"));
            Sprite.Add("sat", "standUp", 0f, 0);
            Sprite.Add("standUp", "standUp", 0.08f, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11);
            Sprite.Add("standUpEnd", "standUp", 0f, 11);
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
            Sprite.Play("sat");
            Facing = Facings.Right;
            Health = 15;
            onCollideV = OnCollideV;
            IgnoreJumpThrus = true;
        }

        public override void Added(Scene scene)
        {
            base.Added(scene);
            string Prefix = SceneAs<Level>().Session.Area.LevelSet;
            if (XaphanModule.ModSaveData.SavedFlags.Contains(Prefix + "_Ch1_Boss_Defeated"))
            {
                Visible = false;
            }
            Sprite.OnFrameChange = delegate (string anim)
            {
                if (Scene != null)
                {
                    int currentAnimationFrame = Sprite.CurrentAnimationFrame;
                    if ((anim.Equals("walk") && (currentAnimationFrame == 7)) || (anim.Equals("walk2") && (currentAnimationFrame == 6)))
                    {
                        SceneAs<Level>().Shake(0.3f);
                        Input.Rumble(RumbleStrength.Medium, RumbleLength.Short);
                        Audio.Play("event:/game/xaphan/torizo_walk", Position);
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
            if (Activated && !Defeated && pc == null)
            {
                colliders = new ColliderList(new Hitbox(8, 32, Position.X + 40, Position.Y + 24), new Hitbox(8, 8, Position.X + 48, Position.Y + 16));
                Add(pc = new PlayerCollider(onCollidePlayer, new Hitbox(24, 60, 32, 16)));
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
            if (Activated && !Defeated && Health > 0)
            {
                Speed.Y = Calc.Approach(Speed.Y, 200f, 800f * Engine.DeltaTime);
            }
            MoveH(Speed.X * Engine.DeltaTime, null);
            MoveV(Speed.Y * Engine.DeltaTime, onCollideV);
            if (colliders != null)
            {
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
            foreach (CustomCrumbleBlock crumbleBlock in crumbleBlocks)
            {
                crumbleBlock.Collidable = crumbleBlock.Destroyed ? false : true;
            }
            if (shield != null)
            {
                shield.Collidable = true;
            }
        }

        public void SetHealth(int health)
        {
            Health = health;
        }

        public IEnumerator SequenceRoutine()
        {
            Activated = Defeated = false;
            if (!SceneAs<Level>().Session.GetFlag("Torizo_Wakeup"))
            {
                while (!SceneAs<Level>().Session.GetFlag("Torizo_Wakeup"))
                {
                    yield return null;
                }
                StandUp();
                while (!Activated)
                {
                    yield return null;
                }
            }
            else
            {
                Sprite.Play("standUpEnd");
                Activated = true;
                CannotSwipeDelay = SceneAs<Level>().Session.GetFlag("boss_Challenge_Mode") ? 1f : 5f;
                SceneAs<Level>().Session.SetFlag("Torizo_Start", false);
            }
            while (!SceneAs<Level>().Session.GetFlag("Torizo_Start"))
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
                        if (SceneAs<Level>().Session.GetFlag("boss_Challenge_Mode") ? MustJumpAway : Math.Abs(player.Center.X - Center.X) < 80 && CannotJumpDelay <= 0f)
                        {
                            MustJumpAway = false;
                            Add(Routine = new Coroutine(JumpBackRoutine()));
                        }
                        else if (Math.Abs(player.Center.X - Center.X) >= 80 && Math.Abs(player.Center.X - Center.X) < 120 && CannotShootDelay <= 0f)
                        {
                            Add(Routine = new Coroutine(ShootRoutine()));
                        }
                        else if ((SceneAs<Level>().Session.GetFlag("boss_Challenge_Mode") ? Math.Abs(player.Center.X - Center.X) >= 80 : Math.Abs(player.Center.X - Center.X) >= 120 && Health < 10f) && CannotSwipeDelay <= 0f)
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
            if (!ForcedDestroy)
            { 
                Add(Routine = new Coroutine(KneelRoutine()));
            }
            ForcedDestroy = false;
            while (Health <= 0)
            {
                yield return null;
            }
            Visible = true;
            Add(new Coroutine(SequenceRoutine()));
            Add(new Coroutine(InvincibilityRoutine()));
            Add(new Coroutine(GravityRoutine()));
        }

        public IEnumerator JumpBackRoutine()
        {
            CannotJumpDelay = 7f;
            Sprite.Play("jumpStart");
            Audio.Play("event:/game/xaphan/torizo_attack_2", Position);
            Speed.Y = -225f;
            Speed.X = 260f * (Facing == Facings.Right ? -1 : 1);
            MidAir = true;
            while (MidAir)
            {
                if (SceneAs<Level>().OnInterval(SceneAs<Level>().Session.GetFlag("boss_Challenge_Mode") ? 0.04f : 0.06f) && Sprite.CurrentAnimationFrame >= 2)
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
            Sprite.Position = new Vector2(Facing == Facings.Right ? 23 : -7, -8);
            Sprite.Play("swipe");
            float swipeDuration = Sprite.CurrentAnimationTotalFrames * 0.08f;
            float waveYPos = 0f;
            while (swipeDuration > 0)
            {
                swipeDuration -= Engine.DeltaTime;
                if (SceneAs<Level>().OnInterval(SceneAs<Level>().Session.GetFlag("boss_Challenge_Mode") ? 0.3f : 0.4f))
                {
                    SceneAs<Level>().Add(new TorizoWave(new Vector2(Position.X + (Facing == Facings.Right ? 64 : 24), Position.Y + 40 + waveYPos * 24), new Vector2(SceneAs<Level>().Session.GetFlag("boss_Challenge_Mode") ? 225f : 175f, 0f), Facing == Facings.Left));
                    waveYPos += 1f;
                    if (waveYPos > 1f)
                    {
                        waveYPos = 0f;
                    }
                }
                yield return null;
            }
            CannotSwipeDelay = SceneAs<Level>().Session.GetFlag("boss_Challenge_Mode") ? Calc.Random.Next(3, 6) : 5f;
            yield return IddleRoutine();
        }

        public IEnumerator ShootRoutine()
        {
            Sprite.Position = new Vector2(Facing == Facings.Right ? 21 : 11, 8);
            Sprite.Play("shoot");
            Audio.Play("event:/game/xaphan/torizo_attack_1", Position);
            float shootAnimDuration = Sprite.CurrentAnimationTotalFrames * 0.08f;
            yield return shootAnimDuration;
            yield return SceneAs<Level>().Session.GetFlag("boss_Challenge_Mode") ? 0.2f : 0.5f;
            float shootDuration = 1f;
            while (shootDuration > 0)
            {
                shootDuration -= Engine.DeltaTime;
                if (SceneAs<Level>().OnInterval(0.06f))
                {
                    SceneAs<Level>().Add(new TorizoFireball(new Vector2(Position.X + (Facing == Facings.Right ? 56 : 24), Position.Y + 32), new Vector2(Calc.Random.Next(SceneAs<Level>().Session.GetFlag("boss_Challenge_Mode")? 60 : 110, SceneAs<Level>().Session.GetFlag("boss_Challenge_Mode") ? 260 : 210), 0f), Facing == Facings.Left));
                }
                yield return null;
            }
            Sprite.Play("shootReverse");
            yield return shootAnimDuration;
            CannotShootDelay = SceneAs<Level>().Session.GetFlag("boss_Challenge_Mode") ? Calc.Random.Next(3, 6) : 5f;
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

        public IEnumerator KneelRoutine(bool skipAnim = false)
        {
            Health = 0;
            colliders = null;
            shield.RemoveSelf();
            pc = null;
            InvincibilityDelay = 0f;
            Speed = Vector2.Zero;
            Collidable = false;
            Activated = false;
            Defeated = true;
            Sprite.Stop();
            if (!skipAnim)
            {
                Audio.Play("event:/game/xaphan/torizo_death", Position);
                float musicFadeStart = 0f;
                while (musicFadeStart < 1)
                {
                    musicFadeStart += Engine.DeltaTime;
                    Audio.SetMusicParam("fade", 1f - musicFadeStart);
                    yield return null;
                }
                yield return 0.75f;
            }
            Sprite.Position = new Vector2(Facing == Facings.Right ? 21 : 11, 16);
            Sprite.Play("kneel");
            if (!skipAnim)
            {
                yield return 1.5f;
            }
            SceneAs<Level>().Displacement.AddBurst(Position + Collider.Center, 0.5f, 16f, 64f, 0.5f);
            for (int i = 0; i < 6; i++)
            {
                for (int j = 0; j < 14; j++)
                {
                    Vector2 vector2 = new(i * 4 + 4f, j * 4 + 4f);
                    TorizoDebris debris = Engine.Pooler.Create<TorizoDebris>().Init(Position + Collider.Position + vector2, Position + Collider.Center);
                    Scene.Add(debris);
                }
            }
            Audio.Play("event:/game/xaphan/drone_destroy", Position);
            Audio.SetMusicParam("fade", 1f);
            SceneAs<Level>().Session.Audio.Music.Event = SFX.EventnameByHandle("event:/music/xaphan/lvl_0_item");
            SceneAs<Level>().Session.Audio.Apply();
            Visible = false;
            while (SceneAs<Level>().Session.GetFlag("In_bossfight"))
            {
                yield return null;
            }
            Sprite.Position = Vector2.Zero;
            Position = OrigPosition;
            Sprite.Play("sat");
            if (Facing == Facings.Left)
            {
                ShouldSwitchFacing = true;
            }
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
            if (!SceneAs<Level>().Session.GetFlag("boss_Challenge_Mode"))
            {
                yield return Health >= 10 ? 0.5f : Health >= 7f ? 0.3f : Health >= 4f ? 0.15f : 0.05f;
            }
            else
            {
                yield return Health >= 10 ? 0.3f : Health >= 7f ? 0.15f : Health >= 4f ? 0.05f : 0f;
            }
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

        public void Appear(bool visible)
        {
            SceneAs<Level>().Displacement.AddBurst(Position + new Vector2(24f, 72f), 0.5f, 16f, 64f, 0.5f);
            Visible = visible;
        }

        public void GetHit()
        {
            if (Health > 0 && InvincibilityDelay <= 0)
            {
                Audio.Play("event:/game/xaphan/torizo_hit", Position);
                Health -= 1;
                if (SceneAs<Level>().Session.GetFlag("boss_Challenge_Mode"))
                {
                    if (Health == 14 || Health == 12 || Health == 9 || Health == 6 || Health <= 3)
                    {
                        MustJumpAway = true;
                    }
                }
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
                Collidable = true;
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
