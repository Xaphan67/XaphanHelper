using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;

namespace Celeste.Mod.XaphanHelper.Triggers
{
    [CustomEntity("XaphanHelper/RemoveRoomInGameMapTrigger")]
    class RemoveRoomInGameMapTrigger : Trigger
    {
        public string flag;

        public bool inverted;

        public RemoveRoomInGameMapTrigger(EntityData data, Vector2 offset) : base(data, offset)
        {
            flag = data.Attr("flag");
            inverted = data.Bool("inverted");
        }

        public override void OnEnter(Player player)
        {
            base.OnEnter(player);
            string Prefix = SceneAs<Level>().Session.Area.LevelSet;
            int chapterIndex = SceneAs<Level>().Session.Area.ChapterIndex == -1 ? 0 : SceneAs<Level>().Session.Area.ChapterIndex;
            string room = SceneAs<Level>().Session.Level;
            if (inverted ? !XaphanModule.ModSaveData.GlobalFlags.Contains(Prefix + "_Ch" + chapterIndex + "_" + flag) : XaphanModule.ModSaveData.GlobalFlags.Contains(Prefix + "_Ch" + chapterIndex + "_" + flag))
            {
                if (XaphanModule.ModSaveData.VisitedRooms.Contains(Prefix + "_Ch" + chapterIndex + "_" + room))
                {
                    XaphanModule.ModSaveData.VisitedRooms.Remove(Prefix + "_Ch" + chapterIndex + "_" + room);
                }
            }
        }
    }
}