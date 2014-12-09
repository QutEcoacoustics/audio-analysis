ClusterQuality <- function (version = 1, 
                            species.mins = NA, 
                            group.mins = NA) {
    
    if (!is.data.frame(species.mins)) {
        species.mins <- GetSpeciesMins()
    }
    
    if (!is.data.frame(group.mins)) {
        group.mins <- GetGroupMins(species.mins)
    }
    
    groups <- unique(group.mins$group)
    species <- unique(species.mins$species.id)
    species <- data.frame(species.id = species[order(species)])
    species$min.count <- sapply(species$species.id, function (s.id) {
        return(sum(species.mins$species.id == s.id))
    })
    groups <- data.frame(group = groups[order(groups)])
    groups$min.count <- sapply(groups$group, function (group) {
        return(sum(group.mins$group == group))
    })
    
    m <- matrix(NA, nrow = nrow(species), ncol = nrow(groups))
    
    for (s in 1:nrow(species)) {
        Dot()
        for (g in 1:nrow(groups)) {
            mins.with.species <- species.mins$min.id[species.mins$species.id == species$species.id[s]] 
            mins.with.group <- group.mins$min.id[group.mins$group == groups$group[g]] 
            mins.with.both <- intersect(mins.with.species, mins.with.group)
            over.species.val <- length(mins.with.both)/length(mins.with.species)  
            over.group.val <- length(mins.with.both)/length(mins.with.species)  
            if (version == 1) {
                title <- "Num mins containing both divided by num mins containg species"
                m[s,g] <- over.species.val
            } else if (version == 2) {
                title <- "Num mins containing both divided by num mins containg group"
                m[s,g] <- over.group.val
            } else if (version == 3) {
                m[s,g] <- MatchSpeciesGroup2(species.id = species$species.id[s], group = groups$group[g], species.mins, group.mins)
            } else if (version == 4) {
                m[s,g] <- MatchSpeciesGroup3(species.id = species$species.id[s], group = groups$group[g], species.mins, group.mins)
            }
        }
    } 
    
    DrawCQMatrix.5(m, species, groups, title)
    
    return(list(m = m, species = species, groups = groups$group, title = title))
    
}

DrawCQMatrix.4 <- function (m, species, groups, title) {
    require(lattice)
    m <- m
    levelplot(m, 
              col.regions=gray.colors(256,start=1,end=0), 
              xlab = "Cluster Groups",
              ylab = "Species id",
              row.values = species,
              column.values = groups)  
}

DrawCQMatrix.3 <- function (m, species, groups, title) {
    
    image(t(m), col=gray.colors(256,start=1,end=0), axes = FALSE, useRaster = TRUE)
    
    at.x <- 1:ncol(m) / ncol(m)
    at.y <- 1:nrow(m) / nrow(m)
    
    #axis(3, at = at.x, labels=as.character(groups), srt=45,tick=FALSE)
    #axis(2, at = at.y, labels=as.character(species), srt=45,tick=FALSE)
    
    mtext("species", 2, line=0)
    mtext("clusters", 1, line=0)
    
    mtext('title')
    
}

DrawCQMatrix.5 <- function (m, species, groups, title) {
    require('grid')
    #ma <- max(m)
    #mi <- min(m)  
    #raster renders higher values as white, so lets inverse
    #amp <- - (((m - mi) / (ma - mi)) - 1)
    
    vals <- m
    
    grid.newpage()
    scale <- 5
    #devsize.cm <- dev.size(units = "cm")
    #devsize.px <- dev.size(units = "px")
    #px.per.cm <- (devsize.px[1] / devsize.cm[1]) / 10
    #vp.width.cm <- ncol(vals) / px.per.cm
    #vp.height.cm <- nrow(vals) / px.per.cm
    container.vp <- viewport(x = 0.5, y = 0.5, width=1, height=1, just = c('center', 'center'), default.units = 'npc')
    pushViewport(container.vp)
    grid.show.viewport(container.vp)
    vp <- viewport(x = 0.5, y = 0.5, width=0.8, height=0.8, just = c('center', 'center'), default.units = 'npc')
    
    pushViewport(vp)
    grid.show.viewport(vp)   
    grid.raster(image = vals, x = unit(0.5, "npc"), y = unit(0.5, "npc"), width = 1, height = 1, vp = vp, interpolate = FALSE)
    
    
    
    #  at.x <- 1:ncol(vals) / ncol(vals)
    #  at.y <- 1:nrow(vals) / nrow(vals)
    
    #axis(3, at = at.x, labels=as.character(groups), srt=45,tick=FALSE)
    #axis(2, at = at.y, labels=as.character(species), srt=45,tick=FALSE)
    
    #   mtext("species", 2, line=0)
    #   mtext("clusters", 1, line=0)
    
    #  mtext('title')
    
}

DrawCQMatrix.2 <- function (m, species, groups) { 
    library(pheatmap)
    m <- m
    pheatmap(m, cluster_row = FALSE, cluster_col = FALSE, color=gray.colors(256,start=1,end=0), scale = 'none')
}

DrawCQMatrix.1 <- function (m, species, groups) { 
    library(gplots)
    #Format the data for the plot
    xval <- formatC(m, format="f", digits=2)
    pal <- colorRampPalette(c(rgb(0.96,0.96,1), rgb(0,0,0)), space = "rgb")
    #Plot the matrix
    x_hm <- heatmap.2(m, 
                      Rowv=FALSE, 
                      Colv=FALSE, 
                      dendrogram="none", 
                      main="8 X 8 Matrix Using Heatmap.2", 
                      xlab="Cluster", 
                      ylab="Species", 
                      col=pal, 
                      tracecol="#303030", 
                      trace="none", 
                      cellnote=xval, 
                      notecol="black", 
                      notecex=0.8, 
                      keysize = 1.5, 
                      margins=c(5, 5))
    
}




MatchSpeciesGroup <- function (species.id, group, species.mins, group.mins, over.species = TRUE) {
    # finds the number of minutes containing both particular species and a particular group
    # divided by the number of minutes containing the species
    # OR
    # the number of mins containing both divided by the number of mins containing the group
    
    mins.with.species <- species.mins$min.id[species.mins$species.id == species.id] 
    mins.with.group <- group.mins$min.id[group.mins$group == group] 
    mins.with.both <- intersect(mins.with.species, mins.with.group)
    
    return(list(mins.with.species = mins.with.species,
                mins.with.group = mins.with.group,
                mins.with.both = mins.with.both))
    
}

MatchSpeciesGroup2 <- function (species.id, group, species.mins, group.mins) {
    # 
    
    total.num.mins <- length(unique(c(species.mins$min.id, group.mins$min.id)))
    
    mins.with.species <- species.mins$min.id[species.mins$species.id == species.id]
    mins.with.group <- group.mins$min.id[group.mins$group == group] 
    mins.with.both <- intersect(mins.with.species, mins.with.group)
    
    chance.of.both.if.random <- (length(mins.with.species)/total.num.mins) * (length(mins.with.group)/total.num.mins)
    
    #score <-  (chance.of.both.if.random ) /  ((length(mins.with.both) / total.num.mins) + )
    
    score <- (length(mins.with.both) / total.num.mins) / chance.of.both.if.random
    
    
    return(score)
    
}

MatchSpeciesGroup3 <- function (species.id, group, species.mins, group.mins) {
    # 
    
    mins.with.species <- species.mins$min.id[species.mins$species.id == species.id]
    mins.with.group <- group.mins$min.id[group.mins$group == group] 
    mins.with.both <- intersect(mins.with.species, mins.with.group)
    
    #score <-  (chance.of.both.if.random ) /  ((length(mins.with.both) / total.num.mins) + )
    
    score <- length(mins.with.both)^2 / (length(mins.with.species) * length(mins.with.group))
    
    return(score)
    
}