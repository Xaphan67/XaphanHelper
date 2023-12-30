using System;
using System.Collections;
using System.Reflection;
using FMOD.Studio;
using Microsoft.Xna.Framework;
using Mono.Cecil.Cil;
using Monocle;
using MonoMod.Cil;
using MonoMod.RuntimeDetour;

namespace Celeste.Mod.XaphanHelper.Upgrades
{
    class LightningDash : Upgrade
    {
        private static ILHook playerOrigUpdateHook;

        private static MethodInfo playerCreateTrail = typeof(Player).GetMethod("CreateTrail", BindingFlags.Instance | BindingFlags.NonPublic);

        private FieldInfo playerDashTrailTimer = typeof(Player).GetField("dashTrailTimer", BindingFlags.Instance | BindingFlags.NonPublic);

        public override int GetDefaultValue()
        {
            return 0;
        }

        public override int GetValue()
        {
            return XaphanModule.ModSettings.LightningDash ? 1 : 0;
        }

        public override void SetValue(int value)
        {
            XaphanModule.ModSettings.LightningDash = (value != 0);
        }

        public override void Load()
        {
            Everest.Events.Player.OnDie += onPlayerDie;
            IL.Celeste.Player.CallDashEvents += modCallDashEvents;
            On.Celeste.Player.CreateTrail += modCreateTrail;
            On.Celeste.Player.DashUpdate += modDashUpdate;
            On.Celeste.Player.DashCoroutine += modDashCoroutine;
            using (new DetourContext() { After = { "*" } })
            {
                playerOrigUpdateHook = new ILHook(typeof(Player).GetMethod("orig_Update"), modPlayerOrigUpdate);
            }
        }

        public override void Unload()
        {
            Everest.Events.Player.OnDie -= onPlayerDie;
            IL.Celeste.Player.CallDashEvents -= modCallDashEvents;
            On.Celeste.Player.CreateTrail -= modCreateTrail;
            On.Celeste.Player.DashUpdate -= modDashUpdate;
            On.Celeste.Player.DashCoroutine -= modDashCoroutine;
            playerOrigUpdateHook?.Dispose();
        }

        public bool Active(Level level)
        {
            return XaphanModule.ModSettings.LightningDash && !XaphanModule.ModSaveData.LightningDashInactive.Contains(level.Session.Area.LevelSet);
        }

        private static void onPlayerDie(Player player)
        {
            player.SceneAs<Level>().Session.SetFlag("Xaphan_Helper_Shinesparking", false);
        }

        private void modCallDashEvents(ILContext il)
        {
            ILCursor cursor = new(il);

            if (cursor.TryGotoNext(MoveType.After, instr => (instr.OpCode == OpCodes.Brtrue || instr.OpCode == OpCodes.Brfalse)))
            {
                cursor.Emit(OpCodes.Ldarg_0);
                cursor.EmitDelegate<Action<Player>>(modifyDashSpeed);
            }
        }

        private void modCreateTrail(On.Celeste.Player.orig_CreateTrail orig, Player self)
        {
            if (Active(self.SceneAs<Level>()) && (self.Speed.X > 600f || self.Speed.X < -600f))
            {
                Vector2 scale = new(Math.Abs(self.Sprite.Scale.X) * (float)self.Facing, self.Sprite.Scale.Y);
                TrailManager.Add(self, scale, Calc.HexToColor("F2EB6D"));
            }
            else
            {
                orig(self);
            }
        }

        private int modDashUpdate(On.Celeste.Player.orig_DashUpdate orig, Player self)
        {
            Level level = self.SceneAs<Level>();
            float dashTrailTimer = (float)playerDashTrailTimer.GetValue(self);
            if (Active(level) && (self.Speed.X > 600f || self.Speed.X < -600f))
            {
                if (dashTrailTimer > 0f)
                {
                    dashTrailTimer -= Engine.DeltaTime;
                    playerDashTrailTimer.SetValue(self, dashTrailTimer);
                }
                if (dashTrailTimer <= 0f)
                {
                    playerCreateTrail.Invoke(self, new object[] { });
                    playerDashTrailTimer.SetValue(self, 0.06f);
                }
                level.ParticlesFG.Emit(FlyFeather.P_Boost, self.Center + Calc.Random.Range(Vector2.One * -2f, Vector2.One * 2f), self.DashDir.Angle());
                return 2;
            }
            else
            {
                return orig(self);
            }
        }

        private IEnumerator modDashCoroutine(On.Celeste.Player.orig_DashCoroutine orig, Player self)
        {
            IEnumerator coroutine = orig.Invoke(self);
            while (coroutine.MoveNext())
            {
                object o = coroutine.Current;
                Level level = self.SceneAs<Level>();
                Vector2 aim = Input.GetAimVector();
                EventInstance sound;
                if (o != null && o.GetType() == typeof(float))
                {
                    if (Active(level) && !self.OnGround() && self.ClimbCheck(-1) && aim.X > 0 && self.Facing == Facings.Right && aim.Y == 0 && ((GravityJacket.determineIfInWater() || GravityJacket.determineIfInLava()) ? GravityJacket.Active(level) : true) && (Input.Grab.Check || level.Session.GetFlag("Xaphan_Helper_Shinesparking")))
                    {
                        level.Session.SetFlag("Xaphan_Helper_Shinesparking", true);
                        sound = Audio.Play("event:/game/xaphan/shinespark_start");
                        while (Active(level) && (self.Speed.X > 600f) && self.StateMachine.State == 2)
                        {
                            yield return null;
                            self.Hair.Color = Calc.HexToColor("F2EB6D");
                            level.CameraOffset = new Vector2(60f, 0f);
                            self.Facing = Facings.Right;
                        }
                        level.DirectionalShake(aim, 0.2f);
                        sound.stop(STOP_MODE.IMMEDIATE);
                        level.CameraOffset = new Vector2(0f, 0f);
                        sound = Audio.Play("event:/game/xaphan/shinespark_end");
                        Input.Rumble(RumbleStrength.Medium, RumbleLength.Medium);
                        level.Session.SetFlag("Xaphan_Helper_Shinesparking", false);
                    }
                    if (Active(level) && !self.OnGround() && self.ClimbCheck(1) && aim.X < 0 && self.Facing == Facings.Left && aim.Y == 0 && ((GravityJacket.determineIfInWater() || GravityJacket.determineIfInLava()) ? GravityJacket.Active(level) : true) && (Input.Grab.Check || level.Session.GetFlag("Xaphan_Helper_Shinesparking")))
                    {
                        level.Session.SetFlag("Xaphan_Helper_Shinesparking", true);
                        sound = Audio.Play("event:/game/xaphan/shinespark_start");
                        while (Active(level) && (self.Speed.X < -600f) && self.StateMachine.State == 2)
                        {
                            yield return null;
                            self.Hair.Color = Calc.HexToColor("F2EB6D");
                            level.CameraOffset = new Vector2(-60f, 0f);
                            self.Facing = Facings.Left;
                        }
                        level.DirectionalShake(aim, 0.2f);
                        sound.stop(STOP_MODE.IMMEDIATE);
                        level.CameraOffset = new Vector2(0f, 0f);
                        sound = Audio.Play("event:/game/xaphan/shinespark_end");
                        Input.Rumble(RumbleStrength.Medium, RumbleLength.Medium);
                        level.Session.SetFlag("Xaphan_Helper_Shinesparking", false);
                    }
                    yield return o;
                }
                else
                {
                    yield return o;
                }
            }
            yield break;
        }

        private PlayerDeadBody modDie(On.Celeste.Player.orig_Die orig, Player self, Vector2 direction, bool evenIfInvincible = false, bool registerDeathInStats = true)
        {
            Level level = self.SceneAs<Level>();
            level.Session.SetFlag("Xaphan_Helper_Shinesparking", false);
            return orig(self, direction, evenIfInvincible, registerDeathInStats);
        }

        private void modifyDashSpeed(Player self)
        {
            Level level = self.SceneAs<Level>();
            Vector2 aim = Input.GetAimVector();
            if (Active(level) && !self.OnGround() && ((self.ClimbCheck(-1) && aim.X > 0 && self.Facing == Facings.Right) || (self.ClimbCheck(1) && aim.X < 0 && self.Facing == Facings.Left)) && aim.Y == 0 && ((GravityJacket.determineIfInWater() || GravityJacket.determineIfInLava()) ? GravityJacket.Active(level) : true) && (Input.Grab.Check || level.Session.GetFlag("Xaphan_Helper_Shinesparking")))
            {
                self.Speed *= 3f;
                self.Hair.Color = Calc.HexToColor("F2EB6D");
            }
            else
            {
                self.Speed *= 1f;
            }
        }

        private static void modPlayerOrigUpdate(ILContext il)
        {
            ILCursor cursor = new(il);
            if (cursor.TryGotoNext(MoveType.After, instr => instr.MatchLdcR4(0.01f), instr => instr.OpCode == OpCodes.Ldloc_S))
            {
                cursor.Emit(OpCodes.Ldarg_0);
                cursor.EmitDelegate<Func<float, Player, float>>((orig, self) =>
                {
                    if (XaphanModule.ModSettings.LightningDash && (self.Speed.X > 600f || self.Speed.X < -600f) && self.SceneAs<Level>().Session.GetFlag("Xaphan_Helper_Shinesparking"))
                    {
                        return 500f;
                    }
                    return orig;
                });
            }
        }
    }
}