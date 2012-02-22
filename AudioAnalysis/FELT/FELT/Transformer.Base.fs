namespace FELT.Tranformers
    open MQUTeR.FSharp.Shared

    
    [<AbstractClass>]
    type TransformerBase() = class
        
        abstract member Transform : Data -> Data -> Data * Data

        end
        