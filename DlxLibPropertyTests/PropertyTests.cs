using System;
using System.Collections.Generic;
using System.Linq;
using DlxLib;
using FsCheck;
using FsCheck.Fluent;
using FsCheckUtils;
using Microsoft.FSharp.Core;
using NUnit.Framework;

namespace DlxLibPropertyTests
{
    using Property = Gen<Rose<Result>>;
    
    [TestFixture]
    public class PropertyTests
    {
        private static readonly Config Config = Config.VerboseThrowOnFailure;

        [Test]
        public void ExactCoverProblemsWithNoSolutionsTest()
        {
            Func<int, string> makeLabel = numSolutions => string.Format(
                "Expected no solutions but got {0}",
                numSolutions);

            var arb = Arb.fromGen(GenMatrixOfIntWithNoSolutions());
            var property = Prop.forAll(arb, FSharpFunc<int[,], Property>.FromConverter(matrix =>
            {
                var solutions = new Dlx().Solve(matrix).ToList();
                return PropExtensions.Label(!solutions.Any(), makeLabel(solutions.Count()));
            }));
            Check.One(Config, property);
        }

        [Test]
        public void ExactCoverProblemsWithSingleSolutionTest()
        {
            Func<int, string> makeLabel = numSolutions => string.Format(
                "Expected exactly one solution but got {0}",
                numSolutions);

            var arb = Arb.fromGen(GenMatrixOfIntWithSingleSolution());
            var property = Prop.forAll(arb, FSharpFunc<int[,], Property>.FromConverter(matrix =>
            {
                var solutions = new Dlx().Solve(matrix).ToList();
                var p1 = PropExtensions.Label(solutions.Count() == 1, makeLabel(solutions.Count()));
                var p2 = CheckSolutions(solutions, matrix);
                return PropExtensions.And(p1, p2);
            }));
            Check.One(Config, property);
        }

        [Test]
        public void ExactCoverProblemsWithMultipleSolutionsTest()
        {
            const int numSolutions = 3;

            Func<int, string> makeLabel = actualNumSolutions => string.Format(
                "Expected exactly {0} solutions but got {1}",
                numSolutions, actualNumSolutions);

            var arb = Arb.fromGen(GenMatrixOfIntWithMultipleSolutions(numSolutions));
            var property = Prop.forAll(arb, FSharpFunc<int[,], Property>.FromConverter(matrix =>
            {
                var solutions = new Dlx().Solve(matrix).ToList();
                var actualNumSolutions = solutions.Count();
                var p1 = PropExtensions.Label(actualNumSolutions == numSolutions, makeLabel(actualNumSolutions));
                var p2 = CheckSolutions(solutions, matrix);
                return PropExtensions.And(p1, p2);
            }));
            Check.One(Config, property);
        }

        private static Property CheckSolutions(IEnumerable<Solution> solutions, int[,] matrix)
        {
            return PropExtensions.AndAll(solutions.Select(solution => CheckSolution(solution, matrix)).ToArray());
        }

        private static Property CheckSolution(Solution solution, int[,] matrix)
        {
            var numCols = matrix.GetLength(1);
            var numSolutionRows = solution.RowIndexes.Count();
            var expectedNumZerosPerColumn = numSolutionRows - 1;
            var colProperties = new List<Property>();

            Func<int, int, string> makeLabel1 = (colIndex, numOnes) => string.Format(
                "Expected column {0} to contain a single 1 but it contains {1}",
                colIndex,
                numOnes);

            Func<int, int, string> makeLabel2 = (colIndex, numZeros) => string.Format(
                "Expected column {0} to contain exactly {1} 0s but it contains {2}",
                colIndex,
                expectedNumZerosPerColumn,
                numZeros);

            for (var colIndex = 0; colIndex < numCols; colIndex++)
            {
                var numZeros = 0;
                var numOnes = 0;

                foreach (var rowIndex in solution.RowIndexes)
                {
                    if (matrix[rowIndex, colIndex] == 0) numZeros++;
                    if (matrix[rowIndex, colIndex] == 1) numOnes++;
                }

                var p1 = PropExtensions.Label(numOnes == 1, makeLabel1(colIndex, numOnes));
                var p2 = PropExtensions.Label(numZeros == expectedNumZerosPerColumn, makeLabel2(colIndex, numZeros));

                colProperties.Add(PropExtensions.And(p1, p2));
            }

            return PropExtensions.AndAll(colProperties.ToArray());
        }

        private static Gen<int[,]> GenMatrixOfIntWithNoSolutions()
        {
            return
                from numCols in Any.IntBetween(2, 20)
                from numRows in Any.IntBetween(2, 20)
                from indexOfAlwaysZeroColumn in Any.IntBetween(0, numCols - 1)
                let genZeroOrOne = Any.ValueIn(0, 1)
                let genRow = genZeroOrOne.MakeListOfLength(numCols).Select(r =>
                {
                    r[indexOfAlwaysZeroColumn] = 0;
                    return r;
                })
                from rows in genRow.MakeListOfLength(numRows)
                select rows.To2DArray();
        }

        private static Gen<int[,]> GenMatrixOfIntWithSingleSolution()
        {
            return
                from numCols in Any.IntBetween(2, 20)
                from numSolutionRows in Any.IntBetween(1, Math.Min(5, numCols))
                from numRows in Any.IntBetween(numSolutionRows, 20)
                from matrix in Any.Value(0).MakeListOfLength(numCols).MakeListOfLength(numRows)
                from solutionRows in GenSolutionRows(numCols, numSolutionRows)
                from randomRowIdxs in GenExtensions.PickValues(numSolutionRows, Enumerable.Range(0, numRows))
                select PokeSolutionRowsIntoMatrix(matrix, solutionRows, randomRowIdxs).To2DArray();
        }

        private static Gen<int[,]> GenMatrixOfIntWithMultipleSolutions(int numSolutions)
        {
            return
                from numCols in Any.IntBetween(10, 20)
                from numRows in Any.IntBetween(10, 20)
                from matrix in Any.Value(0).MakeListOfLength(numCols).MakeListOfLength(numRows)
                from solutions in GenSolution(numCols).MakeListOfLength(numSolutions)
                let _ = DumpSolutions(solutions)
                where NoneOfTheSolutionsOverlap(solutions)
                let combinedSolutions = CombineSolutions(solutions)
                from randomRowIdxs in GenExtensions.PickValues(combinedSolutions.Count, Enumerable.Range(0, numRows))
                select PokeSolutionRowsIntoMatrix(matrix, combinedSolutions, randomRowIdxs).To2DArray();
        }

        private static Gen<List<List<int>>> GenSolution(int numCols)
        {
            //return
            //    from numPartialSolutionRows in Any.IntBetween(1, Math.Min(5, numCols))
            //    from partialSolutionRows in GenPartialSolutionRows(numCols, numPartialSolutionRows)
            //    select partialSolutionRows;

            return
                from partialSolutionRows in GenSolutionRows(numCols, 3)
                select partialSolutionRows;
        }

        private static Gen<List<List<int>>> GenSolutionRows(int numCols, int numSolutionRows)
        {
            return
                from solutionRows in Any.Value(0).MakeListOfLength(numCols).MakeListOfLength(numSolutionRows)
                from randomRowIdxPerColumn in Any.IntBetween(0, numSolutionRows - 1).MakeListOfLength(numCols)
                select RandomlySprinkleOnesIntoSolutionRows(solutionRows, randomRowIdxPerColumn);
        }

        private static List<List<int>> CombineSolutions(IEnumerable<List<List<int>>> partialSolutions)
        {
            return partialSolutions.SelectMany(partialSolution => partialSolution).ToList();
        }

        private static List<List<int>> RandomlySprinkleOnesIntoSolutionRows(List<List<int>> solutionRows, IEnumerable<int> randomRowIdxPerColumn)
        {
            var colIndex = 0;
            foreach (var randomRowIdx in randomRowIdxPerColumn) solutionRows[randomRowIdx][colIndex++] = 1;
            return solutionRows;
        }

        private static List<List<int>> PokeSolutionRowsIntoMatrix(
            List<List<int>> matrix,
            IReadOnlyList<List<int>> solutionRows,
            IEnumerable<int> randomRowIdxs)
        {
            var fromIdx = 0;
            foreach (var toIdx in randomRowIdxs) matrix[toIdx] = solutionRows[fromIdx++];
            return matrix;
        }

        private static bool NoneOfTheSolutionsOverlap(IReadOnlyList<List<List<int>>> solutions)
        {
            for (var i = 0; i < solutions.Count - 1; i++)
            {
                for (var j = i + 1; j < solutions.Count; j++)
                {
                    if (SolutionsOverlap(solutions[i], solutions[j]) || SolutionsOverlap(solutions[j], solutions[i]))
                    {
                        return false;
                    }
                }
            }

            return true;
        }

        private static bool SolutionsOverlap(IEnumerable<List<int>> solutionA, List<List<int>> solutionB)
        {
            foreach (var rowA in solutionA)
            {
                foreach (var rowIndex in Enumerable.Range(0, solutionB.Count))
                {
                    var copyOfSolution = new List<List<int>>(solutionB);
                    copyOfSolution[rowIndex] = rowA;
                    if (CheckSolution(copyOfSolution))
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        private static bool CheckSolution(IReadOnlyList<List<int>> solution)
        {
            var numCols = solution[0].Count;

            for (var colIndex = 0; colIndex < numCols; colIndex++)
            {
                var numOnesInColumn = solution.Count(row => row[colIndex] == 1);
                if (numOnesInColumn != 1) return false;
            }

            return true;
        }

        private static object DumpSolutions(IEnumerable<List<List<int>>> solutions)
        {
            var solutionIndex = 0;
            foreach (var solution in solutions) DumpSolution(solution, solutionIndex++);
            return null;
        }

        private static void DumpSolution(IEnumerable<List<int>> solution, int solutionIndex = -1)
        {
            if (solutionIndex >= 0) Console.WriteLine("Solution {0}", solutionIndex);
            var rowIndex = 0;
            foreach (var row in solution) DumpRow(row, rowIndex++);
        }

        private static void DumpRow(IEnumerable<int> row, int rowIndex)
        {
            Console.WriteLine("[{0}: [{1}]", rowIndex, string.Join(",", row.Select(x => Convert.ToString(x))));
        }
    }
}
