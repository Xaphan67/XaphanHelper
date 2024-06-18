using System.Collections.Generic;
using Celeste.Mod.XaphanHelper.Entities;
using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste.Mod.XaphanHelper.UI_Elements
{
    [Tracked(true)]
    class BossHealthBar : Entity
    {
        private Entity Boss;

        public static Player player;

        private Image Icon;

        private Image section = new(GFX.Gui["bossHealth/section"]);

        private Image sectionSeparator = new(GFX.Gui["bossHealth/separator"]);

        private Image checkpoint = new(GFX.Gui["bossHealth/checkpoint"]);

        public HashSet<Image> Sections = new();

        public int TotalSections;

        private int CheckpointValue;

        private Color borderColor;

        private float width;

        private float Opacity;

        public BossHealthBar(Entity boss, int maxBossHealth, int checkpoint = 0, bool isCM = false)
        {
            Tag = (Tags.HUD | Tags.Persistent | Tags.PauseUpdate);
            Boss = boss;
            TotalSections = maxBossHealth;
            borderColor = Calc.HexToColor("262626");
            Icon = new(GFX.Gui["bossHealth/icon" + (isCM ? "CM" : "")]);
            Depth = -99;
            CheckpointValue = checkpoint;
        }

        public override void Added(Scene scene)
        {
            base.Added(scene);
            SetXPosition();
            Position.Y = 1006f;
        }

        public override void Update()
        {
            base.Update();
            SetXPosition();
            UpdateOpacity();
        }

        public void SetXPosition()
        {
            Sections = GetSections();
            width = Icon.Width + Sections.Count / 2f * (section.Width + section.Width / 6f) + 33f;
            Position.X = Engine.Width / 2 - width / 2;
        }

        private HashSet<Image> GetSections()
        {
            Sections = new HashSet<Image>();
            if (Boss != null)
            {
                for (int i = 1; i <= TotalSections; i++)
                {
                    if (Boss.GetType() == typeof(Torizo))
                    {
                        Torizo boss = Boss as Torizo;
                        if (boss.Health >= i)
                        {
                            Sections.Add(new Image(GFX.Gui["bossHealth/section"]));
                            if (boss.Health == i)
                            {
                                Sections.Add(new Image(GFX.Gui["bossHealth/separatorLast"]));
                            }
                            else
                            {
                                Sections.Add(new Image(GFX.Gui["bossHealth/separator"]));
                            }
                            continue;
                        }
                    }
                    else if (Boss.GetType() == typeof(CustomFinalBoss))
                    {
                        CustomFinalBoss boss = Boss as CustomFinalBoss;
                        if (boss.hits <= TotalSections - i)
                        {
                            Sections.Add(new Image(GFX.Gui["bossHealth/section"]));
                            if (boss.hits == TotalSections - i)
                            {
                                Sections.Add(new Image(GFX.Gui["bossHealth/separatorLast"]));
                            }
                            else
                            {
                                Sections.Add(new Image(GFX.Gui["bossHealth/separator"]));
                            }
                            continue;
                        }
                    }
                    else if (Boss.GetType() == typeof(AncientGuardian))
                    {
                        AncientGuardian boss = Boss as AncientGuardian;
                        if (boss.Health >= i)
                        {
                            Sections.Add(new Image(GFX.Gui["bossHealth/section"]));
                            if (boss.Health == i)
                            {
                                Sections.Add(new Image(GFX.Gui["bossHealth/separatorLast"]));
                            }
                            else
                            {
                                Sections.Add(new Image(GFX.Gui["bossHealth/separator"]));
                            }
                            continue;
                        }
                    }
                    else if (Boss.GetType() == typeof(Genesis))
                    {
                        Genesis boss = Boss as Genesis;
                        if (boss.Health >= i)
                        {
                            Sections.Add(new Image(GFX.Gui["bossHealth/section"]));
                            if (boss.Health == i)
                            {
                                Sections.Add(new Image(GFX.Gui["bossHealth/separatorLast"]));
                            }
                            else
                            {
                                Sections.Add(new Image(GFX.Gui["bossHealth/separator"]));
                            }
                            continue;
                        }
                    }
                    Sections.Add(new Image(GFX.Gui["bossHealth/sectionEmpty"]));
                    Sections.Add(new Image(GFX.Gui["bossHealth/separatorEmpty"]));
                }
            }
            return Sections;
        }

        public void UpdateOpacity()
        {
            player = SceneAs<Level>().Tracker.GetEntity<Player>();
            if (player != null && player.Center.X > SceneAs<Level>().Camera.Right - 32f + width / 6 && player.Center.Y < SceneAs<Level>().Camera.Top + 52)
            {
                Opacity = Calc.Approach(Opacity, 0.3f, Engine.RawDeltaTime * 3f);
            }
            else
            {
                Opacity = Calc.Approach(Opacity, 1f, Engine.RawDeltaTime * 3f);
            }
        }

        public override void Render()
        {
            base.Render();
            Draw.Rect(Position + new Vector2(2), width, 46f, Color.Black * 0.85f * Opacity);
            string name = "";
            if (Boss.GetType() == typeof(Torizo))
            {
                name = Dialog.Clean("LorebookEntry_Boss_1_1_Name");
            }
            else if (Boss.GetType() == typeof(CustomFinalBoss))
            {
                name = Dialog.Clean("LorebookEntry_Boss_2_1_Name");
            }
            else if (Boss.GetType() == typeof(AncientGuardian))
            {
                name = Dialog.Clean("LorebookEntry_Boss_4_1_Name");
            }
            else if (Boss.GetType() == typeof(Genesis))
            {
                name = Dialog.Clean("LorebookEntry_Boss_5_1_Name");
            }
            ActiveFont.DrawOutline(name.ToUpper(), Position + new Vector2((width + 4f) / 2f, 0f), new Vector2(0.5f, 0.5f), Vector2.One * 0.3f, Color.Yellow * Opacity, 2f, Color.Black * Opacity);
            float nameLenght = ActiveFont.Measure(name.ToUpper()).X * 0.3f;

            Draw.Rect(Position, (width + 4f) / 2f - nameLenght / 2 - 10f, 2f, borderColor * Opacity);
            Draw.Rect(Position + new Vector2((width + 4f) / 2f, 0f) + new Vector2(nameLenght / 2 + 11f, 0), (width + 4f) / 2f - nameLenght / 2 - 10f, 2f, borderColor * Opacity);
            Draw.Rect(Position + new Vector2(0f, 2f), 2f, 46f, borderColor * Opacity);
            Draw.Rect(Position + new Vector2(width + 2f, 2f), 2f, 46f, borderColor * Opacity);
            Draw.Rect(Position + new Vector2(0f, 46f + 2f), width + 4f, 2f, borderColor * Opacity);

            Icon.Position = Position + new Vector2(11f, 9f);
            Icon.Render();

            int OffsetX = (int)Icon.Width + 11;
            int Col = 1;
            checkpoint.Position = Position + new Vector2(13f) + new Vector2(OffsetX + (TotalSections - CheckpointValue) * (section.Width + section.Width / 6f) - 4f, -2f);

            foreach (Image section in Sections)
            {
                section.Position = Position + new Vector2(13f) + Vector2.UnitX * OffsetX;
                section.Color = Color.White * Opacity;
                section.Render();
                OffsetX += ((int)section.Width);
                Col++;
            }

            if (CheckpointValue != 0)
            {
                checkpoint.Render();
            }
        }
    }
}
