---
title: Scripting with PowerShell
uid: guides-scripting-r
---

# Scripting with R

For those more comfortable with R over a terminal, here is an example of an R
script that runs *AP.exe*.

Using R like this, where we run *AP.exe* with `system2` is identical to using a
shell; `system2` is R's method for running a command.

It is important to still know how to [use a shell](xref:cli) because you may need
to get help (`--help`) for a command, or construct a command that does something else.
Using R is just a wrapper around this process.

It might be helpful to think of _R interactive_ as a shell (like PowerShell or Bash)
and RStudio as a terminal.

Just using R to invoke AP.exe adds a lot of complexity.
However, you may want to do this if you need to:

1. to integrate an _AP.exe_ analysis in with the rest of your R analysis
2. or to batch analyze data.

## Example: batching analysis

Almost always you'd want to run _AP.exe_ on more than one file at once.
You can use any programming language to do this. Here is an example using R
to generate acoustic indices for a folder of audio files:

```r
# Set the directory containing the files
directory <- "C:\\Temp\\Workshop"
# The directory to store the results
base_output_directory <- "C:\\Temp\\Workshop\\BatchIndicesOutput"

# Get a list of audio files inside the directory
# (Get-ChildItem is just like ls, or dir)
files <- list.files(directory, pattern = "*.wav", full.names = TRUE)

# iterate through each file
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

## Script explained

### Set the directory containing the files

Assign using the left arrow operator `<- `the folder where the audio files
are located to the variable `directory `, like this:

`directory <- "C:\\Temp\\Workshop" `

This is the directory we want to look in for files.

### Storing the results

Similarly, we choose a directory (`base_output_directory`) to store the
results when the analyses finish, like this:

`base_output_directory <- "C:\\Temp\\Workshop\\BatchIndicesOutput"`

### Listing the audio files inside the directory

Which files do we analyze? We could list them all out by hand - but no one got
time for that!

Instead we create a list with all the audio files inside the `directory`.
In this case, we indicated we want all the files with the extension *.wav* to be
listed. If your files have a different extension, you'll need to indicate the
right extension after `pattern = `(and don't forget to put 
the extension between double quotes)

`files <- list.files(directory, pattern = "*.wav", full.names = TRUE)`

or, as an example, for FLAC files:

`files <- list.files(directory, pattern = "*.flac", full.names = TRUE)`

### Iterate through each file

The for loop lets us do something for every file we found. In our example, _for 
each file_ we:

1. Get the file's name. E.g. From `C:\Temp\Workshop\20190801_131213.wav` we get `20190801_131213.wav`
2. Create a folder for each result set inside the `base_output_directory`. The folder
   will have the name of audio file we're currently working with. E.g. `20190801_131213.wav`
3. Prepare the command, which means put together the bits that will form the 
   command that R and the computer will understand. This is equivalent to typing
   out the command in a shell.
4. Finally, use `system2` to execute the command and run our analysis.
   This is equivalent to pressing <kbd>Enter</kbd> to run our command in the shell.
