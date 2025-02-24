using System;
using Celeste.Mod.XaphanHelper.Enemies;
using Microsoft.Xna.Framework;
using Mono.Cecil.Cil;
using Monocle;
using MonoMod.Cil;

namespace Celeste.Mod.XaphanHelper.Hooks
{
    internal class Gravity
    {
        private static float climbJumpGrabCooldown = -1f;

        public static void Load()
        {
            IL.Celeste.Player.NormalUpdate += modNormalUpdate;

            On.Celeste.Player.Update += modUpdate;
            On.Celeste.Player.ClimbJump += modClimbJump;
            Everest.Events.Level.OnExit += onLevelExit;
        }

        public static void Unload()
        {
            IL.Celeste.Player.NormalUpdate -= modNormalUpdate;

            On.Celeste.Player.Update -= modUpdate;
            On.Celeste.Player.ClimbJump -= modClimbJump;
            Everest.Events.Level.OnExit -= onLevelExit;

            climbJumpGrabCooldown = -1f;
        }

        private static void modNormalUpdate(ILContext il)
        {
            ILCursor cursor = new ILCursor(il);

            while (cursor.TryGotoNext(
                instr => instr.OpCode == OpCodes.Ldloc_S,
                instr => instr.MatchLdcR4(900f)))
            {
                cursor.Index++;
                cursor.Emit(OpCodes.Dup);
                cursor.Index++;
                cursor.Emit(OpCodes.Ldarg_0);

                cursor.EmitDelegate<Func<float, float, Player, float>>((target, gravity, player) => {
                    bool detectedPlayer = false;
                    if (Engine.Scene is Level)
                    {
                        Level level = (Level)Engine.Scene;
                        foreach (Skultera skultora in level.Tracker.GetEntities<Skultera>())
                        {
                            if (skultora.CollideDetect.DetectedPlayer)
                            {
                                detectedPlayer = true;
                                break;
                            }
                        }
                    }
                    return gravity * (detectedPlayer ? 0 : 1f);
                });
            }

            cursor.Index = 0;

            if (cursor.TryGotoNext(MoveType.After, instr => instr.MatchLdsfld(typeof(Input), "Grab") || instr.MatchCall(typeof(Input), "get_GrabCheck")) &&
                cursor.TryGotoNext(MoveType.After,
                instr => instr.MatchLdarg(0),
                instr => instr.MatchLdflda<Player>("Speed"),
                instr => instr.MatchLdfld<Vector2>("Y"),
                instr => instr.MatchLdcR4(0f),
                instr => instr.OpCode == OpCodes.Blt_Un || instr.OpCode == OpCodes.Blt_Un_S))
            {

                Instruction afterCheck = cursor.Next;

                cursor.Index -= 4;

                cursor.Emit(OpCodes.Pop);
                cursor.EmitDelegate<Func<bool>>(canGrabEvenWhenGoingUp);
                cursor.Emit(OpCodes.Brtrue, afterCheck);
                cursor.Emit(OpCodes.Ldarg_0);
            }
        }

        private static void modClimbJump(On.Celeste.Player.orig_ClimbJump orig, Player self)
        {
            orig(self);

            climbJumpGrabCooldown = 0.25f;
        }

        private static void modUpdate(On.Celeste.Player.orig_Update orig, Player self)
        {
            orig(self);

            if (climbJumpGrabCooldown >= 0f)
                climbJumpGrabCooldown -= Engine.DeltaTime;
        }

        private static void onLevelExit(Level level, LevelExit exit, LevelExit.Mode mode, Session session, HiresSnow snow)
        {
            climbJumpGrabCooldown = -1f;
        }

        private static bool canGrabEvenWhenGoingUp()
        {
            bool detectedPlayer = false;
            if (Engine.Scene is Level)
            {
                Level level = (Level)Engine.Scene;
                foreach (Skultera skultora in level.Tracker.GetEntities<Skultera>())
                {
                    if (skultora.CollideDetect.DetectedPlayer)
                    {
                        detectedPlayer = true;
                        break;
                    }
                }
            }
            return detectedPlayer && climbJumpGrabCooldown <= 0f;
        }
    }
}
