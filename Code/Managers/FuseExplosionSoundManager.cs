using System.Collections;
using Monocle;

namespace Celeste.Mod.XaphanHelper.Managers
{
    [Tracked(true)]
    class FuseExplosionSoundManager : Entity
    {
        public Coroutine CooldownRoutine = new();

        public FuseExplosionSoundManager()
        {
            Tag = Tags.TransitionUpdate;
        }

        public override void Added(Scene scene)
        {
            base.Added(scene);
            if (CooldownRoutine.Active)
            {
                CooldownRoutine.Cancel();
            }
        }

        public override void Update()
        {
            base.Update();
            if (SceneAs<Level>().Transitioning && CooldownRoutine.Active)
            {
                CooldownRoutine.Cancel();
            }
        }

        public void SetCooldown(float cooldown)
        {
            Add(CooldownRoutine = new Coroutine(ExplosionSoundCooldownRoutine(cooldown)));
        }

        private IEnumerator ExplosionSoundCooldownRoutine(float cooldown)
        {
            while (cooldown > 0)
            {
                cooldown -= Engine.DeltaTime;
                yield return null;
            }
        }
    }
}
