using Celeste.Mod.XaphanHelper.Entities;
using Celeste.Mod.XaphanHelper.UI_Elements;
using Microsoft.Xna.Framework;

namespace Celeste.Mod.XaphanHelper.Upgrades
{
    class PulseRadar : Upgrade
    {
        public override int GetDefaultValue()
        {
            return 0;
        }

        public override int GetValue()
        {
            return XaphanModule.ModSettings.PulseRadar ? 1 : 0;
        }

        public override void SetValue(int value)
        {
            XaphanModule.ModSettings.PulseRadar = (value != 0);
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
            return XaphanModule.ModSettings.PulseRadar && !XaphanModule.ModSaveData.PulseRadarInactive.Contains(level.Session.Area.LevelSet);
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
                    if (self.CanPause && !XaphanModule.PlayerIsControllingRemoteDrone() && player != null && player.StateMachine.State == Player.StNormal && player.Speed == Vector2.Zero && !player.Ducking && !self.Session.GetFlag("In_bossfight") && XaphanModule.ModSettings.UseMiscItemSlot.Pressed && !XaphanModule.ModSettings.OpenMap.Check && !XaphanModule.ModSettings.SelectItem.Check && !self.Session.GetFlag("Map_Opened") && player.Holding == null)
                    {
                        BagDisplay bagDisplay = GetDisplay(self, "misc");
                        if (bagDisplay != null)
                        {
                            if (bagDisplay.currentSelection == 3)
                            {
                                int radar = self.Tracker.GetEntities<Radar>().Count;
                                if (self.Tracker.GetEntities<RadarTile>().Count == 0 && radar == 0)
                                {
                                    self.Add(new Radar(player.Position));
                                }
                            }
                        }
                    }
                }
            }
        }
    }
}
