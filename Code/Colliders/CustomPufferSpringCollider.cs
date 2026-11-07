using System;
using System.Reflection;
using Celeste.Mod.XaphanHelper.Entities;
using Microsoft.Xna.Framework;
using Monocle;
using static Celeste.Spring;

namespace Celeste.Mod.XaphanHelper.Colliders
{
    [Tracked(false)]
    public class CustomPufferSpringCollider : Component
    {
        private static MethodInfo Spring_BounceAnimate = typeof(Spring).GetMethod("BounceAnimate", BindingFlags.Instance | BindingFlags.NonPublic);

        public Action<CustomPuffer> OnCollide;

        public Collider Collider;

        public CustomPufferSpringCollider(Action<CustomPuffer> onCollide, Collider collider = null) : base(active: false, visible: false)
        {
            OnCollide = onCollide;
            Collider = null;
        }

        public static void Load()
        {
            On.Celeste.Spring.ctor_Vector2_Orientations_bool += onSpringCtor;
        }

        public static void Unload()
        {
            On.Celeste.Spring.ctor_Vector2_Orientations_bool -= onSpringCtor;
        }

        private static void onSpringCtor(On.Celeste.Spring.orig_ctor_Vector2_Orientations_bool orig, Spring self, Vector2 position, Spring.Orientations orientation, bool playerCanUse)
        {
            orig(self, position, orientation, playerCanUse);
            CustomPufferSpringCollider pufferCollider = new(OnPuffer);
            self.Add(pufferCollider);
            switch (orientation)
            {
                case Orientations.Floor:
                    pufferCollider.Collider = new Hitbox(16f, 10f, -8f, -10f);
                    break;
                case Orientations.WallLeft:
                    pufferCollider.Collider = new Hitbox(12f, 16f, 0f, -8f);
                    break;
                case Orientations.WallRight:
                    pufferCollider.Collider = new Hitbox(12f, 16f, -12f, -8f);
                    break;
            }
        }

        private static void OnPuffer(CustomPuffer p)
        {
            Spring spring = null;
            foreach (Component item in p.SceneAs<Level>().Tracker.Components[typeof(CustomPufferSpringCollider)])
            {
                if (p.CollideCheck(item.Entity))
                {
                    spring = item.Entity as Spring;
                }
            }
            if (p.HitSpring(spring))
            {
                Spring_BounceAnimate.Invoke(spring, null);
            }
        }

        public void Check(CustomPuffer puffer)
        {
            if (OnCollide != null)
            {
                Collider collider = Entity.Collider;
                if (Collider != null)
                {
                    Entity.Collider = Collider;
                }
                if (puffer.CollideCheck(base.Entity))
                {
                    OnCollide(puffer);
                }
                Entity.Collider = collider;
            }
        }
    }
}
