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

let genRow numCols startIdx endIdx isFirstRow =
    gen {
        let fillerValue = if isFirstRow then 1 else 0
        let prefixPartLength = startIdx
        let randomPartLength = endIdx - startIdx
        let suffixPartLength = numCols - endIdx
        let! prefixPart = listOfLength prefixPartLength (constant fillerValue)
        let! randomPart = listOfLength randomPartLength (constant 0)
        let! suffixPart = listOfLength suffixPartLength (constant fillerValue)
        return List.concat [prefixPart; randomPart; suffixPart]
    }

let randomlySprinkleOnesIntoSolutionRows solutionRows startIdx randomRowIdxs =

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

let allSolutionRowsAreIncluded numSolutionRows randomRowIdxs =
    let allSolutionRowIdxs = seq { 0..numSolutionRows - 1 }
    let solutionRowIdxIsIncluded solutionRowIdx = Seq.exists (fun randomRowIdx -> randomRowIdx = solutionRowIdx) randomRowIdxs
    Seq.forall solutionRowIdxIsIncluded allSolutionRowIdxs

let genPartitionedSolutionRows numCols startIdx endIdx numSolutionRows =
    gen {
        let! firstSolutionRow = genRow numCols startIdx endIdx true
        let! otherSolutionRows = genRow numCols startIdx endIdx false |> listOfLength (numSolutionRows - 1)
        let solutionRows = firstSolutionRow :: otherSolutionRows
        let! randomRowIdxs =
            choose (0, numSolutionRows - 1)
            |> listOfLength (endIdx - startIdx)
            |> suchThat (allSolutionRowsAreIncluded numSolutionRows)
        return randomlySprinkleOnesIntoSolutionRows solutionRows startIdx randomRowIdxs
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

let genSolution numCols =
    genPartitionedSolution numCols 0 numCols

let makePartitions partitionLengths =
    let loop (partitions, currentStartIdx) partitionLength =
        let startIdx = currentStartIdx
        let endIdx = startIdx + partitionLength
        let partition = (startIdx, endIdx)
        (partition :: partitions, endIdx)
    Seq.fold loop (List.Empty, 0) partitionLengths |> fst |> List.rev

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
        let! randomRowIdxs = GenExtensions.PickValues(solution.Length, seq { 0..numRows - 1})
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
        let! randomRowIdxs = GenExtensions.PickValues(combinedSolutions.Length, seq { 0..numRows - 1})
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
        let p3 = p1 .&. p2
        Seq.concat [properties; Seq.singleton p3]

    let colIndexes = seq { 0..numCols - 1 }
    let properties = Seq.fold loop Seq.empty colIndexes
    PropExtensions.AndAll (properties |> Seq.toArray)

let checkSolutions matrix solutions =
    let assertions = Seq.toArray <| Seq.map (checkSolution matrix) solutions
    PropExtensions.AndAll assertions

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
