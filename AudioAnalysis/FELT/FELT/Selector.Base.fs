namespace FELT.Selectors
    open FELT.Core

    
    [<AbstractClass>]
    type SelectorBase() = class
        
        abstract member Pick : Data -> Data

        end
        