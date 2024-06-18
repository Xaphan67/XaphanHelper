using System;
using System.Collections;
using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste.Mod.XaphanHelper.Entities
{
    [CustomEntity("XaphanHelper/Shutter")]
    class Shutter : Solid
    {
        public class ShutterLightWall : Solid
        {
            private Sprite top;

            private BloomPoint bloom;

            private Shutter shutter;
            public ShutterLightWall(Shutter shutter, Vector2 offset) : base(shutter.Position + offset, 8, 8, safe: true)
            {
                this.shutter = shutter;
                Collider = new Hitbox(8f, 8f);
                Add(bloom = new BloomPoint(0.5f, 8f));
                Add(top = new Sprite(GFX.Game, shutter.directory + "/"));
                top.Add("off", "top", 0, 0);
                top.Add("on", "top", 0, 1);
                top.CenterOrigin();
                top.Position = new Vector2(4);
                top.Play("off");
                if (shutter.direction == "Bottom")
                {
                    bloom.Position = new Vector2(4, 6);
                }
                else if (shutter.direction == "Top")
                {
                    top.FlipY = true;
                    bloom.Position = new Vector2(4, 2);
                }
                else if (shutter.direction == "Left")
                {
                    top.Rotation = (float)Math.PI / 2f;
                    bloom.Position = new Vector2(2, 4);
                }
                else if (shutter.direction == "Right")
                {
                    top.Rotation = -(float)Math.PI / 2f;
                    bloom.Position = new Vector2(6, 4);
                }
                Add(new LightOcclude(1f));
            }

            public override void Update()
            {
                base.Update();
                if (shutter.openOffset > 0 && shutter.openOffset != shutter.MaxLength)
                {
                    top.Play("on");
                }
                else
                {
                    top.Play("off");
                }
            }

            public override void Render()
            {
                base.Render();
                top.Render();
            }
        }

        private Sprite gate;

        private Coroutine GateRoutine = new();

        private float openOffset;

        private float MaxLength;

        private Vector2 startPosition;

        private string direction;

        private int length;

        private int speed;

        private string directory;

        private string flag;

        private bool open;

        private bool closed;

        private bool opening;

        private bool closing;

        private bool silent;

        private string sound;

        private bool startOpen;

        public Shutter(EntityData data, Vector2 offset) : base(data.Position + offset, data.Width, data.Height, safe: false)
        {
            direction = data.Attr("direction", "Bottom");
            directory = data.Attr("directory", "objects/XaphanHelper/Shutter");
            if (string.IsNullOrEmpty(directory))
            {
                directory = "objects/XaphanHelper/Shutter";
            }
            flag = data.Attr("flag");
            length = data.Int("length", 4) * 8;
            silent = data.Bool("silent", false);
            sound = data.Attr("sound", "");
            if (string.IsNullOrEmpty(sound))
            {
                sound = "event:/game/xaphan/shutter";
            }
            speed = data.Int("speed", 30);
            startOpen = data.Bool("startOpen", false);
            Add(gate = new Sprite(GFX.Game, directory + "/"));
            gate.Add("gate", "gate", 0);
            gate.CenterOrigin();
            gate.Play("gate");
            if (direction == "Bottom")
            {
                Collider = new Hitbox(8, length + 1);
                Collider.Position += Vector2.UnitY * 7;
            }
            else if (direction == "Top")
            {
                Collider = new Hitbox(8, length + 1);
                gate.FlipY = true;
            }
            else if (direction == "Left")
            {
                Collider = new Hitbox(length + 1, 8);
                gate.Rotation = (float)Math.PI / 2f;
            }
            else if (direction == "Right")
            {
                Collider = new Hitbox(length + 1, 8);
                gate.Rotation = -(float)Math.PI / 2f;
                Collider.Position += Vector2.UnitX * 7;
            }
            MaxLength = length + 1;
            startPosition = Position;
            Add(new LightOcclude());
            closed = true;
        }

        public override void Added(Scene scene)
        {
            base.Added(scene);
            if (direction == "Bottom" || direction == "Right")
            {
                SceneAs<Level>().Add(new ShutterLightWall(this, Vector2.Zero));
            }
            else if (direction == "Top")
            {
                SceneAs<Level>().Add(new ShutterLightWall(this, Vector2.UnitY * length));
            }
            else if (direction == "Left")
            {
                SceneAs<Level>().Add(new ShutterLightWall(this, Vector2.UnitX * length));
            }
            if (startOpen || (!string.IsNullOrEmpty(flag) && SceneAs<Level>().Session.GetFlag(flag)))
            {
                openOffset = MaxLength;
                open = true;
                closed = false;
                if (direction == "Bottom")
                {
                    Collider.Height = 0f;
                }
                else if (direction == "Top")
                {
                    Collider.Height = 0f;
                    Collider.Position.Y += MaxLength;
                }
                else if (direction == "Left")
                {
                    Collider.Width = 0f;
                    Collider.Position.X += MaxLength;
                }
                else if (direction == "Right")
                {
                    Collider.Width = 0f;
                }
            }
        }

        public override void Update()
        {
            base.Update();
            Player player = SceneAs<Level>().Tracker.GetEntity<Player>();
            if (!GateRoutine.Active)
            {
                if (startOpen)
                {
                    if (!string.IsNullOrEmpty(flag) && SceneAs<Level>().Session.GetFlag(flag) && (open || opening))
                    {
                        Add(GateRoutine = new Coroutine(Close(player)));
                    }
                    else if (!string.IsNullOrEmpty(flag) && !SceneAs<Level>().Session.GetFlag(flag) && (closed || closing))
                    {
                        Add(GateRoutine = new Coroutine(Open()));
                    }
                }
                else
                {
                    if (!string.IsNullOrEmpty(flag) && SceneAs<Level>().Session.GetFlag(flag) && (closed || closing))
                    {
                        Add(GateRoutine = new Coroutine(Open()));
                    }
                    else if (!string.IsNullOrEmpty(flag) && !SceneAs<Level>().Session.GetFlag(flag) && (open || opening))
                    {
                        Add(GateRoutine = new Coroutine(Close(player)));
                    }
                }
            }
        }

        private IEnumerator Open()
        {
            bool flagCheck = startOpen ? !SceneAs<Level>().Session.GetFlag(flag) : SceneAs<Level>().Session.GetFlag(flag);
            while (!string.IsNullOrEmpty(flag) && !flagCheck)
            {
                yield return null;
            }
            if (!silent)
            {
                Audio.Play(sound, Position);
            }
            closed = false;
            closing = false;
            opening = true;
            if (direction == "Bottom")
            {
                while (Collider.Height > 0f && !string.IsNullOrEmpty(flag) && flagCheck)
                {
                    Collider.Position.Y += Engine.DeltaTime * speed;
                    Collider.Height -= Engine.DeltaTime * speed;
                    openOffset += Engine.DeltaTime * speed;
                    MoveV(Engine.DeltaTime * -speed);
                    yield return null;
                }
            }
            else if (direction == "Top")
            {
                while (Collider.Height > 0f && !string.IsNullOrEmpty(flag) && flagCheck)
                {
                    Collider.Height -= Engine.DeltaTime * speed;
                    openOffset += Engine.DeltaTime * speed;
                    MoveV(Engine.DeltaTime * speed);
                    yield return null;
                }
            }
            else if (direction == "Left")
            {
                while (Collider.Width > 0f && !string.IsNullOrEmpty(flag) && flagCheck)
                {
                    Collider.Width -= Engine.DeltaTime * speed;
                    openOffset += Engine.DeltaTime * speed;
                    MoveH(Engine.DeltaTime * speed);
                    yield return null;
                }
            }
            else if (direction == "Right")
            {
                while (Collider.Width > 0f && !string.IsNullOrEmpty(flag) && flagCheck)
                {
                    Collider.Position.X += Engine.DeltaTime * speed;
                    Collider.Width -= Engine.DeltaTime * speed;
                    openOffset += Engine.DeltaTime * speed;
                    MoveH(Engine.DeltaTime * -speed);
                    yield return null;
                }
            }
            if (openOffset >= MaxLength)
            {
                openOffset = MaxLength;
            }
            opening = false;
            open = true;
        }

        private IEnumerator Close(Player player)
        {
            bool flagCheck = startOpen ? !SceneAs<Level>().Session.GetFlag(flag) : SceneAs<Level>().Session.GetFlag(flag);
            while (!string.IsNullOrEmpty(flag) && flagCheck)
            {
                yield return null;
            }
            if (!silent)
            {
                Audio.Play(sound, Position);
            }
            open = false;
            opening = false;
            closing = true;
            if (direction == "Bottom")
            {
                while (Collider.Height < MaxLength && !string.IsNullOrEmpty(flag) && !flagCheck)
                {
                    Collider.Position.Y -= Engine.DeltaTime * speed;
                    Collider.Height += Engine.DeltaTime * speed;
                    openOffset -= Engine.DeltaTime * speed;
                    MoveV(Engine.DeltaTime * speed);
                    yield return null;
                }
            }
            else if (direction == "Top")
            {
                while (Collider.Height < MaxLength && !string.IsNullOrEmpty(flag) && !flagCheck)
                {
                    Collider.Height += Engine.DeltaTime * speed;
                    openOffset -= Engine.DeltaTime * speed;
                    MoveV(Engine.DeltaTime * -speed);
                    yield return null;
                }
            }
            else if (direction == "Left")
            {
                while (Collider.Width < MaxLength && !string.IsNullOrEmpty(flag) && !flagCheck)
                {
                    Collider.Width += Engine.DeltaTime * speed;
                    openOffset -= Engine.DeltaTime * speed;
                    MoveH(Engine.DeltaTime * -speed);
                    yield return null;
                }
            }
            else if (direction == "Right")
            {
                while (Collider.Width < MaxLength && !string.IsNullOrEmpty(flag) && !flagCheck)
                {
                    Collider.Position.X -= Engine.DeltaTime * speed;
                    Collider.Width += Engine.DeltaTime * speed;
                    openOffset -= Engine.DeltaTime * speed;
                    MoveH(Engine.DeltaTime * speed);
                    yield return null;
                }
            }
            if (openOffset <= 0)
            {
                openOffset = 0;
            }
            closing = false;
            closed = true;
        }

        public override void Render()
        {
            for (int i = 0; i <= MaxLength / 8; i++)
            {
                if (direction == "Bottom")
                {
                    gate.RenderPosition = startPosition + new Vector2(4, 4 + i * 8 - openOffset);
                    if (gate.RenderPosition.Y > 4 + startPosition.Y)
                    {
                        gate.Render();
                    }
                }
                else if (direction == "Top")
                {
                    gate.RenderPosition = startPosition + new Vector2(4, 4 + i * 8 + openOffset);
                    if (gate.RenderPosition.Y < 4 + startPosition.Y + length)
                    {
                        gate.Render();
                    }
                }
                else if (direction == "Left")
                {
                    gate.RenderPosition = startPosition + new Vector2(4 + i * 8 + openOffset, 4);
                    if (gate.RenderPosition.X < 4 + startPosition.X + length)
                    {
                        gate.Render();
                    }
                }
                else if (direction == "Right")
                {
                    gate.RenderPosition = startPosition + new Vector2(4 + i * 8 - openOffset, 4);
                    if (gate.RenderPosition.X > 4 + startPosition.X)
                    {
                        gate.Render();
                    }
                }
            }
        }
    }
}
