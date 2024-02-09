using System;
using System.Collections;
using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste.Mod.XaphanHelper.Entities
{
    [Tracked(false)]
    [CustomEntity("XaphanHelper/BigScreen")]
    public class BigScreen : Entity
    {
        public const int BGDepth = 9010;

        public Color BackgroundColor;

        public uint Seed;

        private MTexture[,] tiles;

        private MTexture Portrait;

        private TalkComponent talk;

        private bool isOn;

        private bool showPortrait;

        float bgAlpha;

        public BigScreen(EntityData data, Vector2 position) : base(data.Position + position)
        {
            Tag = Tags.TransitionUpdate;
            Collider = new Hitbox(data.Width, data.Height);
            Depth = 9010;
            Add(new CustomBloom(RenderBloom));
        }

        public override void Added(Scene scene)
        {
            base.Added(scene);
            Portrait = GFX.Game["objects/Xaphan/BigScreen/Portrait"];
            Add(talk = new TalkComponent(new Rectangle(28, 80, 24, 16), new Vector2(40f, 64f), Interact));
            talk.PlayerMustBeFacing = false;
        }

        public override void Awake(Scene scene)
        {
            base.Awake(scene);
            MTexture mTexture = GFX.Game["scenery/tvSlices"];
            tiles = new MTexture[mTexture.Width / 8, mTexture.Height / 8];
            for (int i = 0; i < mTexture.Width / 8; i++)
            {
                for (int j = 0; j < mTexture.Height / 8; j++)
                {
                    tiles[i, j] = mTexture.GetSubtexture(new Rectangle(i * 8, j * 8, 8, 8));
                }
            }
            int num = (int)(Width / 8f);
            int num2 = (int)(Height / 8f);
            for (int k = -1; k <= num; k++)
            {
                AutoTile(k, -1);
                AutoTile(k, num2);
            }
            for (int l = 0; l < num2; l++)
            {
                AutoTile(-1, l);
                AutoTile(num, l);
            }
        }

        private void Interact(Player player)
        {
            talk.Enabled = false;
            Add(new Coroutine(Routine(player)));
        }

        public IEnumerator Routine(Player player)
        {
            Level level = SceneAs<Level>();
            Depth = -90000;
            level.PauseLock = true;
            player.StateMachine.State = 11;
            yield return player.DummyWalkToExact((int)X + 40, false, 1f, true);
            yield return level.ZoomTo(new Vector2(160f, 84f), 1.75f, 2f);
            while (bgAlpha <= 0.85f)
            {
                bgAlpha += Engine.RawDeltaTime;
                yield return null;
            }
            bgAlpha = 0.85f;
            yield return 0.2f;
            if (!isOn)
            {
                isOn = true;
                yield return 1.2f;
            }
            showPortrait = true;
            Textbox textBox = new Textbox("Xaphan_Ch5_Lore");
            textBox.RenderOffset.X += 125f;
            textBox.RenderOffset.Y += 700f;
            Engine.Scene.Add(textBox);
            while (textBox.Opened)
            {
                yield return null;
            }
            while (bgAlpha > 0f)
            {
                bgAlpha -= Engine.RawDeltaTime;
                yield return null;
            }
            bgAlpha = 0f;
            yield return level.ZoomBack(1f);
            level.PauseLock = false;
            player.StateMachine.State = 0;
            talk.Enabled = true;
            Depth = 9010;
        }

        private void AutoTile(int x, int y)
        {
            if (Empty(x, y))
            {
                bool flag = !Empty(x - 1, y);
                bool flag2 = !Empty(x + 1, y);
                bool flag3 = !Empty(x, y - 1);
                bool flag4 = !Empty(x, y + 1);
                bool flag5 = !Empty(x - 1, y - 1);
                bool flag6 = !Empty(x + 1, y - 1);
                bool flag7 = !Empty(x - 1, y + 1);
                bool flag8 = !Empty(x + 1, y + 1);
                if (!flag2 && !flag4 && flag8)
                {
                    Tile(x, y, tiles[0, 0]);
                }
                else if (!flag && !flag4 && flag7)
                {
                    Tile(x, y, tiles[2, 0]);
                }
                else if (!flag3 && !flag2 && flag6)
                {
                    Tile(x, y, tiles[0, 2]);
                }
                else if (!flag3 && !flag && flag5)
                {
                    Tile(x, y, tiles[2, 2]);
                }
                else if (flag2 && flag4)
                {
                    Tile(x, y, tiles[3, 0]);
                }
                else if (flag && flag4)
                {
                    Tile(x, y, tiles[4, 0]);
                }
                else if (flag2 && flag3)
                {
                    Tile(x, y, tiles[3, 2]);
                }
                else if (flag && flag3)
                {
                    Tile(x, y, tiles[4, 2]);
                }
                else if (flag4)
                {
                    Tile(x, y, tiles[1, 0]);
                }
                else if (flag2)
                {
                    Tile(x, y, tiles[0, 1]);
                }
                else if (flag)
                {
                    Tile(x, y, tiles[2, 1]);
                }
                else if (flag3)
                {
                    Tile(x, y, tiles[1, 2]);
                }
            }
        }

        private void Tile(int x, int y, MTexture tile)
        {
            Image image = new Image(tile);
            image.Position = new Vector2(x, y) * 8f;
            Add(image);
        }

        private bool Empty(int x, int y)
        {
            return !Scene.CollideCheck<BigScreen>(new Rectangle((int)X + x * 8, (int)Y + y * 8, 8, 8));
        }

        public override void Update()
        {
            base.Update();
            BackgroundColor = isOn ? Color.White * 0.6f : Color.Black;
            if (Scene.OnInterval(0.1f))
            {
                Seed++;
            }
        }

        private void RenderBloom()
        {
            Draw.Rect(Collider, Color.White * 0.4f);
        }

        public override void Render()
        {
            Draw.Rect(new Rectangle((int)Position.X - 80, (int)Position.Y - 80, 160 + (int)Width, 160 + (int)Height), Color.Black * bgAlpha);
            base.Render();
            uint seed = Seed;
            Draw.Rect(Collider, Color.Black);
            Draw.Rect(Collider, BackgroundColor);
            if (Portrait != null && showPortrait)
            {
                Draw.SpriteBatch.Draw(Portrait.Texture.Texture_Safe, Position + new Vector2(20f, 0f), Color.White * 0.75f);
            }
            if (isOn)
            {
                DrawNoise(Collider.Bounds, ref seed, Color.White * 0.1f);
            }
        }

        public static void DrawNoise(Rectangle bounds, ref uint seed, Color color)
        {
            MTexture mTexture = GFX.Game["util/noise"];
            Vector2 vector = new Vector2(PseudoRandRange(ref seed, 0f, mTexture.Width / 2), PseudoRandRange(ref seed, 0f, mTexture.Height / 2));
            Vector2 vector2 = new Vector2(mTexture.Width, mTexture.Height) / 2f;
            for (float num = 0f; num < bounds.Width; num += vector2.X)
            {
                float num2 = Math.Min(bounds.Width - num, vector2.X);
                for (float num3 = 0f; num3 < bounds.Height; num3 += vector2.Y)
                {
                    float num4 = Math.Min(bounds.Height - num3, vector2.Y);
                    int x = (int)(mTexture.ClipRect.X + vector.X);
                    int y = (int)(mTexture.ClipRect.Y + vector.Y);
                    Rectangle value = new Rectangle(x, y, (int)num2, (int)num4);
                    Draw.SpriteBatch.Draw(mTexture.Texture.Texture_Safe, new Vector2(bounds.X + num, bounds.Y + num3), value, color);
                }
            }
        }

        private static uint PseudoRand(ref uint seed)
        {
            seed ^= seed << 13;
            seed ^= seed >> 17;
            return seed;
        }

        private static float PseudoRandRange(ref uint seed, float min, float max)
        {
            return min + (PseudoRand(ref seed) % 1000u) / 1000f * (max - min);
        }
    }
}
