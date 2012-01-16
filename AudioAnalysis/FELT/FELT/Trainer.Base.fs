namespace FELT.Trainers
    open FELT.Core

    [<AbstractClass>]
    type TrainerBase() = class
        
        abstract member Train : Data -> Data

        end
        

