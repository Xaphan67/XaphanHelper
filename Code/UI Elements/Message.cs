using System.Collections;
using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste.Mod.XaphanHelper.UI_Elements
{
    public class Message : Entity
    {
        private string Text;

        private string OptText;

        private int OptNumber;

        private Vector2 TextSize;

        private float height;

        public bool drawText;

        private string OpenSound;

        public Message(Vector2 position, string openSound, string text, int optNumber = 0, string optText = null) : base(position)
        {
            Tag = (Tags.HUD | Tags.Persistent);
            OpenSound = openSound;
            Text = text;
            OptNumber = optNumber;
            OptText = optText;
            TextSize = ActiveFont.Measure(Dialog.Clean(text));
        }

        public override void Added(Scene scene)
        {
            base.Added(scene);
            Add(new Coroutine(OpenRoutine()));
        }

        public IEnumerator OpenRoutine()
        {
            Audio.Play(OpenSound);
            while (height < TextSize.Y * (!string.IsNullOrEmpty(OptText) ? 2 : 1) + 75)
            {
                height += Engine.DeltaTime * 1200;
                yield return null;
            }
            drawText = true;
        }

        public void Close()
        {
            Add(new Coroutine(CloseRoutine()));
        }

        public IEnumerator CloseRoutine()
        {
            drawText = false;
            while (height > 1)
            {
                height -= Engine.DeltaTime * 1200;
                yield return null;
            }
            RemoveSelf();
        }

        public override void Render()
        {
            Draw.Rect(Engine.Width / 2 - TextSize.X / 2 - 50, (Engine.Height / 2 - TextSize.Y / 2 - 130) + ((TextSize.Y + 200) / 2) - height / 2, TextSize.X + 100, height, Color.Black);
            if (drawText)
            {
                float offset = string.IsNullOrEmpty(OptText) ? 0 : TextSize.Y / 2;
                ActiveFont.Draw(Dialog.Clean(Text), new Vector2(Engine.Width / 2, Engine.Height / 2 - TextSize.Y / 2 - offset), new Vector2(0.5f, 0.5f), Vector2.One * 1f, Calc.HexToColor("AA00AA"));
                if (!string.IsNullOrEmpty(OptText))
                {
                    ActiveFont.Draw(OptNumber.ToString() + " " + Dialog.Clean(OptText), new Vector2(Engine.Width / 2, Engine.Height / 2 - TextSize.Y / 2 + offset), new Vector2(0.5f, 0.5f), Vector2.One * 1f, Calc.HexToColor("AA00AA"));
                }
            }
        }
    }
}
