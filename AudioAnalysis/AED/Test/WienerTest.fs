open Common
open Util.Array2 // TODO should a2FloatEquals go in Test/Common.fs instead of Array2.fs
open QutSensors.AudioAnalysis.AED.Wiener
open Xunit

[<Fact>]
let testWiener () = 
    let i1 = loadTestFile "I1.txt"
    let i2 = wiener2 5 i1
    let i2m = loadTestFile "I2.txt"
    Assert.True (a2FloatEquals i2 i2m 0.00001)