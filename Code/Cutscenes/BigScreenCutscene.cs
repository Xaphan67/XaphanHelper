using System.Collections;
using Celeste.Mod.XaphanHelper.Components;
using Celeste.Mod.XaphanHelper.Entities;
using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste.Mod.XaphanHelper.Cutscenes
{
    class BigScreenCutscene : CutsceneEntity
    {
        private readonly Player player;

        private BigScreen screen;

        private readonly string dialogID;

        private readonly string music;

        private readonly bool registerInSaveData;

        public BigScreenCutscene(Player player, string dialogID, string music, bool registerInSaveData)
        {
            this.player = player;
            this.dialogID = dialogID;
            this.music = music;
            this.registerInSaveData = registerInSaveData;
        }

        public override void OnBegin(Level level)
        {
            screen = Scene.Tracker.GetEntity<BigScreen>();
            Add(new Coroutine(Cutscene(level)));
        }

        public override void OnEnd(Level level)
        {
            PlayerCustomAnimation component = player.Components.Get<PlayerCustomAnimation>();
            level.Session.Audio.Music.Event = SFX.EventnameByHandle(music);
            level.Session.Audio.Apply(forceSixteenthNoteHack: false);
            level.TimerHidden = false;
            screen.isOn = true;
            screen.showPortrait = true;
            screen.bgAlpha = 0f;
            if (component != null)
            {
                component.PlaySprite("turnBackpack", "hairBackpack", true, true);
            }
            screen.talk.Enabled = true;
            screen.Depth = 9010;
            string Prefix = level.Session.Area.LevelSet;
            int chapterIndex = level.Session.Area.ChapterIndex;
            level.Session.SetFlag(level.Session.Level + "_" + screen.ID.ID);
            if (registerInSaveData)
            {
                if (!XaphanModule.ModSaveData.SavedFlags.Contains(Prefix + "_Ch" + chapterIndex + "_" + level.Session.Level + "_" + screen.ID.ID))
                {
                    XaphanModule.ModSaveData.SavedFlags.Add(Prefix + "_Ch" + chapterIndex + "_" + level.Session.Level + "_" + screen.ID.ID);
                }
            }
        }

        public IEnumerator Cutscene(Level level)
        {
            PlayerCustomAnimation component = player.Components.Get<PlayerCustomAnimation>();
            screen.Depth = -90000;
            level.TimerHidden = true;
            player.StateMachine.State = 11;
            yield return player.DummyWalkToExact((int)screen.X + 40, false, 1f, true);
            if (component != null)
            {
                component.SetAnim("XaphanHelper_player_turnAround");
                component.PlaySprite("turnBackpack", "hairBackpack", false);
            }
            level.Session.SetFlag(SceneAs<Level>().Session.Level + "_" + screen.ID.ID);
            yield return level.ZoomTo(new Vector2(160f, 92f), 1.75f, 2f);
            if (level.Session.Audio.Music.Event != music || !screen.showPortrait)
            {
                float musicFadeStart = 0f;
                while (musicFadeStart < 1)
                {
                    musicFadeStart += Engine.DeltaTime;
                    Audio.SetMusicParam("fade", 1f - musicFadeStart);
                    yield return null;
                }
                level.Session.Audio.Music.Event = SFX.EventnameByHandle(music);
                level.Session.Audio.Apply(forceSixteenthNoteHack: false);
                Audio.SetMusicParam("fade", 1f);
            }
            while (screen.bgAlpha <= 0.85f)
            {
                screen.bgAlpha += Engine.RawDeltaTime;
                yield return null;
            }
            screen.bgAlpha = 0.85f;
            yield return 0.2f;
            if (!screen.isOn)
            {
                screen.isOn = true;
                yield return 1.2f;
            }
            screen.showPortrait = true;
            Textbox textBox = new(dialogID);
            textBox.RenderOffset.X += 125f;
            textBox.RenderOffset.Y += 650f;
            Engine.Scene.Add(textBox);
            while (textBox.Opened)
            {
                yield return null;
            }
            while (screen.bgAlpha > 0f)
            {
                screen.bgAlpha -= Engine.RawDeltaTime;
                yield return null;
            }
            screen.bgAlpha = 0f;
            yield return level.ZoomBack(1f);
            EndCutscene(Level);
            yield return null;
        }
    }
}
