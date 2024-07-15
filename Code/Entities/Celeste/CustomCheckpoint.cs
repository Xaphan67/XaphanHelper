using Celeste.Mod.Entities;
using Celeste.Mod.XaphanHelper.Managers;
using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste.Mod.XaphanHelper.Entities
{
    [Tracked(true)]
    [CustomEntity("XaphanHelper/CustomCheckpoint")]
    class CustomCheckpoint : Entity
    {
        private Vector2 respawn;

        public bool Activated;

        public bool animated;

        public bool removeBackgroundWhenActive;

        public bool emitLight;

        private string sound;

        private string sprite;

        private float activatedSpriteX;

        private float activatedSpriteY;

        private Sprite bgSprite;

        private Sprite activatedSprite;

        private VertexLight light;

        private BloomPoint bloom;

        private string lightColor;

        public CustomCheckpoint(EntityData data, Vector2 position) : base(data.Position + position)
        {
            removeBackgroundWhenActive = data.Bool("removeBackgroundWhenActive", false);
            sound = data.Attr("sound", "");
            activatedSpriteX = data.Float("activatedSpriteX", 0f);
            activatedSpriteY = data.Float("activatedSpriteY", 0f);
            sprite = data.Attr("sprite");
            emitLight = data.Bool("emitLight");
            lightColor = data.Attr("lightColor");
            if (lightColor == "")
            {
                lightColor = "FFFFFF";
            }
            if (sprite == "")
            {
                sprite = "objects/XaphanHelper/CustomCheckpoint/ruins";
            }
            Add(bgSprite = new Sprite(GFX.Game, sprite + "/"));
            bgSprite.AddLoop("bgSprite", "bg", 0.08f);
            bgSprite.CenterOrigin();
            bgSprite.Play("bgSprite");
            Add(activatedSprite = new Sprite(GFX.Game, sprite + "/"));
            activatedSprite.Justify = new Vector2(-activatedSpriteX + 0.5f, activatedSpriteY + 0.5f);
            activatedSprite.AddLoop("activatedSprite", "active", 0.08f);
            activatedSprite.CenterOrigin();
            Collider = new Hitbox(bgSprite.Width, bgSprite.Height, -17f, -19f);
            Depth = 8999;
            Add(light = new VertexLight(Calc.HexToColor(lightColor), 1f, 48, 64));
            Add(bloom = new BloomPoint(0.5f, 8f));
            bloom.Visible = false;
            light.Visible = false;
        }

        public override void Added(Scene scene)
        {
            base.Added(scene);
            respawn = SceneAs<Level>().GetSpawnPoint(Position);
        }

        public override void Awake(Scene scene)
        {
            base.Awake(scene);
            if (!Activated && CollideCheck<Player>())
            {
                Activated = true;
                Level level = Scene as Level;
                level.Session.RespawnPoint = respawn;
            }
        }

        public void RemoveBGSprite()
        {
            if (removeBackgroundWhenActive)
            {
                bgSprite.RemoveSelf();
            }
            activatedSprite.Visible = true;
            activatedSprite.Play(("activatedSprite"), restart: true);
        }

        public void RestaureBGSprite()
        {
            if (removeBackgroundWhenActive)
            {
                Add(bgSprite = new Sprite(GFX.Game, sprite + "/"));
                bgSprite.AddLoop("bgSprite", "bg", 0.08f);
                bgSprite.CenterOrigin();
                bgSprite.Play("bgSprite");
            }
        }

        public override void Update()
        {
            base.Update();
            Level level = Scene as Level;
            if (!level.Session.GrabbedGolden)
            {
                if (!Activated)
                {
                    Player player = CollideFirst<Player>();
                    if (player != null && player.OnGround() && player.Speed.Y >= 0f)
                    {
                        Activated = true;
                        level.Session.RespawnPoint = respawn;
                        level.Session.UpdateLevelStartDashes();
                        level.Session.HitCheckpoint = true;
                        Audio.Play(sound == "" ? "event:/game/07_summit/checkpoint_confetti" : sound, Position);
                        if (emitLight)
                        {
                            bloom.Visible = true;
                            light.Visible = true;
                        }
                        RemoveBGSprite();
                        foreach (FlagDashSwitch flagDashSwitch in SceneAs<Level>().Tracker.GetEntities<FlagDashSwitch>())
                        {
                            if (!string.IsNullOrEmpty(flagDashSwitch.flag))
                            {
                                flagDashSwitch.startSpawnPoint = respawn;
                                flagDashSwitch.flagState = SceneAs<Level>().Session.GetFlag(flagDashSwitch.flag);
                                int chapterIndex = SceneAs<Level>().Session.Area.ChapterIndex;
                                SceneAs<Level>().Session.SetFlag("Ch" + chapterIndex + "_" + flagDashSwitch.flag + "_true", false);
                                SceneAs<Level>().Session.SetFlag("Ch" + chapterIndex + "_" + flagDashSwitch.flag + "_false", false);
                                if (flagDashSwitch.wasPressed && flagDashSwitch.registerInSaveData && flagDashSwitch.saveDataOnlyAfterCheckpoint)
                                {
                                    string Prefix = SceneAs<Level>().Session.Area.LevelSet;
                                    if (!XaphanModule.ModSaveData.SavedFlags.Contains(Prefix + "_Ch" + chapterIndex + "_" + flagDashSwitch.flag))
                                    {
                                        XaphanModule.ModSaveData.SavedFlags.Add(Prefix + "_Ch" + chapterIndex + "_" + flagDashSwitch.flag);
                                    }
                                    if (XaphanModule.PlayerHasGolden)
                                    {
                                        if (!XaphanModule.ModSaveData.SavedFlags.Contains(Prefix + "_Ch" + chapterIndex + "_" + flagDashSwitch.flag + "_GoldenStrawberry"))
                                        {
                                            XaphanModule.ModSaveData.SavedFlags.Add(Prefix + "_Ch" + chapterIndex + "_" + flagDashSwitch.flag + "_GoldenStrawberry");
                                        }
                                    }
                                }
                            }
                        }
                        foreach (DroneSwitch droneSwitch in SceneAs<Level>().Tracker.GetEntities<DroneSwitch>())
                        {
                            if (!string.IsNullOrEmpty(droneSwitch.flag))
                            {
                                droneSwitch.startSpawnPoint = respawn;
                                droneSwitch.flagState = SceneAs<Level>().Session.GetFlag(droneSwitch.flag);
                                int chapterIndex = SceneAs<Level>().Session.Area.ChapterIndex;
                                SceneAs<Level>().Session.SetFlag("Ch" + chapterIndex + "_" + droneSwitch.flag + "_true", false);
                                SceneAs<Level>().Session.SetFlag("Ch" + chapterIndex + "_" + droneSwitch.flag + "_false", false);
                                if (droneSwitch.wasPressed && droneSwitch.registerInSaveData && droneSwitch.saveDataOnlyAfterCheckpoint)
                                {
                                    string Prefix = SceneAs<Level>().Session.Area.LevelSet;
                                    if (!XaphanModule.ModSaveData.SavedFlags.Contains(Prefix + "_Ch" + chapterIndex + "_" + droneSwitch.flag))
                                    {
                                        XaphanModule.ModSaveData.SavedFlags.Add(Prefix + "_Ch" + chapterIndex + "_" + droneSwitch.flag);
                                    }
                                    if (XaphanModule.PlayerHasGolden)
                                    {
                                        if (!XaphanModule.ModSaveData.SavedFlags.Contains(Prefix + "_Ch" + chapterIndex + "_" + droneSwitch.flag + "_GoldenStrawberry"))
                                        {
                                            XaphanModule.ModSaveData.SavedFlags.Add(Prefix + "_Ch" + chapterIndex + "_" + droneSwitch.flag + "_GoldenStrawberry");
                                        }
                                    }
                                }
                            }
                        }
                        foreach (BombSwitch bombSwitch in SceneAs<Level>().Tracker.GetEntities<BombSwitch>())
                        {
                            if (!string.IsNullOrEmpty(bombSwitch.flag))
                            {
                                bombSwitch.startSpawnPoint = respawn;
                                bombSwitch.flagState = SceneAs<Level>().Session.GetFlag(bombSwitch.flag);
                                int chapterIndex = SceneAs<Level>().Session.Area.ChapterIndex;
                                SceneAs<Level>().Session.SetFlag("Ch" + chapterIndex + "_" + bombSwitch.flag + "_true", false);
                                SceneAs<Level>().Session.SetFlag("Ch" + chapterIndex + "_" + bombSwitch.flag + "_false", false);
                                if (bombSwitch.wasTriggered && bombSwitch.registerInSaveData && bombSwitch.saveDataOnlyAfterCheckpoint)
                                {
                                    string Prefix = SceneAs<Level>().Session.Area.LevelSet;
                                    if (!XaphanModule.ModSaveData.SavedFlags.Contains(Prefix + "_Ch" + chapterIndex + "_" + bombSwitch.flag))
                                    {
                                        XaphanModule.ModSaveData.SavedFlags.Add(Prefix + "_Ch" + chapterIndex + "_" + bombSwitch.flag);
                                    }
                                    if (XaphanModule.PlayerHasGolden)
                                    {
                                        if (!XaphanModule.ModSaveData.SavedFlags.Contains(Prefix + "_Ch" + chapterIndex + "_" + bombSwitch.flag + "_GoldenStrawberry"))
                                        {
                                            XaphanModule.ModSaveData.SavedFlags.Add(Prefix + "_Ch" + chapterIndex + "_" + bombSwitch.flag + "_GoldenStrawberry");
                                        }
                                    }
                                }
                            }
                        }
                        foreach (Lever lever in SceneAs<Level>().Tracker.GetEntities<Lever>())
                        {
                            if (!string.IsNullOrEmpty(lever.Flag))
                            {
                                lever.startSpawnPoint = respawn;
                                lever.flagState = SceneAs<Level>().Session.GetFlag(lever.Flag);
                                int chapterIndex = SceneAs<Level>().Session.Area.ChapterIndex;
                                SceneAs<Level>().Session.SetFlag("Ch" + chapterIndex + "_" + lever.Flag + "_true", false);
                                SceneAs<Level>().Session.SetFlag("Ch" + chapterIndex + "_" + lever.Flag + "_false", false);
                                if (lever.wasSwitched && lever.registerInSaveData && lever.saveDataOnlyAfterCheckpoint)
                                {
                                    string Prefix = SceneAs<Level>().Session.Area.LevelSet;
                                    if (!XaphanModule.ModSaveData.SavedFlags.Contains(Prefix + "_Ch" + chapterIndex + "_" + lever.Flag))
                                    {
                                        XaphanModule.ModSaveData.SavedFlags.Add(Prefix + "_Ch" + chapterIndex + "_" + lever.Flag);
                                    }
                                    if (XaphanModule.PlayerHasGolden)
                                    {
                                        if (!XaphanModule.ModSaveData.SavedFlags.Contains(Prefix + "_Ch" + chapterIndex + "_" + lever.Flag + "_GoldenStrawberry"))
                                        {
                                            XaphanModule.ModSaveData.SavedFlags.Add(Prefix + "_Ch" + chapterIndex + "_" + lever.Flag + "_GoldenStrawberry");
                                        }
                                    }
                                }
                            }
                        }
                        foreach (LightManager manager in SceneAs<Level>().Tracker.GetEntities<LightManager>())
                        {
                            manager.RespawnMode = manager.MainMode;
                        }
                        if (XaphanModule.PlayerIsControllingRemoteDrone())
                        {
                            Drone drone = SceneAs<Level>().Tracker.GetEntity<Drone>();
                            if (drone != null)
                            {
                                XaphanModule.ModSession.CurrentDroneMissile = drone.CurrentMissiles;
                                XaphanModule.ModSession.CurrentDroneSuperMissile = drone.CurrentSuperMissiles;
                                drone.CurrentSpawn = Position;
                            }
                        }
                        foreach (EntityID entity in XaphanModule.ModSession.NoRespawnIds)
                        {
                            SceneAs<Level>().Session.DoNotLoad.Add(entity);
                        }
                        XaphanModule.ModSession.NoRespawnIds.Clear();
                        foreach (CustomCheckpoint customCheckpoint in SceneAs<Level>().Tracker.GetEntities<CustomCheckpoint>())
                        {
                            if (customCheckpoint != this)
                            {
                                customCheckpoint.Activated = false;
                                customCheckpoint.activatedSprite.Stop();
                                customCheckpoint.activatedSprite.Visible = false;
                                customCheckpoint.RestaureBGSprite();
                                customCheckpoint.bloom.Visible = false;
                                customCheckpoint.light.Visible = false;
                            }
                        }
                    }
                }
                else if (Activated && !animated)
                {
                    if (emitLight)
                    {
                        bloom.Visible = true;
                        light.Visible = true;
                    }
                    RemoveBGSprite();
                    animated = true;
                }
            }
        }

        public override void Render()
        {
            if (bgSprite != null)
            {
                bgSprite.Render();
            }
            if (activatedSprite != null && activatedSprite.Visible)
            {
                activatedSprite.Render();
            }
        }
    }
}
