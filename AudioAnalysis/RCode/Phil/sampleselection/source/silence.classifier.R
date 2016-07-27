
# before clustering 1-sec segment, remove non-bird segments. 
# it seems that TDCC is good at distunguishing between birds but also between different types of silence

# use eclipse as training and testing data for this, since we have annotation of birds without species tagged

# downloaded from the website, we have 30-sec sections. 
# filenames are in the form:
# wave files:
# site_siteid_recordingid_yyyymmdd_hhmmss_dd_0_wav
# where dd is the duration (30)
#
# csv files:
# site_siteid_recordingid_yyyymmdd_hhmmss_dd_9-date-time.csv
# where date-time is the date and time at the time of download
#
#

# TODO: currently calculates many features, then selects out only the ones that were later used for the model. 
#       Need to update to only calculate those features. Probably best to 
#       - choose the model to use for classification
#       - check the dependencies of the model and see which features were used, then calculate those same features. 



RemoveSilentSegments <- function () {
    # reads in segements, runs classifier, removes silent segments
    # saves as new output
    res <- ClassifySegments(save = TRUE)
}

RunSilenceClassifier <- function () {
    # does all the steps needed to run and inspect the classifier on new data
    # classify the seconds
    # reads output from disk, prompting for user choice if needed
    res <- ClassifySegments()
    InspectClassification(res$data)
}

BuildSilenceClassifier <- function () {
    # do all the steps needed for for training and testing the model
    
    # builds a list of seconds based on the available training data
    seconds <- BuildSecondList()
    
    # adds a column with the score for annotation cover (estimate of bird activity)
    seconds <- CalculateAnnotationCover(seconds)
    
    # use the annotation cover score to label the training/testing data as bird or not
    # using an arbitrary threshold of annotation cover score
    seconds <- CalculateHasBird(seconds)
    
    # calculate the features used to classify
    features <- CalculateSilenceFeatures(seconds)
    
    # just for run, get the correlations between the annotation cover and each feature
    corr <- GetCorrelations(seconds, features)
    
    # after manual inspection of all the experimental features, keep some and discard others
    features <- KeepSelectedFeatures(features)
    
    # plots the scaled value for the features against the bird cover
    # and against the specrogram for manual inspection
    # PlotFeatures(seconds, features)
    
    # train the model
    seconds <- TrainModel(seconds, features)
    
    return(seconds)
    
    # inspect the classification result, but rendering spectrograms of each class
    InspectClassification(seconds)
}

ClassifySegments <- function (segments = NULL, silence.features = NULL, use.saved.features = FALSE, save.features = TRUE, save.classified = TRUE) {
    # given a list of seconds and a list of features
    # uses the logistic regression model previously saved to classify the seconds
    
    if (is.null(segments)) {
        segments <- ReadOutput('segment.events')
    }
    
    if (!is.data.frame(segments)) {
        segments.data <- segments$data
    } else {
        segments.data <- segments
    }
    
    
    # TODO: add audio path here
    
    if (is.null(silence.features) && is.list(segments) && use.saved.features) {
        silence.features <- ReadOutput(name = 'silence.features', dependencies = list('segment.events' = segments$version), false.if.missing = TRUE)
    } 
    
    if (is.list(silence.features)) {
        silence.features.data <- silence.features$data
    } else if (is.matrix(silence.features)) {
        silence.features.data <- as.data.frame(silence.features)
    } else if (!is.data.frame(silence.features)) {
        # read output didn't find it
        
        #!! temp test
        # segments.data <- tail(segments.data, 15000)
        
        
        silence.features.data <- CalculateSilenceFeatures(seconds = segments.data)
        
        silence.features.data <- KeepSelectedFeatures(silence.features.data)
        
        
        # TODO: update to specify whether to save the features
        # if we are using the segments read from the system, we can save silence features with dependencies
        if (save.features && is.list(segments) && 'version' %in% names(segments)) {
            WriteOutput(silence.features.data, 'silence.features', dependencies = list(segment.events = segments$version))
        }

    }

    model <- ReadOutput('silence.model')
    fitted.results <- predict(model$data,newdata=silence.features.data,type='response')
    fitted.results <- ifelse(fitted.results > 0.5,1,0)
    
    segments.data$classified.has.bird <- fitted.results
    

    if (save.classified) {
        
        # filter results
        before <- nrow(segments.data)
        segments.data <- segments.data[segments.data$classified.has.bird > 0, ]
        Report(5, 'silent segments removed (', nrow(segments.data), 'of', before, ' rows remain)' )
        dependencies <- list(silence.features = silence.features$version, silence.model = model$version)
        WriteOutput(segments.data, 'filtered.segment.events', dependencies = dependencies)
    }
    
    if (is.list(segments)) {
        segments$data <- segments.data
    } else {
        segments <- segments.data
    }
    
    return(segments)
    
}

TrainModel <- function (seconds, features) {
    # logistic regression to classify second as 'active' or 'quite'
    #       training/testing data uses the 'has.bird' flag, generated based on the annotation overlap
    #       
    
    data <- as.data.frame(cbind(features, seconds$has.bird))
    colnames(data) <- c(colnames(features), 'has.bird')
    
    # 10% for testing
    num.train <- floor(nrow(data)*.9)
    train.selection <- rep(FALSE, nrow(data))
    train.selection[sample.int(nrow(data), num.train)] <- TRUE

    
    train <- data[train.selection,]
    test <- data[!train.selection,]
    
    model <- glm(has.bird ~.,family=binomial(link='logit'),data=train)
    summary(model)
    
    test.results <- predict(model,newdata=test,type='response')
    test.results <- ifelse(test.results > 0.5,1,0)
    
    misClasificError <- mean(test.results != test$has.bird)
    print(paste('Accuracy',1-misClasificError))
    
    fitted.results <- predict(model,newdata=data,type='response')
    fitted.results <- ifelse(fitted.results > 0.5,1,0)
    
    seconds$classified.has.bird <- fitted.results
    seconds$is.train <- train.selection
    
    # ideally we would keep track of the params for producing the features, 
    # but that's probably too much effort
    params = list()
    dependencies = list()
    
    WriteOutput(model, 'silence.model', params = params, dependencies = dependencies)
    
    return(seconds)
    
}

InspectClassification <- function (seconds, features = NULL, rows = 10, random = FALSE) {
    # creates a subset of the seconds df
    # generates spectrograms for each second
    # generates a html page to inspect it
    #
    # Args:
    #   seconds: dataframe. Must contain the 'classified.has.bird' column
    #   rows: integer; either the rows to use or how many rows to use
    #   random: if rows is a single integer, and if true, will choose rows at random
    #           if false will choose rows spaced evenly out across the dataframe. 
    #           advantage of false is that it is deterministic so cached spectrograms 
    #           will be used if run multiple times

    
    if (length(rows) == 1) {
        
        if (random) {
            seconds <- seconds[sample(1:nrow(seconds), rows, replace=FALSE),]
        } else {
            selected.rows <- round(seq(1, nrow(seconds), (nrow(seconds) - 1) / (rows - 1)))
            seconds <- seconds[selected.rows,]
        }
        
    } else {
        seconds <- seconds[rows,]
    }
    
    # replace 'wave.path' with file.path for compatibility with inspection function
    colnames(seconds)[colnames(seconds) == 'wave.path'] <- 'file.path'
    seconds$segment.duration <- 1
    
    seg.time <- SetTime(seconds$min, seconds$sec)
    seg.sec.of.day <- seconds$min * 60 + seconds$sec
    
    seconds$img.title <- paste(seconds$event.id, seconds$site, seconds$date, seg.time, seconds$min, sep = ' : ')
    
    if (is.data.frame(features)) {
        seconds$features <- paste(features, collapse = "\n")
    }
    
    seconds$link <- BawLink(site = seconds$site, 
                                         date = paste(seconds$year,seconds$month,seconds$day,sep='-'), 
                                         start.sec = seg.sec.of.day, 
                                         end.sec = seg.sec.of.day + seconds$segment.duration, 
                                         margin = 2)
    
    

    

    
    spectro.list <- SaveSpectroImgsForInspection(seconds, use.parallel = FALSE)
    seconds$img.path <- spectro.list
    
    return(HtmlInspector(seconds, template.file = 'silence.classification.inspector.html', singles = list(title = 'Inspect Silence Classification')))
    
}



InspectClassificationTesting <- function (seconds, by.file = FALSE) {
    # creates a composite image with 2 rows of 1 sec spectrograms
    # top row is seconds classified as having bird
    # bottom row is seconds classified as not having bird
    # from the test set
    
    # use only the testing
    if ('is.train' %in% colnames(seconds)) {
        seconds <- seconds[!seconds$is.train,]     
    }
    
    has.bird <- NULL
    no.bird <- NULL
    
    for (s in 1:nrow(seconds)) {
        
        # create the spectrogram for this second
        spectro <- Sp.CreateFromFile(path = AudioPath(seconds$wav.file[s]),
                                     offset = seconds$file.sec[s],
                                     duration = 1
        )
        
        spectro$vals[1:nrow(spectro$vals),1] <- min(spectro$vals)
        
        if (seconds$classified.has.bird[s]) {
            has.bird <- cbind(has.bird, spectro$vals)
        } else {
            no.bird <- cbind(no.bird, spectro$vals)
        }
        
    }
    
    # merge rows into 1
    # we don't know wide either one is so find the wider one
    width <- max(ncol(has.bird), ncol(no.bird))
    height <- nrow(has.bird) + nrow(no.bird) + 1
    
    canvas <- matrix(min(no.bird), nrow = height, ncol = width)
    canvas[1:nrow(has.bird),1:ncol(has.bird)] <- has.bird
    canvas[1:nrow(no.bird)+nrow(has.bird)+1,1:ncol(no.bird)] <- no.bird
    
    Sp.DrawVals(canvas, 1)
    
}

KeepSelectedFeatures <- function (f) {
    # keep only the best features
    selection <- c('Ht1.nr','Hf1.nr', 'mm2.nr', 'sd.nr', 'amd.2.nr')
    f <- f[,selection]
    return(f)
}

GetCorrelations <- function (seconds, features, versions = c('wn', 'nr')) {
    # for each feature, find the correlation (absolute) 
    # between it and the bird cover for that second
    corrs <- apply(features, 2, function (col) {
        # TODO: significance??
        return(cor(seconds$bird.cover, col))
    })
    
    row.names <- names(corrs[1:(length(corrs)/length(versions))])
    corrs <- matrix(corrs, ncol = length(versions), byrow = FALSE)
    colnames(corrs) <- versions
    rownames(corrs) <- row.names

    return(corrs)
    
}

SelectFeatures <- function (seconds, features, num = 2) {
    # given the correlations between features and bird cover, 
    # selects the best features. 
    # these will be the ones with the highest correlation for each 
    # version. E.g. if there are different features for different levels of noise removal
    # will choose exactly one of each feature
    
    correlations <- abs(GetCorrelations(seconds, features))
    
    # put correlations into matrix where rows are features and columns are using different preprocessing
    corr.matrix = matrix(correlations, ncol = num, byrow = FALSE)
    names.matrix = matrix(names(correlations), ncol = num, byrow = FALSE)
    
    best.col <- apply(corr.matrix, 1, which.max)
    best <- matrix(c(1:nrow(corr.matrix), best.col), byrow = FALSE, ncol = 2)
    
    selected <- names.matrix[best]
    
    return(features[,selected])
    
}

ScaleDf <- function (df) {
    for (c in colnames(df)) {
        df[,c] <- (df[,c] / sd(df[,c])) - mean(df[,c])
    }
    return(df)
}

PlotSeconds <- function (x, y, class) {
    plot(x, y, col=c("red","blue")[as.numeric(class)+1])
    
    print(paste("correlation:", cor(x, y)))
    
}

PlotFeatures <- function (seconds, features, range = 1:30, section = NULL, spectro.size = .5, scale = TRUE) {
    # spectro.size: float [0,1]; the fraction of the vertical height of the graph to use for the spectrogram
    # 0.5 means that it will be half the size of the graph, or 1/3 of the total size

    if (!is.null(section)) {
        # section is specified. Overrides range
        range <- 1:30 + (section - 1) * 30
    }
    
    
    old.par <- par(mfrow=c(2, 1), par(oma=c(0,0,0,0)))
    
    if (scale) {
        features <- scale(features)    
    }

    cn <- colnames(features)
    colors <- rainbow(ncol(features), v = 0.75)
    
    mx <- max(features[range,])
    mn <- min(features[range,])
    w <- length(range)
    y.range <- mx-mn
    mx2 <- mn + (y.range * (1 + spectro.size))
    par(mar=c(0,2,2,2))
    plot(seq(mn, mx2, length.out = w), type = 'n', xlab = 'seconds', ylab = 'scaled feature values (z score)')
    wav.fn <- unique(seconds$wav.file[range])
    if (length(wav.fn) == 1) {
        # if the range covers exactly 1 wave file
        spectro <- Sp.CreateFromFile(wav.fn)
        sv <- PreprocessSpectroVals(spectro$vals)
        
        x <- 1:ncol(sv) * (w / ncol(sv)) + 0.5
        #y <- 1:nrow(sv) * ((mx - mn) / nrow(sv)) + mn
        y <- 1:nrow(sv)  * ((mx2 - mx) / nrow(sv)) + mx
        image(x, y, z = t(sv), col = rev(gray.colors(10)), add = TRUE)
        #grid(nx = w+1, ny = 0, col = 'black')
        for (grid in 0:w) {
            abline(v = grid + 0.5, col = 'darkgrey')
            abline(v = grid + 0.5, col = 'white', lty = 2)
        }
    }
    for (f in 1:ncol(features)) {
        points(features[range, cn[f]], col = colors[f])
        lines(features[range, cn[f]], col = colors[f], lwd= 5)
    }
    
    corr <- round(GetCorrelations(seconds, features), 3)
    corr.range <- round(GetCorrelations(seconds[range,], features[range, ]), 3)
    legend.names <- paste0(cn, "(", corr, ' | ', corr.range, ")")
    legend("bottomright", legend = legend.names, col = colors, pch = 1)
    
    
    # plot the bird cover below
    colors <- rainbow(2, v = 0.75) 
    par(mar=c(2,2,1,2))
    plot(seconds$bird.cover[range], ylab = 'number of seconds of annotation cover', xlab = 'seconds', col = colors[2], type = 'b', oma=c(1,1,0,1))
    #points(seconds$bird.cover.1[range], col = colors[1])
    #lines(seconds$bird.cover.1[range], col = colors[1])
    

    
    for (grid in 0:w) {
        abline(v = grid + 0.5, col = 'darkgrey')
        abline(v = grid + 0.5, col = 'white', lty = 2)
    } 
    
    if(!"classified.has.bird" %in% colnames(seconds)) {
        seconds$classified.has.bird <- 0
    } 
    for (sec in 1:w) {
        if (seconds$classified.has.bird[range][sec] > 0) {
            rect(sec + 0.5, 0.9, sec+1.5, 1.1, col=rgb(1, 0, 0, alpha=0.5), density = NULL, border = NULL, lty = par("lty"), lwd = par("lwd"))
        } 
    }
    
    
    
    
    old.par <- par(old.par)
    
}

PreprocessSpectroVals <- function (m) {
    
    m <- Normalize(m)
    m <- NormaliseSpectrumNoise(m)
    m <- Blur(m)
    m <- MedianSubtraction(m)
    return(m)
    
}


# TODO: completely redo this
# because it is confusing. 
AudioPath <- function (fn = NULL, input.directory = NULL) {
    # given a filename and the input directory, will create the full path and return it. 
    # if the fn already appears to be a full path, it will just return it. This allows processing
    # of csvs with different formats(i.e. filename only or full path)
    # "/Users/n8933464/Documents/SERF/mtlewis"
    
    # check if fn includes full path already
    if (!grepl('/',fn[3])) {
        if (is.null(input.directory)) {
            return(GetFullPath(fn[3], fn[1],fn[2]))
        } else {
            return(file.path(input.directory, 'wav', fn[3]))
        }
    } else {
        return(fn[3])
    }
}

CalculateSilenceFeatures <- function (seconds, wavecol = c('wav.file','wave.path'), parallel = 1) {
    
    #debugging
    #seconds <- seconds[1:4000,]
     
    empty <- rep(NA, nrow(seconds))
    
    wavecol <- intersect(wavecol, colnames(seconds))
    if (length(wavecol) != 1) {
        # add wave col
        wavecol <- 'wave.path'
        seconds[, wavecol] <- GetAudioFileBatch(seconds)
    }

    audio.files <- as.character(unique(seconds[,wavecol])) 
    
    SetReportMode() # reset to console only
    if (parallel > 1) {
        SetReportMode(socket = TRUE)
        cl <- makeCluster(parallel)
        registerDoParallel(cl)
        res <- foreach(f = audio.files,
                       .combine='rbind',
                       .export=ls(envir=globalenv())) %dopar% CalculateSilenceFeaturesForFile(f, seconds, wavecol)

    } else {
        SetReportMode(socket = FALSE)
        res <- data.frame()
        for (f in audio.files) {
            row.res <- CalculateSilenceFeaturesForFile(f, seconds, wavecol)
            res <- rbind(res, row.res)
        }
    }
    SetReportMode() # reset to console only
    
    # Don't scale!! this is wrong!
    # features <- scale(res)
    
    # add combo features
    # create a new columns c that equals max of a and b respectively
    # entropy.features <- data.frame(a = c('Ht1','Ht2','Ht1.nr','Ht2.nr'), 
    #                               b = c('Hf1','Hf2','Hf1.nr','Hf2.nr'),
    #                               c = c('H1m','H2m','H1m.nr','H2m.nr'),
    #                               stringsAsFactors = FALSE)
    # for (r in 1:nrow(entropy.features)) {
    #    features[, entropy.features[r,'c']] <- apply(cbind(features[, entropy.features[r,'a']], features[, entropy.features[r,'b']]), 1, max)
    # }

    return(res)

    
}

CalculateSilenceFeaturesForFile <- function (af.path, seconds, wavecol, segment.duration = 1) {
    
    
    # all features are done on noise-reduced spectrogram except average of entropies (entropy then average)
    feature.names <- c('Ht1', # Ht1 = temporal.entropy (average then entropy)
                       'Hf1',  # Hf1 = spectral.entropy (average then entropy)
                       'Ht2', # Ht2 = temporal.entropy (entropy then average) !
                       'Hf2', # Hf2 = spectral.entropy (entropy then entropy) !
                       'H1m', # H1m = max of Ht2 and Hf2
                       'H2m', # H2m = max of Ht2 and Hf2
                       'mm1', # mm1 = abs(mean - median of second)
                       'mm2', # mm2 = mean - median of 30 seconds
                       'sd', # sd = standard deviation of values in the second
                       'amd.1', # amd.1 = average distance from median
                       'amd.2')    # amd.2 = average distance from the 30 second median
    
    # TODO: Currently, TDCCs and Silence features are generating spectrograms with different cache ids, 
    # meaning that the cache gets bigger than necessary and also the total pipeline takes longer than necessary. I think this is because the frame width is different
    # consider using the same frame width for tdccs as for silence features (would probably need to re-train model). I am not sure how much difference it makes for either. 
    

    
    spectro <- Sp.CreateFromFile(af.path)
    

    
    seconds.selection <- seconds[,wavecol] == af.path
    af.seconds <- seconds[seconds.selection, ]
    # number of seconds to iterate over is either the number of seconds in the audio file
    # or the number of seconds in the seconds list with this file. They should normally be the same
    # but we need to make sure that we don't try to calculate features for a second that doesn't exist (if the file is a bit longer than expected)
    num.secs <- min(c(sum(seconds.selection),round(spectro$duration / segment.duration)))
    
    empty.features <- matrix(rep(NA, length(feature.names)*nrow(af.seconds)), ncol = length(feature.names))
    colnames(empty.features) <- feature.names
    
    second.width <- round(spectro$frames.per.sec)
    
    # frequency normalizaion
    spectro.vals.wn <- Normalize(spectro$vals)
    spectro.vals.wn <- NormaliseSpectrumNoise(spectro.vals.wn)
    spectro.vals.wn <- Blur(spectro.vals.wn)
    
    # noise-removed version (not suitable for H-then-mean versions of entropy, only for mean-then-H)
    spectro.vals.nr <- MedianSubtraction(spectro.vals.wn)
    spectro.vals.nr <- Blur(spectro.vals.nr)
    spectro.vals.nr <- MedianSubtraction(spectro.vals.nr)
    spectro.vals.nr <- Blur(spectro.vals.nr)
    
    
    # list of two spectrograms: with noise and noise removed
    spectro.vals.list <- list(wn = spectro.vals.wn, 
                              nr = spectro.vals.nr)
    
    # two empty featuresets: one for noise and one for noise removed
    features <- list(empty.features, empty.features)
    
    # for each of with noise and noise removed:
    for (sv in 1:length(spectro.vals.list)) {
        
        spectro.vals <- spectro.vals.list[[sv]]
        
        # median of file
        median.spectro.vals <-  median(spectro.vals)
        
        for (sec in 1:num.secs) {
            
            if (basename(af.path) == 'NE_2010-10-17_0019.wav' && sec == 60) {
                print('now')
            }
            
            # spectro column offset of current second in the file spectrogram
            start.offset <- floor((sec-1)*second.width)+1
            
            # it is possible that the last second will be slightly short.
            # it should not be possible that less than half of 1 second is present
            # because we have already rounded the duration of the spectrogram to get the number of seconds to process
            end.offset <- min(c(floor((sec)*second.width),ncol(spectro.vals)))
            
            
            
            cur.sec.spectro.vals <- spectro.vals[,start.offset:end.offset]
            
            H1 <- GetEntropy1(cur.sec.spectro.vals)
            features[[sv]][sec, 'Ht1'] <- 1 - H1$Ht
            features[[sv]][sec, 'Hf1'] <- 1 - H1$Hf
            
            H2 <- GetEntropy2(cur.sec.spectro.vals)
            features[[sv]][sec, 'Ht2'] <- 1 - H2$Ht
            features[[sv]][sec, 'Hf2'] <- 1 - H2$Hf
            
            # mean/median of current second
            mean.val <- mean(cur.sec.spectro.vals)
            med.val <- median(cur.sec.spectro.vals)
            
            features[[sv]][sec, 'mm1'] <- abs(mean.val - med.val)
            
            # this one should not be absolute, because on busy 30 seconds,
            # median might be greater than mean of silent second
            features[[sv]][sec, 'mm2'] <- mean.val - median.spectro.vals
            
            features[[sv]][sec, 'sd'] <- sd(cur.sec.spectro.vals)
            
            # mean distance from the median
            features[[sv]][sec, 'amd.1'] <- mean(abs(cur.sec.spectro.vals - med.val))
            features[[sv]][sec, 'amd.2'] <- mean(abs(cur.sec.spectro.vals - median.spectro.vals))
            
            Dot()
            
        }  #end for each sec in file
        
    }  # end for each version of file spectro (noise/nr)
    
    
    colnames(features[[2]]) <- paste0(colnames(features[[2]]), '.nr')
    all.features <- cbind(features[[1]], features[[2]])
    
    Report(5, 'file complete (',num.secs,' secs)')
    
    
    # make sure that we return the correct number of row for features
    # i.e. each second has a row of features
    
    
    return(all.features)
    
    
}


NormaliseSpectrumNoise <- function (vals, q = .25) {
    # for each row, subtract the difference between the overall percentile (param) and the percentile (q) for that row
    overall <- quantile(vals, q)
    total <- sum(vals)
    rows <- apply(vals, 1, quantile, q)
    diff <- rows - overall
    vals <- vals - matrix(diff, nrow = nrow(vals), ncol = ncol(vals), byrow = FALSE) 
    # make it sum to what it used to
    vals <- vals * total / sum(vals)
    return(vals)
}

GetEntropy1 <- function (spectro.vals) {
    # given a matrix, returns the time entropy and frequency entropy
    # Ht averages the rows of the matrix (to get a single row), 
    # treats the resulting vector as a prob dist
    # then calculates entropy on it
    # Hf averages the columns of the matrix, then does the same
    col <- apply(spectro.vals, 1, mean)
    row <- apply(spectro.vals, 2, mean)
    return(list(Hf = CalculateEntropy(col), Ht = CalculateEntropy(row)))
}

GetEntropy2 <- function (spectro.vals, q = 0.1) {
    # given a matrix, returns the time entropy and frequency entropy
    # H is obtained for each row, then the result averaged for the final Ht, 
    # H is obtained for each column, then the result averaged for the final Hf

    Hts <- apply(spectro.vals, 1, CalculateEntropy)
    Hfs <- apply(spectro.vals, 2, CalculateEntropy)
    
    # use only below the specified quantile
    Hts <- Hts[Hts < quantile(Hts, q)]
    Hfs <- Hfs[Hfs < quantile(Hfs, q)]
    
    
    return(list(Hf = mean(Hfs), Ht = mean(Hts)))
}

CalculateEntropy <- function (pp, normalize = TRUE) {
    if (normalize) {
        pp <- pp / sum(pp)
    }
    H <- - (sum(pp * log2(pp), na.rm = TRUE))
    if (normalize) {
        H <- H / log2(length(pp))
    }
    return(H)
}

CalculateHasBird <- function (seconds, f = 'bird.cover', t = 0.1) {
   # using the annotation cover sets a threshold
    seconds$has.bird <- seconds[,f] > t
    return(seconds)
}

CalculateAnnotationCover <- function (seconds, input.directory = "/Users/n8933464/Documents/SERF/mtlewis") {
    # for each second (row) in the data frame supplied,
    # check the csv to see how much of the second is covered by annotations
    seconds$bird.cover.1 <- 0
    seconds$bird.cover.2 <- 0
    seconds$num.annotations <- 0
    # definitely not the most efficient way to do this
    for (se in 1:nrow(seconds)) {
        path <- file.path(input.directory, 'csv', seconds$csv.file[se])
        annotations <- read.csv(path)
        # figure out the second of the seconds dataframe that each annotation belongs to
        # TODO: move this to the BuildSecondsList function to only do it once per ? or don't bother because it's not that slow
        audio.file.start <- as.POSIXct(paste(annotations$audio_recording_start_date_utc[1], annotations$audio_recording_start_time_utc[1]), tz = 'utc')
        csv.start <- as.POSIXct(seconds$csv.start[se], tz = 'aest')
        csv.secs.from.start.of.audio.file <- as.numeric(difftime(csv.start, audio.file.start, units = 'secs'))
        annotations$event_start_seconds.csv <- annotations$event_start_seconds - csv.secs.from.start.of.audio.file
        annotations$event_end_seconds.csv <- annotations$event_end_seconds - csv.secs.from.start.of.audio.file
        # now we have columns that tell us which second the annotation starts and finishes in, relative to the 'sec' column of the seconds df
        # select all events that start before or during the current second AND end during or after the current second
        overlapping.selection <- annotations$event_start_seconds.csv <= seconds$file.sec[se] + 1 & annotations$event_end_seconds.csv >= seconds$file.sec[se]
        overlapping <- annotations[overlapping.selection,]
        if (nrow(overlapping) > 0) {
            # we have some annotations in this section, so find out the amount of cover
            if (nrow(overlapping) > 1) {
                #debug
                #print('debug')
            }
            start <- overlapping$event_start_seconds.csv
            end <- overlapping$event_end_seconds.csv
            # trim to only be in the current second
            start[start < seconds$file.sec[se]] <- seconds$file.sec[se]
            end[end > seconds$file.sec[se] + 1] <- seconds$file.sec[se] + 1         
            df <- data.frame(s = c(start, end), score = c(rep(1, length(start)), rep(-1, length(end))))
            df <- df[order(df$s),]
            score <- 0
            prev.score <- 0
            start.time <- 0
            bird.cover.1 <- 0
            for (r in 1:nrow(df)) {
                score <- score + df$score[r]
                if (score == 1 && df$score[r] == 1) {
                    # start annotation area
                    start.time <- df$s[r]
                } else if (score == 0 ) {
                    bird.cover.1 <- bird.cover.1 + (df$s[r] - start.time)
                }
            }
            seconds$bird.cover.1[se] <- bird.cover.1
            bc2 <- sum(end - start)
            seconds$bird.cover.2[se] <- bc2
            seconds$num.annotations[se] <- nrow(overlapping)
            
            
        } else {
            # debug
            # print('no annotations')
            
        } # if has annotations
    } # for each second
    # lets say that the final value for bird cover is the average of the 2 estimates
    seconds$bird.cover <- rowMeans(seconds[,c('bird.cover.1', 'bird.cover.2')])
    return(seconds)
}

BuildSecondList <- function (input.directory = "/Users/n8933464/Documents/SERF/mtlewis") {
    # create a csv of seconds based on the files downloaded from the ecosounds website

    
    csv.path <- file.path(input.directory, 'csv')
    wav.path <- file.path(input.directory, 'wav')
    csv.list <- list.files(path = csv.path, no.. = TRUE)
    nu <- numeric()
    ch <- character()
    seconds <- data.frame(site = ch, year = nu, month = nu, day = nu, hour = nu, min = nu, sec = nu, file.sec = nu,
                          wav.file = ch, csv.file = ch, csv.start = character())
    
    # fore each csv, create a row in the 'seconds' for each second in the duration of the csv
    # use the filename to do this

    for (fn in csv.list) {
        parts <- unlist(strsplit(fn, '_'))
        #spaces in the site name are underscored, which is annoying. We need to figure out how many spaces there are 
        # to get the correct offset
        site.length <- which.min(is.na(as.numeric(parts))) - 1
        site <- paste(parts[1:site.length], collapse = "_")
        date <- parts[site.length+3]
        time <- parts[site.length+4]
        duration <- as.numeric(parts[site.length+5])
        year <- as.numeric(substr(date, 1, 4))
        month <- as.numeric(substr(date, 5, 6))
        day <- as.numeric(substr(date, 7, 8))
        hour <- as.numeric(substr(time, 1, 2))
        min <- as.numeric(substr(time, 3, 4))
        sec <- as.numeric(substr(time, 5, 6))
        wav.file <- paste0(paste(parts[1:(length(parts)-1)], collapse = "_"), "_0_wav")
        secs <- 1:duration-1
        
        csv.start <- as.POSIXct(paste(paste(year, month, day, sep='-'), paste(hour, min, sec, sep=':')))
        
        
        cur.seconds <- data.frame(site = site, year = year, month = month, day = day, hour = hour, min = min, sec = secs+sec, file.sec = secs,
                                  wav.file = wav.file, csv.file = fn, csv.start = csv.start, stringsAsFactors = FALSE)

        
        
        
        
        
        
        seconds <- rbind(seconds, cur.seconds)
    }
    
    
    seconds$wav.file <- file.path(wav.path, seconds$wav.file)
    
    

    
    return(seconds)
}










