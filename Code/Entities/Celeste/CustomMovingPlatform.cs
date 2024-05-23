using System;
using System.Collections;
using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste.Mod.XaphanHelper.Entities
{
    [Tracked()]
    [CustomEntity("XaphanHelper/CustomMovingPlatform")]
    class CustomMovingPlatform : Solid
    {
        private Vector2[] nodes;

        private int amount;

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

        public Sprite platform;

        private Sprite node;

        private int direction;

        private bool swapped;

        private bool fromFirstLoad;

        private string mode;

        private int id;

        private float speedMult;

        private int length;

        private string stopFlag;

        private string swapFlag;

        private string moveFlag;

        private string forceInactiveFlag;

        private bool drawTrack;

        private bool particles;

        private bool Moving = true;

        public float noCollideDelay;

        private string Orientation;

        private string AttachedEntityPlatformsIndexes;

        public CustomMovingPlatform(int id, Vector2 position, Vector2[] nodes, string mode, string directory, int length, string lineColorA, string lineColorB, string particlesColorA, string particlesColorB, string orientation, int amount, float speedMult, float startOffset, float spacingOffset, string attachedEntityPlatformsIndexes, string stopFlag, string swapFlag, string moveFlag, string forceInactiveFlag, bool drawTrack, bool particles, int direction, float startPercent = -1f, bool swapped = false, bool fromFirstLoad = false) : base(position, 8, 8, false)
        {
            Tag = Tags.TransitionUpdate;
            noCollideDelay = 0.01f;
            Add(new Coroutine(CollideDelayRoutine()));
            Add(new LedgeBlocker());
            Collider = new Hitbox(8, length * 8, -4, -length * 8 / 2);
            this.id = id;
            this.nodes = nodes;
            this.mode = mode;
            this.directory = directory;
            this.length = length;
            this.lineColorA = lineColorA;
            this.lineColorB = lineColorB;
            this.particlesColorA = particlesColorA;
            this.particlesColorB = particlesColorB;
            this.amount = amount;
            this.speedMult = speedMult;
            this.startOffset = startOffset;
            this.spacingOffset = spacingOffset;
            AttachedEntityPlatformsIndexes = attachedEntityPlatformsIndexes;
            this.stopFlag = stopFlag;
            this.swapFlag = swapFlag;
            this.moveFlag = moveFlag;
            this.forceInactiveFlag = forceInactiveFlag;
            this.drawTrack = drawTrack;
            this.particles = particles;
            this.direction = direction;
            this.swapped = swapped;
            this.fromFirstLoad = fromFirstLoad;
            Orientation = orientation;
            if (string.IsNullOrEmpty(this.directory))
            {
                this.directory = "objects/XaphanHelper/CustomMovingPlatform";
            }
            lengths = new float[nodes.Length];
            for (int i = 1; i < lengths.Length; i++)
            {
                lengths[i] = lengths[i - 1] + Vector2.Distance(nodes[i - 1], nodes[i]);
            }
            speed = speedMult / lengths[lengths.Length - 1];
            percent = startPercent;
            percent %= 1f;
            Add(platform = new Sprite(GFX.Game, this.directory + "/"));
            platform.Add("idle", "platform", 0.01f);
            platform.CenterOrigin();
            platform.Play("idle");
            Add(node = new Sprite(GFX.Game, this.directory + "/"));
            node.AddLoop("node", "node", 0.15f);
            node.CenterOrigin();
            node.Play("node");
            Add(trackSfx = new SoundSource());
            Collidable = false;
            Depth = 9999;
        }

        public CustomMovingPlatform(EntityData data, Vector2 offset, EntityID eid) : this(eid.ID, data.Position, data.NodesWithPosition(offset), data.Attr("mode", "Restart"), data.Attr("directory", "objects/XaphanHelper/CustomMovingPlatform"), data.Int("length", 3), data.Attr("lineColorA", "2A251F"), data.Attr("lineColorB", "C97F35"), data.Attr("particlesColorA", "696A6A"), data.Attr("particlesColorB", "700808"), data.Attr("orientation", "Horizontal"), data.Int("amount", 1), data.Float("speed", 60f), data.Float("startOffset"), data.Float("spacingOffset"), data.Attr("attachedEntityPlatformsIndexes"), data.Attr("stopFlag"), data.Attr("swapFlag"), data.Attr("moveFlag"), data.Attr("forceInactiveFlag"), data.Bool("drawTrack", true), data.Bool("particles", true), 1, fromFirstLoad: true)
        {
        }

        private IEnumerator CollideDelayRoutine()
        {
            while (noCollideDelay > 0)
            {
                noCollideDelay -= Engine.DeltaTime;
                yield return null;
            }
        }

        public override void Added(Scene scene)
        {
            base.Added(scene);
            if (fromFirstLoad)
            {
                for (int i = 0; i < amount; i++)
                {
                    if (Orientation == "Top")
                    {
                        Scene.Add(new JumpThruMovingPlatform(id, Position, nodes, mode, directory, length, lineColorA, lineColorB, particlesColorA, particlesColorB, Orientation, amount, i + 1, speedMult, startOffset, spacingOffset, AttachedEntityPlatformsIndexes, stopFlag, swapFlag, moveFlag, forceInactiveFlag, drawTrack, particles, direction));
                    }
                    else
                    {
                        Scene.Add(new SolidMovingPlatform(id, Position, nodes, mode, directory, length, lineColorA, lineColorB, particlesColorA, particlesColorB, Orientation, amount, i + 1, speedMult, startOffset, spacingOffset, AttachedEntityPlatformsIndexes, stopFlag, swapFlag, moveFlag, forceInactiveFlag, drawTrack, particles, direction));
                    }
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
            alpha += Engine.DeltaTime * 4f;
            if ((!string.IsNullOrEmpty(forceInactiveFlag) && SceneAs<Level>().Session.GetFlag(forceInactiveFlag)) || (!string.IsNullOrEmpty(stopFlag) && SceneAs<Level>().Session.GetFlag(stopFlag)) || !Moving)
            {
                return;
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

        public override void DebugRender(Camera camera)
        {
            
        }
    }
}
