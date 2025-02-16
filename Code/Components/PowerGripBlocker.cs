using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste.Mod.XaphanHelper.Components
{
    [Tracked(false)]
    public class PowerGripBlocker : Component
    {
        public bool Blocking = true;

        public bool Edge;

        public PowerGripBlocker(bool edge) : base(active: true, visible: false)
        {
            Edge = edge;
        }

        public static bool Check(Scene scene, Entity entity, Vector2 at)
        {
            bool result = Check(scene, entity);
            return result;
        }

        public static bool Check(Scene scene, Entity entity)
        {
            foreach (PowerGripBlocker component in scene.Tracker.GetComponents<PowerGripBlocker>())
            {
                if (component.Blocking && entity.CollideCheck(component.Entity))
                {
                    return true;
                }
            }
            return false;
        }

        public static bool EdgeCheck(Scene scene, Entity entity, int dir)
        {
            foreach (PowerGripBlocker component in scene.Tracker.GetComponents<PowerGripBlocker>())
            {
                if (component.Blocking && component.Edge && entity.CollideCheck(component.Entity, entity.Position + Vector2.UnitX * dir))
                {
                    return true;
                }
            }
            return false;
        }
    }
}
