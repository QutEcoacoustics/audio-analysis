namespace Microsoft.FSharp.Collections
    open MQUTeR.FSharp.Shared.Utilities
    module Array =
        /// Implementation stolen from Vector<_>.foldi
        let inline foldi folder state (array:array<_>) =
            let mA = array.zeroLength
            let mutable acc = state
            for i = 0 to mA do acc <- folder i acc array.[i]
            acc

