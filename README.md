
## NuGet

DlxLib is available as a NuGet package:

http://www.nuget.org/packages/DlxLib/

## DlxLib (C#)

DlxLib is C# class library that implements the Dancing Links (DLX) algorithm as described in the following paper: 

[Dancing Links (Donald E. Knuth, Stanford University)](http://arxiv.org/pdf/cs/0011047v1.pdf "Dancing Links (Donald E. Knuth, Stanford University)")

Given a matrix of 1's and 0's, it finds all solutions where a solution identifies a subset of the rows in the matrix such that every column contains exactly one 1. This is known as the "exact cover" problem. It can be used to solve various puzzles.

The difficulty is in representing a given problem as a 2D matrix of 0's/1's. But if you can figure out how to do that, then DlxLib can find all the solutions.

See the following link for a very nice tutorial on how DLX works and a practical application (solving a pentomino puzzle):

[CS575: Dancing Links - Colorado State University](http://www.cs.colostate.edu/~cs420dl/slides/DLX.ppt "CS575: Dancing Links - Colorado State University")

## A Simple Example

Here is a brief example of using DlxLib. I'm using a hardcoded 2D matrix of <code>int</code> - in fact, it is the example matrix from the original paper. I find that a 2D array of 0/1 <code>int</code> values is easier to read than a 2D array of <code>false</code>/<code>true</code> <code>bool</code> values.Therefore, this example uses the second overload of the <code>Solve()</code> method (see the "DlxLib API" section below).

```C#
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
var solutions = dlx.Solve(matrix);
// Do something with the solutions here...
```

## DlxLib API

### The Dlx Class

#### Methods

##### Solve

The <code>Dlx</code> class exposes three overloads of the <code>Solve()</code> method.

```C#
public IEnumerable<Solution> Solve(bool[,] matrix);
```

This overload takes a 2D matrix of <code>bool</code>. It returns an enumerable of <code>Solution</code>.

```C#
public IEnumerable<Solution> Solve<T>(T[,] matrix);
```

This overload takes a 2D matrix of <code>T</code>. It returns an enumerable of <code>Solution</code>. Internally, it converts the supplied matrix to a matrix of <code>bool</code>. Any elements that are not <code>default(T)</code> are considered to represent <code>true</code>.

```C#
public IEnumerable<Solution> Solve<T>(T[,] matrix, Func<T, bool> predicate);
```

This overload takes a 2D matrix of <code>T</code>. It returns an enumerable of <code>Solution</code>. Internally, it converts the supplied matrix to a matrix of <code>bool</code>. It uses the supplied predicate function to determine which elements represent <code>true</code>.

> NOTE: The following new <code>Solve</code> overload has been added in DlxLib 1.1.

```C#
public IEnumerable<Solution> Solve<TData, TRow, TCol>(
    TData data,
    Action<TData, Action<TRow>> iterateRows,
    Action<TRow, Action<TCol>> iterateCols,
    Func<TCol, bool> predicate);
```

This overload allows the caller to pass in any shape of data. However, the caller also needs to supply a function to iterate the rows in the data, a function to iterate the columns in a row and a function to indicate whether a given row/column value represents <code>true</code>.

Following is an example of its use:

```C#
var data = new List<Tuple<int[], string>>
    {
        Tuple.Create(new[] {1, 0, 0}, "Some data associated with row 0"),
        Tuple.Create(new[] {0, 1, 0}, "Some data associated with row 1"),
        Tuple.Create(new[] {0, 0, 1}, "Some data associated with row 2")
    };

var dlx = new Dlx();
var solutions = dlx.Solve<
    IList<Tuple<int[], string>>,
    Tuple<int[], string>,
    int>(
        data,
        (d, f) => { foreach (var r in d) f(r); },
        (r, f) => { foreach (var c in r.Item1) f(c); },
        c => c != 0);
```

##### Cancel

> NOTE: This method has been made obsolete in DlxLib 1.1 in favour of passing a <code>CancellationToken</code> to a new constructor on the <code>Dlx</code> class.
See also the following link on MSDN: [Managed Threading Basics | Cancellation](http://msdn.microsoft.com/en-us/library/dd997364(v=vs.100).aspx).

Finally, the <code>Dlx</code> class exposes the following method to cancel the <code>Solve</code> method. This is useful when the <code>Solve</code> method has been called on a background thread and you want to cancel the operation.

```C#
public void Cancel();
```

#### Events

The <code>Dlx</code> class also exposes the events described below.

##### Started

This event is raised at the beginning of the <code>Solve</code> method.

##### Finished

This event is raised at the end of the <code>Solve</code> method (unless the <code>Cancel</code> method was called).

##### Cancelled

This event is raised at the end of the <code>Solve</code> method if the operation has been cancelled via either the <code>Cancel</code> method or the <code>CancellationToken</code>.

##### SearchStep

This event is raised for each step of the algorithm.

###### SearchStepEventArgs

```C#
        public int Iteration { get; private set; }
        public IEnumerable<int> RowIndexes { get; private set; }
```

##### SolutionFound

This event is raised for each solution found.

###### SolutionFoundEventArgs

```C#
        public Solution Solution { get; private set; }
        public int SolutionIndex { get; private set; }
```

### The Solution Class

Each instance of the <code>Solution</code> class represents a solution to the matrix. It exposes an enumerable of <code>int</code> via the <code>RowIndexes</code> property - these identify a subset of the rows in the matrix that comprise a solution. The row indexes are zero-based and are always in ascending order.

```C#
public IEnumerable<int> RowIndexes { get; private set; }
```

## Differences between DlxLib and the pseudo-code in the original paper

* The <code>Search()</code> method has no <code>k</code> param
    * I have a <code>RowIndex</code> property on <code>DataObject</code>. I maintain a <code>Stack&lt;int&gt;</code> to store row indexes as the algorithm progresses. When a solution is found, I create a <code>Solution</code> object to encapsulate this bunch of row indexes (ordered by ascending value). The calling program can use these row indexes to identify the subset of rows of the matrix that comprise a solution. The program is then free to process this information in any way it chooses.
* The <code>ColumnObject</code> class has no <code>Name</code> property
    * DlxLibDemo2.exe shows how to associate names with columns inside the calling program

## Screenshot of DlxLibDemo.exe

The first demo application shows DlxLib being used to solve 2 simple matrices. For each solution, the entire matrix is written to the console with the solution row indexes and the 1's in the solution rows highlighted in yellow. This makes it clear to see that the solution rows have the property that there is exactly one 1 in each column.

![Screenshot of DlxLibDemo.exe](https://raw.github.com/taylorjg/DlxLib/master/Images/DlxLibDemo_screenshot.png)

## Screenshot of DlxLibDemo2.exe

The second demo application shows DlxLib being used to solve the same 2 simple matrices as the first demo application. However, the solutions are displayed in a different manner - only the rows that comprise the solution are displayed and we display the column names that correspond to the 1's.

![Screenshot of DlxLibDemo2.exe](https://raw.github.com/taylorjg/DlxLib/master/Images/DlxLibDemo2_screenshot.png)

## Screenshot of DlxLibDemo3.exe

The third demo application is a WPF application which shows a 14 piece draughtboard puzzle being solved. It redraws the board for each <code>SearchStep</code> event. It takes 23,316 iterations to find the first solution!

![Screenshot of DlxLibDemo3.exe](https://raw.github.com/taylorjg/DlxLib/master/Images/DlxLibDemo3_screenshot.png)

## Flow Free Puzzle Solver

See also the following project:

* [FlowFreeSolverWpf](https://github.com/taylorjg/FlowFreeSolverWpf "FlowFreeSolverWpf")

![Flow Free Solver Screenshot](https://raw.github.com/taylorjg/FlowFreeSolverWpf/master/Images/Screenshot.png "Flow Free Solver Screenshot")

## Other Links

* [Dancing Links (Wikipedia)](http://en.wikipedia.org/wiki/Dancing_Links "Dancing Links (Wikipedia)")
* [Knuth's Algorithm X (Wikipedia)](http://en.wikipedia.org/wiki/Algorithm_X "Knuth's Algorithm X (Wikipedia)")
* [Exact cover (Wikipedia)](http://en.wikipedia.org/wiki/Exact_cover "Exact cover (Wikipedia)")
