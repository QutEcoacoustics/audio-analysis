---
title: Config Files
uid: basics-config-files
---

# Config Files

Most commands require a configuration file which gives you access to various
parameters whose values change the outcome of the analysis.

## Syntax

The config file must be in strict YAML format and should have the file extension
`.yml`.

- Comments in the config file give further information. All comment lines must
start with a hash symbol `#`
- Be careful with the syntax. Incorrect syntax can lead to errors that are difficult to trace.
- Typically, you will only need to adjust a subset of the available parameters
- Most parameters have default values

You can find an introduction to YAML here:
<https://support.ehelp.edu.au/support/solutions/articles/6000055385-introduction-to-yaml>

You can validate YAML files (to check for syntax errors) here:
<http://yaml-online-parser.appspot.com/>

When editing YAML files follow these rules:

- Use a good editor like [Visual Studio Code](https://code.visualstudio.com/) which will detect mistakes and highlight
  different parts of the file with different colours for you
- Always indent lines with four (4) spaces (<kbd>Space Bar</kbd>)
- **Never** use the <kbd>Tab ↹</kbd> key or tab (`\t`) character to indent lines

## Location

All config files are packaged with _AP.exe_ releases. Inside the package you will
find a `ConfigFiles` folder that contains all the config files.

**IMPORTANT**: Avoid editing these files directly. If you want to change a value:

1. Copy the file to another directory (a personal folder)
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

```ebnf
<author>.<analysis>[.<tag>]*.yml
```

The `author` and `analysis` name sections are mandatory. The `tag` section is optional, ignored by _AP_, and can be repeated.

Here are some valid examples:

- `Towsey.Acoustic.yml`
- `Towsey.Acoustic.Marine.yml`
- `Towsey.LitoriaFallax.CustomSettings_23.AnotherTag.yml`
- `Truskinger.NinoxBoobook.You.Can.Have.As.ManyDottedTags.As.YouWant.AfterTheFirstTwo.yml`

Here are some **invalid** examples:

- `TowseyAcoustic.yml`  
  there's no dot (`.`) between the `Towsey` and `Acoustic` parts
- `Towsey.Acousticyml`  
  there's no dot (`.`) between the `Towsey.Acoustic` and `yml` parts
- `Towsey.Acousticmarine.yml`  
   _AP_ looks for an analysis called `Towsey.Acousticmarine` which doesn't exist. It should be `Towsey.Acoustic.marine.yml`

If you find a config file that does not match this format, it will likely produce an error.
If your config file must be named in a different format the `--analysis-name` (or the short form `-a`) argument can be used to
disambiguate the analysis type you want to use.

Please note this rule does not apply to other config files not directly used by `audio2csv`. For example,
`IndexProperties.yml` needs no particular naming format to valid.

## Editing

Basic changes to a config file can be minimal. For example to change the resample rate for an analysis, only the number
needs to be changed:

```diff
-ResampleRate: 22050
+ResampleRate: 16000
```

Most of our config files contain comments next to the parameters that explain what a parameter does. A comment is any
line that begins with an hash symbol (`#`). You can see the text that is a comment is coloured differently from the 
parameter in the example below:

```yaml
# SegmentDuration: units=seconds;
# Long duration recordings are cut into short segments for more efficient processing.
# Default segment length = 60 seconds.
# WARNING: You should not change this property!!
SegmentDuration: 60
```

## Profiles

The most variable part of a config file is the `Profiles` section. Profiles
allow us to add extra sections to an analysis. This can be useful for dealing with:

- Geographical variation in calls.
  Often a  species call will vary between regions. The same detector can work for the different variants of a call but
  slightly different parameters are needed. In this case we add a profile for each regional variation of the call that
  have slightly different parameters or thresholds.
- Generic recognition efforts.
  Each different type of syllable detection we want to use in a <xref:guides-generic-recognizers> is added into a
  different profile. In this way we can detect many different syllable variants and types in a fairly generic manner.

Some analyses do not have a `Profiles` section. For those there's nothing to change.

For config files that do support a `Profiles` section, the format will be as follows:

```yml
# the word Profiles will always be at the start of the line
Profiles:
    # each profile will have a name
    MyName:
        # Each profile will have some parameters
        SomeParameter: 123
        AnotherParameter: "hello"
    # more than one profile can be added
    #            We use the `!type` notation to tell AP what type of parameters we're giving it
    KoalaExhale: !OscillationParameters
        ComponentName: Oscillation 
        SpeciesName: PhascolarctosCinereus
        FrameSize: 512
        FrameStep: 256
        WindowFunction: HANNING
        BgNoiseThreshold: 0.0
        MinHertz: 250
        MaxHertz: 800
        MinDuration: 0.5
        MaxDuration: 2.5
        DctDuration: 0.30
        DctThreshold: 0.5
        MinOcilFreq: 20
        MaxOcilFreq: 55
        EventThreshold: 0.2
    # And another profile using the blob type (!BlobParameters) parameters
    KoalaInhale: !BlobParameters
        ComponentName: Inhale
        MinHertz: 800          
        MaxHertz: 8000
        MinDuration: 0.15
        MaxDuration: 0.8
        DecibelThresholds:
            - 9.0
```

Profiles can get complicated. Each configuration file should detail the different options available. If they don't, then
please let us know!

For more information on constructing generic recognizers see <xref:guides-generic-recognizers>.
