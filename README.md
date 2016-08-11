audio-analysis
==============

The audio analysis source base for the QUT Bioacoustics Research Group

## Licence

ALL RIGHTS ARE RESERVED.

THIS CODE BELONGS TO QUT. THIS CODE IS THE COPYRIGHT OF QUT.

INTELLECTUAL PROPERTY OF ALL CONCEPTS WITH THIS REPOSITORY REMAINS PROPERTY OF QUT WITH THE EXCEPTION OF STUDENT'S CODE.
IF STUDENTS ASSIGN THEIR INTELLECTUAL PROPERTY TO QUT THEN THOSE CONCEPTS ALSO BELONG TO QUT.
OTHERWISE, THE INTELLECTUAL PROPERTY OF CONCEPTS WRITTEN BY STUDENTS IN THIS REPOSITORY BELONGS TO THEM.

YOU MAY NOT SHARE, USE, REPRODUCE, PUBLISH, OR OTHERWISE MODIFY ANY CONCEPT, CODE, OR ARTEFACT IN THIS REPOSITORY WITHOUT PERMISSION.
ANY CONTRIBUTIONS TO THIS REPOSITORY REMAIN PROPERTY OF QUT UNLESS OTHERWISE AGREED UPON. IF AN AGREEMENT IS MADE, THAT AGREEMENT MUST
BE INCLUDED IN EACH RELEVANT FILE.

WE RESERVE THE RIGHT TO CHANGE THE CONDITIONS OF THIS LICENSE AT ANY TIME, IN ANY WAY, AND APPLY THOSE CHANGES RETROACTIVELY.

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

- Visual Studio 2015
- Resharper Ulitmate (Academic License)
  - Install these plugins (_ReSharper_ menu > _Extension Manager_)
    - ReSpeller Free
    - StyleCop by JetBrains
- [Code Contracts](https://visualstudiogallery.msdn.microsoft.com/1ec7db13-3363-46c9-851f-1ce455f66970)

### R
	
- R
- R Studio

### Matalab

- Matlab
