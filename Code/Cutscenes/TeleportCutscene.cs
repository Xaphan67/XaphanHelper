using System.Collections;
using System.Reflection;
using Celeste.Mod.XaphanHelper.Controllers;
using Celeste.Mod.XaphanHelper.UI_Elements;
using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste.Mod.XaphanHelper.Cutscenes
{
    class TeleportCutscene : CutsceneEntity
    {
        private readonly Player player;

        private readonly Vector2 spawnPoint;

        private readonly string room;

        private float timer;

        private int cameraX;

        private int cameraY;

        private bool cameraOnPlayer;

        private bool fromElevator;

        private bool skipFirstWipe;

        private string wipeType;

        private float wipeDuration;

        private bool respawnAnim;

        private bool wakeUpAnim;

        private Vector2 spawnPosition;

        private bool oldRespawn;

        private bool useLevelWipe;

        private bool faceLeft;

        private float currentSpriterate;

        private bool drone;

        private static FieldInfo PlayerOnGround = typeof(Player).GetField("onGround", BindingFlags.Instance | BindingFlags.NonPublic);

        public TeleportCutscene(Player player, string room, Vector2 spawnPoint, int cameraX, int cameraY, bool cameraOnPlayer, float timer, string wipeType, float wipeDuration = 0.5f, bool fromElevator = false, bool skipFirstWipe = false,
            bool respawnAnim = false, bool useLevelWipe = false, bool wakeUpAnim = false, float spawnPositionX = 0f, float spawnPositionY = 0f, bool oldRespawn = false, bool faceLeft = false, bool drone = false) : base(false)
        {
            Tag = Tags.FrozenUpdate;
            this.player = player;
            this.room = room;
            this.spawnPoint = spawnPoint;
            this.timer = timer;
            this.wipeType = wipeType;
            this.wipeDuration = wipeDuration;
            this.cameraX = cameraX;
            this.cameraY = cameraY;
            this.cameraOnPlayer = cameraOnPlayer;
            this.fromElevator = fromElevator;
            this.skipFirstWipe = skipFirstWipe;
            this.respawnAnim = respawnAnim;
            this.wakeUpAnim = wakeUpAnim;
            this.useLevelWipe = useLevelWipe;
            this.oldRespawn = oldRespawn;
            this.faceLeft = faceLeft;
            this.drone = drone;
            spawnPosition = new Vector2(spawnPositionX, spawnPositionY);
        }

        public override void OnBegin(Level level)
        {
            Add(new Coroutine(Cutscene(level)));
        }

        private IEnumerator Cutscene(Level level)
        {
            player.StateMachine.State = Player.StDummy;
            yield return null;
            while (timer > 0f)
            {
                yield return null;
                timer -= Engine.DeltaTime;
            }
            if (level.Wipe != null)
            {
                level.Wipe.Cancel();
            }
            if (!skipFirstWipe)
            {
                if (useLevelWipe)
                {
                    level.DoScreenWipe(false, () => EndCutscene(level));
                }
                else
                {
                    switch (wipeType)
                    {
                        case "Spotlight":
                            SpotlightWipe WipeA = new(level, false, () => EndCutscene(level))
                            {
                                Duration = wipeDuration
                            };
                            level.Add(WipeA);
                            break;
                        case "Curtain":
                            CurtainWipe WipeB = new(level, false, () => EndCutscene(level))
                            {
                                Duration = wipeDuration
                            };
                            level.Add(WipeB);
                            break;
                        case "Mountain":
                            MountainWipe WipeC = new(level, false, () => EndCutscene(level))
                            {
                                Duration = wipeDuration
                            };
                            level.Add(WipeC);
                            break;
                        case "Dream":
                            DreamWipe WipeD = new(level, false, () => EndCutscene(level))
                            {
                                Duration = wipeDuration
                            };
                            level.Add(WipeD);
                            break;
                        case "Starfield":
                            StarfieldWipe WipeE = new(level, false, () => EndCutscene(level))
                            {
                                Duration = wipeDuration
                            };
                            level.Add(WipeE);
                            break;
                        case "Wind":
                            WindWipe WipeF = new(level, false, () => EndCutscene(level))
                            {
                                Duration = wipeDuration
                            };
                            level.Add(WipeF);
                            break;
                        case "Drop":
                            DropWipe WipeG = new(level, false, () => EndCutscene(level))
                            {
                                Duration = wipeDuration
                            };
                            level.Add(WipeG);
                            break;
                        case "Fall":
                            FallWipe WipeH = new(level, false, () => EndCutscene(level))
                            {
                                Duration = wipeDuration
                            };
                            level.Add(WipeH);
                            break;
                        case "KeyDoor":
                            KeyDoorWipe WipeI = new(level, false, () => EndCutscene(level))
                            {
                                Duration = wipeDuration
                            };
                            level.Add(WipeI);
                            break;
                        case "Angled":
                            AngledWipe WipeJ = new(level, false, () => EndCutscene(level))
                            {
                                Duration = wipeDuration
                            };
                            level.Add(WipeJ);
                            break;
                        case "Heart":
                            HeartWipe WipeK = new(level, false, () => EndCutscene(level))
                            {
                                Duration = wipeDuration
                            };
                            level.Add(WipeK);
                            break;
                        case "Fade":
                            FadeWipe WipeL = new(level, false, () => EndCutscene(level))
                            {
                                Duration = wipeDuration
                            };
                            level.Add(WipeL);
                            break;
                        default:
                            EndCutscene(level);
                            break;
                    }
                }
            }
            else
            {
                EndCutscene(level);
            }
        }

        public override void OnEnd(Level level)
        {
            level.OnEndOfFrame += () =>
            {
                XaphanModule.ShowUI = false;
                string Prefix = level.Session.Area.LevelSet;
                XaphanModule.ModSaveData.SavedFlags.Add(Prefix + "_teleporting");
                Leader.StoreStrawberries(player.Leader);
                level.Remove(player);
                level.UnloadLevel();
                level.Session.Level = room;
                level.Session.FirstLevel = false;
                if (spawnPosition == Vector2.Zero && !oldRespawn)
                {
                    level.Session.RespawnPoint = level.GetSpawnPoint(new Vector2(level.Bounds.Left, level.Bounds.Top) + spawnPoint);
                }
                level.LoadLevel((XaphanModule.ModSaveData.CountdownIntroType || respawnAnim) ? Player.IntroTypes.Respawn : Player.IntroTypes.None);
                if (spawnPosition != Vector2.Zero)
                {
                    Player player2 = level.Tracker.GetEntity<Player>();
                    player2.Position = spawnPosition;
                    if (XaphanModule.useMergeChaptersController && XaphanModule.MergeChaptersControllerMode == "Warps")
                    {
                        level.Session.RespawnPoint = level.GetSpawnPoint(spawnPosition);
                    }
                    if (wakeUpAnim)
                    {
                        player2.StateMachine.State = 11;
                        player2.DummyAutoAnimate = false;
                        currentSpriterate = player2.Sprite.Rate;
                        player2.Sprite.Play("wakeUp");
                        player2.Sprite.Rate = 2f;
                        player2.Sprite.OnLastFrame += RestaurePreviousRate;
                    }
                    if (faceLeft)
                    {
                        player2.Facing = Facings.Left;
                    }
                    else
                    {
                        player2.Facing = Facings.Right;
                    }
                    int Maxtry = 0;
                    if (!XaphanModule.onSlope)
                    {
                        for (int i = 1; i <= 184; i++)
                        {
                            if (!(bool)PlayerOnGround.GetValue(player2))
                            {
                                player2.MoveV(1);
                            }
                            else
                            {
                                Maxtry++;
                            }
                        }
                    }
                    if (Maxtry > 184)
                    {
                        player2.Position = level.Session.RespawnPoint.GetValueOrDefault();
                    }
                }
                if (XaphanModule.ModSaveData.CountdownIntroType)
                {
                    XaphanModule.ModSaveData.CountdownIntroType = false;
                }
                if (level.Wipe != null)
                {
                    level.Wipe.Cancel();
                }
                if (fromElevator)
                {
                    int chapterIndex = level.Session.Area.ChapterIndex == -1 ? 0 : level.Session.Area.ChapterIndex;
                    if (!XaphanModule.ModSaveData.VisitedRooms.Contains(Prefix + "/Ch" + chapterIndex + "/" + room))
                    {
                        XaphanModule.ModSaveData.VisitedRooms.Add(Prefix + "/Ch" + chapterIndex + "/" + room);
                    }
                }
                if (cameraOnPlayer)
                {
                    Player player = level.Tracker.GetEntity<Player>();
                    level.Camera.Position = player.CameraTarget;
                }
                else
                {
                    level.Camera.Position = new Vector2(level.Bounds.Left + cameraX, level.Bounds.Top + cameraY);
                }
                CountdownDisplay timerDisplay = level.Tracker.GetEntity<CountdownDisplay>();
                if (timerDisplay != null)
                {
                    if (fromElevator)
                    {
                        timerDisplay.StopTimer(true, true);
                    }
                    else
                    {
                        timerDisplay.StopTimer(false, true);
                    }
                }
                if (useLevelWipe)
                {
                    level.DoScreenWipe(true);
                }
                else
                {
                    switch (wipeType)
                    {
                        case "Spotlight":
                            SpotlightWipe WipeA = new(level, true)
                            {
                                Duration = wipeDuration
                            };
                            level.Add(WipeA);
                            break;
                        case "Curtain":
                            CurtainWipe WipeB = new(level, true)
                            {
                                Duration = wipeDuration
                            };
                            level.Add(WipeB);
                            break;
                        case "Mountain":
                            MountainWipe WipeC = new(level, true)
                            {
                                Duration = wipeDuration
                            };
                            level.Add(WipeC);
                            break;
                        case "Dream":
                            DreamWipe WipeD = new(level, true)
                            {
                                Duration = wipeDuration
                            };
                            level.Add(WipeD);
                            break;
                        case "Starfield":
                            StarfieldWipe WipeE = new(level, true)
                            {
                                Duration = wipeDuration
                            };
                            level.Add(WipeE);
                            break;
                        case "Wind":
                            WindWipe WipeF = new(level, true)
                            {
                                Duration = wipeDuration
                            };
                            level.Add(WipeF);
                            break;
                        case "Drop":
                            DropWipe WipeG = new(level, true)
                            {
                                Duration = wipeDuration
                            };
                            level.Add(WipeG);
                            break;
                        case "Fall":
                            FallWipe WipeH = new(level, true)
                            {
                                Duration = wipeDuration
                            };
                            level.Add(WipeH);
                            break;
                        case "KeyDoor":
                            KeyDoorWipe WipeI = new(level, true)
                            {
                                Duration = wipeDuration
                            };
                            level.Add(WipeI);
                            break;
                        case "Angled":
                            AngledWipe WipeJ = new(level, true)
                            {
                                Duration = wipeDuration
                            };
                            level.Add(WipeJ);
                            break;
                        case "Heart":
                            HeartWipe WipeK = new(level, true)
                            {
                                Duration = wipeDuration
                            };
                            level.Add(WipeK);
                            break;
                        case "Fade":
                            FadeWipe WipeL = new(level, true)
                            {
                                Duration = wipeDuration
                            };
                            level.Add(WipeL);
                            break;
                        default:
                            break;
                    }
                }

                AreaKey area = level.Session.Area;
                MapData MapData = AreaData.Areas[area.ID].Mode[(int)area.Mode].MapData;
                bool HasEnvironmentalController = false;
                foreach (LevelData levelData in MapData.Levels)
                {
                    if (levelData.Name == level.Session.Level)
                    {
                        foreach (EntityData entity in levelData.Entities)
                        {
                            if (entity.Name == "XaphanHelper/EnvironmentalController")
                            {
                                HasEnvironmentalController = true;
                                break;
                            }
                        }
                    }
                }
                if (!HasEnvironmentalController)
                {
                    foreach (EnvironmentalController controller in level.Entities.FindAll<EnvironmentalController>())
                    {
                        controller.RemoveSelf();
                    }
                }

                if (drone)
                {
                    XaphanModule.ModSaveData.fakePlayerFacing.Remove(level.Session.Area.LevelSet);
                    XaphanModule.ModSaveData.fakePlayerPosition.Remove(level.Session.Area.LevelSet);
                    XaphanModule.ModSaveData.droneStartRoom.Remove(level.Session.Area.LevelSet);
                    XaphanModule.ModSaveData.droneStartSpawn.Remove(level.Session.Area.LevelSet);
                    XaphanModule.TeleportBackFromDrone = false;
                }

                Leader.RestoreStrawberries(level.Tracker.GetEntity<Player>().Leader);
                XaphanModule.ModSaveData.SavedFlags.Remove(Prefix + "_teleporting");
                level.Tracker.GetEntity<Player>().StateMachine.State = Player.StNormal;
                XaphanModule.ModSaveData.CanDisplayPopups = true;
            };
        }

        private void RestaurePreviousRate(string s)
        {
            player.Sprite.Rate = currentSpriterate;
            player.StateMachine.State = Player.StNormal;
            player.DummyAutoAnimate = true;
        }
    }
}
