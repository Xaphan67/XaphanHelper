using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste.Mod.XaphanHelper.Entities
{
    [Tracked(true)]
    [CustomEntity("XaphanHelper/CustomExitBlock")]
    class CustomExitBlock : Solid
    {
        public enum Modes
        {
            Wall,
            Block
        }

        private Modes mode;

        private TileGrid tiles;

        private TransitionListener tl;

        public EffectCutout cutout;

        private float startAlpha;

        private char fillTile;

        private char flagFillTile;

        private bool CloseSound;

        public int group;

        private string flag;

        public CustomExitBlock(EntityData data, Vector2 offset) : base(data.Position + offset, data.Width, data.Height, safe: true)
        {
            Depth = -13000;
            mode = data.Enum<Modes>("mode", Modes.Block);
            flag = data.Attr("flag");
            flagFillTile = data.Char("flagTiletype", '3');
            fillTile = data.Char("tiletype", '3');
            CloseSound = data.Bool("closeSound");
            group = data.Int("group");
            tl = new TransitionListener();
            tl.OnOutBegin = OnTransitionOutBegin;
            tl.OnInBegin = OnTransitionInBegin;
            Add(tl);
            Add(cutout = new EffectCutout());
            SurfaceSoundIndex = SurfaceIndex.TileToIndex[fillTile];
            EnableAssistModeChecks = false;
        }

        private void OnTransitionOutBegin()
        {
            if (Collide.CheckRect(this, SceneAs<Level>().Bounds))
            {
                tl.OnOut = OnTransitionOut;
                startAlpha = tiles.Alpha;
            }
        }

        private void OnTransitionOut(float percent)
        {
            cutout.Alpha = (tiles.Alpha = MathHelper.Lerp(startAlpha, 0f, percent));
            cutout.Update();
        }

        private void OnTransitionInBegin()
        {
            if (Collide.CheckRect(this, SceneAs<Level>().PreviousBounds.Value) && !CollideCheck<Player>())
            {
                cutout.Alpha = 0f;
                tiles.Alpha = 0f;
                tl.OnIn = OnTransitionIn;
            }
        }

        private void OnTransitionIn(float percent)
        {
            cutout.Alpha = (tiles.Alpha = percent);
            cutout.Update();
        }

        public override void Added(Scene scene)
        {
            base.Added(scene);
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
            int count = 0;
            foreach (CustomExitBlock exitblock in Scene.Entities.FindAll<CustomExitBlock>())
            {
                if (exitblock.group == group)
                {
                    if (exitblock.CollideCheck<Player>())
                    {
                        count++;
                    }
                }
            }
            if (count > 0)
            {
                cutout.Alpha = (tiles.Alpha = 0f);
                Collidable = false;
            }
        }

        public override void Update()
        {
            base.Update();
            int count = 0;
            foreach (CustomExitBlock exitblock in Scene.Entities.FindAll<CustomExitBlock>())
            {
                if (exitblock.group == group)
                {
                    if (exitblock.CollideCheck<Player>())
                    {
                        SceneAs<Level>().Session.SetFlag("Inside_ExitBlock_" + group, true);
                        count++;
                    }
                }

            }
            if (count == 0)
            {
                SceneAs<Level>().Session.SetFlag("Inside_ExitBlock_" + group, false);
            }
            if (Collidable)
            {
                cutout.Alpha = (tiles.Alpha = Calc.Approach(tiles.Alpha, 1f, Engine.DeltaTime));
                foreach (CustomFakeWall fakeWall in SceneAs<Level>().Tracker.GetEntities<CustomFakeWall>())
                {
                    if (fakeWall.group == group && CollideCheck(fakeWall))
                    {
                        fakeWall.fade = true;
                        fakeWall.tiles.Alpha = (fakeWall.cutout.Alpha = tiles.Alpha);
                    }
                }
            }
            else if (!SceneAs<Level>().Session.GetFlag("Inside_ExitBlock_" + group))
            {
                Collidable = true;
                if (CloseSound)
                {
                    Audio.Play("event:/game/general/passage_closed_behind", Center);
                }
            }
        }

        public override void Render()
        {
            if (tiles.Alpha >= 1f)
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
    }
}
