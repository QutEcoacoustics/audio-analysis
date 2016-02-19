# for generating links to baw based on site, date, time

base <- 'https://www.ecosounds.org/listen/'

audio.ids <- as.data.frame(matrix(c(
    '2012-10-13','NE','187684',
    '2012-10-14','NE','188263',
    '2012-10-15','NE','188258',
    '2012-10-16','NE','188257',
    '2012-10-17','NE','188252',
    '2012-10-13','NW','188287',
    '2012-10-14','NW','188286',
    '2012-10-15','NW','188285',
    '2012-10-16','NW','188284',
    '2012-10-17','NW','188279',
    '2012-10-13','SE','188292',
    '2012-10-14','SE','188293',
    '2012-10-15','SE','188294',
    '2012-10-16','SE','188295',
    '2012-10-17','SE','188300',
    '2012-10-13','SW','218483',
    '2012-10-14','SW','218482',
    '2012-10-15','SW','218481',
    '2012-10-16','SW','218479',
    '2012-10-17','SW','218485'
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
    
    audio.id <- audio.ids[audio.ids$site == site & audio.ids$date == date, 'id']
    
    link <- paste0(base, audio.id, '?start=', start.sec - margin, '&end=', end.sec+ margin)
    
    return(link)
    
    
}

