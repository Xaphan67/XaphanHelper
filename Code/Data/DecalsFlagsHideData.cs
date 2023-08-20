namespace Celeste.Mod.XaphanHelper.Data
{
    class DecalsFlagsHideData
    {
        public string Decal;

        public string Flags;

        public string Room;

        public bool Inverted;

        public DecalsFlagsHideData(string decal, string flags, string room, bool inverted)
        {
            Decal = decal;
            Flags = flags;
            Room = room;
            Inverted = inverted;
        }
    }
}
