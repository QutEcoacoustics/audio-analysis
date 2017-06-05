# audio-analysis

The audio analysis source base for the QUT Bioacoustics Research Group

## Build status

[![Build status](https://ci.appveyor.com/api/projects/status/ntf6vtuy5wnlww37/branch/master?svg=true)](https://ci.appveyor.com/project/QUTEcoacousticsResearchGroup/audio-analysis/branch/master)

**NEW**: Weekly versions of AnalysisPrograms.exe are now built automatically at midnight Monday night.
You can get copies from the [Releases](https://github.com/QutBioacoustics/audio-analysis/releases) page.

**NEW**: Per-commit builds can be found from the AppVeyor
[artifacts](https://ci.appveyor.com/project/QUTEcoacousticsResearchGroup/audio-analysis/build/artifacts)
page.

## Licence

ALL RIGHTS ARE RESERVED.

THIS CODE BELONGS TO QUT. THIS CODE IS THE COPYRIGHT OF QUT.

INTELLECTUAL PROPERTY OF ALL CONCEPTS WITHIN THIS REPOSITORY REMAIN PROPERTY OF QUT WITH THE EXCEPTION OF STUDENT'S CODE.
IF STUDENTS ASSIGN THEIR INTELLECTUAL PROPERTY TO QUT THEN THOSE CONCEPTS ALSO BELONG TO QUT.
OTHERWISE, THE INTELLECTUAL PROPERTY OF CONCEPTS WRITTEN BY STUDENTS IN THIS REPOSITORY BELONGS TO THEM.

YOU MAY NOT SHARE, USE, REPRODUCE, PUBLISH, OR OTHERWISE MODIFY ANY CONCEPT, CODE, OR ARTEFACT IN THIS REPOSITORY WITHOUT PERMISSION.
ANY CONTRIBUTIONS TO THIS REPOSITORY REMAIN PROPERTY OF QUT UNLESS OTHERWISE AGREED UPON. IF AN AGREEMENT IS MADE, THAT AGREEMENT MUST
BE INCLUDED IN EACH RELEVANT FILE.

WE RESERVE THE RIGHT TO CHANGE THE CONDITIONS OF THIS LICENSE AT ANY TIME, IN ANY WAY, AND APPLY THOSE CHANGES RETROACTIVELY.

## Best Practices

- Set the git `autocrlf` config setting. See <https://help.github.com/articles/dealing-with-line-endings/#global-settings-for-line-endings> for instructions.
- Avoid adding binary content to this repository - especially _RData_ files.
- NEVER commit if the code does not build
- Try to work on branches if your code negatively affects production code
- Write code in American English. Documentation may be written in Australian English.

## Daily routine

- ALWAYS _Sync_ (or `git pull`) before you start work
- Do your work
- Add your files (Check the checkboxes or `git add`)
  - ALWAYS check the files you add - often files you don't want to add are added automatically
- Commit the files in units of work (press the commit button or `git commit`). Always provide a descriptive message.
- ALWAYS _Sync_ (or `git push`) after you start work


## Structure

The `Acoustics` and `AudioAnalysis` folders contain the code and resources for
the audio analysis work.  The `Extra Assemblies` folder contains `.dll` files
and other binary resources.


## Blobs

We use [git-lfs](https://git-lfs.github.com/) to store BLOBs for testing audio
file converters. If you want to run the unit tests you need to have git-lfs 
installed.

Not all BLOBs are stored in git-lfs. So far only the audio files in 
`Acoustics\Acoustics.Test\TestResources` have been added.

## Required Software

### .Net Solutions

- Visual Studio 2017
- Resharper Ulitmate (Academic License)
  - Install these plugins (_ReSharper_ menu > _Extension Manager_)
    - ReSpeller Free
- [msysgit](https://git-for-windows.github.io/)

### R
	
- R
- R Studio

### Matalab

- Matlab

# Making a release

 1. Pull latest changes to your local computer
 2. Make sure the repo is clean (no uncommitted changes)
 8. Open a prompt, `cd` to the git repo folder
 9. Run `. .\release.ps1`
 10. Profit :sparkles: :moneybag: :dollar: :heavy_dollar_sign: :sparkles:
