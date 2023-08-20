using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste.Mod.XaphanHelper.Entities
{
    [Tracked(true)]
    [CustomEntity("XaphanHelper/BreakBlock")]
    public class BreakBlock : Solid
    {
        public enum Modes
        {
            Wall,
            Block
        }

        private Modes mode;

        public char fillTile;

        private char flagFillTile;

        private TileGrid tiles;

        private bool fade;

        private EffectCutout cutout;

        private float transitionStartAlpha;

        private bool transitionFade;

        public EntityID eid;

        public int index;

        public string color;

        public string directory;

        public bool startRevealed;

        public string type;

        private string respawn;

        private bool permanent;

        private string flag;

        private bool destroyStaticMovers;

        public BreakBlock(EntityData data, Vector2 position, EntityID ID) : base(data.Position + position, data.Width, data.Height, safe: true)
        {
            OnDashCollide = OnDashed;
            index = data.Int("index");
            mode = data.Enum<Modes>("mode");
            flag = data.Attr("flag");
            color = data.Attr("color");
            directory = data.Attr("directory");
            type = data.Attr("type");
            startRevealed = data.Bool("startRevealed");
            eid = ID;
            fillTile = data.Char("tiletype", '3');
            flagFillTile = data.Char("flagTiletype", '3');
            permanent = data.Bool("permanent", true);
            respawn = data.Attr("respawn");
            destroyStaticMovers = data.Bool("destroyStaticMovers");
            if (string.IsNullOrEmpty(respawn) && permanent)
            {
                respawn = "Never";
            }
            Collider = new Hitbox(data.Width, data.Height);
            Depth = -13000;
            Add(cutout = new EffectCutout());
            Add(new LightOcclude(0.5f));
        }

        public static void Load()
        {
            On.Celeste.ChangeRespawnTrigger.OnEnter += onChangeRespawnTriggerOnEnter;
        }

        public static void Unload()
        {
            On.Celeste.ChangeRespawnTrigger.OnEnter -= onChangeRespawnTriggerOnEnter;
        }

        private static void onChangeRespawnTriggerOnEnter(On.Celeste.ChangeRespawnTrigger.orig_OnEnter orig, ChangeRespawnTrigger self, Player player)
        {
            bool onSolid = true;
            Vector2 point = self.Target + Vector2.UnitY * -4f;
            Session session = self.SceneAs<Level>().Session;
            if (self.Scene.CollideCheck<Solid>(point))
            {
                onSolid = self.Scene.CollideCheck<FloatySpaceBlock>(point);
            }
            if (onSolid && (!session.RespawnPoint.HasValue || session.RespawnPoint.Value != self.Target))
            {
                foreach (EntityID entity in XaphanModule.ModSession.NoRespawnIds)
                {
                    session.DoNotLoad.Add(entity);
                }
                XaphanModule.ModSession.NoRespawnIds.Clear();
            }
            orig(self, player);
        }

        private DashCollisionResults OnDashed(Player player, Vector2 direction)
        {
            foreach (BreakBlockIndicator indicator in SceneAs<Level>().Tracker.GetEntities<BreakBlockIndicator>())
            {
                if (indicator.block == this && (indicator.CollideCheck<Player>(indicator.Position + Vector2.UnitX * 2) || indicator.CollideCheck<Player>(indicator.Position + Vector2.UnitX * -2) || indicator.CollideCheck<Player>(indicator.Position + Vector2.UnitY * 2) || indicator.CollideCheck<Player>(indicator.Position + Vector2.UnitY * -2)))
                {
                    indicator.OnPlayer(player);
                }
            }
            return DashCollisionResults.NormalCollision;
        }


        public override void Added(Scene scene)
        {
            base.Added(scene);
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
            if (!string.IsNullOrEmpty(type) && !string.IsNullOrEmpty(color))
            {
                for (int i = 0; i < Height / 8; i++)
                {
                    for (int j = 0; j < Width / 8; j++)
                    {
                        if (i == 0 || i == Height / 8 - 1 || j == 0 || j == Width / 8 - 1)
                        {
                            SceneAs<Level>().Add(new BreakBlockIndicator(this, true, Position + new Vector2(j * 8, i * 8)));
                        }
                    }
                }
            }
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
            Player player = SceneAs<Level>().Tracker.GetEntity<Player>();
            if (player != null && SceneAs<Level>().Session.GetFlag("Xaphan_Helper_Shinesparking") && (CollideCheck(player, Position + Vector2.UnitX) || (CollideCheck(player, Position - Vector2.UnitX))))
            {
                OnDashed(player, new Vector2(player.Facing == Facings.Left ? -1 : 1, 0));
            }
            if (player != null && player.CollideCheck(this))
            {
                Break();
            }
        }

        public void Break(bool playSound = true, bool playDebrisSound = true)
        {
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
            if (respawn == "Never")
            {
                level.Session.DoNotLoad.Add(eid);
            }
            else if (respawn == "Until checkpoint")
            {
                XaphanModule.ModSession.NoRespawnIds.Add(eid);
            }
            foreach (BreakBlockIndicator indicator in SceneAs<Level>().Tracker.GetEntities<BreakBlockIndicator>())
            {
                if (indicator.block == this)
                {
                    indicator.RemoveSelf();
                }
            }
            if (destroyStaticMovers)
            {
                DestroyStaticMovers();
            }
            RemoveSelf();
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
    }
}
