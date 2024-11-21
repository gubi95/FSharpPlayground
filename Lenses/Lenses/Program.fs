open FSharpPlus
open FSharpPlus.Lens

type Person = 
    { Name: string
      Address: Address }
and Address = { City: string }    
    
module Address =
    let _city f a = f a.City <&> fun x -> { a with City = x }
    
module Person =
    let _name f p =
        f p.Name <&> fun x -> { p with Name = x }
        
    let _address f p =
        f p.Address <&> fun x -> { p with Address = x }
        
    let _city = Address._city >> _address

[<EntryPoint>]
let main _ =
    let tuple = "Some", "value"
    
    printfn $"%A{tuple}^.2 = %A{tuple^._2}"
    
    printfn $"""setl _2 "edited-value" %A{tuple} = %A{(setl _2 "edited-value" tuple)}"""
    
    let nestedTuple = "Some", ("nested", "value")
    
    printfn $"%A{nestedTuple}^.(_2 << _1) = %A{(nestedTuple^.(_1 >> _2))}"
    
    printfn $"""setl (_1 >> _2) "edited-value" %A{nestedTuple} = %A{(setl (_1 >> _2) "edited-value" nestedTuple)}"""
    
    printfn $""""test"^.to' length = %i{"abc"^.to' length}"""
    
    printfn $"""nestedTuple^.(_1 >> _2) |> fun x -> x + " appended string" = %A{(nestedTuple^.(_1 >> _2) |> fun x -> x + " appended string")}"""
    
    let person: Person = { Name = "John Smith"
                           Address = { City = "Los Angeles" } }
    
    printfn $"""setl Person._name "edited name" %A{person} = %A{(setl Person._name "edited name" person)}"""
    printfn $"""over Person._name (fun x -> x + " appended to name") %A{person} = %A{(over Person._name (fun x -> x + " appended to name") person)}"""
    
    printfn $"""setl Person._city "edited city" %A{person} = %A{(setl Person._city "edited name" person)}"""
    
    0