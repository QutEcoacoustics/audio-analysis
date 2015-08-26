# 3 July 2015
# Calculates the correleation matrix on the normalised indices

#  This file is #3 in the sequence:
#  1. Save_Summary_Indices_ as_csv_file.R
#  2. Plot_Towsey_Summary_Indices.R
# *3. Correlation_Matrix.R
#  4. Principal_Component_Analysis.R
#  5. Quantisation_error.R
#  6. kMeans_Clustering.R
#  7. Distance_matrix.R
#  8. Minimising_error.R
#  9. Segmenting_image.R
# 10. Transition Matrix 
# 11. Cluster time series

#setwd("C:\\Work\\CSV files\\Data 15 to 20 March 2015 Woondum - Wet Eucalypt\\")
#setwd("C:\\Work\\CSV files\\Data 22 to 27  March 2015 Woondum - Eastern Eucalypt\\")
#setwd("C:\\Work\\CSV files\\GympieNP1\\2015_06_21\\")
setwd("C:\\Work\\CSV files\\GympieNP1_new\\2015_06_21\\")
#setwd("C:\\Work\\CSV files\\Woondum3\\2015_06_21\\")
#setwd("C:\\Work\\CSV files\\GympieNP1\\2015_06_28\\")
#setwd("C:\\Work\\CSV files\\Woondum3\\2015_06_28\\")

#indices <- read.csv("Towsey_summary_indices 20150315_133427 to 20150320_153429 .csv", header=T)
#indices <- read.csv("Towsey_Summary_Indices 20150322_113743 to 20150327_103745 .csv", header=T)
#indices <- read.csv("Towsey_Summary_Indices_Gympie NP1 20150622_000000to20150628_064559.csv",header = T)
indices <- read.csv("Towsey_Summary_Indices_Gympie NP1 20150622-000000+1000to20150628-064559+1000.csv")
#indices <- read.csv("Towsey_Summary_Indices_Woondum3 20150622_000000to20150628_133139.csv", header = T)
#indices <- read.csv("Towsey_Summary_Indices_Gympie NP1 20150628_105043to20150705_064555.csv",header = T)
#indices <- read.csv("Towsey_Summary_Indices_Woondum3 20150628_140435to20150705_064558.csv", header = T)

setwd("C:\\Work\\CSV files\\GympieNP1_new\\2015_06_21\\standard_normalisation\\")
site <- indices$site[1]
date <- indices$rec.date[1]

# Standard normalisation and transformation of
# AvgSnrofActiveFrames and Temporal Entropy
new.indices <- indices[4:20]
new.indices[4] <- log(new.indices[4])
new.indices[11] <- log(new.indices[11])
new.indices[1:17] <- scale(new.indices[1:17])
boxplot(new.indices)

# save the correlation matrix_name(location and date).csv in csv folder
a <- cor(new.indices[,1:17][,unlist(lapply(indices[,4:20], is.numeric))])
write.table(a, file = paste("Correlation_matrix_stand_normalisation",site, "_", date,
                            ".csv",sep=""), sep = ",", col.names = NA, 
                            qmethod = "double")