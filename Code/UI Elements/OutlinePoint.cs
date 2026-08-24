using System;
using System.Collections.Generic;
using System.Linq;

namespace Celeste.Mod.XaphanHelper.UI_Elements
{
    public class OutlinePoint
    {
        public float x;

        public float y;

        public bool visible;

        public OutlinePoint(float x, float y, bool visible)
        {
            this.x = x;
            this.y = y;
            this.visible = visible;
        }

        private struct Point : IEquatable<Point>
        {
            public int X;

            public int Y;

            public Point(int x, int y)
            {
                X = x;
                Y = y;
            }

            public bool Equals(Point other) => X == other.X && Y == other.Y;
        }

        public static List<OutlinePoint> GenerateSolidOutline(Solid solid)
        {
            List<OutlinePoint> outline = new();
            for (int i = (int)solid.Width / 2; i < solid.Width; i++)
            {
                Math.DivRem(i, 4, out int pos);
                if (pos == 1 || pos == 2)
                {
                    outline.Add(new OutlinePoint(i, 0f, true));
                }
                else
                {
                    outline.Add(new OutlinePoint(i, 0f, false));
                }
            }
            for (int j = 0; j < solid.Height; j++)
            {
                Math.DivRem(j, 4, out int pos);
                if (pos == 1 || pos == 2)
                {
                    outline.Add(new OutlinePoint(solid.Width - 1, j, true));
                }
                else
                {
                    outline.Add(new OutlinePoint(solid.Width - 1, j, false));
                }
            }
            for (int i = (int)solid.Width; i >= 0; i--)
            {
                Math.DivRem(i, 4, out int pos);
                if (pos == 1 || pos == 2)
                {
                    outline.Add(new OutlinePoint(i, solid.Height - 1, true));
                }
                else
                {
                    outline.Add(new OutlinePoint(i, solid.Height - 1, false));
                }
            }
            for (int j = (int)solid.Height; j >= 0; j--)
            {
                Math.DivRem(j, 4, out int pos);
                if (pos == 1 || pos == 2)
                {
                    outline.Add(new OutlinePoint(0, j, true));
                }
                else
                {
                    outline.Add(new OutlinePoint(0, j, false));
                }
            }
            for (int i = 0; i < solid.Width / 2; i++)
            {
                Math.DivRem(i, 4, out int pos);
                if (pos == 1 || pos == 2)
                {
                    outline.Add(new OutlinePoint(i, 0f, true));
                }
                else
                {
                    outline.Add(new OutlinePoint(i, 0f, false));
                }
            }
            outline.Reverse();
            return outline;
        }

        public static List<List<OutlinePoint>> GenerateGroupOutlines(List<Solid> group, Solid origin)
        {
            var empty = new List<List<OutlinePoint>>();
            if (group.Count == 0)
            {
                return empty;
            }
            if (group.Count == 1 && group[0] == origin)
            {
                return new List<List<OutlinePoint>> { GenerateSolidOutline(origin) };
            }
            int minX = (int)group.Min(s => s.Position.X);
            int minY = (int)group.Min(s => s.Position.Y);
            int maxX = (int)group.Max(s => s.Position.X + s.Width);
            int maxY = (int)group.Max(s => s.Position.Y + s.Height);
            int w = maxX - minX;
            int h = maxY - minY;
            if (w <= 0 || h <= 0)
            {
                return empty;
            }
            bool[,] covered = new bool[w, h];
            foreach (Solid s in group)
            {
                int sx = (int)s.Position.X - minX;
                int sy = (int)s.Position.Y - minY;
                for (int x = sx; x < sx + (int)s.Width; x++)
                {
                    for (int y = sy; y < sy + (int)s.Height; y++)
                    {
                        covered[x, y] = true;
                    }
                }
            }
            bool Inside(int x, int y) => x >= 0 && y >= 0 && x < w && y < h && covered[x, y];
            var neighbors = new Dictionary<Point, List<Point>>();
            void AddEdge(Point a, Point b)
            {
                if (!neighbors.TryGetValue(a, out List<Point> la))
                {
                    neighbors[a] = la = new List<Point>();
                }
                la.Add(b);
                if (!neighbors.TryGetValue(b, out List<Point> lb))
                {
                    neighbors[b] = lb = new List<Point>();
                }
                lb.Add(a);
            }
            for (int x = 0; x <= w; x++)
            {
                for (int y = 0; y < h; y++)
                {
                    if (Inside(x - 1, y) != Inside(x, y))
                    {
                        AddEdge(new Point(x, y), new Point(x, y + 1));
                    }
                }
            }
            for (int y = 0; y <= h; y++)
            {
                for (int x = 0; x < w; x++)
                {
                    if (Inside(x, y - 1) != Inside(x, y))
                    {
                        AddEdge(new Point(x, y), new Point(x + 1, y));
                    }
                }
            }
            if (neighbors.Count == 0)
            {
                return empty;
            }
            var visited = new HashSet<Point>();
            var components = new List<List<Point>>();
            foreach (Point key in neighbors.Keys)
            {
                if (visited.Contains(key))
                {
                    continue;
                }
                List<Point> compVerts = new();
                Stack<Point> stack = new();
                stack.Push(key);
                visited.Add(key);
                while (stack.Count > 0)
                {
                    Point v = stack.Pop();
                    compVerts.Add(v);
                    foreach (Point n in neighbors[v])
                    {
                        if (visited.Add(n))
                        {
                            stack.Push(n);
                        }
                    }
                }
                components.Add(compVerts);
            }
            List<List<Point>> loops = new();
            foreach (List<Point> comp in components)
            {
                Point walkStart = comp.OrderBy(p => p.Y).ThenBy(p => p.X).First();
                List<Point> loop = new() { walkStart };
                Point prev = walkStart;
                Point current = neighbors[walkStart][0];
                int maxSteps = comp.Count * 2 + 16;
                int guard = 0;
                while (!current.Equals(walkStart) && guard < maxSteps)
                {
                    loop.Add(current);
                    List<Point> options = neighbors[current];
                    Point next = (options[0].Equals(prev) && options.Count > 1) ? options[1] : options[0];
                    prev = current;
                    current = next;
                    guard++;
                }
                loops.Add(loop);
            }
            int outerIdx = 0;
            long bestArea = -1;
            for (int i = 0; i < loops.Count; i++)
            {
                int lx = loops[i].Min(p => p.X), hx = loops[i].Max(p => p.X);
                int ly = loops[i].Min(p => p.Y), hy = loops[i].Max(p => p.Y);
                long area = (long)(hx - lx) * (hy - ly);
                if (area > bestArea)
                {
                    bestArea = area;
                    outerIdx = i;
                }
            }
            Point ComputeTopCenterStart(List<Point> loop)
            {
                int loopMinX = loop.Min(p => p.X);
                int loopMaxX = loop.Max(p => p.X);
                float loopCenterX = (loopMinX + loopMaxX) / 2f;

                var topYByX = new Dictionary<int, int>();
                foreach (Point p in loop)
                {
                    if (!topYByX.TryGetValue(p.X, out int currentTop) || p.Y < currentTop)
                    {
                        topYByX[p.X] = p.Y;
                    }
                }

                int bestX = topYByX.Keys.OrderBy(x => Math.Abs(x - loopCenterX)).First();
                return new Point(bestX, topYByX[bestX]);
            }
            for (int i = 0; i < loops.Count; i++)
            {
                Point start = ComputeTopCenterStart(loops[i]);
                int idx = loops[i].IndexOf(start);
                if (idx > 0)
                {
                    loops[i] = loops[i].Skip(idx).Concat(loops[i].Take(idx)).ToList();
                }
            }
            float offsetX = minX - origin.Position.X;
            float offsetY = minY - origin.Position.Y;
            var result = new List<List<OutlinePoint>>();
            foreach (List<Point> loop in loops)
            {
                List<OutlinePoint> pts = new();
                int n = loop.Count;
                for (int i = 0; i < n; i++)
                {
                    Point a = loop[i];
                    Point b = loop[(i + 1) % n];
                    Point pixel = BorderPixelForEdge(a, b, covered, w, h);
                    Math.DivRem(i, 4, out int pos);
                    bool visible = pos == 1 || pos == 2;
                    pts.Add(new OutlinePoint(pixel.X + offsetX, pixel.Y + offsetY, visible));
                }
                result.Add(pts);
            }
            if (outerIdx != 0)
            {
                List<OutlinePoint> outer = result[outerIdx];
                result.RemoveAt(outerIdx);
                result.Insert(0, outer);
            }
            return result;
        }

        private static Point BorderPixelForEdge(Point a, Point b, bool[,] covered, int w, int h)
        {
            bool CoveredAt(int x, int y) => x >= 0 && y >= 0 && x < w && y < h && covered[x, y];
            if (a.X == b.X)
            {
                int x = a.X;
                int y = Math.Min(a.Y, b.Y);
                int col = CoveredAt(x - 1, y) ? x - 1 : x;
                return new Point(col, y);
            }
            else
            {
                int y = a.Y;
                int x = Math.Min(a.X, b.X);
                int row = CoveredAt(x, y - 1) ? y - 1 : y;
                return new Point(x, row);
            }
        }
    }
}
