using System.Collections;
using Celeste.Mod.XaphanHelper.Components;
using Celeste.Mod.XaphanHelper.Entities;
using Celeste.Mod.XaphanHelper.UI_Elements;
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
                    UpgradesComponent component = null;
                    if (player != null)
                    {
                        component = player.Get<UpgradesComponent>();
                        if (component != null)
                        {
                            canUse = component.GrappleHookCooldown == 0;
                        }
                    }
                    if (self.CanPause && !XaphanModule.PlayerIsControllingRemoteDrone() && player != null && component != null && player.StateMachine.State == Player.StNormal && !player.Ducking && XaphanModule.ModSettings.UseBagItemSlot.Pressed && !XaphanModule.ModSettings.UseMiscItemSlot.Pressed && !XaphanModule.ModSettings.OpenMap.Check && !XaphanModule.ModSettings.SelectItem.Check && !self.Session.GetFlag("Map_Opened") && player.Holding == null && canUse)
                    {
                        BagDisplay bagDisplay = GetDisplay(self, "bag");
                        if (bagDisplay != null)
                        {
                            if (bagDisplay.currentSelection == 4 && component.GrappleHookCooldown <= 0f)
                            {
                                player.StateMachine.State = XaphanModule.StGrapple;
                                //bool vertical = Input.MoveY.Value == -1;
                                Grapple gapple = new Grapple(player/*, vertical*/);
                                self.Add(gapple);
                                CooldownCoroutine = new Coroutine(Cooldown(gapple, component));
                            }
                        }
                        
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

        private IEnumerator Cooldown(Grapple gapple, UpgradesComponent component)
        {
            while (gapple.State != Grapple.States.Breaked)
            {
                yield return null;
            }
            component.GrappleHookCooldown = 0.3f;
        }
    }
}
