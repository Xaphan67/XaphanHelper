using System;
using System.Collections.Generic;
using Celeste.Mod.XaphanHelper.Entities;
using Celeste.Mod.XaphanHelper.Managers;
using Celeste.Mod.XaphanHelper.Upgrades;
using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste.Mod.XaphanHelper.UI_Elements
{
    [Tracked(true)]
    class BreathDisplay : Entity
    {
        private Image section = new(GFX.Gui["upgrades/stamina/section"]);

        public HashSet<Image> Sections = new();

        public static Player player;

        private float Opacity;

        private Color borderColor;

        private int sectionSpacing;

        private float width;

        public BreathDisplay()
        {
            Tag = (Tags.HUD | Tags.Persistent | Tags.PauseUpdate);
            borderColor = Calc.HexToColor("262626");
            ButtonBinding Control = XaphanModule.ModSettings.SelectItem;
            Depth = -99;
        }

        public static void Load()
        {
            On.Celeste.Level.Update += onLevelUpdate;
        }

        public static void Unload()
        {
            On.Celeste.Level.Update -= onLevelUpdate;
        }

        private static void onLevelUpdate(On.Celeste.Level.orig_Update orig, Level self)
        {
            orig(self);
            bool sliding = false;
            player = self.Tracker.GetEntity<Player>();
            foreach (PlayerPlatform slope in self.Tracker.GetEntities<PlayerPlatform>())
            {
                if (slope.Sliding)
                {
                    sliding = true;
                    break;
                }
            }
            if ((self.FrozenOrPaused || self.RetryPlayerCorpse != null || self.SkippingCutscene || self.InCutscene) || (player != null && !player.Sprite.Visible && !self.Session.GetFlag("Xaphan_Helper_Ceiling") && !sliding && (self.Tracker.GetEntity<ScrewAttackManager>() != null ? !self.Tracker.GetEntity<ScrewAttackManager>().StartedScrewAttack : true)) || XaphanModule.ShowUI)
            {
                if (self.Tracker.GetEntity<BreathDisplay>() != null)
                {
                    self.Tracker.GetEntity<BreathDisplay>().RemoveSelf();
                }
            }
            else
            {
                if (self.Tracker.GetEntity<BreathDisplay>() == null)
                {
                    self.Add(new BreathDisplay());
                }
            }
        }

        public void SetXPosition()
        {
            Sections = GetSections();
            width = Sections.Count * (section.Width + sectionSpacing) + 22f - sectionSpacing;
            int BagDisplays = SceneAs<Level>().Tracker.GetEntities<BagDisplay>().Count;
            Position.X = 1920f - (XaphanModule.minimapEnabled ? 222f : 0f) - 27f - width - BagDisplays * 120f;
        }

        public void SetYPosition()
        {
            Position.Y = 26f;
            if (XaphanModule.useUpgrades && PowerGrip.isActive && (SceneAs<Level>().Session.Area.LevelSet == "Xaphan/0" ? XaphanModule.ModSettings.SoCMStaminaIndicator : XaphanModule.ModSettings.StaminaIndicator) != 1 && (SceneAs<Level>().Session.Area.LevelSet == "Xaphan/0" ? XaphanModule.ModSettings.SoCMStaminaIndicator : XaphanModule.ModSettings.StaminaIndicator) != 3)
            {
                Position.Y = 80f;
            }
        }

        public override void Added(Scene scene)
        {
            base.Added(scene);
            sectionSpacing = 2;
            SetXPosition();
            SetYPosition();
        }

        public void UpdateOpacity()
        {
            int BagDisplays = SceneAs<Level>().Tracker.GetEntities<BagDisplay>().Count;
            BreathManager manager = SceneAs<Level>().Tracker.GetEntity<BreathManager>();
            if (manager != null)
            {
                if (manager.isVisible)
                {
                    if (player != null && player.Center.X > SceneAs<Level>().Camera.Right - ((BagDisplays == 2 ? 64f : 32f) + width / 6) && player.Center.Y < SceneAs<Level>().Camera.Top + 52)
                    {
                        Opacity = Calc.Approach(Opacity, 0.3f, Engine.RawDeltaTime * 3f);
                    }
                    else
                    {
                        Opacity = Calc.Approach(Opacity, 1f, Engine.RawDeltaTime * 3f);
                    }
                }
                else if (player != null)
                {
                    Opacity = Calc.Approach(Opacity, 0f, Engine.RawDeltaTime * 3f);
                }
            }
        }

        public override void Update()
        {
            base.Update();
            if (SceneAs<Level>().Session.Area.LevelSet == "Xaphan/0" && SceneAs<Level>().Session.Level.Contains("Intro"))
            {
                Visible = false;
                return;
            }
            SetXPosition();
            SetYPosition();
            UpdateOpacity();
        }

        private HashSet<Image> GetSections()
        {
            Sections = new HashSet<Image>();
            BreathManager manager = SceneAs<Level>().Tracker.GetEntity<BreathManager>();
            if (manager != null)
            {
                for (int i = 1; i <= 15; i++)
                {
                    if (manager.GetAirPercent() >= ((i - 1) * (100f / 15f)) && player != null)
                    {
                        Sections.Add(new Image(GFX.Gui["oxygen/section" + (i <= 4 ? "Low" : "")]));
                        continue;
                    }
                    Sections.Add(new Image(GFX.Gui["oxygen/sectionEmpty"]));
                }
            }
            return Sections;
        }

        public override void Render()
        {
            base.Render();
            if ((SceneAs<Level>().Session.Area.LevelSet == "Xaphan/0" ? XaphanModule.ModSettings.SoCMOxygenIndicator : XaphanModule.ModSettings.OxygenIndicator) != 1
                && (SceneAs<Level>().Session.Area.LevelSet == "Xaphan/0" ? XaphanModule.ModSettings.SoCMOxygenIndicator : XaphanModule.ModSettings.OxygenIndicator) != 3)
            {
                Draw.Rect(Position + new Vector2(2), width, 42f, Color.Black * 0.85f * Opacity);
                string name = Dialog.Clean("Xaphanhelper_UI_Breath");
                ActiveFont.DrawOutline(name, Position + new Vector2((width + 4f) / 2f, 0f), new Vector2(0.5f, 0.5f), Vector2.One * 0.3f, Color.Yellow * Opacity, 2f, Color.Black * Opacity);
                float nameLenght = ActiveFont.Measure(name).X * 0.3f;

                Draw.Rect(Position, (width + 4f) / 2f - nameLenght / 2 - 10f, 2f, borderColor * Opacity);
                Draw.Rect(Position + new Vector2((width + 4f) / 2f, 0f) + new Vector2(nameLenght / 2 + 11f, 0), (width + 4f) / 2f - nameLenght / 2 - 10f, 2f, borderColor * Opacity);
                Draw.Rect(Position + new Vector2(0f, 2f), 2f, 42f, borderColor * Opacity);
                Draw.Rect(Position + new Vector2(width + 2f, 2f), 2f, 42f, borderColor * Opacity);
                Draw.Rect(Position + new Vector2(0f, 42f + 2f), width + 4f, 2f, borderColor * Opacity);

                int OffsetX = 0;
                int Col = 1;
                foreach (Image section in Sections)
                {
                    section.Position = Position + new Vector2(13f) + Vector2.UnitX * OffsetX;
                    section.Color = Color.White * Opacity;
                    section.Render();
                    OffsetX += ((int)section.Width + sectionSpacing);
                    Col++;
                }
            }
        }
    }
}
