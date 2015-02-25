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

        [FsCheck.NUnit.Property(Verbose = true)]
        public Property ExactCoverProblemsWithNoSolutionsProperty()
        {
            return Spec
                .For(GenMatrixOfIntWithNoSolutions(), matrix => !new Dlx().Solve(matrix).Any())
                .Shrink(_ => Enumerable.Empty<int[,]>())
                .Build();
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
                var p2 = CheckSolution(solutions.First(), matrix);
                return PropExtensions.And(p1, p2);
            }));
            Check.One(Config, property);
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
                from numPartialSolutionRows in Any.IntBetween(1, Math.Min(5, numCols))
                from numRows in Any.IntBetween(numPartialSolutionRows, 20)
                from matrix in Any.Value(0).MakeListOfLength(numCols).MakeListOfLength(numRows)
                // TODO: We could get GenPartialSolutionRows to poke the 1s into the matrix directly
                // - currently, we generate partial solutions rows separately and then poke them into the matrix
                from randomRowIdxs in GenExtensions.PickValues(numPartialSolutionRows, Enumerable.Range(0, numRows))
                from partialSolutionRows in GenPartialSolutionRows(numCols, numPartialSolutionRows)
                select PokePartialSolutionRowsIntoMatrix(matrix, partialSolutionRows, randomRowIdxs).To2DArray();
        }

        private static Gen<List<List<int>>> GenPartialSolutionRows(int numCols, int numPartialSolutionRows)
        {
            return
                from partialSolutionRows in Any.Value(0).MakeListOfLength(numCols).MakeListOfLength(numPartialSolutionRows)
                from randomRowIdxs in Any.IntBetween(0, numPartialSolutionRows - 1).MakeListOfLength(numCols)
                select RandomlySprinkleOnesIntoPartialSolutionRows(partialSolutionRows, randomRowIdxs);
        }

        private static List<List<int>> RandomlySprinkleOnesIntoPartialSolutionRows(List<List<int>> partialSolutionRows, IEnumerable<int> randomRowIdxs)
        {
            var col = 0;
            foreach (var randomRowIdx in randomRowIdxs) partialSolutionRows[randomRowIdx][col++] = 1;
            return partialSolutionRows;
        }

        private static List<List<int>> PokePartialSolutionRowsIntoMatrix(
            List<List<int>> matrix,
            IReadOnlyList<List<int>> partialSolutionRows,
            IEnumerable<int> randomRowIdxs)
        {
            var fromIdx = 0;
            foreach (var toIdx in randomRowIdxs) matrix[toIdx] = partialSolutionRows[fromIdx++];
            return matrix;
        }
    }
}
