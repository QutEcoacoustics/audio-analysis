
namespace Felt.Results
    module ResultCombiner =
        open FELT.Classifiers

        let twoToOne (combinator: int -> Result -> Result -> Result) (resultA : LazyResult) (resultB: LazyResult) : LazyResult =
            fun  i -> combinator i (resultA i) (resultB i)

