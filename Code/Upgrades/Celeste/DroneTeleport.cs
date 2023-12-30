namespace Celeste.Mod.XaphanHelper.Upgrades
{
    class DroneTeleport : Upgrade
    {
        public override int GetDefaultValue()
        {
            return 0;
        }

        public override int GetValue()
        {
            return XaphanModule.ModSettings.DroneTeleport ? 1 : 0;
        }

        public override void SetValue(int value)
        {
            XaphanModule.ModSettings.DroneTeleport = (value != 0);
        }

        public override void Load()
        {
        }

        public override void Unload()
        {
        }

        public static bool Active(Level level)
        {
            return XaphanModule.ModSettings.DroneTeleport && !XaphanModule.ModSaveData.DroneTeleportInactive.Contains(level.Session.Area.LevelSet);
        }
    }
}
