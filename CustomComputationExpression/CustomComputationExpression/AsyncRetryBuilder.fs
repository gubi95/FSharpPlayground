module CustomComputationExpression

open System

type RetryState<'T, 'Error> =
    { Action: unit -> Result<'T, 'Error> Async
      Retries: TimeSpan list }

type AsyncRetryBuilder() =
    member _.Return(value) = async { return Ok value }

    member _.Bind(action, f) =
        async {
            let! result = action

            match result with
            | Ok data -> return! f data
            | Error e -> return Error e
        }

    member _.MergeSources(action1, action2) =
        async {
            let! result1 = action1

            match result1 with
            | Error e -> return Error e
            | Ok data1 ->
                let! result2 = action2

                match result2 with
                | Error e -> return Error e
                | Ok data2 -> return Ok(data1, data2)
        }

    member _.Zero() =
        { Action = fun () -> async { return Ok() }
          Retries = List.empty }

    member _.Yield(value) =
        { Action = fun () -> async { return Ok value }
          Retries = List.empty }

    member _.For(state, f) =
        async {
            let! result = state.Action()

            match result with
            | Ok data -> return! f data
            | Error e -> return Error e
        }

    [<CustomOperation("withRetry", MaintainsVariableSpace = true)>]
    member _.WithRetry(state, value: TimeSpan) =
        { state with
            Retries = state.Retries @ [ value ] }

let asyncRetry = AsyncRetryBuilder()
