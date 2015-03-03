module DlxLibPropertyTestsFs.PropertyTestsFs

open FsCheck
open FsCheckUtils
open Gen
open Arb
open Prop
open NUnit.Framework
open DlxLib

let config = Config.VerboseThrowOnFailure

let to2DArray (jagged: _ list list) =
    let numRows = jagged.Length
    let numCols = (List.nth jagged 0).Length
    Array2D.init numRows numCols (fun r c -> List.nth (List.nth jagged r) c)

let genMatrixOfIntWithNoSolutions = 
    gen {
        let! numCols = choose (2, 20)
        let! numRows = choose (2, 20)
        let! indexOfAlwaysZeroColumn = choose (0, numCols - 1)
        let genZeroOrOne = elements [0; 1]
        let genBefore = listOfLength indexOfAlwaysZeroColumn genZeroOrOne
        let genZero = listOfLength 1 (constant 0)
        let genAfter = listOfLength (numCols - indexOfAlwaysZeroColumn - 1) genZeroOrOne
        let genRow =  sequence [genBefore; genZero; genAfter] |> map (fun xs -> xs |> Seq.concat |> Seq.toList)
        let! rows = listOfLength numRows genRow
        return rows |> to2DArray
    }

let genRow numCols startIdx endIdx isFirstRow =
    gen {
        let fillerValue = if isFirstRow then 1 else 0
        let prefixPartLength = startIdx
        let randomPartLength = endIdx - startIdx
        let suffixPartLength = numCols - endIdx
        let! prefixPart = listOfLength prefixPartLength (constant fillerValue)
        let! randomPart = listOfLength randomPartLength (constant 0)
        let! suffixPart = listOfLength suffixPartLength (constant fillerValue)
        let row = [prefixPart; randomPart; suffixPart] |> Seq.concat |> Seq.toList
        return row
    }

let randomlySprinkleOnesIntoSolutionRows solutionRows startIdx randomRowIdxs =

    // TODO: find a way to do this functionally - currently using a reference cell and converting a list of lists to a list of arrays.

    let listOfSolutionRowArrays = List.map List.toArray solutionRows
    let colIndex = ref startIdx

    let iterFun = fun randomRowIdx ->
        let solutionRowArray = List.nth listOfSolutionRowArrays randomRowIdx
        Array.set solutionRowArray !colIndex 1
        colIndex := !colIndex + 1

    Seq.iter iterFun randomRowIdxs

    List.map Array.toList listOfSolutionRowArrays

let pokeSolutionRowsIntoMatrix matrix (solutionRows: _ list list) randomRowIdxs =

    // TODO: find a way to do this functionally - currently using a reference cell and converting a list of lists to an array of lists.

    let matrixAsArrayOfLists = matrix |> Seq.toArray
    let fromIndex = ref 0

    let iterFun = fun toIdx ->
        let solutionRow = List.nth solutionRows !fromIndex
        Array.set matrixAsArrayOfLists toIdx solutionRow
        fromIndex := !fromIndex + 1

    Seq.iter iterFun randomRowIdxs

    matrixAsArrayOfLists |> Seq.toList

let allSolutionRowsAreRepresented numSolutionRows randomRowIdxs =
    let allRowIdxs = seq { 0..numSolutionRows - 1 }
    let rowIdxIsRepresented = fun rowIdx -> Seq.exists (fun randomRowIdx -> rowIdx = randomRowIdx) randomRowIdxs
    Seq.forall rowIdxIsRepresented allRowIdxs

let genPartitionedSolutionRows numCols startIdx endIdx numSolutionRows =
    gen {
        let! firstSolutionRow = genRow numCols startIdx endIdx true
        let! otherSolutionRows = genRow numCols startIdx endIdx false |> listOfLength (numSolutionRows - 1)
        let solutionRows = [Seq.singleton firstSolutionRow |> Seq.toList; otherSolutionRows] |> Seq.concat |> Seq.toList
        let! randomRowIdxs =
            choose (0, numSolutionRows - 1)
            |> listOfLength (endIdx - startIdx)
            |> suchThat (allSolutionRowsAreRepresented numSolutionRows)
        return randomlySprinkleOnesIntoSolutionRows solutionRows startIdx randomRowIdxs
    }

let genPartitionedSolution numCols startIdx endIdx =
    gen {
        let! numSolutionRows = choose (1, min 5 (endIdx - startIdx))
        let! solutionRows = genPartitionedSolutionRows numCols startIdx endIdx numSolutionRows
        return solutionRows
    }

let genSolution numCols =
    genPartitionedSolution numCols 0 numCols

let genMatrixOfIntWithSingleSolution = 
    gen {
        let! numCols = choose (2, 20)
        let! solution = genSolution numCols
        let! numRows = choose (solution.Length, solution.Length * 5)
        let! matrix = constant 0 |> listOfLength numCols |> listOfLength numRows
        let! randomRowIdxs = GenExtensions.PickValues(solution.Length, seq { 0..numRows - 1})
        return pokeSolutionRowsIntoMatrix matrix solution randomRowIdxs |> to2DArray
    }

[<Test>]
let ``exact cover problems with no solutions``() =
    let body = fun matrix ->
        let dlx = new Dlx()
        let solutions = dlx.Solve matrix |> Seq.toList
        solutions.Length = 0 |@ sprintf "Expected no solutions but got %d" solutions.Length
    let arbMatrix = fromGen genMatrixOfIntWithNoSolutions
    let property = forAll arbMatrix body
    Check.One (config, property)

[<Test>]
let ``exact cover problems with a single solution``() =
    let body = fun matrix ->
        let dlx = new Dlx()
        let solutions = dlx.Solve matrix |> Seq.toList
        solutions.Length = 1 |@ sprintf "Expected exactly one solution but got %d" solutions.Length
    let arbMatrix = fromGen genMatrixOfIntWithSingleSolution
    let property = forAll arbMatrix body
    Check.One (config, property)
