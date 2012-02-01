namespace FELT.Classifiers
    open MQUTeR.FSharp.Shared

    type TrainingIndex = int
    type Distance = float
    type Result = (Distance * TrainingIndex)[]

    [<AbstractClass>]
    type ClassifierBase() = class
        abstract member Classify : Data * Data -> Result array
        end
        