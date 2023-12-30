using System;
using System.Collections;
using Celeste.Mod.XaphanHelper.Colliders;
using Celeste.Mod.XaphanHelper.Enemies;
using Celeste.Mod.XaphanHelper.Entities;
using Celeste.Mod.XaphanHelper.Upgrades;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Mono.Cecil.Cil;
using Monocle;
using MonoMod.Cil;

namespace Celeste.Mod.XaphanHelper.Managers
{
    [Tracked(true)]
    public class ScrewAttackManager : Entity
    {
        private Sprite ScrewAttackSprite;

        public bool StartedScrewAttack;

        public static bool isScrewAttacking;

        private SoundSource screwAttackSfx;

        public bool CannotScrewAttack;

        private Coroutine QuicksandDelayRoutine = new();

        public static string PlayerPose = "";
        public ScrewAttackManager(Vector2 position) : base(position)
        {
            Tag = Tags.Global;
            Add(ScrewAttackSprite = new Sprite(GFX.Game, "upgrades/ScrewAttack/"));
            ScrewAttackSprite.AddLoop("screw", "screwAttack", 0.04f);
            ScrewAttackSprite.CenterOrigin();
            ScrewAttackSprite.Position += new Vector2(0f, -2f);
            ScrewAttackSprite.Color = Color.White * 0.65f;
            ScrewAttackSprite.Visible = false;
            Collidable = false;
            screwAttackSfx = new SoundSource();
            Add(screwAttackSfx);
        }

        public static void Load()
        {
            On.Celeste.Player.Die += OnPlayerDie;
            IL.Celeste.Player.NormalUpdate += modNormalUpdate;

            On.Monocle.Sprite.Play += PlayerSpritePlayHook;
            IL.Celeste.Player.Render += PlayerRenderIlHook_Color;
        }

        public static void Unload()
        {
            On.Celeste.Player.Die -= OnPlayerDie;
            IL.Celeste.Player.NormalUpdate -= modNormalUpdate;

            On.Monocle.Sprite.Play -= PlayerSpritePlayHook;
            IL.Celeste.Player.Render -= PlayerRenderIlHook_Color;
        }

        private static void PlayerSpritePlayHook(On.Monocle.Sprite.orig_Play orig, Sprite self, string id, bool restart = false, bool randomizeFrame = false) {

            if (self.Entity is Player player && player != null && player.StateMachine.State != Player.StDash) {
                if (PlayerPose != "" && isScrewAttacking) {
                    id = PlayerPose;
                }
            }
            orig(self, id, restart, randomizeFrame);
        }
        private static void PlayerRenderIlHook_Color(ILContext il) {
            ILCursor cursor = new ILCursor(il);

            if (cursor.TryGotoNext(MoveType.After, instr => instr.MatchCall<Color>("get_White"))) {
                Logger.Log("SkinModHelper", $"Patching silhouette color at {cursor.Index} in IL code for Player.Render()");
                cursor.Emit(OpCodes.Ldarg_0);
                cursor.EmitDelegate<Func<Color, Player, Color>>((orig, self) => {

                    if (PlayerPose != "") {
                        orig = Color.Lerp(self.Hair.Color, Calc.HexToColor("44B7FF"), 1 / 4f);
                    }
                    return orig;
                });
            }
        }

        private static PlayerDeadBody OnPlayerDie(On.Celeste.Player.orig_Die orig, Player self, Vector2 direction, bool evenIfInvincible, bool registerDeathInStats)
        {
            foreach (ScrewAttackManager manager in self.SceneAs<Level>().Tracker.GetEntities<ScrewAttackManager>())
            {
                if (manager.ScrewAttackSprite.Visible)
                {
                    manager.screwAttackSfx.Stop();
                    manager.ScrewAttackSprite.Visible = false;
                    self.Sprite.Color = Color.White;
                    PlayerPose = "";
                }
            }
            return orig(self, direction, evenIfInvincible, registerDeathInStats);
        }

        private static void modNormalUpdate(ILContext il)
        {
            ILCursor cursor = new(il);

            if (cursor.TryGotoNext(MoveType.After, instr => instr.MatchLdcR4(90f)) && cursor.TryGotoNext(MoveType.Before, instr => instr.OpCode == OpCodes.Stloc_S && (((VariableDefinition)instr.Operand).Index == 6 || ((VariableDefinition)instr.Operand).Index == 31)))
            {
                VariableDefinition variable = (VariableDefinition)cursor.Next.Operand;
                if (cursor.TryGotoNext(MoveType.Before, instr => instr.OpCode == OpCodes.Ldflda))
                {
                    cursor.Emit(OpCodes.Pop);
                    cursor.Emit(OpCodes.Ldloc_S, variable);
                    cursor.EmitDelegate<Func<float>>(determineSpeedXFactor);
                    cursor.Emit(OpCodes.Mul);
                    cursor.Emit(OpCodes.Stloc_S, variable);
                    cursor.Emit(OpCodes.Ldarg_0);
                }
            }
        }

        private static float determineSpeedXFactor()
        {
            if (Engine.Scene is Level)
            {
                Level level = (Level)Engine.Scene;
                if (level.Tracker.GetEntity<ScrewAttackManager>() != null && isScrewAttacking)
                {
                    return 1.333f;
                }
            }
            return 1f;
        }

        public bool determineIfInQuicksand()
        {
            if (Engine.Scene is Level)
            {
                Level level = (Level)Engine.Scene;
                foreach (Liquid liquid in level.Tracker.GetEntities<Liquid>())
                {
                    if (liquid.PlayerInside() && liquid.liquidType == "quicksand")
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        public override void Update()
        {
            base.Update();
            foreach (ScrewAttackCollider screwAttackCollider in Scene.Tracker.GetComponents<ScrewAttackCollider>())
            {
                screwAttackCollider.Check(this);
            }
            Player player = SceneAs<Level>().Tracker.GetEntity<Player>();
            if (StartedScrewAttack)
            {
                isScrewAttacking = true;
                PlayerPose = "XaphanHelper_spinFast";
            }
            else
            {
                isScrewAttacking = false;
                PlayerPose = "";
            }
            if (ScrewAttackSprite.Visible && !screwAttackSfx.Playing)
            {
                screwAttackSfx.Play("event:/game/xaphan/screw_attack");
            }
            if (player != null)
            {
                if (determineIfInQuicksand())
                {
                    CannotScrewAttack = true;
                    if (QuicksandDelayRoutine.Active)
                    {
                        QuicksandDelayRoutine.Cancel();
                    }
                    Add(QuicksandDelayRoutine = new Coroutine(QuicksandDelay(player)));
                }
                Position = player.Position + new Vector2(0f, -4f);
                ScrewAttackSprite.FlipX = player.Facing == Facings.Left ? true : false;
                if (((player.Sprite.CurrentAnimationID.Contains("jumpFast") || StartedScrewAttack) && player.StateMachine.State == 0 && !player.DashAttacking && !player.OnGround() && player.Holding == null && !SceneAs<Level>().Session.GetFlag("Xaphan_Helper_Ceiling") && !XaphanModule.PlayerIsControllingRemoteDrone() && (GravityJacket.determineIfInLiquid() ? GravityJacket.Active(SceneAs<Level>()) : true)) && !player.Sprite.CurrentAnimationID.Contains("slide") && Math.Abs(player.Speed.X) >= 90 && !CannotScrewAttack)
                {
                    if (!screwAttackSfx.Playing && !SceneAs<Level>().Frozen)
                    {
                        screwAttackSfx.Play("event:/game/xaphan/screw_attack");
                    }
                    if (SceneAs<Level>().Frozen)
                    {
                        screwAttackSfx.Stop();
                    }
                    Collider = new Circle(10f, 0f, -2f);

                    PlayerPose = "XaphanHelper_spinFast";
                    player.Sprite.Play(PlayerPose);

                    ScrewAttackSprite.Play("screw");
                    StartedScrewAttack = true;
                    ScrewAttackSprite.Visible = true;
                    Collidable = true;
                }
                if (StartedScrewAttack && (((player.StateMachine.State != 0 || player.DashAttacking || player.Sprite.CurrentAnimationID.Contains("slide") || player.Sprite.CurrentAnimationID.Contains("climb")) || SceneAs<Level>().Session.GetFlag("Xaphan_Helper_Ceiling") || ((GravityJacket.determineIfInWater() || GravityJacket.determineIfInLava()) ? !GravityJacket.Active(SceneAs<Level>()) : false)) || player.OnGround() || determineIfInQuicksand() || CannotScrewAttack))
                {
                    screwAttackSfx.Stop();
                    Collider = null;
                    ScrewAttackSprite.Stop();
                    StartedScrewAttack = false;
                    ScrewAttackSprite.Visible = false;
                    Collidable = false;
                    PlayerPose = "";
                }
                if (StartedScrewAttack)
                {
                    foreach (BreakBlockIndicator breakBlockIndicator in Scene.Tracker.GetEntities<BreakBlockIndicator>())
                    {
                        if (breakBlockIndicator.mode == "Bomb" || breakBlockIndicator.mode == "ScrewAttack")
                        {
                            if (CollideCheck(breakBlockIndicator))
                            {
                                breakBlockIndicator.BreakSequence();
                            }
                        }
                    }
                    if (XaphanModule.useMetroidGameplay)
                    {
                        foreach (Enemy enemy in Scene.Tracker.GetEntities<Enemy>())
                        {
                            if (CollideCheck(enemy))
                            {
                                enemy.HitByScrewAttack();
                            }
                        }
                    }
                }
            }
            else
            {
                Collider = null;
            }
        }

        private IEnumerator QuicksandDelay(Player player)
        {
            float delay = 0.5f;
            while (delay > 0)
            {
                delay -= Engine.DeltaTime;
                yield return null;
            }
            CannotScrewAttack = false;
        }

        public override void Render()
        {
            base.Render();
            if (ScrewAttackSprite != null && ScrewAttackSprite.Visible)
            {
                ScrewAttackSprite.Render();
            }
        }
    }
}
