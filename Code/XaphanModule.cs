using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Xml;
using Celeste.Mod.Meta;
using Celeste.Mod.XaphanHelper.Components;
using Celeste.Mod.XaphanHelper.Controllers;
using Celeste.Mod.XaphanHelper.Cutscenes;
using Celeste.Mod.XaphanHelper.Data;
using Celeste.Mod.XaphanHelper.Effects;
using Celeste.Mod.XaphanHelper.Entities;
using Celeste.Mod.XaphanHelper.Hooks;
using Celeste.Mod.XaphanHelper.Managers;
using Celeste.Mod.XaphanHelper.Triggers;
using Celeste.Mod.XaphanHelper.UI_Elements;
using Celeste.Mod.XaphanHelper.Upgrades;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Monocle;
using MonoMod.Utils;

namespace Celeste.Mod.XaphanHelper
{
    public class XaphanModule : EverestModule
    {
        public static SpriteBank SpriteBank;

        public static XaphanModule Instance;

        // If you need to store ModSettings:
        public override Type SettingsType => typeof(XaphanModuleSettings);
        public static XaphanModuleSettings ModSettings => (XaphanModuleSettings)Instance._Settings;

        // If you need to store save data:
        public override Type SaveDataType => typeof(XaphanModuleSaveData);
        public static XaphanModuleSaveData ModSaveData => (XaphanModuleSaveData)Instance._SaveData;

        // If you need to store session data:
        public override Type SessionType => typeof(XaphanModuleSession);
        public static XaphanModuleSession ModSession => (XaphanModuleSession)Instance._Session;

        public static List<TeleportToOtherSideData> TeleportToOtherSideData = new();

        public static List<RoomMusicControllerData> RoomMusicControllerData = new();

        private FieldInfo OuiChapterSelect_icons = typeof(OuiChapterSelect).GetField("icons", BindingFlags.Instance | BindingFlags.NonPublic);

        private FieldInfo OuiChapterPanel_modes = typeof(OuiChapterPanel).GetField("modes", BindingFlags.Instance | BindingFlags.NonPublic);

        private FieldInfo OuiChapterPanel_instantClose = typeof(OuiChapterPanel).GetField("instantClose", BindingFlags.Instance | BindingFlags.NonPublic);

        private FieldInfo OuiChapterSelectIcon_tween = typeof(OuiChapterSelectIcon).GetField("tween", BindingFlags.Instance | BindingFlags.NonPublic);

        private FieldInfo OuiChapterSelectIcon_front = typeof(OuiChapterSelectIcon).GetField("front", BindingFlags.Instance | BindingFlags.NonPublic);

        private FieldInfo OuiChapterSelectIcon_back = typeof(OuiChapterSelectIcon).GetField("back", BindingFlags.Instance | BindingFlags.NonPublic);

        private FieldInfo Overworld_transitioning = typeof(Overworld).GetField("transitioning", BindingFlags.Instance | BindingFlags.NonPublic);

        private Type OuiChapterPanel_T_Option = typeof(OuiChapterPanel).GetNestedType("Option", BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static);

        private bool hasOldExtendedVariants = false;

        private bool displayedOldExtVariantsPostcard = false;

        public static Version SoCMVersion;

        public static bool startedAnyChapter = false;

        public static bool startedAnySoCMChapter = false;

        public static bool onSlope;

        public static int onSlopeDir;

        public static bool onSlopeGentle;

        public static float onSlopeTop;

        public static bool onSlopeAffectPlayerSpeed;

        public static float MaxRunSpeed;

        public static bool ChangingSide;

        private Postcard oldExtVariantsPostcard;

        public static bool useMergeChaptersControllerCheck;

        public static bool useMergeChaptersController;

        public static string MergeChaptersControllerMode;

        public static bool MergeChaptersControllerKeepPrologue;

        private string MergeChaptersControllerLevelSet;

        private string lastLevelSet;

        public static bool isInLevel = false;

        public static bool CanOpenMap(Level level)
        {
            return ModSaveData.SavedFlags.Contains(level.Session.Area.GetLevelSet() + "_Can_Open_Map") || ModSettings.SpeedrunMode ? true : false;
        }

        public static bool UIOpened;

        public static bool ShowUI;

        public static bool TeleportBackFromDrone;

        public static bool NoDroneSpawnSound;

        public enum Upgrades
        {
            // Celeste Upgrades

            PowerGrip,
            ClimbingKit,
            SpiderMagnet,
            DroneTeleport,
            JumpBoost,
            Bombs,
            MegaBombs,
            RemoteDrone,
            GoldenFeather,
            Binoculars,
            EtherealDash,
            PortableStation,
            PulseRadar,
            DashBoots,
            HoverJet,
            LightningDash,
            MissilesModule,
            SuperMissilesModule,

            // Metroid Upgrades

            Spazer,
            PlasmaBeam,
            MorphingBall,
            MorphBombs,
            SpringBall,
            HighJumpBoots,
            SpeedBooster,

            //Common Upgrades

            LongBeam,
            IceBeam,
            WaveBeam,
            VariaJacket,
            GravityJacket,
            ScrewAttack,
            SpaceJump
        }

        // Celeste Upgrades

        public static bool PowerGripCollected(Level level)
        {
            return ModSaveData.SavedFlags.Contains(level.Session.Area.GetLevelSet() + "_Upgrade_PowerGrip");
        }

        public static bool ClimbingKitCollected(Level level)
        {
            return ModSaveData.SavedFlags.Contains(level.Session.Area.GetLevelSet() + "_Upgrade_ClimbingKit");
        }

        public static bool SpiderMagnetCollected(Level level)
        {
            return ModSaveData.SavedFlags.Contains(level.Session.Area.GetLevelSet() + "_Upgrade_SpiderMagnet");
        }

        public static bool DroneTeleportCollected(Level level)
        {
            return ModSaveData.SavedFlags.Contains(level.Session.Area.GetLevelSet() + "_Upgrade_DroneTeleport");
        }

        public static bool JumpBoostCollected(Level level)
        {
            return ModSaveData.SavedFlags.Contains(level.Session.Area.GetLevelSet() + "_Upgrade_JumpBoost");
        }

        public static bool BombsCollected(Level level)
        {
            return ModSaveData.SavedFlags.Contains(level.Session.Area.GetLevelSet() + "_Upgrade_Bombs");
        }

        public static bool MegaBombsCollected(Level level)
        {
            return ModSaveData.SavedFlags.Contains(level.Session.Area.GetLevelSet() + "_Upgrade_MegaBombs");
        }

        public static bool RemoteDroneCollected(Level level)
        {
            return ModSaveData.SavedFlags.Contains(level.Session.Area.GetLevelSet() + "_Upgrade_RemoteDrone");
        }

        public static bool GoldenFeatherCollected(Level level)
        {
            return ModSaveData.SavedFlags.Contains(level.Session.Area.GetLevelSet() + "_Upgrade_GoldenFeather");
        }

        public static bool BinocularsCollected(Level level)
        {
            return ModSaveData.SavedFlags.Contains(level.Session.Area.GetLevelSet() + "_Upgrade_Binoculars");
        }

        public static bool EtherealDashCollected(Level level)
        {
            return ModSaveData.SavedFlags.Contains(level.Session.Area.GetLevelSet() + "_Upgrade_EtherealDash");
        }

        public static bool PortableStationCollected(Level level)
        {
            return ModSaveData.SavedFlags.Contains(level.Session.Area.GetLevelSet() + "_Upgrade_PortableStation");
        }

        public static bool PulseRadarCollected(Level level)
        {
            return ModSaveData.SavedFlags.Contains(level.Session.Area.GetLevelSet() + "_Upgrade_PulseRadar");
        }

        public static bool DashBootsCollected(Level level)
        {
            return ModSaveData.SavedFlags.Contains(level.Session.Area.GetLevelSet() + "_Upgrade_DashBoots");
        }

        public static bool HoverJetCollected(Level level)
        {
            return ModSaveData.SavedFlags.Contains(level.Session.Area.GetLevelSet() + "_Upgrade_HoverJet");
        }

        public static bool LightningDashCollected(Level level)
        {
            return ModSaveData.SavedFlags.Contains(level.Session.Area.GetLevelSet() + "_Upgrade_LightningDash");
        }

        public static bool MissilesModuleCollected(Level level)
        {
            return ModSaveData.SavedFlags.Contains(level.Session.Area.GetLevelSet() + "_Upgrade_MissilesModule");
        }

        public static bool SuperMissilesModuleCollected(Level level)
        {
            return ModSaveData.SavedFlags.Contains(level.Session.Area.GetLevelSet() + "_Upgrade_SuperMissilesModule");
        }

        // Metroid Upgrades

        public static bool SpazerCollected(Level level)
        {
            return ModSaveData.SavedFlags.Contains(level.Session.Area.GetLevelSet() + "_Upgrade_Spazer");
        }

        public static bool PlasmaBeamCollected(Level level)
        {
            return ModSaveData.SavedFlags.Contains(level.Session.Area.GetLevelSet() + "_Upgrade_PlasmaBeam");
        }

        public static bool MorphingBallCollected(Level level)
        {
            return ModSaveData.SavedFlags.Contains(level.Session.Area.GetLevelSet() + "_Upgrade_MorphingBall");
        }

        public static bool MorphBombsCollected(Level level)
        {
            return ModSaveData.SavedFlags.Contains(level.Session.Area.GetLevelSet() + "_Upgrade_MorphBombs");
        }

        public static bool SpringBallCollected(Level level)
        {
            return ModSaveData.SavedFlags.Contains(level.Session.Area.GetLevelSet() + "_Upgrade_SpringBall");
        }

        public static bool HighJumpBootsCollected(Level level)
        {
            return ModSaveData.SavedFlags.Contains(level.Session.Area.GetLevelSet() + "_Upgrade_HighJumpBoots");
        }

        public static bool SpeedBoosterCollected(Level level)
        {
            return ModSaveData.SavedFlags.Contains(level.Session.Area.GetLevelSet() + "_Upgrade_SpeedBooster");
        }

        // Common Upgrades

        public static bool LongBeamCollected(Level level)
        {
            return ModSaveData.SavedFlags.Contains(level.Session.Area.GetLevelSet() + "_Upgrade_LongBeam");
        }

        public static bool IceBeamCollected(Level level)
        {
            return ModSaveData.SavedFlags.Contains(level.Session.Area.GetLevelSet() + "_Upgrade_IceBeam");
        }

        public static bool WaveBeamCollected(Level level)
        {
            return ModSaveData.SavedFlags.Contains(level.Session.Area.GetLevelSet() + "_Upgrade_WaveBeam");
        }

        public static bool VariaJacketCollected(Level level)
        {
            return ModSaveData.SavedFlags.Contains(level.Session.Area.GetLevelSet() + "_Upgrade_VariaJacket");
        }

        public static bool GravityJacketCollected(Level level)
        {
            return ModSaveData.SavedFlags.Contains(level.Session.Area.GetLevelSet() + "_Upgrade_GravityJacket");
        }

        public static bool ScrewAttackCollected(Level level)
        {
            return ModSaveData.SavedFlags.Contains(level.Session.Area.GetLevelSet() + "_Upgrade_ScrewAttack");
        }

        public static bool SpaceJumpCollected(Level level)
        {
            return ModSaveData.SavedFlags.Contains(level.Session.Area.GetLevelSet() + "_Upgrade_SpaceJump");
        }

        public static void useIngameMapCheck(Level level)
        {
            AreaKey area = level.Session.Area;
            MapData MapData = AreaData.Areas[area.ID].Mode[(int)area.Mode].MapData;
            foreach (LevelData levelData in MapData.Levels)
            {
                foreach (EntityData entity in levelData.Entities)
                {
                    if (entity.Name == "XaphanHelper/InGameMapController")
                    {
                        useIngameMap = true;
                        inGameMapProgressDisplayMode = entity.Attr("showProgress");
                        break;
                    }
                }
                if (useIngameMap)
                {
                    break;
                }
            }
        }

        public static void allRoomsUseTileControllerCheck(Level level)
        {
            AreaKey area = level.Session.Area;
            MapData MapData = AreaData.Areas[area.ID].Mode[(int)area.Mode].MapData;
            foreach (LevelData levelData in MapData.Levels)
            {
                if (levelData.Spawns.Count != 0)
                {
                    allRoomsUseTileController = false;
                    foreach (EntityData entity in levelData.Entities)
                    {
                        if (entity.Name == "XaphanHelper/InGameMapTilesController")
                        {
                            allRoomsUseTileController = true;
                            break;
                        }
                    }
                    if (!allRoomsUseTileController)
                    {
                        break;
                    }
                }
            }
        }

        public static void useUpgradesCheck(Level level)
        {
            if (useMetroidGameplay)
            {
                useUpgrades = true;
            }
            else
            {
                AreaKey area = level.Session.Area;
                MapData MapData = AreaData.Areas[area.ID].Mode[(int)area.Mode].MapData;
                foreach (LevelData levelData in MapData.Levels)
                {
                    foreach (EntityData entity in levelData.Entities)
                    {
                        if (entity.Name == "XaphanHelper/UpgradeController")
                        {
                            useUpgrades = true;
                            DisableStatusScreen = entity.Bool("disableStatusScreen", false);
                            break;
                        }
                    }
                    if (useUpgrades)
                    {
                        break;
                    }
                }
            }
        }

        public static void useMetroidGameplayCheck(Level level)
        {
            AreaKey area = level.Session.Area;
            MapData MapData = AreaData.Areas[area.ID].Mode[(int)area.Mode].MapData;
            foreach (LevelData levelData in MapData.Levels)
            {
                foreach (EntityData entity in levelData.Entities)
                {
                    if (entity.Name == "XaphanHelper/MetroidGameplayController")
                    {
                        useMetroidGameplay = true;
                        level.Tracker.GetEntity<Player>().ResetSprite(PlayerSpriteMode.Madeline);
                        break;
                    }
                }
                if (useMetroidGameplay)
                {
                    break;
                }
            }
        }

        public static bool useMetroidGameplaySessionCheck(Session session)
        {
            AreaKey area = session.Area;
            MapData MapData = AreaData.Areas[area.ID].Mode[(int)area.Mode].MapData;
            foreach (LevelData levelData in MapData.Levels)
            {
                foreach (EntityData entity in levelData.Entities)
                {
                    if (entity.Name == "XaphanHelper/MetroidGameplayController")
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        public static bool PlayerIsControllingRemoteDrone()
        {
            if (Engine.Scene is Level)
            {
                Level level = (Level)Engine.Scene;
                Drone drone = level.Tracker.GetEntity<Drone>();
                if (drone != null && !Drone.Hold.IsHeld)
                {
                    return true;
                }
            }
            return false;
        }

        public static bool useIngameMap;

        public static string inGameMapProgressDisplayMode;

        public static bool allRoomsUseTileController;

        public static bool useUpgrades;

        public static bool DisableStatusScreen;

        public static bool forceStartingUpgrades;

        public static bool useMetroidGameplay;

        public Dictionary<Upgrades, Upgrade> UpgradeHandlers = new();

        public static bool PlayerHasGolden;

        public bool PlayerLostGolden;

        private bool CanLoadPlayer;

        public float cassetteAlpha;

        public bool cassetteWaitForKeyPress;

        public float cassetteTimer;

        public static bool TriggeredCountDown;

        public XaphanModule()
        {
            Instance = this;

            // Celeste Upgrades

            UpgradeHandlers[Upgrades.PowerGrip] = new PowerGrip();
            UpgradeHandlers[Upgrades.ClimbingKit] = new ClimbingKit();
            UpgradeHandlers[Upgrades.SpiderMagnet] = new SpiderMagnet();
            UpgradeHandlers[Upgrades.DroneTeleport] = new DroneTeleport();
            UpgradeHandlers[Upgrades.JumpBoost] = new JumpBoost();
            UpgradeHandlers[Upgrades.Bombs] = new Bombs();
            UpgradeHandlers[Upgrades.MegaBombs] = new MegaBombs();
            UpgradeHandlers[Upgrades.RemoteDrone] = new RemoteDrone();
            UpgradeHandlers[Upgrades.GoldenFeather] = new GoldenFeather();
            UpgradeHandlers[Upgrades.Binoculars] = new Binoculars();
            UpgradeHandlers[Upgrades.EtherealDash] = new EtherealDash();
            UpgradeHandlers[Upgrades.PortableStation] = new PortableStation();
            UpgradeHandlers[Upgrades.PulseRadar] = new PulseRadar();
            UpgradeHandlers[Upgrades.DashBoots] = new DashBoots();
            UpgradeHandlers[Upgrades.HoverJet] = new HoverJet();
            UpgradeHandlers[Upgrades.LightningDash] = new LightningDash();
            UpgradeHandlers[Upgrades.MissilesModule] = new MissilesModule();
            UpgradeHandlers[Upgrades.SuperMissilesModule] = new SuperMissilesModule();

            //Metroid Upgrades

            UpgradeHandlers[Upgrades.Spazer] = new Spazer();
            UpgradeHandlers[Upgrades.PlasmaBeam] = new PlasmaBeam();
            UpgradeHandlers[Upgrades.MorphingBall] = new MorphingBall();
            UpgradeHandlers[Upgrades.MorphBombs] = new MorphBombs();
            UpgradeHandlers[Upgrades.SpringBall] = new SpringBall();
            UpgradeHandlers[Upgrades.HighJumpBoots] = new HighJumpBoots();
            UpgradeHandlers[Upgrades.SpeedBooster] = new SpeedBooster();

            // Common Upgrades

            UpgradeHandlers[Upgrades.LongBeam] = new LongBeam();
            UpgradeHandlers[Upgrades.IceBeam] = new IceBeam();
            UpgradeHandlers[Upgrades.WaveBeam] = new WaveBeam();
            UpgradeHandlers[Upgrades.VariaJacket] = new VariaJacket();
            UpgradeHandlers[Upgrades.GravityJacket] = new GravityJacket();
            UpgradeHandlers[Upgrades.ScrewAttack] = new ScrewAttack();
            UpgradeHandlers[Upgrades.SpaceJump] = new SpaceJump();
        }

        // Set up any hooks, event handlers and your mod in general here.
        // Load runs before Celeste itself has initialized properly.
        public override void Load()
        {
            DecalRegistry.AddPropertyHandler("XaphanHelper_BGdepth", delegate (Decal decal, XmlAttributeCollection attrs)
            {
                if (attrs["value"] != null && decal.Depth == 9000)
                {
                    decal.Depth = int.Parse(attrs["value"].Value);
                }
            });
            DecalRegistry.AddPropertyHandler("XaphanHelper_flagsHide", (Decal decal, XmlAttributeCollection attrs) => {
                if (attrs["flags"] != null && (attrs["room"] == null || decal.SceneAs<Level>().Session.Level == attrs["room"].Value))
                {
                    decal.Add(new FlagDecalVisibilityToggle(attrs["flags"].Value.Split(','), attrs["inverted"] != null && bool.Parse(attrs["inverted"].Value)));
                }
            });
            DecalRegistry.AddPropertyHandler("XaphanHelper_flagSwap", delegate (Decal decal, XmlAttributeCollection attrs)
            {
                if (attrs["flag"] != null && attrs["offPath"] != null && attrs["onPath"] != null)
                {
                    if (attrs["room"] == null || decal.SceneAs<Level>().Session.Level == attrs["room"].Value)
                    {
                        decal.MakeFlagSwap(attrs["flag"].Value, attrs["offPath"].Value, attrs["onPath"].Value);
                        if (decal.SceneAs<Level>().Session.GetFlag(attrs["flag"].Value))
                        {
                            float X = attrs["offsetX"] != null ? float.Parse(attrs["offsetX"].Value) : 0f;
                            float Y = attrs["offsetY"] != null ? float.Parse(attrs["offsetY"].Value) : 0f;
                            decal.Position += new Vector2(decal.Scale.X == 1 ? X : -X, decal.Scale.Y == 1 ? Y : -Y);
                        }
                    }
                }
            });
            DecalRegistry.AddPropertyHandler("XaphanHelper_randomFlagSwap", (Decal decal, XmlAttributeCollection attrs) => {
                if (attrs["flag"] != null && attrs["offPath"] != null && attrs["onPath"] != null)
                {
                    List<MTexture> offTextures = attrs["offPath"].Value.Split(',').Select(path => GFX.Game[path]).ToList();
                    List<MTexture> onTextures = attrs["onPath"].Value.Split(',').Select(path => GFX.Game[path]).ToList();
                    decal.Add(decal.Image = new RandomFlagSwapImage(attrs["flag"].Value, offTextures, onTextures));
                }
            });
            DecalRegistry.AddPropertyHandler("XaphanHelper_flagLight", delegate (Decal decal, XmlAttributeCollection attrs)
            {
                if (attrs["flag"] != null)
                {
                    bool inverted = attrs["inverted"] != null ? bool.Parse(attrs["inverted"].Value) : false;
                    if (inverted ? !decal.SceneAs<Level>().Session.GetFlag(attrs["flag"].Value) : decal.SceneAs<Level>().Session.GetFlag(attrs["flag"].Value))
                    {
                        float X = attrs["offsetX"] != null ? float.Parse(attrs["offsetX"].Value) : 0f;
                        float Y = attrs["offsetY"] != null ? float.Parse(attrs["offsetY"].Value) : 0f;
                        Color color = attrs["color"] != null ? Calc.HexToColor(attrs["color"].Value) : Color.White;
                        float alpha = attrs["alpha"] != null ? float.Parse(attrs["alpha"].Value) : 1f;
                        int startFade = attrs["startFade"] != null ? int.Parse(attrs["startFade"].Value) : 16;
                        int endFade = attrs["endFade"] != null ? int.Parse(attrs["endFade"].Value) : 24;
                        decal.Add(new VertexLight(new Vector2(X, Y), color, alpha, startFade, endFade));
                    }
                }
            });
            DecalRegistry.AddPropertyHandler("XaphanHelper_flagLightOcclude", delegate (Decal decal, XmlAttributeCollection attrs)
            {
                if (attrs["flag"] != null)
                {
                    bool inverted = attrs["inverted"] != null ? bool.Parse(attrs["inverted"].Value) : false;
                    if (inverted ? !decal.SceneAs<Level>().Session.GetFlag(attrs["flag"].Value) : decal.SceneAs<Level>().Session.GetFlag(attrs["flag"].Value))
                    {
                        int X = attrs["x"] != null ? int.Parse(attrs["x"].Value) : 0;
                        int Y = attrs["y"] != null ? int.Parse(attrs["y"].Value) : 0;
                        int width = attrs["width"] != null ? int.Parse(attrs["width"].Value) : 16;
                        int height = attrs["height"] != null ? int.Parse(attrs["height"].Value) : 16;
                        float alpha = attrs["alpha"] != null ? float.Parse(attrs["alpha"].Value) : 1f;
                        decal.Add(new LightOcclude(new Rectangle(X, Y, width, height), alpha));
                    }
                }
            });
            DecalRegistry.AddPropertyHandler("XaphanHelper_flagRandomFlip", delegate (Decal decal, XmlAttributeCollection attrs)
            {
                if (attrs["flag"] != null)
                {
                    bool inverted = attrs["inverted"] != null ? bool.Parse(attrs["inverted"].Value) : false;
                    bool flipX = attrs["flipX"] != null ? bool.Parse(attrs["flipX"].Value) : false;
                    bool flipY = attrs["flipY"] != null ? bool.Parse(attrs["flipY"].Value) : false;
                    if (inverted ? !decal.SceneAs<Level>().Session.GetFlag(attrs["flag"].Value) : decal.SceneAs<Level>().Session.GetFlag(attrs["flag"].Value))
                    {
                        decal.Scale *= new Vector2(flipX ? (Calc.Random.Next(1, 3) == 2 ? -1 : 1) : 1, flipY ? (Calc.Random.Next(1, 3) == 2 ? -1 : 1) : 1);
                    }
                }
            });
            DecalRegistry.AddPropertyHandler("XaphanHelper_flagSparks", delegate (Decal decal, XmlAttributeCollection attrs)
            {
                if (attrs["flag"] != null)
                {
                    bool inverted = attrs["inverted"] != null ? bool.Parse(attrs["inverted"].Value) : false;
                    float X = attrs["offsetX"] != null ? float.Parse(attrs["offsetX"].Value) : 0f;
                    float Y = attrs["offsetY"] != null ? float.Parse(attrs["offsetY"].Value) : 0f;
                    if (inverted ? !decal.SceneAs<Level>().Session.GetFlag(attrs["flag"].Value) : decal.SceneAs<Level>().Session.GetFlag(attrs["flag"].Value))
                    {
                        decal.SceneAs<Level>().Add(new SparkGenerator(decal.Position + new Vector2(decal.Scale.X < 0 ? X : -X, Y)));
                    }
                }
            });
            DecalRegistry.AddPropertyHandler("XaphanHelper_flame", delegate (Decal decal, XmlAttributeCollection attrs)
            {
                float X = attrs["offsetX"] != null ? float.Parse(attrs["offsetX"].Value) : 0f;
                float Y = attrs["offsetY"] != null ? float.Parse(attrs["offsetY"].Value) : 0f;
                decal.SceneAs<Level>().Add(new CustomTorch(decal.Position - new Vector2(8f), new Vector2(decal.Scale.X < 0 ? X : -X, Y), "FFA500", false, true, "objects/Xaphan/CustomTorch/flame", "", 1, 48, 64, "event:/game/05_mirror_temple/torch_activate", false));
            });
            foreach (Upgrades upgrade in UpgradeHandlers.Keys)
            {
                UpgradeHandlers[upgrade].Load();
            }
            Everest.Events.Level.OnLoadBackdrop += OnLoadBackdrop;
            Everest.Events.Level.OnLoadLevel += onLevelLoad;
            Everest.Events.Level.OnExit += onLevelExit;
            Everest.Events.Level.OnCreatePauseMenuButtons += onCreatePauseMenuButtons;
            On.Celeste.Cassette.UnlockedBSide.EaseIn += modCassetteUnlockedBSideEaseIn;
            On.Celeste.Cassette.UnlockedBSide.EaseOut += modCassetteUnlockedBSideEaseOut;
            On.Celeste.Cassette.UnlockedBSide.Render += modCassetteUnlockedBSideRender;
            On.Celeste.GameplayStats.Render += onGameplayStatsRender;
            On.Celeste.HeartGem.IsCompleteArea += modHeartGemIsCompleteArea;
            On.Celeste.LevelEnter.Routine += modLevelEnterRoutine;
            On.Celeste.LevelEnter.BeforeRender += modLevelEnterBeforeRender;
            On.Celeste.LevelEnter.Go += onLevelEnterGo;
            On.Celeste.Level.Pause += onLevelPause;
            On.Celeste.Level.Update += onLevelUpdate;
            On.Celeste.OuiChapterSelect.GetMinMaxArea += modOuiChapterSelectGetMinMaxArea;
            On.Celeste.OuiChapterSelect.Update += modOuiChapterSelectUpdate;
            On.Celeste.OuiChapterSelectIcon.Update += modOuiChapterSelectIconUpdate;
            On.Celeste.OuiChapterPanel.Enter += modOuiChapterPanelEnter;
            On.Celeste.OuiChapterPanel.Leave += modOuiChapterPanelLeave;
            On.Celeste.OuiChapterPanel.GetModeHeight += modOuiChapterPanelGetModeHeight;
            On.Celeste.OuiChapterPanel.IncrementStats += modOuiChapterPanelIncrementStats;
            On.Celeste.OuiChapterPanel.IncrementStatsDisplay += modOuiChapterPanelIncrementStatsDisplay;
            On.Celeste.OuiChapterPanel.IsStart += modOuiChapterPanelIsStart;
            On.Celeste.OuiChapterPanel.UpdateStats += modOuiChapterPanelUpdateStats;
            On.Celeste.OuiChapterPanel.Reset += modOuiChapterPanelReset;
            On.Celeste.OuiChapterPanel.Start += modOuiChapterPanelStart;
            On.Celeste.OuiChapterPanel.StartRoutine += modOuiChapterPanelStartRoutine;
            On.Celeste.Overworld.SetNormalMusic += modOverworldSetNormalMusic;
            On.Celeste.Player.ctor += modPlayerCtor;
            On.Celeste.Player.CallDashEvents += modPlayerCallDashEvents;
            On.Celeste.Player.Render += onPlayerRender;
            On.Celeste.PlayerDeadBody.Render += onPlayerDeadBodyRender;
            On.Celeste.ReturnMapHint.Render += onReturnMapHintRender;
            On.Celeste.SaveData.FoundAnyCheckpoints += modSaveDataFoundAnyCheckpoints;
            On.Celeste.Session.Restart += onSessionRestart;
            On.Celeste.SpeedrunTimerDisplay.DrawTime += onSpeedrunTimerDisplayDrawTime;
            On.Celeste.Strawberry.OnPlayer += modStrawberryOnPlayer;
            On.Celeste.Strawberry.Update += modStrawberryUpdate;
            On.Celeste.Strawberry.OnLoseLeader += modStrawberryOnLoseLeader;
            On.Celeste.Strawberry.CollectRoutine += onStrawberryCollectRoutine;
            On.Celeste.Mod.UI.OuiMapSearch.Inspect += modOuiMapSearchInspect;
            On.Celeste.Mod.UI.OuiMapList.Inspect += modOuiMapListInspect;
            MetroidGameplayController.Load();
            ScrewAttackManager.Load();
            MapDisplay.Load();
            PlayerPlatform.Load();
            Slope.Load();
            CameraBlocker.Load();
            HeatController.Load();
            JumpBlock.Load();
            TimeManager.Load();
            TimedStrawberry.Load();
            CountdownDisplay.Load();
            Liquid.Load();
            MagneticCeiling.Load();
            Drone.Load();
            FlagDashSwitch.Load();
            TimedDashSwitch.Load();
            BagDisplay.Load();
            StatsFlags.Load();
            CustomEndScreenController.Load();
            Binocular.Load();
            EtherealBlock.Load();
            TilesetsSwap.Load();
            SpikesTextureSwap.Load();
            UpgradesDisplay.Load();
            LaserDetectorManager.Load();
            PushBlock.Load();
            FakePlayer.Load();
            PlayerDeadAction.Load();
            DroneSwitch.Load();
            TransitionBlackEffect.Load();
            WorkRobot.Load();
            BreakBlock.Load();
            CustomRefill.Load();
        }

        // Optional, do anything requiring either the Celeste or mod content here.
        public override void LoadContent(bool firstLoad)
        {
            SpriteBank = new SpriteBank(GFX.Game, "Graphics/Xaphan/CustomSprites.xml");
            On.Celeste.PlayerSprite.CreateFramesMetadata += PlayerSpriteMetadataHook;
        }

        // Unload the entirety of your mod's content. Free up any native resources.
        public override void Unload()
        {
            foreach (Upgrades upgrade in UpgradeHandlers.Keys)
            {
                UpgradeHandlers[upgrade].Unload();
            }
            Everest.Events.Level.OnLoadBackdrop -= OnLoadBackdrop;
            Everest.Events.Level.OnLoadLevel -= onLevelLoad;
            Everest.Events.Level.OnExit -= onLevelExit;
            Everest.Events.Level.OnCreatePauseMenuButtons -= onCreatePauseMenuButtons;
            On.Celeste.Cassette.UnlockedBSide.EaseIn -= modCassetteUnlockedBSideEaseIn;
            On.Celeste.Cassette.UnlockedBSide.EaseOut -= modCassetteUnlockedBSideEaseOut;
            On.Celeste.Cassette.UnlockedBSide.Render -= modCassetteUnlockedBSideRender;
            On.Celeste.GameplayStats.Render -= onGameplayStatsRender;
            On.Celeste.HeartGem.IsCompleteArea -= modHeartGemIsCompleteArea;
            On.Celeste.LevelEnter.Routine -= modLevelEnterRoutine;
            On.Celeste.LevelEnter.BeforeRender -= modLevelEnterBeforeRender;
            On.Celeste.LevelEnter.Go -= onLevelEnterGo;
            On.Celeste.Level.Pause -= onLevelPause;
            On.Celeste.Level.Update -= onLevelUpdate;
            On.Celeste.OuiChapterSelect.GetMinMaxArea -= modOuiChapterSelectGetMinMaxArea;
            On.Celeste.OuiChapterSelect.Update -= modOuiChapterSelectUpdate;
            On.Celeste.OuiChapterSelectIcon.Update -= modOuiChapterSelectIconUpdate;
            On.Celeste.OuiChapterPanel.GetModeHeight -= modOuiChapterPanelGetModeHeight;
            On.Celeste.OuiChapterPanel.IncrementStats -= modOuiChapterPanelIncrementStats;
            On.Celeste.OuiChapterPanel.IncrementStatsDisplay -= modOuiChapterPanelIncrementStatsDisplay;
            On.Celeste.OuiChapterPanel.IsStart -= modOuiChapterPanelIsStart;
            On.Celeste.OuiChapterPanel.UpdateStats -= modOuiChapterPanelUpdateStats;
            On.Celeste.OuiChapterPanel.Reset -= modOuiChapterPanelReset;
            On.Celeste.OuiChapterPanel.Start -= modOuiChapterPanelStart;
            On.Celeste.OuiChapterPanel.StartRoutine -= modOuiChapterPanelStartRoutine;
            On.Celeste.Overworld.SetNormalMusic -= modOverworldSetNormalMusic;
            On.Celeste.Player.ctor -= modPlayerCtor;
            On.Celeste.Player.CallDashEvents -= modPlayerCallDashEvents;
            On.Celeste.Player.Render -= onPlayerRender;
            On.Celeste.PlayerDeadBody.Render -= onPlayerDeadBodyRender;
            On.Celeste.ReturnMapHint.Render -= onReturnMapHintRender;
            On.Celeste.SaveData.FoundAnyCheckpoints -= modSaveDataFoundAnyCheckpoints;
            On.Celeste.Session.Restart -= onSessionRestart;
            On.Celeste.SpeedrunTimerDisplay.DrawTime -= onSpeedrunTimerDisplayDrawTime;
            On.Celeste.Strawberry.OnPlayer -= modStrawberryOnPlayer;
            On.Celeste.Strawberry.Update -= modStrawberryUpdate;
            On.Celeste.Strawberry.OnLoseLeader -= modStrawberryOnLoseLeader;
            On.Celeste.Strawberry.CollectRoutine -= onStrawberryCollectRoutine;
            On.Celeste.Mod.UI.OuiMapSearch.Inspect -= modOuiMapSearchInspect;
            On.Celeste.Mod.UI.OuiMapList.Inspect -= modOuiMapListInspect;
            MetroidGameplayController.Unload();
            ScrewAttackManager.Unload();
            MapDisplay.Unload();
            PlayerPlatform.Unload();
            Slope.Unload();
            CameraBlocker.Unload();
            HeatController.Unload();
            JumpBlock.Unload();
            TimeManager.Unload();
            TimedStrawberry.Unload();
            CountdownDisplay.Unload();
            Liquid.Unload();
            MagneticCeiling.Unload();
            Drone.Unload();
            FlagDashSwitch.Unload();
            TimedDashSwitch.Unload();
            BagDisplay.Unload();
            StatsFlags.Unload();
            CustomEndScreenController.Unload();
            Binocular.Unload();
            EtherealBlock.Unload();
            TilesetsSwap.Unload();
            SpikesTextureSwap.Unload();
            UpgradesDisplay.Unload();
            LaserDetectorManager.Unload();
            PushBlock.Unload();
            FakePlayer.Unload();
            PlayerDeadAction.Unload();
            DroneSwitch.Unload();
            TransitionBlackEffect.Unload();
            WorkRobot.Unload();
            BreakBlock.Unload();
            CustomRefill.Unload();
        }

        // Custom States

        public static int StFastFall;

        private void modPlayerCtor(On.Celeste.Player.orig_ctor orig, Player self, Vector2 position, PlayerSpriteMode spriteMode)
        {
            orig.Invoke(self, position, spriteMode);
            StFastFall = StateMachineExt.AddState(self.StateMachine, FastFallUpdate, FastLabFallCoroutine);
        }

        private int FastFallUpdate()
        {
            if (Engine.Scene is Level)
            {
                Player player = ((Level)Engine.Scene).Tracker.GetEntity<Player>();
                player.Facing = Facings.Right;
                if (!player.OnGround() && player.DummyGravity)
                {
                    player.Speed.Y = 320f;
                }
            }
            return StFastFall;
        }

        private IEnumerator FastLabFallCoroutine()
        {
            if (Engine.Scene is Level)
            {
                Level level = (Level)Engine.Scene;
                Player player = level.Tracker.GetEntity<Player>();
                player.Sprite.Play("fallFast");
                while (!player.OnGround())
                {
                    yield return null;
                }
                player.Play("event:/char/madeline/mirrortemple_big_landing");
                if (player.Dashes <= 1)
                {
                    player.Sprite.Play("fallPose");
                }
                else
                {
                    player.Sprite.Play("idle");
                }
                player.Sprite.Scale.Y = 0.7f;
                Input.Rumble(RumbleStrength.Strong, RumbleLength.Medium);
                level.DirectionalShake(new Vector2(0f, 1f), 0.5f);
                player.Speed.X = 0f;
                level.Particles.Emit(Player.P_SummitLandA, 12, player.BottomCenter, Vector2.UnitX * 3f, -(float)Math.PI / 2f);
                level.Particles.Emit(Player.P_SummitLandB, 8, player.BottomCenter - Vector2.UnitX * 2f, Vector2.UnitX * 2f, 3.403392f);
                level.Particles.Emit(Player.P_SummitLandB, 8, player.BottomCenter + Vector2.UnitX * 2f, Vector2.UnitX * 2f, -(float)Math.PI / 12f);
                for (float p = 0f; p < 1f; p += Engine.DeltaTime)
                {
                    yield return null;
                }
                player.StateMachine.State = 0;
            }
        }

        private void onGameplayStatsRender(On.Celeste.GameplayStats.orig_Render orig, GameplayStats self)
        {
            AreaKey area = self.SceneAs<Level>().Session.Area;
            MapData MapData = AreaData.Areas[area.ID].Mode[(int)area.Mode].MapData;
            if (!useIngameMap || (useIngameMap && inGameMapProgressDisplayMode == "Never"))
            {
                orig(self);
            }
        }

        // Custom Chapter Pannel

        private void modOuiChapterSelectIconUpdate(On.Celeste.OuiChapterSelectIcon.orig_Update orig, OuiChapterSelectIcon self)
        {
            if (useMergeChaptersController && SaveData.Instance != null && (SaveData.Instance.GetLevelSetStats().Name == "Xaphan/0" ? !ModSaveData.SpeedrunMode : true))
            {
                orig(self);
                if (!MergeChaptersControllerKeepPrologue || (MergeChaptersControllerKeepPrologue && SaveData.Instance.LastArea_Safe.ID != SaveData.Instance.GetLevelSetStats().AreaOffset))
                {
                    if (SaveData.Instance == null)
                    {
                        return;
                    }
                    self.sizeEase = Calc.Approach(self.sizeEase, 1f, Engine.DeltaTime * 4f);
                    if (SaveData.Instance.LastArea_Safe.ID == self.Area)
                    {
                        self.Depth = -50;
                    }
                    else
                    {
                        self.Depth = -45;
                    }
                    if (OuiChapterSelectIcon_tween.GetValue(self) == null)
                    {
                        if (self.IsSelected)
                        {
                            CustomOuiChapterPanel uI = (self.Scene as Overworld).GetUI<CustomOuiChapterPanel>();
                            if (uI != null)
                            {
                                self.Position = ((!uI.EnteringChapter) ? uI.OpenPosition : uI.Position) + uI.IconOffset;
                            }
                        }
                        else if (!self.IsHidden)
                        {
                            self.Position = Calc.Approach(self.Position, self.IdlePosition, 2400f * Engine.DeltaTime);
                        }
                    }
                    if (self.Area > SaveData.Instance.GetLevelSetStats().AreaOffset + (MergeChaptersControllerKeepPrologue ? 1 : 0) && self.Area <= (SaveData.Instance.GetLevelSetStats().AreaOffset + SaveData.Instance.GetLevelSetStats().MaxArea))
                    {
                        if (MergeChaptersControllerKeepPrologue && self.Area > SaveData.Instance.GetLevelSetStats().AreaOffset)
                        {
                            OuiChapterSelectIcon_front.SetValue(self, GFX.Gui[AreaData.Areas[SaveData.Instance.GetLevelSetStats().AreaOffset + 1].Icon]);
                            OuiChapterSelectIcon_back.SetValue(self, GFX.Gui.Has(AreaData.Areas[SaveData.Instance.GetLevelSetStats().AreaOffset + 1].Icon + "_back") ? GFX.Gui[AreaData.Areas[SaveData.Instance.GetLevelSetStats().AreaOffset + 1].Icon + "_back"] : GFX.Gui[AreaData.Areas[SaveData.Instance.GetLevelSetStats().AreaOffset + 1].Icon]);
                        }
                        else
                        {
                            OuiChapterSelectIcon_front.SetValue(self, GFX.Gui[AreaData.Areas[SaveData.Instance.GetLevelSetStats().AreaOffset].Icon]);
                            OuiChapterSelectIcon_back.SetValue(self, GFX.Gui.Has(AreaData.Areas[SaveData.Instance.GetLevelSetStats().AreaOffset].Icon + "_back") ? GFX.Gui[AreaData.Areas[SaveData.Instance.GetLevelSetStats().AreaOffset].Icon + "_back"] : GFX.Gui[AreaData.Areas[SaveData.Instance.GetLevelSetStats().AreaOffset].Icon]);
                        }
                    }
                }
            }
            else
            {
                orig(self);
            }
        }

        private void modOuiChapterSelectGetMinMaxArea(On.Celeste.OuiChapterSelect.orig_GetMinMaxArea orig, OuiChapterSelect self, out int areaOffs, out int areaMax)
        {
            foreach (OuiChapterSelectIcon icon in (List<OuiChapterSelectIcon>)OuiChapterSelect_icons.GetValue(self))
            {
                icon.Visible = true;
            }
            if (useMergeChaptersController && (SaveData.Instance.GetLevelSetStats().Name == "Xaphan/0" ? !ModSaveData.SpeedrunMode : true))
            {
                if (MergeChaptersControllerKeepPrologue)
                {
                    areaOffs = SaveData.Instance.GetLevelSetStats().AreaOffset;
                    int areaOffsRaw = SaveData.Instance.GetLevelSetStats().AreaOffset;
                    int areaMaxRaw = Math.Max(areaOffsRaw, SaveData.Instance.UnlockedAreas_Safe);
                    do
                    {
                        areaMax = ((List<OuiChapterSelectIcon>)OuiChapterSelect_icons.GetValue(self)).FindLastIndex((OuiChapterSelectIcon i) => (i != null && i.Area == areaMaxRaw) || i.AssistModeUnlockable);
                    }
                    while (areaMax == -1 && --areaMaxRaw < areaOffsRaw);
                    if (areaMax == -1)
                    {
                        areaMax = areaMaxRaw;
                    }
                    if (areaMax > areaOffs + 1)
                    {
                        areaMax = areaOffs + 1;
                    }
                }
                else
                {
                    areaMax = areaOffs = SaveData.Instance.GetLevelSetStats().AreaOffset;
                }
                foreach (OuiChapterSelectIcon icon in (List<OuiChapterSelectIcon>)OuiChapterSelect_icons.GetValue(self))
                {
                    if (icon.Area > areaMax)
                    {
                        icon.Hide();
                        icon.Visible = false;
                    }
                }
            }
            else
            {
                orig(self, out areaOffs, out areaMax);
            }
        }

        private IEnumerator modOuiChapterPanelIncrementStats(On.Celeste.OuiChapterPanel.orig_IncrementStats orig, OuiChapterPanel self, bool shouldAdvance)
        {
            if (useMergeChaptersController && (MergeChaptersControllerKeepPrologue ? SaveData.Instance.LastArea_Safe.ID != SaveData.Instance.GetLevelSetStats().AreaOffset : true) && (SaveData.Instance.GetLevelSetStats().Name == "Xaphan/0" ? !ModSaveData.SpeedrunMode : true))
            {
                shouldAdvance = false;
            }
            yield return new SwapImmediately(orig(self, shouldAdvance));
        }

        private void modOuiMapSearchInspect(On.Celeste.Mod.UI.OuiMapSearch.orig_Inspect orig, UI.OuiMapSearch self, AreaData area, AreaMode mode)
        {
            MergeChaptersControllerCheck(area);
            if (useMergeChaptersController && (SaveData.Instance.GetLevelSetStats().Name == "Xaphan/0" ? !ModSaveData.SpeedrunMode : true))
            {
                self.Focused = false;
                Audio.Play("event:/ui/world_map/icon/select");
                SaveData.Instance.LastArea_Safe = area.ToKey(mode);
                if (self.OuiIcons != null && area.ID < self.OuiIcons.Count)
                {
                    self.OuiIcons[area.ID > SaveData.Instance.GetLevelSetStats().AreaOffset + 1 ? SaveData.Instance.GetLevelSetStats().AreaOffset + 1 : area.ID].Select();
                }
                self.Overworld.Mountain.Model.EaseState(area.MountainState);
                MergeChaptersControllerCheck(area);
                if (MergeChaptersControllerKeepPrologue && area.ID == SaveData.Instance.GetLevelSetStats().AreaOffset)
                {
                    SaveData.Instance.LastArea_Safe.ID = SaveData.Instance.GetLevelSetStats().AreaOffset;
                    self.Overworld.Goto<OuiChapterPanel>();
                }
                else
                {
                    self.Overworld.Goto<CustomOuiChapterPanel>();
                }
            }
            else
            {
                orig(self, area, mode);
            }
        }

        private void modOuiMapListInspect(On.Celeste.Mod.UI.OuiMapList.orig_Inspect orig, UI.OuiMapList self, AreaData area, AreaMode mode)
        {
            MergeChaptersControllerCheck(area);
            if (useMergeChaptersController && (SaveData.Instance.GetLevelSetStats().Name == "Xaphan/0" ? !ModSaveData.SpeedrunMode : true))
            {
                self.Focused = false;
                Audio.Play("event:/ui/world_map/icon/select");
                SaveData.Instance.LastArea_Safe = area.ToKey(mode);
                if (self.OuiIcons != null && area.ID < self.OuiIcons.Count)
                {
                    self.OuiIcons[area.ID > SaveData.Instance.GetLevelSetStats().AreaOffset + 1 ? SaveData.Instance.GetLevelSetStats().AreaOffset + 1 : area.ID].Select();
                }
                self.Overworld.Mountain.Model.EaseState(area.MountainState);
                MergeChaptersControllerCheck(area);
                if (MergeChaptersControllerKeepPrologue && area.ID == SaveData.Instance.GetLevelSetStats().AreaOffset)
                {
                    SaveData.Instance.LastArea_Safe.ID = SaveData.Instance.GetLevelSetStats().AreaOffset;
                    self.Overworld.Goto<OuiChapterPanel>();
                }
                else
                {
                    self.Overworld.Goto<CustomOuiChapterPanel>();
                }
            }
            else
            {
                orig(self, area, mode);
            }
        }

        private IEnumerator modOuiChapterPanelLeave(On.Celeste.OuiChapterPanel.orig_Leave orig, OuiChapterPanel self, Oui next)
        {
            if (!useMergeChaptersController || (useMergeChaptersController && MergeChaptersControllerKeepPrologue && SaveData.Instance.LastArea_Safe.ID == SaveData.Instance.GetLevelSetStats().AreaOffset) || (SaveData.Instance.GetLevelSetStats().Name == "Xaphan/0" ? ModSaveData.SpeedrunMode : false))
            {
                yield return new SwapImmediately(orig(self, next));
            }
        }

        private IEnumerator modOuiChapterPanelEnter(On.Celeste.OuiChapterPanel.orig_Enter orig, OuiChapterPanel self, Oui from)
        {
            if (useMergeChaptersController && (SaveData.Instance.GetLevelSetStats().Name == "Xaphan/0" ? !ModSaveData.SpeedrunMode : true))
            {
                if (MergeChaptersControllerKeepPrologue && SaveData.Instance.LastArea_Safe.ID == SaveData.Instance.GetLevelSetStats().AreaOffset)
                {
                    yield return new SwapImmediately(orig(self, from));
                }
                else
                {
                    self.Overworld.Goto<CustomOuiChapterPanel>();
                    self.Visible = false;
                    yield break;
                }
            }
            else
            {
                yield return new SwapImmediately(orig(self, from));
            }
        }

        private void modOverworldSetNormalMusic(On.Celeste.Overworld.orig_SetNormalMusic orig, Overworld self)
        {
            if (!useMergeChaptersController || SaveData.Instance == null || self.IsCurrent<OuiMainMenu>())
            {
                orig(self);
            }
            else
            {
                AreaData areaData = AreaData.Get(SaveData.Instance.LastArea_Safe);
                MapMetaMountain mapMetaMountain = areaData?.GetMeta()?.Mountain;
                Audio.SetMusic(mapMetaMountain?.BackgroundMusic ?? "event:/music/menu/level_select");
                Audio.SetAmbience(mapMetaMountain?.BackgroundAmbience ?? "event:/env/amb/worldmap");
                foreach (KeyValuePair<string, float> item in mapMetaMountain?.BackgroundMusicParams ?? new Dictionary<string, float>())
                {
                    Audio.SetMusicParam(item.Key, item.Value);
                }
            }
        }

        private IEnumerator modCassetteUnlockedBSideEaseIn(On.Celeste.Cassette.UnlockedBSide.orig_EaseIn orig, Entity self)
        {
            if (startedAnySoCMChapter)
            {
                while ((cassetteAlpha += Engine.DeltaTime / 0.5f) < 1f)
                {
                    yield return null;
                }
                cassetteAlpha = 1f;
                yield return 1.5f;
                cassetteWaitForKeyPress = true;
            }
            else
            {
                yield return new SwapImmediately(orig(self));
            }
        }

        private IEnumerator modCassetteUnlockedBSideEaseOut(On.Celeste.Cassette.UnlockedBSide.orig_EaseOut orig, Entity self)
        {
            if (startedAnySoCMChapter)
            {
                cassetteWaitForKeyPress = false;
                while ((cassetteAlpha -= Engine.DeltaTime / 0.5f) > 0f)
                {
                    yield return null;
                }
                cassetteAlpha = 0f;
                cassetteTimer = 0f;
                self.RemoveSelf();
            }
            else
            {
                yield return new SwapImmediately(orig(self));
            }
        }

        private void modCassetteUnlockedBSideRender(On.Celeste.Cassette.UnlockedBSide.orig_Render orig, Entity self)
        {
            if (startedAnySoCMChapter)
            {
                cassetteTimer += Engine.DeltaTime;
                float num = Ease.CubeOut(cassetteAlpha);
                string text = Dialog.Clean("Xaphan_0_PortalAppeared");
                Vector2 position = Celeste.TargetCenter + new Vector2(0f, 64f);
                Vector2 adjust = Vector2.UnitY * 64f * (1f - num);
                Draw.Rect(-10f, -10f, 1940f, 1100f, Color.Black * num * 0.8f);
                GFX.Gui["collectables/cassette"].DrawJustified(position - adjust + new Vector2(0f, 32f), new Vector2(0.5f, 1f), Color.White * num);
                ActiveFont.Draw(text, position + adjust, new Vector2(0.5f, 0f), Vector2.One, Color.White * num);
                if (cassetteWaitForKeyPress)
                {
                    GFX.Gui["textboxbutton"].DrawCentered(new Vector2(1824f, 984 + ((cassetteTimer % 1f < 0.25f) ? 6 : 0)));
                }
            }
            else
            {
                orig(self);
            }
        }

        private void onSpeedrunTimerDisplayDrawTime(On.Celeste.SpeedrunTimerDisplay.orig_DrawTime orig, Vector2 position, string timeString, float scale, bool valid, bool finished, bool bestTime, float alpha)
        {
            if (useMergeChaptersController && MergeChaptersControllerMode != "Classic" && SaveData.Instance.CurrentSession.Area.Mode == AreaMode.Normal && (SaveData.Instance.CurrentSession.Area.LevelSet == "Xaphan/0" ? !ModSaveData.SpeedrunMode : true))
            {
                valid = true;
            }
            orig(position, timeString, scale, valid, finished, bestTime, alpha);
        }

        private Session onSessionRestart(On.Celeste.Session.orig_Restart orig, Session self, string intoLevel)
        {
            if (useMergeChaptersController && SaveData.Instance.CurrentSession.Area.Mode == AreaMode.Normal && (SaveData.Instance.CurrentSession.Area.LevelSet == "Xaphan/0" ? !ModSaveData.SpeedrunMode : true))
            {
                Session session = new(self.Area, self.StartCheckpoint, self.OldStats)
                {
                    Time = ModSaveData.SavedTime.ContainsKey(SaveData.Instance.CurrentSession.Area.LevelSet) ? ModSaveData.SavedTime[SaveData.Instance.CurrentSession.Area.LevelSet] : 0L,
                    UnlockedCSide = self.UnlockedCSide
                };
                if (intoLevel != null)
                {
                    session.Level = intoLevel;
                    if (intoLevel != self.MapData.StartLevel().Name)
                    {
                        session.StartedFromBeginning = false;
                    }
                }
                return session;
            }
            else
            {
                return orig(self, intoLevel);
            }
        }

        private bool modHeartGemIsCompleteArea(On.Celeste.HeartGem.orig_IsCompleteArea orig, HeartGem self, bool value)
        {
            if ((SaveData.Instance.CurrentSession.Area.Mode == AreaMode.BSide || SaveData.Instance.CurrentSession.Area.Mode == AreaMode.CSide) && (ModSaveData.SpeedrunMode && SaveData.Instance.CurrentSession.Area.LevelSet == "Xaphan/0"))
            {
                return true;
            }
            return orig(self, value);
        }

        public override void Initialize()
        {
            base.Initialize();
            hasOldExtendedVariants = Everest.Modules.Any(module => module.Metadata.Name == "ExtendedVariantMode" && module.Metadata.Version < new Version(0, 15, 9));
            foreach (EverestModule module in Everest.Modules)
            {
                if (module.Metadata.Name == "TheSecretOfCelesteMountain")
                {
                    SoCMVersion = module.Metadata.Version;
                }
            }
        }

        private void onLevelPause(On.Celeste.Level.orig_Pause orig, Level self, int startIndex, bool minimal, bool quickReset)
        {
            if (useMergeChaptersController && (MergeChaptersControllerKeepPrologue ? SaveData.Instance.LastArea_Safe.ID != SaveData.Instance.GetLevelSetStats().AreaOffset : true) && (self.Session.Area.LevelSet == "Xaphan/0" ? !ModSaveData.SpeedrunMode : true))
            {
                if (quickReset)
                {
                    return;
                }
            }
            orig(self, startIndex, minimal, quickReset);
        }

        private void onReturnMapHintRender(On.Celeste.ReturnMapHint.orig_Render orig, ReturnMapHint self)
        {
            if (useMergeChaptersController && MergeChaptersControllerMode != "Classic" && (SaveData.Instance.CurrentSession.Area.LevelSet == "Xaphan/0" ? !ModSaveData.SpeedrunMode : true) && !((MergeChaptersControllerKeepPrologue && SaveData.Instance.CurrentSession.Area.ID == SaveData.Instance.GetLevelSetStats().AreaOffset)))
            {
                MTexture mTexture = GFX.Gui["checkpoint"];
                string text = "";
                if (MergeChaptersControllerMode == "Rooms")
                {
                    text = Dialog.Clean("XaphanHelper_UI_ReturnToMap_Rooms");
                }
                else if (MergeChaptersControllerMode == "Warps")
                {
                    text = Dialog.Clean("XaphanHelper_UI_ReturnToMap_Warps");
                }
                float width = ActiveFont.Measure(text).X * 0.75f;
                float textureWidth = mTexture.Width * 0.75f;
                Vector2 value2 = new((1920f - width - textureWidth - 64f) / 2f, 730f);
                ActiveFont.DrawOutline(text, value2 + new Vector2(width / 2f, 0f), new Vector2(0.5f, 0.5f), Vector2.One * 0.75f, Color.LightGray, 2f, Color.Black);
                value2.X += width + 64f;
                mTexture.DrawCentered(value2 + new Vector2(textureWidth * 0.5f, 0f), Color.White, 0.75f);
            }
            else
            {
                orig(self);
            }
        }

        private bool modSaveDataFoundAnyCheckpoints(On.Celeste.SaveData.orig_FoundAnyCheckpoints orig, SaveData self, AreaKey area)
        {
            if (area.LevelSet == MergeChaptersControllerLevelSet && MergeChaptersControllerMode != "Classic" && (self.GetLevelSetStats().Name == "Xaphan/0" ? !ModSaveData.SpeedrunMode : true))
            {
                return false;
            }
            return orig(self, area);
        }

        private int modOuiChapterPanelGetModeHeight(On.Celeste.OuiChapterPanel.orig_GetModeHeight orig, OuiChapterPanel self)
        {
            if (SaveData.Instance != null)
            {
                AreaStats areaStats = SaveData.Instance.Areas_Safe[self.Area.ID];
                if (self.Area.Mode == AreaMode.BSide && !areaStats.Cassette && ModSaveData.CSideUnlocked.Contains(SaveData.Instance.GetLevelSetStats().Name + ":" + self.Area.ChapterIndex) && SaveData.Instance.UnlockedModes < 3 && !SaveData.Instance.DebugMode)
                {
                    AreaModeStats areaModeStats = self.RealStats.Modes[2];
                    bool flag = areaModeStats.Strawberries.Count <= 0;
                    if (!self.Data.Interlude_Safe && ((areaModeStats.Deaths > 0 && self.Area.Mode != 0) || areaModeStats.Completed || areaModeStats.HeartGem))
                    {
                        flag = false;
                    }
                    if (!flag)
                    {
                        return 540;
                    }
                    return 300;
                }
            }
            return orig(self);
        }

        private bool modOuiChapterPanelIsStart(On.Celeste.OuiChapterPanel.orig_IsStart orig, OuiChapterPanel self, Overworld overworld, Overworld.StartMode start)
        {
            if ((useMergeChaptersController && !MergeChaptersControllerKeepPrologue) || (useMergeChaptersController && MergeChaptersControllerKeepPrologue && SaveData.Instance.CurrentSession.Area.ChapterIndex > 0))
            {
                self.Position += new Vector2(10000, 0);
                return false;
            }
            if (SaveData.Instance != null && !SaveData.Instance.DebugMode)
            {
                AreaStats areaStats = SaveData.Instance.Areas_Safe[SaveData.Instance.LastArea_Safe.ID];
                bool unlockedBSide = false;
                bool unlockedCSide = false;
                if (areaStats.Cassette)
                {
                    unlockedBSide = true;
                }
                if (ModSaveData.CSideUnlocked.Contains(SaveData.Instance.GetLevelSetStats().Name + ":" + SaveData.Instance.LastArea_Safe.ChapterIndex) && SaveData.Instance.UnlockedModes < 3)
                {
                    unlockedCSide = true;
                }
                if (!unlockedBSide && unlockedCSide)
                {
                    if (SaveData.Instance != null && SaveData.Instance.LastArea_Safe.ID == AreaKey.None.ID)
                    {
                        SaveData.Instance.LastArea_Safe = AreaKey.Default;
                        OuiChapterPanel_instantClose.SetValue(self, true);
                    }
                    if (start == Overworld.StartMode.AreaComplete || start == Overworld.StartMode.AreaQuit)
                    {
                        AreaData areaData = AreaData.Get(SaveData.Instance.LastArea_Safe.ID);
                        areaData = (AreaDataExt.Get(areaData?.GetMeta()?.Parent) ?? areaData);
                        if (areaData != null)
                        {
                            SaveData.Instance.LastArea_Safe.ID = areaData.ID;
                        }
                    }
                    bool num = self.orig_IsStart(overworld, start);
                    if (ModSaveData.LastPlayedSide == 0)
                    {
                        self.Area.Mode = AreaMode.Normal;
                    }
                    else
                    {
                        self.Area.Mode = AreaMode.BSide;
                    }
                    ModSaveData.LastPlayedSide = 0;
                    return num;
                }
                else
                {
                    return orig(self, overworld, start);
                }
            }
            return orig(self, overworld, start);
        }

        private void modOuiChapterPanelUpdateStats(On.Celeste.OuiChapterPanel.orig_UpdateStats orig, OuiChapterPanel self, bool wiggle, bool? overrideStrawberryWiggle, bool? overrideDeathWiggle, bool? overrideHeartWiggle)
        {
            DynData<OuiChapterPanel> OuiChapterPanelData = new(self);
            DeathsCounter deaths = OuiChapterPanelData.Get<DeathsCounter>("deaths");
            HeartGemDisplay heart = OuiChapterPanelData.Get<HeartGemDisplay>("heart");
            StrawberriesCounter strawberries = OuiChapterPanelData.Get<StrawberriesCounter>("strawberries");
            AreaStats areaStats = SaveData.Instance.Areas_Safe[self.Area.ID];
            if (self.Area.Mode == AreaMode.BSide && !areaStats.Cassette && ModSaveData.CSideUnlocked.Contains(SaveData.Instance.GetLevelSetStats().Name + ":" + self.Area.ChapterIndex) && SaveData.Instance.UnlockedModes < 3 && !SaveData.Instance.DebugMode)
            {
                AreaModeStats areaModeStats = self.DisplayedStats.Modes[2];
                AreaData areaData = AreaData.Get(self.Area);
                deaths.Visible = (areaModeStats.Deaths > 0 && (self.Area.Mode != 0 || self.RealStats.Modes[2].Completed) && !AreaData.Get(self.Area).Interlude_Safe);
                deaths.Amount = areaModeStats.Deaths;
                deaths.SetMode(AreaMode.CSide);
                heart.Visible = (areaModeStats.HeartGem && !areaData.Interlude_Safe && areaData.CanFullClear);
                heart.SetCurrentMode(self.Area.Mode, areaModeStats.HeartGem);
                strawberries.Visible = false;
                strawberries.Golden = true;
                if (wiggle)
                {
                    if (strawberries.Visible && (!overrideStrawberryWiggle.HasValue || overrideStrawberryWiggle.Value))
                    {
                        strawberries.Wiggle();
                    }
                    if (heart.Visible && (!overrideHeartWiggle.HasValue || overrideHeartWiggle.Value))
                    {
                        heart.Wiggle();
                    }
                    if (deaths.Visible && (!overrideDeathWiggle.HasValue || overrideDeathWiggle.Value))
                    {
                        deaths.Wiggle();
                    }
                }
            }
            else
            {
                orig(self, wiggle, overrideStrawberryWiggle, overrideDeathWiggle, overrideHeartWiggle);
            }

        }

        private void modOuiChapterPanelReset(On.Celeste.OuiChapterPanel.orig_Reset orig, OuiChapterPanel self)
        {
            orig(self);
            if (!SaveData.Instance.DebugMode)
            {
                if (ModSaveData.CSideUnlocked.Contains(SaveData.Instance.GetLevelSetStats().Name + ":" + self.Area.ChapterIndex) && SaveData.Instance.UnlockedModes < 3)
                {
                    object CSideOption;
                    ((IList)OuiChapterPanel_modes.GetValue(self)).Add(
                        CSideOption = DynamicData.New(OuiChapterPanel_T_Option)(new
                        {
                            Label = Dialog.Clean("overworld_remix2"),
                            Icon = GFX.Gui["menu/rmx2"],
                            ID = "C"
                        })
                    );
                }
            }
        }

        private void MergeChaptersControllerCheck(AreaData data = null)
        {
            useMergeChaptersController = false;
            lastLevelSet = SaveData.Instance.GetLevelSetStats().Name;
            int areaOffset = SaveData.Instance.GetLevelSetStats().AreaOffset;
            if (data != null)
            {
                areaOffset = AreaData.Areas.FindIndex((AreaData area) => area.GetLevelSet() == data.LevelSet);
            }
            MapData MapData = AreaData.Areas[areaOffset].Mode[0].MapData;
            foreach (LevelData levelData in MapData.Levels)
            {
                foreach (EntityData entity in levelData.Entities)
                {
                    if (entity.Name == "XaphanHelper/MergeChaptersController")
                    {
                        useMergeChaptersController = true;
                        MergeChaptersControllerLevelSet = SaveData.Instance.GetLevelSetStats().Name;
                        MergeChaptersControllerMode = entity.Attr("mode");
                        MergeChaptersControllerKeepPrologue = entity.Bool("keepPrologueSeparated") && AreaData.Areas[SaveData.Instance.GetLevelSetStats().AreaOffset].Interlude_Safe;
                        break;
                    }
                }
            }
        }

        private void modOuiChapterSelectUpdate(On.Celeste.OuiChapterSelect.orig_Update orig, OuiChapterSelect self)
        {
            if (ModSaveData != null)
            {
                ModSaveData.LoadedPlayer = false;
            }
            if (SaveData.Instance != null)
            {
                if (lastLevelSet != SaveData.Instance.GetLevelSetStats().Name)
                {
                    MergeChaptersControllerCheck();
                    self.Overworld.Maddy.Hide();
                }
            }
            orig(self);
        }

        private IEnumerator modOuiChapterPanelStartRoutine(On.Celeste.OuiChapterPanel.orig_StartRoutine orig, OuiChapterPanel self, string checkpoint)
        {
            if (useMergeChaptersController && (SaveData.Instance.GetLevelSetStats().Name == "Xaphan/0" ? !ModSaveData.SpeedrunMode : true))
            {
                self.EnteringChapter = true;
                self.Overworld.Maddy.Hide(down: false);
                self.Overworld.Mountain.EaseCamera(self.Area.ID, self.Data.MountainZoom, 1f, true);
                self.Add(new Coroutine(self.EaseOut(removeChildren: false)));
                yield return 0.2f;
                ScreenWipe.WipeColor = Color.Black;
                AreaData.Get(self.Area).Wipe(self.Overworld, false, null);
                Audio.SetMusic(null);
                Audio.SetAmbience(null);
                yield return 0.5f;
                if (MergeChaptersControllerKeepPrologue && self.Area.ID == SaveData.Instance.GetLevelSetStats().AreaOffset)
                {
                    LevelEnter.Go(new Session(self.Area, checkpoint)
                    {
                        Time = 0L
                    }
                    , fromSaveData: false);
                }
                else
                {
                    LevelEnter.Go(new Session(self.Area, checkpoint)
                    {
                        Time = ModSaveData.SavedTime.ContainsKey(SaveData.Instance.GetLevelSetStats().Name) ? ModSaveData.SavedTime[SaveData.Instance.GetLevelSetStats().Name] : 0L
                    }
                    , fromSaveData: false);

                }
            }
            else
            {
                yield return new SwapImmediately(orig(self, checkpoint));
            }
        }

        private void modOuiChapterPanelStart(On.Celeste.OuiChapterPanel.orig_Start orig, OuiChapterPanel self, string checkpoint)
        {
            AreaStats areaStats = SaveData.Instance.Areas_Safe[self.Area.ID];
            if (self.Area.Mode == AreaMode.BSide && !areaStats.Cassette && ModSaveData.CSideUnlocked.Contains(SaveData.Instance.GetLevelSetStats().Name + ":" + self.Area.ChapterIndex) && !SaveData.Instance.DebugMode)
            {
                self.Focused = false;
                Audio.Play("event:/ui/world_map/chapter/checkpoint_start");
                self.Add(new Coroutine(StartChapterCSideRoutine(self, checkpoint)));
            }
            else
            {
                orig(self, checkpoint);
                self.Overworld.ShowInputUI = false;
            }
        }

        private IEnumerator modOuiChapterPanelIncrementStatsDisplay(On.Celeste.OuiChapterPanel.orig_IncrementStatsDisplay orig, OuiChapterPanel self, AreaModeStats modeStats, AreaModeStats newModeStats, bool doHeartGem, bool doStrawberries, bool doDeaths, bool doRemixUnlock)
        {
            AreaStats areaStats = SaveData.Instance.Areas_Safe[self.Data.ID];
            if (self.Area.Mode == AreaMode.BSide && !areaStats.Cassette && ModSaveData.CSideUnlocked.Contains(SaveData.Instance.GetLevelSetStats().Name + ":" + self.Area.ChapterIndex) && SaveData.Instance.UnlockedModes < 3 && !SaveData.Instance.DebugMode)
            {
                modeStats = self.DisplayedStats.Modes[2];
                newModeStats = areaStats.Modes[2];
                doStrawberries = newModeStats.TotalStrawberries > modeStats.TotalStrawberries;
                doHeartGem = newModeStats.HeartGem && !modeStats.HeartGem;
                doDeaths = newModeStats.Deaths > modeStats.Deaths && (self.Area.Mode != 0 || newModeStats.Completed);
            }
            yield return new SwapImmediately(orig(self, modeStats, newModeStats, doHeartGem, doStrawberries, doDeaths, doRemixUnlock));
        }

        private IEnumerator StartChapterCSideRoutine(OuiChapterPanel chapterPanel, string checkpoint = null)
        {
            int num = checkpoint?.IndexOf('|') ?? (-1);
            if (num >= 0)
            {
                chapterPanel.Area = (AreaDataExt.Get(checkpoint.Substring(0, num))?.ToKey(chapterPanel.Area.Mode) ?? chapterPanel.Area);
                checkpoint = checkpoint.Substring(num + 1);
            }
            chapterPanel.EnteringChapter = true;
            chapterPanel.Overworld.Maddy.Hide(down: false);
            chapterPanel.Overworld.Mountain.EaseCamera(chapterPanel.Area.ID, chapterPanel.Data.MountainZoom, 1f, false, true);
            chapterPanel.Add(new Coroutine(chapterPanel.EaseOut(removeChildren: false)));
            yield return 0.2f;
            ScreenWipe.WipeColor = Color.Black;
            AreaData.Get(chapterPanel.Area).Wipe(chapterPanel.Overworld, false, null);
            Audio.SetMusic(null);
            Audio.SetAmbience(null);
            yield return 0.5f;
            LevelEnter.Go(new Session(new AreaKey(chapterPanel.Area.ID, AreaMode.CSide), checkpoint), fromSaveData: false);
        }

        private void getTeleportToOtherSidePortalsData(Level level)
        {
            TeleportToOtherSideData.Clear();
            HashSet<int> Modes = new();
            Modes.Add(0);
            if (AreaData.Areas[level.Session.Area.ID].HasMode(AreaMode.BSide))
            {
                Modes.Add(1);
            }
            if (AreaData.Areas[level.Session.Area.ID].HasMode(AreaMode.CSide))
            {
                Modes.Add(2);
            }
            foreach (int mode in Modes)
            {
                MapData MapData = AreaData.Areas[level.Session.Area.ID].Mode[mode].MapData;
                if (MapData != null)
                {
                    foreach (LevelData levelData in MapData.Levels)
                    {
                        foreach (EntityData entity in levelData.Entities)
                        {
                            if (entity.Name == "XaphanHelper/TeleportToOtherSidePortal")
                            {
                                TeleportToOtherSideData.Add(new TeleportToOtherSideData(mode, levelData.Name, entity.Position, entity.Attr("side")));
                            }
                        }
                    }
                }
            }
        }

        private void getRoomMusicControllerData(Level level)
        {
            RoomMusicControllerData.Clear();
            MapData MapData = AreaData.Areas[level.Session.Area.ID].Mode[0].MapData;
            if (MapData != null)
            {
                foreach (LevelData levelData in MapData.Levels)
                {
                    foreach (EntityData entity in levelData.Entities)
                    {
                        if (entity.Name == "XaphanHelper/RoomMusicController")
                        {
                            RoomMusicControllerData.Add(new RoomMusicControllerData(entity.Attr("rooms"), entity.Attr("excludeRooms"), entity.Attr("flagInnactive"), entity.Attr("flagA"), entity.Attr("flagB"), entity.Attr("flagC"), entity.Attr("flagD"), entity.Attr("musicIfFlagA"), entity.Attr("musicIfFlagB"), entity.Attr("musicIfFlagC"), entity.Attr("musicIfFlagD"), entity.Attr("defaultMusic")));
                        }
                    }
                }
            }
        }

        private void onLevelLoad(Level level, Player.IntroTypes playerIntro, bool isFromLoader)
        {
            isInLevel = true;
            string room = level.Session.Level;
            string Prefix = level.Session.Area.GetLevelSet();
            int chapterIndex = level.Session.Area.ChapterIndex == -1 ? 0 : level.Session.Area.ChapterIndex;
            AreaKey area = level.Session.Area;
            MapData MapData = AreaData.Areas[area.ID].Mode[(int)area.Mode].MapData;

            // Define current side played

            ModSaveData.LastPlayedSide = (int)level.Session.Area.Mode;

            // Reset all checks

            useIngameMap = false;
            allRoomsUseTileController = false;
            useMetroidGameplay = false;
            useUpgrades = false;
            onSlope = false;

            // Checks controllers

            useIngameMapCheck(level);
            allRoomsUseTileControllerCheck(level);
            useMetroidGameplayCheck(level);
            useUpgradesCheck(level);
            getTeleportToOtherSidePortalsData(level);
            getRoomMusicControllerData(level);

            // Room Music stuff

            if (string.IsNullOrEmpty(level.Session.LevelData.Music))
            {
                foreach (RoomMusicControllerData roomMusicControllerData in RoomMusicControllerData)
                {
                    if (!level.Session.GetFlag(roomMusicControllerData.FlagInnactive) || string.IsNullOrEmpty(roomMusicControllerData.FlagInnactive))
                    {
                        string[] excludeRooms = roomMusicControllerData.ExcludeRooms.Split(',');
                        bool canApplyMusic = true;
                        foreach (string excludeRoom in excludeRooms)
                        {
                            if (level.Session.Level == excludeRoom)
                            {
                                canApplyMusic = false;
                                break;
                            }
                        }
                        if (level.Session.Level.Contains(roomMusicControllerData.Rooms) && canApplyMusic)
                        {
                            level.Session.Audio.Music.Event = SFX.EventnameByHandle(roomMusicControllerData.DefaultMusic);
                            if (level.Session.GetFlag(roomMusicControllerData.FlagA))
                            {
                                level.Session.Audio.Music.Event = SFX.EventnameByHandle(roomMusicControllerData.MusicIfFlagA);
                            }
                            else if (level.Session.GetFlag(roomMusicControllerData.FlagB))
                            {
                                level.Session.Audio.Music.Event = SFX.EventnameByHandle(roomMusicControllerData.MusicIfFlagB);
                            }
                            else if (level.Session.GetFlag(roomMusicControllerData.FlagC))
                            {
                                level.Session.Audio.Music.Event = SFX.EventnameByHandle(roomMusicControllerData.MusicIfFlagC);
                            }
                            else if (level.Session.GetFlag(roomMusicControllerData.FlagD))
                            {
                                level.Session.Audio.Music.Event = SFX.EventnameByHandle(roomMusicControllerData.MusicIfFlagD);
                            }
                        }
                        level.Session.Audio.Apply(forceSixteenthNoteHack: false);
                    }
                }
            }

            // In-game Map stuff

            if (useIngameMap)
            {
                // Check for backward compatibility. Restaure explored tiles if needed.

                foreach (string visitedRoom in ModSaveData.VisitedRooms)
                {
                    if (visitedRoom.Contains(Prefix + "/Ch" + chapterIndex + "/"))
                    {
                        if (ModSaveData.VisitedRoomsTiles.Count == 0)
                        {
                            MapDisplay.RestaureExploredTiles(Prefix, chapterIndex, level);
                            break;
                        }
                        else
                        {
                            bool skipRestaure = false;
                            foreach (string tile in ModSaveData.VisitedRoomsTiles)
                            {
                                if (tile.Contains(Prefix + "/Ch" + chapterIndex + "/"))
                                {
                                    skipRestaure = true;
                                }
                            }
                            if (!skipRestaure)
                            {
                                MapDisplay.RestaureExploredTiles(Prefix, chapterIndex, level);
                                break;
                            }
                        }
                    }
                }

                // Add current room to the in-game map

                if (!ModSaveData.VisitedRooms.Contains(Prefix + "/Ch" + chapterIndex + "/" + room))
                {
                    ModSaveData.VisitedRooms.Add(Prefix + "/Ch" + chapterIndex + "/" + room);
                }
                if (level.Session.GetFlag("Map_Opened"))
                {
                    level.Session.SetFlag("Map_Opened", false);
                }
            }

            // Activate Speedrun Mode if needed

            if (ModSaveData.SpeedrunMode)
            {
                ModSettings.SpeedrunMode = true;
                level.Session.SetFlag("Map_Collected");
                if (!ModSaveData.SavedFlags.Contains("Xaphan/0_Ch" + level.Session.Area.ChapterIndex + "_Map_Collected"))
                {
                    ModSaveData.SavedFlags.Add("Xaphan/0_Ch" + level.Session.Area.ChapterIndex + "_Map_Collected");
                }
            }

            // Upgrades stuff

            if (useUpgrades)
            {
                if (!ModSaveData.VisitedChapters.Contains(Prefix + "_Ch" + chapterIndex + "_" + (int)level.Session.Area.Mode))
                {
                    ModSaveData.VisitedChapters.Add(Prefix + "_Ch" + chapterIndex + "_" + (int)level.Session.Area.Mode);
                    ModSaveData.VisitedChapters.Sort();
                }
                GiveUpgradesToPlayer(MapData, level);
            }
            else // Set upgrades to default values if the map is not using upgrades
            {
                ModSettings.PowerGrip = true;
                ModSettings.ClimbingKit = true;
                ModSettings.SpiderMagnet = false;
                ModSettings.DroneTeleport = false;
                ModSettings.JumpBoost = false;
                ModSettings.ScrewAttack = false;
                ModSettings.VariaJacket = false;
                ModSettings.GravityJacket = false;
                ModSettings.Bombs = false;
                ModSettings.MegaBombs = false;
                ModSettings.RemoteDrone = false;
                ModSettings.GoldenFeather = false;
                ModSettings.Binoculars = false;
                ModSettings.EtherealDash = false;
                ModSettings.PortableStation = false;
                ModSettings.PulseRadar = false;
                ModSettings.DashBoots = true;
                ModSettings.SpaceJump = 1;
                ModSettings.HoverJet = false;
                ModSettings.LightningDash = false;
                ModSettings.LongBeam = false;
                ModSettings.IceBeam = false;
                ModSettings.WaveBeam = false;
                ModSettings.Spazer = false;
                ModSettings.PlasmaBeam = false;
                ModSettings.MorphingBall = false;
                ModSettings.MorphBombs = false;
                ModSettings.SpringBall = false;
                ModSettings.HighJumpBoots = false;
                ModSettings.SpeedBooster = false;
                ModSettings.MissilesModule = false;
                ModSettings.SuperMissilesModule = false;
            }

            // Set flags based on previous player progress

            if (!PlayerHasGolden && !ModSaveData.SavedFlags.Contains(Prefix + "_teleporting"))
            {
                if (!ModSettings.SpeedrunMode) // Normal mode only
                {
                    foreach (string savedFlag in ModSaveData.SavedFlags)
                    {
                        if (forceStartingUpgrades)
                        {
                            if (savedFlag.Contains("Upgrade_"))
                            {
                                continue;
                            }
                        }
                        string[] savedFlags = savedFlag.Split('_');
                        if (savedFlags[0] == Prefix && savedFlags[1] == "Ch" + chapterIndex)
                        {
                            string flagPrefix = savedFlags[0] + "_" + savedFlags[1] + "_";
                            string flag = string.Empty;
                            int num = savedFlag.IndexOf(flagPrefix);
                            if (num >= 0)
                            {
                                flag = savedFlag.Remove(num, flagPrefix.Length);
                            }
                            level.Session.SetFlag(flag);
                        }
                    }
                    foreach (string flag in ModSaveData.SavedFlags)
                    {
                        if (forceStartingUpgrades)
                        {
                            if (flag.Contains("Upgrade_"))
                            {
                                continue;
                            }
                        }
                        if (flag.Contains(Prefix))
                        {
                            string toRemove = Prefix + "_";
                            string result = string.Empty;
                            int i = flag.IndexOf(toRemove);
                            if (i >= 0)
                            {
                                result = flag.Remove(i, toRemove.Length);
                            }
                            level.Session.SetFlag(result, true);
                        }
                    }
                    foreach (string flag in ModSaveData.GlobalFlags)
                    {
                        if (flag.Contains(Prefix))
                        {
                            string toRemove = Prefix + "_";
                            string result = string.Empty;
                            int i = flag.IndexOf(toRemove);
                            if (i >= 0)
                            {
                                result = flag.Remove(i, toRemove.Length);
                            }
                            level.Session.SetFlag(result, true);
                        }
                    }
                }
            }

            // SoCM only

            if (level.Session.Area.GetLevelSet() == "Xaphan/0")
            {
                startedAnySoCMChapter = true;

                // Backward saves compatibility

                SaveUpdater.UpdateSave(level);
            }
            startedAnyChapter = true;
        }

        private static void GiveUpgradesToPlayer(MapData MapData, Level level)
        {
            // Remove all upgrades

            ModSettings.PowerGrip = false;
            ModSettings.ClimbingKit = false;
            ModSettings.SpiderMagnet = false;
            ModSettings.DroneTeleport = false;
            ModSettings.JumpBoost = false;
            ModSettings.ScrewAttack = false;
            ModSettings.VariaJacket = false;
            ModSettings.GravityJacket = false;
            ModSettings.Bombs = false;
            ModSettings.MegaBombs = false;
            ModSettings.RemoteDrone = false;
            ModSettings.GoldenFeather = false;
            ModSettings.Binoculars = false;
            ModSettings.EtherealDash = false;
            ModSettings.PortableStation = false;
            ModSettings.PulseRadar = false;
            ModSettings.DashBoots = false;
            ModSettings.SpaceJump = 1;
            ModSettings.HoverJet = false;
            ModSettings.LightningDash = false;
            ModSettings.LongBeam = false;
            ModSettings.IceBeam = false;
            ModSettings.WaveBeam = false;
            ModSettings.Spazer = false;
            ModSettings.PlasmaBeam = false;
            ModSettings.MorphingBall = false;
            ModSettings.MorphBombs = false;
            ModSettings.SpringBall = false;
            ModSettings.HighJumpBoots = false;
            ModSettings.SpeedBooster = false;
            ModSettings.MissilesModule = false;
            ModSettings.SuperMissilesModule = false;
            level.Session.SetFlag("Using_Elevator", false);

            // Get upgrades info from the Upgrade Controller

            bool setPowerGrip = false;
            bool setClimbingKit = false;
            bool setSpiderMagnet = false;
            bool setDroneTeleport = false;
            bool setJumpBoost = false;
            bool setScrewAttack = false;
            bool setVariaJacket = false;
            bool setGravityJacket = false;
            bool setBombs = false;
            bool setMegaBombs = false;
            bool setRemoteDrone = false;
            bool setGoldenFeather = false;
            bool setBinoculars = false;
            bool setEtherealDash = false;
            bool setPortableStation = false;
            bool setPulseRadar = false;
            bool setDashBoots = false;
            bool setSpaceJump = false;
            bool setHoverJet = false;
            bool setLightningDash = false;
            bool setLongBeam = false;
            bool setIceBeam = false;
            bool setWaveBeam = false;
            bool setMissilesModule = false;
            bool setSuperMissilesModule = false;
            bool hasStartingUpgrades = false;
            foreach (LevelData levelData in MapData.Levels)
            {
                foreach (EntityData entity in levelData.Entities)
                {
                    if (entity.Name == "XaphanHelper/UpgradeController")
                    {
                        setPowerGrip = entity.Bool("onlyAllowPowerGrip") || entity.Bool("startWithPowerGrip");
                        setClimbingKit = entity.Bool("onlyAllowClimbingKit") || entity.Bool("startWithClimbingKit");
                        setSpiderMagnet = entity.Bool("onlyAllowSpiderMagnet") || entity.Bool("startWithSpiderMagnet");
                        setDroneTeleport = entity.Bool("onlyAllowDroneTeleport") || entity.Bool("startWithDroneTeleport");
                        setJumpBoost = entity.Bool("onlyAllowJumpBoost") || entity.Bool("startWithJumpBoost");
                        setScrewAttack = entity.Bool("onlyAllowScrewAttack") || entity.Bool("startWithScrewAttack");
                        setVariaJacket = entity.Bool("onlyAllowVariaJacket") || entity.Bool("startWithVariaJacket");
                        setGravityJacket = entity.Bool("onlyAllowGravityJacket") || entity.Bool("startWithGravityJacket");
                        setBombs = entity.Bool("onlyAllowBombs") || entity.Bool("startWithBombs");
                        setMegaBombs = entity.Bool("onlyAllowMegaBombs") || entity.Bool("startWithMegaBombs");
                        setRemoteDrone = entity.Bool("onlyAllowRemoteDrone") || entity.Bool("startWithRemoteDrone");
                        setGoldenFeather = entity.Bool("onlyAllowGoldenFeather") || entity.Bool("startWithGoldenFeather");
                        setBinoculars = entity.Bool("onlyAllowBinoculars") || entity.Bool("startWithBinoculars");
                        setEtherealDash = entity.Bool("onlyAllowEtherealDash") || entity.Bool("startWithEtherealDash");
                        setPortableStation = entity.Bool("onlyAllowPortableStation") || entity.Bool("startWithPortableStation");
                        setPulseRadar = entity.Bool("onlyAllowPulseRadar") || entity.Bool("startWithPulseRadar");
                        setDashBoots = entity.Bool("onlyAllowDashBoots") || entity.Bool("startWithDashBoots");
                        setSpaceJump = entity.Bool("onlyAllowSpaceJump") || entity.Bool("startWithSpaceJump");
                        setHoverJet = entity.Bool("onlyAllowHoverJet") || entity.Bool("startWithHoverJet");
                        setLightningDash = entity.Bool("onlyAllowLightningDash") || entity.Bool("startWithLightningDash");
                        setLongBeam = entity.Bool("onlyAllowLongBeam") || entity.Bool("startWithLongBeam");
                        setIceBeam = entity.Bool("onlyAllowIceBeam") || entity.Bool("startWithIceBeam");
                        setWaveBeam = entity.Bool("onlyAllowWaveBeam") || entity.Bool("startWithWaveBeam");
                        setMissilesModule = entity.Bool("onlyAllowMissilesModule") || entity.Bool("startWithMissilesModule");
                        setSuperMissilesModule = entity.Bool("onlyAllowSuperMissilesModule") || entity.Bool("startWithSuperMissilesModule");
                        hasStartingUpgrades = setPowerGrip || setClimbingKit || setSpiderMagnet || setDroneTeleport || setJumpBoost || setScrewAttack || setVariaJacket || setGravityJacket || setBombs || setMegaBombs || setRemoteDrone || setGoldenFeather || setBinoculars || setEtherealDash || setPortableStation || setPulseRadar || setDashBoots || setSpaceJump || setHoverJet || setLightningDash || setLongBeam || setIceBeam || setWaveBeam || setMissilesModule || setSuperMissilesModule;
                        forceStartingUpgrades = entity.Bool("onlyAllowStartingUpgrades", hasStartingUpgrades ? true : false);
                        break;
                    }
                }
            }

            // Check specified upgrades for the golden berry and speedrun mode

            bool goldenPowerGrip = false;
            bool goldenClimbingKit = false;
            bool goldenSpiderMagnet = false;
            bool goldenDroneTeleport = false;
            bool goldenJumpBoost = false;
            bool goldenScrewAttack = false;
            bool goldenVariaJacket = false;
            bool goldenGravityJacket = false;
            bool goldenBinoculars = false;
            bool goldenEtherealDash = false;
            bool goldenBombs = false;
            bool goldenMegaBombs = false;
            bool goldenRemoteDrone = false;
            bool goldenGoldenFeather = false;
            bool goldenPortableStation = false;
            bool goldenPulseRadar = false;
            bool goldenDashBoots = false;
            bool goldenSpaceJump = false;
            bool goldenHoverJet = false;
            bool goldenLightningDash = false;
            bool goldenLongBeam = false;
            bool goldenIceBeam = false;
            bool goldenWaveBeam = false;
            bool goldenMissilesModule = false;
            bool goldenSuperMissilesModule = false;
            foreach (LevelData levelData in MapData.Levels)
            {
                foreach (EntityData entity in levelData.Entities)
                {
                    if (entity.Name == "XaphanHelper/UpgradeController")
                    {
                        goldenPowerGrip = entity.Bool("goldenStartWithPowerGrip");
                        goldenClimbingKit = entity.Bool("goldenStartWithClimbingKit");
                        goldenSpiderMagnet = entity.Bool("goldenStartWithSpiderMagnet");
                        goldenDroneTeleport = entity.Bool("goldenStartWithDroneTeleport");
                        goldenJumpBoost = entity.Bool("goldenStartWithJumpBoost");
                        goldenScrewAttack = entity.Bool("goldenStartWithScrewAttack");
                        goldenVariaJacket = entity.Bool("goldenStartWithVariaJacket");
                        goldenGravityJacket = entity.Bool("goldenStartWithGravityJacket");
                        goldenBombs = entity.Bool("goldenStartWithBombs");
                        goldenMegaBombs = entity.Bool("goldenStartWithMegaBombs");
                        goldenRemoteDrone = entity.Bool("goldenStartWithRemoteDrone");
                        goldenGoldenFeather = entity.Bool("goldenStartWithGoldenFeather");
                        goldenBinoculars = entity.Bool("goldenStartWithBinoculars");
                        goldenEtherealDash = entity.Bool("goldenStartWithEtherealDash");
                        goldenPortableStation = entity.Bool("goldenStartWithPortableStation");
                        goldenPulseRadar = entity.Bool("goldenStartWithPulseRadar");
                        goldenDashBoots = entity.Bool("goldenStartWithDashBoots");
                        goldenSpaceJump = entity.Bool("goldenStartWithSpaceJump");
                        goldenHoverJet = entity.Bool("goldenStartWithHoverJet");
                        goldenLightningDash = entity.Bool("goldenStartWithLightningDash");
                        goldenLongBeam = entity.Bool("goldenStartWithLongBeam");
                        goldenIceBeam = entity.Bool("goldenStartWithIceBeam");
                        goldenWaveBeam = entity.Bool("goldenStartWithWaveBeam");
                        goldenMissilesModule = entity.Bool("goldenStartWithMissilesModule");
                        goldenSuperMissilesModule = entity.Bool("goldenStartWithSuperMissilesModule");
                        break;
                    }
                }
            }

            // Give specified upgrades

            if (hasStartingUpgrades)
            {
                if (setPowerGrip || level.Session.GetFlag("Upgrade_PowerGrip"))
                {
                    ModSettings.PowerGrip = true;
                    level.Session.SetFlag("Upgrade_PowerGrip", true);
                }
                if (setClimbingKit || level.Session.GetFlag("Upgrade_ClimbingKit"))
                {
                    ModSettings.ClimbingKit = true;
                    level.Session.SetFlag("Upgrade_ClimbingKit", true);
                }
                if (setSpiderMagnet || level.Session.GetFlag("Upgrade_SpiderMagnet"))
                {
                    ModSettings.SpiderMagnet = true;
                    level.Session.SetFlag("Upgrade_SpiderMagnet", true);
                }
                if (setDroneTeleport || level.Session.GetFlag("Upgrade_DroneTeleport"))
                {
                    ModSettings.DroneTeleport = true;
                    level.Session.SetFlag("Upgrade_DroneTeleport", true);
                }
                if (setJumpBoost || level.Session.GetFlag("Upgrade_JumpBoost"))
                {
                    ModSettings.JumpBoost = true;
                    level.Session.SetFlag("Upgrade_JumpBoost", true);
                }
                if (setScrewAttack || level.Session.GetFlag("Upgrade_ScrewAttack"))
                {
                    ModSettings.ScrewAttack = true;
                    level.Session.SetFlag("Upgrade_ScrewAttack", true);
                }
                if (setVariaJacket || level.Session.GetFlag("Upgrade_VariaJacket"))
                {
                    ModSettings.VariaJacket = true;
                    level.Session.SetFlag("Upgrade_VariaJacket", true);
                }
                if (setGravityJacket || level.Session.GetFlag("Upgrade_GravityJacket"))
                {
                    ModSettings.GravityJacket = true;
                    level.Session.SetFlag("Upgrade_GravityJacket", true);
                }
                if (setBombs || level.Session.GetFlag("Upgrade_Bombs"))
                {
                    ModSettings.Bombs = true;
                    level.Session.SetFlag("Upgrade_Bombs", true);
                }
                if (setMegaBombs || level.Session.GetFlag("Upgrade_MegaBombs"))
                {
                    ModSettings.MegaBombs = true;
                    level.Session.SetFlag("Upgrade_MegaBombs", true);
                }
                if (setRemoteDrone || level.Session.GetFlag("Upgrade_RemoteDrone"))
                {
                    ModSettings.RemoteDrone = true;
                    level.Session.SetFlag("Upgrade_RemoteDrone", true);
                }
                if (setGoldenFeather || level.Session.GetFlag("Upgrade_GoldenFeather"))
                {
                    ModSettings.GoldenFeather = true;
                    level.Session.SetFlag("Upgrade_GoldenFeather", true);
                }
                if (setBinoculars || level.Session.GetFlag("Upgrade_Binoculars"))
                {
                    ModSettings.Binoculars = true;
                    level.Session.SetFlag("Upgrade_Binoculars", true);
                }
                if (setEtherealDash || level.Session.GetFlag("Upgrade_EtherealDash"))
                {
                    ModSettings.EtherealDash = true;
                    level.Session.SetFlag("Upgrade_EtherealDash", true);
                }
                if (setPortableStation || level.Session.GetFlag("Upgrade_PortableStation"))
                {
                    ModSettings.PortableStation = true;
                    level.Session.SetFlag("Upgrade_PortableStation", true);
                }
                if (setPulseRadar || level.Session.GetFlag("Upgrade_PulseRadar"))
                {
                    ModSettings.PulseRadar = true;
                    level.Session.SetFlag("Upgrade_PulseRadar", true);
                }
                if (setDashBoots || level.Session.GetFlag("Upgrade_DashBoots"))
                {
                    ModSettings.DashBoots = true;
                    level.Session.SetFlag("Upgrade_DashBoots", true);
                }
                if (setSpaceJump || level.Session.GetFlag("Upgrade_SpaceJump"))
                {
                    ModSettings.SpaceJump = 2;
                    level.Session.SetFlag("Upgrade_SpaceJump", true);
                }
                if (setHoverJet || level.Session.GetFlag("Upgrade_HoverJet"))
                {
                    ModSettings.HoverJet = true;
                    level.Session.SetFlag("Upgrade_HoverJet", true);
                }
                if (setLightningDash || level.Session.GetFlag("Upgrade_LightningDash"))
                {
                    ModSettings.LightningDash = true;
                    level.Session.SetFlag("Upgrade_LightningDash", true);
                }
                if (setLongBeam || level.Session.GetFlag("Upgrade_LongBeam"))
                {
                    ModSettings.LongBeam = true;
                    level.Session.SetFlag("Upgrade_LongBeam", true);
                }
                if (setIceBeam || level.Session.GetFlag("Upgrade_IceBeam"))
                {
                    ModSettings.IceBeam = true;
                    level.Session.SetFlag("Upgrade_IceBeam", true);
                }
                if (setWaveBeam || level.Session.GetFlag("Upgrade_WaveBeam"))
                {
                    ModSettings.WaveBeam = true;
                    level.Session.SetFlag("Upgrade_WaveBeam", true);
                }
                if (setMissilesModule || level.Session.GetFlag("Upgrade_MissilesModule"))
                {
                    ModSettings.MissilesModule = true;
                    level.Session.SetFlag("Upgrade_MissilesModule", true);
                }
                if (setSuperMissilesModule || level.Session.GetFlag("Upgrade_SuperMissilesModule"))
                {
                    ModSettings.SuperMissilesModule = true;
                    level.Session.SetFlag("Upgrade_SuperMissilesModule", true);
                }
            }
            else
            {
                // Give back upgrades the player has unlocked

                if (!forceStartingUpgrades && !PlayerHasGolden && !ModSettings.SpeedrunMode)
                {
                    // Celeste Upgrades

                    if (PowerGripCollected(level))
                    {
                        ModSettings.PowerGrip = true;
                        level.Session.SetFlag("Upgrade_PowerGrip", true);
                    }
                    if (ClimbingKitCollected(level))
                    {
                        ModSettings.ClimbingKit = true;
                        level.Session.SetFlag("Upgrade_ClimbingKit", true);
                    }
                    if (SpiderMagnetCollected(level))
                    {
                        ModSettings.SpiderMagnet = true;
                        level.Session.SetFlag("Upgrade_SpiderMagnet", true);
                    }
                    if (DroneTeleportCollected(level))
                    {
                        ModSettings.DroneTeleport = true;
                        level.Session.SetFlag("Upgrade_DroneTeleport", true);
                    }
                    if (JumpBoostCollected(level))
                    {
                        ModSettings.JumpBoost = true;
                        level.Session.SetFlag("Upgrade_JumpBoost", true);
                    }
                    if (BombsCollected(level))
                    {
                        ModSettings.Bombs = true;
                        level.Session.SetFlag("Upgrade_Bombs", true);
                    }
                    if (MegaBombsCollected(level))
                    {
                        ModSettings.MegaBombs = true;
                        level.Session.SetFlag("Upgrade_MegaBombs", true);
                    }
                    if (RemoteDroneCollected(level))
                    {
                        ModSettings.RemoteDrone = true;
                        level.Session.SetFlag("Upgrade_RemoteDrone", true);
                    }
                    if (GoldenFeatherCollected(level))
                    {
                        ModSettings.GoldenFeather = true;
                        level.Session.SetFlag("Upgrade_GoldenFeather", true);
                    }
                    if (BinocularsCollected(level))
                    {
                        ModSettings.Binoculars = true;
                        level.Session.SetFlag("Upgrade_Binoculars", true);
                    }
                    if (EtherealDashCollected(level))
                    {
                        ModSettings.EtherealDash = true;
                        level.Session.SetFlag("Upgrade_EtherealDash", true);
                    }
                    if (PortableStationCollected(level))
                    {
                        ModSettings.PortableStation = true;
                        level.Session.SetFlag("Upgrade_PortableStation", true);
                    }
                    if (PulseRadarCollected(level))
                    {
                        ModSettings.PulseRadar = true;
                        level.Session.SetFlag("Upgrade_PulseRadar", true);
                    }
                    if (DashBootsCollected(level))
                    {
                        ModSettings.DashBoots = true;
                        level.Session.SetFlag("Upgrade_DashBoots", true);
                    }
                    if (HoverJetCollected(level))
                    {
                        ModSettings.HoverJet = true;
                        level.Session.SetFlag("Upgrade_HoverJet", true);
                    }
                    if (LightningDashCollected(level))
                    {
                        ModSettings.LightningDash = true;
                        level.Session.SetFlag("Upgrade_LightningDash", true);
                    }
                    if (MissilesModuleCollected(level))
                    {
                        ModSettings.MissilesModule = true;
                        level.Session.SetFlag("Upgrade_MissilesModule", true);
                    }
                    if (SuperMissilesModuleCollected(level))
                    {
                        ModSettings.SuperMissilesModule = true;
                        level.Session.SetFlag("SuperUpgrade_MissilesModule", true);
                    }

                    //Metroid Upgrades

                    if (SpazerCollected(level))
                    {
                        ModSettings.Spazer = true;
                        level.Session.SetFlag("Upgrade_Spazer", true);
                    }
                    if (PlasmaBeamCollected(level))
                    {
                        ModSettings.PlasmaBeam = true;
                        level.Session.SetFlag("Upgrade_PlasmaBeam", true);
                    }
                    if (MorphingBallCollected(level))
                    {
                        ModSettings.MorphingBall = true;
                        level.Session.SetFlag("Upgrade_MorphingBall", true);
                    }
                    if (MorphBombsCollected(level))
                    {
                        ModSettings.MorphBombs = true;
                        level.Session.SetFlag("Upgrade_MorphBombs", true);
                    }
                    if (SpringBallCollected(level))
                    {
                        ModSettings.SpringBall = true;
                        level.Session.SetFlag("Upgrade_SpringBall", true);
                    }
                    if (HighJumpBootsCollected(level))
                    {
                        ModSettings.HighJumpBoots = true;
                        level.Session.SetFlag("Upgrade_HighJumpBoots", true);
                    }
                    if (SpeedBoosterCollected(level))
                    {
                        ModSettings.SpeedBooster = true;
                        level.Session.SetFlag("Upgrade_SpeedBooster", true);
                    }

                    // Common Upgrades

                    if (LongBeamCollected(level))
                    {
                        ModSettings.LongBeam = true;
                        level.Session.SetFlag("Upgrade_LongBeam", true);
                    }
                    if (IceBeamCollected(level))
                    {
                        ModSettings.IceBeam = true;
                        level.Session.SetFlag("Upgrade_IceBeam", true);
                    }
                    if (WaveBeamCollected(level))
                    {
                        ModSettings.WaveBeam = true;
                        level.Session.SetFlag("Upgrade_WaveBeam", true);
                    }
                    if (VariaJacketCollected(level))
                    {
                        ModSettings.VariaJacket = true;
                        level.Session.SetFlag("Upgrade_VariaJacket", true);
                    }
                    if (GravityJacketCollected(level))
                    {
                        ModSettings.GravityJacket = true;
                        level.Session.SetFlag("Upgrade_GravityJacket", true);
                    }
                    if (ScrewAttackCollected(level))
                    {
                        ModSettings.ScrewAttack = true;
                        level.Session.SetFlag("Upgrade_ScrewAttack", true);
                    }
                    if (SpaceJumpCollected(level))
                    {
                        ModSettings.SpaceJump = !useMetroidGameplay ? 2 : 6;
                        level.Session.SetFlag("Upgrade_SpaceJump", true);
                    }
                }
                else if (PlayerHasGolden || ModSettings.SpeedrunMode) // If the player has the golden berry or is in speedrun mode
                {
                    if (goldenPowerGrip || level.Session.GetFlag("Upgrade_PowerGrip"))
                    {
                        ModSettings.PowerGrip = true;
                    }
                    if (goldenClimbingKit || level.Session.GetFlag("Upgrade_ClimbingKit"))
                    {
                        ModSettings.ClimbingKit = true;
                    }
                    if (goldenSpiderMagnet || level.Session.GetFlag("Upgrade_SpiderMagnet"))
                    {
                        ModSettings.SpiderMagnet = true;
                    }
                    if (goldenDroneTeleport || level.Session.GetFlag("Upgrade_DroneTeleport"))
                    {
                        ModSettings.DroneTeleport = true;
                    }
                    if (goldenJumpBoost || level.Session.GetFlag("Upgrade_JumpBoost"))
                    {
                        ModSettings.JumpBoost = true;
                    }
                    if (goldenScrewAttack || level.Session.GetFlag("Upgrade_ScrewAttack"))
                    {
                        ModSettings.ScrewAttack = true;
                    }
                    if (goldenVariaJacket || level.Session.GetFlag("Upgrade_VariaJacket"))
                    {
                        ModSettings.VariaJacket = true;
                    }
                    if (goldenGravityJacket || level.Session.GetFlag("Upgrade_GravityJacket"))
                    {
                        ModSettings.GravityJacket = true;
                    }
                    if (goldenBombs || level.Session.GetFlag("Upgrade_Bombs"))
                    {
                        ModSettings.Bombs = true;
                    }
                    if (goldenMegaBombs || level.Session.GetFlag("Upgrade_MegaBombs"))
                    {
                        ModSettings.MegaBombs = true;
                    }
                    if (goldenRemoteDrone || level.Session.GetFlag("Upgrade_RemoteDrone"))
                    {
                        ModSettings.RemoteDrone = true;
                    }
                    if (goldenGoldenFeather || level.Session.GetFlag("Upgrade_GoldenFeather"))
                    {
                        ModSettings.GoldenFeather = true;
                    }
                    if (goldenBinoculars || level.Session.GetFlag("Upgrade_Binoculars"))
                    {
                        ModSettings.Binoculars = true;
                    }
                    if (goldenEtherealDash || level.Session.GetFlag("Upgrade_EtherealDash"))
                    {
                        ModSettings.EtherealDash = true;
                    }
                    if (goldenPortableStation || level.Session.GetFlag("Upgrade_PortableStation"))
                    {
                        ModSettings.PortableStation = true;
                    }
                    if (goldenPulseRadar || level.Session.GetFlag("Upgrade_PulseRadar"))
                    {
                        ModSettings.PulseRadar = true;
                    }
                    if (goldenDashBoots || level.Session.GetFlag("Upgrade_DashBoots"))
                    {
                        ModSettings.DashBoots = true;
                    }
                    if (goldenSpaceJump || level.Session.GetFlag("Upgrade_SpaceJump"))
                    {
                        ModSettings.SpaceJump = 2;
                    }
                    if (goldenHoverJet || level.Session.GetFlag("Upgrade_HoverJet"))
                    {
                        ModSettings.HoverJet = true;
                    }
                    if (goldenLightningDash || level.Session.GetFlag("Upgrade_LightningDash"))
                    {
                        ModSettings.LightningDash = true;
                    }
                    if (goldenLongBeam || level.Session.GetFlag("Upgrade_LongBeam"))
                    {
                        ModSettings.LongBeam = true;
                    }
                    if (goldenIceBeam || level.Session.GetFlag("Upgrade_IceBeam"))
                    {
                        ModSettings.IceBeam = true;
                    }
                    if (goldenWaveBeam || level.Session.GetFlag("Upgrade_WaveBeam"))
                    {
                        ModSettings.WaveBeam = true;
                    }
                    if (goldenMissilesModule || level.Session.GetFlag("Upgrade_MissilesModule"))
                    {
                        ModSettings.MissilesModule = true;
                    }
                    if (goldenSuperMissilesModule || level.Session.GetFlag("Upgrade_SuperMissilesModule"))
                    {
                        ModSettings.SuperMissilesModule = true;
                    }
                }
            }
        }

        private void onLevelExit(Level level, LevelExit exit, LevelExit.Mode mode, Session session, HiresSnow snow)
        {
            // Remove checks;

            useIngameMap = false;
            allRoomsUseTileController = false;
            useUpgrades = false;
            useMetroidGameplay = false;

            // Remove Speedrun Mode

            if (ModSettings.SpeedrunMode)
            {
                ModSaveData.SpeedrunModeUnlockedWarps.Clear();
                ModSaveData.SpeedrunModeStaminaUpgrades.Clear();
                ModSaveData.SpeedrunModeDroneMissilesUpgrades.Clear();
                ModSaveData.SpeedrunModeDroneSuperMissilesUpgrades.Clear();
                ModSaveData.SpeedrunModeDroneFireRateUpgrades.Clear();
                ModSettings.SpeedrunMode = false;
            }

            // Remove PickedGolden flag from save

            PlayerHasGolden = false;

            // Reset Variables

            ModSaveData.DestinationRoom = "";
            ModSaveData.CountdownCurrentTime = -1;
            ModSaveData.CountdownShake = false;
            ModSaveData.CountdownExplode = false;
            if (!string.IsNullOrEmpty(ModSaveData.CountdownActiveFlag) && ModSaveData.SavedSesionFlags.ContainsKey(session.Area.LevelSet))
            {
                ModSaveData.SavedSesionFlags[session.Area.LevelSet] = ModSaveData.SavedSesionFlags[session.Area.LevelSet].Replace(ModSaveData.CountdownActiveFlag + ",", "");
                ModSaveData.SavedSesionFlags[session.Area.LevelSet] = ModSaveData.SavedSesionFlags[session.Area.LevelSet].Replace("," + ModSaveData.CountdownActiveFlag, "");
            }
            ModSaveData.CountdownActiveFlag = "";
            ModSaveData.CountdownStartChapter = -1;
            ModSaveData.CountdownStartRoom = "";
            ModSaveData.CountdownSpawn = new Vector2();
            ModSaveData.CountdownUseLevelWipe = false;
            ModSaveData.BagUIId1 = 0;
            ModSaveData.BagUIId2 = 0;
            ModSaveData.LoadedPlayer = false;
            startedAnySoCMChapter = false;
            minimapEnabled = false;
            TriggeredCountDown = false;
            ModSaveData.CanDisplayAchievementsPopups = false;
            if ((!useMergeChaptersController || (useMergeChaptersController && MergeChaptersControllerMode != "Rooms")) && mode != LevelExit.Mode.SaveAndQuit)
            {
                ModSaveData.startAsDrone.Remove(level.Session.Area.GetLevelSet());
                ModSaveData.droneStartChapter.Remove(level.Session.Area.GetLevelSet());
                ModSaveData.droneStartRoom.Remove(level.Session.Area.GetLevelSet());
                ModSaveData.droneStartSpawn.Remove(level.Session.Area.GetLevelSet());
                ModSaveData.droneCurrentSpawn.Remove(level.Session.Area.GetLevelSet());
                ModSaveData.fakePlayerFacing.Remove(level.Session.Area.GetLevelSet());
                ModSaveData.fakePlayerPosition.Remove(level.Session.Area.GetLevelSet());
            }
            NoDroneSpawnSound = false;
            SaveSettings();

            onSlope = false;
            isInLevel = false;
        }

        private static void onCreatePauseMenuButtons(Level level, TextMenu menu, bool minimal)
        {
            if (useMetroidGameplay)
            {
                // Find the "Retry" item and remove it from the menu if it exist
                int retryIndex = menu.GetItems().FindIndex(item => item.GetType() == typeof(TextMenu.Button) && ((TextMenu.Button)item).Label == Dialog.Clean("MENU_PAUSE_RETRY"));
                if (retryIndex != -1)
                {
                    menu.Remove(menu.Items[retryIndex]);
                }
            }
            if (level.Session.GetFlag("boss_Challenge_Mode") && !level.Session.GetFlag("In_bossfight"))
            {
                // Find the position of "Retry"
                int retryIndex = menu.GetItems().FindIndex(item => item.GetType() == typeof(TextMenu.Button) && ((TextMenu.Button)item).Label == Dialog.Clean("MENU_PAUSE_RETRY"));

                if (retryIndex == -1)
                {
                    // Top of the menu if "Retry" is not found
                    retryIndex = 0;
                }

                // add the "Giveup Challenge Mode" button
                TextMenu.Button GiveUpCMButton = new(Dialog.Clean("XaphanHelper_UI_GiveUpCM"));
                GiveUpCMButton.Pressed(() =>
                {
                    level.PauseMainMenuOpen = false;
                    menu.RemoveSelf();
                    confirmGiveUpCMMenu(level, menu.Selection);
                });
                GiveUpCMButton.ConfirmSfx = "event:/ui/main/message_confirm";
                menu.Insert(retryIndex + 1, GiveUpCMButton);
            }
            if (useMergeChaptersController && (MergeChaptersControllerKeepPrologue ? SaveData.Instance.LastArea_Safe.ID != SaveData.Instance.GetLevelSetStats().AreaOffset : true) && (level.Session.Area.LevelSet == "Xaphan/0" ? !ModSaveData.SpeedrunMode : true))
            {
                // Find the "Restart chapter" button and remove it from the menu if it exist
                int restartAreaIndex = menu.GetItems().FindIndex(item => item.GetType() == typeof(TextMenu.Button) && ((TextMenu.Button)item).Label == Dialog.Clean("MENU_PAUSE_RESTARTAREA"));
                if (restartAreaIndex != -1)
                {
                    menu.Remove(menu.Items[restartAreaIndex]);
                }

                // add the "Restart campaign" button
                TextMenu.Button RestartCampaignButton = new(Dialog.Clean("XaphanHelper_UI_RestartCampaign"));
                RestartCampaignButton.Pressed(() =>
                {
                    level.PauseMainMenuOpen = false;
                    menu.RemoveSelf();
                    confirmRestartCampaign(level, menu.Selection);
                });
                RestartCampaignButton.ConfirmSfx = "event:/ui/main/message_confirm";
                menu.Insert(restartAreaIndex, RestartCampaignButton);
            }
            if (useMergeChaptersController && (level.Session.Area.Mode == AreaMode.BSide || level.Session.Area.Mode == AreaMode.CSide) && (level.Session.Area.LevelSet == "Xaphan/0" ? !ModSaveData.SpeedrunMode : true))
            {
                // Find the position of "Return to map"
                int returnToMapIndex = menu.GetItems().FindIndex(item => item.GetType() == typeof(TextMenu.Button) && ((TextMenu.Button)item).Label == Dialog.Clean("MENU_PAUSE_RETURN"));

                if (returnToMapIndex == -1)
                {
                    // Bottm of the menu if "Return to map" is not found
                    returnToMapIndex = menu.GetItems().Count - 1;
                }

                // remove "Return to map" from the menu
                if (returnToMapIndex != -1)
                {
                    menu.Remove(menu.Items[returnToMapIndex]);
                }

                // add the "Exit X-Side" button
                TextMenu.Button ExitSideButton = new(Dialog.Clean("XaphanHelper_UI_GiveUp" + (level.Session.Area.Mode == AreaMode.BSide ? "B" : "C") + "Side"));
                ExitSideButton.Pressed(() =>
                {
                    level.PauseMainMenuOpen = false;
                    menu.RemoveSelf();
                    confirmExitSideMenu(level, menu.Selection);
                });
                ExitSideButton.ConfirmSfx = "event:/ui/main/message_confirm";
                menu.Insert(returnToMapIndex, ExitSideButton);
            }
        }

        private static void confirmGiveUpCMMenu(Level level, int returnIndex)
        {
            ChallengeMote CMote = level.Tracker.GetEntity<ChallengeMote>();
            if (CMote != null)
            {
                level.Paused = true;
                TextMenu menu = new();
                menu.AutoScroll = false;
                menu.Position = new Vector2(Engine.Width / 2f, Engine.Height / 2f - 100f);
                menu.Add(new TextMenu.Header(Dialog.Clean("XaphanHelper_UI_GiveUpCM_title")));
                menu.Add(new TextMenu.Button(Dialog.Clean("menu_return_continue")).Pressed(delegate
                {
                    menu.RemoveSelf();
                    CMote.ManageUpgrades(level, true);
                    level.Session.SetFlag("boss_Challenge_Mode_Given_Up", true);
                    level.Session.SetFlag("Boss_Defeated", true);
                    level.Session.SetFlag("boss_Challenge_Mode", false);
                    level.Paused = false;
                    Engine.FreezeTimer = 0.15f;
                }));
                menu.Add(new TextMenu.Button(Dialog.Clean("menu_return_cancel")).Pressed(delegate
                {
                    menu.OnCancel();
                }));
                menu.OnPause = (menu.OnESC = delegate
                {
                    menu.RemoveSelf();
                    level.Paused = false;
                    Engine.FreezeTimer = 0.15f;
                    Audio.Play("event:/ui/game/unpause");
                });
                menu.OnCancel = delegate
                {
                    Audio.Play("event:/ui/main/button_back");
                    menu.RemoveSelf();
                    level.Pause(returnIndex, minimal: false);
                };
                level.Add(menu);
            }
        }

        private static void confirmExitSideMenu(Level level, int returnIndex)
        {
            level.Paused = true;
            ReturnToASideHint returnHint = null;
            level.Add(returnHint = new ReturnToASideHint());
            TextMenu menu = new();
            menu.AutoScroll = false;
            menu.Position = new Vector2(Engine.Width / 2f, Engine.Height / 2f - 100f);
            menu.Add(new TextMenu.Header(Dialog.Clean("XaphanHelper_UI_GiveUp" + (level.Session.Area.Mode == AreaMode.BSide ? "B" : "C") + "Side_title")));
            menu.Add(new TextMenu.Button(Dialog.Clean("menu_return_continue")).Pressed(delegate
            {
                menu.RemoveSelf();
                returnHint.RemoveSelf();
                PlayerHasGolden = false;
                ChangingSide = true;
                Audio.SetMusic(null);
                Audio.SetAmbience(null);
                ModSaveData.LoadedPlayer = false;
                level.DoScreenWipe(false, delegate
                {
                    LevelEnter.Go(new Session(new AreaKey(level.Session.Area.ID, AreaMode.Normal))
                    {
                        Time = ModSaveData.SavedTime.ContainsKey(level.Session.Area.LevelSet) ? ModSaveData.SavedTime[level.Session.Area.LevelSet] : 0L
                    }
                    , fromSaveData: false);
                });
            }));
            menu.Add(new TextMenu.Button(Dialog.Clean("menu_return_cancel")).Pressed(delegate
            {
                menu.OnCancel();
            }));
            menu.OnPause = (menu.OnESC = delegate
            {
                menu.RemoveSelf();
                returnHint.RemoveSelf();
                level.Paused = false;
                Engine.FreezeTimer = 0.15f;
                Audio.Play("event:/ui/game/unpause");
            });
            menu.OnCancel = delegate
            {
                Audio.Play("event:/ui/main/button_back");
                menu.RemoveSelf();
                returnHint.RemoveSelf();
                level.Pause(returnIndex, minimal: false);
            };
            level.Add(menu);
        }

        private static void confirmRestartCampaign(Level level, int returnIndex)
        {
            level.Paused = true;
            RestartCampaignHint returnHint = null;
            level.Add(returnHint = new RestartCampaignHint());
            TextMenu menu = new();
            menu.AutoScroll = false;
            menu.Position = new Vector2(Engine.Width / 2f, Engine.Height / 2f - 100f);
            menu.Add(new TextMenu.Header(Dialog.Clean("XaphanHelper_UI_RestartCampaign_title")));
            menu.Add(new TextMenu.Button(Dialog.Clean("menu_restart_continue")).Pressed(delegate
            {
                returnHint.RemoveSelf();
                Engine.TimeRate = 1f;
                menu.Focused = false;
                level.Session.InArea = false;
                Audio.SetMusic(null);
                Audio.BusStopAll("bus:/gameplay_sfx", immediate: true);
                level.DoScreenWipe(false, delegate
                {
                    Commands.Cmd_Clear_InGameMap(true, true);
                    Commands.Cmd_Clear_Warps();
                    Commands.Cmd_Remove_Upgrades();
                    Commands.Cmd_Reset_Collectables_Upgrades();

                    ModSaveData.SavedRoom.Remove(level.Session.Area.LevelSet);
                    ModSaveData.SavedChapter.Remove(level.Session.Area.LevelSet);
                    ModSaveData.SavedSpawn.Remove(level.Session.Area.LevelSet);
                    ModSaveData.SavedLightingAlphaAdd.Remove(level.Session.Area.LevelSet);
                    ModSaveData.SavedBloomBaseAdd.Remove(level.Session.Area.LevelSet);
                    ModSaveData.SavedCoreMode.Remove(level.Session.Area.LevelSet);
                    ModSaveData.SavedMusic.Remove(level.Session.Area.LevelSet);
                    ModSaveData.SavedAmbience.Remove(level.Session.Area.LevelSet);
                    ModSaveData.SavedNoLoadEntities.Remove(level.Session.Area.LevelSet);
                    ModSaveData.SavedTime.Remove(level.Session.Area.LevelSet);
                    ModSaveData.SavedFromBeginning.Remove(level.Session.Area.LevelSet);
                    ModSaveData.SavedSesionFlags.Remove(level.Session.Area.LevelSet);
                    ModSaveData.SavedSessionStrawberries.Remove(level.Session.Area.LevelSet);
                    List<string> FlagsToRemove = new();
                    List<string> CutscenesToRemove = new();
                    List<string> GlobalFlagsToRemove = new();
                    foreach (string savedFlag in ModSaveData.SavedFlags)
                    {
                        if (savedFlag.Contains(level.Session.Area.LevelSet) && savedFlag != "Xaphan/0_Skip_Vignette")
                        {
                            FlagsToRemove.Add(savedFlag);
                        }
                    }
                    foreach (string cutscene in ModSaveData.WatchedCutscenes)
                    {
                        if (cutscene.Contains(level.Session.Area.LevelSet))
                        {
                            CutscenesToRemove.Add(cutscene);
                        }
                    }
                    foreach (string globalFlag in ModSaveData.GlobalFlags)
                    {
                        if (globalFlag.Contains(level.Session.Area.LevelSet))
                        {
                            GlobalFlagsToRemove.Add(globalFlag);
                        }
                    }
                    foreach (string value in FlagsToRemove)
                    {
                        ModSaveData.SavedFlags.Remove(value);
                    }
                    foreach (string value in CutscenesToRemove)
                    {
                        ModSaveData.WatchedCutscenes.Remove(value);
                    }
                    foreach (string value in GlobalFlagsToRemove)
                    {
                        ModSaveData.GlobalFlags.Remove(value);
                    }
                    LevelEnter.Go(new Session(new AreaKey(SaveData.Instance.GetLevelSetStats().AreaOffset + (MergeChaptersControllerKeepPrologue ? 1 : 0), AreaMode.Normal)), fromSaveData: false);
                });
                foreach (LevelEndingHook component in level.Tracker.GetComponents<LevelEndingHook>())
                {
                    if (component.OnEnd != null)
                    {
                        component.OnEnd();
                    }
                }
            }));
            menu.Add(new TextMenu.Button(Dialog.Clean("menu_return_cancel")).Pressed(delegate
            {
                menu.OnCancel();
            }));
            menu.OnPause = (menu.OnESC = delegate
            {
                menu.RemoveSelf();
                returnHint.RemoveSelf();
                level.Paused = false;
                Engine.FreezeTimer = 0.15f;
                Audio.Play("event:/ui/game/unpause");
            });
            menu.OnCancel = delegate
            {
                returnHint.RemoveSelf();
                Audio.Play("event:/ui/main/button_back");
                menu.RemoveSelf();
                level.Pause(returnIndex, minimal: false);
            };
            level.Add(menu);
        }

        private IEnumerator modLevelEnterRoutine(On.Celeste.LevelEnter.orig_Routine orig, LevelEnter self)
        {
            if (startedAnySoCMChapter)
            {
                if (hasOldExtendedVariants && !displayedOldExtVariantsPostcard)
                {
                    self.Add(oldExtVariantsPostcard = new Postcard(Dialog.Get("postcard_Xaphan_OldExtVariants")));
                    yield return oldExtVariantsPostcard.DisplayRoutine();
                    displayedOldExtVariantsPostcard = true;
                    oldExtVariantsPostcard = null;
                }
            }
            IEnumerator origEnum = orig(self);
            while (origEnum.MoveNext()) yield return origEnum.Current;
        }

        private void modLevelEnterBeforeRender(On.Celeste.LevelEnter.orig_BeforeRender orig, LevelEnter self)
        {
            orig(self);
            if (oldExtVariantsPostcard != null) oldExtVariantsPostcard.BeforeRender();
        }

        private void onLevelEnterGo(On.Celeste.LevelEnter.orig_Go orig, Session session, bool fromSaveData)
        {
            if (!fromSaveData && session.StartedFromBeginning && session.Area.Mode == AreaMode.Normal && session.Area.GetSID() == "Xaphan/0/0-Prologue" && !ModSaveData.SavedFlags.Contains("Xaphan/0_Skip_Vignette"))
            {
                ModSaveData.SavedFlags.Add("Xaphan/0_Skip_Vignette");
                Engine.Scene = new SoCMIntroVignette(session);
            }
            else
            {
                orig.Invoke(session, fromSaveData);
            }
        }

        private void modPlayerCallDashEvents(On.Celeste.Player.orig_CallDashEvents orig, Player self)
        {
            if (GravityJacket.determineIfInWater())
            {
                Audio.Play("event:/char/madeline/water_dash_gen");
            }
            orig(self);
        }

        public static bool minimapEnabled;

        private void onLevelUpdate(On.Celeste.Level.orig_Update orig, Level self)
        {
            orig(self);
            Player player = self.Tracker.GetEntity<Player>();
            AreaKey area = self.Session.Area;
            MapData MapData = AreaData.Areas[area.ID].Mode[(int)area.Mode].MapData;

            if (useMergeChaptersController && MergeChaptersControllerMode == "Classic")
            {
                if (self.Session.Level == MapData.StartLevel().Name)
                {
                    if (!ModSaveData.Checkpoints.Contains(self.Session.Area.GetLevelSet() + "|" + self.Session.Area.ChapterIndex) && area.ID != SaveData.Instance.GetLevelSetStats().AreaOffset)
                    {
                        ModSaveData.Checkpoints.Add(self.Session.Area.GetLevelSet() + "|" + self.Session.Area.ChapterIndex);
                        self.AutoSave();
                    }
                }
            }

            // Redo the MergeChaptersController check to prevent issues if the helper is hot re-loaded
            // This should never occur in normal circonstances

            if (!useMergeChaptersControllerCheck)
            {
                MergeChaptersControllerCheck();
                useMergeChaptersControllerCheck = true;
            }

            // Add Metroid UI if using Metroid gameplay

            if (useMetroidGameplay)
            {
                if (self.Tracker.GetEntity<HealthDisplay>() == null)
                {
                    self.Add(new HealthDisplay(new Vector2(22f, 22f)));
                }
                if (self.Tracker.GetEntity<AmmoDisplay>() == null)
                {
                    self.Add(new AmmoDisplay(new Vector2(22f, 31f)));
                }
            }

            // Resume the countdown started from an other chapter when entering a chapter from the start

            if (ModSaveData.CountdownCurrentTime != -1)
            {
                if (self.Tracker.GetEntity<CountdownDisplay>() == null)
                {
                    self.Add(new CountdownDisplay(ModSaveData.CountdownCurrentTime, ModSaveData.CountdownShake, ModSaveData.CountdownExplode, true, ModSaveData.CountdownStartChapter, ModSaveData.CountdownStartRoom, ModSaveData.CountdownSpawn, ModSaveData.CountdownActiveFlag, ModSaveData.CountdownHideFlag, ModSaveData.CountdownEventsFlags)
                    {
                        PauseTimer = true
                    });
                }
            }

            // Add or remove the minimap if the chapter use the in-game Map and conditions are meet

            if (useIngameMap)
            {
                if (self.Tracker.CountEntities<MiniMap>() == 0 && minimapEnabled)
                {
                    minimapEnabled = false;
                }
                if (allRoomsUseTileController && ModSettings.ShowMiniMap && (ModSaveData.SavedFlags.Contains(self.Session.Area.GetLevelSet() + "_Can_Open_Map") || (self.Session.Area.LevelSet == "Xaphan/0" ? ModSaveData.SpeedrunMode : false)) && !minimapEnabled)
                {
                    if (self.Tracker.GetEntity<MiniMap>() == null)
                    {
                        self.Add(new MiniMap(self));
                        minimapEnabled = true;
                    }
                }
                else
                {
                    if (!ModSettings.ShowMiniMap)
                    {
                        MiniMap minimap = self.Tracker.GetEntity<MiniMap>();
                        if (minimap != null)
                        {
                            minimapEnabled = false;
                            minimap.RemoveSelf();
                        }
                    }
                }
            }

            // Add the Achievements Popup UI if current chapter is SoCM

            if (self.Session.Area.LevelSet == "Xaphan/0" && ModSaveData.CanDisplayAchievementsPopups)
            {
                if (self.Tracker.CountEntities<AchievementPopup>() == 0)
                {
                    self.Add(new AchievementPopup());
                }
            }

            // Check if the chapter use the in-game Map or upgrades and allows to open the corresponding screen

            if (useIngameMap || useUpgrades)
            {
                string Prefix = self.Session.Area.GetLevelSet();
                int chapterIndex = self.Session.Area.ChapterIndex == -1 ? 0 : self.Session.Area.ChapterIndex;
                string room = self.Session.Level;
                if (self.CanPause && (self.CanRetry || PlayerIsControllingRemoteDrone()) && player != null && player.StateMachine.State == Player.StNormal && player.Speed == Vector2.Zero && !self.Session.GetFlag("In_bossfight") && player.OnSafeGround && ModSettings.OpenMap.Pressed && !self.Session.GetFlag("Map_Opened"))
                {
                    if (useIngameMap && ModSaveData.VisitedRooms.Contains(Prefix + "/Ch" + chapterIndex + "/" + room) && CanOpenMap(self))
                    {
                        player.StateMachine.State = Player.StDummy;
                        player.DummyAutoAnimate = false;
                        if (!player.Sprite.CurrentAnimationID.Contains("idle") && !player.Sprite.CurrentAnimationID.Contains("edge"))
                        {
                            player.Sprite.Play("idle");
                        }
                        self.Add(new MapScreen(self, false));
                    }
                    else if (useUpgrades && !DisableStatusScreen)
                    {
                        player.StateMachine.State = Player.StDummy;
                        player.DummyAutoAnimate = false;
                        if (!player.Sprite.CurrentAnimationID.Contains("idle") && !player.Sprite.CurrentAnimationID.Contains("edge"))
                        {
                            player.Sprite.Play("idle");
                        }
                        self.Add(new StatusScreen(self, false));
                    }
                }
            }

            // Change starting chapter and room if using a Merge Chapter Controller

            if (useMergeChaptersController && MergeChaptersControllerMode != "Classic" && self.Session.Area.Mode == AreaMode.Normal && (self.Session.Area.LevelSet == "Xaphan/0" ? !ModSaveData.SpeedrunMode : true))
            {
                if ((ModSaveData.LoadedPlayer || self.Session.GetFlag("XaphanHelper_Loaded_Player")) && !CanLoadPlayer) // If for some reason thoses value are true when entering the level, reset them to false. May happen if the game is not exited correctly
                {
                    ModSaveData.LoadedPlayer = false;
                    self.Session.SetFlag("XaphanHelper_Loaded_Player", false);
                }
                CanLoadPlayer = true;
                if (!self.Session.GetFlag("XaphanHelper_Loaded_Player") && !ModSaveData.LoadedPlayer && !self.Paused)
                {
                    bool hasInterlude = false;
                    int maxChapters = SaveData.Instance.GetLevelSetStats().Areas.Count;
                    for (int i = 0; i < maxChapters; i++)
                    {
                        if (AreaData.Areas[(SaveData.Instance.GetLevelSetStats().AreaOffset + i)].Interlude)
                        {
                            hasInterlude = true;
                            break;
                        }
                    }
                    MapData destinationMapData;
                    bool loadAtStartOfCampaign = false;
                    if (ModSaveData.SavedChapter.ContainsKey(self.Session.Area.LevelSet) && ModSaveData.SavedRoom.ContainsKey(self.Session.Area.LevelSet))
                    {
                        int chapter = (ModSaveData.SavedChapter[self.Session.Area.LevelSet] == -1 ? 0 : ModSaveData.SavedChapter[self.Session.Area.LevelSet]) - (hasInterlude ? 0 : 1);
                        destinationMapData = AreaData.Areas[SaveData.Instance.GetLevelSetStats().AreaOffset + chapter].Mode[0].MapData;
                        if (destinationMapData.Get(ModSaveData.SavedRoom[self.Session.Area.LevelSet]) == null)
                        {
                            loadAtStartOfCampaign = true;
                        }
                    }
                    self.Session.SetFlag("XaphanHelper_Loaded_Player", true);
                    self.Session.SetFlag("XaphanHelper_Changed_Start_Room", true);
                    if (!ModSaveData.SavedRoom.ContainsKey(self.Session.Area.LevelSet) || !ModSaveData.SavedChapter.ContainsKey(self.Session.Area.LevelSet)
                        || !ModSaveData.SavedSpawn.ContainsKey(self.Session.Area.LevelSet) || !ModSaveData.SavedLightingAlphaAdd.ContainsKey(self.Session.Area.LevelSet)
                        || !ModSaveData.SavedBloomBaseAdd.ContainsKey(self.Session.Area.LevelSet) || !ModSaveData.SavedCoreMode.ContainsKey(self.Session.Area.LevelSet)
                        || !ModSaveData.SavedMusic.ContainsKey(self.Session.Area.LevelSet) || !ModSaveData.SavedAmbience.ContainsKey(self.Session.Area.LevelSet)
                        || !ModSaveData.SavedNoLoadEntities.ContainsKey(self.Session.Area.LevelSet) || !ModSaveData.SavedTime.ContainsKey(self.Session.Area.LevelSet)
                        || !ModSaveData.SavedFromBeginning.ContainsKey(self.Session.Area.LevelSet) || !ModSaveData.SavedSesionFlags.ContainsKey(self.Session.Area.LevelSet)
                        || !ModSaveData.SavedSessionStrawberries.ContainsKey(self.Session.Area.LevelSet) || (MergeChaptersControllerKeepPrologue && self.Session.Area.ID == SaveData.Instance.GetLevelSetStats().AreaOffset)
                        || loadAtStartOfCampaign)
                    {
                        ModSaveData.LoadedPlayer = true;
                        ModSaveData.CanDisplayAchievementsPopups = true;
                    }
                    else if (ModSaveData.SavedChapter[self.Session.Area.LevelSet] == self.Session.Area.ChapterIndex)
                    {
                        string[] sessionFlags = ModSaveData.SavedSesionFlags[self.Session.Area.LevelSet].Split(',');
                        self.Session.FirstLevel = false;
                        self.Session.LightingAlphaAdd = ModSaveData.SavedLightingAlphaAdd[self.Session.Area.LevelSet];
                        self.Session.BloomBaseAdd = ModSaveData.SavedBloomBaseAdd[self.Session.Area.LevelSet];
                        self.Session.CoreMode = ModSaveData.SavedCoreMode[self.Session.Area.LevelSet];
                        self.Session.DoNotLoad = ModSaveData.SavedNoLoadEntities[self.Session.Area.LevelSet];
                        self.Session.Time = ModSaveData.SavedTime[self.Session.Area.LevelSet];
                        self.Session.StartedFromBeginning = ModSaveData.SavedFromBeginning[self.Session.Area.LevelSet];
                        if (SaveData.Instance != null)
                        {
                            SaveData.Instance.CurrentSession.LightingAlphaAdd = ModSaveData.SavedLightingAlphaAdd[self.Session.Area.LevelSet];
                            SaveData.Instance.CurrentSession.BloomBaseAdd = ModSaveData.SavedBloomBaseAdd[self.Session.Area.LevelSet];
                            SaveData.Instance.CurrentSession.CoreMode = ModSaveData.SavedCoreMode[self.Session.Area.LevelSet];
                        }
                        self.Session.Audio.Music.Event = ModSaveData.SavedMusic[self.Session.Area.LevelSet];
                        self.Session.Audio.Ambience.Event = ModSaveData.SavedAmbience[self.Session.Area.LevelSet];
                        self.Session.Audio.Apply(forceSixteenthNoteHack: false);
                        foreach (string flag in sessionFlags)
                        {
                            if (!flag.Contains("XaphanHelper_StatFlag_") || (flag.Contains("XaphanHelper_StatFlag_") && flag.Contains("-Visited")))
                            {
                                self.Session.SetFlag(flag, true);
                            }
                        }
                        self.Session.Strawberries = ModSaveData.SavedSessionStrawberries[self.Session.Area.LevelSet];
                        ModSaveData.LoadedPlayer = true;
                        self.Add(new TeleportCutscene(player, ModSaveData.SavedRoom[self.Session.Area.LevelSet], MergeChaptersControllerMode == "Warps" ? Vector2.Zero : ModSaveData.SavedSpawn[self.Session.Area.LevelSet], 0, 0, true, 0f, "Fade", skipFirstWipe: true, respawnAnim: true, useLevelWipe: true, spawnPositionX: MergeChaptersControllerMode == "Warps" ? ModSaveData.SavedSpawn[self.Session.Area.LevelSet].X : 0f, spawnPositionY: MergeChaptersControllerMode == "Warps" ? ModSaveData.SavedSpawn[self.Session.Area.LevelSet].Y : 0f));
                    }
                    else
                    {
                        LevelEnter.Go(new Session(new AreaKey(SaveData.Instance.GetLevelSetStats().AreaOffset + (ModSaveData.SavedChapter[self.Session.Area.LevelSet] == -1 ? 0 : ModSaveData.SavedChapter[self.Session.Area.LevelSet] - (hasInterlude ? 0 : 1))))
                        {
                            Time = ModSaveData.SavedTime.ContainsKey(self.Session.Area.LevelSet) ? ModSaveData.SavedTime[self.Session.Area.LevelSet] : 0L
                        }, fromSaveData: false);
                    }
                }

                // Save the room as the one that the player must load into when starting the campaign if using a MergeChaptersController with mode set to Rooms

                else if (useMergeChaptersController && MergeChaptersControllerMode == "Rooms" && !self.Session.GrabbedGolden && !self.Frozen && self.Tracker.GetEntity<CountdownDisplay>() == null && !TriggeredCountDown && self.Tracker.GetEntity<Player>() != null && self.Tracker.GetEntity<Player>().StateMachine.State != Player.StDummy && (self.Session.Area.LevelSet == "Xaphan/0" ? !ModSaveData.SpeedrunMode : true) && !((MergeChaptersControllerKeepPrologue && self.Session.Area.ID == SaveData.Instance.GetLevelSetStats().AreaOffset)))
                {
                    ModSaveData.LoadedPlayer = true;
                    if (!ModSaveData.SavedChapter.ContainsKey(self.Session.Area.LevelSet))
                    {
                        ModSaveData.SavedChapter.Add(self.Session.Area.LevelSet, self.Session.Area.ChapterIndex);
                    }
                    else
                    {
                        if (ModSaveData.SavedChapter[self.Session.Area.LevelSet] != self.Session.Area.ChapterIndex)
                        {
                            ModSaveData.SavedChapter[self.Session.Area.LevelSet] = self.Session.Area.ChapterIndex;
                        }
                    }
                    if (string.IsNullOrEmpty(ModSaveData.CountdownStartRoom))
                    {
                        if (!ModSaveData.SavedRoom.ContainsKey(self.Session.Area.LevelSet))
                        {
                            ModSaveData.SavedRoom.Add(self.Session.Area.LevelSet, self.Session.Level);
                        }
                        else
                        {
                            if (ModSaveData.SavedRoom[self.Session.Area.LevelSet] != self.Session.Level)
                            {
                                ModSaveData.SavedRoom[self.Session.Area.LevelSet] = self.Session.Level;
                            }
                        }
                    }

                    if (self.Session.RespawnPoint == null)
                    {
                        self.Session.RespawnPoint = Vector2.Zero;
                    }
                    if (!ModSaveData.SavedSpawn.ContainsKey(self.Session.Area.LevelSet))
                    {
                        ModSaveData.SavedSpawn.Add(self.Session.Area.LevelSet, (Vector2)self.Session.RespawnPoint - new Vector2(self.Bounds.Left, self.Bounds.Top));
                    }
                    else
                    {
                        if (ModSaveData.SavedSpawn[self.Session.Area.LevelSet] != (Vector2)self.Session.RespawnPoint - new Vector2(self.Bounds.Left, self.Bounds.Top))
                        {
                            ModSaveData.SavedSpawn[self.Session.Area.LevelSet] = (Vector2)self.Session.RespawnPoint - new Vector2(self.Bounds.Left, self.Bounds.Top);
                        }
                    }
                    if (!ModSaveData.SavedLightingAlphaAdd.ContainsKey(self.Session.Area.LevelSet))
                    {
                        ModSaveData.SavedLightingAlphaAdd.Add(self.Session.Area.LevelSet, self.Lighting.Alpha - self.BaseLightingAlpha);
                    }
                    else
                    {
                        if (ModSaveData.SavedLightingAlphaAdd[self.Session.Area.LevelSet] != self.Lighting.Alpha - self.BaseLightingAlpha)
                        {
                            ModSaveData.SavedLightingAlphaAdd[self.Session.Area.LevelSet] = self.Lighting.Alpha - self.BaseLightingAlpha;
                        }
                    }
                    if (!ModSaveData.SavedBloomBaseAdd.ContainsKey(self.Session.Area.LevelSet))
                    {
                        ModSaveData.SavedBloomBaseAdd.Add(self.Session.Area.LevelSet, self.Bloom.Base - AreaData.Get(self).BloomBase);
                    }
                    else
                    {
                        if (ModSaveData.SavedBloomBaseAdd[self.Session.Area.LevelSet] != self.Bloom.Base - AreaData.Get(self).BloomBase)
                        {
                            ModSaveData.SavedBloomBaseAdd[self.Session.Area.LevelSet] = self.Bloom.Base - AreaData.Get(self).BloomBase;
                        }
                    }
                    if (!ModSaveData.SavedCoreMode.ContainsKey(self.Session.Area.LevelSet))
                    {
                        ModSaveData.SavedCoreMode.Add(self.Session.Area.LevelSet, self.Session.CoreMode);
                    }
                    else
                    {
                        if (ModSaveData.SavedCoreMode[self.Session.Area.LevelSet] != self.Session.CoreMode)
                        {
                            ModSaveData.SavedCoreMode[self.Session.Area.LevelSet] = self.Session.CoreMode;
                        }
                    }
                    if (!ModSaveData.SavedMusic.ContainsKey(self.Session.Area.LevelSet))
                    {
                        ModSaveData.SavedMusic.Add(self.Session.Area.LevelSet, self.Session.Audio.Music.Event);
                    }
                    else
                    {
                        if (ModSaveData.SavedMusic[self.Session.Area.LevelSet] != self.Session.Audio.Music.Event)
                        {
                            ModSaveData.SavedMusic[self.Session.Area.LevelSet] = self.Session.Audio.Music.Event;
                        }
                    }
                    if (!ModSaveData.SavedAmbience.ContainsKey(self.Session.Area.LevelSet))
                    {
                        ModSaveData.SavedAmbience.Add(self.Session.Area.LevelSet, self.Session.Audio.Ambience.Event);
                    }
                    else
                    {
                        if (ModSaveData.SavedAmbience[self.Session.Area.LevelSet] != self.Session.Audio.Ambience.Event)
                        {
                            ModSaveData.SavedAmbience[self.Session.Area.LevelSet] = self.Session.Audio.Ambience.Event;
                        }
                    }
                    if (!ModSaveData.SavedNoLoadEntities.ContainsKey(self.Session.Area.LevelSet))
                    {
                        ModSaveData.SavedNoLoadEntities.Add(self.Session.Area.LevelSet, self.Session.DoNotLoad);
                    }
                    else
                    {
                        if (ModSaveData.SavedNoLoadEntities[self.Session.Area.LevelSet] != self.Session.DoNotLoad)
                        {
                            ModSaveData.SavedNoLoadEntities[self.Session.Area.LevelSet] = self.Session.DoNotLoad;
                        }
                    }
                    if (!ModSaveData.SavedFromBeginning.ContainsKey(self.Session.Area.LevelSet))
                    {
                        ModSaveData.SavedFromBeginning.Add(self.Session.Area.LevelSet, self.Session.StartedFromBeginning);
                    }
                    else
                    {
                        if (ModSaveData.SavedFromBeginning[self.Session.Area.LevelSet] != self.Session.StartedFromBeginning)
                        {
                            ModSaveData.SavedFromBeginning[self.Session.Area.LevelSet] = self.Session.StartedFromBeginning;
                        }
                    }
                    string sessionFlags = "";
                    foreach (string flag in self.Session.Flags)
                    {
                        if (sessionFlags == "")
                        {
                            sessionFlags += flag;
                        }
                        else
                        {
                            sessionFlags += "," + flag;
                        }
                    }
                    if (!ModSaveData.SavedSesionFlags.ContainsKey(self.Session.Area.LevelSet))
                    {
                        ModSaveData.SavedSesionFlags.Add(self.Session.Area.LevelSet, sessionFlags);
                    }
                    else
                    {
                        if (ModSaveData.SavedSesionFlags[self.Session.Area.LevelSet] != sessionFlags)
                        {
                            ModSaveData.SavedSesionFlags[self.Session.Area.LevelSet] = sessionFlags;
                        }
                    }
                    if (!ModSaveData.SavedSessionStrawberries.ContainsKey(self.Session.Area.LevelSet))
                    {
                        ModSaveData.SavedSessionStrawberries.Add(self.Session.Area.LevelSet, self.Session.Strawberries);
                    }
                    else
                    {
                        if (ModSaveData.SavedSessionStrawberries[self.Session.Area.LevelSet] != self.Session.Strawberries)
                        {
                            ModSaveData.SavedSessionStrawberries[self.Session.Area.LevelSet] = self.Session.Strawberries;
                        }
                    }
                }

                if (XaphanModule.useMergeChaptersController && (self.Session.Area.LevelSet == "Xaphan/0" ? !ModSaveData.SpeedrunMode : true) && !((MergeChaptersControllerKeepPrologue && self.Session.Area.ID == SaveData.Instance.GetLevelSetStats().AreaOffset)))
                {
                    if (!ModSaveData.SavedTime.ContainsKey(self.Session.Area.LevelSet))
                    {
                        ModSaveData.SavedTime.Add(self.Session.Area.LevelSet, self.Session.Time);
                    }
                    else
                    {
                        if (ModSaveData.SavedTime[self.Session.Area.LevelSet] != self.Session.Time)
                        {
                            ModSaveData.SavedTime[self.Session.Area.LevelSet] = self.Session.Time;
                        }
                    }
                }
            }

            // Change chapter starting room if player used a warp or elevator

            if (!self.Session.GetFlag("XaphanHelper_Changed_Start_Room"))
            {
                self.Session.SetFlag("XaphanHelper_Changed_Start_Room", true);

                // Change level Wipe

                if (!string.IsNullOrEmpty(ModSaveData.Wipe))
                {
                    if (self.Wipe != null && self.Tracker.GetEntities<WarpScreen>().Count == 0)
                    {
                        self.Wipe.Cancel();
                        self.Add(GetWipe(self, true));
                    }
                }
                if (ModSettings.SpeedrunMode) // Clear Speedrun Mode stuff
                {
                    ModSaveData.SpeedrunModeUnlockedWarps.Clear();
                    ModSaveData.SpeedrunModeStaminaUpgrades.Clear();
                    ModSaveData.SpeedrunModeDroneMissilesUpgrades.Clear();
                    ModSaveData.SpeedrunModeDroneSuperMissilesUpgrades.Clear();
                    ModSaveData.SpeedrunModeDroneFireRateUpgrades.Clear();
                }
                if (string.IsNullOrEmpty(ModSaveData.DestinationRoom))
                {
                    if (self.Session.StartedFromBeginning)
                    {
                        string currentRoom = self.Session.Level;
                        ScreenWipe Wipe = null;
                        foreach (LevelData level in MapData.Levels)
                        {
                            if (level.Name == currentRoom)
                            {
                                foreach (EntityData entity in level.Entities)
                                {
                                    if (entity.Name == "XaphanHelper/Elevator")
                                    {
                                        Wipe = new FadeWipe(self, true)
                                        {
                                            Duration = 1.35f
                                        };
                                        break;
                                    }
                                }
                                break;
                            }
                        }
                        if (Wipe != null)
                        {
                            self.Add(Wipe);
                        }
                        else
                        {
                            if (PlayerLostGolden)
                            {
                                self.DoScreenWipe(true);
                                PlayerLostGolden = false;
                            }
                        }
                    }
                }
                else
                {
                    self.Add(GetWipe(self, true));
                    if (!ModSaveData.ConsiderBeginning && !useMergeChaptersController)
                    {
                        self.Session.StartedFromBeginning = false;
                    }
                    ModSaveData.ConsiderBeginning = false;
                    string destinationRoom = ModSaveData.DestinationRoom;
                    Vector2 spawn = ModSaveData.Spawn;
                    string wipe = ModSaveData.Wipe;
                    float wipeDuration = ModSaveData.WipeDuration;
                    bool fromElevator = ModSaveData.TeleportFromElevator;
                    ModSaveData.DestinationRoom = "";
                    ModSaveData.Spawn = new Vector2();
                    ModSaveData.Wipe = "";
                    ModSaveData.WipeDuration = 0f;
                    ModSaveData.TeleportFromElevator = false;
                    if ((self = (Engine.Scene as Level)) != null)
                    {
                        if (string.IsNullOrEmpty(destinationRoom))
                        {
                            self.Add(new MiniTextbox("XaphanHelper_room_name_empty"));
                            return;
                        }
                        if (self.Session.MapData.Get(destinationRoom) == null)
                        {
                            self.Add(new MiniTextbox("XaphanHelper_room_not_exist"));
                            return;
                        }
                    }
                    if (TeleportBackFromDrone)
                    {
                        self.Add(new TransitionBlackEffect(true));
                        self.TimerHidden = true;
                        bool faceLeft = false;
                        if (ModSaveData.fakePlayerFacing[self.Session.Area.GetLevelSet()] == Facings.Left)
                        {
                            faceLeft = true;
                        }
                        self.Add(new TeleportCutscene(player, ModSaveData.droneStartRoom[self.Session.Area.GetLevelSet()], Vector2.Zero, 0, 0, true, 0f, "Fade", wipeDuration, fromElevator, wakeUpAnim: true, spawnPositionX: ModSaveData.fakePlayerPosition[self.Session.Area.GetLevelSet()].X, spawnPositionY: ModSaveData.fakePlayerPosition[self.Session.Area.GetLevelSet()].Y, faceLeft: faceLeft, drone : true));
                    }
                    else
                    {
                        self.Add(new TeleportCutscene(player, destinationRoom, spawn, 0, 0, true, 0f, wipe, wipeDuration, fromElevator, true, useLevelWipe: ModSaveData.CountdownUseLevelWipe));
                    }
                    ModSaveData.CountdownUseLevelWipe = false;
                }
            }

            // Check tiles player has visited

            if (useIngameMap)
            {
                string Prefix = self.Session.Area.GetLevelSet();
                int chapterIndex = self.Session.Area.ChapterIndex == -1 ? 0 : self.Session.Area.ChapterIndex;
                if (player != null && !self.Paused && !self.Transitioning)
                {
                    Vector2 playerPosition = new(Math.Min((float)Math.Floor((player.Center.X - self.Bounds.X) / 320f), (float)Math.Round(self.Bounds.Width / 320f, MidpointRounding.AwayFromZero) - 1), Math.Min((float)Math.Floor((player.Center.Y - self.Bounds.Y) / 184f), (float)Math.Round(self.Bounds.Height / 184f, MidpointRounding.AwayFromZero) + 1));
                    if (playerPosition.X < 0)
                    {
                        playerPosition.X = 0;
                    }
                    if (playerPosition.Y < 0)
                    {
                        playerPosition.Y = 0;
                    }
                    if (!ModSaveData.VisitedRoomsTiles.Contains(Prefix + "/Ch" + chapterIndex + "/" + self.Session.Level + "-" + playerPosition.X + "-" + playerPosition.Y))
                    {
                        ModSaveData.VisitedRoomsTiles.Add(Prefix + "/Ch" + chapterIndex + "/" + self.Session.Level + "-" + playerPosition.X + "-" + playerPosition.Y);
                        InGameMapRoomController roomController = self.Tracker.GetEntity<InGameMapRoomController>();
                        List<Entity> tilesControllers = self.Tracker.GetEntities<InGameMapTilesController>();
                        if (CheckIfTileIsValid(self.Session.Level, playerPosition, tilesControllers))
                        {
                            if (StatsFlags.CurrentTiles != null)
                            {
                                StatsFlags.CurrentTiles[chapterIndex] += 1;
                            }
                            if (StatsFlags.CurrentSubAreaTiles != null)
                            {
                                if (StatsFlags.CurrentSubAreaTiles != null && roomController != null)
                                {
                                    int subAreaIndex = roomController.Data.Int("subAreaIndex");
                                    if (StatsFlags.CurrentSubAreaTiles[chapterIndex].ContainsKey(subAreaIndex))
                                    {
                                        StatsFlags.CurrentSubAreaTiles[chapterIndex][subAreaIndex]++;
                                    }
                                    else
                                    {
                                        StatsFlags.CurrentSubAreaTiles[chapterIndex].Add(subAreaIndex, 1);
                                    }
                                }
                            }
                        }
                        if (ModSettings.ShowMiniMap)
                        {
                            MapDisplay mapDisplay = self.Tracker.GetEntity<MapDisplay>();
                            if (mapDisplay != null)
                            {
                                mapDisplay.GenerateTiles();
                                mapDisplay.GenerateIcons();
                            }
                        }
                    }
                }
            }

            // Detect if player is on top of a slope

            foreach (PlayerPlatform playerPlatform in self.Tracker.GetEntities<PlayerPlatform>())
            {
                if (player != null && playerPlatform.HasPlayerOnTop())
                {
                    onSlope = true;
                    onSlopeDir = playerPlatform.Side == "Right" ? 1 : -1;
                    onSlopeGentle = playerPlatform.Gentle;
                    onSlopeTop = playerPlatform.slopeTop;
                    onSlopeAffectPlayerSpeed = playerPlatform.AffectPlayerSpeed;
                    break;
                }
                else
                {
                    onSlope = false;
                    onSlopeDir = 0;
                    onSlopeGentle = false;
                    onSlopeTop = 0;
                    onSlopeAffectPlayerSpeed = false;
                }
            }

            // SoCM Only

            if (self.Session.Area.GetLevelSet() == "Xaphan/0")
            {
                // Backward saves compatibility

                SaveUpdater.RemoveUpgrades();
            }
        }

        private bool CheckIfTileIsValid(string room, Vector2 playerPosition, List<Entity> TilesControllers)
        {
            bool isValid = false;
            List<InGameMapTilesControllerData> TilesControllerData = new();
            foreach (Entity tileController in TilesControllers)
            {
                InGameMapTilesController controller = tileController as InGameMapTilesController;
                TilesControllerData.Add(new InGameMapTilesControllerData(0, room, controller.Data.Attr("tile0Cords"), controller.Data.Attr("tile0"), controller.Data.Attr("tile1Cords"), controller.Data.Attr("tile1"), controller.Data.Attr("tile2Cords"), controller.Data.Attr("tile2"),
                                            controller.Data.Attr("tile3Cords"), controller.Data.Attr("tile3"), controller.Data.Attr("tile4Cords"), controller.Data.Attr("tile4"), controller.Data.Attr("tile5Cords"), controller.Data.Attr("tile5"), controller.Data.Attr("tile6Cords"), controller.Data.Attr("tile6"),
                                            controller.Data.Attr("tile7Cords"), controller.Data.Attr("tile7"), controller.Data.Attr("tile8Cords"), controller.Data.Attr("tile8"), controller.Data.Attr("tile9Cords"), controller.Data.Attr("tile9"), controller.Data.Attr("display")));
            }
            foreach (InGameMapTilesControllerData tileControllerData in TilesControllerData)
            {
                for (int i = 0; i <= 9; i++)
                {
                    string tileCords = tileControllerData.GetTileCords(i);
                    if (tileCords == (playerPosition.X + "-" + playerPosition.Y))
                    {
                        string tile = tileControllerData.GetTile(i);
                        if (tile != "None" && tile != "ElevatorShaft" && !tile.Contains("Arrow") && !tile.Contains("Connection"))
                        {
                            isValid = true;
                            break;
                        }
                    }
                }
            }
            return isValid;
        }

        private void modStrawberryOnPlayer(On.Celeste.Strawberry.orig_OnPlayer orig, Strawberry self, Player player)
        {
            if (!PlayerHasGolden)
            {
                Level level = player.SceneAs<Level>();
                if (self.Golden)
                {
                    foreach (Strawberry item in level.Entities.FindAll<Strawberry>())
                    {
                        if (item.Golden && item.Follower.Leader != null)
                        {
                            PlayerHasGolden = true;
                            break;
                        }
                    }
                }
                if (useUpgrades)
                {
                    if (PlayerHasGolden) // When the player grab a golden berry
                    {
                        // Reset upgrades

                        level.Session.SetFlag("Upgrade_PowerGrip", false);
                        ModSettings.PowerGrip = false;
                        level.Session.SetFlag("Upgrade_ClimbingKit", false);
                        ModSettings.ClimbingKit = false;
                        level.Session.SetFlag("Upgrade_SpiderMagnet", false);
                        ModSettings.SpiderMagnet = false;
                        level.Session.SetFlag("Upgrade_DroneTeleport", false);
                        ModSettings.DroneTeleport = false;
                        level.Session.SetFlag("Upgrade_JumpBoost", false);
                        ModSettings.JumpBoost = false;
                        level.Session.SetFlag("Upgrade_ScrewAttack", false);
                        ModSettings.ScrewAttack = false;
                        level.Session.SetFlag("Upgrade_VariaJacket", false);
                        ModSettings.VariaJacket = false;
                        level.Session.SetFlag("Upgrade_GravityJacket", false);
                        ModSettings.GravityJacket = false;
                        level.Session.SetFlag("Upgrade_Bombs", false);
                        ModSettings.Bombs = false;
                        level.Session.SetFlag("Upgrade_MegaBombs", false);
                        ModSettings.MegaBombs = false;
                        level.Session.SetFlag("Upgrade_RemoteDrone", false);
                        ModSettings.RemoteDrone = false;
                        level.Session.SetFlag("Upgrade_GoldenFeather", false);
                        ModSettings.GoldenFeather = false;
                        level.Session.SetFlag("Upgrade_Binoculars", false);
                        ModSettings.Binoculars = false;
                        level.Session.SetFlag("Upgrade_EtherealDash", false);
                        ModSettings.EtherealDash = false;
                        level.Session.SetFlag("Upgrade_PortableStation", false);
                        ModSettings.PortableStation = false;
                        level.Session.SetFlag("Upgrade_PulseRadar", false);
                        ModSettings.PulseRadar = false;
                        level.Session.SetFlag("Upgrade_DashBoots", false);
                        ModSettings.DashBoots = false;
                        level.Session.SetFlag("Upgrade_SpaceJump", false);
                        ModSettings.SpaceJump = 1;
                        level.Session.SetFlag("Upgrade_HoverJet", false);
                        ModSettings.HoverJet = false;
                        level.Session.SetFlag("Upgrade_LightningDash", false);
                        ModSettings.LightningDash = false;
                        level.Session.SetFlag("Upgrade_LongBeam", false);
                        ModSettings.LongBeam = false;
                        level.Session.SetFlag("Upgrade_IceBeam", false);
                        ModSettings.IceBeam = false;
                        level.Session.SetFlag("Upgrade_WaveBeam", false);
                        ModSettings.WaveBeam = false;
                        level.Session.SetFlag("Upgrade_MissilesModule", false);
                        ModSettings.MissilesModule = false;
                        level.Session.SetFlag("Upgrade_SuperMissilesModule", false);
                        ModSettings.SuperMissilesModule = false;

                        foreach (string flag in ModSaveData.SavedFlags)
                        {
                            string Prefix = level.Session.Area.GetLevelSet();
                            int chapterIndex = level.Session.Area.ChapterIndex == -1 ? 0 : level.Session.Area.ChapterIndex;
                            string[] str = flag.Split('_');
                            if (str[0] == Prefix && str[1] == "Ch" + chapterIndex)
                            {
                                string toRemove = str[0] + "_" + str[1] + "_";
                                string result = string.Empty;
                                int i = flag.IndexOf(toRemove);
                                if (i >= 0)
                                {
                                    result = flag.Remove(i, toRemove.Length);
                                }
                                level.Session.SetFlag(result, false);
                            }
                        }

                        // Give allowed starting upgrades

                        AreaKey area = level.Session.Area;
                        MapData MapData = AreaData.Areas[area.ID].Mode[(int)area.Mode].MapData;
                        bool goldenPowerGrip = false;
                        bool goldenClimbingKit = false;
                        bool goldenSpiderMagnet = false;
                        bool goldenDroneTeleport = false;
                        bool goldenJumpBoost = false;
                        bool goldenScrewAttack = false;
                        bool goldenVariaJacket = false;
                        bool goldenGravityJacket = false;
                        bool goldenBombs = false;
                        bool goldenMegaBombs = false;
                        bool goldenRemoteDrone = false;
                        bool goldenGoldenFeather = false;
                        bool goldenBinoculars = false;
                        bool goldenEtherealDash = false;
                        bool goldenPortableStation = false;
                        bool goldenPulseRadar = false;
                        bool goldenDashBoots = false;
                        bool goldenSpaceJump = false;
                        bool goldenHoverJet = false;
                        bool goldenLightningDash = false;
                        bool goldenLongBeam = false;
                        bool goldenIceBeam = false;
                        bool goldenWaveBeam = false;
                        bool goldenMissilesModule = false;
                        bool goldenSuperMissilesModule = false;
                        foreach (LevelData levelData in MapData.Levels)
                        {
                            foreach (EntityData entity in levelData.Entities)
                            {
                                if (entity.Name == "XaphanHelper/UpgradeController")
                                {
                                    goldenPowerGrip = entity.Bool("goldenStartWithPowerGrip");
                                    goldenClimbingKit = entity.Bool("goldenStartWithClimbingKit");
                                    goldenSpiderMagnet = entity.Bool("goldenStartWithSpiderMagnet");
                                    goldenDroneTeleport = entity.Bool("goldenStartWithDroneTeleport");
                                    goldenJumpBoost = entity.Bool("goldenStartWithJumpBoost");
                                    goldenScrewAttack = entity.Bool("goldenStartWithScrewAttack");
                                    goldenVariaJacket = entity.Bool("goldenStartWithVariaJacket");
                                    goldenGravityJacket = entity.Bool("goldenStartWithGravityJacket");
                                    goldenBombs = entity.Bool("goldenStartWithBombs");
                                    goldenMegaBombs = entity.Bool("goldenStartWithMegaBombs");
                                    goldenRemoteDrone = entity.Bool("goldenStartWithRemoteDrone");
                                    goldenGoldenFeather = entity.Bool("goldenStartWithGoldenFeather");
                                    goldenBinoculars = entity.Bool("goldenStartWithBinoculars");
                                    goldenEtherealDash = entity.Bool("goldenStartWithEtherealDash");
                                    goldenPortableStation = entity.Bool("goldenStartWithPortableStation");
                                    goldenPulseRadar = entity.Bool("goldenStartWithPulseRadar");
                                    goldenDashBoots = entity.Bool("goldenStartWithDashBoots");
                                    goldenSpaceJump = entity.Bool("goldenStartWithSpaceJump");
                                    goldenHoverJet = entity.Bool("goldenStartWithHoverJet");
                                    goldenLightningDash = entity.Bool("goldenStartWithLightningDash");
                                    goldenLongBeam = entity.Bool("goldenStartWithLongBeam");
                                    goldenIceBeam = entity.Bool("goldenStartWithIceBeam");
                                    goldenWaveBeam = entity.Bool("goldenStartWithWaveBeam");
                                    goldenMissilesModule = entity.Bool("goldenStartWithMissilesModule");
                                    goldenSuperMissilesModule = entity.Bool("goldenStartWithSuperMissilesModule");
                                    break;
                                }
                            }
                        }
                        if (goldenPowerGrip)
                        {
                            ModSettings.PowerGrip = true;
                            level.Session.SetFlag("Upgrade_PowerGrip", true);
                        }
                        if (goldenClimbingKit)
                        {
                            ModSettings.ClimbingKit = true;
                            level.Session.SetFlag("Upgrade_ClimbingKit", true);
                        }
                        if (goldenSpiderMagnet)
                        {
                            ModSettings.SpiderMagnet = true;
                            level.Session.SetFlag("Upgrade_SpiderMagnet", true);
                        }
                        if (goldenDroneTeleport)
                        {
                            ModSettings.DroneTeleport = true;
                            level.Session.SetFlag("Upgrade_DroneTeleport", true);
                        }
                        if (goldenJumpBoost)
                        {
                            ModSettings.JumpBoost = true;
                            level.Session.SetFlag("Upgrade_JumpBoost", true);
                        }
                        if (goldenScrewAttack)
                        {
                            ModSettings.ScrewAttack = true;
                            level.Session.SetFlag("Upgrade_ScrewAttack", true);
                        }
                        if (goldenVariaJacket)
                        {
                            ModSettings.VariaJacket = true;
                            level.Session.SetFlag("Upgrade_VariaJacket", true);
                        }
                        if (goldenGravityJacket)
                        {
                            ModSettings.GravityJacket = true;
                            level.Session.SetFlag("Upgrade_GravityJacket", true);
                        }
                        if (goldenBombs)
                        {
                            ModSettings.Bombs = true;
                            level.Session.SetFlag("Upgrade_Bombs", true);
                        }
                        if (goldenMegaBombs)
                        {
                            ModSettings.MegaBombs = true;
                            level.Session.SetFlag("Upgrade_MegaBombs", true);
                        }
                        if (goldenRemoteDrone)
                        {
                            ModSettings.RemoteDrone = true;
                            level.Session.SetFlag("Upgrade_RemoteDrone", true);
                        }
                        if (goldenGoldenFeather)
                        {
                            ModSettings.GoldenFeather = true;
                            level.Session.SetFlag("Upgrade_GoldenFeather", true);
                        }
                        if (goldenBinoculars)
                        {
                            ModSettings.Binoculars = true;
                            level.Session.SetFlag("Upgrade_Binoculars", true);
                        }
                        if (goldenEtherealDash)
                        {
                            ModSettings.EtherealDash = true;
                            level.Session.SetFlag("Upgrade_EtherealDash", true);
                        }
                        if (goldenPortableStation)
                        {
                            ModSettings.PortableStation = true;
                            level.Session.SetFlag("Upgrade_PortableStation", true);
                        }
                        if (goldenPulseRadar)
                        {
                            ModSettings.PulseRadar = true;
                            level.Session.SetFlag("Upgrade_PulseRadar", true);
                        }
                        if (goldenDashBoots)
                        {
                            ModSettings.DashBoots = true;
                            level.Session.SetFlag("Upgrade_DashBoots", true);
                        }
                        if (goldenSpaceJump)
                        {
                            ModSettings.SpaceJump = 2;
                            level.Session.SetFlag("Upgrade_SpaceJump", true);
                        }
                        if (goldenHoverJet)
                        {
                            ModSettings.HoverJet = true;
                            level.Session.SetFlag("Upgrade_HoverJet", true);
                        }
                        if (goldenLightningDash)
                        {
                            ModSettings.LightningDash = true;
                            level.Session.SetFlag("Upgrade_LightningDash", true);
                        }
                        if (goldenLongBeam)
                        {
                            ModSettings.LongBeam = true;
                            level.Session.SetFlag("Upgrade_LongBeam", true);
                        }
                        if (goldenIceBeam)
                        {
                            ModSettings.IceBeam = true;
                            level.Session.SetFlag("Upgrade_IceBeam", true);
                        }
                        if (goldenWaveBeam)
                        {
                            ModSettings.WaveBeam = true;
                            level.Session.SetFlag("Upgrade_WaveBeam", true);
                        }
                        if (goldenMissilesModule)
                        {
                            ModSettings.MissilesModule = true;
                            level.Session.SetFlag("Upgrade_MissilesModule", true);
                        }
                        if (goldenSuperMissilesModule)
                        {
                            ModSettings.SuperMissilesModule = true;
                            level.Session.SetFlag("Upgrade_SuperMissilesModule", true);
                        }
                    }
                }
                ModSaveData.SpeedrunModeUnlockedWarps.Clear();
                ModSaveData.SpeedrunModeStaminaUpgrades.Clear();
                ModSaveData.SpeedrunModeDroneMissilesUpgrades.Clear();
                ModSaveData.SpeedrunModeDroneSuperMissilesUpgrades.Clear();
                ModSaveData.SpeedrunModeDroneFireRateUpgrades.Clear();
            }
            orig(self, player);
        }

        private void modStrawberryUpdate(On.Celeste.Strawberry.orig_Update orig, Strawberry self)
        {
            if (useUpgrades)
            {
                if (self.Golden && PlayerHasGolden && self.Follower.Leader == null)
                {
                    self.RemoveSelf();
                }
                else
                {
                    orig(self);
                }
            }
            else
            {
                orig(self);
            }
        }

        private void modStrawberryOnLoseLeader(On.Celeste.Strawberry.orig_OnLoseLeader orig, Strawberry self)
        {
            if (self.Golden)
            {
                PlayerLostGolden = true;
            }
            if (useUpgrades && self.Golden)
            {
                PlayerHasGolden = false;
                orig(self);
            }
            else
            {
                orig(self);
            }
        }

        private static IEnumerator onStrawberryCollectRoutine(On.Celeste.Strawberry.orig_CollectRoutine orig, Strawberry self, int collectIndex)
        {
            yield return new SwapImmediately(orig(self, collectIndex));
            if (self.Golden)
            {
                PlayerHasGolden = false;
                if (useUpgrades)
                {
                    AreaKey area = self.SceneAs<Level>().Session.Area;
                    MapData MapData = AreaData.Areas[area.ID].Mode[(int)area.Mode].MapData;
                    GiveUpgradesToPlayer(MapData, self.SceneAs<Level>());
                }
            }
        }

        private ScreenWipe GetWipe(Level level, bool wipeIn)
        {
            ScreenWipe Wipe = null;
            switch (ModSaveData.Wipe)
            {
                case "Spotlight":
                    Wipe = new SpotlightWipe(level, wipeIn)
                    {
                        Duration = ModSaveData.WipeDuration
                    };
                    break;
                case "Curtain":
                    Wipe = new CurtainWipe(level, wipeIn)
                    {
                        Duration = ModSaveData.WipeDuration
                    };
                    break;
                case "Mountain":
                    Wipe = new MountainWipe(level, wipeIn)
                    {
                        Duration = ModSaveData.WipeDuration
                    };
                    break;
                case "Dream":
                    Wipe = new DreamWipe(level, wipeIn)
                    {
                        Duration = ModSaveData.WipeDuration
                    };
                    break;
                case "Starfield":
                    Wipe = new StarfieldWipe(level, wipeIn)
                    {
                        Duration = ModSaveData.WipeDuration
                    };
                    break;
                case "Wind":
                    Wipe = new WindWipe(level, wipeIn)
                    {
                        Duration = ModSaveData.WipeDuration
                    };
                    break;
                case "Drop":
                    Wipe = new DropWipe(level, wipeIn)
                    {
                        Duration = ModSaveData.WipeDuration
                    };
                    break;
                case "Fall":
                    Wipe = new FallWipe(level, wipeIn)
                    {
                        Duration = ModSaveData.WipeDuration
                    };
                    break;
                case "KeyDoor":
                    Wipe = new KeyDoorWipe(level, wipeIn)
                    {
                        Duration = ModSaveData.WipeDuration
                    };
                    break;
                case "Angled":
                    Wipe = new AngledWipe(level, wipeIn)
                    {
                        Duration = ModSaveData.WipeDuration
                    };
                    break;
                case "Heart":
                    Wipe = new HeartWipe(level, wipeIn)
                    {
                        Duration = ModSaveData.WipeDuration
                    };
                    break;
                case "Fade":
                    Wipe = new FadeWipe(level, wipeIn)
                    {
                        Duration = ModSaveData.WipeDuration
                    };
                    break;
                case "Level":
                    level.DoScreenWipe(wipeIn);
                    break;
            }
            return Wipe;
        }

        private void onPlayerRender(On.Celeste.Player.orig_Render orig, Player self)
        {
            if (useUpgrades && (VariaJacket.Active(self.SceneAs<Level>()) || GravityJacket.Active(self.SceneAs<Level>())))
            {
                string id = "";
                if (GravityJacket.Active(self.SceneAs<Level>()))
                {
                    id = !useMetroidGameplay ? "gravity" : "samus_gravity";
                }
                else if (VariaJacket.Active(self.SceneAs<Level>()))
                {
                    id = !useMetroidGameplay ? "varia" : "samus_varia";
                }
                Effect fxColorGrading = GFX.FxColorGrading;
                fxColorGrading.CurrentTechnique = fxColorGrading.Techniques["ColorGradeSingle"];
                Engine.Graphics.GraphicsDevice.Textures[1] = GFX.ColorGrades[id].Texture.Texture_Safe;
                Draw.SpriteBatch.End();
                Draw.SpriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointWrap, DepthStencilState.None, RasterizerState.CullNone, fxColorGrading, (self.Scene as Level).GameplayRenderer.Camera.Matrix);
                orig(self);
                Draw.SpriteBatch.End();
                Draw.SpriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointWrap, DepthStencilState.None, RasterizerState.CullNone, null, (self.Scene as Level).GameplayRenderer.Camera.Matrix);
            }
            else
            {
                orig(self);
            }
        }

        private void onPlayerDeadBodyRender(On.Celeste.PlayerDeadBody.orig_Render orig, PlayerDeadBody self)
        {
            if (useUpgrades && (VariaJacket.Active(self.SceneAs<Level>()) || GravityJacket.Active(self.SceneAs<Level>())))
            {
                string id = "";
                if (GravityJacket.Active(self.SceneAs<Level>()))
                {
                    id = !useMetroidGameplay ? "gravity" : "samus_gravity";
                }
                else if (VariaJacket.Active(self.SceneAs<Level>()))
                {
                    id = !useMetroidGameplay ? "varia" : "samus_varia";
                }
                Effect fxColorGrading = GFX.FxColorGrading;
                fxColorGrading.CurrentTechnique = fxColorGrading.Techniques["ColorGradeSingle"];
                Engine.Graphics.GraphicsDevice.Textures[1] = GFX.ColorGrades[id].Texture.Texture_Safe;
                Draw.SpriteBatch.End();
                Draw.SpriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointWrap, DepthStencilState.None, RasterizerState.CullNone, fxColorGrading, (self.Scene as Level).GameplayRenderer.Camera.Matrix);
                orig(self);
                Draw.SpriteBatch.End();
                Draw.SpriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointWrap, DepthStencilState.None, RasterizerState.CullNone, null, (self.Scene as Level).GameplayRenderer.Camera.Matrix);
            }
            else
            {
                orig(self);
            }
        }

        private Backdrop OnLoadBackdrop(MapData map, BinaryPacker.Element child, BinaryPacker.Element super)
        {
            if (child.Name.Equals("XaphanHelper/HeatParticles", StringComparison.OrdinalIgnoreCase))
            {
                return new HeatParticles(child.Attr("particlesColors"), child.AttrInt("particlesAmount"), child.AttrBool("noMist"));
            }
            if (child.Name.Equals("XaphanHelper/Glow", StringComparison.OrdinalIgnoreCase))
            {
                return new Glow(child.Attr("color"));
            }
            return null;
        }


        public void PlayerSpriteMetadataHook(On.Celeste.PlayerSprite.orig_CreateFramesMetadata orig, string self) {

            string extend_object = "XaphanHelper_Extend__" + self;
            if (!GFX.SpriteBank.Has(extend_object)) {
                extend_object = "XaphanHelper_Extend__player";
            }

            SpriteData extendPlayer = GFX.SpriteBank.SpriteData[extend_object];
            SpriteData objects = GFX.SpriteBank.SpriteData[self];

            PatchSprite(extendPlayer.Sprite, objects.Sprite);
            //Logger.Log(LogLevel.Warn, "XaphanHelper", $"{extendPlayer.Sprite.GetFrame("XaphanHelper_ceilingIdle", 0)}");

            if (extend_object == "XaphanHelper_Extend__" + self) {
                orig(extend_object);
            }
            orig(self);
        }
        private void PatchSprite(Sprite origSprite, Sprite newSprite) {
            Dictionary<string, Sprite.Animation> newAnims = newSprite.GetAnimations();

            // Shallow copy... sometimes new animations get added mid-update?
            Dictionary<string, Sprite.Animation> oldAnims = new(origSprite.GetAnimations());
            foreach (KeyValuePair<string, Sprite.Animation> animEntry in oldAnims) {
                string origAnimId = animEntry.Key;

                Sprite.Animation origAnim = animEntry.Value;
                if (!newAnims.ContainsKey(origAnimId)) {
                    newAnims[origAnimId] = origAnim;
                }
            }
        }

    }
}