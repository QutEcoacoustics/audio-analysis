
module MQUTeR.FSharp.Shared.StringStats  

        /// some sort of histogram style thing
        let averageStrings ss =
            let numStrings = float <| Array.length ss
            let grp (state:Map<string, float>) (txt:Text)  =
                let str = txt.Value
                let count  = if state.ContainsKey(str) then state.[str] else 0.0
                state.Add(str, count + 1.0)

            let grps = Array.fold grp Map.empty<string, float> ss
            
            Map.toArray grps |> Array.map (fun (str, count) -> (str, count / numStrings)) |> Array.sortBy snd