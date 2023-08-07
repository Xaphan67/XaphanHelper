using System.Collections;
using Celeste.Mod.XaphanHelper.Entities;
using Celeste.Mod.XaphanHelper.UI_Elements;
using FMOD.Studio;
using Monocle;

namespace Celeste.Mod.XaphanHelper.Managers
{
    [Tracked(true)]
    public class TimeManager : Entity
    {
        private int Timer;

        public float currentTime;

        private EventInstance sfx;

        private string TickingType;

        private string Flag;

        private TimerIndicator indicator;

        public TimeManager(int timer, string tickingtype, string flag = null)
        {
            Timer = timer;
            TickingType = tickingtype;
            Flag = flag;
        }

        public override void Added(Scene scene)
        {
            base.Added(scene);
            currentTime = Timer;
            scene.Add(indicator = new TimerIndicator(this));
            foreach (TimedBlock block in SceneAs<Level>().Tracker.GetEntities<TimedBlock>())
            {
                block.setCurrentIndex(1);
            }
            foreach (TimedStrawberry strawberry in SceneAs<Level>().Tracker.GetEntities<TimedStrawberry>())
            {
                strawberry.Appear();
            }
            foreach (TimerRefill refill in SceneAs<Level>().Tracker.GetEntities<TimerRefill>())
            {
                refill.Appear();
            }
            foreach (TimedTempleGate gate in SceneAs<Level>().Tracker.GetEntities<TimedTempleGate>())
            {
                if (gate.startOpen)
                {
                    gate.Close();
                }
                else
                {
                    gate.Open();
                }
            }
            Add(new Coroutine(StartTimer()));
        }

        public static void Load()
        {
            On.Celeste.Celeste.Freeze += OnCelesteFreeze;
        }

        public static void Unload()
        {
            On.Celeste.Celeste.Freeze -= OnCelesteFreeze;
        }

        private static void OnCelesteFreeze(On.Celeste.Celeste.orig_Freeze orig, float time)
        {
            if (Engine.FreezeTimer < time)
            {
                Engine.FreezeTimer = time;
                if (Engine.Scene != null)
                {
                    Engine.Scene.Tracker.GetEntity<CassetteBlockManager>()?.AdvanceMusic(time);
                    TimeManager manager = Engine.Scene.Tracker.GetEntity<TimeManager>();
                    if (manager != null)
                    {
                        manager.currentTime -= time;
                    }
                }
            }
        }

        public IEnumerator StartTimer()
        {
            if (!string.IsNullOrEmpty(Flag))
            {
                SceneAs<Level>().Session.SetFlag(Flag, true);
            }
            if (TickingType == "on top" || TickingType == "tick only")
            {
                sfx = Audio.Play("event:/game/xaphan/countdown");
            }
            if (TickingType == "tick only")
            {
                Audio.SetMusicParam("fade", 0);
            }
            while (currentTime > 3f)
            {
                currentTime -= Engine.DeltaTime;
                yield return null;
            }
            if (TickingType == "on top" || TickingType == "tick only")
            {
                sfx.stop(STOP_MODE.IMMEDIATE);
                sfx = Audio.Play("event:/game/xaphan/countdown_fast");
            }
            while (currentTime > 0f && currentTime <= 3f)
            {
                currentTime -= Engine.DeltaTime;
                yield return null;
            }
            if (currentTime > 3f)
            {
                if (TickingType == "on top" || TickingType == "tick only")
                {
                    sfx.stop(STOP_MODE.IMMEDIATE);
                }
                Add(new Coroutine(StartTimer()));
            }
            else
            {
                foreach (TimedDashSwitch timedSwitch in SceneAs<Level>().Tracker.GetEntities<TimedDashSwitch>())
                {
                    if (timedSwitch.pressed)
                    {
                        timedSwitch.ResetSwitch();
                    }
                }
                foreach (TimedBlock block in SceneAs<Level>().Tracker.GetEntities<TimedBlock>())
                {
                    block.setCurrentIndex(0);
                }
                foreach (TimedStrawberry strawberry in SceneAs<Level>().Tracker.GetEntities<TimedStrawberry>())
                {
                    if (!strawberry.keepEvenIfTimerRunOut)
                    {
                        strawberry.Hide();
                    }
                }
                foreach (TimerRefill refill in SceneAs<Level>().Tracker.GetEntities<TimerRefill>())
                {
                    refill.Hide();
                }
                if (TickingType == "tick only")
                {
                    string PreviousMusic = Audio.CurrentMusic;
                    SceneAs<Level>().Session.Audio.Music.Event = SFX.EventnameByHandle("event:/char/dialog/ex");
                    SceneAs<Level>().Session.Audio.Apply(forceSixteenthNoteHack: false);
                    Audio.SetMusicParam("fade", 1);
                    SceneAs<Level>().Session.Audio.Music.Event = SFX.EventnameByHandle(PreviousMusic);
                    SceneAs<Level>().Session.Audio.Apply(forceSixteenthNoteHack: false);
                }
                foreach (TimedTempleGate gate in SceneAs<Level>().Tracker.GetEntities<TimedTempleGate>())
                {
                    if (gate.startOpen)
                    {
                        gate.Open();
                    }
                    else
                    {
                        gate.Close();
                    }
                }
                RemoveSelf();
            }
        }

        public override void Removed(Scene scene)
        {
            if (!string.IsNullOrEmpty(Flag))
            {
                SceneAs<Level>().Session.SetFlag(Flag, false);
            }
            if (TickingType == "tick only")
            {
                string PreviousMusic = Audio.CurrentMusic;
                SceneAs<Level>().Session.Audio.Music.Event = SFX.EventnameByHandle("event:/char/dialog/ex");
                SceneAs<Level>().Session.Audio.Apply(forceSixteenthNoteHack: false);
                Audio.SetMusicParam("fade", 1);
                SceneAs<Level>().Session.Audio.Music.Event = SFX.EventnameByHandle(PreviousMusic);
                SceneAs<Level>().Session.Audio.Apply(forceSixteenthNoteHack: false);
            }
            base.Removed(scene);
            if (TickingType == "on top" || TickingType == "tick only")
            {
                sfx.stop(STOP_MODE.IMMEDIATE);
            }
            scene.Remove(indicator);
        }

        public void AddTime(int time)
        {
            currentTime += time;
            if (currentTime % 1 != 0)
            {
                float mod = currentTime % 1;
                currentTime -= mod;
                if (mod >= 0.5f)
                {
                    currentTime += 1;
                }
            }
        }

        public void SetTime(int time)
        {
            currentTime = time;
        }
    }
}