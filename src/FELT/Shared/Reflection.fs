namespace System.Reflection
    open System
    open Microsoft.FSharp.Reflection
    open Microsoft.FSharp.Metadata
    open Microsoft.FSharp.Quotations
    open System.Reflection

    [<AutoOpen>]
    module ReflectionHelpers =
        let printProperties x =
            let t = x.GetType()
            let properties = t.GetProperties()
            printfn "-----------"
            printfn "%s" t.FullName
            properties |> Array.iter (fun prop ->
                if prop.CanRead then
                    let value = prop.GetValue(x, null)
                    printfn "%s: %O" prop.Name value
                else
                    printfn "%s: ?" prop.Name)

        let getPropertyInternal x prop : 'a =
            let t = x.GetType()
            let ps = t.GetProperties(BindingFlags.Instance ||| BindingFlags.NonPublic)
            let p3 = Array.pick (fun (pi: PropertyInfo) -> if pi.Name = prop then Some(pi) else None) ps
            p3.GetValue(x, null) :?> 'a
        
        let iterateProperties x f =
            let t = x.GetType()
            let properties = t.GetProperties()

            // t.FullName
            let props = properties |> Array.map (fun prop ->
                                                        if prop.CanRead then
                                                            let value = prop.GetValue(x, null)
                                                            f prop.Name value
                                                        else
                                                            (prop.Name, null)
                                                )
            t.FullName, props


        /// Get the type of a module. This works by using some member in the module type to refer to the module.
        /// <example> typeofM SomeModule.someBinding returns typeof<SomeModule> </example>
        let typeofM m =
            
            let t = m.GetType()

            t

        let getNameOfModuleBinding (x:Expr<_>) =
            match x with 
            | Patterns.PropertyGet (e, pi, _) -> pi.Name 
            | _ -> failwith "Does not match property get pattern"

        let getAllModuleBindings (m : Type) =
            
            if not (FSharpType.IsModule m) then
                raise (new ArgumentException("This function is designed to work only on modules"))
            
            let props = m.GetProperties(Reflection.BindingFlags.Public ||| Reflection.BindingFlags.Static)
            let makeCaller (pi:PropertyInfo)() = pi.GetValue(null, null)
            Array.map (fun (pi:PropertyInfo) -> (pi.Name, makeCaller pi ,pi)) props
