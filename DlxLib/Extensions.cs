using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace DlxLib
{
    /// <summary>
    /// Useful (utility) extension methods (not particularly related to DLX/Dancing Links).
    /// </summary>
    public static class Extensions
    {
        /// <summary>
        /// Returns minimum object of a sequence, where the minimum is determined
        /// by keys derived from the objects.
        /// </summary>
        /// <remarks>
        /// I'm not sure why LINQ is missing this overload of Enumerable.Min.  It has
        /// a Min that projects each element of the sequence to a value, but then it
        /// returns the minimum value, not the minimum element.
        ///
        /// Unfortunately, making the comparer argument optional (to be filled in by
        /// Comparer{TSource}.Default, leads to the use of existing Min() overloads
        /// being chosen over this one.
        /// </remarks>
        public static TSource Min<TSource,TValue>(this IEnumerable<TSource> source, Func<TSource,TValue> valueSelector, IComparer<TValue> comparer)
        {
            if (null == comparer)
                comparer = Comparer<TValue>.Default;

            return source.Aggregate((prev, next) => {
                var pKey = valueSelector(prev);
                var nKey = valueSelector(next);
                return (0 >= comparer.Compare(pKey, nKey)) ? prev : next;
            });
        }

        /// <summary>
        /// Sort of like Array.ForEach but a) functional, and b) for 2D arrays: Return
        /// a new array where each element is transformed, by parameter func, from
        /// the old array.
        /// </summary>
        public static U[,] ForEach<T,U>(this T[,] array, Func<T,U> func)
        {
            var result = new U[array.GetLength(0), array.GetLength(1)];
            for (int i = 0; i < array.GetLength(0); i++)
                for (int j = 0; j < array.GetLength(1); j++)
                    result[i, j] = func(array[i, j]);
            return result;
        }

        /// <summary>
        /// Invoke a 2-parameter delegate without having to check first to see if
        /// it is null.  Used for firing events.
        /// </summary>
        /// <remarks>
        /// I've never understood why there wasn't a built-in way to invoke through
        /// a delegate that absorbed the null check.
        /// </remarks>
        public static void Invoke<T1,T2>(this Action<T1,T2> action, T1 p1, T2 p2)
        {
            if (null != action)
                action(p1, p2);
        }
    }
}
