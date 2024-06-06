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
    class SolidMovingPlatform : Solid
    {
        private static FieldInfo SpikesSpikeType = typeof(Spikes).GetField("overrideType", BindingFlags.Instance | BindingFlags.NonPublic);

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

        public string Orientation;

        private bool AttachedEntity;

        public Spikes AttachedSpike;

        public MagneticCeiling AttachedMagneticCeiling;

        public Lever AttachedLever;

        public Vector2 attachedEntityOffset;

        private string AttachedEntityPlatformsIndexes;

        private Vector2 OrigPosition;

        public SolidMovingPlatform(int id, Vector2 position, Vector2[] nodes, string mode, string directory, int length, string particlesColorA, string particlesColorB, string orientation, int amount, int index, float speedMult, float startOffset, float spacingOffset, string attachedEntityPlatformsIndexes, string stopFlag, string swapFlag, string moveFlag, string forceInactiveFlag, bool particles, int direction, float startPercent = -1f, bool swapped = false) : base(position, 8, 8, false)
        {
            Tag = Tags.TransitionUpdate;
            noCollideDelay = 0.01f;
            Add(new Coroutine(CollideDelayRoutine()));
            Add(new LedgeBlocker());
            Collider = orientation == "Bottom" ? new Hitbox(length * 8, 8, -length * 8 / 2, -4) : new Hitbox(8, length * 8, -4, -length * 8 / 2);
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
                this.directory = "objects/XaphanHelper/VerticalMovingPlatform";
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
            OrigPosition = GetPercentPosition(0);
            Position = GetPercentPosition(percent);
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
                if (Orientation == "Bottom")
                {
                    sprite.FlipY = Orientation == "Bottom";
                    sprite.Position.X = -length * 8 / 2 + i * 8 + 4;
                }
                else
                {
                    sprite.Rotation = Orientation == "Left" ? -(float)Math.PI / 2 : (float)Math.PI / 2;
                    if (Orientation == "Left")
                    {
                        sprite.FlipX = true;
                    }
                    sprite.Position.Y = -length * 8 / 2 + i * 8 + 4;
                }
                list.Add(sprite);
                Add(sprite);
            }
            return list;
        }

        public static void Load()
        {
            On.Celeste.Player.Update += onPlayerUpdate;
        }

        public static void Unload()
        {
            On.Celeste.Player.Update -= onPlayerUpdate;
        }

        private static void onPlayerUpdate(On.Celeste.Player.orig_Update orig, Player self)
        {
            List<Entity> platforms = self.Scene.Tracker.GetEntities<SolidMovingPlatform>().ToList();
            foreach (SolidMovingPlatform platform in platforms)
            {
                if (((platform.Orientation == "Left" && self.Right <= platform.Left) || (platform.Orientation == "Right" && self.Left >= platform.Right) && self.Bottom >= platform.Top + 1 && self.Top <= platform.Bottom) || (platform.Orientation == "Bottom" && self.Top >= platform.Bottom))
                {
                    platform.Collidable = true;
                }
                else
                {
                    platform.Collidable = false;
                }
            }
            orig(self);
            foreach (SolidMovingPlatform platform in platforms)
            {
                if (((platform.Orientation == "Left" && self.Right <= platform.Left) || (platform.Orientation == "Right" && self.Left >= platform.Right) && self.Bottom >= platform.Top + 1 && self.Top <= platform.Bottom) || (platform.Orientation == "Bottom" && self.Top >= platform.Bottom))
                {
                    platform.Collidable = true;
                }
                else
                {
                    platform.Collidable = false;
                }
            }
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
                //trackSfx.Play("event:/env/local/09_core/fireballs_idle");
            }

            if (index == 1)
            {
                if (Orientation == "Left")
                {
                    AttachedSpike = CollideFirst<Spikes>(OrigPosition - Vector2.UnitX * 2);
                    AttachedLever = CollideFirst<Lever>(OrigPosition - Vector2.UnitX * 2);
                }
                else if (Orientation == "Right")
                {
                    AttachedSpike = CollideFirst<Spikes>(OrigPosition + Vector2.UnitX * 2);
                    AttachedLever = CollideFirst<Lever>(OrigPosition + Vector2.UnitX * 2);
                }
                else if (Orientation == "Bottom")
                {
                    AttachedSpike = CollideFirst<Spikes>(OrigPosition + Vector2.UnitY * 2);
                    AttachedLever = CollideFirst<Lever>(OrigPosition + Vector2.UnitY * 2);
                    AttachedMagneticCeiling = CollideFirst<MagneticCeiling>(OrigPosition + Vector2.UnitY * 2);
                }
                if (AttachedSpike != null)
                {
                    attachedEntityOffset = OrigPosition - AttachedSpike.Position;
                    foreach (Spikes spike in SceneAs<Level>().Tracker.GetEntities<Spikes>())
                    {
                        if (spike == AttachedSpike)
                        {
                            spike.RemoveSelf();
                        }
                    }
                }
                if (AttachedMagneticCeiling != null)
                {
                    attachedEntityOffset = OrigPosition - AttachedMagneticCeiling.Position;
                    foreach (MagneticCeiling ceiling in SceneAs<Level>().Tracker.GetEntities<MagneticCeiling>())
                    {
                        if (ceiling == AttachedMagneticCeiling)
                        {
                            ceiling.RemoveSelf();
                        }
                    }
                }
                if (AttachedLever != null)
                {
                    attachedEntityOffset = OrigPosition - AttachedLever.Position;
                    foreach (Lever lever in SceneAs<Level>().Tracker.GetEntities<Lever>())
                    {
                        if (lever == AttachedLever)
                        {
                            lever.RemoveSelf();
                        }
                    }
                }
            }
        }

        public override void Update()
        {
            alpha += Engine.DeltaTime * 4f;
            if ((Scene as Level).Transitioning)
            {
                PositionTrackSfx();
                return;
            }
            base.Update();
            if (index >= 1 && !AttachedEntity)
            {
                foreach (SolidMovingPlatform platform in SceneAs<Level>().Tracker.GetEntities<SolidMovingPlatform>())
                {
                    if (platform.id == id && platform.index == 1)
                    {
                        if (platform.AttachedSpike != null)
                        {
                            AttachedSpike = new Spikes(platform.Position - platform.attachedEntityOffset, length * 8, platform.AttachedSpike.Direction, (string)SpikesSpikeType.GetValue(platform.AttachedSpike));
                            AttachedSpike.Depth = Depth + 1;
                        }
                        else if (platform.AttachedMagneticCeiling != null)
                        {
                            AttachedMagneticCeiling = new MagneticCeiling(platform.Position - platform.attachedEntityOffset, Vector2.Zero, platform.AttachedMagneticCeiling.ID, platform.AttachedMagneticCeiling.Width, platform.AttachedMagneticCeiling.Directory, platform.AttachedMagneticCeiling.AnimationSpeed, platform.AttachedMagneticCeiling.CanJump, platform.AttachedMagneticCeiling.NoStaminaDrain);
                        }
                        else if (platform.AttachedLever != null)
                        {
                            AttachedLever = new Lever(platform.Position - platform.attachedEntityOffset, AttachedLever.Directory, AttachedLever.Flag, AttachedLever.CanSwapFlag, AttachedLever.Side);
                            AttachedLever.Depth = 1;
                        }
                        attachedEntityOffset = platform.attachedEntityOffset;
                    }
                    if (platform.id == id && (!string.IsNullOrEmpty(AttachedEntityPlatformsIndexes) ? AttachedEntityPlatformsIndexes.Split(',').ToList().Contains(index.ToString()) : true))
                    {
                        if (AttachedSpike != null)
                        {
                            SceneAs<Level>().Add(AttachedSpike);
                        }
                        else if (AttachedMagneticCeiling != null)
                        {
                            SceneAs<Level>().Add(AttachedMagneticCeiling);
                        }
                        else if (AttachedLever != null)
                        {
                            SceneAs<Level>().Add(AttachedLever);
                        }
                    }
                }
                AttachedEntity = true;
            }

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
                        foreach (SolidMovingPlatform platform in SceneAs<Level>().Tracker.GetEntities<SolidMovingPlatform>())
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
                        foreach (SolidMovingPlatform platform in SceneAs<Level>().Tracker.GetEntities<SolidMovingPlatform>())
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
                                    foreach (SolidMovingPlatform platform in SceneAs<Level>().Tracker.GetEntities<SolidMovingPlatform>())
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
                                    foreach (SolidMovingPlatform platform in SceneAs<Level>().Tracker.GetEntities<SolidMovingPlatform>())
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
                if (AttachedSpike != null)
                {
                    AttachedSpike.Position = GetPercentPosition(percent) - attachedEntityOffset;
                }
                else if (AttachedMagneticCeiling != null)
                {
                    AttachedMagneticCeiling.Position = GetPercentPosition(percent) - attachedEntityOffset;
                }
                else if (AttachedLever != null)
                {
                    AttachedLever.Position = GetPercentPosition(percent) - attachedEntityOffset;
                }
            }
            MoveTo(GetPercentPosition(percent));
            if (AttachedMagneticCeiling != null)
            {
                AttachedMagneticCeiling.Top = Bottom;
            }
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
            if (AttachedSpike != null)
            {
                AttachedSpike.RemoveSelf();
            }
            if (AttachedMagneticCeiling != null)
            {
                AttachedMagneticCeiling.RemoveSelf();
            }
            if (AttachedLever != null)
            {
                AttachedLever.RemoveSelf();
            }
        }
    }
}
