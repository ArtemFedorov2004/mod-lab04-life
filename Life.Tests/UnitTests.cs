namespace Life.Tests
{
    public class UnitTests
    {
        [Fact]
        public void Test1()
        {
            throw new NotImplementedException();
        }

        [Fact]
        public void Test_Cells_WithNoAliveCells()
        {
            var board = new Board(10, 10, 1, 0);
            Assert.Equal(0, Utils.Cells(board));
        }

        [Fact]
        public void Test_Cells_WithOneAliveCell()
        {
            var board = new Board(10, 10, 1, 0);
            board.Cells[0, 0].IsAlive = true;
            Assert.Equal(1, Utils.Cells(board));
        }

        [Fact]
        public void Test_Cells_WithAllAliveCells()
        {
            var board = new Board(10, 10, 1, 0);
            foreach (var cell in board.Cells)
            {
                cell.IsAlive = true;
            }
            Assert.Equal(100, Utils.Cells(board));
        }

        [Fact]
        public void Test_Combinations_WithNoAliveCells()
        {
            var board = new Board(10, 10, 1, 0);
            var combinations = Utils.Combinations(board);
            Assert.Equal(0, combinations.Count);
        }

        [Fact]
        public void Test_Combinations_WithOneAliveCell()
        {
            var board = new Board(10, 10, 1, 0);
            board.Cells[0, 0].IsAlive = true;
            var combinations = Utils.Combinations(board);
            Assert.Equal(1, combinations.Count);
            Assert.Equal(1, combinations[0].Count);
            Assert.Equal((0, 0), combinations[0][0]);
        }

        [Fact]
        public void Test_Combinations_WithTwoDisconnectedAliveCells()
        {
            var board = new Board(10, 10, 1, 0);
            board.Cells[0, 0].IsAlive = true;
            board.Cells[2, 2].IsAlive = true;
            var combinations = Utils.Combinations(board);
            Assert.Equal(2, combinations.Count);
            Assert.Equal(1, combinations[0].Count);
            Assert.Equal(1, combinations[1].Count);
        }

        [Fact]
        public void Test_Combinations_WithConnectedCells()
        {
            var board = new Board(10, 10, 1, 0);
            board.Cells[0, 0].IsAlive = true;
            board.Cells[0, 1].IsAlive = true;
            var combinations = Utils.Combinations(board);
            Assert.Equal(1, combinations.Count);
            Assert.Equal(2, combinations[0].Count);
            Assert.Contains((0, 0), combinations[0]);
            Assert.Contains((0, 1), combinations[0]);
        }

        [Fact]
        public void Test_Combinations_WithMultipleConnectedGroups()
        {
            var board = new Board(10, 10, 1, 0);
            board.Cells[0, 0].IsAlive = true;
            board.Cells[0, 1].IsAlive = true;
            board.Cells[5, 5].IsAlive = true;
            var combinations = Utils.Combinations(board);
            Assert.Equal(2, combinations.Count);
            Assert.Equal(2, combinations[0].Count);
            Assert.Equal(1, combinations[1].Count);
        }

        [Fact]
        public void Test_CombinationsCount_WithNoAliveCells()
        {
            var board = new Board(10, 10, 1, 0);
            Assert.Equal(0, Utils.CombinationsCount(board));
        }

        [Fact]
        public void Test_CombinationsCount_WithOneAliveCell()
        {
            var board = new Board(10, 10, 1, 0);
            board.Cells[0, 0].IsAlive = true;
            Assert.Equal(1, Utils.CombinationsCount(board));
        }

        [Fact]
        public void Test_CombinationsCount_WithTwoDisconnectedAliveCells()
        {
            var board = new Board(10, 10, 1, 0);
            board.Cells[0, 0].IsAlive = true;
            board.Cells[2, 2].IsAlive = true;
            Assert.Equal(2, Utils.CombinationsCount(board));
        }

        [Fact]
        public void Test_CombinationsCount_WithConnectedCells()
        {
            var board = new Board(10, 10, 1, 0);
            board.Cells[0, 0].IsAlive = true;
            board.Cells[0, 1].IsAlive = true;
            Assert.Equal(1, Utils.CombinationsCount(board));
        }

        [Fact]
        public void Test_CombinationsCount_WithMultipleConnectedGroups()
        {
            var board = new Board(10, 10, 1, 0);
            board.Cells[0, 0].IsAlive = true;
            board.Cells[0, 1].IsAlive = true;
            board.Cells[5, 5].IsAlive = true;
            Assert.Equal(2, Utils.CombinationsCount(board));
        }

        [Fact]
        public void Test_CombinationsCount_WithAllCellsAlive()
        {
            var board = new Board(10, 10, 1, 0);
            foreach (var cell in board.Cells)
            {
                cell.IsAlive = true;
            }
            Assert.Equal(1, Utils.CombinationsCount(board));
        }

        [Fact]
        public void Test_CombinationsCount_WithSingleRowAndMultipleAliveCells()
        {
            var board = new Board(10, 10, 1, 0);
            board.Cells[0, 0].IsAlive = true;
            board.Cells[1, 0].IsAlive = true;
            board.Cells[2, 0].IsAlive = true;
            Assert.Equal(1, Utils.CombinationsCount(board));
        }

        [Fact]
        public void Test_CombinationsCount_WithSingleColumnAndMultipleAliveCells()
        {
            var board = new Board(10, 10, 1, 0);
            board.Cells[0, 0].IsAlive = true;
            board.Cells[0, 1].IsAlive = true;
            board.Cells[0, 2].IsAlive = true;
            Assert.Equal(1, Utils.CombinationsCount(board));
        }

        [Fact]
        public void LoadTemplates_ShouldThrowFileNotFoundException_WhenTemplateFileDoesNotExist()
        {
            var invalidFile = new string[] { "nonexistentTemplate.txt" };

            var exception = Assert.Throws<FileNotFoundException>(() => new TemplatesClassifier(invalidFile));
            Assert.Contains("nonexistentTemplate.txt", exception.Message);
        }

        [Fact]
        public void ClassifyBoard_ShouldClassifyCorrectly_WhenBoardMatchesTemplate()
        {
            var templateFileNames = new[] { "block.txt", "glider.txt" };
            var classifier = new TemplatesClassifier(templateFileNames);

            var board = new Board(10, 10, 1, 0);
            board.Cells[1, 1].IsAlive = true;
            board.Cells[1, 2].IsAlive = true;
            board.Cells[2, 1].IsAlive = true;
            board.Cells[2, 2].IsAlive = true;

            var result = classifier.ClassifyBoard(board);

            Assert.Equal(1, result["block"]);
            Assert.Equal(0, result["glider"]);
            Assert.Equal(0, result["unknown"]);
        }

        [Fact]
        public void ClassifyBoard_ShouldReturnUnknown_WhenNoMatchFound()
        {
            var templateFileNames = new[] { "block.txt", "glider.txt" };
            var classifier = new TemplatesClassifier(templateFileNames);

            var board = new Board(10, 10, 1, 0);
            board.Cells[5, 5].IsAlive = true;

            var result = classifier.ClassifyBoard(board);

            Assert.Equal(0, result["block"]);
            Assert.Equal(0, result["glider"]);
            Assert.Equal(1, result["unknown"]);
        }

        [Fact]
        public void Check_ShouldReturnTrue_WhenStableForMinStablePhases()
        {
            var stabilityChecker = new StabilityChecker();
            int stableLiveCells = 5;
            int minStablePhases = 6;

            bool isStable = false;
            for (int i = 0; i < minStablePhases; i++)
            {
                isStable = stabilityChecker.Check(stableLiveCells);
            }

            Assert.True(isStable);
        }

        [Fact]
        public void Check_ShouldReturnFalse_WhenLiveCellsChange()
        {
            var stabilityChecker = new StabilityChecker();
            int firstLiveCells = 5;
            int secondLiveCells = 7;

            bool isStableFirst = stabilityChecker.Check(firstLiveCells);
            bool isStableSecond = stabilityChecker.Check(secondLiveCells);

            Assert.False(isStableFirst);
            Assert.False(isStableSecond);
        }

        [Fact]
        public void Check_ShouldReturnFalse_WhenNotEnoughStablePhases()
        {
            var stabilityChecker = new StabilityChecker();
            int stableLiveCells = 5;
            int minStablePhases = 6;

            bool isStable = false;
            for (int i = 0; i < minStablePhases - 1; i++)
            {
                isStable = stabilityChecker.Check(stableLiveCells);
            }

            Assert.False(isStable);
        }

        [Fact]
        public void Check_ShouldReturnTrue_AfterEnoughStablePhases_WithConstantLiveCells()
        {
            var stabilityChecker = new StabilityChecker();
            int stableLiveCells = 10;

            bool isStable = false;

            for (int i = 0; i < 6; i++)
            {
                isStable = stabilityChecker.Check(stableLiveCells);
            }

            Assert.True(isStable);
        }
    }
}