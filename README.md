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

