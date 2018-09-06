
[![NuGet](https://img.shields.io/nuget/v/DlxLib.svg)](https://www.nuget.org/packages/DlxLib/)

[![Build status](https://ci.appveyor.com/api/projects/status/di25a5u6ny229bvj/branch/master?svg=true)](https://ci.appveyor.com/project/taylorjg/dlxlib/branch/master)

## DlxLib (C#)

DlxLib is a C# class library that solves [exact cover](http://en.wikipedia.org/wiki/Exact_cover) problems by implementing Donald E. Knuth's [Algorithm X](http://en.wikipedia.org/wiki/Algorithm_X) using the Dancing Links technique as described in his paper, [Dancing Links](http://arxiv.org/pdf/cs/0011047v1.pdf "Dancing Links").

Given a matrix of 0s and 1s, it finds all solutions where a solution identifies a subset of the rows in the matrix such that every column contains exactly one 1.

The difficulty is in representing a given problem as a matrix of 0s/1s. But if you can figure out how to do that, then DlxLib can find all the solutions.

See the following link for a very nice tutorial on how DLX works and a practical application (solving a pentomino puzzle):

[CS575: Dancing Links - Colorado State University](http://www.cs.colostate.edu/~cs420dl/slides/DLX.ppt "CS575: Dancing Links - Colorado State University")

## Example

The following example shows how to use DlxLib to find the first two (of three) solutions to the matrix in the original [Dancing Links](http://arxiv.org/pdf/cs/0011047v1.pdf "Dancing Links") paper.

```C#
var matrix = new[,]
    {
        {1, 0, 0, 0},
        {0, 1, 1, 0},
        {1, 0, 0, 1},
        {0, 0, 1, 1},
        {0, 1, 0, 0},
        {0, 0, 1, 0}
    };
var dlx = new Dlx();
var firstTwoSolutions = dlx.Solve(matrix).Take(2);
```

## NuGet

DlxLib is available as a [NuGet package](http://www.nuget.org/packages/DlxLib/).

## Documentation

The documentation for the DlxLib API can be found [here](http://taylorjg.github.io/DlxLib/).

## Gallery

I have created a Wiki page where I encourage people to add links to things that they have done using DlxLib:

* [Gallery](https://github.com/taylorjg/DlxLib/wiki/Gallery)

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

## Potter Kata

See also the following project:

* [PotterKata2](https://github.com/taylorjg/PotterKata2 "PotterKata2")

## References

* [Knuth's Algorithm X (Wikipedia)](http://en.wikipedia.org/wiki/Algorithm_X "Knuth's Algorithm X (Wikipedia)")
* [Dancing Links (Wikipedia)](http://en.wikipedia.org/wiki/Dancing_Links "Dancing Links (Wikipedia)")
* [Exact cover (Wikipedia)](http://en.wikipedia.org/wiki/Exact_cover "Exact cover (Wikipedia)")
* [CS575: Dancing Links - Colorado State University](http://www.cs.colostate.edu/~cs420dl/slides/DLX.ppt "CS575: Dancing Links - Colorado State University")

## License

DlxLib is licensed under MIT. Refer to [license.txt](https://github.com/taylorjg/DlxLib/blob/master/license.txt) for more information.
