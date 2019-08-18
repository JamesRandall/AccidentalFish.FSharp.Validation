AccidentalFish.FSharp.Validation
======

A simple and extensible declarative validation framework for F#.

A [sample console app](https://github.com/JamesRandall/AccidentalFish.FSharp.Validation/tree/master/samples/Demo) is available demonstrating basic usage.

For issues and help please log them in the [Issues area](https://github.com/JamesRandall/AccidentalFish.FSharp.Validation/issues) or contact me on [Twitter](https://twitter.com/azuretrenches).

You can also check out [my blog](https://www.azurefromthetrenches.com) for other .NET projects, articles, and cloud develoment.

## Getting Started

Add the NuGet package _AccidentalFish.FSharp.Validation_ to your project.

Consider the following model:

```fsharp
open AccidentalFish.FSharp.Validation
type Order = {
    id: string
    title: string
    cost: double
}
```

We can declare a validator for that model as follows:

```fsharp
let validateOrder = createValidatorFor<Order>() {
    validate (fun o -> o.id) [
        isNotEmpty
        hasLengthOf 36
    ]
    validate (fun o -> o.title) [
        isNotEmpty
        hasMaxLengthOf 128
    ]
    validate (fun o -> o.cost) [
        hasMinValueOf 0.
    ]    
}
```

The returned validator is a simple function that can be executed as follows:

```fsharp
let order = {
    id = "36467DC0-AC0F-43E9-A92A-AC22C68F25D1"
    title = "A valid order"
    cost = 55.
}

let validationResult = order |> validateOrder
```

The result is a discriminated union and will either be _Ok_ for a valid model or _Errors_ if issues were found. In the latter case this will be of type _ValidationItem list_. _ValidationItem_ contains three properties:

|Property|Description|
|--------|-------|
|errorCode|The error code, typically the name of the failed validation rule|
|message|The validation message|
|property|The path of the property that failed validation|

The below shows an example of outputting errors to the console:

```fsharp
match validationResult with
| Ok -> printf "No validation errors\n\n"
| Errors errors ->
    printf "**ERRORS**\n%s\n\n" (String.concat "\n" (errors |> Seq.map (fun e -> sprintf "%s: %s" e.property e.message)))

```

## Validating Complex Types

Often your models will contain references to other record types and collections. Take the following model as an example:

```fsharp
type OrderItem =
    {
        productName: string
        quantity: int
    }

type Customer =
    {
        name: string        
    }

type Order =
    {
        id: string
        customer: Customer
        items: OrderItem list
    }
```

First if we look at validating the customer name we can do this one of two ways. Firstly we can simply express the full path to the customer name property:

```fsharp
let validateOrder = createValidatorFor<Order>() {
    validate (fun o -> o.customer.name) {
        isNotEmpty
        hasMaxLengthOf 128
    }
}
```

Or, if we want to reuse the customer validations, we can combine validators:

```fsharp
let validateCustomer = createValidatorFor<Customer>() {
    validate (fun c -> c.name) {
        isNotEmpty
        hasMaxLengthOf 128
    }
}

let validateOrder = createValidatorFor<Order>() {
    validate (fun o -> o.customer) {
        withValidator validateCustomer
    }
}
```

In both cases above the property field in the error items will be fully qualified e.g.:

    customer.name

Validating items in collections are similar - we simply need to supply a validator for the items in the collection as shown below:

```fsharp
let validateOrderItem = createValidatorFor<OrderItem>() {
    validate (fun i -> i.productName) {
        isNotEmpty
        hasMaxLengthOf 128
    }
    validate (fun i -> i.quantity) {
        hasMinValueOf 1
    }
}

let validateOrder = createValidatorFor<Order>() {
    validate (fun o -> o.items) {
        isNotEmpty
        eachItemWith validateOrderItem
    }
}
```

Again the property fields in the error items will be fully qualified and contain the index e.g.:

    items.[0].productName


## Built-in Validators

The library includes a number of basic value validators (as seen in the examples above):

|Validator|Description|
|-|-|
|isEqualTo _expected_|Is the tested value equal to the expected value|
|isNotEqualTo _unexpected_|Is the tested value not equal to the unexpected value|
|hasMinValueOf _minValue_|Is the tested value greater than or equal to _minValue_|
|hasMaxValueOf _maxValue_|Is the tested value greater tahn or equal to _maxValue_|
|isNotEmpty|Is the sequence (including a string) not empty|
|eachItemWith _validator_|Apply _validator_ to each item in a sequence|
|hasLengthOf _length_|Is the sequence (including a string) of length _length_|
|hasMinLengthOf _length_|Is the sequence (including a string) of a minimum length of _length_|
|hasMaxLengthOf _length_|Is the sequence (including a string) of a maximum length of _length_|
|isNotEmptyOrWhitespace _value_|Is the tested value not empty and not whitespace|
|withValidator _validator_|Applies the specified validator to the property. Is an alias of _withFunction_|
|withFunction _function_|Apples the given function to the property. The function must have a signature of _'validatorTargetType -> ValidationState_|

I'll expand on this set over time. In the meantime it is easy to add additional validators as shown below.

## Adding Custom Validators

Its easy to add custom validators as all they are are functions with the signature _string -> 'a -> ValidationState_. The first parameter is the name of the property that the validator is being applied to and the second the value. We then return the validation state.

For example lets say we want to write a validator function for a discriminated union and a model that uses it:

```fsharp
type TrafficLightColor = | Red | Green | Blue

type TrafficLight =
    {
        color: TrafficLightColor
    }
```

To check if the traffic light is green we could write a validator as follows:

```fsharp
let isGreen propertyName value =
    match value with
    | Green -> Ok
    | _ -> Errors([{ errorCode="isGreen" ; message="The light was not green" ; property = propertyName }])
```

And we could use it like any other validator:

```fsharp
let trafficLightValidator = createValidatorFor<TrafficLight>() {
    validate (fun r -> r.color) [
        isGreen
    ]
}
```

If we want to be able to supply parameters to the validator then we need to write a function that returns our validator function. For example if we want to be able to specify the color we could write a validator as follows:

```fsharp
let isColor color =
    let comparator propertyName value =
        match value = color with
        | true -> Ok
        | false -> Errors([{ errorCode="isColor" ; message=sprintf "The light was not %O" value ; property = propertyName }])
    comparator
```

And then we can use it like any other validator:

```fsharp
let trafficLightValidator = createValidatorFor<TrafficLight>() {
    validate (fun r -> r.color) [
        isColor Amber
    ]
}
```
