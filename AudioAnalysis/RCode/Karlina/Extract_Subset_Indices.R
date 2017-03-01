DirName <- "C:\\Work\\RHome\\Groote_Projects\\Indices\\ACI"
DirName2 <-"C:\\Work\\RHome\\Groote_Projects\\Indices"
Filelist <- list.files(path = DirName)

#In name, use specific species and type of index that wants to be extracted

dir.create(paste(DirName2, "Lrothii_ACI", sep="\\"))

#if transposed

  #dir.create(paste(DirName, "transposed_LrothiiACI", sep="\\"))
           
for(indices_file in Filelist) { 
  path <- paste(DirName, indices_file, sep="\\")
  index_data <- read.csv(path)
  
#if transpose is desired

  #transposed_index_data <- t(index_data)
  
#Index selection corresponds to the frequency bandwith of a frog. Specific for Groote Eyandt the bandwith are as follows
#L_rothii bandwith = 23:68
#Lbicolor = 49:145
#Lwotjulumensis = 22:96
#Uinundata = 33:80
  
  index_sub_selection <- index_data[,23:68]
  
#use the following if transposed
  
  #index_sub_selection <- transposed_index_data[23:68,]
  

  new_filename <- paste("ACI_", indices_file)
  output_csv <- paste(DirName2, "\\Lrothii_ACI\\", new_filename, ".csv", sep = "")
  write.csv(index_sub_selection, output_csv)
}

