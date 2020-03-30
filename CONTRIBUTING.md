# Contributing to audio-analysis


## Best Practices

- Set the git `autocrlf` config setting. See
  <https://help.github.com/articles/dealing-with-line-endings/#global-settings-for-line-endings> for instructions.
- Avoid adding binary content to this repository. If it must be added, ensure git-lfs is tracking it.
- NEVER commit if the code does not build to the `master` branch
- Try to work on branches if your code negatively affects production code
- Write code in American English. Documentation may be written in Australian English.
- Wherever possible **use un-prefixed SI units for variables**
    1. Variables with no unit **MUST** be standard units
        - `var duration = 30` should **always** be 30 seconds
        - `var bandwidth = 50` should **always be hertz**
    1. **Never** use imperial units
    1. **Always** include the full unit in the name if it does not follow our standard
        - avoid this if possible, see first point
        - e.g. `var minKiloHertz = 3.5`
        - e.g.  `var limitHours = 6`
    1. **Do not** abbreviate units
    1. It is **recommended** that full units be used in any user facing field name
        - e.g. `EventEndSeconds` in a CSV file
- Dates and durations:
    1. **ONLY** format dates in an ISO8601 format
        - a modified ISO8601 format with all punctuation removed is acceptable
          for file/folder names. Example format string: yyyyMMddTHHmmssZ
    1. **ALWAYS** format dates with UTC offset information if available
    1. **PREFER** formatting dates in the UTC timezone
    1. **AVOID** exposing `TimeSpan`s to user facing fields (use seconds instead)
        - if a `TimeSpan` needs to be formatted
            - in a log: the default formatting is acceptable
            - in a filename: use ISO8601 duration formatting


## Required Software

The **required** software for developing new code (not running the program) includes:

- **PowerShell Core** (version 6+)
    - You can install it from here: https://github.com/powershell/powershell#get-powershell
- **.NET Core SDK**
    - We aim to use the latest stable version
    - You can verify the version our project is using by looking in the [global.json](./global.json) file
    - You can download it from here: <https://dotnet.microsoft.com/download/dotnet-core/>
        - Note: you want the _Build apps - SDK_ download
    - Alternately, use one of the dotnet-install scripts from the [./build/](./build) folder
        - e.g. On Windows in PowerShell: ./build/dotnet-install.ps1
        - e.g. On Max/Linux in Bash: ./build/dotnet-install.sh

- An IDE:
    - **Visual Studio 2019** (Windows only)
        - Install features:
            - C# Development
            - .NET Core SDK for Visual Studio 2019
        - If you're at a university that has an Office365 Subscription you can download software from https://azureforeducation.microsoft.com/devtools
        - The [community edition](https://visualstudio.microsoft.com/vs/) of Visual Studio should work fine and is totally free
        - [Optional] Resharper Ulitmate (Academic License)
        - Install these plugins (_ReSharper_ menu > _Extension Manager_)
          - ReSpeller Free
    - **VS Code** (recommended for Mac and Linux)
        - Install from here: <https://code.visualstudio.com/>
        - Open the `ap.code-workspace`
        - Install the recommended workspace extensions
    - **JetBrains Rider** (Windows only)
- **Git**
    - A recent version of the `git` executable must be on your PATH (the standard install should do this)
    - <https://git-scm.com/downloads>
- **Git LFS**
    - <https://git-lfs.github.com/>

## Binary Large Objects (BLOBs)


We use [git-lfs](https://git-lfs.github.com/) to store BLOBs for testing audio
file converters. If you want to run the unit tests you need to have git-lfs 
installed.

Not all BLOBs are stored in git-lfs. See the `.gitattributes` file to list
what files are included.

You can check the status of LFS files with the command `git lfs status`.

### AP001

If you cloned the repository before LFS was installed you will need to:

1. Install Git LFS
2. `cd` to the audio-analysis directory
3. Run `git lfs install` to set  up Git LFS
4. Run `git lfs pull` to download the LFS BLOBs
5. Use `git lfs ls-files` to verify the files have been restored.
  
  > An asterisk (`*`) after the OID indicates a full object, a minus (`-`) indicates an
LFS pointer.


## Third party contributions

Third party contributions should be made by:

- forking the repository
- making changes in a branch
- submitting a pull-request to merge those changes from your-fork and branch, to our copy and master branch

## Help wanted

We mark the most straightforward issues as "up for grabs". This set of issues is the place to start if you are interested
in contributing but new to the codebase.

- [QutEcoacoustics/audio-analysis - "up for grabs"](https://github.com/QutEcoacoustics/audio-analysis/labels/up%20for%20grabs)


## Contribution "Bar"

Project maintainers will merge changes that improve the product significantly and broadly and that align with our roadmap.

Contributions must also satisfy the other published guidelines defined in this document.

We will gladly accept any documentation or script enhancements.

## DOs and DON'Ts

Please do:

* **DO** follow our style (enforced by StyleCop)
* **DO** give priority to the current style of the project or file you're changing even if it diverges from the general 
  guidelines.
* **DO** include tests when adding new features. When fixing bugs, start with adding a test that highlights how the
  current behavior is broken.
* **DO** keep the discussions focused. When a new or related topic comes up
  it's often better to create new issue than to side track the discussion.
* **DO** blog and tweet (or whatever) about your contributions, frequently!

Please do not:

* **DON'T** make PRs for style changes. 
* **DON'T** surprise us with big pull requests. Instead, file an issue and start
  a discussion so we can agree on a direction before you invest a large amount
  of time.
* **DON'T** commit code that you didn't write. If you find code that you think is a good fit, file an issue and start a 
  discussion before proceeding.
* **DON'T** submit PRs that alter licensing related files.

## Commit Messages

Please format commit messages as follows (based on [A Note About Git Commit Messages](http://tbaggery.com/2008/04/19/a-note-about-git-commit-messages.html)):

```
Summarize change in 50 characters or less

Provide more detail after the first line. Leave one blank line below the
summary and wrap all lines at 72 characters or less.

If the change fixes an issue, leave another blank line after the final
paragraph and indicate which issue is fixed in the specific format
below.

Fixes #42
```

Also do your best to factor commits appropriately, not too large with unrelated things in the same commit, and not too
small with the same small change applied N times in N different commits.

## File Headers

StyleCop automatically suggest an appropriate file header. Please use it at the top of all new files.


## Copying Files from Other Projects

We sometimes use files from other projects, typically where a binary distribution does not exist or would be inconvenient.

The following rules must be followed for PRs that include files from another project:

- The license of the file is [permissive](https://en.wikipedia.org/wiki/Permissive_free_software_licence).
- The license of the file is left in-tact.

## Porting Files from Other Projects

There are many good algorithms implemented in other languages that would benefit our project.
The rules for porting an R file to C#, for example, are the same as would be used for copying the same file, as
described above.
