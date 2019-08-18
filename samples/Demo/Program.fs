// Learn more about F# at http://fsharp.org

open System
open AccidentalFish.FSharp.Validation

type Customer =
    {
        age: int
    }

type OrderItem =
    {
        name: string
        quantity: int
    }

type Order =
    {
        title: string
        customer: Customer
        cost: double
        items: OrderItem list
    }

[<EntryPoint>]
let main _ =
    // A helper function to output
    let outputToConsole validationResult =
        match validationResult with
        | Ok -> printf "No validation errors\n\n"
        | Errors errors ->
            printf "**ERRORS**\n%s\n\n" (String.concat "\n" (errors |> Seq.map (fun e -> sprintf "%s: %s" e.property e.message)))
    
    
    // Set up a couple of models - one valid, one invalid
    let validNewOrder = {
        title="Cans of stuff"
        customer = { age = 45 }
        cost = 50.
        items = [
            { name = "Baked Beans" ; quantity = 10 }
            { name = "Kidney Beans" ; quantity = 2 }
        ]
    }
    
    let inValidNewOrder = {
        title=""
        customer = { age = 16 }
        cost = -100.
        items = [
            { name = "Baked Beans" ; quantity = -2 }
            { name = "" ; quantity = 5 }
        ]
    }
        
    // Set up a pair of validators
    let validateCustomer = createValidatorFor<Customer>() {
        validate (fun r -> r.age) [
            hasMinValueOf 18
            hasMaxValueOf 65
        ]
    }
    
    let validateOrderItem = createValidatorFor<OrderItem>() {
        validate (fun r -> r.name) [
            isNotEmpty
            hasMaxLengthOf 128
        ]
        validate (fun r -> r.quantity) [
            hasMinValueOf 1
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
            withValidator validateCustomer
        ]
        validate (fun r -> r.items) [
            isNotEmpty
            eachItemWith validateOrderItem
        ]
    }    
    
    // Test the validators
    inValidNewOrder |> validateOrder |> outputToConsole
    validNewOrder |> validateOrder |> outputToConsole    
        
    // Set up a validator using deep property references rather than a sub validator
    let validateOrderWithDeepReferences = createValidatorFor<Order>() {
        validate (fun r -> r.title) [
            isNotEmpty
            hasMaxLengthOf 128
        ]
        validate (fun r -> r.cost) [
            hasMinValueOf 0.
        ]
        validate (fun r -> r.customer.age) [
            hasMinValueOf 18
        ]
    }
    
    // Test the validator
    inValidNewOrder |> validateOrderWithDeepReferences |> outputToConsole
    validNewOrder |> validateOrderWithDeepReferences |> outputToConsole    
    
    0


