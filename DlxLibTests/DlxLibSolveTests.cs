using System;
using System.Collections.Generic;
using System.Linq;
using DlxLib;
using NUnit.Framework;

namespace DlxLibTests
{
    [TestFixture]
    public class DlxLibSolveTests
    {
        [Test]
        public void NullMatrixThrowsArgumentNullException()
        {
            var dlx = new Dlx();
            Assert.Throws<ArgumentNullException>(() => dlx.Solve<int>(null));
        }

        [Test]
        public void EmptyMatrixReturnsNoSolutions()
        {
            // Arrange
            var matrix = new bool[0, 0];
            var dlx = new Dlx();

            // Act
            var solutions = dlx.Solve(matrix);

            // Assert
            Assert.That(solutions.Count(), Is.EqualTo(0));
        }

        [Test]
        public void MatrixWithSingleRowOfAllOnesReturnsOneSolution()
        {
            // Arrange
            var matrix = new[,]
                {
                    {1, 1, 1}
                };
            var dlx = new Dlx();

            // Act
            var solutions = dlx.Solve(matrix).ToList();

            // Assert
            Assert.That(solutions, Has.Count.EqualTo(1));
            Assert.That(solutions[0].RowIndexes.Count(), Is.EqualTo(1));
            Assert.That(solutions.Select(s => s.RowIndexes), Contains.Item(new[] {0}));
        }

        [Test]
        public void MatrixWithSingleRowOfAllZerosReturnsNoSolutions()
        {
            // Arrange
            var matrix = new[,]
                {
                    {0, 0, 0}
                };
            var dlx = new Dlx();

            // Act
            var solutions = dlx.Solve(matrix).ToList();

            // Assert
            Assert.That(solutions, Has.Count.EqualTo(0));
        }

        [Test]
        public void SolveWithMatrixOfBool()
        {
            // Arrange
            var matrix = new[,]
                {
                    {true, false, false},
                    {false, true, false},
                    {false, false, true}
                };
            var dlx = new Dlx();

            // Act
            var solutions = dlx.Solve(matrix).ToList();

            // Assert
            Assert.That(solutions, Has.Count.EqualTo(1));
            Assert.That(solutions.Select(s => s.RowIndexes), Contains.Item(new[] { 0, 1, 2 }));
        }

        [Test]
        public void SolveWithMatrixOfInt()
        {
            // Arrange
            var matrix = new[,]
            {
                {1, 0, 0},
                {0, 1, 0},
                {0, 0, 1}
            };
            var dlx = new Dlx();

            // Act
            var solutions = dlx.Solve(matrix).ToList();

            // Assert
            Assert.That(solutions, Has.Count.EqualTo(1));
            Assert.That(solutions.Select(s => s.RowIndexes), Contains.Item(new[] {0, 1, 2}));
        }

        [Test]
        public void SolveWithMatrixOfCharAndCustomPredicate()
        {
            // Arrange
            var matrix = new[,]
                {
                    {'X', 'O', 'O'},
                    {'O', 'X', 'O'},
                    {'O', 'O', 'X'}
                };
            var dlx = new Dlx();

            // Act
            var solutions = dlx.Solve(matrix, c=> c == 'X').ToList();

            // Assert
            Assert.That(solutions, Has.Count.EqualTo(1));
            Assert.That(solutions.Select(s => s.RowIndexes), Contains.Item(new[] { 0, 1, 2 }));
        }

        [Test]
        public void MatrixFromTheOriginalDlxPaperReturnsTheCorrectSingleSolution()
        {
            // Arrange
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

            // Act
            var solutions = dlx.Solve(matrix).ToList();

            // Assert
            Assert.That(solutions, Has.Count.EqualTo(1));
            Assert.That(solutions.Select(s => s.RowIndexes), Contains.Item(new[] {0, 3, 4}));
        }

        [Test]
        public void MatrixWithThreeSolutionsReturnsTheCorrectThreeSolutions()
        {
            // Arrange
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

            // Act
            var solutions = dlx.Solve(matrix).ToList();

            // Assert
            Assert.That(solutions, Has.Count.EqualTo(3));
            Assert.That(solutions.Select(s => s.RowIndexes), Contains.Item(new[] {0, 3, 4}));
            Assert.That(solutions.Select(s => s.RowIndexes), Contains.Item(new[] {1, 2}));
            Assert.That(solutions.Select(s => s.RowIndexes), Contains.Item(new[] {2, 4, 5}));
        }

        [Test]
        public void CanGetTheFirstSolutionUsingFirst()
        {
            // Arrange
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

            // Act
            var firstSolution = dlx.Solve(matrix).First();

            // Assert
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
            var solutions = new Dlx().Solve(data, d => d, r => r.Item1).ToList();

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
            var solutions = new Dlx().Solve(data, d => d, r => r.Item1, c => c == 'X').ToList();

            // Assert
            Assert.That(solutions, Has.Count.EqualTo(1));
            Assert.That(solutions.Select(s => s.RowIndexes), Contains.Item(new[] { 0, 1, 2 }));
        }
    }
}
