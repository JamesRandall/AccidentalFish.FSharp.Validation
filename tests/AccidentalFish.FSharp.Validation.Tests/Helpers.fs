module AccidentalFish.FSharp.Validation.Tests.Helpers

open AccidentalFish.FSharp.Validation
open Expecto

let expectSingleError expectedError state =
    match state with
    | Ok -> Expect.isTrue false "Expected to find an error but result was ok"
    | Errors errors ->
        Expect.hasLength errors 1 "Expected to only find a single error but found multiple"
        Expect.equal errors.[0] expectedError "Errors do not match"

let expectError expectedError state =
    match state with
    | Ok -> Expect.isTrue false "Expected to find an error but result was ok"
    | Errors errors ->
        Expect.contains errors expectedError "Error could not be found"
    state
