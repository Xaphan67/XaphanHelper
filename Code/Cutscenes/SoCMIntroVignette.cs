using System;
using System.Collections;
using System.Collections.Generic;
using Celeste.Mod.XaphanHelper.UI_Elements;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Monocle;

namespace Celeste.Mod.XaphanHelper.Cutscenes
{
    internal class SoCMIntroVignette : Scene
    {
        private Session session;

        private string areaMusic;

        private TextMenu menu;

        private HudRenderer hud;

        private bool exiting;

        private bool onTitleScreen;

        private bool skipedIntro;

        private Coroutine IntroCoroutine;

        private Coroutine TitleCoroutine;

        private Coroutine LogoCoroutine;

        private Coroutine InputCoroutine;

        private float textAlpha = 0f;

        private float logoAlpha = 0f;

        private float inputAlpha = 0f;

        private float fade;

        private Sprite BG;

        private Image logo;

        private IntroText message;

        private Image decoLeft;

        private Image decoRight;

        private NormalText FirstInput;

        private Image InputImage;

        public SoCMIntroVignette(Session session, HiresSnow snow = null)
        {
            this.session = session;
            Add(hud = new HudRenderer());
            RendererList.UpdateLists();
            fade = 1;
            BG = new Sprite(GFX.Gui, "vignette/Xaphan/SoCMTitleBG");
            BG.AddLoop("idle", "", 0.05f, 0);
            BG.Play("idle");
            BG.Scale = new Vector2(2, 2);
            BG.Position += new Vector2(-BG.Width, -BG.Height);
            BG.Visible = false;
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
            IntroCoroutine = new Coroutine(IntroSequence());
            TitleCoroutine = new Coroutine(TitleScreen());
            LogoCoroutine = new Coroutine(FadeLogo());
            InputCoroutine = new Coroutine(FadeInput());
        }

        private IEnumerator IntroSequence()
        {
            areaMusic = session.Audio.Music.Event;
            session.Audio.Music.Event = "event:/music/xaphan/lvl_0_intro_start";
            session.Audio.Apply(forceSixteenthNoteHack: false);
            Add(message = new IntroText("Xaphan_0_0_intro_vignette_A", "Middle", Engine.Height / 2, Color.Red));
            message.Show = true;
            float timer = 2f;
            while (timer > 0f)
            {
                yield return null;
                timer -= Engine.DeltaTime;
            }
            message.Show = false;
            message.RemoveSelf();
            BG.Visible = true;
            for (int i = 0; i <= 125; i++)
            {
                BG.X += 7.68f;
                yield return 0.01f;
            }
            BG.Visible = false;
            Add(message = new IntroText("Xaphan_0_0_intro_vignette_B", "Middle", Engine.Height / 2, Color.Red));
            message.Show = true;
            timer = 2f;
            while (timer > 0f)
            {
                yield return null;
                timer -= Engine.DeltaTime;
            }
            message.Show = false;
            message.RemoveSelf();
            BG.Position = Vector2.Zero + new Vector2(-BG.Width / 2, 0);
            BG.Visible = true;
            for (int j = 0; j <= 125; j++)
            {
                BG.X += 7.68f;
                yield return 0.01f;
            }
            BG.Visible = false;
            Add(message = new IntroText("Xaphan_0_0_intro_vignette_C", "Middle", Engine.Height / 2, Color.Red));
            message.Show = true;
            timer = 2f;
            while (timer > 0f)
            {
                yield return null;
                timer -= Engine.DeltaTime;
            }
            message.Show = false;
            message.RemoveSelf();
            BG.Position = Vector2.Zero;
            BG.Visible = true;
            for (int k = 0; k <= 125; k++)
            {
                BG.Y -= 4.32f;
                yield return 0.01f;
            }
            BG.Visible = false;
            Add(message = new IntroText("Xaphan_0_0_intro_vignette_D", "Middle", Engine.Height / 2, Color.Red));
            message.Show = true;
            timer = 2f;
            while (timer > 0f)
            {
                yield return null;
                timer -= Engine.DeltaTime;
            }
            message.Show = false;
            message.RemoveSelf();
            BG.CenterOrigin();
            BG.Position = Vector2.Zero + new Vector2(Engine.Width, Engine.Height) / 2;
            BG.Visible = true;
            for (int l = 140; l > 0; l--)
            {
                BG.Scale = new Vector2(Math.Max(1, l * 0.03f), Math.Max(1, l * 0.03f));
                yield return 0.01f;
                if (BG.Scale == Vector2.One)
                {
                    break;
                }
            }
            onTitleScreen = true;
        }

        public IEnumerator TitleScreen()
        {
            onTitleScreen = true;
            IntroCoroutine = null;
            if (message != null)
            {
                message.RemoveSelf();
            }
            if (skipedIntro)
            {
                FadeWipe fadeWipe = new(this, false);
                fadeWipe.OnUpdate = delegate (float f)
                {
                    textAlpha = Math.Min(textAlpha, 1f - f);
                    fade = Math.Min(fade, 1f - f);
                };
                yield return 0.5f;
                fadeWipe = new FadeWipe(this, true);
                fadeWipe.OnUpdate = delegate (float f)
                {
                    textAlpha = Math.Max(textAlpha, 0f + f);
                    fade = Math.Max(fade, 0f + f);
                };
                session.Audio.Music.Event = "event:/music/xaphan/lvl_0_intro_loop";
                session.Audio.Apply(forceSixteenthNoteHack: false);
            }
            BG.Visible = true;
            BG.CenterOrigin();
            BG.Position = Vector2.Zero + new Vector2(Engine.Width, Engine.Height) / 2;
            BG.Scale = Vector2.One;
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
            Add(FirstInput = new NormalText(prefix, new Vector2(PrefixPosition, Engine.Height / 2 + 206), Color.White, inputAlpha, 1.5f));
            InputImage = new Image(buttonTexture);
            InputImage.CenterOrigin();
            InputImage.Position = new Vector2(InputPosition, Engine.Height / 2 + 206);
            while (!Input.MenuConfirm.Check)
            {
                yield return null;
            }

            Audio.Play("event:/ui/main/button_select");
            decoLeft = null;
            FirstInput.RemoveSelf();
            FirstInput = null;
            InputImage = null;
            decoRight = null;
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
            {
                StartGame();
            }
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
            if (menu == null)
            {
                base.Update();
                if (!exiting)
                {
                    if (!onTitleScreen)
                    {
                        IntroCoroutine.Update();
                        if (Input.Pause.Pressed || Input.ESC.Pressed)
                        {
                            skipedIntro = true;
                            if (message != null)
                            {
                                message.RemoveSelf();
                            }
                            TitleCoroutine.Update();
                        }
                    }
                    else
                    {
                        TitleCoroutine.Update();
                        LogoCoroutine.Update();
                        InputCoroutine.Update();

                    }
                }
            }
            else if (!exiting)
            {
                menu.Update();
            }
        }

        private void StartGame()
        {
            StopSfx();
            IntroCoroutine = null;
            session.Audio.Music.Event = areaMusic;
            if (menu != null)
            {
                menu.RemoveSelf();
                menu = null;
            }
            FadeWipe fadeWipe = new(this, false, delegate
            {
                Engine.Scene = new LevelLoader(session);
            });
            fadeWipe.OnUpdate = delegate (float f)
            {
                fade = Math.Min(fade, 1f - f);
                logoAlpha = Math.Min(logoAlpha, 1f - f);
            };
            exiting = true;
        }

        public override void Render()
        {
            base.Render();
            Draw.SpriteBatch.Begin(0, BlendState.AlphaBlend, SamplerState.LinearClamp, null, RasterizerState.CullNone, null, Engine.ScreenMatrix);
            if (BG.Visible == true)
            {
                BG.Color = Color.White * fade;
                BG.Render();
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
            if (menu != null)
            {
                menu.Render();
            }
            Draw.SpriteBatch.End();
        }

        private void StopSfx()
        {
            List<Component> components = new();
            components.AddRange(Tracker.GetComponents<SoundSource>());
            foreach (SoundSource sound in components)
            {
                sound.RemoveSelf();
            }
        }
    }
}
