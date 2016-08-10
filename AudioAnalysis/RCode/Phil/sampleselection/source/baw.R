# for generating links to baw based on site, date, time

base <- 'https://www.ecosounds.org/listen/'

audio.ids <- as.data.frame(matrix(c(
    '2010-10-13','NE','187684',
    '2010-10-14','NE','188263',
    '2010-10-15','NE','188258',
    '2010-10-16','NE','188257',
    '2010-10-17','NE','188252',
    '2010-10-13','NW','188287',
    '2010-10-14','NW','188286',
    '2010-10-15','NW','188285',
    '2010-10-16','NW','188284',
    '2010-10-17','NW','188279',
    '2010-10-13','SE','188292',
    '2010-10-14','SE','188293',
    '2010-10-15','SE','188294',
    '2010-10-16','SE','188295',
    '2010-10-17','SE','188300',
    '2010-10-13','SW','218483',
    '2010-10-14','SW','218482',
    '2010-10-15','SW','218481',
    '2010-10-16','SW','218479',
    '2010-10-17','SW','218485'
), ncol = 3, byrow = TRUE))
colnames(audio.ids) <- c('date', 'site', 'id')

BawLink <- function (site, date, start.sec, end.sec, margin = 5) {
    # create a link to audio on the baw server 
    #
    # Args:
    #   site: string
    #   date: string
    #   start.sec: int; start sec in the day ??
    #   end.sec: int; end sec in the day
    #   margin: int; how many seconds to show ether side of start and end sec
    
    #audio.id <- audio.ids[audio.ids$site == site & audio.ids$date == date, 'id']
    
    audio.id <- as.character(apply(cbind(site, date), 1, function (r) {
        return(audio.ids[audio.ids$site == r['site'] & audio.ids$date == r['date'], 'id'])
    }))
    
    if (length(audio.id) != length(site)) {
        stop('some sites or dates were invalid in baw link')
    }
    
    start.sec <- start.sec - margin
    start.sec[start.sec < 0] <- 0
    end.sec <- end.sec+ margin
    
    link <- paste0(base, audio.id, '?start=', start.sec, '&end=', end.sec)
    
    return(link)
    
    
}

