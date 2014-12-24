test.SampleAtLeastOne <- function () {
    
    pool <- 1:500
    at.least.one.of <- 10:500
    num <- 499
    
    
    
    for (i in 1:1000) {
        
        #res <- SampleAtLeastOne(pool, num, at.least.one.of = at.least.one.of)
        
        res <- SampleAtLeastOne(pool, num)
        
        if (length(setdiff(at.least.one.of, res)) > 0) {
            
            stop('error')
            
        } else {
            Dot()
        }
        
    }
    
    
    
}


test.AddRandom <- function () {
    
    test1 <- rep(1:10, times = 4)
    test2 <- AddRandom(test1, 0.5)
    print(test2)
    # for testing
    plot(test2)
    points(test1, col = "red")
    
    
}

test.AddRandom.2 <- function () {
 
    amounts <- seq(0, 1, 0.2)
    num <- seq(50, 1000, 100)
    pool <- seq(50, 1000, 100)
    
    
    amounts <- mean(amounts)
    num <- mean(num)
    #pool <- mean(pool)
    
    error <- diff <- expected.diff <- rep(NA, length(amounts) * length(num) * length(pool))
    
    for (p in 1:length(pool)) {
        
        for (n in 1:length(num)) {
            
            for (a in 1:length(amounts)) {
                
                test1 <- sample(pool[p], num[n], replace = TRUE)
                test.rand <- AddRandom(test1, amounts[a])
                
                if (length(setdiff(test1, test.rand)) > 0 || length(test1) != length(test.rand)) {
                    print('invalid value returned')
                }
                
                i <- a*n*p
                
                diff[i] <- sum(test1 != test.rand)
                expected.diff[i] <- round(length(test1) * amounts[a])
                error[i] <- (diff[i] - expected.diff[i]) / length(test.rand)
        
        
            }
        }
    }
    
    #plot(diff, col = 'red') 
    plot(error)
    #lines(diff, col = 'red')
    #lines(expected.diff, col = 'blue')
    
    
}