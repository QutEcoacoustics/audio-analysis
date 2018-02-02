namespace System

        [<AutoOpen>]
        module NumericLiteralG =
            
            type IntConverterDynamicImplTable<'t>() =
              static let result : int -> 't =
                let ty = typeof< 't> //'
                if   ty.Equals(typeof<sbyte>)      then sbyte      |> box |> unbox
                elif ty.Equals(typeof<int16>)      then int16      |> box |> unbox
                elif ty.Equals(typeof<int32>)      then int        |> box |> unbox
                elif ty.Equals(typeof<int64>)      then int64      |> box |> unbox
                elif ty.Equals(typeof<nativeint>)  then nativeint  |> box |> unbox
                elif ty.Equals(typeof<byte>)       then byte       |> box |> unbox
                elif ty.Equals(typeof<uint16>)     then uint16     |> box |> unbox
                elif ty.Equals(typeof<char>)       then char       |> box |> unbox
                elif ty.Equals(typeof<uint32>)     then uint32     |> box |> unbox
                elif ty.Equals(typeof<uint64>)     then uint64     |> box |> unbox
                elif ty.Equals(typeof<unativeint>) then unativeint |> box |> unbox
                elif ty.Equals(typeof<decimal>)    then decimal    |> box |> unbox
                elif ty.Equals(typeof<float>)      then float      |> box |> unbox
                elif ty.Equals(typeof<float32>)    then float32    |> box |> unbox
                else 
                  let m = 
                    try ty.GetMethod("op_Implicit", [| typeof<int> |])
                    with _ -> ty.GetMethod("op_Explicit", [| typeof<int> |])
                  let del =
                    System.Delegate.CreateDelegate(typeof<System.Func<int,'t>>, m)
                    :?> System.Func<int,'t>
                  del.Invoke |> box |> unbox
              static member Result = result

            let inline FromZero() = LanguagePrimitives.GenericZero
            let inline FromOne() = LanguagePrimitives.GenericOne

            let inline constrain< ^t, ^u when (^t or ^u) : (static member op_Explicit : ^t -> ^u)> () = ()
            let inline FromInt32 i : ^t = 
              constrain<int, ^t>()
              IntConverterDynamicImplTable.Result i


       