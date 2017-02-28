#tags <- GetTags()

require('plyr')
require('Matrix')

# constructs a matrix of 1s and 0s where column number is species and row number is minute, and 1 is when that species is in that minute
tagsToMatrix <- function (tags) {
    
    sp.id.map <- getIdMap(tags$species.id)
    min.id.map <- getIdMap(tags$min)
    
    m <- matrix(FALSE, nrow = nrow(min.id.map), ncol = nrow(sp.id.map))
    
    for (i in 1:nrow(tags)) {

        row.num <- match(tags$min[i], min.id.map$id)
        col.num <- match(tags$species.id[i], sp.id.map$id)
        m[row.num,col.num] <- TRUE
        
    }
    
    return(m)
    
}

# row by row, checks if a row is a subset of another row, and if so, removes it
removeSubsets <- function (m) {
    
    remove <- rep(FALSE, nrow(m))
    
    for (r1 in 1:(nrow(m)-1)) {
        if (remove[r1]) {
            next()
        }
        for (r2 in (r1+1):nrow(m)) {
            cat(paste(r1, ' ',r2,' : '))
            if (!remove[r2]) {
                if (isSubset(m[r1,],m[r2,])) {
                    remove[r1] <- TRUE
                    break()
                }
            }
        }
    }
    
    return(m[!remove,])
}


isSubset <- function (a,b) {
    return(all((a | b) == b))
}


 getIdMap <- function (x) {
     x <- unique(x)
     return(data.frame(id = x, temp.id = 1:length(x)))
 }