using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste.Mod.XaphanHelper.Entities
{
    [CustomEntity("XaphanHelper/PressurePlate")]
    public class PressurePlate : Entity
    {
        Sprite sprite;

        string flag;

        string directory;

        public PressurePlate(EntityData data, Vector2 offset) : base(data.Position + offset)
        {
            Collider = new Hitbox(12f, 2f, 2f, 6f);
            flag = data.Attr("flag");
            directory = data.Attr("directory", "objects/XaphanHelper/PressurePlate");
            if (string.IsNullOrEmpty(directory))
            {
                directory = "objects/XaphanHelper/PressurePlate";
            }
            Add(sprite = new Sprite(GFX.Game, directory + "/"));
            sprite.AddLoop("idle", "button", 0f);
            sprite.Play("idle");
        }

        public override void Update()
        {
            base.Update();
            bool CollideActor = false;
            foreach (Actor actor in SceneAs<Level>().Tracker.GetEntities<Actor>())
            {
                if (actor.GetType() != typeof(Debris))
                {
                    if (actor.GetType() == typeof(Bomb))
                    {
                        Bomb bomb = actor as Bomb;
                        if (!bomb.explode && CollideCheck(bomb))
                        {
                            CollideActor = true;
                        }
                    }
                    else if (CollideCheck(actor))
                    {
                        CollideActor = true;
                    }
                }
            }
            if (CollideActor || CollideCheck<WorkRobot>())
            {
                sprite.Position = Vector2.UnitY;
                if (!string.IsNullOrEmpty(flag))
                {
                    SceneAs<Level>().Session.SetFlag(flag, true);
                }
            }
            else
            {
                sprite.Position = Vector2.Zero;
                if (!string.IsNullOrEmpty(flag))
                {
                    SceneAs<Level>().Session.SetFlag(flag, false);
                }
            }
        }

        public override void Render()
        {
            base.Render();
            sprite.DrawOutline();
            sprite.Render();
        }
    }
}
