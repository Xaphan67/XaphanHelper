namespace Celeste.Mod.XaphanHelper.Upgrades
{
    class LongBeam : Upgrade
    {
        public override int GetDefaultValue()
        {
            return 0;
        }

        public override int GetValue()
        {
            return XaphanModule.ModSettings.LongBeam ? 1 : 0;
        }

        public override void SetValue(int value)
        {
            XaphanModule.ModSettings.LongBeam = (value != 0);
        }

        public override void Load()
        {
        }

        public override void Unload()
        {
        }

        public static bool Active(Level level)
        {
            return XaphanModule.ModSettings.LongBeam && !(XaphanModule.Instance._SaveData as XaphanModuleSaveData).LongBeamInactive.Contains(level.Session.Area.LevelSet);
        }
    }
}
