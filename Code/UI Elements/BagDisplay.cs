using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using Celeste.Mod.XaphanHelper.Data;
using Celeste.Mod.XaphanHelper.Entities;
using Celeste.Mod.XaphanHelper.Managers;
using Celeste.Mod.XaphanHelper.Triggers;
using Celeste.Mod.XaphanHelper.Upgrades;
using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste.Mod.XaphanHelper.UI_Elements
{
    [Tracked(true)]
    public class BagDisplay : Entity
    {
        private static MethodInfo Player_Pickup = typeof(Player).GetMethod("Pickup", BindingFlags.Instance | BindingFlags.NonPublic);

        private Level level;

        public string type;

        private static Player player;

        private Sprite Sprite;

        private Sprite Upgrade1;

        private Sprite Upgrade2;

        private Sprite Upgrade3;

        public float Opacity;

        public int currentSelection;

        private Color borderColor;

        private static bool BagDisplayAdded;

        private static bool MiscDisplayAdded;

        private static bool canAddDisplay;

        private MTexture buttonTexture;

        public List<CustomUpgradesData> CustomUpgradesData = new();

        VirtualButton SlotButton = new();

        VirtualButton SelectButton = new();

        private CustomTutorialUI tutorialGui;

        private float tutorialTimer = 0f;

        private bool tutorial;

        public bool preventTutorialDisplay;

        public int totalDisplays;

        private bool isFading;

        public bool shownTutorial;

        private Coroutine CrossRoutine = new();

        private bool drawCross;

        private int totalActiveUpgrades = 0;

        private float selectedAlpha = 0;

        private int alphaStatus = 0;

        public BagDisplay(Level level, string type)
        {
            this.level = level;
            this.type = type;
            Tag = (Tags.HUD | Tags.Persistent | Tags.PauseUpdate);
            Position = new Vector2(-200f, 26f);
            Depth = -10002;
            Sprite = new Sprite(GFX.Gui, "");
            Upgrade1 = new Sprite(GFX.Gui, "");
            Upgrade2 = new Sprite(GFX.Gui, "");
            Upgrade3 = new Sprite(GFX.Gui, "");
            Sprite.Scale = Upgrade1.Scale = Upgrade2.Scale = Upgrade3.Scale = new Vector2(0.15f);
            borderColor = Calc.HexToColor("262626");
            ButtonBinding Control = type == "bag" ? XaphanModule.ModSettings.UseBagItemSlot : XaphanModule.ModSettings.UseMiscItemSlot;
            SlotButton.Binding = Control.Binding;
            buttonTexture = Input.GuiButton(SlotButton, "controls/keyboard/oemquestion");
            ButtonBinding SelectControl = XaphanModule.ModSettings.SelectItem;
            SelectButton.Binding = SelectControl.Binding;
        }

        public static void Load()
        {
            On.Celeste.Level.Update += modLevelUpdate;
            On.Celeste.TalkComponent.Update += modTalkComponentUpdate;
            On.Celeste.Player.ClimbCheck += modPlayerClimbCheck;
            On.Celeste.Player.NormalUpdate += modPlayerNormalUpdate;
            On.Celeste.Player.Throw += modPlayerThrow;
        }

        private static bool modPlayerClimbCheck(On.Celeste.Player.orig_ClimbCheck orig, Player self, int dir, int yAdd)
        {
            if (XaphanModule.useUpgrades)
            {
                if ((XaphanModule.ModSettings.UseBagItemSlot.Pressed || XaphanModule.ModSettings.UseMiscItemSlot.Pressed) && Input.MoveX == 0)
                {
                    return false;
                }
            }
            return orig(self, dir, yAdd);
        }

        public static void Unload()
        {
            On.Celeste.Level.Update -= modLevelUpdate;
            On.Celeste.TalkComponent.Update -= modTalkComponentUpdate;
            On.Celeste.Player.ClimbCheck -= modPlayerClimbCheck;
            On.Celeste.Player.NormalUpdate -= modPlayerNormalUpdate;
            On.Celeste.Player.Throw -= modPlayerThrow;
        }

        private static void modLevelUpdate(On.Celeste.Level.orig_Update orig, Level self)
        {
            orig(self);
            if (XaphanModule.useUpgrades)
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
                Drone drone = self.Tracker.GetEntity<Drone>();
                if ((self.FrozenOrPaused || self.RetryPlayerCorpse != null || self.SkippingCutscene || self.InCutscene) || (player != null && !player.Sprite.Visible && !self.Session.GetFlag("Xaphan_Helper_Ceiling") && !sliding && (self.Tracker.GetEntity<ScrewAttackManager>() != null ? !self.Tracker.GetEntity<ScrewAttackManager>().StartedScrewAttack : true)) || XaphanModule.ShowUI || playerIsInHideTrigger(self))
                {
                    if (canAddDisplay && self.Tracker.CountEntities<BagDisplay>() > 0)
                    {
                        self.Remove(Upgrade.GetDisplay(self, "bag"));
                        self.Remove(Upgrade.GetDisplay(self, "misc"));
                        BagDisplayAdded = false;
                        MiscDisplayAdded = false;
                    }
                    canAddDisplay = false;
                }
                else
                {
                    canAddDisplay = true;
                }
                if (canAddDisplay)
                {
                    if ((Bombs.isActive || MegaBombs.isActive || RemoteDrone.isActive) && self.Tracker.CountEntities<BagDisplay>() == 0)
                    {
                        BagDisplayAdded = false;
                    }
                    if ((Bombs.isActive || MegaBombs.isActive || RemoteDrone.isActive) && !BagDisplayAdded)
                    {
                        BagDisplayAdded = true;
                        self.Add(new BagDisplay(self, "bag"));
                    }
                    else if (!Bombs.isActive && !MegaBombs.isActive && !RemoteDrone.isActive && BagDisplayAdded)
                    {
                        BagDisplayAdded = false;
                        self.Remove(Upgrade.GetDisplay(self, "bag"));
                    }
                    if ((Binoculars.isActive || PortableStation.isActive || PulseRadar.isActive) && self.Tracker.CountEntities<BagDisplay>() == 0)
                    {
                        MiscDisplayAdded = false;
                    }
                    if ((Binoculars.isActive || PortableStation.isActive || PulseRadar.isActive) && !MiscDisplayAdded)
                    {
                        MiscDisplayAdded = true;
                        self.Add(new BagDisplay(self, "misc"));
                    }
                    else if (!Binoculars.isActive && !PortableStation.isActive && !PulseRadar.isActive && MiscDisplayAdded)
                    {
                        MiscDisplayAdded = false;
                        self.Remove(Upgrade.GetDisplay(self, "misc"));
                    }
                }
            }
        }

        private static void modTalkComponentUpdate(On.Celeste.TalkComponent.orig_Update orig, TalkComponent self)
        {
            BagDisplay display = self.SceneAs<Level>().Tracker.GetEntity<BagDisplay>();
            if (display == null || (display != null && !XaphanModule.ModSettings.UseBagItemSlot.Check && !XaphanModule.ModSettings.UseMiscItemSlot.Check))
            {
                orig(self);
            }
        }

        private static int modPlayerNormalUpdate(On.Celeste.Player.orig_NormalUpdate orig, Player self)
        {
            if (XaphanModule.useUpgrades)
            {
                if (self.Holding == null)
                {
                    if (XaphanModule.ModSettings.UseBagItemSlot.Check && !self.Ducking && Input.Aim.Value.Y <= 0)
                    {
                        foreach (Holdable component in self.Scene.Tracker.GetComponents<Holdable>())
                        {
                            if (component.Entity.GetType() == typeof(Bomb) || component.Entity.GetType() == typeof(MegaBomb) || component.Entity.GetType() == typeof(Drone))
                            {
                                if (component.Check(self) && (bool)Player_Pickup.Invoke(self, new object[] { component }))
                                {
                                    return 8;
                                }
                            }
                        }
                    }
                }
            }
            return orig(self);
        }

        private static void modPlayerThrow(On.Celeste.Player.orig_Throw orig, Player self)
        {
            if (!XaphanModule.useUpgrades || (XaphanModule.useUpgrades && (self.Holding.Entity.GetType() != typeof(Bomb) && self.Holding.Entity.GetType() != typeof(MegaBomb) && self.Holding.Entity.GetType() != typeof(Drone))))
            {
                orig(self);
            }
        }

        public void SetXPosition()
        {
            if (!isFading)
            {
                totalDisplays = level.Tracker.GetEntities<BagDisplay>().Count;
                if (XaphanModule.minimapEnabled && level.Tracker.GetEntity<MiniMap>() != null)
                {
                    if (totalDisplays == 2 && type == "bag")
                    {
                        Position.X = 1455f;
                    }
                    else
                    {
                        Position.X = 1575f;
                    }
                }
                else
                {
                    if (totalDisplays == 2 && type == "bag")
                    {
                        Position.X = 1670f;
                    }
                    else
                    {
                        Position.X = 1790f;
                    }
                }
            }
        }

        public override void Added(Scene scene)
        {
            base.Added(scene);
            SetXPosition();
            foreach (string VisitedChapter in XaphanModule.ModSaveData.VisitedChapters)
            {
                string[] str = VisitedChapter.Split('_');
                if (str[0] == level.Session.Area.LevelSet)
                {
                    GetCustomUpgradesData(int.Parse(str[1].Remove(0, 2)), int.Parse(str[2]));
                }
            }
            if (type == "bag")
            {
                Sprite.AddLoop("bombs", getCustomSpritePath("Bombs") + "/bombs", 0.08f, 0);
                Upgrade1.AddLoop("upg", getCustomSpritePath("Bombs") + "/bombs", 0.08f, 0);
                Sprite.AddLoop("megaBombs", getCustomSpritePath("MegaBombs") + "/megaBombs", 0.08f, 0);
                Upgrade2.AddLoop("upg", getCustomSpritePath("MegaBombs") + "/megaBombs", 0.08f, 0);
                Sprite.AddLoop("remoteDrone", getCustomSpritePath("RemoteDrone") + "/remoteDrone", 0.08f, 0);
                Upgrade3.AddLoop("upg", getCustomSpritePath("RemoteDrone") + "/remoteDrone", 0.08f, 0);
                if (XaphanModule.ModSaveData.BagUIId1 == 0)
                {
                    if (Bombs.isActive)
                    {
                        currentSelection = 1;
                    }
                    else if (MegaBombs.isActive)
                    {
                        currentSelection = 2;
                    }
                    else if (RemoteDrone.isActive)
                    {
                        currentSelection = 3;
                    }
                }
                else
                {
                    currentSelection = XaphanModule.ModSaveData.BagUIId1;
                }
            }
            else
            {
                Sprite.AddLoop("binoculars", getCustomSpritePath("Binoculars") + "/binoculars", 0.08f, 0);
                Upgrade1.AddLoop("upg", getCustomSpritePath("Binoculars") + "/binoculars", 0.08f, 0);
                Sprite.AddLoop("portableStation", getCustomSpritePath("PortableStation") + "/portableStation", 0.08f, 0);
                Upgrade2.AddLoop("upg", getCustomSpritePath("PortableStation") + "/portableStation", 0.08f, 0);
                Sprite.AddLoop("pulseRadar", getCustomSpritePath("PulseRadar") + "/pulseRadar", 0.08f, 0);
                Upgrade3.AddLoop("upg", getCustomSpritePath("PulseRadar") + "/pulseRadar", 0.08f, 0);
                if (XaphanModule.ModSaveData.BagUIId2 == 0)
                {
                    if (Binoculars.isActive)
                    {
                        currentSelection = 1;
                    }
                    else if (PortableStation.isActive)
                    {
                        currentSelection = 2;
                    }
                    else if (PulseRadar.isActive)
                    {
                        currentSelection = 3;
                    }
                }
                else
                {
                    currentSelection = XaphanModule.ModSaveData.BagUIId2;
                }
            }
        }

        private void GetCustomUpgradesData(int chapter, int mode)
        {
            AreaKey area = level.Session.Area;
            MapData MapData = AreaData.Areas[area.ID - (area.ChapterIndex == -1 ? 0 : area.ChapterIndex) + chapter].Mode[mode].MapData;
            foreach (LevelData LevelData in MapData.Levels)
            {
                foreach (EntityData entity in LevelData.Entities)
                {
                    if (entity.Name == "XaphanHelper/UpgradeCollectable")
                    {
                        CustomUpgradesData.Add(new CustomUpgradesData(entity.Attr("upgrade"), entity.Attr("customName"), entity.Attr("customSprite")));
                    }
                }
            }
        }

        public string getCustomSpritePath(string upgrade)
        {
            foreach (CustomUpgradesData UpgradesData in CustomUpgradesData)
            {
                if (UpgradesData.Upgrade == upgrade)
                {
                    if (!string.IsNullOrEmpty(UpgradesData.CustomSpritePath))
                    {
                        return UpgradesData.CustomSpritePath;
                    }
                }
            }
            return "collectables/XaphanHelper/UpgradeCollectable";
        }

        public override void Update()
        {
            base.Update();
            player = level.Tracker.GetEntity<Player>();
            totalDisplays = level.Tracker.GetEntities<BagDisplay>().Count;
            SetXPosition();
            Drone drone = SceneAs<Level>().Tracker.GetEntity<Drone>();
            if (drone != null && !Drone.Hold.IsHeld)
            {
                Opacity = Calc.Approach(Opacity, 0f, Engine.RawDeltaTime * 3f);
                isFading = true;
            }
            else
            {
                isFading = false;
                if (player != null && player.Center.X > level.Camera.Right - (totalDisplays == 2 ? 96f : 64f) && player.Center.Y < level.Camera.Top + 52)
                {
                    Opacity = Calc.Approach(Opacity, 0.3f, Engine.RawDeltaTime * 3f);
                }
                else
                {
                    Opacity = Calc.Approach(Opacity, 1f, Engine.RawDeltaTime * 3f);
                }
            }
            if (!preventTutorialDisplay)
            {
                if (tutorialGui == null)
                {
                    tutorialGui = new CustomTutorialUI(Position + new Vector2(50, 425f), Dialog.Clean("XaphanHelper_SwitchAbility"), Dialog.Clean("XaphanHelper_Hold"), SelectButton, "{n}", Dialog.Clean("XaphanHelper_AndPress"), SlotButton);
                    tutorialGui.Open = false;
                    Scene.Add(tutorialGui);
                }
                else if (!tutorialGui.Open && tutorial)
                {
                    int totalActiveUpgrades = 0;
                    for (int i = 1; i <= 3; i++)
                    {
                        if (CheckIfUpgradeIsActive(i))
                        {
                            totalActiveUpgrades++;
                        }
                    }
                    if (totalActiveUpgrades > 1)
                    {
                        tutorialTimer += Engine.DeltaTime;
                        tutorialGui.Open = tutorialTimer > 0.25f;
                        shownTutorial = true;
                    }
                }
                else if (tutorialGui.Open && !tutorial)
                {
                    tutorialGui.Open = false;
                }
            }

            if (XaphanModule.ModSettings.SelectItem.Check && !XaphanModule.PlayerIsControllingRemoteDrone())
            {
                if (alphaStatus == 0 || (alphaStatus == 1 && selectedAlpha != 0.9f))
                {
                    alphaStatus = 1;
                    selectedAlpha = Calc.Approach(selectedAlpha, 0.9f, Engine.DeltaTime);
                    if (selectedAlpha == 0.9f)
                    {
                        alphaStatus = 2;
                    }
                }
                if (alphaStatus == 2 && selectedAlpha != 0.1f)
                {
                    selectedAlpha = Calc.Approach(selectedAlpha, 0.1f, Engine.DeltaTime);
                    if (selectedAlpha == 0.1f)
                    {
                        alphaStatus = 1;
                    }
                }
                if (totalActiveUpgrades == 0)
                {
                    totalActiveUpgrades = CheckTotalUpgradesActive();
                }
            }
            else
            {
                totalActiveUpgrades = 0;
                if (Upgrade1 != null && Upgrade2 != null && Upgrade3 != null)
                {
                    Upgrade1.Visible = Upgrade2.Visible = Upgrade3.Visible = false;
                }
                alphaStatus = 0;
                selectedAlpha = 0;
            }
            if ((type == "bag" ? XaphanModule.ModSettings.UseBagItemSlot.Pressed : XaphanModule.ModSettings.UseMiscItemSlot.Pressed) && XaphanModule.ModSettings.SelectItem.Check)
            {
                if (player != null && Visible && !level.Paused && !XaphanModule.PlayerIsControllingRemoteDrone() && player.Holding == null && !XaphanModule.UIOpened)
                {
                    int nextSelection = currentSelection;
                    bool nextActiveUpgrade = false;
                    while (!nextActiveUpgrade)
                    {
                        nextSelection += 1;
                        if (nextSelection > 3)
                        {
                            nextSelection -= 3;
                        }
                        nextActiveUpgrade = CheckIfUpgradeIsActive(nextSelection);
                    }
                    if (nextSelection != currentSelection)
                    {
                        currentSelection = nextSelection;
                        if (type == "bag")
                        {
                            XaphanModule.ModSaveData.BagUIId1 = nextSelection;
                        }
                        else
                        {
                            XaphanModule.ModSaveData.BagUIId2 = nextSelection;
                        }
                        if (tutorialGui != null && tutorial)
                        {
                            preventTutorialDisplay = true;
                            tutorial = false;
                            tutorialTimer = 0;
                            tutorialGui.Open = false;
                        }
                        Audio.Play("event:/ui/main/rollover_up");
                    }
                }
            }
            if (!XaphanModule.PlayerIsControllingRemoteDrone())
            {
                if (type == "bag")
                {
                    if ((currentSelection == 1 && !(Bombs.isActive && XaphanModule.ModSettings.Bombs)) || (currentSelection == 2 && !(MegaBombs.isActive && XaphanModule.ModSettings.MegaBombs)) || (currentSelection == 3 && !(RemoteDrone.isActive && XaphanModule.ModSettings.RemoteDrone)))
                    {
                        SetToFirstActiveUpgrade();
                    }
                }
                else
                {
                    if ((currentSelection == 1 && !(Binoculars.isActive && XaphanModule.ModSettings.Binoculars)) || (currentSelection == 2 && !(PortableStation.isActive && XaphanModule.ModSettings.PortableStation)) || (currentSelection == 3 && !(PulseRadar.isActive && XaphanModule.ModSettings.PulseRadar)))
                    {
                        SetToFirstActiveUpgrade();
                    }
                }
            }
        }

        public void SetToFirstActiveUpgrade()
        {
            int nextSelection = 0;
            bool nextActiveUpgrade = false;
            bool bombActive = true;
            bool megaBombActive = true;
            bool droneActive = true;
            bool binocularsActive = true;
            bool teleporterActive = true;
            bool radarActive = true;
            if (type == "bag")
            {
                while (!nextActiveUpgrade && (bombActive || megaBombActive || droneActive))
                {
                    nextSelection += 1;
                    if (nextSelection > 3)
                    {
                        nextSelection -= 3;
                    }
                    nextActiveUpgrade = CheckIfUpgradeIsActive(nextSelection);
                    if (nextSelection == 1 && !nextActiveUpgrade)
                    {
                        bombActive = false;
                    }
                    if (nextSelection == 2 && !nextActiveUpgrade)
                    {
                        megaBombActive = false;
                    }
                    if (nextSelection == 3 && !nextActiveUpgrade)
                    {
                        droneActive = false;
                    }
                }
                if (bombActive || megaBombActive || droneActive)
                {
                    currentSelection = nextSelection;
                    XaphanModule.ModSaveData.BagUIId1 = nextSelection;
                }
            }
            else
            {
                while (!nextActiveUpgrade && (binocularsActive || teleporterActive || radarActive))
                {
                    nextSelection += 1;
                    if (nextSelection > 3)
                    {
                        nextSelection -= 3;
                    }
                    nextActiveUpgrade = CheckIfUpgradeIsActive(nextSelection);
                    if (nextSelection == 1 && !nextActiveUpgrade)
                    {
                        binocularsActive = false;
                    }
                    if (nextSelection == 2 && !nextActiveUpgrade)
                    {
                        teleporterActive = false;
                    }
                    if (nextSelection == 3 && !nextActiveUpgrade)
                    {
                        radarActive = false;
                    }
                }
                if (binocularsActive || teleporterActive || radarActive)
                {
                    currentSelection = nextSelection;
                    XaphanModule.ModSaveData.BagUIId2 = nextSelection;
                }
            }
        }

        public bool CheckIfUpgradeIsActive(int upgradeID)
        {
            if (type == "bag")
            {
                if (upgradeID == 1)
                {
                    return Bombs.isActive && XaphanModule.ModSettings.Bombs;
                }
                else if (upgradeID == 2)
                {
                    return MegaBombs.isActive && XaphanModule.ModSettings.MegaBombs;
                }
                else
                {
                    return RemoteDrone.isActive && XaphanModule.ModSettings.RemoteDrone;
                }
            }
            else
            {
                if (upgradeID == 1)
                {
                    return Binoculars.isActive && XaphanModule.ModSettings.Binoculars;
                }
                else if (upgradeID == 2)
                {
                    return PortableStation.isActive && XaphanModule.ModSettings.PortableStation;
                }
                else
                {
                    return PulseRadar.isActive && XaphanModule.ModSettings.PulseRadar;
                }
            }
        }

        private int CheckTotalUpgradesActive()
        {
            int totalActive = 0;
            if (type == "bag")
            {
                if (Bombs.isActive && XaphanModule.ModSettings.Bombs)
                {
                    totalActive++;
                }
                if (MegaBombs.isActive && XaphanModule.ModSettings.MegaBombs)
                {
                    totalActive++;
                }
                if (RemoteDrone.isActive && XaphanModule.ModSettings.RemoteDrone)
                {
                    totalActive++;
                }
            }
            else
            {
                if (Binoculars.isActive && XaphanModule.ModSettings.Binoculars)
                {
                    totalActive++;
                }
                if (PortableStation.isActive && XaphanModule.ModSettings.PortableStation)
                {
                    totalActive++;
                }
                if (PulseRadar.isActive && XaphanModule.ModSettings.PulseRadar)
                {
                    totalActive++;
                }
            }
            return totalActive;
        }

        public void ShowTutorial(bool action)
        {
            tutorial = action;
        }

        public static bool playerIsInHideTrigger(Level level)
        {
            foreach (HideMiniMapTrigger trigger in level.Tracker.GetEntities<HideMiniMapTrigger>())
            {
                if (trigger.playerInside)
                {
                    return true;
                }
            }
            return false;
        }

        public override void Render()
        {
            base.Render();
            Draw.Rect(Position + new Vector2(2), 96f, 96f, Color.Black * 0.85f * Opacity);
            string name = Dialog.Clean(type == "bag" ? "Xaphanhelper_UI_Bag" : "Xaphanhelper_UI_Misc");
            ActiveFont.DrawOutline(name, Position + new Vector2(50f, 0f), new Vector2(0.5f, 0.5f), Vector2.One * 0.3f, Color.Yellow * Opacity, 2f, Color.Black * Opacity);
            float nameLenght = ActiveFont.Measure(name).X * 0.3f;

            Draw.Rect(Position, 50f - nameLenght / 2 - 10f, 2f, borderColor * Opacity);
            Draw.Rect(Position + new Vector2(50f, 0f) + new Vector2(nameLenght / 2 + 11f, 0), 50f - nameLenght / 2 - 10f, 2f, borderColor * Opacity);
            Draw.Rect(Position + new Vector2(0f, 2f), 2f, 96f, borderColor * Opacity);
            Draw.Rect(Position + new Vector2(98f, 2f), 2f, 96f, borderColor * Opacity);
            Draw.Rect(Position + new Vector2(0f, 98f), 100f, 2f, borderColor * Opacity);

            if (buttonTexture != null)
            {
                buttonTexture.DrawCentered(Position + new Vector2(50f, 103f), Color.White * Opacity, 0.4f);
            }

            if (Sprite != null)
            {
                if (type == "bag")
                {
                    if (currentSelection == 1)
                    {
                        Sprite.Play("bombs");
                    }
                    else if (currentSelection == 2)
                    {
                        Sprite.Play("megaBombs");
                    }
                    else if (currentSelection == 3)
                    {
                        Sprite.Play("remoteDrone");
                    }
                }
                else
                {
                    if (currentSelection == 1)
                    {
                        Sprite.Play("binoculars");
                    }
                    else if (currentSelection == 2)
                    {
                        Sprite.Play("portableStation");
                    }
                    else if (currentSelection == 3)
                    {
                        Sprite.Play("pulseRadar");
                    }
                }
                Sprite.RenderPosition = Position + new Vector2(14f);
                Sprite.Color = Color.White * Opacity;
                Sprite.Render();
            }

            MTexture cross = GFX.Gui["upgrades/cross"];
            if (!CrossRoutine.Active)
            {
                if (XaphanModule.ModSettings.UseBagItemSlot.Pressed && XaphanModule.ModSettings.UseMiscItemSlot.Pressed)
                {
                    Add(CrossRoutine = new Coroutine(ShowCross()));
                }
                if (type == "bag")
                {
                    if (currentSelection == 1)
                    {
                        drawCross = !Bombs.canUse;
                    }
                    if (currentSelection == 2)
                    {
                        drawCross = !MegaBombs.canUse;
                    }
                    if (currentSelection == 3)
                    {
                        drawCross = SceneAs<Level>().Session.GetFlag("XaphanHelper_Prevent_Drone") || !RemoteDrone.canUse;
                    }
                }
                else
                {
                    if (currentSelection == 1)
                    {
                        drawCross = !Binoculars.canUse;
                    }
                    if (currentSelection == 2)
                    {
                        drawCross = !PortableStation.canUse;
                    }
                    if (currentSelection == 3)
                    {
                        drawCross = !PulseRadar.canUse;
                    }
                }
            }
            if (drawCross)
            {
                cross.DrawCentered(Center + Vector2.One * 50f, Color.White * Opacity * 0.6f);
            }
            if (XaphanModule.ModSettings.SelectItem.Check && totalActiveUpgrades != 0 && Upgrade1 != null && Upgrade2 != null && Upgrade3 != null && !XaphanModule.PlayerIsControllingRemoteDrone())
            {
                float height = 24f + totalActiveUpgrades * 72f + 12f * (totalActiveUpgrades - 1);
                Draw.Rect(Position + new Vector2(2f, 122f), 96f, height, Color.Black * 0.85f * Opacity);
                Draw.Rect(Position + new Vector2(0f, 120f), 100f, 2f, borderColor * Opacity);
                Draw.Rect(Position + new Vector2(0f, 122f), 2f, height, borderColor * Opacity);
                Draw.Rect(Position + new Vector2(98f, 122f), 2f, height, borderColor * Opacity);
                Draw.Rect(Position + new Vector2(0f, 122f + height), 100f, 2f, borderColor * Opacity);
                Draw.Rect(DetermineDrawPosition(DetermineSlotPosition(currentSelection)) - new Vector2(6f), 84, 84, Color.Yellow * selectedAlpha);
                int currentPos = 1;
                if (CheckIfUpgradeIsActive(1))
                {
                    Upgrade1.Play("upg");
                    Upgrade1.RenderPosition = DetermineDrawPosition(currentPos);
                    Upgrade1.Color = Color.White * Opacity;
                    Upgrade1.Render();
                    currentPos++;
                }
                if (CheckIfUpgradeIsActive(2))
                {
                    Upgrade2.Play("upg");
                    Upgrade2.RenderPosition = DetermineDrawPosition(currentPos);
                    Upgrade2.Color = Color.White * Opacity;
                    Upgrade2.Render();
                    currentPos++;
                }
                if (CheckIfUpgradeIsActive(3))
                {
                    Upgrade3.Play("upg");
                    Upgrade3.RenderPosition = DetermineDrawPosition(currentPos);
                    Upgrade3.Color = Color.White * Opacity;
                    Upgrade3.Render();
                }
            }
        }

        private Vector2 DetermineDrawPosition(int pos)
        {
            switch (pos)
            {
                case 3:
                    return Position + new Vector2(14f, 302f);
                case 2:
                    return Position + new Vector2(14f, 218f);
                default:
                    return Position + new Vector2(14f, 134f);
            }
        }

        private int DetermineSlotPosition(int upgradeID)
        {
            int slot = 0;
            for (int i = 1; i <= upgradeID; i++)
            {
                if (CheckIfUpgradeIsActive(i))
                    slot++;
            }
            return slot;
        }

        private IEnumerator ShowCross()
        {
            while (XaphanModule.ModSettings.UseBagItemSlot.Check && XaphanModule.ModSettings.UseMiscItemSlot.Check)
            {
                drawCross = true;
                yield return null;
            }
            drawCross = false;
        }

        public override void Removed(Scene scene)
        {
            base.Removed(scene);
            if (tutorialGui != null)
            {
                tutorialGui.RemoveSelf();
            }
            XaphanModule.UIOpened = false;
        }
    }
}
