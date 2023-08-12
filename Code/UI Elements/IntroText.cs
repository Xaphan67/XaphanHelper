using System;
using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste.Mod.XaphanHelper.UI_Elements
{
    public class IntroText : Entity
    {
        public string message;

        private SoundSource textSfx;

        private int firstLineLength;

        private float widestCharacter = 0f;

        public float timer = 0f;

        private float alpha = 0f;

        private float index;

        private int textPositionX;

        private int textPositionY;

        public bool Show;

        public bool outline;

        public Color Color;

        public float Scale;

        public bool FastDisplay;

        private bool textSfxPlaying;

        private bool Silent;

        private bool SmallLineHeight;

        public IntroText(string text, string textPositionX, int textPositionY, Color color, float scale = 2f, bool dialog = true, bool outline = false, bool fastdisplay = false, bool silent = true, bool smallLineHeight = false)
        {
            Tag = Tags.HUD | Tags.Global | Tags.PauseUpdate | Tags.TransitionUpdate;
            Silent = silent;
            if (!Silent)
            {
                Add(textSfx = new SoundSource());
            }
            if (dialog)
            {
                message = Dialog.Clean(text);
            }
            else
            {
                message = text;
            }
            Color = color;
            Scale = scale;
            firstLineLength = CountToNewline(0);
            for (int i = 0; i < message.Length; i++)
            {
                float x = ActiveFont.Measure(message[i]).X;
                if (x > widestCharacter)
                {
                    widestCharacter = x;
                }
            }
            widestCharacter *= 0.9f;
            if (textPositionX == "Left")
            {
                this.textPositionX = message.Length * (int)widestCharacter / 2 + 20;
            }
            else if (textPositionX == "Middle")
            {
                this.textPositionX = 960;
            }
            else if (textPositionX == "Right")
            {
                this.textPositionX = 1920 - message.Length * (int)widestCharacter / 2 - 20;
            }
            this.textPositionY = textPositionY;
            this.outline = outline;
            FastDisplay = fastdisplay;
            SmallLineHeight = smallLineHeight;
        }

        public override void Update()
        {
            base.Update();
            timer += Engine.DeltaTime;
            if (!Show)
            {
                alpha = Calc.Approach(alpha, 0f, Engine.DeltaTime);
                if (alpha <= 0f)
                {
                    index = firstLineLength;
                }
            }
            else
            {
                alpha = Calc.Approach(alpha, 1f, FastDisplay ? 1 : Engine.DeltaTime * 2f);
                if (alpha >= 1f)
                {
                    index = Calc.Approach(index, message.Length, FastDisplay ? 1 : 32f * Engine.DeltaTime);
                }
            }
            if (!Silent)
            {
                if (Show && alpha >= 1f && index < message.Length)
                {
                    if (!textSfxPlaying)
                    {
                        textSfxPlaying = true;
                        textSfx.Play("event:/ui/game/memorial_text_loop");
                        textSfx.Param("end", 0f);
                    }
                }
                else if (textSfxPlaying)
                {
                    textSfxPlaying = false;
                    textSfx.Stop();
                    textSfx.Param("end", 1f);
                }
                textSfx.Resume();
            }
        }

        public void UpdatePosition(Vector2 to)
        {
            textPositionY += (int)to.Y;
        }

        public void UpdateText(string text)
        {
            message = text;
        }

        private int CountToNewline(int start)
        {
            int i;
            for (i = start; i < message.Length && message[i] != '\n'; i++)
            {
            }
            return i - start;
        }

        public override void Render()
        {
            if (Visible == true)
            {
                if (!(index > 0f) || !(alpha > 0f))
                {
                    return;
                }
                Vector2 value = new(textPositionX, textPositionY);
                if (SaveData.Instance != null && SaveData.Instance.Assists.MirrorMode)
                {
                    value.X = 1920f - value.X;
                }
                float num = Ease.CubeInOut(alpha);
                int num2 = (int)Math.Min(message.Length, index);
                int num3 = 0;
                float num4 = 64f * (1f - num);
                int num5 = CountToNewline(0);
                for (int i = 0; i < num2; i++)
                {
                    char c = message[i];
                    if (c == '\n')
                    {
                        num3 = 0;
                        num5 = CountToNewline(i + 1);
                        num4 += ActiveFont.LineHeight * (SmallLineHeight ? 0.55f : 1.1f);
                        continue;
                    }
                    float x = Scale;
                    float x2 = -num5 * widestCharacter / 2f + (num3 + 0.5f) * widestCharacter;
                    float num6 = 0f;
                    if (!outline)
                    {
                        ActiveFont.Draw(c, value + new Vector2(x2 * Scale, (num4 + num6) * 2), new Vector2(0.5f, 0.5f), new Vector2(x, Scale), Color * num);
                    }
                    else
                    {
                        ActiveFont.DrawOutline(c.ToString(), value + new Vector2(x2 * Scale, (num4 + num6) * 2), new Vector2(0.5f, 0.5f), new Vector2(x, Scale), Color * num, 2f, Color.Black * num);
                    }
                    num3++;
                }
            }
        }
    }
}
