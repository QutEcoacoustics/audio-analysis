namespace FELT.Runner

    open System
    open System.Configuration
    open System.Xml

    type AnalysesConfig() = class
        inherit ConfigurationSection()

        [<ConfigurationProperty("", IsDefaultCollection = true)>]
        member this.Analyses
            with get() = base.[""] :?> AnalysisCollection
            and set(value:AnalysisCollection) =  base.[""]  <- value

        end
    and
        Analysis(name) = class
            inherit ConfigurationElement()
            do
                base.["name"] <- name

            [<ConfigurationProperty("name", IsRequired = true)>]
            member this.Name
                with get() = base.["name"] :?> string
                and set(value : string) = base.["name"] <- value



            new () = Analysis("")
            end

    and
        [<ConfigurationCollection(typeof<Analysis>, CollectionType = ConfigurationElementCollectionType.BasicMapAlternate)>] 
        AnalysisCollection() = class
            inherit ConfigurationElementCollection()
            
            let itemName = "analysis"


            override this.CreateNewElement() = upcast(new Analysis())

            override this.GetElementKey(element:ConfigurationElement) =
                upcast (element :?> Analysis).Name

            override this.CollectionType
                with get() = ConfigurationElementCollectionType.BasicMapAlternate
    
            override this.ElementName
                with get() = itemName

            override this.IsElementName(elementName) =
                itemName = elementName


            end

