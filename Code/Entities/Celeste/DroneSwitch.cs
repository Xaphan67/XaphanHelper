using System;
using System.Collections;
using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste.Mod.XaphanHelper.Entities
{
    [Tracked(true)]
    [CustomEntity("XaphanHelper/DroneSwitch")]
    public class DroneSwitch : Entity
    {
        public string flag;

        public string side;

        public float cooldown;

        private Sprite buttonSprite;

        private StaticMover staticMover;

        public bool wasPressed;

        public Vector2? startSpawnPoint;

        public bool persistent;

        public bool flagState;

        public bool registerInSaveData;

        public bool saveDataOnlyAfterCheckpoint;

        public bool FlagRegiseredInSaveData()
        {
            Session session = SceneAs<Level>().Session;
            string Prefix = session.Area.GetLevelSet();
            int chapterIndex = session.Area.ChapterIndex;
            if (!Settings.SpeedrunMode)
            {
                return XaphanModule.ModSaveData.SavedFlags.Contains(Prefix + "_Ch" + chapterIndex + "_" + flag);
            }
            else
            {
                return session.GetFlag(flag);
            }
        }

        protected XaphanModuleSettings Settings => XaphanModule.Settings;

        public DroneSwitch(EntityData data, Vector2 position) : base(data.Position + position)
        {
            Tag = Tags.TransitionUpdate;
            side = data.Attr("side");
            flag = data.Attr("flag");
            persistent = data.Bool("persistent");
            registerInSaveData = data.Bool("registerInSaveData");
            saveDataOnlyAfterCheckpoint = data.Bool("saveDataOnlyAfterCheckpoint");
            Add(buttonSprite = new Sprite(GFX.Game, "objects/XaphanHelper/DroneSwitch/"));
            buttonSprite.Add("idle", "idle", 0.2f);
            buttonSprite.Add("active", "active", 0.2f);
            buttonSprite.Origin = new Vector2(buttonSprite.Width / 2, buttonSprite.Height / 2);
            buttonSprite.Position = new Vector2(4f, 4f);
            staticMover = new StaticMover();
            staticMover.OnAttach = delegate (Platform p)
            {
                Depth = p.Depth + 1;
            };
            switch (side)
            {
                case "Left":
                    Collider = new Hitbox(4f, 6f, 0f, 1f);
                    staticMover.SolidChecker = ((Solid s) => CollideCheck(s, Position - Vector2.UnitX));
                    staticMover.JumpThruChecker = ((JumpThru jt) => CollideCheck(jt, Position - Vector2.UnitX));
                    Add(staticMover);
                    buttonSprite.Rotation = (float)Math.PI / 2f;
                    break;
                case "Right":
                    Collider = new Hitbox(4f, 6f, 4f, 1f);
                    staticMover.SolidChecker = ((Solid s) => CollideCheck(s, Position + Vector2.UnitX));
                    staticMover.JumpThruChecker = ((JumpThru jt) => CollideCheck(jt, Position + Vector2.UnitX));
                    Add(staticMover);
                    buttonSprite.Rotation = -(float)Math.PI / 2f;
                    break;
                case "Down":
                    Collider = new Hitbox(6f, 4f, 1f, 0f);
                    staticMover.SolidChecker = ((Solid s) => CollideCheck(s, Position - Vector2.UnitY));
                    staticMover.JumpThruChecker = ((JumpThru jt) => CollideCheck(jt, Position - Vector2.UnitY));
                    Add(staticMover);
                    buttonSprite.Rotation = (float)Math.PI;
                    break;
            }
            staticMover.OnEnable = OnEnable;
            staticMover.OnDisable = OnDisable;
        }

        public static void Load()
        {
            On.Celeste.ChangeRespawnTrigger.OnEnter += onChangeRespawnTriggerOnEnter;
        }

        public static void Unload()
        {
            On.Celeste.ChangeRespawnTrigger.OnEnter -= onChangeRespawnTriggerOnEnter;
        }

        private static void onChangeRespawnTriggerOnEnter(On.Celeste.ChangeRespawnTrigger.orig_OnEnter orig, ChangeRespawnTrigger self, Player player)
        {
            orig(self, player);
            bool onSolid = true;
            Vector2 point = self.Target + Vector2.UnitY * -4f;
            Session session = self.SceneAs<Level>().Session;
            if (self.Scene.CollideCheck<Solid>(point))
            {
                onSolid = self.Scene.CollideCheck<FloatySpaceBlock>(point);
            }
            if (onSolid && (!session.RespawnPoint.HasValue || session.RespawnPoint.Value != self.Target))
            {
                foreach (DroneSwitch droneSwitch in self.SceneAs<Level>().Tracker.GetEntities<DroneSwitch>())
                {
                    if (!string.IsNullOrEmpty(droneSwitch.flag))
                    {
                        droneSwitch.startSpawnPoint = session.RespawnPoint;
                        droneSwitch.flagState = session.GetFlag(droneSwitch.flag);
                        int chapterIndex = self.SceneAs<Level>().Session.Area.ChapterIndex;
                        self.SceneAs<Level>().Session.SetFlag("Ch" + chapterIndex + "_" + droneSwitch.flag + "_true", false);
                        self.SceneAs<Level>().Session.SetFlag("Ch" + chapterIndex + "_" + droneSwitch.flag + "_false", false);
                        if (droneSwitch.wasPressed && droneSwitch.registerInSaveData && !droneSwitch.saveDataOnlyAfterCheckpoint)
                        {
                            string Prefix = self.SceneAs<Level>().Session.Area.GetLevelSet();
                            if (droneSwitch.registerInSaveData && droneSwitch.saveDataOnlyAfterCheckpoint)
                            {
                                if (self.SceneAs<Level>().Session.GetFlag(droneSwitch.flag) && !XaphanModule.ModSaveData.SavedFlags.Contains(Prefix + "_Ch" + chapterIndex + "_" + droneSwitch.flag))
                                {
                                    XaphanModule.ModSaveData.SavedFlags.Add(Prefix + "_Ch" + chapterIndex + "_" + droneSwitch.flag);
                                }
                                else if (self.SceneAs<Level>().Session.GetFlag(droneSwitch.flag) && XaphanModule.ModSaveData.SavedFlags.Contains(Prefix + "_Ch" + chapterIndex + "_" + droneSwitch.flag))
                                {
                                    XaphanModule.ModSaveData.SavedFlags.Remove(Prefix + "_Ch" + chapterIndex + "_" + droneSwitch.flag);
                                }
                            }
                        }
                    }
                }
            }
        }

        public override void Added(Scene scene)
        {
            base.Added(scene);
            int chapterIndex = SceneAs<Level>().Session.Area.ChapterIndex;
            if (FlagRegiseredInSaveData())
            {
                flagState = true;
                SceneAs<Level>().Session.SetFlag(flag, true);
            }
            else
            {
                if (SceneAs<Level>().Session.GetFlag("Ch" + chapterIndex + "_" + flag + "_true"))
                {
                    flagState = true;
                    SceneAs<Level>().Session.SetFlag(flag, true);
                }
                else if (SceneAs<Level>().Session.GetFlag("Ch" + chapterIndex + "_" + flag + "_false"))
                {
                    flagState = false;
                    SceneAs<Level>().Session.SetFlag(flag, false);
                }
                else
                {
                    flagState = SceneAs<Level>().Session.GetFlag(flag);
                    SceneAs<Level>().Session.SetFlag("Ch" + chapterIndex + "_" + flag + "_true", false);
                    SceneAs<Level>().Session.SetFlag("Ch" + chapterIndex + "_" + flag + "_false", false);
                    SceneAs<Level>().Session.SetFlag("Ch" + chapterIndex + "_" + flag + "_" + (flagState ? "true" : "false"), true);
                }
            }
            if (!SceneAs<Level>().Session.GetFlag(flag))
            {
                buttonSprite.Play("idle");
            }
            else
            {
                buttonSprite.Play("active");
            }
        }

        public override void Update()
        {
            base.Update();
            if (SceneAs<Level>().Transitioning && wasPressed)
            {
                flagState = SceneAs<Level>().Session.GetFlag(flag);
                string Prefix = SceneAs<Level>().Session.Area.GetLevelSet();
                int chapterIndex = SceneAs<Level>().Session.Area.ChapterIndex;
                SceneAs<Level>().Session.SetFlag("Ch" + chapterIndex + "_" + flag + "_true", false);
                SceneAs<Level>().Session.SetFlag("Ch" + chapterIndex + "_" + flag + "_false", false);
                if (registerInSaveData && saveDataOnlyAfterCheckpoint)
                {
                    if (SceneAs<Level>().Session.GetFlag(flag) && !XaphanModule.ModSaveData.SavedFlags.Contains(Prefix + "_Ch" + chapterIndex + "_" + flag))
                    {
                        XaphanModule.ModSaveData.SavedFlags.Add(Prefix + "_Ch" + chapterIndex + "_" + flag);
                    }
                    else if (!SceneAs<Level>().Session.GetFlag(flag) && XaphanModule.ModSaveData.SavedFlags.Contains(Prefix + "_Ch" + chapterIndex + "_" + flag))
                    {
                        XaphanModule.ModSaveData.SavedFlags.Remove(Prefix + "_Ch" + chapterIndex + "_" + flag);
                    }
                }
            }
            if (!SceneAs<Level>().Session.GetFlag(flag))
            {
                buttonSprite.Play("idle");
            }
            else
            {
                buttonSprite.Play("active");
            }
        }

        private void OnEnable()
        {
            Active = (Visible = (Collidable = true));
        }

        private void OnDisable()
        {
            Active = (Visible = (Collidable = false));
        }

        public void Triggered(string dir)
        {
            if (cooldown <= 0)
            {
                if (dir != null && dir == side)
                {
                    Add(new Coroutine(SwitchFlag()));
                }
            }
        }

        private IEnumerator SwitchFlag()
        {
            wasPressed = true;
            startSpawnPoint = SceneAs<Level>().Session.RespawnPoint;
            SceneAs<Level>().Session.SetFlag(flag, SceneAs<Level>().Session.GetFlag(flag) ? false : true);
            if (registerInSaveData && !saveDataOnlyAfterCheckpoint)
            {
                string Prefix = SceneAs<Level>().Session.Area.GetLevelSet();
                int chapterIndex = SceneAs<Level>().Session.Area.ChapterIndex;
                if (SceneAs<Level>().Session.GetFlag(flag) && !XaphanModule.ModSaveData.SavedFlags.Contains(Prefix + "_Ch" + chapterIndex + "_" + flag))
                {
                    XaphanModule.ModSaveData.SavedFlags.Add(Prefix + "_Ch" + chapterIndex + "_" + flag);
                }
                else if (!SceneAs<Level>().Session.GetFlag(flag) && XaphanModule.ModSaveData.SavedFlags.Contains(Prefix + "_Ch" + chapterIndex + "_" + flag))
                {
                    XaphanModule.ModSaveData.SavedFlags.Remove(Prefix + "_Ch" + chapterIndex + "_" + flag);
                }
            }
            Audio.Play("event:/game/05_mirror_temple/button_activate", Position);
            cooldown = 0.5f;
            while (cooldown > 0)
            {
                cooldown -= Engine.DeltaTime;
                yield return null;
            }
        }

        public override void Render()
        {
            base.Render();
            buttonSprite.Render();
        }
    }
}
