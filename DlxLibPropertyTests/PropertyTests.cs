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

        //[FsCheck.NUnit.Property]
        //public Property ExactCoverProblemsWithSingleSolution()
        //{
        //    return Spec
        //        .For(GenMatrixOfIntWithSingleSolution(), matrix => new Dlx().Solve(matrix).Count() == 1)
        //        .Build();
        //}

        //[FsCheck.NUnit.Property]
        //public Property ExactCoverProblemsWithMultipleSolutions(int nParam)
        //{
        //    return Spec
        //        .For(Any.Value(nParam), GenMatrixOfIntWithMultipleSolutions(nParam), (n, matrix) => new Dlx().Solve(matrix).Count() == n)
        //        .When((n, _) => n > 1)
        //        .Build();
        //}

        private static Gen<int[,]> GenMatrixOfIntWithNoSolutions()
        {
            var zeroOrOne = Any.ValueIn(0, 1);

            return
                from numRows in Any.IntBetween(2, 100)
                from numCols in Any.IntBetween(2, 500)
                let allZeroes = Enumerable.Repeat(0, numCols).ToList()
                let genRowsA = Any.Value(allZeroes)
                from indexOfAlwaysZeroColumn in Any.IntBetween(0, numCols - 1)
                let prefixLength = indexOfAlwaysZeroColumn
                let suffixLength = numCols - indexOfAlwaysZeroColumn - 1
                let genRowsB =
                    from prefix in zeroOrOne.MakeListOfLength(prefixLength)
                    from suffix in zeroOrOne.MakeListOfLength(suffixLength)
                    select prefix.Concat(new[]{0}).Concat(suffix).ToList()
                let genRows = Any.WeighedGeneratorIn(
                    new WeightAndValue<Gen<List<int>>>(90, genRowsA),
                    new WeightAndValue<Gen<List<int>>>(10, genRowsB))
                from rows in genRows.MakeListOfLength(numRows)
                select rows.To2DArray();
        }

        //private static Gen<int[,]> GenMatrixOfIntWithSingleSolution()
        //{
        //    return Any.OfType<int[,]>();
        //}

        //private static Gen<int[,]> GenMatrixOfIntWithMultipleSolutions(int n)
        //{
        //    return Any.OfType<int[,]>();
        //}
    }
}
