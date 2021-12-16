using System;
using System.Collections.Generic;
using System.Windows.Forms;

namespace Digger
{
    enum DrawingPriority
    {
        Third,
        Second,
        First
    }
    
    public class Terrain : ICreature
    {
        private static string fileName = "Terrain.png";

        public CreatureCommand Act(int x, int y)
        {
            return new CreatureCommand { DeltaX = 0, DeltaY = 0 };
        }

        public bool DeadInConflict(ICreature conflictedObject)
        {
            return conflictedObject is Player;
        }

        public int GetDrawingPriority()
        {
            return (int) DrawingPriority.Second;
        }

        public string GetImageFileName()
        {
            return fileName;
        }
    }

    public class Player : ICreature
    {
        // private static string fileName = "Digger.png";
        public static int dx = 0;
        private static int photoId = 0;

        private static Dictionary<Keys, CreatureCommand> commands = new Dictionary<Keys, CreatureCommand>
        {
            {Keys.Down,  new CreatureCommand { DeltaX = 0, DeltaY = 1}},
            {Keys.Up,    new CreatureCommand { DeltaX = 0, DeltaY = -1}},
            {Keys.Left,  new CreatureCommand { DeltaX = -1, DeltaY = 0}},
            {Keys.Right, new CreatureCommand { DeltaX = 1, DeltaY = 0}}
        };
        
        private static bool CanMoveTo(int x, int y)
        {
            return (0 <= x && x < Game.MapWidth) && (0 <= y && y < Game.MapHeight);
        }

        public CreatureCommand Act(int x, int y)
        {
            //Добавление бомбы
            if (Game.KeyPressed == Keys.W)//не работает
            {
                if(Game.Map[x, y-1] is null)
                    Game.Map[x, y-1] = new Bomb();
            }
            if (Game.KeyPressed == Keys.S)
            {
                if(Game.Map[x, y+1] is null)
                    Game.Map[x, y+1] = new Bomb();
            }
            if (Game.KeyPressed == Keys.A)//не работает
            {
                if(Game.Map[x-1, y] is null)
                    Game.Map[x-1, y] = new Bomb();
            }
            if (Game.KeyPressed == Keys.D)
            {
                if(Game.Map[x+1, y] is null)
                    Game.Map[x+1, y] = new Bomb();
            }
            //========================================
            if (commands.ContainsKey(Game.KeyPressed))
            {
                var command = commands[Game.KeyPressed];

                x += command.DeltaX;
                y += command.DeltaY;

                if (!CanMoveTo(x, y) || Game.Map[x, y] is Sack)
                {
                    dx = 0;
                    return new CreatureCommand();
                }
                

                dx = command.DeltaX;
                photoId = (photoId + 1) % 8;
                return command;
            }
            dx = 0;
            return new CreatureCommand();
        }

        public bool DeadInConflict(ICreature conflictedObject)
        {
            return conflictedObject is Sack || conflictedObject is Monster;
        }

        public int GetDrawingPriority()
        {
            return (int) DrawingPriority.Third;
        }

        public string GetImageFileName()
        {
            return string.Format("{0}.png",photoId);
        }
    }
/*
 * Активность «Бомба»
По нажатию кнопки персонаж кладет бомбу рядом с собой. Бомба — это неподвижный монстр
 */
    public class Bomb : Monster
    {
        public override CreatureCommand Act(int x, int y)
        {
            return new CreatureCommand();
        }
        public override string GetImageFileName()
        {
            return "Bomb.png";
        }
    }
    /*
     *
     *Сделать еще один класс Монстра — который бы выбирал случайное направление,
     * в котором можно идти, шел бы по нему до тех пор, пока не упрется в стену,
     * после чего выбирал бы случайное направление заново.
     * 
     */
    public class SpecialMonster : Monster
    {
        private int commandId = 0;
        private static Dictionary<int, CreatureCommand> commands = new Dictionary<int, CreatureCommand>
        {
            {0,  new CreatureCommand { DeltaX = 0, DeltaY = 1}},
            {1,    new CreatureCommand { DeltaX = 0, DeltaY = -1}},
            {2,  new CreatureCommand { DeltaX = -1, DeltaY = 0}},
            {3, new CreatureCommand { DeltaX = 1, DeltaY = 0}}
        };
        
        private static bool CanMoveToX(int x)
        {
            return (0 <= x && x < Game.MapWidth);
        }
        private static bool CanMoveToY(int y)
        {
            return (0 <= y && y < Game.MapHeight);
        }
        
        public override CreatureCommand Act(int x, int y)
        {
            var move = commands[commandId];
            var posX = x + move.DeltaX;
            var posY = y + move.DeltaY;

            if (CanMoveToX(posX) && CanMoveToY(posY))
            {
                var cell = Game.Map[posX, posY];
                if(cell is null || cell is Player || cell is Gold)
                    return move;
            }
            
            var newCommandId = commandId = new Random().Next(commands.Count);

            if (newCommandId == commandId)
            {
                commandId = (commandId + 1) % commands.Count;
            }
            
            return new CreatureCommand();
        }
        public override string GetImageFileName()
        {
            return "SpecMonster.png";
        }
    }
    
    public class Monster : ICreature
    {
        private static string fileName = "Monster.png";
        private static int playerPosX;
        private static int playerPosY;
        
        private static bool PlayerIsDead()
        {
            for (var x = 0; x < Game.MapWidth; x++)
                for (var y = 0; y < Game.MapHeight; y++)
                    if (Game.Map[x, y] is Player)
                    {
                        playerPosX = x;
                        playerPosY = y;
                        return false;
                    }
            return true;
        }
        
        public virtual CreatureCommand Act(int x, int y)
        {
            if(PlayerIsDead())
                return new CreatureCommand();
            
            var dx = 0;
            var dy = 0;

            if (playerPosX < x)
                dx = -1;
            else if (playerPosX > x)
                dx = 1;
            else if (playerPosY < y)
                dy = -1;
            else if (playerPosY > y)
                dy = 1;
            
            var cellMap = Game.Map[x + dx, y + dy];
            
            if (cellMap is null || cellMap is Player || cellMap is Gold)
                return new CreatureCommand() { DeltaX = dx, DeltaY = dy };

            return new CreatureCommand();
        }

        public bool DeadInConflict(ICreature conflictedObject)
        {
            return conflictedObject is Sack || conflictedObject is Monster;
        }

        public int GetDrawingPriority()
        {
            return (int) DrawingPriority.First;
        }
        
        public virtual string GetImageFileName()
        {
            return fileName;
        }
    }

    public class Sack : ICreature
    {
        private string fileName = "Sack.png";
        private string fallingName1 = "FallingSack1.png";
        private string fallingName2 = "FallingSack2.png";
        private string fallingName3 = "FallingSack3.png";
        private int fallCellsCount = 0;
		
        private ICreature GetNextCellMapOrThis(int x, int y)
        {
            if(y+1 < Game.MapHeight)
                return Game.Map[x, y+1];
            return this;
        }
		
        public CreatureCommand Act(int x, int y)
        {
            var cellMap = GetNextCellMapOrThis(x, y);

            if (cellMap is null || (cellMap is Player || cellMap is Monster) && fallCellsCount >= 1)
            {
                fallCellsCount++;

                var percent = ((double) fallCellsCount) / (Game.MapHeight)*100;

                if (percent > 1)
                    fileName = fallingName1;
                if (percent > 30)
                    fileName = fallingName2;
                if (percent > 50)
                    fileName = fallingName3;
                
                return new CreatureCommand {DeltaX = 0, DeltaY = 1};
            }
            if (fallCellsCount > 1)
            {
                fallCellsCount = 0;
                fileName = "BoomSack.png";
                return new CreatureCommand { DeltaX = 0, DeltaY = 0, TransformTo = new Gold()};
            }
            fileName = "Sack.png";
            fallCellsCount = 0;
            return new CreatureCommand { DeltaX = 0, DeltaY = 0};
        }
        
        public bool DeadInConflict(ICreature conflictedObject)
        {
            return false;
        }

        public int GetDrawingPriority()
        {
            return (int) DrawingPriority.Second;
        }   
        
        public string GetImageFileName()
        {
            return fileName;
        }
    }
    public class Gold : ICreature
    {
        private static string fileName = "Gold.png";
        private const int Coins = 10;
        
        public CreatureCommand Act(int x, int y)
        {
            return new CreatureCommand();
        }
        
        public bool DeadInConflict(ICreature conflictedObject)
        {
            if (conflictedObject is Player)
                Game.Scores += Coins;
            return true;
        }
        
        public int GetDrawingPriority()
        {
            return (int) DrawingPriority.First;
        }
        
        public string GetImageFileName()
        {
            return fileName;
        }
    }
}