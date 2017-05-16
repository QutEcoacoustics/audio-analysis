#install.packages('rjson','tuneR','lattice','grid','biOps','rgl','digest','parallel','doParallel',
#                 'foreach','stringr','digest','plyr','Matrix','dplyr','rattle','RMySQL')

install.my.packages = function () {
  
    install.packages('rjson')
    install.packages('tuneR')
    install.packages('lattice')
    install.packages('grid')
    install.packages('rgl')
    
    install.packages('parallel')
    install.packages('doParallel')
    install.packages('foreach')
    
    install.packages('stringr')
    install.packages('plyr')
    install.packages('Matrix')
    install.packages('dplyr')
    install.packages('rattle')
    install.packages('RMySQL')
    install.packages('htmlwidgets')
    install.packages('testthat')  
    
}

install.my.packages()

# then open package projects in Rstudio and rebuild to install custom built packages



# setwd("/Users/n8933464/Documents/github/audio-analysis/AudioAnalysis/RCode/Phil/sampleselection/source")



