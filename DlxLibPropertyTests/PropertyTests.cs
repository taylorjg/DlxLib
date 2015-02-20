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
            return
                from numRows in Any.IntBetween(20, 100)
                from numCols in Any.IntBetween(4, 100)
                let genRowsA = Any.Value(Enumerable.Repeat(0, numCols).ToArray())
                let genRowsB =
                    from numOnes in Any.IntBetween(1, numCols - 1)
                    select Enumerable.Repeat(1, numOnes).Concat(Enumerable.Repeat(0, numCols - numOnes)).ToArray()
                let genRows = Any.WeighedGeneratorIn(
                    new WeightAndValue<Gen<int[]>>(90, genRowsA),
                    new WeightAndValue<Gen<int[]>>(10, genRowsB))
                from rows in genRows.MakeListOfLength(numRows).Select(x => x.ToArray())
                select JaggedArrayTo2DArray(rows);
        }

        //private static Gen<int[,]> GenMatrixOfIntWithSingleSolution()
        //{
        //    return Any.OfType<int[,]>();
        //}

        //private static Gen<int[,]> GenMatrixOfIntWithMultipleSolutions(int n)
        //{
        //    return Any.OfType<int[,]>();
        //}

        private static T[,] JaggedArrayTo2DArray<T>(T[][] jaggedArray)
        {
            var numRows = jaggedArray.Length;
            var numCols = jaggedArray[0].Length;

            var twoDimensionalArray = new T[numRows, numCols];

            for (var row = 0; row < numRows; row++)
            {
                for (var col = 0; col < numCols; col++)
                {
                    twoDimensionalArray[row, col] = jaggedArray[row][col];
                }
            }

            return twoDimensionalArray;
        }
    }
}
