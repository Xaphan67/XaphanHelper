﻿using System.Collections;
using Microsoft.Xna.Framework;
using Monocle;
using Celeste.Mod.XaphanHelper.Entities;
using System;

namespace Celeste.Mod.XaphanHelper.Cutscenes
{
    class BigScreenCutscene : CutsceneEntity
    {
        private readonly Player player;

        private BigScreen screen;
        public BigScreenCutscene(Player player)
        {
            this.player = player;
        }

        public override void OnBegin(Level level)
        {
            screen = Scene.Tracker.GetEntity<BigScreen>();
            Add(new Coroutine(Cutscene(level)));
        }

        public override void OnEnd(Level level)
        {
            player.StateMachine.State = 0;
            level.Session.Audio.Music.Event = SFX.EventnameByHandle("event:/music/xaphan/lvl_0_mystery");
            level.Session.Audio.Apply(forceSixteenthNoteHack: false);
            level.TimerHidden = false;
            screen.isOn = true;
            screen.showPortrait = true;
            screen.bgAlpha = 0f;
            screen.PlayerPose = "XaphanHelper_turnAround_reverse";
            player.Sprite.Play(screen.PlayerPose);
            player.Sprite.OnLastFrame = resumeSprite;
            screen.talk.Enabled = true;
            screen.Depth = 9010;
        }

        public IEnumerator Cutscene(Level level)
        {
            screen.Depth = -90000;
            level.TimerHidden = true;
            player.StateMachine.State = 11;
            yield return player.DummyWalkToExact((int)screen.X + 40, false, 1f, true);
            screen.PlayerPose = "XaphanHelper_turnAround";
            player.Sprite.Play(screen.PlayerPose);
            player.Sprite.OnLastFrame = stopSprite;
            yield return level.ZoomTo(new Vector2(160f, 92f), 1.75f, 2f);
            if (level.Session.Audio.Music.Event != "event:/music/xaphan/lvl_0_mystery")
            {
                float musicFadeStart = 0f;
                while (musicFadeStart < 1)
                {
                    musicFadeStart += Engine.DeltaTime;
                    Audio.SetMusicParam("fade", 1f - musicFadeStart);
                    yield return null;
                }
                level.Session.Audio.Music.Event = SFX.EventnameByHandle("event:/music/xaphan/lvl_0_mystery");
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
            Textbox textBox = new Textbox("Xaphan_Ch5_Lore");
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

        public void stopSprite(string s)
        {
            screen.PlayerPose = "XaphanHelper_turnAround_end";
            player.Sprite.Play(screen.PlayerPose);
        }

        public void resumeSprite(string s)
        {
            screen.PlayerPose = "";
        }


    }
}
