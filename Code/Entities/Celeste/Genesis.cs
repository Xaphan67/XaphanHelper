using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Celeste.Mod.Entities;
using Celeste.Mod.XaphanHelper.Colliders;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Monocle;

namespace Celeste.Mod.XaphanHelper.Entities
{
    [Tracked(true)]
    [CustomEntity("XaphanHelper/Genesis")]
    public class Genesis : Actor
    {
        [Tracked(true)]
        public class GenesisBarrier : Solid
        {
            public GenesisBarrier(Vector2 offset, int width, int height) : base(offset, width, height, false)
            {
                SurfaceSoundIndex = 0;
                Collidable = false;
            }
        }

        [Tracked(true)]
        private class GenesisAcid : Actor
        {
            Sprite Sprite;

            public Vector2 Speed;

            private Collision onCollideH;

            private Collision onCollideV;

            private bool UpsideDown;

            private bool ToLeft;

            private float cantKillTimer;

            public GenesisAcid(Vector2 offset, Vector2 speed, bool toLeft, bool upsideDown = false) : base(offset)
            {
                Add(new PlayerCollider(onCollidePlayer));
                Collider = new Hitbox(4, 4, upsideDown ? (toLeft ? -6 : -1) : (toLeft ? -8 : -1), upsideDown ? (toLeft ? 3 : -1) : -1);
                Add(Sprite = new Sprite(GFX.Game, "characters/Xaphan/Genesis/"));
                Sprite.Origin = new Vector2(toLeft ? 4f : 10f, 3f);
                Sprite.AddLoop("acid", "acid", 0.08f);
                Sprite.Add("explode", "acidExplode", 0.04f);
                Sprite.Play("acid");
                Speed = new Vector2((speed.X + Calc.Random.Next(-15, 16)) * (toLeft ? -1 : 1), speed.Y + Calc.Random.Next(-15, 16));
                Add(new Coroutine(GravityRoutine()));
                onCollideH = OnCollideH;
                onCollideV = OnCollideV;
                UpsideDown = upsideDown;
                ToLeft = toLeft;
                cantKillTimer = 0.05f;
                Depth = 1;
            }

            public override void Update()
            {
                base.Update();
                if (cantKillTimer > 0f)
                {
                    cantKillTimer -= Engine.DeltaTime;
                }
                if (Sprite != null && Sprite.CurrentAnimationID != "explode")
                {
                    Sprite.Rotation = (float)Math.Atan2(Center.Y + Speed.Y - Center.Y, Center.X + Speed.X - Center.X);
                }
                MoveH(Speed.X * Engine.DeltaTime, onCollideH);
                MoveV(Speed.Y * Engine.DeltaTime, onCollideV);
            }

            public IEnumerator GravityRoutine()
            {
                while (Speed.Y <= 250f && Collidable)
                {
                    Speed.Y += 4f;
                    yield return null;
                }
            }

            private void OnCollideH(CollisionData data)
            {
                SceneAs<Level>().Add(new GenesisAcidSurface(Position, new Vector2(data.Direction.X, 0)));
                onCollide();
            }

            private void OnCollideV(CollisionData data)
            {
                SceneAs<Level>().Add(new GenesisAcidSurface(Position - new Vector2(5, UpsideDown && ToLeft ? -4 : 0), Vector2.UnitY));
                onCollide();
            }

            private void onCollide()
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
                if (cantKillTimer > 0)
                {
                    RemoveSelf();
                }
                else
                {
                    player.Die((player.Position - Position).SafeNormalize());
                }
            }

            private void onLastFrame(string s)
            {
                if (Sprite.CurrentAnimationID == "explode")
                {
                    RemoveSelf();
                }
            }

            public override void Render()
            {
                if (Sprite.CurrentAnimationID == "acid")
                {
                    Sprite.DrawOutline();
                }
                base.Render();
            }
        }

        [Tracked(true)]
        private class GenesisAcidSurface : Entity
        {
            private MTexture Texture;

            private float alpha;

            private Vector2 Direction;

            public GenesisAcidSurface(Vector2 offset, Vector2 direction) : base(offset)
            {
                Collider = new Hitbox(8, 8, direction.X == -1 ? -15 : 2);
                Direction = direction;
                Add(new PlayerCollider(onCollidePlayer));
                if (direction.Y == 1)
                {
                    Texture = GFX.Game["characters/Xaphan/Genesis/acidSurface0" + Calc.Random.Next(0, 2)];
                }
                else
                {
                    if (direction.X == -1)
                    {
                        Texture = GFX.Game["characters/Xaphan/Genesis/acidSurfaceR0" + Calc.Random.Next(0, 2)];
                    }
                    else if (direction.X == 1)
                    {
                        Texture = GFX.Game["characters/Xaphan/Genesis/acidSurfaceL0" + Calc.Random.Next(0, 2)];
                    }
                }
                alpha = 1f;
                Add(new Coroutine(LifeTimeRoutine()));
                Depth = -20000;
            }

            private void onCollidePlayer(Player player)
            {
                player.Die((player.Position - Position).SafeNormalize());
            }

            private IEnumerator LifeTimeRoutine()
            {
                float timer = SceneAs<Level>().Session.GetFlag("boss_Challenge_Mode") ? 20f : 10f + 0.5f;
                while (timer > 0)
                {
                    timer -= Engine.DeltaTime;
                    if (timer <= 1f)
                    {
                        alpha -= Engine.DeltaTime;
                    }
                    if (timer <= 0.5f)
                    {
                        Collidable = false;
                    }
                    yield return null;
                }
                RemoveSelf();
            }

            public override void Render()
            {
                base.Render();
                if (Direction.Y == 1)
                {
                    Texture.Draw(Position + new Vector2(2f, 3f), Vector2.Zero, Color.White * alpha);
                }
                else
                {
                    if (Direction.X == -1)
                    {
                        Texture.Draw(Position + Vector2.UnitX * -16, Vector2.Zero, Color.White * alpha);
                    }
                    else if (Direction.X == 1)
                    {
                        Texture.Draw(Position + Vector2.UnitX * 3, Vector2.Zero, Color.White * alpha);
                    }
                }
            }
        }

        private class GenesisSlashEffect : Entity
        {
            private Sprite Sprite;

            private float alpha;

            public GenesisSlashEffect(Vector2 offset, bool flip) : base(offset)
            {
                Collider = new Hitbox(24, 26, 2, 4);
                Add(new PlayerCollider(onCollidePlayer));
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
                while (alpha > 0)
                {
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

        [Pooled]
        [Tracked(false)]
        public class GenesisBeam : Entity
        {
            public static ParticleType P_Dissipate;

            private Genesis boss;

            private Player player;

            private Sprite beamSprite;

            private Sprite beamStartSprite;

            public float chargeTimer;

            private float followTimer;

            private float activeTimer;

            private float angle;

            private float beamAlpha;

            private float sideFadeAlpha;

            private VertexPositionColor[] fade = new VertexPositionColor[24];

            public GenesisBeam()
            {
                Add(beamSprite = new Sprite(GFX.Game, "characters/Xaphan/Genesis/"));
                beamSprite.Justify = new Vector2(0f, 0.5f);
                beamSprite.AddLoop("charge", "beam", 0.06f, 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13);
                beamSprite.Add("lock", "beam", 0.03f, 14, 15, 16, 17, 18, 19, 20, 21, 22, 23, 24, 25);
                beamSprite.Add("shoot", "beam", 0.04f, 26, 27, 28, 29, 30, 31, 32, 33, 34, 35, 36);
                beamSprite.Play("charge");
                beamSprite.OnLastFrame = delegate (string anim)
                {
                    if (anim == "shoot")
                    {
                        Destroy();
                    }
                };
                Add(beamStartSprite = new Sprite(GFX.Game, "characters/Xaphan/Genesis/"));
                beamStartSprite.Justify = new Vector2(0f, 0.5f);
                beamStartSprite.Add("shoot", "beamStart", 0.06f);
                beamSprite.Play("shoot");
                beamSprite.Visible = false;
                P_Dissipate = new ParticleType
                {
                    Color = Calc.HexToColor("559F1E"),
                    Size = 1f,
                    FadeMode = ParticleType.FadeModes.Late,
                    SpeedMin = 15f,
                    SpeedMax = 30f,
                    DirectionRange = (float)Math.PI / 3f,
                    LifeMin = 0.3f,
                    LifeMax = 0.6f
                };
                Depth = -1000000;
            }

            public GenesisBeam Init(Genesis boss, Player target)
            {
                this.boss = boss;
                chargeTimer = 0.8f; // 1.4f
                followTimer = 0.4f; // 0.9f
                activeTimer = 0.12f;
                beamSprite.Play("charge");
                sideFadeAlpha = 0f;
                beamAlpha = 0f;
                int num = (target.Y <= boss.Y + 16f) ? 1 : (-1);
                if (target.X >= boss.X)
                {
                    num *= -1;
                }
                angle = Calc.Angle(boss.BeamOrigin, target.Center);
                Vector2 to = Calc.ClosestPointOnLine(boss.BeamOrigin, boss.BeamOrigin + Calc.AngleToVector(angle, 2000f), target.Center);
                to += (target.Center - boss.BeamOrigin).Perpendicular().SafeNormalize(100f) * num;
                angle = Calc.Angle(boss.BeamOrigin, to);
                return this;
            }

            public override void Update()
            {
                base.Update();
                player = Scene.Tracker.GetEntity<Player>();
                beamAlpha = Calc.Approach(beamAlpha, 1f, 2f * Engine.DeltaTime);
                if (chargeTimer > 0f && player != null && !player.Dead)
                {
                    sideFadeAlpha = Calc.Approach(sideFadeAlpha, 1f, Engine.DeltaTime);
                    followTimer -= Engine.DeltaTime;
                    chargeTimer -= Engine.DeltaTime;
                    if (followTimer > 0f && player.Center != boss.BeamOrigin)
                    {
                        Vector2 val = Calc.ClosestPointOnLine(boss.BeamOrigin, boss.BeamOrigin + Calc.AngleToVector(angle, 2000f), player.Center);
                        Vector2 center = player.Center;
                        val = Calc.Approach(val, center, 600f * Engine.DeltaTime);
                        angle = Calc.Angle(boss.BeamOrigin, val);
                    }
                    else if (beamSprite.CurrentAnimationID == "charge")
                    {
                        beamSprite.Play("lock");
                    }
                    if (chargeTimer <= 0f)
                    {
                        SceneAs<Level>().DirectionalShake(Calc.AngleToVector(angle, 1f), 0.15f);
                        Input.Rumble(RumbleStrength.Medium, RumbleLength.Medium);
                        DissipateParticles();
                    }
                }
                else if (activeTimer > 0f)
                {
                    sideFadeAlpha = Calc.Approach(sideFadeAlpha, 0f, Engine.DeltaTime * 8f);
                    if (beamSprite.CurrentAnimationID != "shoot")
                    {
                        beamSprite.Play("shoot");
                        beamStartSprite.Play("shoot", restart: true);
                    }
                    activeTimer -= Engine.DeltaTime;
                    if (activeTimer > 0f)
                    {
                        PlayerCollideCheck();
                    }
                }
            }

            private void DissipateParticles()
            {
                Level level = SceneAs<Level>();
                Vector2 vector = level.Camera.Position + new Vector2(160f, 90f);
                Vector2 vector2 = boss.BeamOrigin + Calc.AngleToVector(angle, 12f);
                Vector2 vector3 = boss.BeamOrigin + Calc.AngleToVector(angle, 2000f);
                Vector2 vector4 = (vector3 - vector2).Perpendicular().SafeNormalize();
                Vector2 value = (vector3 - vector2).SafeNormalize();
                Vector2 min = -vector4 * 1f;
                Vector2 max = vector4 * 1f;
                float direction = vector4.Angle();
                float direction2 = (-vector4).Angle();
                float num = Vector2.Distance(vector, vector2) - 12f;
                vector = Calc.ClosestPointOnLine(vector2, vector3, vector);
                for (int i = 0; i < 200; i += 12)
                {
                    for (int j = -1; j <= 1; j += 2)
                    {
                        level.ParticlesFG.Emit(P_Dissipate, vector + value * i + vector4 * 2f * j + Calc.Random.Range(min, max), direction);
                        level.ParticlesFG.Emit(P_Dissipate, vector + value * i - vector4 * 2f * j + Calc.Random.Range(min, max), direction2);
                        if (i != 0 && i < num)
                        {
                            level.ParticlesFG.Emit(P_Dissipate, vector - value * i + vector4 * 2f * j + Calc.Random.Range(min, max), direction);
                            level.ParticlesFG.Emit(P_Dissipate, vector - value * i - vector4 * 2f * j + Calc.Random.Range(min, max), direction2);
                        }
                    }
                }
            }

            private void PlayerCollideCheck()
            {
                Vector2 vector = boss.BeamOrigin + Calc.AngleToVector(angle, 12f);
                Vector2 vector2 = boss.BeamOrigin + Calc.AngleToVector(angle, 2000f);
                Vector2 value = (vector2 - vector).Perpendicular().SafeNormalize(2f);
                Player player = Scene.CollideFirst<Player>(vector + value, vector2 + value);
                if (player == null)
                {
                    player = Scene.CollideFirst<Player>(vector - value, vector2 - value);
                }
                if (player == null)
                {
                    player = Scene.CollideFirst<Player>(vector, vector2);
                }
                player?.Die((player.Center - boss.BeamOrigin).SafeNormalize());
            }

            public override void Render()
            {
                Vector2 beamOrigin = boss.BeamOrigin;
                Vector2 vector = Calc.AngleToVector(angle, beamSprite.Width);
                beamSprite.Rotation = angle;
                beamSprite.Color = Color.White * beamAlpha;
                beamStartSprite.Rotation = angle;
                beamStartSprite.Color = Color.White * beamAlpha;
                if (beamSprite.CurrentAnimationID == "shoot")
                {
                    beamOrigin += Calc.AngleToVector(angle, 8f);
                    beamOrigin -= Vector2.UnitY * 6;
                }
                for (int i = 0; i < 15; i++)
                {
                    beamSprite.RenderPosition = beamOrigin;
                    beamSprite.Render();
                    beamOrigin += vector;
                }
                if (beamSprite.CurrentAnimationID == "shoot")
                {
                    beamStartSprite.RenderPosition = boss.BeamOrigin - Vector2.UnitY * 6;
                    beamStartSprite.Render();
                }
                GameplayRenderer.End();
                Vector2 vector2 = vector.SafeNormalize();
                Vector2 vector3 = vector2.Perpendicular();
                Color color = Color.Black * sideFadeAlpha * 0.35f;
                Color transparent = Color.Transparent;
                vector2 *= 4000f;
                vector3 *= 120f;
                int v = 0;
                Quad(ref v, beamOrigin, -vector2 + vector3 * 2f, vector2 + vector3 * 2f, vector2 + vector3, -vector2 + vector3, color, color);
                Quad(ref v, beamOrigin, -vector2 + vector3, vector2 + vector3, vector2, -vector2, color, transparent);
                Quad(ref v, beamOrigin, -vector2, vector2, vector2 - vector3, -vector2 - vector3, transparent, color);
                Quad(ref v, beamOrigin, -vector2 - vector3, vector2 - vector3, vector2 - vector3 * 2f, -vector2 - vector3 * 2f, color, color);
                GFX.DrawVertices((Scene as Level).Camera.Matrix, fade, fade.Length);
                GameplayRenderer.Begin();
            }

            private void Quad(ref int v, Vector2 offset, Vector2 a, Vector2 b, Vector2 c, Vector2 d, Color ab, Color cd)
            {
                fade[v].Position.X = offset.X + a.X;
                fade[v].Position.Y = offset.Y + a.Y;
                fade[v++].Color = ab;
                fade[v].Position.X = offset.X + b.X;
                fade[v].Position.Y = offset.Y + b.Y;
                fade[v++].Color = ab;
                fade[v].Position.X = offset.X + c.X;
                fade[v].Position.Y = offset.Y + c.Y;
                fade[v++].Color = cd;
                fade[v].Position.X = offset.X + a.X;
                fade[v].Position.Y = offset.Y + a.Y;
                fade[v++].Color = ab;
                fade[v].Position.X = offset.X + c.X;
                fade[v].Position.Y = offset.Y + c.Y;
                fade[v++].Color = cd;
                fade[v].Position.X = offset.X + d.X;
                fade[v].Position.Y = offset.Y + d.Y;
                fade[v++].Color = cd;
            }

            public void Destroy()
            {
                RemoveSelf();
            }
        }

        private enum Facings
        {
            Left,
            Right
        }

        private Vector2 OrigPosition;

        private Facings Facing;

        private PlayerCollider pc;

        public BombCollider bc;

        private bool ShouldSwitchFacing;

        private Coroutine Routine = new();

        private Coroutine HitRoutine = new();

        public  Sprite Sprite;

        public Vector2 BeamOrigin;

        public int Health;

        private float InvincibilityDelay;

        private bool Flashing;

        public Vector2 Speed;

        private bool MidAir;

        private bool noFlip;

        private Collision onCollideH;

        private Collision onCollideV;

        private float CannotLeapDelay;

        private float CannotDashDelay;

        private float CannotShootDelay;

        private float CannotBeamDelay;

        private bool IsSlashingBomb;

        public bool IsStun;

        private bool Falling;

        public bool playerHasMoved;

        private bool ShouldDisengage;

        private bool ShouldDash;

        private SoundSource laserSfx;

        private int FleeDir;

        private bool KilledPlayer;

        private Vector2 PlayerPos;

        [Tracked(true)]
        public Genesis(EntityData data, Vector2 offset) : base(data.Position + offset)
        {
            OrigPosition = Position;
            Visible = false;
            Collider = new Hitbox(44, 23, 2, 1);
            Add(Sprite = new Sprite(GFX.Game, "characters/Xaphan/Genesis/"));
            Sprite.AddLoop("idle", "stand", 0.08f);
            Sprite.AddLoop("walk", "walk", 0.08f);
            Sprite.Add("turn", "turn", 0.08f);
            Sprite.Add("leapUp", "leap", 0f, 0);
            Sprite.Add("dash", "leap", 0.08f);
            Sprite.Add("slash", "slash", 0.12f, 1, 0, 1);
            Sprite.Add("roar", "roar", 0.08f, 0, 1);
            Sprite.Add("roarVertical", "roar", 0.08f);
            Sprite.Add("fall", "leap", 0f, 1);
            Sprite.Play("idle");
            Facing = Facings.Right;
            Health = 15;
            onCollideH = OnCollideH;
            onCollideV = OnCollideV;
            Add(pc = new PlayerCollider(OnPlayer, new Hitbox(25, 15)));
            Add(bc = new BombCollider(OnBomb, new Hitbox(10, 15)));
            Add(laserSfx = new SoundSource());
        }

        public override void Added(Scene scene)
        {
            base.Added(scene);
            foreach (GenesisAcid acid in SceneAs<Level>().Tracker.GetEntities<GenesisAcid>())
            {
                acid.RemoveSelf();
            }
            foreach (GenesisAcidSurface surface in SceneAs<Level>().Tracker.GetEntities<GenesisAcidSurface>())
            {
                surface.RemoveSelf();
            }
            Add(new Coroutine(SequenceRoutine()));
            Add(new Coroutine(InvincibilityRoutine()));
            Add(new Coroutine(GravityRoutine()));
        }

        public void OnPlayer(Player player)
        {
            if (Health > 0)
            {
                if (!IsStun)
                {
                    if (player.StateMachine.State == Player.StDash || player.DashAttacking)
                    {
                        if (Routine.Active)
                        {
                            Routine.Cancel();
                        }
                        foreach (GenesisBeam beam in SceneAs<Level>().Tracker.GetEntities<GenesisBeam>())
                        {
                            beam.Destroy();
                        }
                        Speed = Vector2.Zero;
                        if (laserSfx.EventName == "event:/char/badeline/boss_laser_charge" && laserSfx.Playing)
                        {
                            laserSfx.Stop();
                        }
                        if (Health > 4 && !Sprite.FlipY)
                        {
                            Add(Routine = new Coroutine(IdleRoutine()));
                        }
                        else
                        {
                            CannotLeapDelay = 2.5f;
                            Add(Routine = new Coroutine(FallRoutine(player)));
                        }
                        IsStun = true;
                        if (!HitRoutine.Active)
                        {
                            Add(HitRoutine = new Coroutine(AttractRoutine(player)));
                        }
                    }
                    else
                    {
                        player.Die((player.Position - Position).SafeNormalize());
                    }
                }
                else if (player.StateMachine.State != 7 && player.StateMachine.State != 22 && !IsSlashingBomb)
                {
                    IsSlashingBomb = true;
                    if (Routine.Active)
                    {
                        Routine.Cancel();
                    }
                    Add(Routine = new Coroutine(SlashRoutine(null, player)));
                }
            }
        }

        private IEnumerator AttractRoutine(Player player)
        {
            if (player != null && !player.Dead)
            {
                player.StartAttract(Center + Vector2.UnitX * (Facing == Facings.Right ? 18f : -18f) + Vector2.UnitY * 4f);
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
            if (player != null)
            {
                Celeste.Freeze(0.1f);
                Input.Rumble(RumbleStrength.Strong, RumbleLength.Medium);
            }
            Audio.Play("event:/game/xaphan/genesis_growl", Position);
            PushPlayer(player);
        }

        private void PushPlayer(Player player)
        {
            if (player != null && !player.Dead)
            {
                //int playerFaceDir = player.Facing == (global::Celeste.Facings)1 ? 1 : -1;
                player.FinalBossPushLaunch(Facing == Facings.Right ? 1 : -1);
                player.Speed.X *= 0.95f;
                if (!Sprite.FlipY)
                {
                    player.Speed.Y *= 0.7f;
                }
                else
                {
                    player.Speed.Y = 50f;
                }
            }
            SceneAs<Level>().Displacement.AddBurst(player.Position, 0.4f, 12f, 36f, 0.5f);
            SceneAs<Level>().Displacement.AddBurst(player.Position, 0.4f, 24f, 48f, 0.5f);
            SceneAs<Level>().Displacement.AddBurst(player.Position, 0.4f, 36f, 60f, 0.5f);
        }

        private void OnBomb(Bomb bomb)
        {
            if (Collidable && !IsSlashingBomb && !IsStun && Health > 0)
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
            List<Entity> barriers = Scene.Tracker.GetEntities<GenesisBarrier>().ToList();
            foreach (GenesisBarrier barrier in barriers)
            {
                barrier.Collidable = true;
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
            if (Flashing)
            {
                Sprite.Color = Color.Red;
            }
            else
            {
                Sprite.Color = Color.White;
            }
            pc.Collider.Position = new Vector2(Facing == Facings.Right ? 19 : 4, !Sprite.FlipY ? 4 : 6);
            bc.Collider.Width = IsStun ? 25 : 10;
            bc.Collider.Position = new Vector2(Facing == Facings.Right ? (IsStun ? 19 : 34) : 4, !Sprite.FlipY ? 4 : 6);
            Collidable = !MidAir && SceneAs<Level>().Session.GetFlag("Genesis_Start");
            BeamOrigin = Center + Sprite.Position + new Vector2(Facing == Facings.Right ? 16 : -16, !Sprite.FlipY ? -4 : 9);
            MoveH(Speed.X * Engine.DeltaTime, onCollideH);
            MoveV(Speed.Y * Engine.DeltaTime, onCollideV);
            foreach (GenesisBarrier barrier in barriers)
            {
                barrier.Collidable = false;
            }
        }

        public void SetHealth(int health)
        {
            Health = health;
        }

        public IEnumerator SequenceRoutine()
        {
            while (!SceneAs<Level>().Session.GetFlag("Genesis_Start") || !playerHasMoved)
            {
                yield return null;
            }
            Visible = Collidable = true;
            CannotLeapDelay = 2.5f;
            while (Health > 0)
            {
                Player player = SceneAs<Level>().Tracker.GetEntity<Player>();
                if (player == null && !KilledPlayer)
                {
                    Speed.X = 0;
                    KilledPlayer = true;
                    if (Routine.Active)
                    {
                        Routine.Cancel();
                    }
                    Add(Routine = new Coroutine(IdleRoutine()));
                }
                if (player != null && !Routine.Active && !MidAir && Health > 0)
                {
                    if ((Facing == Facings.Right && player.Center.X > Center.X) || (Facing == Facings.Left && player.Center.X < Center.X)) // If player is in front of Genesis
                    {
                        if (!Sprite.FlipY) // If Genesis is on ground
                        {
                            if (!ShouldDisengage)
                            {
                                if (Calc.Random.Next(1, 101) >= 60 && CannotLeapDelay <= 0f && Health <= 12)
                                {
                                    Add(Routine = new Coroutine(LeapRoutine()));
                                }
                                else if (Math.Abs(player.Center.X - Center.X) >= 60 && Math.Abs(player.Center.X - Center.X) < 160 && CannotShootDelay <= 0f && Health > 4)
                                {
                                    Add(Routine = new Coroutine(ShootRoutine()));
                                }
                                else if (Math.Abs(player.Center.X - Center.X) >= 80 && Math.Abs(player.Center.X - Center.X) < 160 && CannotBeamDelay <= 0f && (SceneAs<Level>().Session.GetFlag("boss_Challenge_Mode") ? true : Health <= 8))
                                {
                                    Add(Routine = new Coroutine(BeamRoutine(player)));
                                }
                                else
                                {
                                    Add(Routine = new Coroutine(WalkRoutine(player)));
                                }
                            }
                            else
                            {
                                Add(Routine = new Coroutine(WalkAwayRoutine(player)));
                            }
                        }
                        else // If genesis is on the ceiling
                        {
                            if ((Math.Abs(player.Center.X - Center.X) >= 40 && Math.Abs(player.Center.X - Center.X) < 120 && CannotDashDelay <= 0f && Health > 4) || ShouldDash)
                            {
                                ShouldDash = false;
                                Add(Routine = new Coroutine(DashRoutine(player)));
                            }
                            else if (Math.Abs(player.Center.X - Center.X) >= 60 && Math.Abs(player.Center.X - Center.X) < 160 && CannotShootDelay <= 0f && Health > 4)
                            {
                                Add(Routine = new Coroutine(ShootRoutine(true)));
                            }
                            else if (Math.Abs(player.Center.X - Center.X) <= 60 && Math.Abs(player.Center.X - Center.X) < 160 && CannotShootDelay <= 0f && Health > 4)
                            {
                                Add(Routine = new Coroutine(ShootRoutine(true, true)));
                            }
                            else if (Math.Abs(player.Center.X - Center.X) >= 80 && Math.Abs(player.Center.X - Center.X) < 160 && CannotBeamDelay <= 0f && (SceneAs<Level>().Session.GetFlag("boss_Challenge_Mode") ? true : Health <= 8))
                            {
                                Add(Routine = new Coroutine(BeamRoutine(player, true)));
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
                if (player != null && player.StateMachine.State != Player.StDash && !player.DashAttacking && (Facing == Facings.Right ? player.Left >= Right - 16f && player.Left <= Right + 8f : player.Right <= Left + 16f && player.Right >= Left - 8f) && player.Top >= Top && player.Bottom <= Bottom && !IsSlashingBomb && !IsStun && !MidAir)
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
                if (CannotBeamDelay > 0)
                {
                    CannotBeamDelay -= Engine.DeltaTime;
                }
                yield return null;
            }
            if (Routine.Active)
            {
                Routine.Cancel();
            }
            Add(Routine = new Coroutine(DeathRoutine()));
            while (Health <= 0)
            {
                yield return null;
            }
            Visible = true;
            Add(new Coroutine(SequenceRoutine()));
            Add(new Coroutine(InvincibilityRoutine()));
            Add(new Coroutine(GravityRoutine()));
        }

        public IEnumerator LeapRoutine()
        {
            Sprite.Position = new Vector2(0, -8f);
            Sprite.Play("leapUp");
            Speed.X = Facing == Facings.Right ? -100f : 100f;
            Speed.Y = -325f;
            MidAir = true;
            Audio.Play("event:/game/xaphan/genesis_jump", Position);
            while (MidAir)
            {
                yield return null;
            }
        }

        public IEnumerator ShootRoutine(bool bellow = false, bool vertical = false)
        {
            Sprite.Position = new Vector2(0f, bellow ? 0 : -8f);
            Speed.X = 0f;
            Sprite.Play("roar" + (vertical ? "Vertical" : ""));
            yield return Sprite.CurrentAnimationTotalFrames * 0.08f;
            yield return SceneAs<Level>().Session.GetFlag("boss_Challenge_Mode") ? 0.15f : 0.3f;
            float shootDuration = 0.5f;
            Audio.Play("event:/game/xaphan/genesis_spit", Position);
            while (shootDuration > 0)
            {
                shootDuration -= Engine.DeltaTime;
                if (SceneAs<Level>().OnInterval(SceneAs<Level>().Session.GetFlag("boss_Challenge_Mode") ? 0.04f : 0.06f))
                {
                    if (!vertical)
                    {
                        SceneAs<Level>().Add(new GenesisAcid(new Vector2(Position.X + (Facing == Facings.Right ? 40 : 8), Position.Y + (bellow ? 23 : 1)), new Vector2(Calc.Random.Next(SceneAs<Level>().Session.GetFlag("boss_Challenge_Mode") ? 140 : 110, SceneAs<Level>().Session.GetFlag("boss_Challenge_Mode") ? 240 : 210), bellow ? 75f : -25f), Facing == Facings.Left, bellow));
                    }
                    else
                    {
                        SceneAs<Level>().Add(new GenesisAcid(new Vector2(Position.X + (Facing == Facings.Right ? 32 : 16), Position.Y + 25), new Vector2(Calc.Random.Next(-50, 51), 75f), Facing == Facings.Left, true));
                    }
                }
                yield return null;
            }
            Sprite.Play("idle");
            Sprite.Position = Vector2.Zero;
            yield return Sprite.CurrentAnimationTotalFrames * 0.08f;
            CannotShootDelay = SceneAs<Level>().Session.GetFlag("boss_Challenge_Mode") ? 4.5f : 3f;
            if (bellow)
            {
                ShouldDash = true;
            }
        }

        private IEnumerator BeamRoutine(Player player, bool bellow = false)
        {
            Sprite.Position = new Vector2(0f, bellow ? 0 : -8f);
            yield return GenerateBeam(player);
            if (SceneAs<Level>().Session.GetFlag("boss_Challenge_Mode") && (Facing == Facings.Right ? player.Center.X > Center.X + 24f : player.Center.X < Center.X - 24f))
            {
                yield return GenerateBeam(player, false);
                yield return 0.5f;
            }
            Sprite.Play("idle");
            Sprite.Position = Vector2.Zero;
            yield return 0.5f;
            laserSfx.Stop();
            CannotBeamDelay = SceneAs<Level>().Session.GetFlag("boss_Challenge_Mode") ? Calc.Random.Next(3, 6) : 5f;
            if (bellow)
            {
                ShouldDash = true;
            }
        }

        private IEnumerator GenerateBeam(Player player, bool playSprite = true)
        {
            laserSfx.Play("event:/char/badeline/boss_laser_charge");
            Speed.X = 0f;
            if (playSprite)
            {
                Sprite.Play("roar", restart: true);
                yield return Sprite.CurrentAnimationTotalFrames * 0.08f;
            }
            GenesisBeam beam = null;
            if (player != null)
            {
                SceneAs<Level>().Add(beam = Engine.Pooler.Create<GenesisBeam>().Init(this, player));
            }
            while ((SceneAs<Level>().Session.GetFlag("boss_Challenge_Mode") ? beam.chargeTimer > 0.5f : beam.chargeTimer > 0))
            {
                if ((Facing == Facings.Right && player.Center.X > Center.X + 16f) || (Facing == Facings.Left && player.Center.X < Center.X - 16f))
                {
                    yield return null;
                }
                else
                {
                    {
                        beam.Destroy();
                        break;
                    }
                }
            }
            Add(new Coroutine(BeamFireSoud()));
        }

        private IEnumerator BeamFireSoud()
        {
            if (SceneAs<Level>().Session.GetFlag("boss_Challenge_Mode"))
            {
                yield return 0.5f;
            }
            Audio.Play("event:/char/badeline/boss_laser_fire", Position);
        }

        public IEnumerator DashRoutine(Player player)
        {
            Sprite.Stop();
            yield return 0.25f;
            Sprite.Position = new Vector2(0, -8f);
            Sprite.Play("dash");
            Sprite.FlipY = false;
            noFlip = true;
            Speed = new Vector2(player.Center.X - Center.X + (Facing == Facings.Right ? 96 : -96), 275f);
            MidAir = true;
            Audio.Play("event:/game/xaphan/genesis_growl", Position);
            while (MidAir)
            {
                yield return null;
            }
            Speed.X = 0f;
            yield return IdleRoutine();
        }

        public IEnumerator WalkRoutine(Player player)
        {
            Sprite.Position = Vector2.Zero;
            Sprite.Play("walk");
            bool walkLeft = player.Center.X < Center.X;
            float speed = SceneAs<Level>().Session.GetFlag("boss_Challenge_Mode") ? 150f : Health >= 13 ? 100f : Health >= 5f ? 125f : 150f;
            Sprite.Rate = SceneAs<Level>().Session.GetFlag("boss_Challenge_Mode") ? 1.5f : Health >= 13 ? 1f : Health >= 5f ? 1.25f : 1.5f;
            Speed.X = speed * (walkLeft ? -1 : 1);
            float walkDuration = Sprite.CurrentAnimationTotalFrames * 0.08f / Sprite.Rate;
            yield return walkDuration;
            Speed.X = 0f;
            Sprite.Rate = 1f;
        }

        public IEnumerator WalkAwayRoutine(Player player)
        {
            Sprite.Position = Vector2.Zero;
            Sprite.Reverse("walk");
            if (Health >= 12)
            {
                FleeDir = player.Center.X > Center.X ? -1 : 1;
            }
            else
            {
                if (FleeDir == 0)
                {
                    FleeDir = player.Center.X > Center.X ? -1 : 1;
                }
                else
                {
                    FleeDir = -FleeDir;
                }
            }
            Sprite.Rate = 2f;
            Speed.X = 150f * FleeDir;
            if (Health <= 4)
            {
                while (FleeDir == -1 ? Center.X > player.Center.X - 80f : Center.X < player.Center.X + 80f)
                {
                    yield return null;
                }
                ShouldSwitchFacing = Facing == Facings.Right ? player.Center.X < Center.X : player.Center.X > Center.X;
                yield return BeamRoutine(player);
                Sprite.Position = Vector2.Zero;
                Sprite.Reverse("walk");
                Sprite.Rate = 2f;
                Speed.X = 150f * FleeDir;
            }
            if (Health >= 13 && ((Center.X <= SceneAs<Level>().Bounds.Left + 104f) || (Center.X >= SceneAs<Level>().Bounds.Right - 104f)))
            {
                yield return WalkRoutine(player);
            }
            else
            {
                while (
                Health <= 4 ?
                    (FleeDir == -1 ?
                        Center.X > SceneAs<Level>().Bounds.Left + 96f
                    :
                        Center.X < SceneAs<Level>().Bounds.Right - 96f
                    )
                :
                    (Health >= 13 ?
                        (FleeDir == -1 ?
                            ((Center.X > PlayerPos.X - 120f) && (Center.X > SceneAs<Level>().Bounds.Left + 96f))
                        :
                            ((Center.X < PlayerPos.X + 120f) && (Center.X < SceneAs<Level>().Bounds.Right - 96f))
                        )
                    :
                        (FleeDir == -1 ?
                            ((Center.X > player.Center.X - 160f) && (Center.X > SceneAs<Level>().Bounds.Left + 96f))
                        :
                            ((Center.X < player.Center.X + 160f) && (Center.X < SceneAs<Level>().Bounds.Right - 96f))
                        )
                    )
                )
                {
                    yield return null;
                }
                PlayerPos = Vector2.Zero;
            }
            Speed.X = 0f;
            Sprite.Rate = 1f;
            ShouldDisengage = false;
            if (Health == 12)
            {
                FleeDir = 0;
            }
            if (Health <= 4)
            {
                yield return LeapRoutine();
            }
            else
            {
                if ((Facing == Facings.Right && Center.X > player.Center.X) || (Facing == Facings.Left && Center.X < player.Center.X))
                {
                    yield return TurnRoutine();
                }
                int chance = Calc.Random.Next(1, 101);
                if (Health >= 5 && Health <= (SceneAs<Level>().Session.GetFlag("boss_Challenge_Mode") ? 15 : 12) && chance <= 30)
                {
                    yield return ShootRoutine();
                }
                else if (Health <= (SceneAs<Level>().Session.GetFlag("boss_Challenge_Mode") ? 15 : 12) && chance <= 80)
                {
                    yield return BeamRoutine(player);
                }
                else if (Health <= 12)
                {
                    yield return LeapRoutine();
                }
                else
                {
                    yield return IdleRoutine();
                }
            }
        }


        public IEnumerator TurnRoutine()
        {
            Sprite.Position = Vector2.Zero;
            Sprite.Play("turn");
            float turnDuration = Sprite.CurrentAnimationTotalFrames * 0.08f;
            yield return turnDuration;
            yield return IdleRoutine(true);
        }

        public IEnumerator SlashRoutine(Bomb bomb, Player player)
        {
            Sprite.Position = new Vector2(-4f, Sprite.FlipY ? 0f : -16f);
            Speed.X = 0f;
            foreach (GenesisBeam beam in SceneAs<Level>().Tracker.GetEntities<GenesisBeam>())
            {
                beam.Destroy();
            }
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
            if (Health <= 12)
            {
                if (bomb != null)
                {
                    int num = Calc.Random.Next(1, 101);
                    if (num <= 50 || player == null)
                    {
                        yield return LeapRoutine();
                    }
                    else
                    {
                        yield return ShootRoutine();
                    }
                }
            }
            yield return IdleRoutine();
        }

        public IEnumerator IdleRoutine(bool flip = false, bool playIddleAnim = true)
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
                float timer = 0f;
                if (IsStun && Health > 0)
                {
                    timer = SceneAs<Level>().Session.GetFlag("boss_Challenge_Mode") ? 1.75f : 2f;
                }
                else
                {
                    timer = Health >= 13 ? 0.3f : Health >= 5f ? 0.2f : Health >= 1f ? 0.1f : 1f;
                }
                while (timer > 0f && (InvincibilityDelay <= 0 || InvincibilityDelay > 0.25f))
                {
                    timer -= Engine.DeltaTime;
                    yield return null;
                }
                if (InvincibilityDelay > 0 && !ShouldDisengage && Health <= 8)
                {
                    yield return LeapRoutine();
                }
            }
            IsStun = false;
        }

        private IEnumerator FallRoutine(Player player)
        {
            while (player.StateMachine.State == 7)
            {
                yield return null;
            }
            Sprite.Position = new Vector2(0, -8f);
            Sprite.Play("fall");
            Falling = true;
            Speed.X = 0f;
            Speed.Y = 325f;
            MidAir = true;
            while (MidAir)
            {
                yield return null;
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

        public void GetHit()
        {
            if (Health > 0 && InvincibilityDelay <= 0)
            {
                Health -= 1;
                if (Health > 0)
                {
                    Audio.Play("event:/game/xaphan/genesis_hit", Position);
                }
                else
                {
                    Audio.Play("event:/game/xaphan/genesis_death", Position);
                }
                Player player = SceneAs<Level>().Tracker.GetEntity<Player>();
                if (player != null)
                {
                    PlayerPos = player.Center;
                }
                if (Health >= 9 || Health <= 4)
                {
                    ShouldDisengage = true;
                }
                InvincibilityDelay = 0.75f;
            }
        }

        public IEnumerator GravityRoutine()
        {
            while (true)
            {
                if (!MidAir)
                {
                    Speed.Y -= Sprite.FlipY ? 4f : -4f;
                }
                yield return null;
            }
        }

        private void OnCollideH(CollisionData data)
        {
            if (Health <= 0 && data.Hit is DashBlock)
            {
                DashBlock block = data.Hit as DashBlock;
                block.Break(Center, -Vector2.UnitY, true);
                Vector2 bounds = new(SceneAs<Level>().Bounds.Left, SceneAs<Level>().Bounds.Top);
                foreach (Spikes spike in SceneAs<Level>().Tracker.GetEntities<Spikes>())
                {
                    if (spike.Position == bounds + new Vector2(576f, 80f))
                    {
                        spike.RemoveSelf();
                    }
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
                if (Falling)
                {
                    if (Health <= 4)
                    {
                        GetHit();
                    }
                    else
                    {
                        IsStun = false;
                        ShouldDisengage = true;
                    }
                    Sprite.FlipY = false;
                    Falling = false;
                    if (Health > 0)
                    {
                        Add(Routine = new Coroutine(IdleRoutine()));
                    }
                }
            }
        }

        private IEnumerator DeathRoutine()
        {
            float musicFadeStart = 0f;
            while (musicFadeStart < 1)
            {
                musicFadeStart += Engine.DeltaTime;
                Audio.SetMusicParam("fade", 1f - musicFadeStart);
                yield return null;
            }
            yield return IdleRoutine();
            Audio.SetMusicParam("fade", 1f);
            SceneAs<Level>().Session.Audio.Music.Event = SFX.EventnameByHandle("event:/music/xaphan/lvl_5_geothermal");
            SceneAs<Level>().Session.Audio.Apply();
            Sprite.Position = Vector2.Zero;
            Facing = Facings.Right;
            Sprite.FlipX = false;
            Sprite.Play("walk");
            Sprite.Rate = 1.5f;
            Speed.X = 150f;
            while (Center.X < SceneAs<Level>().Bounds.Right - 136f)
            {
                yield return null;
            }
            Sprite.Rate = 1f;
            Speed = new Vector2(150f, -130f);
            while (Left < SceneAs<Level>().Bounds.Right + 8f)
            {
                yield return null;
            }
            Visible = false;
            Speed = Vector2.Zero;
            Position = OrigPosition;
            ShouldDisengage = false;
            CannotBeamDelay = CannotDashDelay = CannotLeapDelay = CannotShootDelay = 0f;
        }

        public override void Render()
        {
            Sprite.DrawOutline();
            base.Render();
        }
    }
}
