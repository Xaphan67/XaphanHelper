using System;
using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste.Mod.XaphanHelper.Entities
{
    [CustomEntity("XaphanHelper/Detonator")]
    class Detonator : Solid
    {
        private Sprite sprite;

        private string directory;

        private string side;

        private string speed;

        private PlayerCollider pc;

        private bool pressed;

        private bool wasPressed;

        public Detonator (EntityData data, Vector2 position) : base(data.Position + position, data.Width, data.Height, true)
        {
            directory = data.Attr("directory");
            side = data.Attr("side", "Up");
            speed = data.Attr("speed", "Default");
            Add(sprite = new Sprite(GFX.Game, directory + "/"));
            sprite.Add("idle", "idle", 0);
            sprite.Add("pressed", "pressed", 0);
            sprite.Play("idle");
            sprite.CenterOrigin();
            switch (side)
            {
                case "Up":
                    Collider = new Hitbox(14, 8, -7, 0);
                    Add(pc = new PlayerCollider(onPlayer, new Hitbox(12, 1, -6, -5)));
                    break;
                case "Down":
                    sprite.Rotation = (float)-Math.PI;
                    sprite.FlipX = true;
                    Collider = new Hitbox(14, 8, -7, -8);
                    Add(pc = new PlayerCollider(onPlayer, new Hitbox(12, 1, -6, 4)));
                    break;
                case "Left":
                    sprite.Rotation = (float)-Math.PI / 2;
                    sprite.FlipX = true;
                    Collider = new Hitbox(8, 14, 0, -7);
                    Add(pc = new PlayerCollider(onPlayer, new Hitbox(1, 12, -5, -6)));
                    break;
                case "Right":
                    sprite.Rotation = (float)Math.PI /2;
                    Collider = new Hitbox(8, 14, -8, -7);
                    Add(pc = new PlayerCollider(onPlayer, new Hitbox(1, 12, 4, -6)));
                    break;
            }
            
        }

        public override void Update()
        {
            base.Update();
            if (pressed && !wasPressed)
            {
                wasPressed = true;
                switch (side)
                {
                    case "Up":
                        if (Scene.CollideCheck<Fuse.FuseSection>(new Rectangle((int)X, (int)Y + 8, 1, 1)))
                        {
                            Fuse.FuseSection fuse = CollideFirst<Fuse.FuseSection>(Position + Vector2.UnitY);
                            if (fuse != null)
                            {
                                fuse.speed = speed;
                                fuse.shouldTrigger = true;
                            }
                        }
                        break;
                    case "Down":
                        if (Scene.CollideCheck<Fuse.FuseSection>(new Rectangle((int)X, (int)Y - 16, 1, 1)))
                        {
                            Fuse.FuseSection fuse = CollideFirst<Fuse.FuseSection>(Position - Vector2.UnitY);
                            if (fuse != null)
                            {
                                fuse.speed = speed;
                                fuse.shouldTrigger = true;
                            }
                        }
                        break;
                    case "Left":
                        if (Scene.CollideCheck<Fuse.FuseSection>(new Rectangle((int)X + 8, (int)Y, 1, 1)))
                        {
                            Fuse.FuseSection fuse = CollideFirst<Fuse.FuseSection>(Position + Vector2.UnitX);
                            if (fuse != null)
                            {
                                fuse.speed = speed;
                                fuse.shouldTrigger = true;
                            }
                        }
                        break;
                    case "Right":
                        if (Scene.CollideCheck<Fuse.FuseSection>(new Rectangle((int)X - 16, (int)Y, 1, 1)))
                        {
                            Fuse.FuseSection fuse = CollideFirst<Fuse.FuseSection>(Position - Vector2.UnitX);
                            if (fuse != null)
                            {
                                fuse.speed = speed;
                                fuse.shouldTrigger = true;
                            }
                        }
                        break;
                };
            }
        }

        private void onPlayer(Player player)
        {
            sprite.Play("pressed");
            pressed = true;
            pc.Collider = null;
        }
    }
}
