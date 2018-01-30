
namespace Felt.Results

    open System.Reflection
    open System
    open MQUTeR.FSharp.Shared
    open MQUTeR.FSharp.Shared.Maths
    open Microsoft.FSharp.Collections
    open Microsoft.FSharp.Core
    open System.IO
    open FELT.Classifiers
    open MQUTeR.FSharp.Shared.IO

    module ResultCombiner =
        open FELT.Classifiers

        let twoToOne (combinator: int -> Result -> Result -> Result) (resultA : LazyResult) (resultB: LazyResult) : LazyResult =
            fun  i -> combinator i (resultA i) (resultB i)

