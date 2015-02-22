﻿using System;
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
        [FsCheck.NUnit.Property]
        public Property ExactCoverProblemsWithNoSolutions()
        {
            return Spec
                .For(GenMatrixOfIntWithNoSolutions(), matrix => !new Dlx().Solve(matrix).Any())
                .Build();
        }

        //[FsCheck.NUnit.Property(Verbose = true, MaxTest = 10)]
        //public Property ExactCoverProblemsWithSingleSolution()
        //{
        //    return Spec
        //        .For(GenMatrixOfIntWithSingleSolution(), matrix => new Dlx().Solve(matrix).Count() == 1)
        //        .Build();
        //}

        [FsCheck.NUnit.Property(Verbose = true, MaxTest = 10)]
        public Property GenPieceLengthsSample()
        {
            var gen = GenPieceLengths(18, 3);
            var samples = Gen.sample(10, 10, gen);
            foreach (var sample in samples) Dump("GenPieceLengths sample: {0}", SequenceToString(sample));
            return Prop.ofTestable(true);
        }

        [FsCheck.NUnit.Property(Verbose = true)]
        public Property GenPartialSolutionRowsSample()
        {
            var gen = GenPartialSolutionRows(18, 5);
            var samples = Gen.sample(10, 10, gen);

            var sampleIndex = 0;
            foreach (var sample in samples)
            {
                var rowIndex = 0;
                foreach (var row in sample)
                {
                    Dump("sampleIndex: {0}; rowIndex: {1}; sample: {2}", sampleIndex, rowIndex, SequenceToString(row));
                    rowIndex++;
                }
                sampleIndex++;
            }

            return Prop.ofTestable(true);
        }

        [FsCheck.NUnit.Property(Verbose = true)]
        public Property GenMatrixOfIntWithSingleSolutionSample()
        {
            var gen = GenMatrixOfIntWithSingleSolution();
            var samples = Gen.sample(10, 10, gen);

            var sampleIndex = 0;
            foreach (var sample in samples)
            {
                for (var rowIndex = 0; rowIndex < sample.GetLength(0); rowIndex++)
                {
                    var rowValues = new List<int>();
                    for (var colIndex = 0; colIndex < sample.GetLength(1); colIndex++)
                    {
                        rowValues.Add(sample[rowIndex, colIndex]);
                    }
                    Dump("sampleIndex: {0}; row[{1}]: {2}", sampleIndex, rowIndex, SequenceToString(rowValues));
                }
                Dump("\n");
                sampleIndex++;
            }

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
                from numPieces in Any.IntBetween(1, Math.Min(5, numCols))
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
            return
                from pieceLengths in GenPieceLengths(numCols, numPieces)
                from pieceIndexLists in GenPieceIndexLists(numCols, pieceLengths)
                select pieceIndexLists.Select(selectedIdxs => MakePartialSolutionRow(numCols, selectedIdxs)).ToList();
        }

        private static Gen<List<List<int>>> RecursiveHelper(
            IList<int> remainingPieceLengths,
            int[] remainingIdxs)
        {
            if (!remainingPieceLengths.Any()) return Any.Value(new List<List<int>>());

            var pieceLength = remainingPieceLengths.First();

            return
                from selectedIdxs in GenExtensions.PickValues(pieceLength, remainingIdxs)
                let newRemainingIdxs = RemoveSelectedIdxs(remainingIdxs, selectedIdxs).ToArray()
                from rest in RecursiveHelper(remainingPieceLengths.Skip(1).ToList(), newRemainingIdxs)
                let x = new[] {selectedIdxs}.ToList()
                select x.Concat(rest).ToList();
        }

        private static Gen<List<List<int>>> GenPieceIndexLists(int numCols, IEnumerable<int> pieceLengths)
        {
            var remainingIdxs = Enumerable.Range(0, numCols).ToArray();
            return RecursiveHelper(pieceLengths.ToList(), remainingIdxs);
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

        //private static Gen<int[,]> GenMatrixOfIntWithMultipleSolutions(int n)
        //{
        //    return Any.OfType<int[,]>();
        //}

        private static void Dump(string format, params object[] args)
        {
            Console.WriteLine(format, args);
        }

        private static string SequenceToString<T>(IEnumerable<T> xs)
        {
            return "[" + string.Join(",", xs.Select(x => Convert.ToString(x))) + "]";
        }
    }
}
