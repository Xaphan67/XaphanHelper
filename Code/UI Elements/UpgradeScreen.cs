using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Monocle;
using System;
using System.Text.RegularExpressions;

namespace Celeste.Mod.XaphanHelper.UI_Elements {
    public class UpgradeScreen : Entity {
        public const float MaxWidth = Celeste.TargetWidth * 0.9f;
        public const float NameScale = 1.5f;
        public const float DescriptionScale = 1.2f;

        public float Alpha = 1f;

        private readonly Particle[] particles = new Particle[80];
        private readonly VirtualRenderTarget upgradeScreen;
        private readonly VirtualRenderTarget smoke;
        private readonly VirtualRenderTarget temp;
        private readonly VirtualButton buttonA;
        private readonly VirtualButton buttonB;
        private readonly MTexture upgradeTexture;
        private readonly Color nameColor;
        private readonly Color descColor;
        private readonly Color controlsColor;
        private readonly Color particleColor;
        private readonly string name;
        private readonly string description;
        private readonly string controls;

        private float timer = 0f;
        private bool disposed;

        public UpgradeScreen(string upgradeTexture, string name, string description = "", string controls = "", string nameColor = "FFFFFF", string descColor = "FFFFFF", string controlsColor = "FFFFFF", string particleColor = "FFFFFF", VirtualButton buttonA = null, VirtualButton buttonB = null) {
            this.name = name;
            this.description = ActiveFont.FontSize.AutoNewline(description, (int)(MaxWidth / DescriptionScale));
            this.controls = controls;
            this.buttonA = buttonA ?? new VirtualButton();
            this.buttonB = buttonB ?? new VirtualButton();
            this.nameColor = Calc.HexToColor(nameColor);
            this.descColor = Calc.HexToColor(descColor);
            this.controlsColor = Calc.HexToColor(controlsColor);
            this.particleColor = Calc.HexToColor(particleColor);
            this.upgradeTexture = GFX.Gui.GetAtlasSubtexturesAt(upgradeTexture, 0);

            int width = Math.Min(Engine.ViewWidth, Celeste.TargetWidth);
            int height = Math.Min(Engine.ViewHeight, Celeste.TargetHeight);

            upgradeScreen = VirtualContent.CreateRenderTarget("XaphanHelper_upgrade-a", width, height);
            smoke = VirtualContent.CreateRenderTarget("XaphanHelper_upgrade-b", width / 2, height / 2);
            temp = VirtualContent.CreateRenderTarget("XaphanHelper_upgrade-c", width / 2, height / 2);

            Tag = Tags.HUD | Tags.FrozenUpdate;
            Add(new BeforeRenderHook(BeforeRender));
            for (int i = 0; i < particles.Length; i++) {
                particles[i].Reset(Calc.Random.NextFloat());
                ;
            }
        }

        public override void Update() {
            timer += Engine.DeltaTime;

            for (int i = 0; i < particles.Length; i++) {
                particles[i].Percent += Engine.DeltaTime / particles[i].Duration;
                if (particles[i].Percent > 1f) {
                    particles[i].Reset(0f);
                }
            }
        }

        public override void Render() {
            if (!disposed && !Scene.Paused) {
                float scale = (float)Celeste.TargetWidth / upgradeScreen.Width;
                Draw.SpriteBatch.Draw(smoke, Vector2.Zero, smoke.Bounds, Color.White * 0.3f * Alpha, 0f, Vector2.Zero, scale * 2f, SpriteEffects.None, 0f);
                Draw.SpriteBatch.Draw(upgradeScreen, Vector2.Zero, upgradeScreen.Bounds, Color.White * Alpha, 0f, Vector2.Zero, scale, SpriteEffects.None, 0f);
            }
        }

        public override void Removed(Scene scene) {
            base.Removed(scene);
            Dispose();
        }

        public override void SceneEnd(Scene scene) {
            base.SceneEnd(scene);
            Dispose();
        }

        private void BeforeRender() {
            if (!disposed) {
                Engine.Graphics.GraphicsDevice.SetRenderTarget(upgradeScreen);
                Engine.Graphics.GraphicsDevice.Clear(Color.Transparent);
                Matrix transformationMatrix = Matrix.CreateScale((float)upgradeScreen.Width / Celeste.TargetWidth);

                // Draw upscaled upgrade texture
                Draw.SpriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, null, null, null, transformationMatrix);
                upgradeTexture.DrawCentered(Celeste.TargetCenter, Color.White * 0.5f, 10f);
                Draw.SpriteBatch.End();

                // Draw hi-res assets
                Draw.SpriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.LinearClamp, null, null, null, transformationMatrix);

                MTexture snowTexture = OVR.Atlas["snow"];
                for (int i = 0; i < particles.Length; i++) {
                    Particle particle = particles[i];
                    float num = Ease.SineIn(particle.Percent);
                    Vector2 position = Celeste.TargetCenter + particle.Direction * (1f - num) * Celeste.TargetHeight;
                    float x = 1f + num * 2f;
                    float y = 0.25f * (0.25f + (1f - num) * 0.75f);
                    float scale = 1f - num;
                    snowTexture.DrawCentered(position, particleColor * scale, new Vector2(x, y), (-particle.Direction).Angle());
                }

                if (!string.IsNullOrEmpty(name)) {
                    float nameSpacing = ActiveFont.HeightOf(name) * NameScale / 2f;
                    float descSpacing = ActiveFont.HeightOf(description) * DescriptionScale / 2f;
                    if (!string.IsNullOrEmpty(description) && !string.IsNullOrEmpty(controls)) {
                        float spacing = nameSpacing + descSpacing;
                        DrawName(name, Celeste.TargetCenter - Vector2.UnitY * spacing, nameColor, NameScale);
                        DrawDescription(description, Celeste.TargetCenter, descColor, DescriptionScale);
                        DrawControls(controls, Celeste.TargetCenter + Vector2.UnitY * spacing, controlsColor);
                    } else if (!string.IsNullOrEmpty(description)) {
                        DrawName(name, Celeste.TargetCenter - Vector2.UnitY * nameSpacing, nameColor, NameScale);
                        DrawDescription(description, Celeste.TargetCenter + Vector2.UnitY * descSpacing, descColor, DescriptionScale);
                    } else if (!string.IsNullOrEmpty(controls)) {
                        DrawName(name, Celeste.TargetCenter - Vector2.UnitY * nameSpacing, nameColor, NameScale);
                        DrawControls(controls, Celeste.TargetCenter + Vector2.UnitY * ActiveFont.LineHeight, controlsColor);
                    } else {
                        DrawName(name, Celeste.TargetCenter, nameColor, NameScale);
                    }
                }

                Draw.SpriteBatch.End();
                Engine.Graphics.GraphicsDevice.SetRenderTarget(smoke);
                Engine.Graphics.GraphicsDevice.Clear(Color.Transparent);
                MagicGlow.Render(upgradeScreen, timer, -1f, Matrix.CreateScale(0.5f));
                GaussianBlur.Blur(smoke, temp, smoke);
            }
        }

        private void DrawName(string text, Vector2 position, Color color, float scale) {
            MTexture poemDot = GFX.Gui["poemside"];
            float length = ActiveFont.Measure(text).X * scale;
            poemDot.DrawOutlineCentered(position - Vector2.UnitX * (length / 2 + 64f), color);
            ActiveFont.DrawOutline(text, position, new Vector2(0.5f), Vector2.One * scale, color, 2f, Color.Black);
            poemDot.DrawOutlineCentered(position + Vector2.UnitX * (length / 2 + 64f), color);
        }

        private void DrawDescription(string text, Vector2 position, Color color, float scale) {
            ActiveFont.DrawOutline(text, position, new Vector2(0.5f), Vector2.One * scale, color, 2f, Color.Black);
        }

        private void DrawControls(string text, Vector2 position, Color color) {
            MTexture buttonATexture = Input.GuiButton(buttonA, "controls/keyboard/oemquestion");
            MTexture buttonBTexture = Input.GuiButton(buttonB, "controls/keyboard/oemquestion");
            string[] controlsSplit = Regex.Split(text, @"(\(\w\))");

            float length = 0;
            foreach (string sub in controlsSplit) {
                length += sub switch {
                    "(A)" => buttonATexture.Width,
                    "(B)" => buttonBTexture.Width,
                    "(U)" => upgradeTexture.Width,
                    _ => ActiveFont.Measure(sub).X
                };
            }

            float scale = (length <= MaxWidth) ? 1f : MaxWidth / length;
            position.X -= length / 2 * scale;
            Vector2 leftCenter = new(0f, 0.5f);
            foreach (string sub in controlsSplit) {
                switch (sub) {
                    case "(A)":
                        buttonATexture.DrawOutlineJustified(position, leftCenter, Color.White, scale);
                        position.X += buttonATexture.Width * scale;
                        break;
                    case "(B)":
                        buttonBTexture.DrawOutlineJustified(position, leftCenter, Color.White, scale);
                        position.X += buttonBTexture.Width * scale;
                        break;
                    case "(U)":
                        upgradeTexture.DrawOutlineJustified(position, leftCenter, Color.White * Alpha, scale * 1.5f);
                        position.X += upgradeTexture.Width * 1.5f * scale;
                        break;
                    default:
                        ActiveFont.DrawOutline(sub, position, leftCenter, Vector2.One * scale, color, 2f, Color.Black);
                        position.X += ActiveFont.Measure(sub).X * scale;
                        break;
                }
            }
        }

        private void Dispose() {
            if (!disposed) {
                upgradeScreen.Dispose();
                smoke.Dispose();
                temp.Dispose();
                RemoveSelf();
                disposed = true;
            }
        }

        private struct Particle {
            public Vector2 Direction;
            public float Percent;
            public float Duration;

            public void Reset(float percent) {
                Direction = Calc.AngleToVector(Calc.Random.NextFloat((float)Math.PI * 2f), 1f);
                Percent = percent;
                Duration = 0.5f + Calc.Random.NextFloat() * 0.5f;
            }
        }
    }
}
