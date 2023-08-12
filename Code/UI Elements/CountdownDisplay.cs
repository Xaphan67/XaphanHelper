using System;
using System.Collections;
using System.Collections.Generic;
using Celeste.Mod.XaphanHelper.Cutscenes;
using Celeste.Mod.XaphanHelper.Entities;
using Celeste.Mod.XaphanHelper.Triggers;
using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste.Mod.XaphanHelper.UI_Elements
{
    [Tracked(true)]
    public class CountdownDisplay : Entity
    {
        public bool TimerRanOut;

        public bool IsPaused;

        public bool PauseTimer;

        public bool Shaking;

        public bool Shake;

        public bool Explosing;

        public bool Explode;

        public long CurrentTime;

        public long ChapterTimerAtStart;

        public long PausedTimer;

        public bool playerHasMoved;

        public bool SaveTimer;

        public bool FromOtherChapter;

        public bool Immediate;

        public string startFlag;

        public string activeFlag;

        public string hideFlag;

        public string startRoom;

        public int startChapter;

        public Vector2 SpawnPosition;

        private bool WasTickingBeforeTransition;

        private Coroutine WaitForSpawnRoutine = new();

        private NormalText Timetext;

        private StartCountdownTrigger trigger;

        private static float numberWidth;

        private static float spacerWidth;

        private static PixelFont font;

        private static float fontFaceSize;

        public string rawEventsFlags;

        public Dictionary<float, string> EventsFlags = new();

        public CountdownDisplay(StartCountdownTrigger timer, bool saveTimer, Vector2 spawnPosition, bool immediate = false)
        {
            Tag = (Tags.HUD | Tags.Global | Tags.PauseUpdate | Tags.TransitionUpdate);
            CalculateBaseSizes();
            SpawnPosition = spawnPosition;
            ChapterTimerAtStart = -1;
            startFlag = timer.startFlag;
            activeFlag = timer.activeFlag;
            hideFlag = timer.hideFlag;
            rawEventsFlags = timer.eventsFlags;
            startRoom = timer.startRoom;
            Shake = timer.shake;
            Explode = timer.explode;
            SaveTimer = saveTimer;
            CurrentTime = (long)timer.time * 10000000;
            Immediate = immediate;
            trigger = timer;
            Depth = 1000000;
        }

        public CountdownDisplay(float time, bool shake, bool explode, bool saveTimer, int startingChapter, string startingRoom, Vector2 spawnPositrion, string activeFlag, string hideFlag, string eventsFlags)
        {
            Tag = (Tags.HUD | Tags.Global | Tags.PauseUpdate | Tags.TransitionUpdate);
            CalculateBaseSizes();
            SpawnPosition = spawnPositrion;
            ChapterTimerAtStart = -1;
            this.activeFlag = activeFlag;
            this.hideFlag = hideFlag;
            rawEventsFlags = eventsFlags;
            startRoom = startingRoom;
            Shake = shake;
            Explode = explode;
            SaveTimer = saveTimer;
            CurrentTime = (long)time;
            startChapter = startingChapter;
            FromOtherChapter = true;
            Depth = 1000000;
        }

        public static void Load()
        {
            Everest.Events.Level.OnLoadLevel += onLevelLoad;
        }

        public static void Unload()
        {
            Everest.Events.Level.OnLoadLevel -= onLevelLoad;
        }

        private static void onLevelLoad(Level level, Player.IntroTypes playerIntro, bool isFromLoader)
        {
            CountdownDisplay timerDisplay = level.Tracker.GetEntity<CountdownDisplay>();
            if (timerDisplay != null)
            {
                level.SaveQuitDisabled = true;
            }
        }

        public static void CalculateBaseSizes()
        {
            font = Dialog.Languages["english"].Font;
            fontFaceSize = Dialog.Languages["english"].FontFaceSize;
            PixelFontSize pixelFontSize = font.Get(fontFaceSize);
            for (int i = 0; i < 10; i++)
            {
                float x = pixelFontSize.Measure(i.ToString()).X;
                if (x > numberWidth)
                {
                    numberWidth = x;
                }
            }
            spacerWidth = pixelFontSize.Measure('.').X;
        }

        private IEnumerator ShakeLevel()
        {
            Shaking = true;
            Random rand = new();
            while (ChapterTimerAtStart == -1)
            {
                yield return null;
            }
            while (!SceneAs<Level>().Paused)
            {
                if (rand.Next(0, 101) >= 85)
                {
                    SceneAs<Level>().Shake(0.05f);
                }
                else
                {
                    SceneAs<Level>().DirectionalShake(new Vector2(0.5f, 0), 0.05f);
                }
                Input.Rumble(RumbleStrength.Light, RumbleLength.Short);
                yield return 0.05f;
            }
            Shaking = false;
        }

        private IEnumerator DisplayExplosions()
        {
            Explosing = true;
            Random rand = Calc.Random;
            yield return 0.05f;
            while (!SceneAs<Level>().Paused)
            {
                TimerExplosion explosion = new(SceneAs<Level>().Camera.Position + new Vector2(rand.Next(0, 320), rand.Next(0, 184)));
                SceneAs<Level>().Add(explosion);
                yield return 0.05f;
            }
            Explosing = false;
        }

        public void StopTimer(bool action, bool visible)
        {
            PauseTimer = action;
            Visible = visible;
        }

        public override void Added(Scene scene)
        {
            base.Added(scene);
            foreach (CountdownDisplay display in SceneAs<Level>().Tracker.GetEntities<CountdownDisplay>())
            {
                if (display != this)
                {
                    display.RemoveSelf();
                }
            }
            if (Timetext == null)
            {
                Timetext = new NormalText("Xaphanhelper_UI_Time", new Vector2(Engine.Width / 2 - 120, Engine.Height / 2 - 510), Color.Gold, 1f, 0.7f)
                {
                    Tag = (Tags.HUD | Tags.Global | Tags.PauseUpdate | Tags.TransitionUpdate),
                    Depth = 10000
                };
                SceneAs<Level>().Add(Timetext);
            }
            XaphanModule.TriggeredCountDown = false;
        }

        public override void Removed(Scene scene)
        {
            base.Removed(scene);
            if (Timetext != null)
            {
                Timetext.RemoveSelf();
            }
            trigger.RemoveSelf();
        }

        public override void Update()
        {
            base.Update();
            Player player = SceneAs<Level>().Tracker.GetEntity<Player>();
            if (!TimerRanOut)
            {
                if (player == null)
                {
                    StopTimer(true, true);
                }
                else
                {
                    if (player.StateMachine.State == Player.StIntroRespawn && !WaitForSpawnRoutine.Active)
                    {
                        Add(WaitForSpawnRoutine = new Coroutine(SpawnTimerRestartRoutine(player)));
                        return;
                    }
                }
                if (SceneAs<Level>().Transitioning)
                {
                    WasTickingBeforeTransition = IsPaused;
                    StopTimer(true, true);
                }
                else if (WasTickingBeforeTransition)
                {
                    WasTickingBeforeTransition = false;
                    StopTimer(false, true);
                }
                if (Shake && !Shaking)
                {
                    Add(new Coroutine(ShakeLevel()));
                }
                if (Explode && !Explosing)
                {
                    Add(new Coroutine(DisplayExplosions()));
                }
                if ((!playerHasMoved && player != null && player.Speed != Vector2.Zero) || FromOtherChapter || Immediate)
                {
                    playerHasMoved = true;
                    SceneAs<Level>().SaveQuitDisabled = true;
                    if (!string.IsNullOrEmpty(activeFlag))
                    {
                        SceneAs<Level>().Session.SetFlag(activeFlag, true);
                        XaphanModule.ModSaveData.CountdownActiveFlag = activeFlag;
                    }
                    if (!string.IsNullOrEmpty(rawEventsFlags) && EventsFlags.Count == 0)
                    {
                        string[] eFlags = rawEventsFlags.Split(',');
                        foreach (string flag in eFlags)
                        {
                            if (flag.Contains(":"))
                            {
                                string[] str = flag.Split(':');
                                if (float.TryParse(str[0], out _))
                                {
                                    float time = float.Parse(str[0]);
                                    if (!EventsFlags.ContainsKey(time))
                                    {
                                        EventsFlags.Add(time, str[1]);
                                    }
                                    else
                                    {
                                        string old = EventsFlags[time];
                                        EventsFlags.Remove(time);
                                        EventsFlags.Add(time, old + "," + str[1]);
                                    }
                                }
                            }
                        }
                    }
                }
                if ((string.IsNullOrEmpty(startFlag) ? true : SceneAs<Level>().Session.GetFlag(startFlag)) && ChapterTimerAtStart == -1 && playerHasMoved)
                {
                    ChapterTimerAtStart = SceneAs<Level>().Session.Time;
                }
                if ((SceneAs<Level>().Paused && !IsPaused && !PauseTimer) || (PauseTimer && !IsPaused && !SceneAs<Level>().Paused))
                {
                    PausedTimer = GetRemainingTime();
                    IsPaused = true;
                }
                if ((!SceneAs<Level>().Paused && IsPaused && !PauseTimer) || (!PauseTimer && IsPaused && !SceneAs<Level>().Paused))
                {
                    CurrentTime = PausedTimer;
                    ChapterTimerAtStart = SceneAs<Level>().Session.Time;
                    IsPaused = false;
                }
                if (EventsFlags.Count > 0)
                {
                    foreach (KeyValuePair<float, string> keyValuePair in EventsFlags)
                    {
                        if (GetRemainingTime() <= keyValuePair.Key * 10000000)
                        {
                            string[] flags = keyValuePair.Value.Split(',');
                            foreach (string flag in flags)
                            {
                                SceneAs<Level>().Session.SetFlag(flag, true);
                            }
                        }
                    }
                }
                if (SceneAs<Level>().Session.GetFlag(hideFlag))
                {
                    Timetext.Visible = false;
                }
                else
                {
                    Timetext.Visible = Visible;
                }
            }
            if (GetRemainingTime() <= 0 && !TimerRanOut && !SceneAs<Level>().Paused && !PauseTimer)
            {
                if (player != null)
                {
                    Add(new Coroutine(KillPlayer(player)));
                }
            }
        }

        private IEnumerator SpawnTimerRestartRoutine(Player player)
        {
            while (player.StateMachine.State == Player.StIntroRespawn)
            {
                yield return null;
            }
            StopTimer(false, true);
        }

        public IEnumerator KillPlayer(Player player)
        {
            TimerRanOut = true;
            Level Level = Engine.Scene as Level;
            player.StateMachine.State = 11;
            player.DummyGravity = false;
            player.Speed = Vector2.Zero;
            Level.PauseLock = true;
            Level.Displacement.AddBurst(Position, 0.3f, 0f, 80f);
            Level.Shake();
            Input.Rumble(RumbleStrength.Strong, RumbleLength.Long);
            Audio.Play("event:/char/madeline/death", Position);
            player.Sprite.Visible = (player.Hair.Visible = false);
            CustomDeathEffect deathEffect = new(player.Hair.Color, player.Center);
            deathEffect.OnUpdate = delegate (float f)
            {
                player.Light.Alpha = 1f - f;
            };
            Level.Add(deathEffect);
            Level.Session.Deaths++;
            Level.Session.DeathsInCurrentLevel++;
            SaveData.Instance.AddDeath(Level.Session.Area);
            yield return 0.5f;
            if (!FromOtherChapter)
            {
                Scene.Add(new TeleportCutscene(player, startRoom, SpawnPosition, 0, 0, true, 0f, "Fade", respawnAnim: true, useLevelWipe: true));
                yield return 0.49f;
                UnsetEventsFlags(Level);
                RemoveSelf();
            }
            else
            {
                Level.DoScreenWipe(false, () => ReturnToOrigChapter(Level));
            }

        }

        private void ReturnToOrigChapter(Level level)
        {
            AreaKey area = SceneAs<Level>().Session.Area;
            string Prefix = area.GetLevelSet();
            int currentChapter = area.ChapterIndex == -1 ? 0 : area.ChapterIndex;
            XaphanModule.ModSaveData.DestinationRoom = startRoom;
            XaphanModule.ModSaveData.Spawn = SpawnPosition;
            XaphanModule.ModSaveData.Wipe = "Fade";
            XaphanModule.ModSaveData.WipeDuration = 1.35f;
            XaphanModule.ModSaveData.CountdownCurrentTime = -1;
            XaphanModule.ModSaveData.CountdownShake = false;
            XaphanModule.ModSaveData.CountdownShake = false;
            XaphanModule.ModSaveData.CountdownStartChapter = -1;
            XaphanModule.ModSaveData.CountdownStartRoom = "";
            XaphanModule.ModSaveData.CountdownSpawn = new Vector2();
            XaphanModule.ModSaveData.CountdownIntroType = true;
            XaphanModule.ModSaveData.CountdownUseLevelWipe = true;
            XaphanModule.ModSaveData.CountdownHideFlag = "";
            XaphanModule.ModSaveData.CountdownEventsFlags = "";
            int chapterOffset = startChapter - currentChapter;
            int currentChapterID = SceneAs<Level>().Session.Area.ID;
            if (XaphanModule.useMergeChaptersController && (SceneAs<Level>().Session.Area.LevelSet == "Xaphan/0" ? !XaphanModule.ModSaveData.SpeedrunMode : true))
            {
                long currentTime = SceneAs<Level>().Session.Time;
                LevelEnter.Go(new Session(new AreaKey(currentChapterID + chapterOffset))
                {
                    Time = currentTime
                }
                , fromSaveData: false);
            }
            else
            {
                LevelEnter.Go(new Session(new AreaKey(currentChapterID + chapterOffset)), fromSaveData: false);
            }
            UnsetEventsFlags(level);
        }

        public void UnsetEventsFlags(Level level)
        {
            if (EventsFlags.Count > 0)
            {
                foreach (KeyValuePair<float, string> keyValuePair in EventsFlags)
                {
                    string[] flags = keyValuePair.Value.Split(',');
                    foreach (string flag in flags)
                    {
                        level.Session.SetFlag(flag, false);
                    }
                }
            }
        }

        public long GetSpendTime()
        {
            if (ChapterTimerAtStart == -1 || TimerRanOut)
            {
                return 0;
            }
            else
            {
                return SceneAs<Level>().Session.Time - ChapterTimerAtStart;
            }
        }

        public long GetRemainingTime()
        {
            if (!TimerRanOut)
            {
                return CurrentTime - GetSpendTime();
            }
            return 0;
        }

        public override void Render()
        {
            if (!SceneAs<Level>().Session.GetFlag(hideFlag))
            {
                base.Render();
                string timeString = TimeSpan.FromTicks(IsPaused ? PausedTimer : (GetRemainingTime() <= 0 ? 0 : GetRemainingTime())).ShortGameplayFormat();
                float timeWidth = SpeedrunTimerDisplay.GetTimeWidth(timeString);
                Vector2 position = new Vector2(Engine.Width / 2 - timeWidth * 1.5f / 2, Engine.Height / 2 - 420);
                float scale = 1.5f;
                float alpha = 1f;
                float num = scale;
                for (int i = 0; i < timeString.Length; i++)
                {
                    char c = timeString[i];
                    if (c == '.')
                    {
                        num = scale * 0.7f;
                        position.Y -= 5f * scale;
                    }
                    Color color = ((c == ':' || c == '.' || num < scale) ? Color.LightGray * alpha : Color.White * alpha);
                    bool flashRed = false;
                    if (GetRemainingTime() <= 3000000000 && GetRemainingTime() > 5000000 && !IsPaused)
                    {
                        if ((GetRemainingTime() % 600000000 <= 1000000) || (GetRemainingTime() % 300000000 <= 1000000 && GetRemainingTime() <= 1200000000) || (GetRemainingTime() % 10000000 <= 1000000 && GetRemainingTime() <= 300000000))
                        {
                            flashRed = true;
                        }
                    }
                    if (flashRed)
                    {
                        color = Color.Red;
                    }
                    float offset = (((c == ':' || c == '.') ? spacerWidth : numberWidth) + 4f) * num;
                    font.DrawOutline(fontFaceSize, c.ToString(), new Vector2(position.X + offset / 2f, position.Y), new Vector2(0.5f, 1f), Vector2.One * num, color, 2f, Color.Black);
                    position.X += offset;
                }
            }
        }
    }
}
