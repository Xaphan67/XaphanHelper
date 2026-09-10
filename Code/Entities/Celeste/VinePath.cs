using Celeste.Mod.Entities;
using Celeste.Mod.XaphanHelper.Colliders;
using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste.Mod.XaphanHelper.Entities
{
    [Tracked(true)]
    [CustomEntity("XaphanHelper/VinePath")]
    class VinePath : Entity
    {
        [Tracked(true)]
        public class VinePathSection : Entity
        {
            public bool ConnectedN;

            public bool ConnectedS;

            public bool ConnectedE;

            public bool ConnectedW;

            private Sprite Sprite;

            private Sprite GrownSprite;

            private string directory;

            private string tile;

            private Vector2 tilesSpritePos;

            private bool Grown;

            public int ID;

            private int col;

            private int row;

            private int tileWidth;

            private int tileHeight;

            public VinePathSection(EntityData data, Vector2 position, int col, int row, int tileWidth, int tileHeight) : base(position)
            {
                Tag = Tags.TransitionUpdate;
                this.col = col;
                this.row = row;
                this.tileWidth = tileWidth;
                this.tileHeight = tileHeight;
                Collider = new Hitbox(8f, 8f);
                Add(new PlayerCollider(onPlayer, Collider));
                Add(new WeaponCollider(HitByBeam, HitByMissile, Collider));
                directory = data.Attr("directory");
                if (string.IsNullOrEmpty(directory))
                {
                    directory = "objects/XaphanHelper/Vine";
                }
                Sprite = new Sprite(GFX.Game, directory + "/");
                Sprite.AddLoop("path", "path", 0.08f);
                Sprite.Play("path");
                GrownSprite = new Sprite(GFX.Game, directory + "/");
                GrownSprite.AddLoop("path", "path-grown", 0.08f);
                GrownSprite.Play("path");
                Depth = 8999;
            }

            private void onPlayer(Player player)
            {
                if (CollideFirst<VineHead>() == null)
                {
                    player.Die((player.Position - Position).SafeNormalize());
                }
            }
            private void HitByBeam(Beam beam)
            {
                beam.CollideSolid(beam.Direction);
            }

            private void HitByMissile(Missile missile)
            {
                if (!missile.CollideCheck<VineHead>())
                {
                    missile.CollideImmune(missile.Direction);
                }
            }

            public override void Added(Scene scene)
            {
                base.Added(scene);
                if (CollideCheck<VinePathSection>())
                {
                    RemoveSelf();
                }
            }

            public override void Awake(Scene scene)
            {
                base.Awake(scene);
                ConnectedN = HasCapConnection(-Vector2.UnitY);
                ConnectedS = HasCapConnection(Vector2.UnitY);
                ConnectedE = HasCapConnection(Vector2.UnitX);
                ConnectedW = HasCapConnection(-Vector2.UnitX);
                bool None = !ConnectedN && !ConnectedS && !ConnectedE && !ConnectedW;
                tile = None ? "None" : (ConnectedN ? "N" : "") + (ConnectedS ? "S" : "") + (ConnectedE ? "E" : "") + (ConnectedW ? "W" : "");
                GetSpritePos();
                if (ID == 0)
                {
                    if (CollideCheck<VineHead>())
                    {
                        ID = CollideFirst<VineHead>().ID;
                        if (ConnectedN)
                        {
                            VinePathSection section = CollideFirst<VinePathSection>(Position - Vector2.UnitY);
                            section.AffectID(ID);
                        }
                        if (ConnectedS)
                        {
                            VinePathSection section = CollideFirst<VinePathSection>(Position + Vector2.UnitY);
                            section.AffectID(ID);
                        }
                        if (ConnectedE)
                        {
                            VinePathSection section = CollideFirst<VinePathSection>(Position + Vector2.UnitX);
                            section.AffectID(ID);
                        }
                        if (ConnectedW)
                        {
                            VinePathSection section = CollideFirst<VinePathSection>(Position - Vector2.UnitX);
                            section.AffectID(ID);
                        }
                    }
                }
            }

            private static bool IsAtEdge(int size, int index)
            {
                return size <= 1 || index == 0 || index == size - 1;
            }

            public bool HasCapConnection(Vector2 direction)
            {
                Vector2 cellPos = Position + direction * 8f;
                string axis = direction.Y != 0 ? "x" : "y";
                return HasAdjacentCapOnly(cellPos, axis);
            }

            private bool HasAdjacentCapOnly(Vector2 cellPos, string axis)
            {
                int selfIndex = axis == "x" ? col : row;
                int selfSize = axis == "x" ? tileWidth : tileHeight;

                if (!IsAtEdge(selfSize, selfIndex))
                {
                    return false;
                }

                VinePathSection other = Scene.CollideFirst<VinePathSection>(new Rectangle((int)cellPos.X, (int)cellPos.Y, 1, 1));
                if (other == null)
                {
                    return false;
                }

                int otherIndex = axis == "x" ? other.col : other.row;
                int otherSize = axis == "x" ? other.tileWidth : other.tileHeight;
                return IsAtEdge(otherSize, otherIndex);
            }

            public void AffectID(int id)
            {
                ID = id;
                if (HasCapConnection(-Vector2.UnitY))
                {
                    VinePathSection section = CollideFirst<VinePathSection>(Position - Vector2.UnitY);
                    if (section.ID == 0)
                    {
                        section.AffectID(ID);
                    }
                }
                if (HasCapConnection(Vector2.UnitY))
                {
                    VinePathSection section = CollideFirst<VinePathSection>(Position + Vector2.UnitY);
                    if (section.ID == 0)
                    {
                        section.AffectID(ID);
                    }
                }
                if (HasCapConnection(Vector2.UnitX))
                {
                    VinePathSection section = CollideFirst<VinePathSection>(Position + Vector2.UnitX);
                    if (section.ID == 0)
                    {
                        section.AffectID(ID);
                    }
                }
                if (HasCapConnection(-Vector2.UnitX))
                {
                    VinePathSection section = CollideFirst<VinePathSection>(Position - Vector2.UnitX);
                    if (section.ID == 0)
                    {
                        section.AffectID(ID);
                    }
                }
            }

            public override void Update()
            {
                base.Update();
                Collidable = Grown;
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

            public void SetGrownSprite(bool state)
            {
                Grown = state;
            }

            public override void Render()
            {
                base.Render();
                if (Grown)
                {
                    GrownSprite.RenderPosition = Position;
                    GrownSprite.DrawSubrect(Vector2.Zero, new Rectangle((int)tilesSpritePos.X * 8, (int)tilesSpritePos.Y * 8, 8, 8));
                }
                else
                {
                    Sprite.RenderPosition = Position;
                    Sprite.DrawSubrect(Vector2.Zero, new Rectangle((int)tilesSpritePos.X * 8, (int)tilesSpritePos.Y * 8, 8, 8));
                }
            }
        }

        private EntityData data;

        public VinePath(EntityData data, Vector2 offset) : base(data.Position + offset)
        {
            Tag = Tags.TransitionUpdate;
            Collider = new Hitbox(data.Width, data.Height);
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
                    scene.Add(new VinePathSection(data, Position + new Vector2(x * 8, y * 8), x, y, tileWidth, tileHeight));
                }
            }
        }
    }
}
