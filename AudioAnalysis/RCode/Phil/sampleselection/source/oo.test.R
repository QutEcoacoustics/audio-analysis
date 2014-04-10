

setClass("Output",
              representation(l1 = "character",
                             l2 = "character",
                             l3 = "character",
                             test = "function"))

output.constructor <- function () {
    new("Output", l1 = "a", l2 = "b", l3 = "b", test = function () { print(l1) }) 
}

output <- output.constructor()