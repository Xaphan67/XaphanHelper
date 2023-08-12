using System;
using System.Collections;
using Celeste.Mod.Entities;
using Celeste.Mod.XaphanHelper.Entities;
using Celeste.Mod.XaphanHelper.UI_Elements;
using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste.Mod.XaphanHelper.Triggers
{
    [Tracked(true)]
    [CustomEntity("XaphanHelper/StartCountdownTrigger")]
    public class StartCountdownTrigger : Trigger
    {
        public float time;

        public float messageTimer;

        public string startFlag;

        public string activeFlag;

        public string dialogID;

        public bool shake;

        public bool explode;

        public bool CrossChapter;

        public bool HasStartingText;

        public bool HasBeenTriggered;

        public bool fastMessageDisplay;

        public string startRoom;

        public string MessageColor;

        public CountdownDisplay display;

        public IntroText message;

        public EntityID eid;

        public bool Canceled;

        public Vector2 SpawnPosition;

        public bool notVisible;

        public StartCountdownTrigger(EntityData data, Vector2 offset, EntityID ID) : base(data, offset)
        {
            Tag = Tags.Global;
            eid = ID;
            time = data.Float("time");
            startFlag = data.Attr("startFlag");
            activeFlag = data.Attr("activeFlag");
            shake = data.Bool("shake");
            explode = data.Bool("explosions");
            CrossChapter = data.Bool("crossChapter");
            dialogID = data.Attr("dialogID");
            if (!string.IsNullOrEmpty(dialogID))
            {
                HasStartingText = true;
            }
            messageTimer = data.Float("messageTimer");
            if (string.IsNullOrEmpty(dialogID))
            {
                messageTimer = 0;
            }
            fastMessageDisplay = data.Bool("fastMessageDisplay");
            MessageColor = data.Attr("messageColor");
            if (string.IsNullOrEmpty(MessageColor))
            {
                MessageColor = "FFFFFF";
            }
            SpawnPosition = data.Position;
            notVisible = data.Bool("notVisible");
        }

        public override void Added(Scene scene)
        {
            base.Added(scene);
            startRoom = SceneAs<Level>().Session.Level;
            SceneAs<Level>().Session.SetFlag("Countdown_" + eid.Key, false);
            SceneAs<Level>().Session.SetFlag(activeFlag, false);
            XaphanModule.ModSaveData.CountdownActiveFlag = "";
            
        }

        public override void Update()
        {
            base.Update();
            if (message != null && (SceneAs<Level>().Session.Level != startRoom))
            {
                message.Show = false;
                if (!Canceled)
                {
                    DisplayTimer(true);
                }
            }
        }

        public void ChangeSpawnPosition(Vector2 position)
        {
            SpawnPosition = position;
        }

        public override void OnStay(Player player)
        {
            base.OnStay(player);
            if ((string.IsNullOrEmpty(startFlag) ? true : SceneAs<Level>().Session.GetFlag(startFlag)) && display == null && !HasBeenTriggered)
            {
                HasBeenTriggered = true;
                XaphanModule.TriggeredCountDown = true;
                if (shake && !SceneAs<Level>().Session.GetFlag("Countdown_" + eid.Key))
                {
                    Add(new Coroutine(ShakeLevel()));
                }
                if (explode && !SceneAs<Level>().Session.GetFlag("Countdown_" + eid.Key))
                {
                    Add(new Coroutine(DisplayExplosions()));
                }
                if (HasStartingText && messageTimer > 0 && !SceneAs<Level>().Session.GetFlag("Countdown_" + eid.Key))
                {
                    Add(new Coroutine(DisplayStartingText()));
                }
                else
                {
                    DisplayTimer(false);
                }
            }
        }

        public void CancelTimer()
        {
            Canceled = true;
        }

        public void DisplayTimer(bool immediate)
        {
            display = SceneAs<Level>().Tracker.GetEntity<CountdownDisplay>();
            if (display == null)
            {
                SceneAs<Level>().Add(new CountdownDisplay(this, CrossChapter, SpawnPosition, immediate));
            }
        }

        private IEnumerator ShakeLevel()
        {
            if (display != null)
            {
                while (true)
                {
                    SceneAs<Level>().DirectionalShake(new Vector2(0.5f, 0), 0.05f);
                    Input.Rumble(RumbleStrength.Light, RumbleLength.Short);
                    yield return 0.05f;
                }
            }
        }

        private IEnumerator DisplayExplosions()
        {
            if (display != null)
            {
                Random rand = Calc.Random;
                yield return 0.05f;
                while (true)
                {
                    TimerExplosion explosion = new(SceneAs<Level>().Camera.Position + new Vector2(rand.Next(0, 320), rand.Next(0, 184)));
                    SceneAs<Level>().Add(explosion);
                    yield return 0.05f;
                }
            }
        }

        private IEnumerator DisplayStartingText()
        {
            SceneAs<Level>().Add(message = new IntroText(dialogID, "Middle", 340, Calc.HexToColor(MessageColor), 1.2f, true, true, fastMessageDisplay, false, true));
            message.Show = true;
            while (messageTimer > 2.5f)
            {
                yield return null;
                messageTimer -= Engine.DeltaTime;
            }
            DisplayTimer(true);
            while (messageTimer > 0)
            {
                yield return null;
                messageTimer -= Engine.DeltaTime;
            }
            if (message != null)
            {
                message.Show = false;
            }
        }
    }
}
