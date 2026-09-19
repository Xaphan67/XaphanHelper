using System.Collections;
using Celeste.Mod.XaphanHelper.Entities;
using Monocle;

namespace Celeste.Mod.XaphanHelper.Upgrades
{
    class GrappleHook : Upgrade
    {
        Coroutine CooldownCoroutine = new();

        public static bool canUse = true;

        public override int GetDefaultValue()
        {
            return 0;
        }

        public override int GetValue()
        {
            return XaphanModule.ModSettings.GrappleHook ? 1 : 0;
        }

        public override void SetValue(int value)
        {
            XaphanModule.ModSettings.GrappleHook = (value != 0);
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
            return XaphanModule.ModSettings.GrappleHook && !XaphanModule.ModSaveData.GrappleHookInactive.Contains(level.Session.Area.LevelSet);
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
                    Player player = self.Tracker.GetEntity<Player>();
                    if (self.CanPause && !XaphanModule.PlayerIsControllingRemoteDrone() && player != null && player.StateMachine.State == Player.StNormal && !player.Ducking && XaphanModule.ModSettings.UseBagItemSlot.Pressed && !XaphanModule.ModSettings.UseMiscItemSlot.Pressed && !XaphanModule.ModSettings.OpenMap.Check && !XaphanModule.ModSettings.SelectItem.Check && !self.Session.GetFlag("Map_Opened") && player.Holding == null && canUse)
                    {
                        player.StateMachine.State = XaphanModule.StGrapple;
                        //bool vertical = Input.MoveY.Value == -1;
                        self.Add(new Grapple(player/*, vertical*/));
                        CooldownCoroutine = new Coroutine(Cooldown());
                    }
                    if (CooldownCoroutine != null)
                    {
                        CooldownCoroutine.Update();
                    }
                }
                else
                {
                    isActive = false;
                }
            }
        }

        private IEnumerator Cooldown()
        {
            canUse = false;
            yield return 0.4f;
            canUse = true;
        }
    }
}
