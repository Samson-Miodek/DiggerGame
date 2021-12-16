using System;
using System.Text;
using System.Windows.Forms;

namespace Digger
{
    public static class Game
    {
        private const string mapWithPlayerTerrain = @"
Y    
TYT T
T T T
TT TP";

        private const string mapWithPlayerTerrainSackGold = @"
PTTGTT TS
TST  TSTT
TTTTTTSTT
T TSTS TT
T TTTG ST
TSTSTT TT";

        private const string mapWithPlayerTerrainSackGoldMonster = @"
PTTGTT TST
TST  TSTTM
TTT TTSTTT
T TSTS TTT
T TTTGMSTS
T TMT M TS
TSTSTTMTTT
S TTST  TG
 TGST MTTT
 T  TMTTTT";

        public static ICreature[,] Map;
        public static int Scores;
        public static bool IsOver;

        public static Keys KeyPressed;
        public static int MapWidth => Map.GetLength(0);
        public static int MapHeight => Map.GetLength(1);

        public static void CreateMap()
        {
            /*
             * Активность «Генератор карт»
                Без пояснений.
             */
            var random = new Random();

            var map = new StringBuilder("P");
            var arr = new [] { "T"," "};
            var w = 50;
            var h = 20;

            for (var y = 0; y < h; y++)
            {
                for (var x = 0; x < w-(y == 0 ? 1 : 0); x++)
                {
                    if (random.Next(100) < 3 && y > w*0.1 && x > h*0.1)
                        map.Append("Y");
                    else if (random.Next(100) < 3 && y > w*0.1 && x > h*0.1)
                        map.Append("M");
                    else if (random.Next(100) < 5)
                        map.Append("S");
                    else if (random.Next(100) < 5)
                        map.Append("G");
                    else
                        map.Append(arr[random.Next(arr.Length)]);
                }
                map.Append(Environment.NewLine);
            }
            Map = CreatureMapCreator.CreateMap(map.ToString());
        }
    }
}