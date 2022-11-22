using Celeste.Mod.XaphanHelper.NPCs;
using Celeste.Mod.XaphanHelper.UI_Elements;
using Microsoft.Xna.Framework;
using Monocle;
using System.Collections;

namespace Celeste.Mod.XaphanHelper.Cutscenes
{
    class CS00_GemRoomB : CutsceneEntity
    {
        private readonly Player player;

        private string oldMusic;

        private SoundEmitter sfx;

        private UpgradeScreen upgradeScreen;

        private NPC00_Theo theo;

        protected XaphanModuleSettings Settings => XaphanModule.Settings;

        public CS00_GemRoomB(Player player)
        {
            this.player = player;
        }

        public override void OnBegin(Level level)
        {
            theo = Scene.Entities.FindFirst<NPC00_Theo>();
            Add(new Coroutine(Cutscene(level)));
        }

        public override void OnEnd(Level level)
        {
            if (level.Session.GetFlag("Upgrade_DashBoots") || XaphanModule.ModSaveData.SavedFlags.Contains("Xaphan/0_Upgrade_DashBoots"))
            {
                if (!XaphanModule.ModSaveData.SavedFlags.Contains("Xaphan/0_Can_Open_Map"))
                {
                    XaphanModule.ModSaveData.SavedFlags.Add("Xaphan/0_Can_Open_Map");
                }
                XaphanModule.ModSaveData.WatchedCutscenes.Add("Xaphan/0_Ch0_Gem_Room_B");
                level.Session.SetFlag("CS_Ch0_Gem_Room_B");
            }
            player.StateMachine.State = 0;
        }

        public IEnumerator GetMap(Level level)
        {
            oldMusic = Audio.CurrentMusic;
            level.Session.Audio.Music.Event = SFX.EventnameByHandle("event:/game/xaphan/item_get");
            level.Session.Audio.Apply(forceSixteenthNoteHack: false);
            level.FormationBackdrop.Display = true;
            level.FormationBackdrop.Alpha = 1f;
            sfx = SoundEmitter.Play("event:/game/xaphan/item_get", this);
            Engine.TimeRate = 1f;
            Tag = Tags.FrozenUpdate;
            level.Frozen = true;
            string name = Dialog.Clean("XaphanHelper_get_Map_Name");
            string desc = Dialog.Clean("XaphanHelper_get_Map_Desc");
            string controls = Dialog.Clean("XaphanHelper_get_Map_Controls");
            AreaKey area = level.Session.Area;
            upgradeScreen = new UpgradeScreen("collectables/XaphanHelper/UpgradeCollectable/map", name, desc, controls, "AA00AA", "FFFFFF", "FFFFFF", "533467", Settings.OpenMap.Button);
            upgradeScreen.Alpha = 0f;
            Scene.Add(upgradeScreen);
            for (float t2 = 0f; t2 < 1f; t2 += Engine.RawDeltaTime)
            {
                upgradeScreen.Alpha = Ease.CubeOut(t2);
                yield return null;
            }
            while (!Input.MenuConfirm.Pressed && !Input.MenuCancel.Pressed)
            {
                yield return null;
            }
            sfx.Source.Param("end", 1f);
            level.FormationBackdrop.Display = false;
            for (float t = 0f; t < 1f; t += Engine.RawDeltaTime * 2f)
            {
                upgradeScreen.Alpha = Ease.CubeIn(1f - t);
                yield return null;
            }
            player.Depth = 0;
            level.Frozen = false;
            level.Session.Audio.Music.Event = SFX.EventnameByHandle(oldMusic);
            level.Session.Audio.Apply(forceSixteenthNoteHack: false);
        }

        public IEnumerator Cutscene(Level level)
        {
            if ((Settings.SpeedrunMode && level.Session.GetFlag("Upgrade_DashBoots")) || (!Settings.SpeedrunMode && XaphanModule.ModSaveData.SavedFlags.Contains("Xaphan/0_Upgrade_DashBoots")))
            {
                player.StateMachine.State = 11;
                yield return Level.ZoomTo(new Vector2(160f, 110f), 1.5f, 1f);
                yield return 0.2f;
                yield return Textbox.Say("Xaphan_Ch0_A_Gem_Room_B");
                yield return player.DummyWalkToExact((int)theo.Position.X - 16, false, 1f, false);
                yield return Textbox.Say("Xaphan_Ch0_A_Gem_Room_B_b");
                Add(new Coroutine(GetMap(level)));
                yield return Textbox.Say("Xaphan_Ch0_A_Gem_Room_B_c");
                yield return Level.ZoomBack(0.5f);
            }
            EndCutscene(Level);
        }
    }


}