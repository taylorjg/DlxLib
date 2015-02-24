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

        [FsCheck.NUnit.Property(Verbose = false)]
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
            var arb = Arb.fromGen(GenMatrixOfIntWithSingleSolution());
            var property = Prop.forAll(arb, FSharpFunc<int[,], Property>.FromConverter(matrix =>
            {
                var solutions = new Dlx().Solve(matrix).ToList();
                var p1 = PropExtensions.Label(solutions.Count() == 1, "Expected exactly one solution");
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

            Func<int, int, string> label1 = (colIndex, numOnes) => string.Format(
                "Expected column {0} to contain a single 1 but it contains {1}",
                colIndex,
                numOnes);

            Func<int, int, string> label2 = (colIndex, numZeros) => string.Format(
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

                var p1 = PropExtensions.Label(numOnes == 1, label1(colIndex, numOnes));
                var p2 = PropExtensions.Label(numZeros == expectedNumZerosPerColumn, label2(colIndex, numZeros));

                colProperties.Add(PropExtensions.And(p1, p2));
            }

            return PropExtensions.AndAll(colProperties.ToArray());
        }

        private static Gen<int[,]> GenMatrixOfIntWithNoSolutions()
        {
            return
                from numCols in Any.IntBetween(2, 100)
                from numRows in Any.IntBetween(2, 200)
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
                from numPieces in Any.IntBetween(1, Math.Min(5, numCols))
                from numRows in Any.IntBetween(numPieces, 20)
                from partialSolutionRows in GenPartialSolutionRows(numCols, numPieces)
                let genZeroRow = Any.Value(Enumerable.Repeat(0, numCols).ToList())
                from zeroRows in genZeroRow.MakeListOfLength(numRows)
                from overrideIdxs in GenExtensions.PickValues(numPieces, Enumerable.Range(0, numRows))
                select OverrideSomeRows(zeroRows, partialSolutionRows, overrideIdxs).To2DArray();
        }

        private static Gen<List<List<int>>> GenPartialSolutionRows(int numCols, int numPieces)
        {
            return
                from pieceLengths in GenPieceLengths(numCols, numPieces)
                from pieceIndexLists in GenPieceIndexLists(numCols, pieceLengths)
                select pieceIndexLists.Select(selectedIdxs => MakePartialSolutionRow(numCols, selectedIdxs)).ToList();
        }

        private static List<List<int>> OverrideSomeRows(List<List<int>> rows, IReadOnlyList<List<int>> overrideRows, IEnumerable<int> overrideIdxs)
        {
            var fromIdx = 0;
            foreach (var toIdx in overrideIdxs) rows[toIdx] = overrideRows[fromIdx++];
            return rows;
        }

        private static Gen<List<int>> GenPieceLengths(int numCols, int numPieces)
        {
            if (numPieces == 1) return Any.Value(new[] { numCols }.ToList());

            return
                from x in Any.IntBetween(1, numCols / 2).MakeListOfLength(numPieces - 1)
                let sum = x.Sum()
                where sum < numCols
                select x.Concat(new[] { numCols - sum }).ToList();
        }

        private static Gen<List<List<int>>> GenPieceIndexLists(int numCols, IEnumerable<int> pieceLengths)
        {
            var initialPieceIndexLists = Enumerable.Empty<List<int>>();
            var initialIdxs = Enumerable.Range(0, numCols);
            var initialTuple = Tuple.Create(initialPieceIndexLists, initialIdxs);
            var seed = Any.Value(initialTuple);

            return pieceLengths
                .Aggregate(seed, (acc, pieceLength) => (
                    from tuple in acc
                    let listSoFar = tuple.Item1
                    let remainingIdxs = tuple.Item2
                    from selectedIdxs in GenExtensions.PickValues(pieceLength, remainingIdxs)
                    let newRemainingIdxs = RemoveSelectedIdxs(remainingIdxs, selectedIdxs)
                    select Tuple.Create(listSoFar.Concat(new[] {selectedIdxs}), newRemainingIdxs)))
                .Select(tuple => tuple.Item1.ToList());
        }

        private static List<int> MakePartialSolutionRow(int numCols, IEnumerable<int> selectedIdxs)
        {
            var partialSolutionRow = Enumerable.Repeat(0, numCols).ToList();
            foreach (var selectedIdx in selectedIdxs) partialSolutionRow[selectedIdx] = 1;
            return partialSolutionRow;
        }

        private static IEnumerable<int> RemoveSelectedIdxs(IEnumerable<int> remainingIdxs, IEnumerable<int> selectedIdxs)
        {
            return remainingIdxs.Except(selectedIdxs);
        }
    }
}
