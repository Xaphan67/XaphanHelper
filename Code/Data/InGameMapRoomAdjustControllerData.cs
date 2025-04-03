namespace Celeste.Mod.XaphanHelper.Data
{
    public class InGameMapRoomAdjustControllerData
    {
        public string Room;

        public int PositionX;

        public int PositionY;

        public int SizeX;

        public int SizeY;

        public string HiddenTiles;

        public bool RemoveEntrance0;

        public bool RemoveEntrance1;

        public bool RemoveEntrance2;

        public bool RemoveEntrance3;

        public bool RemoveEntrance4;

        public bool RemoveEntrance5;

        public bool RemoveEntrance6;

        public bool RemoveEntrance7;

        public bool RemoveEntrance8;

        public bool RemoveEntrance9;

        public bool IgonreIcons;

        public string Display;

        public InGameMapRoomAdjustControllerData(string room, int positionX, int positionY, int sizeX, int sizeY, string hiddenTiles, bool removeEntrance0, bool removeEntrance1, bool removeEntrance2, bool removeEntrance3, bool removeEntrance4, bool removeEntrance5, bool removeEntrance6, bool removeEntrance7, bool removeEntrance8, bool removeEntrance9, bool ignoreIcons, string display)
        {
            Room = room;
            PositionX = positionX;
            PositionY = positionY;
            SizeX = sizeX;
            SizeY = sizeY;
            HiddenTiles = hiddenTiles;
            RemoveEntrance0 = removeEntrance0;
            RemoveEntrance1 = removeEntrance1;
            RemoveEntrance2 = removeEntrance2;
            RemoveEntrance3 = removeEntrance3;
            RemoveEntrance4 = removeEntrance4;
            RemoveEntrance5 = removeEntrance5;
            RemoveEntrance6 = removeEntrance6;
            RemoveEntrance7 = removeEntrance7;
            RemoveEntrance8 = removeEntrance8;
            RemoveEntrance9 = removeEntrance9;
            IgonreIcons = ignoreIcons;
            Display = display;
        }

        public bool GetRemoveEntranceField(int entrance)
        {
            if (entrance == 0)
            {
                return RemoveEntrance0;
            }
            if (entrance == 1)
            {
                return RemoveEntrance1;
            }
            if (entrance == 2)
            {
                return RemoveEntrance2;
            }
            if (entrance == 3)
            {
                return RemoveEntrance3;
            }
            if (entrance == 4)
            {
                return RemoveEntrance4;
            }
            if (entrance == 5)
            {
                return RemoveEntrance5;
            }
            if (entrance == 6)
            {
                return RemoveEntrance6;
            }
            if (entrance == 7)
            {
                return RemoveEntrance7;
            }
            if (entrance == 8)
            {
                return RemoveEntrance8;
            }
            if (entrance == 9)
            {
                return RemoveEntrance9;
            }
            return false;
        }
    }
}
