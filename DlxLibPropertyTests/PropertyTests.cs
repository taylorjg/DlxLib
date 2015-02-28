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

        [Test]
        public void Test()
        {
            var arb = Arb.fromGen(GenMatrixOfIntWithMultipleSolutions(2));
            var property = Prop.forAll(arb, FSharpFunc<int[,], Property>.FromConverter(matrix => Prop.ofTestable(true)));
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
                from randomPartLengthOld in Any.IntBetween(1, 5)
                let randomPartLength = 4
                let numCols = randomPartLength*numSolutions
                let partitions =
                    Enumerable.Range(0, numSolutions)
                        .Select(x => Tuple.Create(randomPartLength*x, randomPartLength*(x + 1)))
                from solutions in GenPartitionedSolutions(numCols, partitions)
                let combinedSolutions = CombineSolutions(solutions)
                //from numRows in Any.IntBetween(combinedSolutions.Count, combinedSolutions.Count * 2)
                //from matrix in Any.Value(0).MakeListOfLength(numCols).MakeListOfLength(numRows)
                //from randomRowIdxs in GenExtensions.PickValues(combinedSolutions.Count, Enumerable.Range(0, numRows))
                //select PokeSolutionRowsIntoMatrix(matrix, combinedSolutions, randomRowIdxs).To2DArray();
                select combinedSolutions.To2DArray();
        }

        //private static Gen<List<List<int>>> GenSolution(int numCols)
        //{
        //    return GenPartitionedSolution(numCols, 0, numCols);
        //}

        private static Gen<List<List<int>>> GenSolutionRows(int numCols, int numSolutionRows)
        {
            return GenPartitionedSolutionRows(numCols, 0, numCols, numSolutionRows);
        }

        private static Gen<List<List<List<int>>>> GenPartitionedSolutions(int numCols, IEnumerable<Tuple<int, int>> partitions)
        {
            var seed = Any.Value(Enumerable.Empty<List<List<int>>>());
            var v1 = partitions.Aggregate(seed, (acc, partition) =>
            {
                var startIdx = partition.Item1;
                var endIdx = partition.Item2;

                return (
                    from list in acc
                    from v3 in GenPartitionedSolution(numCols, startIdx, endIdx)
                    let _ = Dump("Inside Aggregate func - numCols: {0}; startIdx: {1}; endIdx: {2}", numCols, startIdx, endIdx)
                    select list.Concat(new[] {v3}));
            });
            return v1.Select(x => x.ToList());
        }

        private static Gen<List<List<int>>> GenPartitionedSolution(int numCols, int startIdx, int endIdx)
        {
            return
                from numSolutionRowsOld in Any.IntBetween(1, Math.Min(5, endIdx - startIdx))
                let numSolutionRows = 2
                from solutionRows in GenPartitionedSolutionRows(numCols, startIdx, endIdx, numSolutionRows)
                select solutionRows;
        }

        private static Gen<List<List<int>>> GenPartitionedSolutionRows(int numCols, int startIdx, int endIdx, int numSolutionRows)
        {
            var prefixPartLength = startIdx;
            var randomPartLength = endIdx - startIdx;
            var suffixPartLength = numCols - endIdx;

            return
                from prefixPart in Any.Value(1).MakeListOfLength(prefixPartLength)
                from randomPart in Any.Value(0).MakeListOfLength(randomPartLength)
                from suffixPart in Any.Value(1).MakeListOfLength(suffixPartLength)
                from firstSolutionRow in GenRow(numCols, startIdx, endIdx, true)
                from otherSolutionRows in GenRow(numCols, startIdx, endIdx, false).MakeListOfLength(numSolutionRows - 1)
                let solutionRows = new[] {firstSolutionRow}.Concat(otherSolutionRows).ToList()
                from randomRowIdxPerColumn in Any.IntBetween(0, numSolutionRows - 1).MakeListOfLength(randomPartLength)
                select RandomlySprinkleOnesIntoSolutionRows(solutionRows, startIdx, randomRowIdxPerColumn);
        }

        private static Gen<List<int>> GenRow(int numCols, int startIdx, int endIdx, bool isFirstRow)
        {
            var fillerValue = (isFirstRow) ? 1 : 0;
            var prefixPartLength = startIdx;
            var randomPartLength = endIdx - startIdx;
            var suffixPartLength = numCols - endIdx;

            return
                from prefixPart in Any.Value(fillerValue).MakeListOfLength(prefixPartLength)
                from randomPart in Any.Value(0).MakeListOfLength(randomPartLength)
                from suffixPart in Any.Value(fillerValue).MakeListOfLength(suffixPartLength)
                let row = prefixPart.Concat(randomPart).Concat(suffixPart).ToList()
                let _ = DumpSequence(row) 
                select row;
        }

        private static List<List<int>> RandomlySprinkleOnesIntoSolutionRows(List<List<int>> solutionRows, int startIdx, IEnumerable<int> randomRowIdxPerColumn)
        {
            var colIndex = startIdx;
            foreach (var randomRowIdx in randomRowIdxPerColumn) solutionRows[randomRowIdx][colIndex++] = 1;
            return solutionRows;
        }

        private static List<List<int>> CombineSolutions(IEnumerable<List<List<int>>> solutions)
        {
            return solutions.SelectMany(solution => solution).ToList();
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

        private static object DumpSequence(IEnumerable<int> sequence)
        {
            return Dump("[" + string.Join(",", sequence.Select(x => Convert.ToString(x))) + "]");
        }

        private static object Dump(string format, params object[] args)
        {
            Console.WriteLine(format, args);
            return null;
        }
    }
}
