namespace FELT.Transformers
    open MQUTeR.FSharp.Shared

    
    [<AbstractClass>]
    type TransformerBase() = class
        
        abstract member Transform : Data -> Data -> Data * Data

        end
        