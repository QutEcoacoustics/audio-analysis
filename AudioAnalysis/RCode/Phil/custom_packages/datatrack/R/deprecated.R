# this file contains functions that are no longer used,
# but which are worth leaving in case they need to be referred to later in a future version of the package



#' given a name and a version, will return a dataframe listing of all of the dependencies and their versions
#'
#' @param name string;
#' @param version int;
#' @param meta: data.frame; optional. The entire dataframe of metadata. If ommitted will be read from disk.
#'
#' @value data.frame with columns name, version
#'
#' @details
#' given a name and a version, will return a dataframe listing of all of the dependencies and their versions
#' if a dataobject type is a dependency of more than one dependency, then it will only appear once.
#' if this occurs, it should have the same version. If there are different versions of the same dependency
#' in the dependency tree, something is wrong.
#'
#' @deprecated
.GetIndirectDependenciesDFStack <- function (name, version, meta = NA) {

    if (!is.data.frame(meta)) {
        meta <- .ReadMeta()
    }
    meta.row <- meta[meta$name == name & meta$version == version, ]
    if (nrow(meta.row) != 1) {
        return(FALSE)
    }
    d <- .DependenciesToDf(meta.row$dependencies)

    if (nrow(d) > 0) {
        for (i in 1:nrow(d)) {
            cur.d.name <- as.character(d$name[i])
            cur.d.version <- as.character(d$version[i])
            cur.d.dependencies <- .GetIndirectDependenciesDFStack(cur.d.name, cur.d.version, meta)
            d <- rbind(d, cur.d.dependencies)
        }
    }
    return(d)
}

#' given a name and version, will return a tree of all the dependencies and their dependencies etc
#'
#' @param name string;
#' @param version int;
#' @param meta data.frame; optional. The dataframe of all dataobjects. If ommited will read from disk
#'
#' @value
#' list in the form
#' list(dependency.1 = list(version = 12,
#'                         dependencies = list(dependency.1.1.name = list(version = 3,
#'                                                                        dependencies = list())
#'                                             dependency.1.2.name = list(version = 2,
#'                                                                        dependencies = list())))
#'     dependency.2 = list(version = 12,
#'                         dependencies = list(dependency.2.1.name = list(version = 3,
#'                                                                        dependencies = list())
#'                                             dependency.2.2.name = list(version = 2,
#'                                                                        dependencies = list()))))
.GetIndirectDependenciesTree <- function (name, version, meta = NA) {

    require('rjson')
    if (!is.data.frame(meta)) {
        meta <- .ReadMeta()
    }
    meta.row <- meta[meta$name == name & meta$version == version, ]
    if (nrow(meta.row) != 1) {
        return(FALSE)
    }
    d <- fromJSON(meta.row$dependencies)
    d.names <- names(d)
    if (length(d) > 0) {
        for (i in 1:length(d)) {
            cur.d.name <- d.names[i]
            cur.d.version <- d[[d.names[i]]]
            cur.d.dependencies <- GetIndirectDependencies.recursive(cur.d.name, cur.d.version, meta)
            d[[cur.d.name]] <- list(version = cur.d.version, dependencies <- cur.d.dependencies)
        }
    }
    return(d)
}


#' Converts data to json for use with D3 and SVG
#' This is now no longer the correct format. Just leaving it here for the sake of it.
#'
#' @details
#'   Final format will be something like this:
#'
#'   {
#'      "groups":{
#'          'names':['name1','name2','name3'], // the different types of data
#'          'format':['csv','object','csv']  // the format of the data
#'          'count':[1,2,10] // how many of each group there is
#'          },
#'      "group_links":{
#'          "from":[from1, from2,from3], // this is redundant, and will be based on the version links
#'          "to":[to1,to2,to3]
#'          },
#'      "versions":{
#'          'group':[0,1,1], // index of the group
#'          'params':[{'name':val},{'name':val},{'name':val}],
#'          'version':[1,1,1,],
#'          'cols':[['a','b','c'],['a','b','c'],['a','b','c']] // the column names of the csv (if applicable)
#'      },
#'      "version_links":{
#'          "from":[], // the index within the versions list
#'          "to":[]
#'      }
#'   }
#'
.DataVisFlat <- function () {

    m <- .ReadMeta()
    group.names <- unique(m$name)
    group.format <- sapply(m$name, .GetType)
    group.count <- tabulate(as.factor(m$name)) #todo: check that this is in the same order as unique

    version.links <- GetLinks(m, TRUE)

    # map to index of groups list, not meta rows
    group.map <- function (m.row) {
        return(which(group.names == m$name[m.row]))
    }
    group.links <- data.frame(from = sapply(version.links$from, group.map),
                              to = sapply(version.links$to, group.map))
    group.links <- unique(group.links)

    group.links <- as.list(group.links)
    version.links <- as.list(version.links)

    versions <- list(group = sapply(1:nrow(m), group.map))
    versions$params <- unname(sapply(m$params, fromJSON))
    versions$version <- m$version

    # TODO: cols
    groups <- list(names = group.names,
                   format = group.format,
                   count = group.count)

    all.data <- list(groups = groups,
                     group_links = group.links,
                     versions = versions,
                     version_links = version.links)

    all.data <- toJSON(all.data)
    return(all.data)
}

#' converts the meta table to json directed graph
#'
#' @param as.json: boolean; whether it should be returned as a list(false) or json(true)
.MakeDataGraph <- function (include.versions = FALSE) {

    m <- .ReadMeta()
    if (!include.versions) {
        m <- m[m$version == 1,]
    }
    if (include.versions) {
        label <- title <- paste(m$name, m$version, sep = ":")
    } else {
        label <- title <- m$name
    }
    nodes <- data.frame(id = 1:nrow(m),
                        label = label,
                        title = title,
                        shape = "square",
                        color = "green",
                        size = 15)
    if (include.versions) {
        nodes$group <- m$name
    }
    edges <- GetLinks(m, include.versions = include.versions)
    edges$arrows <- "middle"
    visNetwork(nodes, edges, width = "100%")
}

#' produces a list of dependency links by row number of the metadata for use in the directed-graph visualization
#'
#' @param as.list: boolean; Whether the format should be as a list (e.g. for conversion to json)
#'                          or a dataframe (e.g. for use with htmlwidgets in R)
#' @param index.from: int; the source/destination pointer to the rows of the meta should start counting rows from 0 or 1?
#' @details
#' https://cran.r-project.org/web/packages/visNetwork/vignettes/Introduction-to-visNetwork.html
#' http://www.htmlwidgets.org/showcase_visNetwork.html
#' http://visjs.org/docs/network/
.GetLinks <- function (m, include.versions, as.list = FALSE, index.from = 1) {

    # max length for links will be half square of number of nodes
    # changing length of list is slow, so initialise to max then trim at the end
    max.links <- (nrow(m) * nrow(m)) / 2

    link.list <-  list()
    length(link.list) <- max.links
    link.df <- data.frame(from = numeric(max.links), to = numeric(max.links))

    # store current index of dependencies separately for efficiency
    cur.d <- 0

    for (r in 1:nrow(m)) {
        dependencies <- .DependenciesToDf(m[r,'dependencies'])
        for (local.d in seq_len(nrow(dependencies))) {
            # add link

            cur.d <- cur.d + 1

            # source row is the index (starting from 0) of the node of the dependency
            # which is the same as the meta row of the dependency - 1
            if (include.versions) {
                link.source <- which(m$name == dependencies$name[local.d] & m$version == dependencies$version[local.d])
            } else {
                link.source <- which(m$name == dependencies$name[local.d])
            }

            link.source <- link.source + index.from - 1
            if (length(link.source) > 0) {
                if (length(link.source) > 1) {
                    msg <- paste('corrupted meta: ',
                                 dependencies$name[local.d],
                                 'version', dependencies$version[local.d],
                                 'exists', length(link.source), 'times, on rows', paste(link.source,sep=","))
                    stop(msg)
                }
            } else {
                stop('missing dependency')
            }


            link.target <- r + index.from - 1
            link.list[[cur.d]] <- list(source = link.source, target = link.target)
            link.df[cur.d,] <- c(link.source, link.target)
        }
    }

    # trim the list
    length(link.list) <- cur.d
    # trim the df
    link.df <- link.df[1:cur.d,]


    if (as.list) {
        return(link.list)
    } else {
        return(link.df)
    }



}




#' ensure that all columns exist in the metadata dataframe
#'
#' @details
#' If new columns of metadata are changed in future versions of this package
#' This will ensure that existing metadata files can be updated to match the new format
#' TODO: integrate this to be called automatically
.AddColsToMeta <- function () {
    meta <- .ReadMeta()
    for (r in 1:nrow(meta)) {
        col.names <- .GetColNames(meta$name[r], meta$version[r])
        if (is.character(col.names)) {
            meta$col.names[r] <- toJSON(col.names)
        }
    }
    .WriteMeta(meta)
}

#' Gets the column names for the specified dataobject file
#'
#' @param name the name of the dataobject file
#' @param version int the version of the dataobject file
#'
#' @value character vector
#'
#' @details
#'   Checks if the file is a csv
#'   Checks if the file exists
#'   reads only the first line
#'   will cause the zip archiver to unzip the file.
#'
#'   #TODO: no longer functional without getType
.GetColNames <- function (name, version) {

    # path will only be right if it is actually a csv
    path <- .DataobjectPath(name, version, csv = 1, unzip = TRUE)
    if (file.exists(path)) {
        header.line <- readLines(path, n=1)
        col.names <- read.table(textConnection(header.line), sep = ",", stringsAsFactors = FALSE)
        return(as.character(col.names))
    }

    return(NULL)
}


#' Returns whether the type of dataobject should saved as an R object or CSV
#'
#' @param name the name of the dataobject type
#'
#' @details
#' some types of dataobject are data.frames and some are not.
#' Those which can be represented as a data frame are saved as a csv\
#' those which can't must be saved as a binary object
#' this function returns which forms for the given type of dataobject
#'
#' clustering is the object returned by clustering method
#' ranked samples is a 3d array with ranking-method, num-clusters and min-num as the dimensions
#' species.in.each.min and optimal.samples are lists
#'
#' TODO: remove this function and have it detect the type of the data to be saved. For reading, check the metadata
.GetType <- function (name) {

    if (name %in% c('clustering', 'clustering.HA','clustering.kmeans', 'ranked.samples', 'species.in.each.min', 'optimal.samples','silence.model')) {
        return('object')
    } else {
        return('csv')
    }
}
