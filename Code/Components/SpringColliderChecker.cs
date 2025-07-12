using Celeste.Mod.XaphanHelper.Colliders;
using Monocle;


namespace Celeste.Mod.XaphanHelper.Components
{
    public class SpringColliderChecker : Component
    {
        public SpringColliderChecker() : base(true, false)
        {

        }

        public override void Update()
        {
            base.Update();
            if (Entity is Spring)
            {
                Spring spring = Entity as Spring;
                foreach (SpringCollider springCollider in spring.Scene.Tracker.GetComponents<SpringCollider>())
                {
                    springCollider.Check(spring);
                }
            }
        }
    }
}
