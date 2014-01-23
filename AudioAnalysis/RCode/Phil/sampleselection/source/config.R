# the lower the number the less reporting is done
# when the report function is called, the priority is specified
# lower numbers are higher priority
g.report.level <- 5

# paths for input and output relative to working directory

# directory containing all audio/event source files 
g.source.dir <- '../../../../../../SERF'

# audio source directory, containing wav files
g.audio.dir  <- file.path(g.source.dir, 'mono')

# events source directory, containing one csv file for each wave file
g.events.source.dir  <- file.path(g.source.dir,'events')

# output parent directory. All output is within this directory
g.output.parent.dir <- '../output'

# the date and minute to start sample selection from
g.start.date <- "2010-10-13"
g.start.min <- 1

# the date and minute to end sample selection at
g.end.date <- "2010-10-13"
g.end.min <- 1440

# which sites to include in sample selection
g.sites <- c('NW')

# select this many percent of the target minutes
# to use as target. Reason for this is to have a 
# representative sample of a long recording without
# needing to process all the long recording
g.percent.of.target <- 100

# how many minute samples to use
g.num.samples <- 60

#column names for the events file
g.events.col.names <- c('filename','site','date','start.sec',
                        'file.start.sec','duration','bottom.freq','top.freq')

# evaluation is done against mysql database of tags
g.tags.db.user <- 'root'
g.tags.db.password <- 'root'
g.tags.db.dbname <- 'tags'
g.tags.db.unix.socket <- '/Applications/MAMP/tmp/mysql/mysql.sock'
