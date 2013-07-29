using System;

namespace DlxLib
{
    public class SolutionFoundEventArgs : EventArgs
    {
        public SolutionFoundEventArgs(Solution solution, int solutionIndex)
        {
            Solution = solution;
            SolutionIndex = solutionIndex;
        }

        public Solution Solution { get; private set; }
        public int SolutionIndex { get; private set; }
    }
}
