# Supported Execution Platforms

## Platform

### Windows

The _AP.exe_ binary will run on any Windows computer with v4.6.2 of the .NET
Framework installed.
Practically, it means that these variants are supported:

- Client SKUs (stock keeping unit)
    - ✓ 10 Anniversary Update
    - \+ 10 November Update 
    - \+ 10 
    - \+ 8.1
    - \+ 7
- Server SKUs
    - ✓ 2016
    - \+ 2012 R2
    - \+ 2012
    - \+ 2008 R2 SP1

The ticks (✓) mean the framework can be installed, the plusses (+) indicate
the framework should be installed.
The above was taken from <https://docs.microsoft.com/en-us/dotnet/framework/migration-guide/versions-and-dependencies>

### Unix

To run _AP.exe_ on Unix systems, the Mono runtime needs to be installed.
We require **Mono Version 5.5** or greater to be installed.

We support the following distributions:

- Ubuntu 16.04 (used in production)
- Debian 9 (~~not tested~~ our upcoming Docker image is based on Debian)
- Raspian (not tested)

To run a .NET Framework binary with Mono, simply prefix the command you would
use on Windows with the `mono` keyword.
For example, in Windows:

```cmd
> AnalysisPrograms.exe list
```

In Linux:

```
$ mono AnalysisPrograms.exe list
```

### MacOSX

Should the same as the Unix requirements, however, we have never run or tested
the code on MacOS systems.

We'd like to hear from you if you do try!