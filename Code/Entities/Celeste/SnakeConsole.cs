using System.Collections.Generic;
using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste.Mod.XaphanHelper.Entities
{
    [CustomEntity("XaphanHelper/SnakeConsole")]
    public class SnakeConsole : Entity
    {
        private class SnakeGame()
        {
            public const int GridSize = 9;

            public enum Status
            {
                Inactive,
                Active,
                Defeat,
                Victory
            }

            public readonly LinkedList<Point> Body = new LinkedList<Point>();

            public Status CurrentStatus = Status.Inactive;

            public Point CurrentFood;

            private Point direction = new Point(1, 0);

            public Point nextDirection = new Point(1, 0);

            public void StartGame()
            {
                Body.Clear();
                direction = new Point(1, 0);

                // Starting position for the snake
                for (int x = 3; x >= 1; x--)
                {
                    AddSegmentAtEnd(new Point(x, GridSize / 2));
                }

                // Add one food
                SpawnFood();

                CurrentStatus = Status.Active;
            }

            public void Tick()
            {
                if (CurrentStatus != Status.Active)
                {
                    return;
                }

                // Redirect the snake based on player input
                // But prevent opposite of current direction
                if (!((nextDirection.Y == 0 && nextDirection.X == -direction.X) || (nextDirection.X == 0 && nextDirection.Y == -direction.Y)))
                {
                    direction = nextDirection;
                }

                // Check if the player collide with itself, a food or a wall
                Point head = Body.First.Value;
                Point next = new Point(head.X + direction.X, head.Y + direction.Y);
                if (next.X < 0 || next.X >= GridSize || next.Y < 0 || next.Y >= GridSize)
                {
                    CurrentStatus = Status.Defeat;
                    return;
                }
                if ((!Body.Contains(next) && next != CurrentFood) || next == Body.Last.Value)
                {
                    Body.RemoveLast();
                }
                if (Body.Contains(next))
                {
                    CurrentStatus = Status.Defeat;
                    return;
                }
                AddSegmentAtStart(next);
                if (next == CurrentFood)
                {
                    SpawnFood();
                }
            }

            public void SpawnFood()
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
                CurrentFood = free[Calc.Random.Next(free.Count)];
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

        private const int CellSize = 4;

        private const float TickTime = 0.75f;

        private TalkComponent talk;

        private float TickTimer;

        public SnakeConsole(EntityData data, Vector2 offset) : base(data.Position + offset)
        {
            Add(talk = new TalkComponent(new Rectangle(-12, 0, 24, 8), new Vector2(0, -12f), Interact));
            talk.PlayerMustBeFacing = false;
        }

        private void Interact(Player player)
        {
            talk.Enabled = false;
            TickTimer = TickTime;
            game.StartGame();
            player.StateMachine.State = Player.StDummy;
        }

        public override void Update()
        {
            base.Update();
            if (game.CurrentStatus == SnakeGame.Status.Inactive)
            {
                return;
            }

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
            if (game.CurrentStatus == SnakeGame.Status.Active)
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

            // Grid
            Vector2 origin = Position - new Vector2(SnakeGame.GridSize * CellSize / 2f, SnakeGame.GridSize * CellSize + 24f);
            float size = SnakeGame.GridSize * CellSize;

            Draw.Rect(origin.X, origin.Y, size, size, Color.Black * 0.85f);
            Draw.HollowRect(origin.X - 1, origin.Y - 1, size + 2, size + 2, Color.White);

            if (game.CurrentStatus != SnakeGame.Status.Inactive)
            {
                // Food
                Draw.Rect(origin.X + game.CurrentFood.X * CellSize + 1, origin.Y + game.CurrentFood.Y * CellSize + 1, CellSize - 2, CellSize - 2, Color.Red);

                // Snake
                bool isHead = true;
                foreach (Point segment in game.Body)
                {
                    Draw.Rect(origin.X + segment.X * CellSize, origin.Y + segment.Y * CellSize, CellSize, CellSize, isHead ? Color.LimeGreen : Color.Green);
                    isHead = false;
                }
            }
        }
    }
}
