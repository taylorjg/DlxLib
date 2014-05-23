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
            Assert.That(solutions.Count(), Is.EqualTo(0));
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
        public void CanGetTheFirstSolutionUsingFirst()
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
            var numSolutionFoundEventsRaised = 0;
            dlx.SolutionFound += (_, __) => numSolutionFoundEventsRaised++;
            var firstSolution = dlx.Solve(matrix).First();
            Assert.That(firstSolution.RowIndexes, Is.EquivalentTo(new[] {0, 3, 4})
                                                    .Or
                                                    .EquivalentTo(new[] {1, 2})
                                                    .Or
                                                    .EquivalentTo(new[] {2, 4, 5}));
            Assert.That(numSolutionFoundEventsRaised, Is.EqualTo(1));
        }

        [Test]
        public void CallerShapedDataUsingDefaultPredicate()
        {
            // Arrange
            var data = new List<Tuple<int[], string>>
                {
                    Tuple.Create(new[] {1, 0, 0}, "Some data associated with row 0"),
                    Tuple.Create(new[] {0, 1, 0}, "Some data associated with row 1"),
                    Tuple.Create(new[] {0, 0, 1}, "Some data associated with row 2")
                };

            // Act
            var solutions = new Dlx().Solve<
                IList<Tuple<int[], string>>,
                Tuple<int[], string>,
                int>(
                    data,
                    (d, f) => { foreach (var r in d) f(r); },
                    (r, f) => { foreach (var c in r.Item1) f(c); }).ToList();

            // Assert
            Assert.That(solutions, Has.Count.EqualTo(1));
            Assert.That(solutions.Select(s => s.RowIndexes), Contains.Item(new[] { 0, 1, 2 }));
        }

        [Test]
        public void CallerShapedDataUsingCustomPredicate()
        {
            // Arrange
            var data = new List<Tuple<char[], string>>
                {
                    Tuple.Create(new[] {'X', 'O', 'O'}, "Some data associated with row 0"),
                    Tuple.Create(new[] {'O', 'X', 'O'}, "Some data associated with row 1"),
                    Tuple.Create(new[] {'O', 'O', 'X'}, "Some data associated with row 2")
                };

            // Act
            var solutions = new Dlx().Solve<
                IList<Tuple<char[], string>>,
                Tuple<char[], string>,
                char>(
                    data,
                    (d, f) => { foreach (var r in d) f(r); },
                    (r, f) => { foreach (var c in r.Item1) f(c); },
                    c => c == 'X').ToList();

            // Assert
            Assert.That(solutions, Has.Count.EqualTo(1));
            Assert.That(solutions.Select(s => s.RowIndexes), Contains.Item(new[] { 0, 1, 2 }));
        }
    }
}
