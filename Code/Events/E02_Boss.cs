using System;
using System.Collections;
using Celeste.Mod.XaphanHelper.Cutscenes;
using Celeste.Mod.XaphanHelper.Entities;
using Celeste.Mod.XaphanHelper.UI_Elements;
using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste.Mod.XaphanHelper.Events
{
    class E02_Boss : CutsceneEntity
    {
        private Player player;

        private Vector2 bounds;

        private CustomFinalBoss boss;

        private CrumbleWallOnRumble ground;

        private ExitBlock cellingTop;

        private Decal cellingTopSprite;

        private NegaBlock cellingLeft;

        private FakeWall cellingLeftSprite;

        private TempleCrackedBlock cellingRight;

        private CoverupWall cellingRightSprite;

        private Booster booster;

        private CustomRefill refill;

        private JumpThru jumpThru1;

        private JumpThru jumpThru2;

        private JumpThru jumpThru3;

        private JumpThru jumpThru4;

        private JumpThru jumpThru5;

        private Spikes spikes;

        private CustomCrumbleBlock crumblePlatform1;

        private CustomCrumbleBlock crumblePlatform2;

        private CustomCrumbleBlock crumblePlatform3;

        private CustomCrumbleBlock crumblePlatform4;

        private Decal arrowUp;

        private Decal arrowDown;

        private Decal arrowLeft;

        private Decal arrowRight;

        private Decal warningSign;

        private Strawberry strawberry;

        public bool BossDefeated()
        {
            return XaphanModule.ModSaveData.SavedFlags.Contains("Xaphan/0_Ch2_Boss_Defeated" + (XaphanModule.PlayerHasGolden ? "_GoldenStrawberry" : ""));
        }

        public bool BossDefeatedCM()
        {
            return XaphanModule.ModSaveData.SavedFlags.Contains("Xaphan/0_Ch2_Boss_Defeated_CM" + (XaphanModule.PlayerHasGolden ? "_GoldenStrawberry" : ""));
        }

        public bool SpaceJumpCollected()
        {
            return XaphanModule.ModSaveData.SavedFlags.Contains("Xaphan/0_Upgrade_SpaceJump" + (XaphanModule.PlayerHasGolden ? "_GoldenStrawberry" : ""));
        }

        public bool DashBootsCollected()
        {
            return XaphanModule.ModSaveData.SavedFlags.Contains("Xaphan/0_Upgrade_DashBoots" + (XaphanModule.PlayerHasGolden ? "_GoldenStrawberry" : ""));
        }

        public bool HadDashBoots;

        public E02_Boss(Player player, Level level)
        {
            this.player = player;
            Level = level;
            bounds = new Vector2(level.Bounds.Left, level.Bounds.Top);
            boss = level.Entities.FindFirst<CustomFinalBoss>();
            ground = level.Entities.FindFirst<CrumbleWallOnRumble>();
            cellingTop = level.Entities.FindFirst<ExitBlock>();
            cellingTopSprite = new Decal("Xaphan/Ch2/boss_top_celling.png", bounds + new Vector2(160f, -32f), new Vector2(1f, 1f), -10000);
            cellingLeft = level.Entities.FindFirst<NegaBlock>();
            cellingLeftSprite = level.Entities.FindFirst<FakeWall>();
            cellingRight = level.Entities.FindFirst<TempleCrackedBlock>();
            cellingRightSprite = level.Entities.FindFirst<CoverupWall>();
            booster = new Booster(bounds, true);
            refill = new CustomRefill(bounds, "Max Dashes", false, 2.5f);
            jumpThru1 = new JumpthruPlatform(bounds + new Vector2(144f, 80f), 32, "Xaphan/abyss_a", 8);
            jumpThru2 = new JumpthruPlatform(bounds + new Vector2(64f, 160f), 24, "Xaphan/abyss_a", 8);
            jumpThru3 = new JumpthruPlatform(bounds + new Vector2(232f, 160f), 24, "Xaphan/abyss_a", 8);
            jumpThru4 = new JumpthruPlatform(bounds, 24, "Xaphan/abyss_a", 8);
            jumpThru5 = new JumpthruPlatform(bounds, 24, "Xaphan/abyss_a", 8);
            crumblePlatform1 = new CustomCrumbleBlock(bounds, Vector2.Zero, 24, 8, 2f, 0.6f, false, false, texture: "objects/Xaphan/CustomCrumbleBlock/abyss_c", lightOccludeValue: 0.2f);
            crumblePlatform2 = new CustomCrumbleBlock(bounds, Vector2.Zero, 24, 8, 2f, 0.6f, false, false, texture: "objects/Xaphan/CustomCrumbleBlock/abyss_c", lightOccludeValue: 0.2f);
            spikes = new Spikes(bounds, 48, Spikes.Directions.Down, "Xaphan/abyss_b");
            arrowUp = new Decal("Xaphan/Common/arrow_up00.png", bounds, new Vector2(1f, 1f), 1);
            arrowDown = new Decal("Xaphan/Common/arrow_down00.png", bounds, new Vector2(1f, 1f), 1);
            arrowLeft = new Decal("Xaphan/Common/arrow_left00.png", bounds, new Vector2(1f, 1f), 1);
            arrowRight = new Decal("Xaphan/Common/arrow_right00.png", bounds, new Vector2(1f, 1f), 1);
            warningSign = new Decal("Xaphan/Common/warning00.png", bounds + new Vector2(160f, 304f), new Vector2(1f, 1f), 1);
            strawberry = level.Entities.FindFirst<Strawberry>();
        }

        public override void OnBegin(Level level)
        {
            level.Session.SetFlag("In_bossfight", false);
            level.InCutscene = false;
            level.CancelCutscene();
            if (!level.Session.GetFlag("boss_Checkpoint"))
            {
                level.CameraOffset = new Vector2(0f, 3f * 32f);
            }
            Add(new Coroutine(Cutscene(level)));
        }

        public override void Update()
        {
            base.Update();
            Add(new Coroutine(ChangeLight()));
        }

        public override void OnEnd(Level level)
        {

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

        public IEnumerator ChangeLight()
        {
            float lightingStart = Level.Lighting.Alpha;
            float lightingEnd = Level.Session.GetFlag("boss_Challenge_Mode") ? Level.BaseLightingAlpha + 0.5f : (Level.BaseLightingAlpha + 0.15f);
            bool lightingWait = lightingStart >= Level.BaseLightingAlpha + 0.5f || lightingEnd >= Level.BaseLightingAlpha + 0.5f;
            if (lightingEnd > lightingStart && lightingWait)
            {
                if (!Level.Session.GetFlag("Boss_Appeared"))
                {
                    Audio.Play("event:/game/05_mirror_temple/room_lightlevel_down");
                }
                while (Level.Lighting.Alpha != lightingEnd)
                {
                    yield return null;
                    if (!Level.Session.GetFlag("Boss_Appeared"))
                    {
                        Level.Lighting.Alpha = Calc.Approach(Level.Lighting.Alpha, lightingEnd, 0.4f * Engine.DeltaTime);
                    }
                    else
                    {
                        Level.Lighting.Alpha = Calc.Approach(Level.Lighting.Alpha, lightingEnd, 20f * Engine.DeltaTime);
                    }
                }
            }
            if (lightingEnd < lightingStart && lightingWait)
            {
                Audio.Play("event:/game/05_mirror_temple/room_lightlevel_up");
                while (Level.Lighting.Alpha != lightingEnd)
                {
                    yield return null;
                    Level.Lighting.Alpha = Calc.Approach(Level.Lighting.Alpha, lightingEnd, 0.4f * Engine.DeltaTime);
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

        public void ShakeBlocks()
        {
            Level.Shake(0.3f);
            Audio.Play("event:/game/general/fallblock_impact", player.Position);
        }

        public IEnumerator TriggerCellingDoors(bool open, int duration, int speed)
        {
            if (open)
            {
                speed = -speed;
            }
            int num = 1;
            do
            {
                cellingLeft.MoveToX(cellingLeft.Position.X + speed);
                cellingLeftSprite.Position = cellingLeftSprite.Position + new Vector2(speed, 0f);
                cellingRight.MoveToX(cellingRight.Position.X - speed);
                cellingRightSprite.Position = cellingRightSprite.Position + new Vector2(-speed, 0f);
                num = num + 1;
                yield return null;
            } while (num <= duration);
            if (!open)
            {
                ShakeBlocks();
            }
        }

        public IEnumerator TriggerWall(string wall, bool raise, int duration, int speed)
        {
            if (raise)
            {
                speed = -speed;
            }
            if (wall == "ground")
            {
                int num = 1;
                do
                {
                    ground.MoveToY(ground.Position.Y + speed);
                    num = num + 1;
                    yield return null;
                } while (num <= duration);
            }
            if (wall == "cellingTop")
            {
                int num = 1;
                while (num <= duration)
                {
                    cellingTop.MoveToY(cellingTop.Position.Y + speed);
                    cellingTopSprite.Position += new Vector2(0f, speed);
                    spikes.Position += new Vector2(0f, speed);
                    num = num + 1;
                    yield return null;
                }
                jumpThru1.RemoveSelf();
            }
        }

        public IEnumerator Cutscene(Level level)
        {
            if (!BossDefeated() || HasGolden() || (BossDefeated() && level.Session.GetFlag("boss_Normal_Mode") || (BossDefeated() && level.Session.GetFlag("boss_Challenge_Mode"))))
            {
                level.Add(cellingTopSprite);
                if (level.Session.GetFlag("boss_Checkpoint"))
                {
                    var randCp = new Random();
                    boss.Position = boss.nodes[randCp.Next(5, 9)];
                    level.CameraOffset = new Vector2(0, -1f * 32f);
                    boss.SetHits(7);
                    refill.Position = bounds + new Vector2(160f, 120f);
                    level.Add(jumpThru1);
                    level.Add(jumpThru2);
                    level.Add(jumpThru3);
                }
                level.Add(refill);
                level.Displacement.AddBurst(refill.Center, 0.5f, 8f, 32f, 0.5f);
                ChallengeMote CMote = level.Tracker.GetEntity<ChallengeMote>();
                if (level.Session.GetFlag("boss_Challenge_Mode"))
                {
                    if (CMote != null)
                    {
                        level.Session.SetFlag("Boss_Defeated", false);
                    }
                }
                // Wait for player to trigger fight start
                while (player.Position.X <= level.Bounds.Left + level.Bounds.Width / 2 && !level.Session.GetFlag("Boss_Appeared"))
                {
                    yield return null;
                }

                // Initialize fight
                while (!boss.playerHasMoved && !level.Session.GetFlag("boss_Normal_Mode_Given_Up") && !level.Session.GetFlag("boss_Challenge_Mode_Given_Up"))
                {
                    yield return null;
                }
                if (level.Session.GetFlag("boss_Normal_Mode_Given_Up") || level.Session.GetFlag("boss_Challenge_Mode_Given_Up"))
                {
                    if (SpaceJumpCollected())
                    {
                        level.Session.Audio.Music.Event = SFX.EventnameByHandle("event:/music/xaphan/lvl_2_abyss_deeper");
                        level.Session.Audio.Apply();
                    }
                    else
                    {
                        level.Session.Audio.Music.Event = SFX.EventnameByHandle("event:/music/xaphan/lvl_0_item");
                        level.Session.Audio.Apply();
                    }
                    if (level.Session.GetFlag("boss_Normal_Mode_Given_Up") && level.Session.GetFlag("boss_Checkpoint"))
                    {
                        level.Session.RespawnPoint = level.GetSpawnPoint(bounds + new Vector2(160f, 320f));
                        player.StateMachine.State = 11;
                        yield return new FadeWipe(Scene, wipeIn: false).Duration = 0.5f;
                        level.CameraOffset = new Vector2(0, 10f * 32f);
                        level.Camera.Position = bounds + new Vector2(0, 184f);
                        player.Position = bounds + new Vector2(160f, 320f);
                        yield return new FadeWipe(Scene, wipeIn: true);
                        player.StateMachine.State = 0;
                        level.Session.SetFlag("boss_Checkpoint", false);
                    }
                }
                else
                {
                    if (!level.Session.GetFlag("Boss_Appeared"))
                    {
                        level.Session.Audio.Music.Event = SFX.EventnameByHandle("event:/music/xaphan/lvl_0_tension");
                        level.Session.Audio.Apply();
                    }
                    booster.Visible = false;
                    if (level.Session.GetFlag("boss_Challenge_Mode"))
                    {
                        spikes.Position = bounds + new Vector2(136f, 16f);
                        level.Add(spikes);
                    }
                    else
                    {
                        level.Add(jumpThru2);
                        level.Add(jumpThru3);
                    }
                    level.Add(jumpThru1);

                    if (level.Session.GetFlag("boss_Checkpoint"))
                    {
                        // Skip phase 1 and half of Phase 2
                        boss.Visible = true;
                        boss.Collidable = true;
                        Audio.Play("event:/char/badeline/appear", boss.Position);
                        level.Displacement.AddBurst(boss.Center, 0.5f, 24f, 96f, 0.4f);
                        level.Session.Audio.Music.Event = SFX.EventnameByHandle("event:/music/xaphan/lvl_0_mini_boss");
                        level.Session.Audio.Apply();
                        level.Session.SetFlag("Boss_Appeared", true);
                        level.Session.SetFlag("In_bossfight", true);
                        level.Add(new BossHealthBar(boss, 15, 7));
                        Add(new Coroutine(TriggerWall("ground", true, 24, 1)));
                    }
                    else
                    {
                        // Phase 1
                        var rand = new Random();
                        boss.Position = boss.nodes[rand.Next(1, 5)];
                        boss.Visible = true;
                        boss.Collidable = true;
                        Audio.Play("event:/char/badeline/appear", boss.Position);
                        level.Displacement.AddBurst(boss.Center, 0.5f, 24f, 96f, 0.4f);
                        level.Session.Audio.Music.Event = SFX.EventnameByHandle("event:/music/xaphan/lvl_0_mini_boss");
                        level.Session.Audio.Apply();
                        level.Session.SetFlag("Boss_Appeared", true);
                        level.Session.SetFlag("In_bossfight", true);
                        level.Add(new BossHealthBar(boss, 15, level.Session.GetFlag("boss_Challenge_Mode") ? 0 : 7, level.Session.GetFlag("boss_Challenge_Mode")));
                        level.Session.RespawnPoint = level.GetSpawnPoint(bounds + new Vector2(160f, 320f));

                        // Phase 2
                        while (boss.hits < 3)
                        {
                            yield return null;
                        }
                        Add(new Coroutine(TriggerCellingDoors(true, 24, 2)));
                        booster.Position = bounds + new Vector2(160f, 232f);
                        level.Add(booster);
                        yield return 0.1f;
                        booster.Appear();
                        refill.Position = bounds + new Vector2(160f, 120f);
                        level.CameraOffset = new Vector2(0, 0);
                        arrowUp.Position = booster.Position + new Vector2(0f, 32f);
                        level.CameraOffset = new Vector2(0, -1f * 32f);
                        level.Add(arrowUp);
                        level.Add(warningSign);
                        Add(new Coroutine(WarningSound()));
                        yield return 1.5f;
                        arrowUp.Visible = false;
                        warningSign.Visible = false;
                        Add(new Coroutine(TriggerWall("ground", true, 24, 1)));
                        while (player.Position.Y >= level.Bounds.Top + level.Bounds.Height / 2)
                        {
                            yield return null;
                        }
                        Add(new Coroutine(TriggerCellingDoors(false, 24, 2)));
                        level.CameraOffset = new Vector2(0, -1.5f * 32f);
                        if (level.Session.GetFlag("boss_Challenge_Mode"))
                        {
                            while (boss.hits < 7)
                            {
                                yield return null;
                            }
                            level.Displacement.AddBurst(refill.Center, 0.5f, 8f, 32f, 0.5f);
                            refill.RemoveSelf();
                            crumblePlatform3 = new CustomCrumbleBlock(bounds + new Vector2(128f, 152f), Vector2.Zero, 16, 8, 2f, 0.6f, false, false, texture: "objects/Xaphan/CustomCrumbleBlock/abyss_c", lightOccludeValue: 0.2f);
                            level.Add(crumblePlatform3);
                            level.Displacement.AddBurst(crumblePlatform3.Center, 0.5f, 8f, 32f, 0.5f);
                            crumblePlatform4 = new CustomCrumbleBlock(bounds + new Vector2(176f, 152f), Vector2.Zero, 16, 8, 2f, 0.6f, false, false, texture: "objects/Xaphan/CustomCrumbleBlock/abyss_c", lightOccludeValue: 0.2f);
                            level.Add(crumblePlatform4);
                            level.Displacement.AddBurst(crumblePlatform4.Center, 0.5f, 8f, 32f, 0.5f);
                            warningSign.Position = bounds + new Vector2(160f, 56f);
                            arrowLeft.Position = warningSign.Position + new Vector2(-32f, 0f);
                            level.Add(arrowLeft);
                            arrowRight.Position = warningSign.Position + new Vector2(32f, 0f);
                            level.Add(arrowRight);
                            warningSign.Visible = true;
                            Add(new Coroutine(WarningSound()));
                            yield return 1.5f;
                            arrowLeft.Visible = false;
                            arrowRight.Visible = false;
                            warningSign.Visible = false;
                            Add(new Coroutine(TriggerWall("cellingTop", false, 8, 8)));
                        }
                        else
                        {
                            while (boss.hits < 7)
                            {
                                yield return null;
                            }
                            level.Session.SetFlag("boss_Checkpoint");
                            level.Session.RespawnPoint = level.GetSpawnPoint(bounds + new Vector2(160f, 80f));
                        }
                    }

                    // Phase 3
                    while (boss.hits < 11)
                    {
                        yield return null;
                    }
                    Add(new Coroutine(TriggerCellingDoors(true, 24, 2)));
                    booster.RemoveSelf();
                    if (refill != null)
                    {
                        level.Displacement.AddBurst(refill.Center, 0.5f, 8f, 32f, 0.5f);
                        refill.RemoveSelf();
                    }
                    if (crumblePlatform3 != null && crumblePlatform4 != null)
                    {
                        level.Displacement.AddBurst(crumblePlatform3.Center, 0.5f, 8f, 32f, 0.5f);
                        crumblePlatform3.RemoveSelf();
                        level.Displacement.AddBurst(crumblePlatform4.Center, 0.5f, 8f, 32f, 0.5f);
                        crumblePlatform4.RemoveSelf();
                    }
                    if (level.Session.GetFlag("boss_Challenge_Mode"))
                    {
                        crumblePlatform1.Position = bounds + new Vector2(104f, 296f);
                        crumblePlatform2.Position = bounds + new Vector2(192f, 296f);
                        level.Add(crumblePlatform1);
                        level.Displacement.AddBurst(crumblePlatform1.Center, 0.5f, 8f, 32f, 0.5f);
                        level.Add(crumblePlatform2);
                        level.Displacement.AddBurst(crumblePlatform2.Center, 0.5f, 8f, 32f, 0.5f);
                    }
                    else
                    {
                        jumpThru4.Position = bounds + new Vector2(104f, 296f);
                        jumpThru5.Position = bounds + new Vector2(192f, 296f);
                        level.Add(jumpThru4);
                        level.Displacement.AddBurst(jumpThru4.Center, 0.5f, 8f, 32f, 0.5f);
                        level.Add(jumpThru5);
                        level.Displacement.AddBurst(jumpThru5.Center, 0.5f, 8f, 32f, 0.5f);
                    }
                    level.CameraOffset = new Vector2(0, 1.5f * 32f);
                    arrowDown.Position = bounds + new Vector2(160, 160);
                    arrowDown.Visible = true;
                    level.Add(arrowDown);
                    Add(new Coroutine(WarningSound()));
                    yield return 1.5f;
                    arrowDown.RemoveSelf();
                    while (player.Position.Y <= level.Bounds.Top + level.Bounds.Height / 2 + 40)
                    {
                        yield return null;
                    }
                    Add(new Coroutine(TriggerCellingDoors(false, 24, 2)));
                    level.CameraOffset = new Vector2(0, 10f * 32f);

                    // End
                    while (boss.hits < 15)
                    {
                        yield return null;
                    }
                    level.Remove(level.Tracker.GetEntity<BossHealthBar>());
                    level.Session.SetFlag("boss_Checkpoint", false);
                    level.Session.RespawnPoint = level.GetSpawnPoint(bounds + new Vector2(160f, 320f));
                    jumpThru2.RemoveSelf();
                    jumpThru3.RemoveSelf();
                    if (DashBootsCollected())
                    {
                        level.Session.SetFlag("Upgrade_DashBoots", false);
                        XaphanModule.ModSettings.DashBoots = false;
                        HadDashBoots = true;
                    }
                    if (level.Session.GetFlag("boss_Challenge_Mode"))
                    {
                        level.Displacement.AddBurst(crumblePlatform1.Center, 0.5f, 8f, 32f, 0.5f);
                        crumblePlatform1.RemoveSelf();
                        level.Displacement.AddBurst(crumblePlatform2.Center, 0.5f, 8f, 32f, 0.5f);
                        crumblePlatform2.RemoveSelf();
                        Add(new Coroutine(TriggerWall("cellingTop", true, 8, 8)));
                        level.Session.Audio.Music.Event = SFX.EventnameByHandle("event:/music/xaphan/lvl_2_abyss_deeper");
                        level.Session.Audio.Apply();
                    }
                    else
                    {
                        level.Displacement.AddBurst(jumpThru4.Center, 0.5f, 8f, 32f, 0.5f);
                        jumpThru4.RemoveSelf();
                        level.Displacement.AddBurst(jumpThru5.Center, 0.5f, 8f, 32f, 0.5f);
                        jumpThru5.RemoveSelf();
                        level.Session.Audio.Music.Event = SFX.EventnameByHandle("event:/music/xaphan/lvl_0_item");
                        level.Session.Audio.Apply();
                    }
                    Add(new Coroutine(TriggerWall("ground", false, 8, 3)));
                    while (!player.OnGround())
                    {
                        yield return null;
                    }
                    string Prefix = level.Session.Area.LevelSet;
                    if (!HasGolden() && !level.Session.GetFlag("boss_Normal_Mode") && !level.Session.GetFlag("boss_Challenge_Mode"))
                    {
                        Scene.Add(new CS02_BossDefeated(player));
                    }
                    else
                    {
                        level.Session.SetFlag("Boss_Defeated", true);
                        if (!XaphanModule.ModSaveData.SavedFlags.Contains(Prefix + "_Ch2_Boss_Defeated"))
                        {
                            XaphanModule.ModSaveData.SavedFlags.Add(Prefix + "_Ch2_Boss_Defeated");
                        }
                        if (XaphanModule.PlayerHasGolden)
                        {
                            if (!XaphanModule.ModSaveData.SavedFlags.Contains(Prefix + "_Ch2_Boss_Defeated_GoldenStrawberry"))
                            {
                                XaphanModule.ModSaveData.SavedFlags.Add(Prefix + "_Ch2_Boss_Defeated_GoldenStrawberry");
                            }
                        }
                    }
                    level.Session.SetFlag("Boss_Defeated", true);
                    if (level.Session.GetFlag("boss_Challenge_Mode"))
                    {
                        level.Session.SetFlag("Boss_Defeated_CM", true);
                        if (!XaphanModule.ModSaveData.SavedFlags.Contains(Prefix + "_Ch2_Boss_Defeated_CM"))
                        {
                            XaphanModule.ModSaveData.SavedFlags.Add(Prefix + "_Ch2_Boss_Defeated_CM");
                        }
                        if (XaphanModule.PlayerHasGolden)
                        {
                            if (!XaphanModule.ModSaveData.SavedFlags.Contains(Prefix + "_Ch2_Boss_Defeated_CM_GoldenStrawberry"))
                            {
                                XaphanModule.ModSaveData.SavedFlags.Add(Prefix + "_Ch2_Boss_Defeated_CM_GoldenStrawberry");
                            }
                        }
                    }
                    level.Session.SetFlag("In_bossfight", false);
                    if (HadDashBoots)
                    {
                        level.Session.SetFlag("Upgrade_DashBoots", true);
                        XaphanModule.ModSettings.DashBoots = true;
                    }
                    level.Session.SetFlag("boss_Normal_Mode", false);
                    if (level.Session.GetFlag("boss_Challenge_Mode"))
                    {
                        CMote.ManageUpgrades(level, true);
                        CMote.Visible = true;
                        level.Session.SetFlag("boss_Challenge_Mode", false);
                    }
                }
                level.Session.SetFlag("boss_Normal_Mode_Given_Up", false);
                level.Session.SetFlag("boss_Challenge_Mode_Given_Up", false);
                if (XaphanModule.ModSettings.ShowMiniMap)
                {
                    MapDisplay mapDisplay = SceneAs<Level>().Tracker.GetEntity<MapDisplay>();
                    if (mapDisplay != null)
                    {
                        AreaKey area = SceneAs<Level>().Session.Area;
                        int chapterIndex = area.ChapterIndex == -1 ? 0 : area.ChapterIndex;
                        mapDisplay.GenerateIcons();
                    }
                }
            }

            // Do nothing anymore unless boss hits got reset
            while (boss.hits > 0)
            {
                yield return null;
            }
            Add(new Coroutine(Cutscene(level)));
        }
    }
}
