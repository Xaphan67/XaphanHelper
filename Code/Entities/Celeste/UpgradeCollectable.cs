using Celeste.Mod.Entities;
using Celeste.Mod.XaphanHelper.UI_Elements;
using Microsoft.Xna.Framework;
using Monocle;
using System.Collections;

namespace Celeste.Mod.XaphanHelper.Entities
{
    [CustomEntity("XaphanHelper/UpgradeCollectable")]
    class UpgradeCollectable : Entity
    {
        public EntityID ID;

        private class BgFlash : Entity
        {
            private float alpha = 1f;

            public BgFlash()
            {
                Depth = 10100;
                Tag = Tags.Persistent;
            }

            public override void Update()
            {
                base.Update();
                alpha = Calc.Approach(alpha, 0f, Engine.DeltaTime * 0.5f);
                if (alpha <= 0f)
                {
                    RemoveSelf();
                }
            }

            public override void Render()
            {
                Vector2 position = (Scene as Level).Camera.Position;
                Draw.Rect(position.X - 10f, position.Y - 10f, 340f, 200f, Color.Black * alpha);
            }
        }

        private string sprite;

        private string collectSound;

        private string oldMusic;

        private string newMusic;

        public string upgrade;

        public Sprite collectable;

        private Wiggler scaleWiggler;

        private Wiggler moveWiggler;

        private string upgradeName;

        private string upgradeDescription;

        private string upgradeControls;

        private string nameColor;

        private string descColor;

        private string particleColor;

        private VirtualButton buttonA;

        private VirtualButton buttonB;

        private UpgradeScreen upgradeScreen;

        private SoundEmitter sfx;

        private XaphanModule.Upgrades upg;

        private string Prefix;

        private string customName;

        private string customSprite;

        private int mapShardIndex;

        private bool FlagRegiseredInSaveData()
        {
            Session session = SceneAs<Level>().Session;
            string Prefix = session.Area.GetLevelSet();
            int chapterIndex = session.Area.ChapterIndex == -1 ? 0 : session.Area.ChapterIndex;
            if (!Settings.SpeedrunMode)
            {
                if (upgrade == "MapShard")
                {
                    if (mapShardIndex == 0)
                    {
                        return XaphanModule.ModSaveData.SavedFlags.Contains(Prefix + "_Ch" + chapterIndex + "_MapShard");
                    }
                    else
                    {
                        return XaphanModule.ModSaveData.SavedFlags.Contains(Prefix + "_Ch" + chapterIndex + "_MapShard_" + mapShardIndex);
                    }
                }
                if (upgrade == "Map")
                {
                    return XaphanModule.ModSaveData.SavedFlags.Contains(Prefix + "_Can_Open_Map");
                }
                return XaphanModule.ModSaveData.SavedFlags.Contains(Prefix + "_Upgrade_" + upgrade);
            }
            else
            {
                return session.GetFlag(upgrade);
            }
        }

        protected XaphanModuleSettings Settings => XaphanModule.Settings;

        public UpgradeCollectable(EntityData data, Vector2 position, EntityID id) : base(data.Position + position)
        {
            ID = id;
            collectSound = data.Attr("collectSound");
            newMusic = data.Attr("newMusic");
            upgrade = data.Attr("upgrade");
            upg = data.Enum("upgrade", XaphanModule.Upgrades.DashBoots);
            nameColor = data.Attr("nameColor");
            descColor = data.Attr("descColor");
            particleColor = data.Attr("particleColor");
            customName = data.Attr("customName");
            customSprite = data.Attr("customSprite");
            mapShardIndex = data.Int("mapShardIndex", 0);
            sprite = (string.IsNullOrEmpty(customSprite) ? "collectables/XaphanHelper/UpgradeCollectable/" : customSprite + "/") + (upgrade == "MapShard" ? "map" : upgrade.ToLower());
            Collider = new Hitbox(12f, 12f, 2f, 2f);
            Add(collectable = new Sprite(GFX.Game, sprite));
            collectable.AddLoop("idle", "", 0.08f);
            collectable.AddLoop("static", "", 1f, 0);
            collectable.Play("idle");
            collectable.CenterOrigin();
            collectable.Position = collectable.Position + new Vector2(8, 8);
            Add(scaleWiggler = Wiggler.Create(0.5f, 4f, delegate (float f)
            {
                collectable.Scale = Vector2.One * (1f + f * 0.3f);
            }));
            moveWiggler = Wiggler.Create(0.8f, 2f);
            moveWiggler.StartZero = true;
            Add(moveWiggler);
            Add(new PlayerCollider(OnPlayer));
        }

        private void OnPlayer(Player player)
        {
            Level level = Scene as Level;
            Add(new Coroutine(Collect(player, level)));
        }

        public override void Added(Scene scene)
        {
            base.Added(scene);
            Prefix = SceneAs<Level>().Session.Area.GetLevelSet();
            bool haveGolden = SceneAs<Level>().Session.GrabbedGolden;
            if (!haveGolden || (haveGolden && (upgrade == "MapShard" || upgrade == "Map")))
            {
                int chapterIndex = SceneAs<Level>().Session.Area.ChapterIndex;
                if (!Settings.SpeedrunMode && FlagRegiseredInSaveData() || SceneAs<Level>().Session.GetFlag("Upgrade_" + upgrade) || (upgrade == "EnergyTank" && XaphanModule.ModSaveData.StaminaUpgrades.Contains(Prefix + "_Ch" + chapterIndex + "_" + ID)) || (upgrade == "FireRateModule" && XaphanModule.ModSaveData.DroneFireRateUpgrades.Contains(Prefix + "_Ch" + chapterIndex + "_" + ID)))
                {
                    RemoveSelf();
                }
            }
        }

        private IEnumerator Collect(Player player, Level level)
        {
            Visible = false;
            Collidable = false;
            Session session = SceneAs<Level>().Session;
            oldMusic = Audio.CurrentMusic;
            session.Audio.Music.Event = SFX.EventnameByHandle(collectSound);
            session.Audio.Apply(forceSixteenthNoteHack: false);
            if (upgrade != "EnergyTank" && upgrade != "FireRateModule")
            {
                session.DoNotLoad.Add(ID);
            }
            sfx = SoundEmitter.Play(collectSound, this);
            AreaKey area = level.Session.Area;
            for (int i = 0; i < 10; i++)
            {
                Scene.Add(new AbsorbOrb(Position));
            }
            level.Shake();
            Input.Rumble(RumbleStrength.Strong, RumbleLength.Medium);
            level.Flash(Color.White);
            level.FormationBackdrop.Display = true;
            level.FormationBackdrop.Alpha = 1f;
            Visible = false;
            if (player.Dead)
            {
                yield return 100f;
            }
            Engine.TimeRate = 1f;
            Tag = Tags.FrozenUpdate;
            level.Frozen = true;

            string upgradeKey = $"XaphanHelper_get_{upgrade}";
            bool droneUpgrade = !XaphanModule.useMetroidGameplay && (upgrade == "LongBeam" || upgrade == "IceBeam" || upgrade == "WaveBeam");
            string droneString = droneUpgrade ? "_drone" : "";

            upgradeName = string.IsNullOrEmpty(customName) ? $"{upgradeKey}_Name".DialogCleanOrNull() : customName.DialogCleanOrNull();
            upgradeDescription = $"{upgradeKey}_Desc{droneString}".DialogCleanOrNull();
            upgradeControls = $"{upgradeKey}_Controls".DialogCleanOrNull();

            if (string.IsNullOrEmpty(particleColor))
            {
                particleColor = "FFFFFF";
            }
            switch (upgrade)
            {
                case "Map":
                    buttonA = Settings.OpenMap.Button;
                    break;
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
            upgradeScreen = new UpgradeScreen(sprite, upgradeName, upgradeDescription, upgradeControls, nameColor, descColor, descColor, particleColor, buttonA, buttonB);
            upgradeScreen.Alpha = 0f;
            Scene.Add(upgradeScreen);
            for (float t2 = 0f; t2 < 1f; t2 += Engine.RawDeltaTime)
            {
                upgradeScreen.Alpha = Ease.CubeOut(t2);
                yield return null;
            }
            while (!Input.MenuConfirm.Pressed && !Input.MenuCancel.Pressed)
            {
                yield return null;
            }
            sfx.Source.Param("end", 1f);
            if (upgrade != "Map" && upgrade != "MapShard" && upgrade != "EnergyTank" && upgrade != "FireRateModule")
            {   
                setUpgrade(upg);
            }
            if (upgrade == "EnergyTank")
            {
                int chapterIndex = area.ChapterIndex;
                XaphanModule.ModSaveData.StaminaUpgrades.Add(Prefix + "_Ch" + chapterIndex + "_" + ID);
                if (XaphanModule.PlayerHasGolden || XaphanModule.Settings.SpeedrunMode)
                {
                    XaphanModule.ModSaveData.SpeedrunModeStaminaUpgrades.Add(Prefix + "_Ch" + chapterIndex + "_" + ID);
                }
            }
            else if (upgrade == "FireRateModule")
            {
                int chapterIndex = area.ChapterIndex;
                XaphanModule.ModSaveData.DroneFireRateUpgrades.Add(Prefix + "_Ch" + chapterIndex + "_" + ID);
                if (XaphanModule.PlayerHasGolden || XaphanModule.Settings.SpeedrunMode)
                {
                    XaphanModule.ModSaveData.SpeedrunModeDroneFireRateUpgrades.Add(Prefix + "_Ch" + chapterIndex + "_" + ID);
                }
            }
            else
            {
                RegisterFlag();
            }
            level.FormationBackdrop.Display = false;
            for (float t = 0f; t < 1f; t += Engine.RawDeltaTime * 2f)
            {
                upgradeScreen.Alpha = Ease.CubeIn(1f - t);
                yield return null;
            }
            player.Depth = 0;
            if (!string.IsNullOrEmpty(newMusic))
            {
                session.Audio.Music.Event = SFX.EventnameByHandle(newMusic);
            }
            else
            {
                session.Audio.Music.Event = SFX.EventnameByHandle(oldMusic);
            }
            session.Audio.Apply(forceSixteenthNoteHack: false);
            EndCutscene();
        }

        private void RegisterFlag()
        {
            Session session = SceneAs<Level>().Session;
            int chapterIndex = session.Area.ChapterIndex == -1 ? 0 : session.Area.ChapterIndex;
            if (!string.IsNullOrEmpty(upgrade) && upgrade != "MapShard")
            {
                session.SetFlag("Upgrade_" + upgrade, true);
            }
            string Prefix = session.Area.GetLevelSet();
            if (upgrade == "MapShard")
            {
                if (mapShardIndex == 0)
                {
                    if (!XaphanModule.ModSaveData.SavedFlags.Contains(Prefix + "_Ch" + chapterIndex + "_MapShard"))
                    {
                        XaphanModule.ModSaveData.SavedFlags.Add(Prefix + "_Ch" + chapterIndex + "_MapShard");
                    }
                }
                else
                {
                    if (!XaphanModule.ModSaveData.SavedFlags.Contains(Prefix + "_Ch" + chapterIndex + "_MapShard_" + mapShardIndex))
                    {
                        XaphanModule.ModSaveData.SavedFlags.Add(Prefix + "_Ch" + chapterIndex + "_MapShard_" + mapShardIndex);
                    }
                }
            }
            else if (upgrade == "Map")
            {
                if (!XaphanModule.ModSaveData.SavedFlags.Contains(Prefix + "_Can_Open_Map"))
                {
                    XaphanModule.ModSaveData.SavedFlags.Add(Prefix + "_Can_Open_Map");
                }
            }
            else
            {
                AreaKey area = session.Area;
                MapData MapData = AreaData.Areas[area.ID].Mode[(int)area.Mode].MapData;
                bool temporary = false;
                foreach (LevelData levelData in MapData.Levels)
                {
                    foreach (EntityData entity in levelData.Entities)
                    {
                        if (entity.Name == "XaphanHelper/UpgradeController")
                        {
                            temporary = entity.Bool("upgradesAreTemporary", false);
                            break;
                        }
                    }
                }
                if (!temporary)
                {
                    if (!XaphanModule.ModSaveData.SavedFlags.Contains(Prefix + "_Upgrade_" + upgrade))
                    {
                        XaphanModule.ModSaveData.SavedFlags.Add(Prefix + "_Upgrade_" + upgrade);
                    }
                }
            }
        }

        private void setUpgrade(XaphanModule.Upgrades upgrade)
        {
            switch (upgrade)
            {
                case XaphanModule.Upgrades.SpaceJump:
                    XaphanModule.Instance.UpgradeHandlers[upgrade].SetValue(2);
                    break;
                default:
                    XaphanModule.Instance.UpgradeHandlers[upgrade].SetValue(1);
                    break;
            }
        }

        private void EndCutscene()
        {
            Level level = Scene as Level;
            if (Settings.ShowMiniMap)
            {
                MapDisplay mapDisplay = level.Tracker.GetEntity<MapDisplay>();
                if (mapDisplay != null)
                {
                    mapDisplay.GenerateIcons();
                }
            }
            level.Frozen = false;
            level.CanRetry = true;
            level.FormationBackdrop.Display = false;
            Engine.TimeRate = 1f;
            if (upgradeScreen != null)
            {
                upgradeScreen.RemoveSelf();
            }
            RemoveSelf();
        }
    }
}
