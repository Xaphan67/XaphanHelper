using System.Collections;
using Celeste.Mod.XaphanHelper.Cutscenes;
using Celeste.Mod.XaphanHelper.Entities;
using Celeste.Mod.XaphanHelper.UI_Elements;
using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste.Mod.XaphanHelper.Events
{
    class E01_Boss : CutsceneEntity
    {
        private Player player;

        private Vector2 bounds;

        private Torizo boss;

        private JumpThru jumpThru1;

        private JumpThru jumpThru2;

        private JumpThru jumpThru3;

        private JumpThru jumpThru4;

        private JumpThru jumpThru5;

        private JumpThru jumpThru6;

        private CustomRefill refill1;

        private CustomRefill refill2;

        private CustomRefill refill3;

        private Decal arrowDown1;

        private Decal arrowDown2;

        private Decal warningSign1;

        private Decal warningSign2;

        private Decal warningSign3;

        private Decal warningSign4;

        private Decal warningSign5;

        private Decal warningSign6;

        public bool BossDefeated()
        {
            return XaphanModule.ModSaveData.SavedFlags.Contains("Xaphan/0_Ch1_Boss_Defeated" + (XaphanModule.PlayerHasGolden ? "_GoldenStrawberry" : ""));
        }

        public bool BossDefeatedCM()
        {
            return XaphanModule.ModSaveData.SavedFlags.Contains("Xaphan/0_Ch1_Boss_Defeated_CM" + (XaphanModule.PlayerHasGolden ? "_GoldenStrawberry" : ""));
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

        public E01_Boss(Player player, Level level)
        {
            this.player = player;
            boss = level.Entities.FindFirst<Torizo>();
            bounds = new Vector2(level.Bounds.Left, level.Bounds.Top);
            jumpThru1 = new JumpthruPlatform(bounds + new Vector2(207f, 152f), 32, "Xaphan/ruins_c", 8);
            jumpThru2 = new JumpthruPlatform(bounds + new Vector2(240f, 152f), 32, "Xaphan/ruins_c", 8);
            jumpThru3 = new JumpthruPlatform(bounds + new Vector2(273f, 152f), 32, "Xaphan/ruins_c", 8);
            jumpThru4 = new JumpthruPlatform(bounds + new Vector2(335f, 152f), 32, "Xaphan/ruins_c", 8);
            jumpThru5 = new JumpthruPlatform(bounds + new Vector2(368f, 152f), 32, "Xaphan/ruins_c", 8);
            jumpThru6 = new JumpthruPlatform(bounds + new Vector2(401f, 152f), 32, "Xaphan/ruins_c", 8);
            refill1 = new CustomRefill(jumpThru3.Position + new Vector2(47f, -64f), "Max Dashes", false, 2.5f);
            refill2 = new CustomRefill(jumpThru2.Position + new Vector2(16f, -64f), "Max Jumps", false, 2.5f);
            refill3 = new CustomRefill(jumpThru5.Position + new Vector2(16f, -64f), "Max Jumps", false, 2.5f);
            arrowDown1 = new Decal("Xaphan/Common/arrow_down00.png", jumpThru3.Position + new Vector2(16f, -16f), new Vector2(1f, 1f), 1);
            arrowDown2 = new Decal("Xaphan/Common/arrow_down00.png", jumpThru4.Position + new Vector2(16f, -16f), new Vector2(1f, 1f), 1);
            warningSign1 = new Decal("Xaphan/Common/warning00.png", jumpThru1.Position + new Vector2(16f, -16f), new Vector2(1f, 1f), 1);
            warningSign2 = new Decal("Xaphan/Common/warning00.png", jumpThru2.Position + new Vector2(16f, -16f), new Vector2(1f, 1f), 1);
            warningSign3 = new Decal("Xaphan/Common/warning00.png", jumpThru3.Position + new Vector2(16f, -16f), new Vector2(1f, 1f), 1);
            warningSign4 = new Decal("Xaphan/Common/warning00.png", jumpThru4.Position + new Vector2(16f, -16f), new Vector2(1f, 1f), 1);
            warningSign5 = new Decal("Xaphan/Common/warning00.png", jumpThru5.Position + new Vector2(16f, -16f), new Vector2(1f, 1f), 1);
            warningSign6 = new Decal("Xaphan/Common/warning00.png", jumpThru6.Position + new Vector2(16f, -16f), new Vector2(1f, 1f), 1);
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
            level.Add(jumpThru6);
            foreach (JumpThru platform in SceneAs<Level>().Tracker.GetEntities<JumpThru>())
            {
                if (platform.X >= 1136 && platform.X <= 1392)
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
                if (level.Session.GetFlag("boss_Normal_Mode") || level.Session.GetFlag("boss_Challenge_Mode"))
                {
                    boss.Appear(true);
                }
                while (player.Right > jumpThru6.Right && !player.OnGround())
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
                level.Session.SetFlag("Torizo_Start", false);
                if (!XaphanModule.ModSaveData.WatchedCutscenes.Contains("Xaphan/0_Ch1_BossStart"))
                {
                    Scene.Add(new CS01_BossStart(player, boss));
                    while (!level.Session.GetFlag("Torizo_Start"))
                    {
                        yield return null;
                    }
                }
                else
                {
                    if (level.Session.GetFlag("boss_Normal_Mode") || level.Session.GetFlag("boss_Challenge_Mode"))
                    {
                        level.Session.SetFlag("D-07_Gate_1", true);
                    }
                    if (level.Session.GetFlag("boss_Checkpoint"))
                    {
                        jumpThru2.RemoveSelf();
                        jumpThru5.RemoveSelf();
                        boss.SetHealth(8);
                    }
                    while (!boss.playerHasMoved && !level.Session.GetFlag("boss_Normal_Mode_Given_Up") && !level.Session.GetFlag("boss_Challenge_Mode_Given_Up"))
                    {
                        yield return null;
                    }
                    if (level.Session.GetFlag("boss_Normal_Mode") || level.Session.GetFlag("boss_Challenge_Mode"))
                    {
                        level.Session.SetFlag("Torizo_Wakeup", true);
                    }
                    level.Session.SetFlag("Torizo_Start", true);
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
                    level.Session.RespawnPoint = level.GetSpawnPoint(bounds + new Vector2(448f, 152f));

                    if (!level.Session.GetFlag("boss_Checkpoint"))
                    {
                        // Phase 2
                        while (boss.Health >= 13)
                        {
                            yield return null;
                        }
                        if (!level.Session.GetFlag("boss_Challenge_Mode"))
                        {
                            level.Add(warningSign2);
                            level.Add(warningSign5);
                            warningSign2.Visible = true;
                            warningSign5.Visible = true;
                            Add(new Coroutine(WarningSound()));
                            yield return 1.5f;
                            warningSign2.Visible = false;
                            warningSign5.Visible = false;
                            level.Displacement.AddBurst(jumpThru2.Center, 0.5f, 8f, 32f, 0.5f);
                            jumpThru2.RemoveSelf();
                            level.Displacement.AddBurst(jumpThru5.Center, 0.5f, 8f, 32f, 0.5f);
                            jumpThru5.RemoveSelf();
                            while (boss.Health >= 9)
                            {
                                yield return null;
                            }
                            level.Session.SetFlag("boss_Checkpoint");
                        }
                        else
                        {
                            level.Add(warningSign3);
                            level.Add(warningSign4);
                            warningSign3.Visible = true;
                            warningSign4.Visible = true;
                            Add(new Coroutine(WarningSound()));
                            yield return 1.5f;
                            warningSign3.Visible = false;
                            warningSign4.Visible = false;
                            level.Displacement.AddBurst(jumpThru3.Center, 0.5f, 8f, 32f, 0.5f);
                            jumpThru3.RemoveSelf();
                            level.Displacement.AddBurst(jumpThru4.Center, 0.5f, 8f, 32f, 0.5f);
                            jumpThru4.RemoveSelf();
                            while (boss.Health >= 9)
                            {
                                yield return null;
                            }
                            level.Add(warningSign1);
                            level.Add(warningSign6);
                            warningSign1.Visible = true;
                            warningSign6.Visible = true;
                            Add(new Coroutine(WarningSound()));
                            yield return 1.5f;
                            warningSign1.Visible = false;
                            warningSign6.Visible = false;
                            level.Displacement.AddBurst(jumpThru1.Center, 0.5f, 8f, 32f, 0.5f);
                            jumpThru1.RemoveSelf();
                            level.Displacement.AddBurst(jumpThru6.Center, 0.5f, 8f, 32f, 0.5f);
                            jumpThru6.RemoveSelf();
                        }
                    }

                    // Phase 3
                    while (boss.Health >= 5)
                    {
                        yield return null;
                    }
                    if (!level.Session.GetFlag("boss_Challenge_Mode"))
                    {
                        level.Add(warningSign1);
                        level.Add(warningSign6);
                        warningSign1.Visible = true;
                        warningSign6.Visible = true;
                        Add(new Coroutine(WarningSound()));
                        yield return 1.5f;
                        warningSign1.Visible = false;
                        warningSign6.Visible = false;
                        level.Displacement.AddBurst(jumpThru1.Center, 0.5f, 8f, 32f, 0.5f);
                        jumpThru1.RemoveSelf();
                        level.Displacement.AddBurst(jumpThru6.Center, 0.5f, 8f, 32f, 0.5f);
                        jumpThru6.RemoveSelf();
                        level.Add(refill1);
                        level.Displacement.AddBurst(refill1.Center, 0.5f, 8f, 32f, 0.5f);
                    }
                    else
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
                    level.Add(jumpThru6);
                    level.Displacement.AddBurst(jumpThru6.Center, 0.5f, 8f, 32f, 0.5f);
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
                        level.Displacement.AddBurst(refill3.Center, 0.5f, 8f, 32f, 0.5f);
                        refill3.RemoveSelf();
                    }
                    string Prefix = level.Session.Area.LevelSet;
                    if (!XaphanModule.ModSaveData.SavedFlags.Contains(Prefix + "_Ch1_Boss_Defeated"))
                    {
                        XaphanModule.ModSaveData.SavedFlags.Add(Prefix + "_Ch1_Boss_Defeated");
                    }
                    if (XaphanModule.PlayerHasGolden)
                    {
                        if (!XaphanModule.ModSaveData.SavedFlags.Contains(Prefix + "_Ch1_Boss_Defeated_GoldenStrawberry"))
                        {
                            XaphanModule.ModSaveData.SavedFlags.Add(Prefix + "_Ch1_Boss_Defeated_GoldenStrawberry");
                        }
                    }
                    if (level.Session.GetFlag("boss_Challenge_Mode"))
                    {
                        level.Session.SetFlag("Boss_Defeated_CM", true);
                        if (!XaphanModule.ModSaveData.SavedFlags.Contains(Prefix + "_Ch1_Boss_Defeated_CM"))
                        {
                            XaphanModule.ModSaveData.SavedFlags.Add(Prefix + "_Ch1_Boss_Defeated_CM");
                        }
                        if (XaphanModule.PlayerHasGolden)
                        {
                            if (!XaphanModule.ModSaveData.SavedFlags.Contains(Prefix + "_Ch1_Boss_Defeated_CM_GoldenStrawberry"))
                            {
                                XaphanModule.ModSaveData.SavedFlags.Add(Prefix + "_Ch1_Boss_Defeated_CM_GoldenStrawberry");
                            }
                        }
                    }
                    while (boss.Visible)
                    {
                        yield return null;
                    }
                    level.Session.SetFlag("In_bossfight", false);
                    if (level.Session.GetFlag("boss_Normal_Mode") || level.Session.GetFlag("boss_Challenge_Mode"))
                    {
                        CMote.ManageUpgrades(level, true);
                        CMote.Visible = true;
                        level.Session.SetFlag("boss_Normal_Mode", false);
                        level.Session.SetFlag("boss_Challenge_Mode", false);
                    }
                }
                if (level.Session.GetFlag("boss_Normal_Mode_Given_Up") || level.Session.GetFlag("boss_Challenge_Mode_Given_Up"))
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
                }
                level.Session.SetFlag("boss_Normal_Mode_Given_Up", false);
                level.Session.SetFlag("boss_Challenge_Mode_Given_Up", false);
                level.Session.SetFlag("D-07_Gate_1", false);
                level.Session.SetFlag("Torizo_Defeated", true);
                level.Session.SetFlag("Boss_Defeated", true);
                level.Session.SetFlag("Torizo_Wakeup", false);
                level.Session.SetFlag("Torizo_Start", false);
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
