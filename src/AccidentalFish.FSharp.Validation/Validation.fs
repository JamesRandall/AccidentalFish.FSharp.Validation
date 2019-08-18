namespace AccidentalFish.FSharp
open System
open System.Linq.Expressions

module Validation =
    type ValidationItem =
        {
            message: string
            property: string
            errorCode: string
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
        
    let private getPropertyPath (expression:Expression<Func<'commandType, 'propertyType>>) =
        let objectQualifiedExpression = expression.Body.ToString()
        let indexOfDot = objectQualifiedExpression.IndexOf('.')
        if indexOfDot = -1 then
            objectQualifiedExpression
        else 
            objectQualifiedExpression.Substring(indexOfDot+1)
        
    let private packageValidator (propertyGetterExpr:Expression<Func<'targetType, 'propertyType>>) (validator:(string -> 'propertyType -> ValidationState)) =
        let propertyName = propertyGetterExpr |> getPropertyPath
        
        let propertyGetter = propertyGetterExpr.Compile()                
        fun (value:obj) -> validator propertyName (propertyGetter.Invoke(value :?> 'targetType)) 
                
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
                              propertyGetter:Expression<Func<'targetType,'propertyType>>,
                              validatorFunctions:(string -> 'propertyType -> ValidationState) list) =
             { config with properties =
                            config.properties |> Seq.append [{
                                validators = validatorFunctions |> Seq.map (packageValidator propertyGetter) |> Seq.toList
                            }] |> Seq.toList }
             
    // General validators
    let isEqualTo comparisonValue =
        let comparator propertyName value =
            match value = comparisonValue with
            | true -> Ok
            | false -> Errors([{ message = sprintf "Must be equal to %O" comparisonValue; property = propertyName ; errorCode = "isEqualTo" }])
        comparator
        
    let isNotEqualTo comparisonValue =
        let comparator propertyName value =
            match not (value = comparisonValue) with
            | true -> Ok
            | false -> Errors([{ message = sprintf "Must not be equal to %O" comparisonValue; property = propertyName ; errorCode = "isNotEqualTo" }])
        comparator
    
    // Numeric validators    
    let hasMinValueOf minValue =
        let comparator propertyName value =
            match value >= minValue with
            | true -> Ok
            | false -> Errors([{ message = sprintf "Must have a minimum value of %O" minValue; property = propertyName ; errorCode = "hasMinValueOf" }])
        comparator
        
    let hasMaxValueOf maxValue =
        let comparator propertyName value =
            match value <= maxValue with
            | true -> Ok 
            | false -> Errors([{ message = sprintf "Must have a maximum value of %O" maxValue; property = propertyName ; errorCode = "hasMaxValueOf" }])
        comparator
        
    // Collection validators
    let isNotEmpty propertyName (value:seq<'item>) = // this also applies to strings
        if isNull(value) then
            Errors([{ message = "Must not be null"; property = propertyName ; errorCode = "isNotEmpty" }])
        elif (Seq.length value) = 0 then
            Errors([{ message = "Must not be empty"; property = propertyName ; errorCode = "isNotEmpty" }])        
        else
            Ok
            
    let eachItemWith (validatorFunc:('validatorTargetType->ValidationState)) =
        let buildIndexedPropertyName rootPropertyName index error =
            { error with property = sprintf "%s.[%d].%s" rootPropertyName index error.property }
        
        let comparator propertyName (items:seq<'validatorTargetType>) =
            let allResults = items |> Seq.mapi (fun index item -> (validatorFunc item),index)
            let failures = allResults
                           |> Seq.map (fun r -> match r with
                                                | (Ok,_) -> []
                                                | (Errors errors, index) ->
                                                     errors
                                                     |> Seq.map (buildIndexedPropertyName propertyName index)
                                                     |> Seq.toList)
                           |> Seq.concat |> Seq.toList
            match failures.Length with
            | 0 -> Ok
            | _ -> Errors(failures)
        comparator
        
    // String validators        
    let isNotEmptyOrWhitespace propertyName (value:string) =
        if isNull(value) then
            Errors([{ message = "Must not be null"; property = propertyName ; errorCode = "isNotEmptyOrWhitespace" }])
        elif String.IsNullOrEmpty(value) then
            Errors([{ message = "Must not be empty"; property = propertyName ; errorCode = "isNotEmptyOrWhitespace" }])
        elif String.IsNullOrWhiteSpace(value) then
            Errors([{ message = "Must not be whitespace"; property = propertyName ; errorCode = "isNotEmptyOrWhitespace" }])
        else
            Ok
        
    let hasLengthOf length =
        let comparator propertyName (value:string) =
            match value.Length = length with
            | true -> Ok
            | false -> Errors([{ message = sprintf "Must have a length of %O" length; property = propertyName ; errorCode = "hasLengthOf" }])
        comparator
        
    let hasMinLengthOf length =
        let comparator propertyName (value:string) =
            match value.Length >= length with
            | true -> Ok
            | false -> Errors([{ message = sprintf "Must have a length no less than %O" length; property = propertyName ; errorCode = "hasMinLengthOf" }])
        comparator
        
    let hasMaxLengthOf length =
        let comparator propertyName (value:string) =
            match value.Length <= length with
            | true -> Ok
            | false -> Errors([{ message = sprintf "Must have a length no greater than %O" length; property = propertyName ; errorCode = "hasMaxLengthOf" }])
        comparator
        
    // Function / sub-validators
    let withFunction (validatorFunc:('validatorTargetType->ValidationState)) =
        let comparator _ (value:'validatorTargetType) =
            validatorFunc value            
        comparator
        
    // Just an alias for withFunction but it makes it read better in calling code
    let withValidator = withFunction
        
    let createValidatorFor<'targetType>() =
        ValidatorBuilder<'targetType>()