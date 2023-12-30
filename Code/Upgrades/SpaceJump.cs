using System;
using System.Linq;
using System.Reflection;
using Celeste.Mod.XaphanHelper.Entities;
using Celeste.Mod.XaphanHelper.UI_Elements;
using Mono.Cecil;
using Mono.Cecil.Cil;
using Monocle;
using MonoMod.Cil;

namespace Celeste.Mod.XaphanHelper.Upgrades
{
    class SpaceJump : Upgrade
    {
        private static FieldInfo playerDreamJump = typeof(Player).GetField("dreamJump", BindingFlags.NonPublic | BindingFlags.Instance);

        //private static FieldInfo playerJumpGraceTimer = typeof(Player).GetField("jumpGraceTimer", BindingFlags.NonPublic | BindingFlags.Instance);

        private static int jumpBuffer = 0;

        public override int GetDefaultValue()
        {
            return 1;
        }

        public override int GetValue()
        {
            return XaphanModule.ModSettings.SpaceJump;
        }

        public override void SetValue(int value)
        {
            XaphanModule.ModSettings.SpaceJump = value;
        }

        public override void Load()
        {
            IL.Celeste.Player.NormalUpdate += patchJumpGraceTimer;
            IL.Celeste.Player.DashUpdate += patchJumpGraceTimer;
            On.Celeste.Player.UseRefill += modUseRefill;
            On.Celeste.Player.DreamDashEnd += modDreamDashEnd;
            On.Celeste.Player.RefillDash += modRefillDash;
            On.Celeste.Level.LoadLevel += modLoadLevel;
            if (Engine.Scene is Level)
            {
                Level level = (Level)Engine.Scene;
                level.Add(new SpaceJumpIndicator());
                level.Entities.UpdateLists();
            }
        }

        public override void Unload()
        {
            IL.Celeste.Player.NormalUpdate -= patchJumpGraceTimer;
            IL.Celeste.Player.DashUpdate -= patchJumpGraceTimer;
            On.Celeste.Player.UseRefill -= modUseRefill;
            On.Celeste.Player.DreamDashEnd -= modDreamDashEnd;
            On.Celeste.Player.RefillDash -= modRefillDash;
            On.Celeste.Level.LoadLevel -= modLoadLevel;
        }

        public static bool Active(Level level)
        {
            return XaphanModule.ModSettings.SpaceJump >= 2 && !(XaphanModule.Instance._SaveData as XaphanModuleSaveData).SpaceJumpInactive.Contains(level.Session.Area.LevelSet);
        }

        private void patchJumpGraceTimer(ILContext il)
        {
            ILCursor cursor = new(il);

            MethodReference wallJumpCheck = seekReferenceToMethod(il, "WallJumpCheck");

            // jump to whenever jumpGraceTimer is retrieved
            while (cursor.TryGotoNext(MoveType.After, instr => instr.MatchLdfld<Player>("jumpGraceTimer")))
            {
                // get "this"
                cursor.Emit(OpCodes.Ldarg_0);

                // call this.WallJumpCheck(1)
                cursor.Emit(OpCodes.Ldarg_0);
                cursor.Emit(OpCodes.Ldc_I4_1);
                cursor.Emit(OpCodes.Callvirt, wallJumpCheck);

                // call this.WallJumpCheck(-1)
                cursor.Emit(OpCodes.Ldarg_0);
                cursor.Emit(OpCodes.Ldc_I4_M1);
                cursor.Emit(OpCodes.Callvirt, wallJumpCheck);

                // replace the jumpGraceTimer with the modded value
                cursor.EmitDelegate<Func<float, Player, bool, bool, float>>(canSpaceJump);
            }
            // go back to the beginning of the method
            /*cursor.Index = 0;
            // and add a call to RefillJumpBuffer so that we can reset the jumpBuffer if normal jumps are available.
            cursor.Emit(OpCodes.Ldarg_0);
            cursor.EmitDelegate<Action<Player>>(refillJumpBuffer);*/
        }

        /// <summary>
        /// Seeks any reference to a named method (callvirt) in IL code.
        /// </summary>
        /// <param name="il">Object allowing CIL patching</param>
        /// <param name="methodName">name of the method</param>
        /// <returns>A reference to the method</returns>
        private MethodReference seekReferenceToMethod(ILContext il, string methodName)
        {
            ILCursor cursor = new(il);
            if (cursor.TryGotoNext(MoveType.Before, instr => instr.OpCode == OpCodes.Callvirt && ((MethodReference)instr.Operand).Name.Contains(methodName)))
            {
                return (MethodReference)cursor.Next.Operand;
            }
            return null;
        }

        /*private void refillJumpBuffer(Player player)
        {
            //float jumpGraceTimer = (float)playerJumpGraceTimer.GetValue(player);

            // Settings.SpaceJump - 1 because the first jump is from vanilla Celeste
            (jumpGraceTimer > 0f)
            {
                jumpBuffer = Settings.SpaceJump - 1;
            }
        }*/

        /// <summary>
        /// Refills the jump buffer, giving the maximum amount of extra jumps possible.
        /// </summary>
        /// <returns>Whether extra jumps were refilled or not.</returns>
        public bool RefillJumpBuffer()
        {
            int oldJumpBuffer = jumpBuffer;
            jumpBuffer = XaphanModule.ModSettings.SpaceJump - 1;
            return oldJumpBuffer != jumpBuffer;
        }

        /// <summary>
        /// Set the jump buffer, giving the specified amount of extra jumps.
        /// </summary>
        /// <returns>Whether extra jumps were refilled or not.</returns>
        public static void SetJumpBuffer(int jumps)
        {
            jumpBuffer = jumps;
        }

        /// <summary>
        /// Adds more jumps to the jump counter.
        /// </summary>
        /// <param name="jumpsToAdd">The number of extra jumps to add</param>
        /// <param name="capped">true if the jump count should not exceed the maximum count, false if we don't care</param>
        /// <returns>Whether the jump count changed or not.</returns>
        public bool AddJumps(int jumpsToAdd, bool capped)
        {
            int oldJumpBuffer = jumpBuffer;

            // even if jumps are set to 0, 2-jump extra refills give back 2 extra jumps.
            jumpBuffer = Math.Max(0, jumpBuffer);
            jumpBuffer += jumpsToAdd;

            if (capped)
            {
                // cap the extra jump count.
                jumpBuffer = Math.Min(jumpBuffer, XaphanModule.ModSettings.SpaceJump - 1);
            }

            return oldJumpBuffer != jumpBuffer;
        }

        /// <summary>
        /// Detour the WallJump method in order to disable it if we want.
        /// </summary>
        private float canSpaceJump(float initialJumpGraceTimer, Player self, bool canWallJumpRight, bool canWallJumpLeft)
        {
            MagneticCeiling lastGrabbedCeiling = null;
            foreach (MagneticCeiling ceiling in self.SceneAs<Level>().Tracker.GetEntities<MagneticCeiling>())
            {
                if (ceiling.JumpGracePeriod)
                {
                    lastGrabbedCeiling = ceiling;
                    break;
                }
            }
            if (XaphanModule.ModSettings.SpaceJump == 0 && jumpBuffer <= 0 || self.SceneAs<Level>().Session.GetFlag("Xaphan_Helper_Ceiling") || (lastGrabbedCeiling != null && !self.DashAttacking) || Liquid.determineIfInQuicksand())
            {
                // we disabled jumping, so let's pretend the grace timer has run out
                // but if the player is grabbing a magnetic ceiling that allows jumping, we give them one free jump
                if (self.SceneAs<Level>().Session.GetFlag("Xaphan_Helper_Ceiling_Can_Jump") || (lastGrabbedCeiling != null && lastGrabbedCeiling.JumpGracePeriod) || Liquid.determineIfInQuicksand())
                {
                    if (Active(self.SceneAs<Level>()))
                    {
                        return 2f;
                    }
                    else
                    {
                        return 1f;
                    }
                }
                return 0f;
            }
            if (canWallJumpLeft || canWallJumpRight)
            {
                // no matter what, don't touch vanilla behavior if a wall jump is possible
                // because inserting extra jumps would kill wall jumping
                return initialJumpGraceTimer;
            }
            if (initialJumpGraceTimer > 0f || (XaphanModule.ModSettings.SpaceJump != 6 && jumpBuffer <= 0))
            {
                // return the default value because we don't want to change anything 
                // (our jump buffer ran out, or vanilla Celeste allows jumping anyway)
                return initialJumpGraceTimer;
            }

            if (self.Speed.Y <= 0 && (self.Speed.X <= -160f || self.Speed.X >= 160f) && self.StateMachine.State != Player.StDash)
            {
                // Pupperfish fix
                return 0f;
            }

            if (!XaphanModule.useMetroidGameplay && Active(self.SceneAs<Level>()) && !XaphanModule.PlayerIsControllingRemoteDrone())
            {
                // consume a Jump(TM)
                jumpBuffer--;
                // be sure that the sound played is not the dream jump one.
                playerDreamJump.SetValue(self, false);
                return 1f;
            }

            if (XaphanModule.useMetroidGameplay)
            {
                if (Active(self.SceneAs<Level>()) && (self.Sprite.LastAnimationID.Contains("jumpFast")) && (GravityJacket.determineIfInLiquid() ? GravityJacket.Active(self.SceneAs<Level>()) : true))
                {
                    return 1f;
                }
            }

            return 0f;
        }

        private void modDreamDashEnd(On.Celeste.Player.orig_DreamDashEnd orig, Player self)
        {
            orig(self);

            // consistently refill jumps, whichever direction the dream dash was in.
            // without this, jumps are only refilled when the coyote jump timer is filled: it only happens on horizontal dream dashes.
            RefillJumpBuffer();
        }

        private void modLoadLevel(On.Celeste.Level.orig_LoadLevel orig, Level self, Player.IntroTypes playerIntro, bool isFromLoader)
        {
            orig(self, playerIntro, isFromLoader);
            if (playerIntro != Player.IntroTypes.Transition)
            {
                // always reset the jump count when the player enters a new level (respawn, new map, etc... everything but a transition)
                RefillJumpBuffer();
            }
            if (!self.Entities.Any(entity => entity is SpaceJumpIndicator))
            {
                // add the entity showing the jump count
                self.Add(new SpaceJumpIndicator());
                //self.Entities.UpdateLists();
            }
        }

        public static int GetJumpBuffer()
        {
            return jumpBuffer;
        }

        private bool modRefillDash(On.Celeste.Player.orig_RefillDash orig, Player self)
        {
            RefillJumpBuffer();
            return orig(self);
        }

        private bool modUseRefill(On.Celeste.Player.orig_UseRefill orig, Player self, bool twoDashes)
        {
            int num = self.MaxDashes;
            if (twoDashes)
            {
                num = 2;
            }
            if (self.Dashes < num || self.Stamina < 20f)
            {
                jumpBuffer = XaphanModule.ModSettings.SpaceJump - 1;
            }
            return orig(self, twoDashes);
        }
    }
}
