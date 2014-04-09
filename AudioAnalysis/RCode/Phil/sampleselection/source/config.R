# the lower the number the less reporting is done
# when the report function is called, the priority is specified
# lower numbers are higher priority
g.report.level <- 6

# paths for input and output relative to working directory

# directory containing all audio/event source files 
g.source.dir <- file.path('..','..','..','..','..','..','SERF')

# audio source directory, containing wav files
g.audio.dir  <- file.path(g.source.dir, 'mono')

g.all.events.version <- 3
#g.all.events.version <- 'a'

# events source directory, containing one csv file for each wave file
# (the matlab output)
g.events.source.dir  <- file.path(g.source.dir,'events',g.all.events.version)


# output parent directory. All output is within this directory
g.output.parent.dir <- file.path('..','output')


g.study.start.date <- "2010-10-13"
g.study.end.date <-  "2010-10-17"
g.study.start.min <- 0
g.study.end.min <- 1439
g.study.sites <- c('NW', 'NE')  # todo: include the others, eventually

# the date and minute to start sample selection from
g.start.date <- "2010-10-13"
g.start.min <- 0
#g.start.min <- 415

# the date and minute to end sample selection at
g.end.date <- "2010-10-13"
g.end.min <- 1439
#g.end.min <- 430

# which sites to include in sample selection
g.sites <- c('NW')

# select this many percent of the target minutes
# to use as target. Reason for this is to have a 
# representative sample of a long recording without
# needing to process all the long recording
g.percent.of.target <- 100

# how many minute samples to use when inspecting
g.num.samples <- 5


# evaluation is done against mysql database of tags
g.tags.db.user <- 'root'
g.tags.db.password <- 'root'
g.tags.db.dbname <- 'speciestags'
g.tags.db.unix.socket <- '/Applications/MAMP/tmp/mysql/mysql.sock'
