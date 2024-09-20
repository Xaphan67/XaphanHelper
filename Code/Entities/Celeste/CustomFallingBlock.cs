using System;
using System.Collections;
using System.Collections.Generic;
using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste.Mod.XaphanHelper.Entities
{
    [Tracked(false)]
    [CustomEntity("XaphanHelper/CustomFallingBlock")]
    public class CustomFallingBlock : Solid
    {
        public static ParticleType P_FallDustA;

        public static ParticleType P_FallDustB;

        public static ParticleType P_LandDust;

        public bool Triggered;

        public float FallDelay;

        private char TileType;

        private TileGrid tiles;

        private bool climbFall;

        private bool fallIfNoSolidOnTop;

        private bool magneticCeilingsNoTrigger;

        private bool canFloat;

        public bool HasStartedFalling { get; private set; }

        public CustomFallingBlock(EntityData data, Vector2 offset) : base(data.Position + offset, data.Width, data.Height, false)
        {
            climbFall = data.Bool("climbFall", true);
            fallIfNoSolidOnTop = data.Bool("fallIfNoSolidOnTop", false);
            magneticCeilingsNoTrigger = data.Bool("magnetingCeilingsDoNotTrigger", false);
            canFloat = data.Bool("canFloat", false);
            int newSeed = Calc.Random.Next();
            Calc.PushRandom(newSeed);
            Add(tiles = GFX.FGAutotiler.GenerateBox(data.Char("tiletype", '3'), data.Width / 8, data.Height / 8).TileGrid);
            Calc.PopRandom();
            Add(new Coroutine(Sequence()));
            Add(new LightOcclude());
            Add(new TileInterceptor(tiles, highPriority: false));
            TileType = data.Char("tiletype", '3');
            SurfaceSoundIndex = SurfaceIndex.TileToIndex[data.Char("tiletype", '3')];
            if (data.Bool("behind"))
            {
                Depth = 5000;
            }
        }

        public override void Update()
        {
            foreach (SolidMovingPlatform platform in SceneAs<Level>().Tracker.GetEntities<SolidMovingPlatform>())
            {
                platform.Collidable = false;
            }
            base.Update();
            if (fallIfNoSolidOnTop)
            {
                if (!CollideCheck<Solid>(Position - Vector2.UnitY))
                {
                    Triggered = true;
                }
            }
            foreach (SolidMovingPlatform platform in SceneAs<Level>().Tracker.GetEntities<SolidMovingPlatform>())
            {
                platform.Collidable = true;
            }
        }

        public override void OnShake(Vector2 amount)
        {
            base.OnShake(amount);
            tiles.Position += amount;
        }

        private bool PlayerFallCheck()
        {
            if (climbFall)
            {
                if (SceneAs<Level>().Session.GetFlag("Xaphan_Helper_Ceiling") && magneticCeilingsNoTrigger)
                {
                    return HasPlayerRider() && !HasPlayerOnTop();
                }
                return HasPlayerRider();
            }
            if (SceneAs<Level>().Session.GetFlag("Xaphan_Helper_Ceiling") && magneticCeilingsNoTrigger)
            {
                return false;
            }
            return HasPlayerOnTop();
        }

        private bool PlayerWaitCheck()
        {
            if (Triggered)
            {
                return true;
            }
            if (PlayerFallCheck())
            {
                return true;
            }
            if (climbFall)
            {
                if (!CollideCheck<Player>(Position - Vector2.UnitX))
                {
                    return CollideCheck<Player>(Position + Vector2.UnitX);
                }
                return true;
            }
            return false;
        }

        private IEnumerator Sequence()
        {
            while (!Triggered && !PlayerFallCheck())
            {
                yield return null;
            }
            while (FallDelay > 0f)
            {
                FallDelay -= Engine.DeltaTime;
                yield return null;
            }
            HasStartedFalling = true;
            while (true)
            {
                ShakeSfx();
                StartShaking();
                Input.Rumble(RumbleStrength.Medium, RumbleLength.Medium);
                yield return 0.2f;
                float timer = 0.4f;
                while (timer > 0f && PlayerWaitCheck())
                {
                    yield return null;
                    timer -= Engine.DeltaTime;
                }
                StopShaking();
                for (int i = 2; i < Width; i += 4)
                {
                    if (Scene.CollideCheck<Solid>(TopLeft + new Vector2(i, -2f)))
                    {
                        SceneAs<Level>().Particles.Emit(FallingBlock.P_FallDustA, 2, new Vector2(X + i, Y), Vector2.One * 4f, (float)Math.PI / 2f);
                    }
                    SceneAs<Level>().Particles.Emit(FallingBlock.P_FallDustB, 2, new Vector2(X + i, Y), Vector2.One * 4f);
                }
                float speed = 0f;
                float maxSpeed = 160f;
                Level level = SceneAs<Level>();
                while (true)
                {
                    speed = Calc.Approach(speed, maxSpeed, 500f * Engine.DeltaTime);
                    if (MoveVCollideSolids(speed * Engine.DeltaTime, thruDashBlocks: true))
                    {
                        break;
                    }
                    if (Top > (level.Bounds.Bottom + 16) || (Top > (level.Bounds.Bottom - 1) && CollideCheck<Solid>(Position + new Vector2(0f, 1f))))
                    {
                        Collidable = (Visible = false);
                        yield return 0.2f;
                        if (level.Session.MapData.CanTransitionTo(level, new Vector2(Center.X, Bottom + 12f)))
                        {
                            yield return 0.2f;
                            SceneAs<Level>().Shake();
                            Input.Rumble(RumbleStrength.Strong, RumbleLength.Medium);
                        }
                        RemoveSelf();
                        DestroyStaticMovers();
                        yield break;
                    }
                    if (CollideCheck<Liquid>() && canFloat)
                    {
                        break;
                    }
                    yield return null;
                }
                if (canFloat)
                {
                    Liquid liquid = SceneAs<Level>().Tracker.GetNearestEntity<Liquid>(BottomCenter);
                    if (liquid != null)
                    {
                        if (liquid.liquidType == "Water")
                        {
                            Audio.Play("event:/char/madeline/water_in", BottomCenter, "deep", 1);
                            liquid.PlaySplashIn(new Vector2(BottomCenter.X - 12f, liquid.Top - 21f));
                        }
                        while ((TopCenter.Y < (liquid.TopCenter.Y - 24f)) && !CollideCheck<SolidTiles>(Position + new Vector2(0f, 1f)))
                        {
                            speed = Calc.Approach(speed, maxSpeed, 500f * Engine.DeltaTime);
                            if (MoveVCollideSolids(speed * Engine.DeltaTime, thruDashBlocks: true))
                            {
                                break;
                            }
                            yield return null;
                        }
                        while (speed > 0)
                        {
                            speed = Calc.Approach(speed, 0, 350f * Engine.DeltaTime);
                            if (MoveVCollideSolids(speed * Engine.DeltaTime, thruDashBlocks: true))
                            {
                                break;
                            }
                            yield return null;
                        }
                        while (TopCenter.Y > (HasPlayerRider() ? liquid.TopCenter.Y + 8 : liquid.TopCenter.Y - 8))
                        {
                            speed = Calc.Approach(speed, -80f, 325f * Engine.DeltaTime);
                            if (MoveVCollideSolids(speed * Engine.DeltaTime, thruDashBlocks: true))
                            {
                                break;
                            }
                            yield return null;
                        }
                        while (true)
                        {
                            if ((HasPlayerRider() && !CollideCheck<SolidTiles>(Position + new Vector2(0f, 1f))) || TopCenter.Y > liquid.TopCenter.Y + 8)
                            {
                                MoveTowardsY(liquid.TopCenter.Y + 8, Math.Max(0.3f, Math.Abs(liquid.TopCenter.Y + 8 - TopCenter.Y) / 24f));
                            }
                            else if (!HasPlayerRider() && (TopCenter.Y != liquid.TopCenter.Y - 8) && !CollideCheck<SolidTiles>(Position - new Vector2(0f, 1f)))
                            {
                                MoveTowardsY(liquid.TopCenter.Y - 8, Math.Max(0.3f, Math.Abs(liquid.TopCenter.Y - 8 - TopCenter.Y) / 16f));
                            }
                            yield return null;
                        }
                    }
                    break;
                }
                else
                {
                    ImpactSfx();
                    Input.Rumble(RumbleStrength.Strong, RumbleLength.Medium);
                    SceneAs<Level>().DirectionalShake(Vector2.UnitY, 0.3f);
                    StartShaking();
                    LandParticles();
                    yield return 0.2f;
                    StopShaking();
                    if (CollideCheck<SolidTiles>(Position + new Vector2(0f, 1f)))
                    {
                        break;
                    }
                    while (CollideCheck<Platform>(Position + new Vector2(0f, 1f)))
                    {
                        yield return 0.1f;
                    }
                    Safe = true;
                }
            }
        }

        private static float JITBarrier(float v)
        {
            return v;
        }

        private void LandParticles()
        {
            for (int i = 2; i <= Width; i += 4)
            {
                if (Scene.CollideCheck<Solid>(BottomLeft + new Vector2(i, 3f)))
                {
                    SceneAs<Level>().ParticlesFG.Emit(FallingBlock.P_FallDustA, 1, new Vector2(X + i, Bottom), Vector2.One * 4f, -(float)Math.PI / 2f);
                    float direction = ((!(i < Width / 2f)) ? 0f : ((float)Math.PI));
                    SceneAs<Level>().ParticlesFG.Emit(FallingBlock.P_LandDust, 1, new Vector2(X + i, Bottom), Vector2.One * 4f, direction);
                }
            }
        }

        private void ShakeSfx()
        {
            if (TileType == '3')
            {
                Audio.Play("event:/game/01_forsaken_city/fallblock_ice_shake", Center);
            }
            else if (TileType == '9')
            {
                Audio.Play("event:/game/03_resort/fallblock_wood_shake", Center);
            }
            else if (TileType == 'g')
            {
                Audio.Play("event:/game/06_reflection/fallblock_boss_shake", Center);
            }
            else
            {
                Audio.Play("event:/game/general/fallblock_shake", Center);
            }
        }

        private void ImpactSfx()
        {
            if (TileType == '3')
            {
                Audio.Play("event:/game/01_forsaken_city/fallblock_ice_impact", BottomCenter);
            }
            else if (TileType == '9')
            {
                Audio.Play("event:/game/03_resort/fallblock_wood_impact", BottomCenter);
            }
            else if (TileType == 'g')
            {
                Audio.Play("event:/game/06_reflection/fallblock_boss_impact", BottomCenter);
            }
            else
            {
                Audio.Play("event:/game/general/fallblock_impact", BottomCenter);
            }
        }
    }
}
