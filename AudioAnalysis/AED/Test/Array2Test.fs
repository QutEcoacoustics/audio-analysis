#light

open Util.Array2
open Xunit // TODO put dll in project and fix reference?

[<Fact>]
let testNeighborhoodBounds () =
    let a = Array2.create 100 100 0
    Assert.Equal(Util.Array2.neighborhoodBounds a 5 0 0, (0, 0, 3, 3))
    Assert.Equal(Util.Array2.neighborhoodBounds a 5 1 0,  (0, 0, 4, 3))
    Assert.Equal(Util.Array2.neighborhoodBounds a 5 2 0, (0, 0, 5, 3))
    Assert.Equal(Util.Array2.neighborhoodBounds a 5 3 0, (1, 0, 5, 3))
    Assert.Equal(Util.Array2.neighborhoodBounds a 5 3 3, (1, 1, 5, 5))
    Assert.Equal(Util.Array2.neighborhoodBounds a 5 97 0, (95, 0, 5, 3))
    Assert.Equal(Util.Array2.neighborhoodBounds a 5 98 0, (96, 0, 4, 3))
    Assert.Equal(Util.Array2.neighborhoodBounds a 5 99 0, (97, 0, 3, 3))