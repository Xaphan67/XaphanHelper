using System;
using System.Collections;
using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste.Mod.XaphanHelper.Entities
{
    [Tracked()]
    [CustomEntity("XaphanHelper/Sawblade")]
    class Sawblade : Entity
    {
        private Vector2[] nodes;

        private int amount;

        private int index;

        private float startOffset;

        private float spacingOffset;

        private float[] lengths;

        private float speed;

        private float percent;

        private string directory;

        private string lineColorA;

        private string lineColorB;

        private string particlesColorA;

        private string particlesColorB;

        public float alpha = 0f;

        private SoundSource trackSfx;

        public Sprite saw;

        private Sprite node;

        private int direction;

        private bool swapped;

        private bool fromFirstLoad;

        private string mode;

        private int id;

        private float speedMult;

        private float radius;

        private string stopFlag;

        private string swapFlag;

        private string moveFlag;

        private string forceInactiveFlag;

        private bool drawTrack;

        private bool particles;

        private bool AtStartOfTrack;

        private bool AtEndOfTrack;

        private bool Moving = true;

        private ParticleType P_Trail;

        public Sawblade(int id, Vector2[] nodes, string mode, string directory, float radius, string lineColorA, string lineColorB, string particlesColorA, string particlesColorB, int amount, int index, float speedMult, float startOffset, float spacingOffset, string stopFlag, string swapFlag, string moveFlag, string forceInactiveFlag, bool drawTrack, bool particles, int direction, float startPercent = -1f, bool swapped = false, bool fromFirstLoad = false)
        {
            Tag = Tags.TransitionUpdate;
            Collider = new Circle(radius);
            this.id = id;
            this.nodes = nodes;
            this.mode = mode;
            this.directory = directory;
            this.radius = radius;
            this.lineColorA = lineColorA;
            this.lineColorB = lineColorB;
            this.particlesColorA = particlesColorA;
            this.particlesColorB = particlesColorB;
            this.amount = amount;
            this.index = index;
            this.speedMult = speedMult;
            this.startOffset = startOffset;
            this.spacingOffset = spacingOffset;
            this.stopFlag = stopFlag;
            this.swapFlag = swapFlag;
            this.moveFlag = moveFlag;
            this.forceInactiveFlag = forceInactiveFlag;
            this.drawTrack = drawTrack;
            this.particles = particles;
            this.direction = direction;
            this.swapped = swapped;
            this.fromFirstLoad = fromFirstLoad;
            if (string.IsNullOrEmpty(this.directory))
            {
                this.directory = "danger/XaphanHelper/Sawblade";
            }
            lengths = new float[nodes.Length];
            for (int i = 1; i < lengths.Length; i++)
            {
                lengths[i] = lengths[i - 1] + Vector2.Distance(nodes[i - 1], nodes[i]);
            }
            speed = speedMult / lengths[lengths.Length - 1];
            if (startPercent == -1f && index != 0)
            {
                percent = (index - 1) * spacingOffset;
                percent += startOffset;
                if (Math.Truncate(percent) % 2 != 0)
                {
                    float substract = Math.Abs(1 - percent);
                    percent = 1 - substract;
                    this.direction = -direction;
                }
            }
            else
            {
                percent = startPercent;
            }
            percent %= 1f;
            Position = GetPercentPosition(percent);
            Add(new PlayerCollider(OnPlayer));
            Add(saw = new Sprite(GFX.Game, this.directory + "/"));
            saw.AddLoop("saw", "saw", 0.01f);
            saw.CenterOrigin();
            saw.Play("saw");
            Add(node = new Sprite(GFX.Game, this.directory + "/"));
            node.AddLoop("node", "node", 0.15f);
            node.CenterOrigin();
            node.Play("node");
            if (index == 0)
            {
                Add(trackSfx = new SoundSource());
                Collidable = false;
            }
            P_Trail = new ParticleType
            {
                Color = Calc.HexToColor(particlesColorA),
                Color2 = Calc.HexToColor(particlesColorB),
                ColorMode = ParticleType.ColorModes.Choose,
                FadeMode = ParticleType.FadeModes.Late,
                LifeMin = 0.3f,
                LifeMax = 0.6f,
                Size = 1f,
                DirectionRange = (float)Math.PI * 2f,
                SpeedMin = 4f,
                SpeedMax = 8f,
                SpeedMultiplier = 0.8f
            };
            if (index == 0)
            {
                Depth = 9999;
            }
        }

        public Sawblade(EntityData data, Vector2 offset, EntityID eid) : this(eid.ID, data.NodesWithPosition(offset), data.Attr("mode", "Restart"), data.Attr("directory", "danger/XaphanHelper/Sawblade"), data.Float("radius", 8f), data.Attr("lineColorA", "2A251F"), data.Attr("lineColorB", "C97F35"), data.Attr("particlesColorA", "696A6A"), data.Attr("particlesColorB", "700808"), data.Int("amount", 1), 0, data.Float("speed", 60f), data.Float("startOffset"), data.Float("spacingOffset"), data.Attr("stopFlag"), data.Attr("swapFlag"), data.Attr("moveFlag"), data.Attr("forceInactiveFlag"), data.Bool("drawTrack", true), data.Bool("particles", true), 1, fromFirstLoad: true)
        {
        }

        public override void Added(Scene scene)
        {
            base.Added(scene);
            if (fromFirstLoad)
            {
                for (int i = 0; i < amount; i++)
                {
                    Scene.Add(new Sawblade(id, nodes, mode, directory, radius, lineColorA, lineColorB, particlesColorA, particlesColorB, amount, i + 1, speedMult, startOffset, spacingOffset, stopFlag, swapFlag, moveFlag, forceInactiveFlag, drawTrack, particles, direction));
                }
            }
            if (trackSfx != null)
            {
                PositionTrackSfx();
                //trackSfx.Play("event:/env/local/09_core/fireballs_idle");
            }
        }

        public override void Removed(Scene scene)
        {
            base.Removed(scene);
        }

        public override void Update()
        {
            if (!string.IsNullOrEmpty(forceInactiveFlag) && SceneAs<Level>().Session.GetFlag(forceInactiveFlag))
            {
                alpha = 0f;
            }
            else
            {
                alpha += Engine.DeltaTime * 4f;
            }
            if ((Scene as Level).Transitioning)
            {
                PositionTrackSfx();
                return;
            }
            if (!string.IsNullOrEmpty(forceInactiveFlag) && SceneAs<Level>().Session.GetFlag(forceInactiveFlag))
            {
                saw.Rate = node.Rate = 0f;
            }
            else
            {
                saw.Rate = node.Rate = 1f;
            }
            base.Update();
            if (mode == "Flag To Move" && !string.IsNullOrEmpty(moveFlag))
            {
                if (!SceneAs<Level>().Session.GetFlag(moveFlag))
                {
                    direction = -1;
                    if (AtEndOfTrack)
                    {
                        AtEndOfTrack = false;
                    }
                }
                else
                {
                    direction = 1;
                    if (AtStartOfTrack)
                    {
                        AtStartOfTrack = false;
                    }
                }
            }
            if ((!string.IsNullOrEmpty(forceInactiveFlag) && SceneAs<Level>().Session.GetFlag(forceInactiveFlag)) || (!string.IsNullOrEmpty(stopFlag) && SceneAs<Level>().Session.GetFlag(stopFlag)) || AtStartOfTrack || AtEndOfTrack || !Moving)
            {
                return;
            }
            if (index != 0)
            {
                if (mode == "Flag To Move")
                {
                    if (string.IsNullOrEmpty(moveFlag))
                    {
                        return;
                    }
                    if (direction == -1)
                    {
                        percent -= speed * Engine.DeltaTime;
                    }
                    else
                    {
                        percent += speed * Engine.DeltaTime;
                    }
                    if (percent <= 0)
                    {
                        foreach (Sawblade blade in SceneAs<Level>().Tracker.GetEntities<Sawblade>())
                        {
                            if (blade.id == id && blade.index != 0)
                            {
                                blade.AtStartOfTrack = true;
                                blade.percent = (blade.index - 1) * blade.spacingOffset;
                            }
                        }
                    }
                    if (percent >= 1)
                    {
                        foreach (Sawblade blade in SceneAs<Level>().Tracker.GetEntities<Sawblade>())
                        {
                            if (blade.id == id && blade.index != 0)
                            {
                                blade.AtEndOfTrack = true;
                                blade.percent = 1 - (blade.amount - blade.index) * blade.spacingOffset;
                            }
                        }
                    }
                }
                else
                {
                    if (direction == -1)
                    {
                        percent -= speed * Engine.DeltaTime;
                        if (percent <= 0)
                        {
                            if (mode == "Restart")
                            {
                                percent = percent + 1f;
                            }
                            else if (mode.Contains("Back And Forth"))
                            {
                                if (mode.Contains("All Sawblades"))
                                {
                                    foreach (Sawblade blade in SceneAs<Level>().Tracker.GetEntities<Sawblade>())
                                    {
                                        if (blade.id == id && blade.index != 0)
                                        {
                                            blade.direction = 1;
                                            if (blade != this)
                                            {
                                                blade.percent -= blade.speed * Engine.DeltaTime * 2;
                                            }
                                        }
                                    }
                                }
                                else
                                {
                                    percent = Math.Abs(percent);
                                    direction = 1;
                                }
                            }
                        }
                    }
                    else
                    {
                        percent += speed * Engine.DeltaTime;
                        if (percent >= 1f)
                        {
                            if (mode == "Restart")
                            {
                                percent = percent - 1f;
                            }
                            else if (mode.Contains("Back And Forth"))
                            {
                                if (mode.Contains("All Sawblades"))
                                {
                                    foreach (Sawblade blade in SceneAs<Level>().Tracker.GetEntities<Sawblade>())
                                    {
                                        if (blade.id == id && blade.index != 0)
                                        {
                                            blade.direction = -1;
                                            blade.percent -= blade.speed * Engine.DeltaTime;
                                        }
                                    }
                                }
                                else
                                {
                                    percent = 1 - (percent - 1f);
                                    direction = -1;
                                }
                            }
                        }
                    }
                    if (!string.IsNullOrEmpty(swapFlag))
                    {
                        if (SceneAs<Level>().Session.GetFlag(swapFlag) && !swapped)
                        {
                            swapped = true;
                            direction = -direction;
                        }
                        else if (!SceneAs<Level>().Session.GetFlag(swapFlag) && swapped)
                        {
                            swapped = false;
                            direction = -direction;
                        }
                    }
                }
                foreach (Crate crate in Scene.Tracker.GetEntities<Crate>())
                {
                    if (CollideCheck(crate))
                    {
                        if (crate.Type == "Wood" && !crate.Destroyed)
                        {
                            crate.Destroy();
                        }
                        else if (crate.Type == "Metal")
                        {
                            crate.Bounce(new Vector2(crate.Center.X >= Center.X ? 1 : -1, crate.Center.Y >= Center.Y ? 1 : -1));
                        }
                    }
                }
            }
            Position = GetPercentPosition(percent);
            PositionTrackSfx();
            if (Scene.OnInterval(0.05f) && index != 0 && particles)
            {
                SceneAs<Level>().ParticlesBG.Emit(P_Trail, 2, Center, Vector2.One * 3f);
            }
        }

        public void PositionTrackSfx()
        {
            if (trackSfx == null)
            {
                return;
            }
            Player entity = Scene.Tracker.GetEntity<Player>();
            if (entity == null)
            {
                return;
            }
            Vector2? vector = null;
            for (int i = 1; i < nodes.Length; i++)
            {
                Vector2 vector2 = Calc.ClosestPointOnLine(nodes[i - 1], nodes[i], entity.Center);
                if (!vector.HasValue || (vector2 - entity.Center).Length() < (vector.Value - entity.Center).Length())
                {
                    vector = vector2;
                }
            }
            if (vector.HasValue)
            {
                trackSfx.Position = vector.Value - Position;
                trackSfx.UpdateSfxPosition();
            }
        }

        public override void Render()
        {
            if (index == 0)
            {
                if (drawTrack)
                {
                    for (int i = 0; i < nodes.Length; i++)
                    {
                        if (i + 1 < nodes.Length)
                        {
                            Draw.Line(nodes[i], nodes[i + 1], Calc.HexToColor(lineColorA), 4);
                            Draw.Line(nodes[i], nodes[i + 1], Calc.HexToColor(lineColorB) * (0.7f * (0.7f + ((float)Math.Sin(alpha) + 1f) * 0.125f)), 2);
                        }
                        if (i < nodes.Length)
                        {
                            node.RenderPosition = nodes[i];
                            node.Render();
                        }
                    }
                }
            }
            else
            {
                saw.DrawOutline(Color.Black);
                saw.Render();
            }
        }

        public override void DebugRender(Camera camera)
        {
            if (index != 0)
            {
                base.DebugRender(camera);
            }
        }

        private void OnPlayer(Player player)
        {
            if (player.Die((player.Position - Position).SafeNormalize()) != null)
            {
                foreach (Sawblade blade in SceneAs<Level>().Tracker.GetEntities<Sawblade>())
                {
                    if (blade.id == id && blade.index != 0)
                    {
                        blade.Moving = false;
                    }
                }
            }
        }

        private Vector2 GetPercentPosition(float percent)
        {
            if (mode != "Flag To Move")
            {
                if (direction == -1)
                {
                    if (percent <= 0f)
                    {
                        return nodes[nodes.Length - 1];
                    }
                    if (percent >= 1f)
                    {
                        return nodes[0];
                    }
                }
                else
                {
                    if (percent <= 0f)
                    {
                        return nodes[0];
                    }
                    if (percent >= 1f)
                    {
                        return nodes[nodes.Length - 1];
                    }
                }
            }
            float num = lengths[lengths.Length - 1];
            float num2 = num * percent;
            int i;
            for (i = 0; i < lengths.Length - 1 && !(lengths[i + 1] > num2); i++)
            {
            }
            if (i == lengths.Length - 1)
            {
                if (mode != "Flag To Move")
                {
                    return nodes[0];
                }
                else
                {
                    return nodes[lengths.Length - 1];
                }
            }
            float min = lengths[i] / num;
            float max = lengths[i + 1] / num;
            float num3 = Calc.ClampedMap(percent, min, max);
            return Vector2.Lerp(nodes[i], nodes[i + 1], num3);
        }
    }
}
