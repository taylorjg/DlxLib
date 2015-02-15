using System;
using System.Collections.Generic;

namespace DlxLib
{
    /// <summary>
    /// 
    /// </summary>
    public class SearchStepEventArgs : EventArgs
    {
        internal SearchStepEventArgs(int iteration, IEnumerable<int> rowIndexes)
        {
            _iteration = iteration;
            _rowIndexes = rowIndexes;
        }

        /// <summary>
        /// 
        /// </summary>
        public int Iteration
        {
            get { return _iteration; }
        }

        /// <summary>
        /// 
        /// </summary>
        public IEnumerable<int> RowIndexes
        {
            get { return _rowIndexes; }
        }

        private readonly int _iteration;
        private readonly IEnumerable<int> _rowIndexes;
    }
}
