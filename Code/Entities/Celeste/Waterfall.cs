using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using Celeste.Mod.Entities;
using Celeste.Mod.Helpers;
using Microsoft.Xna.Framework;
using Monocle;
using MonoMod.Cil;
using MonoMod.RuntimeDetour;

namespace Celeste.Mod.XaphanHelper.Entities
{
    [Tracked(true)]
    [CustomEntity("XaphanHelper/Waterfall")]
    class Waterfall : Entity
    {
        [Tracked(true)]
        public class WaterfallSection : Entity
        {
            private int Index;

            public float colliderHeight;

            public int verticalOffset;

            public int DrawSpriteIndex;

            public Waterfall Waterfall;

            public Sprite sectionSprite;

            public bool collideSolid;

            public bool collideLiquid;

            public int TileWidth => (int)sectionSprite.Width;

            public int RenderHeight => (int)Math.Truncate(Collider.Height + (collideSolid ? 4 : collideLiquid ? 1 : 0) + verticalOffset);

            public bool IsEdge;

            public bool CanBatchWith(WaterfallSection other)
            {
                return !IsEdge && !other.IsEdge && RenderHeight == other.RenderHeight && Y == other.Y;
            }

            public WaterfallSection(Vector2 position, Waterfall waterfall, int index) : base(position)
            {
                Tag = Tags.TransitionUpdate;
                Add(new PlayerCollider(OnCollide));
                Waterfall = waterfall;
                Index = index;
                Add(sectionSprite = new Sprite(GFX.Game, "objects/XaphanHelper/Waterfall/"));
                sectionSprite.AddLoop("waterfall", "waterfall", 0.03f);
                sectionSprite.AddLoop("edge", "edge", 0.03f);
                IsEdge = Index == 0 || Index == (waterfall.Width - 1);
                sectionSprite.Play(IsEdge ? "edge" : "waterfall");
                if (Index == 0 || Index == (waterfall.Width - 1))
                {
                    DrawSpriteIndex = 0;
                }
                else
                {
                    int remainder;
                    float div = Math.DivRem(Index, (int)sectionSprite.Width, out remainder);
                    DrawSpriteIndex = remainder;
                }
                Depth = Waterfall.Depth;
            }

            private void OnCollide(Player player)
            {
                if (XaphanModule.PlayerIsControllingRemoteDrone())
                {
                    Drone drone = SceneAs<Level>().Tracker.GetEntity<Drone>();
                    if (drone != null && !drone.dead && player != drone.FakePlayer)
                    {
                        Add(new Coroutine(drone.Destroy()));
                    }
                }
            }

            public override void Added(Scene scene)
            {
                base.Added(scene);
                sectionSprite.Color = Waterfall.CurrentColor * Waterfall.currentTransparency;
                Collider = new Hitbox(1, 1, 0f, 0f);
                while (CollideCheck<Solid>())
                {
                    Collider.Position.Y += 1f;
                    verticalOffset += 1;
                }
                if (verticalOffset >= Waterfall.Height)
                {
                    RemoveSelf();
                    return;
                }
                Waterfall.AdjustSectionCollider(this);
            }

            public override void Removed(Scene scene)
            {
                base.Removed(scene);
                if (Waterfall.Sections != null && Index < Waterfall.Sections.Length)
                {
                    Waterfall.Sections[Index] = null;
                }
            }

            public override void Update()
            {
                base.Update();
                sectionSprite.Color = Waterfall.CurrentColor * Waterfall.currentTransparency;
                double checkIndex = Index / 8f;
                double result = checkIndex - Math.Truncate(checkIndex);
                float height = Calc.Random.Next(4);
                if ((CollideCheck<WaterWheel>(Position + Vector2.UnitY) ? (result == 0.25f || result == 0.75f) : result == 0.5f) && CullHelper.IsRectangleVisible(X, Y, Width, Height))
                {
                    Vector2 position = new Vector2(X, Y + Collider.Height + height + verticalOffset);
                    if (SceneAs<Level>().IsInBounds(position))
                    {
                        SceneAs<Level>().Particles.Emit(Waterfall.P_Splash, 1, position, Vector2.UnitX * 4f, new Vector2(0f, -1f).Angle());
                    }
                }
            }

            public override void Render()
            {

            }

            public override void DebugRender(Camera camera)
            {

            }
        }

        private struct FloorEntry
        {
            public float Left, Right, Top, Bottom;
        }

        public string color;

        private string poisonedColor;

        private float outsideTransparency;

        private float insideTransparency;

        private float currentTransparency;

        private string purifyFlags;

        private float GradientTimer = 1f;

        public bool purified;

        private bool invertPurifyFlags;

        private static ILHook hookPlayerOrigWallJump = null;

        private CustomParticleSystem Particles;

        private bool PlayerEntered;

        private bool PlayerStartFall;

        public WaterfallSection[] Sections;

        private Dictionary<Entity, FloorEntry> WatchedFloorEntities = new Dictionary<Entity, FloorEntry>();

        public ParticleType P_Splash;

        public Color CurrentColor;

        private List<PlayerPlatform> CachedPlatforms = new List<PlayerPlatform>();

        private List<Spikes> CachedSpikes = new List<Spikes>();

        private FieldInfo PlayerVarJumpTimer = typeof(Player).GetField("varJumpTimer", BindingFlags.Instance | BindingFlags.NonPublic);

        public Waterfall(EntityData data, Vector2 position, EntityID eid) : base(data.Position + position)
        {
            Tag = Tags.TransitionUpdate;
            Collider = new Hitbox(data.Width, data.Height, 0f, 0f);
            Add(new PlayerCollider(OnCollide));
            color = data.Attr("color");
            if (string.IsNullOrEmpty(color))
            {
                color = "669CEE";
            }
            poisonedColor = data.Attr("poisonedColor", "4c9a42");
            outsideTransparency = data.Float("transparency", 0.65f);
            insideTransparency = data.Float("insideTransparency", outsideTransparency);
            purifyFlags = data.Attr("purifyFlags");
            invertPurifyFlags = data.Bool("invertPurifyFlags");
            if (outsideTransparency <= 0f)
            {
                outsideTransparency = 0.65f;
            }
            if (outsideTransparency >= 1f)
            {
                outsideTransparency = 1f;
            }
            if (insideTransparency <= 0f || insideTransparency >= outsideTransparency)
            {
                insideTransparency = outsideTransparency;
            }
            Depth = -1;
        }

        public static void Load()
        {
            IL.Celeste.Player.BeforeUpTransition += modVarJumpTimer;
            IL.Celeste.Player.HiccupJump += modVarJumpTimer;
            IL.Celeste.Player.Jump += modVarJumpTimer;
            IL.Celeste.Player.SuperJump += modVarJumpTimer;
            IL.Celeste.Player.SuperWallJump += modVarJumpTimer;
            IL.Celeste.Player.Bounce += modVarJumpTimer;
            IL.Celeste.Player.SuperBounce += modVarJumpTimer;
            IL.Celeste.Player.SideBounce += modVarJumpTimer;
            IL.Celeste.Player.Rebound += modVarJumpTimer;
            IL.Celeste.Player.StarFlyUpdate += modVarJumpTimer;
            IL.Celeste.Player.FinishFlingBird += modVarJumpTimer;

            hookPlayerOrigWallJump = new ILHook(typeof(Player).GetMethod("orig_WallJump", BindingFlags.NonPublic | BindingFlags.Instance), modVarJumpTimer);
        }

        public static void Unload()
        {
            IL.Celeste.Player.BeforeUpTransition -= modVarJumpTimer;
            IL.Celeste.Player.HiccupJump -= modVarJumpTimer;
            IL.Celeste.Player.Jump -= modVarJumpTimer;
            IL.Celeste.Player.SuperJump -= modVarJumpTimer;
            IL.Celeste.Player.SuperWallJump -= modVarJumpTimer;
            IL.Celeste.Player.Bounce -= modVarJumpTimer;
            IL.Celeste.Player.SuperBounce -= modVarJumpTimer;
            IL.Celeste.Player.SideBounce -= modVarJumpTimer;
            IL.Celeste.Player.Rebound -= modVarJumpTimer;
            IL.Celeste.Player.StarFlyUpdate -= modVarJumpTimer;
            IL.Celeste.Player.FinishFlingBird -= modVarJumpTimer;

            hookPlayerOrigWallJump?.Dispose();
            hookPlayerOrigWallJump = null;
        }

        private static void modVarJumpTimer(ILContext il)
        {
            ILCursor cursor = new ILCursor(il);

            while (cursor.TryGotoNext(instr => instr.MatchStfld<Player>("varJumpTimer")))
            {
                cursor.EmitDelegate<Func<float, float>>(orig => orig * (determineIfInWaterfall() ? 0.6f : 1f));
                cursor.Index++;
            }
        }

        private void OnCollide(Player player)
        {
            if (PlayerInside() && !string.IsNullOrEmpty(purifyFlags) && !purified)
            {
                player.Die(new Vector2(0f, -1f));
            }
        }

        public override void Added(Scene scene)
        {
            base.Added(scene);
            SceneAs<Level>().Add(Particles = new CustomParticleSystem(Depth, 1000));
            SceneAs<Level>().Particles.AddTag(Tags.TransitionUpdate);
            Add(new Coroutine(PoisonedRoutine()));
            purified = CheckIfPurified();
            if (purified)
            {
                GradientTimer = 0f;
            }
            CurrentColor = Utils.GetGradientColor(Calc.HexToColor(color), Calc.HexToColor(poisonedColor), GradientTimer);
            P_Splash = new ParticleType
            {
                Source = GFX.Game["particles/feather"],
                Color = CurrentColor,
                FadeMode = ParticleType.FadeModes.Late,
                Acceleration = new Vector2(0f, 20f),
                Size = 5f / 6f,
                SizeRange = 1f / 3f,
                ScaleOut = true,
                SpeedMin = 30f,
                SpeedMax = 24f,
                SpeedMultiplier = 0.98f,
                Direction = -(float)Math.PI / 2f,
                DirectionRange = 0.6981317f,
                RotationMode = ParticleType.RotationModes.Random,
                LifeMin = 0.35f,
                LifeMax = 0.2f
            };
            RefreshCaches();
            Sections = new WaterfallSection[(int)Width];
            for (int i = 0; i < Width; i++)
            {
                WaterfallSection section = new WaterfallSection(Position + Vector2.UnitX * i, this, i);
                Sections[i] = section;
                scene.Add(section);
            }
        }

        public override void Awake(Scene scene)
        {
            base.Awake(scene);
            Player player = SceneAs<Level>().Tracker.GetEntity<Player>();
            if (player != null)
            {
                if (!PlayerInside())
                {
                    currentTransparency = outsideTransparency;
                }
                else
                {
                    currentTransparency = insideTransparency;
                }
            }
        }

        public override void Update()
        {
            base.Update();
            Player player = SceneAs<Level>().Tracker.GetEntity<Player>();
            if (!SceneAs<Level>().Transitioning)
            {
                if (player != null)
                {
                    if (!PlayerInside())
                    {
                        currentTransparency = Calc.Approach(currentTransparency, outsideTransparency, Engine.DeltaTime * 2f);
                        if (PlayerEntered)
                        {
                            PlayerEntered = PlayerStartFall = false;
                        }
                    }
                    else
                    {
                        currentTransparency = Calc.Approach(currentTransparency, insideTransparency, Engine.DeltaTime * 2f);
                        if (!PlayerStartFall)
                        {
                            PlayerEntered = true;
                            PlayerVarJumpTimer.SetValue(player, 0);
                            if (player.Speed.Y >= 0)
                            {
                                PlayerStartFall = true;
                            }
                        }
                    }
                }
            }
            CurrentColor = Utils.GetGradientColor(Calc.HexToColor(color), Calc.HexToColor(poisonedColor), GradientTimer * 100);
            P_Splash.Color = CurrentColor * (currentTransparency + 0.2f);
            RefreshCaches();
            foreach (int index in DetectFloorChanges())
            {
                WaterfallSection section = Sections[index];
                if (section != null)
                {
                    AdjustSectionCollider(section);
                    section.collideSolid = section.CollideCheck<Solid>(section.Position + Vector2.UnitY);
                    section.collideLiquid = section.CollideCheck<Liquid>(section.Position + Vector2.UnitY);
                }
            }
        }

        private void RefreshCaches()
        {
            CachedPlatforms.Clear();
            foreach (PlayerPlatform platform in SceneAs<Level>().Tracker.GetEntities<PlayerPlatform>())
            {
                CachedPlatforms.Add(platform);
            }

            CachedSpikes.Clear();
            foreach (Spikes spikes in SceneAs<Level>().Tracker.GetEntities<Spikes>())
            {
                CachedSpikes.Add(spikes);
            }
        }

        public bool PlayerInside()
        {
            foreach (Player player in SceneAs<Level>().Tracker.GetEntities<Player>())
            {
                foreach (WaterfallSection section in Sections)
                {
                    if (section != null && section.CollideCheck(player) && player.Left <= section.Right - 4 && player.Right >= section.Left + 4)
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        public void AdjustSectionCollider(WaterfallSection section)
        {
            foreach (PlayerPlatform platform in CachedPlatforms)
            {
                platform.Collidable = false;
            }

            if ((section.CollideCheck<Solid>(section.Position + Vector2.UnitY) || section.CollideCheck<Liquid>(section.Position + Vector2.UnitY) || section.CollideCheck<WaterWheel>(section.Position + Vector2.UnitY)) && !section.CollideCheck<PlayerPlatform>())
            {
                while (section.CollideCheck<Solid>() || section.CollideCheck<Liquid>() || section.CollideCheck<WaterWheel>())
                {
                    section.Collider.Height -= 1;
                    section.colliderHeight = section.Collider.Height;
                }
            }
            else if (!section.CollideCheck<Solid>(section.Position + Vector2.UnitY) && !section.CollideCheck<Liquid>() && !section.CollideCheck<WaterWheel>())
            {
                while (!section.CollideCheck<Solid>(section.Position + Vector2.UnitY) && !section.CollideCheck<Liquid>() && !section.CollideCheck<WaterWheel>()
                    && section.Collider.Height < SceneAs<Level>().Bounds.Bottom - section.Top
                    && section.Collider.Height < Height - section.verticalOffset)
                {
                    section.Collider.Height += 1;
                    section.colliderHeight = section.Collider.Height;
                }
            }

            if (section.CollideCheck<PlayerPlatform>())
            {
                section.Collider.Height = section.colliderHeight;
            }

            foreach (PlayerPlatform platform in CachedPlatforms)
            {
                platform.RestoreCollisionForPlayer();
            }

            foreach (Spikes spikes in CachedSpikes)
            {
                if (section.CollideCheck(spikes))
                {
                    spikes.Depth = section.Depth - 1;
                }
                else if (spikes.Depth == section.Depth - 1)
                {
                    spikes.Depth = -1;
                }
            }
        }

        private HashSet<int> DetectFloorChanges()
        {
            HashSet<int> dirty = new HashSet<int>();
            Level level = SceneAs<Level>();
            Rectangle watchArea = new Rectangle((int)X - 4, (int)Y, (int)Width + 8, (int)Height + 32);
            HashSet<Entity> currentEntities = new HashSet<Entity>();

            void CheckEntities<T>() where T : Entity
            {
                foreach (T entity in level.Tracker.GetEntities<T>())
                {
                    float left = entity.Collider != null ? entity.Left : entity.X;
                    float right = entity.Collider != null ? entity.Right : entity.X + 1;
                    float top = entity.Collider != null ? entity.Top : entity.Y;
                    float bottom = entity.Collider != null ? entity.Bottom : entity.Y + 1;
                    Rectangle bounds = new Rectangle((int)left, (int)top, (int)Math.Max(1, right - left), (int)Math.Max(1, bottom - top));
                    if (!watchArea.Intersects(bounds))
                    {
                        if (WatchedFloorEntities.TryGetValue(entity, out FloorEntry leaving))
                        {
                            MarkSectionsInRange(leaving.Left, leaving.Right, dirty);
                            WatchedFloorEntities.Remove(entity);
                        }
                        continue;
                    }
                    currentEntities.Add(entity);
                    FloorEntry current = new FloorEntry { Left = left, Right = right, Top = top, Bottom = bottom };
                    if (!WatchedFloorEntities.TryGetValue(entity, out FloorEntry previous))
                    {
                        MarkSectionsInRange(left, right, dirty);
                    }
                    else if (previous.Left != current.Left || previous.Right != current.Right || previous.Top != current.Top || previous.Bottom != current.Bottom)
                    {
                        MarkSectionsInRange(Math.Min(previous.Left, current.Left), Math.Max(previous.Right, current.Right), dirty);
                    }

                    WatchedFloorEntities[entity] = current;
                }
            }
            CheckEntities<Solid>();
            CheckEntities<Liquid>();
            CheckEntities<WaterWheel>();
            CheckEntities<PlayerPlatform>();
            CheckEntities<Spikes>();
            List<Entity> goneEntities = null;
            foreach (KeyValuePair<Entity, FloorEntry> kv in WatchedFloorEntities)
            {
                if (!currentEntities.Contains(kv.Key))
                {
                    (goneEntities ??= new List<Entity>()).Add(kv.Key);
                    MarkSectionsInRange(kv.Value.Left, kv.Value.Right, dirty);
                }
            }
            if (goneEntities != null)
            {
                foreach (Entity entity in goneEntities)
                {
                    WatchedFloorEntities.Remove(entity);
                }
            }
            return dirty;
        }

        private void MarkSectionsInRange(float left, float right, HashSet<int> dirty)
        {
            int startIndex = Math.Max(0, (int)(left - X) - 1);
            int endIndex = Math.Min((int)Width - 1, (int)(right - X) + 1);
            for (int i = startIndex; i <= endIndex; i++)
            {
                dirty.Add(i);
            }
        }

        private bool CheckIfPurified()
        {
            if (string.IsNullOrEmpty(purifyFlags))
            {
                return true;
            }
            string[] flags = purifyFlags.Split(',');
            bool purified = true;
            foreach (string flag in flags)
            {
                if (invertPurifyFlags ? SceneAs<Level>().Session.GetFlag(flag) : !SceneAs<Level>().Session.GetFlag(flag))
                {
                    purified = false;
                    break;
                }
            }
            return purified;
        }

        public IEnumerator PoisonedRoutine()
        {
            if (!string.IsNullOrEmpty(purifyFlags))
            {
                bool skip = false;
                if (purified)
                {
                    GradientTimer = 0f;
                }
                else
                {
                    while (!CheckIfPurified())
                    {
                        yield return null;
                    }
                    while (GradientTimer > 0f)
                    {
                        GradientTimer -= Engine.DeltaTime;
                        yield return null;
                        if (GradientTimer <= 0.5f)
                        {
                            purified = true;
                        }
                        if (SceneAs<Level>().Transitioning || !CheckIfPurified())
                        {
                            skip = true;
                            break;
                        }
                    }
                    if (!skip)
                    {
                        GradientTimer = 0f;
                    }
                }

                while (CheckIfPurified())
                {
                    yield return null;
                }
                skip = false;
                while (GradientTimer < 1f)
                {
                    GradientTimer += Engine.DeltaTime;
                    yield return null;
                    if (GradientTimer >= 0.5f)
                    {
                        purified = false;
                    }
                    if (SceneAs<Level>().Transitioning || CheckIfPurified())
                    {
                        skip = true;
                        break;
                    }
                }
                if (!skip)
                {
                    GradientTimer = 1f;
                }
                Add(new Coroutine(PoisonedRoutine()));
            }
        }

        public static bool determineIfInWaterfall()
        {
            if (Engine.Scene is Level level)
            {
                HashSet<Waterfall> checkedWaterfalls = new HashSet<Waterfall>();
                foreach (WaterfallSection section in level.Tracker.GetEntities<WaterfallSection>())
                {
                    if (checkedWaterfalls.Add(section.Waterfall) && section.Waterfall.PlayerInside())
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        public override void Render()
        {
            base.Render();

            Level level = SceneAs<Level>();
            Camera camera = level?.Camera;
            if (camera == null || Sections == null || Sections.Length == 0)
            {
                return;
            }

            int width = Sections.Length;
            int i = 0;

            while (i < width)
            {
                WaterfallSection section = Sections[i];
                if (section == null)
                {
                    i++;
                    continue;
                }
                int tileWidth = section.TileWidth;
                int runEnd = i;
                while (runEnd + 1 < width
                    && (i / tileWidth) == ((runEnd + 1) / tileWidth)
                    && Sections[runEnd + 1] != null
                    && Sections[runEnd + 1].CanBatchWith(section))
                {
                    runEnd++;
                }
                int span = runEnd - i + 1;
                int totalHeight = section.RenderHeight;

                if (totalHeight > 0)
                {
                    int firstVisible = Math.Max(0, (int)(camera.Top - section.Y));
                    int lastVisible = Math.Min(totalHeight, (int)(camera.Bottom - section.Y) + 1);

                    for (int row = firstVisible; row < lastVisible; row++)
                    {
                        section.sectionSprite.RenderPosition = section.Position + Vector2.UnitY * row;
                        section.sectionSprite.DrawSubrect(Vector2.Zero, new Rectangle(section.DrawSpriteIndex, row % 16, span, 1));
                    }
                }

                i = runEnd + 1;
            }
        }

        public override void Removed(Scene scene)
        {
            base.Removed(scene);
            Particles.RemoveSelf();
        }
    }
}
