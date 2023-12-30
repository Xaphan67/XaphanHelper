using System.Collections;
using Celeste.Mod.Entities;
using Celeste.Mod.XaphanHelper.UI_Elements;
using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste.Mod.XaphanHelper.Entities
{
    [Tracked(true)]
    [CustomEntity("XaphanHelper/AmmoCollectable")]
    public class AmmoCollectable : Entity
    {
        private EntityID ID;

        private string sprite;

        private string collectSound;

        private string oldMusic;

        private string newMusic;

        public string ammo;

        private Sprite collectable;

        private string inputActionA;

        private string inputActionB;

        private string poemTextA;

        private string poemTextB;

        private string poemTextC;

        private string nameColor;

        private string descColor;

        private string particleColor;

        private object controlA;

        private object controlB;

        private CustomPoem poem;

        private SoundEmitter sfx;

        private int value;

        public int index;

        public AmmoCollectable(EntityData data, Vector2 position, EntityID id) : base(data.Position + position)
        {
            ID = id;
            collectSound = data.Attr("collectSound");
            newMusic = data.Attr("newMusic");
            ammo = data.Attr("ammo");
            nameColor = data.Attr("nameColor");
            descColor = data.Attr("descColor");
            particleColor = data.Attr("particleColor");
            value = data.Int("value");
            sprite = "collectables/XaphanHelper/AmmoCollectable/" + ammo.ToLower();
            index = data.Int("index", 0);
            Collider = new Hitbox(8f, 8f);
            Add(collectable = new Sprite(GFX.Game, sprite));
            collectable.AddLoop("idle", "", 0.08f);
            collectable.Play("idle");
            Add(new PlayerCollider(OnPlayer));
        }

        private void OnPlayer(Player player)
        {
            Level level = Scene as Level;
            Add(new Coroutine(Collect(player, level)));
        }

        public override void Added(Scene scene)
        {
            base.Added(scene);
            if (index != 0)
            {
                Visible = false;
            }
            if (XaphanModule.ModSaveData.AmmoCollected.Contains(SceneAs<Level>().Session.Area.LevelSet + "_" + ID))
            {
                RemoveSelf();
            }
        }

        private IEnumerator Collect(Player player, Level level)
        {
            Visible = false;
            Collidable = false;
            Session session = SceneAs<Level>().Session;
            oldMusic = Audio.CurrentMusic;
            session.Audio.Music.Event = SFX.EventnameByHandle(collectSound);
            session.Audio.Apply(forceSixteenthNoteHack: false);
            session.DoNotLoad.Add(ID);
            XaphanModule.ModSaveData.AmmoCollected.Add(SceneAs<Level>().Session.Area.LevelSet + "_" + ID);
            sfx = SoundEmitter.Play(collectSound, this);
            AreaKey area = level.Session.Area;
            for (int i = 0; i < 10; i++)
            {
                Scene.Add(new AbsorbOrb(Position));
            }
            level.Shake();
            Input.Rumble(RumbleStrength.Strong, RumbleLength.Medium);
            level.Flash(Color.White);
            level.FormationBackdrop.Display = true;
            level.FormationBackdrop.Alpha = 1f;
            Visible = false;
            if (player.Dead)
            {
                yield return 100f;
            }
            Engine.TimeRate = 1f;
            Tag = Tags.FrozenUpdate;
            level.Frozen = true;
            string metroidGameplay = "";
            if (ammo == "EnergyTank")
            {
                metroidGameplay = "Met_";
            }
            poemTextA = Dialog.Clean("XaphanHelper_get_" + metroidGameplay + ammo + "_Name");
            poemTextB = Dialog.Clean("XaphanHelper_get_" + metroidGameplay + ammo + "_Desc");
            AmmoDisplay ammoDisplay = SceneAs<Level>().Tracker.GetEntity<AmmoDisplay>();
            if (ammo == "PowerBomb" && ammoDisplay.MaxPowerBombs == 0)
            {
                poemTextC = Dialog.Clean("XaphanHelper_MorphMode");
            }
            if (string.IsNullOrEmpty(particleColor))
            {
                particleColor = "FFFFFF";
            }
            switch (ammo)
            {
                case "Missile":
                    if (ammoDisplay != null)
                    {
                        if (ammoDisplay.MaxMissiles == 0)
                        {
                            controlA = XaphanModule.ModSettings.SelectItem;
                            controlB = Input.Dash;
                            inputActionA = "XaphanHelper_Select";
                            inputActionB = "XaphanHelper_Fire";
                        }
                        else
                        {
                            poemTextA = Dialog.Clean("XaphanHelper_get_Missile_Name_b");
                            poemTextB = Dialog.Clean("XaphanHelper_Increase_Missile");
                        }
                    }
                    break;
                case "SuperMissile":
                    if (ammoDisplay != null)
                    {
                        if (ammoDisplay.MaxSuperMissiles == 0)
                        {
                            controlA = XaphanModule.ModSettings.SelectItem;
                            controlB = Input.Dash;
                            inputActionA = "XaphanHelper_Select";
                            inputActionB = "XaphanHelper_Fire";
                        }
                        else
                        {
                            poemTextA = Dialog.Clean("XaphanHelper_get_SuperMissile_Name_b");
                            poemTextB = Dialog.Clean("XaphanHelper_Increase_SuperMissile");
                        }
                    }
                    break;
                case "PowerBomb":
                    if (ammoDisplay != null)
                    {
                        if (ammoDisplay.MaxPowerBombs == 0)
                        {
                            controlA = XaphanModule.ModSettings.SelectItem;
                            controlB = Input.Dash;
                            inputActionA = "XaphanHelper_Select_2";
                            inputActionB = "XaphanHelper_Set";
                        }
                        else
                        {
                            poemTextA = Dialog.Clean("XaphanHelper_get_PowerBomb_Name_b");
                            poemTextB = Dialog.Clean("XaphanHelper_Increase_PowerBomb");
                        }
                    }
                    break;
            }
            poem = new CustomPoem(inputActionA, poemTextA, inputActionB, poemTextB, poemTextC, nameColor, descColor, descColor, particleColor, sprite, 0.5f, controlA, controlB);
            poem.Alpha = 0f;
            Scene.Add(poem);
            for (float t2 = 0f; t2 < 1f; t2 += Engine.RawDeltaTime)
            {
                poem.Alpha = Ease.CubeOut(t2);
                yield return null;
            }
            while (!Input.MenuConfirm.Pressed && !Input.MenuCancel.Pressed)
            {
                yield return null;
            }
            sfx.Source.Param("end", 1f);
            giveAmmo(ammo);
            level.FormationBackdrop.Display = false;
            for (float t = 0f; t < 1f; t += Engine.RawDeltaTime * 2f)
            {
                poem.Alpha = Ease.CubeIn(1f - t);
                yield return null;
            }
            player.Depth = 0;
            if (!string.IsNullOrEmpty(newMusic))
            {
                session.Audio.Music.Event = SFX.EventnameByHandle(newMusic);
            }
            else
            {
                session.Audio.Music.Event = SFX.EventnameByHandle(oldMusic);
            }
            session.Audio.Apply(forceSixteenthNoteHack: false);
            EndCutscene();
        }

        private void giveAmmo(string ammo)
        {
            AmmoDisplay ammoDisplay = SceneAs<Level>().Tracker.GetEntity<AmmoDisplay>();
            HealthDisplay healthDisplay = SceneAs<Level>().Tracker.GetEntity<HealthDisplay>();
            if (ammoDisplay != null && healthDisplay != null)
            {
                switch (ammo)
                {
                    case "Missile":
                        ammoDisplay.AddMissile(value);
                        break;
                    case "SuperMissile":
                        ammoDisplay.AddSuperMissile(value);
                        break;
                    case "PowerBomb":
                        ammoDisplay.AddPowerBomb(value);
                        break;
                    case "EnergyTank":
                        healthDisplay.AddMaxHealth(value);
                        break;
                    default:
                        break;
                }
            }
        }

        private void EndCutscene()
        {
            Level level = Scene as Level;
            level.Frozen = false;
            level.CanRetry = true;
            level.FormationBackdrop.Display = false;
            Engine.TimeRate = 1f;
            if (poem != null)
            {
                poem.RemoveSelf();
            }
            RemoveSelf();
        }
    }
}
