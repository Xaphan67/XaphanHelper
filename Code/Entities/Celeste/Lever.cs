using System;
using System.Collections;
using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste.Mod.XaphanHelper.Entities
{
    [Tracked(true)]
    [CustomEntity("XaphanHelper/Lever")]
    class Lever : Entity
    {
        private Vector2[] nodes;

        private Sprite Sprite;

        public string Directory;

        public string Side;

        public string Flag;

        public bool CanSwapFlag;

        private bool PlayerOnTop;

        private bool PlayerLeft;

        public Vector2? startSpawnPoint;

        public bool flagState;

        public bool registerInSaveData;

        public bool saveDataOnlyAfterCheckpoint;

        public bool wasSwitched;

        private bool canSwitch;

        private StaticMover staticMover;

        private Coroutine DelayRoutine = new();

        public bool FlagRegiseredInSaveData()
        {
            Session session = SceneAs<Level>().Session;
            string Prefix = session.Area.LevelSet;
            int chapterIndex = session.Area.ChapterIndex;
            return XaphanModule.ModSaveData.SavedFlags.Contains(Prefix + "_Ch" + chapterIndex + "_" + Flag + (XaphanModule.PlayerHasGolden ? "_GoldenStrawberry" : ""));
        }

        public Lever(Vector2 position, Vector2[] nodes, string directory, string flag, bool canSwapFlag, string side, bool registerInSaveData, bool saveDataOnlyAfterCheckpoint) : base(position)
        {
            Tag = Tags.TransitionUpdate;
            Directory = directory;
            Add(Sprite = new Sprite(GFX.Game, Directory + "/"));
            Sprite.Add("start", "lever", 0);
            Sprite.Add("toEnd", "lever", 0.08f, 1, 2);
            Sprite.Add("end", "lever", 0, 2);
            Sprite.Add("toStart", "lever", 0.08f, 1, 0);
            Flag = flag;
            CanSwapFlag = canSwapFlag;
            Side = side;
            this.registerInSaveData = registerInSaveData;
            this.saveDataOnlyAfterCheckpoint = saveDataOnlyAfterCheckpoint;
            staticMover = new StaticMover();
            this.nodes = nodes;
            switch (Side)
            {
                case "Up":
                    Collider = new Hitbox(12, 8, 2, 0);
                    staticMover.SolidChecker = ((Solid s) => CollideCheckOutside(s, Position + Vector2.UnitY * 4f));
                    Sprite.Position = new Vector2(-4f, -8f);
                    break;
                case "Down":
                    Collider = new Hitbox(12, 8, 2, -8);
                    staticMover.SolidChecker = ((Solid s) => CollideCheckOutside(s, Position - Vector2.UnitY * 4f));
                    Sprite.Position = new Vector2(-4f, -8f);
                    Sprite.FlipY = true;
                    break;
                case "Left":
                    Collider = new Hitbox(8, 12, 8, -6);
                    staticMover.SolidChecker = ((Solid s) => CollideCheckOutside(s, Position + Vector2.UnitX * 2f));
                    Sprite.Position = new Vector2(0f, 12f);
                    Sprite.Rotation = -(float)Math.PI / 2;
                    Sprite.FlipX = true;
                    break;
                case "Right":
                    Collider = new Hitbox(8, 12, 0, -6);
                    staticMover.SolidChecker = ((Solid s) => CollideCheckOutside(s, Position - Vector2.UnitX * 2f));
                    Sprite.Position = new Vector2(16f, -12f);
                    Sprite.Rotation = (float)Math.PI / 2;
                    break;
            }
            staticMover.OnEnable = onEnable;
            staticMover.OnDisable = onDisable;
            staticMover.OnShake = onShake;
            staticMover.OnMove = onMove;
            Add(staticMover);
            Add(new PlayerCollider(onPlayer, Collider));
        }

        public Lever(EntityData data, Vector2 offset) : this(data.Position + offset, data.NodesWithPosition(offset), data.Attr("directory"), data.Attr("flag"), data.Bool("canSwapFlag", false), data.Attr("side", "Up"), data.Bool("registerInSaveData", false), data.Bool("saveDataOnlyAfterCheckpoint", false))
        {

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
                foreach (Lever lever in self.SceneAs<Level>().Tracker.GetEntities<Lever>())
                {
                    if (!string.IsNullOrEmpty(lever.Flag))
                    {
                        lever.startSpawnPoint = session.RespawnPoint;
                        lever.flagState = session.GetFlag(lever.Flag);
                        int chapterIndex = self.SceneAs<Level>().Session.Area.ChapterIndex;
                        self.SceneAs<Level>().Session.SetFlag("Ch" + chapterIndex + "_" + lever.Flag + "_true", false);
                        self.SceneAs<Level>().Session.SetFlag("Ch" + chapterIndex + "_" + lever.Flag + "_false", false);
                        if (lever.wasSwitched && lever.registerInSaveData && lever.saveDataOnlyAfterCheckpoint)
                        {
                            string Prefix = self.SceneAs<Level>().Session.Area.LevelSet;
                            if (self.SceneAs<Level>().Session.GetFlag(lever.Flag) && !XaphanModule.ModSaveData.SavedFlags.Contains(Prefix + "_Ch" + chapterIndex + "_" + lever.Flag))
                            {
                                XaphanModule.ModSaveData.SavedFlags.Add(Prefix + "_Ch" + chapterIndex + "_" + lever.Flag);
                            }
                            else if (self.SceneAs<Level>().Session.GetFlag(lever.Flag) && XaphanModule.ModSaveData.SavedFlags.Contains(Prefix + "_Ch" + chapterIndex + "_" + lever.Flag))
                            {
                                XaphanModule.ModSaveData.SavedFlags.Remove(Prefix + "_Ch" + chapterIndex + "_" + lever.Flag);
                            }
                            if (XaphanModule.PlayerHasGolden)
                            {
                                if (self.SceneAs<Level>().Session.GetFlag(lever.Flag) && !XaphanModule.ModSaveData.SavedFlags.Contains(Prefix + "_Ch" + chapterIndex + "_" + lever.Flag + "_GoldenStrawberry"))
                                {
                                    XaphanModule.ModSaveData.SavedFlags.Add(Prefix + "_Ch" + chapterIndex + "_" + lever.Flag + "_GoldenStrawberry");
                                }
                                else if (self.SceneAs<Level>().Session.GetFlag(lever.Flag) && XaphanModule.ModSaveData.SavedFlags.Contains(Prefix + "_Ch" + chapterIndex + "_" + lever.Flag + "_GoldenStrawberry"))
                                {
                                    XaphanModule.ModSaveData.SavedFlags.Remove(Prefix + "_Ch" + chapterIndex + "_" + lever.Flag + "_GoldenStrawberry");
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
            canSwitch = true;
            int chapterIndex = SceneAs<Level>().Session.Area.ChapterIndex;
            if (FlagRegiseredInSaveData())
            {
                flagState = true;
                SceneAs<Level>().Session.SetFlag(Flag, true);
            }
            else
            {
                if (SceneAs<Level>().Session.GetFlag("Ch" + chapterIndex + "_" + Flag + "_true"))
                {
                    flagState = true;
                    SceneAs<Level>().Session.SetFlag(Flag, true);
                }
                else if (SceneAs<Level>().Session.GetFlag("Ch" + chapterIndex + "_" + Flag + "_false"))
                {
                    flagState = false;
                    SceneAs<Level>().Session.SetFlag(Flag, false);
                }
                else
                {
                    flagState = SceneAs<Level>().Session.GetFlag(Flag);
                    SceneAs<Level>().Session.SetFlag("Ch" + chapterIndex + "_" + Flag + "_true", false);
                    SceneAs<Level>().Session.SetFlag("Ch" + chapterIndex + "_" + Flag + "_false", false);
                    SceneAs<Level>().Session.SetFlag("Ch" + chapterIndex + "_" + Flag + "_" + (flagState ? "true" : "false"), true);
                }
            }
            if (SceneAs<Level>().Session.GetFlag(Flag))
            {
                Sprite.Play("end");
            }
            else
            {
                Sprite.Play("start");
            }
        }

        private void onPlayer(Player player)
        {
            if (!PlayerOnTop && (CanSwapFlag || canSwitch))
            {
                Add(new Coroutine(AnimateRoutine(true)));
            }
            PlayerOnTop = true;
            if (!DelayRoutine.Active)
            {
                Add(DelayRoutine = new Coroutine(SwitchDelayRoutine()));
            }
        }

        public void UpdateSprite()
        {
            Add(new Coroutine(AnimateRoutine(false)));
        }

        private IEnumerator AnimateRoutine(bool switchFlag)
        {
            if (!string.IsNullOrEmpty(Flag))
            {
                if (switchFlag)
                {
                    foreach (Lever lever in SceneAs<Level>().Tracker.GetEntities<Lever>())
                    {
                        if (lever != this && lever.Flag == Flag)
                        {
                            lever.UpdateSprite();
                            lever.canSwitch = true;
                        }
                    }
                }
                if (SceneAs<Level>().Session.GetFlag(Flag))
                {
                    Sprite.Play("toStart");
                    yield return 0.16f;
                    Sprite.Play("start");
                }
                else
                {
                    Sprite.Play("toEnd");
                    yield return 0.16f;
                    Sprite.Play("end");
                }
                if (switchFlag)
                {
                    SwitchFlag();
                }
            }
        }

        private void SwitchFlag()
        {
            if (!string.IsNullOrEmpty(Flag))
            {
                wasSwitched = true;
                if (!CanSwapFlag)
                {
                    canSwitch = false;
                }
                startSpawnPoint = SceneAs<Level>().Session.RespawnPoint;
                SceneAs<Level>().Session.SetFlag(Flag, !SceneAs<Level>().Session.GetFlag(Flag));
                foreach (Vector2 node in nodes)
                {
                    SceneAs<Level>().Tracker.GetNearestEntity<Lever>(node).canSwitch = false;
                }
            }
        }

        public override void Update()
        {
            base.Update();
            if (SceneAs<Level>().Transitioning && wasSwitched)
            {
                flagState = SceneAs<Level>().Session.GetFlag(Flag);
                string Prefix = SceneAs<Level>().Session.Area.LevelSet;
                int chapterIndex = SceneAs<Level>().Session.Area.ChapterIndex;
                SceneAs<Level>().Session.SetFlag("Ch" + chapterIndex + "_" + Flag + "_true", false);
                SceneAs<Level>().Session.SetFlag("Ch" + chapterIndex + "_" + Flag + "_false", false);
                if (registerInSaveData && saveDataOnlyAfterCheckpoint)
                {
                    if (SceneAs<Level>().Session.GetFlag(Flag) && !XaphanModule.ModSaveData.SavedFlags.Contains(Prefix + "_Ch" + chapterIndex + "_" + Flag))
                    {
                        XaphanModule.ModSaveData.SavedFlags.Add(Prefix + "_Ch" + chapterIndex + "_" + Flag);
                    }
                    else if (!SceneAs<Level>().Session.GetFlag(Flag) && XaphanModule.ModSaveData.SavedFlags.Contains(Prefix + "_Ch" + chapterIndex + "_" + Flag))
                    {
                        XaphanModule.ModSaveData.SavedFlags.Remove(Prefix + "_Ch" + chapterIndex + "_" + Flag);
                    }
                    if (XaphanModule.PlayerHasGolden)
                    {
                        if (SceneAs<Level>().Session.GetFlag(Flag) && !XaphanModule.ModSaveData.SavedFlags.Contains(Prefix + "_Ch" + chapterIndex + "_" + Flag + "_GoldenStrawberry"))
                        {
                            XaphanModule.ModSaveData.SavedFlags.Add(Prefix + "_Ch" + chapterIndex + "_" + Flag + "_GoldenStrawberry");
                        }
                        else if (!SceneAs<Level>().Session.GetFlag(Flag) && XaphanModule.ModSaveData.SavedFlags.Contains(Prefix + "_Ch" + chapterIndex + "_" + Flag + "_GoldenStrawberry"))
                        {
                            XaphanModule.ModSaveData.SavedFlags.Remove(Prefix + "_Ch" + chapterIndex + "_" + Flag + "_GoldenStrawberry");
                        }
                    }
                }
            }
            if (CollideFirst<Player>() == null && PlayerOnTop)
            {
                PlayerLeft = true;
            }
        }

        private IEnumerator SwitchDelayRoutine()
        {
            while (!PlayerLeft)
            {
                yield return null;
            }
            float timer = 1f;
            while (timer > 0)
            {
                timer -= Engine.DeltaTime;
                yield return null;
            }
            if (CollideFirst<Player>() == null)
            {
                PlayerOnTop = false;
            }
        }

        private void onEnable()
        {
            Visible = true;
            Collidable = true;
        }

        private void onDisable()
        {
            Collidable = false;
            Visible = false;
        }

        private void onShake(Vector2 amount)
        {
            Sprite.Position += amount;
        }

        private void onMove(Vector2 amount)
        {
            Position += amount;
        }

        public override void Render()
        {
            base.Render();
            Sprite.DrawOutline();
            Sprite.Render();
        }
    }
}
