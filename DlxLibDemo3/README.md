## Notes

* This puzzle consists of 14 different pieces which can form an 8x8 draughtboard pattern when placed correctly. Each piece can be placed on the board with 4 different orientations. Pieces can only be placed on the board such the colour of the small squares matches the board (i.e. white on white and black on black).
* We construct a large matrix with 78 columns - a column for each piece (14) plus a column for each square on the board (64). We then add a bunch of rows for each piece. Each such row describes one possible legal placement of a piece on the board - identifying a specific piece at a specific orientation (N, S, E or W) and the squares on the board that it occupies.
* Therefore, a solution will identify exactly 14 rows which fully describe how to place the 14 pieces on the board to form the intended pattern. The board will be exactly covered.
* In this application, we run Dlx.Solve() on a background thread that we explicitly create.
* The background thread handles SearchStep and SolutionFound events:
    * SearchStep events - we build a data structure fully describing the placement of the pieces as they are at this step of the algorithm. We then enqueue this data structure to a ConcurrentQueue.
    * SolutionFound events - we call the Cancel() method as soon as we see the first solution because we are only interested in the first solution.
* The main UI thread has a timer running (10ms interval by default - but this can be adjusted using the slider). Each time the timer callback function runs, we dequeue a data structure from the ConcurrentQueue and display the pieces on the board.
