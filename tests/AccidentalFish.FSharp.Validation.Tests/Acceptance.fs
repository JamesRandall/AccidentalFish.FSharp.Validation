module AccidentalFish.FSharp.Validation.Tests.Acceptance
open AccidentalFish.FSharp.Validation
open Expecto
open Helpers

type ExampleModelId = | ExampleModelId of string

type SubModel =
    {
        stringValue: string
    }

type ExampleModel =
    {
        id: ExampleModelId
        value: int
        subModel: SubModel
    }
    
let idIsNotEmpty propertyName value =
    match value with | ExampleModelId innerValue -> isNotEmpty propertyName innerValue
    
[<Tests>]
let acceptanceTests =
    let validateExampleModel = createValidatorFor<ExampleModel>() {
        validate (fun m -> m.id) [
            idIsNotEmpty
            isEqualTo (ExampleModelId("abc"))            
        ]
        validate (fun m -> m.value) [
            hasMinValueOf 0
        ]
        validate (fun m -> m.subModel.stringValue) [
            hasLengthOf 10
        ]
    }

    testList "Acceptance tests" [
        test "Returns ok for valid model" {
            let model = {
                id = ExampleModelId("abc")
                value = 50
                subModel = {
                    stringValue = "helloworld"
                }
            }
            
            let result = model |> validateExampleModel
            
            Expect.equal result Ok "Should return ok" 
        }
        
        test "Returns failure for each validation" {
            let model = {
                id = ExampleModelId("")
                value = -10
                subModel = {
                    stringValue = "hello"
                }
            }
            
            let result = model |> validateExampleModel
            result
                |> expectError { message="Must not be empty" ; property = "id" ; errorCode = "isNotEmpty" }
                |> expectError { message="Must have a minimum value of 0" ; property = "value" ; errorCode = "hasMinValueOf" }
                |> expectError { message="Must have a length of 10" ; property = "subModel.stringValue" ; errorCode = "hasLengthOf" }
                |> ignore
        }
    ]
    
