using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste.Mod.XaphanHelper.Entities
{
    [Tracked(true)]
    [CustomEntity("XaphanHelper/FlagBlock")]
    class FlagBlock : Solid
    {
        public enum Modes
        {
            Wall,
            Block
        }

        private Modes mode;

        private char fillTile;

        private char flagFillTile;

        private TileGrid tiles;

        private bool fade;

        private EffectCutout cutout;

        private float transitionStartAlpha;

        private bool transitionFade;

        public EntityID eid;

        public bool breaking;

        private string[] removeFlags;

        private bool permanent;

        private string flag;

        public FlagBlock(EntityData data, Vector2 position, EntityID ID) : base(data.Position + position, data.Width, data.Height, safe: true)
        {
            mode = data.Enum<Modes>("mode");
            flag = data.Attr("flag");
            eid = ID;
            fillTile = data.Char("tiletype", '3');
            flagFillTile = data.Char("flagTiletype", '3');
            permanent = data.Bool("permanent", true);
            removeFlags = data.Attr("removeFlags").Split(',');
            Collider = new Hitbox(data.Width, data.Height);
            Depth = -13000;
            Add(cutout = new EffectCutout());
            Add(new LightOcclude(0.5f));
        }

        public override void Added(Scene scene)
        {
            base.Added(scene);
            foreach (string flag in removeFlags)
            {
                if (SceneAs<Level>().Session.GetFlag(flag))
                {
                    RemoveSelf();
                    break;
                }
            }
            if (CollideCheck<Player>())
            {
                RemoveSelf();
                SceneAs<Level>().Session.DoNotLoad.Add(eid);
            }
            else
            {
                Collidable = (Visible = true);
            }
            int tilesX = (int)Width / 8;
            int tilesY = (int)Height / 8;
            Level level = SceneAs<Level>();
            if (mode == Modes.Wall)
            {
                Rectangle tileBounds = level.Session.MapData.TileBounds;
                VirtualMap<char> solidsData = level.SolidsData;
                int x = (int)X / 8 - tileBounds.Left;
                int y = (int)Y / 8 - tileBounds.Top;
                tiles = GFX.FGAutotiler.GenerateOverlay((!string.IsNullOrEmpty(flag) && level.Session.GetFlag(flag)) ? flagFillTile : fillTile, x, y, tilesX, tilesY, solidsData).TileGrid;
            }
            else if (mode == Modes.Block)
            {
                tiles = GFX.FGAutotiler.GenerateBox((!string.IsNullOrEmpty(flag) && level.Session.GetFlag(flag)) ? flagFillTile : fillTile, tilesX, tilesY).TileGrid;
            }
            Add(tiles);
            Add(new TileInterceptor(tiles, highPriority: false));
        }

        public override void Awake(Scene scene)
        {
            base.Awake(scene);
            if (CollideCheck<Player>())
            {
                RevealWhenTransition();
            }
            else
            {
                TransitionListener transitionListener = new();
                transitionListener.OnOut = OnTransitionOut;
                transitionListener.OnOutBegin = OnTransitionOutBegin;
                transitionListener.OnIn = OnTransitionIn;
                transitionListener.OnInBegin = OnTransitionInBegin;
                Add(transitionListener);
            }
        }

        public void RevealWhenTransition()
        {
            tiles.Alpha = 0f;
            fade = true;
            cutout.Visible = false;
            SceneAs<Level>().Session.DoNotLoad.Add(eid);
        }

        private void OnTransitionOutBegin()
        {
            if (Collide.CheckRect(this, SceneAs<Level>().Bounds))
            {
                transitionFade = true;
                transitionStartAlpha = tiles.Alpha;
            }
            else
            {
                transitionFade = false;
            }
        }

        private void OnTransitionOut(float percent)
        {
            if (transitionFade)
            {
                tiles.Alpha = transitionStartAlpha * (1f - percent);
            }
        }

        private void OnTransitionInBegin()
        {
            Level level = SceneAs<Level>();
            if (level.PreviousBounds.HasValue && Collide.CheckRect(this, level.PreviousBounds.Value))
            {
                transitionFade = true;
                tiles.Alpha = 0f;
            }
            else
            {
                transitionFade = false;
            }
        }

        private void OnTransitionIn(float percent)
        {
            if (transitionFade)
            {
                tiles.Alpha = percent;
            }
        }

        public override void Update()
        {
            base.Update();
            if (fade)
            {
                tiles.Alpha = Calc.Approach(tiles.Alpha, 0f, 2f * Engine.DeltaTime);
                cutout.Alpha = tiles.Alpha;
                if (tiles.Alpha <= 0f)
                {
                    RemoveSelf();
                }
                return;
            }
            if (!breaking)
            {
                foreach (string flag in removeFlags)
                {
                    if (SceneAs<Level>().Session.GetFlag(flag))
                    {
                        Break();
                        break;
                    }
                }
            }
        }

        public void Break(bool playSound = true, bool playDebrisSound = true)
        {
            breaking = true;
            Level level = SceneAs<Level>();
            if (playSound)
            {
                if (((!string.IsNullOrEmpty(flag) && level.Session.GetFlag(flag)) ? flagFillTile : fillTile) == '1')
                {
                    Audio.Play("event:/game/general/wall_break_dirt", Position);
                }
                else if (((!string.IsNullOrEmpty(flag) && level.Session.GetFlag(flag)) ? flagFillTile : fillTile) == '3')
                {
                    Audio.Play("event:/game/general/wall_break_ice", Position);
                }
                else if (((!string.IsNullOrEmpty(flag) && level.Session.GetFlag(flag)) ? flagFillTile : fillTile) == '9')
                {
                    Audio.Play("event:/game/general/wall_break_wood", Position);
                }
                else
                {
                    Audio.Play("event:/game/general/wall_break_stone", Position);
                }
            }
            for (int i = 0; i < Width / 8f; i++)
            {
                for (int j = 0; j < Height / 8f; j++)
                {
                    Scene.Add(Engine.Pooler.Create<Debris>().Init(Position + new Vector2(4 + i * 8, 4 + j * 8), (!string.IsNullOrEmpty(flag) && level.Session.GetFlag(flag)) ? flagFillTile : fillTile, playDebrisSound).BlastFrom(Center));
                }
            }
            Collidable = false;
            RemoveSelf();
            if (permanent)
            {
                level.Session.DoNotLoad.Add(eid);
            }
        }


        public override void Render()
        {
            if (mode == Modes.Wall)
            {
                Level level = Scene as Level;
                if (level.ShakeVector.X < 0f && level.Camera.X <= level.Bounds.Left && X <= level.Bounds.Left)
                {
                    tiles.RenderAt(Position + new Vector2(-3f, 0f));
                }
                if (level.ShakeVector.X > 0f && level.Camera.X + 320f >= level.Bounds.Right && X + Width >= level.Bounds.Right)
                {
                    tiles.RenderAt(Position + new Vector2(3f, 0f));
                }
                if (level.ShakeVector.Y < 0f && level.Camera.Y <= level.Bounds.Top && Y <= level.Bounds.Top)
                {
                    tiles.RenderAt(Position + new Vector2(0f, -3f));
                }
                if (level.ShakeVector.Y > 0f && level.Camera.Y + 180f >= level.Bounds.Bottom && Y + Height >= level.Bounds.Bottom)
                {
                    tiles.RenderAt(Position + new Vector2(0f, 3f));
                }
            }
            base.Render();
        }

        public override void Removed(Scene scene)
        {
            base.Removed(scene);
            DestroyStaticMovers();
        }
    }
}
