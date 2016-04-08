#' Create a html file based on a template and a dataframe of data to insert
#'
#' @param df dataframe; contains the data that will be inserted in a reapeater
#' @param template.file: string; The name of the file that will be used as the template,
#'                          which is basically a html file with some specially delimeted flags
#'                          showing where the data should be inserted.
#' @param output.fn: string; where the html file should be saved
#' @param singles: list; extra data that is not contained in the data.frame that will be inserted as singles (e.g. page title)
#' @return string; The path where the html file was saved
#' @export
#'
HtmlInspector <- function (template.path, output.path, df = NULL,  singles = list('title' = 'inspect segments')) {

    if (!file.exists(template.path)) stop(paste('template path does not exist: ', template.path))

    if(file.info(template.path)$isdir) {
        # the template is a directory, meaning it consists of a template plus include files
        template.file <- file.path(template.path, 'index.html')
        if (!file.exists(template.file)) stop('If template path is a directory, it must contain index.html')
    } else {
        template.file <- template.path
    }

    # output path must be a file name inside an existing directory
    # or an existing directory
    if (file.exists(output.path)) {
        if (file.info(output.path)$isdir) {
            # output.path is an existing directory
            # use template name as output file name
            output.path <- file.path(output.path, basename(template.file))
        } else {
            # output path is an existing file, overwrite it
        }
    } else {
        if (file.exists(dirname(output.path))) {
            # output path doesn't exist, but its parent directory does
            # output.path
        } else {
            stop("Specified output path doesn't exist")
        }
    }

    template <- readChar(template.file, file.info(template.file)$size)
    if(file.info(template.path)$isdir) {
        template <- .InsertIncludes(template, template.path)
    }
    result <- .InsertData(df, template, singles)


    fileConn<-file(output.path)
    writeLines(result, fileConn)
    close(fileConn)
    return(output.path)
}


#' Recursive function to insert repeating data from a data frame and singles from a list
#'
#' @param df: dataframe
#' @param template: string; The actual template text (not the file name). Since this is recursive,
#'                     it can be the entire template or just the part from inside a repeater
#'                     in a deeper recursion level
#' @param singles: list
.InsertData <- function (df, template, singles = data.frame()) {

    # find first repeater
    open.ex <- "<##startforeach\\{[0-9a-z.]*\\}##>"
    open.loc <- str_locate(template, open.ex)
    if (!is.na(open.loc[1])) {
        open.txt <- str_sub(template, start = open.loc[1], end = open.loc[2])
        rep.name <- str_sub(str_extract(open.txt, "\\{[0-9a-z.]*\\}"), start = 2, end = -2)
        close.ex <- paste0("<##endforeach\\{",rep.name,"\\}##>")
        close.loc <- str_locate(template, close.ex)
        if (is.na(close.loc[1])) {
            stop(paste('error in template: missing closing tag for ', open.txt))
        }
        repeater.txt.template <- str_sub(template, start = open.loc[2]+1, end = close.loc[1]-1)
        repeater.res <- ""
        if (str_detect(repeater.txt.template, open.ex)) {
            if (rep.name != '') {
                groups <- unique(df[,rep.name])
                repeater.res <- rep(NA, length(groups))
                for (g in 1:length(groups)) {
                    subset <- df[df[,rep.name] == groups[g],]
                    sub.singles <- list()
                    sub.singles[[rep.name]] <- groups[g]
                    repeater.res[g] <- .InsertData(subset, repeater.txt.template, singles = sub.singles)
                }
                repeater.res <- paste(repeater.res, collapse = "\n")
            }
            # nothing happens if it is a repeater with a name and no nested repeater,
            # maybe need to fix this but I can't think why this would be ever needed
        } else {
            repeater.res <- .InsertIntoTemplate(repeater.txt.template, df)
            repeater.res <- paste(repeater.res, collapse = "\n")
        }
        template <- paste0(str_sub(template, 1, open.loc[1]-1), repeater.res, str_sub(template, close.loc[2]+1, str_length(template)))
        # recurse back into this function in case there are multiple repeaters (I don't see why there should ever need to be)
        # if it was the last one, then it will just return immediately
        template <- .InsertData(df, template)
    }
    #lastly, insert singles
    template <- .InsertIntoTemplate(template, singles)
    return(template)
}


#' Include external files in the page
#'
#' @param template string;
#' @param path; The path to the the parent directory of the included paths
#' @details Included files can be inserted into the template text with special delimeters.
#'          The advantage is that self-contained code such as css or javascript can be kept in their own files.
#' @return string; The template text after includes have been inserted
.InsertIncludes <- function (template, path) {

    placeholder.name.ex <- '[0-9a-zA-Z._]+'
    placeholders.ex <- paste0("<##:",placeholder.name.ex,"##>")
    placeholders <- unique(unlist(str_extract_all(template, placeholders.ex)))
    placeholder.names <- unlist(str_extract_all(placeholders, placeholder.name.ex))
    if (length(placeholder.names) == 0) {
        return(template)
    }
    include.paths <- file.path(path, placeholder.names)
    verify = file.exists(include.paths)
    if (!all(verify)) {
        warning('some flags from templage were not in the replacement list')
    }
    placeholders <- placeholders[verify]
    include.paths <- include.paths[verify]
    for(i in 1:length(placeholders)) {
        file.text <- readChar(include.paths[i], file.info(include.paths[i])$size)
        template <- str_replace_all(template, placeholders[i], file.text)
    }
    return(template)
}

#'  Given a template and some vals, will replace the template flags with the appropriate vals
#'
#' @param template: string: the text containing some placeholder flags that need to be replaced with values
#' @param vals: data frame; The column names match the placeholder names in the template. If there is more
#'                       one row then the template text will be repeated so that there is one
#'                       copy of the template per row.
.InsertIntoTemplate <- function (template, vals) {

    vals <- as.data.frame(vals)
    if (nrow(vals) == 0) {
        return(template)
    }
    placeholder.name.ex <- '[0-9a-zA-Z._]+'
    placeholders.ex <- paste0("<##",placeholder.name.ex,"##>")
    placeholders <- unique(unlist(str_extract_all(template, placeholders.ex)))
    placeholder.names <- unlist(str_extract_all(placeholders, placeholder.name.ex))
    verify <- placeholder.names %in% colnames(vals)
    if (!all(verify)) {
        warning('some flags from templage were not in the replacement list')
    }
    placeholders <- placeholders[verify]
    placeholder.names <- placeholder.names[verify]
    for(i in 1:length(placeholders)) {
        template <- str_replace_all(template, placeholders[i], vals[,placeholder.names[i]])
    }
    return(template)
}

#' Create an example dataframe to use in example templates, and save it to the package's data so that
#' it is available when the package is loaded
.ExampleData <- function () {

    cities <- data.frame(name = c('Tokyo','Seoul','Shanghai','Guangzhou','New Delhi'),
                     country = c('Japan','South Korea','China','China','India'),
                     population = c(36923000,25620000,24750000,23900000,21753486),
                     img = c('https://upload.wikimedia.org/wikipedia/commons/b/b2/Skyscrapers_of_Shinjuku_2009_January.jpg',
                             'https://upload.wikimedia.org/wikipedia/commons/4/4b/Seoul-Cityscape-03.jpg',
                             'https://upload.wikimedia.org/wikipedia/commons/d/de/Shanghai_montage.png',
                             'https://upload.wikimedia.org/wikipedia/commons/f/fe/Guangzhou_montage.png',
                             'https://upload.wikimedia.org/wikipedia/commons/7/73/NewDelhiMontage.png'))

    save(cities, file = 'data/cities.Rdata')

}

#' Process the example templates using the example data,
#' @details saves the output to /inst/example_output
.Example <- function () {

    # outputs to example_output folder using name of template
    HtmlInspector(template.path = 'inst/example_templates/cities.html', output.path = "./inst/example_output", df = cities)

    # outputs to example_output folder using specified file name
    HtmlInspector(template.path = 'inst/example_templates/cities.html', output.path = "./inst/example_output/test.html", df = cities)

    # example using template directory with include files
    HtmlInspector(template.path = 'inst/example_templates/include_example', output.path = "./inst/example_output/include_test.html", df = cities)
}



