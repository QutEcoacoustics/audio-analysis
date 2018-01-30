namespace FELT.Trainers
    open MQUTeR.FSharp.Shared

    [<AbstractClass>]
    type TrainerBase() = class
        
        abstract member Train : Data -> Data

        end
        

