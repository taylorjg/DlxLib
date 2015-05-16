using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

using NUnit.Framework;

using DlxLib;

namespace DlxLibTests
{
    [TestFixture]
    public class DlxLibEnumerable2DArrayTests
    {
        public static void IsEqual2D<T>(T[,] array1, T[,] array2)
        {
            Assert.That(array1, Is.Not.Null);
            Assert.That(array2, Is.Not.Null);

            Assert.That(array1.GetLength(0), Is.EqualTo(array2.GetLength(0)));
            Assert.That(array1.GetLength(1), Is.EqualTo(array2.GetLength(1)));

            for (int r = 0; r < array1.GetLength(0); r++)
                for (int c = 0; c < array1.GetLength(1); c++)
                    Assert.That(array1[r, c], Is.EqualTo(array2[r, c]), "difference at {0}x{1}", r, c);

        }

        public static void IsEqual2D<T>(T[,] array, IEnumerable<IEnumerable<T>> nested)
        {
            Assert.That(array, Is.Not.Null);
            Assert.That(nested, Is.Not.Null);

            T[][] jagged = nested.Select(inner => inner.ToArray()).ToArray();

            // Check bounds
            int nRows = array.GetLength(0);
            int nCols = array.GetLength(1);

            Assert.That(jagged.Length, Is.EqualTo(nRows));
            Assert.IsTrue(jagged.Select(r => r.Length).All(l => nCols == l));

            // Now we know that the jagged version of nested is actually a 2D array with the correct bounds

            for (int r = 0; r < nRows; r++)
                for (int c = 0; c < nCols; c++)
                    Assert.That(array[r, c], Is.EqualTo(jagged[r][c]), "difference at {0}x{1}", r, c);
        }

        [Test]
        public void VerifyIsEqual2DOnMatch()
        {
            var left = new int[3, 4] { { 1, 2, 3, 4 }, { 10, 20, 30, 40 }, { 100, 200, 300, 400 } };
            var right = new List<List<int>>
            {
                new List<int>{ 1, 2, 3, 4 },
                new List<int>{ 10, 20, 30, 40},
                new List<int>{ 100, 200, 300, 400 }
            };
            IsEqual2D(left, right);
        }

        [Test]
        public void VerifyIsEqual2DMismatch()
        {
            var left = new int[3, 4] { { 1, 2, 3, 4 }, { 10, 20, 30, 40 }, { 100, 200, 300, 400 } };
            var right = new List<List<int>>
            {
                new List<int>{ 1, 2, 3, 4 },
                new List<int>{ 10, 20, 30, 40},
                new List<int>{ 100, -200, 300, 400 }
            };

            var ex = Assert.Throws<NUnit.Framework.AssertionException>(() => IsEqual2D(left, right));
            Assert.That(ex.Message, Is.StringContaining(@"difference at 2x1
  Expected: -200
  But was:  200"));
        }
    }
}
