using System.Collections;
using System.Collections.Generic;
using Celeste.Mod.Entities;
using Celeste.Mod.XaphanHelper.Data;
using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste.Mod.XaphanHelper.Entities
{
    [Tracked(true)]
    [CustomEntity("XaphanHelper/TeleportToOtherSidePortal")]
    public class TeleportToOtherSidePortal : Entity
    {
        private PlayerCollider pc;

        private Sprite PortalSprite;

        private string side;

        private string WipeType;

        private float WipeDuration;

        private string WarpSfx;

        private bool TeleportToStartingSpawnOfChapter;

        private Coroutine TeleportRoutine = new();

        private bool EnteredPortal;

        private bool requireCassetteCollected;

        private bool requireCSideUnlocked;

        private bool RegisterCurrentSideAsCompelete;

        private string flags;

        public TeleportToOtherSidePortal(EntityData data, Vector2 offset) : base(data.Position + offset)
        {
            Collider = new Circle(12f);
            Add(pc = new PlayerCollider(OnCollide, Collider));
            Add(PortalSprite = new Sprite(GFX.Game, "objects/XaphanHelper/TeleportToOtherSidePortal/"));
            PortalSprite.AddLoop("A-Side", "A-Side", 0.07f);
            PortalSprite.AddLoop("B-Side", "B-Side", 0.07f);
            PortalSprite.AddLoop("C-Side", "C-Side", 0.07f);
            PortalSprite.Position -= new Vector2(16, 16);
            PortalSprite.Color = Color.White * 0.75f;
            side = data.Attr("side");
            PortalSprite.Play(side);
            requireCassetteCollected = data.Bool("requireCassetteCollected", false);
            requireCSideUnlocked = data.Bool("requireCSideUnlocked", false);
            flags = data.Attr("flags");
            WipeType = data.Attr("wipeType", "Fade");
            WipeDuration = data.Float("wipeDuration", 1.35f);
            WarpSfx = data.Attr("warpSfx");
            TeleportToStartingSpawnOfChapter = data.Bool("teleportToStartingSpawnOfChapter");
            RegisterCurrentSideAsCompelete = data.Bool("registerCurrentSideAsCompelete");
            Depth = 2000;
        }

        public override void Added(Scene scene)
        {
            base.Added(scene);
            bool hasCassette = false;
            bool hasCSide = false;
            bool hasFlags = true;
            foreach (AreaStats item in SaveData.Instance.Areas_Safe)
            {
                if (item.LevelSet == SceneAs<Level>().Session.Area.LevelSet && item.SID == SceneAs<Level>().Session.Area.SID)
                {
                    if (item.Cassette)
                    {
                        hasCassette = true;
                    }
                }
            }
            if (XaphanModule.ModSaveData.CSideUnlocked.Contains(SaveData.Instance.LevelSetStats.Name + ":" + SceneAs<Level>().Session.Area.ChapterIndex) || SaveData.Instance.UnlockedModes >= 3)
            {
                hasCSide = true;
            }
            if (!string.IsNullOrEmpty(flags))
            {
                string[] flagsStr = flags.Split(',');
                foreach (string flag in flagsStr)
                {
                    if (!SceneAs<Level>().Session.GetFlag(flag))
                    {
                        hasFlags = false;
                        break;
                    }
                }
            }
            if ((side == "B-Side" && !AreaData.Areas[SceneAs<Level>().Session.Area.ID].HasMode(AreaMode.BSide)) || (side == "C-Side" && !AreaData.Areas[SceneAs<Level>().Session.Area.ID].HasMode(AreaMode.CSide)))
            {
                RemoveSelf();
            }
            else if (!SaveData.Instance.DebugMode && (!hasFlags || (requireCassetteCollected && !hasCassette) || (requireCSideUnlocked && !hasCSide)))
            {
                RemoveSelf();
            }
            Add(new Coroutine(NoWarpdelay()));
        }

        private IEnumerator NoWarpdelay()
        {
            yield return 0.1f;
            if (XaphanModule.ChangingSide)
            {
                Player player = SceneAs<Level>().Tracker.GetEntity<Player>();
                while (player != null && player.CollideCheck<TeleportToOtherSidePortal>())
                {
                    yield return null;
                }
                XaphanModule.ChangingSide = false;
            }
        }

        private void OnCollide(Player player)
        {
            if (!XaphanModule.ChangingSide)
            {
                player.StateMachine.State = Player.StDummy;
                player.DummyGravity = false;
                player.DummyAutoAnimate = false;
                if (!TeleportRoutine.Active && !EnteredPortal)
                {
                    Add(TeleportRoutine = new Coroutine(MovePlayer(player)));
                }
            }
        }

        private IEnumerator MovePlayer(Player player)
        {
            if (RegisterCurrentSideAsCompelete)
            {
                SceneAs<Level>().RegisterAreaComplete();
            }
            XaphanModule.PlayerHasGolden = false;
            player.Sprite.Play("spin");
            int moves = 25;
            while (moves > 0 || (player.Center.X == Center.X && player.Center.Y == Center.Y + 8))
            {
                player.Speed = Vector2.Zero;
                player.MoveTowardsX(Center.X, 50f * Engine.DeltaTime);
                player.MoveTowardsY(Center.Y + 8, 50f * Engine.DeltaTime);
                yield return null;
                moves--;
            }
            Audio.Play(string.IsNullOrEmpty(WarpSfx) ? "event:/game/xaphan/warp" : WarpSfx);
            while (player.Sprite.Scale.X > 0.3f)
            {
                player.Sprite.Scale *= 0.9f;
                player.MoveTowardsY(Center.Y + 4, 50f * Engine.DeltaTime);
                yield return null;
            }
            SceneAs<Level>().Displacement.AddBurst(Center, 0.5f, 24f, 96f, 0.4f);
            while (player.Sprite.Scale.X > 0.1f)
            {
                player.Sprite.Scale *= 0.5f;
                player.MoveTowardsY(Center.Y + 4, 50f * Engine.DeltaTime);
                yield return null;
            }
            player.Sprite.Visible = player.Hair.Visible = false;
            Audio.SetAmbience(null);
            Audio.SetMusic(null);
            switch (WipeType)
            {
                case "Spotlight":
                    SceneAs<Level>().Add(new SpotlightWipe(SceneAs<Level>(), false, () => Teleport())
                    {
                        Duration = WipeDuration
                    });
                    break;
                case "Curtain":
                    SceneAs<Level>().Add(new CurtainWipe(SceneAs<Level>(), false, () => Teleport())
                    {
                        Duration = WipeDuration
                    });
                    break;
                case "Mountain":
                    SceneAs<Level>().Add(new MountainWipe(SceneAs<Level>(), false, () => Teleport())
                    {
                        Duration = WipeDuration
                    });
                    break;
                case "Dream":
                    SceneAs<Level>().Add(new DreamWipe(SceneAs<Level>(), false, () => Teleport())
                    {
                        Duration = WipeDuration
                    });
                    break;
                case "Starfield":
                    SceneAs<Level>().Add(new StarfieldWipe(SceneAs<Level>(), false, () => Teleport())
                    {
                        Duration = WipeDuration
                    });
                    break;
                case "Wind":
                    SceneAs<Level>().Add(new WindWipe(SceneAs<Level>(), false, () => Teleport())
                    {
                        Duration = WipeDuration
                    });
                    break;
                case "Drop":
                    SceneAs<Level>().Add(new DropWipe(SceneAs<Level>(), false, () => Teleport())
                    {
                        Duration = WipeDuration
                    });
                    break;
                case "Fall":
                    SceneAs<Level>().Add(new FallWipe(SceneAs<Level>(), false, () => Teleport())
                    {
                        Duration = WipeDuration
                    });
                    break;
                case "KeyDoor":
                    SceneAs<Level>().Add(new KeyDoorWipe(SceneAs<Level>(), false, () => Teleport())
                    {
                        Duration = WipeDuration
                    });
                    break;
                case "Angled":
                    SceneAs<Level>().Add(new AngledWipe(SceneAs<Level>(), false, () => Teleport())
                    {
                        Duration = WipeDuration
                    });
                    break;
                case "Heart":
                    SceneAs<Level>().Add(new HeartWipe(SceneAs<Level>(), false, () => Teleport())
                    {
                        Duration = WipeDuration
                    });
                    break;
                default:
                    SceneAs<Level>().Add(new FadeWipe(SceneAs<Level>(), false, () => Teleport())
                    {
                        Duration = WipeDuration
                    });
                    break;
            }
            EnteredPortal = true;
        }

        private void Teleport()
        {
            if (!TeleportToStartingSpawnOfChapter)
            {
                int DestinationSideIndex = 0;
                string CurrentModeSide = "A-Side";
                if (side == "B-Side")
                {
                    DestinationSideIndex = 1;
                }
                else if (side == "C-Side")
                {
                    DestinationSideIndex = 2;
                }
                if (SceneAs<Level>().Session.Area.Mode == AreaMode.BSide)
                {
                    CurrentModeSide = "B-Side";
                }
                else if (SceneAs<Level>().Session.Area.Mode == AreaMode.CSide)
                {
                    CurrentModeSide = "C-Side";
                }
                foreach (TeleportToOtherSideData data in XaphanModule.TeleportToOtherSideData)
                {
                    if (data.Mode == DestinationSideIndex && data.Destination == CurrentModeSide)
                    {
                        XaphanModule.ModSaveData.DestinationRoom = data.Room;
                        XaphanModule.ModSaveData.Spawn = data.Position;
                        XaphanModule.ModSaveData.Wipe = WipeType;
                        XaphanModule.ModSaveData.WipeDuration = 1f;
                        MapData MapData = AreaData.Areas[SceneAs<Level>().Session.Area.ID].Mode[DestinationSideIndex].MapData;
                        if (data.Room == MapData.StartLevel().Name)
                        {
                            XaphanModule.ModSaveData.ConsiderBeginning = true;
                        }
                        break;
                    }
                }
            }
            XaphanModule.PlayerHasGolden = false;
            XaphanModule.ChangingSide = true;
            switch (side)
            {
                case "A-Side":
                    if (XaphanModule.useMergeChaptersController)
                    {
                        LevelEnter.Go(new Session(new AreaKey(SceneAs<Level>().Session.Area.ID, AreaMode.Normal))
                        {
                            Time = XaphanModule.ModSaveData.SavedTime.ContainsKey(SceneAs<Level>().Session.Area.LevelSet) ? XaphanModule.ModSaveData.SavedTime[SceneAs<Level>().Session.Area.LevelSet] : 0L,
                            DoNotLoad = XaphanModule.ModSaveData.SavedNoLoadEntities.ContainsKey(SceneAs<Level>().Session.Area.LevelSet) ? XaphanModule.ModSaveData.SavedNoLoadEntities[SceneAs<Level>().Session.Area.LevelSet] : new HashSet<EntityID>(),
                            Strawberries = XaphanModule.ModSaveData.SavedSessionStrawberries.ContainsKey(SceneAs<Level>().Session.Area.LevelSet) ? XaphanModule.ModSaveData.SavedSessionStrawberries[SceneAs<Level>().Session.Area.LevelSet] : new HashSet<EntityID>()
                        }
                        , fromSaveData: false);
                    }
                    else
                    {
                        LevelEnter.Go(new Session(new AreaKey(SceneAs<Level>().Session.Area.ID, AreaMode.Normal)), fromSaveData: false);
                    }

                    break;
                case "B-Side":
                    SceneAs<Level>().Session.Cassette = true;
                    SaveData.Instance.RegisterCassette(SceneAs<Level>().Session.Area);
                    LevelEnter.Go(new Session(new AreaKey(SceneAs<Level>().Session.Area.ID, AreaMode.BSide)), fromSaveData: false);
                    break;
                case "C-Side":
                    if (!XaphanModule.ModSaveData.CSideUnlocked.Contains(SceneAs<Level>().Session.Area.LevelSet + ":" + SceneAs<Level>().Session.Area.ChapterIndex))
                    {
                        XaphanModule.ModSaveData.CSideUnlocked.Add(SceneAs<Level>().Session.Area.LevelSet + ":" + SceneAs<Level>().Session.Area.ChapterIndex);
                    }
                    LevelEnter.Go(new Session(new AreaKey(SceneAs<Level>().Session.Area.ID, AreaMode.CSide)), fromSaveData: false);
                    break;
            }
        }
    }
}
