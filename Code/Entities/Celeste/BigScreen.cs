using System;
using System.Collections;
using Celeste.Mod.Entities;
using Celeste.Mod.XaphanHelper.Cutscenes;
using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste.Mod.XaphanHelper.Entities
{
    [Tracked(false)]
    [CustomEntity("XaphanHelper/BigScreen")]
    public class BigScreen : Entity
    {
        public EntityID ID;

        public const int BGDepth = 9010;

        public Color BackgroundColor;

        public uint Seed;

        private MTexture[,] tiles;

        public MTexture Portrait;

        public TalkComponent talk;

        private bool alwaysOn;

        private bool noInteract;

        public bool isOn;

        public bool showPortrait;

        public float bgAlpha;

        public float noiseAlpha = 0.1f;

        private MTexture onLedTexture;

        private string dialogID;

        private string music;

        private string forceInactiveFlag;

        private bool registerInSaveData;

        public string PlayerPose = "";

        private bool skipTurnOnAnim;

        private string PortraitPath;

        public BigScreen(EntityData data, Vector2 position, EntityID eID) : base(data.Position + position)
        {
            Tag = Tags.TransitionUpdate;
            Collider = new Hitbox(data.Width, data.Height);
            Depth = 9010;
            ID = eID;
            onLedTexture = GFX.Game["objects/XaphanHelper/BigScreen/tvSlicesOn"];
            dialogID = data.Attr("dialogID");
            music = data.Attr("music");
            forceInactiveFlag = data.Attr("forceInactiveFlag");
            registerInSaveData = data.Bool("registerInSaveData");
            alwaysOn = data.Bool("alwaysOn");
            noInteract = data.Bool("noInteract");
            PortraitPath = data.Attr("portrait", "objects/Xaphan/BigScreen/Portrait");
            Add(new CustomBloom(RenderBloom));
        }

        public static void Load()
        {
            On.Monocle.Sprite.Play += PlayerSpritePlayHook;
        }

        public static void Unload()
        {
            On.Monocle.Sprite.Play -= PlayerSpritePlayHook;
        }

        private static void PlayerSpritePlayHook(On.Monocle.Sprite.orig_Play orig, Sprite self, string id, bool restart = false, bool randomizeFrame = false)
        {
            if (self.Entity is Player && self.Scene is Level level && !XaphanModule.PlayerIsControllingRemoteDrone())
            {
                foreach (BigScreen screen in level.Tracker.GetEntities<BigScreen>())
                {
                    if (!string.IsNullOrEmpty(screen.PlayerPose) && screen.Active)
                    {
                        id = screen.PlayerPose;
                        break;
                    }
                }
            }
            orig(self, id, restart, randomizeFrame);
        }

        public override void Added(Scene scene)
        {
            base.Added(scene);
            string Prefix = SceneAs<Level>().Session.Area.LevelSet;
            int chapterIndex = SceneAs<Level>().Session.Area.ChapterIndex;
            Portrait = GFX.Game[PortraitPath];
            if (string.IsNullOrEmpty(forceInactiveFlag) || !SceneAs<Level>().Session.GetFlag(forceInactiveFlag))
            {
                if (!noInteract)
                {
                    Add(talk = new TalkComponent(new Rectangle(28, 80, 24, 16), new Vector2(40f, 64f), Interact));
                    talk.PlayerMustBeFacing = false;
                }
                showPortrait = isOn = SceneAs<Level>().Session.GetFlag(Prefix + "_Ch" + chapterIndex + "_" + SceneAs<Level>().Session.Level + "_" + ID.ID + (XaphanModule.PlayerHasGolden ? "_GoldenStrawberry" : ""));
                if (XaphanModule.ModSaveData.SavedFlags.Contains(Prefix + "_Ch" + chapterIndex + "_" + SceneAs<Level>().Session.Level + "_" + ID.ID + (XaphanModule.PlayerHasGolden ? "_GoldenStrawberry" : "")) || alwaysOn)
                {
                    showPortrait = true;
                    isOn = true;
                }
                if (isOn)
                {
                    skipTurnOnAnim = true;
                }
                Add(new Coroutine(NoiseRoutine()));
            }
        }

        public override void Awake(Scene scene)
        {
            base.Awake(scene);
            MTexture mTexture = GFX.Game["objects/XaphanHelper/BigScreen/tvSlices"];
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
            Scene.Add(new BigScreenCutscene(player, dialogID, music, registerInSaveData));
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
            Image image = new(tile);
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
            BackgroundColor = isOn && noiseAlpha < 0.8f ? Calc.HexToColor("#291809") * 0.6f : Color.Black;
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
                Draw.SpriteBatch.Draw(Portrait.Texture.Texture_Safe, Position, Color.White * 0.75f);
            }
            if (isOn)
            {
                DrawNoise(Collider.Bounds, ref seed, Color.White * noiseAlpha);
                onLedTexture.Draw(Position + new Vector2(-8f, Height));
            }
        }

        private IEnumerator NoiseRoutine()
        {
            bool justTurnedOn = false;
            bool vasJustTurnedOn = false;
            while (true)
            {
                if (isOn)
                {
                    if (!justTurnedOn)
                    {
                        justTurnedOn = true;
                    }
                    if (justTurnedOn && !vasJustTurnedOn && !skipTurnOnAnim)
                    {
                        noiseAlpha = 0.8f;
                        yield return 1.2f;
                        noiseAlpha = 0.05f;
                        vasJustTurnedOn = true;
                    }
                    else
                    {
                        yield return Calc.Random.Next(1, 5);
                        while (noiseAlpha <= 0.2f)
                        {
                            noiseAlpha += Engine.DeltaTime;
                            yield return null;
                        }
                        noiseAlpha = 0.2f;
                        while (noiseAlpha > 0.05f)
                        {
                            noiseAlpha -= Engine.DeltaTime;
                            yield return null;
                        }
                        noiseAlpha = 0.05f;
                    }
                }
                yield return null;
            }
        }

        public static void DrawNoise(Rectangle bounds, ref uint seed, Color color)
        {
            MTexture mTexture = GFX.Game["objects/XaphanHelper/BigScreen/noise"];
            Vector2 vector = new(PseudoRandRange(ref seed, 0f, mTexture.Width / 2), PseudoRandRange(ref seed, 0f, mTexture.Height / 2));
            Vector2 vector2 = new Vector2(mTexture.Width, mTexture.Height) / 2f;
            for (float num = 0f; num < bounds.Width; num += vector2.X)
            {
                float num2 = Math.Min(bounds.Width - num, vector2.X);
                for (float num3 = 0f; num3 < bounds.Height; num3 += vector2.Y)
                {
                    float num4 = Math.Min(bounds.Height - num3, vector2.Y);
                    int x = (int)(mTexture.ClipRect.X + vector.X);
                    int y = (int)(mTexture.ClipRect.Y + vector.Y);
                    Rectangle value = new(x, y, (int)num2, (int)num4);
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
