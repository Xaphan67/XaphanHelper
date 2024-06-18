using System.Collections;
using Celeste.Mod.XaphanHelper.Entities;
using Celeste.Mod.XaphanHelper.UI_Elements;
using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste.Mod.XaphanHelper.Events
{
    class E05_Boss : CutsceneEntity
    {
        private Player player;

        private Vector2 bounds;

        private Genesis boss;

        private Genesis.GenesisBarrier ceiling1;

        private Genesis.GenesisBarrier ceiling2;

        private Genesis.GenesisBarrier ceiling3;

        private DashBlock dashBlock;

        private Spikes spikes;

        private Liquid liquid;

        private CustomRefill refill1;

        private CustomRefill refill2;

        private CustomRefill refill3;

        private Decal arrowUp1;

        private Decal arrowUp2;

        private Decal warningSign1;

        private Decal warningSign2;

        private Decal warningSign3;

        private Decal warningSign4;

        public bool BossDefeated()
        {
            return XaphanModule.ModSaveData.SavedFlags.Contains("Xaphan/0_Ch5_Boss_Defeated" + (XaphanModule.PlayerHasGolden ? "_GoldenStrawberry" : ""));
        }

        public bool BossDefeatedCM()
        {
            return XaphanModule.ModSaveData.SavedFlags.Contains("Xaphan/0_Ch5_Boss_Defeated_CM" + (XaphanModule.PlayerHasGolden ? "_GoldenStrawberry" : ""));
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

        public E05_Boss(Player player, Level level)
        {
            this.player = player;
            boss = level.Entities.FindFirst<Genesis>();
            bounds = new Vector2(level.Bounds.Left, level.Bounds.Top);
            ceiling1 = new Genesis.GenesisBarrier(bounds + new Vector2(160f, 24f), 64, 8);
            ceiling2 = new Genesis.GenesisBarrier(bounds + new Vector2(288f, 24f), 64, 8);
            ceiling3 = new Genesis.GenesisBarrier(bounds + new Vector2(416f, 24f), 64, 8);
            dashBlock = new DashBlock(bounds + new Vector2(576f, 56f), 'Y', 64, 80, true, false, false, new EntityID());
            spikes = new Spikes(bounds + new Vector2(576f, 80f), 40, Spikes.Directions.Left, "Xaphan/terminal");
            refill1 = new CustomRefill(bounds + new Vector2(256f, 44f), "Max Dashes", false, 2.5f);
            refill2 = new CustomRefill(bounds + new Vector2(320f, 96f), "Max Dashes", false, 2.5f);
            refill3 = new CustomRefill(bounds + new Vector2(384f, 44f), "Max Dashes", false, 2.5f);
            arrowUp1 = new Decal("Xaphan/Common/arrow_up00.png", bounds + new Vector2(256f, 68f), new Vector2(1f, 1f), 1);
            arrowUp2 = new Decal("Xaphan/Common/arrow_up00.png", bounds + new Vector2(384f, 68f), new Vector2(1f, 1f), 1);
            warningSign1 = new Decal("Xaphan/Common/warning00.png", bounds + new Vector2(128f, 128f), new Vector2(1f, 1f), 1);
            warningSign2 = new Decal("Xaphan/Common/warning00.png", bounds + new Vector2(256f, 128f), new Vector2(1f, 1f), 1);
            warningSign3 = new Decal("Xaphan/Common/warning00.png", bounds + new Vector2(384f, 128f), new Vector2(1f, 1f), 1);
            warningSign4 = new Decal("Xaphan/Common/warning00.png", bounds + new Vector2(512f, 128f), new Vector2(1f, 1f), 1);
        }

        public override void OnBegin(Level level)
        {
            level.Session.SetFlag("In_bossfight", false);
            level.InCutscene = false;
            level.CancelCutscene();
            liquid = level.Tracker.GetEntity<Liquid>();
            Add(new Coroutine(Cutscene(level)));
        }

        public IEnumerator Cutscene(Level level)
        {
            level.Session.SetFlag("Genesis_Start", false);
            if (!BossDefeated() || HasGolden() || (BossDefeated() && level.Session.GetFlag("boss_Normal_Mode")) || (BossDefeated() && level.Session.GetFlag("boss_Challenge_Mode")))
            {
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
                level.Session.SetFlag("Genesis_rise", false);
                if (level.Session.GetFlag("boss_Checkpoint"))
                {
                    boss.SetHealth(8);
                }
                if (!XaphanModule.ModSaveData.SavedFlags.Contains("Xaphan/0_Ch5_Pre_Genesis_Event"))
                {
                    level.Add(dashBlock);
                    level.Add(spikes);
                    level.Session.SetFlag("Genesis_Inactive", true);
                    while (player.Left <= bounds.X + 456f)
                    {
                        yield return null;
                    }
                    while (player.Left >= bounds.X + 256f)
                    {
                        yield return null;
                    }
                    level.Session.SetFlag("Genesis_Inactive", false);
                    XaphanModule.ModSaveData.SavedFlags.Add("Xaphan/0_Ch5_Pre_Genesis_Event");
                }
                else
                {
                    while (!boss.playerHasMoved && !level.Session.GetFlag("boss_Normal_Mode_Given_Up") && !level.Session.GetFlag("boss_Challenge_Mode_Given_Up"))
                    {
                        yield return null;
                    }
                }
                level.Session.SetFlag("Genesis_Start", true);
                level.Session.SetFlag("Genesis_Active", true);
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
                    boss.Visible = boss.Collidable = true;
                    level.Session.Audio.Music.Event = SFX.EventnameByHandle("event:/music/xaphan/lvl_0_genesis");
                    level.Session.Audio.Apply();

                    // Phase 1
                    level.Add(ceiling1);
                    level.Add(ceiling2);
                    level.Add(ceiling3);
                    level.Add(dashBlock);
                    level.Add(spikes);

                    // Phase 2
                    if (!level.Session.GetFlag("boss_Challenge_Mode"))
                    {
                        while (boss.Health >= 9)
                        {
                            yield return null;
                        }
                        level.Session.SetFlag("boss_Checkpoint");
                    }

                    // Phase 3
                    while (boss.Health >= 5)
                    {
                        yield return null;
                    }
                    level.Add(arrowUp1);
                    level.Add(arrowUp2);
                    level.Add(refill1);
                    level.Displacement.AddBurst(refill1.Center, 0.5f, 8f, 32f, 0.5f);
                    level.Add(refill2);
                    level.Displacement.AddBurst(refill2.Center, 0.5f, 8f, 32f, 0.5f);
                    level.Add(refill3);
                    level.Displacement.AddBurst(refill3.Center, 0.5f, 8f, 32f, 0.5f);
                    level.Add(warningSign1);
                    level.Add(warningSign2);
                    level.Add(warningSign3);
                    level.Add(warningSign4);
                    warningSign1.Visible = true;
                    warningSign2.Visible = true;
                    warningSign3.Visible = true;
                    warningSign4.Visible = true;
                    level.Session.SetFlag("Genesis_rise", true);
                    Add(new Coroutine(WarningSound()));
                    yield return 1.5f;
                    arrowUp1.Visible = false;
                    arrowUp2.Visible = false;
                    warningSign1.Visible = false;
                    warningSign2.Visible = false;
                    warningSign3.Visible = false;
                    warningSign4.Visible = false;

                    // End
                    while (boss.Health > 0)
                    {
                        yield return null;
                    }
                    level.Remove(level.Tracker.GetEntity<BossHealthBar>());
                    level.Session.SetFlag("boss_Checkpoint", false);
                    ceiling1.RemoveSelf();
                    ceiling2.RemoveSelf();
                    ceiling3.RemoveSelf();
                    level.Add(ceiling2);
                    level.Add(ceiling3);
                    level.Displacement.AddBurst(refill1.Center, 0.5f, 8f, 32f, 0.5f);
                    refill1.RemoveSelf();
                    level.Displacement.AddBurst(refill2.Center, 0.5f, 8f, 32f, 0.5f);
                    refill2.RemoveSelf();
                    level.Displacement.AddBurst(refill3.Center, 0.5f, 8f, 32f, 0.5f);
                    refill3.RemoveSelf();
                    liquid.ReturnToOrigPosition();
                    string Prefix = level.Session.Area.LevelSet;
                    if (!XaphanModule.ModSaveData.SavedFlags.Contains(Prefix + "_Ch5_Boss_Defeated"))
                    {
                        XaphanModule.ModSaveData.SavedFlags.Add(Prefix + "_Ch5_Boss_Defeated");
                    }
                    if (XaphanModule.PlayerHasGolden)
                    {
                        if (!XaphanModule.ModSaveData.SavedFlags.Contains(Prefix + "_Ch5_Boss_Defeated_GoldenStrawberry"))
                        {
                            XaphanModule.ModSaveData.SavedFlags.Add(Prefix + "_Ch5_Boss_Defeated_GoldenStrawberry");
                        }
                    }
                    if (level.Session.GetFlag("boss_Challenge_Mode"))
                    {
                        level.Session.SetFlag("Boss_Defeated_CM", true);
                        if (!XaphanModule.ModSaveData.SavedFlags.Contains(Prefix + "_Ch5_Boss_Defeated_CM"))
                        {
                            XaphanModule.ModSaveData.SavedFlags.Add(Prefix + "_Ch5_Boss_Defeated_CM");
                        }
                        if (XaphanModule.PlayerHasGolden)
                        {
                            if (!XaphanModule.ModSaveData.SavedFlags.Contains(Prefix + "_Ch5_Boss_Defeated_CM_GoldenStrawberry"))
                            {
                                XaphanModule.ModSaveData.SavedFlags.Add(Prefix + "_Ch5_Boss_Defeated_CM_GoldenStrawberry");
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
                    SceneAs<Level>().Session.Audio.Music.Event = SFX.EventnameByHandle("event:/music/xaphan/lvl_0_item");
                    SceneAs<Level>().Session.Audio.Apply();
                }
                level.Session.SetFlag("boss_Normal_Mode_Given_Up", false);
                level.Session.SetFlag("boss_Challenge_Mode_Given_Up", false);
                level.Session.SetFlag("Boss_Defeated", true);
                level.Session.SetFlag("Genesis_Start", false);
                level.Session.SetFlag("Genesis_rise", false);
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
