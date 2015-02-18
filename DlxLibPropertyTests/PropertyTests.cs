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
                .For(GenMatrixWithNoSolutions(Any.OfType<int>()), matrix => !new Dlx().Solve(matrix).Any())
                .Build();
        }

        [FsCheck.NUnit.Property]
        public Property ExactCoverProblemsWithSingleSolution()
        {
            return Spec
                .For(GenMatrixWithSingleSolution(Any.OfType<int>()), matrix => new Dlx().Solve(matrix).Count() == 1)
                .Build();
        }

        [FsCheck.NUnit.Property]
        public Property ExactCoverProblemsWithMultipleSolutions(int nParam)
        {
            return Spec
                .For(Any.Value(nParam), GenMatrixWithMultipleSolutions(Any.OfType<int>(), nParam), (n, matrix) => new Dlx().Solve(matrix).Count() == n)
                .When((n, _) => n > 1)
                .Build();
        }

        private static Gen<T[,]> GenMatrixWithNoSolutions<T>(Gen<T> genT)
        {
            return Any.OfType<T[,]>();
        }
        private static Gen<T[,]> GenMatrixWithSingleSolution<T>(Gen<T> genT)
        {
            return Any.OfType<T[,]>();
        }

        private static Gen<T[,]> GenMatrixWithMultipleSolutions<T>(Gen<T> genT, int n)
        {
            return Any.OfType<T[,]>();
        }
    }
}
