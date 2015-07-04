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

let initPartitionedSolutionRow numCols startIdx endIdx isFirstRow =
    let fillerValue = if isFirstRow then 1 else 0
    let prefixPartLength = startIdx
    let randomPartLength = endIdx - startIdx
    let suffixPartLength = numCols - endIdx
    List.concat [
        List.replicate prefixPartLength fillerValue
        List.replicate randomPartLength 0
        List.replicate suffixPartLength fillerValue
    ]

let initPartitionedSolutionFirstRow numCols startIdx endIdx =
    initPartitionedSolutionRow numCols startIdx endIdx true

let initPartitionedSolutionOtherRows numCols startIdx endIdx numOtherRows =
    [ for _ in 1..numOtherRows do yield initPartitionedSolutionRow numCols startIdx endIdx false ]

let initPartitionedSolutionRows numCols startIdx endIdx numSolutionRows =
    let firstRow = initPartitionedSolutionFirstRow numCols startIdx endIdx
    let otherRows = initPartitionedSolutionOtherRows numCols startIdx endIdx (numSolutionRows - 1)
    firstRow :: otherRows

let randomlySprinkleOnesIntoSolutionRows solutionRows randomRowIdxs startIdx =

    // TODO: find a way to do this functionally - currently converting a list of lists to a list of arrays.

    let listOfSolutionRowArrays = List.map List.toArray solutionRows

    let loop idx randomRowIdx =
        let solutionRowArray = List.nth listOfSolutionRowArrays randomRowIdx
        Array.set solutionRowArray (startIdx + idx) 1

    Seq.iteri loop randomRowIdxs

    List.map Array.toList listOfSolutionRowArrays

let pokeSolutionRowsIntoMatrix matrix (solutionRows: _ list list) randomRowIdxs =

    // TODO: find a way to do this functionally - currently converting a list of lists to an array of lists.

    let matrixAsArrayOfLists = matrix |> Seq.toArray

    let loop idx randomRowIdx =
        let solutionRow = List.nth solutionRows idx
        Array.set matrixAsArrayOfLists randomRowIdx solutionRow

    Seq.iteri loop randomRowIdxs

    matrixAsArrayOfLists |> Seq.toList

let genPartitionedSolutionRows numCols startIdx endIdx numSolutionRows =

    let allSolutionRowsAreIncluded randomRowIdxs =
        let allSolutionRowIdxs = seq { 0..numSolutionRows - 1 }
        let solutionRowIdxIsIncluded solutionRowIdx =
            Seq.exists (fun randomRowIdx -> randomRowIdx = solutionRowIdx) randomRowIdxs
        Seq.forall solutionRowIdxIsIncluded allSolutionRowIdxs

    gen {
        let solutionRows = initPartitionedSolutionRows numCols startIdx endIdx numSolutionRows
        let! randomRowIdxs =
            choose (0, numSolutionRows - 1)
            |> listOfLength (endIdx - startIdx)
            |> suchThat allSolutionRowsAreIncluded
        return randomlySprinkleOnesIntoSolutionRows solutionRows randomRowIdxs startIdx
    }

let genPartitionedSolution numCols startIdx endIdx =
    gen {
        let! numSolutionRows = choose (1, min 5 (endIdx - startIdx))
        return! genPartitionedSolutionRows numCols startIdx endIdx numSolutionRows
    }

let genPartitionedSolutions numCols partitions =
    let mapping (startIdx, endIdx) = genPartitionedSolution numCols startIdx endIdx
    let mappedPartitions = Seq.map mapping partitions
    sequence (mappedPartitions |> Seq.toList)

let genSolutionRows numCols numSolutionRows =
    gen {
        let! solutionRows = constant 0 |> listOfLength numCols |> listOfLength numSolutionRows
        let! randomRowIdxs = choose (0, numSolutionRows - 1) |> listOfLength numCols
        return randomlySprinkleOnesIntoSolutionRows solutionRows randomRowIdxs 0
    }

let genSolution numCols =
    gen {
        let! numSolutionRows = choose (1, min 5 numCols)
        return! genSolutionRows numCols numSolutionRows
    }

let makePartitions partitionLengths =
    let loop partitions partitionLength =
        let startIdx = match partitions with
                       | [] -> 0
                       | p :: _ -> snd p
        let endIdx = startIdx + partitionLength
        (startIdx, endIdx) :: partitions
    Seq.fold loop List.Empty partitionLengths |> List.rev

let genPartitionLengths numCols numSolutions =
    gen {
        let! partitionLengths =
            listOfLength <| numSolutions - 1 <| choose (1, numCols / 2)
            |> suchThat (fun xs -> Seq.sum xs < numCols)
        let remainingLength = numCols - Seq.sum partitionLengths
        return remainingLength :: partitionLengths
    }

let genPartitions numCols numSolutions =
    gen {
        let! partitionLengths = genPartitionLengths numCols numSolutions
        return makePartitions partitionLengths
    }

let genMatrixOfIntWithNoSolutions = 
    gen {
        let! numCols = choose (2, 20)
        let! numRows = choose (2, 20)
        let! indexOfAlwaysZeroColumn = choose (0, numCols - 1)
        let genZeroOrOne = elements [0; 1]
        let genBefore = listOfLength indexOfAlwaysZeroColumn genZeroOrOne
        let genZero = listOfLength 1 (constant 0)
        let genAfter = listOfLength (numCols - indexOfAlwaysZeroColumn - 1) genZeroOrOne
        let genRow =  sequence [genBefore; genZero; genAfter] |> map List.concat
        let! rows = listOfLength numRows genRow
        return rows |> to2DArray
    }

let genMatrixOfIntWithSingleSolution = 
    gen {
        let! numCols = choose (2, 20)
        let! solution = genSolution numCols
        let! numRows = choose (solution.Length, solution.Length * 5)
        let! matrix = constant 0 |> listOfLength numCols |> listOfLength numRows
        let! randomRowIdxs = seq { 0..numRows - 1} |> pickValues solution.Length
        return pokeSolutionRowsIntoMatrix matrix solution randomRowIdxs |> to2DArray
    }

let genMatrixOfIntWithMultipleSolutions numSolutions = 
    gen {
        let! numCols = choose (numSolutions, numSolutions * 10)
        let! partitions = genPartitions numCols numSolutions
        let! solutions = genPartitionedSolutions numCols partitions
        let combinedSolutions = List.concat solutions
        let! numRows = choose (combinedSolutions.Length, combinedSolutions.Length * 5)
        let! matrix = constant 0 |> listOfLength numCols |> listOfLength numRows
        let! randomRowIdxs = seq { 0..numRows - 1} |> pickValues combinedSolutions.Length
        return pokeSolutionRowsIntoMatrix matrix combinedSolutions randomRowIdxs |> to2DArray
    }

let checkSolution (matrix: _ [,]) (solution: DlxLib.Solution) =

    let numCols = Array2D.length2 matrix
    let numSolutionRows = Seq.length solution.RowIndexes
    let expectedNumZerosPerColumn = numSolutionRows - 1

    let makeLabel1 colIndex numOnes =
        sprintf "Expected column %d to contain a single 1 but it contains %d" colIndex numOnes

    let makeLabel2 colIndex numZeros =
        sprintf "Expected column %d to contain exactly %d 0s but it contains %d" colIndex expectedNumZerosPerColumn numZeros

    let loop properties colIndex =
        let innerLoop (numZeros, numOnes) rowIndex =
            match matrix.[rowIndex, colIndex] with
            | 0 -> (numZeros + 1, numOnes)
            | 1 -> (numZeros, numOnes + 1)
            | _ -> (numZeros, numOnes)
        let numZeros, numOnes = Seq.fold innerLoop (0, 0) solution.RowIndexes
        let p1 = numOnes = 1 |@ makeLabel1 colIndex numOnes
        let p2 = numZeros = expectedNumZerosPerColumn |@ makeLabel2 colIndex numZeros
        p1 :: p2 :: properties

    let colIndexes = seq { 0..numCols - 1 }
    let properties = Seq.fold loop List.empty colIndexes |> List.rev
    ofTestable properties

let checkSolutions matrix solutions =
    List.map (checkSolution matrix) solutions |> ofTestable

[<Test>]
let ``exact cover problems with no solutions``() =
    let body matrix =
        let dlx = new Dlx()
        let solutions = dlx.Solve matrix |> Seq.toList
        solutions.Length = 0 |@ sprintf "Expected no solutions but got %d" solutions.Length
    let arbMatrix = fromGen genMatrixOfIntWithNoSolutions
    let property = forAll arbMatrix body
    Check.One (config, property)

[<Test>]
let ``exact cover problems with a single solution``() =
    let body matrix =
        let dlx = new Dlx()
        let solutions = dlx.Solve matrix |> Seq.toList
        let p1 = solutions.Length = 1 |@ sprintf "Expected exactly one solution but got %d" solutions.Length
        let p2 = checkSolutions matrix solutions
        p1 .&. p2
    let arbMatrix = fromGen genMatrixOfIntWithSingleSolution
    let property = forAll arbMatrix body
    Check.One (config, property)

[<Test>]
let ``exact cover problems with multiple solutions``() =

    let arbNumSolutions = fromGen <| choose (2, 5)

    let body numSolutions =
        let makeLabel actualNumSolutions =
            sprintf "Expected exactly %d solutions but got %d" numSolutions actualNumSolutions
        let arbMatrix = fromGen <| genMatrixOfIntWithMultipleSolutions numSolutions
        let innerBody matrix =
            let dlx = new Dlx()
            let solutions = dlx.Solve matrix |> Seq.toList
            let actualNumSolutions = Seq.length solutions
            let p1 = actualNumSolutions = numSolutions |@ makeLabel actualNumSolutions
            let p2 = checkSolutions matrix solutions
            p1 .&. p2
        forAll arbMatrix innerBody

    let property = forAll arbNumSolutions body

    Check.One (config, property)
