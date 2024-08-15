using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste.Mod.XaphanHelper.UI_Elements
{
    public class RestartCampaignHint : Entity
    {
        bool FromTitleScreen;

        public RestartCampaignHint(bool fromTitleScreen = false)
        {
            Tag = Tags.HUD;
            FromTitleScreen = fromTitleScreen;
        }

        public override void Render()
        {
            MTexture mTexture = GFX.Gui["checkpoint"];
            string text = Dialog.Clean("XaphanHelper_UI_RestartCampaign_info");
            float width = ActiveFont.Measure(text).X * 0.75f;
            float textureWidth = mTexture.Width * 0.75f;
            Vector2 value2 = new((1920f - width - textureWidth - 64f) / 2f, FromTitleScreen ? 930f : 730f);
            ActiveFont.DrawOutline(text, value2 + new Vector2(width / 2f, 0f), new Vector2(0.5f, 0.5f), Vector2.One * 0.75f, Color.LightGray, 2f, Color.Black);
            value2.X += width + 64f;
            mTexture.DrawCentered(value2 + new Vector2(textureWidth * 0.5f, 0f), Color.White, 0.75f);
        }
    }
}
