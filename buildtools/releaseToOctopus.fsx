#r @"FAKE\tools\FakeLib.dll"
#r @"AltLanDS.Fake\lib\net45\AltLanDS.Fake.dll"

// 
open Fake
open AltLanDS.Fake.ALtLanBuild
open System
open System.IO

open Fake.NuGet

#load "commontargets.fsx"


"Start"
    ==> "Clean"
    ==> "UpdateAssemblyInfo"
    ==> "Build"
    ==> "RestoreDeployPackages"
    ==> "BuildOctoPackages"    
    ==> "PublishOctoPackages"
    ==> "ReleaseAndDeployToTest" 
    ==> "Default"


RunTargetOrDefault "Default"


