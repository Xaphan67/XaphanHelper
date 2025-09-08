using System.Collections;
using Celeste.Mod.XaphanHelper.Entities;
using Celeste.Mod.XaphanHelper.UI_Elements;
using Celeste.Mod.XaphanHelper.Upgrades;
using FMOD.Studio;
using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste.Mod.XaphanHelper.Managers
{
    [Tracked(true)]
    class HeatManager : Entity
    {
        public float heat;

        public float maxDuration;

        public string inactiveFlag;

        public Coroutine HeatDamageRoutine = new();

        public static bool damageSfxPlay;

        public static EventInstance damageSfx;

        private bool resetDuration;

        public HeatManager(float maxDuration, string inactiveFlag, bool resetDuration)
        {
            Tag = Tags.TransitionUpdate;
            this.maxDuration = maxDuration;
            this.inactiveFlag = inactiveFlag;
            this.resetDuration = resetDuration;
            Depth = -20000;
        }

        public void updateMaxDuration(float newDuration)
        {
            maxDuration = newDuration;
        }

        public override void Update()
        {
            base.Update();
            Level level = SceneAs<Level>();
            Player player = Scene.Tracker.GetEntity<Player>();
            if (!XaphanModule.UIOpened && (string.IsNullOrEmpty(inactiveFlag) || (!string.IsNullOrEmpty(inactiveFlag) && !level.Session.GetFlag(inactiveFlag))))
            {
                if (!XaphanModule.useMetroidGameplay)
                {
                    if (!level.Transitioning)
                    {
                        if (player != null && player.CanRetry && !VariaJacket.Active(level) && !XaphanModule.PlayerIsControllingRemoteDrone())
                        {
                            heat += Engine.DeltaTime;
                            if (level.Paused && damageSfxPlay)
                            {
                                damageSfxPlay = false;
                            }
                            if (!damageSfxPlay && !SaveData.Instance.Assists.Invincible)
                            {
                                damageSfxPlay = true;
                                damageSfx = Audio.Play("event:/game/xaphan/heat_damage");
                            }
                            if (heat >= maxDuration && !player.Dead)
                            {
                                player.Die(Vector2.Zero);
                            }
                        }
                        if (player == null)
                        {
                            StopSfx();
                            return;
                        }
                        if ((VariaJacket.Active(level) || XaphanModule.PlayerIsControllingRemoteDrone()) && heat > 0)
                        {
                            heat -= Engine.DeltaTime;
                        }
                        if (heat < 0)
                        {
                            heat = 0;
                        }
                    }
                    else
                    {
                        StopSfx();
                        return;
                    }
                }
                else
                {
                    if (player != null && player.CanRetry && !level.Transitioning && !VariaJacket.Active(level) && !XaphanModule.PlayerIsControllingRemoteDrone() && !HeatDamageRoutine.Active)
                    {
                        Add(HeatDamageRoutine = new Coroutine(HeatDamage()));
                    }
                }
            }
            else
            {
                StopSfx();
                if (resetDuration)
                {
                    heat = 0;
                }
                else
                {
                    heat -= Engine.DeltaTime;
                }
            }
        }

        public bool LiquidDamageRoutineActive()
        {
            foreach (Liquid liquid in SceneAs<Level>().Tracker.GetEntities<Liquid>())
            {
                if (liquid.LiquidDamageRoutine.Active)
                {
                    return true;
                }
            }
            return false;
        }

        private IEnumerator HeatDamage()
        {
            HealthDisplay healthDisplay = SceneAs<Level>().Tracker.GetEntity<HealthDisplay>();
            while (healthDisplay != null && healthDisplay.CurrentHealth > 0 && !VariaJacket.Active(SceneAs<Level>()) && !SceneAs<Level>().Transitioning && !SceneAs<Level>().FrozenOrPaused && !XaphanModule.UIOpened && !SceneAs<Level>().Session.GetFlag(inactiveFlag))
            {
                healthDisplay.playDamageSfx();
                healthDisplay.CurrentHealth -= 1;
                healthDisplay.GetEnergyTanks();
                float tickTimer = 0.066f;
                while (tickTimer > 0)
                {
                    tickTimer -= Engine.DeltaTime;
                    yield return null;
                }
            }
            if (!LiquidDamageRoutineActive())
            {
                healthDisplay.stopDamageSfx();
            }
        }

        private void StopSfx()
        {
            if (damageSfxPlay && !SaveData.Instance.Assists.Invincible)
            {
                Audio.Stop(damageSfx, false);
                damageSfxPlay = false;
            }
        }

        public override void Removed(Scene scene)
        {
            base.Removed(scene);
            StopSfx();
        }
    }
}
