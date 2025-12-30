open System
open CustomComputationExpression

let mutable failAction2 = true
let mutable failAction3 = true

let asyncAction1 () : Result<unit, string> Async =
    async {
        printfn "Attempting action 1..."
        return Ok()
    }

let asyncAction2 () : Result<unit, string> Async =
    async {
        printfn "Attempting action 2..."

        return
            match failAction2 with
            | true ->
                failAction2 <- false
                printfn "Action 2 failed."
                Error "Action 2 failed"
            | false -> Ok()
    }

let asyncAction3 () : Result<unit, string> Async =
    async {
        printfn "Attempting action 3..."

        return
            match failAction3 with
            | true ->
                failAction3 <- false
                printfn "Action 3 failed."
                Error "Action 3 failed"
            | false -> Ok()
    }

let alwaysFailingAction () : Result<unit, string> Async =
    async {
        printfn "Attempting always failing action..."
        return Error "Always failing action failed"
    }

let main () =
    asyncRetry {
        withRetries
            [ (TimeSpan.FromSeconds 1.0)
              (TimeSpan.FromSeconds 2.0)
              (TimeSpan.FromSeconds 4.0) ]

        withAction asyncAction1
        withAction asyncAction2

        do!
            async {
                printfn "Attempting inline action 1..."
                return Ok()
            }

        do!
            asyncRetry {
                printfn "Attempting inline action 2 with retry..."

                withRetries [ (TimeSpan.FromSeconds 1.0) ]
                withAction asyncAction3

                return ()
            }
            |> AsyncRetry.run

        do! alwaysFailingAction ()

        return ()
    }

main () |> AsyncRetry.run |> Async.RunSynchronously |> printfn "Result: %A"
