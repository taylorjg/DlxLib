using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace DlxLib.EnumerableArrayAdapter
{
    internal static class EnumerableExtensions
    {
        public static IEnumerable<IEnumerable<T>> AsNestedEnumerables<T>(this T[,] array2D)
        {
            for (int r = 0; r < array2D.GetLength(0); r++)
            {
                yield return array2D.RowAsEnumerable(r);
            }
        }

        public static IEnumerable<T> RowAsEnumerable<T>(this T[,] array2D, int r)
        {
            for (int c = 0; c < array2D.GetLength(1); c++)
            {
                yield return array2D[r, c];
            }
        }
    }
}
