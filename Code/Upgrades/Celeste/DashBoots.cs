using System.Reflection;
using Celeste.Mod.XaphanHelper.Entities;
using Monocle;

namespace Celeste.Mod.XaphanHelper.Upgrades
{
    class DashBoots : Upgrade
    {
        private FieldInfo Player_dashCooldownTimer = typeof(Player).GetField("dashCooldownTimer", BindingFlags.Instance | BindingFlags.NonPublic);

        public override int GetDefaultValue()
        {
            return 1;
        }

        public override int GetValue()
        {
            return XaphanModule.ModSettings.DashBoots ? 1 : 0;
        }

        public override void SetValue(int value)
        {
            XaphanModule.ModSettings.DashBoots = (value != 0);
        }

        public override void Load()
        {
            On.Celeste.Player.Update += modUpdate;
        }

        public override void Unload()
        {
            On.Celeste.Player.Update -= modUpdate;
        }

        public bool Active(Level level)
        {
            if (XaphanModule.useUpgrades)
            {
                return XaphanModule.ModSettings.DashBoots && !XaphanModule.ModSaveData.DashBootsInactive.Contains(level.Session.Area.LevelSet);
            }
            return true;
        }

        private void modUpdate(On.Celeste.Player.orig_Update orig, Player self)
        {
            orig.Invoke(self);
            if (Active(self.SceneAs<Level>()))
            {
                foreach (MagneticCeiling ceiling in self.SceneAs<Level>().Tracker.GetEntities<MagneticCeiling>())
                {
                    if (ceiling.playerWasAttached && ceiling.Active)
                    {
                        Player_dashCooldownTimer.SetValue(self, Engine.DeltaTime + 0.01f);
                    }
                }
            }
            if (!Active(self.SceneAs<Level>()) || XaphanModule.PlayerIsControllingRemoteDrone())
            {
                Player_dashCooldownTimer.SetValue(self, Engine.DeltaTime + 0.1f);
            }
        }
    }
}
