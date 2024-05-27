using System;
using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste.Mod.XaphanHelper.Entities
{
    [CustomEntity("XaphanHelper/HoldableBumper")]
    class HoldableBumper : Entity
    {
        public HoldableBumper(EntityData data, Vector2 offset) : base(data.Position + offset)
        {
            Collider = new Hitbox(data.Width, 4f, -8, -3f);
            Add(new HoldableCollider(OnHoldable));
        }

        private void OnHoldable(Holdable h)
        {
            if (h.Entity.GetType() == typeof(Crate))
            {
                Crate crate = h.Entity as Crate;
                if (!crate.Hold.IsHeld && crate.OnGround())
                {
                    int num = Math.Sign(crate.X - X);
                    if (num == 0)
                    {
                        num = 1;
                    }
                    crate.Speed.X = num * 80f;
                    crate.Speed.Y = -30f;
                }
            }
            else
            {
                h.HitSpinner(this);
            }
        }
    }
}
