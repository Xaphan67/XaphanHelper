using System.Collections;
using Celeste.Mod.Entities;
using Celeste.Mod.XaphanHelper.Managers;
using Celeste.Mod.XaphanHelper.UI_Elements;
using Microsoft.Xna.Framework;
using Monocle;
using MonoMod.Utils;

namespace Celeste.Mod.XaphanHelper.Entities
{
    [Tracked(true)]
    [CustomEntity("XaphanHelper/WarpStation")]
    public class WarpStation : Solid
    {
        private Sprite warpStationSprite;

        private WarpBeam beam;

        private TalkComponent talker;

        private bool activated;

        private bool noBeam;

        private Color beamColor;

        private string flag;

        private string noInteractFlag;

        private string forceInactiveFlag;

        private string sprite;

        private int index;

        private string confirmSfx;

        private string wipeType;

        private float wipeDuration;

        private bool ShouldSave;

        private string warpId;

        private bool onlyCurrentChapter;

        private Level level => (Level)Scene;

        public WarpStation(EntityData data, Vector2 offset) : base(data.Position + offset, data.Width, data.Height, safe: true)
        {
            Tag = Tags.TransitionUpdate;
            index = data.Int("index", 0);
            sprite = string.IsNullOrEmpty(data.Attr("sprite")) ? "objects/XaphanHelper/WarpStation" : data.Attr("sprite");
            confirmSfx = string.IsNullOrEmpty(data.Attr("confirmSfx")) ? "event:/game/xaphan/warp" : data.Attr("confirmSfx");
            wipeType = string.IsNullOrEmpty(data.Attr("wipeType")) ? "Fade" : data.Attr("wipeType");
            wipeDuration = data.Float("wipeDuration") <= 0 ? 0.75f : data.Float("wipeDuration");
            beamColor = Calc.HexToColor(data.Attr("beamColor", "FFFFFF"));
            noBeam = data.Bool("noBeam");
            flag = data.Attr("flag");
            noInteractFlag = data.Attr("noInteractFlag");
            forceInactiveFlag = data.Attr("forceInactiveFlag");
            onlyCurrentChapter = data.Bool("onlyCurrentChapter");
            Collider.Width = 32f;
            Collider.Height = 16f;
            SurfaceSoundIndex = 8;
            Add(warpStationSprite = new Sprite(GFX.Game, sprite + "/"));
            warpStationSprite.AddLoop("idle", "idle", 0.08f);
            warpStationSprite.AddLoop("active", "active", 0.08f);
            warpStationSprite.Play("idle");
            Depth = -9000;
        }

        public override void Awake(Scene scene)
        {
            base.Awake(scene);

            warpId = WarpManager.GetWarpId(level, index);
            if (WarpManager.IsUnlocked(warpId))
            {
                Activate();
            }
        }

        public override void Update()
        {
            base.Update();

            if (!activated && HasPlayerOnTop() && (string.IsNullOrEmpty(flag) || level.Session.GetFlag(flag)))
            {
                Activate();
            }

            // This triggers if e.g. we collect a golden after unlocking a warp
            if (activated && !WarpManager.IsUnlocked(warpId))
            {
                Deactivate();
            }

            string prefix = level.Session.Area.LevelSet;
            if (XaphanModule.useMergeChaptersController && XaphanModule.MergeChaptersControllerMode == "Warps" && HasPlayerOnTop() && !XaphanModule.PlayerHasGolden && !level.Frozen && level.Tracker.GetEntity<CountdownDisplay>() == null && level.Tracker.GetEntity<Player>() != null && level.Tracker.GetEntity<Player>().StateMachine.State != Player.StDummy && !XaphanModule.PlayerIsControllingRemoteDrone() && activated && !ShouldSave && !((XaphanModule.MergeChaptersControllerKeepPrologue && level.Session.Area.ID == SaveData.Instance.LevelSetStats.AreaOffset)))
            {
                if (!XaphanModule.ModSaveData.SavedChapter.ContainsKey(prefix))
                {
                    XaphanModule.ModSaveData.SavedChapter.Add(prefix, level.Session.Area.ChapterIndex);
                }
                else
                {
                    if (XaphanModule.ModSaveData.SavedChapter[prefix] != level.Session.Area.ChapterIndex)
                    {
                        XaphanModule.ModSaveData.SavedChapter[prefix] = level.Session.Area.ChapterIndex;
                        ShouldSave = true;
                    }
                }
                if (!XaphanModule.ModSaveData.SavedRoom.ContainsKey(prefix))
                {
                    XaphanModule.ModSaveData.SavedRoom.Add(prefix, level.Session.Level);
                }
                else
                {
                    if (XaphanModule.ModSaveData.SavedRoom[prefix] != level.Session.Level)
                    {
                        XaphanModule.ModSaveData.SavedRoom[prefix] = level.Session.Level;
                        ShouldSave = true;
                    }
                }
                if (!XaphanModule.ModSaveData.SavedSpawn.ContainsKey(prefix))
                {
                    XaphanModule.ModSaveData.SavedSpawn.Add(prefix, level.Session.GetSpawnPoint(Position));
                }
                else
                {
                    if (XaphanModule.ModSaveData.SavedSpawn[prefix] != level.Session.GetSpawnPoint(Position))
                    {
                        XaphanModule.ModSaveData.SavedSpawn[prefix] = level.Session.GetSpawnPoint(Position);
                        ShouldSave = true;
                    }
                }
                if (!XaphanModule.ModSaveData.SavedLightingAlphaAdd.ContainsKey(prefix))
                {
                    XaphanModule.ModSaveData.SavedLightingAlphaAdd.Add(prefix, level.Lighting.Alpha - level.BaseLightingAlpha);
                }
                else
                {
                    if (XaphanModule.ModSaveData.SavedLightingAlphaAdd[prefix] != level.Lighting.Alpha - level.BaseLightingAlpha)
                    {
                        XaphanModule.ModSaveData.SavedLightingAlphaAdd[prefix] = level.Lighting.Alpha - level.BaseLightingAlpha;
                    }
                }
                if (!XaphanModule.ModSaveData.SavedBloomBaseAdd.ContainsKey(prefix))
                {
                    XaphanModule.ModSaveData.SavedBloomBaseAdd.Add(prefix, level.Bloom.Base - AreaData.Get(level).BloomBase);
                }
                else
                {
                    if (XaphanModule.ModSaveData.SavedBloomBaseAdd[prefix] != level.Bloom.Base - AreaData.Get(level).BloomBase)
                    {
                        XaphanModule.ModSaveData.SavedBloomBaseAdd[prefix] = level.Bloom.Base - AreaData.Get(level).BloomBase;
                    }
                }
                if (!XaphanModule.ModSaveData.SavedCoreMode.ContainsKey(prefix))
                {
                    XaphanModule.ModSaveData.SavedCoreMode.Add(prefix, level.Session.CoreMode);
                }
                else
                {
                    if (XaphanModule.ModSaveData.SavedCoreMode[prefix] != level.Session.CoreMode)
                    {
                        XaphanModule.ModSaveData.SavedCoreMode[prefix] = level.Session.CoreMode;
                    }
                }
                if (!XaphanModule.ModSaveData.SavedMusic.ContainsKey(prefix))
                {
                    XaphanModule.ModSaveData.SavedMusic.Add(prefix, level.Session.Audio.Music.Event);
                }
                else
                {
                    if (XaphanModule.ModSaveData.SavedMusic[prefix] != level.Session.Audio.Music.Event)
                    {
                        XaphanModule.ModSaveData.SavedMusic[prefix] = level.Session.Audio.Music.Event;
                    }
                }
                if (!XaphanModule.ModSaveData.SavedAmbience.ContainsKey(prefix))
                {
                    XaphanModule.ModSaveData.SavedAmbience.Add(prefix, level.Session.Audio.Ambience.Event);
                }
                else
                {
                    if (XaphanModule.ModSaveData.SavedAmbience[prefix] != level.Session.Audio.Ambience.Event)
                    {
                        XaphanModule.ModSaveData.SavedAmbience[prefix] = level.Session.Audio.Ambience.Event;
                    }
                }
                if (!XaphanModule.ModSaveData.SavedNoLoadEntities.ContainsKey(prefix))
                {
                    XaphanModule.ModSaveData.SavedNoLoadEntities.Add(prefix, level.Session.DoNotLoad);
                }
                else
                {
                    if (XaphanModule.ModSaveData.SavedNoLoadEntities[prefix] != level.Session.DoNotLoad)
                    {
                        XaphanModule.ModSaveData.SavedNoLoadEntities[prefix] = level.Session.DoNotLoad;
                    }
                }
                if (!XaphanModule.ModSaveData.SavedFromBeginning.ContainsKey(prefix))
                {
                    XaphanModule.ModSaveData.SavedFromBeginning.Add(prefix, level.Session.StartedFromBeginning);
                }
                else
                {
                    if (XaphanModule.ModSaveData.SavedFromBeginning[prefix] != level.Session.StartedFromBeginning)
                    {
                        XaphanModule.ModSaveData.SavedFromBeginning[prefix] = level.Session.StartedFromBeginning;
                    }
                }
                string sessionFlags = "";
                foreach (string flag in level.Session.Flags)
                {
                    if (sessionFlags == "")
                    {
                        sessionFlags += flag;
                    }
                    else
                    {
                        sessionFlags += "," + flag;
                    }
                }
                if (!XaphanModule.ModSaveData.SavedSesionFlags.ContainsKey(prefix))
                {
                    XaphanModule.ModSaveData.SavedSesionFlags.Add(prefix, sessionFlags);
                }
                else
                {
                    if (XaphanModule.ModSaveData.SavedSesionFlags[prefix] != sessionFlags)
                    {
                        XaphanModule.ModSaveData.SavedSesionFlags[prefix] = sessionFlags;
                    }
                }
                if (!XaphanModule.ModSaveData.SavedSessionStrawberries.ContainsKey(prefix))
                {
                    XaphanModule.ModSaveData.SavedSessionStrawberries.Add(prefix, level.Session.Strawberries);
                }
                else
                {
                    if (XaphanModule.ModSaveData.SavedSessionStrawberries[prefix] != level.Session.Strawberries)
                    {
                        XaphanModule.ModSaveData.SavedSessionStrawberries[prefix] = level.Session.Strawberries;
                    }
                }
                if (ShouldSave)
                {
                    level.AutoSave();
                }
            }
            if (!HasPlayerOnTop() && ShouldSave)
            {
                ShouldSave = false;
            }
        }

        private IEnumerator InteractRoutine(Player player)
        {
            if (player != null)
            {
                player.StateMachine.State = 11;
                yield return player.DummyWalkToExact((int)X + 16, false, 1f, true);
                player.Facing = Facings.Right;

                WarpScreen warpScreen;
                level.Add(warpScreen = new WarpScreen(warpId, confirmSfx, wipeType, wipeDuration, onlyCurrentChapter));

                while (warpScreen.Visible)
                {
                    yield return null;
                }
            }
        }

        private void Activate()
        {
            activated = true;
            if (string.IsNullOrEmpty(forceInactiveFlag) || (!string.IsNullOrEmpty(forceInactiveFlag) && !SceneAs<Level>().Session.GetFlag(forceInactiveFlag)))
            {
                warpStationSprite.Play("active");
                if (string.IsNullOrEmpty(noInteractFlag) || (!string.IsNullOrEmpty(noInteractFlag) && !SceneAs<Level>().Session.GetFlag(noInteractFlag)))
                {
                    if (!noBeam)
                    {
                        level.Add(beam = new WarpBeam(Position + new Vector2(16, 0), beamColor));
                    }
                    Add(talker = new TalkComponent(new Rectangle(4, -8, 24, 8), new Vector2(16f, -16f), (player) => Add(new Coroutine(InteractRoutine(player))))
                    {
                        PlayerMustBeFacing = false
                    });
                    WarpManager.ActivateWarp(warpId);
                }
            }
            else
            {
                WarpManager.ActivateWarp(warpId);
            }
        }

        private void Deactivate()
        {
            activated = false;
            warpStationSprite.Play("idle");
            beam?.RemoveSelf();
            talker?.RemoveSelf();
            WarpManager.DeactivateWarp(warpId);
        }

        public class WarpBeam : LightBeam
        {
            public WarpBeam(Vector2 position, Color beamColor)
                : base(new EntityData() { Values = new() }, position)
            {
                LightWidth = 28;
                LightLength = 24;
                Rotation = Rotation = 180f * Calc.DegToRad;
                DynamicData.For(this).Set("color", beamColor);
            }

            public override void Update()
            {
                base.Update();
                DynamicData.For(this).Set("alpha", 1f);
            }
        }
    }
}
