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

        private float Opacity;

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

        public BagDisplay(Level level, string type)
        {
            this.level = level;
            this.type = type;
            Tag = (Tags.HUD | Tags.Persistent | Tags.PauseUpdate);
            Position = new Vector2(-200f, 26f);
            Depth = -10002;
            Sprite = new Sprite(GFX.Gui, "");
            Sprite.Scale = new Vector2(0.15f);
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
                if ((self.FrozenOrPaused || self.RetryPlayerCorpse != null || self.SkippingCutscene || self.InCutscene) || (player != null && !player.Sprite.Visible && !self.Session.GetFlag("Xaphan_Helper_Ceiling") && !sliding && (self.Tracker.GetEntity<ScrewAttackManager>() != null ? !self.Tracker.GetEntity<ScrewAttackManager>().StartedScrewAttack : true)) || XaphanModule.ShowUI || XaphanModule.PlayerIsControllingRemoteDrone() || playerIsInHideTrigger(self))
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
                    if (XaphanModule.ModSettings.UseBagItemSlot.Check && !self.Ducking)
                    {
                        foreach (Holdable component in self.Scene.Tracker.GetComponents<Holdable>())
                        {
                            if (component.Check(self) && (bool)Player_Pickup.Invoke(self, new object[] { component }))
                            {
                                return 8;
                            }
                        }
                    }
                }
            }
            return orig(self);
        }

        private static void modPlayerThrow(On.Celeste.Player.orig_Throw orig, Player self)
        {
            if (XaphanModule.useUpgrades)
            {
                if (!XaphanModule.ModSettings.UseBagItemSlot.Check)
                {
                    orig(self);
                }
            }
            else
            {
                orig(self);
            }
        }

        public void SetXPosition()
        {
            totalDisplays = level.Tracker.GetEntities<BagDisplay>().Count;
            if (XaphanModule.minimapEnabled)
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

        public override void Added(Scene scene)
        {
            base.Added(scene);
            SetXPosition();
            foreach (string VisitedChapter in XaphanModule.ModSaveData.VisitedChapters)
            {
                string[] str = VisitedChapter.Split('_');
                if (str[0] == level.Session.Area.GetLevelSet())
                {
                    GetCustomUpgradesData(int.Parse(str[1].Remove(0, 2)), int.Parse(str[2]));
                }
            }
            if (type == "bag")
            {
                Sprite.AddLoop("bombs", getCustomSpritePath("Bombs") + "/bombs", 0.08f, 0);
                Sprite.AddLoop("megaBombs", getCustomSpritePath("MegaBombs") + "/megaBombs", 0.08f, 0);
                Sprite.AddLoop("remoteDrone", getCustomSpritePath("RemoteDrone") + "/remoteDrone", 0.08f, 0);
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
                Sprite.AddLoop("portableStation", getCustomSpritePath("PortableStation") + "/portableStation", 0.08f, 0);
                Sprite.AddLoop("pulseRadar", getCustomSpritePath("PulseRadar") + "/pulseRadar", 0.08f, 0);
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
            if (player != null && player.Center.X > level.Camera.Right - (totalDisplays == 2 ? 96f : 64f) && player.Center.Y < level.Camera.Top + 52)
            {
                Opacity = Calc.Approach(Opacity, 0.3f, Engine.RawDeltaTime * 3f);
            }
            else
            {
                Opacity = Calc.Approach(Opacity, 1f, Engine.RawDeltaTime * 3f);
            }
            SetXPosition();
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
                    }
                }
                else if (tutorialGui.Open && !tutorial)
                {
                    tutorialGui.Open = false;
                }
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
