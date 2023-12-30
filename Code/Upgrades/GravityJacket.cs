using System;
using System.Collections;
using System.Reflection;
using Celeste.Mod.XaphanHelper.Entities;
using Mono.Cecil.Cil;
using Monocle;
using MonoMod.Cil;
using MonoMod.RuntimeDetour;
using MonoMod.Utils;

namespace Celeste.Mod.XaphanHelper.Upgrades
{
    class GravityJacket : Upgrade
    {
        private ILHook wallJumpHook;

        private ILHook dashCoroutineHookForTimer;

        private ILHook dashCoroutineHookForCounter;

        private FieldInfo LookoutAnimPrefix = typeof(Lookout).GetField("animPrefix", BindingFlags.Instance | BindingFlags.NonPublic);

        public override int GetDefaultValue()
        {
            return 0;
        }

        public override int GetValue()
        {
            return XaphanModule.ModSettings.GravityJacket ? 1 : 0;
        }

        public override void SetValue(int value)
        {
            XaphanModule.ModSettings.GravityJacket = (value != 0);
        }

        public override void Load()
        {
            IL.Celeste.Player.NormalUpdate += modNormalUpdate;
            IL.Celeste.Player.Jump += modJump;
            IL.Celeste.Player.SuperJump += modSuperJump;
            IL.Celeste.Player.SuperWallJump += modSuperWallJump;
            IL.Celeste.Player.DashBegin += modDashLength;
            IL.Celeste.Player.ClimbUpdate += modClimbUpdate;
            wallJumpHook = new ILHook(typeof(Player).GetMethod("orig_WallJump", BindingFlags.Instance | BindingFlags.NonPublic), modWallJump);
            MethodInfo dashCoroutine = typeof(Player).GetMethod("DashCoroutine", BindingFlags.NonPublic | BindingFlags.Instance).GetStateMachineTarget();
            dashCoroutineHookForTimer = new ILHook(dashCoroutine, modDashLength);
            dashCoroutineHookForCounter = new ILHook(dashCoroutine, modDashTrailCounter);
            On.Celeste.Lookout.LookRoutine += modLookoutLookRoutine;
        }

        public override void Unload()
        {
            IL.Celeste.Player.NormalUpdate -= modNormalUpdate;
            IL.Celeste.Player.Jump -= modJump;
            IL.Celeste.Player.SuperJump -= modSuperJump;
            IL.Celeste.Player.SuperWallJump -= modSuperWallJump;
            IL.Celeste.Player.DashBegin -= modDashLength;
            IL.Celeste.Player.ClimbUpdate -= modClimbUpdate;
            if (wallJumpHook != null)
            {
                wallJumpHook.Dispose();
            }
            if (dashCoroutineHookForTimer != null)
            {
                dashCoroutineHookForTimer.Dispose();
            }
            if (dashCoroutineHookForCounter != null)
            {
                dashCoroutineHookForCounter.Dispose();
            }
            On.Celeste.Lookout.LookRoutine -= modLookoutLookRoutine;
        }

        public static bool Active(Level level)
        {
            return XaphanModule.ModSettings.GravityJacket && !(XaphanModule.Instance._SaveData as XaphanModuleSaveData).GravityJacketInactive.Contains(level.Session.Area.LevelSet);
        }

        private void modNormalUpdate(ILContext il)
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
        }

        private void modJump(ILContext il)
        {
            ILCursor cursor = new(il);
            while (cursor.TryGotoNext(MoveType.After, instr => instr.MatchLdcR4(-105f)))
            {
                cursor.EmitDelegate<Func<float>>(determineJumpHeightFactor);
                cursor.Emit(OpCodes.Mul);
            }
        }

        private void modSuperJump(ILContext il)
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

        private void modWallJump(ILContext il)
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

        private void modSuperWallJump(ILContext il)
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

        private void modDashLength(ILContext il)
        {
            ILCursor cursor = new(il);
            while (cursor.TryGotoNext(MoveType.After, instr => instr.MatchLdcR4(0.3f) || instr.MatchLdcR4(0.15f)))
            {
                cursor.EmitDelegate<Func<float>>(determineDashLengthFactor);
                cursor.Emit(OpCodes.Mul);
            }
        }

        private void modClimbUpdate(ILContext il)
        {
            ILCursor cursor = new(il);
            while (cursor.TryGotoNext(MoveType.After, instr => instr.MatchLdcR4(45.4545441f)))
            {
                cursor.EmitDelegate<Func<float>>(determineStaminaUpCostFactor);
                cursor.Emit(OpCodes.Mul);
            }
            while (cursor.TryGotoNext(MoveType.After, instr => instr.MatchLdcR4(10f)))
            {
                cursor.EmitDelegate<Func<float>>(determineStaminaIdleCostFactor);
                cursor.Emit(OpCodes.Mul);
            }
        }

        private float determineStaminaUpCostFactor()
        {
            if (Engine.Scene is Level)
            {
                Level level = (Level)Engine.Scene;
                if (determineIfInLiquid() && (!Active(level) || XaphanModule.PlayerIsControllingRemoteDrone()))
                {
                    return 2f;
                }
            }
            return 1f;
        }

        private float determineStaminaIdleCostFactor()
        {
            if (Engine.Scene is Level)
            {
                Level level = (Level)Engine.Scene;
                if (determineIfInLiquid() && (!Active(level) || XaphanModule.PlayerIsControllingRemoteDrone()))
                {
                    return 2f;
                }
            }
            return 1f;
        }

        private void modDashTrailCounter(ILContext il)
        {
            ILCursor cursor = new(il);
            while (cursor.TryGotoNext(instr => instr.MatchStfld<Player>("dashTrailCounter")))
            {
                cursor.EmitDelegate<Func<int, int>>(applyDashTrailCounter);
                cursor.Index++;
            }
        }

        private IEnumerator modLookoutLookRoutine(On.Celeste.Lookout.orig_LookRoutine orig, Lookout self, Player player)
        {
            if (Active(player.SceneAs<Level>()))
            {
                LookoutAnimPrefix.SetValue(self, "gravity_");
            }
            IEnumerator origEnum = orig(self, player);
            while (origEnum.MoveNext()) yield return origEnum.Current;
        }

        private int applyDashTrailCounter(int dashTrailCounter)
        {
            if (Engine.Scene is Level)
            {
                Level level = (Level)Engine.Scene;
                if (determineIfInLiquid() && (!Active(level) || XaphanModule.PlayerIsControllingRemoteDrone()))
                {
                    float lastDashDuration = SaveData.Instance.Assists.SuperDashing ? 0.3f : 0.15f;
                    return (int)Math.Round(lastDashDuration * 4) - 1;
                }
            }
            return dashTrailCounter;
        }

        public static float determineJumpHeightFactor()
        {
            if (Engine.Scene is Level)
            {
                Level level = (Level)Engine.Scene;
                if (determineIfInLiquid() && (!Active(level) || XaphanModule.PlayerIsControllingRemoteDrone()))
                {
                    return 0.7f;
                }
            }
            return 1f;
        }

        public static float determineSpeedXFactor()
        {
            if (Engine.Scene is Level)
            {
                Level level = (Level)Engine.Scene;
                if (determineIfInLiquid() && (!Active(level) || XaphanModule.PlayerIsControllingRemoteDrone()))
                {
                    return 0.6f;
                }
            }
            return 1f;
        }

        private float determineDashLengthFactor()
        {
            if (Engine.Scene is Level)
            {
                Level level = (Level)Engine.Scene;
                if (determineIfInLiquid() && (!Active(level) || XaphanModule.PlayerIsControllingRemoteDrone()))
                {
                    return 0.5f;
                }
            }
            return 1f;
        }

        public static bool determineIfInWater()
        {
            if (Engine.Scene is Level)
            {
                Level level = (Level)Engine.Scene;
                foreach (Liquid liquid in level.Tracker.GetEntities<Liquid>())
                {
                    if (liquid.PlayerInside() && liquid.liquidType == "water")
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        public static bool determineIfInLava()
        {
            if (Engine.Scene is Level)
            {
                Level level = (Level)Engine.Scene;
                foreach (Liquid liquid in level.Tracker.GetEntities<Liquid>())
                {
                    if (liquid.PlayerInside() && liquid.liquidType == "lava")
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        public static bool determineIfInAcid()
        {
            if (Engine.Scene is Level)
            {
                Level level = (Level)Engine.Scene;
                foreach (Liquid liquid in level.Tracker.GetEntities<Liquid>())
                {
                    if (liquid.PlayerInside() && liquid.liquidType.Contains("acid"))
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        public static bool determineIfInLiquid()
        {
            if (Engine.Scene is Level)
            {
                Level level = (Level)Engine.Scene;
                if (!XaphanModule.useMetroidGameplay)
                {
                    return determineIfInWater() || (VariaJacket.Active(level) && determineIfInLava());
                }
                return determineIfInWater() || determineIfInLava() || determineIfInAcid();
            }
            return false;
        }
    }
}
