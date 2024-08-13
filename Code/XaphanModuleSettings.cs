using Microsoft.Xna.Framework.Input;

namespace Celeste.Mod.XaphanHelper
{
    [SettingName("XaphanModuleSettings")]
    public class XaphanModuleSettings : EverestModuleSettings
    {
        // Mods Options Settings

        public enum JumpIndicatorSize { None, Small, Large };

        [SettingName("ModOptions_XaphanModule_ShowMiniMap")]
        [SettingSubText("ModOptions_XaphanModule_ShowMiniMap_Desc")]
        public bool ShowMiniMap { get; set; } = true;

        public int MiniMapOpacity { get; set; } = 10;

        [SettingName("ModOptions_XaphanModule_ShowHeatLevel")]
        [SettingSubText("ModOptions_XaphanModule_ShowHeatLevel_Desc")]
        public bool ShowHeatLevel { get; set; } = true;

        [SettingName("ModOptions_XaphanModule_SpaceJumpIndicator")]
        public JumpIndicatorSize SpaceJumpIndicator { get; set; } = JumpIndicatorSize.Large;

        [SettingName("ModOptions_XaphanModule_ShowCompleteSlopesHitboxes")]
        [SettingSubText("ModOptions_XaphanModule_ShowCompleteSlopesHitboxes_Desc")]
        public static bool ShowCompleteSlopesHitboxes { get; set; } = false;

        public void CreateMiniMapOpacityEntry(TextMenu menu, bool inGame)
        {
            menu.Add(new TextMenu.Slider(Dialog.Clean("ModOptions_XaphanModule_MiniMapOpacity"), delegate (int i)
            {
                switch (i)
                {
                    default:
                        return Dialog.Clean("ModOptions_XaphanModule_100");
                    case 9:
                        return Dialog.Clean("ModOptions_XaphanModule_90");
                    case 8:
                        return Dialog.Clean("ModOptions_XaphanModule_80");
                    case 7:
                        return Dialog.Clean("ModOptions_XaphanModule_70");
                    case 6:
                        return Dialog.Clean("ModOptions_XaphanModule_60");
                    case 5:
                        return Dialog.Clean("ModOptions_XaphanModule_50");
                    case 4:
                        return Dialog.Clean("ModOptions_XaphanModule_40");
                    case 3:
                        return Dialog.Clean("ModOptions_XaphanModule_30");
                    case 2:
                        return Dialog.Clean("ModOptions_XaphanModule_20");
                    case 1:
                        return Dialog.Clean("ModOptions_XaphanModule_10");
                }
            }, 1, 10, MiniMapOpacity).Change(delegate (int i)
            {
                MiniMapOpacity = i;
            }));
        }

        // Bindings

        [DefaultButtonBinding(Buttons.Back, Keys.Tab)]
        public ButtonBinding OpenMap { get; set; }

        [DefaultButtonBinding(Buttons.Y, Keys.A)]
        public ButtonBinding SelectItem { get; set; }

        [DefaultButtonBinding(Buttons.LeftShoulder, Keys.S)]
        public ButtonBinding UseBagItemSlot { get; set; }

        [DefaultButtonBinding(Buttons.RightShoulder, Keys.D)]
        public ButtonBinding UseMiscItemSlot { get; set; }

        [DefaultButtonBinding(Buttons.LeftTrigger, Keys.Z)]
        public ButtonBinding MapScreenShowProgressDisplay { get; set; }

        [DefaultButtonBinding(Buttons.Y, Keys.A)]
        public ButtonBinding MapScreenShowMapOrWorldMap { get; set; }

        [DefaultButtonBinding(Buttons.A, Keys.C)]
        public ButtonBinding MapScreenShowHints { get; set; }

        // Celeste Upgrades

        [SettingIgnore]
        public bool PowerGrip { get; set; } = false;

        [SettingIgnore]
        public bool ClimbingKit { get; set; } = false;

        [SettingIgnore]
        public bool SpiderMagnet { get; set; } = false;

        [SettingIgnore]
        public bool DroneTeleport { get; set; } = false;

        [SettingIgnore]
        public bool JumpBoost { get; set; } = false;

        [SettingIgnore]
        public bool Bombs { get; set; } = false;

        [SettingIgnore]
        public bool MegaBombs { get; set; } = false;

        [SettingIgnore]
        public bool RemoteDrone { get; set; } = false;

        [SettingIgnore]
        public bool GoldenFeather { get; set; } = false;

        [SettingIgnore]
        public bool Binoculars { get; set; } = false;

        [SettingIgnore]
        public bool EtherealDash { get; set; } = false;

        [SettingIgnore]
        public bool PortableStation { get; set; } = false;

        [SettingIgnore]
        public bool PulseRadar { get; set; } = false;

        [SettingIgnore]
        public bool DashBoots { get; set; } = false;

        [SettingIgnore]
        public bool HoverJet { get; set; } = false;

        [SettingIgnore]
        public bool LightningDash { get; set; } = false;

        [SettingIgnore]
        public bool MissilesModule { get; set; } = false;

        [SettingIgnore]
        public bool SuperMissilesModule { get; set; } = false;

        // Metroid Upgrades

        [SettingIgnore]
        public bool Spazer { get; set; } = false;

        [SettingIgnore]
        public bool PlasmaBeam { get; set; } = false;

        [SettingIgnore]
        public bool MorphingBall { get; set; } = false;

        [SettingIgnore]
        public bool MorphBombs { get; set; } = false;

        [SettingIgnore]
        public bool SpringBall { get; set; } = false;

        [SettingIgnore]
        public bool HighJumpBoots { get; set; } = false;

        [SettingIgnore]
        public bool SpeedBooster { get; set; } = false;

        // Common Upgrades

        [SettingIgnore]
        public bool LongBeam { get; set; } = false;

        [SettingIgnore]
        public bool IceBeam { get; set; } = false;

        [SettingIgnore]
        public bool WaveBeam { get; set; } = false;

        [SettingIgnore]
        public bool VariaJacket { get; set; } = false;

        [SettingIgnore]
        public bool GravityJacket { get; set; } = false;

        [SettingIgnore]
        public bool ScrewAttack { get; set; } = false;

        [SettingIgnore]
        public int SpaceJump { get; set; } = 1;

        // SoCM only settings

        [SettingIgnore]
        public bool ShowAchievementsPopups { get; set; } = true;

        [SettingIgnore]
        public bool ShowLorebookPopups { get; set; } = true;

        [SettingIgnore]
        public bool AutoSkipCutscenes { get; set; } = false;
    }
}
