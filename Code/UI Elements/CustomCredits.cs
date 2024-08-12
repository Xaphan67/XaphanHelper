using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste.Mod.XaphanHelper.UI_Elements
{
    public class CustomCredits
    {
        private abstract class CreditNode
        {
            public abstract void Render(Vector2 position, float alignment = 0.5f, float scale = 1f);

            public abstract float Height(float scale = 1f);
        }

        private class Thanks : CreditNode
        {
            public const float TitleScale = 1.4f;

            public const float CreditsScale = 1.15f;

            public float Spacing = 8f;

            public readonly Color TitleColor = Color.Gold;

            public readonly Color CreditsColor = Color.White;

            public int TopPadding;

            public string Title;

            public string[] Credits;

            public Thanks(string title, params string[] to)
                : this(0, title, to)
            {
            }

            public Thanks(int topPadding, string title, params string[] to)
            {
                TopPadding = topPadding;
                Title = title;
                Credits = to;
            }

            public Thanks(int topPadding, float spacing, string title, params string[] to)
            {
                TopPadding = topPadding;
                Spacing = spacing;
                Title = title;
                Credits = to;
            }

            public override void Render(Vector2 position, float alignment = 0.5f, float scale = 1f)
            {
                position.Y += TopPadding * scale;
                ActiveFont.DrawOutline(Title, position.Floor() - new Vector2(860 + (ActiveFont.Measure(Title).X * 1.4f) / 2, 0), new Vector2(0f, 0f), Vector2.One * 1.4f * scale, TitleColor, 2f, BorderColor);
                position.Y += (LineHeight * 1.4f + 8f + Spacing) * scale;
                for (int i = 0; i < Credits.Length; i++)
                {
                    PixelFont font = Font;
                    ActiveFont.DrawOutline(Credits[i], position.Floor() - new Vector2(860 + (ActiveFont.Measure(Credits[i]).X * 1.15f) / 2, 0), new Vector2(0f, 0f), Vector2.One * 1.15f * scale, CreditsColor, 2f, BorderColor);
                    position.Y += (LineHeight * 1.15f + Spacing) * scale;
                }
            }

            public override float Height(float scale = 1f)
            {
                return (LineHeight * (1.4f + Credits.Length * 1.15f) + Credits.Length * Spacing + ((Credits.Length != 0) ? 8f + Spacing : 0f) + TopPadding) * scale;
            }
        }

        private class MultiCredit : CreditNode
        {
            public class Section
            {
                public string Subtitle;

                public int SubtitleLines;

                public string[] Credits;

                public Section(string subtitle, params string[] credits)
                {
                    Subtitle = subtitle.ToUpper();
                    SubtitleLines = subtitle.Split('\n').Length;
                    Credits = credits;
                }
            }

            public const float TitleScale = 1.4f;

            public const float SubtitleScale = 0.7f;

            public const float CreditsScale = 1.15f;

            public float Spacing = 8f;

            public float SectionSpacing = 32f;

            public readonly Color TitleColor = Color.Gold;

            public readonly Color SubtitleColor = Calc.HexToColor("a8a694");

            public readonly Color CreditsColor = Color.White;

            public int TopPadding;

            public string Title;

            public Section[] Sections;

            public MultiCredit(int topPadding, float spacing, float sectionSpacing, string title, params Section[] to)
            {
                TopPadding = topPadding;
                Spacing = spacing;
                SectionSpacing = sectionSpacing;
                Title = title;
                Sections = to;
            }

            public override void Render(Vector2 position, float alignment = 0.5f, float scale = 1f)
            {
                position.Y += TopPadding * scale;
                ActiveFont.DrawOutline(Title, position.Floor() - new Vector2(860 + (ActiveFont.Measure(Title).X * 1.4f) / 2, 0), new Vector2(0f, 0f), Vector2.One * 1.4f * scale, TitleColor, 2f, BorderColor);
                position.Y += (LineHeight * 1.4f + 8f + Spacing) * scale;
                for (int i = 0; i < Sections.Length; i++)
                {
                    Section section = Sections[i];
                    string subtitle = section.Subtitle;
                    ActiveFont.DrawOutline(section.Subtitle, position.Floor() - new Vector2(860 + (ActiveFont.Measure(section.Subtitle).X * 0.7f) / 2, 0), new Vector2(0f, 0f), Vector2.One * 0.7f * scale, SubtitleColor, 2f, BorderColor);
                    position.Y += (section.SubtitleLines * LineHeight * 0.7f + Spacing) * scale;
                    for (int j = 0; j < section.Credits.Length; j++)
                    {
                        ActiveFont.DrawOutline(section.Credits[j], position.Floor() - new Vector2(860 + (ActiveFont.Measure(section.Credits[j]).X * 1.15f) / 2, 0), new Vector2(0f, 0f), Vector2.One * 1.15f * scale, CreditsColor, 2f, BorderColor);
                        position.Y += (LineHeight * 1.15f + Spacing) * scale;
                    }
                    position.Y += (32f + SectionSpacing) * scale;
                }
            }

            public override float Height(float scale = 1f)
            {
                float num = 0f;
                num += TopPadding;
                num += LineHeight * 1.4f + 8f + Spacing;
                for (int i = 0; i < Sections.Length; i++)
                {
                    num += Sections[i].SubtitleLines * LineHeight * 0.7f + Spacing;
                    num += LineHeight * 1.15f * Sections[i].Credits.Length + Spacing * Sections[i].Credits.Length;
                }
                num += (32f + SectionSpacing) * (Sections.Length - 1);
                return num * scale;
            }
        }

        private class Ending : CreditNode
        {
            public string Text;

            public bool Spacing;

            public Ending(string text, bool spacing)
            {
                Text = text;
                Spacing = spacing;
            }

            public override void Render(Vector2 position, float alignment = 0.5f, float scale = 1f)
            {
                if (Spacing)
                {
                    position.Y += 540f;
                }
                else
                {
                    position.Y += ActiveFont.LineHeight * 1.5f * scale * 0.5f;
                }
                ActiveFont.DrawOutline(Text, new Vector2(960f, position.Y), new Vector2(0.5f, 0.5f), Vector2.One * 1.5f * scale, Color.White, 2f, BorderColor);
            }

            public override float Height(float scale = 1f)
            {
                if (Spacing)
                {
                    return 540f;
                }
                return ActiveFont.LineHeight * 1.5f * scale;
            }
        }

        private class Image : CreditNode
        {
            public Atlas Atlas;

            public string ImagePath;

            public float BottomPadding;

            public float Rotation;

            public bool ScreenCenter;

            public Image(string path, float bottomPadding = 0f)
                : this(GFX.Gui, path, bottomPadding)
            {
            }

            public Image(Atlas atlas, string path, float bottomPadding = 0f, float rotation = 0f, bool screenCenter = false)
            {
                Atlas = atlas;
                ImagePath = path;
                BottomPadding = bottomPadding;
                Rotation = rotation;
                ScreenCenter = screenCenter;
            }

            public override void Render(Vector2 position, float alignment = 0.5f, float scale = 1f)
            {
                MTexture mTexture = Atlas[ImagePath];
                Vector2 position2 = position + new Vector2(mTexture.Width * (0.5f - alignment), mTexture.Height * 0.5f) * scale;
                if (ScreenCenter)
                {
                    position2.X = 960f;
                }
                mTexture.DrawCentered(position2, Color.White, scale, Rotation);
            }

            public override float Height(float scale = 1f)
            {
                return (Atlas[ImagePath].Height + BottomPadding) * scale;
            }
        }

        private class ImageRow : CreditNode
        {
            private Image[] images;

            public ImageRow(params Image[] images)
            {
                this.images = images;
            }

            public override void Render(Vector2 position, float alignment = 0.5f, float scale = 1f)
            {
                float num = Height(scale);
                float num2 = 0f;
                Image[] array = images;
                foreach (Image image in array)
                {
                    num2 += (image.Atlas[image.ImagePath].Width + 32) * scale;
                }
                num2 -= 32f * scale;
                Vector2 value = position - new Vector2(alignment * num2, 0f);
                Image[] array2 = images;
                foreach (Image image2 in array2)
                {
                    image2.Render(value + new Vector2(0f, (num - image2.Height(scale)) / 2f), 0f, scale);
                    value.X += (image2.Atlas[image2.ImagePath].Width + 32) * scale;
                }
            }

            public override float Height(float scale = 1f)
            {
                float num = 0f;
                Image[] array = images;
                foreach (Image image in array)
                {
                    if (image.Height(scale) > num)
                    {
                        num = image.Height(scale);
                    }
                }
                return num;
            }
        }

        private class Break : CreditNode
        {
            public float Size;

            public Break(float size = 64f)
            {
                Size = size;
            }

            public override void Render(Vector2 position, float alignment = 0.5f, float scale = 1f)
            {
            }

            public override float Height(float scale = 1f)
            {
                return Size * scale;
            }
        }

        public static Color BorderColor = Color.Black;

        public const float CreditSpacing = 64f;

        public const float AutoScrollSpeed = 50f;

        public const float InputScrollSpeed = 600f;

        public const float ScrollResumeDelay = 1f;

        public const float ScrollAcceleration = 1800f;

        private List<CreditNode> credits;

        public float AutoScrollSpeedMultiplier = 0.55f;

        private float scrollSpeed = 50f;

        private float scroll = 0f;

        private float height = 0f;

        private float scrollDelay = 0f;

        private float scrollbarAlpha = 0f;

        private float alignment;

        private float scale;

        public float BottomTimer = 0f;

        public bool Enabled = true;

        public bool AllowInput = true;

        public static PixelFont Font;

        public static float FontSize;

        public static float LineHeight;

        private static List<CreditNode> CreateCredits(bool title, bool polaroids, bool FromTitleScreen)
        {
            List<CreditNode> list = new();
            if (title)
            {
                string logoPath;
                if (Settings.Instance.Language == "english" || Settings.Instance.Language == "french")
                {
                    logoPath = "vignette/Xaphan/logo-" + Settings.Instance.Language;
                }
                else
                {
                    logoPath = "vignette/Xaphan/logo-english";
                }
                list.Add(new Image(GFX.Gui, logoPath, 320f, 0, true));
            }
            // org 97 for padding and 0 for spacings
            float spacing = 15f;
            float sectionSpacing = 15f;
            int padding = 123;
            list.Add(new Thanks(0, spacing, "Created by", "Xaphan"));
            list.Add(new Thanks(padding, spacing, "Inspired by", "Another Metroid 2 Remake fangame (AM2R)", "Metroid", "Super Metroid"));
            list.Add(new Thanks(padding, spacing, "Custom Tilesets and Graphics", "AM2R", "Little Water Studio", "Meowsmith", "pyxelbit", "Super Metroid"));
            list.Add(new MultiCredit(padding, spacing, sectionSpacing, "Custom Musics and effects", new MultiCredit.Section("AM2R tracks", "\"Ancient Power\"", "\"Flooded Complex\"", "\"Power Plant\"", "\"The Tower\"", "\"Transport Room\"", "by Milton \"DoctorM64\" Guasti, Darren Kerwin", "and Torbjørn \"Falcool\" Brandrud"), new MultiCredit.Section("Harmony of a Hunter tracks", "\"Danger in Old Tourian\" by DoctorM64", "\"In the Begining\" by Mercury Adept"), new MultiCredit.Section("Others tracks", "\"Arrival on Crateria (Remastered/Cover)\" by Maned Wolf", "\"Crateria - First Landing\" by Aaron Talbert", "\"Ending and Credits\" by DJ @tomnium", "\"Super Metroid Title Screen Theme\" by nikori", "\"Torizo Battle\" by DJ @tomnium"), new MultiCredit.Section("B-Sides tracks", "\"New Strong Resistance\" by Caluctor")));
            list.Add(new Thanks(padding, spacing, "Playtesters", "Alex Tholen", "iamdadbod", "lennygold"));
            list.Add(new Thanks(padding, spacing, "English dialogs revision", "Accelyte", "JorgeDelLanis"));
            list.Add(new Thanks(padding, spacing, "Korean translation", "Prime "));
            list.Add(new Thanks(padding, spacing, "Special Thanks", "0x0ade", "Cruor", "The Devs of Celeste", "The FMod Celeste Project", "The Mt. Celeste Climbing Association Discord", "Vexatos"));
            list.Add(new Thanks(padding, spacing, Dialog.Clean("Xaphan_0_Credits_End_Message")));
            list.Add(new Thanks(padding - 135, spacing, Dialog.Clean("Xaphan_0_Credits_End_Message_b")));
            list.Add(new Break(FromTitleScreen ? 475f : 540f));
            if (!FromTitleScreen)
            {
                list.Add(new Ending(Dialog.Clean("CREDITS_THANKYOU"), !polaroids));
            }
            return list;
        }

        public CustomCredits(float alignment = 0.5f, float scale = 1f, bool haveTitle = true, bool havePolaroids = false, bool FromTitleScreen = false)
        {
            this.alignment = alignment;
            this.scale = scale;
            credits = CreateCredits(haveTitle, havePolaroids, FromTitleScreen);
            Font = Dialog.Languages["english"].Font;
            FontSize = Dialog.Languages["english"].FontFaceSize;
            LineHeight = Font.Get(FontSize).LineHeight;
            height = 0f;
            foreach (CreditNode credit in credits)
            {
                height += credit.Height(scale) + 64f * scale;
            }
            height += 476f;
            if (havePolaroids)
            {
                height -= 280f;
            }
        }

        public void Update()
        {
            if (Enabled)
            {
                scroll += scrollSpeed * Engine.DeltaTime * scale;
                if (scrollDelay <= 0f)
                {
                    scrollSpeed = Calc.Approach(scrollSpeed, 100f * AutoScrollSpeedMultiplier, 1800f * Engine.DeltaTime);
                }
                else
                {
                    scrollDelay -= Engine.DeltaTime;
                }
                if (AllowInput)
                {
                    if (Input.MenuDown.Check)
                    {
                        scrollDelay = 1f;
                        scrollSpeed = Calc.Approach(scrollSpeed, 600f, 1800f * Engine.DeltaTime);
                    }
                    else if (Input.MenuUp.Check)
                    {
                        scrollDelay = 1f;
                        scrollSpeed = Calc.Approach(scrollSpeed, -600f, 1800f * Engine.DeltaTime);
                    }
                    else if (scrollDelay > 0f)
                    {
                        scrollSpeed = Calc.Approach(scrollSpeed, 0f, 1800f * Engine.DeltaTime);
                    }
                }
                if (scroll < 0f || scroll > height)
                {
                    scrollSpeed = 0f;
                }
                scroll = Calc.Clamp(scroll, 0f, height);
                if (scroll >= height)
                {
                    BottomTimer += Engine.DeltaTime;
                }
                else
                {
                    BottomTimer = 0f;
                }
            }
            scrollbarAlpha = Calc.Approach(scrollbarAlpha, (Enabled && scrollDelay > 0f) ? 1f : 0f, Engine.DeltaTime * 2f);
        }

        public void Render(Vector2 position)
        {
            Vector2 position2 = position + new Vector2(0f, 1080f - scroll).Floor();
            foreach (CreditNode credit in credits)
            {
                float num = credit.Height(scale);
                if (position2.Y > 0f - num && position2.Y < 1080f)
                {
                    credit.Render(position2, alignment, scale);
                }
                position2.Y += num + 64f * scale;
            }
            if (scrollbarAlpha > 0f)
            {
                int num2 = 64;
                int num3 = 1080 - num2 * 2;
                float num4 = num3 * (num3 / height);
                float num5 = scroll / height * (num3 - num4);
                Draw.Rect(1844f, num2, 12f, num3, Color.White * 0.2f * scrollbarAlpha);
                Draw.Rect(1844f, num2 + num5, 12f, num4, Color.White * 0.5f * scrollbarAlpha);
            }
        }
    }
}
