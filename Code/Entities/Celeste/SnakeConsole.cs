using System;
using System.Collections;
using System.Collections.Generic;
using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste.Mod.XaphanHelper.Entities
{
    [Tracked(true)]
    [CustomEntity("XaphanHelper/SnakeConsole")]
    public class SnakeConsole : Entity
    {
        private class SnakeGame()
        {
            public const int GridSize = 9;

            public enum Status
            {
                MainMenu,
                Playing,
                Defeat,
                Victory
            }

            public readonly LinkedList<Point> Body = new LinkedList<Point>();

            public Status CurrentStatus = Status.MainMenu;

            public Point CurrentStrawberry;

            private Point direction = new Point(1, 0);

            public Point nextDirection = new Point(1, 0);

            private int InternalScore = 0;

            public int InternalLevel = 1;

            public string Score = "0";

            public string Level = "1";

            public void StartGame()
            {
                Body.Clear();
                direction = new Point(1, 0);
                InternalScore = 0;
                InternalLevel = 1;
                Score = "0";
                Level = "1";

                // Starting position for the snake
                for (int x = 3; x >= 1; x--)
                {
                    AddSegmentAtEnd(new Point(x, GridSize / 2));
                }

                // Add one Strawberry
                SpawnStrawberry();

                CurrentStatus = Status.Playing;
            }

            public void Tick()
            {
                if (CurrentStatus != Status.Playing)
                {
                    return;
                }

                // Redirect the snake based on player input
                // But prevent opposite of current direction
                if (!((nextDirection.Y == 0 && nextDirection.X == -direction.X) || (nextDirection.X == 0 && nextDirection.Y == -direction.Y)))
                {
                    direction = nextDirection;
                }

                // Check if the player collide with itself, a strawberry or a wall
                Point head = Body.First.Value;
                Point next = new Point(head.X + direction.X, head.Y + direction.Y);
                if (next.X < 0 || next.X >= GridSize || next.Y < 0 || next.Y >= GridSize)
                {
                    CurrentStatus = Status.Defeat;
                    return;
                }
                if ((!Body.Contains(next) && next != CurrentStrawberry) || next == Body.Last.Value)
                {
                    Body.RemoveLast();
                }
                if (Body.Contains(next))
                {
                    CurrentStatus = Status.Defeat;
                    return;
                }
                AddSegmentAtStart(next);
                if (next == CurrentStrawberry)
                {
                    InternalScore++;
                    Score = InternalScore.ToString();

                    // Increase Level every 10 strawberries
                    if (InternalScore % 10 == 0)
                    {
                        InternalLevel++;
                        Level = InternalLevel.ToString();
                    }
                    SpawnStrawberry();
                }
            }

            public void SpawnStrawberry()
            {
                // Check free spots and select one randomly
                // If no free spôts left, player is victorious
                List<Point> free = new List<Point>();
                for (int x = 0; x < GridSize; x++)
                {
                    for (int y = 0; y < GridSize; y++)
                    {
                        if (!Body.Contains(new Point(x, y))) {
                            free.Add(new Point(x, y));
                        }
                    }
                }
                if (free.Count == 0)
                {
                    CurrentStatus = Status.Victory;
                    return;
                }
                CurrentStrawberry = free[Calc.Random.Next(free.Count)];
            }

            private void AddSegmentAtStart(Point p)
            {
                Body.AddFirst(p);
            }

            private void AddSegmentAtEnd(Point p)
            {
                Body.AddLast(p);
            }
        }

        private readonly SnakeGame game = new SnakeGame();

        private const int CellSize = 8;

        private float TickTime;

        public Color BackgroundColor;

        public uint Seed;

        private TalkComponent talk;

        private MTexture[,] tiles;

        private MTexture onLedTexture;

        private MTexture strawberryTexture;

        private string forceInactiveFlag;

        private float TickTimer;

        public float noiseAlpha = 0.1f;

        private bool skipTurnOnAnim;

        public bool isOn;

        private PixelFontSize pixelFont;

        public SnakeConsole(EntityData data, Vector2 offset) : base(data.Position + offset)
        {
            Collider = new Hitbox(144f, 88f);
            forceInactiveFlag = data.Attr("forceInactiveFlag");
            onLedTexture = GFX.Game["objects/XaphanHelper/BigScreen/tvSlicesOn"];
            strawberryTexture = GFX.Game["objects/Xaphan/SnakeGame/strawberry"];
        }

        public override void Added(Scene scene)
        {
            base.Added(scene);
            string Prefix = SceneAs<Level>().Session.Area.LevelSet;
            int chapterIndex = SceneAs<Level>().Session.Area.ChapterIndex;
            if (string.IsNullOrEmpty(forceInactiveFlag) || !SceneAs<Level>().Session.GetFlag(forceInactiveFlag))
            {
                Add(talk = new TalkComponent(new Rectangle(60, 104, 24, 8), new Vector2(72, 96f), Interact));
                talk.PlayerMustBeFacing = false;
                if (XaphanModule.ModSaveData.SavedFlags.Contains("Xaphan/0_Ch5_SnakeConsole"))
                {
                    isOn = true;
                }
                if (isOn)
                {
                    skipTurnOnAnim = true;
                }
                Add(new Coroutine(NoiseRoutine()));
            }
            PixelFont font = Fonts.Load("PICO-8 Regular");
            pixelFont = font.Get(64f);
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
            return !Scene.CollideCheck<SnakeConsole>(new Rectangle((int)X + x * 8, (int)Y + y * 8, 8, 8));
        }

        private void Interact(Player player)
        {
            talk.Enabled = false;
            player.StateMachine.State = Player.StDummy;
            TickTimer = TickTime;
            isOn = true;
            Add(new Coroutine(WaitBeforeStart()));
        }

        private IEnumerator WaitBeforeStart()
        {
            yield return 1.2f;
            game.StartGame();
        }

        public override void Update()
        {
            base.Update();

            // Noise
            BackgroundColor = isOn && noiseAlpha < 0.8f ? Calc.HexToColor("#291809") * 0.6f : Color.Black;
            if (Scene.OnInterval(0.1f))
            {
                Seed++;
            }

            if (game.CurrentStatus == SnakeGame.Status.MainMenu)
            {
                return;
            }

            TickTime = 0.85f - game.InternalLevel * 0.1f;

            // Register player direction input
            if (Input.MenuUp.Pressed)
            {
                game.nextDirection = new Point(0, -1);
            }
            else if (Input.MenuDown.Pressed)
            {
                game.nextDirection = new Point(0, 1);
            }
            else if (Input.MenuLeft.Pressed)
            {
                game.nextDirection = new Point(-1, 0);
            }
            else if (Input.MenuRight.Pressed)
            {
                game.nextDirection = new Point(1, 0);
            }

            // Progress the game each 'tick'
            if (game.CurrentStatus == SnakeGame.Status.Playing)
            {
                TickTimer -= Engine.DeltaTime;
                if (TickTimer <= 0)
                {
                    game.Tick();
                    TickTimer = TickTime;
                }
            }
        }

        public override void Render()
        {
            base.Render();
            uint seed = Seed;

            // Background
            Draw.Rect(Collider, Color.Black);
            Draw.Rect(Collider, BackgroundColor);

            if (game.CurrentStatus == SnakeGame.Status.Playing)
            {
                // Grid
                Vector2 Origin = Position + Vector2.One * 8f;
                Draw.HollowRect(Origin.X - 1, Origin.Y - 1, 74, 74, Color.White);

                // Strawberry
                strawberryTexture.Draw(Origin + new Vector2(game.CurrentStrawberry.X, game.CurrentStrawberry.Y) * CellSize);

                // Snake
                bool isHead = true;
                foreach (Point segment in game.Body)
                {
                    Draw.Rect(Origin.X + segment.X * CellSize, Origin.Y + segment.Y * CellSize, CellSize, CellSize, isHead ? Color.LimeGreen : Color.Green);
                    isHead = false;
                }

                // HUD
                pixelFont.Draw(Dialog.Clean("Xaphan_Ch5_Snake_player"), Position + new Vector2(88f, 6f), Vector2.Zero, Vector2.One * 0.1f, Color.Gold);
                pixelFont.Draw(SaveData.Instance.Name, Position + new Vector2(88f, 14f), Vector2.Zero, Vector2.One * 0.1f, Color.White);
                pixelFont.Draw(Dialog.Clean("Xaphan_Ch5_Snake_level"), Position + new Vector2(88f, 26f), Vector2.Zero, Vector2.One * 0.1f, Color.Gold);
                pixelFont.Draw(game.Level, Position + new Vector2(88f, 34f), Vector2.Zero, Vector2.One * 0.1f, Color.White);
                pixelFont.Draw(Dialog.Clean("Xaphan_Ch5_Snake_score"), Position + new Vector2(88f, 46f), Vector2.Zero, Vector2.One * 0.1f, Color.Gold);
                pixelFont.Draw(game.Score, Position + new Vector2(88f, 54f), Vector2.Zero, Vector2.One * 0.1f, Color.White);
                pixelFont.Draw(Dialog.Clean("Xaphan_Ch5_Snake_hiscore"), Position + new Vector2(88f, 66f), Vector2.Zero, Vector2.One * 0.1f, Color.Gold);
                pixelFont.Draw('0', Position + new Vector2(88f, 74f), Vector2.Zero, Vector2.One * 0.1f, Color.White);
            }

            // Noise
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

        public override void Removed(Scene scene)
        {
            base.Removed(scene);
            Fonts.Unload("PICO-8 Regular");
        }
    }
}
