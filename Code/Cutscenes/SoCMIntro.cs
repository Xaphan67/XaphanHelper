using System;
using System.Collections;
using System.Reflection;
using Celeste.Mod.XaphanHelper.Entities;
using Celeste.Mod.XaphanHelper.UI_Elements;
using FMOD.Studio;
using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste.Mod.XaphanHelper.Cutscenes
{
    class SoCMIntro : CutsceneEntity
    {
        private FieldInfo LevelPauseSnapshot = typeof(Level).GetField("PauseSnapshot", BindingFlags.Static | BindingFlags.NonPublic);

        private readonly Player player;

        private Coroutine IntroCoroutine;

        private Coroutine TitleScreenCoroutine;

        private Coroutine LogoCoroutine;

        private bool skipedIntro;

        private IntroText message;

        private bool DrawBlackBg;

        private Image logo;

        private float textAlpha = 0f;

        private float logoAlpha = 0f;

        private float fade;

        private Vector2 normalCameraPos;

        private TextMenu mainMenu;

        private TextMenu optionsMenu;

        private TextMenu infoMenu;

        private SaveProgressDisplay progressDisplay;

        private bool StartCampaign;

        private CS_Credits CreditsCutscene;

        private bool BackToMainMenu;

        public SoCMIntro(Player player)
        {
            this.player = player;
            Tag = (Tags.Global | Tags.HUD);
            if (Settings.Instance.Language == "english" || Settings.Instance.Language == "french")
            {
                logo = new Image(GFX.Gui["vignette/Xaphan/logo-" + Settings.Instance.Language]);
            }
            else
            {
                logo = new Image(GFX.Gui["vignette/Xaphan/logo-english"]);
            }
            logo.CenterOrigin();
            logo.Position = new Vector2(Engine.Width / 2, 250);
            Depth = 1000;
        }

        public override void OnBegin(Level level)
        {
            Audio.ReleaseSnapshot((EventInstance)LevelPauseSnapshot.GetValue(level));
            LevelPauseSnapshot.SetValue(level, null);
            MInput.Disabled = true;
            level.TimerStopped = true;
            Add(new Coroutine(Cutscene(level)));
            SceneAs<Level>().Add(progressDisplay = new SaveProgressDisplay(new Vector2(Engine.Width / 2, Engine.Height / 2 + 55f))
            {
                Visible = false
            });
            Add(IntroCoroutine = new Coroutine(IntroSequence(level)));
        }

        public override void OnEnd(Level level)
        {
            XaphanModule.SkipSoCMIntro = true;
            level.TimerStopped = false;
            XaphanModule.NoDroneSpawnSound = false;
        }

        public IEnumerator Cutscene(Level level)
        {
            player.StateMachine.State = 11;
            level.SnapColorGrade("panicattack");
            if (XaphanModule.ModSaveData.SavedFlags.Contains("Xaphan/0_Ch5_Gem_Collected"))
            {
                CustomCollectable gem = level.Tracker.GetEntity<CustomCollectable>();
                gem.RemoveSelf();
            }
            level.PauseLock = true;
            level.TimerHidden = true;
            level.Session.Audio.Music.Event = "event:/music/xaphan/lvl_0_intro_start";
            level.Session.Audio.Apply(forceSixteenthNoteHack: false);
            while (IntroCoroutine.Active)
            {
                yield return null;
            }
            Add(TitleScreenCoroutine = new Coroutine(TitleScreen(level)));
            Add(LogoCoroutine = new Coroutine(FadeLogo()));
            while (TitleScreenCoroutine.Active)
            {
                yield return null;
            }
            level.DoScreenWipe(false);
            yield return 0.47f;
            player.StateMachine.Locked = false;
            player.StateMachine.State = 0;
            EndCutscene(Level);
        }

        public IEnumerator IntroSequence(Level level)
        {
            XaphanModule.onTitleScreen = true;
            normalCameraPos = level.Camera.Position;
            DrawBlackBg = true;
            yield return 1f;
            yield return level.ZoomTo(new Vector2(240f, 135f), 2f, 0f);
            level.Add(message = new IntroText("Xaphan_0_0_intro_vignette_A", "Middle", Engine.Height / 2, Color.Red));
            message.Show = true;
            float timer = 2.2f;
            while (timer > 0f)
            {
                yield return null;
                timer -= Engine.DeltaTime;
            }
            message.Show = false;
            level.Remove(message);
            DrawBlackBg = false;
            for (int i = 0; i <= 85; i++)
            {
                level.Camera.X -= 1f;
                yield return 0.01f;
            }
            DrawBlackBg = true;
            level.Camera.Position = normalCameraPos;
            yield return level.ZoomTo(new Vector2(167f, 45f), 2f, 0f);
            level.Add(message = new IntroText("Xaphan_0_0_intro_vignette_B", "Middle", Engine.Height / 2, Color.Red));
            message.Show = true;
            timer = 2.2f;
            while (timer > 0f)
            {
                yield return null;
                timer -= Engine.DeltaTime;
            }
            message.Show = false;
            level.Remove(message);
            DrawBlackBg = false;
            for (int i = 0; i <= 85; i++)
            {
                level.Camera.X -= 1f;
                yield return 0.01f;
            }
            DrawBlackBg = true;
            level.Add(message = new IntroText("Xaphan_0_0_intro_vignette_C", "Middle", Engine.Height / 2, Color.Red));
            message.Show = true;
            timer = 2.2f;
            while (timer > 0f)
            {
                yield return null;
                timer -= Engine.DeltaTime;
            }
            message.Show = false;
            level.Remove(message);
            DrawBlackBg = false;
            for (int i = 0; i <= 85; i++)
            {
                level.Camera.Y += 1f;
                yield return 0.01f;
            }
            DrawBlackBg = true;
            yield return level.ZoomTo(new Vector2(160f, 92f), 5f, 0f);
            level.Add(message = new IntroText("Xaphan_0_0_intro_vignette_D", "Middle", Engine.Height / 2, Color.Red));
            message.Show = true;
            timer = 2.2f;
            while (timer > 0f)
            {
                yield return null;
                timer -= Engine.DeltaTime;
            }
            message.Show = false;
            level.Remove(message);
            DrawBlackBg = false;
            level.Camera.Position = normalCameraPos;
            level.NextColorGrade("none", 0.14f);
            yield return level.ZoomBack(7f);
        }

        public IEnumerator TitleScreen(Level level)
        {
            if (message != null)
            {
                message.RemoveSelf();
            }
            if (IntroCoroutine.Active)
            {
                IntroCoroutine.Cancel();
            }
            if (skipedIntro)
            {
                FadeWipe fadeWipe = new(level, false);
                fadeWipe.OnUpdate = delegate (float f)
                {
                    textAlpha = Math.Min(textAlpha, 1f - f);
                    fade = Math.Min(fade, 1f - f);
                };
                yield return 0.3f;
                DrawBlackBg = true;
                level.Camera.Position = normalCameraPos;
                yield return level.ZoomBack(0f);
                level.SnapColorGrade("none");
                yield return 0.2f;
                DrawBlackBg = false;
                fadeWipe = new FadeWipe(level, true);
                fadeWipe.OnUpdate = delegate (float f)
                {
                    textAlpha = Math.Max(textAlpha, 0f + f);
                    fade = Math.Max(fade, 0f + f);
                };
                level.Session.Audio.Music.Event = "event:/music/xaphan/lvl_0_intro_loop";
                level.Session.Audio.Apply(forceSixteenthNoteHack: false);
            }
            SceneAs<Level>().Add(mainMenu = new TextMenu());
            mainMenu.AutoScroll = false;
            mainMenu.Position = new Vector2(Engine.Width / 2f, Engine.Height / 2f + 300f);
            mainMenu.Add(new TextMenu.Button(Dialog.Clean("Xaphan_0_0_intro_vignette_Play").ToUpper()).Pressed(delegate
            {
                Add(new Coroutine(ShowPlayerInfoRoutine(SceneAs<Level>())));
            }));
            mainMenu.Add(new TextMenu.Button(Dialog.Clean("Xaphan_0_0_intro_vignette_Settings").ToUpper()).Pressed(delegate
            {
                Add(new Coroutine(ShowOptionsRoutine(SceneAs<Level>())));
            }));
            mainMenu.Add(new TextMenu.Button(Dialog.Clean("Xaphan_0_0_intro_vignette_Credits").ToUpper()).Pressed(delegate
            {
                Add(new Coroutine(ShowCreditsRoutine(SceneAs<Level>())));
            }));
            TextMenu.Item item = null;
            mainMenu.Add(item = new TextMenu.Button(Dialog.Clean("Xaphan_0_0_intro_vignette_Exit").ToUpper()).Pressed(delegate
            {
                level.Paused = true;
                Engine.TimeRate = 1f;
                mainMenu.Focused = false;
                level.Session.InArea = false;
                Audio.SetMusic(null);
                Audio.BusStopAll("bus:/gameplay_sfx", immediate: true);
                level.DoScreenWipe(wipeIn: false, delegate
                {
                    Engine.Scene = new LevelExit(LevelExit.Mode.GiveUp, level.Session, level.HiresSnow);
                }, hiresSnow: true);
                foreach (LevelEndingHook component in level.Tracker.GetComponents<LevelEndingHook>())
                {
                    if (component.OnEnd != null)
                    {
                        component.OnEnd();
                    }
                }
                XaphanModule.SoCMTitleFromGame = false;
                XaphanModule.startedGame = false;
                mainMenu.RemoveSelf();
            }));
            while (!StartCampaign)
            {
                logo.Visible = optionsMenu != null ? !optionsMenu.Visible : true;
                if (!XaphanModule.ModSettings.WatchedCredits)
                {
                    XaphanModule.ModSettings.AllowDebug = false;
                }
                yield return null;
            }
            mainMenu.Focused = false;
            XaphanModule.startedGame = true;
        }

        private IEnumerator ShowPlayerInfoRoutine(Level level)
        {
            BackToMainMenu = false;
            mainMenu.Focused = mainMenu.Visible = false;
            progressDisplay.Visible = true;
            SceneAs<Level>().Add(infoMenu = new TextMenu());
            infoMenu.AutoScroll = false;
            infoMenu.Position = new Vector2(Engine.Width / 2f, Engine.Height / 2f + 300f);
            infoMenu.Add(new TextMenu.Button(Dialog.Clean("Xaphan_0_0_intro_vignette_" + ((XaphanModule.ModSaveData.SavedTime.ContainsKey("Xaphan/0") && XaphanModule.ModSaveData.SavedTime["Xaphan/0"] > 170000) ? "Resume" : "Start")).ToUpper()).Pressed(delegate
            {
                infoMenu.Focused = false;
                StartCampaign = true;
                XaphanModule.SoCMTitleFromGame = false;
            }));
            infoMenu.Add(new TextMenu.Button(Dialog.Clean("Xaphan_0_0_intro_vignette_Erase").ToUpper()).Pressed(delegate
            {
                level.FormationBackdrop.Display = true;
                infoMenu.Focused = infoMenu.Visible = false;
                progressDisplay.Visible = false;
                XaphanModule.confirmRestartCampaign(level, infoMenu.Selection, true, infoMenu, progressDisplay);
            }));
            infoMenu.OnCancel = ReturnToMainmenu;
            while (!BackToMainMenu)
            {
                yield return null;
            }
        }

        private IEnumerator ShowOptionsRoutine(Level level)
        {
            level.FormationBackdrop.Display = true;
            BackToMainMenu = false;
            mainMenu.Focused = mainMenu.Visible = false;
            SceneAs<Level>().Add(optionsMenu = new TextMenu());
            CreateSettingsMenu(optionsMenu);
            optionsMenu.OnCancel = ReturnToMainmenu;
            while (!BackToMainMenu)
            {
                yield return null;
            }
            XaphanModule.SaveModSettings();
            level.FormationBackdrop.Display = false;
        }

        public static void CreateSettingsMenu(TextMenu menu)
        {
            menu.Position = new Vector2(Engine.Width / 2f, Engine.Height / 2f);
            menu.Add(new TextMenu.Header(Dialog.Clean("Xaphan_0_0_intro_vignette_Settings").ToUpper()));
            menu.Add(new TextMenu.SubHeader(Dialog.Clean("Xaphan_0_Options_UI")));
            menu.Add(new TextMenu.OnOff(Dialog.Clean("ModOptions_XaphanModule_ShowMiniMap"), XaphanModule.ModSettings.SoCMShowMiniMap).Change(delegate (bool b)
            {
                XaphanModule.ModSettings.SoCMShowMiniMap = b;
                foreach (var item in menu.Items)
                {
                    if (item.GetType() == typeof(TextMenu.Slider) && ((TextMenu.Slider)item).Label == Dialog.Clean("ModOptions_XaphanModule_MiniMapOpacity"))
                    {
                        item.Disabled = !b;
                        break;
                    }
                }
            }));
            menu.Add(new TextMenu.Slider(Dialog.Clean("ModOptions_XaphanModule_MiniMapOpacity"), (int i) => i switch
            {
                1 => Dialog.Clean("ModOptions_XaphanModule_10"),
                2 => Dialog.Clean("ModOptions_XaphanModule_20"),
                3 => Dialog.Clean("ModOptions_XaphanModule_30"),
                4 => Dialog.Clean("ModOptions_XaphanModule_40"),
                5 => Dialog.Clean("ModOptions_XaphanModule_50"),
                6 => Dialog.Clean("ModOptions_XaphanModule_60"),
                7 => Dialog.Clean("ModOptions_XaphanModule_70"),
                8 => Dialog.Clean("ModOptions_XaphanModule_80"),
                9 => Dialog.Clean("ModOptions_XaphanModule_90"),
                _ => Dialog.Clean("ModOptions_XaphanModule_100"),
            }, 1, 10, XaphanModule.ModSettings.SoCMMiniMapOpacity).Change(delegate (int i)
            {
                XaphanModule.ModSettings.SoCMMiniMapOpacity = i;
            }));
            menu.Add(new TextMenu.Slider(Dialog.Clean("ModOptions_XaphanModule_SpaceJumpIndicator"), (int i) => i switch
            {
                0 => Dialog.Clean("ModOptions_XaphanModule_SpaceJumpIndicator_None"),
                1 => Dialog.Clean("ModOptions_XaphanModule_SpaceJumpIndicator_Small"),
                _ => Dialog.Clean("ModOptions_XaphanModule_SpaceJumpIndicator_Large"),
            }, 0, 2, XaphanModule.ModSettings.SoCMSpaceJumpIndicator).Change(delegate (int i)
            {
                XaphanModule.ModSettings.SoCMSpaceJumpIndicator = i;
            }));
            menu.Add(new TextMenu.Slider(Dialog.Clean("ModOptions_XaphanModule_StaminaIndicator"), (int i) => i switch
            {
                0 => Dialog.Clean("ModOptions_XaphanModule_StaminaIndicator_UI_Only"),
                1 => Dialog.Clean("ModOptions_XaphanModule_StaminaIndicator_Player_Only"),
                2 => Dialog.Clean("ModOptions_XaphanModule_StaminaIndicator_Both"),
                _ => Dialog.Clean("ModOptions_XaphanModule_SpaceJumpIndicator_None"),
            }, 0, 3, XaphanModule.ModSettings.SoCMStaminaIndicator).Change(delegate (int i)
            {
                XaphanModule.ModSettings.SoCMStaminaIndicator = i;
            }));
            menu.Add(new TextMenu.Slider(Dialog.Clean("ModOptions_XaphanModule_OxygenIndicator"), (int i) => i switch
            {
                0 => Dialog.Clean("ModOptions_XaphanModule_StaminaIndicator_UI_Only"),
                1 => Dialog.Clean("ModOptions_XaphanModule_StaminaIndicator_Player_Only"),
                2 => Dialog.Clean("ModOptions_XaphanModule_StaminaIndicator_Both"),
                _ => Dialog.Clean("ModOptions_XaphanModule_SpaceJumpIndicator_None"),
            }, 0, 3, XaphanModule.ModSettings.SoCMOxygenIndicator).Change(delegate (int i)
            {
                XaphanModule.ModSettings.SoCMOxygenIndicator = i;
            }));
            menu.Add(new TextMenu.SubHeader(Dialog.Clean("Xaphan_0_Options_Popups")));
            menu.Add(new TextMenu.OnOff(Dialog.Clean("Xaphan_0_Options_Popups_ShowAchievementsPopups"), XaphanModule.ModSettings.ShowAchievementsPopups).Change(delegate (bool b)
            {
                XaphanModule.ModSettings.ShowAchievementsPopups = b;
            }));
            menu.Add(new TextMenu.OnOff(Dialog.Clean("Xaphan_0_Options_Popups_ShowLorebookPopups"), XaphanModule.ModSettings.ShowLorebookPopups).Change(delegate (bool b)
            {
                XaphanModule.ModSettings.ShowLorebookPopups = b;
            }));
            menu.Add(new TextMenu.SubHeader(Dialog.Clean("Xaphan_0_Options_Cutscenes")));
            menu.Add(new TextMenu.OnOff(Dialog.Clean("Xaphan_0_Options_Cutscenes_AutoSkipCutscenes"), XaphanModule.ModSettings.AutoSkipCutscenes).Change(delegate (bool b)
            {
                XaphanModule.ModSettings.AutoSkipCutscenes = b;
            }));
            menu.Add(new TextMenu.SubHeader(Dialog.Clean("options_controls")));
            menu.Add(new TextMenu.Button(Dialog.Clean("options_keyconfig")).Pressed(() => {
                menu.Focused = false;
                Engine.Scene.Add(CreateKeyboardConfigUI(menu));
                Engine.Scene.OnEndOfFrame += () => Engine.Scene.Entities.UpdateLists();
            }));
            menu.Add(new TextMenu.Button(Dialog.Clean("options_btnconfig")).Pressed(() => {
                menu.Focused = false;
                Engine.Scene.Add(CreateButtonConfigUI(menu));
                Engine.Scene.OnEndOfFrame += () => Engine.Scene.Entities.UpdateLists();
            }));
            menu.Add(new TextMenu.SubHeader(Dialog.Clean("Xaphan_0_Options_Others")));
            TextMenu.OnOff DebugSetting = new TextMenu.OnOff(Dialog.Clean("Xaphan_0_Options_Others_Debug"), XaphanModule.ModSettings.AllowDebug);
            DebugSetting.Disabled = !XaphanModule.ModSettings.WatchedCredits;
            menu.Add(DebugSetting.Change(delegate (bool b)
            {
                XaphanModule.ModSettings.AllowDebug = b;
            }));
        }

        private static Entity CreateKeyboardConfigUI(TextMenu menu)
        {
            return new ModuleSettingsKeyboardConfigUI(XaphanModule.Instance)
            {
                OnClose = () => menu.Focused = true
            };
        }

        private static Entity CreateButtonConfigUI(TextMenu menu)
        {
            return new ModuleSettingsButtonConfigUI(XaphanModule.Instance)
            {
                OnClose = () => menu.Focused = true
            };
        }

        private void ReturnToMainmenu()
        {
            BackToMainMenu = true;
            mainMenu.Focused = mainMenu.Visible = true;
            if (optionsMenu != null)
            {
                optionsMenu.Focused = optionsMenu.Visible = false;
                optionsMenu.RemoveSelf();
            }
            if (infoMenu != null)
            {
                infoMenu.Focused = infoMenu.Visible = false;
                infoMenu.RemoveSelf();
            }
            if (progressDisplay != null)
            {
                progressDisplay.Visible = false;
            }
        }

        private IEnumerator ShowCreditsRoutine(Level level)
        {
            mainMenu.Focused = false;
            Scene.Add(CreditsCutscene = new CS_Credits(player, true));
            yield return null;
        }

        private IEnumerator FadeLogo()
        {
            if (skipedIntro)
            {
                float timer = 0.7f;
                while (timer > 0f)
                {
                    yield return null;
                    timer -= Engine.DeltaTime;
                }
            }
            while (logoAlpha != 1)
            {
                yield return null;
                logoAlpha = Calc.Approach(logoAlpha, 1, Engine.DeltaTime * 2f);
            }
        }

        public override void Update()
        {
            base.Update();
            if ((Input.Pause.Pressed || Input.ESC.Pressed) && IntroCoroutine.Active)
            {
                skipedIntro = true;
                if (message != null)
                {
                    message.RemoveSelf();
                }
                IntroCoroutine.Cancel();
            }
            if (mainMenu != null && LogoCoroutine.Active)
            {
                foreach (TextMenu.Item item in mainMenu.Items)
                {
                    if (item is TextMenu.Button)
                    {
                        TextMenu.Button button = (TextMenu.Button)item;
                        button.Container.Alpha = logoAlpha;
                    }
                }
            }
        }

        public override void Render()
        {
            base.Render();
            if (DrawBlackBg)
            {
                Draw.Rect(-10f, -10f, 1940f, 1100f, Color.Black);
            }
            logo.Color = Color.White * logoAlpha;
            if (logo.Visible)
            {
                logo.Render();
            }
            if (mainMenu != null && !Level.FormationBackdrop.Display)
            {
                ActiveFont.Draw(XaphanModule.SoCMVersion.ToString(), new Vector2(Engine.Width - 100f, Engine.Height - 35f), Vector2.One * 0.5f, Vector2.One * 0.55f, Color.Gold * mainMenu.Alpha);
            }
        }
    }
}
