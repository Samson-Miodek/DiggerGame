using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace Digger
{
    public static class CreatureMapCreator
    {
        private static readonly Dictionary<string, Func<ICreature>> factory = new Dictionary<string, Func<ICreature>>();
        /*
         * Активность «Генератор карт»
            Без пояснений.
         */
        public static ICreature[,] CreateRandomMap(int w, int h)
        {
            var random = new Random();

            var map = new StringBuilder("P");
            var cells = new [] { "T"," "};

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
                        map.Append(cells[random.Next(cells.Length)]);
                }
                map.Append(Environment.NewLine);
            }

            return CreateMap(map.ToString());
        }
        public static ICreature[,] CreateMap(string map, string separator = "\r\n")
        {
            var rows = map.Split(new[] {separator}, StringSplitOptions.RemoveEmptyEntries);
            if (rows.Select(z => z.Length).Distinct().Count() != 1)
                throw new Exception($"Wrong test map '{map}'");
            var result = new ICreature[rows[0].Length, rows.Length];
            for (var x = 0; x < rows[0].Length; x++)
            for (var y = 0; y < rows.Length; y++)
                result[x, y] = CreateCreatureBySymbol(rows[y][x]);
            return result;
        }

        private static ICreature CreateCreatureByTypeName(string name)
        {
            // Это использование механизма рефлексии. 
            // Ему посвящена одна из последних лекций второй части курса Основы программирования
            // В обычном коде можно было обойтись без нее, но нам нужно было написать такой код,
            // который работал бы, даже если вы ещё не создали класс Monster или Gold. 
            // Просто написать new Gold() мы не могли, потому что это не скомпилировалось бы до создания класса Gold.
            if (!factory.ContainsKey(name))
            {
                var type = Assembly
                    .GetExecutingAssembly()
                    .GetTypes()
                    .FirstOrDefault(z => z.Name == name);
                if (type == null)
                    throw new Exception($"Can't find type '{name}'");
                factory[name] = () => (ICreature) Activator.CreateInstance(type);
            }

            return factory[name]();
        }


        private static ICreature CreateCreatureBySymbol(char c)
        {
            switch (c)
            {
                case 'P':
                    return CreateCreatureByTypeName("Player");
                case 'T':
                    return CreateCreatureByTypeName("Terrain");
                case 'G':
                    return CreateCreatureByTypeName("Gold");
                case 'S':
                    return CreateCreatureByTypeName("Sack");
                case 'M':
                    return CreateCreatureByTypeName("Monster");
                case 'Y':
                    return CreateCreatureByTypeName("SpecialMonster");
                case 'B':
                    return CreateCreatureByTypeName("Bomb");
                case ' ':
                    return null;
                default:
                    throw new Exception($"wrong character for ICreature {c}");
            }
        }
    }
}
