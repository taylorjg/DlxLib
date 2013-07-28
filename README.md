
## Stuff To Do

* Mention that DlxLib is a general engine for solving "exact cover" problems
    * Just need to figure out how to represent your the problem as a 2D matrix of 0's/1's
* Show main usage in brief code example (essentially a chunk of DlxLibDemo)
* Mention overloads of the Solve() method e.g. T[,] + an optional predicate
* Mention the differences between my implementation and the pseudo-code in the original paper
    * Search() method has no 'k' param
    * ColumnObject has no Name property
        * DlxLibDemo2 shows how to associate names with columns inside the calling program
            * Add a screenshot of DlxLibDemo2 in action
* Make DlxLib available as a NuGet package ?

## DlxLib (C#)

DlxLib is C# class library that implements Dancing Links (DLX) as described in the following paper: 

[Dancing Links (Donald E. Knuth, Stanford University)](http://arxiv.org/pdf/cs/0011047v1.pdf "Dancing Links (Donald E. Knuth, Stanford University)")

Given a matrix of 1's and 0's, it finds all solutions where a solution identifies a subset of the rows in the matrix such that every column contains exactly one 1. This is known as the "exact cover" problem. It can be used to solve various puzzles.

## API

```C#
public IEnumerable<Solution> Solve(bool[,] matrix);
public IEnumerable<Solution> Solve<T>(T[,] matrix);
public IEnumerable<Solution> Solve<T>(T[,] matrix, Func<T, bool> predicate);
```

## Simple Example

Here is a brief example of using DlxLib. The matrix is a hardcoded 2D matrix of <code>int</code>.

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



## Screenshot of DlxLibDemo.exe

![Screenshot of DlxLibDemo.exe](https://raw.github.com/taylorjg/DlxLib/master/Images/DlxLibDemo_screenshot.png)

## Screenshot of DlxLibDemo2.exe

![Screenshot of DlxLibDemo2.exe](https://raw.github.com/taylorjg/DlxLib/master/Images/DlxLibDemo2_screenshot.png)

## Other Links

* [Dancing Links (Wikipedia)](http://en.wikipedia.org/wiki/Dancing_Links "Dancing Links (Wikipedia)")
* [Knuth's Algorithm X (Wikipedia)](http://en.wikipedia.org/wiki/Algorithm_X "Knuth's Algorithm X (Wikipedia)")
* [Exact cover (Wikipedia)](http://en.wikipedia.org/wiki/Exact_cover "Exact cover (Wikipedia)")
