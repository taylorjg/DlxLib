using System;
using System.Collections.Generic;
using System.Linq;
using DlxLib;
using NUnit.Framework;

namespace DlxLibTests
{
    [TestFixture]
    internal class DlxLibSolveTests
    {
        [Test]
        public void NullMatrixThrowsArgumentNullException()
        {
            var dlx = new Dlx();
            Assert.Throws<ArgumentNullException>(() => dlx.Solve(null));
        }

        [Test]
        public void EmptyMatrixReturnsNoSolutions()
        {
            var matrix = new bool[0, 0];
            var dlx = new Dlx();
            var solutions = dlx.Solve(matrix);
            Assert.That(solutions, Has.Count.EqualTo(0));
        }

        [Test]
        public void MatrixWithSingleRowOfAllOnesReturnsOneSolution()
        {
            var matrix = new[,]
                {
                    {1, 1, 1}
                };
            var dlx = new Dlx();
            var solutions = dlx.Solve(matrix).ToList();
            Assert.That(solutions, Has.Count.EqualTo(1));
            Assert.That(solutions[0].RowIndexes, Has.Count.EqualTo(1));
            Assert.That(solutions.Select(s => s.RowIndexes), Contains.Item(new[] {0}));
        }

        [Test]
        public void MatrixWithSingleRowOfAllZerosReturnsNoSolutions()
        {
            var matrix = new[,]
                {
                    {0, 0, 0}
                };
            var dlx = new Dlx();
            var solutions = dlx.Solve(matrix).ToList();
            Assert.That(solutions, Has.Count.EqualTo(0));
        }

        [Test]
        public void IdentityMatrixThreeByThreeReturnsOneSolution()
        {
            var matrix = new[,]
                {
                    {1, 0, 0},
                    {0, 1, 0},
                    {0, 0, 1}
                };
            var dlx = new Dlx();
            var solutions = dlx.Solve(matrix).ToList();
            Assert.That(solutions, Has.Count.EqualTo(1));
            Assert.That(solutions.Select(s => s.RowIndexes), Contains.Item(new[] {0, 1, 2}));
        }

        [Test]
        public void MatrixFromTheOriginalDlxPaperReturnsTheCorrectSingleSolution()
        {
            var matrix = new[,]
                {
                    {0, 0, 1, 0, 1, 1, 0},
                    {1, 0, 0, 1, 0, 0, 1},
                    {0, 1, 1, 0, 0, 1, 0},
                    {1, 0, 0, 1, 0, 0, 0},
                    {0, 1, 0, 0, 0, 0, 1},
                    {0, 0, 0, 1, 1, 0, 1}
                };
            var dlx = new Dlx();
            var solutions = dlx.Solve(matrix).ToList();
            Assert.That(solutions, Has.Count.EqualTo(1));
            Assert.That(solutions.Select(s => s.RowIndexes), Contains.Item(new[] {0, 3, 4}));
        }

        [Test]
        public void MatrixWithThreeSolutionsReturnsTheCorrectThreeSolutions()
        {
            var matrix = new[,]
                {
                    {1, 0, 0, 0},
                    {0, 1, 1, 0},
                    {1, 0, 0, 1},
                    {0, 0, 1, 1},
                    {0, 1, 0, 0},
                    {0, 0, 1, 0}
                };
            var dlx = new Dlx();
            var solutions = dlx.Solve(matrix).ToList();
            Assert.That(solutions, Has.Count.EqualTo(3));
            Assert.That(solutions.Select(s => s.RowIndexes), Contains.Item(new[] {0, 3, 4}));
            Assert.That(solutions.Select(s => s.RowIndexes), Contains.Item(new[] {1, 2}));
            Assert.That(solutions.Select(s => s.RowIndexes), Contains.Item(new[] {2, 4, 5}));
        }

        [Test]
        public void CallerShapedDataSimpleTest()
        {
            // Arrange
            var data = new List<Tuple<IList<int>, string>>
                {
                    Tuple.Create(new[] {1, 0, 0} as IList<int>, "Row 0"),
                    Tuple.Create(new[] {0, 1, 0} as IList<int>, "Row 1"),
                    Tuple.Create(new[] {0, 0, 1} as IList<int>, "Row 2")
                };

            // Act
            var dlx = new Dlx();
            var solutions = dlx.Solve<
                IList<Tuple<IList<int>, string>>,
                Tuple<IList<int>, string>,
                int>(
                    data,
                    (d, f) => { foreach (var r in d) f(r); },
                    (r, f) => { foreach (var c in r.Item1) f(c); },
                    c => c != 0);

            // Assert
            Assert.That(solutions, Has.Count.EqualTo(1));
            Assert.That(solutions.Select(s => s.RowIndexes), Contains.Item(new[] { 0, 1, 2 }));
        }
    }
}
