# the lower the number the less reporting is done
# when the report function is called, the priority is specified
# lower numbers are higher priority
g.report.level <- 5

#####
# paths for input and output relative to working directory

# directory containing all audio/event source files 
g.source.dir <- file.path('','Users','n8933464','Documents','SERF')

# audio source directory, containing wav files
g.audio.dir  <- file.path(g.source.dir, 'mono')

# lines source directory, containing lines files
g.lines.dir  <- file.path(g.source.dir, 'lines')

# events produced by different parameters of the AED can be stored in 
# different directories, named by the version number. This is the version
# number to use
g.all.events.version <- 4
#g.all.events.version <- 'a'

# events source directory
g.events.source.dir  <- file.path(g.source.dir,'events',g.all.events.version)






# output parent directory. All output is within this directory
g.output.parent.dir <- file.path('','Users','n8933464','Documents','sample_selection_output')

#####
# start dates for study and target
# study is all the audio that we might look at (i.e. all the audio we have)
# This shouldn't change from run to run. Minute ids are unique over the whole study and start from 1.
# target is the subsection we are looking at for a particular run
# Minute ids are unique in the target, but are not necessarily consecutive or starting at 1


g.study.start.date <- "2010-10-13"
g.study.end.date <-  "2010-10-17"
g.study.start.min <- 0
g.study.end.min <- 1439
g.study.sites <- c('NE','NW','SE','SW')

# the date and minute to start sample selection from
g.start.date <- "2010-10-13"

# the date and minute to end sample selection at
g.end.date <- "2010-10-17"

# alternative to g.start.min. and g.end.min,
# allowing more control ofer which parts of the day are to be used
# g.minute.ranges <- c(0, 1439)
g.minute.ranges <- c(0, 1439)

# which sites to include in sample selection
g.sites <- c('NE','NW','SE','SW')





g.target <- list(
    'NE' = list(
        '2010-10-13' = c(0, 1439),
        '2010-10-14' = c(0, 1439),
        '2010-10-17' = c(0, 1439)
    ),
    'NW' = list(
        '2010-10-13' = c(0, 1439),
        '2010-10-14' = c(0, 1439)
    ),
    'SE' = list(
        '2010-10-13' = c(0, 1439),
        '2010-10-17' = c(0, 1439)
    ),
    'SW' = list(
        '2010-10-16' = c(0, 1439)
    )
)



# select this many percent of the target minutes
# to use as target. Reason for this is to have a 
# representative sample of a long recording without
# needing to process all the long recording
g.percent.of.target <- 100

#####
# user input 
# if values are set here, will be used instead of asking the user
# to get user input, set to NULL
g.user.input <- list(
    'clustering.method' = 'Kmeans',
    'features.for.clustering' = c('duration', 'mean.peak.f', 'f.range')
)
    


# how many minute samples to use when inspecting
g.num.samples <- 15

#####
# Database settings


# evaluation is done against mysql database of tags
g.tags.db.user <- 'root'
g.tags.db.password <- 'root'
g.tags.db.dbname <- 'speciestags'
g.tags.db.unix.socket <- '/Applications/MAMP/tmp/mysql/mysql.sock'
