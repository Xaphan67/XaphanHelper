using System;
using System.Collections;
using Celeste.Mod.XaphanHelper.UI_Elements;
using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste.Mod.XaphanHelper.Cutscenes
{
    class CS_Credits : CutsceneEntity
    {
        public class TimeDisplay : Component
        {
            public Vector2 Position;

            public string Time;

            public TimeDisplay(string time, float positionX, float positionY)
                : base(active: true, visible: true)
            {
                Time = time;
                Position = new Vector2(positionX, positionY);
            }

            public void UpdatePosition(Vector2 to)
            {
                Position.Y += (int)to.Y;
            }

            public override void Render()
            {
                SpeedrunTimerDisplay.DrawTime(Position, Time);
            }
        }

        private const int TotalItems = 57;

        private readonly Player player;

        public static CS_Credits Instance;

        private IntroText intro;

        private IntroText endTextA;

        private IntroText endTextB;

        private IntroText endTextC;

        private IntroText endTextD;

        private IntroText endTextE;

        private CustomCredits credits;

        private Sprite Bg;

        private bool DrawBlackBg;

        private bool Skipped;

        private Wiggler skipWiggle;

        private float skipWiggleDelay;

        public bool DashBootsCollected(Level level)
        {
            return XaphanModule.ModSaveData.SavedFlags.Contains(level.Session.Area.LevelSet + "_Upgrade_DashBoots");
        }

        public bool PowerGripCollected(Level level)
        {
            return XaphanModule.ModSaveData.SavedFlags.Contains(level.Session.Area.LevelSet + "_Upgrade_PowerGrip");
        }

        public bool ClimbingKitCollected(Level level)
        {
            return XaphanModule.ModSaveData.SavedFlags.Contains(level.Session.Area.LevelSet + "_Upgrade_ClimbingKit");
        }

        public bool BombsCollected(Level level)
        {
            return XaphanModule.ModSaveData.SavedFlags.Contains(level.Session.Area.LevelSet + "_Upgrade_Bombs");
        }

        public bool SpaceJumpCollected(Level level)
        {
            return XaphanModule.ModSaveData.SavedFlags.Contains(level.Session.Area.LevelSet + "_Upgrade_SpaceJump");
        }

        public CS_Credits(Player player)
        {
            this.player = player;
            Instance = this;
            Tag = (Tags.Global | Tags.HUD);
            player.StateMachine.State = 11;
            player.DummyAutoAnimate = false;
            player.Sprite.Rate = 0f;
            RemoveOnSkipped = false;
            Bg = new Sprite(GFX.Gui, "credits/Xaphan/bg_credits");
            Bg.Position = Vector2.Zero + new Vector2(682, 6);
            Bg.Add("colored", "", 0);
            Bg.Add("toGrayscale", "", 0.06f, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18, 19, 20);
            Bg.Add("grayscale", "", 0, 20);
            Bg.Visible = false;
            Add(skipWiggle = Wiggler.Create(0.4f, 4f));
        }

        public override void OnBegin(Level level)
        {
            Add(new Coroutine(Cutscene(level)));
        }

        public override void Added(Scene scene)
        {
            base.Added(scene);
            (base.Scene as Level).InCredits = true;
        }

        public override void Update()
        {
            if (credits != null)
            {
                credits.Update();
            }
            SceneAs<Level>().TimerStopped = true;
            SceneAs<Level>().TimerHidden = true;
            SceneAs<Level>().SaveQuitDisabled = true;
            SceneAs<Level>().PauseLock = true;
            SceneAs<Level>().AllowHudHide = false;
            Audio.SetAmbience(null);
            if (Input.ESC.Check && skipWiggleDelay <= 0f)
            {
                skipWiggle.Start();
                skipWiggleDelay = 0.5f;
            }
            skipWiggleDelay -= Engine.DeltaTime;
            base.Update();
        }

        public override void Render()
        {
            if (DrawBlackBg)
            {
                Draw.Rect(-10f, -10f, 1940f, 1100f, Color.Black);
            }
            bool flag = SaveData.Instance != null && SaveData.Instance.Assists.MirrorMode;
            if (intro != null && !Level.Paused)
            {
                intro.Render();
            }
            if (credits != null && !Level.Paused)
            {
                credits.Render(new Vector2(flag ? 100 : 1820, 0f));
            }
            if (Bg != null && Bg.Visible)
            {
                Bg.Render();
            }
            if (credits != null)
            {
                float num = 0.5f;
                string label = Dialog.Clean("XaphanHelper_UI_skip");
                MTexture buttonTexture = Input.GuiButton(Input.ESC, "controls/keyboard/oemquestion");
                int buttonTextureWidth = buttonTexture.Width;
                float num3 = ButtonUI.Width(label, Input.ESC);
                Vector2 position = new(0f, 1045f);
                position.X = 1920 - num3 / 2 + buttonTextureWidth + (Settings.Instance.Language == "french" ? 19 : 0);
                ButtonUI.Render(position, label, Input.ESC, num, 1f, skipWiggle.Value * 0.05f);
            }
            base.Render();
        }

        public override void OnEnd(Level level)
        {
            Audio.SetMusicParam("fade", 0);
            level.CompleteArea(spotlightWipe: false, skipScreenWipe: true, skipCompleteScreen: true);
        }

        public IEnumerator Cutscene(Level level)
        {
            Scene.Add(new FadeWipe(level, wipeIn: false)
            {
                Duration = 2f
            });
            float fade = 1f;
            while (fade > 0f)
            {
                yield return null;
                Audio.SetMusicParam("fade", fade);
                fade -= Engine.DeltaTime;
            }
            DrawBlackBg = true;
            level.Session.Audio.Apply();
            float timer = 3f;
            intro = new IntroText("Xaphan_0_Credits_Intro", "Middle", Engine.Height / 2, Color.White, 1f);
            Scene.Add(intro);
            intro.Show = true;
            while (timer > 0f)
            {
                yield return null;
                timer -= Engine.DeltaTime;
            }
            yield return new FadeWipe(level, wipeIn: false)
            {
                Duration = 2.25f
            }.Duration;
            intro.Visible = false;
            intro.RemoveSelf();
            yield return 1f;
            level.Session.Audio.Music.Event = SFX.EventnameByHandle("event:/music/xaphan/menu/credits");
            level.Session.Audio.Apply();
            yield return 1.5f;
            credits = new CustomCredits(1.085f);
            credits.AllowInput = false;
            credits.Enabled = true;
            while (credits.BottomTimer <= 3f && !Skipped)
            {
                if (Input.ESC.Pressed)
                {
                    Skipped = true;
                }
                yield return null;
            }
            if (!Skipped)
            {
                yield return new FadeWipe(level, wipeIn: false)
                {
                    Duration = 2.25f
                }.Duration;
                credits = null;
                yield return 1f;
                Add(Bg);
                Bg.Play("colored");
                Bg.Visible = true;
                yield return new FadeWipe(level, wipeIn: true)
                {
                    Duration = 1.25f
                }.Duration;
                yield return 0.5f;
                Bg.Play("toGrayscale");
                yield return 0.5f;
            }
            else
            {
                yield return new FadeWipe(level, wipeIn: false)
                {
                    Duration = 1f
                }.Duration;
                credits = null;
                fade = 1f;
                while (fade > 0f)
                {
                    yield return null;
                    Audio.SetMusicParam("fade", fade);
                    fade -= Engine.DeltaTime;
                }
                Add(Bg);
                Bg.Play("grayscale");
                Bg.Visible = true;
                Scene.Add(new FadeWipe(level, wipeIn: true)
                {
                    Duration = 1f
                });
            }
            int TotalUpgradeCount = 0;
            int TotalStrawberryCount = 0;
            int TotalheartCount = 0;
            int TotalCassetteCount = 0;
            if (DashBootsCollected(level))
            {
                TotalUpgradeCount += 1;
            }
            if (PowerGripCollected(level))
            {
                TotalUpgradeCount += 1;
            }
            if (ClimbingKitCollected(level))
            {
                TotalUpgradeCount += 1;
            }
            if (BombsCollected(level))
            {
                TotalUpgradeCount += 1;
            }
            if (SpaceJumpCollected(level))
            {
                TotalUpgradeCount += 1;
            }
            foreach (AreaStats item in SaveData.Instance.Areas_Safe)
            {
                if (item.LevelSet == "Xaphan/0")
                {
                    AreaModeStats areaModeStats = item.Modes[0];
                    int strawberryCount = 0;
                    if (areaModeStats.TotalStrawberries > 0 || item.TotalStrawberries > 0)
                    {
                        strawberryCount = item.TotalStrawberries;
                    }
                    int heartCount = 0;
                    if (areaModeStats.HeartGem)
                    {
                        heartCount += 1;
                    }
                    int cassetteCount = 0;
                    if (item.Cassette)
                    {
                        cassetteCount += 1;
                    }
                    TotalStrawberryCount += strawberryCount;
                    TotalheartCount += heartCount;
                    TotalCassetteCount += cassetteCount;
                }
            }
            TimeSpan timespan = TimeSpan.FromTicks(XaphanModule.ModSaveData.SavedTime.ContainsKey(SaveData.Instance.CurrentSession.Area.LevelSet) ? XaphanModule.ModSaveData.SavedTime[SaveData.Instance.CurrentSession.Area.LevelSet] : 0L);
            string gameTime = ((int)timespan.TotalHours).ToString() + timespan.ToString("\\:mm\\:ss\\.fff");
            float timeWidth = SpeedrunTimerDisplay.GetTimeWidth(gameTime);
            TimeDisplay totaltime = new(gameTime, 960 - timeWidth / 2, Engine.Height / 2 + 146);
            int TotalItemsCollected = TotalUpgradeCount + TotalStrawberryCount + TotalheartCount + TotalCassetteCount;
            double ItemPercentage = Math.Round(TotalItemsCollected * 100f / TotalItems, 0, MidpointRounding.AwayFromZero);
            if (!Skipped)
            {
                Scene.Add(endTextA = new IntroText("Xaphan_0_Credits_ClearTime", "Middle", Engine.Height / 2 + 35, Color.DarkGreen, 1f, outline: true)
                {
                    Show = true
                });
                yield return 2f;
                Add(totaltime);
                yield return 2f;
                for (int i = 0; i <= 25; i++)
                {
                    endTextA.UpdatePosition(new Vector2(0, -6));
                    totaltime.UpdatePosition(new Vector2(0, -6));
                    yield return null;
                }
                yield return 1f;
                Scene.Add(endTextB = new IntroText("Xaphan_0_Credits_CollectedItems", "Middle", Engine.Height / 2 + 75, Color.DarkGreen, 1f, outline: true)
                {
                    Show = true
                });
                yield return 1f;
                Scene.Add(endTextC = new IntroText("Xaphan_0_Credits_CollectedItems_b", "Middle", Engine.Height / 2 + 155, Color.DarkGreen, 1f, outline: true)
                {
                    Show = true
                });
                yield return 2f;
                Scene.Add(endTextD = new IntroText(ItemPercentage.ToString() + "%", "Middle", Engine.Height / 2 + 265, Color.Gold, 1.5f, false, true)
                {
                    Show = true
                });
                yield return 2f;
                for (int i = 0; i <= 25; i++)
                {
                    endTextB.UpdatePosition(new Vector2(0, 6));
                    endTextC.UpdatePosition(new Vector2(0, 6));
                    endTextD.UpdatePosition(new Vector2(0, 6));
                    yield return null;
                }
                yield return 1f;
                Scene.Add(endTextE = new IntroText("Xaphan_0_Credits_End", "Middle", Engine.Height / 2 + 95, Color.White, 1.5f, outline: true)
                {
                    Show = true
                });
                yield return 1f;
            }
            else
            {
                level.Session.Audio.Music.Event = SFX.EventnameByHandle("event:/music/xaphan/menu/credits_end");
                level.Session.Audio.Apply();
                Scene.Add(endTextA = new IntroText("Xaphan_0_Credits_ClearTime", "Middle", Engine.Height / 2 + 35, Color.DarkGreen, 1f, outline: true, fastdisplay: true)
                {
                    Show = true
                });
                for (int i = 0; i <= 25; i++)
                {
                    endTextA.UpdatePosition(new Vector2(0, -6));
                    totaltime.UpdatePosition(new Vector2(0, -6));
                    yield return null;
                }
                Add(totaltime);
                Scene.Add(endTextB = new IntroText("Xaphan_0_Credits_CollectedItems", "Middle", Engine.Height / 2 + 75, Color.DarkGreen, 1f, outline: true, fastdisplay: true)
                {
                    Show = true
                });
                Scene.Add(endTextC = new IntroText("Xaphan_0_Credits_CollectedItems_b", "Middle", Engine.Height / 2 + 155, Color.DarkGreen, 1f, outline: true, fastdisplay: true)
                {
                    Show = true
                });
                Scene.Add(endTextD = new IntroText(ItemPercentage.ToString() + "%", "Middle", Engine.Height / 2 + 265, Color.Gold, 1.5f, false, true, fastdisplay: true)
                {
                    Show = true
                });
                for (int i = 0; i <= 25; i++)
                {
                    endTextB.UpdatePosition(new Vector2(0, 6));
                    endTextC.UpdatePosition(new Vector2(0, 6));
                    endTextD.UpdatePosition(new Vector2(0, 6));
                    yield return null;
                }
                Scene.Add(endTextE = new IntroText("Xaphan_0_Credits_End", "Middle", Engine.Height / 2 + 95, Color.White, 1.5f, outline: true, fastdisplay: true)
                {
                    Show = true
                });
            }
            while (!Input.ESC.Pressed && !Input.MenuConfirm.Pressed)
            {
                yield return null;
            }
            Audio.Play("event:/new_content/game/10_farewell/endscene_final_input");
            level.RegisterAreaComplete();
            yield return new FadeWipe(level, false, () => EndCutscene(Level))
            {
                Duration = 5.25f
            }.Duration;
        }
    }
}
