using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Celeste.Mod.XaphanHelper.Colliders;
using Celeste.Mod.XaphanHelper.Upgrades;
using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste.Mod.XaphanHelper.Entities
{
    [Tracked(true)]
    public class Missile : Actor
    {
        private Player Player;

        public bool SuperMissile;

        private Sprite missileSprite;

        public ParticleType P_Trail;

        public ParticleType P_Trail_B;

        public float playerSpeedY;

        public float colliderWidth;

        public float colliderHeight;

        public string missileSpritePath;

        public string particlesColor;

        public Vector2 ShootOffset;

        public Vector2 Direction;

        public Vector2 startPosition;

        public int damage;

        public Vector2 Speed;

        private float Rotation;

        public Missile(Player player, Vector2 position, bool superMissile, bool fromDrone = false) : base(position)
        {
            Player = player;
            SuperMissile = superMissile;
            playerSpeedY = Player.Speed.Y < 0 ? Player.Speed.Y : 0;
            ShootOffset = new Vector2(0f, -6f);
            Direction = Vector2.UnitX;
            if (Input.MoveY == 0 || HoverJet.Floating || (Input.MoveY == -1 && Input.MoveX != 0 && XaphanModule.useMetroidGameplay) || (Input.MoveY == 1 && (XaphanModule.useMetroidGameplay ? Player.OnGround() : true)))
            {
                if (Player.Facing == Facings.Left)
                {
                    ShootOffset.X = -5f;
                    Direction.X *= -1;
                }
                else
                {
                    ShootOffset.X = 0f;
                }
            }
            else
            {
                Direction = Vector2.UnitY;
                if (Input.MoveY != 1)
                {
                    Direction.Y *= -1;
                    ShootOffset.Y = -10f;
                }
                if (Player.Facing == Facings.Left)
                {
                    ShootOffset.X = -3f;
                }
                else
                {
                    ShootOffset.X = 0f;
                }
            }
            if (Direction.X != 0)
            {
                ShootOffset.Y = -7f;
                if (Player.Facing == Facings.Left)
                {
                    ShootOffset.X = -3f;
                }
                else
                {
                    ShootOffset.X = -5f;
                }
            }
            if (Direction.Y != 0)
            {
                if (Player.Facing == Facings.Left)
                {
                    ShootOffset.X = 2f;
                }
                else
                {
                    ShootOffset.X = -3f;
                }
                ShootOffset.Y = Input.MoveY == -1 ? -16f : -8f;
            }
            if (!XaphanModule.useMetroidGameplay)
            {
                colliderWidth = 5f;
                colliderHeight = 3f;
                if (Direction.X != 0)
                {
                    ShootOffset.Y = -6f;
                    if (Player.Facing == Facings.Left)
                    {
                        ShootOffset.X = -5f;
                    }
                    else
                    {
                        ShootOffset.X = 0f;
                    }
                }
                if (Direction.Y != 0)
                {
                    if (Player.Facing == Facings.Left)
                    {
                        ShootOffset.X = -2f;
                        if (Player.Speed.X != 0 && !Player.CollideCheck<Solid>(Player.Position + new Vector2(-1, 0)))
                        {
                            ShootOffset.X = -5f;
                        }
                    }
                    else
                    {
                        ShootOffset.X = -1f - (SuperMissile ? 2 : 0);
                        if (Player.Speed.X != 0 && !Player.CollideCheck<Solid>(Player.Position + new Vector2(1, 0)))
                        {
                            ShootOffset.X = 2f;
                        }
                    }
                    ShootOffset.Y = -10f;
                }
            }
            missileSpritePath = "upgrades/" + (SuperMissile ? "SuperMissile" : "Missile") + "/";
            colliderWidth = 5f;
            colliderHeight = SuperMissile ? 5f : 3f;
            particlesColor = "D0A500";
            damage = SuperMissile ? 300 : 100;
            Position += ShootOffset;
            if (Direction.Y == 0)
            {
                Collider = new Hitbox(colliderWidth, colliderHeight);
            }
            else
            {
                Collider = new Hitbox(colliderHeight, colliderWidth);
            }
            Add(missileSprite = new Sprite(GFX.Game, missileSpritePath));
            missileSprite.AddLoop("missile", SuperMissile ? "superMissile" : "missile", 0.05f);
            if (Direction.X == -1)
            {
                missileSprite.FlipX = true;
            }
            else if (Direction.Y == -1)
            {
                missileSprite.Rotation = (float)Math.PI / 2 * 3;
                missileSprite.Position.Y += colliderWidth;
            }
            else if (Direction.Y == 1)
            {
                missileSprite.Rotation = (float)Math.PI / 2;
                missileSprite.Position.X += colliderHeight;
            }
            if (SuperMissile)
            {
                if (Direction.X != 0)
                {
                    if (Player.Facing == Facings.Right)
                    {
                        missileSprite.Position.X -= 1;
                    }
                }
                if (Direction.Y != 0)
                {
                    if (Direction.Y == -1)
                    {
                        missileSprite.Position.Y += 1;
                    }
                    else if (Direction.Y == 1)
                    {
                        missileSprite.Position.Y -= 1;
                    }
                }
            }
            else
            {
                if (Direction.X != 0)
                {
                    if (Player.Facing == Facings.Left)
                    {
                        missileSprite.Position -= Vector2.One;
                    }
                    else
                    {
                        missileSprite.Position.Y -= 1;
                    }
                }
                if (Direction.Y != 0)
                {
                    if (Direction.Y == -1)
                    {
                        missileSprite.Position.X -= 1;
                    }
                    else if (Direction.Y == 1)
                    {
                        missileSprite.Position.X += 1;
                    }
                }
            }
            missileSprite.Play("missile");
            P_Trail = new ParticleType
            {
                Source = GFX.Game["particles/blob"],
                Color = Calc.HexToColor(SuperMissile ? "014718" : "631F00"),
                FadeMode = ParticleType.FadeModes.Linear,
                LifeMin = 0.1f,
                LifeMax = 0.2f,
                Size = 0.6f,
                SizeRange = 0.25f,
                ScaleOut = true,
                SpeedMin = 10f,
                SpeedMax = 20f,
                SpeedMultiplier = 0.01f,
            };
            P_Trail_B = new ParticleType
            {
                Source = GFX.Game["particles/blob"],
                Color = Calc.HexToColor(SuperMissile ? "0BB61A" : "C5170F"),
                FadeMode = ParticleType.FadeModes.Linear,
                LifeMin = 0.1f,
                LifeMax = 0.2f,
                Size = 0.4f,
                SizeRange = 0.25f,
                ScaleOut = true,
                SpeedMin = 10f,
                SpeedMax = 20f,
                SpeedMultiplier = 0.01f,
            };
            Depth = 100;
        }

        public override void Added(Scene scene)
        {
            base.Added(scene);
            startPosition = Position;
            Audio.Play("event:/game/xaphan/missile_fire");
        }

        public override void Update()
        {
            if (CollideCheck<Drone>())
            {
                Depth = 100;
            }
            else
            {
                Depth = -9000;
            }
            if (Collidable)
            {
                foreach (WeaponCollider weaponCollider in Scene.Tracker.GetComponents<WeaponCollider>())
                {
                    weaponCollider.Check(this);
                }
            }
            List<Entity> slopes = SceneAs<Level>().Tracker.GetEntities<Slope>().ToList();
            List<Entity> playerPlatforms = SceneAs<Level>().Tracker.GetEntities<PlayerPlatform>().ToList();
            slopes.ForEach(entity => entity.Collidable = true);
            playerPlatforms.ForEach(entity => entity.Collidable = false);
            base.Update();
            if (Collidable)
            {
                float multX = Math.Abs(Position.X - startPosition.X) / 5;
                float multY = Math.Abs(Position.Y - startPosition.Y) / 5;
                if (Direction.X > 0)
                {
                    Speed.X += 900f * Engine.DeltaTime * Math.Min(Math.Max(multX, 1f), 2f);
                }
                else if (Direction.X < 0)
                {
                    Speed.X -= 900f * Engine.DeltaTime * Math.Min(Math.Max(multX, 1f), 2f);
                }
                else if (Direction.Y > 0)
                {
                    Speed.Y += 900f * Engine.DeltaTime * Math.Min(Math.Max(multY, 1f), 2f);
                }
                else if (Direction.Y < 0)
                {
                    Speed.Y -= 900f * Engine.DeltaTime * Math.Min(Math.Max(multY, 1f), 2f);
                }
                if (Left > SceneAs<Level>().Bounds.Right || Right < SceneAs<Level>().Bounds.Left || Top > SceneAs<Level>().Bounds.Bottom || Bottom < SceneAs<Level>().Bounds.Top)
                {
                    RemoveSelf();
                }
                if (SceneAs<Level>().OnRawInterval(0.01f) && (Position.X <= startPosition.X - 25f || Position.X >= startPosition.X + 25f || Position.Y <= startPosition.Y - 25f || Position.Y >= startPosition.Y + 25f))
                {
                    Vector2 position = Direction.Y == 0 ? (Direction.X < 0 ? CenterRight + new Vector2(5f, 0f) : CenterLeft - new Vector2(5f, 0f)) : (Direction.Y == -1 ? BottomCenter + new Vector2(0f, 5f) : TopCenter - new Vector2(0f, 5f));
                    SceneAs<Level>().Particles.Emit(P_Trail, position);
                    SceneAs<Level>().Particles.Emit(P_Trail_B, position);
                }
            }
            MoveH(Speed.X * Engine.DeltaTime, onCollideSolid);
            MoveV(Speed.Y * Engine.DeltaTime, onCollideSolid);
            slopes.ForEach(entity => entity.Collidable = false);
            playerPlatforms.ForEach(entity => entity.Collidable = true);
        }

        private void onCollideSolid(CollisionData data)
        {
            CollideSolid(data.Direction);
        }

        public void CollideSolid(Vector2 dir)
        {
            if (dir.Y == 0)
            {
                if (dir.X > 0)
                {
                    while (CollideCheck<Solid>())
                    {
                        Collider.Width -= 1;
                    }
                }
                else if (dir.X < 0)
                {
                    while (CollideCheck<Solid>())
                    {
                        Collider.Left += 1;
                        Collider.Width -= 1;
                    }
                }
            }
            else
            {
                if (dir.Y < 0)
                {
                    while (CollideCheck<Solid>())
                    {
                        Collider.Top += 1;
                        Collider.Height -= 1;
                    }
                }
                else if (dir.Y > 0)
                {
                    while (CollideCheck<Solid>())
                    {
                        Collider.Height -= 1;
                    }
                }
            }
            SceneAs<Level>().Add(new Explosion(dir.Y == 0 ? (dir.X < 0 ? CenterLeft : CenterRight) : (dir.Y == -1 ? TopCenter : BottomCenter), SuperMissile ? true : false));
            if (!SuperMissile)
            {
                Audio.Play("event:/game/xaphan/missile_collide_solid");
            }
            else
            {
                SceneAs<Level>().Shake(0.3f);
                Audio.Play("event:/game/xaphan/super_missile_collide_solid");
            }
            foreach (WorkRobot workRobot in Scene.Tracker.GetEntities<WorkRobot>())
            {
                if (CollideCheck(workRobot, Position + dir))
                {
                    if (!SuperMissile)
                    {
                        workRobot.Push(new Vector2(150, -75), dir);
                    }
                    else
                    {
                        // To Do : Destroy the Robot
                    }
                }
            }
            foreach (DroneSwitch droneSwitch in Scene.Tracker.GetEntities<DroneSwitch>())
            {
                if (CollideCheck(droneSwitch, Position + dir) && (droneSwitch.type == "Beam" || droneSwitch.type == "Missile" || (SuperMissile && droneSwitch.type == "SuperMissile")))
                {
                    string Direction = null;
                    if (dir == new Vector2(-1, 0))
                    {
                        Direction = "Left";
                    }
                    else if (dir == new Vector2(1, 0))
                    {
                        Direction = "Right";
                    }
                    else if (dir == new Vector2(0, -1))
                    {
                        Direction = "Down";
                    }
                    droneSwitch.Triggered(Direction);
                }
            }
            foreach (PlayerBlocker playerBlocker in Scene.Tracker.GetEntities<PlayerBlocker>())
            {
                if (playerBlocker.Enemy != null)
                {
                    if (CollideCheck(playerBlocker, Position + dir))
                    {
                        playerBlocker.HitByMissile(this);
                    }
                }
            }
            if (XaphanModule.useMetroidGameplay)
            {
                foreach (BubbleDoor bubbleDoor in Scene.Tracker.GetEntities<BubbleDoor>())
                {
                    if (bubbleDoor.color == "Blue" || bubbleDoor.color == "Red" || (SuperMissile && bubbleDoor.color == "Green") || bubbleDoor.color == "Grey" && bubbleDoor.isActive && !bubbleDoor.locked)
                    {
                        if (CollideCheck(bubbleDoor, Position + dir))
                        {
                            bubbleDoor.keepOpen = true;
                            bubbleDoor.Open();
                        }
                    }
                }
                foreach (DestructibleBlock destructibleBlock in Scene.Tracker.GetEntities<DestructibleBlock>())
                {
                    if ((destructibleBlock.mode == "Missile" && !SuperMissile) || (destructibleBlock.mode == "SuperMissile" && SuperMissile))
                    {
                        if (CollideCheck(destructibleBlock, Position + dir))
                        {
                            destructibleBlock.Break();
                        }
                    }
                    else if (!destructibleBlock.revealed)
                    {
                        if (CollideCheck(destructibleBlock, Position + dir))
                        {
                            destructibleBlock.Reveal();
                        }
                    }
                }
            }
            foreach (BreakBlock breackBlock in Scene.Tracker.GetEntities<BreakBlock>())
            {
                if (breackBlock.type == "Missile" || (breackBlock.type == "SuperMissile" && SuperMissile))
                {
                    if (CollideCheck(breackBlock, Position + dir))
                    {
                        breackBlock.Break();
                    }
                }
            }
            RemoveSelf();
        }

        public void CollideImmune(Vector2 dir, float extraXSpeed = 0f)
        {
            Collidable = false;
            Audio.Play("event:/game/xaphan/impact_immune", Position);
            Add(new Coroutine(BounceRoutine(dir, extraXSpeed)));
        }

        private IEnumerator BounceRoutine(Vector2 dir, float extraXSpeed)
        {
            Speed = new Vector2(extraXSpeed, dir.Y == 0 ? -125f : 125f);
            Rotation = missileSprite.Rotation;
            missileSprite.CenterOrigin();
            missileSprite.Position = new Vector2(Width / 2, Height / 2);
            float RotationVelocity = 20f;
            int bounceDirection = dir.X < 0 ? 1 : -1;
            while (true)
            {
                Rotation -= MathHelper.ToRadians(RotationVelocity);
                missileSprite.Rotation = Rotation;
                Speed.Y += 8f;
                if (dir.Y == 0 && Math.Abs(Speed.X) <= 30f)
                {
                    Speed.X += 5f * bounceDirection;
                }
                yield return null;
            }
        }
    }
}
