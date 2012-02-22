namespace FELT.Runner

    open System
    open System.Configuration
    open System.Xml

    type TransformsConfig() = class
        inherit ConfigurationSection()

        //let mutable transformationsValue = base.[""]

        [<ConfigurationProperty("", IsDefaultCollection = true)>]
        member this.Transformations
            with get() = base.[""] :?> TransformCollection
           // and set(value) = transformationsValue <- value

        end
    and
        TransformElement() = class
            inherit ConfigurationElement()

            let mutable featureValue = String.Empty
            let mutable nameValue = String.Empty
            let mutable usingValue = String.Empty

            [<ConfigurationProperty("feature", IsRequired = true)>]
            member this.Feature
                with get() = featureValue
                and set(value) = featureValue <- value


            [<ConfigurationProperty("newName", IsRequired = true)>]
            member this.NewName
                with get() = nameValue
                and set(value) = nameValue <- value


            [<ConfigurationProperty("using", IsRequired = true)>]
            member this.Using
                with get() = usingValue
                and set(value) = usingValue <- value


            end

    and
        TransformCollection() = class
            inherit ConfigurationElementCollection()

            override this.CreateNewElement() = upcast(new TransformElement())

            override this.GetElementKey(element:ConfigurationElement) =
                upcast (element :?> TransformElement).Feature

    //        override this.CollectionType
    //            with get() = ConfigurationElementCollectionType.BasicMap
    //
    //        override this.ElementName
    //            with get() = "transform"
            end

