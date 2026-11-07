using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste.Mod.XaphanHelper.UI_Elements
{
    public abstract class JournalPage
    {
        public class Table
        {
            private const float headerHeight = 80f;

            private const float headerBottomMargin = 20f;

            private const float rowHeight = 60f;

            private List<Row> rows = new();

            public int Rows => rows.Count;

            public Row Header
            {
                get
                {
                    if (rows.Count <= 0)
                    {
                        return null;
                    }
                    return rows[0];
                }
            }

            public Table AddColumn(Cell label)
            {
                if (rows.Count == 0)
                {
                    AddRow();
                }
                rows[0].Add(label);
                return this;
            }

            public Row AddRow()
            {
                Row row = new();
                rows.Add(row);
                return row;
            }

            public float Height()
            {
                return 100f + 60f * (rows.Count - 1);
            }

            public void Render(Vector2 position)
            {
                if (Header == null)
                {
                    return;
                }
                float num = 0f;
                float num2 = 0f;
                for (int i = 0; i < Header.Count; i++)
                {
                    num2 += Header[i].Width() + 20f;
                }
                for (int j = 0; j < Header.Count; j++)
                {
                    float num3 = Header[j].Width();
                    Header[j].Render(position + new Vector2(num + num3 * 0.5f, 40f), num3);
                    int num4 = 1;
                    float num5 = 130f;
                    for (int k = 1; k < rows.Count; k++)
                    {
                        Vector2 center = position + new Vector2(num + num3 * 0.5f, num5);
                        if (rows[k].Count > 0)
                        {
                            if (num4 % 2 == 0)
                            {
                                Draw.Rect(center.X - num3 * 0.5f, center.Y - 27f, num3 + 20f, 54f, Color.Black * 0.08f);
                            }
                            if (j < rows[k].Count && rows[k][j] != null)
                            {
                                Cell cell = rows[k][j];
                                if (cell.SpreadOverColumns > 1)
                                {
                                    for (int l = j + 1; l < j + cell.SpreadOverColumns; l++)
                                    {
                                        center.X += Header[l].Width() * 0.5f;
                                    }
                                    center.X += (cell.SpreadOverColumns - 1) * 20f * 0.5f;
                                }
                                rows[k][j].Render(center, num3);
                            }
                            num4++;
                            num5 += 60f;
                        }
                        else
                        {
                            Draw.Rect(center.X - num3 * 0.5f, center.Y - 25.5f, num3 + 20f, 6f, Color.Black * 0.3f);
                            num5 += 15f;
                        }
                    }
                    num += num3 + 20f;
                }
            }
        }

        public class Row
        {
            public List<Cell> Entries = new();

            public int Count => Entries.Count;

            public Cell this[int index] => Entries[index];

            public Row Add(Cell entry)
            {
                Entries.Add(entry);
                return this;
            }
        }

        public abstract class Cell
        {
            public int SpreadOverColumns = 1;

            public virtual float Width()
            {
                return 0f;
            }

            public virtual void Render(Vector2 center, float columnWidth)
            {
            }
        }

        public class EmptyCell : Cell
        {
            private float width;

            public EmptyCell(float width)
            {
                this.width = width;
            }

            public override float Width()
            {
                return width;
            }
        }

        public class TextCell : Cell
        {
            private string text;

            private Vector2 justify;

            private float scale;

            private Color color;

            private float width;

            private bool forceWidth;

            public TextCell(string text, Vector2 justify, float scale, Color color, float width = 0f, bool forceWidth = false)
            {
                this.text = text;
                this.justify = justify;
                this.scale = scale;
                this.color = color;
                this.width = width;
                this.forceWidth = forceWidth;
            }

            public override float Width()
            {
                if (forceWidth)
                {
                    return width;
                }
                return Math.Max(width, ActiveFont.Measure(text).X * scale);
            }

            public override void Render(Vector2 center, float columnWidth)
            {
                float num = ActiveFont.Measure(text).X * scale;
                float num2 = 1f;
                if (!forceWidth && num > columnWidth)
                {
                    num2 = columnWidth / num;
                }
                ActiveFont.Draw(text, center + new Vector2((0f - columnWidth) / 2f + columnWidth * justify.X, 0f), justify, Vector2.One * scale * num2, color);
            }
        }

        public class IconCell : Cell
        {
            private string icon;

            private float width;

            private Image image;

            public IconCell(string icon, float width = 0f, float rotation = 0f)
            {
                this.icon = icon;
                this.width = width;
                image = new Image(MTN.Journal[icon]);
                image.CenterOrigin();
                image.Rotation = rotation;
            }

            public override float Width()
            {
                return Math.Max(MTN.Journal[icon].Width, width);
            }

            public override void Render(Vector2 center, float columnWidth)
            {
                image.Position = center;
                image.Render();
            }
        }

        public class IconsCell : Cell
        {
            private float iconSpacing = 4f;

            private string[] icons;

            public IconsCell(float iconSpacing, params string[] icons)
            {
                this.iconSpacing = iconSpacing;
                this.icons = icons;
            }

            public IconsCell(params string[] icons)
            {
                this.icons = icons;
            }

            public override float Width()
            {
                float num = 0f;
                for (int i = 0; i < icons.Length; i++)
                {
                    num += MTN.Journal[icons[i]].Width;
                }
                return num + (icons.Length - 1) * iconSpacing;
            }

            public override void Render(Vector2 center, float columnWidth)
            {
                float num = Width();
                Vector2 position = center + new Vector2((0f - num) * 0.5f, 0f);
                for (int i = 0; i < icons.Length; i++)
                {
                    MTexture mTexture = MTN.Journal[icons[i]];
                    mTexture.DrawJustified(position, new Vector2(0f, 0.5f));
                    position.X += mTexture.Width + iconSpacing;
                }
            }
        }

        public const int PageWidth = 1610;

        public const int PageHeight = 1000;

        public readonly Vector2 TextJustify = new(0.5f, 0.5f);

        public const float TextScale = 0.5f;

        public readonly Color TextColor = Color.Black * 0.6f;

        public int PageIndex;

        public string PageTexture;

        public Journal Journal;

        public bool ForceRedraw;

        public JournalPage(Journal journal)
        {
            Journal = journal;
        }

        public virtual void Redraw(VirtualRenderTarget buffer)
        {
            Engine.Graphics.GraphicsDevice.SetRenderTarget(buffer);
            Engine.Graphics.GraphicsDevice.Clear(Color.Transparent);
        }

        public virtual void Update()
        {
        }
    }
}
