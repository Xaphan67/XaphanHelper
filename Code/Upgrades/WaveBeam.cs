namespace Celeste.Mod.XaphanHelper.Upgrades
{
    class WaveBeam : Upgrade
    {
        public override int GetDefaultValue()
        {
            return 0;
        }

        public override int GetValue()
        {
            return XaphanModule.ModSettings.WaveBeam ? 1 : 0;
        }

        public override void SetValue(int value)
        {
            XaphanModule.ModSettings.WaveBeam = (value != 0);
        }

        public override void Load()
        {
        }

        public override void Unload()
        {
        }

        public static bool Active(Level level)
        {
            return XaphanModule.ModSettings.WaveBeam && !(XaphanModule.Instance._SaveData as XaphanModuleSaveData).WaveBeamInactive.Contains(level.Session.Area.LevelSet);
        }
    }
}
