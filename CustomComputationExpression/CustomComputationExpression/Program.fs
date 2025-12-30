open System
open CustomComputationExpression

let mutable failAction2 = true

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

let main () =
    asyncRetry {
        withRetries
            [ (TimeSpan.FromSeconds 1.0)
              (TimeSpan.FromSeconds 2.0)
              (TimeSpan.FromSeconds 4.0) ]

        withAction asyncAction1
        withAction asyncAction2

        let! _ =
            async {
                printfn "Attempting inline action 1..."
                return Ok 5
            }

        let! _ =
            asyncRetry {
                printfn "Attempting inline action 2..."
                return Ok 10
            }

        return ()
    }

main () |> AsyncRetry.run |> Async.RunSynchronously |> printfn "Result: %A"
