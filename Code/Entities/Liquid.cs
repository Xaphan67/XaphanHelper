using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Celeste.Mod.Entities;
using Celeste.Mod.Helpers;
using Celeste.Mod.XaphanHelper.Managers;
using Celeste.Mod.XaphanHelper.UI_Elements;
using Celeste.Mod.XaphanHelper.Upgrades;
using FMOD.Studio;
using Microsoft.Xna.Framework;
using Mono.Cecil.Cil;
using Monocle;
using MonoMod.Cil;
using MonoMod.RuntimeDetour;

namespace Celeste.Mod.XaphanHelper.Entities
{
    [Tracked(true)]
    [CustomEntity("XaphanHelper/Liquid")]
    class Liquid : Entity
    {
        public class AirBubble : Actor
        {
            private Sprite sprite;

            private Liquid liquid;

            private SineWave sine;

            private Collision onCollide;

            public AirBubble(Vector2 position, Liquid sourceLquid) : base(position)
            {
                Collider = new Hitbox(4f, 4f);
                Add(sprite = new Sprite(GFX.Game, "objects/XaphanHelper/liquid/"));
                sprite.AddLoop("idle", "bubble", 0.08f, 0);
                sprite.AddLoop("break", "bubble", 0.08f, 1);
                sprite.Color = Color.White * 0.7f;
                sprite.Play("idle");
                liquid = sourceLquid;
                Add(sine = new SineWave(0.44f, 0f).Randomize());
                onCollide = OnCollide;
            }

            public override void Update()
            {
                base.Update();
                sine.Update();
                if (liquid != null && Top > liquid.Top)
                {
                    MoveV(-25f * Engine.DeltaTime, onCollide);
                }
                else
                {
                    Break();
                }
            }

            private void OnCollide(CollisionData data)
            {
                Break();
            }
            private void Break()
            {
                sprite.Play("break");
                sprite.OnLastFrame = delegate
                {
                    RemoveSelf();
                };
            }
        }

        public int lowPosition;

        private Sprite liquidSprite;

        private Sprite waterSplashIn;

        private Sprite waterSplashOut;

        public string liquidType;

        private PlayerCollider pc;

        private SoundSource sfx;

        private SoundSource sfx2;

        public static FieldInfo fillColorField = typeof(Water).GetField("FillColor", BindingFlags.Static | BindingFlags.Public);

        public static FieldInfo surfaceColorField = typeof(Water).GetField("SurfaceColor", BindingFlags.Static | BindingFlags.Public);

        public static FieldInfo rayTopColorField = typeof(Water).GetField("RayTopColor", BindingFlags.Static | BindingFlags.Public);

        public static FieldInfo fillField = typeof(Water).GetField("fill", BindingFlags.Instance | BindingFlags.NonPublic);

        private static FieldInfo Player_dashCooldownTimer = typeof(Player).GetField("dashCooldownTimer", BindingFlags.Instance | BindingFlags.NonPublic);

        private static ILHook wallJumpHook;

        private HashSet<WaterInteraction> contains = new();

        private bool waterIn;

        private bool waterOut;

        public bool[,] grid;

        private EntityID ID;

        private float delay;

        private string color;

        private string poisonedColor;

        private float outsideTransparency;

        private float insideTransparency;

        private float currentTransparency;

        private bool foreground;

        private bool waving;

        private bool rising;

        private bool riseEnd;

        private float riseDelay;

        private int riseDistance;

        private float riseSpeed;

        private bool riseShake;

        private string riseFlag;

        private string riseEndFlag;

        private string appearFlags;

        private string removeFlags;

        private string purifyFlags;

        private int origLevelBottom;

        private bool playerHasMoved;

        private Vector2 StartPos;

        private Vector2 FinalPos;

        private bool riseSound;

        public EventInstance riseSoundSource;

        public Coroutine LiquidDamageRoutine = new();

        public bool FlashingRed;

        public int surfaceHeight;

        public string directory;

        public int customSurfaceHeight;

        private Tween waveTween;

        public bool visualOnly;

        private bool canSwim;

        private bool upsideDown;

        public DisplacementRenderHook Displacement;

        public int group;

        public bool groupLeader;

        private Liquid leader;

        public bool canDrown;

        public float airTimer;

        public bool VariaPreventDying;

        private bool poisoned;

        private float GradientTimer = 1f;

        private bool purified;

        private bool invertPurifyFlags;

        public Liquid(EntityData data, Vector2 position, EntityID eid) : base(data.Position + position)
        {
            Tag = Tags.TransitionUpdate;
            Collider = new Hitbox(data.Width, data.Height, 0f, 0f);
            ID = eid;
            liquidType = data.Attr("liquidType", "acid");
            lowPosition = data.Int("lowPosition");
            delay = data.Float("frameDelay");
            poisoned = data.Bool("poisoned", false);
            color = data.Attr("color");
            poisonedColor = (poisoned && liquidType == "water") ? data.Attr("poisonedColor", "4c9a42") : color;
            outsideTransparency = data.Float("transparency");
            insideTransparency = data.Float("insideTransparency", outsideTransparency);
            foreground = data.Bool("foreground");
            riseDelay = data.Float("riseDelay");
            riseDistance = data.Int("riseDistance");
            riseSpeed = data.Float("riseSpeed");
            riseShake = data.Bool("riseShake");
            riseFlag = data.Attr("riseFlag");
            riseEndFlag = data.Attr("riseEndFlag");
            appearFlags = data.Attr("appearFlags");
            removeFlags = data.Attr("removeFlags");
            purifyFlags = data.Attr("purifyFlags");
            invertPurifyFlags = data.Bool("invertPurifyFlags");
            riseSound = data.Bool("riseSound");
            directory = data.Attr("directory");
            customSurfaceHeight = data.Int("surfaceHeight", 0);
            visualOnly = data.Bool("visualOnly", false);
            canSwim = data.Bool("canSwim", false);
            upsideDown = data.Bool("upsideDown", false);
            group = data.Int("group", -1);
            canDrown = data.Bool("canDrown", false);
            airTimer = data.Float("airTimer", 15f);
            VariaPreventDying = data.Bool("variaPreventDying", false);
            StartPos = Position;
            FinalPos = Position - new Vector2(0, riseDistance);
            if (delay == 0)
            {
                delay = 0.15f;
            }
            if (riseSpeed <= 0)
            {
                riseSpeed = 1;
            }
            if (string.IsNullOrEmpty(color))
            {
                switch (liquidType)
                {
                    case "acid":
                        color = "88C098";
                        break;
                    case "acid_b":
                        color = "88C098";
                        break;
                    case "lava":
                        color = "F85818";
                        break;
                    case "quicksand":
                        color = "C8B078";
                        break;
                    case "water":
                        color = "669CEE";
                        break;
                    default:
                        color = "FFFFFF";
                        break;
                }
            }
            if (outsideTransparency <= 0f)
            {
                outsideTransparency = 0.65f;
            }
            if (outsideTransparency >= 1f)
            {
                outsideTransparency = 1f;
            }
            if (insideTransparency <= 0f || insideTransparency >= outsideTransparency)
            {
                insideTransparency = outsideTransparency;
            }
            Depth = foreground ? -19999 : -9999;
            if (liquidType == "lava")
            {
                Add(sfx = new SoundSource());
                if (data.Width <= 320)
                {
                    sfx.Position = new Vector2(data.Width, data.Height) / 2f;
                }
                else
                {
                    Add(sfx2 = new SoundSource());
                    sfx.Position = new Vector2(data.Width / 4f, data.Height / 2f);
                    sfx2.Position = new Vector2(data.Width / 4f * 3f, data.Height / 2f);
                }

            }
            if (liquidType == "water")
            {
                grid = new bool[(int)(Collider.Width / 8f), (int)(Collider.Height / 8f)];
                Add(Displacement = new DisplacementRenderHook(RenderDisplacement));
            }
            if (customSurfaceHeight == 0)
            {
                if (liquidType == "lava")
                {
                    surfaceHeight = 8;
                }
                else if (liquidType == "water" || liquidType == "quicksand")
                {
                    surfaceHeight = 16;
                }
                else if (liquidType.Contains("acid"))
                {
                    surfaceHeight = 24;
                }
            }
            else
            {
                surfaceHeight = customSurfaceHeight * 8;
            }
            Add(liquidSprite = new Sprite(GFX.Game, (string.IsNullOrEmpty(directory) ? "objects/XaphanHelper/liquid" : directory) + "/" + liquidType + "/"));
            liquidSprite.AddLoop("liquid", "liquid", delay);
            liquidSprite.Play("liquid");
            Add(waterSplashIn = new Sprite(GFX.Game, "objects/XaphanHelper/liquid/water/"));
            waterSplashIn.AddLoop("splash", "splash", 0.04f);
            Add(waterSplashOut = new Sprite(GFX.Game, "objects/XaphanHelper/liquid/water/"));
            waterSplashOut.AddLoop("splash", "splash", 0.04f);
            Add(pc = new PlayerCollider(OnCollide));
            if (upsideDown)
            {
                liquidSprite.FlipY = true;
                waterSplashIn.FlipY = true;
                waterSplashOut.FlipY = true;
            }
        }

        public static void Load()
        {
            On.Celeste.Level.Update += LevelOnUpdate;
            On.Celeste.Player.Update += PlayerOnUpdate;
            IL.Celeste.Player.NormalUpdate += modNormalUpdate;
            IL.Celeste.Player.Jump += modJump;
            IL.Celeste.Player.SuperJump += modSuperJump;
            IL.Celeste.Player.SuperWallJump += modSuperWallJump;
            On.Celeste.Player.ClimbJump += PlayerOnClimbJump;
            IL.Celeste.Player.ClimbUpdate += onPlayerClimbUpdate;
            On.Celeste.Player.SwimCheck += onPlayerSwimCheck;
            On.Celeste.Player.SwimUnderwaterCheck += onPlayerSwimUnderwaterCheck;
            On.Celeste.Player.SwimJumpCheck += onPlayerSwimJumpCheck;
            On.Celeste.Player.SwimRiseCheck += onPlayerSwimRiseCheck;
            On.Celeste.Player._IsOverWater += onPlayerIsOverWater;
            wallJumpHook = new ILHook(typeof(Player).GetMethod("orig_WallJump", BindingFlags.Instance | BindingFlags.NonPublic), modWallJump);
        }

        private static void LevelOnUpdate(On.Celeste.Level.orig_Update orig, Level self)
        {
            if (self.Tracker.GetEntities<Liquid>().Count() > 0 && self.Tracker.GetEntities<BreathManager>().Count() == 0)
            {
                self.Add(new BreathManager());
            }
            orig(self);
        }

        public static void Unload()
        {
            On.Celeste.Level.Update -= LevelOnUpdate;
            On.Celeste.Player.Update -= PlayerOnUpdate;
            IL.Celeste.Player.NormalUpdate -= modNormalUpdate;
            IL.Celeste.Player.Jump -= modJump;
            IL.Celeste.Player.SuperJump -= modSuperJump;
            IL.Celeste.Player.SuperWallJump -= modSuperWallJump;
            On.Celeste.Player.ClimbJump -= PlayerOnClimbJump;
            IL.Celeste.Player.ClimbUpdate -= onPlayerClimbUpdate;
            On.Celeste.Player.SwimCheck -= onPlayerSwimCheck;
            On.Celeste.Player.SwimUnderwaterCheck -= onPlayerSwimUnderwaterCheck;
            On.Celeste.Player.SwimJumpCheck -= onPlayerSwimJumpCheck;
            On.Celeste.Player.SwimRiseCheck -= onPlayerSwimRiseCheck;
            On.Celeste.Player._IsOverWater -= onPlayerIsOverWater;
            if (wallJumpHook != null)
            {
                wallJumpHook.Dispose();
            }
        }

        private static bool onPlayerIsOverWater(On.Celeste.Player.orig__IsOverWater orig, Player self)
        {
            Liquid liquid = null;
            foreach (Liquid liquid2 in self.SceneAs<Level>().Tracker.GetEntities<Liquid>())
            {
                if (self.CollideCheck(liquid2, self.Position))
                {
                    liquid = liquid2;
                    break;
                }
            }
            if (liquid != null && liquid.canSwim)
            {
                Rectangle bounds = self.Collider.Bounds;
                bounds.Height += 2;
                return self.Scene.CollideCheck<Liquid>(bounds);
            }
            return orig(self);
        }

        private static bool onPlayerSwimCheck(On.Celeste.Player.orig_SwimCheck orig, Player self)
        {
            Liquid liquid = null;
            foreach (Liquid liquid2 in self.SceneAs<Level>().Tracker.GetEntities<Liquid>())
            {
                if (self.CollideCheck(liquid2, self.Position))
                {
                    liquid = liquid2;
                    break;
                }
            }
            if (liquid != null && liquid.canSwim && self.CollideCheck(liquid, self.Position + Vector2.UnitY * -8f))
            {
                return self.CollideCheck<Liquid>(self.Position);
            }
            return orig(self);
        }

        private static bool onPlayerSwimUnderwaterCheck(On.Celeste.Player.orig_SwimUnderwaterCheck orig, Player self)
        {
            Liquid liquid = null;
            foreach (Liquid liquid2 in self.SceneAs<Level>().Tracker.GetEntities<Liquid>())
            {
                if (self.CollideCheck(liquid2, self.Position))
                {
                    liquid = liquid2;
                    break;
                }
            }
            if (liquid != null && liquid.canSwim && self.CollideCheck(liquid))
            {
                return self.CollideCheck(liquid, self.Position + Vector2.UnitY * -9f);
            }
            return orig(self);
        }

        private static bool onPlayerSwimJumpCheck(On.Celeste.Player.orig_SwimJumpCheck orig, Player self)
        {
            Liquid liquid = null;
            foreach (Liquid liquid2 in self.SceneAs<Level>().Tracker.GetEntities<Liquid>())
            {
                if (self.CollideCheck(liquid2, self.Position))
                {
                    liquid = liquid2;
                    break;
                }
            }
            if (liquid != null && liquid.canSwim && self.CollideCheck(liquid))
            {
                return !self.CollideCheck(liquid, self.Position + Vector2.UnitY * -14f);
            }
            return orig(self);
        }

        private static bool onPlayerSwimRiseCheck(On.Celeste.Player.orig_SwimRiseCheck orig, Player self)
        {
            Liquid liquid = null;
            foreach (Liquid liquid2 in self.SceneAs<Level>().Tracker.GetEntities<Liquid>())
            {
                if (self.CollideCheck(liquid2, self.Position))
                {
                    liquid = liquid2;
                    break;
                }
            }
            if (liquid != null && liquid.canSwim && self.CollideCheck(liquid))
            {
                return !self.CollideCheck(liquid, self.Position + Vector2.UnitY * -18f);
            }
            return orig(self);
        }


        private static void PlayerOnUpdate(On.Celeste.Player.orig_Update orig, Player self)
        {
            orig.Invoke(self);
            if (Engine.Scene is Level)
            {
                Level level = (Level)Engine.Scene;
                if (determineIfInQuicksand())
                {
                    Player_dashCooldownTimer.SetValue(self, Engine.DeltaTime + 0.1f);
                }
            }
        }

        private static void modNormalUpdate(ILContext il)
        {
            ILCursor cursor = new(il);

            if (cursor.TryGotoNext(MoveType.After, instr => instr.MatchLdcR4(90f)) && cursor.TryGotoNext(MoveType.Before, instr => instr.OpCode == OpCodes.Stloc_S && (((VariableDefinition)instr.Operand).Index == 6 || ((VariableDefinition)instr.Operand).Index == 31)))
            {
                VariableDefinition variable = (VariableDefinition)cursor.Next.Operand;
                if (cursor.TryGotoNext(MoveType.Before, instr => instr.OpCode == OpCodes.Ldflda))
                {
                    cursor.Emit(OpCodes.Pop);
                    cursor.Emit(OpCodes.Ldloc_S, variable);
                    cursor.EmitDelegate<Func<float>>(determineSpeedXFactor);
                    cursor.Emit(OpCodes.Mul);
                    cursor.Emit(OpCodes.Stloc_S, variable);
                    cursor.Emit(OpCodes.Ldarg_0);
                }
            }

            while (cursor.TryGotoNext(MoveType.After, instr => instr.OpCode == OpCodes.Ldc_R4 && ((float)instr.Operand == 160f || (float)instr.Operand == 240f)))
            {
                cursor.EmitDelegate<Func<float>>(determineFallSpeedFactor);
                cursor.Emit(OpCodes.Mul);
            }
        }

        private static void modJump(ILContext il)
        {
            ILCursor cursor = new(il);
            while (cursor.TryGotoNext(MoveType.After, instr => instr.MatchLdcR4(-105f)))
            {
                cursor.EmitDelegate<Func<float>>(determineJumpHeightFactor);
                cursor.Emit(OpCodes.Mul);
            }
        }

        private static void modSuperJump(ILContext il)
        {
            ILCursor cursor = new(il);
            while (cursor.TryGotoNext(MoveType.After, instr => instr.MatchLdcR4(260f)))
            {
                cursor.EmitDelegate<Func<float>>(determineSpeedXFactor);
                cursor.Emit(OpCodes.Mul);
            }
            while (cursor.TryGotoNext(MoveType.After, instr => instr.MatchLdcR4(-105f)))
            {
                cursor.EmitDelegate<Func<float>>(determineJumpHeightFactor);
                cursor.Emit(OpCodes.Mul);
            }
        }

        private static void modWallJump(ILContext il)
        {
            ILCursor cursor = new(il);
            while (cursor.TryGotoNext(MoveType.After, instr => instr.MatchLdcR4(130f)))
            {
                cursor.EmitDelegate<Func<float>>(determineSpeedXFactor);
                cursor.Emit(OpCodes.Mul);
            }
            while (cursor.TryGotoNext(MoveType.After, instr => instr.MatchLdcR4(-105f)))
            {
                cursor.EmitDelegate<Func<float>>(determineJumpHeightFactor);
                cursor.Emit(OpCodes.Mul);
            }
        }

        private static void modSuperWallJump(ILContext il)
        {
            ILCursor cursor = new(il);
            while (cursor.TryGotoNext(MoveType.After, instr => instr.MatchLdcR4(170f)))
            {
                cursor.EmitDelegate<Func<float>>(determineSpeedXFactor);
                cursor.Emit(OpCodes.Mul);
            }
            while (cursor.TryGotoNext(MoveType.After, instr => instr.MatchLdcR4(-160f)))
            {
                cursor.EmitDelegate<Func<float>>(determineJumpHeightFactor);
                cursor.Emit(OpCodes.Mul);
            }
        }

        private static void PlayerOnClimbJump(On.Celeste.Player.orig_ClimbJump orig, Player self)
        {
            if (!determineIfInQuicksand())
            {
                orig(self);
            }
        }

        private static void onPlayerClimbUpdate(ILContext il)
        {
            ILCursor cursor = new(il);

            if (cursor.TryGotoNext(MoveType.After, instr => instr.MatchCallvirt<VirtualButton>("get_Pressed")))
            {
                cursor.Emit(OpCodes.Ldarg_0);
                cursor.Emit(OpCodes.Ldarg_0);
                cursor.Emit(OpCodes.Ldfld, typeof(Player).GetField("moveX", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance));
                cursor.EmitDelegate<Func<bool, Player, int, bool>>(modJumpButtonCheck);
            }

            while (cursor.TryGotoNext(MoveType.After, instr => instr.MatchLdsfld(typeof(Input), "MoveY"), instr => instr.MatchLdfld<VirtualIntegerAxis>("Value")))
            {
                cursor.EmitDelegate<Func<int, int>>(orig =>
                {
                    if (Engine.Scene is Level)
                    {
                        Level level = (Level)Engine.Scene;
                        if (!determineIfInQuicksand())
                        {
                            return orig;
                        }
                    }

                    return 0;
                });
            }
        }

        private static bool modJumpButtonCheck(bool actualValue, Player self, int moveX)
        {
            if (!determineIfInQuicksand())
            {
                // nothing to do
                return actualValue;
            }

            if (moveX == -(int)self.Facing)
            {
                // This will lead to a wall jump. We want to kill climb jumping. So let it go
                return actualValue;
            }

            // let the game believe Jump is not pressed, so it won't return the player to the Normal state (leading to a weird animation / sound effect).
            return false;
        }

        private static float determineJumpHeightFactor()
        {
            if (determineIfInQuicksand())
            {
                return 0.3f;
            }
            return 1f;
        }

        private static float determineSpeedXFactor()
        {
            if (determineIfInQuicksand())
            {
                return 0.15f;
            }
            return 1f;
        }

        public static float determineFallSpeedFactor()
        {
            if (determineIfInQuicksand())
            {
                return 0.1f;
            }
            return 1f;
        }

        public static bool determineIfInQuicksand()
        {
            if (Engine.Scene is Level)
            {
                Level level = (Level)Engine.Scene;
                foreach (Liquid liquid in level.Tracker.GetEntities<Liquid>())
                {
                    if (liquid.PlayerInside() && liquid.liquidType == "quicksand")
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        private void onLastFrameIn(string s)
        {
            waterSplashIn.Stop();
            waterSplashIn.Visible = false;
            waterIn = false;
        }

        private void onLastFrameOut(string s)
        {
            waterSplashOut.Stop();
            waterSplashOut.Visible = false;
            waterOut = false;
        }

        public override void Added(Scene scene)
        {
            base.Added(scene);
            Add(new Coroutine(PoisonedRoutine()));
            if (!string.IsNullOrEmpty(appearFlags))
            {
                string[] flags = appearFlags.Split(',');
                foreach (string flag in flags)
                {
                    if (!SceneAs<Level>().Session.GetFlag(flag))
                    {
                        RemoveSelf();
                        break;
                    }
                }
            }
            if (!string.IsNullOrEmpty(removeFlags))
            {
                string[] flags = removeFlags.Split(',');
                foreach (string flag in flags)
                {
                    if (SceneAs<Level>().Session.GetFlag(flag))
                    {
                        RemoveSelf();
                        break;
                    }
                }
            }
            liquidSprite.Color = waterSplashIn.Color = waterSplashOut.Color = Utils.GetGradientColor(Calc.HexToColor(color), Calc.HexToColor(poisonedColor), GradientTimer) * (PlayerCompletelyInside() ? insideTransparency : outsideTransparency);
            origLevelBottom = SceneAs<Level>().Bounds.Bottom;
            if (liquidType == "lava")
            {
                sfx.Play("event:/env/local/09_core/lavagate_idle");
                if (Width > 320)
                {
                    sfx2.Play("event:/env/local/09_core/lavagate_idle");
                }
            }
            if (liquidType == "water")
            {
                CheckSolidsForDisplacement();
            }
            if (!string.IsNullOrEmpty(riseEndFlag) && SceneAs<Level>().Session.GetFlag(riseEndFlag))
            {
                Position -= new Vector2(0, riseDistance);
                Collider = new Hitbox(Width, (origLevelBottom - Position.Y), 0f, 0f);
                if (Position.Y > SceneAs<Level>().Bounds.Bottom)
                {
                    RemoveSelf();
                }
            }
            if (group != -1)
            {
                groupLeader = true;
                foreach (Liquid liquid in SceneAs<Level>().Tracker.GetEntities<Liquid>())
                {
                    if (liquid != this && liquid.group == group)
                    {
                        groupLeader = false;
                    }
                }
            }
            purified = CheckIfPurified();
        }

        private bool CheckIfPurified()
        {
            string[] flags = purifyFlags.Split(',');
            bool purified = true;
            foreach (string flag in flags)
            {
                if (invertPurifyFlags ? SceneAs<Level>().Session.GetFlag(flag) : !SceneAs<Level>().Session.GetFlag(flag))
                {
                    purified = false;
                    break;
                }
            }
            return purified;
        }

        public IEnumerator PoisonedRoutine()
        {
            if (!string.IsNullOrEmpty(purifyFlags))
            {
                bool skip = false;
                if (purified)
                {
                    GradientTimer = 0f;
                }
                else
                {
                    while (!CheckIfPurified())
                    {
                        yield return null;
                    }
                    while (GradientTimer > 0f)
                    {
                        GradientTimer -= Engine.DeltaTime;
                        yield return null;
                        if (GradientTimer <= 0.5f)
                        {
                            purified = true;
                        }
                        if (SceneAs<Level>().Transitioning || !CheckIfPurified())
                        {
                            skip = true;
                            break;
                        }
                    }
                    if (!skip)
                    {
                        GradientTimer = 0f;
                    }
                }

                while (CheckIfPurified())
                {
                    yield return null;
                }
                skip = false;
                while (GradientTimer < 1f)
                {
                    GradientTimer += Engine.DeltaTime;
                    yield return null;
                    if (GradientTimer >= 0.5f)
                    {
                        purified = false;
                    }
                    if (SceneAs<Level>().Transitioning || CheckIfPurified())
                    {
                        skip = true;
                        break;
                    }
                }
                if (!skip)
                {
                    GradientTimer = 1f;
                }
                Add(new Coroutine(PoisonedRoutine()));
            }
        }

        public override void Awake(Scene scene)
        {
            base.Awake(scene);
            if (!groupLeader)
            {
                foreach (Liquid liquid in SceneAs<Level>().Tracker.GetEntities<Liquid>())
                {
                    if (liquid.group == group && liquid.groupLeader)
                    {
                        leader = liquid;
                        break;
                    }
                }
            }
            Player player = SceneAs<Level>().Tracker.GetEntity<Player>();
            if (player != null)
            {
                if (!PlayerCompletelyInside(true))
                {
                    currentTransparency = outsideTransparency;
                }
                else
                {
                    if (group == -1 || groupLeader)
                    {
                        currentTransparency = insideTransparency;
                    }
                    else
                    {
                        foreach (Liquid liquid in SceneAs<Level>().Tracker.GetEntities<Liquid>())
                        {
                            if (leader != null && liquid.group == group)
                            {
                                currentTransparency = leader.currentTransparency;
                            }
                        }
                    }
                }
            }
        }

        private IEnumerator ShakeLevel()
        {
            while (SceneAs<Level>().Transitioning)
            {
                yield return null;
            }
            while (!SceneAs<Level>().Paused && rising)
            {
                SceneAs<Level>().DirectionalShake(new Vector2(0.5f, 0), 0.05f);
                Input.Rumble(RumbleStrength.Light, RumbleLength.Short);
                yield return 0.05f;
            }
        }

        public IEnumerator RiseRoutine()
        {
            if (Position != FinalPos)
            {
                if (riseShake)
                {
                    Add(new Coroutine(ShakeLevel()));
                }
                while (SceneAs<Level>().Transitioning)
                {
                    yield return null;
                }
                if (riseSound)
                {
                    riseSoundSource = Audio.Play("event:/game/xaphan/liquid_rise");
                }
                while (riseDelay > 0)
                {
                    riseDelay -= Engine.DeltaTime;
                    yield return null;
                }
                Player player = SceneAs<Level>().Tracker.GetEntity<Player>();
                Vector2 start = Position;
                float coef = riseSpeed / 8;
                float num = Vector2.Distance(start, FinalPos) / 4f / coef;
                Tween tween = Tween.Create(Tween.TweenMode.Oneshot, Ease.SineInOut, num, start: true);
                tween.OnUpdate = delegate (Tween t)
                {
                    if (player != null)
                    {
                        Position = Vector2.Lerp(start, FinalPos, t.Eased);
                    }
                };
                Add(tween);
                while (Position != FinalPos)
                {
                    if (liquidType == "water" && Displacement != null)
                    {
                        Displacement.RemoveSelf();
                        grid = new bool[(int)(Collider.Width / 8f), (int)(Collider.Height / 8f)];
                        CheckSolidsForDisplacement();
                        Add(Displacement = new DisplacementRenderHook(RenderDisplacement));
                    }
                    if (SceneAs<Level>().Transitioning)
                    {
                        tween.Stop();
                        stopRiseSound = true;
                        rising = false;
                        riseEnd = true;
                        break;
                    }
                    yield return null;
                }
                if (riseSoundSource != null)
                {
                    riseSoundSource.stop(STOP_MODE.ALLOWFADEOUT);
                }
                rising = false;
                riseEnd = true;
            }
        }

        public void ReturnToOrigPosition()
        {
            Add(new Coroutine(ReverseRiseRoutine()));
        }

        public IEnumerator ReverseRiseRoutine()
        {
            rising = true;
            Player player = SceneAs<Level>().Tracker.GetEntity<Player>();
            Vector2 start = Position;
            float coef = riseSpeed * 10 / 8;
            float num = Vector2.Distance(start, StartPos) / 4f / coef;
            Tween tween = Tween.Create(Tween.TweenMode.Oneshot, Ease.SineInOut, num, start: true);
            tween.OnUpdate = delegate (Tween t)
            {
                if (player != null)
                {
                    Position = Vector2.Lerp(start, StartPos, t.Eased);
                }
            };
            Add(tween);
            while (Position != StartPos)
            {
                if (liquidType == "water" && Displacement != null)
                {
                    Displacement.RemoveSelf();
                    grid = new bool[(int)(Collider.Width / 8f), (int)(Collider.Height / 8f)];
                    CheckSolidsForDisplacement();
                    Add(Displacement = new DisplacementRenderHook(RenderDisplacement));
                }
                if (SceneAs<Level>().Transitioning)
                {
                    tween.Stop();
                    stopRiseSound = true;
                    rising = false;
                    riseEnd = true;
                    break;
                }
                yield return null;
            }
            if (riseSoundSource != null)
            {
                riseSoundSource.stop(STOP_MODE.ALLOWFADEOUT);
            }
            rising = false;
            riseEnd = true;
        }

        public IEnumerator LevelBottomDelay()
        {
            float timer = 1.5f;
            while (timer > 0)
            {
                timer -= Engine.DeltaTime;
                yield return null;
            }
            if (riseSoundSource != null)
            {
                riseSoundSource.stop(STOP_MODE.ALLOWFADEOUT);
            }
            RemoveSelf();
        }

        public bool stopRiseSound;

        public override void Removed(Scene scene)
        {
            base.Removed(scene);
            if (riseSoundSource != null)
            {
                riseSoundSource.stop(STOP_MODE.ALLOWFADEOUT);
            }
        }

        public override void Update()
        {
            base.Update();
            Player player = SceneAs<Level>().Tracker.GetEntity<Player>();
            if (!SceneAs<Level>().Transitioning)
            {
                if (player != null)
                {
                    /*if (PlayerInside())
                    {
                        OnCollide(player);
                    }*/
                    if (!PlayerCompletelyInside())
                    {
                        currentTransparency = Calc.Approach(currentTransparency, outsideTransparency, Engine.DeltaTime * 2f);
                    }
                    else
                    {
                        if (group == -1 || groupLeader)
                        {
                            currentTransparency = Calc.Approach(currentTransparency, insideTransparency, Engine.DeltaTime * 2f);
                        }
                        else
                        {
                            foreach (Liquid liquid in SceneAs<Level>().Tracker.GetEntities<Liquid>())
                            {
                                if (leader != null && liquid.group == group)
                                {
                                    currentTransparency = leader.currentTransparency;
                                }
                            }
                        }
                    }
                }
            }
            liquidSprite.Color = waterSplashIn.Color = waterSplashOut.Color = Utils.GetGradientColor(Calc.HexToColor(color), Calc.HexToColor(poisonedColor), GradientTimer * 100) * currentTransparency;
            if ((liquidType == "lava" && GravityJacket.determineIfInLava() && !GravityJacket.Active(SceneAs<Level>())) || (liquidType.Contains("acid") && GravityJacket.determineIfInAcid()))
            {
                FlashingRed = true;
            }
            else
            {
                if (FlashingRed)
                {
                    FlashingRed = false;
                }
            }
            if (stopRiseSound)
            {
                if (riseSoundSource != null)
                {
                    riseSoundSource.stop(STOP_MODE.ALLOWFADEOUT);
                }
            }
            if (Top > SceneAs<Level>().Bounds.Bottom + (liquidType == "lava" ? 8 : 0) && riseDistance < 0)
            {
                Visible = false;
                Add(new Coroutine(LevelBottomDelay()));
                return;
            }
            if (!playerHasMoved && player != null && player.Speed != Vector2.Zero)
            {
                playerHasMoved = true;
            }
            if (rising)
            {
                Collider = new Hitbox(Width, (origLevelBottom - Position.Y), 0f, 0f);
            }
            if (playerHasMoved && !riseEnd && !rising && riseDistance != 0 && (string.IsNullOrEmpty(riseFlag) || SceneAs<Level>().Session.GetFlag(riseFlag)) && (string.IsNullOrEmpty(riseEndFlag) || !SceneAs<Level>().Session.GetFlag(riseEndFlag) || riseEndFlag == riseFlag))
            {
                rising = true;
                Add(new Coroutine(RiseRoutine()));
            }
            else if (!rising && !waving && lowPosition != 0)
            {
                waving = true;
                Vector2 start = Position;
                Vector2 end = Position + new Vector2(0, lowPosition);
                float coef = lowPosition / 8;
                float num = Vector2.Distance(start, end) / 4f / coef;
                waveTween = Tween.Create(Tween.TweenMode.YoyoLooping, Ease.SineInOut, num, start: true);
                waveTween.OnUpdate = delegate (Tween t)
                {
                    Position = Vector2.Lerp(start, end, t.Eased);
                };
                Add(waveTween);
            }
            if (rising)
            {
                if (waveTween != null)
                {
                    waving = false;
                    waveTween.Stop();
                }
            }
            if (liquidType == "water")
            {
                if (player != null && player.Left < Right && player.Right > Left)
                {
                    if ((upsideDown ? (player.Top <= Bottom + 1f && player.Top >= Bottom - 1f && player.Speed.Y < 0) : (player.Bottom >= Top - 1f && player.Bottom <= Top + 1f && player.Speed.Y > 0)) && !waterIn)
                    {
                        waterIn = true;
                        PlaySplashIn(new Vector2(player.Position.X - 12f, (upsideDown ? Bottom - 20f : Top - 21f)));
                    }
                    if ((upsideDown ? (player.Top <= Bottom + 1f && player.Top >= Bottom - 1f && player.Speed.Y > 0) : (player.Bottom >= Top - 1f && player.Bottom <= Top + 1f && player.Speed.Y < 0)) && !waterOut)
                    {
                        waterOut = true;
                        waterSplashOut.RenderPosition = new Vector2(player.Position.X - 12f, (upsideDown ? Bottom - 20f : Top - 21f));
                        waterSplashOut.Play("splash", restart: true);
                        waterSplashOut.OnLastFrame = onLastFrameOut;
                    }
                }
                List<Component> components = Scene.Tracker.GetComponents<WaterInteraction>();
                foreach (WaterInteraction item in components)
                {
                    Rectangle bounds = item.Bounds;
                    bool flag = contains.Contains(item);
                    bool flag2 = CollideRect(bounds);
                    if (flag != flag2)
                    {
                        bool flag3 = item.IsDashing();
                        int num = (bounds.Center.Y < Center.Y && !Scene.CollideCheck<Solid>(bounds)) ? 1 : 0;
                        if (flag)
                        {
                            if (flag3)
                            {
                                Audio.Play("event:/char/madeline/water_dash_out", bounds.Center.ToVector2(), "deep", num);
                            }
                            else if (!SceneAs<Level>().Transitioning && player != null && player.Bottom >= Top)
                            {
                                Audio.Play("event:/char/madeline/water_out", bounds.Center.ToVector2(), "deep", num);
                            }
                            item.DrippingTimer = 2f;
                        }
                        else
                        {
                            if (flag3 && num == 1)
                            {
                                Audio.Play("event:/char/madeline/water_dash_in", bounds.Center.ToVector2(), "deep", num);
                            }
                            else if (!SceneAs<Level>().Transitioning && player != null && player.Top < Top)
                            {
                                Audio.Play("event:/char/madeline/water_in", bounds.Center.ToVector2(), "deep", num);
                            }
                            item.DrippingTimer = 0f;
                        }
                        if (flag)
                        {
                            contains.Remove(item);
                        }
                        else
                        {
                            contains.Add(item);
                        }
                    }
                }
            }
            if (liquidType == "quicksand" && player != null && !XaphanModule.useMetroidGameplay)
            {
                if (player.Speed.Y >= 10 && PlayerInside())
                {
                    player.Speed.Y = 10;
                }
                if (player.Speed.X >= 13.5f && PlayerInside())
                {
                    player.Speed.X = 13.5f;
                }
                if (player.Speed.X <= -13.5f && PlayerInside())
                {
                    player.Speed.X = -13.5f;
                }
                if (PlayerInside() && player.Top >= Top + 6)
                {
                    player.Die(Vector2.Zero);
                }
            }
        }

        public void PlaySplashIn(Vector2 position)
        {
            waterSplashIn.RenderPosition = position;
            waterSplashIn.Play("splash", restart: true);
            waterSplashIn.OnLastFrame = onLastFrameIn;
        }

        public override void Render()
        {
            if (liquidType == "water")
            {
                if (waterSplashIn != null)
                {
                    waterSplashIn.Render();
                }
                if (waterSplashOut != null)
                {
                    waterSplashOut.Render();
                }
            }
            if (upsideDown)
            {
                for (int i = 0; i < Width / liquidSprite.Width; i++)
                {
                    liquidSprite.RenderPosition = Position + new Vector2(i * liquidSprite.Width, Height - liquidSprite.Height + 8);
                    if (CullHelper.IsRectangleVisible(liquidSprite.RenderPosition.X, liquidSprite.RenderPosition.Y, liquidSprite.Width, liquidSprite.Height))
                    {
                        if ((i + 1) * liquidSprite.Width <= Width && liquidSprite.Height <= Height)
                        {
                            liquidSprite.Render();
                        }
                        else if ((i + 1) * liquidSprite.Width > Width && liquidSprite.Height <= Height)
                        {
                            liquidSprite.DrawSubrect(Vector2.Zero, new Rectangle(0, 0, (int)liquidSprite.Width - ((i + 1) * (int)liquidSprite.Width - (int)Width), (int)liquidSprite.Height));
                        }
                        else if ((i + 1) * liquidSprite.Width <= Width && liquidSprite.Height > Height)
                        {
                            liquidSprite.DrawSubrect(new Vector2(0, liquidSprite.Height - Height - 8), new Rectangle(0, 0, (int)liquidSprite.Width, (int)liquidSprite.Height - ((int)liquidSprite.Height - (int)Height - 8)));
                        }
                        else
                        {
                            liquidSprite.DrawSubrect(new Vector2(0, liquidSprite.Height - Height - 8), new Rectangle(0, 0, (int)liquidSprite.Width - ((i + 1) * (int)liquidSprite.Width - (int)Width), (int)liquidSprite.Height - ((int)liquidSprite.Height - (int)Height - 8)));
                        }
                    }
                }
                int totalLines = (int)(liquidSprite.Height - surfaceHeight) / 8;
                for (int i = 0; i < Width / liquidSprite.Width; i++)
                {
                    for (int j = 0; j < (Height + 8 - liquidSprite.Height) / 8; j++)
                    {
                        liquidSprite.RenderPosition = Position + new Vector2(i * liquidSprite.Width, (Height + 8 - liquidSprite.Height) - j * 8 - 8);
                        if (CullHelper.IsRectangleVisible(liquidSprite.RenderPosition.X, liquidSprite.RenderPosition.Y, liquidSprite.Width, liquidSprite.Height))
                        {
                            Math.DivRem(j, totalLines, out int Variation);
                            if ((i + 1) * liquidSprite.Width <= Width)
                            {
                                liquidSprite.DrawSubrect(Vector2.Zero, new Rectangle(0, surfaceHeight + 8 * Variation, (int)liquidSprite.Width, 8));
                            }
                            else
                            {
                                liquidSprite.DrawSubrect(Vector2.Zero, new Rectangle(0, surfaceHeight + 8 * Variation, (int)liquidSprite.Width - ((i + 1) * (int)liquidSprite.Width - (int)Width), 8));
                            }
                        }
                    }
                }
            }
            else
            {
                for (int i = 0; i < Width / liquidSprite.Width; i++)
                {
                    liquidSprite.RenderPosition = Position + new Vector2(i * liquidSprite.Width, -8f);
                    if (CullHelper.IsRectangleVisible(liquidSprite.RenderPosition.X, liquidSprite.RenderPosition.Y, liquidSprite.Width, liquidSprite.Height))
                    {
                        if ((i + 1) * liquidSprite.Width <= Width && liquidSprite.Height <= Height)
                        {
                            liquidSprite.Render();
                        }
                        else if ((i + 1) * liquidSprite.Width > Width && liquidSprite.Height <= Height)
                        {
                            liquidSprite.DrawSubrect(Vector2.Zero, new Rectangle(0, 0, (int)liquidSprite.Width - ((i + 1) * (int)liquidSprite.Width - (int)Width), (int)liquidSprite.Height));
                        }
                        else if ((i + 1) * liquidSprite.Width <= Width && liquidSprite.Height > Height)
                        {
                            liquidSprite.DrawSubrect(Vector2.Zero, new Rectangle(0, 0, (int)liquidSprite.Width, (int)liquidSprite.Height - ((int)liquidSprite.Height - (int)Height - 8)));
                        }
                        else
                        {
                            liquidSprite.DrawSubrect(Vector2.Zero, new Rectangle(0, 0, (int)liquidSprite.Width - ((i + 1) * (int)liquidSprite.Width - (int)Width), (int)liquidSprite.Height - ((int)liquidSprite.Height - (int)Height - 8)));
                        }
                    }
                }
                for (int i = 0; i < Width / liquidSprite.Width; i++)
                {
                    for (int j = 0; j < (Height + 8 - liquidSprite.Height) / (liquidSprite.Height - surfaceHeight); j++)
                    {
                        liquidSprite.RenderPosition = Position + new Vector2(i * liquidSprite.Width, liquidSprite.Height - 8 + j * (liquidSprite.Height - surfaceHeight));
                        if (CullHelper.IsRectangleVisible(liquidSprite.RenderPosition.X, liquidSprite.RenderPosition.Y, liquidSprite.Width, liquidSprite.Height))
                        {
                            if ((i + 1) * liquidSprite.Width <= Width && liquidSprite.Height - 8 + j * (liquidSprite.Height - surfaceHeight) <= (Height + 8 - liquidSprite.Height))
                            {
                                liquidSprite.DrawSubrect(Vector2.Zero, new Rectangle(0, surfaceHeight, (int)liquidSprite.Width, (int)liquidSprite.Height - surfaceHeight));
                            }
                            else if ((i + 1) * liquidSprite.Width > Width && liquidSprite.Height - 8 + j * (liquidSprite.Height - surfaceHeight) <= (Height + 8 - liquidSprite.Height))
                            {
                                liquidSprite.DrawSubrect(Vector2.Zero, new Rectangle(0, surfaceHeight, (int)liquidSprite.Width - ((i + 1) * (int)liquidSprite.Width - (int)Width), (int)liquidSprite.Height - surfaceHeight));
                            }
                            else if ((i + 1) * liquidSprite.Width <= Width && liquidSprite.Height - 8 + j * (liquidSprite.Height - surfaceHeight) > (Height + 8 - liquidSprite.Height))
                            {
                                liquidSprite.DrawSubrect(Vector2.Zero, new Rectangle(0, surfaceHeight, (int)liquidSprite.Width, (int)(liquidSprite.Height - surfaceHeight) - ((int)(liquidSprite.Height - surfaceHeight + j * (liquidSprite.Height - surfaceHeight)) - (int)(Height + 8 - liquidSprite.Height))));
                            }
                            else
                            {
                                liquidSprite.DrawSubrect(Vector2.Zero, new Rectangle(0, surfaceHeight, (int)liquidSprite.Width - ((i + 1) * (int)liquidSprite.Width - (int)Width), (int)(liquidSprite.Height - surfaceHeight) - ((int)(liquidSprite.Height - surfaceHeight + j * (liquidSprite.Height - surfaceHeight)) - (int)(Height + 8 - liquidSprite.Height))));
                            }
                        }
                    }
                }
            }
        }

        public void CheckSolidsForDisplacement()
        {
            int i = 0;
            for (int length = grid.GetLength(0); i < length; i++)
            {
                int j = 0;
                for (int length2 = grid.GetLength(1); j < length2; j++)
                {
                    if (foreground)
                    {
                        grid[i, j] = true;
                    }
                    else
                    {
                        grid[i, j] = !Scene.CollideCheck<Solid>(new Rectangle((int)X + i * 8, (int)Y + j * 8, 8, 8));
                    }
                }
            }
        }

        public void RenderDisplacement()
        {
            Color color = new(0.5f, 0.5f, 0.25f, 1f);
            int i = 0;
            int length = grid.GetLength(0);
            int length2 = grid.GetLength(1);
            for (; i < length; i++)
            {
                if (length2 > 0 && grid[i, 0])
                {
                    Draw.Rect(X + (i * 8), Y + 3f, 8f, 5f, color);
                }
                for (int j = 1; j < length2; j++)
                {
                    if (grid[i, j])
                    {
                        int k;
                        for (k = 1; j + k < length2 && grid[i, j + k]; k++)
                        {
                        }
                        Draw.Rect(X + (i * 8), Y + (j * 8), 8f, k * 8, color);
                        j += k - 1;
                    }
                }
            }
        }

        public bool PlayerInside()
        {
            if (!visualOnly)
            {
                foreach (Player player in SceneAs<Level>().Tracker.GetEntities<Player>())
                {
                    if (CollideCheck(player) && (player.Bottom >= Top + 3f))
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        public bool PlayerCompletelyInside(bool duringTransition = false)
        {
            foreach (Player player in SceneAs<Level>().Tracker.GetEntities<Player>())
            {
                if (group == -1)
                {
                    if (CollideCheck(player) && (upsideDown ? player.Bottom <= Bottom - 4 + (duringTransition ? 12 : 0) : player.Top >= Top + 4 - (duringTransition ? 12 : 0)) && player.Left >= Left - (duringTransition ? 8 : 0) && player.Right <= Right + (duringTransition ? 8 : 0))
                    {
                        return true;
                    }
                }
                else
                {
                    foreach (Liquid liquid in SceneAs<Level>().Tracker.GetEntities<Liquid>())
                    {
                        if (liquid.CollideCheck(player) && (liquid.upsideDown ? player.Bottom <= liquid.Bottom - 4 + (duringTransition ? 12 : 0) : player.Top >= liquid.Top + 4 - (duringTransition ? 12 : 0)))
                        {
                            return true;
                        }
                    }

                }
            }
            return false;
        }

        private void OnCollide(Player player)
        {
            if (!visualOnly)
            {
                if (liquidType == "water")
                {
                    if (XaphanModule.PlayerIsControllingRemoteDrone())
                    {
                        Drone drone = SceneAs<Level>().Tracker.GetEntity<Drone>();
                        if (drone != null && !drone.dead && player != drone.FakePlayer)
                        {
                            Add(new Coroutine(drone.Destroy()));
                        }
                    }
                    else if (poisoned && !purified)
                    {
                        player.Die(new Vector2(0f, -1f));
                    }
                }
                if (liquidType == "lava")
                {
                    if (!XaphanModule.useUpgrades)
                    {
                        player.Die(new Vector2(0f, -1f));
                    }
                    else
                    {
                        if (!XaphanModule.useMetroidGameplay ? (VariaJacket.Active(SceneAs<Level>()) ? (!VariaPreventDying && !player.JustRespawned) : !player.JustRespawned) : !GravityJacket.Active(SceneAs<Level>()))
                        {
                            if (!XaphanModule.PlayerIsControllingRemoteDrone())
                            {
                                if (!XaphanModule.UIOpened)
                                {
                                    if (!XaphanModule.useMetroidGameplay)
                                    {
                                        player.Die(new Vector2(0f, -1f));
                                    }
                                    else
                                    {
                                        if (player != null && player.CanRetry && !GravityJacket.Active(SceneAs<Level>()) && !LiquidDamageRoutine.Active)
                                        {
                                            Add(LiquidDamageRoutine = new Coroutine(LiquidDamage(liquidType)));
                                        }
                                    }
                                }
                            }
                            /*else -- Kill the drone in dummy state ??? Why ?
                            {
                                Drone drone = SceneAs<Level>().Tracker.GetEntity<Drone>();
                                if (player.StateMachine.State == 11 && drone != null && !drone.dead)
                                {
                                    player.Die(new Vector2(0f, -1f));
                                    Add(new Coroutine(drone.Destroy(true, true)));
                                }
                            }*/
                        }
                    }
                }
                if (liquidType.Contains("acid"))
                {
                    if (XaphanModule.useUpgrades && XaphanModule.PlayerIsControllingRemoteDrone())
                    {
                        Drone drone = SceneAs<Level>().Tracker.GetEntity<Drone>();
                        if (player.StateMachine.State == 11 && drone != null && !drone.dead)
                        {
                            player.Die(new Vector2(0f, -1f));
                            Add(new Coroutine(drone.Destroy(true, true)));
                        }
                        else if (drone != null && !drone.dead)
                        {
                            Add(new Coroutine(drone.Destroy()));
                        }
                    }
                    else
                    {
                        if (!XaphanModule.useMetroidGameplay)
                        {
                            if (VariaJacket.Active(SceneAs<Level>()))
                            {
                                if (!VariaPreventDying && !player.JustRespawned)
                                {
                                    player.Die(new Vector2(0f, -1f));
                                }
                            }
                            else
                            {
                                if (!player.JustRespawned)
                                {
                                    player.Die(new Vector2(0f, -1f));
                                }
                            }
                        }
                        else
                        {
                            if (player != null && player.CanRetry && !LiquidDamageRoutine.Active)
                            {
                                Add(LiquidDamageRoutine = new Coroutine(LiquidDamage(liquidType)));
                            }
                        }
                    }
                }
                if (liquidType == "quicksand")
                {
                    if (XaphanModule.PlayerIsControllingRemoteDrone())
                    {
                        Drone drone = SceneAs<Level>().Tracker.GetEntity<Drone>();
                        if (drone != null && !drone.dead && player != drone.FakePlayer)
                        {
                            Add(new Coroutine(drone.Destroy()));
                        }
                    }
                }
            }
        }

        private IEnumerator LiquidDamage(string liquidType)
        {
            HealthDisplay healthDisplay = SceneAs<Level>().Tracker.GetEntity<HealthDisplay>();
            HeatManager manager = SceneAs<Level>().Tracker.GetEntity<HeatManager>();
            if (manager == null || (manager != null && !manager.HeatDamageRoutine.Active))
            {
                healthDisplay.playDamageSfx();
            }
            while (healthDisplay != null && healthDisplay.CurrentHealth > 0 && !SceneAs<Level>().Transitioning && !SceneAs<Level>().FrozenOrPaused && !XaphanModule.UIOpened && GravityJacket.determineIfInLiquid())
            {
                healthDisplay.CurrentHealth -= 1;
                healthDisplay.GetEnergyTanks();
                float tickTimer = (0.066f / (liquidType == "lava" ? 2 : 6)) * (VariaJacket.Active(SceneAs<Level>()) && GravityJacket.Active(SceneAs<Level>()) ? 4 : VariaJacket.Active(SceneAs<Level>()) ? 2 : 1);
                while (tickTimer > 0)
                {
                    tickTimer -= Engine.DeltaTime;
                    yield return null;
                }
            }
            if (manager == null || (manager != null && !manager.HeatDamageRoutine.Active))
            {
                healthDisplay.stopDamageSfx();
            }
        }
    }
}
