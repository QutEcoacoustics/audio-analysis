namespace FELT.Trainers
    open FELT.Trainers
    open System
    open Microsoft.FSharp.Collections
    open MQUTeR.FSharp.Shared.CSV
    open MQUTeR.FSharp.Shared
    open MathNet.Numerics


    type GroupTrainer(agg) =
        inherit TrainerBase()


            

        let averageBits xs =
            stats.mean xs

        /// some sort of histogram style thing
        let averageStrings ss =
            
            ss
            
                         
            

        let aggregator state key (value: int list) = 
            let (oldData, avgData) = state
            let (dt:DataType) = oldData.Headers.[key]
            match dt with
                | DataType.Number -> {state with Numbers =  state.Numbers.Add(key, (state.Numbers.[key].getValues value) |> stats.mean)  }
                | _ -> failwith "Aggregator function not defined for type %A" dt
            

        let grp index (state: Map<string, int list>) ele =
            if state.ContainsKey(ele) then
                state.Add(ele, (index :: state.[ele]))
            else
                state.Add(ele, [index])

        override this.Train (data: Data) : Data =
            // groups by class
            let c = data.Classes
            
            let groups = Array.foldi grp Map.empty<string, int list> c
            
            let names = groups.
            // then run aggregator function over all other values
            

            let avgValues :  Map<ColumnHeader, Value array> = Map.fold aggregator data groups

            let data' = {data with Instances = (avgValues)}
            data'