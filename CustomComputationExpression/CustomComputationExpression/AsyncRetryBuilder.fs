module CustomComputationExpression

open System

type RetryState<'T, 'Error> =
    { Action: unit -> Result<unit, 'Error> Async
      Retries: TimeSpan list }

let private runWithRetries<'T, 'Error>
    (action: unit -> Result<'T, 'Error> Async)
    (retries: TimeSpan list)
    : Result<'T, 'Error> Async =
    let rec attempt (retriesLeft: TimeSpan list) =
        async {
            printfn "Attempting execution..."
            let! result = action ()

            match result with
            | Ok data -> return Ok data
            | Error e ->
                match retriesLeft with
                | [] -> return Error e
                | waitTime :: rest ->
                    do! Async.Sleep(int waitTime.TotalMilliseconds)
                    return! attempt rest
        }

    attempt retries

module AsyncRetry =
    let run (state: RetryState<_, _>) =
        runWithRetries state.Action state.Retries

type AsyncRetryBuilder() =
    member _.Return(value) = async { return Ok value }

    member _.Bind(action: Result<_, _> Async, f) =
        async {
            let! result = action

            match result with
            | Ok data -> return! f data
            | Error e -> return Error e
        }

    member _.For(state, f) =
        let newAction =
            fun () ->
                async {
                    let! result = state.Action()

                    match result with
                    | Ok _ -> return! f ()
                    | Error e -> return Error e
                }

        { state with Action = newAction }

    member _.Yield(()) = ()

    [<CustomOperation("withRetries")>]
    member _.WithRetries((), value: TimeSpan list) =
        { Action = (fun () -> async { return Ok() })
          Retries = value }

    [<CustomOperation("withAction")>]
    member _.WithAction(state: RetryState<'T, 'Error>, newAction: unit -> Result<unit, 'Error> Async) =
        { state with
            Action =
                fun () ->
                    async {
                        let! result = state.Action()

                        match result with
                        | Ok _ -> return! newAction ()
                        | Error e -> return Error e
                    } }

let asyncRetry = AsyncRetryBuilder()
