namespace Celeste.Mod.XaphanHelper.Upgrades
{
    class HighJumpBoots : Upgrade
    {
        public override int GetDefaultValue()
        {
            return 0;
        }

        public override int GetValue()
        {
            return XaphanModule.ModSettings.HighJumpBoots ? 1 : 0;
        }

        public override void SetValue(int value)
        {
            XaphanModule.ModSettings.HighJumpBoots = (value != 0);
        }

        public override void Load()
        {
        }

        public override void Unload()
        {
        }

        public static bool Active(Level level)
        {
            return XaphanModule.ModSettings.HighJumpBoots && !(XaphanModule.Instance._SaveData as XaphanModuleSaveData).HighJumpBootsInactive.Contains(level.Session.Area.LevelSet);
        }
    }
}
