using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Celeste.Mod.XaphanHelper.Colliders;
using Celeste.Mod.XaphanHelper.Entities;
using Celeste.Mod.XaphanHelper.Managers;
using Celeste.Mod.XaphanHelper.UI_Elements;
using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste.Mod.XaphanHelper.Enemies
{
    [Tracked(true)]
    public class Enemy : Actor
    {
        public int Health = 100;

        public int Damage;

        public bool Flashing;

        public bool Freezable = true;

        public float FreezeTimer = 6.66f;

        public bool Freezed;

        private PlayerBlocker playerBlocker;

        public List<Sprite> sprites = new();

        public PlayerCollider pc;

        public WeaponCollider bc;

        public List<Entity> enemyBarriers;

        public List<Entity> playerBlockers;

        private bool beamIgnore;

        private bool powerBombIgnore;

        private Coroutine BeamIgnoreRoutine = new();

        private Coroutine PowerBombIgnoreRoutine = new();

        public string DeathSound = "event:/game/xaphan/enemy_hit_screw_attack";

        public string HitSound = "event:/game/xaphan/enemy_hit_screw_attack";

        public string FreezeSound = "event:/game/xaphan/enemy_hit_screw_attack";

        public Enemy(EntityData data, Vector2 offset) : base(data.Position + offset)
        {
            Add(pc = new PlayerCollider(OnCollidePlayer));
            Add(bc = new WeaponCollider
            {
                OnCollideBeam = HitByBeam,
                OnCollideMissile = HitByMissile
            });
        }

        public void BeforeUpdate()
        {
            Slope.SetCollisionBeforeUpdate(this);
            enemyBarriers = Scene.Tracker.GetEntities<EnemyBarrier>().ToList();
            playerBlockers = Scene.Tracker.GetEntities<PlayerBlocker>().ToList();
            enemyBarriers.ForEach(entity => entity.Collidable = true);
            playerBlockers.ForEach(entity => entity.Collidable = false);
        }

        public override void Update()
        {
            base.Update();
            if (beamIgnore && !BeamIgnoreRoutine.Active)
            {
                Add(BeamIgnoreRoutine = new Coroutine(ImmuneToBeamSequence()));
            }
            if (powerBombIgnore && !PowerBombIgnoreRoutine.Active)
            {
                Add(PowerBombIgnoreRoutine = new Coroutine(ImmuneToPowerBombSequence()));
            }
        }

        public void AfterUpdate()
        {
            Slope.SetCollisionAfterUpdate(this);
            enemyBarriers.ForEach(entity => entity.Collidable = false);
            playerBlockers.ForEach(entity => entity.Collidable = true);
        }

        private void OnCollidePlayer(Player player)
        {
            ScrewAttackManager manager = SceneAs<Level>().Tracker.GetEntity<ScrewAttackManager>();
            if (SceneAs<Level>().Session.GetFlag("Xaphan_Helper_Shinesparking"))
            {
                HitBySpeedBooster();
            }
            else if (manager == null || manager != null && !manager.StartedScrewAttack)
            {
                if (!XaphanModule.useMetroidGameplay)
                {
                    player.Die(new Vector2(0f, -1f));
                }
                else
                {
                    HealthDisplay healthDisplay = SceneAs<Level>().Tracker.GetEntity<HealthDisplay>();
                    if (healthDisplay != null && !healthDisplay.CannotTakeDamage && !Freezed)
                    {
                        if (player.Bottom < Bottom)
                        {
                            player.Speed = new Vector2(player.Facing == Facings.Left ? 180f : -180f, -180f);
                        }
                        else
                        {
                            player.Speed = new Vector2(player.Facing == Facings.Left ? 180f : -180f, 180f);
                        }
                        healthDisplay.UpdateHealth(-Damage);
                    }
                }
            }
        }

        private IEnumerator ImmuneToBeamSequence()
        {
            float timer = 0.2f;
            while (timer > 0)
            {
                if (Scene.OnRawInterval(0.06f) && !Freezed)
                {
                    Flashing = !Flashing;
                }
                timer -= Engine.DeltaTime;
                yield return null;
            }
            Flashing = false;
            beamIgnore = false;
        }

        private IEnumerator ImmuneToPowerBombSequence()
        {
            while (CollideCheck<PowerBomb>())
            {
                yield return null;
            }
            powerBombIgnore = false;
        }

        private IEnumerator HitFlashingSequence()
        {
            float timer = 0.2f;
            while (timer > 0)
            {
                if (Scene.OnRawInterval(0.06f) && !Freezed)
                {
                    Flashing = !Flashing;
                }
                timer -= Engine.DeltaTime;
                yield return null;
            }
            Flashing = false;
        }

        private void HitByBeam(Beam beam)
        {
            if (Health > 0)
            {
                if (!beamIgnore)
                {
                    if (Health <= beam.damage && beam.beamType.Contains("Ice") && Freezable && !Freezed)
                    {
                        Freezed = true;
                        Add(new Coroutine(FreezedSequence()));
                    }
                    else
                    {
                        Health -= beam.damage;
                    }
                    if (Freezed)
                    {
                        Audio.Play(FreezeSound, Center);
                    }
                    else
                    {
                        Audio.Play(HitSound, Center);
                    }
                }
                if (!beam.beamType.Contains("Plasma"))
                {
                    beam.RemoveSelf();
                }
                beamIgnore = true;
            }
            if (Health <= 0)
            {
                Die();
            }
        }

        private IEnumerator FreezedSequence()
        {
            for (int i = 0; i <= sprites.Count -1; i++)
            {
                sprites[i].Active = false;
            }

            SceneAs<Level>().Add(playerBlocker = new PlayerBlocker(Position + pc.Collider.AbsolutePosition, pc.Collider.Width, pc.Collider.Height));
            float timer = FreezeTimer;
            while (timer > 0)
            {
                if (timer <= FreezeTimer / 3)
                {
                    if (Scene.OnRawInterval(0.06f))
                    {
                        Flashing = !Flashing;
                    }
                }
                timer -= Engine.DeltaTime;
                yield return null;
            }
            SceneAs<Level>().Remove(playerBlocker);
            Flashing = false;

            for (int i = 0; i <= sprites.Count - 1; i++)
            {
                sprites[i].Active = true;
            }

            Freezed = false;
        }

        private void HitByMissile(Missile missile)
        {
            if (Health > 0)
            {
                if (!missile.SuperMissile)
                {
                    Health -= missile.damage;
                    Audio.Play(HitSound, Center);
                    missile.RemoveSelf();
                }
                else
                {
                    Health -= missile.damage;
                    SceneAs<Level>().Shake(0.3f);
                    Audio.Play(HitSound, Center);
                    missile.RemoveSelf();
                }
                Add(new Coroutine(HitFlashingSequence()));
            }
            if (Health <= 0)
            {
                Die();
            }
        }

        public void HitByMorphBomb(MorphBomb bomb)
        {
            if (Health > 0)
            {
                Health -= bomb.damage;
                Audio.Play(HitSound, Center);
                Add(new Coroutine(HitFlashingSequence()));
            }
            if (Health <= 0)
            {
                Die();
            }
        }

        public void HitByPowerBomb(PowerBomb powerBomb)
        {
            if (Health > 0)
            {
                if (!powerBombIgnore)
                {
                    Health -= powerBomb.damage;
                    Audio.Play(HitSound, Center);
                    Add(new Coroutine(HitFlashingSequence()));
                    powerBombIgnore = true;
                }
            }
            if (Health <= 0)
            {
                Die();
            }
        }

        public void HitBySpeedBooster()
        {
            Die();
        }

        public void HitByScrewAttack()
        {
            Die();
        }

        protected override void OnSquish(CollisionData data)
        {
            Die();
        }

        public void Die()
        {
            SceneAs<Level>().Add(new Explosion(Center));
            Audio.Play(DeathSound, Center);
            if (playerBlocker != null)
            {
                SceneAs<Level>().Remove(playerBlocker);
            }
            RemoveSelf();
        }

        public override void Render()
        {
            base.Render();
            if (sprites.Count() > 0)
            {
                foreach (Sprite sprite in sprites)
                {
                    sprite.DrawOutline();
                }
                foreach (Sprite sprite in sprites)
                {
                    sprite.Render();
                    if (Freezed)
                    {
                        if (Flashing)
                        {
                            sprite.Color = Calc.HexToColor("A4F9F9");
                        }
                        else
                        {
                            sprite.Color = Calc.HexToColor("60DCF8");
                        }

                    }
                    else
                    {
                        if (Flashing)
                        {
                            sprite.Color = Color.Red;
                        }
                        else
                        {
                            sprite.Color = Color.White;
                        }
                    }
                }
            }
        }
    }
}
