﻿// Learn more about F# at http://fsharp.org

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
    
type TrafficLightColor = | Red | Green | Amber

type TrafficLight = {
    color: TrafficLightColor
}

type DiscountOrder = {
    value: int
    discountExplanation: string
    discountPercentage: int
}

type OptionalExample = {
    value: int
    message: string option
}

type SingleCaseId = SingleCaseId of string

type EntityWithSingleUnionId = {
    id: SingleCaseId
}

type MultiCaseUnion =
    | NumericValue of double
    | StringValue of string

type UnionExample =
    {
        value: MultiCaseUnion
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
            isGreaterThanOrEqualTo 18
            isLessThanOrEqualTo 65
        ]
    }
    
    let validateOrderItem = createValidatorFor<OrderItem>() {
        validate (fun r -> r.name) [
            isNotEmpty
            hasMaxLengthOf 128
        ]
        validate (fun r -> r.quantity) [
            isGreaterThanOrEqualTo 1
        ]
    }

    let validateOrder = createValidatorFor<Order>() {
        validate (fun r -> r.title) [
            isNotEmpty
            hasMaxLengthOf 128
        ]
        validate (fun r -> r.cost) [
            isGreaterThan 0.
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
            isGreaterThanOrEqualTo 0.
        ]
        validate (fun r -> r.customer.age) [
            isGreaterThanOrEqualTo 18
        ]
    }
    
    // Test the validator
    inValidNewOrder |> validateOrderWithDeepReferences |> outputToConsole
    validNewOrder |> validateOrderWithDeepReferences |> outputToConsole
    
    
    // Traffic light validation
    
    let isGreen propertyName value =
        match value with
        | Green -> Ok
        | _ -> Errors([{ errorCode="isGreen" ; message="The light was not green" ; property = propertyName }])
        
    let isColor color =
        let comparator propertyName value =
            match value = color with
            | true -> Ok
            | false -> Errors([{ errorCode="isColor" ; message=sprintf "The light was not %O" value ; property = propertyName }])
        comparator
    
    let trafficLightValidator = createValidatorFor<TrafficLight>() {
        validate (fun r -> r.color) [
            isColor Green
        ]
    }
    
    { color = Amber } |> trafficLightValidator |> outputToConsole
    
    // Conditional validation using validateWhen
    
    let discountOrderValidatorWithValidateWhen = createValidatorFor<DiscountOrder>() {
        validateWhen (fun w -> w.value < 100) (fun o -> o.discountPercentage) [
            isEqualTo 0
        ]
        validateWhen (fun w -> w.value < 100) (fun o -> o.discountExplanation) [
            isEmpty
        ]
        validateWhen (fun w -> w.value >= 100) (fun o -> o.discountPercentage) [
            isEqualTo 10
        ]
        validateWhen (fun w -> w.value >= 100) (fun o -> o.discountExplanation) [
            isNotEmpty
        ]
        validate (fun o -> o.value) [
            isGreaterThan 0
        ]
    }
    
    printf "Discount should pass\n"
    { value = 50 ; discountPercentage = 0; discountExplanation = "" } |> discountOrderValidatorWithValidateWhen |> outputToConsole
    
    printf "Should fail on explanation and discount being applied \n"
    { value = 50 ; discountPercentage = 10; discountExplanation = "An explanation" } |> discountOrderValidatorWithValidateWhen |> outputToConsole
    
    printf "Should fail on missing discount explanation\n"
    { value = 150 ; discountPercentage = 10; discountExplanation = "" } |> discountOrderValidatorWithValidateWhen |> outputToConsole
    
    printf "Should pass\n"
    { value = 150 ; discountPercentage = 10; discountExplanation = "An explanation" } |> discountOrderValidatorWithValidateWhen |> outputToConsole
    
    let orderWithDiscount = createValidatorFor<DiscountOrder>() {
        validate (fun o -> o.discountPercentage) [
            isEqualTo 10
        ]
        validate (fun o -> o.discountExplanation) [
            isNotEmpty
        ]
    }
    let orderWithNoDiscount = createValidatorFor<DiscountOrder>() {
        validate (fun o -> o.discountPercentage) [
            isEqualTo 0
        ]
        validate (fun o -> o.discountExplanation) [
            isEmpty
        ]
    }
    
    let discountOrderValidatorWithValidatorWhen = createValidatorFor<DiscountOrder>() {
        validate (fun o -> o) [
            withValidatorWhen (fun o -> o.value < 100) orderWithNoDiscount
            withValidatorWhen (fun o -> o.value >= 100) orderWithDiscount            
        ]
        validate (fun o -> o.value) [
            isGreaterThan 0
        ]
    }
    
    printf "Discount should pass\n"
    { value = 50 ; discountPercentage = 0; discountExplanation = "" } |> discountOrderValidatorWithValidatorWhen |> outputToConsole
    
    printf "Should fail on explanation and discount being applied \n"
    { value = 50 ; discountPercentage = 10; discountExplanation = "An explanation" } |> discountOrderValidatorWithValidatorWhen |> outputToConsole
    
    printf "Should fail on missing discount explanation\n"
    { value = 150 ; discountPercentage = 10; discountExplanation = "" } |> discountOrderValidatorWithValidatorWhen |> outputToConsole
    
    printf "Should pass\n"
    { value = 150 ; discountPercentage = 10; discountExplanation = "An explanation" } |> discountOrderValidatorWithValidatorWhen |> outputToConsole
    
    
    let discountOrderValidatorWithValidatorWhen = createValidatorFor<DiscountOrder>() {
        validate (fun o -> o) [
            withValidatorWhen (fun o -> o.value < 100) (createValidatorFor<DiscountOrder>() {
                validate (fun o -> o) [
                    withValidatorWhen (fun o -> o.value < 100) orderWithNoDiscount
                    withValidatorWhen (fun o -> o.value >= 100) orderWithDiscount            
                ]
                validate (fun o -> o.value) [
                    isGreaterThan 0
                ]
            })
            withValidatorWhen (fun o -> o.value >= 100) (createValidatorFor<DiscountOrder>() {
                validate (fun o -> o.discountPercentage) [
                    isEqualTo 10
                ]
                validate (fun o -> o.discountExplanation) [
                    isNotEmpty
                ]
            })
        ]
        validate (fun o -> o.value) [
            isGreaterThan 0
        ]
    }
    
    printf "Discount should pass\n"
    { value = 50 ; discountPercentage = 0; discountExplanation = "" } |> discountOrderValidatorWithValidatorWhen |> outputToConsole
    
    printf "Should fail on explanation and discount being applied \n"
    { value = 50 ; discountPercentage = 10; discountExplanation = "An explanation" } |> discountOrderValidatorWithValidatorWhen |> outputToConsole
    
    printf "Should fail on missing discount explanation\n"
    { value = 150 ; discountPercentage = 10; discountExplanation = "" } |> discountOrderValidatorWithValidatorWhen |> outputToConsole
    
    printf "Should pass\n"
    { value = 150 ; discountPercentage = 10; discountExplanation = "An explanation" } |> discountOrderValidatorWithValidatorWhen |> outputToConsole
    
    
    let optionalRequiredValidator = createValidatorFor<OptionalExample>() {
        validateRequired (fun o -> o.message) [
            hasMaxLengthOf 10
        ]
    }
    
    printf "Should fail due to missing message\n"
    { value= 10 ; message = None } |> optionalRequiredValidator |> outputToConsole
    printf "Should fail due to having a message but it being too long\n"
    { value= 10 ; message = Some "0123456789a" } |> optionalRequiredValidator |> outputToConsole
    printf "Should pass due to having a message and it being within the length constraint\n"
    { value= 10 ; message = Some "0123456789" } |> optionalRequiredValidator |> outputToConsole
    
    
    let optionalUnrequiredValidator = createValidatorFor<OptionalExample>() {
        validateUnrequired (fun o -> o.message) [
            hasMaxLengthOf 10
        ]
    }
    
    printf "Should succeed with missing message\n"
    { value= 10 ; message = None } |> optionalUnrequiredValidator |> outputToConsole
    printf "Should fail due to having a message but it being too long\n"
    { value= 10 ; message = Some "0123456789a" } |> optionalUnrequiredValidator |> outputToConsole
    printf "Should pass due to having a message and it being within the length constraint\n"
    { value= 10 ; message = Some "0123456789" } |> optionalUnrequiredValidator |> outputToConsole
    
    
    let unwrap (SingleCaseId id) = id
    let singleCaseIdValidator = createValidatorFor<EntityWithSingleUnionId>() {
        validateSingleCaseUnion (fun o -> o.id) unwrap [
            isNotEmpty
            hasMaxLengthOf 36
        ]
    }
    
    printf "Single case union validation should succeed\n"
    { id = SingleCaseId("123") } |> singleCaseIdValidator |> outputToConsole
    
    
    let unionValidator = createValidatorFor<UnionExample>() {
        validateUnion (fun o -> o.value) (fun v -> match v with | StringValue s -> Unwrapped(s) | _ -> Ignore) [
            isNotEmpty
            hasMinLengthOf 10
        ]
        
        validateUnion (fun o -> o.value) (fun v -> match v with | NumericValue n -> Unwrapped(n) | _ -> Ignore) [
            isGreaterThan 0.
        ]
    }
    
    printf "Should fail validation on a string rule\n"
    { value = StringValue("jim") } |> unionValidator |> outputToConsole
    printf "Should fail validation on a numeric rule"
    { value = NumericValue(-5.5) } |> unionValidator |> outputToConsole
    
    0


