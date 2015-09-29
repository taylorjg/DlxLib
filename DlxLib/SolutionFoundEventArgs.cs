using System;

namespace DlxLib

{
    /// <summary>
    /// Provides data for the <see cref="Dlx.SolutionFound" /> event.
    /// </summary>
    public class SolutionFoundEventArgs : EventArgs
    {
        internal SolutionFoundEventArgs(Solution solution, int solutionIndex)
        {
            Solution = solution;
            SolutionIndex = solutionIndex;
        }

        /// <summary>
        /// Gives details of the solution.
        /// </summary>
        public Solution Solution { get; }

        /// <summary>
        /// The zero-based index of the solution i.e. the first solution found has a SolutionIndex of 0,
        /// the second solution found has a SolutionIndex of 1, etc.
        /// </summary>
        public int SolutionIndex { get; }
    }
}
