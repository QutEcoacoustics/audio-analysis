namespace FELT.Trainers
    open FELT.Trainers
    open System
    open Microsoft.FSharp.Collections
    open MQUTeR.FSharp.Shared.CSV
    open MQUTeR.FSharp.Shared
    open MathNet.Numerics
    


    type GroupTrainer(agg) =
        inherit TrainerBase()

        let emptyDT = Map.empty<ColumnHeader, Value array> 

        let grp index (state:Map<_, index list>) ele =
            if state.ContainsKey(ele) then
                state.Add(ele, (index :: state.[ele]))
            else
                state.Add(ele, [index])

            
//            let avg (idxs: index list) = 
//                let select  = values. .getValues idxs
//                avgValue  (select values)


        /// some sort of histogram style thing
        let averageStrings ss =
            let numStrings = float <| Array.length ss
            let grp (state:Map<string, float>) (txt:Text)  =
                let str = txt.Value
                let count  = if state.ContainsKey(str) then state.[str] else 0.0
                state.Add(str, state.[str] + 1.0)

            let grps = Array.fold grp Map.empty<string, float> ss
            
            Map.toArray grps |> Array.map (fun (str, count) -> (str, count / numStrings)) |> Array.sortBy snd

        let avgValue (xs: Value array) : Value =
            
            match xs with
            | IsNumbers ns -> 
                
                let n = ( ns |> Array.average )
                upcast n
            | IsTexts ss ->
                let ss' = averageStrings ss
                upcast new AverageText(ss' |> head |> fst , ss') 
            | _ -> failwith "not implemented"

        
            
                              

        /// this function deals with the data from one column
        let aggregator (grps:Map<Class, index list>) (state:Map<ColumnHeader, Value array>) columnName (values: Value array) = 

            let avg (i: index list) =
                (values.getValues i) |> avgValue   

            // for each group, pick out all the values and average them
            let values' = Map.fold (fun list _ indexes -> avg(indexes) :: list) list.Empty grps |> List.toArray

            // we are then left with all the elements for this column
            Map.add columnName values' state


        override this.Train (data: Data) : Data =
            // groups by class
            let c = data.Classes
            
            let groupedClasses = Array.foldi grp Map.empty<Class, int list> c
            let agFunc = aggregator groupedClasses

            

            // then run aggregator function over all other values
            let avgValuesForAllColumns = Map.fold agFunc emptyDT data.Instances
            
            let data' = {data with Instances = (avgValuesForAllColumns)}
            data'