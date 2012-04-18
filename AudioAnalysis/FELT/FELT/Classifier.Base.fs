namespace FELT.Classifiers
    open MQUTeR.FSharp.Shared

    type TrainingIndex = int
    type Distance = float
    type Result = (Distance * TrainingIndex)[]
    type ClassifierResult =
        | Nothing
        | Function of (int -> Result)
        | ResultSeq of seq<Result>
        | ResultArray of Result[]

    [<AbstractClass>]
    type ClassifierBase() = class
        abstract member Classify : Data * Data -> ClassifierResult
        end
        

    //module Helpers =
        