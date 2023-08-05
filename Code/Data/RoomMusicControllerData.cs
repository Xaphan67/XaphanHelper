namespace Celeste.Mod.XaphanHelper.Data
{
    public class RoomMusicControllerData
    {
        public string Rooms;

        public string ExcludeRooms;

        public string FlagInnactive;

        public string FlagA;

        public string FlagB;

        public string FlagC;

        public string FlagD;

        public string MusicIfFlagA;

        public string MusicIfFlagB;

        public string MusicIfFlagC;

        public string MusicIfFlagD;

        public string DefaultMusic;

        public RoomMusicControllerData(string rooms, string excludeRooms, string flagInnactive, string flagA, string flagB, string flagC, string flagD, string musicIfFlagA, string musicIfFlagB, string musicIfFlagC, string musicIfFlagD, string defaultMusic)
        {
            Rooms = rooms;
            ExcludeRooms = excludeRooms;
            FlagInnactive = flagInnactive;
            FlagA = flagA;
            FlagB = flagB;
            FlagC = flagC;
            FlagD = flagD;
            MusicIfFlagA = musicIfFlagA;
            MusicIfFlagB = musicIfFlagB;
            MusicIfFlagC = musicIfFlagC;
            MusicIfFlagD = musicIfFlagD;
            DefaultMusic = defaultMusic;
        }
    }
}
