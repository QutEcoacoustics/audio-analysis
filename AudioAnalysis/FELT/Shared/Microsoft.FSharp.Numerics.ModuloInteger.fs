namespace Microsoft.FSharp.Numerics
    
//    type ModuloInteger(n, limit) =
//        
//        let zNumber = let z = n % limit in (max ((z + limit) % limit) z)
//
//        member this.ToInt32() =
//            zNumber
//        member this.Limit
//            with get() = limit
//        override this.ToString() =
//            sprintf "%d (mod %d)" (this.ToInt32()) limit
//
//        static member Create(n, limit) =
//            new ModuloInteger(n, limit)
//
//        static member IsZNum (number:obj) = if number :? ModuloInteger then Option.Some(number :?> ModuloInteger) else Option.None
//        static member private LimitsMatch (a:ModuloInteger) (b:ModuloInteger) = if a.Limit = b.Limit then (a.ToInt32(), b.ToInt32(), a.Limit) else failwith "Modulo operation undefined for different modulo numbers"
//        static member (+) (a, b) = let (x, y, l) = ModuloInteger.LimitsMatch a b in  ModuloInteger.Create(x + y, l)
//        static member (-) ( a, b) = let (x, y, l) = ModuloInteger.LimitsMatch a b in  ModuloInteger.Create(x - y, l)
//        static member (*) (a, b) = let (x, y, l) = ModuloInteger.LimitsMatch a b in  ModuloInteger.Create(x * y, l)
//        static member Zero = Z5 0
//        static member One  = Z5 1
//
//   
//
//    [<AutoOpen>]
//    module IntegerZ5TopLevelOperations = 
//        let inline zNum a l = ModuloInteger.Create(int a, l)
//
//        let (|ZNum|_|) (number:obj) = if number :? ModuloInteger then Option.Some(number :?> ModuloInteger) else Option.None
//
//    module NumericLiteralZ = 
//        let FromZero () = zNUm 0
//        let FromOne  () = Z5 1 
//        let FromInt32 a = IntegerZ5.Create(a%5)
//        let FromInt64 a = IntegerZ5.Create(int(a%5L))

 
    type IntegerZ1440 = 
        | Z1440 of int
        member z.ToInt32() =  
          let (Z1440 n) = z in n
        override z.ToString() = 
          sprintf "%d (mod 1440)" (z.ToInt32())

        static member Create(n) = 
          let z1440 = n % 1440
          Z1440(max ((z1440 + 1440) % 1440) z1440)

        static member (+) (Z1440 a, Z1440 b) = IntegerZ1440.Create(a + b)
        static member (-) (Z1440 a, Z1440 b) = IntegerZ1440.Create(a - b)
        static member (*) (Z1440 a, Z1440 b) = IntegerZ1440.Create(a * b)

        /// For the moment this is defined as default integer division
        /// A proper implementation of finite field division is unecessary for our purposes
        static member (/) (Z1440 a, Z1440 b) = IntegerZ1440.Create(a / b)
        static member Zero = Z1440 0
        static member One  = Z1440 1

    [<AutoOpen>]
    module IntegerZ1440TopLevelOperations = 
      let inline z1440 a = IntegerZ1440.Create(int a)
    
    module NumericLiteralZ = 
      let FromZero () = Z1440 0
      let FromOne  () = Z1440 1 
      let FromInt32 a = IntegerZ1440.Create(a%1440)
      let FromInt64 a = IntegerZ1440.Create(int(a%1440L))


    module IntegerZ1440Associations = 
      let IntegerZ1440Numerics = 
        { new INumeric<IntegerZ1440> with 
             member z.Zero = IntegerZ1440.Zero
             member z.One = IntegerZ1440.One
             member z.Add(a,b) = a + b
             member z.Subtract(a,b) = a - b
             member z.Multiply(a,b) = a * b
             
             member z.Equals(Z1440 a, Z1440 b) = (a = b)
             member z.Compare(Z1440 a, Z1440 b) = compare a b
             member z.Negate(a) = 0Z - a
             member z.Abs(a) = a
             member z.Sign(Z1440 a) = System.Math.Sign(a)
             member z.ToString(Z1440 n,fmt,fmtprovider) = 
               n.ToString(fmt,fmtprovider) + " (mod 1440)"
             member z.Parse(s,numstyle,fmtprovider) = 
               z1440 (System.Int32.Parse(s,numstyle,fmtprovider)) }

      let Init() = 
        GlobalAssociations.RegisterNumericAssociation IntegerZ1440Numerics