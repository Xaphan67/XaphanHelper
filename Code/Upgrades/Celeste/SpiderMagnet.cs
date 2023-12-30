using System;
using Mono.Cecil.Cil;
using Monocle;
using MonoMod.Cil;

namespace Celeste.Mod.XaphanHelper.Upgrades
{
    class SpiderMagnet : Upgrade
    {
        public override int GetDefaultValue()
        {
            return 0;
        }

        public override int GetValue()
        {
            return XaphanModule.ModSettings.SpiderMagnet ? 1 : 0;
        }

        public override void SetValue(int value)
        {
            XaphanModule.ModSettings.SpiderMagnet = (value != 0);
        }

        public override void Load()
        {
            IL.Celeste.Player.NormalUpdate += modNormalUpdate;
        }

        public override void Unload()
        {
            IL.Celeste.Player.NormalUpdate -= modNormalUpdate;
        }

        public static bool Active(Level level)
        {
            return XaphanModule.ModSettings.SpiderMagnet && !(XaphanModule.Instance._SaveData as XaphanModuleSaveData).SpiderMagnetInactive.Contains(level.Session.Area.LevelSet);
        }

        private void modNormalUpdate(ILContext il)
        {
            ILCursor cursor = new(il);

            // find out where the constant 900 (downward acceleration) is loaded into the stack
            while (cursor.TryGotoNext(MoveType.After, instr => instr.MatchLdcR4(900f)))
            {
                // add two instructions to multiply those constants with the "gravity factor"
                cursor.EmitDelegate<Func<float>>(determineGravityFactor);
                cursor.Emit(OpCodes.Mul);
            }
        }

        private float determineGravityFactor()
        {
            if (Engine.Scene is Level)
            {
                Level level = (Level)Engine.Scene;
                if (level.Session.GetFlag("Xaphan_Helper_Ceiling") && !(Active(level) || !XaphanModule.useUpgrades))
                {
                    return 0f;
                }
            }
            return 1f;
        }
    }
}
