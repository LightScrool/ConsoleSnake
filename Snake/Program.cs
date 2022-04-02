using System;
using System.Threading;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Snake
{
    class Data
    {
        public const int WINDOW_WIDTH = 70;
        public const int WINDOW_HEIGHT = 30;
        public const int SPEED = 9;
        public const int START_SNAKE_LENGTH = 8;
        public const char SNAKE_CHAR = '#';
        public const char APPLE_CHAR = '@';
        public const int APPLE_NUMBER = 4;
        public static Snake snake;
        public static List<Block> appleList;
        public static int score;
        public static bool snakeIsEaten;
    }

    class Program
    {
        static void Main()
        {
            Console.SetWindowSize(Data.WINDOW_WIDTH, Data.WINDOW_HEIGHT);
            Console.CursorVisible = false;

            Console.WriteLine("Добро пожаловать в класическую игру \"Змейка\".\n" +
                "Управление осуществляется стрелочками или клавишами WASD.\n" +
                "Вы можете закончить игру в любой момент, нажав клавишу Escape\n" +
                "Чтобы начать нажмите любую клавишу");
            Console.ReadKey(true);

            while (true)
            {
                StartGame();
                GameOver();
            }
        }

        static void StartGame()
        {
            // Обнуление
            Data.score = 0;
            Data.snakeIsEaten = false;
            Console.Clear();
            Data.snake = new Snake();
            Data.appleList = new List<Block>();
            GenerateApples();

            // Все действия змейки происходят во второстепенном потоке,
            // в основном потоке реализовано взаимодействие пользователя с игрой
            ThreadStart startGameThread = new ThreadStart(Moving);
            Thread gameThread = new Thread(startGameThread);
            gameThread.Start();

            while (!Data.snakeIsEaten)
            {
                switch (Console.ReadKey(true).Key)
                {
                    case ConsoleKey.RightArrow:
                    case ConsoleKey.D:
                        if (Data.snake.Direction != 'l')
                            Data.snake.Direction = 'r';
                        break;
                    case ConsoleKey.LeftArrow:
                    case ConsoleKey.A:
                        if (Data.snake.Direction != 'r')
                            Data.snake.Direction = 'l';
                        break;
                    case ConsoleKey.UpArrow:
                    case ConsoleKey.W:
                        if (Data.snake.Direction != 'd')
                            Data.snake.Direction = 'u';
                        break;
                    case ConsoleKey.DownArrow:
                    case ConsoleKey.S:
                        if (Data.snake.Direction != 'u')
                            Data.snake.Direction = 'd';
                        break;
                    case ConsoleKey.Escape:
                        Data.snakeIsEaten = true;
                        break;
                }
            }
        }

        static void GameOver()
        {
            Console.Clear();
            Console.WriteLine($"Игра окончена!\n" +
                $"Ваш счёт: {Data.score}\n");
            Thread.Sleep(1000);
            Console.WriteLine($"---------------------------------------------\n" +
                $"Нажмите любую клавишу, чтобы сыграть ещё раз.");
            Console.ReadKey(true);
        }

        // Главная процедура второстепенного потока
        static void Moving()
        {
            while (!Data.snakeIsEaten)
            {
                Data.snake.Move();
                Thread.Sleep((int)(1000 / Data.SPEED));
            }
        }

        // Случайным образом располагает яблоки в блоках, в
        // которых нет других яблок или змейки
        public static void GenerateApples()
        {
            Random rand = new Random();
            int n = Data.APPLE_NUMBER - Data.appleList.Count;
            for (int i = 0; i < n; i++)
            {
                int x = rand.Next(Data.WINDOW_WIDTH);
                int y = rand.Next(Data.WINDOW_HEIGHT);

                if (Data.snake.Exists(block => block.X == x && block.Y == y) ||
                    Data.appleList.Exists(block => block.X == x && block.Y == y))
                    continue;

                Data.appleList.Add(new Block(x, y, Data.APPLE_CHAR));
            }
        }
    }

    /// <summary>
    /// Консольный аналог пикселя. Объекты хранят информацию о месте положения и о том, какой символ используется.
    /// При создании объекта блок выводится на консоль.
    /// </summary>
    class Block
    {
        public int X { get; set; }
        public int Y { get; set; }
        public char Char { get; set; }

        public Block(int _x, int _y, char _char)
        {
            X = (Data.WINDOW_WIDTH + _x) % Data.WINDOW_WIDTH;
            Y = (Data.WINDOW_HEIGHT + _y) % Data.WINDOW_HEIGHT;
            Char = _char;
            Console.SetCursorPosition(X, Y);
            Console.Write(Char);
        }

        /// <summary>
        /// Переащает блок относительно точки, в которой он находится
        /// </summary>
        /// <param name="xChange">Смещение по x</param>
        /// <param name="yChange">Смещение по y</param>
        public void Move(int xChange, int yChange)
        {
            int newX = (Data.WINDOW_WIDTH + X + xChange) % Data.WINDOW_WIDTH;
            int newY = (Data.WINDOW_HEIGHT + Y + yChange) % Data.WINDOW_HEIGHT;

            Console.SetCursorPosition(X, Y);
            Console.Write(' ');
            Console.SetCursorPosition(newX, newY);
            Console.Write(Char);

            X = newX;
            Y = newY;
        }

        /// <summary>
        /// Перемещает блок в заданную точку
        /// </summary>
        /// <param name="_x">Новая координата x</param>
        /// <param name="_y">Новая координата y</param>
        public void MoveTo(int _x, int _y)
        {

            Console.SetCursorPosition(X, Y);
            Console.Write(' ');
            Console.SetCursorPosition(_x, _y);
            Console.Write(Char);

            X = _x;
            Y = _y;
        }
    }

    class Snake
    {
        private List<Block> blockList = new List<Block>();

        public Block this[int index]
        {
            get => blockList[index];
        }

        public int Length { get => blockList.Count; }

        public char Direction { get; set; }

        /// <summary>
        /// Змейка создаётся из блоков в левом верхнем углу консоли, взгляд вправо, длина берётся из константы класса Data
        /// </summary>
        public Snake()
        {
            if (Data.START_SNAKE_LENGTH > Data.WINDOW_WIDTH)
                throw new ArgumentOutOfRangeException();

            Direction = 'r';

            int y = 0;
            int x = Data.START_SNAKE_LENGTH - 1;
            blockList.Add(new Block(x, y, Data.SNAKE_CHAR));

            for (x--; x >= 0; x--)
            {
                blockList.Add(new Block(x, y, Data.SNAKE_CHAR));
            }
        }

        /// <summary>
        /// Движение змейки в сторону, указанную в поле Direction
        /// </summary>
        public void Move()
        {
            int newPosX = blockList[0].X;
            int newPosY = blockList[0].Y;

            switch (Direction)
            {
                case 'l':
                    blockList[0].Move(-1, 0);
                    break;
                case 'r':
                    blockList[0].Move(1, 0);
                    break;
                case 'u':
                    blockList[0].Move(0, -1);
                    break;
                case 'd':
                    blockList[0].Move(0, 1);
                    break;
                default:
                    return;
            }

            // Каждый блок с индексом i >= 1 встаёт на место блока с индексом i-1
            for (int i = 1; i < blockList.Count; i++)
            {
                int posX = blockList[i].X;
                int posY = blockList[i].Y;
                blockList[i].MoveTo(newPosX, newPosY);

                newPosX = posX;
                newPosY = posY;

            }

            SelfEatenCheck();
            EatAppleCheck();
        }

        public void SelfEatenCheck()
        {
            int headX = blockList[0].X;
            int headY = blockList[0].Y;

            int asHeadCoord = blockList.FindLastIndex(block => block.X == headX && block.Y == headY);
            Data.snakeIsEaten = !(asHeadCoord == 0);
        }

        public void EatAppleCheck()
        {
            int maxI = blockList.Count - 1;
            // В отличие от SelfEatenCheck, съедение яблок проверяется по хвосту, а не по голове змейки
            int tailX = blockList[maxI].X;
            int tailY = blockList[maxI].Y;

            for (int i = 0; i < Data.appleList.Count; i++)
            {
                if (Data.appleList[i].X == tailX && Data.appleList[i].Y == tailY)
                {
                    Data.score++;
                    Data.appleList.RemoveAt(i);
                    Program.GenerateApples();
                    blockList.Add(new Block(tailX, tailY, Data.SNAKE_CHAR));
                }
            }
        }

        public bool Exists(Predicate<Block> match)
        {
            return blockList.Exists(match);
        }
    }
}
