
## TODO

* Make DlxLib available as a NuGet package ?

## DlxLib (C#)

DlxLib is C# class library that implements Dancing Links (DLX) as described in the following paper: 

[Dancing Links (Donald E. Knuth, Stanford University)](http://arxiv.org/pdf/cs/0011047v1.pdf "Dancing Links (Donald E. Knuth, Stanford University)")

Given a matrix of 1's and 0's, it finds all solutions where a solution identifies a subset of the rows in the matrix such that every column contains exactly one 1. This is known as the "exact cover" problem. It can be used to solve various puzzles.

The difficulty is in representing a given problem as a 2D matrix of 0's/1's. But if you can figure out how to do that, then DlxLib can find all the solutions.

*TODO: give an example of how to represent a problem as a 2D matrix.*

## DlxLib API

### The Dlx Class

The Dlx class exposes three overloads of the <code>Solve()</code> method.

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

### The Solution Class

Each instance of the <code>Solution</code> class represents a solution to the matrix. It exposes an enumerable of <code>int</code> via the <code>RowIndexes</code> property - these identify a subset of the rows in the matrix that comprise a solution. The row indexes are zero-based.

```C#
public IEnumerable<int> RowIndexes { get; private set; }
```

## Simple Example

Here is a brief example of using DlxLib. The matrix is a hardcoded 2D matrix of <code>int</code> - in fact, it is the example matrix from the original paper. This example uses the second overload of the <code>Solve()</code> method. It is looks nicer to create a 2D array of 0/1 <code>int</code> values than a 2D array of <code>false</code>/<code>true</code> <code>bool</code> values.

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

## Differences between DlxLib and the pseudo-code in the original paper

* The <code>Search()</code> method has no <code>k</code> param
    * I have a <code>RowIndex</code> property on <code>DataObject</code>. I maintain a <code>Stack&lt;int&gt;</code> to store row indexes as the algorithm progresses. When a solution is found, I create a Solution object to encapsulate this bunch of row indexes (ordered by ascending value). The calling program can use these row indexes to identify the subset of rows of the matrix that comprise a solution. The program is then free to process this information in any way it chooses.
* The <code>ColumnObject</code> class has no <code>Name</code> property
    * DlxLibDemo2.exe shows how to associate names with columns inside the calling program

## Screenshot of DlxLibDemo.exe

![Screenshot of DlxLibDemo.exe](https://raw.github.com/taylorjg/DlxLib/master/Images/DlxLibDemo_screenshot.png)

## Screenshot of DlxLibDemo2.exe

![Screenshot of DlxLibDemo2.exe](https://raw.github.com/taylorjg/DlxLib/master/Images/DlxLibDemo2_screenshot.png)

## Other Links

* [Dancing Links (Wikipedia)](http://en.wikipedia.org/wiki/Dancing_Links "Dancing Links (Wikipedia)")
* [Knuth's Algorithm X (Wikipedia)](http://en.wikipedia.org/wiki/Algorithm_X "Knuth's Algorithm X (Wikipedia)")
* [Exact cover (Wikipedia)](http://en.wikipedia.org/wiki/Exact_cover "Exact cover (Wikipedia)")
