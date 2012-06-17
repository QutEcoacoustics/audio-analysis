namespace MQUTeR.FSharp.Shared
    // https://gist.github.com/2071722
    module Serialization =
        

        open Microsoft.FSharp.Reflection
        open System.IO
        open System.Reflection
        open System.Runtime.Serialization
        open System.Runtime.Serialization.Formatters.Binary
        open System.Runtime.Serialization.Json
        open System.Text
        open System.Xml
        open System.Xml.Serialization

        let toString = System.Text.Encoding.ASCII.GetString
        let toBytes (x : string) = System.Text.Encoding.ASCII.GetBytes x

        // #region Json

        let serializeJson<'a> (x : 'a) = 
            let jsonSerializer = new DataContractJsonSerializer(typedefof<'a>)

            use stream = new MemoryStream()
            jsonSerializer.WriteObject(stream, x)
            toString <| stream.ToArray()

        let deserializeJson<'a> (json : string) =
            let jsonSerializer = new DataContractJsonSerializer(typedefof<'a>)

            use stream = new MemoryStream(toBytes json)
            jsonSerializer.ReadObject(stream) :?> 'a

        // #endregion

        // #region XML

        let serializeXml<'a> (x : 'a) =
            let xmlSerializer = new DataContractSerializer(typedefof<'a>)

            use stream = new MemoryStream()
            xmlSerializer.WriteObject(stream, x)
            toString <| stream.ToArray()

        let deserializeXml<'a> (xml : string) =
            let xmlSerializer = new DataContractSerializer(typedefof<'a>)

            use stream = new MemoryStream(toBytes xml)
            xmlSerializer.ReadObject(stream) :?> 'a

        // #endregion

        // #region Binary

        let serializeBinary<'a> (x :'a) =
            let binFormatter = new BinaryFormatter()

            use stream = new MemoryStream()
            binFormatter.Serialize(stream, x)
            stream.ToArray()

        let serializeBinaryToFile<'a> (x :'a) file =
            let binFormatter = new BinaryFormatter()

            use stream = File.Open(file, FileMode.CreateNew)
            binFormatter.Serialize(stream, x)
            

        let deserializeBinary<'a> (arr : byte[]) =
            let binFormatter = new BinaryFormatter()

            use stream = new MemoryStream(arr)
            binFormatter.Deserialize(stream) :?> 'a

        let deserializeBinaryStream<'a> (stream : FileStream) =
            let binFormatter = new BinaryFormatter()

            binFormatter.Deserialize(stream) :?> 'a

        // #endregion