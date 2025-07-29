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
        public Sprite PlayerSprite;

        private Sprite PlayerHairSprite;

        private Sprite ScrewAttackSprite;

        public bool StartedScrewAttack;

        public static bool isScrewAttacking;

        private SoundSource screwAttackSfx;

        public bool CannotScrewAttack;

        private Coroutine QuicksandDelayRoutine = new();

        public ScrewAttackManager(Vector2 position) : base(position)
        {
            Tag = Tags.Global;
            Add(PlayerSprite = GFX.SpriteBank.Create("XaphanHelper_player_spinJump"));
            PlayerSprite.CenterOrigin();
            PlayerSprite.Position += new Vector2(0f, -2f);
            PlayerSprite.Visible = false;
            Add(PlayerHairSprite = GFX.SpriteBank.Create("XaphanHelper_player_spinJump"));
            PlayerHairSprite.CenterOrigin();
            PlayerHairSprite.Position += new Vector2(0f, -2f);
            PlayerHairSprite.Visible = false;
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
        }

        public static void Unload()
        {
            On.Celeste.Player.Die -= OnPlayerDie;
            IL.Celeste.Player.NormalUpdate -= modNormalUpdate;
        }

        private static PlayerDeadBody OnPlayerDie(On.Celeste.Player.orig_Die orig, Player self, Vector2 direction, bool evenIfInvincible, bool registerDeathInStats)
        {
            foreach (ScrewAttackManager manager in self.SceneAs<Level>().Tracker.GetEntities<ScrewAttackManager>())
            {
                if (manager.ScrewAttackSprite.Visible)
                {
                    manager.screwAttackSfx.Stop();
                    manager.PlayerSprite.Visible = false;
                    manager.PlayerHairSprite.Visible = false;
                    manager.ScrewAttackSprite.Visible = false;
                    self.Sprite.Visible = true;
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
            }
            else
            {
                isScrewAttacking = false;
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
                PlayerSprite.Color = player.Sprite.Color;
                Position = player.Position + new Vector2(0f, -4f);
                PlayerSprite.FlipX = player.Facing == Facings.Left;
                PlayerHairSprite.FlipX = player.Facing == Facings.Left;
                ScrewAttackSprite.FlipX = player.Facing == Facings.Left;
                if (((player.fastJump && player.Speed.Y < 0f) || StartedScrewAttack) && player.StateMachine.State == 0 && !player.DashAttacking && !player.OnGround() && player.Holding == null && !SceneAs<Level>().Session.GetFlag("Xaphan_Helper_Ceiling") && !XaphanModule.PlayerIsControllingRemoteDrone() && (GravityJacket.determineIfInLiquid() ? GravityJacket.Active(SceneAs<Level>()) : true) && Math.Abs(player.Speed.X) >= 90 && !CannotScrewAttack)
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
                    string backpack = SceneAs<Level>().Session.Inventory.Backpack ? "Backpack" : "NoBackpack";
                    PlayerSprite.Play("spin" + backpack);
                    PlayerHairSprite.Play("hair" + backpack);
                    ScrewAttackSprite.Play("screw");
                    StartedScrewAttack = true;
                    if (!XaphanModule.useMetroidGameplay)
                    {
                        PlayerSprite.Visible = true;
                        PlayerHairSprite.Visible = true;
                        player.Sprite.Visible = player.Hair.Visible = false;
                    }
                    ScrewAttackSprite.Visible = true;
                    Collidable = true;
                }
                if (StartedScrewAttack && (((player.StateMachine.State != 0 || player.DashAttacking || player.Sprite.CurrentAnimationID.Contains("slide") || player.Sprite.CurrentAnimationID.Contains("climb")) || SceneAs<Level>().Session.GetFlag("Xaphan_Helper_Ceiling") || ((GravityJacket.determineIfInWater() || GravityJacket.determineIfInLava()) ? !GravityJacket.Active(SceneAs<Level>()) : false)) || player.OnGround() || determineIfInQuicksand() || CannotScrewAttack))
                {
                    screwAttackSfx.Stop();
                    Collider = null;
                    PlayerSprite.Stop();
                    PlayerHairSprite.Stop();
                    ScrewAttackSprite.Stop();
                    StartedScrewAttack = false;
                    PlayerSprite.Visible = false;
                    PlayerHairSprite.Visible = false;
                    ScrewAttackSprite.Visible = false;
                    player.Sprite.Visible = player.Hair.Visible = true;
                    Collidable = false;
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
                    if (!XaphanModule.useMetroidGameplay)
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
            if (!XaphanModule.useMetroidGameplay)
            {
                if (XaphanModule.useUpgrades && (VariaJacket.Active(SceneAs<Level>()) || GravityJacket.Active(SceneAs<Level>())))
                {
                    string id = "";
                    if (GravityJacket.Active(SceneAs<Level>()))
                    {
                        id = "gravity";
                    }
                    else if (VariaJacket.Active(SceneAs<Level>()))
                    {
                        id = "varia";
                    }
                    Effect fxColorGrading = GFX.FxColorGrading;
                    fxColorGrading.CurrentTechnique = fxColorGrading.Techniques["ColorGradeSingle"];
                    Engine.Graphics.GraphicsDevice.Textures[1] = GFX.ColorGrades[id].Texture.Texture_Safe;
                    Draw.SpriteBatch.End();
                    Draw.SpriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointWrap, DepthStencilState.None, RasterizerState.CullNone, fxColorGrading, (Scene as Level).GameplayRenderer.Camera.Matrix);
                }
                if (PlayerSprite != null && PlayerSprite.Visible)
                {
                    PlayerSprite.Render();
                }
                Player player = SceneAs<Level>().Tracker.GetEntity<Player>();
                if (PlayerHairSprite != null && PlayerHairSprite.Visible && player != null)
                {
                    PlayerHairSprite.Color = player.Hair.Color;
                    PlayerHairSprite.Render();
                }
            }
            if (ScrewAttackSprite != null && ScrewAttackSprite.Visible)
            {
                ScrewAttackSprite.Render();
            }
            if (!XaphanModule.useMetroidGameplay && XaphanModule.useUpgrades && (VariaJacket.Active(SceneAs<Level>()) || GravityJacket.Active(SceneAs<Level>())))
            {
                Draw.SpriteBatch.End();
                Draw.SpriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointWrap, DepthStencilState.None, RasterizerState.CullNone, null, (Scene as Level).GameplayRenderer.Camera.Matrix);
            }
        }

        public override void Removed(Scene scene)
        {
            base.Removed(scene);
            Player player = scene.Tracker.GetEntity<Player>();
            if (player != null)
            {
                player.Sprite.Visible = player.Hair.Visible = true;
            }
        }
    }
}
