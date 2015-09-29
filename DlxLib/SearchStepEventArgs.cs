using System;
using System.Collections.Generic;

namespace DlxLib
{
    /// <summary>
    /// Provides data for the <see cref="Dlx.SearchStep" /> event.
    /// </summary>
    public class SearchStepEventArgs : EventArgs
    {
        internal SearchStepEventArgs(int iteration, IEnumerable<int> rowIndexes)
        {
            Iteration = iteration;
            RowIndexes = rowIndexes;
        }

        /// <summary>
        /// The number of iterations that the internal search algorithm has performed up to this point.
        /// </summary>
        public int Iteration { get; }

        /// <summary>
        /// The indexes of the set of rows, in the original matrix, that is currently being considered.
        /// </summary>
        public IEnumerable<int> RowIndexes { get; }
    }
}
