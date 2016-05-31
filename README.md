audio-analysis
==============

The audio analysis source base for the QUT Bioacoustics Research Group

Structure
---------

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
- [Code Contracts](https://visualstudiogallery.msdn.microsoft.com/1ec7db13-3363-46c9-851f-1ce455f66970)

### R
	
- R
- R Studio

### Matalab

- Matlab