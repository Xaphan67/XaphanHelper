using System;
using Celeste.Mod.XaphanHelper.Colliders;
using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste.Mod.XaphanHelper.Entities
{
    [Tracked(true)]
    public class Crate : Actor
    {
        [Tracked(true)]
        public class LaserBlocker : Entity
        {
            public Vector2 Offset;

            public Crate Crate;

            private WeaponCollider WeaponCollider;

            public LaserBlocker(Vector2 position, Vector2 offset, Crate crate) : base(position + offset)
            {
                Tag = Tags.Persistent;
                Collider = new Hitbox(14f, 14f);
                Offset = offset;
                Crate = crate;
                Add(WeaponCollider = new WeaponCollider
                {
                    OnCollideBeam = Crate.HitByBeam,
                    OnCollideMissile = Crate.HitByMissile
                });
            }

            public override void Update()
            {
                base.Update();
                if (Crate != null)
                { 
                    Position = Crate.Position + Offset;
                }
                else
                {
                    RemoveSelf();
                }
            }
        }

        public CratesSpawner SourceSpawner;

        public string Type;

        private LaserBlocker LaserBlock;

        private MTexture Texture;

        public Vector2 Speed;

        public float noGravityTimer;

        private Vector2 prevLiftSpeed;

        private Collision onCollideH;

        private Collision onCollideV;

        public Holdable Hold;

        private HoldableCollider hitSeeker;

        private float swatTimer;

        private Vector2 previousPosition;

        public bool Destroyed;

        public Crate(Vector2 position, string type, CratesSpawner spawner) : base(position)
        {
            SourceSpawner = spawner;
            Type = type;
            Collider = new Hitbox(8f, 10f, -4f, -10f);
            previousPosition = Position;
            Texture = GFX.Game["objects/XaphanHelper/Crate/" + Type.ToLower() + "00"];
            Add(Hold = new Holdable(0.1f));
            Hold.PickupCollider = new Hitbox(12f, 14f, -6f, -14f);
            Hold.SlowFall = false;
            Hold.SlowRun = false;
            Hold.OnPickup = OnPickup;
            Hold.OnRelease = OnRelease;
            Hold.DangerousCheck = Dangerous;
            Hold.OnHitSeeker = HitSeeker;
            Hold.OnSwat = Swat;
            Hold.OnHitSpring = HitSpring;
            Hold.OnHitSpinner = HitSpinner;
            Hold.SpeedGetter = (() => Speed);
            onCollideH = OnCollideH;
            onCollideV = OnCollideV;
            Depth = 100;
        }

        public static void Load()
        {
            On.Celeste.Spikes.Added += OnSpikesAdded;
        }

        public static void Unload()
        {
            On.Celeste.Spikes.Added -= OnSpikesAdded;
        }

        private static void OnSpikesAdded(On.Celeste.Spikes.orig_Added orig, Spikes self, Scene scene)
        {
            orig(self, scene);
            if (scene.Tracker.GetEntities<Crate>().Count > 0 || scene.Tracker.GetEntities<CratesSpawner>().Count > 0)
            {
                self.Add(new CrateCollider(OnCrate, self.Direction));
            }
        }

        private static void OnCrate(Crate crate, Spikes.Directions direction)
        {
            if (crate.Type == "Wood")
            {
                Vector2 speed = new Vector2(crate.Hold.IsHeld ? crate.Hold.Holder.Speed.X : crate.Speed.X, crate.Hold.IsHeld ? crate.Hold.Holder.Speed.Y : crate.Speed.Y);
                switch (direction.ToString())
                {
                    case "Up":
                        if (crate.Hold.IsHeld ? speed.Y > 0f : speed.Y >= 0f)
                        {
                            crate.Destroy();
                        }
                        break;
                    case "Down":
                        if (crate.Hold.IsHeld ? speed.Y < 0f : speed.Y <= 0f)
                        {
                            crate.Destroy();
                        }
                        break;
                    case "Left":
                        if (crate.Hold.IsHeld ? speed.X > 0f : speed.X >= 0f)
                        {
                            crate.Destroy();
                        }
                        break;
                    case "Right":
                        if (crate.Hold.IsHeld ? speed.X < 0f : speed.X <= 0f)
                        {
                            crate.Destroy();
                        }
                        break;
                }
            }
        }

        public override void Added(Scene scene)
        {
            base.Added(scene);
            SceneAs<Level>().Add(LaserBlock = new LaserBlocker(Position, new Vector2(-7f, -15f), this));
        }

        private void OnPickup()
        {
            Speed = Vector2.Zero;
            AddTag(Tags.Persistent);
            AllowPushing = false;
        }

        private void OnRelease(Vector2 force)
        {
            RemoveTag(Tags.Persistent);
            if (force.X != 0f && force.Y == 0f)
            {
                force.Y = -0.4f;
            }
            Speed = force * 200f;
            if (Speed != Vector2.Zero)
            {
                noGravityTimer = 0.1f;
            }
            AllowPushing = true;
        }

        public void Swat(HoldableCollider hc, int dir)
        {
            if (Hold.IsHeld && hitSeeker == null)
            {
                swatTimer = 0.1f;
                hitSeeker = hc;
                Hold.Holder.Swat(dir);
            }
        }

        public bool Dangerous(HoldableCollider holdableCollider)
        {
            return !Hold.IsHeld && Speed != Vector2.Zero && hitSeeker != holdableCollider;
        }

        public void HitSeeker(Seeker seeker)
        {
            if (!Hold.IsHeld)
            {
                Speed = (Center - seeker.Center).SafeNormalize(120f);
            }
            HitSound();
        }

        public void HitSpinner(Entity spinner)
        {
            if (!Hold.IsHeld && Speed.Length() < 0.01f && LiftSpeed.Length() < 0.01f && (previousPosition - ExactPosition).Length() < 0.01f && OnGround())
            {
                int num = Math.Sign(X - spinner.X);
                if (num == 0)
                {
                    num = 1;
                }
                Speed.X = num * 80f;
                Speed.Y = -30f;
            }
        }

        public bool HitSpring(Spring spring)
        {
            if (!Hold.IsHeld)
            {
                if (spring.Orientation == Spring.Orientations.Floor && Speed.Y >= 0f)
                {
                    Speed.X *= 0.5f;
                    Speed.Y = -160f;
                    noGravityTimer = 0.15f;
                    return true;
                }
                if (spring.Orientation == Spring.Orientations.WallLeft && Speed.X <= 0f)
                {
                    MoveTowardsY(spring.CenterY + 5f, 4f);
                    Speed.X = 220f;
                    Speed.Y = -80f;
                    noGravityTimer = 0.1f;
                    return true;
                }
                if (spring.Orientation == Spring.Orientations.WallRight && Speed.X >= 0f)
                {
                    MoveTowardsY(spring.CenterY + 5f, 4f);
                    Speed.X = -220f;
                    Speed.Y = -80f;
                    noGravityTimer = 0.1f;
                    return true;
                }
            }
            return false;
        }

        private void OnCollideH(CollisionData data)
        {
            HitSound();
            if (Math.Abs(Speed.X) > 100f)
            {
                ImpactParticles(data.Direction);
            }
            Speed.X *= -0.2f;
        }

        private void OnCollideV(CollisionData data)
        {
            if (Speed.Y > 0f)
            {
                HitSound();
            }
            if (Speed.Y > 160f)
            {
                ImpactParticles(data.Direction);
            }
            if (Speed.Y > 140f && !(data.Hit is SwapBlock) && !(data.Hit is DashSwitch))
            {
                Speed.Y *= -0.4f;
            }
            else
            {
                Speed.Y = 0f;
            }
        }

        private void HitSound()
        {
            if (Type == "Metal")
            {
                Audio.Play("event:/char/madeline/landing", Position, "surface_index", 7);
            }
            else
            {
                Audio.Play("event:/char/madeline/landing", Position, "surface_index", 18);
            }
        }

        private void ImpactParticles(Vector2 dir)
        {
            float direction;
            Vector2 position = default(Vector2);
            Vector2 positionRange;
            if (dir.X > 0f)
            {
                direction = (float)Math.PI;
                position = new Vector2(Right, Y - 4f);
                positionRange = Vector2.UnitY * 6f;
            }
            else if (dir.X < 0f)
            {
                direction = 0f;
                position = new Vector2(Left, Y - 4f);
                positionRange = Vector2.UnitY * 6f;
            }
            else if (dir.Y > 0f)
            {
                direction = -(float)Math.PI / 2f;
                position = new Vector2(X, Bottom);
                positionRange = Vector2.UnitX * 6f;
            }
            else
            {
                direction = (float)Math.PI / 2f;
                position = new Vector2(X, Top);
                positionRange = Vector2.UnitX * 6f;
            }
            SceneAs<Level>().Particles.Emit(TheoCrystal.P_Impact, 12, position, positionRange, direction);
        }

        public void Destroy()
        {
            Destroyed = true;
            AllowPushing = false;
            if (Type == "Metal")
            {
                Audio.Play("event:/game/general/wall_break_ice", Position);
            }
            else
            {
                Audio.Play("event:/game/general/wall_break_wood", Position);
            }

            for (int i = 0; i < Width / 8f; i++)
            {
                for (int j = 0; j < Height / 8f; j++)
                {
                    if (Type == "Metal")
                    {
                        Scene.Add(Engine.Pooler.Create<Debris>().Init(Position + new Vector2(4 + (i * 8), 4 + (j * 8)) + new Vector2(-8f, -16f), '8', false).BlastFrom(Center));
                    }
                    else
                    {
                        Scene.Add(Engine.Pooler.Create<Debris>().Init(Position + new Vector2(4 + (i * 8), 4 + (j * 8)) + new Vector2(-8f, -16f), '9', false).BlastFrom(Center));
                    }
                }
            }

            RemoveSelf();
        }

        public void Bounce(Vector2 dir)
        {
            if (Hold.IsHeld)
            {
                Hold.Holder.Throw();
            }
            Audio.Play("event:/game/xaphan/crate_bounce", Position);
            Speed = new Vector2(170f * dir.X, 130f * dir.Y);
        }

        private void HitByBeam(Beam beam)
        {
            HitSound();
            if (Type == "Wood")
            {
                Speed = new Vector2(130f * beam.Direction.X, 0f);
            }
            beam.CollideSolid(beam.Direction);
        }

        private void HitByMissile(Missile missile)
        {
            HitSound();
            if (Type == "Wood")
            {
                Destroy();
            }
            else if (Type == "Metal" && missile.SuperMissile)
            {
                Destroy();
            }
            missile.CollideSolid(missile.Direction);
        }

        public override void Update()
        {
            Slope.SetCollisionBeforeUpdate(this);
            if (CollideCheck(SourceSpawner))
            {
                SourceSpawner.Collidable = false;
            }
            base.Update();
            if (!Hold.IsHeld)
            {
                foreach (Slope slope in SceneAs<Level>().Tracker.GetEntities<Slope>())
                {
                    if (slope.UpsideDown && CollideCheck(slope))
                    {
                        Position.Y += 1;
                    }
                }
            }
            if (swatTimer > 0f)
            {
                swatTimer -= Engine.DeltaTime;
            }
            if (hitSeeker != null && swatTimer <= 0f && !hitSeeker.Check(Hold))
            {
                hitSeeker = null;
            }
            if (!Hold.IsHeld)
            {
                if (OnGround())
                {
                    float target = (!OnGround(Position + Vector2.UnitX * 3f)) ? 20f : (OnGround(Position - Vector2.UnitX * 3f) ? 0f : (-20f));
                    Speed.X = Calc.Approach(Speed.X, target, 800f * Engine.DeltaTime);
                    Vector2 liftSpeed = LiftSpeed;
                    if (liftSpeed == Vector2.Zero && prevLiftSpeed != Vector2.Zero)
                    {
                        Speed = prevLiftSpeed;
                        prevLiftSpeed = Vector2.Zero;
                        Speed.Y = Math.Min(Speed.Y * 0.6f, 0f);
                        if (Speed.X != 0f && Speed.Y == 0f)
                        {
                            Speed.Y = -60f;
                        }
                        if (Speed.Y < 0f)
                        {
                            noGravityTimer = 0.15f;
                        }
                    }
                    else
                    {
                        prevLiftSpeed = liftSpeed;
                        if (liftSpeed.Y < 0f && Speed.Y < 0f)
                        {
                            Speed.Y = 0f;
                        }
                    }
                }
                else
                {
                    float num = 800f;
                    if (Math.Abs(Speed.Y) <= 30f)
                    {
                        num *= 0.5f;
                    }
                    float num2 = 350f;
                    if (Speed.Y < 0f)
                    {
                        num2 *= 0.5f;
                    }
                    Speed.X = Calc.Approach(Speed.X, 0f, num2 * Engine.DeltaTime);
                    if (noGravityTimer > 0f)
                    {
                        noGravityTimer -= Engine.DeltaTime;
                    }
                    else
                    {
                        Speed.Y = Calc.Approach(Speed.Y, 200f, num * Engine.DeltaTime);
                    }
                }
                previousPosition = ExactPosition;
                MoveH(Speed.X * Engine.DeltaTime, onCollideH);
                MoveV(Speed.Y * Engine.DeltaTime, onCollideV);
                if (Left > SceneAs<Level>().Bounds.Right || Right < SceneAs<Level>().Bounds.Left || Top > SceneAs<Level>().Bounds.Bottom)
                {
                    RemoveSelf();
                }
                Hold.CheckAgainstColliders();
            }
            foreach (CrateCollider crateCollider in Scene.Tracker.GetComponents<CrateCollider>())
            {
                crateCollider.Check(this);
            }
            SourceSpawner.Collidable = true;
            Slope.SetCollisionAfterUpdate(this);
        }

        public override void Render()
        {
            base.Render();
            if (!Destroyed)
            {
                Texture.Draw(Position - new Vector2(8f, 16f));
            }
        }

        public override void Removed(Scene scene)
        {
            base.Removed(scene);
            LaserBlock.RemoveSelf();
        }
    }
}
