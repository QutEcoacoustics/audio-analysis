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

 
    type IntegerZ5 = 
        | Z5 of int
        member z.ToInt32() =  
          let (Z5 n) = z in n
        override z.ToString() = 
          sprintf "%d (mod 5)" (z.ToInt32())

        static member Create(n) = 
          let z5 = n % 5
          Z5(max ((z5 + 5) % 5) z5)

        static member (+) (Z5 a, Z5 b) = IntegerZ5.Create(a + b)
        static member (-) (Z5 a, Z5 b) = IntegerZ5.Create(a - b)
        static member (*) (Z5 a, Z5 b) = IntegerZ5.Create(a * b)
        static member Zero = Z5 0
        static member One  = Z5 1

    [<AutoOpen>]
    module IntegerZ5TopLevelOperations = 
      let inline z5 a = IntegerZ5.Create(int a)
    
    module NumericLiteralZ = 
      let FromZero () = Z5 0
      let FromOne  () = Z5 1 
      let FromInt32 a = IntegerZ5.Create(a%5)
      let FromInt64 a = IntegerZ5.Create(int(a%5L))


    module IntegerZ5Associations = 
      let IntegerZ5Numerics = 
        { new INumeric<IntegerZ5> with 
             member z.Zero = IntegerZ5.Zero
             member z.One = IntegerZ5.One
             member z.Add(a,b) = a + b
             member z.Subtract(a,b) = a - b
             member z.Multiply(a,b) = a * b
             member z.Equals(Z5 a, Z5 b) = (a = b)
             member z.Compare(Z5 a, Z5 b) = compare a b
             member z.Negate(a) = 0Z - a
             member z.Abs(a) = a
             member z.Sign(Z5 a) = System.Math.Sign(a)
             member z.ToString(Z5 n,fmt,fmtprovider) = 
               n.ToString(fmt,fmtprovider) + " (mod 5)"
             member z.Parse(s,numstyle,fmtprovider) = 
               z5 (System.Int32.Parse(s,numstyle,fmtprovider)) }

      let Init() = 
        GlobalAssociations.RegisterNumericAssociation IntegerZ5Numerics