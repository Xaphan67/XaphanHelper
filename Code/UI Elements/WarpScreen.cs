using System;
using System.Collections;
using System.Collections.Generic;
using Celeste.Mod.XaphanHelper.Managers;
using Microsoft.Xna.Framework;
using Monocle;
using MonoMod.Utils;

namespace Celeste.Mod.XaphanHelper.UI_Elements
{
    [Tracked]
    public class WarpScreen : Entity
    {
        private static readonly MTexture arrowTex = GFX.Gui["towerarrow"];

        private static readonly MTexture playerIcon = GFX.Gui["maps/player"];
        private static readonly MTexture playerIconHair = GFX.Gui["maps/player_hair"];
        private static readonly string areaProgressLabel = Dialog.Clean("XaphanHelper_UI_progress_area");
        private static readonly string subareaProgressLabel = Dialog.Clean("XaphanHelper_UI_progress_subarea");
        private static readonly string showProgressLabel = Dialog.Clean("XaphanHelper_UI_showProgress");
        private static readonly string changeProgressLabel = Dialog.Clean("XaphanHelper_UI_changeProgress");
        private static readonly string hideProgressLabel = Dialog.Clean("XaphanHelper_UI_hideProgress");
        private readonly Wiggler progressWiggle;
        private MapDisplay mapDisplay;
        private MapProgressDisplay mapProgressDisplay;
        private float progressWiggleDelay;
        private bool displayIcon;

        private readonly string confirmSfx;
        private readonly string wipeType;
        private readonly float wipeDuration;
        private readonly string currentWarp;
        private string title;

        private int currentMenu;
        private WarpMenu warpMenu;
        private readonly List<List<WarpInfo>> warpsPerArea = new();

        public WarpScreen(string currentWarp = "", string confirmSfx = "event:/game/xaphan/warp", string wipeType = "Fade", float wipeDuration = 0.75f)
        {
            this.currentWarp = currentWarp;
            this.confirmSfx = confirmSfx;
            this.wipeType = wipeType;
            this.wipeDuration = wipeDuration;

            Tag = Tags.HUD;
            Depth = -10002;

            Add(progressWiggle = Wiggler.Create(0.4f, 4f));
        }

        public WarpInfo SelectedWarp => warpMenu.SelectedWarp;

        public List<WarpInfo> ActiveWarps => warpsPerArea[currentMenu];

        public override void Added(Scene scene)
        {
            base.Added(scene);
            XaphanModule.UIOpened = true;
            Level level = Scene as Level;

            int currentAreaId = level.Session.Area.ID;
            string levelSet = level.Session.Area.LevelSet;
            LevelSetStats stats = SaveData.Instance.GetLevelSetStatsFor(levelSet);

            for (int i = 0; i < stats.Areas.Count; i++)
            {
                int areaId = stats.AreaOffset + i;

                List<WarpInfo> warps = WarpManager.GetWarpTargets(areaId);
                if (warps.Count > 0)
                {
                    if (areaId == currentAreaId)
                    {
                        currentMenu = warpsPerArea.Count;
                    }
                    warpsPerArea.Add(warps);
                }
            }

            Scene.Add(warpMenu = new WarpMenu()
            {
                CurrentWarp = currentWarp,
                ConfirmSfx = confirmSfx,
                OnCancel = CloseScreen,
                OnESC = CloseScreen,
                OnUpdate = UpdateMenu,
                WipeDuration = wipeDuration,
                WipeType = wipeType
            });

            OpenScreen();
        }

        public override void Render()
        {
            base.Render();
            if (mapDisplay?.Scene != null)
            {
                Draw.Rect(new Vector2(-10, -10), 1940, 182, Color.Black);
                Draw.Rect(new Vector2(-10, 172), 100, 856, Color.Black);
                Draw.Rect(new Vector2(1030, 172), 900, 856, Color.Black);
                Draw.Rect(new Vector2(-10, 1028), 1940, 62, Color.Black);
                Draw.Rect(new Vector2(90, 172), 940, 8, Color.White);
                Draw.Rect(new Vector2(90, 180), 10, 840, Color.White);
                Draw.Rect(new Vector2(1020, 180), 10, 840, Color.White);
                Draw.Rect(new Vector2(90, 1020), 940, 8, Color.White);
            }

            float colorAlpha = SceneAs<Level>().FormationBackdrop.Display ? (float)DynamicData.For(SceneAs<Level>().FormationBackdrop).Get("fade") : 1f;

            if (!string.IsNullOrEmpty(title) && XaphanModule.ShowUI)
            {
                ActiveFont.DrawEdgeOutline(title, new Vector2(Celeste.TargetWidth / 2f, 80f), new Vector2(0.5f, 0.5f), Vector2.One * 2f, Color.Gray * colorAlpha, 4f, Color.DarkSlateBlue * colorAlpha, 2f, Color.Black * colorAlpha);
                if (currentMenu > 0)
                {
                    arrowTex.DrawCentered(new Vector2(960f - ActiveFont.Measure(title).X - 100f, 80f), Color.White * colorAlpha);
                }
                if (currentMenu < warpsPerArea.Count - 1)
                {
                    arrowTex.DrawCentered(new Vector2(960f + ActiveFont.Measure(title).X + 100f, 80f), Color.White * colorAlpha, 1f, (float)Math.PI);
                }
            }

            if (warpMenu.LastPossibleSelection > 10 && warpMenu.Selection <= ActiveWarps.Count - 5)
            {
                arrowTex.DrawCentered(new Vector2(warpMenu.X, 1009f), Color.White, 1f, (float)Math.PI * 3f / 2f);
            }
            if (warpMenu.Selection > 6 && warpMenu.LastPossibleSelection > 10)
            {
                arrowTex.DrawCentered(new Vector2(warpMenu.X, 190f), Color.White, 1f, (float)Math.PI / 2f);
            }

            if (mapDisplay?.Scene != null)
            {
                if (mapProgressDisplay?.Scene != null && !mapProgressDisplay.Hidden)
                {
                    float scale = 0.5f;
                    Vector2 position = new(1030f, 1055f);
                    string progressDisplayStatus = mapProgressDisplay.mode == 0 ? (mapProgressDisplay.getSubAreaIndex() == -1 || mapProgressDisplay.SubAreaControllerData.Count == 1 ? hideProgressLabel : changeProgressLabel) : mapProgressDisplay.mode == 1 ? hideProgressLabel : showProgressLabel;
                    ButtonBindingButtonUI.Render(position, progressDisplayStatus, XaphanModule.ModSettings.MapScreenShowProgressDisplay, scale, 1f, progressWiggle.Value * 0.05f);
                    if (mapProgressDisplay.mode != 2)
                    {
                        string progressDisplayMode = mapProgressDisplay.mode == 0 ? areaProgressLabel : subareaProgressLabel;
                        float progressDisplayWidth = ActiveFont.Measure(progressDisplayMode).X;
                        ActiveFont.Draw(progressDisplayMode, new Vector2(90 + progressDisplayWidth / 4, position.Y), new Vector2(0.5f), new Vector2(scale), Color.White);
                    }
                }

                if (displayIcon)
                {
                    Vector2 iconPos = new(mapDisplay.Grid.Left + 441f, mapDisplay.Grid.Top + 401f);
                    playerIcon.Draw(iconPos);
                    playerIconHair.Draw(iconPos, Vector2.Zero, Scene.Tracker.GetEntity<Player>()?.Hair.Color ?? Color.White);
                }
            }
        }

        private void OpenScreen()
        {
            Level level = Scene as Level;
            level.PauseLock = true;
            level.Session.SetFlag("Map_Opened", true);
            level.Tracker.GetEntity<CountdownDisplay>()?.StopTimer(true, false);

            if (Scene.Tracker.GetEntity<Player>() is Player player)
            {
                player.StateMachine.State = Player.StDummy;
            }

            Audio.Play(SFX.ui_game_pause);
            Add(new Coroutine(TransitionRoutine(onFadeOut: InitializeScreen)));
        }

        private void InitializeScreen()
        {
            XaphanModule.ShowUI = true;
            warpMenu.UpdateWarps(ActiveWarps);

            AreaKey area = new(SelectedWarp.AreaId);
            title = Dialog.Clean(area.SID);

            bool usingMap = false;
            MapData mapData = AreaData.Areas[SelectedWarp.AreaId].Mode[0].MapData;
            if (mapData.GetEntityData("XaphanHelper/InGameMapController") is EntityData mapController)
            {
                string mapName = mapController.Attr("mapName");
                if (!string.IsNullOrEmpty(mapName))
                {
                    title = Dialog.Clean(mapName);
                }
                usingMap = true;
                Add(new Coroutine(MapRoutine()));
            }
            else
            {
                mapDisplay?.RemoveSelf();
                mapProgressDisplay?.RemoveSelf();
            }

            warpMenu.Visible = true;
            warpMenu.center = !usingMap;

            SceneAs<Level>().FormationBackdrop.Display = !usingMap;
        }

        public void UninitializeScreen()
        {
            XaphanModule.ShowUI = false;
            warpMenu.Close();
            mapDisplay?.RemoveSelf();
            mapProgressDisplay?.RemoveSelf();
            SceneAs<Level>().FormationBackdrop.Display = false;
        }

        public void StartDelay()
        {
            Add(new Coroutine(ControlDelayRoutine()));
        }

        private IEnumerator ControlDelayRoutine()
        {
            float timer = 0.05f;
            while (timer > 0)
            {
                yield return null;
                timer -= Engine.DeltaTime;
            }
            if (Scene.Tracker.GetEntity<Player>() is Player player)
            {
                player.StateMachine.State = Player.StNormal;
            }
            XaphanModule.ShowUI = false;
            RemoveSelf();
        }

        private void CloseScreen()
        {
            Audio.Play(SFX.ui_game_unpause);
            MapData mapData = AreaData.Areas[SceneAs<Level>().Session.Area.ID].Mode[0].MapData;
            if (!mapData.HasEntity("XaphanHelper/InGameMapController"))
            {
                Level level = Scene as Level;
                UninitializeScreen();
                level.PauseLock = false;
                level.Tracker.GetEntity<CountdownDisplay>()?.StopTimer(false, true);
                if (Scene.Tracker.GetEntity<Player>() is Player player)
                {
                    player.StateMachine.State = Player.StNormal;
                }
                XaphanModule.UIOpened = false;
                RemoveSelf();
            }
            else
            {
                Add(new Coroutine(TransitionRoutine(onFadeOut: UninitializeScreen, onFadeIn: () =>
                {
                    Level level = Scene as Level;
                    level.PauseLock = false;
                    level.Session.SetFlag("Map_Opened", false);
                    level.Tracker.GetEntity<CountdownDisplay>()?.StopTimer(false, true);

                    if (Scene.Tracker.GetEntity<Player>() is Player player)
                    {
                        player.StateMachine.State = Player.StNormal;
                    }
                    XaphanModule.ShowUI = false;
                    XaphanModule.UIOpened = false;
                    RemoveSelf();
                })));
            }
        }

        private IEnumerator TransitionRoutine(float duration = 0.5f, Action onFadeOut = null, Action onFadeIn = null)
        {
            duration = Math.Max(0f, duration);
            MapData mapData = AreaData.Areas[SceneAs<Level>().Session.Area.ID].Mode[0].MapData;
            warpMenu.Focused = false;

            if (mapData.HasEntity("XaphanHelper/InGameMapController"))
            {
                yield return new FadeWipe(Scene, false)
                {
                    Duration = duration / 2f,
                    OnComplete = onFadeOut
                }.Wait();

                yield return new FadeWipe(Scene, true)
                {
                    Duration = duration / 2f,
                    OnComplete = onFadeIn
                }.Wait();
            }
            else
            {
                InitializeScreen();
            }
            warpMenu.Focused = true;
        }

        private IEnumerator MapRoutine()
        {
            Level level = SceneAs<Level>();

            if (mapDisplay == null)
            {
                Scene.Add(mapDisplay = new MapDisplay(level, "warp")
                {
                    currentRoom = level.Session.Level
                });
                yield return mapDisplay.GenerateMap();
            }

            mapDisplay.Display = false;
            // FIXME: hackfix for now, MapDisplay.UpdateMap should be refactored to use area IDs
            int chapterIndex = new AreaKey(SelectedWarp.AreaId).ChapterIndex;
            mapDisplay.UpdateMap(Math.Max(0, chapterIndex), SelectedWarp.Room, 0);

            mapProgressDisplay?.RemoveSelf();
            if (mapDisplay.InGameMapControllerData.ShowProgress != "Never")
            {
                Scene.Add(mapProgressDisplay = new MapProgressDisplay(new Vector2(mapDisplay.Grid.X + 18f, mapDisplay.Grid.Y), level, mapDisplay.InGameMapControllerData, mapDisplay.SubAreaControllerData, mapDisplay.RoomControllerData, mapDisplay.TilesControllerData, mapDisplay.EntitiesData, mapDisplay.chapterIndex, mapDisplay.currentRoom));
            }
        }

        private void UpdateMenu()
        {
            progressWiggleDelay -= Engine.DeltaTime;

            if (Scene.OnRawInterval(0.3f))
            {
                displayIcon = !displayIcon;
            }

            if (!warpMenu.Focused)
            {
                return;
            }

            if (Input.MenuLeft.Pressed && currentMenu > 0)
            {
                Audio.Play("event:/ui/main/rollover_down");
                currentMenu--;
                InitializeScreen();
            }
            else if (Input.MenuRight.Pressed && currentMenu < warpsPerArea.Count - 1)
            {
                Audio.Play("event:/ui/main/rollover_up");
                currentMenu++;
                InitializeScreen();
            }
            else
            {
                if (mapDisplay != null)
                {
                    if (mapDisplay.currentRoom != SelectedWarp.Room)
                    {
                        mapDisplay.currentRoom = SelectedWarp.Room;
                        // FIXME: hackfix for now, MapDisplay.GetRoomOffset should be refactored to accept WarpInfo (?)
                        Vector2 roomOffset = mapDisplay.GetRoomOffset(null, SelectedWarp.Room, 0);
                        mapDisplay.SetCurrentRoomCoordinates(roomOffset);
                    }

                    if (mapProgressDisplay != null && XaphanModule.ModSettings.MapScreenShowProgressDisplay.Check && progressWiggleDelay <= 0f)
                    {
                        progressWiggle.Start();
                        progressWiggleDelay = 0.5f;
                    }
                }
            }
        }
    }
}
