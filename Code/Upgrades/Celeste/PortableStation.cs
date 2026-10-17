using System.Collections;
using Celeste.Mod.XaphanHelper.Entities;
using Celeste.Mod.XaphanHelper.UI_Elements;
using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste.Mod.XaphanHelper.Upgrades
{
    class PortableStation : Upgrade
    {
        Coroutine UsePortableStationCoroutine = new();

        public static bool canUse = true;

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
            On.Celeste.Player.Die += onPlayerDie;
        }

        private PlayerDeadBody onPlayerDie(On.Celeste.Player.orig_Die orig, Player self, Vector2 direction, bool evenIfInvincible, bool registerDeathInStats)
        {
            if (UsePortableStationCoroutine.Active)
            {
                UsePortableStationCoroutine.Cancel();
            }
            return orig(self, direction, evenIfInvincible, registerDeathInStats);
        }

        public override void Unload()
        {
            On.Celeste.Level.Update -= modLevelUpdate;
            On.Celeste.Player.Die -= onPlayerDie;
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
                    if (player != null)
                    {
                        canUse = player.OnSafeGround && !self.Session.GetFlag("In_bossfight");
                    }
                    if (self.CanPause && !XaphanModule.PlayerIsControllingRemoteDrone() && player != null && player.StateMachine.State == Player.StNormal && !player.Ducking && !self.Session.GetFlag("In_bossfight") && XaphanModule.ModSettings.UseMiscItemSlot.Pressed && !XaphanModule.ModSettings.UseBagItemSlot.Pressed && !XaphanModule.ModSettings.OpenMap.Check && !XaphanModule.ModSettings.SelectItem.Check && !self.Session.GetFlag("Map_Opened") && player.Holding == null)
                    {
                        BagDisplay bagDisplay = GetDisplay(self, "misc");
                        if (bagDisplay != null)
                        {
                            if (bagDisplay.currentSelection == 2)
                            {
                                UsePortableStationCoroutine = new Coroutine(UsePortableStation(player, self));
                            }
                        }
                    }
                    if (UsePortableStationCoroutine != null)
                    {
                        UsePortableStationCoroutine.Update();
                    }
                }
            }
        }

        private IEnumerator UsePortableStation(Player player, Level level)
        {
            bool usedStation = false;
            float leniency = 0.5f;
            while (XaphanModule.ModSettings.UseMiscItemSlot.Check && !usedStation)
            {
                while ((player.Speed.X != 0 || player.Dead || !player.OnSafeGround) && leniency > 0)
                {
                    leniency -= Engine.DeltaTime;
                    yield return null;
                }
                if (leniency <= 0)
                {
                    yield break;
                }
                if (player.Scene != null && !player.Dead && !player.DashAttacking && player.StateMachine.State != Player.StClimb)
                {
                    level.Add(new WarpScreen());
                    usedStation = true;
                }
                yield return null;
            }
        }
    }
}
