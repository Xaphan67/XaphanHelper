using System;
using System.Collections.Generic;
using System.Net.Http;
using Celeste.Mod.XaphanHelper.Data;

namespace Celeste.Mod.XaphanHelper
{
    static class PlayerStat
    {
        static readonly HttpClient client = new();

        public static string PlayersClearsList;

        public static string PlayersFullClearsList;

        public static string PlayersGoldenClearsList;

        public static string PlayersGoldenFullClearsList;

        public static async void GetPlayersList()
        {
            try
            {
                PlayersClearsList = await client.GetStringAsync("http://cf-formation.fr/clears.txt");
                PlayersFullClearsList = await client.GetStringAsync("http://cf-formation.fr/fullClears.txt");
                PlayersGoldenClearsList = await client.GetStringAsync("http://cf-formation.fr/goldenClears.txt");
                PlayersGoldenFullClearsList = await client.GetStringAsync("http://cf-formation.fr/goldenFullCLears.txt");
            }
            catch (HttpRequestException e)
            {
                Logger.Log(LogLevel.Info, "XaphanHelper/PlayerStats", "Could not get data for players stats. Reason: " + e.Message);
            }
        }

        public static List<PlayerStatData> GeneratePlayersClearsList()
        {
            List<PlayerStatData> list = new();

            if (!string.IsNullOrEmpty(PlayersClearsList))
            {
                string[] playersData = PlayersClearsList.Split(
                    new string[] { Environment.NewLine },
                    StringSplitOptions.None
                    );
                foreach (string player in playersData)
                {
                    list.Add(new PlayerStatData(
                        name: player.Split(',')[0],
                        medals: int.Parse(player.Split(',')[1]),
                        blueCrystalHearts: int.Parse(player.Split(',')[2]),
                        yellowCrystalHearts: int.Parse(player.Split(',')[3]),
                        strawberries: int.Parse(player.Split(',')[4]),
                        deaths: int.Parse(player.Split(',')[5]),
                        time: player.Split(',')[6],
                        version: player.Split(',')[7]
                    ));
                }
            }

            return list;
        }

        public static List<PlayerStatData> GeneratePlayersFullClearsList()
        {
            List<PlayerStatData> list = new();

            if (!string.IsNullOrEmpty(PlayersFullClearsList))
            {
                string[] playersData = PlayersFullClearsList.Split(
                    new string[] { Environment.NewLine },
                    StringSplitOptions.None
                    );
                foreach (string player in playersData)
                {
                    list.Add(new PlayerStatData(
                        name: player.Split(',')[0],
                        medals: int.Parse(player.Split(',')[1]),
                        blueCrystalHearts: int.Parse(player.Split(',')[2]),
                        yellowCrystalHearts: int.Parse(player.Split(',')[3]),
                        strawberries: int.Parse(player.Split(',')[4]),
                        deaths: int.Parse(player.Split(',')[5]),
                        time: player.Split(',')[6],
                        version: player.Split(',')[7]
                    ));
                }
            }

            return list;
        }

        public static List<PlayerStatData> GeneratePlayersGoldenClearsList()
        {
            List<PlayerStatData> list = new();

            if (!string.IsNullOrEmpty(PlayersGoldenClearsList))
            {
                string[] playersData = PlayersGoldenClearsList.Split(
                    new string[] { Environment.NewLine },
                    StringSplitOptions.None
                    );
                foreach (string player in playersData)
                {
                    list.Add(new PlayerStatData(
                        name: player.Split(',')[0],
                        medals: int.Parse(player.Split(',')[1]),
                        blueCrystalHearts: int.Parse(player.Split(',')[2]),
                        yellowCrystalHearts: int.Parse(player.Split(',')[3]),
                        strawberries: int.Parse(player.Split(',')[4]),
                        deaths: int.Parse(player.Split(',')[5]),
                        time: player.Split(',')[6],
                        version: player.Split(',')[7]
                    ));
                }
            }

            return list;
        }

        public static List<PlayerStatData> GeneratePlayersGoldenFullClearsList()
        {
            List<PlayerStatData> list = new();

            if (!string.IsNullOrEmpty(PlayersGoldenFullClearsList))
            {
                string[] playersData = PlayersGoldenFullClearsList.Split(
                    new string[] { Environment.NewLine },
                    StringSplitOptions.None
                    );
                foreach (string player in playersData)
                {
                    list.Add(new PlayerStatData(
                        name: player.Split(',')[0],
                        medals: int.Parse(player.Split(',')[1]),
                        blueCrystalHearts: int.Parse(player.Split(',')[2]),
                        yellowCrystalHearts: int.Parse(player.Split(',')[3]),
                        strawberries: int.Parse(player.Split(',')[4]),
                        deaths: int.Parse(player.Split(',')[5]),
                        time: player.Split(',')[6],
                        version: player.Split(',')[7]
                    ));
                }
            }

            return list;
        }
    }
}
