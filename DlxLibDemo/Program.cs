using System;
using System.Collections.Generic;
using System.Linq;
using DlxLib;

namespace DlxLibDemo
{
    internal class Program
    {
        private static void Main()
        {
            Demo1();
            Demo2();
        }

        private static void Demo1()
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

            var dlx = new Dlx();
            var solutions = dlx.Solve(matrix);
            PrintSolutions(matrix, solutions);
        }

        private static void Demo2()
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

            var dlx = new Dlx();
            var solutions = dlx.Solve(matrix);
            PrintSolutions(matrix, solutions);
        }

        private static void PrintSolutions(int[,] matrix, IEnumerable<Solution> solutions)
        {
            // ReSharper disable ReturnValueOfPureMethodIsNotUsed
            solutions.Select((solution, index) =>
                {
                    PrintSolution(matrix, solution, index);
                    return 0;
                }).ToList();
            // ReSharper restore ReturnValueOfPureMethodIsNotUsed
        }

        private static void PrintSolution(int[,] matrix, Solution solution, int index)
        {
            var rowIndexes = "[" + string.Join(", ", solution.RowIndexes) + "]";
            Console.WriteLine("Solution number {0} ({1}):", index + 1, rowIndexes);

            var numRows = matrix.GetLength(0);
            var numCols = matrix.GetLength(1);

            for (var rowIndex = 0; rowIndex < numRows; rowIndex++)
            {
                Console.Write("matrix[{0}]: {{", rowIndex);
                for (var colIndex = 0; colIndex < numCols; colIndex++)
                {
                    var value = matrix[rowIndex, colIndex];

                    ChangeConsoleForegroundColorIf(
                        solution.RowIndexes.Contains(rowIndex) && value != 0,
                        ConsoleColor.Yellow,
                        () => Console.Write("{0}", value));

                    if (colIndex + 1 < numCols)
                    {
                        Console.Write(", ");
                    }
                }
                Console.WriteLine("}");
            }

            Console.WriteLine();
        }

        private static void ChangeConsoleForegroundColorIf(bool condition, ConsoleColor consoleColor, Action action)
        {
            var oldForegroundColor = Console.ForegroundColor;

            if (condition)
            {
                Console.ForegroundColor = consoleColor;
            }

            action();

            if (condition)
            {
                Console.ForegroundColor = oldForegroundColor;
            }
        }
    }
}
