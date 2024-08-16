using System;
using System.Collections.Generic;
using System.Linq;
using Celeste.Mod.XaphanHelper.Data;
using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste.Mod.XaphanHelper.UI_Elements
{
    [Tracked(true)]
    public class SaveProgressDisplay : Entity
    {
        public Sprite Portrait;

        private string SubAreaName;

        private string Time;

        private static float numberWidth;

        private static float spacerWidth;

        private HashSet<string> Upgrades = new();

        private string Medals;

        private string Items;

        private string Map;

        private int Strawberries;

        private int Tanks;

        private int FireRateModules;

        private int MissilesModules;

        private int Cassettes;

        private int BlueHearts;

        private int RedHearts;

        private int YellowHearts;

        private string Gems;

        public SaveProgressDisplay(Vector2 position) : base(position)
        {
            Tag = Tags.HUD;
            string text = "portrait_madeline";
            Portrait = GFX.PortraitsSpriteBank.Create(text);
            Portrait.Play("idle_normal");
            Portrait.Scale = Vector2.One * (200f / GFX.PortraitsSpriteBank.SpriteData[text].Sources[0].XML.AttrInt("size", 160));
            Add(Portrait);
            SubAreaName = GetCurrentSubAreaName();
            Time = GetCurrentTime();
            CalculateBaseSizes();
            GetCurrentUpgrades();
            Gems = GetCurrentGems();
        }

        public override void Added(Scene scene)
        {
            base.Added(scene);
            Medals = GetCurrentAchievementsMedals().ToString();
            Items = GetCurrentItemsPercent();
            Map = GetCurrentMapPercent();
            Strawberries = GetCurrentStrawberries();
            Tanks = GetCurrentEnergyTanks();
            FireRateModules = GetCurrentFireRateModules();
            MissilesModules = GetCurrentMissilesModules();
            Cassettes = GetCurrentCassettes();
            BlueHearts = GetCurrentBlueHearts();
            RedHearts = GetCurrentRedHearts();
            YellowHearts = GetCurrentYellowHearts();
        }

        public override void Render()
        {
            float Width = 1400f; // Add 64 per slot on the right side for future items
            // Background
            Draw.Rect(Position - new Vector2(Width / 2f - 5f, 130f), Width - 10f, 260f, Color.Black * 0.6f);
            Draw.HollowRect(Position - new Vector2(Width / 2f - 4f, 131f), Width - 8f, 262f, Color.Black * 0.5f);
            Draw.HollowRect(Position - new Vector2(Width / 2f - 3f, 132f), Width - 6f, 264f, Color.Black * 0.4f);
            Draw.HollowRect(Position - new Vector2(Width / 2f - 2f, 133f), Width - 4f, 266f, Color.Black * 0.3f);
            Draw.HollowRect(Position - new Vector2(Width / 2f - 1f, 134f), Width - 2f, 268f, Color.Black * 0.2f);
            Draw.HollowRect(Position - new Vector2(Width / 2f, 135f), Width, 270f, Color.Black * 0.1f);

            // Borders
            MTexture border = GFX.Gui["feather/border"];
            border.Draw(Position - new Vector2(Width / 2f + 10f, 145f));
            border.Draw(Position + new Vector2(Width / 2f + 10f, -145f), Vector2.Zero, Color.White, new Vector2(-1, 1));
            border.Draw(Position - new Vector2(Width / 2f + 10f, -145f), Vector2.Zero, Color.White, new Vector2(1, -1));
            border.Draw(Position + new Vector2(Width / 2f + 10f, 145f), Vector2.Zero, Color.White, new Vector2(-1));

            // Portrait
            Portrait.RenderPosition = Position - new Vector2(Width / 2f - 135f, 0f);
            Portrait.Render();
            Draw.HollowRect(Position - new Vector2(Width / 2f - 35f, 100f), Portrait.Width * Portrait.Scale.X, Portrait.Height * Portrait.Scale.Y, Color.Black * 0.8f);
            Draw.HollowRect(Position - new Vector2(Width / 2f - 36f, 99f), Portrait.Width * Portrait.Scale.X - 2, Portrait.Height * Portrait.Scale.Y - 2, Color.Black * 0.6f);
            Draw.HollowRect(Position - new Vector2(Width / 2f - 37f, 98f), Portrait.Width * Portrait.Scale.X - 4, Portrait.Height * Portrait.Scale.Y - 4, Color.Black * 0.4f);
            Draw.HollowRect(Position - new Vector2(Width / 2f - 38f, 97f), Portrait.Width * Portrait.Scale.X - 6, Portrait.Height * Portrait.Scale.Y - 6, Color.Black * 0.2f);

            // Sub-Area Name
            ActiveFont.DrawOutline(SubAreaName, Position - new Vector2(Width / 2f - Portrait.Width * Portrait.Scale.X - 70f, 114f), Vector2.Zero, Vector2.One, Color.White, 2f, Color.Black);

            // Timer
            DrawTime(Position + new Vector2(Width / 2f - SpeedrunTimerDisplay.GetTimeWidth(Time) - 35f, -50f), Time, 1f);
            MTexture timer = MTN.Journal["time"];
            timer.Draw(Position + new Vector2(Width / 2f - SpeedrunTimerDisplay.GetTimeWidth(Time) - 90f, -108f));

            // Medals
            MTexture medal = GFX.Gui["achievements/medal"];
            ActiveFont.DrawOutline(Medals, Position - new Vector2(Width / 2f - Portrait.Width * Portrait.Scale.X - 15f - medal.Width * 0.35f - 70f, -50f), Vector2.Zero, new Vector2(0.78f), Color.White, 2f, Color.Black);
            medal.Draw(Position - new Vector2(Width / 2f - Portrait.Width * Portrait.Scale.X - 70f, -52f), Vector2.Zero, Color.White, new Vector2(0.35f));

            // Items
            MTexture item = GFX.Gui["maps/keys/upgrade"];
            ActiveFont.DrawOutline(Items, Position - new Vector2(Width / 2f - Portrait.Width * Portrait.Scale.X - 15f - item.Width * 0.95f - 70f - (58f * 3f), -50f), Vector2.Zero, new Vector2(0.78f), Color.White, 2f, Color.Black);
            item.Draw(Position - new Vector2(Width / 2f - Portrait.Width * Portrait.Scale.X - 70f - (58f * 3f), -50f), Vector2.Zero, Color.White, new Vector2(0.95f));

            // Map
            MTexture map = GFX.Gui["maps/keys/map"];
            ActiveFont.DrawOutline(Map, Position - new Vector2(Width / 2f - Portrait.Width * Portrait.Scale.X - 15f - map.Width * 0.95f - 70f - (58f * 6.5f), -50f), Vector2.Zero, new Vector2(0.78f), Color.White, 2f, Color.Black);
            map.Draw(Position - new Vector2(Width / 2f - Portrait.Width * Portrait.Scale.X - 70f - (58f * 6.5f), -50f), Vector2.Zero, Color.White, new Vector2(0.95f));

            // Gems
            foreach (string gem in Gems.Split(','))
            {
                switch (gem)
                {
                    case "1":
                        MTexture gem1 = GFX.Gui["vignette/Xaphan/gem1"];
                        gem1.Draw(Position - new Vector2(Width / 2f - Portrait.Width * Portrait.Scale.X - 70f - (58f * 10f) + 9f, -55f));
                        break;
                    case "2":
                        MTexture gem2 = GFX.Gui["vignette/Xaphan/gem2"];
                        gem2.Draw(Position - new Vector2(Width / 2f - Portrait.Width * Portrait.Scale.X - 70f - (58f * 10f) - 31f, -55f));
                        break;
                    case "1-2":
                        MTexture gem3 = GFX.Gui["vignette/Xaphan/gem1-2"];
                        gem3.Draw(Position - new Vector2(Width / 2f - Portrait.Width * Portrait.Scale.X - 70f - (58f * 10f) - 71f, -55f));
                        break;
                    case "4":
                        MTexture gem4 = GFX.Gui["vignette/Xaphan/gem4"];
                        gem4.Draw(Position - new Vector2(Width / 2f - Portrait.Width * Portrait.Scale.X - 70f - (58f * 10f) - 111f, -55f));
                        break;
                    case "5":
                        MTexture gem5 = GFX.Gui["vignette/Xaphan/gem5"];
                        gem5.Draw(Position - new Vector2(Width / 2f - Portrait.Width * Portrait.Scale.X - 70f - (58f * 10f) - 151f, -55f));
                        break;
                    case "3":
                        MTexture gem6 = GFX.Gui["vignette/Xaphan/gem3"];
                        gem6.Draw(Position - new Vector2(Width / 2f - Portrait.Width * Portrait.Scale.X - 70f - (58f * 10f) - 191f, -55f));
                        break;
                }
            }

            // Upgrades
            int upgradeXPos = 0;
            int upgradeYPos = 52;
            int currentUpgrade = 1;
            foreach (string upgrade in Upgrades)
            {
                MTexture icon = GFX.Gui["collectables/XaphanHelper/UpgradeCollectable/" + upgrade + "00"];
                icon.Draw(Position - new Vector2(Width / 2f - Portrait.Width * Portrait.Scale.X - 70f - upgradeXPos, upgradeYPos), Vector2.Zero, Color.White, new Vector2(0.1f));
                if (currentUpgrade < 14)
                {
                    upgradeXPos += 58;
                }
                else
                {
                    if (currentUpgrade == 14)
                    {
                        upgradeXPos = 0;
                        upgradeYPos = 0;
                    }
                    else
                    {
                        upgradeXPos += 58;
                    }
                }
                currentUpgrade++;
            }

            // Strawberries
            if (Strawberries > 0)
            {
                MTexture strawberry = GFX.Gui["maps/keys/strawberry"];
                strawberry.Draw(Position - new Vector2(Width / 2f - Portrait.Width * Portrait.Scale.X - 70f - (58f * 14f) - 35f - 2f, 52f));
                ActiveFont.DrawOutline("x " + Strawberries.ToString(), Position - new Vector2(Width / 2f - Portrait.Width * Portrait.Scale.X - 70f - (58f * 14f) - 35f - 2f - (strawberry.Width / 2), 52f - strawberry.Height - 9f - 4f), new Vector2(0.5f), new Vector2(0.4f), Color.White, 2f, Color.Black);
            }

            // Energy Tanks
            if (Tanks > 0)
            {
                MTexture tank = GFX.Gui["maps/keys/energyTank"];
                tank.Draw(Position - new Vector2(Width / 2f - Portrait.Width * Portrait.Scale.X - 70f - (58f * 14f) - 35f - (1f * 64f), 52f));
                ActiveFont.DrawOutline("x " + Tanks.ToString(), Position - new Vector2(Width / 2f - Portrait.Width * Portrait.Scale.X - 70f - (58f * 14f) - 35f - (1f * 64f) - (tank.Width / 2), 52f - tank.Height - 9f), new Vector2(0.5f), new Vector2(0.4f), Color.White, 2f, Color.Black);
            }

            // Fire Rate Modules
            if (FireRateModules > 0)
            {
                MTexture fireRateModule = GFX.Gui["maps/keys/fireRateModule"];
                fireRateModule.Draw(Position - new Vector2(Width / 2f - Portrait.Width * Portrait.Scale.X - 70f - (58f * 14f) - 35f - (2f * 64f) - 2f, 52f));
                ActiveFont.DrawOutline("x " + FireRateModules.ToString(), Position - new Vector2(Width / 2f - Portrait.Width * Portrait.Scale.X - 70f - (58f * 14f) - 35f - (2f * 64f) - 2f - (fireRateModule.Width / 2), 52f - fireRateModule.Height - 9f - 4f), new Vector2(0.5f), new Vector2(0.4f), Color.White, 2f, Color.Black);
            }

            // Missiles Modules
            if (MissilesModules > 0)
            {
                MTexture missileModule = GFX.Gui["maps/keys/missile"];
                missileModule.Draw(Position - new Vector2(Width / 2f - Portrait.Width * Portrait.Scale.X - 70f - (58f * 14f) - 35f - (3f * 64f), 52f));
                ActiveFont.DrawOutline("x " + MissilesModules.ToString(), Position - new Vector2(Width / 2f - Portrait.Width * Portrait.Scale.X - 70f - (58f * 14f) - 35f - (3f * 64f) - (missileModule.Width / 2), 52f - missileModule.Height - 9f), new Vector2(0.5f), new Vector2(0.4f), Color.White, 2f, Color.Black);
            }

            // Cassettes
            if (Cassettes > 0)
            {
                MTexture cassette = GFX.Gui["maps/keys/cassette"];
                cassette.Draw(Position - new Vector2(Width / 2f - Portrait.Width * Portrait.Scale.X - 70f - (58f * 14f) - 35f - 2f, -26f));
                ActiveFont.DrawOutline("x " + Cassettes.ToString(), Position - new Vector2(Width / 2f - Portrait.Width * Portrait.Scale.X - 70f - (58f * 14f) - 35f - 2f - (cassette.Width / 2), -26f - cassette.Height - 9f - 4f), new Vector2(0.5f), new Vector2(0.4f), Color.White, 2f, Color.Black);
            }

            // Blue Hearts
            if (BlueHearts > 0)
            {
                MTexture blueHeart = GFX.Gui["maps/keys/heart"];
                blueHeart.Draw(Position - new Vector2(Width / 2f - Portrait.Width * Portrait.Scale.X - 70f - (58f * 14f) - 35f - (1f * 64f) - 2f, -26f));
                ActiveFont.DrawOutline("x " + BlueHearts.ToString(), Position - new Vector2(Width / 2f - Portrait.Width * Portrait.Scale.X - 70f - (58f * 14f) - 35f - 2f - (1f * 64f) - (blueHeart.Width / 2), -26f - blueHeart.Height - 9f - 4f), new Vector2(0.5f), new Vector2(0.4f), Color.White, 2f, Color.Black);
            }

            // Red Hearts
            if (RedHearts > 0)
            {
                MTexture redHeart = GFX.Gui["maps/keys/heartB"];
                redHeart.Draw(Position - new Vector2(Width / 2f - Portrait.Width * Portrait.Scale.X - 70f - (58f * 14f) - 35f - (2f * 64f) - 2f, -26f));
                ActiveFont.DrawOutline("x " + RedHearts.ToString(), Position - new Vector2(Width / 2f - Portrait.Width * Portrait.Scale.X - 70f - (58f * 14f) - 35f - 2f - (2f * 64f) - (redHeart.Width / 2), -26f - redHeart.Height - 9f - 4f), new Vector2(0.5f), new Vector2(0.4f), Color.White, 2f, Color.Black);
            }

            // Yellow Hearts
            if (YellowHearts > 0)
            {
                MTexture yellowHeart = GFX.Gui["maps/keys/heartC"];
                yellowHeart.Draw(Position - new Vector2(Width / 2f - Portrait.Width * Portrait.Scale.X - 70f - (58f * 14f) - 35f - (3f * 64f) - 2f, -26f));
                ActiveFont.DrawOutline("x " + YellowHearts.ToString(), Position - new Vector2(Width / 2f - Portrait.Width * Portrait.Scale.X - 70f - (58f * 14f) - 35f - 2f - (3f * 64f) - (yellowHeart.Width / 2), -26f - yellowHeart.Height - 9f - 4f), new Vector2(0.5f), new Vector2(0.4f), Color.White, 2f, Color.Black);
            }
        }

        public static void CalculateBaseSizes()
        {
            PixelFont font = Dialog.Languages["english"].Font;
            float fontFaceSize = Dialog.Languages["english"].FontFaceSize;
            PixelFontSize pixelFontSize = font.Get(fontFaceSize);
            for (int i = 0; i < 10; i++)
            {
                float x = pixelFontSize.Measure(i.ToString()).X;
                if (x > numberWidth)
                {
                    numberWidth = x;
                }
            }
            spacerWidth = pixelFontSize.Measure('.').X;
        }

        public static void DrawTime(Vector2 position, string timeString, float scale = 1f)
        {
            PixelFont font = Dialog.Languages["english"].Font;
            float fontFaceSize = Dialog.Languages["english"].FontFaceSize;
            float num = scale;
            float num2 = position.X;
            float num3 = position.Y;
            Color color = Color.White;
            Color color2 = Color.LightGray;
            for (int i = 0; i < timeString.Length; i++)
            {
                char c = timeString[i];
                if (c == '.')
                {
                    num = scale * 0.7f;
                    num3 -= 5f * scale;
                }
                Color color3 = ((c == ':' || c == '.' || num < scale) ? color2 : color);
                float num4 = (((c == ':' || c == '.') ? spacerWidth : numberWidth) + 4f) * num;
                font.DrawOutline(fontFaceSize, c.ToString(), new Vector2(num2 + num4 / 2f, num3), new Vector2(0.5f, 1f), Vector2.One * num, color3, 2f, Color.Black);
                num2 += num4;
            }
        }

        private string GetCurrentSubAreaName()
        {
            if (XaphanModule.ModSaveData.SavedChapter.ContainsKey("Xaphan/0"))
            {
                int savedChapterIndex = XaphanModule.ModSaveData.SavedChapter["Xaphan/0"];
                string savedRoom = XaphanModule.ModSaveData.SavedRoom["Xaphan/0"];
                if (savedChapterIndex == -1)
                {
                    if (savedRoom == "A-00" || savedRoom == "A-01" || savedRoom == "A-02" || savedRoom == "A-03")
                    {
                        return Dialog.Clean("Xaphan_0_Sub_Ch0_A1");
                    }
                    else
                    {
                        return Dialog.Clean("Xaphan_0_Sub_Ch0_A2");
                    }
                }
                return Dialog.Clean("Xaphan_0_Sub_Ch" + savedChapterIndex + "_" + savedRoom.Substring(0, 1));
            }
            return "-";
        }

        private string GetCurrentTime()
        {
            if (XaphanModule.ModSaveData.SavedChapter.ContainsKey("Xaphan/0"))
            {
                long savedTime = XaphanModule.ModSaveData.SavedTime["Xaphan/0"];
                TimeSpan savedTimeSpan = TimeSpan.FromTicks(savedTime);
                return ((!(savedTimeSpan.TotalHours >= 1.0)) ? savedTimeSpan.ToString("mm\\:ss") : ((int)savedTimeSpan.TotalHours + ":" + savedTimeSpan.ToString("mm\\:ss")));
            }
            TimeSpan timeSpan = TimeSpan.FromTicks(0);
            return ((!(timeSpan.TotalHours >= 1.0)) ? timeSpan.ToString("mm\\:ss") : ((int)timeSpan.TotalHours + ":" + timeSpan.ToString("mm\\:ss")));
        }

        private void GetCurrentUpgrades()
        {
            foreach (string flag in XaphanModule.ModSaveData.SavedFlags)
            {
                if (flag.Contains("Xaphan/0_Upgrade_"))
                {
                    Upgrades.Add(flag.Replace("Xaphan/0_Upgrade_", ""));
                }
            }
            SortUpgrades();
        }

        private void SortUpgrades()
        {
            SortedDictionary<int, string> orderedUpgrades = new();
            foreach (string upgrade in Upgrades)
            {
                orderedUpgrades.Add(SetUpgradeOrder(upgrade), upgrade);
            }
            Upgrades.Clear();
            foreach (KeyValuePair<int, string> orderedMadelineUpgrade in orderedUpgrades)
            {
                Upgrades.Add(orderedMadelineUpgrade.Value);
            }
        }

        private int SetUpgradeOrder(string upgrade)
        {
            switch (upgrade)
            {
                case "PowerGrip":
                    return 0;
                case "ClimbingKit":
                    return 1;
                case "SpiderMagnet":
                    return 2;
                case "DashBoots":
                    return 3;
                case "SpaceJump":
                    return 4;
                case "LightningDash":
                    return 5;
                case "VariaJacket":
                    return 6;
                case "GravityJacket":
                    return 7;
                case "Bombs":
                    return 8;
                case "MegaBombs":
                    return 9;
                case "RemoteDrone":
                    return 10;
                case "GoldenFeather":
                    return 11;
                case "EtherealDash":
                    return 12;
                case "ScrewAttack":
                    return 13;
                case "Binoculars":
                    return 14;
                case "PortableStation":
                    return 15;
                case "PulseRadar":
                    return 16;
                case "DroneTeleport":
                    return 17;
                case "JumpBoost":
                    return 18;
                case "HoverJet":
                    return 19;
                case "MissilesModule":
                    return 20;
                case "SuperMissilesModule":
                    return 21;
                case "LongBeam":
                    return 22;
                case "IceBeam":
                    return 23;
                case "WaveBeam":
                    return 24;
            }
            return -1;
        }

        private int GetCurrentAchievementsMedals()
        {
            int medals = 0;
            foreach (AchievementData achievement in Achievements.GenerateAchievementsList(SceneAs<Level>().Session))
            {
                if (SceneAs<Level>().Session.GetFlag(achievement.Flag))
                {
                    medals += achievement.Medals;
                }
            }
            return medals;
        }

        private string GetCurrentItemsPercent()
        {
            int currentItems = 0;
            int maxItems = 0;
            foreach (AchievementData achievement in Achievements.GenerateAchievementsList(SceneAs<Level>().Session))
            {
                if (achievement.AchievementID == "items")
                {
                    currentItems = achievement.CurrentValue;
                    maxItems = achievement.MaxValue;
                    break;
                }
            }
            return (currentItems * 100 / maxItems).ToString() + " %";
        }

        private string GetCurrentMapPercent()
        {
            int currentTiles = 0;
            int maxTiles = 0;
            foreach (AchievementData achievement in Achievements.GenerateAchievementsList(SceneAs<Level>().Session))
            {
                if (achievement.AchievementID == "map")
                {
                    currentTiles = achievement.CurrentValue;
                    maxTiles = achievement.MaxValue;
                    break;
                }
            }
            return (currentTiles * 100 / maxTiles).ToString() + " %";
        }

        private int GetCurrentStrawberries()
        {
            foreach (AchievementData achievement in Achievements.GenerateAchievementsList(SceneAs<Level>().Session))
            {
                if (achievement.AchievementID == "strwb")
                {
                    return achievement.CurrentValue;
                }
            }
            return 0;
        }

        private int GetCurrentEnergyTanks()
        {
            foreach (AchievementData achievement in Achievements.GenerateAchievementsList(SceneAs<Level>().Session))
            {
                if (achievement.AchievementID == "tank")
                {
                    return achievement.CurrentValue;
                }
            }
            return 0;
        }

        private int GetCurrentFireRateModules()
        {
            foreach (AchievementData achievement in Achievements.GenerateAchievementsList(SceneAs<Level>().Session))
            {
                if (achievement.AchievementID == "dfrm")
                {
                    return achievement.CurrentValue;
                }
            }
            return 0;
        }

        private int GetCurrentMissilesModules()
        {
            foreach (AchievementData achievement in Achievements.GenerateAchievementsList(SceneAs<Level>().Session))
            {
                if (achievement.AchievementID == "dmiss")
                {
                    return achievement.CurrentValue;
                }
            }
            return 0;
        }

        private int GetCurrentCassettes()
        {
            foreach (AchievementData achievement in Achievements.GenerateAchievementsList(SceneAs<Level>().Session))
            {
                if (achievement.AchievementID == "cass")
                {
                    return achievement.CurrentValue;
                }
            }
            return 0;
        }

        private int GetCurrentBlueHearts()
        {
            foreach (AchievementData achievement in Achievements.GenerateAchievementsList(SceneAs<Level>().Session))
            {
                if (achievement.AchievementID == "heart")
                {
                    return achievement.CurrentValue;
                }
            }
            return 0;
        }

        private int GetCurrentRedHearts()
        {
            int currentHearts = 0;
            foreach (bool heart in StatsFlags.BSideHearts)
            {
                if (heart)
                {
                    currentHearts++;
                }
            }
            return currentHearts;
        }

        private int GetCurrentYellowHearts()
        {
            int currentHearts = 0;
            foreach (AchievementData achievement in Achievements.GenerateAchievementsList(SceneAs<Level>().Session))
            {
                if (achievement.AchievementID.Contains("boss")  && achievement.AchievementID.Contains("cm"))
                {
                    if (SceneAs<Level>().Session.GetFlag(achievement.Flag))
                    {
                        currentHearts++;
                    }
                }
            }
            return currentHearts;
        }

        private string GetCurrentGems()
        {
            string currentGems = "";
            foreach (string flag in XaphanModule.ModSaveData.SavedFlags)
            {
                if (flag.Contains("Xaphan/0") && (flag.Contains("Gem_Collected") || flag.Contains("Gem2_Collected")))
                {
                    if (!string.IsNullOrEmpty(currentGems))
                    {
                        currentGems += ",";
                    }
                    currentGems += flag.Split('_')[1].Remove(0, 2) + (flag.Split('_')[2].Remove(0, 3).Length > 0 ? "-" + flag.Split('_')[2].Remove(0, 3) : "");
                }
            }
            return currentGems;
        }
    }
}
