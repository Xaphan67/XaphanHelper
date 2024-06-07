using System.Collections;
using Monocle;
using Celeste.Mod.XaphanHelper.Entities;
using System;

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
