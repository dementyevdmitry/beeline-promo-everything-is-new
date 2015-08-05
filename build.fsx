// include Fake lib
#r @"packages\FAKE.4.0.3\tools\FakeLib.dll"
open Fake

// Properties
let buildDir = "./build/Default"

// Targets
Target "Clean" (fun _ ->
    CleanDir buildDir
)
Target "BuildApp" (fun _ ->
    !! @"D:\Rep\Promo.EverythingIsNew\Promo.EverythingIsNew.WebApp/Promo.EverythingIsNew.WebApp.csproj"
      |> MSBuildRelease buildDir "Build"
      |> Log "AppBuild-Output: "
)

Target "Default" (fun _ ->
    trace "Hello World from FAKE"
)

// Dependencies
"Clean"
  ==> "BuildApp"
  ==> "Default"

// start build
RunTargetOrDefault "Default"