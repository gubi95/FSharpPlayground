open System
open CustomComputationExpression

let main () =
    asyncRetry {
        withRetry (TimeSpan.FromSeconds 1.0)
        withRetry (TimeSpan.FromSeconds 2.0)
        withRetry (TimeSpan.FromSeconds 4.0)

        let! result = async { return Ok 10 }

        return result + 5
    }

main () |> Async.RunSynchronously |> printfn "Result: %A"
