using System;
using System.Collections.Generic;
using System.Linq;
using DlxLib;

namespace DlxLibDemo2
{
    internal class Program
    {
        private static void Main()
        {
            Test1();
            Test2();
        }

        private static void Test1()
        {
            var matrix = new[,]
                {
                    {0, 0, 1, 0, 1, 1, 0},
                    {1, 0, 0, 1, 0, 0, 1},
                    {0, 1, 1, 0, 0, 1, 0},
                    {1, 0, 0, 1, 0, 0, 0},
                    {0, 1, 0, 0, 0, 0, 1},
                    {0, 0, 0, 1, 1, 0, 1}
                };

            var columnNames = new[] {"A", "B", "C", "D", "E", "F", "G"};

            var dlx = new Dlx();
            dlx.SolutionFound += (sender, args) =>
            {
                var solution = "[" + string.Join(", ", args.Solution.RowIndexes) + "]";
                Console.WriteLine("SolutionFound - Solution: {0}; SolutionIndex: {1}", solution, args.SolutionIndex);
            };
            dlx.SearchStep += (sender, args) =>
            {
                var rowIndexes = "[" + string.Join(", ", args.RowIndexes) + "]";
                Console.WriteLine("SearchStep - Step: {0}; RowIndexes: {1}", args.Step, rowIndexes);
            };
            var solutions = dlx.Solve(matrix);
            PrintSolutions(matrix, columnNames, solutions);
        }

        private static void Test2()
        {
            var matrix = new[,]
                {
                    {1, 0, 0, 0},
                    {0, 1, 1, 0},
                    {1, 0, 0, 1},
                    {0, 0, 1, 1},
                    {0, 1, 0, 0},
                    {0, 0, 1, 0}
                };

            var columnNames = new[] {"One", "Two", "Three", "Four"};

            var dlx = new Dlx();
            dlx.SolutionFound += (sender, args) =>
            {
                var solution = "[" + string.Join(", ", args.Solution.RowIndexes) + "]";
                Console.WriteLine("SolutionFound - Solution: {0}; SolutionIndex: {1}", solution, args.SolutionIndex);
            };
            dlx.SearchStep += (sender, args) =>
            {
                var rowIndexes = "[" + string.Join(", ", args.RowIndexes) + "]";
                Console.WriteLine("SearchStep - Step: {0}; RowIndexes: {1}", args.Step, rowIndexes);
            };
            var solutions = dlx.Solve(matrix);
            PrintSolutions(matrix, columnNames, solutions);
        }

        private static void PrintSolutions(int[,] matrix, IList<string> columnNames, IEnumerable<Solution> solutions)
        {
            // ReSharper disable ReturnValueOfPureMethodIsNotUsed
            solutions.Select((solution, index) =>
            {
                PrintSolution(matrix, columnNames, solution, index);
                return 0;
            }).ToList();
            // ReSharper restore ReturnValueOfPureMethodIsNotUsed
        }

        private static void PrintSolution(int[,] matrix, IList<string> columnNames, Solution solution, int index)
        {
            var rowIndexes = solution.RowIndexes.ToList();
            Console.WriteLine("Solution number {0}:", index + 1);

            var maxColumnNameLength = columnNames.Max(s => s.Length);
            var columnNameFormatString = string.Format("{{0,-{0}}}", maxColumnNameLength);

            var numRowsInSolution = rowIndexes.Count;
            var numCols = matrix.GetLength(1);

            for (var solutionRowIndex = 0; solutionRowIndex < numRowsInSolution; solutionRowIndex++)
            {
                var matrixRowIndex = rowIndexes[solutionRowIndex];
                Console.Write("matrix[{0}]: ", matrixRowIndex);
                for (var matrixColIndex = 0; matrixColIndex < numCols; matrixColIndex++)
                {
                    var columnName = string.Empty;
                    if (matrix[matrixRowIndex, matrixColIndex] != 0)
                    {
                        columnName = columnNames[matrixColIndex];
                    }
                    Console.Write(columnNameFormatString, columnName);

                    if (matrixColIndex + 1 < numCols)
                    {
                        Console.Write("  ");
                    }
                }
                Console.WriteLine();
            }

            Console.WriteLine();
        }
    }
}
