using System;
using System.Collections;
using System.Collections.Generic;
using Celeste.Mod.XaphanHelper.Data;
using Celeste.Mod.XaphanHelper.Upgrades;
using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste.Mod.XaphanHelper.UI_Elements
{
    class StatusDisplay : Entity
    {
        [Tracked(true)]
        public class UpgradeDisplay : Entity
        {
            public int col;

            public int row;

            public float width;

            public float height;

            public string Name;

            public HashSet<string> InactiveList;

            private Sprite Sprite;

            private StatusScreen StatusScreen;

            private string LevelSet;

            public bool Selected;

            private float selectedAlpha = 0;

            private int alphaStatus = 0;

            private float scale;

            public string internalName;

            public float paddingH;

            public float paddingV;

            public bool canSelect;

            private bool drawName;

            public UpgradeDisplay(Level level, bool canSelect, Vector2 position, int col, int row, string name, string spritePath, string spriteName, HashSet<string> inactiveList, float width = 550f, float height = 50f, bool notDialog = false, float scale = 0.85f, float paddingH = 10f, float paddingV = 0f, bool drawName = false, bool small = false) : base(position)
            {
                Tag = Tags.HUD;
                this.width = width;
                this.height = height;
                this.scale = scale;
                this.paddingH = paddingH;
                this.paddingV = paddingV;
                this.canSelect = canSelect;
                this.col = col;
                this.row = row;
                this.drawName = drawName;
                Name = notDialog ? name : Dialog.Clean(name);
                InactiveList = inactiveList;
                Sprite = new Sprite(GFX.Gui, spritePath + "/");
                Sprite.AddLoop("on", spriteName, 0.05f, 0);
                Sprite.Play("on");
                Sprite.Scale = small ? new Vector2(0.1f) : new Vector2(0.2f);
                Sprite.Position = position + Vector2.One * (small ? 3 : 6);
                StatusScreen = level.Tracker.GetEntity<StatusScreen>();
                LevelSet = level.Session.Area.LevelSet;
                internalName = spriteName;
                Depth = -10001;
            }

            public override void Update()
            {
                base.Update();
                if (StatusScreen.SelectedCol == col && StatusScreen.SelectedRow == row && canSelect)
                {
                    Selected = true;
                    if (StatusScreen.prompt == null)
                    {
                        if (Input.MenuConfirm.Pressed && InactiveList.Contains(LevelSet))
                        {
                            InactiveList.Remove(LevelSet);
                            Audio.Play("event:/ui/main/message_confirm");
                        }
                        else if (Input.MenuConfirm.Pressed && !InactiveList.Contains(LevelSet))
                        {
                            InactiveList.Add(LevelSet);
                            Audio.Play("event:/ui/main/button_back");
                        }
                    }
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
                }
                else
                {
                    Selected = false;
                }
                if (!InactiveList.Contains(LevelSet))
                {
                    Sprite.Color = Color.White;
                }
                else
                {
                    Sprite.Color = Color.Gray;
                }
            }

            public override void Render()
            {
                base.Render();
                if (canSelect && Selected)
                {
                    Draw.Rect(Position, width, height, Color.Yellow * selectedAlpha);
                }
                Sprite.Render();
                if (XaphanModule.useMetroidGameplay || drawName)
                {
                    float lenght = ActiveFont.Measure(Name).X * scale;
                    bool smallText = false;
                    if (lenght > 500f)
                    {
                        lenght = ActiveFont.Measure(Name).X * (scale - 0.1f);
                        smallText = true;
                    }
                    ActiveFont.DrawOutline(Name, Position + new Vector2(60 + paddingH + lenght / 2 - 10, height / 2 + paddingV), new Vector2(0.5f, 0.5f), Vector2.One * (smallText ? 0.75f : scale), !InactiveList.Contains(LevelSet) ? Color.White : Color.Gray, 2f, Color.Black);
                }
            }
        }

        public MapData MapData;

        private Level level;

        private Sprite PlayerSprite;

        public List<UpgradeDisplay> Displays = new();

        public UpgradeDisplay SelectedDisplay;

        public List<CustomUpgradesData> CustomUpgradesData = new();

        VirtualButton ButtonA = new();

        VirtualButton ButtonB = new();

        public StatusDisplay(Level level, bool useMap)
        {
            this.level = level;
            AreaKey area = level.Session.Area;
            MapData = AreaData.Areas[area.ID].Mode[(int)area.Mode].MapData;
            Tag = Tags.HUD;
            PlayerSprite = new Sprite(GFX.Gui, "upgrades/");
            string character = XaphanModule.useMetroidGameplay ? "samus" : "madeline";
            PlayerSprite.AddLoop("normal", character + "_normal", 0.05f, 0);
            PlayerSprite.AddLoop("varia", character + "_varia", 0.05f, 0);
            PlayerSprite.AddLoop("gravity", character + "_gravity", 0.05f, 0);
            float playerSpriteScale = 0.95f;
            PlayerSprite.Scale = Vector2.One * playerSpriteScale;
            PlayerSprite.Position = Position + new Vector2(Engine.Width / 2 - PlayerSprite.Width * playerSpriteScale / 2, XaphanModule.useMetroidGameplay ? 600 - PlayerSprite.Height * playerSpriteScale / 2 : 540 - PlayerSprite.Height * playerSpriteScale / 2);
            Depth = -10001;
        }

        public override void Added(Scene scene)
        {
            base.Added(scene);
            foreach (string VisitedChapter in XaphanModule.ModSaveData.VisitedChapters)
            {
                string[] str = VisitedChapter.Split('_');
                if (str[0] == level.Session.Area.LevelSet)
                {
                    GetCustomUpgradesData(int.Parse(str[1].Remove(0, 2)), int.Parse(str[2]));
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

        public override void Update()
        {
            base.Update();
            if (GravityJacket.Active(level))
            {
                PlayerSprite.Play("gravity");
            }
            else if (VariaJacket.Active(level))
            {
                PlayerSprite.Play("varia");
            }
            else
            {
                PlayerSprite.Play("normal");
            }
            foreach (UpgradeDisplay display in Scene.Tracker.GetEntities<UpgradeDisplay>())
            {
                if (display.Selected)
                {
                    SelectedDisplay = display;
                }
            }
        }

        public string getCustomName(string upgrade)
        {
            string name = "XaphanHelper_get_" + upgrade + "_Name";
            foreach (CustomUpgradesData UpgradesData in CustomUpgradesData)
            {
                if (UpgradesData.Upgrade == upgrade)
                {
                    if (!string.IsNullOrEmpty(UpgradesData.CustomName))
                    {
                        name = UpgradesData.CustomName;
                    }
                }
            }
            return name;
        }

        public string getCustomSpritePath(string upgrade)
        {
            string spritePath = "collectables/XaphanHelper/UpgradeCollectable";
            foreach (CustomUpgradesData UpgradesData in CustomUpgradesData)
            {
                if (UpgradesData.Upgrade == upgrade)
                {
                    if (!string.IsNullOrEmpty(UpgradesData.CustomSpritePath))
                    {
                        spritePath = UpgradesData.CustomSpritePath;
                    }
                }
            }
            return spritePath;
        }

        public IEnumerator GennerateUpgradesDisplay()
        {
            if (!XaphanModule.useMetroidGameplay)
            {
                /* Arms */
                if (XaphanModule.ModSettings.PowerGrip)
                {
                    Scene.Add(new UpgradeDisplay(level, true, new Vector2(266f, 255f), 1, 0, getCustomName("PowerGrip"), getCustomSpritePath("PowerGrip"), "PowerGrip", XaphanModule.ModSaveData.PowerGripInactive, 108f, 108f));
                }
                if (XaphanModule.ModSettings.ClimbingKit)
                {
                    Scene.Add(new UpgradeDisplay(level, true, new Vector2(376f, 255f), 2, 0, getCustomName("ClimbingKit"), getCustomSpritePath("ClimbingKit"), "ClimbingKit", XaphanModule.ModSaveData.ClimbingKitInactive, 108f, 108f));
                }
                if (XaphanModule.ModSettings.SpiderMagnet)
                {
                    Scene.Add(new UpgradeDisplay(level, true, new Vector2(486f, 255f), 3, 0, getCustomName("SpiderMagnet"), getCustomSpritePath("SpiderMagnet"), "SpiderMagnet", XaphanModule.ModSaveData.SpiderMagnetInactive, 108f, 108f));
                }
                /* Legs */
                if (XaphanModule.ModSettings.DashBoots)
                {
                    Scene.Add(new UpgradeDisplay(level, true, new Vector2(266f, 440f), 1, 1, getCustomName("DashBoots"), getCustomSpritePath("DashBoots"), "DashBoots", XaphanModule.ModSaveData.DashBootsInactive, 108f, 108f));
                }
                if (XaphanModule.ModSettings.SpaceJump == 2)
                {
                    Scene.Add(new UpgradeDisplay(level, true, new Vector2(376f, 440f), 2, 1, getCustomName("SpaceJump"), getCustomSpritePath("SpaceJump"), "SpaceJump", XaphanModule.ModSaveData.SpaceJumpInactive, 108f, 108f));
                }
                if (XaphanModule.ModSettings.LightningDash)
                {
                    Scene.Add(new UpgradeDisplay(level, true, new Vector2(486f, 440f), 3, 1, getCustomName("LightningDash"), getCustomSpritePath("LightningDash"), "LightningDash", XaphanModule.ModSaveData.LightningDashInactive, 108f, 108f));
                }
                /* Drone */
                if (XaphanModule.ModSettings.LongBeam)
                {
                    Scene.Add(new UpgradeDisplay(level, true, new Vector2(156f, 625f), 0, 2, getCustomName("LongBeam"), getCustomSpritePath("LongBeam"), "LongBeam", XaphanModule.ModSaveData.LongBeamInactive, 108f, 108f));
                }
                if (XaphanModule.ModSettings.IceBeam)
                {
                    Scene.Add(new UpgradeDisplay(level, true, new Vector2(266f, 625f), 1, 2, getCustomName("IceBeam"), getCustomSpritePath("IceBeam"), "IceBeam", XaphanModule.ModSaveData.IceBeamInactive, 108f, 108f));
                }
                if (XaphanModule.ModSettings.WaveBeam)
                {
                    Scene.Add(new UpgradeDisplay(level, true, new Vector2(376f, 625f), 2, 2, getCustomName("WaveBeam"), getCustomSpritePath("WaveBeam"), "WaveBeam", XaphanModule.ModSaveData.WaveBeamInactive, 108f, 108f));
                }
                if (XaphanModule.ModSettings.MissilesModule)
                {
                    Scene.Add(new UpgradeDisplay(level, true, new Vector2(156f, 735f), 0, 3, getCustomName("MissilesModule"), getCustomSpritePath("MissilesModule"), "MissilesModule", XaphanModule.ModSaveData.MissilesModuleInactive, 108f, 108f));
                }
                if (XaphanModule.ModSettings.SuperMissilesModule)
                {
                    Scene.Add(new UpgradeDisplay(level, true, new Vector2(266f, 735f), 1, 3, getCustomName("SuperMissilesModule"), getCustomSpritePath("SuperMissilesModule"), "SuperMissilesModule", XaphanModule.ModSaveData.SuperMissilesModuleInactive, 108f, 108f));
                }
                if (XaphanModule.ModSettings.JumpBoost)
                {
                    Scene.Add(new UpgradeDisplay(level, true, new Vector2(376f, 735f), 2, 3, getCustomName("JumpBoost"), getCustomSpritePath("JumpBoost"), "JumpBoost", XaphanModule.ModSaveData.JumpBoostInactive, 108f, 108f));
                }
                if (XaphanModule.ModSettings.HoverJet)
                {
                    Scene.Add(new UpgradeDisplay(level, true, new Vector2(486f, 735f), 3, 3, getCustomName("HoverJet"), getCustomSpritePath("HoverJet"), "HoverJet", XaphanModule.ModSaveData.HoverJetInactive, 108f, 108f));
                }
                /* Jacket */
                if (XaphanModule.ModSettings.GravityJacket)
                {
                    Scene.Add(new UpgradeDisplay(level, true, new Vector2(1328f, 255f), 6, 0, getCustomName("GravityJacket"), getCustomSpritePath("GravityJacket"), "GravityJacket", XaphanModule.ModSaveData.GravityJacketInactive, 108f, 108f));
                }
                if (XaphanModule.ModSettings.VariaJacket)
                {
                    Scene.Add(new UpgradeDisplay(level, true, new Vector2(1438f, 255f), 7, 0, getCustomName("VariaJacket"), getCustomSpritePath("VariaJacket"), "VariaJacket", XaphanModule.ModSaveData.VariaJacketInactive, 108f, 108f));
                }
                if (XaphanModule.ModSettings.ScrewAttack)
                {
                    Scene.Add(new UpgradeDisplay(level, true, new Vector2(1548f, 255f), 8, 0, getCustomName("ScrewAttack"), getCustomSpritePath("ScrewAttack"), "ScrewAttack", XaphanModule.ModSaveData.ScrewAttackInactive, 108f, 108f));
                }
                /* Bag */
                if (XaphanModule.ModSettings.Bombs)
                {
                    Scene.Add(new UpgradeDisplay(level, true, new Vector2(1328f, 440f), 6, 1, getCustomName("Bombs"), getCustomSpritePath("Bombs"), "Bombs", XaphanModule.ModSaveData.BombsInactive, 108f, 108f));
                }
                if (XaphanModule.ModSettings.MegaBombs)
                {
                    Scene.Add(new UpgradeDisplay(level, true, new Vector2(1438f, 440f), 7, 1, getCustomName("MegaBombs"), getCustomSpritePath("MegaBombs"), "MegaBombs", XaphanModule.ModSaveData.MegaBombsInactive, 108f, 108f));
                }
                if (XaphanModule.ModSettings.RemoteDrone)
                {
                    Scene.Add(new UpgradeDisplay(level, true, new Vector2(1548f, 440f), 8, 1, getCustomName("RemoteDrone"), getCustomSpritePath("RemoteDrone"), "RemoteDrone", XaphanModule.ModSaveData.RemoteDroneInactive, 108f, 108f));
                }
                /* Misc */
                if (XaphanModule.ModSettings.Binoculars)
                {
                    Scene.Add(new UpgradeDisplay(level, true, new Vector2(1328f, 625f), 6, 2, getCustomName("Binoculars"), getCustomSpritePath("Binoculars"), "Binoculars", XaphanModule.ModSaveData.BinocularsInactive, 108f, 108f));
                }
                if (XaphanModule.ModSettings.PortableStation)
                {
                    Scene.Add(new UpgradeDisplay(level, true, new Vector2(1438f, 625f), 7, 2, getCustomName("PortableStation"), getCustomSpritePath("PortableStation"), "PortableStation", XaphanModule.ModSaveData.PortableStationInactive, 108f, 108f));
                }
                if (XaphanModule.ModSettings.PulseRadar)
                {
                    Scene.Add(new UpgradeDisplay(level, true, new Vector2(1548f, 625f), 8, 2, getCustomName("PulseRadar"), getCustomSpritePath("PulseRadar"), "PulseRadar", XaphanModule.ModSaveData.PulseRadarInactive, 108f, 108f));
                }
                if (XaphanModule.ModSettings.GoldenFeather)
                {
                    Scene.Add(new UpgradeDisplay(level, true, new Vector2(1328f, 735f), 6, 3, getCustomName("GoldenFeather"), getCustomSpritePath("GoldenFeather"), "GoldenFeather", XaphanModule.ModSaveData.GoldenFeatherInactive, 108f, 108f));
                }
                if (XaphanModule.ModSettings.EtherealDash)
                {
                    Scene.Add(new UpgradeDisplay(level, true, new Vector2(1438f, 735f), 7, 3, getCustomName("EtherealDash"), getCustomSpritePath("EtherealDash"), "EtherealDash", XaphanModule.ModSaveData.EtherealDashInactive, 108f, 108f));
                }
                if (XaphanModule.ModSettings.DroneTeleport)
                {
                    Scene.Add(new UpgradeDisplay(level, true, new Vector2(1548f, 735f), 8, 3, getCustomName("DroneTeleport"), getCustomSpritePath("DroneTeleport"), "DroneTeleport", XaphanModule.ModSaveData.DroneTeleportInactive, 108f, 108f));
                }
                /* Collectables */
                float scale = 0.4f;
                string Prefix = SceneAs<Level>().Session.Area.LevelSet;
                if (XaphanModule.ModSettings.PowerGrip)
                {
                    int staminaCount = 0;
                    foreach (string staminaUpgrade in XaphanModule.PlayerHasGolden ? XaphanModule.ModSaveData.GoldenStrawberryStaminaUpgrades : XaphanModule.ModSaveData.StaminaUpgrades)
                    {
                        if (staminaUpgrade.Contains(Prefix))
                        {
                            staminaCount++;
                        }
                    }
                    if (staminaCount > 0)
                    {
                        string qty = "x " + staminaCount;
                        Scene.Add(new UpgradeDisplay(level, false, new Vector2(320f, 309f), 1, 0, qty, getCustomSpritePath("EnergyTank"), "EnergyTank", XaphanModule.ModSaveData.PowerGripInactive, 54f, 54f, true, scale, -23f - ActiveFont.Measure(qty).X * scale / 2, 15f, true, true));
                    }
                }
                if (XaphanModule.ModSettings.MissilesModule)
                {
                    int missileCount = 10;
                    foreach (string missileUpgrade in XaphanModule.PlayerHasGolden ? XaphanModule.ModSaveData.GoldenStrawberryDroneMissilesUpgrades : XaphanModule.ModSaveData.DroneMissilesUpgrades)
                    {
                        if (missileUpgrade.Contains(Prefix))
                        {
                            missileCount += 2;
                        }
                    }
                    string qty = "x " + missileCount;
                    Scene.Add(new UpgradeDisplay(level, false, new Vector2(210f, 789f), 0, 3, qty, getCustomSpritePath("MissilesModule"), "MissilesModule", XaphanModule.ModSaveData.MissilesModuleInactive, 54f, 54f, true, scale, -23f - ActiveFont.Measure(qty).X * scale / 2, 15f, true, true));
                }
                if (XaphanModule.ModSettings.SuperMissilesModule)
                {
                    int superMissileCount = 5;
                    foreach (string superMissileUpgrade in XaphanModule.PlayerHasGolden ? XaphanModule.ModSaveData.GoldenStrawberryDroneSuperMissilesUpgrades : XaphanModule.ModSaveData.DroneSuperMissilesUpgrades)
                    {
                        if (superMissileUpgrade.Contains(Prefix))
                        {
                            superMissileCount++;
                        }
                    }
                    string qty = "x " + superMissileCount;
                    Scene.Add(new UpgradeDisplay(level, false, new Vector2(320f, 789f), 1, 3, qty, getCustomSpritePath("SuperMissilesModule"), "SuperMissilesModule", XaphanModule.ModSaveData.SuperMissilesModuleInactive, 54f, 54f, true, scale, -25f - ActiveFont.Measure(qty).X * scale / 2, 15f, true, true));
                }
                if (XaphanModule.ModSettings.RemoteDrone)
                {
                    int fireRateCount = 0;
                    foreach (string fireRateModuleUpgrade in XaphanModule.PlayerHasGolden ? XaphanModule.ModSaveData.GoldenStrawberryDroneFireRateUpgrades : XaphanModule.ModSaveData.DroneFireRateUpgrades)
                    {
                        if (fireRateModuleUpgrade.Contains(Prefix))
                        {
                            fireRateCount++;
                        }
                    }
                    if (fireRateCount > 0)
                    {
                        string qty = "x " + fireRateCount;
                        Scene.Add(new UpgradeDisplay(level, false, new Vector2(1602f, 494f), 8, 1, qty, getCustomSpritePath("FireRateModule"), "FireRateModule", XaphanModule.ModSaveData.RemoteDroneInactive, 54f, 54f, true, scale, -25f - ActiveFont.Measure(qty).X * scale / 2, 15f, true, true));
                    }
                }
            }
            else
            {
                if (XaphanModule.ModSettings.LongBeam)
                {
                    Scene.Add(new UpgradeDisplay(level, true, new Vector2(155f, 610f), 0, 0, "XaphanHelper_get_LongBeam_Name", "collectables/XaphanHelper/SamusUpgradeCollectable", "LongBeam", XaphanModule.ModSaveData.LongBeamInactive));
                }
                if (XaphanModule.ModSettings.IceBeam)
                {
                    Scene.Add(new UpgradeDisplay(level, true, new Vector2(155f, 670f), 0, 1, "XaphanHelper_get_IceBeam_Name", "collectables/XaphanHelper/SamusUpgradeCollectable", "IceBeam", XaphanModule.ModSaveData.IceBeamInactive));
                }
                if (XaphanModule.ModSettings.WaveBeam)
                {
                    Scene.Add(new UpgradeDisplay(level, true, new Vector2(155f, 730f), 0, 2, "XaphanHelper_get_WaveBeam_Name", "collectables/XaphanHelper/SamusUpgradeCollectable", "WaveBeam", XaphanModule.ModSaveData.WaveBeamInactive));
                }
                if (XaphanModule.ModSettings.Spazer)
                {
                    Scene.Add(new UpgradeDisplay(level, true, new Vector2(155f, 790f), 0, 3, "XaphanHelper_get_Spazer_Name", "collectables/XaphanHelper/SamusUpgradeCollectable", "Spazer", XaphanModule.ModSaveData.SpazerInactive));
                }
                if (XaphanModule.ModSettings.PlasmaBeam)
                {
                    Scene.Add(new UpgradeDisplay(level, true, new Vector2(155f, 850f), 0, 4, "XaphanHelper_get_PlasmaBeam_Name", "collectables/XaphanHelper/SamusUpgradeCollectable", "PlasmaBeam", XaphanModule.ModSaveData.PlasmaBeamInactive));
                }
                if (XaphanModule.ModSettings.VariaJacket)
                {
                    Scene.Add(new UpgradeDisplay(level, true, new Vector2(1215f, 250f), 0, 5, "XaphanHelper_get_Met_VariaJacket_Name", "collectables/XaphanHelper/SamusUpgradeCollectable", "VariaJacket", XaphanModule.ModSaveData.VariaJacketInactive));
                }
                if (XaphanModule.ModSettings.GravityJacket)
                {
                    Scene.Add(new UpgradeDisplay(level, true, new Vector2(1215f, 310f), 0, 1, "XaphanHelper_get_Met_GravityJacket_Name", "collectables/XaphanHelper/SamusUpgradeCollectable", "GravityJacket", XaphanModule.ModSaveData.GravityJacketInactive));
                }
                if (XaphanModule.ModSettings.MorphingBall)
                {
                    Scene.Add(new UpgradeDisplay(level, true, new Vector2(1215f, 460f), 0, 2, "XaphanHelper_get_MorphingBall_Name", "collectables/XaphanHelper/SamusUpgradeCollectable", "MorphingBall", XaphanModule.ModSaveData.MorphingBallInactive));
                }
                if (XaphanModule.ModSettings.MorphBombs)
                {
                    Scene.Add(new UpgradeDisplay(level, true, new Vector2(1215f, 520f), 0, 3, "XaphanHelper_get_MorphBombs_Name", "collectables/XaphanHelper/SamusUpgradeCollectable", "MorphBombs", XaphanModule.ModSaveData.MorphBombsInactive));
                }
                if (XaphanModule.ModSettings.SpringBall)
                {
                    Scene.Add(new UpgradeDisplay(level, true, new Vector2(1215f, 580f), 0, 4, "XaphanHelper_get_SpringBall_Name", "collectables/XaphanHelper/SamusUpgradeCollectable", "SpringBall", XaphanModule.ModSaveData.SpringBallInactive));
                }
                if (XaphanModule.ModSettings.ScrewAttack)
                {
                    Scene.Add(new UpgradeDisplay(level, true, new Vector2(1215f, 640f), 0, 5, "XaphanHelper_get_Met_ScrewAttack_Name", "collectables/XaphanHelper/SamusUpgradeCollectable", "ScrewAttack", XaphanModule.ModSaveData.ScrewAttackInactive));
                }
                if (XaphanModule.ModSettings.HighJumpBoots)
                {
                    Scene.Add(new UpgradeDisplay(level, true, new Vector2(1215f, 790f), 0, 6, "XaphanHelper_get_HighJumpBoots_Name", "collectables/XaphanHelper/SamusUpgradeCollectable", "HighJumpBoots", XaphanModule.ModSaveData.HighJumpBootsInactive));
                }
                if (XaphanModule.ModSettings.SpaceJump == 6)
                {
                    Scene.Add(new UpgradeDisplay(level, true, new Vector2(1215f, 850f), 0, 7, "XaphanHelper_get_Met_SpaceJump_Name", "collectables/XaphanHelper/SamusUpgradeCollectable", "SpaceJump", XaphanModule.ModSaveData.SpaceJumpInactive));
                }
                if (XaphanModule.ModSettings.SpeedBooster)
                {
                    Scene.Add(new UpgradeDisplay(level, true, new Vector2(1215f, 910f), 0, 8, "XaphanHelper_get_SpeedBooster_Name", "collectables/XaphanHelper/SamusUpgradeCollectable", "SpeedBooster", XaphanModule.ModSaveData.SpeedBoosterInactive));
                }
            }
            yield return null;
            foreach (UpgradeDisplay display in Scene.Tracker.GetEntities<UpgradeDisplay>())
            {
                if (display.canSelect)
                {
                    Displays.Add(display);
                }
            }
        }

        public override void Removed(Scene scene)
        {
            base.Removed(scene);
            foreach (UpgradeDisplay display in level.Tracker.GetEntities<UpgradeDisplay>())
            {
                display.RemoveSelf();
            }
        }

        private Sprite upgradeSprite;

        public override void Render()
        {
            base.Render();
            Level level = Scene as Level;
            if (level != null && (level.FrozenOrPaused || level.RetryPlayerCorpse != null || level.SkippingCutscene))
            {
                return;
            }
            Draw.Rect(new Vector2(100, 180), 1720, 840, Color.Black * 0.85f);
            PlayerSprite.Render();
            float SectionTitleLenght;
            float SectionTitleHeight;
            string SectionName;
            Vector2 SectionPosition;
            int SectionMaxLines;
            if (!XaphanModule.useMetroidGameplay)
            {
                SectionName = Dialog.Clean("Xaphanhelper_UI_Arms");
                SectionPosition = new Vector2(430f, 235f);
                SectionMaxLines = 1;
                ActiveFont.DrawOutline(SectionName, Position + SectionPosition, new Vector2(0.5f, 0.5f), Vector2.One * 1f, Color.Yellow, 2f, Color.Black);
                SectionTitleLenght = ActiveFont.Measure(SectionName).X;
                SectionTitleHeight = ActiveFont.Measure(SectionName).Y;
                Draw.Rect(Position + SectionPosition + new Vector2(SectionTitleLenght / 2 + 10, -4), 220f - (SectionTitleLenght / 2 + 10) + 15, 8f, Color.White);
                Draw.Rect(Position + SectionPosition + new Vector2(-220f - 15, -4), 220f - (SectionTitleLenght / 2 + 10) + 15, 8f, Color.White);
                Draw.Rect(Position + SectionPosition + new Vector2(SectionTitleLenght / 2 + 10 + 220f - (SectionTitleLenght / 2 + 10) + 5, -4), 10f, SectionMaxLines * 108f + SectionTitleHeight / 2 + 4, Color.White);
                Draw.Rect(Position + SectionPosition + new Vector2(-220f - 15, -4), 10f, SectionMaxLines * 108f + SectionTitleHeight / 2 + 4, Color.White);
                Draw.Rect(Position + SectionPosition + new Vector2(-220f - 15, SectionMaxLines * 108f + SectionTitleHeight / 2 - 8), 470f, 8f, Color.White);

                Draw.Line(Position + SectionPosition + new Vector2(SectionTitleLenght / 2 + 10 + 220f - (SectionTitleLenght / 2 + 10) + 15, (SectionMaxLines * 108f + 26) / 2), Position + SectionPosition + new Vector2(SectionTitleLenght / 2 + 10 + 220f - (SectionTitleLenght / 2 + 10) + 15 + 105, (SectionMaxLines * 108f + 26) / 2), Color.Black, 6f);
                Draw.Line(Position + SectionPosition + new Vector2(SectionTitleLenght / 2 + 10 + 220f - (SectionTitleLenght / 2 + 10) + 15 + 105, (SectionMaxLines * 108f + 26) / 2 - 2), Position + SectionPosition + new Vector2(SectionTitleLenght / 2 + 10 + 220f - (SectionTitleLenght / 2 + 10) + 15 + 105, (SectionMaxLines * 108f + 26) / 2 + 173), Color.Black, 6f);
                Draw.Line(Position + SectionPosition + new Vector2(SectionTitleLenght / 2 + 10 + 220f - (SectionTitleLenght / 2 + 10) + 15 + 105, (SectionMaxLines * 108f + 26) / 2 + 173), Position + SectionPosition + new Vector2(SectionTitleLenght / 2 + 10 + 220f - (SectionTitleLenght / 2 + 10) + 15 + 225, (SectionMaxLines * 108f + 26) / 2 + 173), Color.Black, 6f);
                Draw.Rect(Position + SectionPosition + new Vector2(SectionTitleLenght / 2 + 10 + 220f - (SectionTitleLenght / 2 + 10) + 15 + 225 - 7, (SectionMaxLines * 108f + 26) / 2 + 173 - 7), 14f, 14f, Color.Black);
                Draw.Line(Position + SectionPosition + new Vector2(SectionTitleLenght / 2 + 10 + 220f - (SectionTitleLenght / 2 + 10) + 15, (SectionMaxLines * 108f + 26) / 2), Position + SectionPosition + new Vector2(SectionTitleLenght / 2 + 10 + 220f - (SectionTitleLenght / 2 + 10) + 15 + 105, (SectionMaxLines * 108f + 26) / 2), Color.White, 4f);
                Draw.Line(Position + SectionPosition + new Vector2(SectionTitleLenght / 2 + 10 + 220f - (SectionTitleLenght / 2 + 10) + 15 + 105, (SectionMaxLines * 108f + 26) / 2 - 2), Position + SectionPosition + new Vector2(SectionTitleLenght / 2 + 10 + 220f - (SectionTitleLenght / 2 + 10) + 15 + 105, (SectionMaxLines * 108f + 26) / 2 + 173), Color.White, 4f);
                Draw.Line(Position + SectionPosition + new Vector2(SectionTitleLenght / 2 + 10 + 220f - (SectionTitleLenght / 2 + 10) + 15 + 103, (SectionMaxLines * 108f + 26) / 2 + 173), Position + SectionPosition + new Vector2(SectionTitleLenght / 2 + 10 + 220f - (SectionTitleLenght / 2 + 10) + 15 + 225, (SectionMaxLines * 108f + 26) / 2 + 173), Color.White, 4f);
                Draw.Rect(Position + SectionPosition + new Vector2(SectionTitleLenght / 2 + 10 + 220f - (SectionTitleLenght / 2 + 10) + 15 + 225 - 6, (SectionMaxLines * 108f + 26) / 2 + 173 - 6), 12f, 12f, Color.White);

                SectionName = Dialog.Clean("Xaphanhelper_UI_Legs");
                SectionPosition = new Vector2(430f, 420f);
                SectionMaxLines = 1;
                ActiveFont.DrawOutline(SectionName, Position + SectionPosition, new Vector2(0.5f, 0.5f), Vector2.One * 1f, Color.Yellow, 2f, Color.Black);
                SectionTitleLenght = ActiveFont.Measure(SectionName).X;
                SectionTitleHeight = ActiveFont.Measure(SectionName).Y;
                Draw.Rect(Position + SectionPosition + new Vector2(SectionTitleLenght / 2 + 10, -4), 220f - (SectionTitleLenght / 2 + 10) + 15, 8f, Color.White);
                Draw.Rect(Position + SectionPosition + new Vector2(-220f - 15, -4), 220f - (SectionTitleLenght / 2 + 10) + 15, 8f, Color.White);
                Draw.Rect(Position + SectionPosition + new Vector2(SectionTitleLenght / 2 + 10 + 220f - (SectionTitleLenght / 2 + 10) + 5, -4), 10f, SectionMaxLines * 108f + SectionTitleHeight / 2 + 4, Color.White);
                Draw.Rect(Position + SectionPosition + new Vector2(-220f - 15, -4), 10f, SectionMaxLines * 108f + SectionTitleHeight / 2 + 4, Color.White);
                Draw.Rect(Position + SectionPosition + new Vector2(-220f - 15, SectionMaxLines * 108f + SectionTitleHeight / 2 - 8), 470f, 8f, Color.White);

                Draw.Line(Position + SectionPosition + new Vector2(SectionTitleLenght / 2 + 10 + 220f - (SectionTitleLenght / 2 + 10) + 15, (SectionMaxLines * 108f + 26) / 2), Position + SectionPosition + new Vector2(SectionTitleLenght / 2 + 10 + 220f - (SectionTitleLenght / 2 + 10) + 15 + 105, (SectionMaxLines * 108f + 26) / 2), Color.Black, 6f);
                Draw.Line(Position + SectionPosition + new Vector2(SectionTitleLenght / 2 + 10 + 220f - (SectionTitleLenght / 2 + 10) + 15 + 105, (SectionMaxLines * 108f + 26) / 2 - 2), Position + SectionPosition + new Vector2(SectionTitleLenght / 2 + 10 + 220f - (SectionTitleLenght / 2 + 10) + 15 + 105, (SectionMaxLines * 108f + 26) / 2 + 215), Color.Black, 6f);
                Draw.Line(Position + SectionPosition + new Vector2(SectionTitleLenght / 2 + 10 + 220f - (SectionTitleLenght / 2 + 10) + 15 + 103, (SectionMaxLines * 108f + 26) / 2 + 215), Position + SectionPosition + new Vector2(SectionTitleLenght / 2 + 10 + 220f - (SectionTitleLenght / 2 + 10) + 15 + 210, (SectionMaxLines * 108f + 26) / 2 + 215), Color.Black, 6f);
                Draw.Rect(Position + SectionPosition + new Vector2(SectionTitleLenght / 2 + 10 + 220f - (SectionTitleLenght / 2 + 10) + 15 + 210 - 7, (SectionMaxLines * 108f + 26) / 2 + 215 - 7), 14f, 14f, Color.Black);
                Draw.Line(Position + SectionPosition + new Vector2(SectionTitleLenght / 2 + 10 + 220f - (SectionTitleLenght / 2 + 10) + 15, (SectionMaxLines * 108f + 26) / 2), Position + SectionPosition + new Vector2(SectionTitleLenght / 2 + 10 + 220f - (SectionTitleLenght / 2 + 10) + 15 + 105, (SectionMaxLines * 108f + 26) / 2), Color.White, 4f);
                Draw.Line(Position + SectionPosition + new Vector2(SectionTitleLenght / 2 + 10 + 220f - (SectionTitleLenght / 2 + 10) + 15 + 105, (SectionMaxLines * 108f + 26) / 2 - 2), Position + SectionPosition + new Vector2(SectionTitleLenght / 2 + 10 + 220f - (SectionTitleLenght / 2 + 10) + 15 + 105, (SectionMaxLines * 108f + 26) / 2 + 215), Color.White, 4f);
                Draw.Line(Position + SectionPosition + new Vector2(SectionTitleLenght / 2 + 10 + 220f - (SectionTitleLenght / 2 + 10) + 15 + 103, (SectionMaxLines * 108f + 26) / 2 + 215), Position + SectionPosition + new Vector2(SectionTitleLenght / 2 + 10 + 220f - (SectionTitleLenght / 2 + 10) + 15 + 210, (SectionMaxLines * 108f + 26) / 2 + 215), Color.White, 4f);
                Draw.Rect(Position + SectionPosition + new Vector2(SectionTitleLenght / 2 + 10 + 220f - (SectionTitleLenght / 2 + 10) + 15 + 210 - 6, (SectionMaxLines * 108f + 26) / 2 + 215 - 6), 12f, 12f, Color.White);

                SectionName = XaphanModule.ModSettings.RemoteDrone ? Dialog.Clean("Xaphanhelper_UI_Drone") : "???";
                SectionPosition = new Vector2(430f, 605f);
                SectionMaxLines = 2;
                ActiveFont.DrawOutline(SectionName, Position + SectionPosition, new Vector2(0.5f, 0.5f), Vector2.One * 1f, Color.Yellow, 2f, Color.Black);
                SectionTitleLenght = ActiveFont.Measure(SectionName).X;
                SectionTitleHeight = ActiveFont.Measure(SectionName).Y;
                Draw.Rect(Position + SectionPosition + new Vector2(SectionTitleLenght / 2 + 10, -4), 275f - (SectionTitleLenght / 2 + 10) + 15, 8f, Color.White);
                Draw.Rect(Position + SectionPosition + new Vector2(-275f - 15, -4), 275f - (SectionTitleLenght / 2 + 10) + 15, 8f, Color.White);
                Draw.Rect(Position + SectionPosition + new Vector2(SectionTitleLenght / 2 + 10 + 275f - (SectionTitleLenght / 2 + 10) + 5, -4), 10f, SectionMaxLines * 108f + (SectionMaxLines - 1) * 2 + SectionTitleHeight / 2 + 4, Color.White);
                Draw.Rect(Position + SectionPosition + new Vector2(-275f - 15, -4), 10f, SectionMaxLines * 108f + (SectionMaxLines - 1) * 2 + SectionTitleHeight / 2 + 4, Color.White);
                Draw.Rect(Position + SectionPosition + new Vector2(-275f - 15, SectionMaxLines * 108f + (SectionMaxLines - 1) * 2 + SectionTitleHeight / 2 - 8), 580f, 8f, Color.White);

                SectionName = Dialog.Clean("Xaphanhelper_UI_Jacket");
                SectionPosition = new Vector2(1490f, 235f);
                SectionMaxLines = 1;
                ActiveFont.DrawOutline(SectionName, Position + SectionPosition, new Vector2(0.5f, 0.5f), Vector2.One * 1f, Color.Yellow, 2f, Color.Black);
                SectionTitleLenght = ActiveFont.Measure(SectionName).X;
                SectionTitleHeight = ActiveFont.Measure(SectionName).Y;
                Draw.Rect(Position + SectionPosition + new Vector2(SectionTitleLenght / 2 + 10, -4), 220f - (SectionTitleLenght / 2 + 10) + 15, 8f, Color.White);
                Draw.Rect(Position + SectionPosition + new Vector2(-220f - 15, -4), 220f - (SectionTitleLenght / 2 + 10) + 15, 8f, Color.White);
                Draw.Rect(Position + SectionPosition + new Vector2(SectionTitleLenght / 2 + 10 + 220f - (SectionTitleLenght / 2 + 10) + 5, -4), 10f, SectionMaxLines * 108f + SectionTitleHeight / 2 + 4, Color.White);
                Draw.Rect(Position + SectionPosition + new Vector2(-220f - 15, -4), 10f, SectionMaxLines * 108f + SectionTitleHeight / 2 + 4, Color.White);
                Draw.Rect(Position + SectionPosition + new Vector2(-220f - 15, SectionMaxLines * 108f + SectionTitleHeight / 2 - 8), 470f, 8f, Color.White);

                Draw.Line(Position + SectionPosition + new Vector2(-220f - 15, (SectionMaxLines * 108f + 26) / 2), Position + SectionPosition + new Vector2(-220f - 15 - 105, (SectionMaxLines * 108f + 26) / 2), Color.Black, 6f);
                Draw.Line(Position + SectionPosition + new Vector2(-220f - 15 - 105, (SectionMaxLines * 108f + 26) / 2 - 2), Position + SectionPosition + new Vector2(-220f - 15 - 105, (SectionMaxLines * 108f + 26) / 2 + 115), Color.Black, 6f);
                Draw.Line(Position + SectionPosition + new Vector2(-220f - 15 - 103, (SectionMaxLines * 108f + 26) / 2 + 115), Position + SectionPosition + new Vector2(-220f - 15 - 251, (SectionMaxLines * 108f + 26) / 2 + 115), Color.Black, 6f);
                Draw.Rect(Position + SectionPosition + new Vector2(-220f - 15 - 251 - 7, (SectionMaxLines * 108f + 26) / 2 + 115 - 7), 14f, 14f, Color.Black);
                Draw.Line(Position + SectionPosition + new Vector2(-220f - 15, (SectionMaxLines * 108f + 26) / 2), Position + SectionPosition + new Vector2(-220f - 15 - 105, (SectionMaxLines * 108f + 26) / 2), Color.White, 4f);
                Draw.Line(Position + SectionPosition + new Vector2(-220f - 15 - 105, (SectionMaxLines * 108f + 26) / 2 - 2), Position + SectionPosition + new Vector2(-220f - 15 - 105, (SectionMaxLines * 108f + 26) / 2 + 115), Color.White, 4f);
                Draw.Line(Position + SectionPosition + new Vector2(-220f - 15 - 103, (SectionMaxLines * 108f + 26) / 2 + 115), Position + SectionPosition + new Vector2(-220f - 15 - 251, (SectionMaxLines * 108f + 26) / 2 + 115), Color.White, 4f);
                Draw.Rect(Position + SectionPosition + new Vector2(-220f - 15 - 251 - 6, (SectionMaxLines * 108f + 26) / 2 + 115 - 6), 12f, 12f, Color.White);

                SectionName = Dialog.Clean("Xaphanhelper_UI_Bag");
                SectionPosition = new Vector2(1490f, 420f);
                SectionMaxLines = 1;
                ActiveFont.DrawOutline(SectionName, Position + SectionPosition, new Vector2(0.5f, 0.5f), Vector2.One * 1f, Color.Yellow, 2f, Color.Black);
                SectionTitleLenght = ActiveFont.Measure(SectionName).X;
                SectionTitleHeight = ActiveFont.Measure(SectionName).Y;
                Draw.Rect(Position + SectionPosition + new Vector2(SectionTitleLenght / 2 + 10, -4), 220f - (SectionTitleLenght / 2 + 10) + 15, 8f, Color.White);
                Draw.Rect(Position + SectionPosition + new Vector2(-220f - 15, -4), 220f - (SectionTitleLenght / 2 + 10) + 15, 8f, Color.White);
                Draw.Rect(Position + SectionPosition + new Vector2(SectionTitleLenght / 2 + 10 + 220f - (SectionTitleLenght / 2 + 10) + 5, -4), 10f, SectionMaxLines * 108f + SectionTitleHeight / 2 + 4, Color.White);
                Draw.Rect(Position + SectionPosition + new Vector2(-220f - 15, -4), 10f, SectionMaxLines * 108f + SectionTitleHeight / 2 + 4, Color.White);
                Draw.Rect(Position + SectionPosition + new Vector2(-220f - 15, SectionMaxLines * 108f + SectionTitleHeight / 2 - 8), 470f, 8f, Color.White);

                Draw.Line(Position + SectionPosition + new Vector2(-220f - 15, (SectionMaxLines * 108f + 26) / 2), Position + SectionPosition + new Vector2(-220f - 15 - 105, (SectionMaxLines * 108f + 26) / 2), Color.Black, 6f);
                Draw.Line(Position + SectionPosition + new Vector2(-220f - 15 - 105, (SectionMaxLines * 108f + 26) / 2 + 2), Position + SectionPosition + new Vector2(-220f - 15 - 105, (SectionMaxLines * 108f + 26) / 2 - 45), Color.Black, 6f);
                Draw.Line(Position + SectionPosition + new Vector2(-220f - 15 - 103, (SectionMaxLines * 108f + 26) / 2 - 45), Position + SectionPosition + new Vector2(-220f - 15 - 230, (SectionMaxLines * 108f + 26) / 2 - 45), Color.Black, 6f);
                Draw.Rect(Position + SectionPosition + new Vector2(-220f - 15 - 230 - 7, (SectionMaxLines * 108f + 26) / 2 - 45 - 7), 14f, 14f, Color.Black);
                Draw.Line(Position + SectionPosition + new Vector2(-220f - 15, (SectionMaxLines * 108f + 26) / 2), Position + SectionPosition + new Vector2(-220f - 15 - 105, (SectionMaxLines * 108f + 26) / 2), Color.White, 4f);
                Draw.Line(Position + SectionPosition + new Vector2(-220f - 15 - 105, (SectionMaxLines * 108f + 26) / 2 + 2), Position + SectionPosition + new Vector2(-220f - 15 - 105, (SectionMaxLines * 108f + 26) / 2 - 45), Color.White, 4f);
                Draw.Line(Position + SectionPosition + new Vector2(-220f - 15 - 103, (SectionMaxLines * 108f + 26) / 2 - 45), Position + SectionPosition + new Vector2(-220f - 15 - 230, (SectionMaxLines * 108f + 26) / 2 - 45), Color.White, 4f);
                Draw.Rect(Position + SectionPosition + new Vector2(-220f - 15 - 230 - 6, (SectionMaxLines * 108f + 26) / 2 - 45 - 6), 12f, 12f, Color.White);

                SectionName = Dialog.Clean("Xaphanhelper_UI_Misc");
                SectionPosition = new Vector2(1490f, 605f);
                SectionMaxLines = 2;
                ActiveFont.DrawOutline(SectionName, Position + SectionPosition, new Vector2(0.5f, 0.5f), Vector2.One * 1f, Color.Yellow, 2f, Color.Black);
                SectionTitleLenght = ActiveFont.Measure(SectionName).X;
                SectionTitleHeight = ActiveFont.Measure(SectionName).Y;
                Draw.Rect(Position + SectionPosition + new Vector2(SectionTitleLenght / 2 + 10, -4), 220f - (SectionTitleLenght / 2 + 10) + 15, 8f, Color.White);
                Draw.Rect(Position + SectionPosition + new Vector2(-220f - 15, -4), 220f - (SectionTitleLenght / 2 + 10) + 15, 8f, Color.White);
                Draw.Rect(Position + SectionPosition + new Vector2(SectionTitleLenght / 2 + 10 + 220f - (SectionTitleLenght / 2 + 10) + 5, -4), 10f, SectionMaxLines * 108f + (SectionMaxLines - 1) * 2 + SectionTitleHeight / 2 + 4, Color.White);
                Draw.Rect(Position + SectionPosition + new Vector2(-220f - 15, -4), 10f, SectionMaxLines * 108f + (SectionMaxLines - 1) * 2 + SectionTitleHeight / 2 + 4, Color.White);
                Draw.Rect(Position + SectionPosition + new Vector2(-220f - 15, SectionMaxLines * 108f + (SectionMaxLines - 1) * 2 + SectionTitleHeight / 2 - 8), 470f, 8f, Color.White);

                Draw.Line(Position + SectionPosition + new Vector2(-220f - 15, (SectionMaxLines * 108f + 26) / 2), Position + SectionPosition + new Vector2(-220f - 15 - 105, (SectionMaxLines * 108f + 26) / 2), Color.Black, 6f);
                Draw.Line(Position + SectionPosition + new Vector2(-220f - 15 - 105, (SectionMaxLines * 108f + 26) / 2 + 2), Position + SectionPosition + new Vector2(-220f - 15 - 105, (SectionMaxLines * 108f + 26) / 2 - 200), Color.Black, 6f);
                Draw.Line(Position + SectionPosition + new Vector2(-220f - 15 - 103, (SectionMaxLines * 108f + 26) / 2 - 200), Position + SectionPosition + new Vector2(-220f - 15 - 288, (SectionMaxLines * 108f + 26) / 2 - 200), Color.Black, 6f);
                Draw.Rect(Position + SectionPosition + new Vector2(-220f - 15 - 288 - 7, (SectionMaxLines * 108f + 26) / 2 - 200 - 7), 14f, 14f, Color.Black);
                Draw.Line(Position + SectionPosition + new Vector2(-220f - 15, (SectionMaxLines * 108f + 26) / 2), Position + SectionPosition + new Vector2(-220f - 15 - 105, (SectionMaxLines * 108f + 26) / 2), Color.White, 4f);
                Draw.Line(Position + SectionPosition + new Vector2(-220f - 15 - 105, (SectionMaxLines * 108f + 26) / 2 + 2), Position + SectionPosition + new Vector2(-220f - 15 - 105, (SectionMaxLines * 108f + 26) / 2 - 200), Color.White, 4f);
                Draw.Line(Position + SectionPosition + new Vector2(-220f - 15 - 103, (SectionMaxLines * 108f + 26) / 2 - 200), Position + SectionPosition + new Vector2(-220f - 15 - 288, (SectionMaxLines * 108f + 26) / 2 - 200), Color.White, 4f);
                Draw.Rect(Position + SectionPosition + new Vector2(-220f - 15 - 288 - 6, (SectionMaxLines * 108f + 26) / 2 - 200 - 6), 12f, 12f, Color.White);

                Draw.Rect(Position + new Vector2(140, 896f), 10f, 108f, Color.White);
                Draw.Rect(Position + new Vector2(1770f, 896f), 10, 108f, Color.White);
                Draw.Rect(Position + new Vector2(140, 1004f), 1640f, 8f, Color.White);
                if (SelectedDisplay != null)
                {
                    SectionPosition = new Vector2(Engine.Width / 2f, 900f);
                    string UpgName = Dialog.Clean("XaphanHelper_get_" + SelectedDisplay.internalName + "_Name");
                    float UpgNameLenght = ActiveFont.Measure(UpgName).X;
                    ActiveFont.DrawOutline(UpgName, Position + SectionPosition, new Vector2(0.5f, 0.5f), Vector2.One * 1f, Color.Yellow, 2f, Color.Black);
                    Draw.Rect(Position + SectionPosition + new Vector2(UpgNameLenght / 2 + 10, -4), 805f - (UpgNameLenght / 2 + 10) + 15, 8f, Color.White);
                    Draw.Rect(Position + SectionPosition + new Vector2(-805f - 15, -4), 805f - (UpgNameLenght / 2 + 10) + 15, 8f, Color.White);
                    string drone = "";
                    if ((SelectedDisplay.internalName == "LongBeam" || SelectedDisplay.internalName == "IceBeam" || SelectedDisplay.internalName == "WaveBeam") && !XaphanModule.useMetroidGameplay)
                    {
                        drone = "_drone";
                    }
                    string UpgDesc = Dialog.Clean("XaphanHelper_get_" + SelectedDisplay.internalName + "_Desc" + drone);
                    string UpgDesc_b = null;
                    bool useExtraLineDesc = false;
                    object controlA = null;
                    object controlB = null;
                    string inputActionA = null;
                    string inputActionB = null;
                    bool select = false;
                    switch (SelectedDisplay.internalName)
                    {
                        case "PowerGrip":
                            controlA = Input.Grab;
                            inputActionA = "XaphanHelper_Press";
                            useExtraLineDesc = true;
                            break;
                        case "ClimbingKit":
                            controlA = Input.MenuUp;
                            controlB = Input.MenuDown;
                            inputActionA = "XaphanHelper_Hold";
                            inputActionB = "XaphanHelper_Or";
                            useExtraLineDesc = true;
                            break;
                        case "SpiderMagnet":
                            controlA = Input.Grab;
                            inputActionA = "XaphanHelper_Hold";
                            useExtraLineDesc = true;
                            break;
                        case "DashBoots":
                            controlA = Input.Dash;
                            inputActionA = "XaphanHelper_Press";
                            useExtraLineDesc = true;
                            break;
                        case "SpaceJump":
                            controlA = Input.Jump;
                            inputActionA = "XaphanHelper_Press";
                            useExtraLineDesc = true;
                            break;
                        case "HoverJet":
                            controlA = Input.Grab;
                            inputActionA = "XaphanHelper_Hold";
                            useExtraLineDesc = true;
                            break;
                        case "LightningDash":
                            controlA = Input.Dash;
                            inputActionA = "XaphanHelper_ClingingPress";
                            useExtraLineDesc = true;
                            break;
                        case "LongBeam":
                            UpgDesc_b = null;
                            break;
                        case "IceBeam":
                            UpgDesc_b = null;
                            break;
                        case "WaveBeam":
                            UpgDesc_b = null;
                            break;
                        case "MissilesModule":
                            useExtraLineDesc = true;
                            break;
                        case "SuperMissilesModule":
                            useExtraLineDesc = true;
                            break;
                        case "DroneTeleport":
                            controlA = XaphanModule.ModSettings.UseBagItemSlot;
                            inputActionA = "XaphanHelper_Press";
                            useExtraLineDesc = true;
                            break;
                        case "GravityJacket":
                            UpgDesc_b = null;
                            break;
                        case "Bombs":
                            select = true;
                            controlA = XaphanModule.ModSettings.SelectItem;
                            controlB = XaphanModule.ModSettings.UseBagItemSlot;
                            inputActionA = "XaphanHelper_ThenHold";
                            useExtraLineDesc = true;
                            break;
                        case "MegaBombs":
                            select = true;
                            controlA = XaphanModule.ModSettings.SelectItem;
                            controlB = XaphanModule.ModSettings.UseBagItemSlot;
                            inputActionA = "XaphanHelper_ThenHold";
                            useExtraLineDesc = true;
                            break;
                        case "RemoteDrone":
                            select = true;
                            controlA = XaphanModule.ModSettings.SelectItem;
                            controlB = XaphanModule.ModSettings.UseBagItemSlot;
                            inputActionA = "XaphanHelper_ThenHold";
                            useExtraLineDesc = true;
                            break;
                        case "GoldenFeather":
                            controlA = Input.Grab;
                            inputActionA = "XaphanHelper_Hold";
                            useExtraLineDesc = true;
                            break;
                        case "EtherealDash":
                            UpgDesc_b = null;
                            break;
                        case "ScrewAttack":
                            useExtraLineDesc = true;
                            break;
                        case "Binoculars":
                            select = true;
                            controlA = XaphanModule.ModSettings.SelectItem;
                            controlB = XaphanModule.ModSettings.UseMiscItemSlot;
                            inputActionA = "XaphanHelper_ThenPress";
                            useExtraLineDesc = true;
                            break;
                        case "PortableStation":
                            select = true;
                            controlA = XaphanModule.ModSettings.SelectItem;
                            controlB = XaphanModule.ModSettings.UseMiscItemSlot;
                            inputActionA = "XaphanHelper_ThenPress";
                            useExtraLineDesc = true;
                            break;
                        case "PulseRadar":
                            select = true;
                            controlA = XaphanModule.ModSettings.SelectItem;
                            controlB = XaphanModule.ModSettings.UseMiscItemSlot;
                            inputActionA = "XaphanHelper_ThenPress";
                            useExtraLineDesc = true;
                            break;
                        case "JumpBoost":
                            controlA = Input.Jump;
                            inputActionA = "XaphanHelper_Hold";
                            useExtraLineDesc = true;
                            break;
                        case "EnergyTank":
                            UpgDesc_b = null;
                            break;
                        case "FireRateModule":
                            UpgDesc_b = null;
                            break;
                        default:
                            break;
                    }
                    if (useExtraLineDesc)
                    {
                        UpgDesc_b = Dialog.Clean("XaphanHelper_get_" + SelectedDisplay.internalName + "_Desc_b");
                    }
                    if (select)
                    {
                        upgradeSprite = new Sprite(GFX.Gui, getCustomSpritePath(SelectedDisplay.internalName) + "/");
                        upgradeSprite.AddLoop("static", SelectedDisplay.internalName, 1f, 0);
                        upgradeSprite.Play("static");
                        upgradeSprite.CenterOrigin();
                        upgradeSprite.Position = upgradeSprite.Position + new Vector2(8, 8);
                    }
                    float buttonATextureWidth = 0;
                    float buttonBTextureWidth = 0;
                    float spriteWidth = 0f;
                    if (controlA is VirtualButton)
                    {
                        MTexture buttonATexture = Input.GuiButton((VirtualButton)controlA, "controls/keyboard/oemquestion");
                        buttonATextureWidth = buttonATexture.Width * 0.5f;
                    }
                    else if (controlA is ButtonBinding)
                    {
                        ButtonBinding ControlA = (ButtonBinding)controlA;
                        ButtonA.Binding = ControlA.Binding;
                        MTexture buttonATexture = Input.GuiButton(ButtonA, "controls/keyboard/oemquestion");
                        buttonATextureWidth = buttonATexture.Width * 0.5f;
                    }
                    if (controlB is VirtualButton)
                    {
                        MTexture buttonBTexture = Input.GuiButton((VirtualButton)controlB, "controls/keyboard/oemquestion");
                        buttonBTextureWidth = buttonBTexture.Width * 0.5f;
                    }
                    else if (controlB is ButtonBinding)
                    {
                        ButtonBinding ControlB = (ButtonBinding)controlB;
                        ButtonB.Binding = ControlB.Binding;
                        MTexture buttonBTexture = Input.GuiButton(ButtonB, "controls/keyboard/oemquestion");
                        buttonBTextureWidth = buttonBTexture.Width * 0.5f;
                    }
                    string selectHold = Dialog.Clean("XaphanHelper_Hold");
                    string selectAndPress = Dialog.Clean("XaphanHelper_AndPress");
                    string selectString = !XaphanModule.useMetroidGameplay ? Dialog.Clean("XaphanHelper_ToSelect") : Dialog.Clean("XaphanHelper_Select");
                    if (upgradeSprite != null)
                    {
                        upgradeSprite.Scale = new Vector2(0.08f);
                        spriteWidth = upgradeSprite.Width * 0.08f;
                    }
                    string inputActA = Dialog.Clean(inputActionA);
                    string inputActB = Dialog.Clean(inputActionB);
                    Vector2 vector = new(960f, 980f);
                    float TotalLenght = 0;

                    if (select)
                    {
                        if (!XaphanModule.useMetroidGameplay)
                        {
                            TotalLenght = ActiveFont.Measure(selectHold).X * 0.75f + buttonATextureWidth + ActiveFont.Measure(selectAndPress).X * 0.75f + buttonBTextureWidth + ActiveFont.Measure(selectString).X * 0.75f + spriteWidth + ActiveFont.Measure(inputActA).X * 0.75f + 10 + buttonATextureWidth + 10 + (controlB != null ? ActiveFont.Measure(inputActB).X * 0.75f + 10 + buttonBTextureWidth + 10 : 0) + ActiveFont.Measure(UpgDesc_b).X * 0.75f;
                        }
                        else
                        {
                            TotalLenght = ActiveFont.Measure(selectString).X * 0.75f + spriteWidth + ActiveFont.Measure(inputActA).X * 0.75f + 10 + buttonATextureWidth + 10 + (controlB != null ? ActiveFont.Measure(inputActB).X * 0.75f + 10 + buttonBTextureWidth + 10 : 0) + ActiveFont.Measure(UpgDesc_b).X * 0.75f;
                        }
                    }
                    else
                    {
                        TotalLenght = ActiveFont.Measure(inputActA).X * 0.75f + 10 + buttonATextureWidth + 10 + (controlB != null ? ActiveFont.Measure(inputActB).X * 0.75f + 10 + buttonBTextureWidth + 10 : 0) + ActiveFont.Measure(UpgDesc_b).X * 0.75f;
                    }
                    float selectHoldPosition = 0;
                    float selectAndPressPosition = 0;
                    float selectPosition = 0;
                    float SpritePosition = 0;
                    float inputActAPosition = 0;
                    float InputAPosition = 0;
                    float inputActBPosition = 0;
                    float InputBPosition = 0;
                    float InputCPosition = 0f;
                    float TextCPosition = 0;
                    if (!select)
                    {
                        inputActAPosition = vector.X - TotalLenght / 2f + (ActiveFont.Measure(inputActA).X * 0.75f) / 2;
                        InputAPosition = inputActAPosition + (ActiveFont.Measure(inputActA).X * 0.75f) / 2 + 10 + buttonATextureWidth / 2;
                        inputActBPosition = InputAPosition + buttonATextureWidth / 2 + 10 + (ActiveFont.Measure(inputActB).X * 0.75f) / 2;
                        InputBPosition = inputActBPosition + (ActiveFont.Measure(inputActB).X * 0.75f) / 2 + 10 + buttonBTextureWidth / 2;
                        TextCPosition = (controlB == null ? -2 * 10 : 0) + InputBPosition + buttonBTextureWidth / 2 + 10 + (ActiveFont.Measure(UpgDesc_b).X * 0.75f) / 2;
                    }
                    else
                    {
                        if (!XaphanModule.useMetroidGameplay)
                        {
                            selectHoldPosition = vector.X - TotalLenght / 2f + (ActiveFont.Measure(selectHold).X * 0.75f) / 2;
                            InputAPosition = selectHoldPosition + (ActiveFont.Measure(selectHold).X * 0.75f) / 2 + 10 + buttonATextureWidth / 2;
                            selectAndPressPosition = InputAPosition + buttonATextureWidth / 2 + 10 + (ActiveFont.Measure(selectAndPress).X * 0.75f) / 2;
                            InputCPosition = selectAndPressPosition + (ActiveFont.Measure(selectAndPress).X * 0.75f) / 2 + 10 + buttonBTextureWidth / 2;
                            selectPosition = InputCPosition + buttonBTextureWidth / 2 + 10 + (ActiveFont.Measure(selectString).X * 0.75f) / 2;
                            SpritePosition = selectPosition + (ActiveFont.Measure(selectString).X * 0.75f) / 2 + 10 + spriteWidth / 2;
                            inputActAPosition = SpritePosition + spriteWidth / 2 + 10 + (ActiveFont.Measure(inputActA).X * 0.75f) / 2;
                            InputBPosition = inputActAPosition + (ActiveFont.Measure(inputActA).X * 0.75f) / 2 + 10 + buttonBTextureWidth / 2;
                            TextCPosition = (controlB == null ? -2 * 10 : 0) + InputBPosition + buttonBTextureWidth / 2 + 10 + (ActiveFont.Measure(UpgDesc_b).X * 0.75f) / 2;
                        }
                        else
                        {
                            selectPosition = vector.X - TotalLenght / 2f + (ActiveFont.Measure(selectString).X * 0.75f) / 2;
                            SpritePosition = selectPosition + (ActiveFont.Measure(selectString).X * 0.75f) / 2 + 10 + spriteWidth / 2;
                            inputActAPosition = SpritePosition + spriteWidth / 2 + 10 + (ActiveFont.Measure(inputActA).X * 0.75f) / 2;
                            InputAPosition = inputActAPosition + (ActiveFont.Measure(inputActA).X * 0.75f) / 2 + 10 + buttonATextureWidth / 2;
                            inputActBPosition = InputAPosition + buttonATextureWidth / 2 + 10 + (ActiveFont.Measure(inputActB).X * 0.75f) / 2;
                            InputBPosition = inputActBPosition + (ActiveFont.Measure(inputActB).X * 0.75f) / 2 + 10 + buttonBTextureWidth / 2;
                            TextCPosition = (controlB == null ? -2 * 10 : 0) + InputBPosition + buttonBTextureWidth / 2 + 10 + (ActiveFont.Measure(UpgDesc_b).X * 0.75f) / 2;
                        }
                    }
                    ActiveFont.DrawOutline(UpgDesc, Position + new Vector2(960f, UpgDesc_b == null ? 960f : 940f), new Vector2(0.5f, 0.5f), Vector2.One * 0.75f, Color.White, 2f, Color.Black);
                    if (select)
                    {
                        if (!XaphanModule.useMetroidGameplay)
                        {
                            ActiveFont.Draw(selectHold, new Vector2(selectHoldPosition, vector.Y), new Vector2(0.5f, 0.5f), Vector2.One * 0.75f, Color.White);
                            ActiveFont.Draw(selectAndPress, new Vector2(selectAndPressPosition, vector.Y), new Vector2(0.5f, 0.5f), Vector2.One * 0.75f, Color.White);
                            ActiveFont.Draw(selectString, new Vector2(selectPosition, vector.Y), new Vector2(0.5f, 0.5f), Vector2.One * 0.75f, Color.White);
                            upgradeSprite.Position = new Vector2(SpritePosition, vector.Y);
                            if (controlB is VirtualButton)
                            {
                                MTexture buttonBTexture = Input.GuiButton((VirtualButton)controlB, "controls/keyboard/oemquestion");
                                buttonBTexture.DrawCentered(new Vector2(InputCPosition, vector.Y), Color.White, 0.5f);
                            }
                            else if (controlB is ButtonBinding)
                            {
                                ButtonBinding ControlB = (ButtonBinding)controlB;
                                ButtonB.Binding = ControlB.Binding;
                                MTexture buttonBTexture = Input.GuiButton(ButtonB, "controls/keyboard/oemquestion");
                                buttonATextureWidth = buttonBTexture.Width;
                                buttonBTexture.DrawCentered(new Vector2(InputCPosition, vector.Y), Color.White, 0.5f);
                            }
                        }
                        else
                        {
                            ActiveFont.Draw(selectString, new Vector2(selectPosition, vector.Y), new Vector2(0.5f, 0.5f), Vector2.One * 0.75f, Color.White);
                            upgradeSprite.Position = new Vector2(SpritePosition, vector.Y);
                        }
                    }
                    ActiveFont.Draw(inputActA, new Vector2(inputActAPosition, vector.Y), new Vector2(0.5f, 0.5f), Vector2.One * 0.75f, Color.White);
                    if (controlA is VirtualButton)
                    {
                        MTexture buttonATexture = Input.GuiButton((VirtualButton)controlA, "controls/keyboard/oemquestion");
                        buttonATexture.DrawCentered(new Vector2(InputAPosition, vector.Y), Color.White, 0.5f);
                    }
                    else if (controlA is ButtonBinding)
                    {
                        ButtonBinding ControlA = (ButtonBinding)controlA;
                        ButtonA.Binding = ControlA.Binding;
                        MTexture buttonATexture = Input.GuiButton(ButtonA, "controls/keyboard/oemquestion");
                        buttonATextureWidth = buttonATexture.Width;
                        buttonATexture.DrawCentered(new Vector2(InputAPosition, vector.Y), Color.White, 0.5f);
                    }
                    ActiveFont.Draw(inputActB, new Vector2(inputActBPosition, vector.Y), new Vector2(0.5f, 0.5f), Vector2.One * 0.75f, Color.White);
                    if (controlB is VirtualButton)
                    {
                        MTexture buttonBTexture = Input.GuiButton((VirtualButton)controlB, "controls/keyboard/oemquestion");
                        buttonBTexture.DrawCentered(new Vector2(InputBPosition, vector.Y), Color.White, 0.5f);
                    }
                    else if (controlB is ButtonBinding)
                    {
                        ButtonBinding ControlB = (ButtonBinding)controlB;
                        ButtonB.Binding = ControlB.Binding;
                        MTexture buttonBTexture = Input.GuiButton(ButtonB, "controls/keyboard/oemquestion");
                        buttonATextureWidth = buttonBTexture.Width;
                        buttonBTexture.DrawCentered(new Vector2(InputBPosition, vector.Y), Color.White, 0.5f);
                    }
                    if (select && upgradeSprite != null)
                    {
                        upgradeSprite.Play("static");
                        upgradeSprite.Render();
                    }
                    ActiveFont.Draw(UpgDesc_b, new Vector2(TextCPosition, vector.Y), new Vector2(0.5f, 0.5f), Vector2.One * 0.75f, Color.White);
                }
            }
            else
            {
                SectionName = Dialog.Clean("Xaphanhelper_UI_Metroid_Beams");
                SectionPosition = new Vector2(430f, 590f);
                SectionMaxLines = 5;
                ActiveFont.DrawOutline(SectionName, Position + SectionPosition, new Vector2(0.5f, 0.5f), Vector2.One * 1f, Color.Yellow, 2f, Color.Black);
                SectionTitleLenght = ActiveFont.Measure(SectionName).X;
                SectionTitleHeight = ActiveFont.Measure(SectionName).Y;
                Draw.Rect(Position + SectionPosition + new Vector2(SectionTitleLenght / 2 + 10, -4), 275f - (SectionTitleLenght / 2 + 10) + 15, 8f, Color.White);
                Draw.Rect(Position + SectionPosition + new Vector2(-275f - 15, -4), 275f - (SectionTitleLenght / 2 + 10) + 15, 8f, Color.White);
                Draw.Rect(Position + SectionPosition + new Vector2(SectionTitleLenght / 2 + 10 + 275f - (SectionTitleLenght / 2 + 10) + 5, -4), 10f, SectionMaxLines * 108f + SectionTitleHeight / 2 + 4, Color.White);
                Draw.Rect(Position + SectionPosition + new Vector2(-275f - 15, -4), 10f, SectionMaxLines * 108f + SectionTitleHeight / 2 + 4, Color.White);
                Draw.Rect(Position + SectionPosition + new Vector2(-275f - 15, SectionMaxLines * 108f + SectionTitleHeight / 2 - 8), 580f, 8f, Color.White);

                Draw.Line(Position + SectionPosition + new Vector2(SectionTitleLenght / 2 + 10 + 275f - (SectionTitleLenght / 2 + 10) + 15, (SectionMaxLines * 108f + 26) / 2), Position + SectionPosition + new Vector2(SectionTitleLenght / 2 + 10 + 275f - (SectionTitleLenght / 2 + 10) + 15 + 50, (SectionMaxLines * 108f + 26) / 2), Color.Black, 6f);
                Draw.Line(Position + SectionPosition + new Vector2(SectionTitleLenght / 2 + 10 + 275f - (SectionTitleLenght / 2 + 10) + 15 + 50, (SectionMaxLines * 108f + 26) / 2 + 2), Position + SectionPosition + new Vector2(SectionTitleLenght / 2 + 10 + 275f - (SectionTitleLenght / 2 + 10) + 15 + 50, (SectionMaxLines * 108f + 26) / 2 - 135), Color.Black, 6f);
                Draw.Line(Position + SectionPosition + new Vector2(SectionTitleLenght / 2 + 10 + 275f - (SectionTitleLenght / 2 + 10) + 15 + 48, (SectionMaxLines * 108f + 26) / 2 - 135), Position + SectionPosition + new Vector2(SectionTitleLenght / 2 + 10 + 275f - (SectionTitleLenght / 2 + 10) + 15 + 127, (SectionMaxLines * 108f + 26) / 2 - 135), Color.Black, 6f);
                Draw.Rect(Position + SectionPosition + new Vector2(SectionTitleLenght / 2 + 10 + 275f - (SectionTitleLenght / 2 + 10) + 15 + 127 - 7, (SectionMaxLines * 108f + 26) / 2 - 135 - 7), 14f, 14f, Color.Black);
                Draw.Line(Position + SectionPosition + new Vector2(SectionTitleLenght / 2 + 10 + 275f - (SectionTitleLenght / 2 + 10) + 15, (SectionMaxLines * 108f + 26) / 2), Position + SectionPosition + new Vector2(SectionTitleLenght / 2 + 10 + 275f - (SectionTitleLenght / 2 + 10) + 15 + 50, (SectionMaxLines * 108f + 26) / 2), Color.White, 4f);
                Draw.Line(Position + SectionPosition + new Vector2(SectionTitleLenght / 2 + 10 + 275f - (SectionTitleLenght / 2 + 10) + 15 + 50, (SectionMaxLines * 108f + 26) / 2 + 2), Position + SectionPosition + new Vector2(SectionTitleLenght / 2 + 10 + 275f - (SectionTitleLenght / 2 + 10) + 15 + 50, (SectionMaxLines * 108f + 26) / 2 - 135), Color.White, 4f);
                Draw.Line(Position + SectionPosition + new Vector2(SectionTitleLenght / 2 + 10 + 275f - (SectionTitleLenght / 2 + 10) + 15 + 48, (SectionMaxLines * 108f + 26) / 2 - 135), Position + SectionPosition + new Vector2(SectionTitleLenght / 2 + 10 + 275f - (SectionTitleLenght / 2 + 10) + 15 + 127, (SectionMaxLines * 108f + 26) / 2 - 135), Color.White, 4f);
                Draw.Rect(Position + SectionPosition + new Vector2(SectionTitleLenght / 2 + 10 + 275f - (SectionTitleLenght / 2 + 10) + 15 + 127 - 6, (SectionMaxLines * 108f + 26) / 2 - 135 - 6), 12f, 12f, Color.White);

                SectionName = Dialog.Clean("Xaphanhelper_UI_Metroid_Suits");
                SectionPosition = new Vector2(1490f, 230f);
                SectionMaxLines = 2;
                ActiveFont.DrawOutline(SectionName, Position + SectionPosition, new Vector2(0.5f, 0.5f), Vector2.One * 1f, Color.Yellow, 2f, Color.Black);
                SectionTitleLenght = ActiveFont.Measure(SectionName).X;
                SectionTitleHeight = ActiveFont.Measure(SectionName).Y;
                Draw.Rect(Position + SectionPosition + new Vector2(SectionTitleLenght / 2 + 10, -4), 275f - (SectionTitleLenght / 2 + 10) + 15, 8f, Color.White);
                Draw.Rect(Position + SectionPosition + new Vector2(-275f - 15, -4), 275f - (SectionTitleLenght / 2 + 10) + 15, 8f, Color.White);
                Draw.Rect(Position + SectionPosition + new Vector2(SectionTitleLenght / 2 + 10 + 275f - (SectionTitleLenght / 2 + 10) + 5, -4), 10f, SectionMaxLines * 108f + SectionTitleHeight / 2 + 4, Color.White);
                Draw.Rect(Position + SectionPosition + new Vector2(-275f - 15, -4), 10f, SectionMaxLines * 108f + SectionTitleHeight / 2 + 4, Color.White);
                Draw.Rect(Position + SectionPosition + new Vector2(-275f - 15, SectionMaxLines * 108f + SectionTitleHeight / 2 - 8), 580f, 8f, Color.White);

                Draw.Line(Position + SectionPosition + new Vector2(-275f - 15, (SectionMaxLines * 108f + 26) / 2), Position + SectionPosition + new Vector2(-275f - 15 - 50, (SectionMaxLines * 108f + 26) / 2), Color.Black, 6f);
                Draw.Line(Position + SectionPosition + new Vector2(-275f - 15 - 50, (SectionMaxLines * 108f + 26) / 2 - 2), Position + SectionPosition + new Vector2(-275f - 15 - 50, (SectionMaxLines * 108f + 26) / 2 + 108), Color.Black, 6f);
                Draw.Line(Position + SectionPosition + new Vector2(-275f - 15 - 48, (SectionMaxLines * 108f + 26) / 2 + 108), Position + SectionPosition + new Vector2(-275f - 15 - 240, (SectionMaxLines * 108f + 26) / 2 + 108), Color.Black, 6f);
                Draw.Rect(Position + SectionPosition + new Vector2(-275f - 15 - 240 - 7, (SectionMaxLines * 108f + 26) / 2 + 108 - 7), 14f, 14f, Color.Black);
                Draw.Line(Position + SectionPosition + new Vector2(-275f - 15, (SectionMaxLines * 108f + 26) / 2), Position + SectionPosition + new Vector2(-275f - 15 - 50, (SectionMaxLines * 108f + 26) / 2), Color.White, 4f);
                Draw.Line(Position + SectionPosition + new Vector2(-275f - 15 - 50, (SectionMaxLines * 108f + 26) / 2 - 2), Position + SectionPosition + new Vector2(-275f - 15 - 50, (SectionMaxLines * 108f + 26) / 2 + 108), Color.White, 4f);
                Draw.Line(Position + SectionPosition + new Vector2(-275f - 15 - 48, (SectionMaxLines * 108f + 26) / 2 + 108), Position + SectionPosition + new Vector2(-275f - 15 - 240, (SectionMaxLines * 108f + 26) / 2 + 108), Color.White, 4f);
                Draw.Rect(Position + SectionPosition + new Vector2(-275f - 15 - 240 - 6, (SectionMaxLines * 108f + 26) / 2 + 108 - 6), 12f, 12f, Color.White);

                SectionName = Dialog.Clean("Xaphanhelper_UI_Metroid_Misc");
                SectionPosition = new Vector2(1490f, 440f);
                SectionMaxLines = 4;
                ActiveFont.DrawOutline(SectionName, Position + SectionPosition, new Vector2(0.5f, 0.5f), Vector2.One * 1f, Color.Yellow, 2f, Color.Black);
                SectionTitleLenght = ActiveFont.Measure(SectionName).X;
                SectionTitleHeight = ActiveFont.Measure(SectionName).Y;
                Draw.Rect(Position + SectionPosition + new Vector2(SectionTitleLenght / 2 + 10, -4), 275f - (SectionTitleLenght / 2 + 10) + 15, 8f, Color.White);
                Draw.Rect(Position + SectionPosition + new Vector2(-275f - 15, -4), 275f - (SectionTitleLenght / 2 + 10) + 15, 8f, Color.White);
                Draw.Rect(Position + SectionPosition + new Vector2(SectionTitleLenght / 2 + 10 + 275f - (SectionTitleLenght / 2 + 10) + 5, -4), 10f, SectionMaxLines * 108f + SectionTitleHeight / 2 + 4, Color.White);
                Draw.Rect(Position + SectionPosition + new Vector2(-275f - 15, -4), 10f, SectionMaxLines * 108f + SectionTitleHeight / 2 + 4, Color.White);
                Draw.Rect(Position + SectionPosition + new Vector2(-275f - 15, SectionMaxLines * 108f + SectionTitleHeight / 2 - 8), 580f, 8f, Color.White);

                Draw.Line(Position + SectionPosition + new Vector2(-275f - 15, (SectionMaxLines * 108f + 26) / 2), Position + SectionPosition + new Vector2(-275f - 15 - 50, (SectionMaxLines * 108f + 26) / 2), Color.Black, 6f);
                Draw.Line(Position + SectionPosition + new Vector2(-275f - 15 - 50, (SectionMaxLines * 108f + 26) / 2 + 2), Position + SectionPosition + new Vector2(-275f - 15 - 50, (SectionMaxLines * 108f + 26) / 2 - 101), Color.Black, 6f);
                Draw.Line(Position + SectionPosition + new Vector2(-275f - 15 - 48, (SectionMaxLines * 108f + 26) / 2 - 101), Position + SectionPosition + new Vector2(-275f - 15 - 240, (SectionMaxLines * 108f + 26) / 2 - 101), Color.Black, 6f);
                Draw.Rect(Position + SectionPosition + new Vector2(-275f - 15 - 240 - 7, (SectionMaxLines * 108f + 26) / 2 - 101 - 7), 14f, 14f, Color.Black);
                Draw.Line(Position + SectionPosition + new Vector2(-275f - 15, (SectionMaxLines * 108f + 26) / 2), Position + SectionPosition + new Vector2(-275f - 15 - 50, (SectionMaxLines * 108f + 26) / 2), Color.White, 4f);
                Draw.Line(Position + SectionPosition + new Vector2(-275f - 15 - 50, (SectionMaxLines * 108f + 26) / 2 + 2), Position + SectionPosition + new Vector2(-275f - 15 - 50, (SectionMaxLines * 108f + 26) / 2 - 101), Color.White, 4f);
                Draw.Line(Position + SectionPosition + new Vector2(-275f - 15 - 48, (SectionMaxLines * 108f + 26) / 2 - 101), Position + SectionPosition + new Vector2(-275f - 15 - 240, (SectionMaxLines * 108f + 26) / 2 - 101), Color.White, 4f);
                Draw.Rect(Position + SectionPosition + new Vector2(-275f - 15 - 240 - 6, (SectionMaxLines * 108f + 26) / 2 - 101 - 6), 12f, 12f, Color.White);

                SectionName = Dialog.Clean("Xaphanhelper_UI_Metroid_Boots");
                SectionPosition = new Vector2(1490f, 770f);
                SectionMaxLines = 3;
                ActiveFont.DrawOutline(SectionName, Position + SectionPosition, new Vector2(0.5f, 0.5f), Vector2.One * 1f, Color.Yellow, 2f, Color.Black);
                SectionTitleLenght = ActiveFont.Measure(SectionName).X;
                SectionTitleHeight = ActiveFont.Measure(SectionName).Y;
                Draw.Rect(Position + SectionPosition + new Vector2(SectionTitleLenght / 2 + 10, -4), 275f - (SectionTitleLenght / 2 + 10) + 15, 8f, Color.White);
                Draw.Rect(Position + SectionPosition + new Vector2(-275f - 15, -4), 275f - (SectionTitleLenght / 2 + 10) + 15, 8f, Color.White);
                Draw.Rect(Position + SectionPosition + new Vector2(SectionTitleLenght / 2 + 10 + 275f - (SectionTitleLenght / 2 + 10) + 5, -4), 10f, SectionMaxLines * 108f + SectionTitleHeight / 2 + 4, Color.White);
                Draw.Rect(Position + SectionPosition + new Vector2(-275f - 15, -4), 10f, SectionMaxLines * 108f + SectionTitleHeight / 2 + 4, Color.White);
                Draw.Rect(Position + SectionPosition + new Vector2(-275f - 15, SectionMaxLines * 108f + SectionTitleHeight / 2 - 8), 580f, 8f, Color.White);

                Draw.Line(Position + SectionPosition + new Vector2(-275f - 15, (SectionMaxLines * 108f + 26) / 2), Position + SectionPosition + new Vector2(-275f - 15 - 50, (SectionMaxLines * 108f + 26) / 2), Color.Black, 6f);
                Draw.Line(Position + SectionPosition + new Vector2(-275f - 15 - 50, (SectionMaxLines * 108f + 26) / 2 + 2), Position + SectionPosition + new Vector2(-275f - 15 - 50, (SectionMaxLines * 108f + 26) / 2 - 50), Color.Black, 6f);
                Draw.Line(Position + SectionPosition + new Vector2(-275f - 15 - 48, (SectionMaxLines * 108f + 26) / 2 - 50), Position + SectionPosition + new Vector2(-275f - 15 - 159, (SectionMaxLines * 108f + 26) / 2 - 50), Color.Black, 6f);
                Draw.Rect(Position + SectionPosition + new Vector2(-275f - 15 - 159 - 7, (SectionMaxLines * 108f + 26) / 2 - 50 - 7), 14f, 14f, Color.Black);
                Draw.Line(Position + SectionPosition + new Vector2(-275f - 15, (SectionMaxLines * 108f + 26) / 2), Position + SectionPosition + new Vector2(-275f - 15 - 50, (SectionMaxLines * 108f + 26) / 2), Color.White, 4f);
                Draw.Line(Position + SectionPosition + new Vector2(-275f - 15 - 50, (SectionMaxLines * 108f + 26) / 2 + 2), Position + SectionPosition + new Vector2(-275f - 15 - 50, (SectionMaxLines * 108f + 26) / 2 - 50), Color.White, 4f);
                Draw.Line(Position + SectionPosition + new Vector2(-275f - 15 - 48, (SectionMaxLines * 108f + 26) / 2 - 50), Position + SectionPosition + new Vector2(-275f - 15 - 159, (SectionMaxLines * 108f + 26) / 2 - 50), Color.White, 4f);
                Draw.Rect(Position + SectionPosition + new Vector2(-275f - 15 - 159 - 6, (SectionMaxLines * 108f + 26) / 2 - 50 - 6), 12f, 12f, Color.White);
            }
        }
    }
}
