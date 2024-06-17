

using System.Collections;
using Celeste.Mod.XaphanHelper.Cutscenes;
using Celeste.Mod.XaphanHelper.Entities;
using Celeste.Mod.XaphanHelper.UI_Elements;
using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste.Mod.XaphanHelper.Events
{
    class E04_Boss : CutsceneEntity
    {
        private Player player;

        private Vector2 bounds;

        private AncientGuardian boss;

        private JumpThru jumpThru1;

        private JumpThru jumpThru2;

        private JumpThru jumpThru3;

        private JumpThru jumpThru4;

        private JumpThru jumpThru5;

        private CustomRefill refill1;

        private CustomRefill refill2;

        private Decal warningSign1;

        private Decal warningSign2;

        public bool BossDefeated()
        {
            return XaphanModule.ModSaveData.SavedFlags.Contains("Xaphan/0_Ch4_Boss_Defeated" + (XaphanModule.PlayerHasGolden ? "_GoldenStrawberry" : ""));
        }

        public bool BossDefeatedCM()
        {
            return XaphanModule.ModSaveData.SavedFlags.Contains("Xaphan/0_Ch4_Boss_Defeated_CM" + (XaphanModule.PlayerHasGolden ? "_GoldenStrawberry" : ""));
        }

        public bool HasGolden()
        {
            foreach (Strawberry item in Scene.Entities.FindAll<Strawberry>())
            {
                if (item.Golden && item.Follower.Leader != null)
                {
                    return true;
                }
            }
            return false;
        }

        public E04_Boss(Player player, Level level)
        {
            this.player = player;
            boss = level.Entities.FindFirst<AncientGuardian>();
            bounds = new Vector2(level.Bounds.Left, level.Bounds.Top);
            jumpThru1 = new JumpthruPlatform(bounds + new Vector2(68f, 124f), 24, "Xaphan/gorge_a", 5);
            jumpThru2 = new JumpthruPlatform(bounds + new Vector2(96f, 148f), 24, "Xaphan/gorge_a", 5);
            jumpThru3 = new JumpthruPlatform(bounds + new Vector2(148f, 156f), 24, "Xaphan/gorge_a", 5);
            jumpThru4 = new JumpthruPlatform(bounds + new Vector2(200f, 148f), 24, "Xaphan/gorge_a", 5);
            jumpThru5 = new JumpthruPlatform(bounds + new Vector2(228f, 124f), 24, "Xaphan/gorge_a", 5);
            refill1 = new CustomRefill(bounds + new Vector2(84f, 64f), "Max Jumps", false, 2.5f);
            refill2 = new CustomRefill(bounds + new Vector2(236f, 64f), "Max Jumps", false, 2.5f);
            warningSign1 = new Decal("Xaphan/Common/warning00.png", jumpThru1.Position + new Vector2(12f, -16f), new Vector2(1f, 1f), 1);
            warningSign2 = new Decal("Xaphan/Common/warning00.png", jumpThru5.Position + new Vector2(12f, -16f), new Vector2(1f, 1f), 1);
        }

        public override void OnBegin(Level level)
        {
            level.Session.SetFlag("In_bossfight", false);
            level.InCutscene = false;
            level.CancelCutscene();
            level.Add(jumpThru1);
            level.Add(jumpThru2);
            level.Add(jumpThru3);
            level.Add(jumpThru4);
            level.Add(jumpThru5);
            foreach (JumpThru platform in SceneAs<Level>().Tracker.GetEntities<JumpThru>())
            {
                if (platform.Top <= 892)
                {
                    platform.RemoveSelf();
                }
            }
            Add(new Coroutine(Cutscene(level)));
        }

        public IEnumerator Cutscene(Level level)
        {
            if (!BossDefeated() || HasGolden() || (BossDefeated() && level.Session.GetFlag("boss_Normal_Mode")) || (BossDefeated() && level.Session.GetFlag("boss_Challenge_Mode")))
            {
                /*if (level.Session.GetFlag("boss_Normal_Mode") || level.Session.GetFlag("boss_Challenge_Mode"))
                {
                    boss.Appear(true);
                }*/
                while (player.Center.X >= jumpThru3.Right - 4f)
                {
                    yield return null;
                }
                if (player.Dead)
                {
                    yield break;
                }
                ChallengeMote CMote = level.Tracker.GetEntity<ChallengeMote>();
                if (level.Session.GetFlag("boss_Challenge_Mode"))
                {
                    if (CMote != null)
                    {
                        level.Session.SetFlag("Boss_Defeated", false);
                    }
                }
                level.Session.SetFlag("AncientGuardian_Start", false);
                level.Session.SetFlag("AncientGuardian_Platforms", true);
                if (!XaphanModule.ModSaveData.WatchedCutscenes.Contains("Xaphan/0_Ch4_BossStart"))
                {
                    Scene.Add(new CS04_BossStart(player));
                    while (!level.Session.GetFlag("AncientGuardian_Start"))
                    {
                        yield return null;
                    }
                }
                else
                {
                    if (level.Session.GetFlag("boss_Normal_Mode") || level.Session.GetFlag("boss_Challenge_Mode"))
                    {
                        //level.Session.SetFlag("D-07_Gate_1", true); // Flag for the gates
                    }
                    if (level.Session.GetFlag("boss_Checkpoint"))
                    {
                        jumpThru1.RemoveSelf();
                        jumpThru5.RemoveSelf();
                        boss.SetHealth(8);
                    }
                    while (!boss.playerHasMoved && !level.Session.GetFlag("boss_Normal_Mode_Given_Up") && !level.Session.GetFlag("boss_Challenge_Mode_Given_Up"))
                    {
                        yield return null;
                    }
                    level.Session.SetFlag("AncientGuardian_Start", true);
                }

                if (level.Session.GetFlag("boss_Normal_Mode_Given_Up") || level.Session.GetFlag("boss_Challenge_Mode_Given_Up"))
                {
                    if (level.Session.GetFlag("boss_Checkpoint"))
                    {
                        level.Session.SetFlag("boss_Checkpoint", false);
                    }
                }
                else
                {
                    // Initialise fight
                    level.Session.SetFlag("In_bossfight", true);
                    level.Add(new BossHealthBar(boss, 15, level.Session.GetFlag("boss_Challenge_Mode") ? 0 : 7, level.Session.GetFlag("boss_Challenge_Mode")));
                    level.Session.Audio.Music.Event = SFX.EventnameByHandle("event:/music/xaphan/lvl_0_mini_boss");
                    level.Session.Audio.Apply();
                    level.Session.RespawnPoint = level.GetSpawnPoint(bounds + new Vector2(160f, 156f));

                    if (!level.Session.GetFlag("boss_Checkpoint"))
                    {
                        // Phase 2
                        while (boss.Health >= 13)
                        {
                            yield return null;
                        }
                        if (!level.Session.GetFlag("boss_Challenge_Mode"))
                        {
                            level.Add(warningSign1);
                            level.Add(warningSign2);
                            warningSign1.Visible = true;
                            warningSign2.Visible = true;
                            Add(new Coroutine(WarningSound()));
                            level.Session.SetFlag("AncientGuardian_Platforms", false);
                            yield return 1.5f;
                            warningSign1.Visible = false;
                            warningSign2.Visible = false;
                            level.Displacement.AddBurst(jumpThru1.Center, 0.5f, 8f, 32f, 0.5f);
                            jumpThru1.RemoveSelf();
                            level.Displacement.AddBurst(jumpThru5.Center, 0.5f, 8f, 32f, 0.5f);
                            jumpThru5.RemoveSelf();
                            level.Add(refill1);
                            level.Displacement.AddBurst(refill1.Center, 0.5f, 8f, 32f, 0.5f);
                            level.Add(refill2);
                            level.Displacement.AddBurst(refill2.Center, 0.5f, 8f, 32f, 0.5f);
                        }
                    }

                    // Phase 2
                    level.Session.SetFlag("AncientGuardian_Platforms", false);

                    /*else
                    {
                        level.Add(arrowDown1);
                        level.Add(arrowDown2);
                        level.Add(jumpThru3);
                        level.Displacement.AddBurst(jumpThru3.Center, 0.5f, 8f, 32f, 0.5f);
                        level.Add(jumpThru4);
                        level.Displacement.AddBurst(jumpThru4.Center, 0.5f, 8f, 32f, 0.5f);
                        level.Add(warningSign2);
                        level.Add(warningSign5);
                        warningSign2.Visible = true;
                        warningSign5.Visible = true;
                        Add(new Coroutine(WarningSound()));
                        yield return 1.5f;
                        arrowDown1.Visible = false;
                        arrowDown2.Visible = false;
                        warningSign2.Visible = false;
                        warningSign5.Visible = false;
                        level.Displacement.AddBurst(jumpThru2.Center, 0.5f, 8f, 32f, 0.5f);
                        jumpThru2.RemoveSelf();
                        level.Displacement.AddBurst(jumpThru5.Center, 0.5f, 8f, 32f, 0.5f);
                        jumpThru5.RemoveSelf();
                    }
                    */
                    // End
                    while (boss.Health > 0)
                    {
                        yield return null;
                    }
                    level.Remove(level.Tracker.GetEntity<BossHealthBar>());
                    level.Session.SetFlag("boss_Checkpoint", false);
                    level.Add(jumpThru1);
                    level.Displacement.AddBurst(jumpThru1.Center, 0.5f, 8f, 32f, 0.5f);
                    level.Add(jumpThru2);
                    level.Displacement.AddBurst(jumpThru2.Center, 0.5f, 8f, 32f, 0.5f);
                    level.Add(jumpThru5);
                    level.Displacement.AddBurst(jumpThru5.Center, 0.5f, 8f, 32f, 0.5f);
                    level.Displacement.AddBurst(refill1.Center, 0.5f, 8f, 32f, 0.5f);
                    refill1.RemoveSelf();
                    if (level.Session.GetFlag("boss_Challenge_Mode"))
                    {
                        level.Add(jumpThru3);
                        level.Displacement.AddBurst(jumpThru3.Center, 0.5f, 8f, 32f, 0.5f);
                        level.Add(jumpThru4);
                        level.Displacement.AddBurst(jumpThru4.Center, 0.5f, 8f, 32f, 0.5f);
                        level.Displacement.AddBurst(refill2.Center, 0.5f, 8f, 32f, 0.5f);
                        refill2.RemoveSelf();
                    }
                    string Prefix = level.Session.Area.LevelSet;
                    if (!XaphanModule.ModSaveData.SavedFlags.Contains(Prefix + "_Ch4_Boss_Defeated"))
                    {
                        XaphanModule.ModSaveData.SavedFlags.Add(Prefix + "_Ch4_Boss_Defeated");
                    }
                    if (XaphanModule.PlayerHasGolden)
                    {
                        if (!XaphanModule.ModSaveData.SavedFlags.Contains(Prefix + "_Ch4_Boss_Defeated_GoldenStrawberry"))
                        {
                            XaphanModule.ModSaveData.SavedFlags.Add(Prefix + "_Ch4_Boss_Defeated_GoldenStrawberry");
                        }
                    }
                    if (level.Session.GetFlag("boss_Challenge_Mode"))
                    {
                        level.Session.SetFlag("Boss_Defeated_CM", true);
                        if (!XaphanModule.ModSaveData.SavedFlags.Contains(Prefix + "_Ch4_Boss_Defeated_CM"))
                        {
                            XaphanModule.ModSaveData.SavedFlags.Add(Prefix + "_Ch4_Boss_Defeated_CM");
                        }
                        if (XaphanModule.PlayerHasGolden)
                        {
                            if (!XaphanModule.ModSaveData.SavedFlags.Contains(Prefix + "_Ch4_Boss_Defeated_CM_GoldenStrawberry"))
                            {
                                XaphanModule.ModSaveData.SavedFlags.Add(Prefix + "_Ch4_Boss_Defeated_CM_GoldenStrawberry");
                            }
                        }
                    }
                    /*while (boss.Visible)
                    {
                        yield return null;
                    }*/
                    level.Session.SetFlag("In_bossfight", false);
                    if (level.Session.GetFlag("boss_Normal_Mode") || level.Session.GetFlag("boss_Challenge_Mode"))
                    {
                        CMote.ManageUpgrades(level, true);
                        CMote.Visible = true;
                        level.Session.SetFlag("boss_Normal_Mode", false);
                        level.Session.SetFlag("boss_Challenge_Mode", false);
                    }
                }
                /*if (level.Session.GetFlag("boss_Normal_Mode_Given_Up") || level.Session.GetFlag("boss_Challenge_Mode_Given_Up"))
                {
                    if (boss.Activated)
                    {
                        boss.ForcedDestroy = true;
                        yield return boss.KneelRoutine(true);
                    }
                    else
                    {
                        boss.Appear(false);
                    }
                    SceneAs<Level>().Session.Audio.Music.Event = SFX.EventnameByHandle("event:/music/xaphan/lvl_0_item");
                    SceneAs<Level>().Session.Audio.Apply();
                }*/
                level.Session.SetFlag("boss_Normal_Mode_Given_Up", false);
                level.Session.SetFlag("boss_Challenge_Mode_Given_Up", false);
                //level.Session.SetFlag("D-07_Gate_1", false);
                level.Session.SetFlag("AncientGuardian_Defeated", true);
                level.Session.SetFlag("Boss_Defeated", true);
                level.Session.SetFlag("AncientGuardian_Start", false);
            }

            // Do nothing anymore unless boss hits got reset
            while (boss.Health < 15)
            {
                yield return null;
            }
            Add(new Coroutine(Cutscene(level)));
        }

        public IEnumerator WarningSound()
        {
            int num = 1;
            do
            {
                Audio.Play("event:/game/general/thing_booped", player.Position);
                num = num + 1;
                yield return 0.3f;
            } while (num <= 5);
        }

        public override void OnEnd(Level level)
        {

        }
    }
}
