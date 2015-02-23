using System;
using System.Collections.Generic;
using System.Linq;
using DlxLib;
using FsCheck;
using FsCheck.Fluent;
using FsCheckUtils;
using NUnit.Framework;

namespace DlxLibPropertyTests
{
    using Property = Gen<Rose<Result>>;
    
    [TestFixture]
    public class PropertyTests
    {
        [FsCheck.NUnit.Property(Verbose = false)]
        public Property ExactCoverProblemsWithNoSolutions()
        {
            return Spec
                .For(GenMatrixOfIntWithNoSolutions(), matrix => !new Dlx().Solve(matrix).Any())
                .Build();
        }

        [FsCheck.NUnit.Property(Verbose = false)]
        public Property ExactCoverProblemsWithSingleSolution()
        {
            return Spec
                .For(GenMatrixOfIntWithSingleSolution(), matrix =>
                {
                    var solutions = new Dlx().Solve(matrix).ToList();
                    return solutions.Count == 1 && CheckSolution(solutions.First(), matrix);
                })
                .Build();
        }

        //[FsCheck.NUnit.Property(Verbose = false)]
        //public Property ExactCoverProblemsWithMultipleSolutions(int nParam)
        //{
        //    return Spec
        //        .For(Any.Value(nParam), GenMatrixOfIntWithMultipleSolutions(nParam), (n, matrix) => new Dlx().Solve(matrix).Count() == n)
        //        .When((n, _) => n > 1)
        //        .Build();
        //}

        private static bool CheckSolution(Solution solution, int[,] matrix)
        {
            var numCols = matrix.GetLength(1);
            var numOnes = 0;
            var numZeros = 0;

            foreach (var rowIndex in solution.RowIndexes)
            {
                for (var colIndex = 0; colIndex < numCols; colIndex++)
                {
                    if (matrix[rowIndex, colIndex] == 1) numOnes++;
                    if (matrix[rowIndex, colIndex] == 0) numZeros++;
                }
            }

            return (numOnes == numCols) && (numOnes + numZeros == numCols * solution.RowIndexes.Count());
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

        //private static Gen<int[,]> GenMatrixOfIntWithMultipleSolutions(int n)
        //{
        //    return Any.OfType<int[,]>();
        //}

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

        private static Gen<List<List<int>>> GenPartialSolutionRows(int numCols, int numPieces)
        {
            return
                from pieceLengths in GenPieceLengths(numCols, numPieces)
                from pieceIndexLists in GenPieceIndexLists(numCols, pieceLengths)
                select pieceIndexLists.Select(selectedIdxs => MakePartialSolutionRow(numCols, selectedIdxs)).ToList();
        }

        private static Gen<List<List<int>>> GenPieceIndexLists(int numCols, IEnumerable<int> pieceLengths)
        {
            var remainingIdxs = Enumerable.Range(0, numCols).ToList();
            // TODO: Could we do this with a call to pieceLengths.Aggregate() instead ?
            return GenPieceIndexListsRecursiveHelper(pieceLengths.ToList(), remainingIdxs);
        }

        private static Gen<List<List<int>>> GenPieceIndexListsRecursiveHelper(IList<int> remainingPieceLengths, IList<int> remainingIdxs)
        {
            if (!remainingPieceLengths.Any()) return Any.Value(new List<List<int>>());

            var pieceLength = remainingPieceLengths.First();

            return
                from selectedIdxs in GenExtensions.PickValues(pieceLength, remainingIdxs.ToArray())
                let newRemainingIdxs = RemoveSelectedIdxs(remainingIdxs, selectedIdxs).ToList()
                let head = new[] { selectedIdxs }.ToList()
                from tail in GenPieceIndexListsRecursiveHelper(remainingPieceLengths.Skip(1).ToList(), newRemainingIdxs)
                select head.Concat(tail).ToList();
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
