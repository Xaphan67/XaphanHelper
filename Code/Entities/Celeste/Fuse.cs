using System.Collections;
using System.Linq;
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
            public bool ConnectedN;

            public bool ConnectedS;

            public bool ConnectedE;

            public bool ConnectedW;

            private Sprite Sprite;

            private Sprite ExplosionSprite;

            private string directory;

            private string tile;

            private Vector2 tilesSpritePos;

            public bool triggered;

            public FuseExplosionSoundManager manager;

            public int ID;

            private int col;

            private int row;

            private int tileWidth;

            private int tileHeight;

            public FuseSection(EntityData data, Vector2 position, int id, int col, int row, int tileWidth, int tileHeight) : base(position)
            {
                Tag = Tags.TransitionUpdate;
                ID = id;
                this.col = col;
                this.row = row;
                this.tileWidth = tileWidth;
                this.tileHeight = tileHeight;
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
                ExplosionSprite.Add("explode", "explode", 0.04f);
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
                ConnectedN = HasAdjacentThinSide(new Vector2(X, Y - 8), "x");
                ConnectedS = HasAdjacentThinSide(new Vector2(X, Y + 8), "x");
                ConnectedE = HasAdjacentThinSide(new Vector2(X + 8, Y), "y");
                ConnectedW = HasAdjacentThinSide(new Vector2(X - 8, Y), "y");
                bool None = !ConnectedN && !ConnectedS && !ConnectedE && !ConnectedW;
                tile = None ? "None" : (ConnectedN ? "N" : "") + (ConnectedS ? "S" : "") + (ConnectedE ? "E" : "") + (ConnectedW ? "W" : "");
                GetSpritePos();
            }

            private bool HasAdjacentThinSide(Vector2 cellPos, string axis)
            {
                float selfX = X - col * 8;
                float selfY = Y - row * 8;
                float selfWidth = tileWidth * 8;
                float selfHeight = tileHeight * 8;

                Rectangle cellRect = new Rectangle((int)cellPos.X, (int)cellPos.Y, 1, 1);

                FuseSection other = Scene.CollideFirst<FuseSection>(cellRect);
                if (other != null)
                {
                    float ox = other.X - other.col * 8;
                    float oy = other.Y - other.row * 8;
                    float ow = other.tileWidth * 8;
                    float oh = other.tileHeight * 8;

                    float overlapLength;
                    if (axis == "y")
                    {
                        float top = System.Math.Max(selfY, oy);
                        float bottom = System.Math.Min(selfY + selfHeight, oy + oh);
                        overlapLength = bottom - top;
                    }
                    else
                    {
                        float left = System.Math.Max(selfX, ox);
                        float right = System.Math.Min(selfX + selfWidth, ox + ow);
                        overlapLength = right - left;
                    }

                    if (overlapLength <= 8f)
                    {
                        return true;
                    }
                }

                return Scene.CollideCheck<Detonator>(cellRect);
            }

            public IEnumerator ExplodeRoutine(float speed, string flag = null, bool registerInSaveData = false)
            {
                if (manager == null)
                {
                    if (SceneAs<Level>().Tracker.GetEntities<FuseExplosionSoundManager>().Count() == 0)
                    {
                        SceneAs<Level>().Add(new FuseExplosionSoundManager());
                        yield return null;
                    }
                    manager = SceneAs<Level>().Tracker.GetEntity<FuseExplosionSoundManager>();
                }
                triggered = true;
                if (!manager.CooldownRoutine.Active)
                {
                    Audio.Play("event:/game/xaphan/fuse_explosion", Position);
                    manager.SetCooldown(speed > 0.1f ? speed : 0.1f);
                    yield return null;
                }
                ExplosionSprite.Play("explode");
                ExplosionSprite.Rate *= speed <= 0.05f ? 4f : speed <= 0.1f ? 3f : speed <= 0.15f ? 2f : 1f;
                Add(new Coroutine(DestroyEntitiesRoutine()));
                float waitTimer = speed;
                while (waitTimer > 0)
                {
                    waitTimer -= Engine.DeltaTime * 2;
                    yield return null;
                }
                bool isEnd = true;
                if (ConnectedN && CollideCheck<FuseSection>(Position - Vector2.UnitY))
                {
                    FuseSection nextSection = CollideFirst<FuseSection>(Position - Vector2.UnitY);
                    if (nextSection != null && !nextSection.triggered)
                    {
                        nextSection.Add(new Coroutine(nextSection.ExplodeRoutine(speed, flag, registerInSaveData)));
                        isEnd = false;
                    }
                }
                if (ConnectedS && CollideCheck<FuseSection>(Position + Vector2.UnitY))
                {
                    FuseSection nextSection = CollideFirst<FuseSection>(Position + Vector2.UnitY);
                    if (nextSection != null && !nextSection.triggered)
                    {
                        nextSection.Add(new Coroutine(nextSection.ExplodeRoutine(speed, flag, registerInSaveData)));
                        isEnd = false;

                    }
                }
                if (ConnectedE && CollideCheck<FuseSection>(Position + Vector2.UnitX))
                {
                    FuseSection nextSection = CollideFirst<FuseSection>(Position + Vector2.UnitX);
                    if (nextSection != null && !nextSection.triggered)
                    {
                        nextSection.Add(new Coroutine(nextSection.ExplodeRoutine(speed, flag, registerInSaveData)));
                        isEnd = false;

                    }
                }
                if (ConnectedW && CollideCheck<FuseSection>(Position - Vector2.UnitX))
                {
                    FuseSection nextSection = CollideFirst<FuseSection>(Position - Vector2.UnitX);
                    if (nextSection != null && !nextSection.triggered)
                    {
                        nextSection.Add(new Coroutine(nextSection.ExplodeRoutine(speed, flag, registerInSaveData)));
                        isEnd = false;

                    }
                }
                if (isEnd && flag != null && CollideCheck<FlagBlock>(Position))
                {
                    SceneAs<Level>().Session.SetFlag(flag);
                    if (registerInSaveData)
                    {
                        string Prefix = SceneAs<Level>().Session.Area.LevelSet;
                        int chapterIndex = SceneAs<Level>().Session.Area.ChapterIndex;
                        if (!XaphanModule.ModSaveData.SavedFlags.Contains(Prefix + "_Ch" + chapterIndex + "_" + flag))
                        {
                            XaphanModule.ModSaveData.SavedFlags.Add(Prefix + "_Ch" + chapterIndex + "_" + flag);
                        }
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

        private EntityData data;

        private int ID;

        public Fuse(EntityData data, Vector2 position, EntityID ID) : base(data.Position + position)
        {
            Tag = Tags.TransitionUpdate;
            Collider = new Hitbox(data.Width, data.Height);
            this.ID = ID.ID;
            this.data = data;
        }

        public override void Added(Scene scene)
        {
            base.Added(scene);
            int tileWidth = (int)System.Math.Ceiling(data.Width / 8f);
            int tileHeight = (int)System.Math.Ceiling(data.Height / 8f);
            for (int x = 0; x < data.Width / 8; x++)
            {
                for (int y = 0; y < data.Height / 8; y++)
                {
                    scene.Add(new FuseSection(data, Position + new Vector2(x * 8, y * 8), ID, x, y, tileWidth, tileHeight));
                }
            }
        }

        public override void Removed(Scene scene)
        {
            foreach (FuseSection section in SceneAs<Level>().Tracker.GetEntities<FuseSection>())
            {
                if (section.ID == ID)
                {
                    section.RemoveSelf();
                }
            }
            if (CollideCheck<Fuse>(Position - Vector2.UnitY))
            {
                Fuse nextFuse = CollideFirst<Fuse>(Position - Vector2.UnitY);
                if (nextFuse != null)
                {
                    nextFuse.RemoveSelf();
                }
            }
            if (CollideCheck<Fuse>(Position + Vector2.UnitY))
            {
                Fuse nextFuse = CollideFirst<Fuse>(Position + Vector2.UnitY);
                if (nextFuse != null)
                {
                    nextFuse.RemoveSelf();
                }
            }
            if (CollideCheck<Fuse>(Position + Vector2.UnitX))
            {
                Fuse nextFuse = CollideFirst<Fuse>(Position + Vector2.UnitX);
                if (nextFuse != null)
                {
                    nextFuse.RemoveSelf();
                }
            }
            if (CollideCheck<Fuse>(Position - Vector2.UnitX))
            {
                Fuse nextFuse = CollideFirst<Fuse>(Position - Vector2.UnitX);
                if (nextFuse != null)
                {
                    nextFuse.RemoveSelf();
                }
            }
            base.Removed(scene);
        }
    }
}
