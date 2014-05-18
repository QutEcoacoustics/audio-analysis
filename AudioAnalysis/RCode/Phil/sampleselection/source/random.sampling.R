g.dawn.from = 315
g.dawn.to = 495

IsDawn <- function (mins, all.dawn = FALSE) {
    # given a list of mins, returns weather any of the dawn are within the mins
    # or if all.dawn is TRUE, whether all of dawn is within mins
    
    dawn <- g.dawn.from:g.dawn.to
    
    intersection <- intersect(mins, dawn)
    
    if (all.dawn && length(intersection) == length(dawn)) {
        return(TRUE)
    } else if (all.dawn == FALSE & length(intersection) > 0) {
        return(TRUE)
    } else {
        return(FALSE)
    }
    
    
}

RandomSamples <- function (speciesmins = NA, species.in.each.sample= NA, 
                           mins = NA, 
                           num.repetitions = 100, 
                           dawn.first = TRUE, 
                           dawn.from = 315, dawn.to = 495, 
                           block.size = 1) {
    # repeatedly performs a sample selection from random selection of dawn minutes
    # 
    # Args:
    #   speciesmins: data.frame; list of annotations
    #   species.in.each.sample: list; the species present in each minute of the target mins
    #   num.repetitions: int; how many times to run random sampling (more produces a smoother average)
    #   mins: integer vector; the minute ids to include
    #   dawn.first: boolean; whether to first use dawn minutes for selection
    #   dawn.from, dawn.to: int; the start and end minutes of the day for dawn
    #   block.size: int; 1 minute samples will always be chosen in groups of this many. This allows us to compare say, 30 mins randomly chosen from anywhere in the day,
    #                    with a block of 30 consecutive mins chosen at random
    # 
    # Value:
    #   list: mean: the mean species count progression (count only)
    #   list: sd: the standard deviation minute by minute
    
    Report(3, 'performing random sampling at dawn (RSAD)')
    
    mins <- ValidateMins(mins)
    
    #species.in.each.min is optional. 
    if (class(species.in.each.sample) != 'list') {
        if (class(speciesmins) != 'data.frame') {
            speciesmins <- GetTags()
        }
        species.in.each.sample <- ListSpeciesInEachMinute(speciesmins, mins = mins$min.id) 
    }
    
    repetitions <- matrix(rep(NA, num.repetitions * nrow(mins)), ncol = num.repetitions)
    
    Report(4, 'performing', num.repetitions, 'repetitions of RSAD')
    
    if (dawn.first) {
        
        dawn.to <- AdjustDawnTo(dawn.from, dawn.to, block.size)
        
        which.dawn <- mins$min >= dawn.from & mins$min <= dawn.to
        mins.dawn <- mins[which.dawn, ]
        mins.notdawn <- mins[!which.dawn, ]
        
        min.ids.dawn <- mins.dawn$min.id
        min.ids.notdawn <- mins.notdawn$min.id  
        
        Report(4, length(min.ids.dawn), 'of the target are at dawn')
    }

    
    # get the progression for random mins many times
    for (i in 1:num.repetitions) {
        
        if (i %% 10) {
            Dot()
        }
        
        if (dawn.first) {
            jumbled.min.ids <- JumbleMinIdsDawnFirst(min.ids.dawn, min.ids.notdawn, block.size)
        } else {
            sample.order <- Jumble(nrow(mins), block.size)
            jumbled.min.ids <- mins$min.id[sample.order]
        }


        found.species.progression <- GetProgression(species.in.each.sample, jumbled.min.ids)
        repetitions[,i] <- found.species.progression$count  
    }
    #get average progression of counts 
    progression.average <- apply(repetitions, 1, mean)
    #progression.average <- round(progression.average)
    progression.sd <- apply(repetitions, 1, sd)
    
    Report(5, "RSAD complete")
    return(list(mean = progression.average, sd = progression.sd))  
    
}

AdjustDawnTo <- function (from, to, block.size) {
    # finds a new end of dawn time so that dawn length is 
    # a multople of block size
    
    lenth.dawn <- to - from + 1 
    over <- lenth.dawn %% block.size
    if (over > 0) {
        add <- block.size - over   
    } else {
        add <- 0
    }
    to <- from + lenth.dawn + add - 1
    return(to)
    
}


JumbleMinIdsDawnFirst <- function (min.ids.dawn, min.ids.notdawn, block.size = 1) {
    # given a list of min ids from dawn and not dawn,
    # will return a single vector of min ids jumbled
    
        # create a jumbled version of the list of species in each min
        # putting the dawn part always at the start
        sample.order.dawn <- Jumble(length(min.ids.dawn), block.size)
        sample.order.notdawn <- Jumble(length(min.ids.notdawn), block.size)
        jumbled.min.ids <- c(min.ids.dawn[sample.order.dawn], min.ids.notdawn[sample.order.notdawn])
    return(jumbled.min.ids)
    
}


Jumble <- function (len, block.size, fit = TRUE) {  
    # returns the numbers of 1:len jumbled 
    # so that there are consecutive runs of length block.size
    # taken randomly, 
    #
    # Args
    #   len: int; how many to jumble
    #   block.size: int; how long the runs of consecutive numbers should be
    #   fit: if len is not a multiple of block size, fit means that the last mins will be 
    #        a consecutive run of any mins that are left over (not of block.size length)    
    
    if (len < 1) {
        return(integer(0))
    }
    nums <- (1:floor((len)/block.size) * block.size) + 1 - block.size
    jumbled.nums <- sample(nums, length(nums), replace = FALSE)
    m <- matrix(NA, ncol = length(jumbled.nums), nrow = block.size)
    for (i in 1:block.size) {     
        m[i,] <- jumbled.nums + (i - 1)    
    }
    v <- as.vector(m)   
    if (fit && length(jumbled.nums) < len) {
        v <- c(v, (max(v)+1):len)
    } 
    return(v) 
}

GetRandomDawnMins <- function (how.many, dates = "2010-10-13",  sites = "NW") {
    mins <- GetDawnMins(dates, sites)
    if (how.many > nrow(mins)) {
        stop('trying to get more random at dawn than exist')
    }
    mins <- mins[sample(nrow(mins), how.many, replace = FALSE), ]
    return(mins)
}

GetDawnMins <- function (dates = "2010-10-13",  sites = "NW") {
    mins <- GetMinuteList()
    in.dawn <- mins$min >= g.dawn.from & mins$min <= g.dawn.to & mins$date %in% dates & mins$site %in% sites
    mins <- mins[in.dawn, ]
    return(mins)
    
}

