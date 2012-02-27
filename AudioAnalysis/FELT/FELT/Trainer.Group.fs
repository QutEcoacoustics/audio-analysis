namespace FELT.Trainers
    open FELT.Trainers
    open System
    open Microsoft.FSharp.Collections
    open MQUTeR.FSharp.Shared.CSV
    open MQUTeR.FSharp.Shared
    open MathNet.Numerics
    open MQUTeR.FSharp.Shared.StringStats
    


    type GroupTrainer() =
        inherit TrainerBase()

        let emptyDT = Map.empty<ColumnHeader, Value array> 

        let grp index (state:Map<_, index list>) ele =
            if state.ContainsKey(ele) then
                state.Add(ele, (index :: state.[ele]))
            else
                state.Add(ele, [index])
        
        /// given an array of values to average, average them
        abstract member AvgValue: Value array -> Value
            default this.AvgValue (xs: Value array) : Value =
                match xs with
                | IsNumbers ns -> 
                
                    let n = ( ns |> Array.average )
                    upcast n
                | IsTexts ss ->
                    let ss' = averageStrings ss
                    upcast new AverageText(ss' |> head |> fst , ss') 
                | _ -> failwith "not implemented"

        /// this function deals with the data from one column
        member private this.aggregator (grps:Map<Class, index list>) (state:Map<ColumnHeader, Value array>) columnName (values: Value array) = 

            let avg (i: index list) =
                (values.getValues i) |> this.AvgValue   

            // for each group, pick out all the values (by index) and average them
            let values' = Map.foldBack (fun _ indexes list -> avg(indexes) :: list) grps list.Empty |> List.toArray

            // we are then left with all the elements for this column
            Map.add columnName values' state

        abstract member PostProcess : Map<ColumnHeader, Value array> -> Map<ColumnHeader, Value array>
            default this.PostProcess instances = 
                instances

        override this.Train (data: Data) : Data =
            // groups by class
            let c = data.Classes
            
            let groupedClasses = Array.foldi grp Map.empty<Class, int list> c
            let agFunc = this.aggregator groupedClasses

            // then run aggregator function over all other values
            let avgValuesForAllColumns = Map.fold agFunc emptyDT data.Instances

            // optional post-processing step used in sub classes
            let instances = this.PostProcess avgValuesForAllColumns
            
            let data' = {data with Classes = (Map.keys groupedClasses); Instances = (instances)}
            data'