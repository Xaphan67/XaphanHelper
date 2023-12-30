using System.Collections;
using Celeste.Mod.XaphanHelper.Entities;
using Celeste.Mod.XaphanHelper.UI_Elements;
using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste.Mod.XaphanHelper.Upgrades
{
    class MegaBombs : Upgrade
    {
        float delay = 0;

        bool cooldown;

        Coroutine UseBombCoroutine = new();

        public static bool isActive;

        public override int GetDefaultValue()
        {
            return 0;
        }

        public override int GetValue()
        {
            return XaphanModule.ModSettings.MegaBombs ? 1 : 0;
        }

        public override void SetValue(int value)
        {
            XaphanModule.ModSettings.MegaBombs = (value != 0);
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
            return XaphanModule.ModSettings.MegaBombs && !XaphanModule.ModSaveData.MegaBombsInactive.Contains(level.Session.Area.LevelSet);
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
                    if (!cooldown && self.CanPause && !XaphanModule.PlayerIsControllingRemoteDrone() && player != null && player.StateMachine.State == Player.StNormal && !player.Ducking && !self.Session.GetFlag("In_bossfight") && XaphanModule.ModSettings.UseBagItemSlot.Pressed && !XaphanModule.ModSettings.OpenMap.Check && !XaphanModule.ModSettings.SelectItem.Check && !self.Session.GetFlag("Map_Opened") && player.Holding == null && !UseBombCoroutine.Active)
                    {
                        BagDisplay bagDisplay = GetDisplay(self, "bag");
                        if (bagDisplay != null)
                        {
                            int totalBombs = self.Tracker.CountEntities<MegaBomb>();
                            if (bagDisplay.currentSelection == 2 && delay <= 0f && totalBombs == 0)
                            {
                                delay = 0.3f;
                                UseBombCoroutine = new Coroutine(UseBomb(player, self));
                            }
                        }
                    }
                    if (UseBombCoroutine != null)
                    {
                        UseBombCoroutine.Update();
                    }
                }
            }
        }

        private IEnumerator UseBomb(Player player, Level level)
        {
            bool usedBomb = false;
            while (XaphanModule.ModSettings.UseBagItemSlot.Check && !usedBomb)
            {
                while (player.Speed != Vector2.Zero)
                {
                    yield return null;
                }
                if (player.Scene != null && player.OnGround() && !player.Dead && !player.DashAttacking && player.StateMachine.State != Player.StClimb)
                {
                    cooldown = true;
                    level.Add(new MegaBomb(player.Position, player));
                    usedBomb = true;
                    while (delay > 0f)
                    {
                        delay -= Engine.DeltaTime;
                        yield return null;
                    }
                }
                yield return null;
            }
            delay = 0f;
            cooldown = false;
        }
    }
}
