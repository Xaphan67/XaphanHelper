using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste.Mod.XaphanHelper.UI_Elements
{
    [Tracked(true)]
    class AmmoDisplay : Entity
    {
        public string Prefix;

        private Player player;

        private float Opacity;

        public Image MissileFirstFigure;

        public Image MissileSecondFigure;

        public Image MissileThirdFigure;

        public Image SuperMissileLeftFigure;

        public Image SuperMissileRightFigure;

        public Image PowerBombLeftFigure;

        public Image PowerBombRightFigure;

        public int CurrentMissiles = 0;

        public int MaxMissiles = 0;

        public Image MissileIcon;

        public bool MissileSelected;

        public int CurrentSuperMissiles = 0;

        public int MaxSuperMissiles = 0;

        public Image SuperMissileIcon;

        public bool SuperMissileSelected;

        public int CurrentPowerBombs = 0;

        public int MaxPowerBombs = 0;

        public Image PowerBombIcon;

        public bool PowerBombSelected;

        public AmmoDisplay(Vector2 position) : base(position)
        {
            Tag = (Tags.HUD | Tags.Persistent | Tags.PauseUpdate);
            MissileFirstFigure = new Image(GFX.Gui["ammo/0"]);
            MissileSecondFigure = new Image(GFX.Gui["ammo/0"]);
            MissileThirdFigure = new Image(GFX.Gui["ammo/0"]);
            SuperMissileLeftFigure = new Image(GFX.Gui["ammo/0"]);
            SuperMissileRightFigure = new Image(GFX.Gui["ammo/0"]);
            PowerBombLeftFigure = new Image(GFX.Gui["ammo/0"]);
            PowerBombRightFigure = new Image(GFX.Gui["ammo/0"]);
            MissileIcon = new Image(GFX.Gui["ammo/MissileOff"]);
            SuperMissileIcon = new Image(GFX.Gui["ammo/SuperMissileOff"]);
            PowerBombIcon = new Image(GFX.Gui["ammo/PowerBombOff"]);
            Depth = -10001;
        }

        public override void Added(Scene scene)
        {
            base.Added(scene);
            Prefix = SceneAs<Level>().Session.Area.LevelSet;
            foreach (KeyValuePair<string, int> maxMissiles in XaphanModule.ModSaveData.MaxMissiles)
            {
                if (maxMissiles.Key == Prefix)
                {
                    MaxMissiles = maxMissiles.Value;
                    break;
                }
            }
            CurrentMissiles = MaxMissiles;
            foreach (KeyValuePair<string, int> maxSuperMissiles in XaphanModule.ModSaveData.MaxSuperMissiles)
            {
                if (maxSuperMissiles.Key == Prefix)
                {
                    MaxSuperMissiles = maxSuperMissiles.Value;
                    break;
                }
            }
            CurrentSuperMissiles = MaxSuperMissiles;
            foreach (KeyValuePair<string, int> maxPowerBombs in XaphanModule.ModSaveData.MaxPowerBombs)
            {
                if (maxPowerBombs.Key == Prefix)
                {
                    MaxPowerBombs = maxPowerBombs.Value;
                    break;
                }
            }
            CurrentPowerBombs = MaxPowerBombs;
        }

        public override void Update()
        {
            base.Update();
            HealthDisplay healthDisplay = SceneAs<Level>().Tracker.GetEntity<HealthDisplay>();
            if (healthDisplay != null)
            {
                Position.X = 22f + healthDisplay.UIWidth + 10f;
            }
            Visible = !SceneAs<Level>().FrozenOrPaused;
            player = SceneAs<Level>().Tracker.GetEntity<Player>();
            if (player != null && player.Center.X < SceneAs<Level>().Camera.Left + 184f && player.Center.Y < SceneAs<Level>().Camera.Top + 52)
            {
                Opacity = Calc.Approach(Opacity, 0.3f, Engine.RawDeltaTime * 3f);
            }
            else
            {
                Opacity = Calc.Approach(Opacity, 1f, Engine.RawDeltaTime * 3f);
            }

            string MissileString = CurrentMissiles.ToString();
            if (MissileString.Length == 3)
            {
                MissileFirstFigure = new Image(GFX.Gui["ammo/" + MissileString[0]]);
                MissileSecondFigure = new Image(GFX.Gui["ammo/" + MissileString[1]]);
                MissileThirdFigure = new Image(GFX.Gui["ammo/" + MissileString[2]]);
            }
            else if (MissileString.Length == 2)
            {
                MissileFirstFigure = new Image(GFX.Gui["ammo/0"]);
                MissileSecondFigure = new Image(GFX.Gui["ammo/" + MissileString[0]]);
                MissileThirdFigure = new Image(GFX.Gui["ammo/" + MissileString[1]]);
            }
            else
            {
                MissileFirstFigure = new Image(GFX.Gui["ammo/0"]);
                MissileSecondFigure = new Image(GFX.Gui["ammo/0"]);
                MissileThirdFigure = new Image(GFX.Gui["ammo/" + MissileString[0]]);
            }
            string SuperMissileString = CurrentSuperMissiles.ToString();
            if (SuperMissileString.Length == 2)
            {
                SuperMissileLeftFigure = new Image(GFX.Gui["ammo/" + SuperMissileString[0]]);
                SuperMissileRightFigure = new Image(GFX.Gui["ammo/" + SuperMissileString[1]]);
            }
            else
            {
                SuperMissileLeftFigure = new Image(GFX.Gui["ammo/0"]);
                SuperMissileRightFigure = new Image(GFX.Gui["ammo/" + SuperMissileString[0]]);
            }
            string PowerBombString = CurrentPowerBombs.ToString();
            if (PowerBombString.Length == 2)
            {
                PowerBombLeftFigure = new Image(GFX.Gui["ammo/" + PowerBombString[0]]);
                PowerBombRightFigure = new Image(GFX.Gui["ammo/" + PowerBombString[1]]);
            }
            else
            {
                PowerBombLeftFigure = new Image(GFX.Gui["ammo/0"]);
                PowerBombRightFigure = new Image(GFX.Gui["ammo/" + PowerBombString[0]]);
            }
            if (!SceneAs<Level>().Paused && !SceneAs<Level>().PauseLock)
            {
                bool NoneSelected = !MissileSelected && !SuperMissileSelected && !PowerBombSelected;
                if (NoneSelected && XaphanModule.ModSettings.SelectItem.Pressed)
                {
                    if (MaxMissiles > 0 && CurrentMissiles > 0)
                    {
                        MissileSelected = true;
                        MissileIcon = new Image(GFX.Gui["ammo/MissileOn"]);
                    }
                    else if (MaxSuperMissiles > 0 && CurrentSuperMissiles > 0)
                    {
                        SuperMissileSelected = true;
                        SuperMissileIcon = new Image(GFX.Gui["ammo/SuperMissileOn"]);
                    }
                    else if (MaxPowerBombs > 0 && CurrentPowerBombs > 0)
                    {
                        PowerBombSelected = true;
                        PowerBombIcon = new Image(GFX.Gui["ammo/PowerBombOn"]);
                    }
                }
                else if (MissileSelected && XaphanModule.ModSettings.SelectItem.Pressed)
                {
                    MissileSelected = false;
                    MissileIcon = new Image(GFX.Gui["ammo/MissileOff"]);
                    if (MaxSuperMissiles > 0 && CurrentSuperMissiles > 0)
                    {
                        SuperMissileSelected = true;
                        SuperMissileIcon = new Image(GFX.Gui["ammo/SuperMissileOn"]);
                    }
                    else if (MaxPowerBombs > 0 && CurrentPowerBombs > 0)
                    {
                        PowerBombSelected = true;
                        PowerBombIcon = new Image(GFX.Gui["ammo/PowerBombOn"]);
                    }
                }
                else if (SuperMissileSelected && XaphanModule.ModSettings.SelectItem.Pressed)
                {
                    SuperMissileSelected = false;
                    SuperMissileIcon = new Image(GFX.Gui["ammo/SuperMissileOff"]);
                    if (MaxPowerBombs > 0 && CurrentPowerBombs > 0)
                    {
                        PowerBombSelected = true;
                        PowerBombIcon = new Image(GFX.Gui["ammo/PowerBombOn"]);
                    }
                }
                else if (PowerBombSelected && XaphanModule.ModSettings.SelectItem.Pressed)
                {
                    PowerBombSelected = false;
                    PowerBombIcon = new Image(GFX.Gui["ammo/PowerBombOff"]);
                }
                if (MissileSelected && CurrentMissiles == 0)
                {
                    MissileSelected = false;
                    Audio.Play("event:/game/xaphan/item_select");
                    MissileIcon = new Image(GFX.Gui["ammo/MissileOff"]);
                }
                else if (SuperMissileSelected && CurrentSuperMissiles == 0)
                {
                    SuperMissileSelected = false;
                    Audio.Play("event:/game/xaphan/item_select");
                    SuperMissileIcon = new Image(GFX.Gui["ammo/SuperMissileOff"]);
                }
                else if (PowerBombSelected && CurrentPowerBombs == 0)
                {
                    PowerBombSelected = false;
                    Audio.Play("event:/game/xaphan/item_select");
                    PowerBombIcon = new Image(GFX.Gui["ammo/PowerBombOff"]);
                }
                if ((CurrentMissiles > 0 || CurrentSuperMissiles > 0 || CurrentPowerBombs > 0) && XaphanModule.ModSettings.SelectItem.Pressed)
                {
                    Audio.Play("event:/game/xaphan/item_select");
                }
            }
        }

        public void AddMissile(int value)
        {
            Prefix = SceneAs<Level>().Session.Area.LevelSet;
            MaxMissiles += value;
            CurrentMissiles += value;
            if (!XaphanModule.ModSaveData.MaxMissiles.ContainsKey(Prefix))
            {
                XaphanModule.ModSaveData.MaxMissiles.Add(Prefix, MaxMissiles);
            }
            else
            {
                XaphanModule.ModSaveData.MaxMissiles[Prefix] += value;
            }
        }

        public void AddSuperMissile(int value)
        {
            Prefix = SceneAs<Level>().Session.Area.LevelSet;
            MaxSuperMissiles += value;
            CurrentSuperMissiles += value;
            if (!XaphanModule.ModSaveData.MaxSuperMissiles.ContainsKey(Prefix))
            {
                XaphanModule.ModSaveData.MaxSuperMissiles.Add(Prefix, MaxSuperMissiles);
            }
            else
            {
                XaphanModule.ModSaveData.MaxSuperMissiles[Prefix] += value;
            }
        }

        public void AddPowerBomb(int value)
        {
            Prefix = SceneAs<Level>().Session.Area.LevelSet;
            MaxPowerBombs += value;
            CurrentPowerBombs += value;
            if (!XaphanModule.ModSaveData.MaxPowerBombs.ContainsKey(Prefix))
            {
                XaphanModule.ModSaveData.MaxPowerBombs.Add(Prefix, MaxPowerBombs);
            }
            else
            {
                XaphanModule.ModSaveData.MaxPowerBombs[Prefix] += value;
            }
        }

        public void MoveX(float to)
        {
            Position.X = to;
        }

        public override void Render()
        {
            base.Render();

            // Missiles

            if (MaxMissiles > 0)
            {
                Draw.Rect(Position.X, Position.Y, 262f, 70f, Color.Black * 0.65f * Opacity);
                if (MissileFirstFigure != null && MissileSecondFigure != null && MissileThirdFigure != null)
                {
                    MissileFirstFigure.Position = Position + new Vector2(100f, 17f);
                    MissileFirstFigure.Color = (MissileSelected ? Color.Yellow : Color.White) * Opacity;
                    MissileFirstFigure.Render();
                    MissileSecondFigure.Position = Position + new Vector2(150f, 17f);
                    MissileSecondFigure.Color = (MissileSelected ? Color.Yellow : Color.White) * Opacity;
                    MissileSecondFigure.Render();
                    MissileThirdFigure.Position = Position + new Vector2(200f, 17f);
                    MissileThirdFigure.Color = (MissileSelected ? Color.Yellow : Color.White) * Opacity;
                    MissileThirdFigure.Render();
                }
                if (MissileIcon != null)
                {
                    MissileIcon.Position = Position + new Vector2(10f, 5f);
                    MissileIcon.Color = Color.White * Opacity;
                    MissileIcon.Render();
                }
            }

            // Super Missiles

            if (MaxSuperMissiles > 0)
            {
                int offset = MaxMissiles > 0 ? 272 : 0;
                Draw.Rect(Position.X + offset, Position.Y, 212f, 70f, Color.Black * 0.65f * Opacity);
                if (SuperMissileLeftFigure != null && SuperMissileRightFigure != null)
                {
                    SuperMissileLeftFigure.Position = Position + new Vector2(100f + offset, 17f);
                    SuperMissileLeftFigure.Color = (SuperMissileSelected ? Color.Yellow : Color.White) * Opacity;
                    SuperMissileLeftFigure.Render();
                    SuperMissileRightFigure.Position = Position + new Vector2(150f + offset, 17f);
                    SuperMissileRightFigure.Color = (SuperMissileSelected ? Color.Yellow : Color.White) * Opacity;
                    SuperMissileRightFigure.Render();
                }
                if (SuperMissileIcon != null)
                {
                    SuperMissileIcon.Position = Position + new Vector2(10f + offset, 5f);
                    SuperMissileIcon.Color = Color.White * Opacity;
                    SuperMissileIcon.Render();
                }
            }

            // Power Bombs

            if (MaxPowerBombs > 0)
            {
                int offset = MaxMissiles > 0 && MaxSuperMissiles > 0 ? 494 : MaxMissiles > 0 || MaxSuperMissiles > 0 ? 272 : 0;
                Draw.Rect(Position.X + offset, Position.Y, 194f, 70f, Color.Black * 0.65f * Opacity);
                if (PowerBombLeftFigure != null && PowerBombRightFigure != null)
                {
                    PowerBombLeftFigure.Position = Position + new Vector2(80f + offset, 17f);
                    PowerBombLeftFigure.Color = (PowerBombSelected ? Color.Yellow : Color.White) * Opacity;
                    PowerBombLeftFigure.Render();
                    PowerBombRightFigure.Position = Position + new Vector2(130f + offset, 17f);
                    PowerBombRightFigure.Color = (PowerBombSelected ? Color.Yellow : Color.White) * Opacity;
                    PowerBombRightFigure.Render();
                }
                if (PowerBombIcon != null)
                {
                    PowerBombIcon.Position = Position + new Vector2(offset, 5f);
                    PowerBombIcon.Color = Color.White * Opacity;
                    PowerBombIcon.Render();
                }
            }
        }
    }
}
