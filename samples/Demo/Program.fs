// Learn more about F# at http://fsharp.org

open System
open AccidentalFish.FSharp.Validation

type Customer = {
    age: int
}

type Order = {
    title: string
    customer: Customer
    cost: double
}

[<EntryPoint>]
let main _ =
    
    let validateCustomer = createValidatorFor<Customer>() {
        validate (fun r -> r.age) [
            hasMinValueOf 18
            hasMaxValueOf 65
        ]
    }

    let validateOrder = createValidatorFor<Order>() {
        validate (fun r -> r.title) [
            isNotEmpty
            hasMaxLengthOf 128
        ]
        validate (fun r -> r.cost) [
            hasMinValueOf 0.
        ]
        validate (fun r -> r.customer) [
            withFunction validateCustomer
        ]
    }
    
    
    let inValidNewOrder = { title="" ; customer = { age = 16 } ; cost = -100. }    
    match (validateOrder inValidNewOrder) with
    | Ok -> printf "All good"
    | Errors errors -> printf "**ERRORS**\n%s\n\n" (String.concat "\n" (errors |> Seq.map (fun e -> e.message)))
    
    let validOrder = { title="Baked Beans" ; customer = { age = 45 }; cost = 50. }
    match (validateOrder validOrder) with
    | Ok -> printf "All good"
    | Errors errors -> printf "**ERRORS**\n%s\n\n" (String.concat "\n" (errors |> Seq.map (fun e -> e.message)))
    
    0


