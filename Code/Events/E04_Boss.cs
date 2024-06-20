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

        private GuardianGate LeftGate;

        private GuardianGate RightGate;

        private JumpThru jumpThru1;

        private JumpThru jumpThru2;

        private JumpThru jumpThru3;

        private JumpThru jumpThru4;

        private JumpThru jumpThru5;

        private CustomCrumbleBlock crumblePlatform1;

        private CustomRefill refill1;

        private CustomRefill refill2;

        private CustomRefill refill3;

        private CustomRefill refill4;

        private CustomRefill refill5;

        private Decal arrowDown1;

        private Decal warningSign1;

        private Decal warningSign2;

        private Decal warningSign3;

        private Decal warningSign4;

        private Decal warningSign5;

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
            crumblePlatform1 = new CustomCrumbleBlock(bounds + new Vector2(148f, 140f), Vector2.Zero, 24, 8, 2f, 0.6f, false, false, texture: "objects/Xaphan/CustomCrumbleBlock/gorge_a", lightOccludeValue: 0.2f);
            refill1 = new CustomRefill(bounds + new Vector2(92f, 64f), "Max Jumps", true, 2.5f);
            refill2 = new CustomRefill(bounds + new Vector2(228f, 64f), "Max Jumps", true, 2.5f);
            refill3 = new CustomRefill(bounds + new Vector2(160f, 92f), "Max Dashes", false, 2.5f);
            refill4 = new CustomRefill(bounds + new Vector2(64f, 56f), "Max Dashes", true, 2.5f);
            refill5 = new CustomRefill(bounds + new Vector2(256f, 56f), "Max Dashes", true, 2.5f);
            arrowDown1 = new Decal("Xaphan/Common/arrow_down00.png", crumblePlatform1.Position + new Vector2(12f, -16f), new Vector2(1f, 1f), 1);
            warningSign1 = new Decal("Xaphan/Common/warning00.png", jumpThru1.Position + new Vector2(12f, -16f), new Vector2(1f, 1f), 1);
            warningSign2 = new Decal("Xaphan/Common/warning00.png", jumpThru5.Position + new Vector2(12f, -16f), new Vector2(1f, 1f), 1);
            warningSign3 = new Decal("Xaphan/Common/warning00.png", jumpThru2.Position + new Vector2(12f, -16f), new Vector2(1f, 1f), 1);
            warningSign4 = new Decal("Xaphan/Common/warning00.png", jumpThru4.Position + new Vector2(12f, -16f), new Vector2(1f, 1f), 1);
            warningSign5 = new Decal("Xaphan/Common/warning00.png", jumpThru3.Position + new Vector2(12f, -16f), new Vector2(1f, 1f), 1);
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
            if (!level.Session.GetFlag("Boss_Defeated"))
            {
                level.Add(LeftGate = new GuardianGate(bounds + new Vector2(-48f, 112f), 40f, 40f, 'q', 'Q', "Ch4_?"));
                level.Add(RightGate = new GuardianGate(bounds + new Vector2(328f, 112f), 40f, 40f, 'q', 'Q', "Ch4_?"));
                level.Add(new Spikes(bounds + new Vector2(-8f, 112f), 40, Spikes.Directions.Right, "Xaphan/gorge"));
                level.Add(new Spikes(bounds + new Vector2(328f, 112f), 40, Spikes.Directions.Left, "Xaphan/gorge"));
            }
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
                if (level.Session.GetFlag("boss_Normal_Mode") || level.Session.GetFlag("boss_Challenge_Mode"))
                {
                    boss.Appear(true);
                    level.Add(LeftGate = new GuardianGate(bounds + new Vector2(-48f, 112f), 40f, 40f, 'q', 'Q', "Ch4_?"));
                    level.Add(RightGate = new GuardianGate(bounds + new Vector2(328f, 112f), 40f, 40f, 'q', 'Q', "Ch4_?"));
                    level.Add(new Spikes(bounds + new Vector2(-8f, 112f), 40, Spikes.Directions.Right, "Xaphan/gorge"));
                    level.Add(new Spikes(bounds + new Vector2(328f, 112f), 40, Spikes.Directions.Left, "Xaphan/gorge"));
                    yield return null;
                    if (!level.Session.GetFlag("AncientGuardian_Gates"))
                    {
                        LeftGate.Move();
                        RightGate.Move();
                    }
                    level.Session.SetFlag("AncientGuardian_Gates", true);
                    if (level.Session.GetFlag("boss_Challenge_Mode"))
                    {
                        level.Displacement.AddBurst(jumpThru1.Center, 0.5f, 8f, 32f, 0.5f);
                        jumpThru1.RemoveSelf();
                        level.Displacement.AddBurst(jumpThru5.Center, 0.5f, 8f, 32f, 0.5f);
                        jumpThru5.RemoveSelf();
                    }
                }
                while (player.Center.X >= jumpThru3.Right + 8f)
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
                    yield return null;
                    LeftGate.Move();
                    RightGate.Move();
                    Scene.Add(new CS04_BossStart(player, boss));
                    while (!level.Session.GetFlag("AncientGuardian_Start"))
                    {
                        yield return null;
                    }
                    level.Session.SetFlag("AncientGuardian_Gates", true);
                }
                else
                {
                    foreach (CustomSpinner spinner in level.Tracker.GetEntities<CustomSpinner>())
                    {
                        if (spinner.Hidden)
                        {
                            spinner.Show();
                        }
                    }
                    if (level.Session.GetFlag("boss_Checkpoint"))
                    {
                        jumpThru1.RemoveSelf();
                        jumpThru5.RemoveSelf();
                        level.Add(new CustomRefill(refill1.Position, refill1.type, refill1.oneUse, refill1.respawnTime));
                        level.Add(new CustomRefill(refill2.Position, refill2.type, refill2.oneUse, refill2.respawnTime));
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
                    if (level.Session.GetFlag("boss_Challenge_Mode_Given_Up"))
                    {
                        level.Add(jumpThru1);
                        level.Displacement.AddBurst(jumpThru1.Center, 0.5f, 8f, 32f, 0.5f);
                        level.Add(jumpThru5);
                        level.Displacement.AddBurst(jumpThru5.Center, 0.5f, 8f, 32f, 0.5f);
                    }
                }
                else
                {
                    // Initialise fight
                    level.Session.SetFlag("In_bossfight", true);
                    level.Add(new BossHealthBar(boss, 15, level.Session.GetFlag("boss_Challenge_Mode") ? 0 : 7, level.Session.GetFlag("boss_Challenge_Mode")));
                    level.Session.Audio.Music.Event = SFX.EventnameByHandle("event:/music/xaphan/lvl_4_ancient_guardian");
                    level.Session.Audio.Apply();
                    level.Session.RespawnPoint = level.GetSpawnPoint(bounds + new Vector2(160f, 156f));
                    Add(new Coroutine(RefillsRoutine()));

                    if (!level.Session.GetFlag("boss_Checkpoint"))
                    {
                        // Phase 1
                        if (level.Session.GetFlag("boss_Challenge_Mode"))
                        {
                            SceneAs<Level>().Add(new CustomRefill(refill4.Position, refill4.type, refill4.oneUse, refill4.respawnTime));
                            SceneAs<Level>().Displacement.AddBurst(refill4.Center, 0.5f, 8f, 32f, 0.5f);
                            SceneAs<Level>().Add(new CustomRefill(refill5.Position, refill5.type, refill5.oneUse, refill5.respawnTime));
                            SceneAs<Level>().Displacement.AddBurst(refill5.Center, 0.5f, 8f, 32f, 0.5f);
                            SceneAs<Level>().Add(new CustomRefill(refill3.Position, refill4.type, refill4.oneUse, refill4.respawnTime));
                            SceneAs<Level>().Displacement.AddBurst(refill3.Center, 0.5f, 8f, 32f, 0.5f);
                        }

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
                            while (boss.Health >= 9)
                            {
                                yield return null;
                            }
                            level.Session.SetFlag("boss_Checkpoint");
                        }
                    }

                    // Phase 2
                    level.Session.SetFlag("AncientGuardian_Platforms", false);
                    if (level.Session.GetFlag("boss_Challenge_Mode"))
                    {
                        while (boss.Health >= 9)
                        {
                            yield return null;
                        }
                        level.Add(warningSign5);
                        warningSign5.Visible = true;
                        Add(new Coroutine(WarningSound()));
                        yield return 1.5f;
                        warningSign5.Visible = false;
                        level.Displacement.AddBurst(jumpThru3.Center, 0.5f, 8f, 32f, 0.5f);
                        jumpThru3.RemoveSelf();
                    }
                    while (boss.Health >= 5)
                    {
                        yield return null;
                    }

                    // Phase 3
                    level.Add(warningSign3);
                    level.Add(warningSign4);
                    warningSign3.Visible = true;
                    warningSign4.Visible = true;
                    if (level.Session.GetFlag("boss_Challenge_Mode"))
                    {
                        level.Add(arrowDown1);
                        arrowDown1.Visible = true;
                        level.Add(crumblePlatform1);
                        level.Displacement.AddBurst(crumblePlatform1.Center, 0.5f, 8f, 32f, 0.5f);
                    }
                    Add(new Coroutine(WarningSound()));
                    yield return 1.5f;
                    warningSign3.Visible = false;
                    warningSign4.Visible = false;
                    level.Displacement.AddBurst(jumpThru2.Center, 0.5f, 8f, 32f, 0.5f);
                    jumpThru2.RemoveSelf();
                    level.Displacement.AddBurst(jumpThru4.Center, 0.5f, 8f, 32f, 0.5f);
                    jumpThru4.RemoveSelf();
                    if (level.Session.GetFlag("boss_Challenge_Mode"))
                    {
                        arrowDown1.Visible = false;
                    }
                    else
                    {
                        foreach (CustomRefill refill in level.Tracker.GetEntities<CustomRefill>())
                        {
                            refill.RemoveSelf();
                        }
                        level.Displacement.AddBurst(refill1.Center, 0.5f, 8f, 32f, 0.5f);
                        level.Displacement.AddBurst(refill2.Center, 0.5f, 8f, 32f, 0.5f);
                        level.Add(refill3);
                        level.Displacement.AddBurst(refill3.Center, 0.5f, 8f, 32f, 0.5f);
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
                    if (level.Session.GetFlag("boss_Challenge_Mode"))
                    {
                        level.Add(jumpThru3);
                        level.Displacement.AddBurst(jumpThru3.Center, 0.5f, 8f, 32f, 0.5f);
                        level.Displacement.AddBurst(crumblePlatform1.Center, 0.5f, 8f, 32f, 0.5f);
                        crumblePlatform1.RemoveSelf();
                    }
                    level.Add(jumpThru4);
                    level.Displacement.AddBurst(jumpThru4.Center, 0.5f, 8f, 32f, 0.5f);
                    level.Add(jumpThru5);
                    level.Displacement.AddBurst(jumpThru5.Center, 0.5f, 8f, 32f, 0.5f);
                    level.Displacement.AddBurst(refill3.Center, 0.5f, 8f, 32f, 0.5f);
                    refill3.RemoveSelf();
                    bool stopPlatforms = false;
                    while (!stopPlatforms)
                    {
                        foreach (SolidMovingPlatform platform in level.Tracker.GetEntities<SolidMovingPlatform>())
                        {
                            if (platform.Restarted)
                            {
                                stopPlatforms = true;
                                break;
                            }
                        }
                        yield return null;
                    }
                    level.Session.SetFlag("AncientGuardian_Platforms", true);
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
                    boss.ForcedDestroy = true;
                    yield return boss.DeathRoutine(true);
                    SceneAs<Level>().Session.Audio.Music.Event = SFX.EventnameByHandle("event:/music/xaphan/lvl_0_item");
                    SceneAs<Level>().Session.Audio.Apply();
                }
                level.Session.SetFlag("boss_Normal_Mode_Given_Up", false);
                level.Session.SetFlag("boss_Challenge_Mode_Given_Up", false);
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

        public IEnumerator RefillsRoutine()
        {
            while (boss.Health >= 13)
            {
                if (SceneAs<Level>().Session.GetFlag("boss_Challenge_Mode"))
                {
                    if (SceneAs<Level>().Tracker.GetEntities<CustomRefill>().Count <= 0)
                    {
                        yield return 3f;
                        if (boss.Health >= 13)
                        {
                            SceneAs<Level>().Add(new CustomRefill(refill1.Position, refill3.type, refill1.oneUse, refill1.respawnTime));
                            SceneAs<Level>().Displacement.AddBurst(refill1.Center, 0.5f, 8f, 32f, 0.5f);
                            SceneAs<Level>().Add(new CustomRefill(refill2.Position, refill3.type, refill2.oneUse, refill2.respawnTime));
                            SceneAs<Level>().Displacement.AddBurst(refill2.Center, 0.5f, 8f, 32f, 0.5f);
                            SceneAs<Level>().Add(new CustomRefill(refill3.Position, refill3.type, true, refill3.respawnTime));
                            SceneAs<Level>().Displacement.AddBurst(refill3.Center, 0.5f, 8f, 32f, 0.5f);
                        }
                    }
                }
                yield return null;
            }
            if (SceneAs<Level>().Tracker.GetEntities<CustomRefill>().Count > 0 && !SceneAs<Level>().Session.GetFlag("boss_Checkpoint"))
            {
                foreach (CustomRefill refill in SceneAs<Level>().Tracker.GetEntities<CustomRefill>())
                {
                    SceneAs<Level>().Displacement.AddBurst(refill.Center, 0.5f, 8f, 32f, 0.5f);
                    SceneAs<Level>().Remove(refill);
                }
            }
            if (SceneAs<Level>().Tracker.GetEntities<CustomRefill>().Count <= 0)
            {
                SceneAs<Level>().Add(new CustomRefill(refill1.Position, refill1.type, refill1.oneUse, refill1.respawnTime));
                SceneAs<Level>().Displacement.AddBurst(refill1.Center, 0.5f, 8f, 32f, 0.5f);
                SceneAs<Level>().Add(new CustomRefill(refill2.Position, refill2.type, refill2.oneUse, refill2.respawnTime));
                SceneAs<Level>().Displacement.AddBurst(refill2.Center, 0.5f, 8f, 32f, 0.5f);
            }
            yield return null;
            while (boss.Health >= 5)
            {
                if (SceneAs<Level>().Tracker.GetEntities<CustomRefill>().Count <= 0)
                {
                    yield return 3f;
                    if (boss.Health >= 5)
                    {
                        SceneAs<Level>().Add(new CustomRefill(refill1.Position, refill1.type, refill1.oneUse, refill1.respawnTime));
                        SceneAs<Level>().Displacement.AddBurst(refill1.Center, 0.5f, 8f, 32f, 0.5f);
                        SceneAs<Level>().Add(new CustomRefill(refill2.Position, refill2.type, refill2.oneUse, refill2.respawnTime));
                        SceneAs<Level>().Displacement.AddBurst(refill2.Center, 0.5f, 8f, 32f, 0.5f);
                    }
                }
                yield return null;
            }
            if (SceneAs<Level>().Session.GetFlag("boss_Challenge_Mode"))
            {
                int countedOneUseRefills = 0;
                int currentOneUseRefills;
                while (boss.Health >= 0)
                {
                    foreach (CustomRefill refill in SceneAs<Level>().Tracker.GetEntities<CustomRefill>())
                    {
                        if (refill.oneUse)
                        {
                            countedOneUseRefills++;
                        }
                    }
                    currentOneUseRefills = countedOneUseRefills;
                    countedOneUseRefills = 0;
                    yield return null;
                    if (currentOneUseRefills == 0)
                    {
                        yield return 3f;
                        SceneAs<Level>().Add(new CustomRefill(refill1.Position, refill1.type, refill1.oneUse, refill1.respawnTime));
                        SceneAs<Level>().Displacement.AddBurst(refill1.Center, 0.5f, 8f, 32f, 0.5f);
                        SceneAs<Level>().Add(new CustomRefill(refill2.Position, refill2.type, refill2.oneUse, refill2.respawnTime));
                        SceneAs<Level>().Displacement.AddBurst(refill2.Center, 0.5f, 8f, 32f, 0.5f);
                        yield return null;
                    }
                }
            }
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
