AccidentalFish.FSharp.Validation
======

A simple and extensible declarative validation framework for F#.

## Getting Started

Add the NuGet package AccidentalFish.FSharp.Validation to your project.

The below shows how to declare validations, run them against a simple model and output to the console:

```fsharp
open AccidentalFish.FSharp.Validation
type Order = {
    id: string
    title: string
    cost: double
}

let validateOrder = createValidatorFor<Order>() {
    validate (fun r -> r.id) [
        isNotEmpty
        hasLengthOf 36
    ]
    validate (fun r -> r.title) [
        isNotEmpty
        hasMaxLengthOf 128
    ]
    validate (fun r -> r.cost) [
        hasMinValueOf 0.
    ]    
}

let order = {
    id = "36467DC0-AC0F-43E9-A92A-AC22C68F25D1"
    title = "A valid order"
    cost = 55.
}

let validationResult = order |> validateOrder

match validationResult with
| Ok -> printf "No validation errors\n\n"
| Errors errors ->
    printf "**ERRORS**\n%s\n\n" (String.concat "\n" (errors |> Seq.map (fun e -> sprintf "%s: %s" e.property e.message)))

```