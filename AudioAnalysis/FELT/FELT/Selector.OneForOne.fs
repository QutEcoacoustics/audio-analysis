namespace FELT.Selectors
    open FELT.Selectors
    open FELT.Core

    type OneForOneSelector() =
        inherit SelectorBase()

        override this.Pick data =
            data
        
        

