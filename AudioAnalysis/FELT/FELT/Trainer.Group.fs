namespace FELT.Trainers
    open FELT.Trainers
    open System
    open Microsoft.FSharp.Collections

    type GroupTrainer(agg) =
        inherit TrainerBase()
        let aggregator = agg

        let grp index (state: Map<string, int list>) ele =
            if state.ContainsKey(ele) then
                state.Add(ele, (index :: state.[ele]))
            else
                state.Add(ele, [index])

        override this.Train data =
            // groups by class
            let c = data.Classes
            
            let groups = Array.foldi grp Map.empty<string, int list> c
            
            // then run aggregator function over all other values



            data