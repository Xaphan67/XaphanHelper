using System;
using System.Reflection;
using Mono.Cecil.Cil;
using Monocle;
using MonoMod.Cil;
using MonoMod.RuntimeDetour;

namespace Celeste.Mod.XaphanHelper.Upgrades
{
    class JumpBoost : Upgrade
    {
        public static MethodInfo Player_launchBegin = typeof(Player).GetMethod("LaunchBegin", BindingFlags.Instance | BindingFlags.NonPublic);

        private ILHook wallJumpHook;

        public override int GetDefaultValue()
        {
            return 0;
        }

        public override int GetValue()
        {
            return XaphanModule.ModSettings.JumpBoost ? 1 : 0;
        }

        public override void SetValue(int value)
        {
            XaphanModule.ModSettings.JumpBoost = (value != 0);
        }

        public override void Load()
        {
            On.Celeste.Level.Update += modLevelUpdate;
            IL.Celeste.Player.Jump += ilPlayerJump;
            wallJumpHook = new ILHook(typeof(Player).GetMethod("orig_WallJump", BindingFlags.Instance | BindingFlags.NonPublic), ilWallJump);
        }

        public override void Unload()
        {
            On.Celeste.Level.Update -= modLevelUpdate;
            IL.Celeste.Player.Jump -= ilPlayerJump;
            if (wallJumpHook != null) wallJumpHook.Dispose();
        }

        public static bool Active(Level level)
        {
            return XaphanModule.ModSettings.JumpBoost && !XaphanModule.ModSaveData.JumpBoostInactive.Contains(level.Session.Area.LevelSet);
        }

        public static bool isActive;

        private void modLevelUpdate(On.Celeste.Level.orig_Update orig, Level self)
        {
            orig(self);
            if (XaphanModule.useUpgrades)
            {
                if (Active(self))
                {
                    isActive = true;
                }
                else
                {
                    isActive = false;
                }
            }
        }

        private static void ilPlayerJump(ILContext il)
        {
            ILCursor cursor = new(il);

            while (cursor.TryGotoNext(instr => instr.MatchStfld<Player>("varJumpTimer")))
            {
                cursor.EmitDelegate<Func<float, float>>(orig => orig * ((isActive && XaphanModule.PlayerIsControllingRemoteDrone()) ? 1.3f : 1f));
                cursor.Index++;
            }

            while (cursor.TryGotoNext(MoveType.After, instr => instr.MatchLdcR4(-105f)))
            {
                cursor.EmitDelegate<Func<float>>(determineJumpHeightFactor);
                cursor.Emit(OpCodes.Mul);
            }

        }

        private void ilWallJump(ILContext il)
        {
            ILCursor cursor = new(il);

            while (cursor.TryGotoNext(MoveType.After, instr => instr.MatchLdcR4(-105f)))
            {
                cursor.EmitDelegate<Func<float>>(determineJumpHeightFactor);
                cursor.Emit(OpCodes.Mul);
            }
        }

        private static float determineJumpHeightFactor()
        {
            if (Engine.Scene is Level)
            {
                Level level = (Level)Engine.Scene;
                if (Active(level) && XaphanModule.PlayerIsControllingRemoteDrone())
                {
                    return 1.2f;
                }
            }
            return 1f;
        }
    }
}
