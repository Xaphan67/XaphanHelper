using System;
using System.Collections;
using System.Collections.Generic;
using Celeste.Mod.Entities;
using Celeste.Mod.XaphanHelper.Components;
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
            public class ScoreEntry
            {
                public string Name;

                public int Score;

                public string ScoreText;

                public bool IsPlayer;

                public ScoreEntry(string name, int score, bool isPlayer = false)
                {
                    Name = name;
                    Score = score;
                    ScoreText = score.ToString();
                    IsPlayer = isPlayer;
                }
            }

            public List<ScoreEntry> highScores = new List<ScoreEntry>
            {
                new (Dialog.Clean("LAMBERT"), 67),
                new (Dialog.Clean("KANE"), 50),
                new (Dialog.Clean("BRETT"), 42),
                new (Dialog.Clean("KNOLAN"), 28),
                new (Dialog.Clean("HUDSON"), 21),
                new (Dialog.Clean("ORACCA"), 15),
            };

            private Dictionary<int, Color> difficultyColors = new Dictionary<int, Color>
            {
                [0] = Color.DeepSkyBlue,
                [10] = Color.LimeGreen,
                [20] = Color.Yellow,
                [30] = Color.Orange,
                [40] = Color.Red,
                [50] = Color.MediumPurple,
                [60] = Color.Gray
            };

            public const int GridSize = 9;

            public enum Status
            {
                Inactive,
                MainMenu,
                Help,
                Scores,
                Playing,
                Defeat,
                Victory
            }

            public readonly LinkedList<Point> Body = new LinkedList<Point>();

            public Status CurrentStatus = Status.Inactive;

            public Point CurrentStrawberry;

            public Point direction = new Point(1, 0);

            public Point nextDirection = new Point(1, 0);

            public int InternalScore = 0;

            public int InternalLevel = 1;

            public string Score = "0";

            public string Level = "1";

            public string currentBest;

            public Color SnakeColor;

            public void ShowMainMenu()
            {
                CurrentStatus = Status.MainMenu;
                Audio.SetMusic("event:/music/xaphan/snakegame/lvl_0_snakegame_menu", true, false);
            }

            public void StartGame()
            {
                SnakeColor = GetSnakeColor(0);
                Body.Clear();
                direction = new Point(1, 0);
                nextDirection = new Point(1, 0);
                InternalScore = 0;
                InternalLevel = 1;
                Score = "0";
                Level = "1";
                currentBest = highScores.Find(e => e.IsPlayer)?.ScoreText ?? "0";
                Audio.SetMusic("event:/music/xaphan/snakegame/lvl_0_snakegame_main", true, false);

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
                direction = nextDirection;

                // Check if the player collide with itself, a strawberry or a wall
                Point head = Body.First.Value;
                Point next = new Point(head.X + direction.X, head.Y + direction.Y);
                if (next.X < 0 || next.X >= GridSize || next.Y < 0 || next.Y >= GridSize)
                {
                    GameOver();
                    return;
                }
                if ((!Body.Contains(next) && next != CurrentStrawberry) || next == Body.Last.Value)
                {
                    Body.RemoveLast();
                }
                if (Body.Contains(next))
                {
                    GameOver();
                    return;
                }
                AddSegmentAtStart(next);
                if (next == CurrentStrawberry)
                {
                    InternalScore++;
                    SnakeColor = GetSnakeColor(InternalScore);
                    Score = InternalScore.ToString();

                    // Increase Level every 10 strawberries
                    if (InternalScore % 10 == 0)
                    {
                        // 70 strawberries collected = victory
                        if (InternalScore == 70)
                        {
                            Audio.SetMusic(null);
                            CurrentStatus = Status.Victory;
                            return;
                        }
                        Audio.Play("event:/game/xaphan/snakegame/level_up");
                        InternalLevel++;
                        Level = InternalLevel.ToString();
                        SpawnStrawberry();
                    }
                    else
                    {
                        Audio.Play("event:/game/xaphan/snakegame/strawberry");
                        SpawnStrawberry();
                    }
                }
            }

            public void GameOver()
            {
                CurrentStatus = Status.Defeat;
                Audio.SetMusic(null);
                Audio.Play("event:/game/xaphan/snakegame/game_over");
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

            private Color GetSnakeColor(int score)
            {
                int bestKey = -1;
                Color color = Color.White;

                foreach (KeyValuePair<int, Color> pair in difficultyColors)
                {
                    if (pair.Key <= score && pair.Key > bestKey)
                    {
                        bestKey = pair.Key;
                        color = pair.Value;
                    }
                }

                return color;
            }
        }

        private readonly SnakeGame game = new SnakeGame();

        private Player player;

        private const int CellSize = 8;

        private float TickTime;

        public Color BackgroundColor;

        public uint Seed;

        private TalkComponent talk;

        private MTexture[,] tiles;

        private MTexture onLedTexture;

        private MTexture headTexture;

        private MTexture bodyTexture;

        private MTexture cornerTexture;

        private MTexture tailTexture;

        private MTexture strawberryTexture;

        private MTexture logoTexture;

        private string forceInactiveFlag;

        private float TickTimer;

        public float bgAlpha;

        public float noiseAlpha = 0.1f;

        private bool skipTurnOnAnim;

        public bool isOn;

        private PixelFontSize pixelFont;

        public int selection = 0;

        private bool blink;

        private Coroutine Noise = new Coroutine();

        private Coroutine MainMenuSelect = new Coroutine();

        private string previousMusic;

        private string previousAmbiance;

        private float previousParallaxAlpha;

        private Parallax parallax;

        public SnakeConsole(EntityData data, Vector2 offset) : base(data.Position + offset)
        {
            Collider = new Hitbox(144f, 88f);
            forceInactiveFlag = data.Attr("forceInactiveFlag");
            onLedTexture = GFX.Game["objects/XaphanHelper/BigScreen/tvSlicesOn"];
            headTexture = GFX.Game["objects/Xaphan/SnakeGame/head"];
            bodyTexture = GFX.Game["objects/Xaphan/SnakeGame/body"];
            cornerTexture = GFX.Game["objects/Xaphan/SnakeGame/corner"];
            tailTexture = GFX.Game["objects/Xaphan/SnakeGame/tail"];
            strawberryTexture = GFX.Game["objects/Xaphan/SnakeGame/strawberry"];
            logoTexture = GFX.Game["objects/Xaphan/SnakeGame/logo"];
        }

        public override void Added(Scene scene)
        {
            base.Added(scene);
            string Prefix = SceneAs<Level>().Session.Area.LevelSet;
            int chapterIndex = SceneAs<Level>().Session.Area.ChapterIndex;
            if (string.IsNullOrEmpty(forceInactiveFlag) || !SceneAs<Level>().Session.GetFlag(forceInactiveFlag))
            {
                Add(talk = new TalkComponent(new Rectangle(60, 120, 24, 8), new Vector2(72, 96f), Interact));
                talk.PlayerMustBeFacing = false;
            }
            if (XaphanModule.ModSaveData.SnakeHighScore != 0)
            {
                SubmitScore(XaphanModule.ModSaveData.SnakeHighScore);
            }
            PixelFont font = Fonts.Load("PICO-8 Regular");
            pixelFont = font.Get(64f);
            parallax = GetFogParallax();
            previousParallaxAlpha = parallax.Alpha;
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
            Depth = -90000;
            previousMusic = SceneAs<Level>().Session.Audio.Music.Event;
            previousAmbiance = SceneAs<Level>().Session.Audio.Ambience.Event;
            if (Noise != null && Noise.Active)
            {
                Noise.Cancel();
            }
            Add(Noise = new Coroutine(NoiseRoutine()));
            talk.Enabled = false;
            this.player = player;
            player.StateMachine.State = Player.StDummy;
            TickTimer = TickTime;
            Add(new Coroutine(WaitBeforeStart()));
        }

        private IEnumerator StartGame()
        {
            Audio.Play("event:/game/xaphan/snakegame/menu_select");
            float blinktimer = 1f;
            while (blinktimer > 0f)
            {
                if (Scene.OnInterval(0.1f)) {
                    blink = !blink;
                }
                yield return null;
                blinktimer -= Engine.DeltaTime;
            }
            blink = false;
            selection = 0;
            game.StartGame();
        }

        private IEnumerator Exit()
        {
            PlayerCustomAnimation component = player.Components.Get<PlayerCustomAnimation>();
            game.CurrentStatus = SnakeGame.Status.Inactive;
            Audio.SetMusic(previousMusic);
            Audio.SetAmbience(previousAmbiance);
            selection = 0;
            isOn = false;
            while (bgAlpha > 0f)
            {
                bgAlpha -= Engine.RawDeltaTime;
                if (parallax.Alpha < previousParallaxAlpha)
                {
                    parallax.Alpha += Engine.RawDeltaTime;
                }
                else
                {
                    parallax.Alpha = previousParallaxAlpha;
                }
                yield return null;
            }
            bgAlpha = 0f;
            yield return SceneAs<Level>().ZoomBack(1f);
            if (component != null)
            {
                component.PlaySprite("turnBackpack", "hairBackpack", true, true);
            }
            SceneAs<Level>().TimerHidden = false;
            SceneAs<Level>().Session.SetFlag("SnakeConsole_Playing", false);
            talk.Enabled = true;
            Depth = 9010;
            yield return 0.1f;
            player.StateMachine.State = Player.StNormal;
        }

        private IEnumerator WaitBeforeStart()
        {
            PlayerCustomAnimation component = player.Components.Get<PlayerCustomAnimation>();
            SceneAs<Level>().TimerHidden = true;
            yield return player.DummyWalkToExact((int)X + 72, false, 1f, true);
            if (component != null)
            {
                component.SetAnim("XaphanHelper_player_turnAround");
                component.PlaySprite("turnBackpack", "hairBackpack", false);
            }
            SceneAs<Level>().Session.SetFlag("SnakeConsole_Playing");
            yield return SceneAs<Level>().ZoomTo(new Vector2(160f, 92f), 1.3f, 2f);
            while (bgAlpha <= 0.85f)
            {
                bgAlpha += Engine.RawDeltaTime;
                if (parallax.Alpha > 0f)
                {
                    parallax.Alpha -= Engine.RawDeltaTime;
                }
                else
                {
                    parallax.Alpha = 0f;
                }
                yield return null;
            }
            bgAlpha = 0.85f;
            yield return 0.2f;
            Audio.SetMusic(null);
            Audio.SetAmbience(null);
            isOn = true;
            yield return 1.2f;
            game.ShowMainMenu();
        }

        private Parallax GetFogParallax()
        {
            Parallax result = null;
            foreach (Backdrop backdrop in SceneAs<Level>().Foreground.Backdrops)
            {
                if (backdrop is Parallax parallax)
                {
                    if (parallax.Tags.Contains("Fog"))
                    {
                        result = parallax;
                        break;
                    }
                }
            }

            return result;
        }

        private void SubmitScore(int score)
        {
            // Check if player already have a score
            // If yes, check that new score is higher
            SnakeGame.ScoreEntry previous = game.highScores.Find(e => e.IsPlayer);
            if (previous != null)
            {
                if (previous.Score >= score)
                {
                    return;
                }
                game.highScores.Remove(previous);
            }

            // Find position and insert player score
            int index = game.highScores.FindIndex(e => e.Score < score);
            if (index < 0)
            {
                index = game.highScores.Count;
            }
            game.highScores.Insert(index, new SnakeGame.ScoreEntry(SaveData.Instance.Name, score, true));
            XaphanModule.ModSaveData.SnakeHighScore = score;
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

            // Do nothing more if game is not active
            if (game.CurrentStatus == SnakeGame.Status.Inactive)
            {
                return;
            }

            // Main Menu
            if (game.CurrentStatus == SnakeGame.Status.MainMenu && !MainMenuSelect.Active)
            {
                if (Input.MenuUp.Pressed)
                {
                    Audio.Play("event:/game/xaphan/snakegame/menu_change");
                    selection--;
                    if (selection < 0)
                    {
                        selection = 3;
                    }
                }
                else if (Input.MenuDown.Pressed)
                {
                    Audio.Play("event:/game/xaphan/snakegame/menu_change");
                    selection++;
                    if (selection > 3)
                    {
                        selection = 0;
                    }
                }
                else
                {
                    if (Input.MenuConfirm.Pressed)
                    {
                        switch (selection)
                        {
                            case 0:
                                Add(MainMenuSelect = new Coroutine(StartGame()));
                                break;
                            case 1:
                                Input.MenuConfirm.ConsumePress();
                                game.CurrentStatus = SnakeGame.Status.Help;
                                break;
                            case 2:
                                Input.MenuConfirm.ConsumePress();
                                game.CurrentStatus = SnakeGame.Status.Scores;
                                break;
                            case 3:
                                Add(new Coroutine(Exit()));
                                break;
                        }
                    }
                }
            }

            // Help and High Scores screens
            if (game.CurrentStatus == SnakeGame.Status.Help || game.CurrentStatus == SnakeGame.Status.Scores)
            {
                if (Input.MenuConfirm.Pressed)
                {
                    Input.MenuConfirm.ConsumePress();
                    selection = 0;
                    game.CurrentStatus = SnakeGame.Status.MainMenu;
                }
            }

            // Main Game
            if (game.CurrentStatus == SnakeGame.Status.Playing)
            {
                TickTime = 0.85f - game.InternalLevel * 0.1f;

                // Register player direction input
                if (Input.MenuUp.Pressed && game.direction != new Point(0, 1))
                {
                    game.nextDirection = new Point(0, -1);
                }
                else if (Input.MenuDown.Pressed && game.direction != new Point(0, -1))
                {
                    game.nextDirection = new Point(0, 1);
                }
                else if (Input.MenuLeft.Pressed && game.direction != new Point(1, 0))
                {
                    game.nextDirection = new Point(-1, 0);
                }
                else if (Input.MenuRight.Pressed && game.direction != new Point(-1, 0))
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

            // Game Over and Victory screens
            if (game.CurrentStatus == SnakeGame.Status.Defeat || game.CurrentStatus == SnakeGame.Status.Victory)
            {
                if (Input.MenuUp.Pressed)
                {
                    Audio.Play("event:/game/xaphan/snakegame/menu_change");
                    selection--;
                    if (selection < 0)
                    {
                        selection = 1;
                    }
                }
                else if (Input.MenuDown.Pressed)
                {
                    Audio.Play("event:/game/xaphan/snakegame/menu_change");
                    selection++;
                    if (selection > 1)
                    {
                        selection = 0;
                    }
                }
                else
                {
                    if (Input.MenuConfirm.Pressed)
                    {
                        SubmitScore(game.InternalScore);
                        switch (selection)
                        {
                            case 0:
                                Input.MenuConfirm.ConsumePress();
                                game.StartGame();
                                break;
                            case 1:
                                Input.MenuConfirm.ConsumePress();
                                selection = 0;
                                game.ShowMainMenu();
                                break;
                        }
                    }
                }
            }
        }

        public override void Render()
        {
            Draw.Rect(new Rectangle((int)Position.X - 80, (int)Position.Y - 80, 160 + (int)Width, 160 + (int)Height), Color.Black * bgAlpha);
            base.Render();
            uint seed = Seed;

            // Background
            Draw.Rect(Collider, Color.Black);
            Draw.Rect(Collider, BackgroundColor);

            if (game.CurrentStatus == SnakeGame.Status.MainMenu)
            {
                logoTexture.Draw(Position + new Vector2(Width / 2 - logoTexture.Width / 2, 11f));
                pixelFont.Draw(Dialog.Clean("Xaphan_Ch5_Snake_version"), Position + new Vector2(Width / 2, 30f), new Vector2(0.5f, 0f), Vector2.One * 0.1f, Color.White);
                pixelFont.Draw(Dialog.Clean("Xaphan_Ch5_Snake_new_game"), Position + new Vector2(Width / 2, 50f), new Vector2(0.5f, 0f), Vector2.One * 0.1f, selection == 0 ? (blink ? Color.LightYellow : Color.LightGreen) : Color.White);
                pixelFont.Draw(Dialog.Clean("Xaphan_Ch5_Snake_how_to_play"), Position + new Vector2(Width / 2, 57f), new Vector2(0.5f, 0f), Vector2.One * 0.1f, selection == 1 ? Color.LightGreen : Color.White);
                pixelFont.Draw(Dialog.Clean("Xaphan_Ch5_Snake_high_scores"), Position + new Vector2(Width / 2, 64f), new Vector2(0.5f, 0f), Vector2.One * 0.1f, selection == 2 ? Color.LightGreen : Color.White);
                pixelFont.Draw(Dialog.Clean("Xaphan_Ch5_Snake_exit"), Position + new Vector2(Width / 2, 71f), new Vector2(0.5f, 0f), Vector2.One * 0.1f, selection == 3 ? Color.LightGreen : Color.White);
            }

            if (game.CurrentStatus == SnakeGame.Status.Help)
            {
                pixelFont.Draw(Dialog.Clean("Xaphan_Ch5_Snake_how_to_play"), Position + new Vector2(Width / 2, 10f), new Vector2(0.5f, 0f), Vector2.One * 0.1f, Color.Gold);
                pixelFont.Draw(Dialog.Clean("Xaphan_Ch5_Snake_how_to_play_a"), Position + new Vector2(Width / 2, 20f), new Vector2(0.5f, 0f), Vector2.One * 0.1f, Color.White);
                pixelFont.Draw(Dialog.Clean("Xaphan_Ch5_Snake_how_to_play_b"), Position + new Vector2(Width / 2, 26f), new Vector2(0.5f, 0f), Vector2.One * 0.1f, Color.White);
                pixelFont.Draw(Dialog.Clean("Xaphan_Ch5_Snake_how_to_play_c"), Position + new Vector2(Width / 2, 35f), new Vector2(0.5f, 0f), Vector2.One * 0.1f, Color.White);
                pixelFont.Draw(Dialog.Clean("Xaphan_Ch5_Snake_how_to_play_d"), Position + new Vector2(Width / 2, 44f), new Vector2(0.5f, 0f), Vector2.One * 0.1f, Color.White);
                pixelFont.Draw(Dialog.Clean("Xaphan_Ch5_Snake_how_to_play_e"), Position + new Vector2(Width / 2, 50f), new Vector2(0.5f, 0f), Vector2.One * 0.1f, Color.White);
                pixelFont.Draw(Dialog.Clean("Xaphan_Ch5_Snake_how_to_play_f"), Position + new Vector2(Width / 2, 59f), new Vector2(0.5f, 0f), Vector2.One * 0.1f, Color.White);
                pixelFont.Draw(Dialog.Clean("Xaphan_Ch5_Snake_menu"), Position + new Vector2(Width / 2, 71f), new Vector2(0.5f, 0f), Vector2.One * 0.1f, Color.LightGreen);
            }

            if (game.CurrentStatus == SnakeGame.Status.Scores)
            {
                pixelFont.Draw(Dialog.Clean("Xaphan_Ch5_Snake_high_scores"), Position + new Vector2(Width / 2, 10f), new Vector2(0.5f, 0f), Vector2.One * 0.1f, Color.Gold);
                for (int i = 0; i < game.highScores.Count; i++)
                {
                    SnakeGame.ScoreEntry entry = game.highScores[i];
                    float y = 20f + i * 6f;
                    Color color = entry.IsPlayer ? Color.Cyan : Color.White;
                    pixelFont.Draw(entry.Name, Position + new Vector2(Width / 2 - 40, y), Vector2.Zero, Vector2.One * 0.1f, color);
                    pixelFont.Draw(entry.ScoreText, Position + new Vector2(Width / 2 + 40, y), new Vector2(1f, 0f), Vector2.One * 0.1f, color);
                }
                pixelFont.Draw(Dialog.Clean("Xaphan_Ch5_Snake_menu"), Position + new Vector2(Width / 2, 71f), new Vector2(0.5f, 0f), Vector2.One * 0.1f, Color.LightGreen);
            }

            if (game.CurrentStatus == SnakeGame.Status.Playing)
            {
                // Grid
                Vector2 Origin = Position + Vector2.One * 8f;
                Draw.HollowRect(Origin.X - 1, Origin.Y - 1, 74, 74, Color.White);

                // Strawberry
                strawberryTexture.Draw(Origin + new Vector2(game.CurrentStrawberry.X, game.CurrentStrawberry.Y) * CellSize);

                // Snake
                for (LinkedListNode<Point> node = game.Body.First; node != null; node = node.Next)
                {
                    Point current = node.Value;
                    Vector2 center = Origin + new Vector2((current.X + 0.5f) * CellSize, (current.Y + 0.5f) * CellSize);
                    MTexture texture;
                    float rotation;
                    Vector2 scale = Vector2.One;

                    if (node.Previous == null) // Head
                    {
                        texture = headTexture;
                        rotation = Angle(Diff(current, node.Next.Value));
                    }
                    else if (node.Next == null) // Tail
                    {
                        texture = tailTexture;
                        rotation = Angle(Diff(node.Previous.Value, current));
                    }
                    else
                    {
                        Point inDir = Diff(current, node.Next.Value);
                        Point outDir = Diff(node.Previous.Value, current);

                        if (inDir == outDir) // Line body
                        {
                            texture = bodyTexture;
                            rotation = Angle(inDir);
                        }
                        else // Corner body
                        {
                            texture = cornerTexture;
                            rotation = Angle(inDir);
                            if (inDir.X * outDir.Y - inDir.Y * outDir.X < 0)
                            {
                                scale = new Vector2(1f, -1f);
                            }
                        }
                    }

                    texture.DrawCentered(center, game.SnakeColor, scale, rotation);
                }

                // HUD
                pixelFont.Draw(Dialog.Clean("Xaphan_Ch5_Snake_player"), Position + new Vector2(88f, 6f), Vector2.Zero, Vector2.One * 0.1f, Color.Gold);
                pixelFont.Draw(SaveData.Instance.Name, Position + new Vector2(88f, 14f), Vector2.Zero, Vector2.One * 0.1f, Color.White);
                pixelFont.Draw(Dialog.Clean("Xaphan_Ch5_Snake_level"), Position + new Vector2(88f, 26f), Vector2.Zero, Vector2.One * 0.1f, Color.Gold);
                pixelFont.Draw(game.Level, Position + new Vector2(88f, 34f), Vector2.Zero, Vector2.One * 0.1f, Color.White);
                pixelFont.Draw(Dialog.Clean("Xaphan_Ch5_Snake_strawberries"), Position + new Vector2(88f, 46f), Vector2.Zero, Vector2.One * 0.1f, Color.Gold);
                pixelFont.Draw(game.Score, Position + new Vector2(88f, 54f), Vector2.Zero, Vector2.One * 0.1f, Color.White);
                pixelFont.Draw(Dialog.Clean("Xaphan_Ch5_Snake_highscore"), Position + new Vector2(88f, 66f), Vector2.Zero, Vector2.One * 0.1f, Color.Gold);
                pixelFont.Draw(game.currentBest, Position + new Vector2(88f, 74f), Vector2.Zero, Vector2.One * 0.1f, Color.White);
            }

            if (game.CurrentStatus == SnakeGame.Status.Defeat || game.CurrentStatus == SnakeGame.Status.Victory)
            {
                pixelFont.Draw(Dialog.Clean(game.CurrentStatus == SnakeGame.Status.Defeat ? "Xaphan_Ch5_Snake_game_over" : "Xaphan_Ch5_Snake_congratulations"), Position + new Vector2(Width / 2, 10f), new Vector2(0.5f, 0f), Vector2.One * 0.1f, Color.Gold);
                pixelFont.Draw(Dialog.Clean("Xaphan_Ch5_Snake_final_score"), Position + new Vector2(Width / 2, 30f), new Vector2(0.5f, 0f), Vector2.One * 0.1f, Color.White);
                pixelFont.Draw(game.Score, Position + new Vector2(Width / 2, 40f), new Vector2(0.5f, 0f), Vector2.One * 0.2f, Color.White);
                pixelFont.Draw(Dialog.Clean("Xaphan_Ch5_Snake_replay"), Position + new Vector2(Width / 2, 64f), new Vector2(0.5f, 0f), Vector2.One * 0.1f, selection == 0 ? Color.LightGreen : Color.White);
                pixelFont.Draw(Dialog.Clean("Xaphan_Ch5_Snake_menu"), Position + new Vector2(Width / 2, 71f), new Vector2(0.5f, 0f), Vector2.One * 0.1f, selection == 1 ? Color.LightGreen : Color.White);
            }

            // Noise
            if (isOn)
            {
                DrawNoise(Collider.Bounds, ref seed, Color.White * noiseAlpha);
                onLedTexture.Draw(Position + new Vector2(-8f, Height));
            }
        }

        private static Point Diff(Point a, Point b)
        {
            return new Point(a.X - b.X, a.Y - b.Y);
        }

        private static float Angle(Point d)
        {
            return (float)Math.Atan2(d.Y, d.X);
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
