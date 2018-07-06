namespace Thingstead.Engine

open Thingstead.Types

module Tests = 
    let defaultTestExecutor (env: Environment) testMethod : TestResult =
        testMethod env

    let private handleUnsafeTestAction unsafeAction (environment: Environment) exceptionHandler =
        try
            unsafeAction environment
        with
        | e -> e |> exceptionHandler

    let private executeTests testRunner (environment: Environment) (testMethod: Environment -> TestResult) =
        let runner = fun env -> testRunner env testMethod
        handleUnsafeTestAction runner environment (ExceptionFailure >> Failure)

    let private runAsBookend (environment: Environment) f =
        handleUnsafeTestAction f environment (PrePostExceptionFailure >> Failure)

    let bookEndProcess (environment: Environment) before (action: Environment -> TestResult) after = 
        let doIt (prevResult: EngineResult<Environment, FailureType>) =
            match prevResult with
            | Success env ->
                match action env with
                | Success _ -> Success env
                | Failure result -> Failure result
            | _ -> prevResult
            
        let doAfter prevResult =
            let a env = 
                match after |> runAsBookend env with
                | Success _ -> Success ()
                | Failure error ->
                    error
                    |> AfterFailure
                    |> Failure
                    
            match prevResult with
            | Success env ->
                a env
            | Failure error ->
                a environment
                |> combine (Failure error)

        let doBefore env =
            match before |> runAsBookend env with
            | Success env -> Success env
            | Failure error -> 
                error
                |> BeforeFailure
                |> Failure

        environment
        |> doBefore
        |> doIt
        |> doAfter


    let runTestWith environment executor (test: Test) =
        let testFunction = (fun env -> 
            test.TestMethod |> executeTests executor env
        )
        
        bookEndProcess environment (test.Before) testFunction (test.After)
        