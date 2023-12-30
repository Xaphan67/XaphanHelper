using System;
using System.Collections;
using System.Collections.Generic;
using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste.Mod.XaphanHelper.Entities
{
    [Tracked(true)]
    [CustomEntity("XaphanHelper/CustomFinalBoss")]
    public class CustomFinalBoss : Entity
    {
        public static ParticleType P_Burst;

        public bool cameraLockY;

        public const float CameraXPastMax = 140f;

        private const float MoveSpeed = 600f;

        private const float AvoidRadius = 12f;

        public string spriteName;

        public Sprite Sprite;

        public PlayerSprite NormalSprite;

        private PlayerHair normalHair;

        private Vector2 avoidPos;

        public float CameraYPastMax;

        public bool Moving;

        public bool Sitting;

        private int facing;

        private Level level;

        private Circle circle;

        public Vector2[] nodes;

        public int hits;

        private int patternIndex;

        private int previousPatternIndex;

        private Coroutine attackCoroutine;

        private Coroutine triggerBlocksCoroutine;

        private List<Entity> fallingBlocks;

        private List<Entity> movingBlocks;

        public bool playerHasMoved;

        private SineWave floatSine;

        private bool startHit;

        private VertexLight light;

        private Wiggler scaleWiggler;

        private FinalBossStarfield bossBg;

        private SoundSource chargeSfx;

        private SoundSource laserSfx;

        private bool canChangeMusic;

        public Vector2 BeamOrigin => base.Center + Sprite.Position + new Vector2(0f, -14f);

        public Vector2 ShotOrigin => base.Center + Sprite.Position + new Vector2(6f * Sprite.Scale.X, 2f);

        public BossShield bossShield;

        public Vector2 target = Vector2.Zero;

        private bool StopShoot;

        public CustomFinalBoss(EntityData data, Vector2 offset) : base(data.Position + offset)
        {
            canChangeMusic = data.Bool("canChangeMusic", defaultValue: true);
            patternIndex = data.Int("patternIndex");
            previousPatternIndex = data.Int("patternIndex");
            CameraYPastMax = data.Float("cameraYPastMax");
            startHit = data.Bool("startHit");
            cameraLockY = data.Bool("cameraLockY");
            spriteName = data.Attr("spriteName");
            Add(light = new VertexLight(Color.White, 1f, 32, 64));
            Collider = (circle = new Circle(14f, 0f, -6f));
            Add(new PlayerCollider(OnPlayer));
            nodes = new Vector2[data.Nodes.GetLength(0) + 1];
            nodes[0] = data.Position + offset;
            for (int i = 0; i < data.Nodes.GetLength(0); i++)
            {
                nodes[i + 1] = data.Nodes[i] + offset;
            }
            attackCoroutine = new Coroutine(removeOnComplete: false);
            Add(attackCoroutine);
            triggerBlocksCoroutine = new Coroutine(removeOnComplete: false);
            Add(triggerBlocksCoroutine);
            Add(new CameraLocker(cameraLockY ? Level.CameraLockModes.FinalBoss : Level.CameraLockModes.FinalBossNoY, 140f, CameraYPastMax));
            Add(floatSine = new SineWave(0.6f, 0f));
            Add(scaleWiggler = Wiggler.Create(0.6f, 3f));
            Add(chargeSfx = new SoundSource());
            Add(laserSfx = new SoundSource());
            Visible = false;
            Collidable = false;
        }

        public override void Added(Scene scene)
        {
            base.Added(scene);
            level = SceneAs<Level>();
            CreateBossSprite();
            bossBg = level.Background.Get<FinalBossStarfield>();
            if (patternIndex == 0 && !level.Session.GetFlag("boss_intro") && level.Session.Level.Equals("boss-00"))
            {
                level.Session.Audio.Music.Event = "event:/music/lvl2/phone_loop";
                level.Session.Audio.Apply(forceSixteenthNoteHack: false);
                if (bossBg != null)
                {
                    bossBg.Alpha = 0f;
                }
                Sitting = true;
                Position.Y += 16f;
                NormalSprite.Play("pretendDead");
                NormalSprite.Scale.X = 1f;
            }
            else if (patternIndex == 0 && !level.Session.GetFlag("boss_mid") && level.Session.Level.Equals("boss-14"))
            {
                level.Add(new CS06_BossMid());
            }
            else if (startHit)
            {
                Alarm.Set(this, 0.5f, delegate
                {
                    OnPlayer(null);
                });
            }
            light.Position = ((Sprite != null) ? Sprite : NormalSprite).Position + new Vector2(0f, -10f);
        }

        public override void Awake(Scene scene)
        {
            base.Awake(scene);
            fallingBlocks = base.Scene.Tracker.GetEntitiesCopy<FallingBlock>();
            fallingBlocks.Sort((Entity a, Entity b) => (int)(a.X - b.X));
            movingBlocks = base.Scene.Tracker.GetEntitiesCopy<FinalBossMovingBlock>();
            movingBlocks.Sort((Entity a, Entity b) => (int)(a.X - b.X));
        }

        private void CreateBossSprite()
        {
            Add(Sprite = XaphanModule.SpriteBank.Create("XaphanHelper_Custom_boss_" + spriteName));
            Sprite.OnFrameChange = delegate (string anim)
            {
                if (anim == "idle" && Sprite.CurrentAnimationFrame == 18)
                {
                    Audio.Play("event:/char/badeline/boss_idle_air", Position);
                }
            };
            facing = -1;
            if (NormalSprite != null)
            {
                Sprite.Position = NormalSprite.Position;
                Remove(NormalSprite);
            }
            if (normalHair != null)
            {
                Remove(normalHair);
            }
            NormalSprite = null;
            normalHair = null;
        }

        public override void Update()
        {
            base.Update();
            Sprite sprite = (Sprite != null) ? Sprite : NormalSprite;
            if (!Sitting)
            {
                Player entity = Scene.Tracker.GetEntity<Player>();
                if (!Moving && entity != null)
                {
                    if (facing == -1 && entity.X > X + 20f)
                    {
                        facing = 1;
                        scaleWiggler.Start();
                    }
                    else if (facing == 1 && entity.X < X - 20f)
                    {
                        facing = -1;
                        scaleWiggler.Start();
                    }
                }
                if (!playerHasMoved && entity != null && entity.Speed != Vector2.Zero)
                {
                    playerHasMoved = true;
                    if (patternIndex != 0)
                    {
                        StartAttacking();
                    }
                }
                if (!Moving)
                {
                    sprite.Position = avoidPos + new Vector2(floatSine.Value * 3f, floatSine.ValueOverTwo * 4f);
                }
                else
                {
                    Collidable = false;
                    sprite.Position = Calc.Approach(sprite.Position, Vector2.Zero, 12f * Engine.DeltaTime);
                }
                float radius = circle.Radius;
                circle.Radius = 6f;
                CollideFirst<DashBlock>()?.Break(Center, -Vector2.UnitY, true);
                circle.Radius = radius;
                if (!level.IsInBounds(Position, 24f))
                {
                    Active = (Visible = (Collidable = false));
                    return;
                }
                Vector2 target;
                if (!Moving && entity != null)
                {
                    float val = (base.Center - entity.Center).Length();
                    val = Calc.ClampedMap(val, 32f, 88f, 12f, 0f);
                    target = ((!(val <= 0f)) ? (base.Center - entity.Center).SafeNormalize(val) : Vector2.Zero);
                }
                else
                {
                    target = Vector2.Zero;
                }
                avoidPos = Calc.Approach(avoidPos, target, 40f * Engine.DeltaTime);
            }
            light.Position = sprite.Position + new Vector2(0f, -10f);
        }

        public override void Render()
        {
            if (Sprite != null)
            {
                Sprite.Scale.X = facing;
                Sprite.Scale.Y = 1f;
                Sprite.Scale *= 1f + scaleWiggler.Value * 0.2f;
            }
            if (NormalSprite != null)
            {
                Vector2 position = NormalSprite.Position;
                NormalSprite.Position = NormalSprite.Position.Floor();
                base.Render();
                NormalSprite.Position = position;
            }
            else
            {
                base.Render();
            }
        }

        public void OnPlayer(Player player)
        {
            if (Sprite == null)
            {
                CreateBossSprite();
            }
            Sprite.Play("getHit");
            Audio.Play("event:/char/badeline/boss_hug", Position);
            chargeSfx.Stop();
            if (laserSfx.EventName == "event:/char/badeline/boss_laser_charge" && laserSfx.Playing)
            {
                laserSfx.Stop();
            }
            Collidable = false;
            avoidPos = Vector2.Zero;
            hits++;
            foreach (CustomFinalBossShot entity in level.Tracker.GetEntities<CustomFinalBossShot>())
            {
                entity.Destroy();
            }
            foreach (CustomFinalBossShot entity2 in level.Tracker.GetEntities<CustomFinalBossShot>())
            {
                entity2.Destroy();
            }

            if (bossShield != null)
            {
                bossShield.orbs.ForEach(delegate (BossShieldOrb orb)
                {
                    bossShield.Disappear(orb);
                });
                bossShield = null;
            }

            attackCoroutine.Active = false;
            Moving = true;
            Add(new Coroutine(MoveSequence(player)));
        }

        private IEnumerator MoveSequence(Player player)
        {
            Tween tween3 = Tween.Create(Tween.TweenMode.Oneshot, null, 0.3f, start: true);
            tween3.OnUpdate = delegate (Tween t)
            {
                Glitch.Value = 0.5f * (1f - t.Eased);
            };
            Add(tween3);
            if (player != null && !player.Dead)
            {
                player.StartAttract(Center + Vector2.UnitY * 4f);
            }
            float timer = 0.15f;
            while (player != null && !player.Dead && !player.AtAttractTarget)
            {
                yield return null;
                timer -= Engine.DeltaTime;
            }
            if (timer > 0f)
            {
                yield return timer;
            }
            foreach (ReflectionTentacles tentacle in Scene.Tracker.GetEntities<ReflectionTentacles>())
            {
                tentacle.Retreat();
            }
            if (player != null)
            {
                Celeste.Freeze(0.1f);
                Engine.TimeRate = 0.75f;
                Input.Rumble(RumbleStrength.Strong, RumbleLength.Medium);
            }
            PushPlayer(player);
            level.Shake();
            yield return 0.05f;
            for (float i = 0f; i < (float)Math.PI * 2f; i += 0.17453292f)
            {
                Vector2 at = Center + Sprite.Position + Calc.AngleToVector(i + Calc.Random.Range(-(float)Math.PI / 90f, (float)Math.PI / 90f), Calc.Random.Range(16, 20));
                level.Particles.Emit(FinalBoss.P_Burst, at, i);
            }
            yield return 0.05f;
            Audio.SetMusicParam("boss_pitch", 0f);
            float from2 = Engine.TimeRate;
            Tween tween4 = Tween.Create(Tween.TweenMode.Oneshot, null, 0.35f / Engine.TimeRateB, start: true);
            tween4.UseRawDeltaTime = true;
            tween4.OnUpdate = delegate (Tween t)
            {
                if (bossBg != null && bossBg.Alpha < t.Eased)
                {
                    bossBg.Alpha = t.Eased;
                }
                Engine.TimeRate = MathHelper.Lerp(from2, 1f, t.Eased);
            };
            Add(tween4);
            yield return 0.2f;
            Vector2 from = Position;
            Vector2 to = GetNextPosition();
            while (to == Position)
            {
                to = GetNextPosition();
            }
            float time = Vector2.Distance(from, to) / 600f;
            float dir = (to - from).Angle();
            Tween tween = Tween.Create(Tween.TweenMode.Oneshot, Ease.SineInOut, time, start: true);
            tween.OnUpdate = delegate (Tween t)
            {
                Position = Vector2.Lerp(from, to, t.Eased);
                if (t.Eased >= 0.1f && t.Eased <= 0.9f && Scene.OnInterval(0.02f))
                {
                    TrailManager.Add(this, Player.NormalHairColor, 0.5f, frozenUpdate: false, useRawDeltaTime: false);
                    level.Particles.Emit(Player.P_DashB, 2, Center, Vector2.One * 3f, dir);
                }
            };
            tween.OnComplete = delegate
            {
                Sprite.Play("recoverHit");
                Moving = false;
                Collidable = true;
                Player entity = Scene.Tracker.GetEntity<Player>();
                if (entity != null)
                {
                    facing = Math.Sign(entity.X - X);
                    if (facing == 0)
                    {
                        facing = -1;
                    }
                }
                if (hits == 15)
                {
                    level.Displacement.AddBurst(Center, 0.5f, 24f, 96f, 0.4f);
                    changePattern(0);
                    Visible = false;
                    Collidable = false;
                }
                GetAttackPattern();
                floatSine.Reset();
            };
            Add(tween);
        }

        public Vector2 GetNextPosition()
        {
            var rand = new Random();
            Vector2 nextPosition = Position;
            if (hits == 2)
            {
                nextPosition = nodes[rand.Next(1, 3)];
            }
            else if (hits < 3 || (hits >= 11 && hits != 15))
            {
                nextPosition = nodes[rand.Next(1, 5)];
            }
            else if (hits < 11)
            {
                if (level.Session.GetFlag("boss_Challenge_Mode"))
                {
                    if (Position == nodes[5])
                    {
                        nextPosition = nodes[rand.Next(7, 9)];
                    }
                    else if (Position == nodes[8])
                    {
                        nextPosition = nodes[rand.Next(5, 7)];
                    }
                    else
                    {
                        nextPosition = nodes[rand.Next(5, 9)];
                    }
                }
                else
                {
                    nextPosition = nodes[rand.Next(5, 9)];
                }
            }
            else if (hits == 15)
            {
                nextPosition = Position + new Vector2(1, 0);
            }
            return nextPosition;
        }

        public void GetAttackPattern()
        {
            var rand = new Random();
            if (hits == 3 || hits == 5 || hits == 7 || hits == 9)
            {
                while (patternIndex == previousPatternIndex)
                {
                    changePattern(rand.Next(2, 5));
                }
                previousPatternIndex = patternIndex;
            }
            else if (hits >= 11)
            {
                while (patternIndex == previousPatternIndex)
                {
                    changePattern(rand.Next(5, 8));
                }
                previousPatternIndex = patternIndex;
            }
            StartAttacking();
        }

        private void PushPlayer(Player player)
        {
            if (player != null && !player.Dead)
            {
                int num = Position.X <= level.Bounds.Left + level.Bounds.Width / 2 ? 1 : -1;
                player.FinalBossPushLaunch(num);
                player.Speed *= 0.95f;
                player.Speed.Y *= 0.7f;
            }
            SceneAs<Level>().Displacement.AddBurst(Position, 0.4f, 12f, 36f, 0.5f);
            SceneAs<Level>().Displacement.AddBurst(Position, 0.4f, 24f, 48f, 0.5f);
            SceneAs<Level>().Displacement.AddBurst(Position, 0.4f, 36f, 60f, 0.5f);
        }

        private void StartAttacking()
        {
            switch (patternIndex)
            {
                case 0:
                case 1:
                    attackCoroutine.Replace(AttackSequenceP1());
                    break;
                case 2:
                    attackCoroutine.Replace(AttackSequenceP2A());
                    break;
                case 3:
                    attackCoroutine.Replace(AttackSequenceP2B());
                    break;
                case 4:
                    attackCoroutine.Replace(AttackSequenceP2C());
                    break;
                case 5:
                    attackCoroutine.Replace(AttackSequenceP3A());
                    break;
                case 6:
                    attackCoroutine.Replace(AttackSequenceP3B());
                    break;
                case 7:
                    attackCoroutine.Replace(AttackSequenceP3C());
                    break;
            }
        }

        private void StartShootCharge()
        {
            Sprite.Play("attack1Begin");
            chargeSfx.Play("event:/char/badeline/boss_bullet");
        }

        private IEnumerator AttackSequenceP1()
        {
            while (true)
            {
                if (Visible && InView())
                {
                    if (level.Session.GetFlag("boss_Challenge_Mode"))
                    {
                        yield return CastShield(3, 36, 2.8f, Position.X - level.Bounds.Left <= level.Bounds.Width / 2 ? true : false);
                    }
                    StartShootCharge();
                    yield return 0.4f;
                    GetTarget();
                    ShootAt(target);
                    yield return 0.7f;
                }
                else
                {
                    yield return null;
                }
            }
        }

        private IEnumerator AttackSequenceP2A()
        {
            while (true)
            {
                if (InView())
                {
                    if (level.Session.GetFlag("boss_Challenge_Mode"))
                    {
                        yield return CastShield(4, 36, 3.5f, Position.X - level.Bounds.Left <= level.Bounds.Width / 2 ? true : false);
                    }
                    yield return 0.4f;
                    StartShootCharge();
                    GetTarget();
                    ShootAt(target);
                    yield return 0.15f;
                    StartShootCharge();
                    ShootAt(target);
                    yield return 0.6f;
                }
                else
                {
                    yield return null;
                }
            }
        }

        private IEnumerator AttackSequenceP2B()
        {
            while (true)
            {
                if (InView())
                {
                    if (level.Session.GetFlag("boss_Challenge_Mode"))
                    {
                        yield return CastShield(4, 36, 3.5f, Position.X - level.Bounds.Left <= level.Bounds.Width / 2 ? true : false);
                    }
                    StartShootCharge();
                    yield return 0.6f;
                    GetTarget();
                    for (int i = 0; i < 3; i++)
                    {

                        if (i != 0)
                        {
                            StartShootCharge();
                        }
                        ShootAt(target);
                        yield return 0.15f;
                    }
                    yield return 0.4f;
                    StartShootCharge();
                    yield return 0.6f;
                    GetTarget();
                    for (int i = 0; i < 2; i++)
                    {

                        if (i != 0)
                        {
                            StartShootCharge();
                        }
                        ShootAt(target);
                        yield return 0.3f;
                    }
                }
                else
                {
                    yield return null;
                }
            }
        }

        private IEnumerator AttackSequenceP2C()
        {
            while (true)
            {
                if (InView())
                {
                    if (level.Session.GetFlag("boss_Challenge_Mode"))
                    {
                        yield return CastShield(4, 36, 3.5f, Position.X - level.Bounds.Left <= level.Bounds.Width / 2 ? true : false);
                    }
                    StartShootCharge();
                    yield return 0.35f;
                    GetTarget();
                    ShootAt(target);
                    yield return 0.4f;
                }
                else
                {
                    yield return null;
                }
            }
        }

        private IEnumerator AttackSequenceP3A()
        {
            while (true)
            {
                if (InView())
                {
                    if (level.Session.GetFlag("boss_Challenge_Mode"))
                    {
                        yield return CastShield(3, 36, 2.8f, Position.X - level.Bounds.Left <= level.Bounds.Width / 2 ? true : false);
                    }
                    yield return 0.4f;
                    StartShootCharge();
                    GetTarget();
                    ShootAt(target);
                    yield return 0.15f;
                    StartShootCharge();
                    ShootAt(target);
                    yield return 0.6f;
                }
                else
                {
                    yield return null;
                }
            }
        }

        private IEnumerator AttackSequenceP3B()
        {
            while (true)
            {
                if (InView())
                {
                    if (level.Session.GetFlag("boss_Challenge_Mode"))
                    {
                        yield return CastShield(3, 36, 2.8f, Position.X - level.Bounds.Left <= level.Bounds.Width / 2 ? true : false);
                    }
                    StartShootCharge();
                    yield return 0.6f;
                    GetTarget();
                    for (int i = 0; i < 3; i++)
                    {
                        if (i != 0)
                        {
                            StartShootCharge();
                        }
                        ShootAt(target);
                        yield return 0.15f;
                    }
                    yield return 0.4f;
                    StartShootCharge();
                    yield return 0.6f;
                    GetTarget();
                    for (int i = 0; i < 2; i++)
                    {
                        if (i != 0)
                        {
                            StartShootCharge();
                        }
                        ShootAt(target);
                        yield return 0.3f;
                    }
                }
                else
                {
                    yield return null;
                }
            }
        }

        private IEnumerator AttackSequenceP3C()
        {
            while (true)
            {
                if (InView())
                {
                    if (level.Session.GetFlag("boss_Challenge_Mode"))
                    {
                        yield return CastShield(3, 36, 2.8f, Position.X - level.Bounds.Left <= level.Bounds.Width / 2 ? true : false);
                    }
                    StartShootCharge();
                    yield return 0.35f;
                    GetTarget();
                    ShootAt(target);
                    yield return 0.4f;
                }
                else
                {
                    yield return null;
                }
            }
        }


        public void changePattern(int pattern)
        {
            patternIndex = pattern;
        }

        private bool InView()
        {
            Camera camera = (Scene as Level).Camera;
            return base.X > camera.X - 16f && Y > camera.Y - 16f && X < camera.X + 320f + 16f && Y < camera.Y + 180f + 16f;
        }

        private void GetTarget()
        {
            Player entity = level.Tracker.GetEntity<Player>();
            if (entity != null)
            {
                target = entity.Position;
            }
            else
            {
                StopShoot = true;
                attackCoroutine.Active = false;
            }
        }

        private void Shoot(float angleOffset = 0f)
        {
            if (!StopShoot)
            {
                if (!chargeSfx.Playing)
                {
                    chargeSfx.Play("event:/char/badeline/boss_bullet", "end", 1f);
                }
                else
                {
                    chargeSfx.Param("end", 1f);
                }
                Sprite.Play("attack1Recoil", restart: true);
                Player entity = level.Tracker.GetEntity<Player>();
                if (entity != null)
                {
                    level.Add(Engine.Pooler.Create<CustomFinalBossShot>().Init(this, entity, angleOffset));
                }
            }
        }

        private void ShootAt(Vector2 at)
        {
            if (!StopShoot)
            {
                if (!chargeSfx.Playing)
                {
                    chargeSfx.Play("event:/char/badeline/boss_bullet", "end", 1f);
                }
                else
                {
                    chargeSfx.Param("end", 1f);
                }
                Sprite.Play("attack1Recoil", restart: true);
                level.Add(Engine.Pooler.Create<CustomFinalBossShot>().InitAt(this, at));
            }
        }

        private IEnumerator Beam()
        {
            laserSfx.Play("event:/char/badeline/boss_laser_charge");
            Sprite.Play("attack2Begin", restart: true);
            yield return 0.1f;
            Player player = level.Tracker.GetEntity<Player>();
            if (player != null)
            {
                level.Add(Engine.Pooler.Create<CustomFinalBossBeam>().Init(this, player));
            }
            yield return 0.9f;
            Sprite.Play("attack2Lock", restart: true);
            yield return 0.5f;
            laserSfx.Stop();
            Audio.Play("event:/char/badeline/boss_laser_fire", Position);
            Sprite.Play("attack2Recoil");
        }

        private IEnumerator CastShield(int quantity, int radius, float rotationTime, bool clockwise)
        {
            yield return 0.05f;
            while (Moving)
            {
                yield return null;
            }
            if (bossShield == null)
            {
                Scene.Add(bossShield = new BossShield(this, quantity, radius, rotationTime, clockwise));
            }
        }

        public override void Removed(Scene scene)
        {
            if (bossBg != null && patternIndex == 0)
            {
                bossBg.Alpha = 1f;
            }
            base.Removed(scene);
        }

        private static bool _CanChangeMusic(bool value, CustomFinalBoss self)
        {
            return self.CanChangeMusic(value);
        }

        public bool CanChangeMusic(bool value)
        {
            if ((base.Scene as Level).Session.Area.LevelSet == "Celeste")
            {
                return value;
            }
            return canChangeMusic;
        }

        public void SetHits(int hits)
        {
            this.hits = hits;
        }
    }
}