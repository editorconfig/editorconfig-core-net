module Paths

open System
open System.IO

let ToolName = "editorconfig-core-net"
let Repository = sprintf "editorconfig/%s" ToolName
let MainTFM = "net10.0"
let SignKey = "fe6ce3ea283749f2"

let ValidateAssemblyName = false
let IncludeGitHashInInformational = true
let GenerateApiChanges = false

let Root =
    let mutable dir = DirectoryInfo(".")
    while dir.GetFiles("*.sln").Length = 0 && dir.GetFiles("*.slnx").Length = 0 do dir <- dir.Parent
    Environment.CurrentDirectory <- dir.FullName
    dir

let RootRelative path = Path.GetRelativePath(Root.FullName, path)

let Output = DirectoryInfo(Path.Combine(Root.FullName, "build", "output"))

let ToolProject = DirectoryInfo(Path.Combine(Root.FullName, "src", ToolName))

// Map of project name to NuGet package id for managed (signed) packages.
// AOT per-RID tool packages are not in this map — validation and API-diff
// are intentionally skipped for them.
let mapProjectToNuget =
    Map.empty
          .Add("EditorConfig.App", "editorconfig-tool")
          .Add("EditorConfig.Core", "editorconfig")

let mapNugetToTFM =
    Map.empty
          .Add("editorconfig-tool", "net10.0")

let mapNugetToProject =
    mapProjectToNuget
    |> Map.fold (fun (m: Map<string, string>) key value -> m.Add(value, key)) Map.empty<string, string>
