
## TODO

* Make DlxLib available as a NuGet package ?

## DlxLib (C#)

DlxLib is C# class library that implements Dancing Links (DLX) as described in the following paper: 

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

#### Solve

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

#### Events

The <code>Dlx</code> class also exposes the events described below.

##### Started

This event is raised at the beginning of the <code>Solve</code> method.

##### Finished

This event is raised at the end of the <code>Solve</code> method (unless the <code>Cancel</code> method was called).

##### Cancelled

This event is raised at the end of the <code>Solve</code> method if the <code>Cancel</code> method was called.

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

#### Cancel

Finally, the <code>Dlx</code> class exposes the following method to cancel the <code>Solve</code> method. This is useful when the <code>Solve</code> method has been called on a background thread and you want to cancel the operation.

```C#
public void Cancel();
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

The second demo application shows DlxLib being used to solve the same 2 simple matrices as the first demo application. However, the solutions are in a different manner - only the rows that comprise the solution are displayed and we display the column names that correspond to the 1's.

![Screenshot of DlxLibDemo2.exe](https://raw.github.com/taylorjg/DlxLib/master/Images/DlxLibDemo2_screenshot.png)

## Screenshot of DlxLibDemo3.exe

DlxLibDemo3.exe is a WPF application which shows a 14 piece draughtboard puzzle being solved. It redraws the board for each <code>SearchStep</code> event.

![Screenshot of DlxLibDemo3.exe](https://raw.github.com/taylorjg/DlxLib/master/Images/DlxLibDemo3_screenshot.png)

## Other Links

* [Dancing Links (Wikipedia)](http://en.wikipedia.org/wiki/Dancing_Links "Dancing Links (Wikipedia)")
* [Knuth's Algorithm X (Wikipedia)](http://en.wikipedia.org/wiki/Algorithm_X "Knuth's Algorithm X (Wikipedia)")
* [Exact cover (Wikipedia)](http://en.wikipedia.org/wiki/Exact_cover "Exact cover (Wikipedia)")
