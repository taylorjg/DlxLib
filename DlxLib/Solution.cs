using System.Collections.Generic;
using System.Linq;

namespace DlxLib
{
    public class Solution
    {
        public Solution(IEnumerable<int> rowIndexes)
        {
            RowIndexes = rowIndexes.OrderBy(rowIndex => rowIndex).ToList();
        }

        public IEnumerable<int> RowIndexes { get; private set; }
    }
}
