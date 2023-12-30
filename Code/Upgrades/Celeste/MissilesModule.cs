namespace Celeste.Mod.XaphanHelper.Upgrades
{
    class MissilesModule : Upgrade
    {
        public override int GetDefaultValue()
        {
            return 0;
        }

        public override int GetValue()
        {
            return XaphanModule.ModSettings.MissilesModule ? 1 : 0;
        }

        public override void SetValue(int value)
        {
            XaphanModule.ModSettings.MissilesModule = (value != 0);
        }

        public override void Load()
        {
        }

        public override void Unload()
        {
        }

        public static bool Active(Level level)
        {
            return XaphanModule.ModSettings.MissilesModule && !XaphanModule.ModSaveData.MissilesModuleInactive.Contains(level.Session.Area.LevelSet);
        }
    }
}
