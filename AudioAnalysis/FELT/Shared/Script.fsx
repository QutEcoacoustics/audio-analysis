// This file is a script that can be executed with the F# Interactive.  
// It can be used to explore and test the library project.
// Note that script files will not be part of the project build.

#load "Operators.fs"
#load "CSV.fs"

open Felt.Shared.Utilities
open Felt.Shared.CSV

rowToList "Hello, this is  a,test,\"hell,lo\", to see,, what happen, s" ',';;