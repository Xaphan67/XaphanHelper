namespace Celeste.Mod.XaphanHelper.Upgrades
{
    class PlasmaBeam : Upgrade
    {
        public override int GetDefaultValue()
        {
            return 0;
        }

        public override int GetValue()
        {
            return XaphanModule.ModSettings.PlasmaBeam ? 1 : 0;
        }

        public override void SetValue(int value)
        {
            XaphanModule.ModSettings.PlasmaBeam = (value != 0);
        }

        public override void Load()
        {
        }

        public override void Unload()
        {
        }

        public static bool Active(Level level)
        {
            return XaphanModule.ModSettings.PlasmaBeam && !(XaphanModule.Instance._SaveData as XaphanModuleSaveData).PlasmaBeamInactive.Contains(level.Session.Area.LevelSet);
        }
    }
}
