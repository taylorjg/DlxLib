using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace DlxLib
{
    /// <summary>
    /// This data object holds the intermediate state of a cover search - the stack
    /// of currently covered rows - and also the event hooks to be called during the
    /// search process.
    /// </summary>
    /// <remarks>
    /// The event hooks are here so that the search can be put in RootObject where it
    /// belongs but cover problems - and the handling of the interface to an
    /// application - can be handled by Dlx
    /// </remarks>
    internal class SearchData
    {
        public SearchData()
        {
            // By providing do-nothing lambdas as the event hooks the code looks a little
            // nicer - no tests against null - while taking only nanoseconds longer.  (One
            // could imagine a jitter that even removed the calls to the do-nothing events
            // on a per-instance basis, but we probably don't have that.)
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

        /// <summary>
        /// Current number of steps in the search.  (A step is choosing a row and covering a column.)
        /// </summary>
        public int IterationCount { get; private set; }

        /// <summary>
        /// Current number of solutions found.
        /// </summary>
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
