module  MQUTeR.FSharp.Shared.Equality
    open Microsoft.FSharp.Quotations
    open Microsoft.FSharp.Quotations.Patterns
    open Microsoft.FSharp.Collections
    open System

    let equalsOn f x (yobj:obj) =
        match yobj with
        | :? 'T as y -> (f x = f y)
        | _ -> false
 
    let hashOn f x =  hash (f x)
 
    let compareOn f x (yobj: obj) =
        match yobj with
            | :? 'T as y -> compare (f x) (f y)
            | _ -> invalidArg "yobj" "cannot compare values of different types"


    //    type tt() = class
    //        member this.A with get() = 3
    //        member this.B with get() = 4
    //        end
    //
    //    let getPropertyValue<'T when 'T : equality> p o=  
    //        match p with
    //            | PropertyGet(_,pi,_) -> pi.GetValue(o, null) :?> 'T
    //            | _ -> invalidOp "Expecting property getter expression"
    //
    //    let equals (l: Expr<'T> list) x y = 
    //        
    //        let test t = 
    //            (getPropertyValue t x) = (getPropertyValue t y)
    //    
    //        List.forall test l
    //
    //
    //
    //    let n =
    //        let e = fun x -> x.GetType() 
    //        //let v = e(3)
    //        let n (v: 'a when 'a : equality) = <@ v = v @>
            //e



    let equals x y (tests)  =
            List.forall (fun (f: 'a-> 'b) -> Unchecked.equals (f x) (f y) ) tests

    let equalsCast (x: 'T) (y:obj) tests = 
        match y with
            | :? 'T as y' -> equals x y' tests
            | _ -> false
               
    /// http://musingmarc.blogspot.com/2008/03/sometimes-you-make-hash-of-things.html
    let GetHashCode (tests)   = 
        let hs = List.map (fun a -> Unchecked.hash a) tests
        let combineHashes = List.fold (fun state h -> ((state <<< 5) + h) ^^^ h) 0
        hs |> combineHashes

    

    let inline CompareTo (x: 'T) (y: obj) (tests: ('T -> 'a ) list when 'a :> obj ) =
        match y with
            | :? 'T as y' -> 
//                match (x,y') with
//                    | (null, null) -> 0
//                    | (null, _) -> -1
//                    | (_, null) -> 1
//                    | _ ->
                        match List.tryPick (fun f -> let c = Unchecked.compare (f x) (f y') in if c = 0 then None else Some(c)) tests with
                            | None -> 0
                            | Some s -> s
   
            | _ -> invalidArg "yobj" "cannot compare values of different types"
        
        
          

