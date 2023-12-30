namespace Celeste.Mod.XaphanHelper.Upgrades
{
    class IceBeam : Upgrade
    {
        public override int GetDefaultValue()
        {
            return 0;
        }

        public override int GetValue()
        {
            return XaphanModule.ModSettings.IceBeam ? 1 : 0;
        }

        public override void SetValue(int value)
        {
            XaphanModule.ModSettings.IceBeam = (value != 0);
        }

        public override void Load()
        {
        }

        public override void Unload()
        {
        }

        public static bool Active(Level level)
        {
            return XaphanModule.ModSettings.IceBeam && !(XaphanModule.Instance._SaveData as XaphanModuleSaveData).IceBeamInactive.Contains(level.Session.Area.LevelSet);
        }
    }
}
