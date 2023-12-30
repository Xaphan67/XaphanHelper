using Celeste.Mod.XaphanHelper.UI_Elements;
using Microsoft.Xna.Framework;

namespace Celeste.Mod.XaphanHelper.Upgrades
{
    class PortableStation : Upgrade
    {
        public override int GetDefaultValue()
        {
            return 0;
        }

        public override int GetValue()
        {
            return XaphanModule.ModSettings.PortableStation ? 1 : 0;
        }

        public override void SetValue(int value)
        {
            XaphanModule.ModSettings.PortableStation = (value != 0);
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
            return XaphanModule.ModSettings.PortableStation && !XaphanModule.ModSaveData.PortableStationInactive.Contains(level.Session.Area.LevelSet);
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
                }
                else
                {
                    isActive = false;
                }
                if (isActive)
                {
                    Player player = self.Tracker.GetEntity<Player>();
                    if (self.CanPause && !XaphanModule.PlayerIsControllingRemoteDrone() && player != null && player.StateMachine.State == Player.StNormal && player.Speed == Vector2.Zero && !player.Ducking && !self.Session.GetFlag("In_bossfight") && player.OnSafeGround && XaphanModule.ModSettings.UseMiscItemSlot.Pressed && !XaphanModule.ModSettings.OpenMap.Check && !XaphanModule.ModSettings.SelectItem.Check && !self.Session.GetFlag("Map_Opened") && player.Holding == null)
                    {
                        BagDisplay bagDisplay = GetDisplay(self, "misc");
                        if (bagDisplay != null)
                        {
                            if (bagDisplay.currentSelection == 2)
                            {
                                self.Add(new WarpScreen());
                            }
                        }
                    }
                }
            }
        }
    }
}
