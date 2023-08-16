using System.Collections;
using Celeste.Mod.Entities;
using Celeste.Mod.XaphanHelper.UI_Elements;
using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste.Mod.XaphanHelper.Entities
{
    [Tracked(true)]
    [CustomEntity("XaphanHelper/SamusUpgradeCollectable")]
    class SamusUpgradeCollectable : Entity
    {
        public EntityID ID;

        private class BgFlash : Entity
        {
            private float alpha = 1f;

            public BgFlash()
            {
                Depth = 10100;
                Tag = Tags.Persistent;
            }

            public override void Update()
            {
                base.Update();
                alpha = Calc.Approach(alpha, 0f, Engine.DeltaTime * 0.5f);
                if (alpha <= 0f)
                {
                    RemoveSelf();
                }
            }

            public override void Render()
            {
                Vector2 position = (Scene as Level).Camera.Position;
                Draw.Rect(position.X - 10f, position.Y - 10f, 340f, 200f, Color.Black * alpha);
            }
        }

        private string sprite;

        private string collectSound;

        private string oldMusic;

        private string newMusic;

        public string upgrade;

        public Sprite collectable;

        private Wiggler scaleWiggler;

        private Wiggler moveWiggler;

        private string inputActionA;

        private string poemTextA;

        private string poemTextB;

        private string poemTextC;

        private string nameColor;

        private string descColor;

        private string particleColor;

        private object controlA;

        private CustomPoem poem;

        private SoundEmitter sfx;

        private XaphanModule.Upgrades upg;

        private string Prefix;

        public int index;

        private bool FlagRegiseredInSaveData()
        {
            Session session = SceneAs<Level>().Session;
            string Prefix = session.Area.GetLevelSet();
            int chapterIndex = session.Area.ChapterIndex == -1 ? 0 : session.Area.ChapterIndex;
            if (upgrade == "MapShard")
            {
                return XaphanModule.ModSaveData.SavedFlags.Contains(Prefix + "_Ch" + chapterIndex + "_" + upgrade);
            }
            if (upgrade == "Map")
            {
                return XaphanModule.ModSaveData.SavedFlags.Contains(Prefix + "_Can_Open_Map");
            }
            return XaphanModule.ModSaveData.SavedFlags.Contains(Prefix + "_Upgrade_" + upgrade);
        }

        public SamusUpgradeCollectable(EntityData data, Vector2 position, EntityID id) : base(data.Position + position)
        {
            ID = id;
            collectSound = data.Attr("collectSound");
            newMusic = data.Attr("newMusic");
            upgrade = data.Attr("upgrade");
            upg = data.Enum("upgrade", XaphanModule.Upgrades.MorphingBall);
            nameColor = data.Attr("nameColor");
            descColor = data.Attr("descColor");
            particleColor = data.Attr("particleColor");
            sprite = "collectables/XaphanHelper/SamusUpgradeCollectable/" + upgrade.ToLower();
            index = data.Int("index", 0);
            Collider = new Hitbox(8f, 8f);
            Add(collectable = new Sprite(GFX.Game, sprite));
            collectable.AddLoop("idle", "", 0.08f);
            collectable.Play("idle");
            Add(scaleWiggler = Wiggler.Create(0.5f, 4f, delegate (float f)
            {
                collectable.Scale = Vector2.One * (1f + f * 0.3f);
            }));
            moveWiggler = Wiggler.Create(0.8f, 2f);
            moveWiggler.StartZero = true;
            Add(moveWiggler);
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
            Prefix = SceneAs<Level>().Session.Area.GetLevelSet();
            if (FlagRegiseredInSaveData() || SceneAs<Level>().Session.GetFlag(upgrade))
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
            if (upgrade == "VariaJacket" || upgrade == "GravityJacket" || upgrade == "SpaceJump" || upgrade == "ScrewAttack")
            {
                metroidGameplay = "Met_";
            }
            poemTextA = Dialog.Clean("XaphanHelper_get_" + metroidGameplay + upgrade + "_Name");
            poemTextB = Dialog.Clean("XaphanHelper_get_" + metroidGameplay + upgrade + "_Desc");
            poemTextC = Dialog.Clean("XaphanHelper_get_" + metroidGameplay + upgrade + "_Desc_b");
            if (string.IsNullOrEmpty(particleColor))
            {
                particleColor = "FFFFFF";
            }
            bool select = false;
            switch (upgrade)
            {
                case "LongBeam":
                    poemTextC = null;
                    break;
                case "IceBeam":
                    poemTextC = null;
                    break;
                case "WaveBeam":
                    poemTextC = null;
                    break;
                case "Spazer":
                    poemTextC = null;
                    break;
                case "PlasmaBeam":
                    poemTextC = null;
                    break;
                case "VariaJacket":
                    poemTextC = null;
                    break;
                case "GravityJacket":
                    poemTextC = null;
                    break;
                case "MorphingBall":
                    controlA = Input.MenuDown;
                    inputActionA = "XaphanHelper_Press";
                    break;
                case "MorphBombs":
                    controlA = Input.Dash;
                    inputActionA = "XaphanHelper_Press";
                    break;
                case "SpringBall":
                    controlA = Input.Jump;
                    inputActionA = "XaphanHelper_Press";
                    break;
                case "ScrewAttack":
                    poemTextC = null;
                    break;
                case "HighJumpBoots":
                    poemTextC = null;
                    break;
                case "SpaceJump":
                    controlA = Input.Jump;
                    inputActionA = "XaphanHelper_Press";
                    break;
                case "SpeedBooster":
                    controlA = Input.Grab;
                    inputActionA = "XaphanHelper_Hold";
                    break;
            }
            poem = new CustomPoem(inputActionA, poemTextA, null, poemTextB, poemTextC, nameColor, descColor, descColor, particleColor, sprite, 0.5f, controlA, null, select);
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
            if (upgrade != "Map" && upgrade != "MapShard")
            {
                setUpgrade(upg);
            }
            RegisterFlag();
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

        private void RegisterFlag()
        {
            Session session = SceneAs<Level>().Session;
            int chapterIndex = session.Area.ChapterIndex == -1 ? 0 : session.Area.ChapterIndex;
            if (upgrade != "")
            {
                session.SetFlag("Upgrade_" + upgrade, true);
            }
            string Prefix = session.Area.GetLevelSet();
            if (upgrade == "MapShard")
            {
                if (!XaphanModule.ModSaveData.SavedFlags.Contains(Prefix + "_Ch" + chapterIndex + "_MapShard"))
                {
                    XaphanModule.ModSaveData.SavedFlags.Add(Prefix + "_Ch" + chapterIndex + "_MapShard");
                }
            }
            else if (upgrade == "Map")
            {
                if (!XaphanModule.ModSaveData.SavedFlags.Contains(Prefix + "_Can_Open_Map"))
                {
                    XaphanModule.ModSaveData.SavedFlags.Add(Prefix + "_Can_Open_Map");
                }
            }
            else
            {
                if (!XaphanModule.ModSaveData.SavedFlags.Contains(Prefix + "_Upgrade_" + upgrade))
                {
                    XaphanModule.ModSaveData.SavedFlags.Add(Prefix + "_Upgrade_" + upgrade);
                }
            }
        }

        private void setUpgrade(XaphanModule.Upgrades upgrade)
        {
            switch (upgrade)
            {
                case XaphanModule.Upgrades.SpaceJump:
                    XaphanModule.Instance.UpgradeHandlers[upgrade].SetValue(6);
                    break;
                default:
                    XaphanModule.Instance.UpgradeHandlers[upgrade].SetValue(1);
                    break;
            }
        }

        private void EndCutscene()
        {
            Level level = Scene as Level;
            if (XaphanModule.ModSettings.ShowMiniMap)
            {
                MapDisplay mapDisplay = level.Tracker.GetEntity<MapDisplay>();
                if (mapDisplay != null)
                {
                    AreaKey area = level.Session.Area;
                    int chapterIndex = area.ChapterIndex == -1 ? 0 : area.ChapterIndex;
                    mapDisplay.GenerateIcons();
                }
            }
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
