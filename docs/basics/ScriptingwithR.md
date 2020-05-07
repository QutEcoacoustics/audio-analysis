# Scripting with R
Since most ecologists and biologists are more used to using R than shell, we provide here an R script to run *AP.exe*

## Set the directory containing the files
Assing using ``` <- ``` the folder where the audio files are located to the object ``` directory ```, like this:

``` directory <- "C:\\Temp\\Workshop" ```

## Storing the results
Assign (```<-```) now a directory (```base_output_directory```) to store the results when the analyses finish, like this:

``` base_output_directory <- "C:\\Temp\\Workshop\\BatchIndicesOutput" ```

## Listing the audio files inside the directory
Create a list with all the audio files inside the ```directory```. In this case, we indicated we want all the files with the extension *.wav* to be listed. If your files have a different extension, you'll need to indicate the right extension after ```pattern = ``` (and don't forget to put the extenstion between double quotes)

``` files <- list.files(directory, pattern = "*.wav", full.names = TRUE) ```

## Iterate through each file
Here we indicate we will do this whole process for each file in our list of files:
1. Get the filename
2. Create a folder for each file inside the ```base_output_directory```
3. Prepare the command, which means put together the bits that will form the command that R and the computer will understand
4. Execute the command - Run the analysis

```
for(file in files) {
  message("Processing ", file) 
  
  # get just the name of the file
  file_name <- basename(file)
  
  # make a folder for results
  output_directory <- normalizePath(file.path(base_output_directory, file_name))
  dir.create(output_directory, recursive = TRUE)
  
  # prepare command
  command <- sprintf('audio2csv "%s" "Towsey.Acoustic.yml" "%s" ', file, output_directory)
  
  # finally, execute the command
  system2('C:\\AP\\AnalysisPrograms.exe', command)
}
```
