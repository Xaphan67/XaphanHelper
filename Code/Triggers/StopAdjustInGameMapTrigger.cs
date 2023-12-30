using Celeste.Mod.Entities;
using Celeste.Mod.XaphanHelper.Controllers;
using Celeste.Mod.XaphanHelper.UI_Elements;
using Microsoft.Xna.Framework;

namespace Celeste.Mod.XaphanHelper.Triggers
{
    [CustomEntity("XaphanHelper/StopAdjustInGameMapTrigger")]
    class StopAdjustInGameMapTrigger : Trigger
    {
        public MapData MapData;

        public StopAdjustInGameMapTrigger(EntityData data, Vector2 offset) : base(data, offset)
        {
        }

        public override void OnEnter(Player player)
        {
            base.OnEnter(player);
            if (!SceneAs<Level>().Session.GetFlag("Ignore_Room_Adjust_" + SceneAs<Level>().Session.Level))
            {
                AreaKey area = SceneAs<Level>().Session.Area;
                MapData = AreaData.Areas[area.ID].Mode[(int)area.Mode].MapData;
                int chapterIndex = area.ChapterIndex;
                InGameMapRoomAdjustController controller = SceneAs<Level>().Entities.FindFirst<InGameMapRoomAdjustController>();
                if (controller != null)
                {
                    foreach (LevelData level in MapData.Levels)
                    {
                        if (level.Name == SceneAs<Level>().Session.Level)
                        {
                            foreach (EntityData entity in level.Entities)
                            {
                                if (entity.Name == "XaphanHelper/InGameMapRoomAdjustController")
                                {
                                    string Prefix = SceneAs<Level>().Session.Area.LevelSet;
                                    SceneAs<Level>().Session.SetFlag("Ignore_Room_Adjust_" + level.Name, true);
                                    XaphanModule.ModSaveData.SavedFlags.Add(Prefix + "_Ignore_Room_Adjust_Ch" + chapterIndex + "_" + level.Name);
                                    break;
                                }
                            }
                        }
                    }
                    MiniMap minimap = SceneAs<Level>().Tracker.GetEntity<MiniMap>();
                    if (minimap != null)
                    {
                        minimap.mapDisplay.GenerateIcons();
                    }
                }
            }
        }
    }
}