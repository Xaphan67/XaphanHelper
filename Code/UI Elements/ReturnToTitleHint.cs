using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste.Mod.XaphanHelper.UI_Elements
{
    public class RestartToTitleHint : Entity
    {
        public RestartToTitleHint()
        {
            Tag = Tags.HUD;
        }

        public override void Render()
        {
            MTexture mTexture = GFX.Gui["checkpoint"];
            string text = Dialog.Clean("Xaphan_0_Pause_Menu_ReturnTitle_info");
            float width = ActiveFont.Measure(text).X * 0.75f;
            float textureWidth = mTexture.Width * 0.75f;
            Vector2 value2 = new((1920f - width - textureWidth - 64f) / 2f, 730f);
            ActiveFont.DrawOutline(text, value2 + new Vector2(width / 2f, 0f), new Vector2(0.5f, 0.5f), Vector2.One * 0.75f, Color.LightGray, 2f, Color.Black);
            value2.X += width + 64f;
            mTexture.DrawCentered(value2 + new Vector2(textureWidth * 0.5f, 0f), Color.White, 0.75f);
        }
    }
}
