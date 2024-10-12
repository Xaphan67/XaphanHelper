using System;
using Celeste.Mod.XaphanHelper.Entities;
using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste.Mod.XaphanHelper.Colliders
{
    [Tracked(false)]
    public class WeaponCollider : Component
    {
        public Action<Beam> OnCollideBeam;

        public Action<Missile> OnCollideMissile;

        public Collider Collider;

        public WeaponCollider(Action<Beam> onCollideBeam, Action<Missile> onCollideMissile, Collider collider = null) : base(active: false, visible: false)
        {
            OnCollideBeam = onCollideBeam;
            OnCollideMissile = onCollideMissile;
            Collider = collider;
        }

        public bool Check(Beam beam)
        {
            Collider collider = Collider;
            if (collider == null)
            {
                if (beam.CollideCheck(Entity))
                {
                    OnCollideBeam(beam);
                    return true;
                }
                return false;
            }
            Collider collider2 = Entity.Collider;
            Entity.Collider = collider;
            bool flag = beam.CollideCheck(Entity);
            Entity.Collider = collider2;
            if (flag)
            {
                OnCollideBeam(beam);
                return true;
            }
            return false;
        }

        public bool Check(Missile missile)
        {
            Collider collider = Collider;
            if (collider == null)
            {
                if (missile.CollideCheck(Entity))
                {
                    OnCollideMissile(missile);
                    return true;
                }
                return false;
            }
            Collider collider2 = Entity.Collider;
            Entity.Collider = collider;
            bool flag = missile.CollideCheck(Entity);
            Entity.Collider = collider2;
            if (flag)
            {
                OnCollideMissile(missile);
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
