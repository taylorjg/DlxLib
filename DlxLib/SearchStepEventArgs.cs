using System;
using System.Collections.Generic;

namespace DlxLib
{
    public class SearchStepEventArgs : EventArgs
    {
        public SearchStepEventArgs(int iteration, IEnumerable<int> rowIndexes)
        {
            Iteration = iteration;
            RowIndexes = rowIndexes;
        }

        public int Iteration { get; private set; }
        public IEnumerable<int> RowIndexes { get; private set; }
    }
}
