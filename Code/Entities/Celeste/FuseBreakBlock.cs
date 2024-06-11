using System;
using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste.Mod.XaphanHelper.Entities
{
    [Tracked(true)]
    [CustomEntity("XaphanHelper/FuseBreakBlock")]
    public class FuseBreakBlock : Entity
    {
        [Tracked()]
        public class FuseBreakBlockTile : Solid
        {
            private string texture;

            private MTexture[,] tilesetTextures;

            private string tilesetMask = "000-010-000";

            public FuseBreakBlockTile(EntityData data, Vector2 offset) : base(data.Position + offset, 8, 8, safe: true)
            {
                Tag = Tags.TransitionUpdate;
                SurfaceSoundIndex = data.Int("soundIndex");
                texture = data.Attr("texture");
                if (!string.IsNullOrEmpty(texture))
                {
                    MTexture mtexture = GFX.Game["tilesets/" + texture];
                    tilesetTextures = new MTexture[6, 15];
                    for (int i = 0; i < 6; i++)
                    {
                        for (int j = 0; j < 15; j++)
                        {
                            tilesetTextures[i, j] = mtexture.GetSubtexture(new Rectangle(i * 8, j * 8, 8, 8));
                        }
                    }
                }
                Collider = new Hitbox(8, 8);
                Add(new LightOcclude(1f));
                Depth = -10002;
            }

            public override void Added(Scene scene)
            {
                base.Added(scene);
                if (CollideCheck<Player>())
                {
                    RemoveSelf();
                }
            }

            public override void Awake(Scene scene)
            {
                base.Awake(scene);
                if (!string.IsNullOrEmpty(texture) && tilesetTextures != null)
                {
                    string SolidTopLeft = "0";
                    string SolidTop = "0";
                    string SolidTopRight = "0";
                    string SolidLeft = "0";
                    string SolidRight = "0";
                    string SolidBottomLeft = "0";
                    string SolidBottom = "0";
                    string SolidBottomRight = "0";
                    string SolidFarTop = "0";
                    string SolidFarLeft = "0";
                    string SolidFarRight = "0";
                    string SolidFarBottom = "0";
                    if (CollideCheck<SolidTiles>(Position + new Vector2(-8f, -8f)) || CollideCheck< FuseBreakBlockTile>(Position + new Vector2(-8f, -8f)))
                    {
                        SolidTopLeft = "1";
                    }
                    else
                    {
                        SolidTopLeft = "0";
                    }
                    if (CollideCheck<SolidTiles>(Position + new Vector2(0f, -8f)) || CollideCheck<FuseBreakBlockTile>(Position + new Vector2(0f, -8f)))
                    {
                        SolidTop = "1";
                    }
                    else
                    {
                        SolidTop = "0";
                    }
                    if (CollideCheck<SolidTiles>(Position + new Vector2(8f, -8f)) || CollideCheck<FuseBreakBlockTile>(Position + new Vector2(8f, -8f)))
                    {
                        SolidTopRight = "1";
                    }
                    else
                    {
                        SolidTopRight = "0";
                    }
                    if (CollideCheck<SolidTiles>(Position + new Vector2(-8f, 0f)) || CollideCheck<FuseBreakBlockTile>(Position + new Vector2(-8f, 0f)))
                    {
                        SolidLeft = "1";
                    }
                    else
                    {
                        SolidLeft = "0";
                    }
                    if (CollideCheck<SolidTiles>(Position + new Vector2(8f, 0f)) || CollideCheck<FuseBreakBlockTile>(Position + new Vector2(8f, 0f)))
                    {
                        SolidRight = "1";
                    }
                    else
                    {
                        SolidRight = "0";
                    }
                    if (CollideCheck<SolidTiles>(Position + new Vector2(-8f, 8f)) || CollideCheck<FuseBreakBlockTile>(Position + new Vector2(-8f, 8f)))
                    {
                        SolidBottomLeft = "1";
                    }
                    else
                    {
                        SolidBottomLeft = "0";
                    }
                    if (CollideCheck<SolidTiles>(Position + new Vector2(0f, 8f)) || CollideCheck<FuseBreakBlockTile>(Position + new Vector2(0f, 8f)))
                    {
                        SolidBottom = "1";
                    }
                    else
                    {
                        SolidBottom = "0";
                    }
                    if (CollideCheck<SolidTiles>(Position + new Vector2(8f, 8f)) || CollideCheck<FuseBreakBlockTile>(Position + new Vector2(8f, 8f)))
                    {
                        SolidBottomRight = "1";
                    }
                    else
                    {
                        SolidBottomRight = "0";
                    }
                    if (Scene.CollideCheck<SolidTiles>(new Rectangle((int)X, (int)Y - 16, 1, 1)) || Scene.CollideCheck<FuseBreakBlockTile>(new Rectangle((int)X, (int)Y - 16, 1, 1)))
                    {
                        SolidFarTop = "1";
                    }
                    else
                    {
                        SolidFarTop = "0";
                    }
                    if (Scene.CollideCheck<SolidTiles>(new Rectangle((int)X - 16, (int)Y, 1, 1)) || Scene.CollideCheck<FuseBreakBlockTile>(new Rectangle((int)X - 16, (int)Y, 1, 1)))
                    {
                        SolidFarLeft = "1";
                    }
                    else
                    {
                        SolidFarLeft = "0";
                    }
                    if (Scene.CollideCheck<SolidTiles>(new Rectangle((int)X + 16, (int)Y, 1, 1)) || Scene.CollideCheck<FuseBreakBlockTile>(new Rectangle((int)X + 16, (int)Y, 1, 1)))
                    {
                        SolidFarRight = "1";
                    }
                    else
                    {

                        SolidFarRight = "0";
                    }
                    if (Scene.CollideCheck<SolidTiles>(new Rectangle((int)X, (int)Y + 16, 1, 1)) || Scene.CollideCheck<FuseBreakBlockTile>(new Rectangle((int)X, (int)Y + 16, 1, 1)))
                    {
                        SolidFarBottom = "1";
                    }
                    else
                    {
                        SolidFarBottom = "0";
                    }
                    tilesetMask = SolidTopLeft + SolidTop + SolidTopRight + "-" + SolidLeft + "1" + SolidRight + "-" + SolidBottomLeft + SolidBottom + SolidBottomRight + "-" + SolidFarTop + SolidFarLeft + SolidFarRight + SolidFarBottom;
                }
            }

            public override void Removed(Scene scene)
            {
                base.Removed(scene);
                foreach (StaticMover staticMover in staticMovers)
                {
                    staticMover.Destroy();
                }
            }

            public override void Render()
            {
                base.Render();
                int positionXLastDigit = Math.Abs((int)Position.X / 8 % 10);
                int positionYLastDigit = Math.Abs((int)Position.Y / 8 % 10);
                int seed = positionXLastDigit + positionYLastDigit;
                int variation = 0;
                int variationPadding = 0;
                if (seed == 1 || seed == 5 || seed == 9 || seed == 13 || seed == 17)
                {
                    variation = 1;
                }
                else if (seed == 2 || seed == 6 || seed == 10 || seed == 14 || seed == 18)
                {
                    variation = 2;
                }
                else if (seed == 3 || seed == 7 || seed == 11 || seed == 15)
                {
                    variation = 3;
                }
                if (seed <= 9) // Should adapt to number of padding tiles of tileset
                {
                    variationPadding = seed;
                }
                else
                {
                    variationPadding = seed - 10; // Should adapt to number of padding tiles of tileset
                }
                if (tilesetTextures != null)
                {
                    if (tilesetMask[0] == '1' && tilesetMask[1] == '1' && tilesetMask[2] == '1' && tilesetMask[4] == '1' && tilesetMask[6] == '1' && tilesetMask[8] == '1' && tilesetMask[9] == '1' && tilesetMask[10] == '1')
                    {
                        if (tilesetMask[12] == '1' && tilesetMask[13] == '1' && tilesetMask[14] == '1' && tilesetMask[15] == '1')
                        {
                            tilesetTextures[5, 12].Draw(Position);
                        }
                        else
                        {
                            tilesetTextures[5, variationPadding].Draw(Position);
                        }
                    }
                    else if (tilesetMask[0] == '1' && tilesetMask[1] == '1' && tilesetMask[2] == '1' && tilesetMask[4] == '1' && tilesetMask[6] == '1' && tilesetMask[8] == '1' && tilesetMask[9] == '1' && tilesetMask[10] == '0')
                    {
                        tilesetTextures[4, 0].Draw(Position);
                    }
                    else if (tilesetMask[0] == '1' && tilesetMask[1] == '1' && tilesetMask[2] == '0' && tilesetMask[4] == '1' && tilesetMask[6] == '1' && tilesetMask[8] == '1' && tilesetMask[9] == '1' && tilesetMask[10] == '1')
                    {
                        tilesetTextures[4, 1].Draw(Position);
                    }
                    else if (tilesetMask[0] == '1' && tilesetMask[1] == '1' && tilesetMask[2] == '1' && tilesetMask[4] == '1' && tilesetMask[6] == '1' && tilesetMask[8] == '0' && tilesetMask[9] == '1' && tilesetMask[10] == '1')
                    {
                        tilesetTextures[4, 2].Draw(Position);
                    }
                    else if (tilesetMask[0] == '0' && tilesetMask[1] == '1' && tilesetMask[2] == '1' && tilesetMask[4] == '1' && tilesetMask[6] == '1' && tilesetMask[8] == '1' && tilesetMask[9] == '1' && tilesetMask[10] == '1')
                    {
                        tilesetTextures[4, 3].Draw(Position);
                    }
                    else if (tilesetMask[0] == '1' && tilesetMask[1] == '1' && tilesetMask[2] == '0' && tilesetMask[4] == '1' && tilesetMask[6] == '1' && tilesetMask[8] == '1' && tilesetMask[9] == '1' && tilesetMask[10] == '0')
                    {
                        tilesetTextures[4, 4].Draw(Position);
                    }
                    else if (tilesetMask[0] == '0' && tilesetMask[1] == '1' && tilesetMask[2] == '0' && tilesetMask[4] == '1' && tilesetMask[6] == '1' && tilesetMask[8] == '1' && tilesetMask[9] == '1' && tilesetMask[10] == '1')
                    {
                        tilesetTextures[4, 5].Draw(Position);
                    }
                    else if (tilesetMask[0] == '0' && tilesetMask[1] == '1' && tilesetMask[2] == '1' && tilesetMask[4] == '1' && tilesetMask[6] == '1' && tilesetMask[8] == '0' && tilesetMask[9] == '1' && tilesetMask[10] == '1')
                    {
                        tilesetTextures[4, 6].Draw(Position);
                    }
                    else if (tilesetMask[0] == '1' && tilesetMask[1] == '1' && tilesetMask[2] == '1' && tilesetMask[4] == '1' && tilesetMask[6] == '1' && tilesetMask[8] == '0' && tilesetMask[9] == '1' && tilesetMask[10] == '0')
                    {
                        tilesetTextures[4, 7].Draw(Position);
                    }
                    else if (tilesetMask[0] == '0' && tilesetMask[1] == '1' && tilesetMask[2] == '0' && tilesetMask[4] == '1' && tilesetMask[6] == '1' && tilesetMask[8] == '1' && tilesetMask[9] == '1' && tilesetMask[10] == '0')
                    {
                        tilesetTextures[4, 8].Draw(Position);
                    }
                    else if (tilesetMask[0] == '0' && tilesetMask[1] == '1' && tilesetMask[2] == '0' && tilesetMask[4] == '1' && tilesetMask[6] == '1' && tilesetMask[8] == '0' && tilesetMask[9] == '1' && tilesetMask[10] == '1')
                    {
                        tilesetTextures[4, 9].Draw(Position);
                    }
                    else if (tilesetMask[0] == '0' && tilesetMask[1] == '1' && tilesetMask[2] == '1' && tilesetMask[4] == '1' && tilesetMask[6] == '1' && tilesetMask[8] == '0' && tilesetMask[9] == '1' && tilesetMask[10] == '0')
                    {
                        tilesetTextures[4, 10].Draw(Position);
                    }
                    else if (tilesetMask[0] == '1' && tilesetMask[1] == '1' && tilesetMask[2] == '0' && tilesetMask[4] == '1' && tilesetMask[6] == '1' && tilesetMask[8] == '0' && tilesetMask[9] == '1' && tilesetMask[10] == '0')
                    {
                        tilesetTextures[4, 11].Draw(Position);
                    }
                    else if (tilesetMask[0] == '0' && tilesetMask[1] == '1' && tilesetMask[2] == '0' && tilesetMask[4] == '1' && tilesetMask[6] == '1' && tilesetMask[8] == '0' && tilesetMask[9] == '1' && tilesetMask[10] == '0')
                    {
                        tilesetTextures[4, 12].Draw(Position);
                    }
                    else if (tilesetMask[0] == '1' && tilesetMask[1] == '1' && tilesetMask[2] == '0' && tilesetMask[4] == '1' && tilesetMask[6] == '1' && tilesetMask[8] == '0' && tilesetMask[9] == '1' && tilesetMask[10] == '1')
                    {
                        tilesetTextures[4, 13].Draw(Position);
                    }
                    else if (tilesetMask[0] == '0' && tilesetMask[1] == '1' && tilesetMask[2] == '1' && tilesetMask[4] == '1' && tilesetMask[6] == '1' && tilesetMask[8] == '1' && tilesetMask[9] == '1' && tilesetMask[10] == '0')
                    {
                        tilesetTextures[4, 14].Draw(Position);
                    }
                    else if (tilesetMask[1] == '0' && tilesetMask[4] == '1' && tilesetMask[6] == '1' && tilesetMask[9] == '1')
                    {
                        tilesetTextures[variation, 0].Draw(Position);
                    }
                    else if (tilesetMask[1] == '1' && tilesetMask[4] == '1' && tilesetMask[6] == '1' && tilesetMask[9] == '0')
                    {
                        tilesetTextures[variation, 1].Draw(Position);
                    }
                    else if (tilesetMask[1] == '1' && tilesetMask[4] == '0' && tilesetMask[6] == '1' && tilesetMask[9] == '1')
                    {
                        tilesetTextures[variation, 2].Draw(Position);
                    }
                    else if (tilesetMask[1] == '1' && tilesetMask[4] == '1' && tilesetMask[6] == '0' && tilesetMask[9] == '1')
                    {
                        tilesetTextures[variation, 3].Draw(Position);
                    }
                    else if (tilesetMask[1] == '0' && tilesetMask[4] == '1' && tilesetMask[6] == '1' && tilesetMask[9] == '0')
                    {
                        tilesetTextures[variation, 4].Draw(Position);
                    }
                    else if (tilesetMask[1] == '1' && tilesetMask[4] == '0' && tilesetMask[6] == '0' && tilesetMask[9] == '1')
                    {
                        tilesetTextures[variation, 5].Draw(Position);
                    }
                    else if (tilesetMask[1] == '0' && tilesetMask[4] == '0' && tilesetMask[6] == '0' && tilesetMask[9] == '1')
                    {
                        tilesetTextures[variation, 6].Draw(Position);
                    }
                    else if (tilesetMask[1] == '1' && tilesetMask[4] == '0' && tilesetMask[6] == '0' && tilesetMask[9] == '0')
                    {
                        tilesetTextures[variation, 7].Draw(Position);
                    }
                    else if (tilesetMask[1] == '0' && tilesetMask[4] == '0' && tilesetMask[6] == '1' && tilesetMask[9] == '0')
                    {
                        tilesetTextures[variation, 8].Draw(Position);
                    }
                    else if (tilesetMask[1] == '0' && tilesetMask[4] == '1' && tilesetMask[6] == '0' && tilesetMask[9] == '0')
                    {
                        tilesetTextures[variation, 9].Draw(Position);
                    }
                    else if (tilesetMask[1] == '0' && tilesetMask[4] == '0' && tilesetMask[6] == '0' && tilesetMask[9] == '0')
                    {
                        tilesetTextures[variation, 10].Draw(Position);
                    }
                    else if (tilesetMask[1] == '0' && tilesetMask[4] == '0' && tilesetMask[6] == '1' && tilesetMask[9] == '1')
                    {
                        tilesetTextures[variation, 11].Draw(Position);
                    }
                    else if (tilesetMask[1] == '0' && tilesetMask[4] == '1' && tilesetMask[6] == '0' && tilesetMask[9] == '1')
                    {
                        tilesetTextures[variation, 12].Draw(Position);
                    }
                    else if (tilesetMask[1] == '1' && tilesetMask[4] == '0' && tilesetMask[6] == '1' && tilesetMask[9] == '0')
                    {
                        tilesetTextures[variation, 13].Draw(Position);
                    }
                    else if (tilesetMask[1] == '1' && tilesetMask[4] == '1' && tilesetMask[6] == '0' && tilesetMask[9] == '0')
                    {
                        tilesetTextures[variation, 14].Draw(Position);
                    }
                }
            }
        }

        private EntityData data;

        public FuseBreakBlock(EntityData data, Vector2 position) : base(position)
        {
            this.data = data;
        }

        public override void Added(Scene scene)
        {
            base.Added(scene);
            for(int x = 0; x < data.Width / 8; x++)
            {
                for (int y = 0; y < data.Height / 8; y++)
                {
                    SceneAs<Level>().Add(new FuseBreakBlockTile(data, Position + new Vector2(x * 8, y * 8)));
                }
            }
        }
    }
}
