
namespace FELT.Selectors
    open FELT.Selectors
    open MathNet.Numerics.Random
    open Microsoft.FSharp.Collections
    open MQUTeR.FSharp.Shared

    /// <summary>
    ///
    /// </summary>
    
    type RandomiserSelector() =
        inherit SelectorBase()
        
        override this.Pick data =
            // initiate random vector
            let labels = data.Classes
            let dlength = labels.Length

            let rng = new SystemCryptoRandomNumberGenerator()
            let nxt () = rng.Next dlength
            let indexes = Array.init dlength id

            // mutation...
            Array.iteri (fun i x -> 
                let ri = nxt()
                let tmp = indexes.[ri]
                indexes.[ri] <- x
                indexes.[i] <- tmp
                ) 
                indexes

            let resortedLabels = Array.foldBack (fun ele state -> labels.[ele] :: state ) indexes List<Class>.Empty
            
            {data with Classes = (List.toArray resortedLabels)}