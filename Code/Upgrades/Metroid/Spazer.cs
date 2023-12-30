namespace Celeste.Mod.XaphanHelper.Upgrades
{
    class Spazer : Upgrade
    {
        public override int GetDefaultValue()
        {
            return 0;
        }

        public override int GetValue()
        {
            return XaphanModule.ModSettings.Spazer ? 1 : 0;
        }

        public override void SetValue(int value)
        {
            XaphanModule.ModSettings.Spazer = (value != 0);
        }

        public override void Load()
        {
        }

        public override void Unload()
        {
        }

        public static bool Active(Level level)
        {
            return XaphanModule.ModSettings.Spazer && !(XaphanModule.Instance._SaveData as XaphanModuleSaveData).SpazerInactive.Contains(level.Session.Area.LevelSet);
        }
    }
}
