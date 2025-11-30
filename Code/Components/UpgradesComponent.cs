using Monocle;

namespace Celeste.Mod.XaphanHelper.Components
{
    public class UpgradesComponent : Component
    {
        public float BombsCooldown;

        public float MegaBombsCooldown;

        public UpgradesComponent() : base(true, false)
        {
        }

        public override void Added(Entity entity)
        {
            base.Added(entity);
        }

        public override void Update()
        {
            base.Update();
            if (BombsCooldown > 0)
            {
                BombsCooldown -= Engine.DeltaTime;
            }
            else
            {
                BombsCooldown = 0f;
            }
            if (MegaBombsCooldown > 0)
            {
                MegaBombsCooldown -= Engine.DeltaTime;
            }
            else
            {
                MegaBombsCooldown = 0f;
            }
        }
    }
}
