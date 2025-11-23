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

        private static bool bouceJumped;

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
            On.Celeste.Player.NormalUpdate += modNormalUpdate;
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
            On.Celeste.Player.NormalUpdate -= modNormalUpdate;
            On.Celeste.Player.DashCoroutine -= modDashCoroutine;
            playerOrigUpdateHook?.Dispose();
        }

        private int modNormalUpdate(On.Celeste.Player.orig_NormalUpdate orig, Player self)
        {
            Level level = self.SceneAs<Level>();
            if (Active(level) && (self.Speed.X > 300f || self.Speed.X < -300f) && self.Speed.Y < -100f && StartedLightningDash)
            {
                self.Hair.Color = Calc.HexToColor("F2EB6D");
                if (!level.Session.GetFlag("Xaphan_Helper_Shinesparking"))
                {
                    level.Session.SetFlag("Xaphan_Helper_Shinesparking", true);
                    if (!bouceJumped)
                    {
                        self.Play("event:/game/xaphan/shinespark_start");
                    }
                    bouceJumped = true;
                }
                if (level.OnRawInterval(0.075f))
                {
                    Vector2 scale = new(Math.Abs(self.Sprite.Scale.X) * (float)self.Facing, self.Sprite.Scale.Y);
                    TrailManager.Add(self, scale, Calc.HexToColor("F2EB6D"));
                }
                level.ParticlesFG.Emit(FlyFeather.P_Boost, self.Center + Calc.Random.Range(Vector2.One * -2f, Vector2.One * 2f), self.DashDir.Angle());
            }
            else
            {
                level.Session.SetFlag("Xaphan_Helper_Shinesparking", false);
                StartedLightningDash = false;
            }
            if (self.OnGround())
            {
                bouceJumped = false;
            }
            return orig(self);
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

        private bool StartedLightningDash;

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
                    if (Active(level) && !self.OnGround() && ((GravityJacket.determineIfInWater() || GravityJacket.determineIfInLava()) ? GravityJacket.Active(level) : true) && (Input.GrabCheck || level.Session.GetFlag("Xaphan_Helper_Shinesparking")))
                    {
                        Vector2 currentPlayerPos = self.Position;
                        Vector2 previousPlayerPos = Vector2.Zero;
                        if (self.ClimbCheck(-1) && aim.X > 0 && self.Facing == Facings.Right && aim.Y == 0)
                        {
                            level.Session.SetFlag("Xaphan_Helper_Shinesparking", true);
                            sound = Audio.Play("event:/game/xaphan/shinespark_start");
                            while (Active(level) && (self.Speed.X > 600f) && self.StateMachine.State == 2 && previousPlayerPos != currentPlayerPos)
                            {
                                previousPlayerPos = self.Position;
                                yield return null;
                                currentPlayerPos = self.Position;
                                self.Hair.Color = Calc.HexToColor("F2EB6D");
                                level.CameraOffset.X = 60f;
                                self.Facing = Facings.Right;
                            }
                            EndLightningDash(level, previousPlayerPos, currentPlayerPos, aim, sound);
                        }
                        if (self.ClimbCheck(1) && aim.X < 0 && self.Facing == Facings.Left && aim.Y == 0)
                        {
                            level.Session.SetFlag("Xaphan_Helper_Shinesparking", true);
                            sound = Audio.Play("event:/game/xaphan/shinespark_start");
                            while (Active(level) && (self.Speed.X < -600f) && self.StateMachine.State == 2 && previousPlayerPos != currentPlayerPos)
                            {
                                previousPlayerPos = self.Position;
                                yield return null;
                                currentPlayerPos = self.Position;
                                self.Hair.Color = Calc.HexToColor("F2EB6D");
                                level.CameraOffset.X = -60f;
                                self.Facing = Facings.Left;
                            }
                            EndLightningDash(level, previousPlayerPos, currentPlayerPos, aim, sound);
                        }
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

        private void EndLightningDash(Level level, Vector2 previousPlayerPos, Vector2 currentPlayerPos, Vector2 aim, EventInstance sound)
        {
            sound.stop(STOP_MODE.IMMEDIATE);
            if (previousPlayerPos != currentPlayerPos)
            {
                level.DirectionalShake(aim, 0.2f);
                sound = Audio.Play("event:/game/xaphan/shinespark_end");
                Input.Rumble(RumbleStrength.Medium, RumbleLength.Medium);
            }
            level.CameraOffset.X = 0f;
            level.Session.SetFlag("Xaphan_Helper_Shinesparking", false);
        }

        private void modifyDashSpeed(Player self)
        {
            Level level = self.SceneAs<Level>();
            Vector2 aim = Input.GetAimVector();
            if (Active(level) && !self.OnGround() && ((self.ClimbCheck(-1) && aim.X > 0 && self.Facing == Facings.Right) || (self.ClimbCheck(1) && aim.X < 0 && self.Facing == Facings.Left)) && aim.Y == 0 && ((GravityJacket.determineIfInWater() || GravityJacket.determineIfInLava()) ? GravityJacket.Active(level) : true) && (Input.GrabCheck || level.Session.GetFlag("Xaphan_Helper_Shinesparking")))
            {
                StartedLightningDash = true;
                self.Speed *= 3f;
                self.Hair.Color = Calc.HexToColor("F2EB6D");
            }
            else
            {
                StartedLightningDash = false;
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