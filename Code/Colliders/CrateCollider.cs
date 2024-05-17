using System;
using Celeste.Mod.XaphanHelper.Entities;
using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste.Mod.XaphanHelper.Colliders
{
    [Tracked(false)]
    public class CrateCollider : Component
    {
        public Action<Crate, Spikes.Directions> OnCollide;

        public Collider Collider;

        public Spikes.Directions Direction;

        public CrateCollider(Action<Crate, Spikes.Directions> onCollide, Spikes.Directions direction, Collider collider = null) : base(active: false, visible: false)
        {
            OnCollide = onCollide;
            Collider = collider;
            Direction = direction;
        }

        public bool Check(Crate Crate)
        {
            Collider collider = Collider;
            if (collider == null)
            {
                if (Crate.CollideCheck(Entity))
                {
                    OnCollide(Crate, Direction);
                    return true;
                }
                return false;
            }
            Collider collider2 = Entity.Collider;
            Entity.Collider = collider;
            bool flag = Crate.CollideCheck(Entity);
            if (flag)
            {
                OnCollide(Crate, Direction);
                return true;
            }
            return false;
        }

        public override void DebugRender(Camera camera)
        {
            if (Collider != null)
            {
                Collider collider = Entity.Collider;
                Entity.Collider = Collider;
                Collider.Render(camera, Color.HotPink);
                Entity.Collider = collider;
            }
        }
    }
}
