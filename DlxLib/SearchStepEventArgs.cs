using System;
using System.Collections.Generic;

namespace DlxLib
{
    public class SearchStepEventArgs : EventArgs
    {
        public SearchStepEventArgs(int depth, int iteration, IEnumerable<int> rowIndexes)
        {
            Depth = depth;
            Iteration = iteration;
            RowIndexes = rowIndexes;
        }

        public int Depth { get; private set; }
        public int Iteration { get; private set; }
        public IEnumerable<int> RowIndexes { get; private set; }
    }
}
