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

        private Image decoLeft;

        private Image decoRight;

        private NormalText FirstInput;

        private Image InputImage;

        private bool DrawBlackBg;

        private Image logo;

        private float textAlpha = 0f;

        private float logoAlpha = 0f;

        private float inputAlpha = 0f;

        private float fade;

        private Vector2 normalCameraPos;

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
            Add(InputCoroutine = new Coroutine(FadeInput()));
            while (TitleScreenCoroutine.Active)
            {
                yield return null;
            }
            level.DoScreenWipe(false);
            yield return 0.47f;
            decoLeft = null;
            FirstInput.RemoveSelf();
            FirstInput = null;
            InputImage = null;
            decoRight = null;
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
            decoLeft = new Image(GFX.Gui["poemside"]);
            decoRight = new Image(GFX.Gui["poemside"]);
            string prefix = "UI_QUICK_RESTART_PRESS";
            VirtualButton control = Input.MenuConfirm;
            MTexture buttonTexture = Input.GuiButton(control, "controls/keyboard/oemquestion");
            float TotalLenght = decoLeft.Width + 19 + ActiveFont.Measure(Dialog.Clean(prefix)).X * 1.5f + 29 + buttonTexture.Width + 19 + decoRight.Width;
            float DecoLeftPosition = Engine.Width / 2f - TotalLenght / 2f + decoLeft.Width / 2;
            float PrefixPosition = DecoLeftPosition + decoLeft.Width / 2 + 19 + (ActiveFont.Measure(Dialog.Clean(prefix)).X * 1.5f) / 2;
            float InputPosition = PrefixPosition + (ActiveFont.Measure(Dialog.Clean(prefix)).X * 1.5f) / 2 + 29 + buttonTexture.Width / 2;
            float DecoRightPosition = InputPosition + buttonTexture.Width / 2 + 19 + decoRight.Width / 2;
            decoLeft.CenterOrigin();
            decoLeft.Position = new Vector2(DecoLeftPosition, Engine.Height / 2 + 206);
            decoRight.CenterOrigin();
            decoRight.Position = new Vector2(DecoRightPosition, Engine.Height / 2 + 206);
            level.Add(FirstInput = new NormalText(prefix, new Vector2(PrefixPosition, Engine.Height / 2 + 206), Color.White, inputAlpha, 1.5f));
            InputImage = new Image(buttonTexture);
            InputImage.CenterOrigin();
            InputImage.Position = new Vector2(InputPosition, Engine.Height / 2 + 206);
            while (!Input.MenuConfirm.Check)
            {
                yield return null;
            }
            Audio.Play("event:/ui/main/button_select");
            /*if (XaphanModule.ModSettings.SpeedrunModeUnlocked)
            {
                Add(menu = new TextMenu());
                menu.Add(new TextMenu.SubHeader(""));
                menu.Add(new TextMenu.SubHeader(""));
                menu.Add(new TextMenu.SubHeader(""));
                menu.Add(new TextMenu.SubHeader(""));
                menu.Add(new TextMenu.SubHeader(""));
                menu.Add(new TextMenu.SubHeader(""));
                menu.Add(new TextMenu.SubHeader(""));
                menu.Add(new TextMenu.SubHeader(""));
                menu.Add(new TextMenu.Button(Dialog.Clean("Xaphan_0_0_intro_vignette_Start")).Pressed(StartGame));
                menu.Add(new TextMenu.Button(Dialog.Clean("Xaphan_0_0_intro_vignette_Speedrun")).Pressed(ActivateSpeedrunMode));
                while (!Input.MenuConfirm.Pressed)
                {
                    yield return null;
                }
            }
            else*/
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

        private IEnumerator FadeInput()
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
            while (inputAlpha != 1)
            {
                yield return null;
                inputAlpha = Calc.Approach(inputAlpha, 1, Engine.DeltaTime * 2f);
                if (FirstInput != null)
                {
                    FirstInput.Appear();
                }
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
            if (decoLeft != null)
            {
                decoLeft.Color = Color.White * inputAlpha;
                decoLeft.Render();
            }
            if (FirstInput != null)
            {
                FirstInput.Render();
            }
            if (InputImage != null)
            {
                InputImage.Color = Color.White * inputAlpha;
                InputImage.Render();
            }
            if (decoRight != null)
            {
                decoRight.Color = Color.White * inputAlpha;
                decoRight.Render();
            }
            /*if (menu != null)
            {
                menu.Render();
            }*/
        }
    }
}
