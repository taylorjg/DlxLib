using System;

namespace DlxLib

{
    /// <summary>
    /// 
    /// </summary>
    public class SolutionFoundEventArgs : EventArgs
    {
        internal SolutionFoundEventArgs(Solution solution, int solutionIndex)
        {
            _solution = solution;
            _solutionIndex = solutionIndex;
        }

        /// <summary>
        /// 
        /// </summary>
        public Solution Solution
        {
            get { return _solution; }
        }

        /// <summary>
        /// 
        /// </summary>
        public int SolutionIndex
        {
            get { return _solutionIndex; }
        }

        private readonly Solution _solution;
        private readonly int _solutionIndex;
    }
}
