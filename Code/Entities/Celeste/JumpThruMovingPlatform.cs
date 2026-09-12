using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste.Mod.XaphanHelper.Entities
{
    [Tracked()]
    class JumpThruMovingPlatform : JumpThru
    {
        private static FieldInfo SpikesSpikeType = typeof(Spikes).GetField("overrideType", BindingFlags.Instance | BindingFlags.NonPublic);

        private class AttachableInfo
        {
            public Func<Entity, bool> Validate;
            public Func<Entity, JumpThruMovingPlatform, Entity> Clone;
        }

        private static readonly Dictionary<Type, AttachableInfo> AttachableRegistry = new();

        public static void RegisterAttachable<T>(Func<T, bool> validate, Func<T, JumpThruMovingPlatform, Entity> clone) where T : Entity
        {
            AttachableRegistry[typeof(T)] = new AttachableInfo
            {
                Validate = e => validate((T)e),
                Clone = (e, target) => clone((T)e, target)
            };
        }

        static JumpThruMovingPlatform()
        {
            RegisterAttachable<Spikes>(
                validate: s => s.Direction == Spikes.Directions.Up,
                clone: (s, target) => new Spikes(target.Position - target.attachedEntityOffset, target.length * 8, s.Direction, (string)SpikesSpikeType.GetValue(s))
            );

            RegisterAttachable<Spring>(
                validate: s => s.Orientation == Spring.Orientations.Floor,
                clone: (s, target) => new Spring(target.Position - target.attachedEntityOffset, s.Orientation, true)
            );

            RegisterAttachable<Lever>(
                validate: l => l.Side == "Up",
                clone: (l, target) => new Lever(target.Position - target.attachedEntityOffset, l.nodes, l.Directory, l.Flag, l.CanSwapFlag, l.Side, l.registerInSaveData, l.saveDataOnlyAfterCheckpoint)
            );
        }

        private Vector2[] nodes;

        private int amount;

        private int index;

        private float startOffset;

        private float spacingOffset;

        private float[] lengths;

        private float speed;

        private float percent;

        private string directory;

        private string particlesColorA;

        private string particlesColorB;

        public float alpha = 0f;

        private SoundSource trackSfx;

        private List<Sprite> sprites = new();

        private int direction;

        private bool swapped;

        private string mode;

        private int id;

        private float speedMult;

        private int length;

        private string stopFlag;

        private string swapFlag;

        private string moveFlag;

        private string forceInactiveFlag;

        private bool particles;

        private bool AtStartOfTrack;

        private bool AtEndOfTrack;

        private bool Moving = true;

        private ParticleType P_Trail;

        public float noCollideDelay;

        private string Orientation;

        public Entity AttachedEntity;

        private Type attachedEntityType;

        private bool attachedEntityResolved;

        public Vector2 attachedEntityOffset;

        private string AttachedEntityPlatformsIndexes;

        private Vector2 OrigPosition;

        public JumpThruMovingPlatform(int id, Vector2 position, Vector2[] nodes, string mode, string directory, int length, string particlesColorA, string particlesColorB, string orientation, int amount, int index, float speedMult, float startOffset, float spacingOffset, string attachedEntityPlatformsIndexes, string stopFlag, string swapFlag, string moveFlag, string forceInactiveFlag, bool particles, int direction, float startPercent = -1f, bool swapped = false) : base(position, 8, false)
        {
            Tag = Tags.TransitionUpdate;
            noCollideDelay = 0.01f;
            Add(new Coroutine(CollideDelayRoutine()));
            Add(new LedgeBlocker());
            Collider = new Hitbox(length * 8, 8, -length * 8 / 2, -4);
            this.id = id;
            this.nodes = nodes;
            this.mode = mode;
            this.directory = directory;
            this.length = length;
            this.particlesColorA = particlesColorA;
            this.particlesColorB = particlesColorB;
            Orientation = orientation;
            this.amount = amount;
            this.index = index;
            this.speedMult = speedMult;
            this.startOffset = startOffset;
            this.spacingOffset = spacingOffset;
            AttachedEntityPlatformsIndexes = attachedEntityPlatformsIndexes;
            this.stopFlag = stopFlag;
            this.swapFlag = swapFlag;
            this.moveFlag = moveFlag;
            this.forceInactiveFlag = forceInactiveFlag;
            this.particles = particles;
            this.direction = direction;
            this.swapped = swapped;
            if (string.IsNullOrEmpty(this.directory))
            {
                this.directory = "objects/XaphanHelper/JumpThruMovingPlatform";
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
                    if (mode == "Restart")
                    {
                        percent = substract;
                    }
                    else
                    {
                        percent = 1 - substract;
                        this.direction = -direction;
                    }
                }
            }
            else
            {
                percent = startPercent;
            }
            percent %= 1f;
            OrigPosition = GetPercentPosition(0);
            Vector2 rawPosition = GetPercentPosition(percent);
            Position = new Vector2((float)Math.Round(rawPosition.X), (float)Math.Round(rawPosition.Y));
            sprites = BuildSprite();
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
            Depth = -100;
        }

        private List<Sprite> BuildSprite()
        {
            List<Sprite> list = new();
            for (int i = 0; i < length; i++)
            {
                Sprite sprite = new(GFX.Game, directory + "/");
                sprite.AddLoop("idle", "platform", 0f);
                sprite.CenterOrigin();
                sprite.Play("idle");
                sprite.FlipY = Orientation == "Bottom";
                sprite.Position.X = -length * 8 / 2 + i * 8 + 4;
                list.Add(sprite);
                Add(sprite);
            }
            return list;
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

            if (trackSfx != null)
            {
                PositionTrackSfx();
            }

            if (index == 1)
            {
                foreach (Entity entity in scene.Entities)
                {
                    if (!AttachableRegistry.TryGetValue(entity.GetType(), out AttachableInfo info))
                    {
                        continue;
                    }
                    if (!CollideCheck(entity, OrigPosition - Vector2.UnitY * 2))
                    {
                        continue;
                    }
                    if (!info.Validate(entity))
                    {
                        continue;
                    }

                    AttachedEntity = entity;
                    attachedEntityType = entity.GetType();
                    attachedEntityOffset = OrigPosition - entity.Position;
                    entity.RemoveSelf();
                    break;
                }
            }
        }

        public override void Update()
        {
            alpha += Engine.DeltaTime * 4f;
            base.Update();
            if (index >= 1 && !attachedEntityResolved)
            {
                foreach (JumpThruMovingPlatform platform in SceneAs<Level>().Tracker.GetEntities<JumpThruMovingPlatform>())
                {
                    if (platform.id == id && platform.index == 1 && platform.AttachedEntity != null)
                    {
                        attachedEntityType = platform.attachedEntityType;
                        attachedEntityOffset = platform.attachedEntityOffset;
                        if (AttachableRegistry.TryGetValue(attachedEntityType, out AttachableInfo info))
                        {
                            AttachedEntity = info.Clone(platform.AttachedEntity, this);
                            AttachedEntity.Depth = Depth + 1;
                        }
                    }
                    if (platform.id == id && (!string.IsNullOrEmpty(AttachedEntityPlatformsIndexes) ? AttachedEntityPlatformsIndexes.Split(',').ToList().Contains(index.ToString()) : true))
                    {
                        if (AttachedEntity != null)
                        {
                            SceneAs<Level>().Add(AttachedEntity);
                        }
                    }
                }
                attachedEntityResolved = true;
            }
            if ((Scene as Level).Transitioning)
            {
                if ((!string.IsNullOrEmpty(forceInactiveFlag) && SceneAs<Level>().Session.GetFlag(forceInactiveFlag)) || (!string.IsNullOrEmpty(stopFlag) && SceneAs<Level>().Session.GetFlag(stopFlag)) || AtStartOfTrack || AtEndOfTrack || !Moving)
                {
                    if (AttachedEntity != null)
                    {
                        AttachedEntity.Position = GetPercentPosition(percent) - attachedEntityOffset;
                    }
                    return;
                }
                if (index >= 1)
                {
                    if (AttachedEntity != null)
                    {
                        AttachedEntity.Position = GetPercentPosition(percent) - attachedEntityOffset;
                    }
                }
            }
            else
            {
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
                if ((!string.IsNullOrEmpty(forceInactiveFlag) && SceneAs<Level>().Session.GetFlag(forceInactiveFlag)) || (!string.IsNullOrEmpty(stopFlag) && SceneAs<Level>().Session.GetFlag(stopFlag)) || (!string.IsNullOrEmpty(moveFlag) && !SceneAs<Level>().Session.GetFlag(moveFlag) && mode != "Flag To Move") || AtStartOfTrack || AtEndOfTrack || !Moving)
                {
                    if (AttachedEntity != null)
                    {
                        AttachedEntity.Position = GetPercentPosition(percent) - attachedEntityOffset;
                    }
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
                            foreach (JumpThruMovingPlatform platform in SceneAs<Level>().Tracker.GetEntities<JumpThruMovingPlatform>())
                            {
                                if (platform.id == id && platform.index != 0)
                                {
                                    platform.AtStartOfTrack = true;
                                    platform.percent = (platform.index - 1) * platform.spacingOffset;
                                }
                            }
                        }
                        if (percent >= 1)
                        {
                            foreach (JumpThruMovingPlatform platform in SceneAs<Level>().Tracker.GetEntities<JumpThruMovingPlatform>())
                            {
                                if (platform.id == id && platform.index != 0)
                                {
                                    platform.AtEndOfTrack = true;
                                    platform.percent = 1 - (platform.amount - platform.index) * platform.spacingOffset;
                                }
                            }
                        }
                    }
                    else
                    {
                        if ((!string.IsNullOrEmpty(moveFlag) && !SceneAs<Level>().Session.GetFlag(moveFlag)))
                        {
                            return;
                        }
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
                                    if (mode.Contains("All Platforms"))
                                    {
                                        foreach (JumpThruMovingPlatform platform in SceneAs<Level>().Tracker.GetEntities<JumpThruMovingPlatform>())
                                        {
                                            if (platform.id == id && platform.index != 0)
                                            {
                                                platform.direction = 1;
                                                if (platform != this)
                                                {
                                                    platform.percent -= platform.speed * Engine.DeltaTime * 2;
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
                                    if (mode.Contains("All Platforms"))
                                    {
                                        foreach (JumpThruMovingPlatform platform in SceneAs<Level>().Tracker.GetEntities<JumpThruMovingPlatform>())
                                        {
                                            if (platform.id == id && platform.index != 0)
                                            {
                                                platform.direction = -1;
                                                platform.percent -= platform.speed * Engine.DeltaTime;
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
                }
                if (index >= 1)
                {
                    if (AttachedEntity != null)
                    {
                        AttachedEntity.Position = GetPercentPosition(percent) - attachedEntityOffset;
                    }
                }
                MoveTo(GetPercentPosition(percent));
                PositionTrackSfx();
                if (Scene.OnInterval(0.05f) && index != 0 && particles)
                {
                    SceneAs<Level>().ParticlesBG.Emit(P_Trail, 2, Center, Vector2.One * 3f);
                }
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

        public override void DebugRender(Camera camera)
        {
            if (index != 0)
            {
                base.DebugRender(camera);
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

        public override void Removed(Scene scene)
        {
            base.Removed(scene);
            AttachedEntity?.RemoveSelf();
        }
    }
}