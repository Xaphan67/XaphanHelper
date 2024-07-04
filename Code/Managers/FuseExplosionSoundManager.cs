using System.Collections;
using Monocle;

namespace Celeste.Mod.XaphanHelper.Managers
{
    [Tracked(true)]
    class FuseExplosionSoundManager : Entity
    {
        public float explosionSoundCooldown;

        public FuseExplosionSoundManager()
        {
            Tag = Tags.TransitionUpdate;
        }

        public override void Added(Scene scene)
        {
            base.Added(scene);
            Add(new Coroutine(ExplosionSoundCooldownRoutine()));
        }

        public void SetCooldown(float cooldown)
        {
            explosionSoundCooldown = cooldown;
        }

        private IEnumerator ExplosionSoundCooldownRoutine()
        {
            while (true)
            {
                if (explosionSoundCooldown > 0)
                {
                    explosionSoundCooldown -= Engine.DeltaTime;
                    yield return null;
                }
                else
                {
                    yield return null;
                }
            }
        }
    }
}
