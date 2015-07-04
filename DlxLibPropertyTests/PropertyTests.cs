using System;
using System.Collections.Generic;
using System.Linq;
using DlxLib;
using FsCheck;
using NUnit.Framework;

namespace DlxLibPropertyTests
{
    [TestFixture]
    public class PropertyTests
    {
        private static readonly Config Config = Config.QuickThrowOnFailure;

        [Test]
        public void ExactCoverProblemsWithNoSolutionsTest()
        {
            Func<int, string> makeLabel = numSolutions => string.Format(
                "Expected no solutions but got {0}",
                numSolutions);

            var arbMatrix = Arb.From(GenMatrixOfIntWithNoSolutions());

            var property = Prop.ForAll(arbMatrix, matrix =>
            {
                var solutions = new Dlx().Solve(matrix).ToList();
                return (!solutions.Any()).Label(makeLabel(solutions.Count()));
            });

            Check.One(Config, property);
        }

        [Test]
        public void ExactCoverProblemsWithSingleSolutionTest()
        {
            Func<int, string> makeLabel = numSolutions => string.Format(
                "Expected exactly one solution but got {0}",
                numSolutions);

            var arbMatrix = Arb.From(GenMatrixOfIntWithSingleSolution());

            var property = Prop.ForAll(arbMatrix, matrix =>
            {
                var solutions = new Dlx().Solve(matrix).ToList();
                var p1 = (solutions.Count() == 1).Label(makeLabel(solutions.Count()));
                var p2 = CheckSolutions(solutions, matrix);
                return FsCheckUtils.And(p1, p2);
            });

            Check.One(Config, property);
        }

        [Test]
        public void ExactCoverProblemsWithMultipleSolutionsTest()
        {
            var arbNumSolutions = Arb.From(Gen.Choose(2, 5));

            var property = Prop.ForAll(arbNumSolutions, numSolutions =>
            {
                Func<int, string> makeLabel = actualNumSolutions => string.Format(
                    "Expected exactly {0} solutions but got {1}",
                    numSolutions, actualNumSolutions);

                var arbMatrix = Arb.From(GenMatrixOfIntWithMultipleSolutions(numSolutions));

                return Prop.ForAll(arbMatrix, matrix =>
                {
                    var solutions = new Dlx().Solve(matrix).ToList();
                    var actualNumSolutions = solutions.Count();
                    var p1 = (actualNumSolutions == numSolutions).Label(makeLabel(actualNumSolutions));
                    var p2 = CheckSolutions(solutions, matrix);
                    return FsCheckUtils.And(p1, p2);
                });
            });

            Check.One(Config, property);
        }

        private static Property CheckSolutions(IEnumerable<Solution> solutions, int[,] matrix)
        {
            return FsCheckUtils.And(solutions.Select(solution => CheckSolution(solution, matrix)));
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

                var p1 = (numOnes == 1).Label(makeLabel1(colIndex, numOnes));
                var p2 = (numZeros == expectedNumZerosPerColumn).Label(makeLabel2(colIndex, numZeros));

                colProperties.Add(p1);
                colProperties.Add(p2);
            }

            return FsCheckUtils.And(colProperties);
        }

        private static Gen<int[,]> GenMatrixOfIntWithNoSolutions()
        {
            return
                from numCols in Gen.Choose(2, 20)
                from numRows in Gen.Choose(2, 20)
                from indexOfAlwaysZeroColumn in Gen.Choose(0, numCols - 1)
                from rows in GenRowWithZeroInGivenColumn(numCols, indexOfAlwaysZeroColumn).ListOf(numRows)
                select rows.To2DArray();
        }

        private static Gen<int[,]> GenMatrixOfIntWithSingleSolution()
        {
            return
                from numCols in Gen.Choose(2, 20)
                from solution in GenSolution(numCols)
                from numRows in Gen.Choose(solution.Count, solution.Count * 5)
                from matrix in Gen.Constant(0).ListOf(numCols).ListOf(numRows)
                from randomRowIdxs in PickRandomRowIdxs(solution.Count, numRows)
                select PokeSolutionRowsIntoMatrix(matrix, solution, randomRowIdxs).To2DArray();
        }

        private static Gen<int[,]> GenMatrixOfIntWithMultipleSolutions(int numSolutions)
        {
            return
                from numCols in Gen.Choose(numSolutions, numSolutions * 10)
                from partitions in GenPartitions(numCols, numSolutions)
                from solutions in GenPartitionedSolutions(numCols, partitions)
                let combinedSolutions = CombineSolutions(solutions)
                from numRows in Gen.Choose(combinedSolutions.Count, combinedSolutions.Count * 5)
                from matrix in Gen.Constant(0).ListOf(numCols).ListOf(numRows)
                from randomRowIdxs in PickRandomRowIdxs(combinedSolutions.Count, numRows)
                select PokeSolutionRowsIntoMatrix(matrix, combinedSolutions, randomRowIdxs).To2DArray();
        }

        private static Gen<IList<int>> GenRowWithZeroInGivenColumn(int numCols, int indexOfAlwaysZeroColumn)
        {
            return
                from row in Gen.Choose(0, 1).ListOf(numCols).Select(r =>
                {
                    r[indexOfAlwaysZeroColumn] = 0;
                    return r;
                })
                select row;
        }

        private static Gen<IEnumerable<Tuple<int, int>>> GenPartitions(int numCols, int numSolutions)
        {
            return
                from partitionLengths in GenPartitionLengths(numCols, numSolutions)
                select MakePartitions(partitionLengths);
        }

        private static Gen<IEnumerable<int>> GenPartitionLengths(int numCols, int numSolutions)
        {
            return
                from partitionLengths in Gen.Choose(1, numCols / 2).ListOf(numSolutions - 1)
                let sum = partitionLengths.Sum()
                where sum < numCols
                select partitionLengths.Concat(new[] { numCols - sum });
        }

        private static IEnumerable<Tuple<int, int>> MakePartitions(IEnumerable<int> partitionLengths)
        {
            var currentStartIdx = 0;

            return partitionLengths.Select(partitionLength =>
            {
                var tuple = Tuple.Create(currentStartIdx, currentStartIdx + partitionLength);
                currentStartIdx += partitionLength;
                return tuple;
            });
        }

        private static Gen<IList<IList<IList<int>>>> GenPartitionedSolutions(int numCols, IEnumerable<Tuple<int, int>> partitions)
        {
            return Gen.Sequence(partitions.Select(partition =>
            {
                var startIdx = partition.Item1;
                var endIdx = partition.Item2;
                return GenPartitionedSolution(numCols, startIdx, endIdx);
            })).Select(x => x.ToList() as IList<IList<IList<int>>>);
        }

        private static Gen<IList<IList<int>>> GenSolution(int numCols)
        {
            return
                from numSolutionRows in Gen.Choose(1, Math.Min(5, numCols))
                from solutionRows in GenSolutionRows(numCols, numSolutionRows)
                select solutionRows;
        }

        private static Gen<IList<IList<int>>> GenSolutionRows(int numCols, int numSolutionRows)
        {
            return
                from solutionRows in Gen.Constant(0).ListOf(numCols).ListOf(numSolutionRows)
                from randomRowIdxs in Gen.Choose(0, numSolutionRows - 1).ListOf(numCols)
                select RandomlySprinkleOnesIntoSolutionRows(solutionRows, randomRowIdxs);
        }

        private static Gen<IList<IList<int>>> GenPartitionedSolution(int numCols, int startIdx, int endIdx)
        {
            return
                from numSolutionRows in Gen.Choose(1, Math.Min(5, endIdx - startIdx))
                from solutionRows in GenPartitionedSolutionRows(numCols, startIdx, endIdx, numSolutionRows)
                select solutionRows;
        }

        private static Gen<IList<IList<int>>> GenPartitionedSolutionRows(int numCols, int startIdx, int endIdx, int numSolutionRows)
        {
            return
                from solutionRows in Gen.Constant(InitPartitionedSolutionRows(numCols, startIdx, endIdx, numSolutionRows))
                from randomRowIdxs in Gen.Choose(0, numSolutionRows - 1).ListOf(endIdx - startIdx)
                where Enumerable.Range(0, numSolutionRows).All(randomRowIdxs.Contains)
                select RandomlySprinkleOnesIntoSolutionRows(solutionRows, randomRowIdxs, startIdx);
        }

        private static IList<IList<int>> InitPartitionedSolutionRows(int numCols, int startIdx, int endIdx, int numSolutionRows)
        {
            var firstRow = InitPartitionedSolutionFirstRow(numCols, startIdx, endIdx);
            var otherRows = InitPartitionedSolutionOtherRows(numCols, startIdx, endIdx, numSolutionRows - 1);
            var combinedRows = new List<IList<int>> {firstRow};
            combinedRows.AddRange(otherRows);
            return combinedRows;
        }

        private static IList<int> InitPartitionedSolutionFirstRow(int numCols, int startIdx, int endIdx)
        {
            return InitPartitionedSolutionRow(numCols, startIdx, endIdx, true);
        }

        private static IEnumerable<IList<int>> InitPartitionedSolutionOtherRows(int numCols, int startIdx, int endIdx, int numOtherRows)
        {
            return Enumerable.Range(0, numOtherRows)
                .Select(_ => InitPartitionedSolutionRow(numCols, startIdx, endIdx, false));
        }

        private static IList<int> InitPartitionedSolutionRow(int numCols, int startIdx, int endIdx, bool isFirstRow)
        {
            var fillerValue = (isFirstRow) ? 1 : 0;

            var prefixPartLength = startIdx;
            var randomPartLength = endIdx - startIdx;
            var suffixPartLength = numCols - endIdx;

            var prefixPart = Enumerable.Repeat(fillerValue, prefixPartLength);
            var randomPart = Enumerable.Repeat(0, randomPartLength);
            var suffixPart = Enumerable.Repeat(fillerValue, suffixPartLength);

            return prefixPart.Concat(randomPart).Concat(suffixPart).ToList();
        }

        private static IList<IList<int>> RandomlySprinkleOnesIntoSolutionRows(
            IList<IList<int>> solutionRows,
            IEnumerable<int> randomRowIdxs,
            int startIdx = 0)
        {
            var colIndex = startIdx;
            foreach (var randomRowIdx in randomRowIdxs) solutionRows[randomRowIdx][colIndex++] = 1;
            return solutionRows;
        }

        private static IList<IList<int>> CombineSolutions(IEnumerable<IList<IList<int>>> solutions)
        {
            return solutions.SelectMany(solution => solution).ToList();
        }

        private static Gen<IList<int>> PickRandomRowIdxs(int numSolutionsRows, int numRows)
        {
            return FsCheckUtils.PickValues(numSolutionsRows, Enumerable.Range(0, numRows));
        }

        private static IList<IList<int>> PokeSolutionRowsIntoMatrix(
            IList<IList<int>> matrix,
            IList<IList<int>> solutionRows,
            IEnumerable<int> randomRowIdxs)
        {
            var fromIdx = 0;
            foreach (var toIdx in randomRowIdxs) matrix[toIdx] = solutionRows[fromIdx++];
            return matrix;
        }
    }
}
