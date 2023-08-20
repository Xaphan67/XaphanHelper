using System.Collections;
using Celeste.Mod.Entities;
using Celeste.Mod.XaphanHelper.UI_Elements;
using Microsoft.Xna.Framework;
using Monocle;

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

        private string inputActionA;

        private string inputActionB;

        private string poemTextA;

        private string poemTextB;

        private string poemTextC;

        private string nameColor;

        private string descColor;

        private string particleColor;

        private object controlA;

        private object controlB;

        private CustomPoem poem;

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
            if (XaphanModule.PlayerHasGolden)
            {
                return XaphanModule.ModSaveData.SavedFlags.Contains(Prefix + "_Upgrade_" + upgrade + "_GoldenStrawberry");
            }
            return XaphanModule.ModSaveData.SavedFlags.Contains(Prefix + "_Upgrade_" + upgrade);
        }

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
                if (FlagRegiseredInSaveData() || SceneAs<Level>().Session.GetFlag("Upgrade_" + upgrade))
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
            session.DoNotLoad.Add(ID);
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
            if (string.IsNullOrEmpty(customName))
            {
                poemTextA = Dialog.Clean("XaphanHelper_get_" + upgrade + "_Name");
            }
            else
            {
                poemTextA = Dialog.Clean(customName);
            }
            string drone = "";
            if ((upgrade == "LongBeam" || upgrade == "IceBeam" || upgrade == "WaveBeam") && !XaphanModule.useMetroidGameplay)
            {
                drone = "_drone";
            }
            poemTextB = Dialog.Clean("XaphanHelper_get_" + upgrade + "_Desc" + drone);
            poemTextC = Dialog.Clean("XaphanHelper_get_" + upgrade + "_Desc_b");
            if (string.IsNullOrEmpty(particleColor))
            {
                particleColor = "FFFFFF";
            }
            bool select = false;
            switch (upgrade)
            {
                case "Map":
                    controlA = XaphanModule.ModSettings.OpenMap;
                    inputActionA = "XaphanHelper_Press";
                    break;
                case "PowerGrip":
                    controlA = Input.Grab;
                    inputActionA = "XaphanHelper_Press";
                    break;
                case "ClimbingKit":
                    controlA = Input.MenuUp;
                    controlB = Input.MenuDown;
                    inputActionA = "XaphanHelper_Hold";
                    inputActionB = "XaphanHelper_Or";
                    break;
                case "SpiderMagnet":
                    controlA = Input.Grab;
                    inputActionA = "XaphanHelper_Hold";
                    break;
                case "DashBoots":
                    controlA = Input.Dash;
                    inputActionA = "XaphanHelper_Press";
                    break;
                case "SpaceJump":
                    controlA = Input.Jump;
                    inputActionA = "XaphanHelper_Press";
                    break;
                case "HoverJet":
                    controlA = Input.Grab;
                    inputActionA = "XaphanHelper_Hold";
                    break;
                case "LightningDash":
                    controlA = Input.Dash;
                    inputActionA = "XaphanHelper_ClingingPress";
                    break;
                case "LongBeam":
                    poemTextC = null;
                    break;
                case "IceBeam":
                    poemTextC = null;
                    break;
                case "WaveBeam":
                    poemTextC = null;
                    break;
                case "DroneTeleport":
                    controlA = XaphanModule.ModSettings.UseBagItemSlot;
                    inputActionA = "XaphanHelper_Press";
                    break;
                case "GravityJacket":
                    poemTextC = null;
                    break;
                case "Bombs":
                    select = true;
                    controlA = XaphanModule.ModSettings.SelectItem;
                    controlB = XaphanModule.ModSettings.UseBagItemSlot;
                    inputActionA = "XaphanHelper_ThenHold";
                    break;
                case "MegaBombs":
                    select = true;
                    controlA = XaphanModule.ModSettings.SelectItem;
                    controlB = XaphanModule.ModSettings.UseBagItemSlot;
                    inputActionA = "XaphanHelper_ThenHold";
                    break;
                case "RemoteDrone":
                    select = true;
                    controlA = XaphanModule.ModSettings.SelectItem;
                    controlB = XaphanModule.ModSettings.UseBagItemSlot;
                    inputActionA = "XaphanHelper_ThenHold";
                    break;
                case "GoldenFeather":
                    controlA = Input.Grab;
                    inputActionA = "XaphanHelper_Hold";
                    break;
                case "EtherealDash":
                    poemTextC = null;
                    break;
                case "Binoculars":
                    select = true;
                    controlA = XaphanModule.ModSettings.SelectItem;
                    controlB = XaphanModule.ModSettings.UseMiscItemSlot;
                    inputActionA = "XaphanHelper_ThenPress";
                    break;
                case "PortableStation":
                    select = true;
                    controlA = XaphanModule.ModSettings.SelectItem;
                    controlB = XaphanModule.ModSettings.UseMiscItemSlot;
                    inputActionA = "XaphanHelper_ThenPress";
                    break;
                case "PulseRadar":
                    select = true;
                    controlA = XaphanModule.ModSettings.SelectItem;
                    controlB = XaphanModule.ModSettings.UseMiscItemSlot;
                    inputActionA = "XaphanHelper_ThenPress";
                    break;
                case "JumpBoost":
                    controlA = Input.Jump;
                    inputActionA = "XaphanHelper_Hold";
                    break;
                case "MissilesModule":
                    poemTextC = null;
                    break;
                case "SuperMissilesModule":
                    poemTextC = null;
                    break;
                default:
                    break;
            }
            poem = new CustomPoem(inputActionA, poemTextA, inputActionB, poemTextB, poemTextC, nameColor, descColor, descColor, particleColor, sprite, 0.5f, controlA, controlB, select);
            poem.Alpha = 0f;
            Scene.Add(poem);
            for (float t2 = 0f; t2 < 1f; t2 += Engine.RawDeltaTime)
            {
                poem.Alpha = Ease.CubeOut(t2);
                yield return null;
            }
            while (!Input.MenuConfirm.Pressed && !Input.MenuCancel.Pressed)
            {
                yield return null;
            }
            sfx.Source.Param("end", 1f);
            if (upgrade != "Map" && upgrade != "MapShard")
            {
                setUpgrade(upg);
            }
            RegisterFlag();
            level.FormationBackdrop.Display = false;
            for (float t = 0f; t < 1f; t += Engine.RawDeltaTime * 2f)
            {
                poem.Alpha = Ease.CubeIn(1f - t);
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
                    if (XaphanModule.PlayerHasGolden)
                    {
                        if (!XaphanModule.ModSaveData.SavedFlags.Contains(Prefix + "_Upgrade_" + upgrade + "_GoldenStrawberry"))
                        {
                            XaphanModule.ModSaveData.SavedFlags.Add(Prefix + "_Upgrade_" + upgrade + "_GoldenStrawberry");
                        }
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
            if (XaphanModule.ModSettings.ShowMiniMap)
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
            if (poem != null)
            {
                poem.RemoveSelf();
            }
            RemoveSelf();
        }
    }
}
