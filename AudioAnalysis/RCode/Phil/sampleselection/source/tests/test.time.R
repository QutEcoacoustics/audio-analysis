
source("time.R")

context('time functions')


    
test_that("GetDateParts returns the correct list when given some dates", {
        
    expect_equal(GetDateParts(dates = c('2010-10-10', '2010-10-11', '2010-10-12', '2010-11-10')), 
                 list(prefix = '2010', dates = c("10-10","10-11","10-12","11-10"), selector = c(FALSE, TRUE, TRUE)))
    expect_equal(GetDateParts(dates = c('2010-10-10', '2010-10-11', '2010-10-12')), 
                 list(prefix = '2010-10', dates = c("10","11","12"), selector = c(FALSE, FALSE, TRUE)))
    expect_equal(GetDateParts(dates = '2010-10-10'), 
                 list(prefix = '2010-10-10', dates = "", selector = c(FALSE, FALSE, FALSE)))
    expect_equal(GetDateParts(dates = c('2010-10-10','2010-10-10')), 
                 list(prefix = '2010-10-10', dates = c("",""), selector = c(FALSE, FALSE, FALSE)))
        
})    
    




