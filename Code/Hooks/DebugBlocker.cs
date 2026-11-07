using System;
using System.Collections.Generic;
using Monocle;
using MonoMod.Utils;

namespace Celeste.Mod.XaphanHelper.Hooks
{
    internal class DebugBlocker
    {
        public static void Load()
        {
            On.Celeste.Commands.CmdE += onCommandsE;
            On.Celeste.Editor.MapEditor.ctor += onEditorMapEditorCtor;
            On.Celeste.Editor.MapEditor.Update += onEditorMapEditorUpdate;
            On.Celeste.Editor.MapEditor.Render += onEditorMapEditorRender;
            On.Monocle.EntityList.DebugRender += onMonocleEntityListDebugRender;
            On.Celeste.Level.Update += onLevelUpdate;
        }

        public static void Unload()
        {
            On.Celeste.Commands.CmdE -= onCommandsE;
            On.Celeste.Editor.MapEditor.ctor -= onEditorMapEditorCtor;
            On.Celeste.Editor.MapEditor.Update -= onEditorMapEditorUpdate;
            On.Celeste.Editor.MapEditor.Render -= onEditorMapEditorRender;
            On.Monocle.EntityList.DebugRender -= onMonocleEntityListDebugRender;
            On.Celeste.Level.Update -= onLevelUpdate;
        }

        private static void onCommandsE(On.Celeste.Commands.orig_CmdE orig, int index, int mode)
        {
            AreaKey area = new(index, (AreaMode)mode);
            if (area.LevelSet != "Xaphan/0" || (area.LevelSet == "Xaphan/0" && XaphanModule.SoCMVersion >= new Version(3, 0, 0) && XaphanModule.ModSettings.AllowDebug))
            {
                orig(index, mode);
            }
        }

        private static void onEditorMapEditorCtor(On.Celeste.Editor.MapEditor.orig_ctor orig, Editor.MapEditor self, AreaKey area, bool reloadMapData)
        {
            orig(self, area, reloadMapData);
            DynData<Editor.MapEditor> mapEditorData = new(self);
            if (area.LevelSet == "Xaphan/0" && XaphanModule.SoCMVersion >= new Version(3, 0, 0) && !XaphanModule.ModSettings.AllowDebug)
            {
                List<Editor.LevelTemplate> levels = mapEditorData.Get<List<Editor.LevelTemplate>>("levels");
                MapData mapdata = mapEditorData.Get<MapData>("mapData");
                List<Editor.LevelTemplate> levelsToHide = new();
                foreach (Editor.LevelTemplate template in levels)
                {
                    foreach (LevelData levelData in mapdata.Levels)
                    {
                        levelsToHide.Add(template);
                    }
                }
                foreach (Editor.LevelTemplate template in levelsToHide)
                {
                    levels.Remove(template);
                }
            }
        }

        private static void onEditorMapEditorUpdate(On.Celeste.Editor.MapEditor.orig_Update orig, Editor.MapEditor self)
        {
            DynData<Editor.MapEditor> mapEditorData = new(self);
            AreaKey areaKey = mapEditorData.Get<AreaKey>("area");
            Session session = mapEditorData.Get<Session>("CurrentSession");
            if (areaKey.LevelSet != "Xaphan/0" || (areaKey.LevelSet == "Xaphan/0" && XaphanModule.SoCMVersion >= new Version(3, 0, 0) && XaphanModule.ModSettings.AllowDebug))
            {
                orig(self);
            }
            else
            {
                Engine.Scene = new LevelLoader(session);
            }
        }

        private static void onEditorMapEditorRender(On.Celeste.Editor.MapEditor.orig_Render orig, Editor.MapEditor self)
        {
            DynData<Editor.MapEditor> mapEditorData = new(self);
            AreaKey areaKey = mapEditorData.Get<AreaKey>("area");
            if (areaKey.LevelSet != "Xaphan/0" || (areaKey.LevelSet == "Xaphan/0" && XaphanModule.SoCMVersion >= new Version(3, 0, 0) && XaphanModule.ModSettings.AllowDebug))
            {
                orig(self);
            }
        }

        private static void onMonocleEntityListDebugRender(On.Monocle.EntityList.orig_DebugRender orig, EntityList self, Camera camera)
        {
            if (SaveData.Instance.LevelSetStats.Name != "Xaphan/0" || (SaveData.Instance.LevelSetStats.Name == "Xaphan/0" && XaphanModule.SoCMVersion >= new Version(3, 0, 0) && XaphanModule.ModSettings.AllowDebug)) // Conditions that must be true for hitboxes to show
            {
                orig(self, camera);
            }
        }

        private static void onLevelUpdate(On.Celeste.Level.orig_Update orig, Level self)
        {
            orig(self);
            if (self.Session.Area.LevelSet == "Xaphan/0" && XaphanModule.SoCMVersion >= new Version(3, 0, 0) && !XaphanModule.ModSettings.AllowDebug)
            {
                Engine.Commands.Enabled = false;
            }
        }
    }
}
