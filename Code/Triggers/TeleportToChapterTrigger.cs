using System.Collections;
using System.Collections.Generic;
using Celeste.Mod.Entities;
using Celeste.Mod.XaphanHelper.Entities;
using Celeste.Mod.XaphanHelper.UI_Elements;
using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste.Mod.XaphanHelper.Triggers
{
    [CustomEntity("XaphanHelper/TeleportToChapterTrigger")]
    class TeleportToChapterTrigger : Trigger
    {
        private int ToChapter;

        private int SpawnRoomX;

        private int SpawnRoomY;

        private string DestinationRoom;

        private string wipeType;

        private float wipeDuration;

        private bool RegisterCurrentChapterAsCompelete;

        AreaKey area;

        private bool canInteract;

        private TalkComponent talk;

        private bool triggered;

        public TeleportToChapterTrigger(EntityData data, Vector2 offset) : base(data, offset)
        {
            ToChapter = data.Int("toChapter");
            SpawnRoomX = data.Int("spawnRoomX");
            SpawnRoomY = data.Int("spawnRoomY");
            DestinationRoom = data.Attr("destinationRoom");
            wipeType = data.Attr("wipeType");
            wipeDuration = data.Float("wipeDuration");
            RegisterCurrentChapterAsCompelete = data.Bool("registerCurrentChapterAsCompelete");
            canInteract = data.Bool("canInteract", false);
        }

        public override void Added(Scene scene)
        {
            base.Added(scene);
            area = SceneAs<Level>().Session.Area;
            if (canInteract)
            {
                Add(talk = new TalkComponent(new Rectangle(0, 0, (int)Width, (int)Height), new Vector2(Width / 2, 0f), Interact));
                talk.PlayerMustBeFacing = false;
            }
        }

        public override void OnEnter(Player player)
        {
            if (!triggered)
            {
                triggered = true;
                base.OnEnter(player);
                if (!canInteract)
                {
                    Interact(player);
                }
            }        
        }

        private void Interact(Player player)
        {
            int currentChapter = area.ChapterIndex == -1 ? 0 : area.ChapterIndex;
            if (ToChapter != currentChapter)
            {
                if (talk != null)
                {
                    talk.Enabled = false;
                }
                Add(new Coroutine(onEnterRoutine(player)));
            }
        }

        public IEnumerator onEnterRoutine(Player player)
        {
            player.StateMachine.State = Player.StDummy;
            XaphanModule.ModSaveData.WipeDuration = wipeDuration;
            switch (wipeType)
            {
                case "Spotlight":
                    SceneAs<Level>().Add(new SpotlightWipe(SceneAs<Level>(), false, () => ExitChapter(player))
                    {
                        Duration = wipeDuration
                    });
                    break;
                case "Curtain":
                    SceneAs<Level>().Add(new CurtainWipe(SceneAs<Level>(), false, () => ExitChapter(player))
                    {
                        Duration = wipeDuration
                    });
                    break;
                case "Mountain":
                    SceneAs<Level>().Add(new MountainWipe(SceneAs<Level>(), false, () => ExitChapter(player))
                    {
                        Duration = wipeDuration
                    });
                    break;
                case "Dream":
                    SceneAs<Level>().Add(new DreamWipe(SceneAs<Level>(), false, () => ExitChapter(player))
                    {
                        Duration = wipeDuration
                    });
                    break;
                case "Starfield":
                    SceneAs<Level>().Add(new StarfieldWipe(SceneAs<Level>(), false, () => ExitChapter(player))
                    {
                        Duration = wipeDuration
                    });
                    break;
                case "Wind":
                    SceneAs<Level>().Add(new WindWipe(SceneAs<Level>(), false, () => ExitChapter(player))
                    {
                        Duration = wipeDuration
                    });
                    break;
                case "Drop":
                    SceneAs<Level>().Add(new DropWipe(SceneAs<Level>(), false, () => ExitChapter(player))
                    {
                        Duration = wipeDuration
                    });
                    break;
                case "Fall":
                    SceneAs<Level>().Add(new FallWipe(SceneAs<Level>(), false, () => ExitChapter(player))
                    {
                        Duration = wipeDuration
                    });
                    break;
                case "KeyDoor":
                    SceneAs<Level>().Add(new KeyDoorWipe(SceneAs<Level>(), false, () => ExitChapter(player))
                    {
                        Duration = wipeDuration
                    });
                    break;
                case "Angled":
                    SceneAs<Level>().Add(new AngledWipe(SceneAs<Level>(), false, () => ExitChapter(player))
                    {
                        Duration = wipeDuration
                    });
                    break;
                case "Heart":
                    SceneAs<Level>().Add(new HeartWipe(SceneAs<Level>(), false, () => ExitChapter(player))
                    {
                        Duration = wipeDuration
                    });
                    break;
                case "Fade":
                    SceneAs<Level>().Add(new FadeWipe(SceneAs<Level>(), false, () => ExitChapter(player))
                    {
                        Duration = wipeDuration
                    });
                    break;
                default:
                    ExitChapter(player);
                    break;
            }
            yield return null;
        }

        public void ExitChapter(Player player)
        {
            int currentChapter = area.ChapterIndex == -1 ? 0 : area.ChapterIndex;
            XaphanModule.ModSaveData.DestinationRoom = DestinationRoom;
            XaphanModule.ModSaveData.Spawn = new Vector2(SpawnRoomX, SpawnRoomY);
            XaphanModule.ModSaveData.Wipe = wipeType;
            XaphanModule.ModSaveData.WipeDuration = wipeDuration;
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
                }
            }
            int chapterOffset = ToChapter - currentChapter;
            int currentChapterID = SceneAs<Level>().Session.Area.ID;
            if (RegisterCurrentChapterAsCompelete)
            {
                SceneAs<Level>().RegisterAreaComplete();
            }
            if (XaphanModule.useMergeChaptersController && (SceneAs<Level>().Session.Area.LevelSet == "Xaphan/0" ? !XaphanModule.ModSaveData.SpeedrunMode : true))
            {
                long currentTime = SceneAs<Level>().Session.Time;
                LevelEnter.Go(new Session(new AreaKey(currentChapterID + chapterOffset))
                {
                    Time = currentTime,
                    DoNotLoad = XaphanModule.ModSaveData.SavedNoLoadEntities.ContainsKey(SceneAs<Level>().Session.Area.LevelSet) ? XaphanModule.ModSaveData.SavedNoLoadEntities[SceneAs<Level>().Session.Area.LevelSet] : new HashSet<EntityID>(),
                    Strawberries = XaphanModule.ModSaveData.SavedSessionStrawberries.ContainsKey(SceneAs<Level>().Session.Area.LevelSet) ? XaphanModule.ModSaveData.SavedSessionStrawberries[SceneAs<Level>().Session.Area.LevelSet] : new HashSet<EntityID>()
                }
                , fromSaveData: false);
            }
            else
            {
                LevelEnter.Go(new Session(new AreaKey(currentChapterID + chapterOffset)), fromSaveData: false);
            }
        }
    }
}
