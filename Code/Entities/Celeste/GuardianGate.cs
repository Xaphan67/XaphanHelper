using System.Collections;
using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste.Mod.XaphanHelper.Entities
{
    [Tracked(true)]
    class GuardianGate : Solid
    {
        public char fillTile;

        public char flagFillTile;

        private TileGrid tiles;

        public string flag;

        public GuardianGate(Vector2 position, float width, float height, char tiletype, char flagTiletype, string flag) : base(position, width, height, safe: true)
        {
            this.flag = flag;
            fillTile = tiletype;
            flagFillTile = flagTiletype;
            Add(new LightOcclude(0.5f));
        }

        public override void Awake(Scene scene)
        {
            base.Awake(scene);
            int tilesX = (int)Width / 8;
            int tilesY = (int)Height / 8;
            Add(tiles = GFX.FGAutotiler.GenerateBox((!string.IsNullOrEmpty(flag) && SceneAs<Level>().Session.GetFlag(flag)) ? flagFillTile : fillTile, tilesX, tilesY).TileGrid);
            Add(new TileInterceptor(tiles, highPriority: false));
            if (SceneAs<Level>().Session.GetFlag("AncientGuardian_Gates"))
            {
                StartAtEndPosition();
            }
        }

        private IEnumerator EnterSequence(Vector2 moveTo)
        {
            Visible = (Collidable = true);
            yield return 0.25f;
            Audio.Play("event:/game/04_cliffside/stone_blockade", Position);
            yield return 0.25f;
            Vector2 start = Position;
            Tween tween = Tween.Create(Tween.TweenMode.Oneshot, Ease.CubeOut, 1f, start: true);
            tween.OnUpdate = delegate (Tween t)
            {
                MoveTo(Vector2.Lerp(start, moveTo, t.Eased));
            };
            Add(tween);
        }

        public void Move()
        {
            Add(new Coroutine(EnterSequence(Position + Vector2.UnitX * (Center.X < SceneAs<Level>().Bounds.Center.X ? 32f : -32f))));
        }

        public void StartAtEndPosition()
        {
            MoveTo(Position +Vector2.UnitX * (Center.X < SceneAs<Level>().Bounds.Center.X ? 32f : -32f));
        }

        public void Break()
        {
            Level level = SceneAs<Level>();
            Audio.Play("event:/game/general/wall_break_dirt", Position);
            for (int i = 0; i < Width / 8f; i++)
            {
                for (int j = 0; j < Height / 8f; j++)
                {
                    Scene.Add(Engine.Pooler.Create<Debris>().Init(Position + new Vector2(4 + i * 8, 4 + j * 8), (!string.IsNullOrEmpty(flag) && level.Session.GetFlag(flag)) ? flagFillTile : fillTile).BlastFrom(Center));
                }
            }
            Collidable = false;
            DestroyStaticMovers();
            RemoveSelf();
        }
    }
}
