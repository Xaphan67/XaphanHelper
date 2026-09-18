using Monocle;
using Microsoft.Xna.Framework;
using Celeste.Mod.Entities;

namespace Celeste.Mod.XaphanHelper.Entities
{
    [Tracked(true)]
    [CustomEntity("XaphanHelper/GrapplePoint")]
    public class GrapplePoint : Solid
    {
        private Sprite sprite;

        public GrapplePoint(EntityData data, Vector2 position) : base(data.Position + position, 8, 8, safe: true)
        {
            Collider = new Hitbox(8f, 8f);
            Add(sprite = new Sprite(GFX.Game, "objects/XaphanHelper/GrapplePoint/"));
            sprite.Add("block", "block", 0);
            sprite.Add("node", "node", 0);
            sprite.Play(data.Bool("notSolid") ? "node" : "block");
            Collidable = !data.Bool("notSolid");
            Depth = 100;
        }
    }
}
