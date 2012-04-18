namespace MQUTeR.FSharp.Shared
    
    // WARNING: BAD FUNCTIONAL PROGRAMMING! sTATE MUTATION!
    type MutexSwitch(intialState) =
        
        let mutable state = intialState

        member this.Flip() =
            if (state = intialState) then
                state <- not state

        member this.IsFlipped 
            with get() =
                not (state = intialState)
