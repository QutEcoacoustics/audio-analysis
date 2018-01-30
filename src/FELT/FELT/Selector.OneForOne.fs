namespace FELT.Selectors
    open FELT.Selectors
    open MQUTeR.FSharp.Shared

    type OneForOneSelector() =
        inherit SelectorBase()

        override this.Pick data =
            data
        
        

