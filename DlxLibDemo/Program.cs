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

            var dlx = new Dlx();
            var solutions = dlx.Solve(matrix);
            PrintMatrix(matrix);
            PrintSolutions(solutions);
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

            var dlx = new Dlx();
            var solutions = dlx.Solve(matrix);
            PrintMatrix(matrix);
            PrintSolutions(solutions);
        }

        private static void PrintMatrix(int[,] matrix)
        {
            var numRows = matrix.GetLength(0);
            var numCols = matrix.GetLength(1);

            for (var rowIndex = 0; rowIndex < numRows; rowIndex++)
            {
                var values = new List<int>();
                for (var colIndex = 0; colIndex < numCols; colIndex++)
                {
                    values.Add(matrix[rowIndex, colIndex]);
                }
                var line = string.Join(", ", values);
                Console.WriteLine("matrix[{0}]: {{{1}}}", rowIndex, line);
            }
        }

        private static void PrintSolutions(IEnumerable<Solution> solutions)
        {
            // ReSharper disable ReturnValueOfPureMethodIsNotUsed
            solutions.Select((solution, index) =>
            {
                var rowIndexes = string.Join(", ", solution.RowIndexes);
                var line = string.Format("Solution[{0}] row indexes: [{1}]", index, rowIndexes);
                Console.WriteLine(line);
                return line;
            }).ToList();
            // ReSharper restore ReturnValueOfPureMethodIsNotUsed
        }
    }
}
