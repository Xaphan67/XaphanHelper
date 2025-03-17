using System;
using System.Collections;
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

            private int DrawSpriteIndex;

            private float colliderHeight;

            private int verticalOffset;

            public Waterfall Waterfall;

            private Sprite sectionSprite;

            private ParticleType P_Splash;

            private bool collideSolid;

            private bool collideLiquid;

            public WaterfallSection(Vector2 position, Waterfall waterfall, int index) : base(position)
            {
                Tag = Tags.TransitionUpdate;
                Waterfall = waterfall;
                Index = index;
                Add(sectionSprite = new Sprite(GFX.Game, "objects/XaphanHelper/Waterfall/"));
                sectionSprite.AddLoop("waterfall", "waterfall", 0.03f);
                sectionSprite.AddLoop("edge", "edge", 0.03f);
                sectionSprite.Play((Index == 0 || Index == (waterfall.Width - 1)) ? "edge" : "waterfall");
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
                P_Splash = new ParticleType
                {
                    Source = GFX.Game["particles/feather"],
                    Color = Utils.GetGradientColor(Calc.HexToColor(Waterfall.color), Calc.HexToColor(Waterfall.poisonedColor), Waterfall.GradientTimer),
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
                Depth = Waterfall.Depth;
            }

            public override void Added(Scene scene)
            {
                base.Added(scene);
                sectionSprite.Color = Utils.GetGradientColor(Calc.HexToColor(Waterfall.color), Calc.HexToColor(Waterfall.poisonedColor), Waterfall.GradientTimer) * Waterfall.currentTransparency;
                Collider = new Hitbox(1, 1, 0f, 0f);
                while (CollideCheck<Solid>())
                {
                    Collider.Position.Y += 1f;
                    verticalOffset += 1;
                }
                if (verticalOffset >= Waterfall.Height)
                {
                    RemoveSelf();
                }
                AdjustColliderSize();
            }

            public void AdjustColliderSize()
            {
                foreach (PlayerPlatform plateform in SceneAs<Level>().Tracker.GetEntities<PlayerPlatform>())
                {
                    plateform.Collidable = false;
                }
                if ((CollideCheck<Solid>(Position + Vector2.UnitY) || CollideCheck<Liquid>(Position + Vector2.UnitY) || CollideCheck<WaterWheel>(Position + Vector2.UnitY)) && !CollideCheck<PlayerPlatform>())
                {
                    while (CollideCheck<Solid>() || CollideCheck<Liquid>() || CollideCheck<WaterWheel>())
                    {
                        Collider.Height -= 1;
                        colliderHeight = Collider.Height;
                    }
                }
                else
                {
                    if (!CollideCheck<Solid>(Position + Vector2.UnitY) && !CollideCheck<Liquid>() && !CollideCheck<WaterWheel>())
                    {
                        while ((!CollideCheck<Solid>(Position + Vector2.UnitY) && !CollideCheck<Liquid>() && !CollideCheck<WaterWheel>()) && Collider.Height < SceneAs<Level>().Bounds.Bottom - Top && Collider.Height < Waterfall.Height - verticalOffset)
                        {
                            Collider.Height += 1;
                            colliderHeight = Collider.Height;
                        }
                    }
                }
                if (CollideCheck<PlayerPlatform>())
                {
                    Collider.Height = colliderHeight;
                }
                foreach (PlayerPlatform plateform in SceneAs<Level>().Tracker.GetEntities<PlayerPlatform>())
                {
                    plateform.RestoreCollisionForPlayer();
                }
            }

            public override void Update()
            {
                base.Update();
                AdjustColliderSize();
                P_Splash.Color = Utils.GetGradientColor(Calc.HexToColor(Waterfall.color), Calc.HexToColor(Waterfall.poisonedColor), Waterfall.GradientTimer * 100) * (Waterfall.currentTransparency + 0.2f);
                sectionSprite.Color = Utils.GetGradientColor(Calc.HexToColor(Waterfall.color), Calc.HexToColor(Waterfall.poisonedColor), Waterfall.GradientTimer * 100) * Waterfall.currentTransparency;
                double checkIndex = Index / 8f;
                double result = checkIndex - Math.Truncate(checkIndex);
                float height = Calc.Random.Next(4);
                if ((CollideCheck<WaterWheel>(Position + Vector2.UnitY) ? (result == 0.25f || result == 0.75f) : result == 0.5f) && CullHelper.IsRectangleVisible(X, Y, Width, Height))
                {
                    Vector2 position = new Vector2(X, Y + Collider.Height + height + verticalOffset);
                    if (SceneAs<Level>().IsInBounds(position))
                    {
                        SceneAs<Level>().Particles.Emit(P_Splash, 1, position, Vector2.UnitX * 4f, new Vector2(0f, -1f).Angle());
                    }
                }
                collideSolid = CollideCheck<Solid>(Position + Vector2.UnitY);
                collideLiquid = CollideCheck<Solid>(Position + Vector2.UnitY);
            }

            public override void Render()
            {
                int section = 0;
                //bool collideSolid = Scene.CollideCheck<Solid>(new Vector2(Position.X, Position.Y + Collider.Height + verticalOffset + 1));
                //bool collideLiquid = Scene.CollideCheck<Liquid>(new Vector2(Position.X, Position.Y + Collider.Height + verticalOffset + 1));
                for (int i = 0; i < Math.Truncate(Collider.Height + (collideSolid ? 4 : collideLiquid ? 1 : 0) + verticalOffset); i++)
                {
                    sectionSprite.RenderPosition = Position + Vector2.UnitY * i;
                    if (CullHelper.IsRectangleVisible(sectionSprite.RenderPosition.X, sectionSprite.RenderPosition.Y, 1, 1))
                    {
                        sectionSprite.DrawSubrect(Vector2.Zero, new Rectangle(DrawSpriteIndex, section, 1, 1));
                    }
                    section += 1;
                    if (section > 15)
                    {
                        section = 0;
                    }
                }
            }

            public override void DebugRender(Camera camera)
            {
                
            }
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
            if (PlayerInside() && ((!string.IsNullOrEmpty(purifyFlags) && !purified) || XaphanModule.PlayerIsControllingRemoteDrone()))
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
            for (int i = 0; i < Width; i++)
            {
                scene.Add(new WaterfallSection(Position + Vector2.UnitX * i, this, i));
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
        }

        public bool PlayerInside()
        {
            foreach (Player player in SceneAs<Level>().Tracker.GetEntities<Player>())
            {
                foreach (WaterfallSection waterfall in SceneAs<Level>().Tracker.GetEntities<WaterfallSection>())
                {
                    if (waterfall.Waterfall == this)
                    {
                        if (waterfall.CollideCheck(player) && player.Left <= waterfall.Right - 4 && player.Right >= waterfall.Left + 4)
                        {
                            return true;
                        }
                    }
                }
            }
            return false;
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
            if (Engine.Scene is Level)
            {
                Level level = (Level)Engine.Scene;
                foreach (WaterfallSection waterfall in level.Tracker.GetEntities<WaterfallSection>())
                {
                    if (waterfall.Waterfall.PlayerInside())
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        public override void Removed(Scene scene)
        {
            base.Removed(scene);
            Particles.RemoveSelf();
        }
    }
}
