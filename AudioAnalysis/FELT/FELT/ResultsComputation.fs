namespace FELT.Results
    open System.Reflection
    
    // distributions graph
    // species composition


    type ResultsComputation() = class
        
        member this.Calculate someStuff =
            
            (* have to do lots of stuff in this class.
                - ensure output directory exists
                - output csv files
                    - 
                - output excel file
                    - results summary
                        - 
                    - major results (CSV files) as work sheets
                    - Graphs
                        - 
            *)

            let version = Assembly.GetAssembly(typeof<ResultsComputation>).FullName


            ()
        end
