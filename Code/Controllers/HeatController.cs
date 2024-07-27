using System;
using System.Reflection;
using Celeste.Mod.Entities;
using Celeste.Mod.XaphanHelper.UI_Elements;
using Celeste.Mod.XaphanHelper.Upgrades;
using Microsoft.Xna.Framework;
using Monocle;
using MonoMod.Cil;

namespace Celeste.Mod.XaphanHelper.Controllers
{
    [Tracked(true)]
    [CustomEntity("XaphanHelper/HeatController")]
    class HeatController : Entity
    {
        public float maxDuration;

        public static bool Flashing;

        public bool FlashingRed;

        public bool heatEffect;

        private bool[,] grid;

        public string inactiveFlag;

        private static FieldInfo playerFlash = typeof(Player).GetField("flash", BindingFlags.NonPublic | BindingFlags.Instance);

        public HeatController(EntityData data, Vector2 offset) : base(data.Position + offset)
        {
            Flashing = false;
            maxDuration = data.Float("maxDuration", 3f);
            heatEffect = data.Bool("heatEffect");
            inactiveFlag = data.Attr("inactiveFlag");
        }

        public void RenderDisplacement()
        {
            if (!SceneAs<Level>().Transitioning)
            {
                Color color = new(0.5f, 0.5f, 0.25f, 1f);
                int i = 0;
                int length = grid.GetLength(0);
                int length2 = grid.GetLength(1);
                for (; i < length; i++)
                {
                    if (length2 > 0 && grid[i, 0])
                    {
                        Draw.Rect(SceneAs<Level>().Bounds.X + (i * 8), SceneAs<Level>().Bounds.Y + 3f, 8f, 5f, color);
                    }
                    for (int j = 1; j < length2; j++)
                    {
                        if (grid[i, j])
                        {
                            int k;
                            for (k = 1; j + k < length2 && grid[i, j + k]; k++)
                            {
                            }
                            Draw.Rect(SceneAs<Level>().Bounds.X + (i * 8), SceneAs<Level>().Bounds.Y + (j * 8), 8f, k * 8, color);
                            j += k - 1;
                        }
                    }
                }
            }
        }

        public override void Added(Scene scene)
        {
            base.Added(scene);
            if (heatEffect)
            {
                grid = new bool[SceneAs<Level>().Bounds.Width / 8, SceneAs<Level>().Bounds.Height / 8];
                Add(new DisplacementRenderHook(RenderDisplacement));
            }
            if (SceneAs<Level>().Tracker.GetEntity<HeatIndicator>() == null)
            {
                SceneAs<Level>().Add(new HeatIndicator(maxDuration, inactiveFlag));
            }
            else
            {
                SceneAs<Level>().Tracker.GetEntity<HeatIndicator>().updateMaxDuration(maxDuration);
            }
            if (heatEffect)
            {
                int i = 0;
                for (int length = grid.GetLength(0); i < length; i++)
                {
                    int j = 0;
                    for (int length2 = grid.GetLength(1); j < length2; j++)
                    {
                        grid[i, j] = !Scene.CollideCheck<Solid>(new Rectangle(SceneAs<Level>().Bounds.X + i * 8, SceneAs<Level>().Bounds.Y + j * 8, 8, 8));
                    }
                }
            }
        }

        public static void Load()
        {
            IL.Celeste.Player.Render += modILPlayerRender;
            On.Celeste.Player.Render += modPlayerRender;
        }

        public static void Unload()
        {
            IL.Celeste.Player.Render -= modILPlayerRender;
            On.Celeste.Player.Render -= modPlayerRender;
        }

        private static void modPlayerRender(On.Celeste.Player.orig_Render orig, Player self)
        {
            orig(self);
            if (Flashing && !self.Dead)
            {
                self.Sprite.Color = Color.Red;
            }
        }

        private static void modILPlayerRender(ILContext il)
        {
            ILCursor cursor = new(il);

            if (cursor.TryGotoNext(instr => instr.MatchCallvirt<StateMachine>("get_State"), instr => instr.MatchLdcI4(19)))
            {
                cursor.Index++;
                cursor.EmitDelegate<Func<int, int>>(orig =>
                {
                    if (determineifHeatController())
                    {
                        return 19;
                    }
                    return orig;
                });
            }
        }

        private static bool determineifHeatController()
        {
            if (Engine.Scene is Level)
            {
                Level level = (Level)Engine.Scene;
                HeatController controller = level.Tracker.GetEntity<HeatController>();
                if (controller != null)
                {
                    if (Flashing)
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        public override void Update()
        {
            base.Update();
            Player player = Scene.Tracker.GetEntity<Player>();
            if (player != null && !player.Dead)
            {
                if (!VariaJacket.Active(SceneAs<Level>()) && !SceneAs<Level>().Session.GetFlag(inactiveFlag) && !XaphanModule.PlayerIsControllingRemoteDrone())
                {
                    if (Scene.OnRawInterval(0.06f))
                    {
                        Flashing = !Flashing;
                    }
                    FlashingRed = true;
                }
                else
                {
                    if (FlashingRed)
                    {
                        FlashingRed = false;
                    }
                }
                if (VariaJacket.Active(SceneAs<Level>()) || SceneAs<Level>().Session.GetFlag(inactiveFlag) || XaphanModule.PlayerIsControllingRemoteDrone())
                {
                    if (player.Sprite.Color == Color.Red)
                    {
                        player.Sprite.Color = Color.White;
                    }
                    Flashing = false;
                }
            }
        }
    }
}
