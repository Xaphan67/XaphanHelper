using Celeste.Mod.XaphanHelper.Data;
using Celeste.Mod.XaphanHelper.Upgrades;
using Microsoft.Xna.Framework;
using Monocle;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace Celeste.Mod.XaphanHelper.UI_Elements
{
    class StatusDisplay : Entity
    {
        [Tracked(true)]
        public class UpgradeDisplay : Entity
        {
            protected static XaphanModuleSettings Settings => XaphanModule.Settings;

            public int ID;

            public float width = 550f;

            public float height = 50f;

            private string Name;

            public HashSet<string> InactiveList;

            private Sprite Sprite;

            private StatusScreen StatusScreen;

            private StatusDisplay Display;

            private string LevelSet;

            public bool Selected;

            private float selectedAlpha = 0;

            private int alphaStatus = 0;

            public string internalName;

            public UpgradeDisplay(Level level, StatusDisplay display, Vector2 position, int id, string name, string spritePath, string spriteName, HashSet<string> inactiveList) : base(position)
            {
                Tag = Tags.HUD;
                Display = display;
                ID = id;
                Name = Dialog.Clean(name);
                InactiveList = inactiveList;
                Sprite = new Sprite(GFX.Gui, spritePath + "/");
                Sprite.AddLoop("on", spriteName, 0.05f, 0);
                Sprite.Play("on");
                Sprite.Position = position + new Vector2(1, 1);
                StatusScreen = level.Tracker.GetEntity<StatusScreen>();
                LevelSet = level.Session.Area.GetLevelSet();
                internalName = spriteName;
                Depth = -10001;
            }

            public override void Update()
            {
                base.Update();
                if (StatusScreen.Selection == ID)
                {
                    Selected = true;
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
            }

            public override void Render()
            {
                base.Render();
                float lenght = ActiveFont.Measure(Name).X * 0.85f;
                bool smallText = false;
                if (lenght > 500f)
                {
                    lenght = ActiveFont.Measure(Name).X * 0.75f;
                    smallText = true;
                }
                if (Selected)
                {
                    Draw.Rect(Position, width, height, Color.Yellow * selectedAlpha);
                }
                Sprite.Render();
                ActiveFont.DrawOutline(Name, Position + new Vector2(70 + lenght / 2 - 10, height / 2), new Vector2(0.5f, 0.5f), Vector2.One * (smallText ? 0.75f : 0.85f), !InactiveList.Contains(LevelSet) ? Color.White : Color.Gray, 2f, Color.Black);
            }
        }

        protected XaphanModuleSettings Settings => XaphanModule.Settings;

        public MapData MapData;

        private Level level;

        private Sprite PlayerSprite;

        public HashSet<int> LeftDisplays = new HashSet<int>();

        public HashSet<int> RightDisplays = new HashSet<int>();

        public UpgradeDisplay SelectedDisplay;

        public List<CustomUpgradesData> CustomUpgradesData  = new List<Data.CustomUpgradesData>();

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
            PlayerSprite.Position = Position + new Vector2(Engine.Width / 2 - PlayerSprite.Width / 2, XaphanModule.useMetroidGameplay ? 600 - PlayerSprite.Height / 2 : 550 - PlayerSprite.Height / 2);
            Depth = -10001;
        }

        public override void Added(Scene scene)
        {
            base.Added(scene);
            foreach (string VisitedChapter in XaphanModule.ModSaveData.VisitedChapters)
            {
                string[] str = VisitedChapter.Split('_');
                if (str[0] == level.Session.Area.GetLevelSet())
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
                if (display.ID <= 11)
                {
                    if (!LeftDisplays.Contains(display.ID))
                    {
                        LeftDisplays.Add(display.ID);
                    }
                }
                if (display.ID >= 12)
                {
                    if (!RightDisplays.Contains(display.ID))
                    {
                        RightDisplays.Add(display.ID);
                    }
                }
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
                if (Settings.PowerGrip)
                {
                    Scene.Add(new UpgradeDisplay(level, this, new Vector2(155f, 225f), 1, getCustomName("PowerGrip"), getCustomSpritePath("PowerGrip"), "PowerGrip", XaphanModule.ModSaveData.PowerGripInactive));
                }
                if (Settings.ClimbingKit)
                {
                    Scene.Add(new UpgradeDisplay(level, this, new Vector2(155f, 275f), 2, getCustomName("ClimbingKit"), getCustomSpritePath("ClimbingKit"), "ClimbingKit", XaphanModule.ModSaveData.ClimbingKitInactive));
                }
                if (Settings.SpiderMagnet)
                {
                    Scene.Add(new UpgradeDisplay(level, this, new Vector2(155f, 325f), 3, getCustomName("SpiderMagnet"), getCustomSpritePath("SpiderMagnet"), "SpiderMagnet", XaphanModule.ModSaveData.SpiderMagnetInactive));
                }
                if (Settings.DashBoots)
                {
                    Scene.Add(new UpgradeDisplay(level, this, new Vector2(155f, 430f), 4, getCustomName("DashBoots"), getCustomSpritePath("DashBoots"), "DashBoots", XaphanModule.ModSaveData.DashBootsInactive));
                }
                if (Settings.SpaceJump == 2)
                {
                    Scene.Add(new UpgradeDisplay(level, this, new Vector2(155f, 480f), 5, getCustomName("SpaceJump"), getCustomSpritePath("SpaceJump"), "SpaceJump", XaphanModule.ModSaveData.SpaceJumpInactive));
                }
                if (Settings.HoverBoots)
                {
                    Scene.Add(new UpgradeDisplay(level, this, new Vector2(155f, 530f), 6, getCustomName("HoverBoots"), getCustomSpritePath("HoverBoots"), "HoverBoots", XaphanModule.ModSaveData.HoverBootsInactive));
                }
                if (Settings.LightningDash)
                {
                    Scene.Add(new UpgradeDisplay(level, this, new Vector2(155f, 580f), 7, getCustomName("LightningDash"), getCustomSpritePath("LightningDash"), "LightningDash", XaphanModule.ModSaveData.LightningDashInactive));
                }
                if (Settings.LongBeam)
                {
                    Scene.Add(new UpgradeDisplay(level, this, new Vector2(155f, 685f), 8, getCustomName("LongBeam"), getCustomSpritePath("LongBeam"), "LongBeam", XaphanModule.ModSaveData.LongBeamInactive));
                }
                if (Settings.IceBeam)
                {
                    Scene.Add(new UpgradeDisplay(level, this, new Vector2(155f, 735f), 9, getCustomName("IceBeam"), getCustomSpritePath("IceBeam"), "IceBeam", XaphanModule.ModSaveData.IceBeamInactive));
                }
                if (Settings.WaveBeam)
                {
                    Scene.Add(new UpgradeDisplay(level, this, new Vector2(155f, 785f), 10, getCustomName("WaveBeam"), getCustomSpritePath("WaveBeam"), "WaveBeam", XaphanModule.ModSaveData.WaveBeamInactive));
                }
                if (Settings.DroneTeleport)
                {
                    Scene.Add(new UpgradeDisplay(level, this, new Vector2(155f, 835f), 11, getCustomName("DroneTeleport"), getCustomSpritePath("DroneTeleport"), "DroneTeleport", XaphanModule.ModSaveData.DroneTeleportInactive));
                }
                if (Settings.VariaJacket)
                {
                    Scene.Add(new UpgradeDisplay(level, this, new Vector2(1215f, 225f), 12, getCustomName("VariaJacket"), getCustomSpritePath("VariaJacket"), "VariaJacket", XaphanModule.ModSaveData.VariaJacketInactive));
                }
                if (Settings.GravityJacket)
                {
                    Scene.Add(new UpgradeDisplay(level, this, new Vector2(1215f, 275f), 13, getCustomName("GravityJacket"), getCustomSpritePath("GravityJacket"), "GravityJacket", XaphanModule.ModSaveData.GravityJacketInactive));
                }
                if (Settings.Bombs)
                {
                    Scene.Add(new UpgradeDisplay(level, this, new Vector2(1215f, 380f), 14, getCustomName("Bombs"), getCustomSpritePath("Bombs"), "Bombs", XaphanModule.ModSaveData.BombsInactive));
                }
                if (Settings.MegaBombs)
                {
                    Scene.Add(new UpgradeDisplay(level, this, new Vector2(1215f, 430f), 15, getCustomName("MegaBombs"), getCustomSpritePath("MegaBombs"), "MegaBombs", XaphanModule.ModSaveData.MegaBombsInactive));
                }
                if (Settings.RemoteDrone)
                {
                    Scene.Add(new UpgradeDisplay(level, this, new Vector2(1215f, 480f), 16, getCustomName("RemoteDrone"), getCustomSpritePath("RemoteDrone"), "RemoteDrone", XaphanModule.ModSaveData.RemoteDroneInactive));
                }
                /*if (Settings.JumpBoost)
                {
                    Scene.Add(new UpgradeDisplay(level, this, new Vector2(1215f, 470f), 14, getCustomName("JumpBoost"), getCustomSpritePath("JumpBoost"), "JumpBoost", XaphanModule.ModSaveData.JumpBoostInactive));
                }*/
                if (Settings.GoldenFeather)
                {
                    Scene.Add(new UpgradeDisplay(level, this, new Vector2(1215f, 585f), 17, getCustomName("GoldenFeather"), getCustomSpritePath("GoldenFeather"), "GoldenFeather", XaphanModule.ModSaveData.GoldenFeatherInactive));
                }
                if (Settings.EtherealDash)
                {
                    Scene.Add(new UpgradeDisplay(level, this, new Vector2(1215f, 635f), 18, getCustomName("EtherealDash"), getCustomSpritePath("EtherealDash"), "EtherealDash", XaphanModule.ModSaveData.EtherealDashInactive));
                }
                if (Settings.ScrewAttack)
                {
                    Scene.Add(new UpgradeDisplay(level, this, new Vector2(1215f, 685f), 19, getCustomName("ScrewAttack"), getCustomSpritePath("ScrewAttack"), "ScrewAttack", XaphanModule.ModSaveData.ScrewAttackInactive));
                }
                if (Settings.Binoculars)
                {
                    Scene.Add(new UpgradeDisplay(level, this, new Vector2(1215f, 735f), 20, getCustomName("Binoculars"), getCustomSpritePath("Binoculars"), "Binoculars", XaphanModule.ModSaveData.BinocularsInactive));
                }
                if (Settings.PortableStation)
                {
                    Scene.Add(new UpgradeDisplay(level, this, new Vector2(1215f, 785f), 21, getCustomName("PortableStation"), getCustomSpritePath("PortableStation"), "PortableStation", XaphanModule.ModSaveData.PortableStationInactive));
                }
                if (Settings.PulseRadar)
                {
                    Scene.Add(new UpgradeDisplay(level, this, new Vector2(1215f, 835f), 22, getCustomName("PulseRadar"), getCustomSpritePath("PulseRadar"), "PulseRadar", XaphanModule.ModSaveData.PulseRadarInactive));
                }
            }
            else
            {
                if (Settings.LongBeam)
                {
                    Scene.Add(new UpgradeDisplay(level, this, new Vector2(155f, 610f), 6, "XaphanHelper_get_LongBeam_Name", "collectables/XaphanHelper/SamusUpgradeCollectable", "LongBeam", XaphanModule.ModSaveData.LongBeamInactive));
                }
                if (Settings.IceBeam)
                {
                    Scene.Add(new UpgradeDisplay(level, this, new Vector2(155f, 670f), 7, "XaphanHelper_get_IceBeam_Name", "collectables/XaphanHelper/SamusUpgradeCollectable", "IceBeam", XaphanModule.ModSaveData.IceBeamInactive));
                }
                if (Settings.WaveBeam)
                {
                    Scene.Add(new UpgradeDisplay(level, this, new Vector2(155f, 730f), 8, "XaphanHelper_get_WaveBeam_Name", "collectables/XaphanHelper/SamusUpgradeCollectable", "WaveBeam", XaphanModule.ModSaveData.WaveBeamInactive));
                }
                if (Settings.Spazer)
                {
                    Scene.Add(new UpgradeDisplay(level, this, new Vector2(155f, 790f), 9, "XaphanHelper_get_Spazer_Name", "collectables/XaphanHelper/SamusUpgradeCollectable", "Spazer", XaphanModule.ModSaveData.SpazerInactive));
                }
                if (Settings.PlasmaBeam)
                {
                    Scene.Add(new UpgradeDisplay(level, this, new Vector2(155f, 850f), 10, "XaphanHelper_get_PlasmaBeam_Name", "collectables/XaphanHelper/SamusUpgradeCollectable", "PlasmaBeam", XaphanModule.ModSaveData.PlasmaBeamInactive));
                }
                if (Settings.VariaJacket)
                {
                    Scene.Add(new UpgradeDisplay(level, this, new Vector2(1215f, 250f), 11, "XaphanHelper_get_Met_VariaJacket_Name", "collectables/XaphanHelper/SamusUpgradeCollectable", "VariaJacket", XaphanModule.ModSaveData.VariaJacketInactive));
                }
                if (Settings.GravityJacket)
                {
                    Scene.Add(new UpgradeDisplay(level, this, new Vector2(1215f, 310f), 12, "XaphanHelper_get_Met_GravityJacket_Name", "collectables/XaphanHelper/SamusUpgradeCollectable", "GravityJacket", XaphanModule.ModSaveData.GravityJacketInactive));
                }
                if (Settings.MorphingBall)
                {
                    Scene.Add(new UpgradeDisplay(level, this, new Vector2(1215f, 460f), 13, "XaphanHelper_get_MorphingBall_Name", "collectables/XaphanHelper/SamusUpgradeCollectable", "MorphingBall", XaphanModule.ModSaveData.MorphingBallInactive));
                }
                if (Settings.MorphBombs)
                {
                    Scene.Add(new UpgradeDisplay(level, this, new Vector2(1215f, 520f), 14, "XaphanHelper_get_MorphBombs_Name", "collectables/XaphanHelper/SamusUpgradeCollectable", "MorphBombs", XaphanModule.ModSaveData.MorphBombsInactive));
                }
                if (Settings.SpringBall)
                {
                    Scene.Add(new UpgradeDisplay(level, this, new Vector2(1215f, 580f), 15, "XaphanHelper_get_SpringBall_Name", "collectables/XaphanHelper/SamusUpgradeCollectable", "SpringBall", XaphanModule.ModSaveData.SpringBallInactive));
                }
                if (Settings.ScrewAttack)
                {
                    Scene.Add(new UpgradeDisplay(level, this, new Vector2(1215f, 640f), 16, "XaphanHelper_get_Met_ScrewAttack_Name", "collectables/XaphanHelper/SamusUpgradeCollectable", "ScrewAttack", XaphanModule.ModSaveData.ScrewAttackInactive));
                }
                if (Settings.HighJumpBoots)
                {
                    Scene.Add(new UpgradeDisplay(level, this, new Vector2(1215f, 790f), 17, "XaphanHelper_get_HighJumpBoots_Name", "collectables/XaphanHelper/SamusUpgradeCollectable", "HighJumpBoots", XaphanModule.ModSaveData.HighJumpBootsInactive));
                }
                if (Settings.SpaceJump == 6)
                {
                    Scene.Add(new UpgradeDisplay(level, this, new Vector2(1215f, 850f), 18, "XaphanHelper_get_Met_SpaceJump_Name", "collectables/XaphanHelper/SamusUpgradeCollectable", "SpaceJump", XaphanModule.ModSaveData.SpaceJumpInactive));
                }
                if (Settings.SpeedBooster)
                {
                    Scene.Add(new UpgradeDisplay(level, this, new Vector2(1215f, 910f), 19, "XaphanHelper_get_SpeedBooster_Name", "collectables/XaphanHelper/SamusUpgradeCollectable", "SpeedBooster", XaphanModule.ModSaveData.SpeedBoosterInactive));
                }
            }
            yield return null;
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
            int SectionMaxUpgrades;
            if (!XaphanModule.useMetroidGameplay)
            {
                SectionName = Dialog.Clean("Xaphanhelper_UI_Arms");
                SectionPosition = new Vector2(430f, 205f);
                SectionMaxUpgrades = 3;
                ActiveFont.DrawOutline(SectionName, Position + SectionPosition, new Vector2(0.5f, 0.5f), Vector2.One * 1f, Color.Yellow, 2f, Color.Black);
                SectionTitleLenght = ActiveFont.Measure(SectionName).X;
                SectionTitleHeight = ActiveFont.Measure(SectionName).Y;
                Draw.Rect(Position + SectionPosition + new Vector2(SectionTitleLenght / 2 + 10, -4), 275f - (SectionTitleLenght / 2 + 10) + 15, 8f, Color.White);
                Draw.Rect(Position + SectionPosition + new Vector2(-275f - 15, -4), 275f - (SectionTitleLenght / 2 + 10) + 15, 8f, Color.White);
                Draw.Rect(Position + SectionPosition + new Vector2(SectionTitleLenght / 2 + 10 + 275f - (SectionTitleLenght / 2 + 10) + 5, -4), 10f, SectionMaxUpgrades * 50f + SectionTitleHeight / 2 + 4, Color.White);
                Draw.Rect(Position + SectionPosition + new Vector2(-275f - 15, -4), 10f, SectionMaxUpgrades * 50f + SectionTitleHeight / 2 + 4, Color.White);
                Draw.Rect(Position + SectionPosition + new Vector2(-275f - 15, SectionMaxUpgrades * 50f + SectionTitleHeight / 2 - 8), 580f, 8f, Color.White);

                Draw.Line(Position + SectionPosition + new Vector2(SectionTitleLenght / 2 + 10 + 275f - (SectionTitleLenght / 2 + 10) + 15, (SectionMaxUpgrades * 50f + 26) / 2), Position + SectionPosition + new Vector2(SectionTitleLenght / 2 + 10 + 275f - (SectionTitleLenght / 2 + 10) + 15 + 50, (SectionMaxUpgrades * 50f + 26) / 2), Color.Black, 6f);
                Draw.Line(Position + SectionPosition + new Vector2(SectionTitleLenght / 2 + 10 + 275f - (SectionTitleLenght / 2 + 10) + 15 + 50, (SectionMaxUpgrades * 50f + 26) / 2 - 2), Position + SectionPosition + new Vector2(SectionTitleLenght / 2 + 10 + 275f - (SectionTitleLenght / 2 + 10) + 15 + 50, (SectionMaxUpgrades * 50f + 26) / 2 + 188), Color.Black, 6f);
                Draw.Line(Position + SectionPosition + new Vector2(SectionTitleLenght / 2 + 10 + 275f - (SectionTitleLenght / 2 + 10) + 15 + 48, (SectionMaxUpgrades * 50f + 26) / 2 + 188), Position + SectionPosition + new Vector2(SectionTitleLenght / 2 + 10 + 275f - (SectionTitleLenght / 2 + 10) + 15 + 170, (SectionMaxUpgrades * 50f + 26) / 2 + 188), Color.Black, 6f);
                Draw.Rect(Position + SectionPosition + new Vector2(SectionTitleLenght / 2 + 10 + 275f - (SectionTitleLenght / 2 + 10) + 15 + 170 - 7, (SectionMaxUpgrades * 50f + 26) / 2 + 188 - 7), 14f, 14f, Color.Black);
                Draw.Line(Position + SectionPosition + new Vector2(SectionTitleLenght / 2 + 10 + 275f - (SectionTitleLenght / 2 + 10) + 15, (SectionMaxUpgrades * 50f + 26) / 2), Position + SectionPosition + new Vector2(SectionTitleLenght / 2 + 10 + 275f - (SectionTitleLenght / 2 + 10) + 15 + 50, (SectionMaxUpgrades * 50f + 26) / 2), Color.White, 4f);
                Draw.Line(Position + SectionPosition + new Vector2(SectionTitleLenght / 2 + 10 + 275f - (SectionTitleLenght / 2 + 10) + 15 + 50, (SectionMaxUpgrades * 50f + 26) / 2 - 2), Position + SectionPosition + new Vector2(SectionTitleLenght / 2 + 10 + 275f - (SectionTitleLenght / 2 + 10) + 15 + 50, (SectionMaxUpgrades * 50f + 26) / 2 + 188), Color.White, 4f);
                Draw.Line(Position + SectionPosition + new Vector2(SectionTitleLenght / 2 + 10 + 275f - (SectionTitleLenght / 2 + 10) + 15 + 48, (SectionMaxUpgrades * 50f + 26) / 2 + 188), Position + SectionPosition + new Vector2(SectionTitleLenght / 2 + 10 + 275f - (SectionTitleLenght / 2 + 10) + 15 + 170, (SectionMaxUpgrades * 50f + 26) / 2 + 188), Color.White, 4f);
                Draw.Rect(Position + SectionPosition + new Vector2(SectionTitleLenght / 2 + 10 + 275f - (SectionTitleLenght / 2 + 10) + 15 + 170 - 6, (SectionMaxUpgrades * 50f + 26) / 2 + 188 - 6), 12f, 12f, Color.White);

                SectionName = Dialog.Clean("Xaphanhelper_UI_Legs");
                SectionPosition = new Vector2(430f, 410f);
                SectionMaxUpgrades = 4;
                ActiveFont.DrawOutline(SectionName, Position + SectionPosition, new Vector2(0.5f, 0.5f), Vector2.One * 1f, Color.Yellow, 2f, Color.Black);
                SectionTitleLenght = ActiveFont.Measure(SectionName).X;
                SectionTitleHeight = ActiveFont.Measure(SectionName).Y;
                Draw.Rect(Position + SectionPosition + new Vector2(SectionTitleLenght / 2 + 10, -4), 275f - (SectionTitleLenght / 2 + 10) + 15, 8f, Color.White);
                Draw.Rect(Position + SectionPosition + new Vector2(-275f - 15, -4), 275f - (SectionTitleLenght / 2 + 10) + 15, 8f, Color.White);
                Draw.Rect(Position + SectionPosition + new Vector2(SectionTitleLenght / 2 + 10 + 275f - (SectionTitleLenght / 2 + 10) + 5, -4), 10f, SectionMaxUpgrades * 50f + SectionTitleHeight / 2 + 4, Color.White);
                Draw.Rect(Position + SectionPosition + new Vector2(-275f - 15, -4), 10f, SectionMaxUpgrades * 50f + SectionTitleHeight / 2 + 4, Color.White);
                Draw.Rect(Position + SectionPosition + new Vector2(-275f - 15, SectionMaxUpgrades * 50f + SectionTitleHeight / 2 - 8), 580f, 8f, Color.White);

                Draw.Line(Position + SectionPosition + new Vector2(SectionTitleLenght / 2 + 10 + 275f - (SectionTitleLenght / 2 + 10) + 15, (SectionMaxUpgrades * 50f + 26) / 2), Position + SectionPosition + new Vector2(SectionTitleLenght / 2 + 10 + 275f - (SectionTitleLenght / 2 + 10) + 15 + 50, (SectionMaxUpgrades * 50f + 26) / 2), Color.Black, 6f);
                Draw.Line(Position + SectionPosition + new Vector2(SectionTitleLenght / 2 + 10 + 275f - (SectionTitleLenght / 2 + 10) + 15 + 50, (SectionMaxUpgrades * 50f + 26) / 2 - 2), Position + SectionPosition + new Vector2(SectionTitleLenght / 2 + 10 + 275f - (SectionTitleLenght / 2 + 10) + 15 + 50, (SectionMaxUpgrades * 50f + 26) / 2 + 200), Color.Black, 6f);
                Draw.Line(Position + SectionPosition + new Vector2(SectionTitleLenght / 2 + 10 + 275f - (SectionTitleLenght / 2 + 10) + 15 + 48, (SectionMaxUpgrades * 50f + 26) / 2 + 200), Position + SectionPosition + new Vector2(SectionTitleLenght / 2 + 10 + 275f - (SectionTitleLenght / 2 + 10) + 15 + 155, (SectionMaxUpgrades * 50f + 26) / 2 + 200), Color.Black, 6f);
                Draw.Rect(Position + SectionPosition + new Vector2(SectionTitleLenght / 2 + 10 + 275f - (SectionTitleLenght / 2 + 10) + 15 + 155 - 7, (SectionMaxUpgrades * 50f + 26) / 2 + 200 - 7), 14f, 14f, Color.Black);
                Draw.Line(Position + SectionPosition + new Vector2(SectionTitleLenght / 2 + 10 + 275f - (SectionTitleLenght / 2 + 10) + 15, (SectionMaxUpgrades * 50f + 26) / 2), Position + SectionPosition + new Vector2(SectionTitleLenght / 2 + 10 + 275f - (SectionTitleLenght / 2 + 10) + 15 + 50, (SectionMaxUpgrades * 50f + 26) / 2), Color.White, 4f);
                Draw.Line(Position + SectionPosition + new Vector2(SectionTitleLenght / 2 + 10 + 275f - (SectionTitleLenght / 2 + 10) + 15 + 50, (SectionMaxUpgrades * 50f + 26) / 2 - 2), Position + SectionPosition + new Vector2(SectionTitleLenght / 2 + 10 + 275f - (SectionTitleLenght / 2 + 10) + 15 + 50, (SectionMaxUpgrades * 50f + 26) / 2 + 200), Color.White, 4f);
                Draw.Line(Position + SectionPosition + new Vector2(SectionTitleLenght / 2 + 10 + 275f - (SectionTitleLenght / 2 + 10) + 15 + 48, (SectionMaxUpgrades * 50f + 26) / 2 + 200), Position + SectionPosition + new Vector2(SectionTitleLenght / 2 + 10 + 275f - (SectionTitleLenght / 2 + 10) + 15 + 155, (SectionMaxUpgrades * 50f + 26) / 2 + 200), Color.White, 4f);
                Draw.Rect(Position + SectionPosition + new Vector2(SectionTitleLenght / 2 + 10 + 275f - (SectionTitleLenght / 2 + 10) + 15 + 155 - 6, (SectionMaxUpgrades * 50f + 26) / 2 + 200 - 6), 12f, 12f, Color.White);

                SectionName = Settings.RemoteDrone ? Dialog.Clean("Xaphanhelper_UI_Drone") : "???";
                SectionPosition = new Vector2(430f, 665f);
                SectionMaxUpgrades = 4;
                ActiveFont.DrawOutline(SectionName, Position + SectionPosition, new Vector2(0.5f, 0.5f), Vector2.One * 1f, Color.Yellow, 2f, Color.Black);
                SectionTitleLenght = ActiveFont.Measure(SectionName).X;
                SectionTitleHeight = ActiveFont.Measure(SectionName).Y;
                Draw.Rect(Position + SectionPosition + new Vector2(SectionTitleLenght / 2 + 10, -4), 275f - (SectionTitleLenght / 2 + 10) + 15, 8f, Color.White);
                Draw.Rect(Position + SectionPosition + new Vector2(-275f - 15, -4), 275f - (SectionTitleLenght / 2 + 10) + 15, 8f, Color.White);
                Draw.Rect(Position + SectionPosition + new Vector2(SectionTitleLenght / 2 + 10 + 275f - (SectionTitleLenght / 2 + 10) + 5, -4), 10f, SectionMaxUpgrades * 50f + SectionTitleHeight / 2 + 4, Color.White);
                Draw.Rect(Position + SectionPosition + new Vector2(-275f - 15, -4), 10f, SectionMaxUpgrades * 50f + SectionTitleHeight / 2 + 4, Color.White);
                Draw.Rect(Position + SectionPosition + new Vector2(-275f - 15, SectionMaxUpgrades * 50f + SectionTitleHeight / 2 - 8), 580f, 8f, Color.White);

                SectionName = Dialog.Clean("Xaphanhelper_UI_Jacket");
                SectionPosition = new Vector2(1490f, 205f);
                SectionMaxUpgrades = 2;
                ActiveFont.DrawOutline(SectionName, Position + SectionPosition, new Vector2(0.5f, 0.5f), Vector2.One * 1f, Color.Yellow, 2f, Color.Black);
                SectionTitleLenght = ActiveFont.Measure(SectionName).X;
                SectionTitleHeight = ActiveFont.Measure(SectionName).Y;
                Draw.Rect(Position + SectionPosition + new Vector2(SectionTitleLenght / 2 + 10, -4), 275f - (SectionTitleLenght / 2 + 10) + 15, 8f, Color.White);
                Draw.Rect(Position + SectionPosition + new Vector2(-275f - 15, -4), 275f - (SectionTitleLenght / 2 + 10) + 15, 8f, Color.White);
                Draw.Rect(Position + SectionPosition + new Vector2(SectionTitleLenght / 2 + 10 + 275f - (SectionTitleLenght / 2 + 10) + 5, -4), 10f, SectionMaxUpgrades * 50f + SectionTitleHeight / 2 + 4, Color.White);
                Draw.Rect(Position + SectionPosition + new Vector2(-275f - 15, -4), 10f, SectionMaxUpgrades * 50f + SectionTitleHeight / 2 + 4, Color.White);
                Draw.Rect(Position + SectionPosition + new Vector2(-275f - 15, SectionMaxUpgrades * 50f + SectionTitleHeight / 2 - 8), 580f, 8f, Color.White);

                Draw.Line(Position + SectionPosition + new Vector2(-275f - 15, (SectionMaxUpgrades * 50f + 26) / 2), Position + SectionPosition + new Vector2(-275f - 15 - 50, (SectionMaxUpgrades * 50f + 26) / 2), Color.Black, 6f);
                Draw.Line(Position + SectionPosition + new Vector2(-275f - 15 - 50, (SectionMaxUpgrades * 50f + 26) / 2 - 2), Position + SectionPosition + new Vector2(-275f - 15 - 50, (SectionMaxUpgrades * 50f + 26) / 2 + 150), Color.Black, 6f);
                Draw.Line(Position + SectionPosition + new Vector2(-275f - 15 - 48, (SectionMaxUpgrades * 50f + 26) / 2 + 150), Position + SectionPosition + new Vector2(-275f - 15 - 196, (SectionMaxUpgrades * 50f + 26) / 2 + 150), Color.Black, 6f);
                Draw.Rect(Position + SectionPosition + new Vector2(-275f - 15 - 196 - 7, (SectionMaxUpgrades * 50f + 26) / 2 + 150 - 7), 14f, 14f, Color.Black);
                Draw.Line(Position + SectionPosition + new Vector2(-275f - 15, (SectionMaxUpgrades * 50f + 26) / 2), Position + SectionPosition + new Vector2(-275f - 15 - 50, (SectionMaxUpgrades * 50f + 26) / 2), Color.White, 4f);
                Draw.Line(Position + SectionPosition + new Vector2(-275f - 15 - 50, (SectionMaxUpgrades * 50f + 26) / 2 - 2), Position + SectionPosition + new Vector2(-275f - 15 - 50, (SectionMaxUpgrades * 50f + 26) / 2 + 150), Color.White, 4f);
                Draw.Line(Position + SectionPosition + new Vector2(-275f - 15 - 48, (SectionMaxUpgrades * 50f + 26) / 2 + 150), Position + SectionPosition + new Vector2(-275f - 15 - 196, (SectionMaxUpgrades * 50f + 26) / 2 + 150), Color.White, 4f);
                Draw.Rect(Position + SectionPosition + new Vector2(-275f - 15 - 196 - 6, (SectionMaxUpgrades * 50f + 26) / 2 + 150 - 6), 12f, 12f, Color.White);

                SectionName = Dialog.Clean("Xaphanhelper_UI_Bag");
                SectionPosition = new Vector2(1490f, 360f);
                SectionMaxUpgrades = 3;
                ActiveFont.DrawOutline(SectionName, Position + SectionPosition, new Vector2(0.5f, 0.5f), Vector2.One * 1f, Color.Yellow, 2f, Color.Black);
                SectionTitleLenght = ActiveFont.Measure(SectionName).X;
                SectionTitleHeight = ActiveFont.Measure(SectionName).Y;
                Draw.Rect(Position + SectionPosition + new Vector2(SectionTitleLenght / 2 + 10, -4), 275f - (SectionTitleLenght / 2 + 10) + 15, 8f, Color.White);
                Draw.Rect(Position + SectionPosition + new Vector2(-275f - 15, -4), 275f - (SectionTitleLenght / 2 + 10) + 15, 8f, Color.White);
                Draw.Rect(Position + SectionPosition + new Vector2(SectionTitleLenght / 2 + 10 + 275f - (SectionTitleLenght / 2 + 10) + 5, -4), 10f, SectionMaxUpgrades * 50f + SectionTitleHeight / 2 + 4, Color.White);
                Draw.Rect(Position + SectionPosition + new Vector2(-275f - 15, -4), 10f, SectionMaxUpgrades * 50f + SectionTitleHeight / 2 + 4, Color.White);
                Draw.Rect(Position + SectionPosition + new Vector2(-275f - 15, SectionMaxUpgrades * 50f + SectionTitleHeight / 2 - 8), 580f, 8f, Color.White);

                Draw.Line(Position + SectionPosition + new Vector2(-275f - 15, (SectionMaxUpgrades * 50f + 26) / 2), Position + SectionPosition + new Vector2(-275f - 15 - 170, (SectionMaxUpgrades * 50f + 26) / 2), Color.Black, 6f);
                Draw.Rect(Position + SectionPosition + new Vector2(-275f - 15 - 170 - 7, (SectionMaxUpgrades * 50f + 26) / 2 - 7), 14f, 14f, Color.Black);
                Draw.Line(Position + SectionPosition + new Vector2(-275f - 15, (SectionMaxUpgrades * 50f + 26) / 2), Position + SectionPosition + new Vector2(-275f - 15 - 170, (SectionMaxUpgrades * 50f + 26) / 2), Color.White, 4f);
                Draw.Rect(Position + SectionPosition + new Vector2(-275f - 15 - 170 - 6, (SectionMaxUpgrades * 50f + 26) / 2 - 6), 12f, 12f, Color.White);

                SectionName = Dialog.Clean("Xaphanhelper_UI_Misc");
                SectionPosition = new Vector2(1490f, 565f);
                SectionMaxUpgrades = 6;
                ActiveFont.DrawOutline(SectionName, Position + SectionPosition, new Vector2(0.5f, 0.5f), Vector2.One * 1f, Color.Yellow, 2f, Color.Black);
                SectionTitleLenght = ActiveFont.Measure(SectionName).X;
                SectionTitleHeight = ActiveFont.Measure(SectionName).Y;
                Draw.Rect(Position + SectionPosition + new Vector2(SectionTitleLenght / 2 + 10, -4), 275f - (SectionTitleLenght / 2 + 10) + 15, 8f, Color.White);
                Draw.Rect(Position + SectionPosition + new Vector2(-275f - 15, -4), 275f - (SectionTitleLenght / 2 + 10) + 15, 8f, Color.White);
                Draw.Rect(Position + SectionPosition + new Vector2(SectionTitleLenght / 2 + 10 + 275f - (SectionTitleLenght / 2 + 10) + 5, -4), 10f, SectionMaxUpgrades * 50f + SectionTitleHeight / 2 + 4, Color.White);
                Draw.Rect(Position + SectionPosition + new Vector2(-275f - 15, -4), 10f, SectionMaxUpgrades * 50f + SectionTitleHeight / 2 + 4, Color.White);
                Draw.Rect(Position + SectionPosition + new Vector2(-275f - 15, SectionMaxUpgrades * 50f + SectionTitleHeight / 2 - 8), 580f, 8f, Color.White);

                Draw.Line(Position + SectionPosition + new Vector2(-275f - 15, (SectionMaxUpgrades * 50f + 26) / 2), Position + SectionPosition + new Vector2(-275f - 15 - 50, (SectionMaxUpgrades * 50f + 26) / 2), Color.Black, 6f);
                Draw.Line(Position + SectionPosition + new Vector2(-275f - 15 - 50, (SectionMaxUpgrades * 50f + 26) / 2 + 2), Position + SectionPosition + new Vector2(-275f - 15 - 50, (SectionMaxUpgrades * 50f + 26) / 2 - 230), Color.Black, 6f);
                Draw.Line(Position + SectionPosition + new Vector2(-275f - 15 - 48, (SectionMaxUpgrades * 50f + 26) / 2 - 230), Position + SectionPosition + new Vector2(-275f - 15 - 233, (SectionMaxUpgrades * 50f + 26) / 2 - 230), Color.Black, 6f);
                Draw.Rect(Position + SectionPosition + new Vector2(-275f - 15 - 233 - 7, (SectionMaxUpgrades * 50f + 26) / 2 - 230 - 7), 14f, 14f, Color.Black);
                Draw.Line(Position + SectionPosition + new Vector2(-275f - 15, (SectionMaxUpgrades * 50f + 26) / 2), Position + SectionPosition + new Vector2(-275f - 15 - 50, (SectionMaxUpgrades * 50f + 26) / 2), Color.White, 4f);
                Draw.Line(Position + SectionPosition + new Vector2(-275f - 15 - 50, (SectionMaxUpgrades * 50f + 26) / 2 + 2), Position + SectionPosition + new Vector2(-275f - 15 - 50, (SectionMaxUpgrades * 50f + 26) / 2 - 230), Color.White, 4f);
                Draw.Line(Position + SectionPosition + new Vector2(-275f - 15 - 48, (SectionMaxUpgrades * 50f + 26) / 2 - 230), Position + SectionPosition + new Vector2(-275f - 15 - 233, (SectionMaxUpgrades * 50f + 26) / 2 - 230), Color.White, 4f);
                Draw.Rect(Position + SectionPosition + new Vector2(-275f - 15 - 233 - 6, (SectionMaxUpgrades * 50f + 26) / 2 - 230 - 6), 12f, 12f, Color.White);

                Draw.Rect(Position + new Vector2(140, 910f), 1640f, 8f, Color.White);
                Draw.Rect(Position + new Vector2(140, 918f), 10f, 86f, Color.White);
                Draw.Rect(Position + new Vector2(1770f, 918f), 10, 86f, Color.White);
                Draw.Rect(Position + new Vector2(140, 1004f), 1640f, 8f, Color.White);
                if (SelectedDisplay != null)
                {
                    string upgrade = SelectedDisplay.internalName;
                    string upgradeKey = $"XaphanHelper_get_{upgrade}";
                    bool droneUpgrade = !XaphanModule.useMetroidGameplay && (upgrade == "LongBeam" || upgrade == "IceBeam" || upgrade == "WaveBeam");
                    string droneString = droneUpgrade ? "_drone" : "";

                    string description = $"{upgradeKey}_Desc{droneString}".DialogCleanOrNull();
                    string controls = $"{upgradeKey}_Controls".DialogCleanOrNull();

                    VirtualButton buttonA = new();
                    VirtualButton buttonB = new();

                    switch (SelectedDisplay.internalName)
                    {
                        case "PowerGrip":
                            buttonA = Input.Grab;
                            break;
                        case "ClimbingKit":
                            buttonA = Input.MenuUp;
                            buttonB = Input.MenuDown;
                            break;
                        case "SpiderMagnet":
                            buttonA = Input.Grab;
                            break;
                        case "DashBoots":
                            buttonA = Input.Dash;
                            break;
                        case "SpaceJump":
                            buttonA = Input.Jump;
                            break;
                        case "HoverBoots":
                            buttonA = Input.MenuUp;
                            break;
                        case "LightningDash":
                            buttonA = Input.Dash;
                            break;
                        case "DroneTeleport":
                            buttonA = Settings.UseBagItemSlot.Button;
                            break;
                        case "Bombs":
                            buttonA = Settings.SelectItem.Button;
                            buttonB = Settings.UseBagItemSlot.Button;
                            break;
                        case "MegaBombs":
                            buttonA = Settings.SelectItem.Button;
                            buttonB = Settings.UseBagItemSlot.Button;
                            break;
                        case "RemoteDrone":
                            buttonA = Settings.SelectItem.Button;
                            buttonB = Settings.UseBagItemSlot.Button;
                            break;
                        case "GoldenFeather":
                            buttonA = Input.Grab;
                            break;
                        case "Binoculars":
                            buttonA = Settings.SelectItem.Button;
                            buttonB = Settings.UseMiscItemSlot.Button;
                            break;
                        case "PortableStation":
                            buttonA = Settings.SelectItem.Button;
                            buttonB = Settings.UseMiscItemSlot.Button;
                            break;
                        case "PulseRadar":
                            buttonA = Settings.SelectItem.Button;
                            buttonB = Settings.UseMiscItemSlot.Button;
                            break;
                        /*case "JumpBoost":
                            controlA = Input.MenuUp;
                            inputActionA = "XaphanHelper_Hold";
                            break;*/
                        default:
                            break;
                    }

                    float maxWidth = 1500f;
                    float textScale = 0.75f;
                    float buttonScale = 0.5f;
                    float upgradeScale = 1f;

                    float descLength = ActiveFont.Measure(description).X * textScale;
                    float descScale = (descLength < maxWidth) ? 1f : maxWidth / descLength;
                    ActiveFont.DrawOutline(description, Position + new Vector2(960f, controls == null ? 960f : 940f), new Vector2(0.5f, 0.5f), Vector2.One * textScale * descScale, Color.White, 2f, Color.Black);

                    if (controls != null) {
                        MTexture buttonATexture = Input.GuiButton(buttonA, "controls/keyboard/oemquestion");
                        MTexture buttonBTexture = Input.GuiButton(buttonB, "controls/keyboard/oemquestion");
                        string upgradeTexturePath = $"{getCustomSpritePath(upgrade)}/{upgrade}";
                        MTexture upgradeTexture = GFX.Gui.GetAtlasSubtexturesAt(upgradeTexturePath, 0);

                        string[] controlsSplit = Regex.Split(controls, @"(\(\w\))");

                        float length = 0;
                        foreach (string sub in controlsSplit) {
                            length += sub switch {
                                "(A)" => buttonATexture.Width * buttonScale,
                                "(B)" => buttonBTexture.Width * buttonScale,
                                "(U)" => upgradeTexture.Width * upgradeScale,
                                _ => ActiveFont.Measure(sub).X * textScale
                            };
                        }

                        Vector2 pos = new(960f, 980f);

                        float controlsScale = (length < maxWidth) ? 1f : maxWidth / length;
                        pos.X -= length / 2 * controlsScale;
                        Vector2 leftCenter = new(0f, 0.5f);
                        foreach (string sub in controlsSplit) {
                            switch (sub) {
                                case "(A)":
                                    buttonATexture.DrawOutlineJustified(pos, leftCenter, Color.White, buttonScale * controlsScale);
                                    pos.X += buttonATexture.Width * buttonScale * controlsScale;
                                    break;
                                case "(B)":
                                    buttonBTexture.DrawOutlineJustified(pos, leftCenter, Color.White, buttonScale);
                                    pos.X += buttonBTexture.Width * buttonScale * controlsScale;
                                    break;
                                case "(U)":
                                    upgradeTexture.DrawOutlineJustified(pos, leftCenter, Color.White);
                                    pos.X += upgradeTexture.Width * upgradeScale * controlsScale;
                                    break;
                                default:
                                    ActiveFont.DrawOutline(sub, pos, leftCenter, Vector2.One * textScale * controlsScale, Color.White, 2f, Color.Black);
                                    pos.X += ActiveFont.Measure(sub).X * textScale * controlsScale;
                                    break;
                            }
                        }
                    }
                }
            }
            else
            {
                SectionName = Dialog.Clean("Xaphanhelper_UI_Metroid_Beams");
                SectionPosition = new Vector2(430f, 590f);
                SectionMaxUpgrades = 5;
                ActiveFont.DrawOutline(SectionName, Position + SectionPosition, new Vector2(0.5f, 0.5f), Vector2.One * 1f, Color.Yellow, 2f, Color.Black);
                SectionTitleLenght = ActiveFont.Measure(SectionName).X;
                SectionTitleHeight = ActiveFont.Measure(SectionName).Y;
                Draw.Rect(Position + SectionPosition + new Vector2(SectionTitleLenght / 2 + 10, -4), 275f - (SectionTitleLenght / 2 + 10) + 15, 8f, Color.White);
                Draw.Rect(Position + SectionPosition + new Vector2(-275f - 15, -4), 275f - (SectionTitleLenght / 2 + 10) + 15, 8f, Color.White);
                Draw.Rect(Position + SectionPosition + new Vector2(SectionTitleLenght / 2 + 10 + 275f - (SectionTitleLenght / 2 + 10) + 5, -4), 10f, SectionMaxUpgrades * 50f + SectionTitleHeight / 2 + 4, Color.White);
                Draw.Rect(Position + SectionPosition + new Vector2(-275f - 15, -4), 10f, SectionMaxUpgrades * 50f + SectionTitleHeight / 2 + 4, Color.White);
                Draw.Rect(Position + SectionPosition + new Vector2(-275f - 15, SectionMaxUpgrades * 50f + SectionTitleHeight / 2 - 8), 580f, 8f, Color.White);

                Draw.Line(Position + SectionPosition + new Vector2(SectionTitleLenght / 2 + 10 + 275f - (SectionTitleLenght / 2 + 10) + 15, (SectionMaxUpgrades * 50f + 26) / 2), Position + SectionPosition + new Vector2(SectionTitleLenght / 2 + 10 + 275f - (SectionTitleLenght / 2 + 10) + 15 + 50, (SectionMaxUpgrades * 50f + 26) / 2), Color.Black, 6f);
                Draw.Line(Position + SectionPosition + new Vector2(SectionTitleLenght / 2 + 10 + 275f - (SectionTitleLenght / 2 + 10) + 15 + 50, (SectionMaxUpgrades * 50f + 26) / 2 + 2), Position + SectionPosition + new Vector2(SectionTitleLenght / 2 + 10 + 275f - (SectionTitleLenght / 2 + 10) + 15 + 50, (SectionMaxUpgrades * 50f + 26) / 2 - 135), Color.Black, 6f);
                Draw.Line(Position + SectionPosition + new Vector2(SectionTitleLenght / 2 + 10 + 275f - (SectionTitleLenght / 2 + 10) + 15 + 48, (SectionMaxUpgrades * 50f + 26) / 2 - 135), Position + SectionPosition + new Vector2(SectionTitleLenght / 2 + 10 + 275f - (SectionTitleLenght / 2 + 10) + 15 + 127, (SectionMaxUpgrades * 50f + 26) / 2 - 135), Color.Black, 6f);
                Draw.Rect(Position + SectionPosition + new Vector2(SectionTitleLenght / 2 + 10 + 275f - (SectionTitleLenght / 2 + 10) + 15 + 127 - 7, (SectionMaxUpgrades * 50f + 26) / 2 - 135 - 7), 14f, 14f, Color.Black);
                Draw.Line(Position + SectionPosition + new Vector2(SectionTitleLenght / 2 + 10 + 275f - (SectionTitleLenght / 2 + 10) + 15, (SectionMaxUpgrades * 50f + 26) / 2), Position + SectionPosition + new Vector2(SectionTitleLenght / 2 + 10 + 275f - (SectionTitleLenght / 2 + 10) + 15 + 50, (SectionMaxUpgrades * 50f + 26) / 2), Color.White, 4f);
                Draw.Line(Position + SectionPosition + new Vector2(SectionTitleLenght / 2 + 10 + 275f - (SectionTitleLenght / 2 + 10) + 15 + 50, (SectionMaxUpgrades * 50f + 26) / 2 + 2), Position + SectionPosition + new Vector2(SectionTitleLenght / 2 + 10 + 275f - (SectionTitleLenght / 2 + 10) + 15 + 50, (SectionMaxUpgrades * 50f + 26) / 2 - 135), Color.White, 4f);
                Draw.Line(Position + SectionPosition + new Vector2(SectionTitleLenght / 2 + 10 + 275f - (SectionTitleLenght / 2 + 10) + 15 + 48, (SectionMaxUpgrades * 50f + 26) / 2 - 135), Position + SectionPosition + new Vector2(SectionTitleLenght / 2 + 10 + 275f - (SectionTitleLenght / 2 + 10) + 15 + 127, (SectionMaxUpgrades * 50f + 26) / 2 - 135), Color.White, 4f);
                Draw.Rect(Position + SectionPosition + new Vector2(SectionTitleLenght / 2 + 10 + 275f - (SectionTitleLenght / 2 + 10) + 15 + 127 - 6, (SectionMaxUpgrades * 50f + 26) / 2 - 135 - 6), 12f, 12f, Color.White);

                SectionName = Dialog.Clean("Xaphanhelper_UI_Metroid_Suits");
                SectionPosition = new Vector2(1490f, 230f);
                SectionMaxUpgrades = 2;
                ActiveFont.DrawOutline(SectionName, Position + SectionPosition, new Vector2(0.5f, 0.5f), Vector2.One * 1f, Color.Yellow, 2f, Color.Black);
                SectionTitleLenght = ActiveFont.Measure(SectionName).X;
                SectionTitleHeight = ActiveFont.Measure(SectionName).Y;
                Draw.Rect(Position + SectionPosition + new Vector2(SectionTitleLenght / 2 + 10, -4), 275f - (SectionTitleLenght / 2 + 10) + 15, 8f, Color.White);
                Draw.Rect(Position + SectionPosition + new Vector2(-275f - 15, -4), 275f - (SectionTitleLenght / 2 + 10) + 15, 8f, Color.White);
                Draw.Rect(Position + SectionPosition + new Vector2(SectionTitleLenght / 2 + 10 + 275f - (SectionTitleLenght / 2 + 10) + 5, -4), 10f, SectionMaxUpgrades * 50f + SectionTitleHeight / 2 + 4, Color.White);
                Draw.Rect(Position + SectionPosition + new Vector2(-275f - 15, -4), 10f, SectionMaxUpgrades * 50f + SectionTitleHeight / 2 + 4, Color.White);
                Draw.Rect(Position + SectionPosition + new Vector2(-275f - 15, SectionMaxUpgrades * 50f + SectionTitleHeight / 2 - 8), 580f, 8f, Color.White);

                Draw.Line(Position + SectionPosition + new Vector2(-275f - 15, (SectionMaxUpgrades * 50f + 26) / 2), Position + SectionPosition + new Vector2(-275f - 15 - 50, (SectionMaxUpgrades * 50f + 26) / 2), Color.Black, 6f);
                Draw.Line(Position + SectionPosition + new Vector2(-275f - 15 - 50, (SectionMaxUpgrades * 50f + 26) / 2 - 2), Position + SectionPosition + new Vector2(-275f - 15 - 50, (SectionMaxUpgrades * 50f + 26) / 2 + 108), Color.Black, 6f);
                Draw.Line(Position + SectionPosition + new Vector2(-275f - 15 - 48, (SectionMaxUpgrades * 50f + 26) / 2 + 108), Position + SectionPosition + new Vector2(-275f - 15 - 240, (SectionMaxUpgrades * 50f + 26) / 2 + 108), Color.Black, 6f);
                Draw.Rect(Position + SectionPosition + new Vector2(-275f - 15 - 240 - 7, (SectionMaxUpgrades * 50f + 26) / 2 + 108 - 7), 14f, 14f, Color.Black);
                Draw.Line(Position + SectionPosition + new Vector2(-275f - 15, (SectionMaxUpgrades * 50f + 26) / 2), Position + SectionPosition + new Vector2(-275f - 15 - 50, (SectionMaxUpgrades * 50f + 26) / 2), Color.White, 4f);
                Draw.Line(Position + SectionPosition + new Vector2(-275f - 15 - 50, (SectionMaxUpgrades * 50f + 26) / 2 - 2), Position + SectionPosition + new Vector2(-275f - 15 - 50, (SectionMaxUpgrades * 50f + 26) / 2 + 108), Color.White, 4f);
                Draw.Line(Position + SectionPosition + new Vector2(-275f - 15 - 48, (SectionMaxUpgrades * 50f + 26) / 2 + 108), Position + SectionPosition + new Vector2(-275f - 15 - 240, (SectionMaxUpgrades * 50f + 26) / 2 + 108), Color.White, 4f);
                Draw.Rect(Position + SectionPosition + new Vector2(-275f - 15 - 240 - 6, (SectionMaxUpgrades * 50f + 26) / 2 + 108 - 6), 12f, 12f, Color.White);

                SectionName = Dialog.Clean("Xaphanhelper_UI_Metroid_Misc");
                SectionPosition = new Vector2(1490f, 440f);
                SectionMaxUpgrades = 4;
                ActiveFont.DrawOutline(SectionName, Position + SectionPosition, new Vector2(0.5f, 0.5f), Vector2.One * 1f, Color.Yellow, 2f, Color.Black);
                SectionTitleLenght = ActiveFont.Measure(SectionName).X;
                SectionTitleHeight = ActiveFont.Measure(SectionName).Y;
                Draw.Rect(Position + SectionPosition + new Vector2(SectionTitleLenght / 2 + 10, -4), 275f - (SectionTitleLenght / 2 + 10) + 15, 8f, Color.White);
                Draw.Rect(Position + SectionPosition + new Vector2(-275f - 15, -4), 275f - (SectionTitleLenght / 2 + 10) + 15, 8f, Color.White);
                Draw.Rect(Position + SectionPosition + new Vector2(SectionTitleLenght / 2 + 10 + 275f - (SectionTitleLenght / 2 + 10) + 5, -4), 10f, SectionMaxUpgrades * 50f + SectionTitleHeight / 2 + 4, Color.White);
                Draw.Rect(Position + SectionPosition + new Vector2(-275f - 15, -4), 10f, SectionMaxUpgrades * 50f + SectionTitleHeight / 2 + 4, Color.White);
                Draw.Rect(Position + SectionPosition + new Vector2(-275f - 15, SectionMaxUpgrades * 50f + SectionTitleHeight / 2 - 8), 580f, 8f, Color.White);

                Draw.Line(Position + SectionPosition + new Vector2(-275f - 15, (SectionMaxUpgrades * 50f + 26) / 2), Position + SectionPosition + new Vector2(-275f - 15 - 50, (SectionMaxUpgrades * 50f + 26) / 2), Color.Black, 6f);
                Draw.Line(Position + SectionPosition + new Vector2(-275f - 15 - 50, (SectionMaxUpgrades * 50f + 26) / 2 + 2), Position + SectionPosition + new Vector2(-275f - 15 - 50, (SectionMaxUpgrades * 50f + 26) / 2 - 101), Color.Black, 6f);
                Draw.Line(Position + SectionPosition + new Vector2(-275f - 15 - 48, (SectionMaxUpgrades * 50f + 26) / 2 - 101), Position + SectionPosition + new Vector2(-275f - 15 - 240, (SectionMaxUpgrades * 50f + 26) / 2 - 101), Color.Black, 6f);
                Draw.Rect(Position + SectionPosition + new Vector2(-275f - 15 - 240 - 7, (SectionMaxUpgrades * 50f + 26) / 2 - 101 - 7), 14f, 14f, Color.Black);
                Draw.Line(Position + SectionPosition + new Vector2(-275f - 15, (SectionMaxUpgrades * 50f + 26) / 2), Position + SectionPosition + new Vector2(-275f - 15 - 50, (SectionMaxUpgrades * 50f + 26) / 2), Color.White, 4f);
                Draw.Line(Position + SectionPosition + new Vector2(-275f - 15 - 50, (SectionMaxUpgrades * 50f + 26) / 2 + 2), Position + SectionPosition + new Vector2(-275f - 15 - 50, (SectionMaxUpgrades * 50f + 26) / 2 - 101), Color.White, 4f);
                Draw.Line(Position + SectionPosition + new Vector2(-275f - 15 - 48, (SectionMaxUpgrades * 50f + 26) / 2 - 101), Position + SectionPosition + new Vector2(-275f - 15 - 240, (SectionMaxUpgrades * 50f + 26) / 2 - 101), Color.White, 4f);
                Draw.Rect(Position + SectionPosition + new Vector2(-275f - 15 - 240 - 6, (SectionMaxUpgrades * 50f + 26) / 2 - 101 - 6), 12f, 12f, Color.White);

                SectionName = Dialog.Clean("Xaphanhelper_UI_Metroid_Boots");
                SectionPosition = new Vector2(1490f, 770f);
                SectionMaxUpgrades = 3;
                ActiveFont.DrawOutline(SectionName, Position + SectionPosition, new Vector2(0.5f, 0.5f), Vector2.One * 1f, Color.Yellow, 2f, Color.Black);
                SectionTitleLenght = ActiveFont.Measure(SectionName).X;
                SectionTitleHeight = ActiveFont.Measure(SectionName).Y;
                Draw.Rect(Position + SectionPosition + new Vector2(SectionTitleLenght / 2 + 10, -4), 275f - (SectionTitleLenght / 2 + 10) + 15, 8f, Color.White);
                Draw.Rect(Position + SectionPosition + new Vector2(-275f - 15, -4), 275f - (SectionTitleLenght / 2 + 10) + 15, 8f, Color.White);
                Draw.Rect(Position + SectionPosition + new Vector2(SectionTitleLenght / 2 + 10 + 275f - (SectionTitleLenght / 2 + 10) + 5, -4), 10f, SectionMaxUpgrades * 50f + SectionTitleHeight / 2 + 4, Color.White);
                Draw.Rect(Position + SectionPosition + new Vector2(-275f - 15, -4), 10f, SectionMaxUpgrades * 50f + SectionTitleHeight / 2 + 4, Color.White);
                Draw.Rect(Position + SectionPosition + new Vector2(-275f - 15, SectionMaxUpgrades * 50f + SectionTitleHeight / 2 - 8), 580f, 8f, Color.White);

                Draw.Line(Position + SectionPosition + new Vector2(-275f - 15, (SectionMaxUpgrades * 50f + 26) / 2), Position + SectionPosition + new Vector2(-275f - 15 - 50, (SectionMaxUpgrades * 50f + 26) / 2), Color.Black, 6f);
                Draw.Line(Position + SectionPosition + new Vector2(-275f - 15 - 50, (SectionMaxUpgrades * 50f + 26) / 2 + 2), Position + SectionPosition + new Vector2(-275f - 15 - 50, (SectionMaxUpgrades * 50f + 26) / 2 - 50), Color.Black, 6f);
                Draw.Line(Position + SectionPosition + new Vector2(-275f - 15 - 48, (SectionMaxUpgrades * 50f + 26) / 2 - 50), Position + SectionPosition + new Vector2(-275f - 15 - 159, (SectionMaxUpgrades * 50f + 26) / 2 - 50), Color.Black, 6f);
                Draw.Rect(Position + SectionPosition + new Vector2(-275f - 15 - 159 - 7, (SectionMaxUpgrades * 50f + 26) / 2 - 50 - 7), 14f, 14f, Color.Black);
                Draw.Line(Position + SectionPosition + new Vector2(-275f - 15, (SectionMaxUpgrades * 50f + 26) / 2), Position + SectionPosition + new Vector2(-275f - 15 - 50, (SectionMaxUpgrades * 50f + 26) / 2), Color.White, 4f);
                Draw.Line(Position + SectionPosition + new Vector2(-275f - 15 - 50, (SectionMaxUpgrades * 50f + 26) / 2 + 2), Position + SectionPosition + new Vector2(-275f - 15 - 50, (SectionMaxUpgrades * 50f + 26) / 2 - 50), Color.White, 4f);
                Draw.Line(Position + SectionPosition + new Vector2(-275f - 15 - 48, (SectionMaxUpgrades * 50f + 26) / 2 - 50), Position + SectionPosition + new Vector2(-275f - 15 - 159, (SectionMaxUpgrades * 50f + 26) / 2 - 50), Color.White, 4f);
                Draw.Rect(Position + SectionPosition + new Vector2(-275f - 15 - 159 - 6, (SectionMaxUpgrades * 50f + 26) / 2 - 50 - 6), 12f, 12f, Color.White);
            }
        }
    }
}
