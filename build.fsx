#r @"packages/FAKE/tools/FakeLib.dll"
open Fake
open FileSystemHelper
open ProcessHelper
open System.IO

RestorePackages()

let justPrint x = printfn "%A" x

let removeExtensionFromPath (fileInfo:FileInfo) = 
    fileInfo.FullName.Remove (fileInfo.FullName.Length - fileInfo.Extension.Length, fileInfo.Extension.Length)

Target "draw" (fun _ ->
    let magick = tryFindFileOnPath "magick.exe"
    let scales = ["86x48"; "480x270"; "640x360"; "1280x720"; "1920x1080"]
    let targetType = "png"

    let svgs = filesInDirMatchingRecursive "*.svg" (directoryInfo ".")
    let pathes = svgs |> Array.toList |> List.map (fun x -> x.FullName)
        
    let targetname (path:string) scale = sprintf "%s%s.%s" (path.Remove (path.Length - 4)) scale targetType
    let createArgs path scale = sprintf "-size %s %s %s" scale path (targetname path scale)
    let createScaledArgs path = scales |> List.map (fun s -> createArgs path s)

    let we = pathes
                |> List.map createScaledArgs
                |> List.concat
                |> List.map justPrint
    
    let errorCode = match magick with
                      | Some m -> 
                            let execute arguments = Shell.Exec(m, arguments, ".")
                            let errorCodes = pathes 
                                              |> List.map createScaledArgs
                                              |> List.concat
                                              |> List.map execute
                            match List.tryFind (fun x -> x = -1) errorCodes with
                              | Some ec -> ec
                              | None -> 1
                      | None -> 
                            printfn "no magick"
                            -1
    ()
)

"draw"

RunTargetOrDefault "draw"