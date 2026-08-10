using System.Collections;
using System.Reflection;

namespace Celeste.Mod.XaphanHelper.Upgrades
{
    class VariaJacket : Upgrade
    {
        private FieldInfo LookoutAnimPrefix = typeof(Lookout).GetField("animPrefix", BindingFlags.Instance | BindingFlags.NonPublic);

        public override int GetDefaultValue()
        {
            return 0;
        }

        public override int GetValue()
        {
            return XaphanModule.ModSettings.VariaJacket ? 1 : 0;
        }

        public override void SetValue(int value)
        {
            XaphanModule.ModSettings.VariaJacket = (value != 0);
        }

        public override void Load()
        {
        }

        public override void Unload()
        {
        }

        public static bool Active(Level level)
        {
            return XaphanModule.ModSettings.VariaJacket && !(XaphanModule.Instance._SaveData as XaphanModuleSaveData).VariaJacketInactive.Contains(level.Session.Area.LevelSet);
        }
    }
}
