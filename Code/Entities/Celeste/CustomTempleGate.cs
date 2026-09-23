using System;
using System.Linq;
using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;
using Monocle;
using MonoMod.Utils;

namespace Celeste.Mod.XaphanHelper.Entities
{
    [Tracked(true)]
    [CustomEntity("XaphanHelper/FlagTempleGate", "XaphanHelper/CustomTempleGate")]
    class CustomTempleGate : Solid
    {
        private bool horizontal;

        private bool attachRight;

        private int closedHeight;

        private Sprite sprite;

        private Shaker shaker;

        private float drawHeight;

        private float drawHeightMoveSpeed;

        private Vector2 holdingCheckFrom;

        private bool open;

        private string spriteName;

        private string flag;

        public bool startOpen;

        private bool openOnHeartCollection;

        private bool silent;

        private bool openedBySwitch;

        public bool ClaimedByASwitch;

        public string LevelID;

        public bool ControlledBySwitch => string.IsNullOrEmpty(flag);

        private bool FlagActive => ControlledBySwitch ? openedBySwitch : SceneAs<Level>().Session.GetFlag(flag);

        private static CustomTempleGate chosenGate;

        public CustomTempleGate(EntityData data, Vector2 offset) : base(data.Position + offset, data.Width, data.Height, true)
        {
            LevelID = data.Level.Name;
            horizontal = data.Bool("horizontal", false);
            attachRight = data.Bool("attachRight", false);
            flag = data.Attr("flag", "");
            startOpen = data.Bool("startOpen", false);
            openOnHeartCollection = data.Bool("openOnHeartCollection");
            spriteName = data.Attr("spriteName", "default");
            silent = data.Bool("silent", false);
            closedHeight = data.Height;
            Add(sprite = GFX.SpriteBank.Create("templegate_" + spriteName));
            if (horizontal)
            {
                sprite.Rotation = -(float)Math.PI / 2f;
                if (attachRight)
                {
                    sprite.Rotation = (float)Math.PI / 2f;
                }
                sprite.Position += new Vector2(0, 5);
            }
            if (horizontal)
            {
                if (attachRight)
                {
                    sprite.Position += new Vector2(48, -1);
                }
                else
                {
                    sprite.X = Collider.Left;
                }
            }
            else
            {
                sprite.X = Collider.Width / 2;
            }

            sprite.Play("idle");
            Add(shaker = new Shaker(on: false));
            Depth = -9000;
            holdingCheckFrom = Position + (horizontal ? new Vector2(data.Height / 2, Width / 2f) : new Vector2(Width / 2f, data.Height / 2));
        }

        public static void Load()
        {
            On.Celeste.DashSwitch.OnDashed += onDashSwitchOnDashed;
            On.Celeste.DashSwitch.Awake += onDashSwitchAwake;
            On.Celeste.DashSwitch.GetGate += onDashSwitchGetGate;
        }

        public static void Unload()
        {
            On.Celeste.DashSwitch.OnDashed -= onDashSwitchOnDashed;
            On.Celeste.DashSwitch.Awake -= onDashSwitchAwake;
            On.Celeste.DashSwitch.GetGate -= onDashSwitchGetGate;
        }

        private static TempleGate onDashSwitchGetGate(On.Celeste.DashSwitch.orig_GetGate orig, DashSwitch self)
        {
            chosenGate = null;
            TempleGate vanillaGate = orig(self);
            EntityID id = new DynamicData(self).Get<EntityID>("id");
            CustomTempleGate flagGate = self.Scene.Tracker.GetEntities<CustomTempleGate>()
                .Cast<CustomTempleGate>()
                .Where(g => g.ControlledBySwitch && !g.ClaimedByASwitch && g.LevelID == id.Level)
                .OrderBy(g => Vector2.DistanceSquared(self.Position, g.Position))
                .FirstOrDefault();
            if (flagGate != null && (vanillaGate == null ||
                Vector2.DistanceSquared(self.Position, flagGate.Position) < Vector2.DistanceSquared(self.Position, vanillaGate.Position)))
            {
                if (vanillaGate != null)
                {
                    vanillaGate.ClaimedByASwitch = false;
                }
                flagGate.ClaimedByASwitch = true;
                chosenGate = flagGate;
                return null;
            }
            return vanillaGate;
        }

        private static DashCollisionResults onDashSwitchOnDashed(On.Celeste.DashSwitch.orig_OnDashed orig, DashSwitch self, Player player, Vector2 direction)
        {
            DynamicData data = new DynamicData(self);
            bool wasPressed = data.Get<bool>("pressed");
            chosenGate = null;
            DashCollisionResults result = orig(self, player, direction);
            if (!wasPressed && data.Get<bool>("pressed"))
            {
                OpenFlagGates(self, data, false);
            }
            chosenGate = null;
            return result;
        }

        private static void onDashSwitchAwake(On.Celeste.DashSwitch.orig_Awake orig, DashSwitch self, Scene scene)
        {
            chosenGate = null;
            orig(self, scene);
            DynamicData data = new DynamicData(self);
            if (data.Get<bool>("pressed"))
            {
                OpenFlagGates(self, data, true);
            }
            chosenGate = null;
        }

        private static void OpenFlagGates(DashSwitch sw, DynamicData data, bool instant)
        {
            if (data.Get<bool>("allGates"))
            {
                EntityID id = data.Get<EntityID>("id");
                foreach (CustomTempleGate gate in sw.Scene.Tracker.GetEntities<CustomTempleGate>()
                    .Cast<CustomTempleGate>()
                    .Where(g => g.ControlledBySwitch && g.LevelID == id.Level))
                {
                    gate.SwitchOpen(instant);
                }
            }
            else
            {
                chosenGate?.SwitchOpen(instant);
            }
        }

        public override void Awake(Scene scene)
        {
            base.Awake(scene);
            drawHeight = Math.Max(4f, Height);
            if ((!startOpen && FlagActive) || (startOpen && !FlagActive) || (openOnHeartCollection && SceneAs<Level>().Session.HeartGem))
            {
                StartOpen();
            }
            else
            {
                SetHeight(closedHeight);
                drawHeight = Math.Max(4f, horizontal ? Width : Height);
            }
        }

        public void SwitchOpen(bool instant = false)
        {
            openedBySwitch = true;
            if (instant && !open)
            {
                StartOpen();
            }
        }

        public void Open()
        {
            Collidable = false;
            if (!silent)
            {
                Audio.Play("event:/game/05_mirror_temple/gate_main_open", Position);
            }
            drawHeightMoveSpeed = 200f;
            drawHeight = horizontal ? Width : Height;
            shaker.ShakeFor(0.2f, removeOnFinish: false);
            SetHeight(0);
            sprite.Play("open");
            open = true;
        }

        public void Close()
        {
            Collidable = true;
            if (!silent)
            {
                Audio.Play("event:/game/05_mirror_temple/gate_main_close", Position);
            }
            drawHeightMoveSpeed = 300f;
            drawHeight = Math.Max(4f, horizontal ? Width : Height);
            shaker.ShakeFor(0.2f, removeOnFinish: false);
            SetHeight(closedHeight);
            sprite.Play("hit");
            open = false;
        }

        public void StartOpen()
        {
            Collidable = false;
            SetHeight(0);
            drawHeight = 4f;
            open = true;
        }

        private void SetHeight(int height)
        {
            if (!horizontal)
            {
                if (height < Collider.Height)
                {
                    Collider.Height = height;
                    return;
                }
                float y = Y;
                int num = (int)Collider.Height;
                if (Collider.Height < 64f)
                {
                    Y -= 64f - Collider.Height;
                    Collider.Height = 64f;
                }
                MoveVExact(height - num);
                Y = y;
                Collider.Height = height;

            }
            else if (height < Collider.Width)
            {
                Collider.Width = height;
            }
            else
            {
                float x = X;
                int num = (int)Collider.Width;
                if (Collider.Width < 64f)
                {
                    X -= 64f - Collider.Width;
                    Collider.Width = 64f;
                }
                MoveHExact(height - num);
                X = x;
                Collider.Width = height;
            }
            if (horizontal)
            {
                Collider.Height = 8f;
            }
        }

        public override void Update()
        {
            base.Update();
            float num = Math.Max(4f, horizontal ? Width : Height);
            if (drawHeight != num)
            {
                drawHeight = Calc.Approach(drawHeight, num, drawHeightMoveSpeed * Engine.DeltaTime);
            }
            if (((!startOpen && (FlagActive)) || (startOpen && (!FlagActive)) || (openOnHeartCollection && SceneAs<Level>().Session.HeartGem)) && !open)
            {
                Open();
            }
            else if (!openOnHeartCollection)
            {
                if (((!startOpen && !FlagActive) || (startOpen && FlagActive)) && open)
                {
                    Close();
                }
            }
            else if (openOnHeartCollection)
            {
                if (((!startOpen && !FlagActive) || (startOpen && FlagActive)) && !SceneAs<Level>().Session.HeartGem && open)
                {
                    Close();
                }
            }
        }

        public override void Render()
        {
            if (horizontal)
            {
                Vector2 value = new(0f, Math.Sign(shaker.Value.Y));
                sprite.DrawSubrect(Vector2.Zero + value, new Rectangle(0, (int)(sprite.Height - drawHeight), (int)sprite.Width, (int)drawHeight));
            }
            else
            {
                Vector2 value = new(Math.Sign(shaker.Value.X), 0f);
                sprite.DrawSubrect(Vector2.Zero + value, new Rectangle(0, (int)(sprite.Height - drawHeight), (int)sprite.Width, (int)drawHeight));
            }
        }

        public override void DebugRender(Camera camera)
        {
            if (Collidable || !horizontal)
            {
                base.DebugRender(camera);
            }
        }
    }
}
