//extern alias Viv;
using System;
using Celeste.Mod.Entities;
using Celeste.Mod.XaphanHelper.Colliders;
using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste.Mod.XaphanHelper.Entities
{

    [Tracked(false)]
    [CustomEntity("XaphanHelper/Cell")]
    public class Cell : Actor
    {
        public static ParticleType P_Impact;

        public string flag;

        public string sprite;

        public Vector2 Speed;

        private bool tutorial;

        public Holdable Hold;

        private Sprite cellSprite;

        private bool dead;

        private Level Level;

        private Collision onCollideH;

        private Collision onCollideV;

        public float noGravityTimer;

        private Vector2 prevLiftSpeed;

        private Vector2 previousPosition;

        private HoldableCollider hitSeeker;

        private float swatTimer;

        private float hardVerticalHitSoundCooldown = 0f;

        private BirdTutorialGui tutorialGui;

        private float tutorialTimer = 0f;

        private Level level => (Level)Scene;

        private bool DropWhenDreamDash;

        private float bounceMultiplier;

        private float throwForceMultiplier;

        private float throwUpForceMultiplier;

        private float gravityMultiplier;

        private float frictionMultiplier;

        private float postThrowNoGravityTimer;

        private string deathColor;

        private bool killPlayerOnDeath;

        private string hitSidesSound;

        private string hitGroundSound;

        private string deathSound;

        public Cell(EntityData data, Vector2 offset) : base(data.Position + offset)
        {
            tutorial = data.Bool("tutorial");
            flag = data.Attr("flag");
            sprite = data.Attr("sprite");
            bounceMultiplier = data.Float("bounceMultiplier", 0.4f);
            throwForceMultiplier = data.Float("throwForceMultiplier", 1);
            throwUpForceMultiplier = data.Float("throwUpForceMultiplier", 0.4f);
            gravityMultiplier = data.Float("gravityMultiplier", 1f);
            frictionMultiplier = data.Float("frictionMultiplier", 1f);
            postThrowNoGravityTimer = data.Float("postThrowNoGravityTimer", 0.1f);
            DropWhenDreamDash = data.Bool("dropWhenDreamDash", false);
            deathColor = data.Attr("deathColor", "0088E8");
            killPlayerOnDeath = data.Bool("killPlayerOnDeath", true);
            hitSidesSound = data.Attr("hitSidesSound", "event:/game/05_mirror_temple/crystaltheo_hit_side");
            hitGroundSound = data.Attr("hitGroundSound", "event:/game/05_mirror_temple/crystaltheo_hit_ground");
            deathSound = data.Attr("deathSound", "event:/char/madeline/death");
            previousPosition = Position;
            Depth = 100;
            Collider = new Hitbox(8f, 10f, -4f, -10f);
            Add(cellSprite = new Sprite(GFX.Game, sprite + "/"));
            cellSprite.AddLoop("cellSprite", "cell", 0.08f);
            cellSprite.CenterOrigin();
            cellSprite.Justify = new Vector2(0.5f, 1f);
            cellSprite.Play("cellSprite");
            cellSprite.Scale.X = -1f;
            Add(Hold = new Holdable(0.1f));
            Hold.PickupCollider = new Hitbox(16f, 16f, -8f, -16f);
            Hold.SlowFall = false;
            Hold.SlowRun = true;
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
            LiftSpeedGraceTime = 0.1f;
            if (data.Bool("emitLight", true))
            {
                Add(new VertexLight(Collider.Center, Calc.HexToColor(data.Attr("lightColor", "FFFFFF")), 1f, 32, 64));
            }
            Add(new MirrorReflection());
        }

        public override void Added(Scene scene)
        {
            base.Added(scene);
            Level = SceneAs<Level>();
            string Prefix = Level.Session.Area.LevelSet;
            int chapterIndex = Level.Session.Area.ChapterIndex;
            if (!string.IsNullOrEmpty(flag))
            {
                if (Level.Session.GetFlag(flag) || Level.Session.GetFlag(flag + "_sloted") || XaphanModule.ModSaveData.SavedFlags.Contains(Prefix + "_Ch" + chapterIndex + "_" + flag + (XaphanModule.PlayerHasGolden ? "_GoldenStrawberry" : "")))
                {
                    RemoveSelf();
                }
            }
            if (tutorial)
            {
                tutorialGui = new BirdTutorialGui(this, new Vector2(0f, -24f), Dialog.Clean("tutorial_carry"), Dialog.Clean("tutorial_hold"), Input.Grab);
                tutorialGui.Open = false;
                Scene.Add(tutorialGui);
            }
        }

        public override void Update()
        {
            Slope.SetCollisionBeforeUpdate(this);
            base.Update();
            string Prefix = Level.Session.Area.LevelSet;
            int chapterIndex = Level.Session.Area.ChapterIndex;
            if (!string.IsNullOrEmpty(flag))
            {
                if (Level.Session.GetFlag(flag) || Level.Session.GetFlag(flag + "_sloted") || XaphanModule.ModSaveData.SavedFlags.Contains(Prefix + "_Ch" + chapterIndex + "_" + flag + (XaphanModule.PlayerHasGolden ? "_GoldenStrawberry" : "")))
                {
                    RemoveSelf();
                }
            }
            if (dead)
            {
                return;
            }
            if (swatTimer > 0f)
            {
                swatTimer -= Engine.DeltaTime;
            }
            hardVerticalHitSoundCooldown -= Engine.DeltaTime;
            Depth = 100;
            if (Hold.IsHeld)
            {
                prevLiftSpeed = Vector2.Zero;
                if (DropWhenDreamDash && Hold.Holder.StateMachine.State == Player.StDreamDash)
                {
                    Hold.Holder.Throw();
                }
            }
            else
            {
                foreach (Slope slope in SceneAs<Level>().Tracker.GetEntities<Slope>())
                {
                    if (slope.UpsideDown && CollideCheck(slope))
                    {
                        Position.Y += 1;
                    }
                }
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
                else if (Hold.ShouldHaveGravity)
                {
                    float num = 800f * gravityMultiplier;
                    if (Math.Abs(Speed.Y) <= 30f)
                    {
                        num *= 0.5f;
                    }
                    float num2 = 350f * frictionMultiplier;
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
                        Speed.Y = Calc.Approach(Speed.Y, 200f * gravityMultiplier, num * Engine.DeltaTime);
                    }
                }
                previousPosition = ExactPosition;
                MoveH(Speed.X * Engine.DeltaTime, onCollideH);
                MoveV(Speed.Y * Engine.DeltaTime, onCollideV);
                if (Center.X > Level.Bounds.Right)
                {
                    MoveH(32f * Engine.DeltaTime);
                    if (Left - 8f > Level.Bounds.Right)
                    {
                        RemoveSelf();
                    }
                }
                else if (Center.X < Level.Bounds.Left)
                {
                    MoveH(-32f * Engine.DeltaTime);
                    if (Right + 8f < Level.Bounds.Left)
                    {
                        RemoveSelf();
                    }
                }
                else if (Top < Level.Bounds.Top - 4)
                {
                    Top = Level.Bounds.Top + 4;
                    Speed.Y = 0f;
                }
                else if (Bottom > Level.Bounds.Bottom && SaveData.Instance.Assists.Invincible)
                {
                    Bottom = Level.Bounds.Bottom;
                    Speed.Y = -300f;
                    Audio.Play("event:/game/general/assist_screenbottom", Position);
                }
                else if (Top > Level.Bounds.Bottom)
                {
                    Die();
                }
                Player entity = Scene.Tracker.GetEntity<Player>();
                TempleGate templeGate = CollideFirst<TempleGate>();
                if (templeGate != null && entity != null)
                {
                    templeGate.Collidable = false;
                    MoveH(Math.Sign(entity.X - X) * 32 * Engine.DeltaTime);
                    templeGate.Collidable = true;
                }
            }
            if (!dead)
            {
                foreach (CellCollider cellCollider in Scene.Tracker.GetComponents<CellCollider>())
                {
                    cellCollider.Check(this);
                }
                Hold.CheckAgainstColliders();
            }
            if (hitSeeker != null && swatTimer <= 0f && !hitSeeker.Check(Hold))
            {
                hitSeeker = null;
            }
            if (tutorialGui != null)
            {
                Player player = Scene.Tracker.GetEntity<Player>();
                if (!Hold.IsHeld && OnGround() && player != null && player.StateMachine.State != 11)
                {
                    tutorialTimer += Engine.DeltaTime;
                }
                else
                {
                    tutorialTimer = 0f;
                }
                tutorialGui.Open = (tutorial && tutorialTimer > 0.25f);
            }
            Slope.SetCollisionAfterUpdate(this);
        }

        public void ExplodeLaunch(Vector2 from)
        {
            if (!Hold.IsHeld)
            {
                Speed = (Center - from).SafeNormalize(120f);
                SlashFx.Burst(Center, Speed.Angle());
            }
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
            Audio.Play(hitSidesSound, Position);
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
            /*if (!OnGround() && !Hold.IsHeld)
            {
                if (spinner.GetType() == typeof(CrystalStaticSpinner))
                {
                    CrystalStaticSpinner spin = (CrystalStaticSpinner)spinner;
                    spin.Destroy();
                    return;
                }
                else if (spinner.GetType() == typeof(CustomSpinner))
                {
                    CustomSpinner spin = (CustomSpinner)spinner;
                    spin.Destroy();
                    return;
                }
                if(XaphanModule.hasFrostHelper)
                {
                    if (spinner.GetType() == typeof(FrostHelper.CustomSpinner))
                    {
                        FrostHelper.CustomSpinner spin = (FrostHelper.CustomSpinner)spinner;
                        spin.Destroy();
                        return;
                    }
                }
                else if (XaphanModule.hasVivHelper && spinner.GetType() == typeof(Viv.VivHelper.Entities.CustomSpinner))
                {
                    Viv.VivHelper.Entities.CustomSpinner spin = (Viv.VivHelper.Entities.CustomSpinner)spinner;
                    spin.Destroy();
                }
            }*/
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
            if (data.Hit is DashSwitch)
            {
                (data.Hit as DashSwitch).OnDashCollide(null, Vector2.UnitX * Math.Sign(Speed.X));
            }
            if (data.Hit is FlagDashSwitch)
            {
                (data.Hit as FlagDashSwitch).OnDashCollide(null, Vector2.UnitX * Math.Sign(Speed.X));
            }
            if (data.Hit is TimedDashSwitch)
            {
                (data.Hit as TimedDashSwitch).OnDashCollide(null, Vector2.UnitX * Math.Sign(Speed.X));
            }
            Audio.Play(hitSidesSound, Position);
            if (Math.Abs(Speed.X) > 100f)
            {
                ImpactParticles(data.Direction);
            }
            Speed.X *= -bounceMultiplier;
        }

        private void OnCollideV(CollisionData data)
        {
            if (data.Hit is DashSwitch)
            {
                (data.Hit as DashSwitch).OnDashCollide(null, Vector2.UnitY * Math.Sign(Speed.Y));
            }
            if (data.Hit is FlagDashSwitch)
            {
                (data.Hit as FlagDashSwitch).OnDashCollide(null, Vector2.UnitY * Math.Sign(Speed.Y));
            }
            if (data.Hit is TimedDashSwitch)
            {
                (data.Hit as TimedDashSwitch).OnDashCollide(null, Vector2.UnitY * Math.Sign(Speed.Y));
            }
            if (Speed.Y > 0f)
            {
                if (hardVerticalHitSoundCooldown <= 0f)
                {
                    Audio.Play(hitGroundSound, Position, "crystal_velocity", Calc.ClampedMap(Speed.Y, 0f, 200f));
                    hardVerticalHitSoundCooldown = 0.5f;
                }
                else
                {
                    Audio.Play(hitGroundSound, Position, "crystal_velocity", 0f);
                }
            }
            if (Speed.Y > 160f)
            {
                ImpactParticles(data.Direction);
            }
            if (Speed.Y > 140f && !(data.Hit is SwapBlock) && !(data.Hit is DashSwitch) && !(data.Hit is FlagDashSwitch) && !(data.Hit is TimedDashSwitch))
            {
                Speed.Y *= -bounceMultiplier;
            }
            else
            {
                Speed.Y = 0f;
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
            Level.Particles.Emit(TheoCrystal.P_Impact, 12, position, positionRange, direction);
        }

        public override bool IsRiding(Solid solid)
        {
            return Speed.Y == 0f && base.IsRiding(solid);
        }

        protected override void OnSquish(CollisionData data)
        {
            if (!TrySquishWiggle(data) && !SaveData.Instance.Assists.Invincible)
            {
                Die();
            }
        }

        private void OnPickup()
        {
            Speed = Vector2.Zero;
            AddTag(Tags.Persistent);
            tutorial = false;
            AllowPushing = false;
        }

        private void OnRelease(Vector2 force)
        {
            RemoveTag(Tags.Persistent);
            if (force.X != 0f && force.Y == 0f)
            {
                force.Y = -throwUpForceMultiplier;
            }
            Speed = force * 200f * throwForceMultiplier;
            if (Speed != Vector2.Zero)
            {
                noGravityTimer = postThrowNoGravityTimer;
            }
            AllowPushing = true;
        }

        public void Die()
        {
            if (!dead)
            {
                dead = true;
                if (killPlayerOnDeath)
                {
                    Player entity = Level.Tracker.GetEntity<Player>();
                    entity?.Die(-Vector2.UnitX * (float)entity.Facing);
                    Audio.Play(deathSound, Position);
                }
                Add(new DeathEffect(Calc.HexToColor("0088E8"), Center - Position));
                cellSprite.Visible = false;
                Depth = -1000000;
                AllowPushing = false;
            }
        }
    }
}