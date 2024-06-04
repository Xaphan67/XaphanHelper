using System.Collections;
using Monocle;
using Celeste.Mod.XaphanHelper.Entities;

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

        public static void Load()
        {
            Everest.Events.Level.OnLoadLevel += modOnLevelLoad;
        }

        public static void Unload()
        {
            Everest.Events.Level.OnLoadLevel -= modOnLevelLoad;
        }

        private static void modOnLevelLoad(Level level, Player.IntroTypes playerIntro, bool isFromLoader)
        {
            level.Add(new FuseExplosionSoundManager());
        }

        public override void Added(Scene scene)
        {
            base.Added(scene);
            Add(new Coroutine(ExplosionSoundCooldownRoutine()));
        }

        public void PlaySound(Fuse.FuseSection section)
        {
            if (explosionSoundCooldown <= 0)
            {
                Audio.Play("event:/game/xaphan/explosion", section.Position);
                explosionSoundCooldown = 0.1f;
            }
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
