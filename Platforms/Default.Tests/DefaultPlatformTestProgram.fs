﻿// Learn more about F# at http://fsharp.org

open SolStone.TestRunner.Default.Framework
open SolStone.Core.SharedTypes
open SolStone.Reporters.Console.Reporter
open SolStone.TestBuilder.Scripting

let createTest name fn = 
    {
        TestContainerPath = []
        TestName = name
        TestFunction = fn
    }

let createSuccessfullTest name =
    createTest name (fun () -> Success)

let createFailingTest name failure =
    createTest name (fun () -> Failure failure)

let getSimpleTestName test =
    test.TestName


[<EntryPoint>]
let main _argv =
    let tests = 
        product "SolStone" (
            suite "Default Test Execution" [
                "Shows a successful test as being successfull"
                    |> testedWith (fun () ->
                        let testCase = createSuccessfullTest "A passing test"
                        let result = executer [testCase] |> fun result -> result.Successes |> List.head
                    
                        let expected : string = testCase.TestName
                        let actual : string = result.TestName

                        actual |> expectsToBe expected
                    )

                "Shows a failed test as failing"
                    |> testedWith (fun () ->
                        let failure = GeneralFailure "Bad Test"
                        let testCase = createFailingTest "A passing test" failure

                        let result = executer [testCase] |> fun result -> result.Failures |> List.head
                        let expected = testCase.TestName, failure
                        let actual = 
                            match result with
                            | test, testResult ->
                                test.TestName, testResult

                        actual |> expectsToBe expected
                    )

                "Multiple tests run in random order"
                    |> testedWith (fun () ->
                        let testCase1 = createSuccessfullTest "A"
                        let testCase2 = createSuccessfullTest "B"
                        let testCase3 = createSuccessfullTest "C"

                        let resultSeedA, resultA = 45   |> executerWithSeed [testCase1; testCase2; testCase3] |> fun result -> result.Seed, result.Successes |> List.map getSimpleTestName
                        let resultSeedB, resultB = 1889 |> executerWithSeed [testCase1; testCase2; testCase3] |> fun result -> result.Seed, result.Successes |> List.map getSimpleTestName
                        let _, resultC = 45   |> executerWithSeed [testCase1; testCase2; testCase3] |> fun result -> result.Seed, result.Successes |> List.map getSimpleTestName

                        resultA 
                            |> expectsToNotBe resultB
                            |> andAlso 
                                expectsToBe resultA resultC
                            |> andAlso
                                expectsToBe resultSeedA (Some 45)
                            |> andAlso
                                expectsToBe resultSeedB (Some 1889)
                    )
            ]
        )

    let result = tests |> executer
    let failedTestCount = result |> getFailCount

    "SolStone Default Test Executioner Tests" |> printHeader
    result |> report |> ignore

    failedTestCount
