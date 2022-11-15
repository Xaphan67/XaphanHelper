using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Monocle;
using System;
using System.Collections.Generic;

namespace Celeste.Mod.XaphanHelper.UI_Elements
{
    public class CustomPoem : Entity
    {
        private struct Particle
        {
            public Vector2 Direction;

            public float Percent;

            public float Duration;

            public void Reset(float percent)
            {
                Direction = Calc.AngleToVector(Calc.Random.NextFloat((float)Math.PI * 2f), 1f);
                Percent = percent;
                Duration = 0.5f + Calc.Random.NextFloat() * 0.5f;
            }
        }

        private const float textScale = 1.5f;

        public float Alpha = 1f;

        public float TextAlpha = 1f;

        public Vector2 Offset;

        public float ParticleSpeed = 1f;

        public float Shake = 0f;

        private float timer = 0f;

        private string inputActionA;

        private string inputActionB;

        private string textA;

        private string textB;

        private string textC;

        private string poemColorA;

        private string poemColorB;

        private string poemColorC;

        private string poemParticleColor;

        private bool disposed;

        private VirtualRenderTarget poem;

        private VirtualRenderTarget smoke;

        private VirtualRenderTarget temp;

        private Particle[] particles = new Particle[80];

        private Sprite Sprite;

        private object controlA;

        private object controlB;

        private bool select;

        private Sprite upgradeSprite;

        protected XaphanModuleSettings Settings => XaphanModule.Settings;

        public CustomPoem(string inputActionA, string textA, string inputActionB = null, string textB = null, string textC = null, string poemColorA = "FFFFFF", string poemColorB = "FFFFFF", string poemColorC = "FFFFFF", string poemParticleColor = "FFFFFF", string sprite = "", float spriteAlpha = 1f, object controlA = null, object controlB = null, bool select = false)
        {
            if (textA != null)
            {
                this.textA = ActiveFont.FontSize.AutoNewline(textA, 1536);
            }
            if (textB != null)
            {
                this.textB = ActiveFont.FontSize.AutoNewline(textB, 1536);
            }
            if (textC != null)
            {
                this.textC = ActiveFont.FontSize.AutoNewline(textC, 1536);
            }
            this.controlA = controlA;
            this.controlB = controlB;
            this.inputActionA = inputActionA;
            this.inputActionB = inputActionB;
            this.poemColorA = poemColorA;
            this.poemColorB = poemColorB;
            this.poemColorC = poemColorC;
            this.poemParticleColor = poemParticleColor;
            this.select = select;
            if (sprite != "")
            {
                Sprite = new Sprite(GFX.Gui, sprite);
                Sprite.Scale = new Vector2(10f);
                Sprite.AddLoop("static", "", 0.08f, 0);
                Sprite.Play("static");
                Sprite.CenterOrigin();
                Sprite.Position = new Vector2(1920f, 1080f) * 0.5f;
                Sprite.Color = Color.White * spriteAlpha;
            }
            if (select)
            {
                upgradeSprite = new Sprite(GFX.Gui, sprite);
                upgradeSprite.Scale = new Vector2(1.5f);
                upgradeSprite.AddLoop("static", "", 1f, 0);
                upgradeSprite.Play("static");
                upgradeSprite.CenterOrigin();
                upgradeSprite.Position = upgradeSprite.Position + new Vector2(8, 8);
            }
            int num = Math.Min(1920, Engine.ViewWidth);
            int num2 = Math.Min(1080, Engine.ViewHeight);
            poem = VirtualContent.CreateRenderTarget("poem-a", num, num2);
            smoke = VirtualContent.CreateRenderTarget("poem-b", num / 2, num2 / 2);
            temp = VirtualContent.CreateRenderTarget("poem-c", num / 2, num2 / 2);
            Tag = Tags.HUD | Tags.FrozenUpdate;
            Add(new BeforeRenderHook(BeforeRender));
            for (int i = 0; i < particles.Length; i++)
            {
                particles[i].Reset(Calc.Random.NextFloat());
            }
        }

        public void Dispose()
        {
            if (!disposed)
            {
                poem.Dispose();
                smoke.Dispose();
                temp.Dispose();
                RemoveSelf();
                disposed = true;
            }
        }

        private void DrawPoem(string text, Vector2 offset, Color color, float scale, float spacing, bool noDeco = false, bool showInput = false)
        {
            if (!noDeco && !showInput)
            {
                MTexture mTexture = GFX.Gui["poemside"];
                float num = ActiveFont.Measure(textA).X * scale;
                Vector2 vector = new Vector2(960f, 540f) + offset;
                mTexture.DrawCentered(vector - Vector2.UnitX * (num / 2f + 64f), color);
                ActiveFont.Draw(text, vector, new Vector2(0.5f, 0.5f), Vector2.One * scale, color);
                mTexture.DrawCentered(vector + Vector2.UnitX * (num / 2f + 64f), color);
            }
            else if (noDeco && !showInput)
            {
                Vector2 vector = new Vector2(960f, 540f) + offset;
                ActiveFont.Draw(text, vector, new Vector2(0.5f, 0.5f), Vector2.One * scale, color);
            }
            else if (showInput)
            {
                float buttonATextureWidth = 0;
                float buttonBTextureWidth = 0;
                float spriteWidth = 0f;
                if (controlA is VirtualButton)
                {
                    MTexture buttonATexture = Input.GuiButton((VirtualButton)controlA, "controls/keyboard/oemquestion");
                    buttonATextureWidth = buttonATexture.Width;
                }
                else if (controlA is ButtonBinding)
                {
                    VirtualButton Button = new VirtualButton();
                    ButtonBinding ControlA = (ButtonBinding)controlA;
                    Button.Binding = ControlA.Binding;
                    MTexture buttonATexture = Input.GuiButton(Button, "controls/keyboard/oemquestion");
                    buttonATextureWidth = buttonATexture.Width;
                }
                if (controlB is VirtualButton)
                {
                    MTexture buttonBTexture = Input.GuiButton((VirtualButton)controlB, "controls/keyboard/oemquestion");
                    buttonBTextureWidth = buttonBTexture.Width;
                }
                else if (controlB is ButtonBinding)
                {
                    VirtualButton Button = new VirtualButton();
                    ButtonBinding ControlB = (ButtonBinding)controlB;
                    Button.Binding = ControlB.Binding;
                    MTexture buttonBTexture = Input.GuiButton(Button, "controls/keyboard/oemquestion");
                    buttonBTextureWidth = buttonBTexture.Width;
                }
                string selectHold = Dialog.Clean("XaphanHelper_Hold");
                string selectAndPress = Dialog.Clean("XaphanHelper_AndPress");
                string selectString = !XaphanModule.useMetroidGameplay ? Dialog.Clean("XaphanHelper_ToSelect") : Dialog.Clean("XaphanHelper_Select");
                if (upgradeSprite != null)
                {
                    spriteWidth = upgradeSprite.Width * 1.5f;
                }
                string inputActA = Dialog.Clean(inputActionA);
                string inputActB = Dialog.Clean(inputActionB);
                Vector2 vector = new Vector2(960f, 540f) + offset;
                float TotalLenght = 0;
                if (select)
                {
                    if (!XaphanModule.useMetroidGameplay)
                    {
                        TotalLenght = ActiveFont.Measure(selectHold).X * scale + buttonATextureWidth + ActiveFont.Measure(selectAndPress).X * scale + buttonBTextureWidth + ActiveFont.Measure(selectString).X * scale + spriteWidth + 10 + ActiveFont.Measure(inputActA).X * scale + spacing + buttonATextureWidth + spacing + (controlB != null ? ActiveFont.Measure(inputActB).X * scale + spacing + buttonBTextureWidth + spacing : 0) + ActiveFont.Measure(textC).X * scale;
                    }
                    else
                    {
                        TotalLenght = ActiveFont.Measure(selectString).X * scale + spriteWidth + spacing + ActiveFont.Measure(inputActA).X * scale + spacing + buttonATextureWidth + spacing + (controlB != null ? ActiveFont.Measure(inputActB).X * scale + spacing + buttonBTextureWidth + spacing : 0) + ActiveFont.Measure(textC).X * scale;
                    }
                }
                else
                {
                    TotalLenght = ActiveFont.Measure(inputActA).X * scale + spacing + buttonATextureWidth + spacing + (controlB != null ? ActiveFont.Measure(inputActB).X * scale + spacing + buttonBTextureWidth + spacing : 0) + ActiveFont.Measure(textC).X * scale;
                }
                if (TotalLenght >= 1920)
                {
                    scale -= 0.15f;
                    spacing -= 3.5f;
                    if (select)
                    {
                        if (!XaphanModule.useMetroidGameplay)
                        {
                            TotalLenght = ActiveFont.Measure(selectHold).X * scale + buttonATextureWidth + ActiveFont.Measure(selectAndPress).X * scale + buttonBTextureWidth + ActiveFont.Measure(selectString).X * scale + spriteWidth + ActiveFont.Measure(inputActA).X * scale + spacing + buttonATextureWidth + spacing + (controlB != null ? ActiveFont.Measure(inputActB).X * scale + spacing + buttonBTextureWidth + spacing : 0) + ActiveFont.Measure(textC).X * scale;
                        }
                        else
                        {
                            TotalLenght = ActiveFont.Measure(selectString).X * scale + spriteWidth + ActiveFont.Measure(inputActA).X * scale + spacing + buttonATextureWidth + spacing + (controlB != null ? ActiveFont.Measure(inputActB).X * scale + spacing + buttonBTextureWidth + spacing : 0) + ActiveFont.Measure(textC).X * scale;
                        }
                    }
                    else
                    {
                        TotalLenght = ActiveFont.Measure(inputActA).X * scale + spacing + buttonATextureWidth + spacing + (controlB != null ? ActiveFont.Measure(inputActB).X * scale + spacing + buttonBTextureWidth + spacing : 0) + ActiveFont.Measure(textC).X * scale;
                    }
                }
                float selectHoldPosition = 0;
                float selectAndPressPosition = 0;
                float selectPosition = 0;
                float SpritePosition = 0;
                float inputActAPosition = 0;
                float InputAPosition = 0;
                float inputActBPosition = 0;
                float InputBPosition = 0;
                float InputCPosition = 0f;
                float TextCPosition = 0;
                if (!select)
                {
                    inputActAPosition = vector.X - TotalLenght / 2f + (ActiveFont.Measure(inputActA).X * scale) / 2;
                    InputAPosition = inputActAPosition + (ActiveFont.Measure(inputActA).X * scale) / 2 + spacing + buttonATextureWidth / 2;
                    inputActBPosition = InputAPosition + buttonATextureWidth / 2 + spacing + (ActiveFont.Measure(inputActB).X * scale) / 2;
                    InputBPosition = inputActBPosition + (ActiveFont.Measure(inputActB).X * scale) / 2 + spacing + buttonBTextureWidth / 2;
                    TextCPosition = (controlB == null ? -2 * spacing : 0) + InputBPosition + buttonBTextureWidth / 2 + spacing + (ActiveFont.Measure(textC).X * scale) / 2;
                }
                else
                {
                    if (!XaphanModule.useMetroidGameplay)
                    {
                        selectHoldPosition = vector.X - TotalLenght / 2f + (ActiveFont.Measure(selectHold).X * scale) / 2;
                        InputAPosition = selectHoldPosition + (ActiveFont.Measure(selectHold).X * scale) / 2 + spacing + buttonATextureWidth / 2;
                        selectAndPressPosition = InputAPosition + buttonATextureWidth / 2 + spacing + (ActiveFont.Measure(selectAndPress).X * scale) / 2;
                        InputCPosition = selectAndPressPosition + (ActiveFont.Measure(selectAndPress).X * scale) / 2 + spacing + buttonBTextureWidth / 2;
                        selectPosition = InputCPosition + buttonBTextureWidth / 2 + spacing + (ActiveFont.Measure(selectString).X * scale) / 2;
                        SpritePosition = selectPosition + (ActiveFont.Measure(selectString).X * scale) / 2 + spacing + spriteWidth / 2;
                        inputActAPosition = SpritePosition + spriteWidth / 2 + spacing + (ActiveFont.Measure(inputActA).X * scale) / 2;
                        InputBPosition = inputActAPosition + (ActiveFont.Measure(inputActA).X * scale) / 2 + spacing + buttonBTextureWidth / 2;
                        TextCPosition = (controlB == null ? -2 * spacing : 0) + InputBPosition + buttonBTextureWidth / 2 + spacing + (ActiveFont.Measure(textC).X * scale) / 2;
                    }
                    else
                    {
                        selectPosition = vector.X - TotalLenght / 2f + (ActiveFont.Measure(selectString).X * scale) / 2;
                        SpritePosition = selectPosition + (ActiveFont.Measure(selectString).X * scale) / 2 + spacing + spriteWidth / 2;
                        inputActAPosition = SpritePosition + spriteWidth / 2 + spacing + (ActiveFont.Measure(inputActA).X * scale) / 2;
                        InputAPosition = inputActAPosition + (ActiveFont.Measure(inputActA).X * scale) / 2 + spacing + buttonATextureWidth / 2;
                        inputActBPosition = InputAPosition + buttonATextureWidth / 2 + spacing + (ActiveFont.Measure(inputActB).X * scale) / 2;
                        InputBPosition = inputActBPosition + (ActiveFont.Measure(inputActB).X * scale) / 2 + spacing + buttonBTextureWidth / 2;
                        TextCPosition = (controlB == null ? -2 * spacing : 0) + InputBPosition + buttonBTextureWidth / 2 + spacing + (ActiveFont.Measure(textC).X * scale) / 2;
                    }
                }
                if (select)
                {
                    if (!XaphanModule.useMetroidGameplay)
                    {
                        ActiveFont.Draw(selectHold, new Vector2(selectHoldPosition, vector.Y), new Vector2(0.5f, 0.5f), Vector2.One * scale, color);
                        ActiveFont.Draw(selectAndPress, new Vector2(selectAndPressPosition, vector.Y), new Vector2(0.5f, 0.5f), Vector2.One * scale, color);
                        ActiveFont.Draw(selectString, new Vector2(selectPosition, vector.Y), new Vector2(0.5f, 0.5f), Vector2.One * scale, color);
                        upgradeSprite.Position = new Vector2(SpritePosition, vector.Y);
                        if (controlB is VirtualButton)
                        {
                            MTexture buttonBTexture = Input.GuiButton((VirtualButton)controlB, "controls/keyboard/oemquestion");
                            buttonBTexture.DrawCentered(new Vector2(InputCPosition, vector.Y), Color.White);
                        }
                        else if (controlB is ButtonBinding)
                        {
                            VirtualButton Button = new VirtualButton();
                            ButtonBinding ControlB = (ButtonBinding)controlB;
                            Button.Binding = ControlB.Binding;
                            MTexture buttonBTexture = Input.GuiButton(Button, "controls/keyboard/oemquestion");
                            buttonATextureWidth = buttonBTexture.Width;
                            buttonBTexture.DrawCentered(new Vector2(InputCPosition, vector.Y), Color.White);
                        }
                    }
                    else
                    {
                        ActiveFont.Draw(selectString, new Vector2(selectPosition, vector.Y), new Vector2(0.5f, 0.5f), Vector2.One * scale, color);
                        upgradeSprite.Position = new Vector2(SpritePosition, vector.Y);
                    }
                }
                ActiveFont.Draw(inputActA, new Vector2(inputActAPosition, vector.Y), new Vector2(0.5f, 0.5f), Vector2.One * scale, color);
                if (controlA is VirtualButton)
                {
                    MTexture buttonATexture = Input.GuiButton((VirtualButton)controlA, "controls/keyboard/oemquestion");
                    buttonATexture.DrawCentered(new Vector2(InputAPosition, vector.Y), Color.White);
                }
                else if (controlA is ButtonBinding)
                {
                    VirtualButton Button = new VirtualButton();
                    ButtonBinding ControlA = (ButtonBinding)controlA;
                    Button.Binding = ControlA.Binding;
                    MTexture buttonATexture = Input.GuiButton(Button, "controls/keyboard/oemquestion");
                    buttonATextureWidth = buttonATexture.Width;
                    buttonATexture.DrawCentered(new Vector2(InputAPosition, vector.Y), Color.White);
                }
                ActiveFont.Draw(inputActB, new Vector2(inputActBPosition, vector.Y), new Vector2(0.5f, 0.5f), Vector2.One * scale, color);
                if (controlB is VirtualButton)
                {
                    MTexture buttonBTexture = Input.GuiButton((VirtualButton)controlB, "controls/keyboard/oemquestion");
                    buttonBTexture.DrawCentered(new Vector2(InputBPosition, vector.Y), Color.White);
                }
                else if (controlB is ButtonBinding)
                {
                    VirtualButton Button = new VirtualButton();
                    ButtonBinding ControlB = (ButtonBinding)controlB;
                    Button.Binding = ControlB.Binding;
                    MTexture buttonBTexture = Input.GuiButton(Button, "controls/keyboard/oemquestion");
                    buttonATextureWidth = buttonBTexture.Width;
                    buttonBTexture.DrawCentered(new Vector2(InputBPosition, vector.Y), Color.White);
                }
                ActiveFont.Draw(text, new Vector2(TextCPosition, vector.Y), new Vector2(0.5f, 0.5f), Vector2.One * scale, color);
            }
        }

        public override void Update()
        {
            timer += Engine.DeltaTime;
            for (int i = 0; i < particles.Length; i++)
            {
                particles[i].Percent += Engine.DeltaTime / particles[i].Duration * ParticleSpeed;
                if (particles[i].Percent > 1f)
                {
                    particles[i].Reset(0f);
                }
            }
            if (Sprite != null)
            {
                Sprite.Update();
            }
            if (upgradeSprite != null)
            {
                upgradeSprite.Color = Color.White * Alpha;
            }
        }

        public void BeforeRender()
        {
            if (!disposed)
            {
                Engine.Graphics.GraphicsDevice.SetRenderTarget(poem);
                Engine.Graphics.GraphicsDevice.Clear(Color.Transparent);
                Matrix transformationMatrix = Matrix.CreateScale(poem.Width / 1920f);
                Draw.SpriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.LinearClamp, null, null, null, transformationMatrix);
                MTexture mTexture = OVR.Atlas["snow"];
                for (int i = 0; i < particles.Length; i++)
                {
                    Particle particle = particles[i];
                    float num = Ease.SineIn(particle.Percent);
                    Vector2 position = Offset + new Vector2(1920f, 1080f) * 0.5f + particle.Direction * (1f - num) * 1920f;
                    float x = 1f + num * 2f;
                    float y = 0.25f * (0.25f + (1f - num) * 0.75f);
                    float scale = 1f - num;
                    mTexture.DrawCentered(position, Calc.HexToColor(poemParticleColor) * scale, new Vector2(x, y), (-particle.Direction).Angle());
                }
                if (Sprite != null)
                {
                    Sprite.Render();
                }
                if (upgradeSprite != null)
                {
                    upgradeSprite.Render();
                }
                if (!string.IsNullOrEmpty(textA) && string.IsNullOrEmpty(textB) && string.IsNullOrEmpty(textC))
                {
                    DrawPoem(textA, Offset + new Vector2(-2f, 0f), Color.Black * TextAlpha, 1.5f, 29f);
                    DrawPoem(textA, Offset + new Vector2(2f, 0f), Color.Black * TextAlpha, 1.5f, 29f);
                    DrawPoem(textA, Offset + new Vector2(0f, -2f), Color.Black * TextAlpha, 1.5f, 29f);
                    DrawPoem(textA, Offset + new Vector2(0f, 2f), Color.Black * TextAlpha, 1.5f, 29f);
                    DrawPoem(textA, Offset + Vector2.Zero, Calc.HexToColor(poemColorA) * TextAlpha, 1.5f, 29f);
                }
                else if (!string.IsNullOrEmpty(textA) && !string.IsNullOrEmpty(textB) && string.IsNullOrEmpty(textC) && controlA == null)
                {
                    DrawPoem(textA, Offset + new Vector2(-2f, -50f), Color.Black * TextAlpha, 1.5f, 29f);
                    DrawPoem(textA, Offset + new Vector2(2f, -50f), Color.Black * TextAlpha, 1.5f, 29f);
                    DrawPoem(textA, Offset + new Vector2(0f, -52f), Color.Black * TextAlpha, 1.5f, 29f);
                    DrawPoem(textA, Offset + new Vector2(0f, -48f), Color.Black * TextAlpha, 1.5f, 29f);
                    DrawPoem(textA, Offset + new Vector2(0f, -50f), Calc.HexToColor(poemColorA) * TextAlpha, 1.5f, 29f);
                    DrawPoem(textB, Offset + new Vector2(-2f, 50f), Color.Black * TextAlpha, 1.3f, 22f, true);
                    DrawPoem(textB, Offset + new Vector2(2f, 50f), Color.Black * TextAlpha, 1.3f, 22f, true);
                    DrawPoem(textB, Offset + new Vector2(0f, 48f), Color.Black * TextAlpha, 1.3f, 22f, true);
                    DrawPoem(textB, Offset + new Vector2(0f, 52f), Color.Black * TextAlpha, 1.3f, 22f, true);
                    DrawPoem(textB, Offset + new Vector2(0f, 50f), Calc.HexToColor(poemColorB) * TextAlpha, 1.3f, 22f, true);
                }
                else if (!string.IsNullOrEmpty(textA) && !string.IsNullOrEmpty(textB) && string.IsNullOrEmpty(textC) && controlA != null)
                {
                    DrawPoem(textA, Offset + new Vector2(-2f, -100f), Color.Black * TextAlpha, 1.5f, 29f);
                    DrawPoem(textA, Offset + new Vector2(2f, -100f), Color.Black * TextAlpha, 1.5f, 29f);
                    DrawPoem(textA, Offset + new Vector2(0f, -102f), Color.Black * TextAlpha, 1.5f, 29f);
                    DrawPoem(textA, Offset + new Vector2(0f, -98f), Color.Black * TextAlpha, 1.5f, 29f);
                    DrawPoem(textA, Offset + new Vector2(0f, -100f), Calc.HexToColor(poemColorA) * TextAlpha, 1.5f, 29f);
                    DrawPoem(textB, Offset + new Vector2(-2f, 0f), Color.Black * TextAlpha, 1.3f, 22f, true);
                    DrawPoem(textB, Offset + new Vector2(2f, 0f), Color.Black * TextAlpha, 1.3f, 22f, true);
                    DrawPoem(textB, Offset + new Vector2(0f, -2f), Color.Black * TextAlpha, 1.3f, 22f, true);
                    DrawPoem(textB, Offset + new Vector2(0f, 2f), Color.Black * TextAlpha, 1.3f, 22f, true);
                    DrawPoem(textB, Offset + Vector2.Zero, Calc.HexToColor(poemColorB) * TextAlpha, 1.3f, 22f, true);
                    DrawPoem("", Offset + new Vector2(-2f, 100f), Color.Black * TextAlpha, 1f, 15f, true, true);
                    DrawPoem("", Offset + new Vector2(2f, 100f), Color.Black * TextAlpha, 1f, 15f, true, true);
                    DrawPoem("", Offset + new Vector2(0f, 98f), Color.Black * TextAlpha, 1f, 15f, true, true);
                    DrawPoem("", Offset + new Vector2(0f, 102f), Color.Black * TextAlpha, 1f, 15f, true, true);
                    DrawPoem("", Offset + new Vector2(0f, 100f), Calc.HexToColor(poemColorC) * TextAlpha, 1f, 15f, true, true);
                }
                else if (!string.IsNullOrEmpty(textA) && string.IsNullOrEmpty(textB) && !string.IsNullOrEmpty(textC) && controlA != null)
                {
                    DrawPoem(textA, Offset + new Vector2(-2f, -50f), Color.Black * TextAlpha, 1.5f, 29f);
                    DrawPoem(textA, Offset + new Vector2(2f, -50f), Color.Black * TextAlpha, 1.5f, 29f);
                    DrawPoem(textA, Offset + new Vector2(0f, -52f), Color.Black * TextAlpha, 1.5f, 29f);
                    DrawPoem(textA, Offset + new Vector2(0f, -48f), Color.Black * TextAlpha, 1.5f, 29f);
                    DrawPoem(textA, Offset + new Vector2(0f, -50f), Calc.HexToColor(poemColorA) * TextAlpha, 1.5f, 29f);
                    DrawPoem(textC, Offset + new Vector2(-2f, 50f), Color.Black * TextAlpha, 1.3f, 22f, true, true);
                    DrawPoem(textC, Offset + new Vector2(2f, 50f), Color.Black * TextAlpha, 1.3f, 22f, true, true);
                    DrawPoem(textC, Offset + new Vector2(0f, 48f), Color.Black * TextAlpha, 1.3f, 22f, true, true);
                    DrawPoem(textC, Offset + new Vector2(0f, 52f), Color.Black * TextAlpha, 1.3f, 22f, true, true);
                    DrawPoem(textC, Offset + new Vector2(0f, 50f), Calc.HexToColor(poemColorC) * TextAlpha, 1.3f, 22f, true, true);
                }
                else if (!string.IsNullOrEmpty(textA) && !string.IsNullOrEmpty(textB) && !string.IsNullOrEmpty(textC) && controlA == null)
                {
                    DrawPoem(textA, Offset + new Vector2(-2f, -100f), Color.Black * TextAlpha, 1.5f, 29f);
                    DrawPoem(textA, Offset + new Vector2(2f, -100f), Color.Black * TextAlpha, 1.5f, 29f);
                    DrawPoem(textA, Offset + new Vector2(0f, -102f), Color.Black * TextAlpha, 1.5f, 29f);
                    DrawPoem(textA, Offset + new Vector2(0f, -98f), Color.Black * TextAlpha, 1.5f, 29f);
                    DrawPoem(textA, Offset + new Vector2(0f, -100f), Calc.HexToColor(poemColorA) * TextAlpha, 1.5f, 29f);
                    DrawPoem(textB, Offset + new Vector2(-2f, 0f), Color.Black * TextAlpha, 1.3f, 22f, true);
                    DrawPoem(textB, Offset + new Vector2(2f, 0f), Color.Black * TextAlpha, 1.3f, 22f, true);
                    DrawPoem(textB, Offset + new Vector2(0f, -2f), Color.Black * TextAlpha, 1.3f, 22f, true);
                    DrawPoem(textB, Offset + new Vector2(0f, 2f), Color.Black * TextAlpha, 1.3f, 22f, true);
                    DrawPoem(textB, Offset + Vector2.Zero, Calc.HexToColor(poemColorB) * TextAlpha, 1.3f, 22f, true);
                    DrawPoem(textC, Offset + new Vector2(-2f, 100f), Color.Black * TextAlpha, 1.3f, 22f, true);
                    DrawPoem(textC, Offset + new Vector2(2f, 100f), Color.Black * TextAlpha, 1.3f, 22f, true);
                    DrawPoem(textC, Offset + new Vector2(0f, 98f), Color.Black * TextAlpha, 1.3f, 22f, true);
                    DrawPoem(textC, Offset + new Vector2(0f, 102f), Color.Black * TextAlpha, 1.3f, 22f, true);
                    DrawPoem(textC, Offset + new Vector2(0f, 100f), Calc.HexToColor(poemColorC) * TextAlpha, 1.3f, 22f, true);
                }
                else if (!string.IsNullOrEmpty(textA) && !string.IsNullOrEmpty(textB) && !string.IsNullOrEmpty(textC) && controlA != null)
                {
                    DrawPoem(textA, Offset + new Vector2(-2f, -100f), Color.Black * TextAlpha, 1.5f, 29f);
                    DrawPoem(textA, Offset + new Vector2(2f, -100f), Color.Black * TextAlpha, 1.5f, 29f);
                    DrawPoem(textA, Offset + new Vector2(0f, -102f), Color.Black * TextAlpha, 1.5f, 29f);
                    DrawPoem(textA, Offset + new Vector2(0f, -98f), Color.Black * TextAlpha, 1.5f, 29f);
                    DrawPoem(textA, Offset + new Vector2(0f, -100f), Calc.HexToColor(poemColorA) * TextAlpha, 1.5f, 29f);
                    DrawPoem(textB, Offset + new Vector2(-2f, 0f), Color.Black * TextAlpha, 1.3f, 22f, true);
                    DrawPoem(textB, Offset + new Vector2(2f, 0f), Color.Black * TextAlpha, 1.3f, 22f, true);
                    DrawPoem(textB, Offset + new Vector2(0f, -2f), Color.Black * TextAlpha, 1.3f, 22f, true);
                    DrawPoem(textB, Offset + new Vector2(0f, 2f), Color.Black * TextAlpha, 1.3f, 22f, true);
                    DrawPoem(textB, Offset + Vector2.Zero, Calc.HexToColor(poemColorB) * TextAlpha, 1.3f, 22f, true);
                    DrawPoem(textC, Offset + new Vector2(-2f, 100f), Color.Black * TextAlpha, 1f, 15f, true, true);
                    DrawPoem(textC, Offset + new Vector2(2f, 100f), Color.Black * TextAlpha, 1f, 15f, true, true);
                    DrawPoem(textC, Offset + new Vector2(0f, 98f), Color.Black * TextAlpha, 1f, 15f, true, true);
                    DrawPoem(textC, Offset + new Vector2(0f, 102f), Color.Black * TextAlpha, 1f, 15f, true, true);
                    DrawPoem(textC, Offset + new Vector2(0f, 100f), Calc.HexToColor(poemColorC) * TextAlpha, 1f, 15f, true, true);
                }
                Draw.SpriteBatch.End();
                Engine.Graphics.GraphicsDevice.SetRenderTarget(smoke);
                Engine.Graphics.GraphicsDevice.Clear(Color.Transparent);
                MagicGlow.Render(poem, timer, -1f, Matrix.CreateScale(0.5f));
                GaussianBlur.Blur(smoke, temp, smoke);
            }
        }

        public override void Render()
        {
            if (!disposed && !Scene.Paused)
            {
                float num = 1920f / poem.Width;
                Draw.SpriteBatch.Draw(smoke, Vector2.Zero, smoke.Bounds, Color.White * 0.3f * Alpha, 0f, Vector2.Zero, num * 2f, SpriteEffects.None, 0f);
                Draw.SpriteBatch.Draw(poem, Vector2.Zero, poem.Bounds, Color.White * Alpha, 0f, Vector2.Zero, num, SpriteEffects.None, 0f);
            }
            if (select && upgradeSprite != null)
            {
                upgradeSprite.Play("static");
                upgradeSprite.Render();
            }
        }

        public override void Removed(Scene scene)
        {
            base.Removed(scene);
            Dispose();
        }

        public override void SceneEnd(Scene scene)
        {
            base.SceneEnd(scene);
            Dispose();
        }
    }
}