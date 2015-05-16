using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace DlxLib
{
    internal class SearchData
    {
        public SearchData()
        {
            _onStarted = () => { };
            _onFinished = () => { };
            _checkIfCancelled = () => false;
            _onCancelled = () => { };
            _onSearchStep = (_,__) => { };
            _onSolutionFound = (_,__) => { };
            _currentSolution = new Stack<int>();
        }

        public void OnStartedCall(Action onStarted)
        {
            if (null != onStarted) _onStarted = onStarted;
        }

        public void OnFinishedCall(Action onFinished)
        {
            if (null != onFinished) _onFinished = onFinished;
        }

        public void OnCancelledCall(Func<bool> checkIfCancelled, Action onCancelled)
        {
            if (null != checkIfCancelled) _checkIfCancelled = checkIfCancelled;
            if (null != onCancelled) _onCancelled = onCancelled;
        }

        public void OnSearchStepCall(Action<int, Func<IList<int>>> onSearchStep)
        {
            if (null != onSearchStep) _onSearchStep = onSearchStep;
        }

        public void OnSolutionFoundCall(Action<int, Func<IList<int>>> onSolutionFound)
        {
            if (null != onSolutionFound) _onSolutionFound = onSolutionFound;
        }

        public int IterationCount { get; private set; }
        public int SolutionCount { get; private set; }

        public void IncrementIterationCount() { IterationCount++; }
        public void IncrementSolutionCount() { SolutionCount++; }

        public void PushCurrentSolutionRowIndex(int rowIndex) { _currentSolution.Push(rowIndex); }
        public void PopCurrentSolutionRowIndex() { _currentSolution.Pop(); }

        public IList<int> CurrentStep
        {
            get
            {
                // It isn't safe to return an IEnumerable here unless you instantiate
                // the list anyway, because in a LINQ context where usage might be
                // delayed, the underlying stack will already be changed.  Same for
                // passing the IEnumerable to an event - by the time the receiver gets
                // the enumerable the underlying data may have been changed.
                return _currentSolution.OrderBy(id => id).ToList();
            }
        }

        public Solution CurrentSolution
        {
            get
            {
                return new Solution(CurrentStep);
            }
        }

        public void RaiseStarted()
        {
            _onStarted();
        }

        public void RaiseFinished()
        {
            _onFinished();
        }

        public bool IsCancelled()
        {
            return _checkIfCancelled();
        }

        public void RaiseCancelled()
        {
            _onCancelled();
        }

        public void RaiseSearchStep()
        {
            _onSearchStep(IterationCount, () => CurrentStep);
        }

        public void RaiseSolutionFound()
        {
            _onSolutionFound(SolutionCount, () => CurrentStep);
        }

        private readonly Stack<int> _currentSolution;
        private Action _onStarted;
        private Action _onFinished;
        private Func<bool> _checkIfCancelled;
        private Action _onCancelled;
        private Action<int, Func<IList<int>>> _onSearchStep;
        private Action<int, Func<IList<int>>> _onSolutionFound;
    }
}
