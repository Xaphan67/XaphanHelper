namespace Celeste.Mod.XaphanHelper.Upgrades
{
    class SuperMissilesModule : Upgrade
    {
        public override int GetDefaultValue()
        {
            return 0;
        }

        public override int GetValue()
        {
            return XaphanModule.ModSettings.SuperMissilesModule ? 1 : 0;
        }

        public override void SetValue(int value)
        {
            XaphanModule.ModSettings.SuperMissilesModule = (value != 0);
        }

        public override void Load()
        {
        }

        public override void Unload()
        {
        }

        public static bool Active(Level level)
        {
            return XaphanModule.ModSettings.SuperMissilesModule && !XaphanModule.ModSaveData.SuperMissilesModuleInactive.Contains(level.Session.Area.LevelSet);
        }
    }
}
