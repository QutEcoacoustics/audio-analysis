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
        TransformElement(features, newName, using) = class
            inherit ConfigurationElement()
            do
                base.["features"] <- features
                base.["newName"] <- newName
                base.["using"] <- using
            [<ConfigurationProperty("features", IsRequired = true)>]
            member this.Features
                with get() = base.["features"] :?> string
                and set(value : string) = base.["features"] <- value


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
                upcast (element :?> TransformElement).Features

    //        override this.CollectionType
    //            with get() = ConfigurationElementCollectionType.BasicMap
    //
    //        override this.ElementName
    //            with get() = "transform"
            end

