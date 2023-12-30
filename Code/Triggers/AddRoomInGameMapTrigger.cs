using Celeste.Mod.Entities;
using Celeste.Mod.XaphanHelper.UI_Elements;
using Microsoft.Xna.Framework;

namespace Celeste.Mod.XaphanHelper.Triggers
{
    [CustomEntity("XaphanHelper/AddRoomInGameMapTrigger")]
    class AddRoomInGameMapTrigger : Trigger
    {
        public string Rooms;

        public AddRoomInGameMapTrigger(EntityData data, Vector2 offset) : base(data, offset)
        {
            Rooms = data.Attr("rooms");
        }

        public override void OnEnter(Player player)
        {
            base.OnEnter(player);
            string Prefix = SceneAs<Level>().Session.Area.LevelSet;
            int chapterIndex = SceneAs<Level>().Session.Area.ChapterIndex == -1 ? 0 : SceneAs<Level>().Session.Area.ChapterIndex;
            string[] rooms = Rooms.Split(',');
            foreach (string room in rooms)
            {
                if (!XaphanModule.ModSaveData.ExtraUnexploredRooms.Contains(Prefix + "/Ch" + chapterIndex + "/" + room))
                {
                    XaphanModule.ModSaveData.ExtraUnexploredRooms.Add(Prefix + "/Ch" + chapterIndex + "/" + room);
                }
            }
            MapDisplay mapDisplay = SceneAs<Level>().Tracker.GetEntity<MapDisplay>();
            if (mapDisplay != null && mapDisplay.ExtraUnexploredRooms.Count > mapDisplay.ExtraUnexploredRoomsCount)
            {
                mapDisplay.UpdateExtraRooms();
            }
        }
    }
}