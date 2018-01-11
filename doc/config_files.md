# Config Files

Most actions require a configuration file which gives you access to various parameters whose values change the outcome of the analysis.

## Format

The config file must be in strict YAML format and should have the file extension `.yml`. 

Comments in the config file give further information. All comment lines must start with a hash symbol `#`. Be careful with the syntax. Incorrect syntax can lead to errors that are difficult to trace. Typically, you will only need to adjust a subset of the available parameters. They all have default values.

You can find an introduction to YAML here: <https://support.ehelp.edu.au/support/solutions/articles/6000055385-introduction-to-yaml>

You can validate YAML files (to check for syntax errors) here: <http://yaml-online-parser.appspot.com/>

## Location

All config files are packaged with _AP.exe_ releases. Inside the package you will
find a `ConfigFiles` folder that contains all the config files.

**IMPORTANT**: Avoid editing these files directly. If you want to change a value:

1. Copy the file to another directory
1. Rename the file to describe the changes
    - e.g. `Towsey.Acoustic.yml` might become `Towsey.Acoustic.HighResolution.yml`
1. Change the values in the file
1. Remember to update the path to the config file when you run _AP.exe_
