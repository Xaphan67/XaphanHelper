namespace Celeste.Mod.XaphanHelper.Upgrades
{
    class EtherealDash : Upgrade
    {
        public override int GetDefaultValue()
        {
            return 0;
        }

        public override int GetValue()
        {
            return XaphanModule.ModSettings.EtherealDash ? 1 : 0;
        }

        public override void SetValue(int value)
        {
            XaphanModule.ModSettings.EtherealDash = (value != 0);
        }

        public override void Load()
        {
            On.Celeste.Level.Update += modLevelUpdate;
        }

        public override void Unload()
        {
            On.Celeste.Level.Update -= modLevelUpdate;
        }

        public static bool Active(Level level)
        {
            return XaphanModule.ModSettings.EtherealDash && !XaphanModule.ModSaveData.EtherealDashInactive.Contains(level.Session.Area.LevelSet);
        }

        public static bool isActive;

        private void modLevelUpdate(On.Celeste.Level.orig_Update orig, Level self)
        {
            orig(self);
            if (XaphanModule.useUpgrades)
            {
                if (Active(self))
                {
                    isActive = true;
                    if (!self.Session.Inventory.DreamDash)
                    {
                        self.Session.Inventory.DreamDash = true;
                    }
                }
                else
                {
                    isActive = false;
                    if (self.Session.Inventory.DreamDash)
                    {
                        self.Session.Inventory.DreamDash = false;
                    }
                }
            }
        }
    }
}
