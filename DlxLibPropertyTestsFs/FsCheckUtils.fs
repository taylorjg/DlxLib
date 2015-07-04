module FsCheckUtils

open FsCheck.Gen

let pickValues n l = 
    let arr = Seq.toArray l
    seq { 0 .. arr.Length - 1 }
        |> elements
        |> listOfLength n
        |> suchThat (fun idxs -> (Seq.distinct >> Seq.length) idxs = n)
        |> map (fun idxs -> List.map (Array.get arr) idxs)
