module  MQUTeR.FSharp.Shared.Equality
    open Microsoft.FSharp.Quotations
    open Microsoft.FSharp.Quotations.Patterns

    let equalsOn f x (yobj:obj) =
        match yobj with
        | :? 'T as y -> (f x = f y)
        | _ -> false
 
    let hashOn f x =  hash (f x)
 
    let compareOn f x (yobj: obj) =
        match yobj with
        | :? 'T as y -> compare (f x) (f y)
        | _ -> invalidArg "yobj" "cannot compare values of different types"


    type tt() = class
        member this.A with get() = 3
        member this.B with get() = 4
        end

    let getPropertyValue<'T when 'T : equality> p o=  
        match p with
            | PropertyGet(_,pi,_) -> pi.GetValue(o, null) :?> 'T
            | _ -> invalidOp "Expecting property getter expression"

    let equals (l: Expr<'T> list) x y = 
        
        let test t = 
            (getPropertyValue t x) = (getPropertyValue t y)
    
        List.forall test l



    let n =
        let e = fun x -> x.GetType() 
        //let v = e(3)
        let n (v: 'a when 'a : equality) = <@ v = v @>
        e
    (*

     override x.Equals(yobj) =
            let t = x.Value
            match yobj with
            | :? BaseValue<'T> as y -> Unchecked.equals x.Value y.Value
            | _ -> false
    
        override x.GetHashCode() = Unchecked.hash x.Value
 
        interface System.IComparable with
            member x.CompareTo yobj =
                match yobj with
                | :? BaseValue<'T> as y -> Unchecked.compare x.Value y.Value
                | _ -> invalidArg "yobj" "cannot compare values of different types"
   
            end


            *)