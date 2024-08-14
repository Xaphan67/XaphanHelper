using System;
using System.Collections;
using Celeste.Mod.XaphanHelper.Entities;
using Celeste.Mod.XaphanHelper.UI_Elements;
using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste.Mod.XaphanHelper.Cutscenes
{
    class SoCMIntro : CutsceneEntity
    {
        private readonly Player player;

        private Coroutine IntroCoroutine;

        private Coroutine TitleScreenCoroutine;

        private Coroutine LogoCoroutine;

        private Coroutine InputCoroutine;

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
        }

        public override void OnBegin(Level level)
        {
            level.TimerStopped = true;
            Add(new Coroutine(Cutscene(level)));
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
            normalCameraPos = level.Camera.Position;
            DrawBlackBg = true;
            yield return 1f;
            yield return level.ZoomTo(new Vector2(240f, 135f), 2f, 0f);
            level.Add(message = new IntroText("Xaphan_0_0_intro_vignette_A", "Middle", Engine.Height / 2, Color.Red));
            message.Show = true;
            float timer = 2f;
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
                level.Camera.X -= 0.5f;
                yield return 0.01f;
            }
            DrawBlackBg = true;
            level.Camera.Position = normalCameraPos;
            yield return level.ZoomTo(new Vector2(167f, 45f), 2f, 0f);
            level.Add(message = new IntroText("Xaphan_0_0_intro_vignette_B", "Middle", Engine.Height / 2, Color.Red));
            message.Show = true;
            timer = 2f;
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
                level.Camera.X -= 0.5f;
                yield return 0.01f;
            }
            DrawBlackBg = true;
            level.Add(message = new IntroText("Xaphan_0_0_intro_vignette_C", "Middle", Engine.Height / 2, Color.Red));
            message.Show = true;
            timer = 2f;
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
                level.Camera.Y += 0.5f;
                yield return 0.01f;
            }
            DrawBlackBg = true;
            yield return level.ZoomTo(new Vector2(160f, 92f), 5f, 0f);
            level.Add(message = new IntroText("Xaphan_0_0_intro_vignette_D", "Middle", Engine.Height / 2, Color.Red));
            message.Show = true;
            timer = 2f;
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
            mainMenu.Add(new TextMenu.Button(Dialog.Clean("Xaphan_0_0_intro_vignette_" + (XaphanModule.SoCMTitleFromGame ? "Resume" : "Start")).ToUpper()).Pressed(delegate
            {
                StartCampaign = true;
                XaphanModule.SoCMTitleFromGame = false;
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
                mainMenu.RemoveSelf();
            }));
            while (!StartCampaign)
            {
                yield return null;
            }
            mainMenu.Focused = false;
        }

        private IEnumerator ShowOptionsRoutine(Level level)
        {
            level.FormationBackdrop.Display = true;
            BackToMainMenu = false;
            mainMenu.Focused = mainMenu.Visible = false;
            SceneAs<Level>().Add(optionsMenu = new TextMenu());
            optionsMenu.AutoScroll = false;
            optionsMenu.Position = new Vector2(Engine.Width / 2f, Engine.Height / 2f + 150f);
            optionsMenu.Add(new TextMenu.SubHeader(Dialog.Clean("Xaphan_0_Options_Popups")));
            optionsMenu.Add(new TextMenu.OnOff(Dialog.Clean("Xaphan_0_Options_Popups_ShowAchievementsPopups"), XaphanModule.ModSettings.ShowAchievementsPopups).Change(delegate (bool b)
            {
                XaphanModule.ModSettings.ShowAchievementsPopups = b;
            }));
            optionsMenu.Add(new TextMenu.OnOff(Dialog.Clean("Xaphan_0_Options_Popups_ShowLorebookPopups"), XaphanModule.ModSettings.ShowLorebookPopups).Change(delegate (bool b)
            {
                XaphanModule.ModSettings.ShowLorebookPopups = b;
            }));
            optionsMenu.Add(new TextMenu.SubHeader(Dialog.Clean("Xaphan_0_Options_Cutscenes")));
            optionsMenu.Add(new TextMenu.OnOff(Dialog.Clean("Xaphan_0_Options_Cutscenes_AutoSkipCutscenes"), XaphanModule.ModSettings.AutoSkipCutscenes).Change(delegate (bool b)
            {
                XaphanModule.ModSettings.AutoSkipCutscenes = b;
            }));
            optionsMenu.Add(new TextMenu.SubHeader(Dialog.Clean("options_controls")));
            optionsMenu.Add(new TextMenu.Button(Dialog.Clean("options_keyconfig")).Pressed(() => {
                optionsMenu.Focused = false;
                Engine.Scene.Add(CreateKeyboardConfigUI(optionsMenu));
                Engine.Scene.OnEndOfFrame += () => Engine.Scene.Entities.UpdateLists();
            }));
            optionsMenu.Add(new TextMenu.Button(Dialog.Clean("options_btnconfig")).Pressed(() => {
                optionsMenu.Focused = false;
                Engine.Scene.Add(CreateButtonConfigUI(optionsMenu));
                Engine.Scene.OnEndOfFrame += () => Engine.Scene.Entities.UpdateLists();
            }));
            optionsMenu.OnCancel = ReturnToMainmenu;
            while (!BackToMainMenu)
            {
                yield return null;
            }
            level.FormationBackdrop.Display = false;
        }

        private Entity CreateKeyboardConfigUI(TextMenu menu)
        {
            return new ModuleSettingsKeyboardConfigUI(XaphanModule.Instance)
            {
                OnClose = () => menu.Focused = true
            };
        }

        private Entity CreateButtonConfigUI(TextMenu menu)
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
            optionsMenu.RemoveSelf();
        }

        private IEnumerator ShowCreditsRoutine(Level level)
        {
            mainMenu.Focused = false;
            Scene.Add(CreditsCutscene = new CS_Credits(player, true));
            yield return null;
            while (!CreditsCutscene.Finished)
            {
                yield return null;
            }
            mainMenu.Selection = 0;
            mainMenu.Focused = true;
            level.Session.Audio.Music.Event = "event:/music/xaphan/lvl_0_intro_loop";
            level.Session.Audio.Apply(forceSixteenthNoteHack: false);
            yield return new FadeWipe(level, wipeIn: true)
            {
                Duration = 1f
            }.Duration;
            Audio.SetMusicParam("fade", 1f);
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
            if (mainMenu != null)
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
            logo.Render();
        }
    }
}
