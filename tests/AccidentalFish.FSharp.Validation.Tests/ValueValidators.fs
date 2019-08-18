module AccidentalFish.FSharp.Validation.Tests.ValueValidators
open Expecto
open AccidentalFish.FSharp.Validation
open Helpers

let private propertyName = "propertyName"
        
let private expectErrorWithMessage message errorCode state =
    state |> expectSingleError { message = message; property = propertyName ; errorCode = errorCode }

[<Tests>]
let isEqualToTests =
    testList "isEqualTo tests" [
        test "Returns ok when values are equal" {
            let comparator = isEqualTo "thanksforallthefish"
            
            let result = comparator propertyName "thanksforallthefish"
            
            Expect.equal result Ok "Comparison should return Ok"
        }
        
        test "Returns error when values are not equal" {
            let comparator = isEqualTo "littlewhitemice"
            
            let result = comparator propertyName "thanksforallthefish"
            
            result |> expectErrorWithMessage "Must be equal to littlewhitemice" "isEqualTo"          
        }
    ]
    
[<Tests>]
let isNotEqualToTests =
    testList "isNotEqualTo tests" [
        test "Returns ok when values are not equal" {
            let comparator = isNotEqualTo "littlewhitemice"
            
            let result = comparator propertyName "thanksforallthefish"
            
            Expect.equal result Ok "Comparison should return Ok"
        }
        
        test "Returns error when values are not equal" {
            let comparator = isNotEqualTo "thanksforallthefish"
            
            let result = comparator propertyName "thanksforallthefish"
            
            result |> expectErrorWithMessage "Must not be equal to thanksforallthefish" "isNotEqualTo"          
        }
    ]
    
[<Tests>]
let hasMinValueOfTests =
    testList "hasMinValueOf tests" [
        test "Returns ok when integer has minimum value" {
            let comparator = hasMinValueOf 42
            
            let result = comparator propertyName 42
            
            Expect.equal result Ok "Comparison should return Ok"
        }
        
        test "Returns error when integer is less than minimum value" {
            let comparator = hasMinValueOf 42
            
            let result = comparator propertyName 41
            
            result |> expectErrorWithMessage "Must have a minimum value of 42" "hasMinValueOf"
        }
        
        test "Returns ok when double has minimum value" {
            let comparator = hasMinValueOf 42.
            
            let result = comparator propertyName 42.
            
            Expect.equal result Ok "Comparison should return Ok"
        }
        
        test "Returns error when double is less than minimum value" {
            let comparator = hasMinValueOf 42.42
            
            let result = comparator propertyName 41.9
            
            result |> expectErrorWithMessage "Must have a minimum value of 42.42" "hasMinValueOf"
        }
    ]

[<Tests>]
let hasMaxValueOfTests =
    testList "hasMaxValueOf tests" [
        test "Returns ok when integer has maximum value" {
            let comparator = hasMaxValueOf 42
            
            let result = comparator propertyName 42
            
            Expect.equal result Ok "Comparison should return Ok"
        }
        
        test "Returns error when integer is greater than maximum value" {
            let comparator = hasMaxValueOf 42
            
            let result = comparator propertyName 43
            
            result |> expectErrorWithMessage "Must have a maximum value of 42" "hasMaxValueOf"
        }
        
        test "Returns ok when double has maximum value" {
            let comparator = hasMaxValueOf 42.
            
            let result = comparator propertyName 42.
            
            Expect.equal result Ok "Comparison should return Ok"
        }
        
        test "Returns error when double is greater than maximum value" {
            let comparator = hasMaxValueOf 42.42
            
            let result = comparator propertyName 43.
            
            result |> expectErrorWithMessage "Must have a maximum value of 42.42" "hasMaxValueOf"
        }
    ]

[<Tests>]
let isNotEmptyTests =
    testList "isNotEmpty tests" [
        test "Returns ok when string is not empty" {
            let comparator = isNotEmpty
            
            let result = comparator propertyName "thanksforallthefish"
            
            Expect.equal result Ok "Comparison should return ok"
        }
        
        test "Return error when string is null" {
            let comparator = isNotEmpty
            
            let result = comparator propertyName null
            
            result |> expectErrorWithMessage "Must not be null" "isNotEmpty"
        }
        
        test "Return error when string is whitespace" {
            let comparator = isNotEmpty
            
            let result = comparator propertyName "  "
            
            result |> expectErrorWithMessage "Must not be whitespace" "isNotEmpty"
        }
        
        test "Return error when string is zero length" {
            let comparator = isNotEmpty
            
            let result = comparator propertyName ""
            
            result |> expectErrorWithMessage "Must not be empty" "isNotEmpty"
        }
    ]
    
[<Tests>]
let hasLengthOfTests =
    testList "hasLengthOf tests" [
        test "Returns ok when string has exact length" {
            let comparator = hasLengthOf 10
            
            let result = comparator propertyName "0123456789"
            
            Expect.equal result Ok "Comparison should return ok"
        }
        
        test "Returns error when string is not of exact length" {
            let comparator = hasLengthOf 10
            
            let result = comparator propertyName "012345678"
            
            result |> expectErrorWithMessage "Must have a length of 10" "hasLengthOf"
        }
    ]
    
[<Tests>]
let hasMinLengthOfTests =
    testList "hasMinLengthOf tests" [
        test "Returns ok when string has minimum length" {
            let comparator = hasMinLengthOf 10
            
            let result = comparator propertyName "0123456789"
            
            Expect.equal result Ok "Comparison should return ok"
        }
        
        test "Returns error when string is shorter than minimum length" {
            let comparator = hasMinLengthOf 10
            
            let result = comparator propertyName "012345678"
            
            result |> expectErrorWithMessage "Must have a length no less than 10" "hasMinLengthOf"
        }
    ]
    
[<Tests>]
let hasMaxLengthOfTests =
    testList "hasMaxLengthOf tests" [
        test "Returns ok when string has maximum length" {
            let comparator = hasMaxLengthOf 10
            
            let result = comparator propertyName "0123456789"
            
            Expect.equal result Ok "Comparison should return ok"
        }
        
        test "Returns error when string is shorter than minimum length" {
            let comparator = hasMaxLengthOf 10
            
            let result = comparator propertyName "0123456789a"
            
            result |> expectErrorWithMessage "Must have a length no greater than 10" "hasMaxLengthOf"
        }
    ]