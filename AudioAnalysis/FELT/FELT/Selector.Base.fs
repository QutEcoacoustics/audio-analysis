namespace FELT.Selectors
    open MQUTeR.FSharp.Shared

    
    [<AbstractClass>]
    type SelectorBase() = class
        

        abstract member Pick : Data -> Data

        end
        