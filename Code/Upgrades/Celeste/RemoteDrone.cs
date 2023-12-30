using System.Collections;
using Celeste.Mod.XaphanHelper.Entities;
using Celeste.Mod.XaphanHelper.UI_Elements;
using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste.Mod.XaphanHelper.Upgrades
{
    class RemoteDrone : Upgrade
    {
        Coroutine UseDroneCoroutine = new();

        public static bool isActive;

        public override int GetDefaultValue()
        {
            return 0;
        }

        public override int GetValue()
        {
            return XaphanModule.ModSettings.RemoteDrone ? 1 : 0;
        }

        public override void SetValue(int value)
        {
            XaphanModule.ModSettings.RemoteDrone = (value != 0);
        }

        public override void Load()
        {
            On.Celeste.Level.Update += modLevelUpdate;
        }

        public override void Unload()
        {
            On.Celeste.Level.Update -= modLevelUpdate;
        }

        public bool Active(Level level)
        {
            return XaphanModule.ModSettings.RemoteDrone && !(XaphanModule.Instance._SaveData as XaphanModuleSaveData).RemoteDroneInactive.Contains(level.Session.Area.LevelSet);
        }

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
                if (isActive && !XaphanModule.PlayerIsControllingRemoteDrone() && !GravityJacket.determineIfInWater())
                {
                    Player player = self.Tracker.GetEntity<Player>();
                    if (self.CanPause && !XaphanModule.PlayerIsControllingRemoteDrone() && player != null && player.StateMachine.State == Player.StNormal && !player.Ducking && !self.Session.GetFlag("In_bossfight") && XaphanModule.ModSettings.UseBagItemSlot.Pressed && !XaphanModule.ModSettings.OpenMap.Check && !XaphanModule.ModSettings.SelectItem.Check && !self.Session.GetFlag("Map_Opened") && player.Holding == null && !UseDroneCoroutine.Active)
                    {
                        BagDisplay bagDisplay = GetDisplay(self, "bag");
                        if (bagDisplay != null)
                        {
                            int totalDrones = self.Tracker.CountEntities<Drone>();
                            if (bagDisplay.currentSelection == 3 && totalDrones == 0)
                            {
                                UseDroneCoroutine = new Coroutine(UseDrone(player, self));
                            }
                        }
                    }
                    if (UseDroneCoroutine != null)
                    {
                        UseDroneCoroutine.Update();
                    }
                }
            }
        }

        private IEnumerator UseDrone(Player player, Level level)
        {
            bool usedDrone = false;
            while (XaphanModule.ModSettings.UseBagItemSlot.Check && !usedDrone)
            {
                while (player.Speed != Vector2.Zero)
                {
                    yield return null;
                }
                if (player.Scene != null && player.OnGround() && !player.Dead && !player.DashAttacking && player.StateMachine.State != Player.StClimb)
                {
                    level.Add(new Drone(player.Position, player));
                    usedDrone = true;
                }
                yield return null;
            }
        }
    }
}
