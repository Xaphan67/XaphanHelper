using System.Linq;
using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;
using Monocle;
using static Celeste.Mod.XaphanHelper.Entities.Grapple;

namespace Celeste.Mod.XaphanHelper.Entities
{
    [Tracked(true)]
    [CustomEntity("XaphanHelper/GrapplePoint")]
    public class GrapplePoint : Solid
    {
        private Sprite sprite;

        private Sprite spark;

        public GrapplePoint(EntityData data, Vector2 position) : base(data.Position + position, 8, 8, safe: true)
        {
            Collider = new Hitbox(8f, 8f);
            string directory = data.Attr("directory", "objects/XaphanHelper/GrapplePoint");
            Add(sprite = new Sprite(GFX.Game, directory + "/"));
            sprite.Add("block", "block", 0);
            sprite.Add("node", "node", 0);
            sprite.Play(data.Bool("notSolid") ? "node" : "block");
            Add(spark = new Sprite(GFX.Game, directory + "/"));
            spark.AddLoop("spark", "spark", 0.08f, 0, 1, 2, 1);
            spark.Visible = false;
            spark.Play("spark");
            Collidable = !data.Bool("notSolid");
            Depth = 100;
        }

        public override void Update()
        {
            base.Update();
            Grapple grapple = SceneAs<Level>().Tracker.GetEntity<Grapple>();
            if (grapple != null && (grapple.State == States.Attached || grapple.State == States.Reached))
            {
                Rectangle zone = Collider.Bounds;
                zone.Inflate(1, 1);
                SetSparks(zone.Intersects(grapple.Collider.Bounds));
            }
            else
            {
                SetSparks(false);
            }
        }

        public void SetSparks(bool attached)
        {
            spark.Visible = attached;
        }

        public override void Render()
        {
            if (!Collidable)
            {
                sprite.DrawOutline();
            }
            base.Render();
        }
    }
}
