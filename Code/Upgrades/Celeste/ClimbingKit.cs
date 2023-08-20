using System;
using System.Reflection;
using Mono.Cecil.Cil;
using Monocle;
using MonoMod.Cil;
using MonoMod.RuntimeDetour;

namespace Celeste.Mod.XaphanHelper.Upgrades
{
    class ClimbingKit : Upgrade
    {
        private ILHook wallJumpHook;

        MethodInfo wallJump = typeof(Player).GetMethod("WallJump", BindingFlags.Instance | BindingFlags.NonPublic);

        public override int GetDefaultValue()
        {
            return 1;
        }

        public override int GetValue()
        {
            return XaphanModule.ModSettings.ClimbingKit ? 1 : 0;
        }

        public override void SetValue(int value)
        {
            XaphanModule.ModSettings.ClimbingKit = (value != 0);
        }

        public override void Load()
        {
            IL.Celeste.Player.ClimbUpdate += ilPlayerClimbUpdate;
            On.Celeste.Player.ClimbJump += onPlayerClimbJump;
            wallJumpHook = new ILHook(typeof(Player).GetMethod("orig_WallJump", BindingFlags.Instance | BindingFlags.NonPublic), modWallJump);
        }

        public override void Unload()
        {
            IL.Celeste.Player.ClimbUpdate -= ilPlayerClimbUpdate;
            On.Celeste.Player.ClimbJump -= onPlayerClimbJump;
            if (wallJumpHook != null)
            {
                wallJumpHook.Dispose();
            }
        }

        public bool Active(Level level)
        {
            if (XaphanModule.useUpgrades)
            {
                return XaphanModule.ModSettings.ClimbingKit && !XaphanModule.ModSaveData.ClimbingKitInactive.Contains(level.Session.Area.GetLevelSet());
            }
            return true;
        }

        private void ilPlayerClimbUpdate(ILContext il)
        {
            ILCursor cursor = new(il);

            while (cursor.TryGotoNext(MoveType.After, instr => instr.MatchLdsfld(typeof(Input), "MoveY"), instr => instr.MatchLdfld<VirtualIntegerAxis>("Value")))
            {
                cursor.EmitDelegate<Func<int, int>>(orig =>
                {
                    if (Engine.Scene is Level)
                    {
                        Level level = (Level)Engine.Scene;
                        if (Active(level))
                        {
                            return orig;
                        }
                    }

                    return 0;
                });
            }
        }

        private void onPlayerClimbJump(On.Celeste.Player.orig_ClimbJump orig, Player self)
        {
            if (!Active(self.SceneAs<Level>()))
            {
                wallJump.Invoke(self, new object[] { -(int)self.Facing });
            }
            else
            {
                orig(self);
            }
        }

        private void modWallJump(ILContext il)
        {
            ILCursor cursor = new(il);

            if (cursor.TryGotoNext(MoveType.Before, instr => instr.OpCode == OpCodes.Ldarg_0, instr => instr.MatchLdfld<Player>("moveX")))
            {
                cursor.Index++;
                ILCursor cursorAfterBranch = cursor.Clone();
                if (cursorAfterBranch.TryGotoNext(MoveType.After, instr => instr.OpCode == OpCodes.Brfalse_S))
                {
                    cursor.Emit(OpCodes.Pop);
                    cursor.EmitDelegate<Func<bool>>(neutralJumpingEnabled);
                    cursor.Emit(OpCodes.Brfalse_S, cursorAfterBranch.Next);
                    cursor.Emit(OpCodes.Ldarg_0);
                }
            }
        }

        private bool neutralJumpingEnabled()
        {
            if (Engine.Scene is Level)
            {
                Level level = (Level)Engine.Scene;
                return Active(level);
            }
            return false;
        }
    }
}
