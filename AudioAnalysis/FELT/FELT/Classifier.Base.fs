namespace FELT.Classifiers
    open FELT.Core

    [<AbstractClass>]
    type ClassifierBase() = class
        abstract member Classify : Data * Data -> obj
        end
        