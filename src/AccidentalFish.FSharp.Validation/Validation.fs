namespace AccidentalFish.FSharp
open System

module Validation =
    type ValidationItem =
        {
            message: string
            property: string option
        }
        
    type ValidationState =
        | Errors of ValidationItem list        
        | Ok
       
    type PropertyValidatorConfig =
        {
            validators: (obj -> ValidationState) list
        }
    
    type ValidatorConfig<'targetType> =
        {
            properties: PropertyValidatorConfig list       
        }
        
    let private packageValidator (propertyGetter:('targetType -> 'propertyType)) (validator:('propertyType -> ValidationState)) =
        fun (value:obj) -> validator(propertyGetter(value :?> 'targetType))
                
    type ValidatorBuilder<'targetType>() =
        member __.Yield (_: unit) : ValidatorConfig<'targetType> =
            {
                properties = []                    
            }
            
        member __.Run (config: ValidatorConfig<'targetType>) =
            let execValidation (record:'targetType) : ValidationState =
                let results = config.properties
                              |> Seq.map(fun p -> p.validators |> Seq.map (fun v -> v(record)))
                              |> Seq.concat
                              |> Seq.map(fun f -> match f with | Errors e -> e | _ -> [])
                              |> Seq.concat
                              
                match (results |> Seq.isEmpty) with
                | true -> Ok
                | false -> Errors(results |> Seq.toList)
            execValidation
            
        [<CustomOperation("validate")>]
        member this.validate (config: ValidatorConfig<'targetType>,
                              propertyGetter:('targetType -> 'propertyType),
                              validatorFunctions:('propertyType -> ValidationState) list) =
             { config with properties =
                            config.properties |> Seq.append [{
                                validators = validatorFunctions |> Seq.map (packageValidator propertyGetter) |> Seq.toList
                            }] |> Seq.toList }
             
    let hasMinValueOf minValue =
        let comparator value =
            match value >= minValue with
            | true -> Ok
            | false -> Errors([{ message = sprintf "Must have a minimum value of %O" minValue; property = None }])
        comparator
        
    let hasMaxValueOf maxValue =
        let comparator value =
            match value <= maxValue with
            | true -> Ok 
            | false -> Errors([{ message = sprintf "Must have a max value of %O" maxValue; property = None}])
        comparator
        
    let isNotEmpty (value:string) =
        if isNull(value) then
            Errors([{ message = "Must not be null"; property = None }])
        elif String.IsNullOrEmpty(value) then
            Errors([{ message = "Must not be empty"; property = None }])
        elif String.IsNullOrWhiteSpace(value) then
            Errors([{ message = "Must not be whitespace"; property = None }])
        else
            Ok
        
    let hasExactLengthOf length =
        let comparator (value:string) =
            match value.Length = length with
            | true -> Ok
            | false -> Errors([{ message = sprintf "Must have a length value of %O" length; property = None }])
        comparator
        
    let hasMaxLengthOf length =
        let comparator (value:string) =
            match value.Length <= length with
            | true -> Ok
            | false -> Errors([{ message = sprintf "Must have a length no bigger than %O" length; property = None }])
        comparator
        
    let withFunction (validatorFunc:('validatorTargetType->ValidationState)) =
        let comparator (value:'validatorTargetType) =
            validatorFunc value            
        comparator
        
    let createValidatorFor<'targetType>() =
        ValidatorBuilder<'targetType>()