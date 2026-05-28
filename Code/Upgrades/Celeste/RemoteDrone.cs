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

        public static bool canUse = true;

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
                if (isActive)
                {
                    Player player = self.Tracker.GetEntity<Player>();
                    if (player != null)
                    {
                        canUse = player.OnSafeGround && !GravityJacket.determineIfInWater();
                    }
                    if (self.CanPause && !XaphanModule.PlayerIsControllingRemoteDrone() && !GravityJacket.determineIfInWater() && player != null && player.StateMachine.State == Player.StNormal && !player.Ducking && !self.Session.GetFlag("In_bossfight") && !self.Session.GetFlag("XaphanHelper_Prevent_Drone") && XaphanModule.ModSettings.UseBagItemSlot.Pressed && !XaphanModule.ModSettings.UseMiscItemSlot.Pressed && !XaphanModule.ModSettings.OpenMap.Check && !XaphanModule.ModSettings.SelectItem.Check && !self.Session.GetFlag("Map_Opened") && player.Holding == null && !UseDroneCoroutine.Active && player.OnSafeGround)
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
            float leniency = 0.5f;
            while (XaphanModule.ModSettings.UseBagItemSlot.Check && !usedDrone && Input.Aim != Vector2.UnitY && !Input.MenuDown.Check)
            {
                while ((player.Speed.X != 0 || player.Dead || !player.OnGround()) &&  leniency > 0)
                {
                    leniency -= Engine.DeltaTime;
                    yield return null;
                }
                if (leniency <= 0)
                {
                    yield break;
                }
                if (player.Scene != null && player.OnGround() && !player.Dead && !player.DashAttacking && player.StateMachine.State != Player.StClimb)
                {
                    Drone drone = new Drone(player.Position, player);
                    level.Add(drone);
                    Drone.Hold.Pickup(player);
                    usedDrone = true;
                }
                yield return null;
            }
        }
    }
}
