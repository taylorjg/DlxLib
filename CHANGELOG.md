
### 1.2  (18th Feb 2015)


#### Features

* Added new <code>Solve</code> method overloads allowing the caller to pass an arbitrary data structure representing the matrix to replace the one added in DlxLib 1.1 (see breaking changes below). The new overloads are easier to use than their predecessor. The new overloads have the following signatures:

```C#
public IEnumerable<Solution> Solve<TData, TRow, TCol>(
    TData data,
    Func<TData, IEnumerable<TRow>> iterateRows,
    Func<TRow, IEnumerable<TCol>> iterateCols)

public IEnumerable<Solution> Solve<TData, TRow, TCol>(
    TData data,
    Func<TData, IEnumerable<TRow>> iterateRows,
    Func<TRow, IEnumerable<TCol>> iterateCols,
    Func<TCol, bool> predicate)
```

* All <code>Solve</code> method overloads now yield solutions as they are found. Previously, the internal search algorithm would find all solutions before returning. If only the first solution was required, clients would have to handle the <code>SolutionFound</code> event and cancel the solving process. Now, clients can just use <code>.First()</code> or <code>.Take(1)</code> etc.
* Added XML documentation comments.
* Added [online MSDN-style documentation](http://taylorjg.github.io/DlxLib/) that was built from the XML documentation comments using [Sandcastle Help File Builder](https://github.com/EWSoftware/SHFB). 

#### Breaking Changes

* Removed the <code>Cancel</code> method that was marked obsolete in DlxLib 1.1.
* Removed the <code>Solve</code> method overload with the following signature:

```C#
public IEnumerable<Solution> Solve(bool[,] matrix)
```

because it is unnecessary due to the following overload when <code>T</code> is a <code>bool</code>:

```C#
public IEnumerable<Solution> Solve<T>(T[,] matrix)
```

* Removed the following <code>Solve</code> method overload because it was unnecessarily complicated (what was I smoking ?!?):
 
```C#
public IEnumerable<Solution> Solve<TData, TRow, TCol>(
    TData data,
    Action<TData, Action<TRow>> iterateRows,
    Action<TRow, Action<TCol>> iterateCols,
    Func<TCol, bool> predicate)
```

### 1.1  (8th Nov 2013)


#### Features

* Added a Dlx constructor taking a <code>CancellationToken</code> because this is a more idiomatic way to provide support for cancellation in .NET.
* Marked the <code>Cancel</code> method as obsolete.
* Added a <code>Solve</code> method overload allowing the caller to pass an arbitrary data structure representing the matrix along with a couple of actions to iterate rows and columns and a predicate.  


### 1.0  (8th Aug 2013)


#### Features

* Initial release
