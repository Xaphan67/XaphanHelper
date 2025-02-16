using System;
using System.Collections.Generic;
using System.Linq;
using Celeste.Mod.XaphanHelper.Components;
using Celeste.Mod.XaphanHelper.Entities;
using Celeste.Mod.XaphanHelper.UI_Elements;
using Microsoft.Xna.Framework;
using Mono.Cecil.Cil;
using Monocle;
using MonoMod.Cil;

namespace Celeste.Mod.XaphanHelper.Upgrades
{
    class PowerGrip : Upgrade
    {
        public static bool isActive;

        public override int GetDefaultValue()
        {
            return 1;
        }

        public override int GetValue()
        {
            return XaphanModule.ModSettings.PowerGrip ? 1 : 0;
        }

        public override void SetValue(int value)
        {
            XaphanModule.ModSettings.PowerGrip = (value != 0);
        }

        public override void Load()
        {
            On.Celeste.Level.Update += modLevelUpdate;
            On.Celeste.Player.ClimbBoundsCheck += PlayerOnClimbBoundsCheck;
            IL.Celeste.Player.ClimbUpdate += onPlayerClimbUpdate;
            On.Celeste.Level.LoadLevel += modLoadLevel;
            if (Engine.Scene is Level)
            {
                Level level = (Level)Engine.Scene;
                level.Add(new StaminaIndicator());
                level.Entities.UpdateLists();
            }
        }

        public override void Unload()
        {
            On.Celeste.Level.Update -= modLevelUpdate;
            On.Celeste.Player.ClimbBoundsCheck -= PlayerOnClimbBoundsCheck;
            IL.Celeste.Player.ClimbUpdate -= onPlayerClimbUpdate;
            On.Celeste.Level.LoadLevel -= modLoadLevel;
        }

        public bool Active(Level level)
        {
            if (XaphanModule.useUpgrades)
            {
                return XaphanModule.ModSettings.PowerGrip && !XaphanModule.ModSaveData.PowerGripInactive.Contains(level.Session.Area.LevelSet);
            }
            return true;
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
            }
        }

        private void modLoadLevel(On.Celeste.Level.orig_LoadLevel orig, Level self, Player.IntroTypes playerIntro, bool isFromLoader)
        {
            orig(self, playerIntro, isFromLoader);
            if (!self.Entities.Any(entity => entity is StaminaIndicator))
            {
                // add the entity showing the stamina
                self.Add(new StaminaIndicator());
                //self.Entities.UpdateLists();
            }
            if (XaphanModule.useUpgrades && !XaphanModule.useMetroidGameplay)
            {
                StaminaIndicator.getStaminaData(self);
            }
        }

        private bool PlayerOnClimbBoundsCheck(On.Celeste.Player.orig_ClimbBoundsCheck orig, Player self, int dir)
        {
            BagDisplay bagDisplay = GetDisplay(self.SceneAs<Level>(), "bag");
            if (Active(self.SceneAs<Level>()) && !self.SceneAs<Level>().Session.GetFlag("Xaphan_Helper_Ceiling") && !XaphanModule.PlayerIsControllingRemoteDrone() && !PowerGripBlocker.Check(self.SceneAs<Level>(), self) && (bagDisplay != null ? (self.OnGround() ? !XaphanModule.ModSettings.UseBagItemSlot.Check : true) : true))
            {
                BagDisplay display = self.SceneAs<Level>().Tracker.GetEntity<BagDisplay>();
                List<Entity> conveyors = self.SceneAs<Level>().Tracker.GetEntities<Conveyor>();
                foreach (Entity entity in conveyors)
                {
                    Conveyor conveyor = entity as Conveyor;
                    if (conveyor.noGrabTimer > 0)
                    {
                        return false;
                    }
                }
                if (self.OnGround() && self.Speed == Vector2.Zero && (((Input.MenuUp.Check && Input.Grab.Check && display != null && XaphanModule.useUpgrades) || (XaphanModule.ModSettings.OpenMap.Pressed && XaphanModule.useIngameMap)) && self.StateMachine.State == 0))
                {
                    return false;
                }
                return orig(self, dir);
            }
            return false;
        }

        private void onPlayerClimbUpdate(ILContext il)
        {
            ILCursor cursor = new(il);

            if (cursor.TryGotoNext(MoveType.After, instr => instr.MatchCallvirt<VirtualButton>("get_Pressed")))
            {
                cursor.Emit(OpCodes.Ldarg_0);
                cursor.Emit(OpCodes.Ldarg_0);
                cursor.Emit(OpCodes.Ldfld, typeof(Player).GetField("moveX", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance));
                cursor.EmitDelegate<Func<bool, Player, int, bool>>(modJumpButtonCheck);
            }
        }

        private bool modJumpButtonCheck(bool actualValue, Player self, int moveX)
        {
            if (Active(self.SceneAs<Level>()) && !XaphanModule.PlayerIsControllingRemoteDrone() && !PowerGripBlocker.Check(self.SceneAs<Level>(), self))
            {
                return actualValue;
            }
            if (moveX == -(int)self.Facing)
            {
                return actualValue;
            }
            return false;
        }
    }
}
