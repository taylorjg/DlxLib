using System.Collections.Generic;
using System.Linq;

namespace DlxLib
{
    /// <summary>
    /// Represents a solution to an exact cover problem.
    /// </summary>
    public class Solution
    {
        internal Solution(IList<int> rowIndexes)
        {
            _rowIndexes = rowIndexes;
        }

        /// <summary>
        /// The indexes of the set of rows, in the original matrix, that constitute the solution.
        /// The indexes are always sorted in ascending order (because SearchStep returns them
        /// that way, deliberately).
        /// </summary>
        public IList<int> RowIndexes
        {
            get { return _rowIndexes; }
        }

        private readonly IList<int> _rowIndexes;
    }
}
