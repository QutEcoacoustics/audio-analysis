namespace FELT.Classifiers
    open MQUTeR.FSharp.Shared

    [<AbstractClass>]
    type ClassifierBase() = class
        abstract member Classify : Data * Data -> obj
        end
        