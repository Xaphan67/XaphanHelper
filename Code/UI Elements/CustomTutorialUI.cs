using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste.Mod.XaphanHelper.UI_Elements
{
    public class CustomTutorialUI : Entity
    {
        public enum ButtonPrompt
        {
            Dash,
            Jump,
            Grab,
            Talk
        }

        public bool Open;

        public float Scale;

        private object info;

        private List<object> controls;

        private float controlsWidth;

        private float controlsWidth2;

        private float infoWidth;

        private float infoHeight;

        private float buttonPadding;

        private Color bgColor;

        private Color lineColor;

        private Color textColor;

        private float arrowOffset;

        private Vector2 startPosition;

        private static Dictionary<VirtualButton, ButtonPrompt> controlsButtonsToID = new();

        private static Dictionary<ButtonPrompt, VirtualButton> controlsIDToButton = new();

        public CustomTutorialUI(Vector2 position, object info, params object[] controls)
        {
            Tag = (Tags.HUD | Tags.Persistent | Tags.PauseUpdate);
            for (int i = 0; i < controls.Length; i++)
            {
                if (controls[i] is VirtualButton virtualButton)
                {
                    if (!controlsButtonsToID.TryGetValue(virtualButton, out var value))
                    {
                        value = (ButtonPrompt)(-(controlsButtonsToID.Count + 1));
                        controlsButtonsToID[virtualButton] = value;
                        controlsIDToButton[value] = virtualButton;
                    }
                    controls[i] = value;
                }
            }
            buttonPadding = 8f;
            bgColor = Calc.HexToColor("061526");
            lineColor = new Color(1f, 1f, 1f);
            textColor = Calc.HexToColor("6179e2");
            Position = position;
            startPosition = position;
            this.info = info;
            this.controls = new List<object>(controls);
            if (info is string)
            {
                infoWidth = ActiveFont.Measure((string)info).X;
                infoHeight = ActiveFont.LineHeight;
            }
            else if (info is MTexture)
            {
                infoWidth = ((MTexture)info).Width;
                infoHeight = ((MTexture)info).Height;
            }
            UpdateControlsSize();
            float num = (Math.Max(controlsWidth, Math.Max(controlsWidth2, infoWidth)) + 64f);
            if (Position.X + (num + 6f) / 2 >= 1899f)
            {
                arrowOffset = Position.X + (num + 6f) / 2 - 1899f;
                Position.X -= arrowOffset;
            }
        }

        public void UpdateControlsSize()
        {
            controlsWidth = 0f;
            controlsWidth2 = 0f;
            int startIndex = 0;
            foreach (object control in controls)
            {
                if (control is ButtonPrompt)
                {
                    int index = controls.FindIndex(i => i == control);
                    controlsWidth += Input.GuiButton(ButtonPromptToVirtualButton((ButtonPrompt)control), Input.PrefixMode.Attached).Width + (index < controls.Count - 1 ? buttonPadding * 2f : 0f);
                }
                else if (control is Vector2)
                {
                    controlsWidth += Input.GuiDirection((Vector2)control).Width + buttonPadding * 2f;
                }
                else if (control is string)
                {
                    if (control.ToString() == "{n}")
                    {
                        startIndex = controls.FindIndex(i => i == control) + 1;
                        break;
                    }
                    controlsWidth += ActiveFont.Measure(control.ToString()).X;
                }
                else if (control is MTexture)
                {
                    controlsWidth += ((MTexture)control).Width;
                }
            }
            if (startIndex != 0)
            {
                foreach (object control in controls)
                {
                    if (controls.FindIndex(i => i == control) >= startIndex)
                    {
                        if (control is ButtonPrompt)
                        {
                            int index = controls.FindIndex(i => i == control);
                            controlsWidth2 += Input.GuiButton(ButtonPromptToVirtualButton((ButtonPrompt)control), Input.PrefixMode.Attached).Width + (index < controls.Count - 1 ? buttonPadding * 2f : 0f);
                        }
                        else if (control is Vector2)
                        {
                            controlsWidth2 += Input.GuiDirection((Vector2)control).Width + buttonPadding * 2f;
                        }
                        else if (control is string)
                        {
                            controlsWidth2 += ActiveFont.Measure(control.ToString()).X;
                        }
                        else if (control is MTexture)
                        {
                            controlsWidth2 += ((MTexture)control).Width;
                        }
                    }
                }
            }
        }

        public override void Update()
        {
            UpdateControlsSize();
            Scale = Calc.Approach(Scale, Open ? 1 : 0, Engine.RawDeltaTime * 8f);
            Position.Y = startPosition.Y + TutorialVerticalPositionOffset();
            base.Update();
        }

        private float TutorialVerticalPositionOffset()
        {
            float offset = 0f;
            if (XaphanModule.ModSettings.SelectItem.Check && !XaphanModule.PlayerIsControllingRemoteDrone())
            {
                foreach (BagDisplay display in SceneAs<Level>().Tracker.GetEntities<BagDisplay>())
                {
                    float displayUIHeight = 24f + display.totalActiveUpgrades * 72f + 12f * (display.totalActiveUpgrades - 1);
                    if (displayUIHeight > offset)
                    {
                        offset = displayUIHeight;
                    }
                }
            }
            return offset;
        }

        public override void Render()
        {
            Level level = Scene as Level;
            if (level.FrozenOrPaused || level.RetryPlayerCorpse != null || Scale <= 0f)
            {
                return;
            }
            float lineHeight = ActiveFont.LineHeight;
            float num = (Math.Max(controlsWidth, Math.Max(controlsWidth2, infoWidth)) + 64f) * Scale;
            float num2 = infoHeight + lineHeight * (controlsWidth2 != 0 ? 2 : 1) + 32f;
            float num3 = Position.X - num / 2f;
            float num4 = Position.Y - num2 - 32f;
            Draw.Rect(num3 - 6f, num4 - 6f, num + 12f, num2 + 12f, lineColor);
            Draw.Rect(num3, num4, num, num2, bgColor);
            for (int i = 0; i <= 36; i++)
            {
                float num5 = (i * 2) * Scale;
                Draw.Rect(Position.X - num5 / 2f + arrowOffset, num4 - 37f + i, num5, 1f, lineColor);
                if (num5 > 12f)
                {
                    Draw.Rect(Position.X - num5 / 2f + 6f + arrowOffset, num4 - 37f + i, num5 - 12f, 1f, bgColor);
                }
            }
            if (!(num > 3f))
            {
                return;
            }
            Vector2 vector2 = new(Position.X, num4 + 16f);
            if (info is string)
            {
                ActiveFont.Draw((string)info, vector2, new Vector2(0.5f, 0f), new Vector2(Scale, 1f), textColor);
            }
            else if (info is MTexture)
            {
                ((MTexture)info).DrawJustified(vector2, new Vector2(0.5f, 0f), Color.White, new Vector2(Scale, 1f));
            }
            vector2.Y += infoHeight + lineHeight * 0.5f;
            Vector2 vector3 = new((0f - controlsWidth) / 2f, 0f);
            int startIndex = 0;
            foreach (object control in controls)
            {
                if (control is ButtonPrompt)
                {
                    MTexture mTexture = Input.GuiButton(ButtonPromptToVirtualButton((ButtonPrompt)control), Input.PrefixMode.Attached);
                    vector3.X += buttonPadding;
                    mTexture.Draw(vector2, new Vector2(0f - vector3.X, mTexture.Height / 2), Color.White, new Vector2(Scale, 1f));
                    vector3.X += mTexture.Width + buttonPadding;
                }
                else if (control is Vector2 direction)
                {
                    if (SaveData.Instance != null && SaveData.Instance.Assists.MirrorMode)
                    {
                        direction.X = 0f - direction.X;
                    }
                    MTexture mTexture2 = Input.GuiDirection(direction);
                    vector3.X += buttonPadding;
                    mTexture2.Draw(vector2, new Vector2(0f - vector3.X, mTexture2.Height / 2), Color.White, new Vector2(Scale, 1f));
                    vector3.X += mTexture2.Width + buttonPadding;
                }
                else if (control is string)
                {
                    string text = control.ToString();
                    if (text == "{n}")
                    {
                        vector2.Y += lineHeight;
                        vector3 = new Vector2((0f - controlsWidth2) / 2f, 0f);
                        startIndex = controls.FindIndex(i => i == control) + 1;
                        break;
                    }
                    float x = ActiveFont.Measure(text).X;
                    ActiveFont.Draw(text, vector2 + new Vector2(1f, 2f), new Vector2((0f - vector3.X) / x, 0.5f), new Vector2(Scale, 1f), textColor);
                    ActiveFont.Draw(text, vector2 + new Vector2(1f, -2f), new Vector2((0f - vector3.X) / x, 0.5f), new Vector2(Scale, 1f), Color.White);
                    vector3.X += x + 1f;
                }
                else if (control is MTexture)
                {
                    MTexture mTexture3 = (MTexture)control;
                    mTexture3.Draw(vector2, new Vector2(0f - vector3.X, mTexture3.Height / 2), Color.White, new Vector2(Scale, 1f));
                    vector3.X += mTexture3.Width;
                }
            }
            if (startIndex != 0)
            {
                foreach (object control in controls)
                {
                    if (controls.FindIndex(i => i == control) >= startIndex)
                    {
                        if (control is ButtonPrompt)
                        {
                            MTexture mTexture = Input.GuiButton(ButtonPromptToVirtualButton((ButtonPrompt)control), Input.PrefixMode.Attached);
                            vector3.X += buttonPadding;
                            mTexture.Draw(vector2, new Vector2(0f - vector3.X, mTexture.Height / 2), Color.White, new Vector2(Scale, 1f));
                            vector3.X += mTexture.Width + buttonPadding;
                        }
                        else if (control is Vector2 direction)
                        {
                            if (SaveData.Instance != null && SaveData.Instance.Assists.MirrorMode)
                            {
                                direction.X = 0f - direction.X;
                            }
                            MTexture mTexture2 = Input.GuiDirection(direction);
                            vector3.X += buttonPadding;
                            mTexture2.Draw(vector2, new Vector2(0f - vector3.X, mTexture2.Height / 2), Color.White, new Vector2(Scale, 1f));
                            vector3.X += mTexture2.Width + buttonPadding;
                        }
                        else if (control is string)
                        {
                            string text = control.ToString();
                            float x = ActiveFont.Measure(text).X;
                            ActiveFont.Draw(text, vector2 + new Vector2(1f, 2f), new Vector2((0f - vector3.X) / x, 0.5f), new Vector2(Scale, 1f), textColor);
                            ActiveFont.Draw(text, vector2 + new Vector2(1f, -2f), new Vector2((0f - vector3.X) / x, 0.5f), new Vector2(Scale, 1f), Color.White);
                            vector3.X += x + 1f;
                        }
                        else if (control is MTexture)
                        {
                            MTexture mTexture3 = (MTexture)control;
                            mTexture3.Draw(vector2, new Vector2(0f - vector3.X, mTexture3.Height / 2), Color.White, new Vector2(Scale, 1f));
                            vector3.X += mTexture3.Width;
                        }
                    }
                }
            }
        }

        public static VirtualButton ButtonPromptToVirtualButton(ButtonPrompt prompt)
        {
            if (controlsIDToButton.TryGetValue(prompt, out var value))
            {
                return value;
            }
            return orig_ButtonPromptToVirtualButton(prompt);
        }

        public static VirtualButton orig_ButtonPromptToVirtualButton(ButtonPrompt prompt)
        {
            return prompt switch
            {
                ButtonPrompt.Dash => Input.Dash,
                ButtonPrompt.Jump => Input.Jump,
                ButtonPrompt.Grab => Input.Grab,
                ButtonPrompt.Talk => Input.Talk,
                _ => Input.Jump,
            };
        }
    }
}
