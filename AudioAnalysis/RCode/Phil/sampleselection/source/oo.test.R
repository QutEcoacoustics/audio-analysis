
OutputState <- setRefClass("OutputState", 
                           fields = list(state = 'list'), 
                           methods = list(
                               new = function () {
                                   state$a <<- 'b'
                                   
                               }
                               ))
g.output.state <- OutputState$new()

PrintOutput <- function (msg) {
    
    last.msg <- g.output.state$state$last.msg

    if (!is.null(last.msg) && msg == last.msg) {
        return('You just printed that')
    }
    
    
    print(msg)
    
    g.output.state$state$last.msg <- msg
    
    return(TRUE)
    
}

g.state <- list()

test1 <- function (msg) {
    
    g.state$one <- msg
    g.state$two <<- msg
    
}





