using System.Collections;
using Celeste.Mod.Entities;
using Celeste.Mod.XaphanHelper.Cutscenes;
using Celeste.Mod.XaphanHelper.UI_Elements;
using FMOD.Studio;
using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste.Mod.XaphanHelper.Entities
{
    [Tracked(true)]
    [CustomEntity("XaphanHelper/Elevator")]
    class Elevator : Solid
    {
        private float speed;

        private float lerp;

        private float Timer;

        private bool EndAreaEntrance;

        private bool CanTalk;

        private bool Activated;

        private bool UsableInSpeedrunMode;

        private int ReversePosition;

        private int ToChapter;

        private int SpawnRoomX;

        private int SpawnRoomY;

        private string DestinationRoom;

        private string sprite;

        private Vector2 StartPosition;

        private Vector2 EndPosition;

        private Sprite Sprite;

        public EventInstance sound;

        private TalkComponent talk;

        private AreaKey area;

        private bool OneUse;

        private string flag;

        public Elevator(Vector2 position, string sprite, bool canTalk, bool usableInSpeedrunMode, float timer, bool endAreaEntrance, int endPosition, int toChapter, string destinationRoom, int spawnRoomX, int spawnRoomY, string flag) : base(position, 32f, 8f, safe: true)
        {
            CanTalk = canTalk;
            Timer = timer;
            EndAreaEntrance = endAreaEntrance;
            UsableInSpeedrunMode = usableInSpeedrunMode;
            StartPosition = Position;
            EndPosition = Position + new Vector2(0, endPosition);
            ReversePosition = endPosition;
            ToChapter = toChapter;
            DestinationRoom = destinationRoom;
            SpawnRoomX = spawnRoomX;
            SpawnRoomY = spawnRoomY;
            this.flag = flag;
            Add(Sprite = new Sprite(GFX.Game, sprite + "/"));
            Sprite.Position = new Vector2(0f, -1f);
            Sprite.AddLoop("idle", "elevator", 0.06f);
            Sprite.AddLoop("inactive", "inactive", 0.24f);
            Sprite.Play("idle");
            Depth = 8999;
        }

        public Elevator(EntityData data, Vector2 offset) : base(data.Position + offset, data.Width, data.Height, true)
        {
            CanTalk = data.Bool("canTalk");
            Timer = data.Float("timer");
            EndAreaEntrance = data.Bool("endAreaEntrance");
            UsableInSpeedrunMode = data.Bool("usableInSpeedrunMode");
            StartPosition = Position;
            EndPosition = Position + new Vector2(0, data.Int("endPosition"));
            ReversePosition = data.Int("endPosition");
            ToChapter = data.Int("toChapter") == -1 ? area.ChapterIndex : data.Int("toChapter");
            DestinationRoom = data.Attr("destinationRoom");
            SpawnRoomX = data.Int("spawnRoomX");
            SpawnRoomY = data.Int("spawnRoomY");
            OneUse = data.Bool("oneUse");
            flag = data.Attr("flag");
            sprite = data.Attr("sprite");
            Add(Sprite = new Sprite(GFX.Game, sprite + "/"));
            Sprite.Position = new Vector2(0f, -1f);
            Sprite.AddLoop("idle", "elevator", 0.06f);
            Sprite.AddLoop("inactive", "inactive", 0.24f);
            Sprite.Play("idle");
            Depth = 8999;
        }

        public override void Added(Scene scene)
        {
            base.Added(scene);
            area = SceneAs<Level>().Session.Area;
            if (string.IsNullOrEmpty(flag) || (!string.IsNullOrEmpty(flag) && SceneAs<Level>().Session.GetFlag(flag)))
            {
                if (CanTalk)
                {
                    if ((!SceneAs<Level>().Session.GrabbedGolden || (SceneAs<Level>().Session.GrabbedGolden && ToChapter == SceneAs<Level>().Session.Area.ChapterIndex)) && (!XaphanModule.ModSettings.SpeedrunMode || (XaphanModule.ModSettings.SpeedrunMode && UsableInSpeedrunMode)) && (!EndAreaEntrance || (EndAreaEntrance && SceneAs<Level>().Session.GetFlag("Open_End_Area"))))
                    {
                        Add(talk = new TalkComponent(new Rectangle(4, -8, 24, 8), new Vector2(16f, -16f), Interact));
                        talk.PlayerMustBeFacing = false;
                    }
                }
                else
                {
                    Add(new Coroutine(Sequence(EndPosition)));
                }
            }
            else
            {
                Sprite.Play("inactive");
            }
        }

        public override void Update()
        {
            base.Update();
            if (Position != EndPosition)
            {
                if (!string.IsNullOrEmpty(flag) && SceneAs<Level>().Session.GetFlag(flag) && Sprite.CurrentAnimationID == "inactive")
                {
                    Sprite.Play("idle");
                    if (CanTalk)
                    {
                        if ((!SceneAs<Level>().Session.GrabbedGolden || (SceneAs<Level>().Session.GrabbedGolden && ToChapter == SceneAs<Level>().Session.Area.ChapterIndex)) && (!XaphanModule.ModSettings.SpeedrunMode || (XaphanModule.ModSettings.SpeedrunMode && UsableInSpeedrunMode)) && (!EndAreaEntrance || (EndAreaEntrance && SceneAs<Level>().Session.GetFlag("Open_End_Area"))))
                        {
                            Add(talk = new TalkComponent(new Rectangle(4, -8, 24, 8), new Vector2(16f, -16f), Interact));
                            talk.PlayerMustBeFacing = false;
                        }
                    }
                    else
                    {
                        Add(new Coroutine(Sequence(EndPosition)));
                    }
                }
                else if (!string.IsNullOrEmpty(flag) && !SceneAs<Level>().Session.GetFlag(flag) && Sprite.CurrentAnimationID == "idle")
                {
                    Sprite.Play("inactive");
                    if (talk != null)
                    {
                        talk.RemoveSelf();
                    }
                }
            }
            if (!Activated && CanTalk && EndAreaEntrance && SceneAs<Level>().Session.GetFlag("Open_End_Area") && Sprite.CurrentAnimationID == "idle")
            {
                Add(talk = new TalkComponent(new Rectangle(4, -8, 24, 8), new Vector2(16f, -16f), Interact));
                talk.PlayerMustBeFacing = false;
                Activated = true;
            }
        }

        private void Interact(Player player)
        {
            talk.Enabled = false;
            CountdownDisplay timerDisplay = SceneAs<Level>().Tracker.GetEntity<CountdownDisplay>();
            if (timerDisplay != null)
            {
                timerDisplay.StopTimer(true, true);
            }
            Add(new Coroutine(MoveElevator(EndPosition)));
        }

        public IEnumerator MoveElevator(Vector2 end)
        {
            if (!XaphanModule.TeleportBackFromDrone)
            {
                foreach (Elevator elevator in SceneAs<Level>().Tracker.GetEntities<Elevator>())
                {
                    if (CanTalk)
                    {
                        if (!elevator.CanTalk)
                        {
                            elevator.RemoveSelf();
                        }
                    }
                    else
                    {
                        if (elevator.CanTalk)
                        {
                            elevator.RemoveSelf();
                        }
                    }
                }
                Player player = Scene.Tracker.GetEntity<Player>();
                SceneAs<Level>().PauseLock = true;
                player.StateMachine.State = 11;
                yield return player.DummyWalkToExact((int)X + 16, false, 1f, true);
                player.Facing = Facings.Right;
                while (Timer > 0f)
                {
                    yield return null;
                    Timer -= Engine.DeltaTime;
                }
                sound = Audio.Play("event:/game/xaphan/elevator", Center);
                SceneAs<Level>().Session.SetFlag("Using_Elevator", true);
                while (Position != EndPosition && player != null)
                {
                    if (((ReversePosition < 0 && Position == EndPosition + new Vector2(0f, 48f)) || (ReversePosition > 0 && Position == EndPosition + new Vector2(0f, -48f))) && CanTalk)
                    {
                        int currentChapter = area.ChapterIndex == -1 ? 0 : area.ChapterIndex;
                        if (ToChapter != currentChapter)
                        {
                            FadeWipe Wipe = new(SceneAs<Level>(), false, () => ExitRoom(player))
                            {
                                Duration = 1.35f
                            };
                            SceneAs<Level>().Add(Wipe);
                        }
                        else
                        {
                            ExitRoom(player);
                        }
                    }
                    speed = 120f / Vector2.Distance(StartPosition, EndPosition);
                    lerp = Calc.Approach(lerp, 1, speed * Engine.DeltaTime);
                    Vector2 liftSpeed = (EndPosition - StartPosition) * speed;
                    MoveTo(Vector2.Lerp(StartPosition, EndPosition, lerp), liftSpeed);
                    SceneAs<Level>().Camera.Position = player.CameraTarget;
                    yield return null;
                }
                if (Position == EndPosition && player != null)
                {
                    sound.stop(STOP_MODE.IMMEDIATE);
                    SceneAs<Level>().Session.SetFlag("Using_Elevator", false);
                    CountdownDisplay timerDisplay = SceneAs<Level>().Tracker.GetEntity<CountdownDisplay>();
                    if (!CanTalk)
                    {
                        if (timerDisplay != null)
                        {
                            timerDisplay.StopTimer(false, true);
                        }
                        if (!OneUse)
                        {
                            Scene.Add(new Elevator(Position, sprite, true, UsableInSpeedrunMode, 1f, false, -ReversePosition, ToChapter, DestinationRoom, SpawnRoomX, SpawnRoomY, flag));
                            RemoveSelf();
                        }
                    }
                    SceneAs<Level>().PauseLock = false;
                    Sprite.Play("inactive");
                    player.StateMachine.State = 0;
                }
            }
        }

        public IEnumerator Sequence(Vector2 end)
        {
            while (!HasPlayerRider())
            {
                yield return null;
            }
            Add(new Coroutine(MoveElevator(end)));
        }

        public void ExitRoom(Player player)
        {
            string Prefix = area.GetLevelSet();
            int currentChapter = area.ChapterIndex == -1 ? 0 : area.ChapterIndex;
            if (ToChapter == currentChapter)
            {
                Scene.Add(new TeleportCutscene(player, DestinationRoom, new Vector2(SpawnRoomX, SpawnRoomY), 0, 0, true, 0f, "Fade", 1.35f, true));
            }
            else
            {
                XaphanModule.ModSaveData.DestinationRoom = DestinationRoom;
                XaphanModule.ModSaveData.Spawn = new Vector2(SpawnRoomX, SpawnRoomY);
                XaphanModule.ModSaveData.Wipe = "Fade";
                XaphanModule.ModSaveData.WipeDuration = 1.35f;
                XaphanModule.ModSaveData.TeleportFromElevator = true;
                CountdownDisplay timerDisplay = SceneAs<Level>().Tracker.GetEntity<CountdownDisplay>();
                if (timerDisplay != null)
                {
                    if (timerDisplay.SaveTimer)
                    {
                        XaphanModule.ModSaveData.CountdownCurrentTime = timerDisplay.PausedTimer;
                        XaphanModule.ModSaveData.CountdownShake = timerDisplay.Shake;
                        XaphanModule.ModSaveData.CountdownExplode = timerDisplay.Explode;
                        if (XaphanModule.ModSaveData.CountdownStartChapter == -1)
                        {
                            XaphanModule.ModSaveData.CountdownStartChapter = area.ChapterIndex == -1 ? 0 : area.ChapterIndex;
                        }
                        XaphanModule.ModSaveData.CountdownStartRoom = timerDisplay.startRoom;
                        XaphanModule.ModSaveData.CountdownSpawn = timerDisplay.SpawnPosition;
                        XaphanModule.ModSaveData.CountdownNotVisible = timerDisplay.NotVisible;
                    }
                }
                int chapterOffset = ToChapter - currentChapter;
                int currentChapterID = SceneAs<Level>().Session.Area.ID;
                if (ToChapter > currentChapter)
                {
                    RegisterChapterCompleteFlag(Prefix, currentChapter);
                    SceneAs<Level>().RegisterAreaComplete();
                }
                if (XaphanModule.useMergeChaptersController && (SceneAs<Level>().Session.Area.LevelSet == "Xaphan/0" ? !XaphanModule.ModSaveData.SpeedrunMode : true))
                {
                    long currentTime = SceneAs<Level>().Session.Time;
                    LevelEnter.Go(new Session(new AreaKey(currentChapterID + chapterOffset))
                    {
                        Time = currentTime,
                        DoNotLoad = XaphanModule.ModSaveData.SavedNoLoadEntities[SceneAs<Level>().Session.Area.LevelSet],
                        Strawberries = XaphanModule.ModSaveData.SavedSessionStrawberries[SceneAs<Level>().Session.Area.LevelSet]
                    }
                    , fromSaveData: false);
                }
                else
                {
                    LevelEnter.Go(new Session(new AreaKey(currentChapterID + chapterOffset)), fromSaveData: false);
                }
            }
        }

        public void RegisterChapterCompleteFlag(string Prefix, int currentChapter)
        {
            if (!XaphanModule.ModSaveData.SavedFlags.Contains(Prefix + "_Chapter_" + currentChapter + "_Complete"))
            {
                XaphanModule.ModSaveData.SavedFlags.Add(Prefix + "_Chapter_" + currentChapter + "_Complete");
            }
        }
    }
}
