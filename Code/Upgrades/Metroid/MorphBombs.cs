namespace Celeste.Mod.XaphanHelper.Upgrades
{
    class MorphBombs : Upgrade
    {
        public override int GetDefaultValue()
        {
            return 0;
        }

        public override int GetValue()
        {
            return XaphanModule.ModSettings.MorphBombs ? 1 : 0;
        }

        public override void SetValue(int value)
        {
            XaphanModule.ModSettings.MorphBombs = (value != 0);
        }

        public override void Load()
        {
        }

        public override void Unload()
        {
        }

        public static bool Active(Level level)
        {
            return XaphanModule.ModSettings.MorphBombs && !(XaphanModule.Instance._SaveData as XaphanModuleSaveData).MorphBombsInactive.Contains(level.Session.Area.LevelSet);
        }
    }
}
