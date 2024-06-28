using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste.Mod.XaphanHelper.Entities
{
    [Tracked(true)]
    [CustomEntity("XaphanHelper/Dynamite")]
    class Dynamite : Entity
    {
        private MTexture Texture;

        public Dynamite(EntityData data, Vector2 position) : base(data.Position + position)
        {
            Tag = Tags.TransitionUpdate;
            Collider = new Hitbox(10f, 10f, -1f, -1f);
            Texture = GFX.Game["objects/XaphanHelper/Dynamite/dynamite"];
            Depth = -20000;
        }

        public override void Update()
        {
            base.Update();
            if (!CollideCheck<Solid>())
            {
                RemoveSelf();
            }
        }

        public override void Render()
        {
            base.Render();
            Texture.Draw(Position - new Vector2(4f, 4f));
        }
    }
}
