using System.Collections;
using Celeste.Mod.Entities;
using Celeste.Mod.XaphanHelper.Cutscenes;
using Celeste.Mod.XaphanHelper.UI_Elements;
using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste.Mod.XaphanHelper.Entities
{
    [CustomEntity("XaphanHelper/PipeGate")]
    class PipeGate : Entity
    {
        private class InvisibleGround : Solid
        {
            public float speed;

            public float lerp;

            public InvisibleGround(Vector2 position, float width, float height) : base(position, width, height, safe: true)
            {

            }
        }

        public Sprite Gate;

        private bool open;

        private bool stop;

        private bool isInPipe;

        private string openSound;

        private string closeSound;

        private string direction;

        private string playerDirection;

        private int ToChapter;

        private int SpawnRoomX;

        private int SpawnRoomY;

        private string DestinationRoom;

        private bool RegisterCurrentChapterAsCompelete;

        private TalkComponent talk;

        private InvisibleGround ground;

        private InvisibleGround barrier;

        private AreaKey area;

        private int currentChapter;

        public PipeGate(EntityData data, Vector2 position) : base(data.Position + position)
        {
            openSound = data.Attr("openSound", "");
            closeSound = data.Attr("closeSound", "");
            direction = data.Attr("direction", "Left");
            playerDirection = data.Attr("playerDirection", "Left");
            ToChapter = data.Int("toChapter", -1);
            SpawnRoomX = data.Int("spawnRoomX");
            SpawnRoomY = data.Int("spawnRoomY");
            DestinationRoom = data.Attr("destinationRoom");
            RegisterCurrentChapterAsCompelete = data.Bool("registerCurrentChapterAsCompelete");
            Add(Gate = XaphanModule.SpriteBank.Create("XaphanHelper_PipeGate"));
            Gate.Play("closed", restart: true);
            Collider = new Hitbox(48f, 48f, -24f, -24f);
            Add(talk = new TalkComponent(new Rectangle(-13, 8, 26, 5), new Vector2(0f, -16f), Interact));
            talk.Enabled = false;
            talk.PlayerMustBeFacing = false;
        }

        public override void Added(Scene scene)
        {
            base.Added(scene);
            area = SceneAs<Level>().Session.Area;
            currentChapter = area.ChapterIndex == -1 ? 0 : area.ChapterIndex;
            if (PlayerIsInside())
            {
                Depth = -9000;
            }
            else
            {
                Depth = 5000;
            }
            if (direction == "Left" || direction == "Right")
            {
                SceneAs<Level>().Add(barrier = new InvisibleGround(Position + new Vector2(direction == "Left" ? -24 : 13, -8f), 11f, 16f));
            }
            else
            {
                SceneAs<Level>().Add(barrier = new InvisibleGround(Position + new Vector2(-8f, direction == "Up" ? -24f : 13f), 16f, 11f));
            }
        }

        public override void Awake(Scene scene)
        {
            base.Awake(scene);
            Add(new Coroutine(WaitForOpen()));
        }

        public override void Update()
        {
            base.Update();
            if (!stop)
            {
                Player player = Scene.Tracker.GetEntity<Player>();
                if (direction == "Left" || direction == "Right")
                {
                    if (player != null && (direction == "Left" ? player.Position.X <= (int)X - 80 : player.Position.X >= (int)X + 80) && player.Top > Top && player.Bottom < Bottom && !isInPipe)
                    {
                        isInPipe = true;
                        Add(new Coroutine(HorizontalGetOutOfPipe(player)));
                    }
                }
                else
                {
                    if (player != null && (direction == "Up" ? player.Position.Y <= (int)Y - 72 : player.Position.Y >= (int)Y + 80) && player.Left > Left && player.Right < Right && !isInPipe)
                    {
                        isInPipe = true;
                        Add(new Coroutine(VerticalGetOutOfPipe(player)));
                    }
                }
            }
        }

        private IEnumerator WaitForOpen()
        {
            while (!open)
            {
                Player player = Scene.Tracker.GetEntity<Player>();
                if (PlayerIsInside())
                {
                    break;
                }
                yield return null;
            }
            Open();
        }

        private IEnumerator WaitForClose()
        {
            while (open)
            {
                Player player = Scene.Tracker.GetEntity<Player>();
                if (!PlayerIsInside())
                {
                    break;
                }
                yield return null;
            }
            Close();
        }

        public void Open()
        {
            if (!stop)
            {
                Audio.Play(string.IsNullOrEmpty(openSound) ? "event:/game/05_mirror_temple/gate_main_open" : openSound, Position);
                open = true;
                Gate.Play("open", restart: true);
                talk.Enabled = true;
                Add(new Coroutine(WaitForClose()));
            }
        }

        public void Close()
        {
            if (!stop)
            {
                Audio.Play(string.IsNullOrEmpty(closeSound) ? "event:/game/05_mirror_temple/gate_main_close" : closeSound, Position);
                open = false;
                Gate.Play("close", restart: true);
                talk.Enabled = false;
                Add(new Coroutine(WaitForOpen()));
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
            if (direction == "Left" || direction == "Right")
            {
                Add(new Coroutine(InteractHorizontalRoutine()));
            }
            else
            {
                Add(new Coroutine(InteractVerticalRoutine()));
            }
        }

        private IEnumerator InteractHorizontalRoutine()
        {
            stop = true;
            barrier.Collidable = false;
            SceneAs<Level>().Add(ground = new InvisibleGround(Position + new Vector2(-16f, 13f), 32f, 8f));
            Player player = Scene.Tracker.GetEntity<Player>();
            SceneAs<Level>().PauseLock = true;
            player.StateMachine.State = 11;
            yield return player.DummyWalkToExact((int)X, false, 1f, true);
            player.Facing = playerDirection == "Left" ? Facings.Left : Facings.Right;
            yield return 0.5f;
            Audio.Play(string.IsNullOrEmpty(closeSound) ? "event:/game/05_mirror_temple/gate_main_close" : closeSound, Position);
            Depth = -9000;
            Gate.Play("close", restart: true);
            yield return 0.2f;
            while (player.Light.Alpha > 0f)
            {
                player.Light.Alpha -= 0.1f;
                yield return null;
            }
            yield return 0.3f;
            Vector2 EndPosition = ground.Position + new Vector2(0, -5f);
            while (ground.Position != EndPosition)
            {
                ground.speed = 120f / Vector2.Distance(ground.Position, EndPosition);
                ground.lerp = Calc.Approach(ground.lerp, 1, ground.speed * Engine.DeltaTime);
                Vector2 liftSpeed = (EndPosition - ground.Position) * ground.speed;
                ground.MoveTo(Vector2.Lerp(ground.Position, EndPosition, ground.lerp), liftSpeed);
                yield return null;
            }
            Add(new Coroutine(WaitForTeleportRoutine(player)));
            yield return player.DummyWalkToExact(direction == "Left" ? (int)X - 88 : (int)X + 88, false, 1f, true);
        }

        private IEnumerator InteractVerticalRoutine()
        {
            stop = true;
            Player player = Scene.Tracker.GetEntity<Player>();
            SceneAs<Level>().PauseLock = true;
            player.StateMachine.State = 11;
            yield return player.DummyWalkToExact((int)X, false, 1f, true);
            player.Facing = playerDirection == "Left" ? Facings.Left : Facings.Right;
            yield return 0.5f;
            Audio.Play(string.IsNullOrEmpty(closeSound) ? "event:/game/05_mirror_temple/gate_main_close" : closeSound, Position);
            Depth = -9000;
            Gate.Play("close", restart: true);
            yield return 0.2f;
            while (player.Light.Alpha > 0f)
            {
                player.Light.Alpha -= 0.1f;
                yield return null;
            }
            yield return 0.3f;
            barrier.Collidable = false;
            Add(new Coroutine(WaitForTeleportRoutine(player)));
            if (direction == "Up")
            {
                player.DummyGravity = false;
                player.Jump(false, false);
            }
        }

        private IEnumerator HorizontalGetOutOfPipe(Player player)
        {
            CountdownDisplay timerDisplay = SceneAs<Level>().Tracker.GetEntity<CountdownDisplay>();
            if (timerDisplay != null)
            {
                timerDisplay.CurrentTime += 340000;
                timerDisplay.StopTimer(true, true);
            }
            barrier.Collidable = false;
            stop = true;
            Depth = -9000;
            player.Light.Alpha = 0f;
            SceneAs<Level>().Add(ground = new InvisibleGround(Position + new Vector2(-16f, 8f), 32f, 8f));
            player.StateMachine.State = 11;
            yield return player.DummyWalkToExact((int)X, false, 1f, false);
            barrier.Collidable = true;
            Vector2 EndPosition = ground.Position + new Vector2(0, 5f);
            while (ground.Position != EndPosition)
            {
                ground.speed = 120f / Vector2.Distance(ground.Position, EndPosition);
                ground.lerp = Calc.Approach(ground.lerp, 1, ground.speed * Engine.DeltaTime);
                Vector2 liftSpeed = (EndPosition - ground.Position) * ground.speed;
                ground.MoveTo(Vector2.Lerp(ground.Position, EndPosition, ground.lerp), liftSpeed);
                yield return null;
            }
            ground.RemoveSelf();
            while (player.Light.Alpha < 1f)
            {
                player.Light.Alpha += 0.1f;
                yield return null;
            }
            Audio.Play(string.IsNullOrEmpty(openSound) ? "event:/game/05_mirror_temple/gate_main_open" : openSound, Position);
            open = true;
            Gate.Play("open", restart: true);
            talk.Enabled = true;
            Depth = 5000;
            isInPipe = false;
            SceneAs<Level>().Session.RespawnPoint = SceneAs<Level>().GetSpawnPoint(Position);
            if (timerDisplay != null)
            {
                timerDisplay.StopTimer(false, true);
            }
            player.StateMachine.State = 0;
            stop = false;
            Add(new Coroutine(WaitForClose()));
        }

        private IEnumerator VerticalGetOutOfPipe(Player player)
        {
            CountdownDisplay timerDisplay = SceneAs<Level>().Tracker.GetEntity<CountdownDisplay>();
            if (timerDisplay != null)
            {
                timerDisplay.CurrentTime += 340000;
                timerDisplay.StopTimer(true, true);
            }
            player.Facing = playerDirection == "Left" ? Facings.Left : Facings.Right;
            barrier.Collidable = false;
            stop = true;
            Depth = -9000;
            player.Light.Alpha = 0f;
            player.StateMachine.State = 11;
            foreach (PlayerBlocker blocker in SceneAs<Level>().Tracker.GetEntities<PlayerBlocker>())
            {
                if (blocker.CanJumpThrough)
                {
                    blocker.RemoveSelf();
                }
            }
            foreach (PlayerPlatform platform in SceneAs<Level>().Tracker.GetEntities<PlayerPlatform>())
            {
                platform.TurnOffCollision(true);
            }
            if (direction == "Down")
            {
                player.DummyGravity = false;
                player.Jump(false, false);
                while (player.Bottom > Bottom - 10)
                {
                    yield return null;
                }
                player.DummyGravity = true;
            }
            else
            {
                while (player.Top < Top + 10)
                {
                    yield return null;
                }
            }
            barrier.Collidable = true;
            while (player.Light.Alpha < 1f)
            {
                player.Light.Alpha += 0.1f;
                yield return null;
            }
            Audio.Play(string.IsNullOrEmpty(openSound) ? "event:/game/05_mirror_temple/gate_main_open" : openSound, Position);
            open = true;
            Gate.Play("open", restart: true);
            talk.Enabled = true;
            Depth = 5000;
            isInPipe = false;
            SceneAs<Level>().Session.RespawnPoint = SceneAs<Level>().GetSpawnPoint(Position);
            if (timerDisplay != null)
            {
                timerDisplay.StopTimer(false, true);
            }
            foreach (PlayerPlatform platform in SceneAs<Level>().Tracker.GetEntities<PlayerPlatform>())
            {
                platform.TurnOffCollision(false);
            }
            player.StateMachine.State = 0;
            stop = false;
            Add(new Coroutine(WaitForClose()));
        }

        private IEnumerator WaitForTeleportRoutine(Player player)
        {
            if (direction == "Left" || direction == "Right")
            {
                while (direction == "Left" ? player.Center.X > (int)X - 56 : player.Center.X < (int)X + 56)
                {
                    yield return null;
                }
            }
            else
            {
                while (direction == "Up" ? player.Center.Y > (int)Y - 56 : player.Center.Y < (int)Y + 56)
                {
                    yield return null;
                }
            }
            if (ToChapter != -1)
            {
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
        }

        public void ExitRoom(Player player)
        {
            AreaKey area = SceneAs<Level>().Session.Area;
            if (ToChapter == currentChapter)
            {
                Scene.Add(new TeleportCutscene(player, DestinationRoom, new Vector2(SpawnRoomX, SpawnRoomY), 0, 0, true, 0f, "Fade", 1.35f, false));
            }
            else
            {
                XaphanModule.ModSaveData.DestinationRoom = DestinationRoom;
                XaphanModule.ModSaveData.Spawn = new Vector2(SpawnRoomX, SpawnRoomY);
                XaphanModule.ModSaveData.Wipe = "Fade";
                XaphanModule.ModSaveData.WipeDuration = 1.35f;
                CountdownDisplay timerDisplay = SceneAs<Level>().Tracker.GetEntity<CountdownDisplay>();
                if (timerDisplay != null)
                {
                    if (timerDisplay.SaveTimer)
                    {
                        XaphanModule.ModSaveData.CountdownCurrentTime = timerDisplay.GetRemainingTime();
                        XaphanModule.ModSaveData.CountdownShake = timerDisplay.Shake;
                        XaphanModule.ModSaveData.CountdownExplode = timerDisplay.Explode;
                        if (XaphanModule.ModSaveData.CountdownStartChapter == -1)
                        {
                            XaphanModule.ModSaveData.CountdownStartChapter = area.ChapterIndex == -1 ? 0 : area.ChapterIndex;
                        }
                        XaphanModule.ModSaveData.CountdownStartRoom = timerDisplay.startRoom;
                        XaphanModule.ModSaveData.CountdownSpawn = timerDisplay.SpawnPosition;
                        XaphanModule.ModSaveData.CountdownHideFlag = timerDisplay.hideFlag;
                        XaphanModule.ModSaveData.CountdownEventsFlags = timerDisplay.rawEventsFlags;
                    }
                }
                int chapterOffset = ToChapter - currentChapter;
                int currentChapterID = SceneAs<Level>().Session.Area.ID;
                if (RegisterCurrentChapterAsCompelete)
                {
                    SceneAs<Level>().RegisterAreaComplete();
                }
                if (XaphanModule.useMergeChaptersController)
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

        public bool PlayerIsInside()
        {
            Player player = Scene.Tracker.GetEntity<Player>();
            return player != null && player.Right > Left && player.Left < Right && player.Bottom > Top && player.Top < Bottom && Scene.Tracker.GetEntity<Drone>() == null;
        }

        public override void Render()
        {
            Gate.Render();
        }
    }
}
