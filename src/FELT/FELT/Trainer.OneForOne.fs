namespace FELT.Trainers
    open FELT.Trainers

    type OneForOneTrainer() =
        inherit TrainerBase()
        
        override this.Train data =
            data
        
