namespace FELT.Trainers
    open FELT.Trainers
    open System
    open Microsoft.FSharp.Collections
    open MQUTeR.FSharp.Shared.CSV
    open MQUTeR.FSharp.Shared
    open MathNet.Numerics


    type GroupTrainer(agg) =
        inherit TrainerBase()

        let grp index (state:Map<_, index list>) ele =
            if state.ContainsKey(ele) then
                state.Add(ele, (index :: state.[ele]))
            else
                state.Add(ele, [index])

            

        let avgValue (xs: Value array) : Value =
            
            failwith "not implemented"

        /// some sort of histogram style thing
        let averageStrings ss =
            
            ss
            
                              

        /// For column 
        let aggregator (grps:Map<Class, index list>) state columnName (values: Value array) = 
            // inside this function is the data from one column

            let avg (i: index list) =
                (values.getValues i) |> avgValue   

            // for each group, pick out all the values and average them
            let values = Map.fold (fun list _ indexes -> avg(indexes) :: list) list.Empty grps |> List.toArray

            // we are then left with all the elements for this column
            Map.add columnName values state


        override this.Train (data: Data) : Data =
            // groups by class
            let c = data.Classes
            
            let groupedClasses = Array.foldi grp Map.empty<Class, int list> c
            let agFunc = aggregator groupedClasses

            // then run aggregator function over all other values
            let avgValuesForAllColumns = Map.fold agFunc Map.empty<ColumnHeader, Value array> data.Instances
            
            let data' = {data with Instances = (avgValuesForAllColumns)}
            data'