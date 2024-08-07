using System;
using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste.Mod.XaphanHelper.Entities
{
    [Tracked(true)]
    [CustomEntity("XaphanHelper/DroneGate")]
    public class DroneGate : Solid
    {
        public class DroneGateWall : Solid
        {
            public DroneGateWall(DroneGate gate, Vector2 offset) : base(gate.Position + offset, 8, 8, safe: true)
            {
                Collider = new Hitbox(8f, 8f);
                Add(new LightOcclude(0.5f));
            }
        }

        public string side;

        public string directory;

        public string flag;

        private string forceInactiveFlag;

        public Sprite gateSprite;

        public Sprite lightSprite;

        public bool open;

        public DroneGate(EntityData data, Vector2 offset) : base(data.Position + offset, data.Width, data.Height, safe: true)
        {
            Tag = Tags.TransitionUpdate;
            side = data.Attr("side");
            flag = data.Attr("flag");
            forceInactiveFlag = data.Attr("forceInactiveFlag");
            directory = data.Attr("directory", "objects/XaphanHelper/DroneGate");
            Add(gateSprite = new Sprite(GFX.Game, directory + "/"));
            gateSprite.Add("closed", "closed", 0f);
            gateSprite.Add("open", "open", 0.05f, 0, 1, 2);
            gateSprite.Add("opened", "opened", 0f);
            gateSprite.Add("close", "open", 0.05f, 2, 1, 0);
            Add(lightSprite = new Sprite(GFX.Game, directory + "/"));
            lightSprite.Add("greenL", "greenL", 0.1f);
            lightSprite.Add("greenR", "greenR", 0.1f);
            lightSprite.Add("greenT", "greenT", 0.1f);
            lightSprite.Add("greenB", "greenB", 0.1f);
            lightSprite.Add("redL", "redL", 0.1f);
            lightSprite.Add("redR", "redR", 0.1f);
            lightSprite.Add("redT", "redT", 0.1f);
            lightSprite.Add("redB", "redB", 0.1f);
            lightSprite.Add("lockL", "lockL", 0.1f);
            lightSprite.Add("lockR", "lockR", 0.1f);
            lightSprite.Add("lockT", "lockT", 0.1f);
            lightSprite.Add("lockB", "lockB", 0.1f);
            switch (side)
            {
                case "Left":
                    Collider = new Hitbox(4f, 8f);
                    gateSprite.Position = new Vector2(0f, 16f);
                    gateSprite.FlipX = true;
                    gateSprite.Rotation = -(float)Math.PI / 2f;
                    lightSprite.Position = new Vector2(0f, -8f);
                    lightSprite.Play("redL");
                    break;
                case "Right":
                    Collider = new Hitbox(4f, 8f, 4f, 0f);
                    gateSprite.Position = new Vector2(8f, -8f);
                    gateSprite.Rotation = (float)Math.PI / 2f;
                    lightSprite.Position = new Vector2(0f, -8f);
                    lightSprite.Play("redR");
                    break;
                case "Top":
                    Collider = new Hitbox(8f, 4f);
                    gateSprite.Position = new Vector2(-8f, 0f);
                    lightSprite.Position = new Vector2(-8f, 0f);
                    lightSprite.Play("redT");
                    break;
                case "Bottom":
                    Collider = new Hitbox(8f, 4f, 0f, 4f);
                    gateSprite.Position = new Vector2(-8f, 0f);
                    gateSprite.FlipY = true;
                    lightSprite.Position = new Vector2(-8f, 0f);
                    lightSprite.Play("redB");
                    break;
            }
            gateSprite.Play("closed");
            Add(new LightOcclude(0.5f));
            Depth = -10000;
        }

        public override void Added(Scene scene)
        {
            base.Added(scene);
            if (side == "Left" || side == "Right")
            {
                SceneAs<Level>().Add(new DroneGateWall(this, new Vector2(0f, -8f)));
                SceneAs<Level>().Add(new DroneGateWall(this, new Vector2(0f, 8f)));
                if (!string.IsNullOrEmpty(flag) && !SceneAs<Level>().Session.GetFlag(flag))
                {
                    if (side == "Left")
                    {
                        lightSprite.Play("lockL");
                    }
                    else
                    {
                        lightSprite.Play("lockR");
                    }
                }
            }
            else if (side == "Top" || side == "Bottom")
            {
                SceneAs<Level>().Add(new DroneGateWall(this, new Vector2(-8f, 0f)));
                SceneAs<Level>().Add(new DroneGateWall(this, new Vector2(8f, 0f)));
                if (!string.IsNullOrEmpty(flag) && !SceneAs<Level>().Session.GetFlag(flag))
                {
                    if (side == "Top")
                    {
                        lightSprite.Play("lockT");
                    }
                    else
                    {
                        lightSprite.Play("lockB");
                    }
                }
            }
        }

        public override void Update()
        {
            base.Update();
            {
                Drone drone = SceneAs<Level>().Tracker.GetEntity<Drone>();
                if (string.IsNullOrEmpty(forceInactiveFlag) || (!string.IsNullOrEmpty(forceInactiveFlag) && !SceneAs<Level>().Session.GetFlag(forceInactiveFlag)))
                {
                    if (string.IsNullOrEmpty(flag) || (!string.IsNullOrEmpty(flag) && SceneAs<Level>().Session.GetFlag(flag)))
                    {
                        if (lightSprite.CurrentAnimationID.Contains("lock"))
                        {
                            if (side == "Left" || side == "Right")
                            {
                                lightSprite.Play(side == "Left" ? "redL" : "redR");
                            }
                            else
                            {
                                lightSprite.Play(side == "Top" ? "redT" : "redB");

                            }
                        }
                        if (drone != null && drone.enabled && !drone.dead)
                        {
                            if (side == "Left" || side == "Right")
                            {
                                if (drone.Left > Left - 24f && drone.Right < Right + 24f && drone.Top > Top - 16f && drone.Bottom < Bottom + 16f)
                                {
                                    if (!open)
                                    {
                                        open = true;
                                        Audio.Play("event:/game/xaphan/pipegate_open_close", Position);
                                        gateSprite.Play("open");
                                        lightSprite.Play(side == "Left" ? "greenL" : "greenR");
                                        gateSprite.OnLastFrame += onLastFrame;
                                    }
                                    Collidable = false;
                                }
                                else
                                {
                                    if (open)
                                    {
                                        open = false;
                                        Audio.Play("event:/game/xaphan/pipegate_open_close", Position);
                                        gateSprite.Play("close");
                                        lightSprite.Play(side == "Left" ? "redL" : "redR");
                                        gateSprite.OnLastFrame += onLastFrame;
                                    }
                                    Collidable = true;
                                }
                            }
                            else if (side == "Top" || side == "Bottom")
                            {
                                if (drone.Left > Left - 16f && drone.Right < Right + 16f && drone.Top > Top - 24f && drone.Bottom < Bottom + 24f)
                                {
                                    if (!open)
                                    {
                                        open = true;
                                        Audio.Play("event:/game/xaphan/pipegate_open_close", Position);
                                        gateSprite.Play("open");
                                        lightSprite.Play(side == "Top" ? "greenT" : "greenB");
                                        gateSprite.OnLastFrame += onLastFrame;
                                    }
                                    Collidable = false;
                                }
                                else
                                {
                                    if (open)
                                    {
                                        open = false;
                                        Audio.Play("event:/game/xaphan/pipegate_open_close", Position);
                                        gateSprite.Play("close");
                                        lightSprite.Play(side == "Top" ? "redT" : "redB");
                                        gateSprite.OnLastFrame += onLastFrame;
                                    }
                                    Collidable = true;
                                }
                            }
                        }
                        else
                        {
                            if ((side == "Left" || side == "Right") && open)
                            {
                                open = false;
                                Audio.Play("event:/game/xaphan/pipegate_open_close", Position);
                                gateSprite.Play("close");
                                lightSprite.Play(side == "Left" ? "redL" : "redR");
                                gateSprite.OnLastFrame += onLastFrame;
                            }
                            else if ((side == "Top" || side == "Bottom") && open)
                            {
                                open = false;
                                Audio.Play("event:/game/xaphan/pipegate_open_close", Position);
                                gateSprite.Play("close");
                                lightSprite.Play(side == "Top" ? "redT" : "redB");
                                gateSprite.OnLastFrame += onLastFrame;
                            }
                            Collidable = true;
                        }
                    }
                    else
                    {
                        if ((side == "Left" || side == "Right") && open)
                        {
                            open = false;
                            Audio.Play("event:/game/xaphan/pipegate_open_close", Position);
                            gateSprite.Play("close");
                            lightSprite.Play(side == "Left" ? "lockL" : "lockR");
                            gateSprite.OnLastFrame += onLastFrame;
                        }
                        else if ((side == "Top" || side == "Bottom") && open)
                        {
                            open = false;
                            Audio.Play("event:/game/xaphan/pipegate_open_close", Position);
                            gateSprite.Play("close");
                            lightSprite.Play(side == "Top" ? "lockT" : "lockB");
                            gateSprite.OnLastFrame += onLastFrame;
                        }
                        else
                        {
                            if (side == "Left" || side == "Right")
                            {
                                lightSprite.Play(side == "Left" ? "lockL" : "lockR");
                            }
                            else
                            {
                                lightSprite.Play(side == "Top" ? "lockT" : "lockB");
                            }
                        }
                    }
                }
            }
        }

        private void onLastFrame(string s)
        {
            if (open)
            {
                gateSprite.Play("opened");
            }
            else
            {
                gateSprite.Play("closed");
            }
        }
    }
}
