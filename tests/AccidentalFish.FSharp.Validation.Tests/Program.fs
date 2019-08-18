module AccidentalFish.FSharp.Validation.Tests.EntryPoint

open Expecto

[<EntryPoint>]
let main args =
    runTestsInAssemblyWithCLIArgs [||] args
