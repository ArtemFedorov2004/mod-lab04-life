using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.IO;
using System.Text.Json;
using ScottPlot;

namespace cli_life
{
    public class Cell
    {
        public bool IsAlive;
        public readonly List<Cell> neighbors = new List<Cell>();
        private bool IsAliveNext;
        public void DetermineNextLiveState()
        {
            int liveNeighbors = neighbors.Where(x => x.IsAlive).Count();
            if (IsAlive)
                IsAliveNext = liveNeighbors == 2 || liveNeighbors == 3;
            else
                IsAliveNext = liveNeighbors == 3;
        }
        public void Advance()
        {
            IsAlive = IsAliveNext;
        }
    }
    public class Board
    {
        public readonly Cell[,] Cells;
        public readonly int CellSize;

        public int Columns { get { return Cells.GetLength(0); } }
        public int Rows { get { return Cells.GetLength(1); } }
        public int Width { get { return Columns * CellSize; } }
        public int Height { get { return Rows * CellSize; } }

        public Board(int width, int height, int cellSize, double liveDensity = .1)
        {
            CellSize = cellSize;

            Cells = new Cell[width / cellSize, height / cellSize];
            for (int x = 0; x < Columns; x++)
                for (int y = 0; y < Rows; y++)
                    Cells[x, y] = new Cell();

            ConnectNeighbors();
            Randomize(liveDensity);
        }

        readonly Random rand = new Random();
        public void Randomize(double liveDensity)
        {
            foreach (var cell in Cells)
                cell.IsAlive = rand.NextDouble() < liveDensity;
        }

        public void Advance()
        {
            foreach (var cell in Cells)
                cell.DetermineNextLiveState();
            foreach (var cell in Cells)
                cell.Advance();
        }
        private void ConnectNeighbors()
        {
            for (int x = 0; x < Columns; x++)
            {
                for (int y = 0; y < Rows; y++)
                {
                    int xL = (x > 0) ? x - 1 : Columns - 1;
                    int xR = (x < Columns - 1) ? x + 1 : 0;

                    int yT = (y > 0) ? y - 1 : Rows - 1;
                    int yB = (y < Rows - 1) ? y + 1 : 0;

                    Cells[x, y].neighbors.Add(Cells[xL, yT]);
                    Cells[x, y].neighbors.Add(Cells[x, yT]);
                    Cells[x, y].neighbors.Add(Cells[xR, yT]);
                    Cells[x, y].neighbors.Add(Cells[xL, y]);
                    Cells[x, y].neighbors.Add(Cells[xR, y]);
                    Cells[x, y].neighbors.Add(Cells[xL, yB]);
                    Cells[x, y].neighbors.Add(Cells[x, yB]);
                    Cells[x, y].neighbors.Add(Cells[xR, yB]);
                }
            }
        }
    }

    public class Utils
    {
        public static int Cells(Board board)
        {
            int total = 0;

            for (int x = 0; x < board.Columns; x++)
            {
                for (int y = 0; y < board.Rows; y++)
                {
                    if (board.Cells[x, y].IsAlive)
                    {
                        total++;
                    }
                }
            }

            return total;
        }

        public static List<List<(int, int)>> Combinations(Board board)
        {
            var combinations = new List<List<(int, int)>>();
            bool[,] visited = new bool[board.Columns, board.Rows];

            for (int i = 0; i < board.Columns; i++)
            {
                for (int j = 0; j < board.Rows; j++)
                {
                    if (board.Cells[i, j].IsAlive && !visited[i, j])
                    {
                        var group = new List<(int, int)>();
                        DFS(board, visited, group, i, j);
                        combinations.Add(group);
                    }
                }
            }

            return combinations;
        }

        public static int CombinationsCount(Board board)
        {
            return Combinations(board)
                .Count;
        }

        private static void DFS(Board board, bool[,] visited, List<(int, int)> group, int x, int y)
        {
            if (visited[x, y] || !board.Cells[x, y].IsAlive)
                return;

            visited[x, y] = true;
            group.Add((x, y));

            for (int dx = -1; dx <= 1; dx++)
            {
                for (int dy = -1; dy <= 1; dy++)
                {
                    if (dx == 0 && dy == 0) continue;

                    int X = (x + dx + board.Columns) % board.Columns;
                    int Y = (y + dy + board.Rows) % board.Rows;

                    DFS(board, visited, group, X, Y);
                }
            }
        }
    }

    public class TemplatesClassifier
    {
        private Dictionary<string, int[,]> _templates = new Dictionary<string, int[,]>();

        private void LoadTemplates(string[] files)
        {
            var templates = new Dictionary<string, int[,]>();
            string dir = Path.Combine(
                Directory.GetParent(Environment.CurrentDirectory)
                    .Parent
                    .Parent
                    .FullName,
                "templates");

            for (int i = 0; i < files.Length; i++)
            {
                string path = Path.Combine(dir, files[i]);
                if (!File.Exists(path))
                {
                    throw new FileNotFoundException(path);
                }

                string templateTitle = files[i].Remove(files[i].Length - 4);
                _templates[templateTitle] = ReadTemplate(path);
            }
        }

        private int[,] ReadTemplate(string path)
        {
            var lines = File.ReadAllLines(path);
            var template = new int[lines[0].Length, lines.Length];

            for (int y = 0; y < lines.Length; y++)
                for (int x = 0; x < lines[y].Length; x++)
                    template[x, y] = lines[y][x] == '1' ? 1 : 0;

            return template;
        }

        public TemplatesClassifier(string[] files)
        {
            LoadTemplates(files);
        }

        public Dictionary<string, int> ClassifyBoard(Board board)
        {
            var result = _templates.Keys.ToDictionary(k => k, _ => 0);
            result["unknown"] = 0;

            List<List<(int, int)>> combinations = Utils.Combinations(board);

            foreach (var combination in combinations)
            {
                result[Classify(combination)] += 1;
            }

            return result;
        }

        private string Classify(List<(int, int)> combination)
        {
            int[,] combinationTemplate = FormTemplate(combination);

            foreach (var kvp in _templates)
            {
                if (AreTemplatesEqual(kvp.Value, combinationTemplate))
                {
                    return kvp.Key;
                }
            }

            return "unknown";
        }

        private int[,] FormTemplate(List<(int x, int y)> combination)
        {
            int minY = combination.Min(c => c.y);
            int maxY = combination.Max(c => c.y);

            int minX = combination.Min(c => c.x);
            int maxX = combination.Max(c => c.x);

            var template = new int[maxX - minX + 1, maxY - minY + 1];

            foreach (var (x, y) in combination)
                template[x - minX, y - minY] = 1;

            return template;
        }

        private static bool AreTemplatesEqual(int[,] template1, int[,] template2)
        {
            if (template1.GetLength(0) != template2.GetLength(0) || template1.GetLength(1) != template2.GetLength(1))
            {
                return false;
            }

            for (int i = 0; i < template1.GetLength(0); i++)
            {
                for (int j = 0; j < template1.GetLength(1); j++)
                {
                    if (template1[i, j] != template2[i, j])
                    {
                        return false;
                    }
                }
            }

            return true;
        }
    }

    public class StabilityChecker
    {
        private int _stablePhases;

        private int _liveCells;

        private int _minStablePhases;

        public StabilityChecker()
        {
            _stablePhases = 0;
            _liveCells = 0;
            _minStablePhases = 6;
        }

        public bool Check(int liveCells)
        {
            if (_stablePhases != 0)
            {
                if (_liveCells == liveCells)
                {
                    _stablePhases += 1;
                    if (_stablePhases >= _minStablePhases)
                    {
                        return true;
                    }
                }
                else
                {
                    _stablePhases = 1;
                    _liveCells = liveCells;
                }
            }
            else
            {
                _stablePhases += 1;
                _liveCells = liveCells;
            }

            return false;
        }
    }

    record BoardProps(
            int width,
            int height,
            int cellSize,
            double lifeDensity
            );

    class Program
    {
        static Board board;

        static private void SetUpBoard(string file)
        {
            string jsonString = File.ReadAllText(file);

            var props = JsonSerializer.Deserialize<BoardProps>(jsonString);

            board = new Board(props.width, props.height, props.cellSize, props.lifeDensity);
        }

        static private void SaveBoard(string file)
        {
            using (StreamWriter sw = new StreamWriter(file))
            {
                for (int i = 0; i < board.Rows; i++)
                {
                    for (int j = 0; j < board.Columns; j++)
                    {
                        sw.Write(board.Cells[j, i].IsAlive ? '1' : '0');
                    }
                    sw.WriteLine();
                }
            }
        }

        static private void LoadBoard(string file)
        {
            using (StreamReader sr = new StreamReader(file))
            {
                var lines = File.ReadAllLines(file);
                int col = board.Cells.GetLength(0);
                int rows = board.Cells.GetLength(1);

                for (int i = 0; i < rows; i++)
                {
                    for (int j = 0; j < col; j++)
                    {
                        board.Cells[j, i].IsAlive = lines[i][j] == '1';
                    }
                }
            }
        }

        static private void PlaceOnBoard(string template, int xOffset, int yOffset)
        {
            var lines = File.ReadAllLines(template);
            for (int i = 0; i < lines.Length; i++)
            {
                for (int j = 0; j < lines[i].Length; j++)
                {
                    int xCoordinate = (j + xOffset) % board.Columns;
                    int yCoordinate = (i + yOffset) % board.Rows;
                    board.Cells[xCoordinate, yCoordinate].IsAlive = '1' == lines[i][j];
                }
            }
        }

        static int OnClick(string file)
        {
            if (Console.KeyAvailable)
            {
                var key = Console.ReadKey(true).Key;
                if (key == ConsoleKey.Escape)
                {
                    return 1;
                }
                if (key == ConsoleKey.S)
                {
                    SaveBoard(file);
                }
                if (key == ConsoleKey.L)
                {
                    LoadBoard(file);
                    _stabilityChecker = new StabilityChecker();
                }
            }
            return 0;
        }

        static void Render()
        {
            for (int row = 0; row < board.Rows; row++)
            {
                for (int col = 0; col < board.Columns; col++)
                {
                    var cell = board.Cells[col, row];
                    if (cell.IsAlive)
                    {
                        Console.Write('*');
                    }
                    else
                    {
                        Console.Write(' ');
                    }
                }
                Console.Write('\n');
            }
        }

        static TemplatesClassifier _templatesClassifier;

        static void SetUpTemplatesClassifier()
        {
            string[] templateFileNames =
            {
                "block.txt",
                "box.txt",
                "glider.txt",
                "hive.txt",
                "long_ship.txt",
                "pond.txt",
                "snake.txt",
            };

            _templatesClassifier = new TemplatesClassifier(templateFileNames);
        }
        static void PrintClassification()
        {
            var dict = _templatesClassifier.ClassifyBoard(board);

            Console.WriteLine("Фигуры:");
            Console.WriteLine("Название --- количество");
            foreach (var kvp in dict.Where(r => r.Value > 0))
            {
                Console.WriteLine($"{kvp.Key}: {kvp.Value}");
            }
        }

        static StabilityChecker _stabilityChecker = new StabilityChecker();

        static int _generationNumber;

        static void Main(string[] args)
        {
            string dir = Directory.GetParent(Environment.CurrentDirectory)
                    .Parent
                    .Parent
                    .FullName;
            string cfg = Path.Combine(dir, "application.json");
            string backup = Path.Combine(dir, "backup.txt");
            string templateExample = Path.Combine(dir, "templates/snake.txt");

            SetUpBoard(cfg);
            SetUpTemplatesClassifier();

            PlaceOnBoard(templateExample, 0, 0);

            GameLoop(backup);

            //AverageValueOfStableGenerations(cfg, backup);

            //PrepareDataForPlot(cfg, backup);

            //string dataFile = Path.Combine(dir, "data.txt");
            //string plotFile = Path.Combine(dir, "plot.png");
            //CreatePopulationPlot(dataFile, plotFile);
        }

        static void GameLoop(string backup)
        {
            _generationNumber = 0;
            while (true)
            {
                if (OnClick(backup) == 1)
                    break;

                _generationNumber++;
                Console.Clear();
                Render();

                int liveCells = Utils.Cells(board);
                int combinations = Utils.CombinationsCount(board);
                Console.WriteLine($"Число элементов. Клеток : {liveCells}, Комбинаций : {combinations}");
                if (_stabilityChecker.Check(liveCells))
                {
                    Console.WriteLine($"Система стабилизировалась на поколении {_generationNumber}");
                    return;
                }

                PrintClassification();

                board.Advance();
                Thread.Sleep(10);
            }
        }

        static void AverageValueOfStableGenerations(string cfg, string backup)
        {
            int numberOfStarts = 16;
            double lifeDensity = 0.1;

            string outDir = Path.Combine(
                Directory.GetParent(Environment.CurrentDirectory)
                    .Parent
                    .Parent
                    .FullName,
                "average_value_of_stable_generations");

            while (lifeDensity < 0.9)
            {
                string jsonString = File.ReadAllText(cfg);

                var props = JsonSerializer.Deserialize<BoardProps>(jsonString);

                string fileName = $"life_density_{lifeDensity:0.0}.txt".Replace(',', '.');
                string filePath = Path.Combine(outDir, fileName);
                string str;
                List<int> generations = [];

                using (FileStream fs = File.Create(filePath))
                {
                }
                for (int i = 0; i < numberOfStarts; i++)
                {
                    _stabilityChecker = new StabilityChecker();
                    _generationNumber = 0;
                    board = new Board(props.width, props.height, props.cellSize, lifeDensity);

                    GameLoop(backup);
                    generations.Add(_generationNumber);
                    str = $"Номер запуска - {i + 1}. Число поколений: {_generationNumber}\n";
                    File.AppendAllText(filePath, str);
                }
                str = $"Среднее число поколений: {Math.Round(generations.Average())}\n";
                File.AppendAllText(filePath, str);
                lifeDensity += 0.1;
            }
        }

        private static void PrepareDataForPlot(string cfg, string backup)
        {
            double lifeDensity = 0;
            double step = 0.02;
            string file = Path.Combine(
                Directory.GetParent(Environment.CurrentDirectory)
                    .Parent
                    .Parent
                    .FullName,
                "data.txt");

            string str = "Количество поколений в зависимости от плотности\n";
            File.AppendAllText(file, str);
            while (lifeDensity < 1.01)
            {
                string jsonString = File.ReadAllText(cfg);

                var props = JsonSerializer.Deserialize<BoardProps>(jsonString);

                _stabilityChecker = new StabilityChecker();
                _generationNumber = 0;
                board = new Board(props.width, props.height, props.cellSize, lifeDensity);

                GameLoop(backup);

                str = $"{Math.Round(lifeDensity, 2)} {_generationNumber}\n";
                File.AppendAllText(file, str);
                lifeDensity += step;
            }
        }

        private static void CreatePopulationPlot(string dataFile, string plotFile)
        {
            var data = File.ReadAllLines(dataFile)
                .Skip(1)
                .Select(line => line.Split(' '))
                .Where(parts => parts.Length == 2)
                .Select(parts => new {
                    LifeDensity = double.Parse(parts[0]),
                    NumberOfGenerations = int.Parse(parts[1])
                })
                .ToList();

            var plot = new Plot();

            var lifeDensities = data.Select(x => (double)x.LifeDensity).ToArray();
            var generations = data.Select(x => (double)x.NumberOfGenerations).ToArray();
            var scatter = plot.Add.Scatter(lifeDensities, generations);
            scatter.MarkerSize = 1;

            plot.YLabel("Количество поколений", size: 15);
            plot.XLabel("Плотность", size: 15);
            plot.Title("Количество поколений в зависимости от плотности", size: 17);

            plot.Axes.AutoScale();

            plot.SavePng(plotFile, 600, 600);
        }
    }
}