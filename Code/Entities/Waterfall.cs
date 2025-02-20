using System;
using System.Collections;
using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;
using Monocle;

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

            private Waterfall Waterfall;

            private Sprite sectionSprite;

            public WaterfallSection(Vector2 position, Waterfall waterfall, int index) : base(position)
            {
                Tag = Tags.TransitionUpdate;
                Collider = new Hitbox(1, waterfall.Height, 0f, 0f);
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
                Add(new PlayerCollider(OnCollide));
                Depth = Waterfall.Depth;
            }

            public override void Added(Scene scene)
            {
                base.Added(scene);
                sectionSprite.Color = Utils.GetGradientColor(Calc.HexToColor(Waterfall.color), Calc.HexToColor(Waterfall.poisonedColor), Waterfall.GradientTimer) * 0.65f/*(PlayerCompletelyInside() ? insideTransparency : outsideTransparency)*/;
            }

            public override void Update()
            {
                base.Update();
                foreach (PlayerPlatform plateform in SceneAs<Level>().Tracker.GetEntities<PlayerPlatform>())
                {
                    plateform.Collidable = false;
                }
                base.Update();  
                if ((CollideCheck<Solid>() || CollideCheck<Liquid>()) && !CollideCheck<PlayerPlatform>())
                {
                    while (CollideCheck<Solid>() || CollideCheck<Liquid>())
                    {
                        Collider.Height -= 1;
                        colliderHeight = Collider.Height;
                    }
                }
                else
                {
                    if (!CollideCheck<Solid>(Position + Vector2.UnitY) && !CollideCheck<Liquid>(Position + Vector2.UnitY))
                    {
                        while ((!CollideCheck<Solid>(Position + Vector2.UnitY) && !CollideCheck<Liquid>(Position + Vector2.UnitY)) && Collider.Height < SceneAs<Level>().Bounds.Bottom - Top && Collider.Height < Waterfall.Height)
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
                sectionSprite.Color = Utils.GetGradientColor(Calc.HexToColor(Waterfall.color), Calc.HexToColor(Waterfall.poisonedColor), Waterfall.GradientTimer * 100) * 0.65f /*currentTransparency*/;
            }

            private void OnCollide(Player player)
            {
                if (Waterfall.poisoned && !Waterfall.purified)
                {
                    player.Die(new Vector2(0f, -1f));
                }
            }

            public override void Render()
            {
                int section = 0;
                bool collideSolid = Scene.CollideCheck<Solid>(new Vector2(Position.X, Position.Y + Height));
                for (int i = 0; i < Math.Truncate(Collider.Height + (collideSolid ? 4 : 0)) ; i++)
                {
                    sectionSprite.RenderPosition = Position + Vector2.UnitY * i;
                    sectionSprite.DrawSubrect(Vector2.Zero, new Rectangle(DrawSpriteIndex, section, 1, 1));
                    section += 1;
                    if (section > 15)
                    {
                        section = 0;
                    }
                }
            }
        }

        public string color;

        private string poisonedColor;

        private string purifyFlags;

        private bool poisoned;

        private float GradientTimer = 1f;

        private bool purified;

        private bool invertPurifyFlags;

        public Waterfall(EntityData data, Vector2 position, EntityID eid) : base(data.Position + position)
        {
            Tag = Tags.TransitionUpdate;
            Collider = new Hitbox(data.Width, data.Height, 0f, 0f);
            poisoned = data.Bool("poisoned", false);
            color = data.Attr("color");
            if (string.IsNullOrEmpty(color))
            {
                color = "669CEE";
            }
            poisonedColor = poisoned ? data.Attr("poisonedColor", "4c9a42") : color;
            purifyFlags = data.Attr("purifyFlags");
            invertPurifyFlags = data.Bool("invertPurifyFlags");
            Depth = -1;
        }

        public override void Added(Scene scene)
        {
            base.Added(scene);
            Add(new Coroutine(PoisonedRoutine()));
            purified = CheckIfPurified();
            for (int i = 0; i < Width; i++)
            {
                scene.Add(new WaterfallSection(Position + Vector2.UnitX * i, this, i));
            }
        }

        private bool CheckIfPurified()
        {
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
    }
}
