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
        TransformElement(feature, newName, using) = class
            inherit ConfigurationElement()
            do
                base.["feature"] <- feature
                base.["newName"] <- newName
                base.["using"] <- using
            [<ConfigurationProperty("feature", IsRequired = true)>]
            member this.Feature
                with get() = base.["feature"] :?> string
                and set(value : string) = base.["feature"] <- value


            [<ConfigurationProperty("newName", IsRequired = true)>]
            member this.NewName
                with get() = base.["newName"] :?> string
                and set(value : string) = base.["newName"] <- value


            [<ConfigurationProperty("using", IsRequired = true)>]
            member this.Using
                with get() = base.["using"] :?> string
                and set(value : string) = base.["using"] <- value

            new () = TransformElement("", "", "")
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

