using System;
using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste.Mod.XaphanHelper.Entities
{
    [Tracked(true)]
    [CustomEntity("XaphanHelper/Detonator")]
    class Detonator : Solid
    {
        private Sprite sprite;

        private string directory;

        private string side;

        private float speed;

        private PlayerCollider pc;

        private bool pressed;

        private bool wasPressed;

        public Detonator (EntityData data, Vector2 position) : base(data.Position + position, data.Width, data.Height, true)
        {
            directory = data.Attr("directory");
            side = data.Attr("side", "Up");
            speed = data.Float("speed", 0.1f);
            Add(sprite = new Sprite(GFX.Game, directory + "/"));
            sprite.Add("idle", "idle", 0);
            sprite.Add("pressed", "pressed", 0);
            sprite.Play("idle");
            sprite.CenterOrigin();
            switch (side)
            {
                case "Up":
                    Collider = new Hitbox(14, 4, -7, 4);
                    Add(pc = new PlayerCollider(onPlayer, new Hitbox(12, 1, -6, -1)));
                    break;
                case "Down":
                    sprite.Rotation = (float)-Math.PI;
                    sprite.FlipX = true;
                    Collider = new Hitbox(14, 4, -7, -8);
                    Add(pc = new PlayerCollider(onPlayer, new Hitbox(12, 1, -6, 0)));
                    break;
                case "Left":
                    sprite.Rotation = (float)-Math.PI / 2;
                    sprite.FlipX = true;
                    Collider = new Hitbox(4, 14, 4, -7);
                    Add(pc = new PlayerCollider(onPlayer, new Hitbox(1, 12, -1, -6)));
                    break;
                case "Right":
                    sprite.Rotation = (float)Math.PI /2;
                    Collider = new Hitbox(4, 14, -8, -7);
                    Add(pc = new PlayerCollider(onPlayer, new Hitbox(1, 12, 0, -6)));
                    break;
            }
            Depth = 100;
        }

        public static void Load()
        {
            On.Celeste.Solid.MoveVExact += OnSolidMoveVExact;
            On.Celeste.Solid.MoveHExact += OnSolidMoveHExact;
        }

        public static void Unload()
        {
            On.Celeste.Solid.MoveVExact -= OnSolidMoveVExact;
            On.Celeste.Solid.MoveHExact -= OnSolidMoveHExact;
        }

        private static void OnSolidMoveVExact(On.Celeste.Solid.orig_MoveVExact orig, Solid self, int move)
        {
            orig(self, move);
            if (self.Scene.CollideCheck<Detonator>(new Rectangle((int)self.X + 1, move >= 1 ? (int)self.Y + (int)self.Height + 5 : (int)self.Y - 5, (int)self.Width - 2, 1)))
            {
                Detonator detonator = self.CollideFirst<Detonator>(self.Position + Vector2.UnitY * (move >= 1 ? 5 : -5));
                if (detonator != null && (move >= 1 ? detonator.side == "Up" : detonator.side == "Down") && !detonator.pressed)
                {
                    detonator.pressed = true;
                    detonator.sprite.Play("pressed");
                }
            }
        }

        private static void OnSolidMoveHExact(On.Celeste.Solid.orig_MoveHExact orig, Solid self, int move)
        {
            orig(self, move);
            if (self.Scene.CollideCheck<Detonator>(new Rectangle(move >= 1 ? (int)self.X + (int)self.Width + 5 : (int)self.X - 5, (int)self.Y + 1, 1, (int)self.Height - 2)))
            {
                Detonator detonator = self.CollideFirst<Detonator>(self.Position + Vector2.UnitX * (move >= 1 ? 5 : -5));
                if (detonator != null && (move >= 1 ? detonator.side == "Left" : detonator.side == "Right") && !detonator.pressed)
                {
                    detonator.pressed = true;
                    detonator.sprite.Play("pressed");
                }
            }
        }

        public override void Update()
        {
            base.Update();
            if (side == "Left" || side == "Right")
            {
                DisplacePlayerOnTop();
            }
            if (pressed && !wasPressed)
            {
                wasPressed = true;
                Fuse.FuseSection fuse = null;
                switch (side)
                {
                    case "Up":
                        if (Scene.CollideCheck<Fuse.FuseSection>(new Rectangle((int)X, (int)Y + 8, 1, 1)))
                        {
                            fuse = CollideFirst<Fuse.FuseSection>(Position + Vector2.UnitY);
                        }
                        break;
                    case "Down":
                        if (Scene.CollideCheck<Fuse.FuseSection>(new Rectangle((int)X, (int)Y - 16, 1, 1)))
                        {
                            fuse = CollideFirst<Fuse.FuseSection>(Position - Vector2.UnitY);
                        }
                        break;
                    case "Left":
                        if (Scene.CollideCheck<Fuse.FuseSection>(new Rectangle((int)X + 8, (int)Y, 1, 1)))
                        {
                            fuse = CollideFirst<Fuse.FuseSection>(Position + Vector2.UnitX);
                        }
                        break;
                    case "Right":
                        if (Scene.CollideCheck<Fuse.FuseSection>(new Rectangle((int)X - 16, (int)Y, 1, 1)))
                        {
                            fuse = CollideFirst<Fuse.FuseSection>(Position - Vector2.UnitX);
                        }
                        break;
                };
                if (fuse != null)
                {
                    fuse.speed = speed;
                    fuse.shouldTrigger = true;
                }
            }
        }

        private void DisplacePlayerOnTop()
        {
            if (!HasPlayerOnTop())
            {
                return;
            }
            Player player = GetPlayerOnTop();
            if (player == null)
            {
                return;
            }
            else if (player.Bottom == Top && player.Speed.Y >= 0)
            {
                if (side == "Left")
                {
                    if (player.Left >= Left)
                    {
                        player.Left = Right;
                        player.Y += 1f;
                    }
                }
                else if (player.Right <= Right)
                {
                    player.Right = Left;
                    player.Y += 1f;
                }
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
