#r @"FAKE\tools\FakeLib.dll"
#r @"AltLanDS.Fake\lib\net45\AltLanDS.Fake.dll"


open Fake
open AltLanDS.Fake.ALtLanBuild
open System
open System.IO

Target "CopyToDrop" (fun _ ->
    if (not (directoryExists dropDir)) then CreateDir dropDir
    let dropZipFile = Path.Combine(dropDir,"Release_" + fileVersion + ".zip")
    !! (artifactsDir + "/**/*.*") -- "*.zip" |> Zip artifactsDir dropZipFile
)

Target "Clean" (fun _ -> 
    CleanDirs [baseBuildDir]
)

Target "Start" (fun _ ->
   trace " --- Start --- "
)

Target "Default" (fun _ ->
   trace " --- End --- "
)

Target "Nuget <add_name>" (fun _ ->
    SetReadOnly false (!! (artifactsDir @@ "<add_name>.nuspec"))
    AltLanDS.Fake.NuGetHelper.NuGet (fun p -> 
        {p with                       
            Version = nugetVersion
            Project = "<add_name>"
            WorkingDir = artifactsDir
            OutputPath = artifactsDir            
            PublishUnc=nugetPublishDir
            Publish = true}) 
            (Path.Combine(artifactsDir,"<add_name>.nuspec"))
)