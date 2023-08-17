using Monocle;

namespace Celeste.Mod.XaphanHelper.Hooks
{
    internal class TilesetsSwap
    {
        public static void Load()
        {
            On.Celeste.Autotiler.Generate += onAutotilerGenerate;
        }

        public static void Unload()
        {
            On.Celeste.Autotiler.Generate -= onAutotilerGenerate;
        }

        private static Autotiler.Generated onAutotilerGenerate(On.Celeste.Autotiler.orig_Generate orig, Autotiler self, VirtualMap<char> mapData, int startX, int startY, int tilesX, int tilesY, bool forceSolid, char forceID, Autotiler.Behaviour behaviour)
        {
            if (SaveData.Instance.GetLevelSetStats().Name == "Xaphan/0" && mapData != null && !forceSolid) // Swap tilesets only in SoCM
            {
                if (self == GFX.FGAutotiler) // Foreground Tiles
                {
                    if (XaphanModule.ModSaveData.GlobalFlags.Contains("Xaphan/0_Ch4_Escape_Complete" + (XaphanModule.PlayerHasGolden ? "_GoldenStrawberry" : ""))) // Change Tilesets after escape has occured
                    {
                        for (int x = 0; x < mapData.Columns; x++)
                        {
                            for (int y = 0; y < mapData.Rows; y++)
                            {
                                if (mapData[x, y] != '0')
                                {
                                    if (mapData[x, y] == 'v')
                                    {
                                        mapData[x, y] = 'V';
                                    }
                                    else if (mapData[x, y] == 'w')
                                    {
                                        mapData[x, y] = 'W';
                                    }
                                    else if (mapData[x, y] == 'x')
                                    {
                                        mapData[x, y] = 'X';
                                    }
                                    else if (mapData[x, y] == 'y')
                                    {
                                        mapData[x, y] = 'Y';
                                    }
                                }
                            }
                        }
                    }
                }
                if (self == GFX.BGAutotiler) // Background Tiles
                {
                    if (XaphanModule.ModSaveData.GlobalFlags.Contains("Xaphan/0_Ch4_Escape_Complete" + (XaphanModule.PlayerHasGolden ? "_GoldenStrawberry" : ""))) // Change Tilesets after escape has occured
                    {
                        for (int x = 0; x < mapData.Columns; x++)
                        {
                            for (int y = 0; y < mapData.Rows; y++)
                            {
                                if (mapData[x, y] != '0')
                                {
                                    if (mapData[x, y] == 'v')
                                    {
                                        mapData[x, y] = 'V';
                                    }
                                    else if (mapData[x, y] == 'w')
                                    {
                                        mapData[x, y] = 'W';
                                    }
                                    else if (mapData[x, y] == 'x')
                                    {
                                        mapData[x, y] = 'X';
                                    }
                                    else if (mapData[x, y] == 'y')
                                    {
                                        mapData[x, y] = 'Y';
                                    }
                                }
                            }
                        }
                    }
                }
            }
            return orig(self, mapData, startX, startY, tilesX, tilesY, forceSolid, forceID, behaviour);
        }
    }
}
