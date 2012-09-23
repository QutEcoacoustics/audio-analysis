namespace Microsoft.FSharp.Core
    open System

    [<AutoOpen>]
    [<CompiledName("OperatorsExtensions")>]
    [<System.Runtime.CompilerServices.ExtensionAttribute>]
    module public Operators2 =
        let fsts tuples = Seq.map (fst) tuples

        let third3 (_,_,c) = c
        let fst3 (a,_,_) = a
        let snd3 (_,b,_) = b

    
    module Option =
        let applyifSome f x =
            if Option.isSome x then 
                x.Value |> f |> Some
            else
                None
        let fromNullable (n: _ Nullable) =
            if n.HasValue
                then Some n.Value
                else None
        let toNullable =
            function
            | None -> Nullable()
            | Some x -> Nullable(x)
        let mapToNullable f =
            function
            | None -> Nullable()
            | Some x -> Nullable(f x)

    [<AutoOpen>]
    module LanguagePrimitives =
        let inline GenericN n = NumericLiteralG.FromInt32 n

        let inline Generic2() = LanguagePrimitives.GenericOne + LanguagePrimitives.GenericOne


