using System.Collections;
using Celeste.Mod.XaphanHelper.NPCs;
using Celeste.Mod.XaphanHelper.UI_Elements;
using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste.Mod.XaphanHelper.Cutscenes
{
    class CS00_GemRoomB : CutsceneEntity
    {
        private readonly Player player;

        private string oldMusic;

        private SoundEmitter sfx;

        private CustomPoem poem;

        private NPC00_Theo theo;

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
            string poemTextA = Dialog.Clean("XaphanHelper_get_Map_Name");
            string poemTextB = Dialog.Clean("XaphanHelper_get_Map_Desc");
            string poemTextC = Dialog.Clean("XaphanHelper_get_Map_Desc_b");
            AreaKey area = level.Session.Area;
            poem = new CustomPoem("XaphanHelper_Press", poemTextA, null, poemTextB, poemTextC, "AA00AA", "FFFFFF", "FFFFFF", "533467", "collectables/XaphanHelper/UpgradeCollectable/map", 0.5f, XaphanModule.ModSettings.OpenMap);
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
            level.FormationBackdrop.Display = false;
            for (float t = 0f; t < 1f; t += Engine.RawDeltaTime * 2f)
            {
                poem.Alpha = Ease.CubeIn(1f - t);
                yield return null;
            }
            player.Depth = 0;
            level.Frozen = false;
            level.Session.Audio.Music.Event = SFX.EventnameByHandle(oldMusic);
            level.Session.Audio.Apply(forceSixteenthNoteHack: false);
        }

        public IEnumerator Cutscene(Level level)
        {
            if (XaphanModule.ModSaveData.SavedFlags.Contains("Xaphan/0_Upgrade_DashBoots"))
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