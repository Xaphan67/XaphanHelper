using System;
using System.Collections;
using Celeste.Mod.Entities;
using Celeste.Mod.XaphanHelper.Managers;
using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste.Mod.XaphanHelper.Entities
{
    [Tracked(true)]
    [CustomEntity("XaphanHelper/Fuse")]
    class Fuse : Entity
    {
        [Tracked(true)]
        public class FuseSection : Entity
        {
            private Sprite Sprite;

            private Sprite ExplosionSprite;

            private string directory;

            private string tile;

            private Vector2 tilesSpritePos;

            public float speed;

            public bool triggered;

            public bool shouldTrigger;

            public FuseExplosionSoundManager manager;

            public FuseSection(EntityData data, Vector2 position) : base(position)
            {
                Collider = new Hitbox(8f, 8f);
                directory = data.Attr("directory");
                if (string.IsNullOrEmpty(directory))
                {
                    directory = "objects/XaphanHelper/Fuse";
                }
                Sprite = new Sprite(GFX.Game, directory + "/");
                Sprite.AddLoop("frame", "frame", 0.08f);
                Sprite.Play("frame");
                Add(ExplosionSprite = new Sprite(GFX.Game, directory + "/"));
                ExplosionSprite.Add("explode", "explode", 0.05f);
                ExplosionSprite.Position -= Vector2.One * 2;
                ExplosionSprite.OnLastFrame += onLastFrame;
                Depth = -19999;
            }

            private void onLastFrame(string obj)
            {
                RemoveSelf();
            }

            public override void Awake(Scene scene)
            {
                base.Awake(scene);
                bool N = false;
                bool S = false;
                bool E = false;
                bool W = false;
                bool None = false;
                if (Scene.CollideCheck<FuseSection>(new Rectangle((int)X, (int)Y - 8, 1, 1)))
                {
                    N = true;
                }
                if (Scene.CollideCheck<FuseSection>(new Rectangle((int)X, (int)Y + 8, 1, 1)))
                {
                    S = true;
                }
                if (Scene.CollideCheck<FuseSection>(new Rectangle((int)X + 8, (int)Y, 1, 1)))
                {
                    E = true;
                }
                if (Scene.CollideCheck<FuseSection>(new Rectangle((int)X - 8, (int)Y, 1, 1)))
                {
                    W = true;
                }
                None = !N && !S && !E && !W;
                tile = None ? "None" : (N ? "N" : "") + (S ? "S" : "") + (E ? "E" : "") + (W ? "W" : "");
                GetSpritePos();
            }

            public override void Update()
            {
                base.Update();
                if (manager == null)
                {
                    manager = SceneAs<Level>().Tracker.GetEntity<FuseExplosionSoundManager>();
                }
                if (!triggered && shouldTrigger)
                {
                    triggered = true;
                    shouldTrigger = false;
                    Add(new Coroutine(ExplodeRoutine()));
                }
            }

            private IEnumerator ExplodeRoutine()
            {
                if (manager.explosionSoundCooldown <= 0)
                {
                    Audio.Play("event:/game/xaphan/explosion", Position);
                    manager.explosionSoundCooldown = speed > 0.1f ? speed : 0.1f;
                }
                ExplosionSprite.Play("explode");
                Add(new Coroutine(DestroyEntitiesRoutine()));
                yield return speed;
                if (Scene.CollideCheck<FuseSection>(new Rectangle((int)X, (int)Y - 8, 1, 1)))
                {
                    FuseSection nextSection = CollideFirst<FuseSection>(Position - Vector2.UnitY);
                    if (nextSection != null && !nextSection.triggered)
                    {
                        nextSection.speed = speed;
                        nextSection.shouldTrigger = true;
                    }
                }
                if (Scene.CollideCheck<FuseSection>(new Rectangle((int)X, (int)Y + 8, 1, 1)))
                {
                    FuseSection nextSection = CollideFirst<FuseSection>(Position + Vector2.UnitY);
                    if (nextSection != null && !nextSection.triggered)
                    {
                        nextSection.speed = speed;
                        nextSection.shouldTrigger = true;
                    }
                }
                if (Scene.CollideCheck<FuseSection>(new Rectangle((int)X + 8, (int)Y, 1, 1)))
                {
                    FuseSection nextSection = CollideFirst<FuseSection>(Position + Vector2.UnitX);
                    if (nextSection != null && !nextSection.triggered)
                    {
                        nextSection.speed = speed;
                        nextSection.shouldTrigger = true;
                    }
                }
                if (Scene.CollideCheck<FuseSection>(new Rectangle((int)X - 8, (int)Y, 1, 1)))
                {
                    FuseSection nextSection = CollideFirst<FuseSection>(Position - Vector2.UnitX);
                    if (nextSection != null && !nextSection.triggered)
                    {
                        nextSection.speed = speed;
                        nextSection.shouldTrigger = true;
                    }
                }
            }

            private IEnumerator DestroyEntitiesRoutine()
            {
                while (ExplosionSprite.CurrentAnimationFrame < 3)
                {
                    yield return null;
                }
                FuseBreakBlock.FuseBreakBlockTile breakBlock = CollideFirst<FuseBreakBlock.FuseBreakBlockTile>(Position);
                if (breakBlock != null && !breakBlock.CollideCheck<SolidTiles>())
                {
                    breakBlock.RemoveSelf();
                }
            }

            public void GetSpritePos()
            {
                switch (tile)
                {
                    case "None":
                        tilesSpritePos = new Vector2(1, 3);
                        break;
                    case "N":
                        tilesSpritePos = new Vector2(3, 2);
                        break;
                    case "S":
                        tilesSpritePos = new Vector2(4, 2);
                        break;
                    case "E":
                        tilesSpritePos = new Vector2(4, 3);
                        break;
                    case "W":
                        tilesSpritePos = new Vector2(3, 3);
                        break;
                    case "SE":
                        tilesSpritePos = new Vector2(0, 0);
                        break;
                    case "SW":
                        tilesSpritePos = new Vector2(2, 0);
                        break;
                    case "NS":
                        tilesSpritePos = new Vector2(0, 1);
                        break;
                    case "NE":
                        tilesSpritePos = new Vector2(0, 2);
                        break;
                    case "EW":
                        tilesSpritePos = new Vector2(1, 2);
                        break;
                    case "NW":
                        tilesSpritePos = new Vector2(2, 2);
                        break;
                    case "NSE":
                        tilesSpritePos = new Vector2(3, 0);
                        break;
                    case "SEW":
                        tilesSpritePos = new Vector2(4, 0);
                        break;
                    case "NEW":
                        tilesSpritePos = new Vector2(3, 1);
                        break;
                    case "NSW":
                        tilesSpritePos = new Vector2(4, 1);
                        break;
                    case "NSEW":
                        tilesSpritePos = new Vector2(0, 3);
                        break;
                }
            }

            public override void Render()
            {
                base.Render();
                if (ExplosionSprite.CurrentAnimationFrame < 3)
                {
                    Sprite.RenderPosition = Position;
                    Sprite.DrawSubrect(Vector2.Zero, new Rectangle((int)tilesSpritePos.X * 8, (int)tilesSpritePos.Y * 8, 8, 8));
                }
                ExplosionSprite.Render();
            }
        }

        EntityData data;

        public Fuse(EntityData data, Vector2 position, EntityID ID) : base(data.Position + position)
        {
            Tag = Tags.TransitionUpdate;
            this.data = data;
        }

        public override void Added(Scene scene)
        {
            base.Added(scene);
            for (int x = 0; x < data.Width / 8; x++)
            {
                for (int y = 0; y < data.Height / 8; y++)
                {
                    scene.Add(new FuseSection(data, Position + new Vector2(x * 8, y * 8)));
                }
            }
        }        
    }
}
