# Config Files

Most commands require a configuration file which gives you access to various
parameters whose values change the outcome of the analysis.

## Syntax

The config file must be in strict YAML format and should have the file extension
`.yml`. 

Comments in the config file give further information. All comment lines must
start with a hash symbol `#`. Be careful with the syntax. Incorrect syntax can
lead to errors that are difficult to trace. Typically, you will only need to
adjust a subset of the available parameters. They all have default values.

You can find an introduction to YAML here:
<https://support.ehelp.edu.au/support/solutions/articles/6000055385-introduction-to-yaml>

You can validate YAML files (to check for syntax errors) here:
<http://yaml-online-parser.appspot.com/>

## Location

All config files are packaged with _AP.exe_ releases. Inside the package you will
find a `ConfigFiles` folder that contains all the config files.

**IMPORTANT**: Avoid editing these files directly. If you want to change a value:

1. Copy the file to another directory
1. Rename the file to describe the changes
    - e.g. `Towsey.Acoustic.yml` might become `Towsey.Acoustic.HighResolution.yml`
    - See the [Naming] section below for naming rules for the config file
1. Change the values in the file
1. Remember to update the path to the config file when you run _AP.exe_

## Naming

Since [72aab48](https://github.com/QutEcoacoustics/audio-analysis/commit/72aab48d1488622e535d147de521512691102a5a)
the naming format of the config files is now important. We use the name to determine which analysis to run.
For any config file used by `audio2csv`/`AnalyzeLongRecording`
the name of the config file must follow this format:

```
<author>.<analysis>[.<tag>].yml
```

If you find a config file that does not match this format, it will likely produce an error.
If your config file must be named in a different format the `--analysis-name` argument can be used to
disambiguate the analysis type you want to use.

Here are some valid examples:

- `Towsey.Acoustic.yml`
- `Towsey.Acoustic.Marine.yml`
- `Towsey.LitoriaFallax.CustomSettings_23.AnotherTag.yml`

Here are some **invalid** examples:

- `TowseyAcoustic.yml`
- `Towsey.Acousticyml`
- `Towsey.Acousticmarine.yml`

Please note this rule does not apply to other config files not directly used by `audio2csv`. For example,
`IndexProperties.yml` needs no partiuclar naming format to valid.
