---
uid: cli
---

# Introduction To Running Commands

## You need a shell

On Windows we recommend using a PowerShell prompt, but CMD will also work. On
Unix systems any terminal, like BaSH will work.

## Example syntax

For examples of how to call the executable from the command line, we use the `$`
symbol to denote a command line prompt. Do not type the `$` symbol yourself.

On Windows machines the standard prompt is a `>` character. We use a `$` sign in
these examples only for consistency.

Special mentions:

- Powershell & Bash
    - All executables must be prefixed with a `./` if you're in the same folder as the executable

## Your first execution

In your shell, type the following and press <kbd>Enter</kbd>.

```
$ AnalysisPrograms.exe
```

This will produce several lines of output, including the program version number
and instructions on how to set debug levels and verbosity. These options have
sensible defaults and you do not need to change them.

## Seeing what _AP.exe_ can do

```
$ AnalysisPrograms.exe list
```

Will print a list of the available *commands*. Every command has an command-name by
which it is invoked. Some commands perform acoustic analyses.

When constructing a command line, the first argument after the executable file
name must be the command name, which will typically be followed by a list of
options for that command:

```
$ AnalysisPrograms <command-name> [options...]
```

To obtain help with the options of a particular command, type:

```
$ AnalysisPrograms.exe help <command-name>
```

For example:

```
$ AnalysisPrograms.exe help audio2csv
```

In this command line, ‘help’ is a *command* and the command-name, *audio2csv*, is
an option.

## Options

The command line options (all prefixed with a hyphen (`-`)), have a short and
long form. The short form is always shown to the left of the long form and using
either is equivalent. The value for the option should follow the option name,
separated by a space. Use double quotes to group values together.

 We use long form options in this manual for clarity. See output from the `help`
 command for all options.

## Global options

Here is a short description of some of global options. As mentioned previously,
you can ignore these options until you need them.

### Console and Log Verbosity

Verbosity of the logging output can be set by appending the `loglevel` options
to the command line:

Valid verbosity values are:

-   `None` = 0 - show nothing
-   `Error` = 1 - show only errors
-   `Warn` = 2 - show only warnings
-   `Info` = 3 - the standard level
-   `Debug` = 4 - print some debug statements that show variable state and extra
    information
-   `Trace` = 5 - print many more debugging statements with detailed variable
    values
-   `Verbose` = 6 - print all stdout and stderr from associated tools
-   `All` = 7 - print absolutely everything

For example: `-l 4`, will give you the `debug` level of verbosity.

Alternatively, you can append one of the following switches to the command line:

-   `-v` Sets the logging to verbose. Equivalent to LogLevel = `Debug` = 4
-   `-vv` Sets the logging to very verbose. Equivalent to LogLevel = `Trace` = 5
-   `-vvv` Sets the logging to extremely verbose. Equivalent to LogLevel = `All`
    = 7

### Environment variables

- `AP_PLAIN_LOGGING`: `[true|false]`-- Enables simpler logging and no color
  output--the default is value is `false`
- [NOT YET IMPLEMENTED] `AP_DISABLE_METRICS` -- if defined will not send
  performance metrics back to the developers

## Beware these Syntax Gotchas**

-   **Never** finish a double quoted string argument with a backslash (\\). In
    particular, do *not* end directory names like this:
    “C:\\\\Path\\OutputDirectory\\”. The parsing rules for such cases are
    complicated and outside of our control. See
    [here](https://msdn.microsoft.com/en-us/library/system.environment.getcommandlineargs.aspx)
    for details.
-   You can test arguments on Windows with the `echoargs.EXE` program
-   The arguments used are one of the first lines logged in _AP.exe_ log file
-   If an input argument is an array (e.g. directoryinfo\[\]), any commas in the
    argument will delimit the values. For example, "Y:\\Results\\abc, 123,
    doo-dah-dee" will be parsed as "Y:\\Results\\abc", " 123", " doo-dah-dee".
