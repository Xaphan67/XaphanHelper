using System;
using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;
using Monocle;


namespace Celeste.Mod.XaphanHelper.Entities
{
    [Tracked(true)]
    [CustomEntity("XaphanHelper/FlagTempleGate")]
    class FlagTempleGate : Solid
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

        public FlagTempleGate(EntityData data, Vector2 offset) : base(data.Position + offset, data.Width, data.Height, true)
        {
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

        public override void Awake(Scene scene)
        {
            base.Awake(scene);
            drawHeight = Math.Max(4f, Height);
            if ((!startOpen && SceneAs<Level>().Session.GetFlag(flag)) || (startOpen && !SceneAs<Level>().Session.GetFlag(flag)) || (openOnHeartCollection && SceneAs<Level>().Session.HeartGem))
            {
                StartOpen();
            }
            else
            {
                SetHeight(closedHeight);
                drawHeight = Math.Max(4f, horizontal ? Width : Height);
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
            if (((!startOpen && (SceneAs<Level>().Session.GetFlag(flag))) || (startOpen && (!SceneAs<Level>().Session.GetFlag(flag))) || (openOnHeartCollection && SceneAs<Level>().Session.HeartGem)) && !open)
            {
                Open();
            }
            else if (!openOnHeartCollection)
            {
                if (((!startOpen && !SceneAs<Level>().Session.GetFlag(flag)) || (startOpen && SceneAs<Level>().Session.GetFlag(flag))) && open)
                {
                    Close();
                }
            }
            else if (openOnHeartCollection)
            {
                if (((!startOpen && !SceneAs<Level>().Session.GetFlag(flag)) || (startOpen && SceneAs<Level>().Session.GetFlag(flag))) && !SceneAs<Level>().Session.HeartGem && open)
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
