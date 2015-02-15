using System.Collections.Generic;
using System.Linq;

namespace DlxLib
{
    /// <summary>
    /// 
    /// </summary>
    public class Solution
    {
        internal Solution(IEnumerable<int> rowIndexes)
        {
            _rowIndexes = rowIndexes.OrderBy(rowIndex => rowIndex);
        }

        /// <summary>
        /// 
        /// </summary>
        public IEnumerable<int> RowIndexes
        {
            get { return _rowIndexes; }
        }

        private readonly IEnumerable<int> _rowIndexes;
    }
}
