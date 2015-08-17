library(shiny)

GetSpeciesListFromFile <- function () {
    # gets the species list from a csv file
    # if file is not present, will use the database
    path <- file.path(GetPathToCsv(),'sp.csv')
    if (file.exists(path)) {
        species.list <- read.csv(path)       
    } else {
        write.csv(species.list, path)
    }  
    return(species.list)  
}


GetPathToCsv <- function () {
    if (file.exists(file.path('csv','sp.csv'))) {
        return('csv')
    } else {
        return(file.path('species_freq_profiles','csv'))
    }   
}

species <- GetSpeciesListFromFile()
species.labels <- as.list(species$id)
names(species.labels) <- species$name

# Define UI for application that draws the species list
shinyUI(fluidPage(
    
    tags$head(
        tags$style(HTML(".shiny-options-group { max-height: 300px; overflow-y: scroll; }"))
    ),
    
    # Application title
    titlePanel("Species Frequency Profiles!"),
    
    # Sidebar with a checkbox for species
    sidebarLayout(
        sidebarPanel(
            checkboxGroupInput("sp.id", "Species Id:", species.labels)
        ),
        # Show a plot of the generated distribution
        mainPanel(
            plotOutput("distPlot"),
            plotOutput("hms")
        )
    )

))
