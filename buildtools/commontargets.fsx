#r @"FAKE\tools\FakeLib.dll"
#r @"AltLanDS.Fake\lib\net45\AltLanDS.Fake.dll"


open Fake
open AltLanDS.Fake.ALtLanBuild
open System
open System.IO
open Fake.OctoTools
open Fake.AssemblyInfoFile
open Fake.AssemblyInfoHelper

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

Target "UpdateAssemblyInfo" (fun _ ->
    let file = "./GlobalAssemblyInfo.cs"
    let attributes = File.GetAttributes file
    File.SetAttributes(file, FileAttributes.Normal)
    DeleteFile file

    CreateCSharpAssemblyInfo file
        [
            Attribute.Product "Promo.EverythingIsNew.WebApp"
            Attribute.Company "AltLanDS"
            Attribute.Copyright "Copyright © AltLanDS 2012-2015"
            Attribute.Version versionNumber
            Attribute.FileVersion fileVersion]
)

Target "RestoreDeployPackages" (fun _ -> 
     "buildtools/packages.config"      
     |> RestorePackage (fun p ->
         { p with
             Sources = "https://www.myget.org/F/altlan-ds/" :: "https://www.nuget.org/api/v2/" :: p.Sources
             OutputPath = "./build/deployment"                         
             Retries = 1 })
 )

Target "Build" (fun _ ->
    trace "-- Build ---"
    buildWebApp "Promo.EverythingIsNew.WebApp"
)

let buildOctoPackage project =  
    let octo = ExecProcess (fun info -> 
        info.FileName <- "./build/deployment/OctopusTools.2.6.1.52/Octo.exe" 
        info.WorkingDirectory <- artifactsDir + "/" + project
        info.Arguments <- "pack --id="+project + " --outFolder=.. --version=" + fileVersion
    )
    let result = octo (TimeSpan.FromMinutes 1.0)
    in result

Target "BuildOctoPackages" (fun _ ->
    trace "-- Build Octopus Deploy Package ---"
    let result = buildOctoPackage "Promo.EverythingIsNew.WebApp"
    if result <> 0 then failwithf "Octo.exe returned with a non-zero exit code"
)

let publishOctoPackage project = 
    NuGetPublish (fun p -> 
        {p with
            AccessKey = "API-CBGKC2U6XCVMWXQVWRQP3OL4GPC"
            PublishUrl = "http://alt-dev001/nuget/packages"      
            WorkingDir = artifactsDir                  
            Version = fileVersion
            Project = "../" + artifactsDir + "/" + project
        })

Target "PublishOctoPackages" (fun _ ->
    trace "-- Publish Octo Nugets ---"
    publishOctoPackage "Promo.EverythingIsNew.WebApp"
)

let OctoRelease () =
    let release = { releaseOptions with 
                        Project = "Promo.EverythingIsNew.WebApp"
                        PackageVersion = fileVersion }
    let server = { Server = "http://alt-dev001/api"; ApiKey = "API-CBGKC2U6XCVMWXQVWRQP3OL4GPC" }
    Octo (fun octoParams ->
        { octoParams with
            ToolPath = "./build/deployment/OctopusTools.2.6.1.52"
            Server   = server
            Command  = CreateRelease (release, None) }
    )

let OctoReleaseAndDeploy environment = 
    let release = { 
        releaseOptions with 
            Project = "Promo.EverythingIsNew.WebApp"
            PackageVersion = fileVersion
            Version = fileVersion
        }
    let deploy  = { deployOptions with 
                        DeployTo = environment
        }
    let server = {Server = "http://alt-dev001/api"; ApiKey = "API-CBGKC2U6XCVMWXQVWRQP3OL4GPC" }
    Octo (fun octoParams ->
        { octoParams with
            ToolPath = "./build/deployment/OctopusTools.2.6.1.52"
            Server   = server
            Command  = CreateRelease (release, Some deploy) })

Target "ReleaseAndDeployToTest" (fun _ ->
    trace "-- ReleaseAndDeployToTest ---"
    OctoReleaseAndDeploy "TEST"
)

Target "Release" (fun _ ->
    trace "-- Release ---"
    OctoRelease()
)
