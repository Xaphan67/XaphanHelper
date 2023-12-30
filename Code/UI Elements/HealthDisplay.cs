using System;
using System.Collections;
using System.Collections.Generic;
using Celeste.Mod.XaphanHelper.Controllers;
using Celeste.Mod.XaphanHelper.Cutscenes;
using Celeste.Mod.XaphanHelper.Entities;
using Celeste.Mod.XaphanHelper.Upgrades;
using FMOD.Studio;
using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste.Mod.XaphanHelper.UI_Elements
{
    [Tracked(true)]
    class HealthDisplay : Entity
    {
        public string Prefix;

        private Player player;

        private float Opacity;

        public float UIWidth;

        public Image LeftFigure;

        public Image RightFigure;

        public int CurrentHealth = 99;

        public int MaxHealth = 99;

        public int DisplayedHealth;

        public bool CannotTakeDamage;

        public bool ColorRed;

        public bool TookDamage;

        public static bool damageSfxPlay;

        public static EventInstance damageSfx;

        public HashSet<Image> EnergyTanks = new();

        public bool PlayerWasKilled;

        public HealthDisplay(Vector2 position) : base(position)
        {
            Tag = (Tags.HUD | Tags.Persistent | Tags.PauseUpdate | Tags.TransitionUpdate);
            LeftFigure = new Image(GFX.Gui["health/0"]);
            RightFigure = new Image(GFX.Gui["health/0"]);
            Depth = -10001;
        }

        public override void Added(Scene scene)
        {
            base.Added(scene);
            Prefix = SceneAs<Level>().Session.Area.LevelSet;
            foreach (KeyValuePair<string, int> maxHealth in XaphanModule.ModSaveData.MaxHealth)
            {
                if (maxHealth.Key == Prefix)
                {
                    MaxHealth = maxHealth.Value;
                    break;
                }
            }
            CurrentHealth = MaxHealth;
            EnergyTanks = GetEnergyTanks();
        }

        public bool liquidFlashRed()
        {
            foreach (Liquid liquid in SceneAs<Level>().Tracker.GetEntities<Liquid>())
            {
                if (liquid.FlashingRed)
                {
                    return true;
                }
            }
            return false;
        }

        public void playDamageSfx()
        {
            if (!damageSfxPlay && !MetroidGameplayController.isLoadingFromSave)
            {
                damageSfxPlay = true;
                damageSfx = Audio.Play("event:/game/xaphan/heat_damage");
            }
        }

        public void stopDamageSfx()
        {
            if (damageSfxPlay)
            {
                Audio.Stop(damageSfx, false);
                damageSfxPlay = false;
            }
        }

        public override void Update()
        {
            base.Update();
            Visible = !SceneAs<Level>().FrozenOrPaused;
            player = SceneAs<Level>().Tracker.GetEntity<Player>();
            if (XaphanModule.UIOpened || (SceneAs<Level>().Tracker.GetEntity<HeatController>() == null && !GravityJacket.determineIfInLiquid()))
            {
                stopDamageSfx();
            }
            HeatController controler = SceneAs<Level>().Tracker.GetEntity<HeatController>();
            if (player != null)
            {
                if (TookDamage)
                {
                    ColorRed = true;
                }
                else if ((controler != null && controler.FlashingRed && !VariaJacket.Active(SceneAs<Level>())) || (liquidFlashRed()))
                {
                    if (Scene.OnRawInterval(0.06f))
                    {
                        ColorRed = !ColorRed;
                    }
                }
                else
                {
                    ColorRed = false;
                }
            }
            if (player != null && player.Center.X < SceneAs<Level>().Camera.Left + 184f && player.Center.Y < SceneAs<Level>().Camera.Top + 52)
            {
                Opacity = Calc.Approach(Opacity, 0.3f, Engine.RawDeltaTime * 3f);
            }
            else
            {
                Opacity = Calc.Approach(Opacity, 1f, Engine.RawDeltaTime * 3f);
            }
            UIWidth = 168f + Math.Min(EnergyTanks.Count, 7) * 40 - (EnergyTanks.Count != 0 ? 2 : 0);
            Math.DivRem(CurrentHealth, 100, out DisplayedHealth);
            string HealthString = DisplayedHealth.ToString();
            if (HealthString.Length == 2)
            {
                LeftFigure = new Image(GFX.Gui["health/" + HealthString[0]]);
                RightFigure = new Image(GFX.Gui["health/" + HealthString[1]]);
            }
            else
            {
                LeftFigure = new Image(GFX.Gui["health/0"]);
                RightFigure = new Image(GFX.Gui["health/" + HealthString[0]]);
            }
            if (CurrentHealth == 0 && player != null && !PlayerWasKilled)
            {
                Add(new Coroutine(KillPlayer(player)));
            }
        }

        public HashSet<Image> GetEnergyTanks()
        {
            EnergyTanks = new HashSet<Image>();
            int remains;
            int TotalEnergyTanks = (MaxHealth - 99) / 100;
            int TotalEnergyTanksFilled = Math.DivRem(CurrentHealth, 100, out remains);
            for (int i = 1; i <= 14; i++)
            {
                if (i <= TotalEnergyTanks)
                {
                    if (i <= TotalEnergyTanksFilled)
                    {
                        EnergyTanks.Add(new Image(GFX.Gui["health/EnergyTankFilled"]));
                    }
                    else
                    {
                        EnergyTanks.Add(new Image(GFX.Gui["health/EnergyTankEmpty"]));
                    }
                }
                else
                {
                    break;
                }
            }
            return EnergyTanks;
        }

        public void UpdateHealth(int value)
        {
            if (value <= 0)
            {
                CannotTakeDamage = true;
                Audio.Play("event:/game/xaphan/take_damage");
                int totalsuits = (VariaJacket.Active(SceneAs<Level>()) ? 1 : 0) + (GravityJacket.Active(SceneAs<Level>()) ? 1 : 0);
                int calcValue = value / (totalsuits == 0 ? 1 : (totalsuits == 1 ? 2 : 4));
                if (CurrentHealth + calcValue < 0)
                {
                    CurrentHealth = 0;
                }
                else
                {
                    CurrentHealth += calcValue;
                }
                Add(new Coroutine(InvincibleCoroutine()));
            }
            else if (value > 0)
            {
                if (CurrentHealth + value > MaxHealth)
                {
                    CurrentHealth = MaxHealth;
                }
                else
                {
                    CurrentHealth += value;
                }
            }
            EnergyTanks = GetEnergyTanks();
        }

        private IEnumerator InvincibleCoroutine()
        {
            Player player = SceneAs<Level>().Tracker.GetEntity<Player>();
            string dir = player.Facing == Facings.Left ? "Left" : "";
            string suit = GravityJacket.Active(SceneAs<Level>()) ? "Gravity" : VariaJacket.Active(SceneAs<Level>()) ? "Varia" : "Power";
            float timer = 1.5f;
            while (timer > 0)
            {
                if (timer > 1.40f && !SceneAs<Level>().Transitioning)
                {
                    TookDamage = true;
                    if (!player.Sprite.LastAnimationID.Contains("morphLoop"))
                    {
                        player.Sprite.Play("jumpSlow");
                    }
                }
                else
                {
                    TookDamage = false;
                }
                if (player != null)
                {
                    if (Engine.Scene.OnRawInterval(0.1f))
                    {
                        player.Visible = false;
                    }
                    else
                    {
                        player.Visible = true;
                    }
                }
                timer -= Engine.DeltaTime;
                yield return null;
            }
            player.Visible = true;
            CannotTakeDamage = false;
        }

        public void AddMaxHealth(int value)
        {
            Prefix = SceneAs<Level>().Session.Area.LevelSet;
            MaxHealth += value;
            CurrentHealth = MaxHealth;
            if (!XaphanModule.ModSaveData.MaxHealth.ContainsKey(Prefix))
            {
                XaphanModule.ModSaveData.MaxHealth.Add(Prefix, MaxHealth);
            }
            else
            {
                XaphanModule.ModSaveData.MaxHealth[Prefix] += value;
            }
            EnergyTanks = GetEnergyTanks();
        }

        public override void Render()
        {
            base.Render();

            Draw.Rect(Position, UIWidth, 88f, Color.Black * 0.65f * Opacity);

            if (LeftFigure != null && RightFigure != null)
            {
                LeftFigure.Position = Position + new Vector2(8f, 8f);
                LeftFigure.Color = (ColorRed ? Color.Red : Color.White) * Opacity;
                LeftFigure.Render();
                RightFigure.Position = Position + new Vector2(88f, 8f);
                RightFigure.Color = (ColorRed ? Color.Red : Color.White) * Opacity;
                RightFigure.Render();
            }

            int Col = 0;
            int EnergyTankIndex = 0;
            foreach (Image EnergyTank in EnergyTanks)
            {
                EnergyTank.Position = Position + (new Vector2(168f + Col, 9 + (EnergyTankIndex >= 7 ? 40f : 0f)));
                EnergyTank.Color = Color.White * Opacity;
                EnergyTank.Render();
                EnergyTankIndex += 1;
                Col += 40;
                if (EnergyTankIndex == 7)
                {
                    Col = 0;
                }
            }
        }

        public IEnumerator KillPlayer(Player player)
        {
            PlayerWasKilled = true;
            Level Level = Engine.Scene as Level;
            player.StateMachine.State = 11;
            player.DummyAutoAnimate = false;
            Level.PauseLock = true;
            Level.Displacement.AddBurst(Position, 0.3f, 0f, 80f);
            Level.Shake();
            Input.Rumble(RumbleStrength.Strong, RumbleLength.Long);
            Audio.Play("event:/char/madeline/death", Position);
            CustomDeathEffect deathEffect = new(player.Hair.Color, player.Center);
            deathEffect.OnUpdate = delegate (float f)
            {
                player.Light.Alpha = 1f - f;
            };
            Level.Add(deathEffect);
            Level.Session.Deaths++;
            Level.Session.DeathsInCurrentLevel++;
            SaveData.Instance.AddDeath(Level.Session.Area);
            float timer = 0.5f;
            while (timer > 0)
            {
                timer -= Engine.DeltaTime;
                player.Speed = Vector2.Zero;
                player.Sprite.Visible = false;
                yield return null;
            }
            string DestinationRoom = XaphanModule.ModSaveData.SavedRoom[Level.Session.Area.LevelSet];
            int chapterIndex = XaphanModule.ModSaveData.SavedChapter[Level.Session.Area.LevelSet];
            if (chapterIndex == (Level.Session.Area.ChapterIndex == -1 ? 0 : Level.Session.Area.ChapterIndex))
            {
                Vector2 spawnPoint = Vector2.Zero;
                AreaKey area = Level.Session.Area;
                MapData MapData = AreaData.Areas[area.ID].Mode[(int)area.Mode].MapData;
                foreach (LevelData levelData in MapData.Levels)
                {
                    if (levelData.Name == DestinationRoom)
                    {
                        foreach (EntityData entity in levelData.Entities)
                        {
                            if (entity.Name == "XaphanHelper/SaveStation")
                            {
                                spawnPoint = new Vector2(entity.Position.X, entity.Position.Y);
                            }
                        }
                        break;
                    }
                }
                MetroidGameplayController.IsLoadingFromSave(true);
                Level.Add(new TeleportCutscene(player, DestinationRoom, spawnPoint, 0, 0, true, 0f, "Fade", 0.75f, false, false));
            }
        }
    }
}
