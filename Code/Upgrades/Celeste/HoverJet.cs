using System;
using System.Collections;
using Celeste.Mod.XaphanHelper.Entities;
using Microsoft.Xna.Framework;
using Mono.Cecil.Cil;
using Monocle;
using MonoMod.Cil;

namespace Celeste.Mod.XaphanHelper.Upgrades
{
    class HoverJet : Upgrade
    {
        public static bool CanFloat = true;

        public static bool Floating;

        public static float floatTimer;

        public static Tween tween;

        public static Coroutine FloatTimerRoutine = new();

        public bool useAltColor;

        public override int GetDefaultValue()
        {
            return 0;
        }

        public override int GetValue()
        {
            return XaphanModule.ModSettings.HoverJet ? 1 : 0;
        }

        public override void SetValue(int value)
        {
            XaphanModule.ModSettings.HoverJet = (value != 0);
        }

        public override void Load()
        {
            IL.Celeste.Player.NormalUpdate += ilPlayerNormalUpdate;
            On.Celeste.Player.Update += onPlayerUpdate;
        }

        public override void Unload()
        {
            IL.Celeste.Player.NormalUpdate -= ilPlayerNormalUpdate;
            On.Celeste.Player.Update -= onPlayerUpdate;
        }

        public static bool Active(Level level)
        {
            return XaphanModule.ModSettings.HoverJet && !XaphanModule.ModSaveData.HoverJetInactive.Contains(level.Session.Area.LevelSet);
        }

        private static void ilPlayerNormalUpdate(ILContext il)
        {
            ILCursor cursor = new(il);

            if (cursor.TryGotoNext(MoveType.After, instr => instr.MatchLdcR4(0.65f)))
            {
                cursor.EmitDelegate<Func<float>>(determineFrictionFactor);
                cursor.Emit(OpCodes.Mul);
            }

            while (cursor.TryGotoNext(MoveType.After, instr => instr.MatchLdcR4(900f)))
            {
                cursor.EmitDelegate<Func<float>>(determineGravityFactor);
                cursor.Emit(OpCodes.Mul);
            }
        }

        private static float determineFrictionFactor()
        {
            if (Engine.Scene is Level)
            {
                Level level = (Level)Engine.Scene;
                if (Active(level) && XaphanModule.PlayerIsControllingRemoteDrone())
                {
                    if (Floating)
                    {
                        return 0.5f;
                    }
                }
            }
            return 1f;
        }

        private static float determineGravityFactor()
        {
            if (Engine.Scene is Level)
            {
                Level level = (Level)Engine.Scene;
                if (Active(level) && XaphanModule.PlayerIsControllingRemoteDrone())
                {
                    if (Floating)
                    {
                        return 0f;
                    }
                }
            }
            return 1f;
        }

        private void onPlayerUpdate(On.Celeste.Player.orig_Update orig, Player self)
        {
            orig(self);
            if (self.GetType() == typeof(Player))
            {
                if (Active(self.SceneAs<Level>()) && XaphanModule.PlayerIsControllingRemoteDrone())
                {
                    if (self.Speed.Y >= 0 && Input.Grab.Check && !self.OnGround() && !self.DashAttacking && self.StateMachine.State == 0 && CanFloat && !Liquid.determineIfInQuicksand())
                    {
                        if (!Floating)
                        {
                            Floating = true;
                            Vector2 start = self.Position;
                            Vector2 end = self.Position + new Vector2(0f, 4f);
                            float num = Vector2.Distance(start, end) / 12.5f;
                            tween = Tween.Create(Tween.TweenMode.YoyoLooping, Ease.SineInOut, num, start: true);
                            tween.OnUpdate = delegate (Tween t)
                            {
                                self.MoveToY(Vector2.Lerp(start, end, t.Eased).Y);
                            };
                            self.Add(tween);
                        }
                        self.Speed.Y = 0f;
                        if (self.SceneAs<Level>().OnInterval(0.1f))
                        {
                            self.SceneAs<Level>().Add(Engine.Pooler.Create<SpeedRing>().Init(self.Center + Vector2.UnitY * 3, (float)Math.PI / 2f, useAltColor ? Calc.HexToColor("873724") : Calc.HexToColor("D9A066")));
                            useAltColor = !useAltColor;
                        }
                        Drone drone = self.SceneAs<Level>().Tracker.GetEntity<Drone>();
                        if (!FloatTimerRoutine.Active)
                        {
                            self.Add(FloatTimerRoutine = new Coroutine(FloatTimer()));
                        }
                    }
                    else
                    {
                        StopFloating(self);
                    }
                }
                else
                {
                    StopFloating(self);
                }
            }
        }

        private void StopFloating(Player player)
        {
            Floating = false;
            if (tween != null)
            {
                tween.Stop();
                player.Position.Y = (float)Math.Floor(player.Position.Y);
            }
            if (player.OnGround())
            {
                FloatTimerRoutine.Cancel();
                Floating = false;
                floatTimer = 1f;
                CanFloat = true;
            }
        }

        private IEnumerator FloatTimer()
        {
            while (floatTimer > 0f)
            {
                if (Floating)
                {
                    floatTimer -= Engine.DeltaTime;
                    yield return null;
                }
                else
                {
                    yield return null;
                }
            }
            CanFloat = false;
        }
    }
}
