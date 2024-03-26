using System.Collections;
using Celeste.Mod.XaphanHelper.Cutscenes;
using Celeste.Mod.XaphanHelper.Entities;
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

        private Decal warningSign1;

        private Decal warningSign2;

        private Decal warningSign5;

        private Decal warningSign6;

        private CustomRefill refill1;

        private CustomRefill refill2;

        public bool BossDefeated()
        {
            return XaphanModule.ModSaveData.SavedFlags.Contains("Xaphan/0_Ch1_Boss_Defeated" + (XaphanModule.PlayerHasGolden ? "_GoldenStrawberry" : ""));
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
            refill1 = new CustomRefill(jumpThru3.Position + new Vector2(16f, -64f), "Max Dashes", false, 2.5f);
            refill2 = new CustomRefill(jumpThru4.Position + new Vector2(16f, -64f), "Max Dashes", false, 2.5f);
            warningSign1 = new Decal("Xaphan/Common/warning00.png", jumpThru1.Position + new Vector2(16f, -16f), new Vector2(1f, 1f), 1);
            warningSign2 = new Decal("Xaphan/Common/warning00.png", jumpThru2.Position + new Vector2(16f, -16f), new Vector2(1f, 1f), 1);
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
            if (!BossDefeated() || HasGolden() || (BossDefeated() && level.Session.GetFlag("boss_Challenge_Mode")))
            {
                while (player.Right > jumpThru6.Right && !player.OnGround())
                {
                    yield return null;
                }
                if (player.Dead)
                {
                    yield break;
                }
                SceneAs<Level>().Session.SetFlag("Torizo_Start", false);
                if (!level.Session.GetFlag("CS01_BossStart"))
                {
                    Scene.Add(new CS01_BossStart(player, boss));
                    while (!level.Session.GetFlag("Torizo_Start"))
                    {
                        yield return null;
                    }
                }
                else
                {
                    while (!boss.playerHasMoved)
                    {
                        yield return null;
                    }
                    SceneAs<Level>().Session.SetFlag("Torizo_Start", true);
                }

                // Phase 1
                level.Session.SetFlag("In_bossfight", true);
                level.Session.Audio.Music.Event = SFX.EventnameByHandle("event:/music/xaphan/lvl_0_mini_boss");
                level.Session.Audio.Apply();
                level.Session.RespawnPoint = level.GetSpawnPoint(bounds + new Vector2(448f, 152f));

                // Phase 2
                while (boss.Health >= 13)
                {
                    yield return null;
                }
                level.Add(warningSign2);
                level.Add(warningSign5);
                Add(new Coroutine(WarningSound()));
                yield return 1.5f;
                warningSign2.Visible = false;
                warningSign5.Visible = false;
                level.Displacement.AddBurst(jumpThru2.Center, 0.5f, 8f, 32f, 0.5f);
                jumpThru2.RemoveSelf();
                level.Displacement.AddBurst(jumpThru5.Center, 0.5f, 8f, 32f, 0.5f);
                jumpThru5.RemoveSelf();

                // Phase 3
                while (boss.Health >= 5)
                {
                    yield return null;
                }
                level.Add(warningSign1);
                level.Add(warningSign6);
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
                level.Add(refill2);
                level.Displacement.AddBurst(refill2.Center, 0.5f, 8f, 32f, 0.5f);

                // End
                while (boss.Health > 0)
                {
                    yield return null;
                }
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
                level.Displacement.AddBurst(refill2.Center, 0.5f, 8f, 32f, 0.5f);
                refill2.RemoveSelf();
                level.Session.SetFlag("D-07_Gate_1", false);
                level.Session.SetFlag("Torizo_Defeated", true);
                level.Session.SetFlag("Torizo_Wakeup", false);
                level.Session.SetFlag("Torizo_Start", false);
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
                level.Session.Audio.Music.Event = SFX.EventnameByHandle("event:/music/xaphan/lvl_0_item");
                level.Session.Audio.Apply();
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
