using Microsoft.Xna.Framework;

namespace Celeste.Mod.XaphanHelper.Data
{
    public class InGameMapMarkersData
    {
        public int ChapterIndex;

        public string Room;

        public Vector2 Position;

        public string Type;

        public InGameMapMarkersData(int chapterIndex, string room, Vector2 position, string type)
        {
            ChapterIndex = chapterIndex;
            Room = room;
            Type = type;
            Position = position;
        }
    }
}
