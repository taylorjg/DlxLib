using System;
using System.Collections.Generic;

namespace DlxLib
{
    public class SearchStepEventArgs : EventArgs
    {
        public SearchStepEventArgs(int step, IEnumerable<int> rowIndexes)
        {
            Step = step;
            RowIndexes = rowIndexes;
        }

        public int Step { get; private set; }
        public IEnumerable<int> RowIndexes { get; private set; }
    }
}
