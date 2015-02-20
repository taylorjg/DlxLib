using System.Collections.Generic;

namespace DlxLibPropertyTests
{
    public static class JaggedArrayExtensions
    {
        public static T[,] To2DArray<T>(this IReadOnlyList<IReadOnlyList<T>> jaggedArray)
        {
            var numRows = jaggedArray.Count;
            var numCols = jaggedArray[0].Count;

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
