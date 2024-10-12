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
    public class Beam : Actor
    {
        private Player Player;

        private Sprite beamSprite;

        public ParticleType P_Collide;

        public ParticleType P_Expire;

        public ParticleType P_Ice;

        public float playerSpeedY;

        public string beamType;

        public float colliderWidth;

        public float colliderHeight;

        public string beamSpritePath;

        public string particlesColor;

        public string beamSound;

        public Vector2 ShootOffset;

        public Vector2 Direction;

        public Vector2 startPosition;

        public int wideOffset;

        public bool Silent;

        public bool waving;

        public int damage;

        public Vector2 Speed;

        public Beam(Player player, string beamType, string beamSound, Vector2 position, int wideOffset = 0, bool silent = false) : base(position)
        {
            Player = player;
            playerSpeedY = Player.Speed.Y < 0 ? Player.Speed.Y : 0;
            this.beamType = beamType;
            this.beamSound = beamSound;
            this.wideOffset = wideOffset;
            Silent = silent;
            SetBeam(beamType);
            SetDamage(beamType);
            Position += ShootOffset;
            if (Direction.Y == 0)
            {
                Collider = new Hitbox(colliderWidth, colliderHeight);
            }
            else
            {
                Collider = new Hitbox(colliderHeight, colliderWidth);
            }
            Add(beamSprite = new Sprite(GFX.Game, beamSpritePath));
            if (!beamType.Contains("Power") && beamType.Contains("WaveIce"))
            {
                if (beamType.Contains("Spazer"))
                {
                    beamType = "SpazerIce";
                }
                else
                {
                    beamType = "PlasmaIce";
                }
            }
            beamSprite.AddLoop("beam", beamType.Substring(0, 1).ToLower() + beamType.Substring(1), 0.05f);
            if (Direction.X == -1)
            {
                beamSprite.FlipX = true;
            }
            else if (Direction.Y == -1)
            {
                beamSprite.Rotation = (float)Math.PI / 2 * 3;
                beamSprite.Position.Y += colliderWidth;
            }
            else if (Direction.Y == 1)
            {
                beamSprite.Rotation = (float)Math.PI / 2;
                beamSprite.Position.X += colliderHeight;
            }
            if (beamType == "PowerWave")
            {
                beamSprite.Position.Y -= 1;
            }
            beamSprite.Play("beam");
            P_Collide = new ParticleType
            {
                Color = Calc.HexToColor(particlesColor),
                FadeMode = ParticleType.FadeModes.Linear,
                Size = 1f,
                SizeRange = 0f,
                SpeedMin = 20f,
                SpeedMax = 40f,
                Direction = (float)Math.PI / 2f,
                DirectionRange = 0.05f,
                Acceleration = Vector2.UnitY * 20f,
                LifeMin = 0.2f,
                LifeMax = 0.4f
            };
            P_Expire = new ParticleType
            {
                Size = 1f,
                Color = Calc.HexToColor(particlesColor),
                DirectionRange = (float)Math.PI / 30f,
                LifeMin = 0.2f,
                LifeMax = 0.4f,
                SpeedMin = 20f,
                SpeedMax = 30f,
                SpeedMultiplier = 0.25f,
                FadeMode = ParticleType.FadeModes.Late
            };
            P_Ice = new ParticleType
            {
                Color = Calc.HexToColor("327DDC"),
                Color2 = Color.White,
                ColorMode = ParticleType.ColorModes.Blink,
                FadeMode = ParticleType.FadeModes.Late,
                LifeMin = 0.25f,
                LifeMax = 0.375f,
                Size = 1f,
                SpeedMin = 2f,
                SpeedMax = 8f,
                Acceleration = new Vector2(0f, 320f),
                Direction = (float)Math.PI / 2f,
                DirectionRange = (float)Math.PI * 2f
            };
            Depth = 100;
        }

        public override void Added(Scene scene)
        {
            base.Added(scene);
            startPosition = Position;
            if (!Silent)
            {
                Audio.Play(beamSound);
            }
            Add(new Coroutine(Lifetime(Direction)));
        }

        public void SetBeam(string beamType)
        {
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
            beamSpritePath = "upgrades/Beams/";
            if (!XaphanModule.useMetroidGameplay)
            {
                colliderWidth = 5f;
                colliderHeight = 3f;
                if (beamType.Contains("Wave"))
                {
                    colliderWidth = 4f;
                    colliderHeight = 4f;
                }
                particlesColor = "D0A500";
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
                        ShootOffset.X = -1f;
                        if (Player.Speed.X != 0 && !Player.CollideCheck<Solid>(Player.Position + new Vector2(1, 0)))
                        {
                            ShootOffset.X = 2f;
                        }
                    }
                    ShootOffset.Y = -10f;
                }
            }
            else
            {
                if (beamType.Contains("Power"))
                {
                    colliderWidth = 5f;
                    colliderHeight = 3f;
                    if (beamType.Contains("Wave"))
                    {
                        colliderWidth = 4f;
                        colliderHeight = 4f;
                    }
                    particlesColor = "D0A500";
                    if (Direction.X != 0)
                    {
                        ShootOffset.Y = -8f;
                        if (Player.Facing == Facings.Left)
                        {
                            ShootOffset.X = 0f;
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
                            ShootOffset.X = 1f;
                        }
                        else
                        {
                            ShootOffset.X = -4f;
                        }
                        ShootOffset.Y = Input.MoveY == -1 ? -13f : -8f;
                    }
                }
                else if (beamType.Contains("Spazer"))
                {
                    colliderWidth = 8f;
                    colliderHeight = 1f;
                    particlesColor = "F8F8D0";
                }
                else if (beamType.Contains("Plasma"))
                {
                    colliderWidth = 16f;
                    colliderHeight = 1f;
                    particlesColor = "00F870";
                    if (Direction.X != 0)
                    {
                        ShootOffset.Y = -7f;
                        if (Player.Facing == Facings.Left)
                        {
                            ShootOffset.X = -11f;
                        }
                    }
                    if (Direction.Y != 0)
                    {
                        ShootOffset.Y = Input.MoveY == -1 ? -24f : -8f;
                    }
                }
            }
            if (beamType.Contains("Wave"))
            {
                particlesColor = "BD00FF";
            }
            if (beamType.Contains("Ice"))
            {
                particlesColor = "327DDC";
            }
        }

        public void SetDamage(string beamType)
        {
            if (beamType == "Power")
            {
                damage = 20;
            }
            else if (beamType == "PowerWave")
            {
                damage = 50;
            }
            else if (beamType == "PowerIce")
            {
                damage = 30;
            }
            else if (beamType == "PowerWaveIce")
            {
                damage = 60;
            }
            else if (beamType == "Spazer")
            {
                damage = 40;
            }
            else if (beamType == "SpazerWave")
            {
                damage = 70;
            }
            else if (beamType == "SpazerIce")
            {
                damage = 60;
            }
            else if (beamType == "SpazerWaveIce")
            {
                damage = 100;
            }
            else if (beamType == "Plasma")
            {
                damage = 150;
            }
            else if (beamType == "PlasmaWave")
            {
                damage = 250;
            }
            else if (beamType == "PlasmaIce")
            {
                damage = 200;
            }
            else if (beamType == "PlasmaWaveIce")
            {
                damage = 300;
            }
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
            foreach (WeaponCollider weaponCollider in Scene.Tracker.GetComponents<WeaponCollider>())
            {
                weaponCollider.Check(this);
            }
            List<Entity> slopes = SceneAs<Level>().Tracker.GetEntities<Slope>().ToList();
            List<Entity> playerPlatforms = SceneAs<Level>().Tracker.GetEntities<PlayerPlatform>().ToList();
            slopes.ForEach(entity => entity.Collidable = true);
            playerPlatforms.ForEach(entity => entity.Collidable = false);
            base.Update();
            float beamSpeed = (beamType == "PowerWave" || beamType == "PowerWaveIce") ? 200f : 300f;
            if (Direction.X > 0)
            {
                Speed.X = beamSpeed + Player.Speed.X;
            }
            else if (Direction.X < 0)
            {
                Speed.X = -(beamSpeed - Player.Speed.X);
            }
            else if (Direction.Y > 0)
            {
                Speed.Y = beamSpeed;
            }
            else if (Direction.Y < 0)
            {
                Speed.Y = -beamSpeed ;
            }
            if (wideOffset != 0 && !waving)
            {
                waving = true;
                float closeOffset = (beamType.Contains("Plasma") && beamType.Contains("Wave") && !Spazer.Active(SceneAs<Level>())) ? 1f : 2f;
                float coef = Math.Abs(wideOffset);
                if (beamType.Contains("Wave"))
                {
                    if ((beamType != "PowerWave" && beamType != "PowerWaveIce"))
                    {
                        if (Direction.X != 0)
                        {
                            Vector2 start = startPosition + new Vector2(0, wideOffset > 0 ? closeOffset : -closeOffset);
                            Vector2 end = startPosition + new Vector2(0, wideOffset);
                            float num = Vector2.Distance(start, end) / (!LongBeam.Active(SceneAs<Level>()) ? 10 : 4) / coef;
                            Tween tween = Tween.Create(Tween.TweenMode.YoyoLooping, Ease.SineInOut, num, start: true);
                            tween.OnUpdate = delegate (Tween t)
                            {
                                Position = Vector2.Lerp(new Vector2(Position.X, start.Y), new Vector2(Position.X, end.Y), t.Eased);
                            };
                            Add(tween);
                        }
                        else if (Direction.Y != 0)
                        {
                            Vector2 start = startPosition + new Vector2(wideOffset > 0 ? closeOffset : -closeOffset, 0);
                            Vector2 end = startPosition + new Vector2(wideOffset, 0);
                            float num = Vector2.Distance(start, end) / (!LongBeam.Active(SceneAs<Level>()) ? 10 : 4) / coef;
                            Tween tween = Tween.Create(Tween.TweenMode.YoyoLooping, Ease.SineInOut, num, start: true);
                            tween.OnUpdate = delegate (Tween t)
                            {
                                Position = Vector2.Lerp(new Vector2(start.X, Position.Y), new Vector2(end.X, Position.Y), t.Eased);
                            };
                            Add(tween);
                        }
                    }
                    else
                    {
                        if (Direction.X != 0)
                        {
                            Vector2 start = startPosition + new Vector2(0, wideOffset);
                            Vector2 end = startPosition + new Vector2(0, -wideOffset);
                            float num = Vector2.Distance(start, end) / 125;
                            Tween tween = Tween.Create(Tween.TweenMode.YoyoLooping, Ease.SineInOut, num, start: true);
                            tween.OnUpdate = delegate (Tween t)
                            {
                                Position = Vector2.Lerp(new Vector2(Position.X, start.Y), new Vector2(Position.X, end.Y), t.Eased);
                            };
                            Add(tween);
                        }
                        else if (Direction.Y != 0)
                        {
                            Vector2 start = startPosition + new Vector2(wideOffset, 0);
                            Vector2 end = startPosition + new Vector2(-wideOffset, 0);
                            float num = Vector2.Distance(start, end) / 125;
                            Tween tween = Tween.Create(Tween.TweenMode.YoyoLooping, Ease.SineInOut, num, start: true);
                            tween.OnUpdate = delegate (Tween t)
                            {
                                Position = Vector2.Lerp(new Vector2(start.X, Position.Y), new Vector2(end.X, Position.Y), t.Eased);
                            };
                            Add(tween);
                        }
                    }
                }
                else
                {
                    if (Direction.X != 0)
                    {
                        Vector2 start = startPosition + new Vector2(0, wideOffset > 0 ? closeOffset : -closeOffset);
                        Vector2 end = startPosition + new Vector2(0, wideOffset);
                        float num = Vector2.Distance(start, end) / 10 / coef;
                        Tween tween = Tween.Create(Tween.TweenMode.Oneshot, Ease.SineInOut, num, start: true);
                        tween.OnUpdate = delegate (Tween t)
                        {
                            Position = Vector2.Lerp(new Vector2(Position.X, start.Y), new Vector2(Position.X, end.Y), t.Eased);
                        };
                        Add(tween);
                    }
                    else if (Direction.Y != 0)
                    {
                        Vector2 start = startPosition + new Vector2(wideOffset > 0 ? closeOffset : -closeOffset, 0);
                        Vector2 end = startPosition + new Vector2(wideOffset, 0);
                        float num = Vector2.Distance(start, end) / 10 / coef;
                        Tween tween = Tween.Create(Tween.TweenMode.Oneshot, Ease.SineInOut, num, start: true);
                        tween.OnUpdate = delegate (Tween t)
                        {
                            Position = Vector2.Lerp(new Vector2(start.X, Position.Y), new Vector2(end.X, Position.Y), t.Eased);
                        };
                        Add(tween);
                    }
                }
            }
            if (beamType.Contains("Wave") && CollideCheck<DroneSwitch>())
            {
                CollideDroneSwitch(Direction);
            }
            if (Left > SceneAs<Level>().Bounds.Right || Right < SceneAs<Level>().Bounds.Left || Top > SceneAs<Level>().Bounds.Bottom || Bottom < SceneAs<Level>().Bounds.Top)
            {
                RemoveSelf();
            }
            if (beamType.Contains("Ice") && SceneAs<Level>().OnRawInterval(0.06f))
            {
                SceneAs<Level>().Particles.Emit(P_Ice, Direction.Y == 0 ? (Direction.X < 0 ? CenterRight : CenterLeft) : (Direction.Y == -1 ? BottomCenter : TopCenter) + Vector2.One);
            }
            Collider MainCollider = Collider;
            if (beamType.Contains("Wave"))
            {
                Collider = null;
            }
            MoveH(Speed.X * Engine.DeltaTime, onCollideSolid);
            MoveV(Speed.Y * Engine.DeltaTime, onCollideSolid);
            if (beamType.Contains("Wave"))
            {
                Collider = MainCollider;
            }
            slopes.ForEach(entity => entity.Collidable = false);
            playerPlatforms.ForEach(entity => entity.Collidable = true);
        }

        private void onCollideSolid(CollisionData data)
        {
            CollideSolid(data.Direction);
        }

        public void CollideSolid(Vector2 dir)
        {
            if (!beamType.Contains("Wave"))
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
                ImpactParticles(dir);
            }
            foreach (BreakBlockIndicator breakBlockIndicator in Scene.Tracker.GetEntities<BreakBlockIndicator>())
            {
                if (breakBlockIndicator.mode == "Drone")
                {
                    if (CollideCheck(breakBlockIndicator, Position + dir))
                    {
                        breakBlockIndicator.BreakSequence();
                    }
                }
                else
                {
                    if (CollideCheck(breakBlockIndicator, Position + dir))
                    {
                        breakBlockIndicator.RevealSequence();
                    }
                }
            }
            foreach (DroneSwitch droneSwitch in Scene.Tracker.GetEntities<DroneSwitch>())
            {
                if (CollideCheck(droneSwitch, Position + dir) && droneSwitch.type == "Beam")
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
                        playerBlocker.HitByBeam(this);
                    }
                }
            }
            if (XaphanModule.useMetroidGameplay)
            {
                foreach (BubbleDoor bubbleDoor in Scene.Tracker.GetEntities<BubbleDoor>())
                {
                    if (bubbleDoor.color == "Blue" || bubbleDoor.color == "Grey" && bubbleDoor.isActive && !bubbleDoor.locked)
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
                    if (destructibleBlock.mode == "Shoot")
                    {
                        if (CollideCheck(destructibleBlock, Position + dir))
                        {
                            destructibleBlock.Break();
                        }
                    }
                    else
                    {
                        if (CollideCheck(destructibleBlock, Position + dir))
                        {
                            destructibleBlock.Reveal();
                        }
                    }
                }
            }
            if (!beamType.Contains("Wave"))
            {
                RemoveSelf();
            }
        }

        public void CollideImmune(Vector2 dir)
        {
            Collidable = false;
            Audio.Play("event:/game/xaphan/impact_immune", Position);
            CollideSolid(dir);
        }

        public void CollideDroneSwitch(Vector2 dir)
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
            foreach (Entity entity in Scene.Tracker.GetEntities<DroneSwitch>())
            {
                DroneSwitch droneSwitch = (DroneSwitch)entity;
                if (CollideCheck(droneSwitch) && droneSwitch.type == "Beam")
                {
                    droneSwitch.Triggered(Direction);
                }
            }
        }

        public void ImpactParticles(Vector2 dir)
        {
            float direction;
            Vector2 positionRange;
            if (dir.X > 0f)
            {
                direction = (float)Math.PI;
                positionRange = Vector2.UnitY * 3f;
            }
            else if (dir.X < 0f)
            {
                direction = 0f;
                positionRange = Vector2.UnitY * 3f;
            }
            else if (dir.Y > 0f)
            {
                direction = -(float)Math.PI / 2f;
                positionRange = Vector2.UnitX * 3f;
            }
            else
            {
                direction = (float)Math.PI / 2f;
                positionRange = Vector2.UnitX * 3f;
            }
            SceneAs<Level>().Particles.Emit(P_Collide, 12, dir.Y == 0 ? (dir.X < 0 ? CenterLeft : CenterRight) : (dir.Y == -1 ? TopCenter : BottomCenter), positionRange, direction);
        }

        private IEnumerator Lifetime(Vector2 direction)
        {
            float time = (LongBeam.Active(SceneAs<Level>()) ? (XaphanModule.useMetroidGameplay ? 0.8f : 0.5f) : 0.2f) * ((beamType == "PowerWave" || beamType == "PowerWaveIce") ? 1.5f : 1);
            while (time > 0)
            {
                time -= Engine.DeltaTime;
                yield return null;
            }
            ParticleSystem particlesBG = SceneAs<Level>().ParticlesBG;
            for (int i = 0; i < 360; i += 30)
            {
                particlesBG.Emit(P_Expire, 1, direction.Y == 0 ? (direction.X < 0 ? CenterLeft : CenterRight) : (direction.Y == -1 ? TopCenter : BottomCenter) + Vector2.One, Vector2.One * 2, i * ((float)Math.PI / 180f));
            }
            RemoveSelf();
        }
    }
}
