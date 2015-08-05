// include Fake lib
#r @"packages\FAKE.4.0.3\tools\FakeLib.dll"
open Fake

// Default target
Target "Default" (fun _ ->
    trace "Hello World from FAKE"
)

// start build
RunTargetOrDefault "Default"