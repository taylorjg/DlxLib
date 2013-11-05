using System.Threading;
using DlxLib;
using NUnit.Framework;

namespace DlxLibTests
{
    [TestFixture]
    internal class DlxLibEventTests
    {
        [Test]
        public void StartedEventFires()
        {
            var matrix = new bool[0, 0];
            var dlx = new Dlx();
            var startedEventHasBeenRaised = false;
            dlx.Started += (_, __) => startedEventHasBeenRaised = true;
            dlx.Solve(matrix);
            Assert.That(startedEventHasBeenRaised, Is.True, "Expected the Started event to have been raised");
        }

        [Test]
        public void FinishedEventFires()
        {
            var matrix = new bool[0, 0];
            var dlx = new Dlx();
            var finishedEventHasBeenRaised = false;
            dlx.Finished += (_, __) => finishedEventHasBeenRaised = true;
            dlx.Solve(matrix);
            Assert.That(finishedEventHasBeenRaised, Is.True, "Expected the Finished event to have been raised");
        }

        [Test]
        public void CancelledEventFires()
        {
            var matrix = new bool[0, 0];
            var dlx = new Dlx();
            var cancelledEventHasBeenRaised = false;
            dlx.Cancelled += (_, __) => cancelledEventHasBeenRaised = true;
            dlx.Started += (sender, __) => ((Dlx)sender).Cancel();
            var thread = new Thread(() => dlx.Solve(matrix));
            thread.Start();
            thread.Join();
            Assert.That(cancelledEventHasBeenRaised, Is.True, "Expected the Cancelled event to have been raised");
        }

        [Test]
        public void CancelledEventFiresUsingCancellationToken()
        {
            var matrix = new bool[0, 0];
            var cancellationTokenSource = new CancellationTokenSource();
            var dlx = new Dlx(cancellationTokenSource.Token);
            var cancelledEventHasBeenRaised = false;
            dlx.Cancelled += (_, __) => cancelledEventHasBeenRaised = true;
            dlx.Started += (_, __) => cancellationTokenSource.Cancel();
            var thread = new Thread(() => dlx.Solve(matrix));
            thread.Start();
            thread.Join();
            Assert.That(cancelledEventHasBeenRaised, Is.True, "Expected the Cancelled event to have been raised");
        }

        [Test]
        public void SearchStepEventFires()
        {
            var matrix = new[,]
                {
                    {0, 0, 1, 0, 1, 1, 0},
                    {1, 0, 0, 1, 0, 0, 1},
                    {0, 1, 1, 0, 0, 1, 0},
                    {1, 0, 0, 1, 0, 0, 0},
                    {0, 1, 0, 0, 0, 0, 1},
                    {0, 0, 0, 1, 1, 0, 1}
                };
            var dlx = new Dlx();
            var searchStepEventHasBeenRaised = false;
            dlx.SearchStep += (_, __) => searchStepEventHasBeenRaised = true;
            dlx.Solve(matrix);
            Assert.That(searchStepEventHasBeenRaised, Is.True, "Expected the SearchStep event to have been raised");
        }

        [Test]
        public void SolutionFoundEventFires()
        {
            var matrix = new[,]
                {
                    {0, 0, 1, 0, 1, 1, 0},
                    {1, 0, 0, 1, 0, 0, 1},
                    {0, 1, 1, 0, 0, 1, 0},
                    {1, 0, 0, 1, 0, 0, 0},
                    {0, 1, 0, 0, 0, 0, 1},
                    {0, 0, 0, 1, 1, 0, 1}
                };
            var dlx = new Dlx();
            var solutionFoundEventHasBeenRaised = false;
            dlx.SolutionFound += (_, __) => solutionFoundEventHasBeenRaised = true;
            dlx.Solve(matrix);
            Assert.That(solutionFoundEventHasBeenRaised, Is.True, "Expected the SolutionFound event to have been raised");
        }
    }
}
