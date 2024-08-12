using System;
using System.Collections.Generic;
using System.Reflection;
using Celeste.Mod.XaphanHelper.Entities;
using Celeste.Mod.XaphanHelper.Managers;
using Celeste.Mod.XaphanHelper.Upgrades;
using Microsoft.Xna.Framework;
using Monocle;
using MonoMod.Cil;
using MonoMod.RuntimeDetour;
using MonoMod.Utils;

namespace Celeste.Mod.XaphanHelper.UI_Elements
{
    [Tracked(true)]
    class UpgradesDisplay : Entity
    {
        private static ILHook playerUpdateHook;

        private static ILHook summitGemSmashRoutineHook;

        private MTexture missileIcon = GFX.Gui["upgrades/ammo/missile"];

        private MTexture superMissileIcon = GFX.Gui["upgrades/ammo/superMissile"];

        private Image section = new(GFX.Gui["upgrades/stamina/section"]);

        public HashSet<Image> Sections = new();

        public static Player player;

        public int TotalSections;

        public static float BaseStamina;

        public static bool ShowStaminaBar;

        public static string Prefix;

        public int CurrentMissiles;

        public int CurrentSuperMissiles;

        public bool MissileSelected;

        public bool SuperMissileSelected;

        private bool HasMissilesUpgrade;

        private bool HasSuperMissilesUpgrade;

        public bool ResetSelectedAmmo;

        private float Opacity;

        private Color borderColor;

        private int sectionSpacing;

        private float width;

        private float height;

        VirtualButton Button = new();

        private MTexture buttonTexture;

        private bool ShowAmmo;

        public static void getStaminaData(Level level)
        {
            AreaKey area = level.Session.Area;
            MapData MapData = AreaData.Areas[area.ID].Mode[(int)area.Mode].MapData;
            string entity = "XaphanHelper/UpgradeController";
            if (MapData.HasEntity(entity))
            {
                BaseStamina = MapData.GetEntityData(entity).Float("baseStamina", 110);
                ShowStaminaBar = MapData.GetEntityData(entity).Bool("showStaminaBar", false);
                Prefix = area.LevelSet;
            }
        }

        public UpgradesDisplay()
        {
            Tag = (Tags.HUD | Tags.Persistent | Tags.PauseUpdate);
            borderColor = Calc.HexToColor("262626");
            ButtonBinding Control = XaphanModule.ModSettings.SelectItem;
            Button.Binding = Control.Binding;
            buttonTexture = Input.GuiButton(Button, "controls/keyboard/oemquestion");
            Depth = -99;
        }

        public static void Load()
        {
            Everest.Events.Level.OnLoadLevel += onLevelLoad;
            IL.Celeste.Player.ClimbUpdate += patchOutStamina;
            IL.Celeste.Player.SwimBegin += patchOutStamina;
            IL.Celeste.Player.DreamDashBegin += patchOutStamina;
            IL.Celeste.Player.ctor += patchOutStamina;
            On.Celeste.Player.RefillStamina += modRefillStamina;
            playerUpdateHook = new ILHook(typeof(Player).GetMethod("orig_Update"), patchOutStamina);
            summitGemSmashRoutineHook = new ILHook(typeof(SummitGem).GetMethod("SmashRoutine", BindingFlags.NonPublic | BindingFlags.Instance).GetStateMachineTarget(), patchOutStamina);
            On.Celeste.Level.Update += onLevelUpdate;
            On.Celeste.TotalStrawberriesDisplay.Update += onTotalStrawberriesDisplayUpdate;
        }

        public static void Unload()
        {
            Everest.Events.Level.OnLoadLevel -= onLevelLoad;
            IL.Celeste.Player.ClimbUpdate -= patchOutStamina;
            IL.Celeste.Player.SwimBegin -= patchOutStamina;
            IL.Celeste.Player.DreamDashBegin -= patchOutStamina;
            IL.Celeste.Player.ctor -= patchOutStamina;
            On.Celeste.Player.RefillStamina -= modRefillStamina;
            if (playerUpdateHook != null) playerUpdateHook.Dispose();
            if (summitGemSmashRoutineHook != null) summitGemSmashRoutineHook.Dispose();
            On.Celeste.Level.Update -= onLevelUpdate;
            On.Celeste.TotalStrawberriesDisplay.Update -= onTotalStrawberriesDisplayUpdate;
        }

        private static void onLevelLoad(Level level, Player.IntroTypes playerIntro, bool isFromLoader)
        {
            if (XaphanModule.useUpgrades && !XaphanModule.useMetroidGameplay)
            {
                getStaminaData(level);
            }
        }

        private static void patchOutStamina(ILContext il)
        {
            ILCursor cursor = new(il);
            while (cursor.TryGotoNext(MoveType.After, instr => instr.MatchLdcR4(110f)))
            {
                cursor.EmitDelegate<Func<float, float>>(orig =>
                {
                    if (XaphanModule.useUpgrades && !XaphanModule.useMetroidGameplay)
                    {
                        return determineBaseStamina();
                    }
                    return orig;
                });
            }
        }

        private static void modRefillStamina(On.Celeste.Player.orig_RefillStamina orig, Player self)
        {
            orig.Invoke(self);
            if (XaphanModule.useUpgrades && !XaphanModule.useMetroidGameplay)
            {
                self.Stamina = determineBaseStamina();
            }
        }

        private static float determineBaseStamina()
        {
            int ExtraStamina = 0;
            if (Engine.Scene is Level)
            {
                foreach (string upgrade in XaphanModule.PlayerHasGolden ? XaphanModule.ModSaveData.GoldenStrawberryStaminaUpgrades : XaphanModule.ModSaveData.StaminaUpgrades)
                {
                    if (upgrade.Contains(Prefix))
                    {
                        ExtraStamina += 5;
                    }
                }
            }
            return BaseStamina + ExtraStamina;
        }

        private static void onLevelUpdate(On.Celeste.Level.orig_Update orig, Level self)
        {
            orig(self);
            if (XaphanModule.useUpgrades && !XaphanModule.useMetroidGameplay)
            {
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
                    if (self.Tracker.GetEntity<UpgradesDisplay>() != null)
                    {
                        self.Tracker.GetEntity<UpgradesDisplay>().RemoveSelf();
                    }
                }
                else
                {
                    if (self.Tracker.GetEntity<UpgradesDisplay>() == null)
                    {
                        self.Add(new UpgradesDisplay());
                    }
                }
            }
        }

        private static void onTotalStrawberriesDisplayUpdate(On.Celeste.TotalStrawberriesDisplay.orig_Update orig, TotalStrawberriesDisplay self)
        {
            orig(self);
            if (Engine.Scene is Level)
            {
                Level level = (Level)Engine.Scene;
                if (XaphanModule.useUpgrades && ShowStaminaBar)
                {
                    float num = self.Y = 154f;
                    if (!level.TimerHidden)
                    {
                        if (Settings.Instance.SpeedrunClock == SpeedrunType.File)
                        {
                            num += 20f;
                        }
                    }
                    self.Y = num;
                }
            }
        }

        public void SetXPosition()
        {
            if (player != null)
            {
                TotalSections = (int)determineBaseStamina() / 5;
                Sections = GetSections();
            }
            Drone drone = SceneAs<Level>().Tracker.GetEntity<Drone>();
            if (!XaphanModule.PlayerIsControllingRemoteDrone())
            {
                width = Sections.Count * (section.Width + sectionSpacing) + 22f - sectionSpacing;
            }
            else
            {
                if (drone != null && drone.canDestroy)
                {
                    width = 160f;
                }
                HasMissilesUpgrade = MissilesModule.Active(SceneAs<Level>());
                HasSuperMissilesUpgrade = SuperMissilesModule.Active(SceneAs<Level>());
                height = ((HasMissilesUpgrade && !HasSuperMissilesUpgrade) || (HasSuperMissilesUpgrade && !HasMissilesUpgrade)) ? 54f : 96f;
                if (Opacity <= 0.05f && !drone.dead)
                {
                    Position.X = 1920f - (XaphanModule.minimapEnabled ? 222f : 0f) - 27f - width;
                }
            }
            if (drone != null ? !drone.enabled && Opacity <= 0.5f : true)
            {
                int BagDisplays = SceneAs<Level>().Tracker.GetEntities<BagDisplay>().Count;
                Position.X = 1920f - (XaphanModule.minimapEnabled ? 222f : 0f) - 27f - width - BagDisplays * 120f;
            }
        }

        public override void Added(Scene scene)
        {
            base.Added(scene);
            sectionSpacing = 2;
            SetXPosition();
            Position.Y = 26f;
        }

        public void UpdateOpacity()
        {
            int BagDisplays = SceneAs<Level>().Tracker.GetEntities<BagDisplay>().Count;
            if (player != null && player.Center.X > SceneAs<Level>().Camera.Right - ((ShowAmmo ? 52f : (BagDisplays == 2 ? 64f : 32f)) + width / 6) && player.Center.Y < SceneAs<Level>().Camera.Top + 52)
            {
                Opacity = Calc.Approach(Opacity, 0.3f, Engine.RawDeltaTime * 3f);
            }
            else
            {
                Opacity = Calc.Approach(Opacity, 1f, Engine.RawDeltaTime * 3f);
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
            Drone drone = SceneAs<Level>().Tracker.GetEntity<Drone>();
            ShowAmmo = (drone != null && !Drone.Hold.IsHeld && drone.canDestroy);
            if (drone != null && !Drone.Hold.IsHeld)
            {
                if (!drone.canDestroy || drone.dead || !drone.enabled)
                {
                    Opacity = Calc.Approach(Opacity, 0f, Engine.RawDeltaTime * 3f);
                }
                else
                {
                    UpdateOpacity();
                }
            }
            else
            {
                UpdateOpacity();
            }
            if (!SceneAs<Level>().Paused && !SceneAs<Level>().PauseLock && XaphanModule.PlayerIsControllingRemoteDrone())
            {
                HasMissilesUpgrade = MissilesModule.Active(SceneAs<Level>());
                HasSuperMissilesUpgrade = SuperMissilesModule.Active(SceneAs<Level>());
                bool NoneSelected = !MissileSelected && !SuperMissileSelected;
                if (NoneSelected && XaphanModule.ModSettings.SelectItem.Pressed)
                {
                    if (HasMissilesUpgrade && CurrentMissiles > 0)
                    {
                        MissileSelected = true;
                    }
                    else if (HasSuperMissilesUpgrade && CurrentSuperMissiles > 0)
                    {
                        SuperMissileSelected = true;
                    }
                }
                else if (MissileSelected && XaphanModule.ModSettings.SelectItem.Pressed)
                {
                    MissileSelected = false;
                    if (HasSuperMissilesUpgrade && CurrentSuperMissiles > 0)
                    {
                        SuperMissileSelected = true;
                    }
                }
                else if (SuperMissileSelected && XaphanModule.ModSettings.SelectItem.Pressed)
                {
                    SuperMissileSelected = false;
                }
                if (MissileSelected && CurrentMissiles == 0)
                {
                    MissileSelected = false;
                    Audio.Play("event:/game/xaphan/item_select");
                }
                else if (SuperMissileSelected && CurrentSuperMissiles == 0)
                {
                    SuperMissileSelected = false;
                    Audio.Play("event:/game/xaphan/item_select");
                }
                if (!ResetSelectedAmmo && (MissileSelected || SuperMissileSelected))
                {
                    if (MissileSelected)
                    {
                        XaphanModule.ModSession.CurrentAmmoSelected = 1;
                    }
                    else if (SuperMissileSelected)
                    {
                        XaphanModule.ModSession.CurrentAmmoSelected = 2;
                    }
                }
                else
                {
                    MissileSelected = false;
                    SuperMissileSelected = false;
                    XaphanModule.ModSession.CurrentAmmoSelected = 0;
                }
                if (((HasMissilesUpgrade && CurrentMissiles > 0) || (HasSuperMissilesUpgrade && CurrentSuperMissiles > 0)) && XaphanModule.ModSettings.SelectItem.Pressed)
                {
                    Audio.Play("event:/game/xaphan/item_select");
                }
            }
        }

        private HashSet<Image> GetSections()
        {
            Sections = new HashSet<Image>();
            if (player != null)
            {
                for (int i = 1; i <= TotalSections; i++)
                {
                    if ((int)Math.Ceiling(player.Stamina / 5) >= i)
                    {
                        Sections.Add(new Image(GFX.Gui["upgrades/stamina/section" + (i <= 4 ? "Low" : "")]));
                        continue;
                    }
                    Sections.Add(new Image(GFX.Gui["upgrades/stamina/sectionEmpty"]));
                }
            }
            return Sections;
        }

        public override void Render()
        {
            base.Render();
            if (ShowStaminaBar && PowerGrip.isActive && !ShowAmmo)
            {
                Draw.Rect(Position + new Vector2(2), width, 46f, Color.Black * 0.85f * Opacity);
                string name = Dialog.Clean("Xaphanhelper_UI_Stamina");
                ActiveFont.DrawOutline(name, Position + new Vector2((width + 4f) / 2f, 0f), new Vector2(0.5f, 0.5f), Vector2.One * 0.3f, Color.Yellow * Opacity, 2f, Color.Black * Opacity);
                float nameLenght = ActiveFont.Measure(name).X * 0.3f;

                Draw.Rect(Position, (width + 4f) / 2f - nameLenght / 2 - 10f, 2f, borderColor * Opacity);
                Draw.Rect(Position + new Vector2((width + 4f) / 2f, 0f) + new Vector2(nameLenght / 2 + 11f, 0), (width + 4f) / 2f - nameLenght / 2 - 10f, 2f, borderColor * Opacity);
                Draw.Rect(Position + new Vector2(0f, 2f), 2f, 46f, borderColor * Opacity);
                Draw.Rect(Position + new Vector2(width + 2f, 2f), 2f, 46f, borderColor * Opacity);
                Draw.Rect(Position + new Vector2(0f, 46f + 2f), width + 4f, 2f, borderColor * Opacity);

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
            else if (ShowAmmo && (HasMissilesUpgrade || HasSuperMissilesUpgrade))
            {
                Draw.Rect(Position + new Vector2(2), width, height, Color.Black * 0.85f * Opacity);
                string name = Dialog.Clean("Xaphanhelper_UI_Ammo");
                ActiveFont.DrawOutline(name, Position + new Vector2((width + 4f) / 2f, 0f), new Vector2(0.5f, 0.5f), Vector2.One * 0.3f, Color.Yellow * Opacity, 2f, Color.Black * Opacity);
                float nameLenght = ActiveFont.Measure(name).X * 0.3f;

                Draw.Rect(Position, (width + 4f) / 2f - nameLenght / 2 - 10f, 2f, borderColor * Opacity);
                Draw.Rect(Position + new Vector2((width + 4f) / 2f, 0f) + new Vector2(nameLenght / 2 + 10f, 0), (width + 4f) / 2f - nameLenght / 2 - 10f, 2f, borderColor * Opacity);
                Draw.Rect(Position + new Vector2(0f, 2f), 2f, height, borderColor * Opacity);
                Draw.Rect(Position + new Vector2(width + 2f, 2f), 2f, height, borderColor * Opacity);
                Draw.Rect(Position + new Vector2(0f, height + 2f), width + 4f, 2f, borderColor * Opacity);

                Vector2 missileIconPos = Vector2.Zero;
                Vector2 superMissileIconPos = Vector2.Zero;

                if (HasMissilesUpgrade)
                {
                    missileIconPos = Position + new Vector2(13f, 13f);
                }
                if (HasSuperMissilesUpgrade && !HasMissilesUpgrade)
                {
                    superMissileIconPos = Position + new Vector2(13f, 13f);
                }
                else if (HasMissilesUpgrade && HasSuperMissilesUpgrade)
                {
                    superMissileIconPos = Position + new Vector2(13f, 55f);
                }

                if (MissileSelected)
                {
                    Draw.Rect(missileIconPos - new Vector2(4), 146f, missileIcon.Height + 8f, Color.Green * 0.85f * Opacity);
                }
                if (SuperMissileSelected)
                {
                    Draw.Rect(superMissileIconPos - new Vector2(4), 146f, superMissileIcon.Height + 8f, Color.Green * 0.85f * Opacity);
                }

                string currentMissiles = CurrentMissiles.ToString();
                string currentSuperMissiles = CurrentSuperMissiles.ToString();

                if (HasMissilesUpgrade)
                {
                    missileIcon.Draw(missileIconPos, Vector2.Zero, MissileSelected ? Color.Gold * Opacity : Color.White * Opacity);
                    MTexture FirstFigure = GFX.Gui["upgrades/ammo/0"];
                    MTexture SecondFigure;
                    if (currentMissiles.Length == 2)
                    {
                        FirstFigure = GFX.Gui["upgrades/ammo/" + currentMissiles[0]];
                        SecondFigure = GFX.Gui["upgrades/ammo/" + currentMissiles[1]];
                    }
                    else
                    {
                        SecondFigure = GFX.Gui["upgrades/ammo/" + currentMissiles[0]];
                    }
                    FirstFigure.Draw(Position + new Vector2(missileIcon.Width + 13f + 34f, missileIconPos.Y - 22f), Vector2.Zero, MissileSelected ? Color.Gold * Opacity : Color.White * Opacity);
                    SecondFigure.Draw(Position + new Vector2(missileIcon.Width + 13f + 34f + 28f, missileIconPos.Y - 22f), Vector2.Zero, MissileSelected ? Color.Gold * Opacity : Color.White * Opacity);
                }
                if ((HasSuperMissilesUpgrade && !HasMissilesUpgrade) || (HasMissilesUpgrade && HasSuperMissilesUpgrade))
                {
                    superMissileIcon.Draw(superMissileIconPos, Vector2.Zero, SuperMissileSelected ? Color.Gold * Opacity : Color.White * Opacity);
                }
                if (HasSuperMissilesUpgrade)
                {
                    MTexture FirstFigure = GFX.Gui["upgrades/ammo/0"];
                    MTexture SecondFigure;
                    if (currentSuperMissiles.Length == 2)
                    {
                        FirstFigure = GFX.Gui["upgrades/ammo/" + currentSuperMissiles[0]];
                        SecondFigure = GFX.Gui["upgrades/ammo/" + currentSuperMissiles[1]];
                    }
                    else
                    {
                        SecondFigure = GFX.Gui["upgrades/ammo/" + currentSuperMissiles[0]];
                    }
                    FirstFigure.Draw(Position + new Vector2(superMissileIcon.Width + 13f + 34f, superMissileIconPos.Y - 22f), Vector2.Zero, SuperMissileSelected ? Color.Gold * Opacity : Color.White * Opacity);
                    SecondFigure.Draw(Position + new Vector2(superMissileIcon.Width + 13f + 34f + 28f, superMissileIconPos.Y - 22f), Vector2.Zero, SuperMissileSelected ? Color.Gold * Opacity : Color.White * Opacity);
                }

                if (buttonTexture != null)
                {
                    buttonTexture.DrawCentered(Position + new Vector2(width / 2, height + 5f), Color.White * Opacity, 0.4f);
                }
            }
        }
    }
}
