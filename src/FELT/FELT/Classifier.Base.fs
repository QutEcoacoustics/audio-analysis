namespace FELT.Classifiers
    open MQUTeR.FSharp.Shared

    type TrainingIndex = int
    type Distance = float
    type Result = (Distance * TrainingIndex)[]
    type LazyResult = (int -> Result)

    type ClassifierResult =
        | Nothing
        | Function of LazyResult
        | ResultSeq of seq<Result>
        | ResultArray of Result[]

    [<AbstractClass>]
    type ClassifierBase() = class
        abstract member Classify : Data -> Data -> ClassifierResult
        end
        

    //module Helpers =
        