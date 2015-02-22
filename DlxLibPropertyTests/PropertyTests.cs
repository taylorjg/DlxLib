using System;
using System.Collections.Generic;
using System.Linq;
using DlxLib;
using FsCheck;
using FsCheck.Fluent;
using NUnit.Framework;

namespace DlxLibPropertyTests
{
    using Property = Gen<Rose<Result>>;
    
    [TestFixture]
    public class PropertyTests
    {
        [FsCheck.NUnit.Property]
        public Property ExactCoverProblemsWithNoSolutions()
        {
            return Spec
                .For(GenMatrixOfIntWithNoSolutions(), matrix => !new Dlx().Solve(matrix).Any())
                .Build();
        }

        [FsCheck.NUnit.Property(Verbose = true, MaxTest = 10)]
        public Property ExactCoverProblemsWithSingleSolution()
        {
            return Spec
                .For(GenMatrixOfIntWithSingleSolution(), matrix => new Dlx().Solve(matrix).Count() == 1)
                .Build();
        }

        [FsCheck.NUnit.Property(Verbose = true, MaxTest = 10)]
        public Property PieceLengthsTest()
        {
            var gen = GenPieceLengths(18, 3);
            var samples = Gen.sample(10, 10, gen);
            foreach (var sample in samples) Console.WriteLine("sample: {0}", SequenceToString(sample));
            return Prop.ofTestable(true);
        }

        //[FsCheck.NUnit.Property]
        //public Property ExactCoverProblemsWithMultipleSolutions(int nParam)
        //{
        //    return Spec
        //        .For(Any.Value(nParam), GenMatrixOfIntWithMultipleSolutions(nParam), (n, matrix) => new Dlx().Solve(matrix).Count() == n)
        //        .When((n, _) => n > 1)
        //        .Build();
        //}

        // TODO: add a helper method to check each solution...

        private static Gen<int[,]> GenMatrixOfIntWithNoSolutions()
        {
            // TODO: It would be easier to generate random rows and then nuke an entire column of the matrix to 0s

            var genZeroOrOne = Any.ValueIn(0, 1);
            var zeroList = new[] {0}.ToList();

            return
                from numCols in Any.IntBetween(2, 100)
                from numRows in Any.IntBetween(2, 500)
                let rowOfAllZeros = Enumerable.Repeat(0, numCols).ToList()
                let genRowsA = Any.Value(rowOfAllZeros)
                from indexOfAlwaysZeroColumn in Any.IntBetween(0, numCols - 1)
                let prefixLength = indexOfAlwaysZeroColumn
                let suffixLength = numCols - indexOfAlwaysZeroColumn - 1
                let genRowsB =
                    from prefixList in genZeroOrOne.MakeListOfLength(prefixLength)
                    from suffixList in genZeroOrOne.MakeListOfLength(suffixLength)
                    select prefixList.Concat(zeroList).Concat(suffixList).ToList()
                let genRows = Any.WeighedGeneratorIn(
                    new WeightAndValue<Gen<List<int>>>(90, genRowsA),
                    new WeightAndValue<Gen<List<int>>>(10, genRowsB))
                from rows in genRows.MakeListOfLength(numRows)
                select rows.To2DArray();
        }

        private static Gen<int[,]> GenMatrixOfIntWithSingleSolution()
        {
            return
                //from numCols in Any.IntBetween(2, 100)
                from numCols in Any.IntBetween(2, 20)
                //from numPieces in Any.IntBetween(1, Math.Min(5, numCols))
                from numPieces in Any.IntBetween(1, Math.Min(2, numCols))
                from partialSolutionRows in GenPartialSolutionRows(numCols, numPieces)
                // TODO: need to generate a mix of 90% rows of all 0s, 10% partial solution rows
                select partialSolutionRows.To2DArray();
        }

        // TODO: add an FsCheckUtils method to shuffle a list of items...

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
            // how long should each piece be ? random length
            // e.g. numCols = 10, numPieces = 3
            // choose random lengths
            // e.g. p1 = length 3, p2 = length 2, p3 = remaining length e.g. 10 - 3 - 2 = 5
            // could keep choosing a random value between 1 and remaining length/2
            // e.g. initially (1,10/2) then (1,7/2) then remainder
            // start with set of all available idxs e.g. [0,1,2,3,4,5,6,7,8,9]
            // p1 = pick 3 e.g. [0,1,2] leaving [3,4,5,6,7,8,9]
            // p2 = pick 2 e.g. [3,4] leaving [5,6,7,8,9]
            // p3 = last piece is the remainder e.g. [5,6,7,8,9]

            return null;
        }

        private static object Dump(string format, params object[] args)
        {
            Console.WriteLine(format, args);
            return null;
        }

        private static string SequenceToString<T>(IEnumerable<T> xs)
        {
            return "[" + string.Join(",", xs.Select(x => Convert.ToString(x))) + "]";
        }

        private static List<int> MakePartialSolutionRow(int numCols, IList<int> remainingIdxs, IList<int> selectedIdxs)
        {
            var result = Enumerable.Repeat(0, numCols).ToList();
            foreach (var idx in selectedIdxs) result[idx] = 1;
            InPlaceRemoveSelectedIdxs(remainingIdxs, selectedIdxs);
            return result;
        }

        //private static IEnumerable<int> RemoveSelectedIdxs(IEnumerable<int> remainingIdxs, IEnumerable<int> selectedIdxs)
        //{
        //    return remainingIdxs.Except(selectedIdxs);
        //}

        private static void InPlaceRemoveSelectedIdxs(ICollection<int> remainingIdxs, IEnumerable<int> selectedIdxs)
        {
            foreach (var selectedIdx in selectedIdxs) remainingIdxs.Remove(selectedIdx);
        }

        //private static Gen<int[,]> GenMatrixOfIntWithMultipleSolutions(int n)
        //{
        //    return Any.OfType<int[,]>();
        //}
    }
}
