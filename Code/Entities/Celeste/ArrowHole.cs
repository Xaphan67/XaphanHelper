
using System;
using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste.Mod.XaphanHelper.Entities
{
    [Tracked(true)]
    [CustomEntity("XaphanHelper/ArrowHole")]
    class ArrowHole : Entity
    {
        private string side;

        private Sprite sprite;

        private string directory;

        public ArrowHole(EntityData data, Vector2 offset) : base(data.Position + offset)
        {
            side = data.Attr("side", "Left");
            directory = data.Attr("directory", "objects/XaphanHelper/ArrowHole");
            Add(sprite = new Sprite(GFX.Game, directory + "/"));
            sprite.Add("hole", "hole", 0f);
            sprite.Origin = new Vector2(sprite.Width / 2, sprite.Height / 2);
            sprite.Play("hole");
            if (side == "Left")
            {
                Collider = new Hitbox(1, 2, 0, 7);
                sprite.Position = new Vector2(4f, 8f);
                sprite.Rotation = -(float)Math.PI / 2f;
            }
            else if (side == "Right")
            {
                Collider = new Hitbox(1, 2, 7, 7);
                sprite.Position = new Vector2(4f, 8f);
                sprite.Rotation = (float)Math.PI / 2f;
            }
            else if (side == "Top")
            {
                Collider = new Hitbox(2, 1, 7, 7);
                sprite.Position = new Vector2(8f, 4f);
                sprite.Rotation = (float)Math.PI;
            }
            else if (side == "Bottom")
            {
                Collider = new Hitbox(2, 1, 7, 0);
                sprite.Position = new Vector2(8f, 4f);
            }
            Depth = -15000;
        }

        public override void Render()
        {
            base.Render();
        }
    }
}
