using Celeste.Mod.XaphanHelper.Managers;

namespace Celeste.Mod.XaphanHelper.Upgrades
{
    class ScrewAttack : Upgrade
    {
        public override int GetDefaultValue()
        {
            return 0;
        }

        public override int GetValue()
        {
            return XaphanModule.ModSettings.ScrewAttack ? 1 : 0;
        }

        public override void SetValue(int value)
        {
            XaphanModule.ModSettings.ScrewAttack = (value != 0);
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
            return XaphanModule.ModSettings.ScrewAttack && !(XaphanModule.Instance._SaveData as XaphanModuleSaveData).ScrewAttackInactive.Contains(level.Session.Area.LevelSet);
        }

        private void modLevelUpdate(On.Celeste.Level.orig_Update orig, Level self)
        {
            orig(self);
            if (XaphanModule.useUpgrades)
            {
                if (Active(self))
                {
                    Player player = self.Tracker.GetEntity<Player>();
                    if (self.Tracker.GetEntity<ScrewAttackManager>() == null && player != null)
                    {
                        self.Add(new ScrewAttackManager(player.Center));
                    }
                }
                else
                {
                    ScrewAttackManager manager = self.Tracker.GetEntity<ScrewAttackManager>();
                    if (manager != null)
                    {
                        manager.RemoveSelf();
                    }
                }
            }
        }
    }
}
