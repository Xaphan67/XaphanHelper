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

        public static bool canUse = true;

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
            On.Celeste.Holdable.Check += onHoldableCheck;
        }

        private bool onHoldableCheck(On.Celeste.Holdable.orig_Check orig, Holdable self, Player player)
        {
            if (self.Entity.GetType() == typeof(MegaBomb))
            {
                MegaBomb bomb = (MegaBomb)self.Entity;
                if (!bomb.WasThrown && Input.GrabCheck)
                {
                    return false;
                }
            }
            return orig(self, player);
        }

        public bool Active(Level level)
        {
            return XaphanModule.ModSettings.MegaBombs && !XaphanModule.ModSaveData.MegaBombsInactive.Contains(level.Session.Area.LevelSet);
        }

        public override void Unload()
        {
            On.Celeste.Level.Update -= modLevelUpdate;
            On.Celeste.Holdable.Check -= onHoldableCheck;
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
                        canUse = player.Holding != null ? true : self.Tracker.GetEntity<MegaBomb>() == null && player.OnGround() && !GravityJacket.determineIfInWater();
                    }
                    if (!cooldown && self.CanPause && !XaphanModule.PlayerIsControllingRemoteDrone() && !GravityJacket.determineIfInWater() && player != null && player.StateMachine.State == Player.StNormal && !player.Ducking && XaphanModule.ModSettings.UseBagItemSlot.Pressed && !XaphanModule.ModSettings.UseMiscItemSlot.Pressed && !XaphanModule.ModSettings.OpenMap.Check && !XaphanModule.ModSettings.SelectItem.Check && !self.Session.GetFlag("Map_Opened") && player.Holding == null)
                    {
                        BagDisplay bagDisplay = GetDisplay(self, "bag");
                        if (bagDisplay != null)
                        {
                            int totalBombs = self.Tracker.CountEntities<MegaBomb>();
                            if (bagDisplay.currentSelection == 2 && delay <= 0f && totalBombs == 0)
                            {
                                delay = 0.35f;
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
            float leniency = 0.5f;
            while (XaphanModule.ModSettings.UseBagItemSlot.Check && !usedBomb)
            {
                while ((player.Speed.X != 0 || player.Dead || !player.OnGround()) && leniency > 0)
                {
                    leniency -= Engine.DeltaTime;
                    yield return null;
                }
                if (leniency <= 0)
                {
                    yield break;
                }
                if (player.Scene != null && !player.Dead && !player.DashAttacking && player.StateMachine.State != Player.StClimb && !GravityJacket.determineIfInLiquid())
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
