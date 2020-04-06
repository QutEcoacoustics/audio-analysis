# Changelog

## Unreleased

<!--manual-content-insert-here-->



<!--generated-content-insert-here-->

## Ecoacoustics Audio Analysis Software v20.4.0.144 2020-04-06

Version v20.4.0.144

### Notes

**MAJOR CHANGE: Now runs on .NET Core**

They way you use AnalysisPrograms will not be the same if you are on Linux
or MaxOSX. You will no longer need to install mono, or prefix commands with
`mono`. Please see our
[installing](https://github.com/QutEcoacoustics/audio-analysis/blob/master/docs/installing.md)
documentation for more details!

### Details

[Compare v20.2.0.99...v20.4.0.144](https://github.com/QutBioacoustics/audio-analysis/compare/v20.2.0.99...v20.4.0.144)

- 8743147c Anthony Truskinger - ([#196](https://github.com/QutEcoacoustics/audio-analysis/issues/196))
  Fix ap version var in release script
- ab71c274 QUT Ecoacoustics - Update changelog for v
- 19edbb4d Anthony Truskinger - ([#196](https://github.com/QutEcoacoustics/audio-analysis/issues/196))
  Adds GitHub service connection to pipelines release task
- d1629bc9 Anthony Truskinger - ([#196](https://github.com/QutEcoacoustics/audio-analysis/issues/196))
  Yet another method of resolving the branch name 😪
- 17847379 Anthony Truskinger - ([#196](https://github.com/QutEcoacoustics/audio-analysis/issues/196))
  Change git branch detection in git version script again
- 8f2315ca Anthony Truskinger - ([#196](https://github.com/QutEcoacoustics/audio-analysis/issues/196))
  Add debugging information to git version script
- 4176273d Anthony Truskinger - ([#196](https://github.com/QutEcoacoustics/audio-analysis/issues/196))
  Debug release script
- b6519400 Anthony Truskinger - ([#196](https://github.com/QutEcoacoustics/audio-analysis/issues/196))
  Fix spelling mistake
- 1e77ebcc Anthony Truskinger - Add dependency betwen release jobs
- 90e70e30 Anthony Truskinger - Add names to release jobs and change release condition to use those names
- de17dc6a Anthony Truskinger - Fix bad powershell like
- 24c0b81f Anthony Truskinger - ([#196](https://github.com/QutEcoacoustics/audio-analysis/issues/196))
  fix line break in github credentials
- ecfd17c1 Anthony Truskinger - Revert changes to ACT wait/pump
- 791c44e3 Anthony Truskinger - Battle harden AC tests
- 6d2d3ed5 Anthony Truskinger - ([#196](https://github.com/QutEcoacoustics/audio-analysis/issues/196))
  Remove redundant artifact publish step
- 4b8bd727 Anthony Truskinger - ([#196](https://github.com/QutEcoacoustics/audio-analysis/issues/196))
  Attempt to get release stage to Work
- bd652094 Anthony Truskinger - Fixes for unit tests
- 45e6232b Anthony Truskinger - Fix bad option in publish test results
- 2fd130ec Anthony Truskinger - ([#169](https://github.com/QutEcoacoustics/audio-analysis/issues/169))
  Fix arguments for git_version script
- b1cf305a Anthony Truskinger - ([#196](https://github.com/QutEcoacoustics/audio-analysis/issues/196))
  Adds repeat test attribute for flaky tests
- db5118ad Anthony Truskinger - ([#196](https://github.com/QutEcoacoustics/audio-analysis/issues/196), [#196](https://github.com/QutEcoacoustics/audio-analysis/issues/196))
  [WIP] Add release stage to pipeline
- 53e9efbe Anthony Truskinger - ([#196](https://github.com/QutEcoacoustics/audio-analysis/issues/196))
  Attempts to save version vars for release stage
- e2a5a6bd Anthony Truskinger - ([#196](https://github.com/QutEcoacoustics/audio-analysis/issues/196))
  Try renabling AED code coverage
- 2161ce36 Anthony Truskinger - ([#196](https://github.com/QutEcoacoustics/audio-analysis/issues/196))
  Fix bad variable
- f5f0b330 Anthony Truskinger - ([#196](https://github.com/QutEcoacoustics/audio-analysis/issues/196))
  Fix unblanced parantheses
- 83b6d20c Anthony Truskinger - ([#196](https://github.com/QutEcoacoustics/audio-analysis/issues/196))
  Try and enhance code coverage
- 66651010 Anthony Truskinger - ([#196](https://github.com/QutEcoacoustics/audio-analysis/issues/196))
  Up async wait for test, remove self contianed arg for fx-dependent build
- 074e24fd Anthony Truskinger - ([#196](https://github.com/QutEcoacoustics/audio-analysis/issues/196))
  One last tweak to tests
- fd5162f5 Anthony Truskinger - ([#196](https://github.com/QutEcoacoustics/audio-analysis/issues/196))
  More CI Fixes
- 7cf239ad Anthony Truskinger - ([#196](https://github.com/QutEcoacoustics/audio-analysis/issues/196))
  Fix tests for Mac CI
- 4432180d Anthony Truskinger - ([#196](https://github.com/QutEcoacoustics/audio-analysis/issues/196))
  Add missing if to azure pipelines yaml
- de523ecc Anthony Truskinger - ([#196](https://github.com/QutEcoacoustics/audio-analysis/issues/196))
  Fix bad null literal in buid yaml
- 2a3f6545 Anthony Truskinger - ([#196](https://github.com/QutEcoacoustics/audio-analysis/issues/196))
  Removes support for MP3 for SoX on OSX
- a3091c02 Anthony Truskinger - ([#196](https://github.com/QutEcoacoustics/audio-analysis/issues/196))
  Remove appveyor references
- e196c076 Anthony Truskinger - ([#196](https://github.com/QutEcoacoustics/audio-analysis/issues/196))
  Bump SDK Version
- ab7f71cb Anthony Truskinger - ([#196](https://github.com/QutEcoacoustics/audio-analysis/issues/196))
  Adds LFS file checkout check to build
- da575741 Anthony Truskinger - ([#196](https://github.com/QutEcoacoustics/audio-analysis/issues/196))
  Fix string quoting issues.
- 69f4d483 Anthony Truskinger - ([#196](https://github.com/QutEcoacoustics/audio-analysis/issues/196))
  Adds random MP3 C libs to SoX in attempt to get it work on OSX
- dd28a847 Anthony Truskinger - ([#196](https://github.com/QutEcoacoustics/audio-analysis/issues/196))
  Adds framework dependent build
- 8abafea9 Anthony Truskinger - ([#196](https://github.com/QutEcoacoustics/audio-analysis/issues/196))
  Add extra logging to log file cleanup
- 3381ee93 Anthony Truskinger - ([#196](https://github.com/QutEcoacoustics/audio-analysis/issues/196))
  Disabel musl tests and and more timeouts
- 50fe4497 Anthony Truskinger - ([#196](https://github.com/QutEcoacoustics/audio-analysis/issues/196))
  Attempt to get more information about failing tests
- ebfe8e86 Anthony Truskinger - ([#196](https://github.com/QutEcoacoustics/audio-analysis/issues/196))
  Fixes runtime identifer code generation
- d32268d3 Anthony Truskinger - ([#196](https://github.com/QutEcoacoustics/audio-analysis/issues/196))
  Fixes for CI and tests
- ce8b1996 Anthony Truskinger - ([#196](https://github.com/QutEcoacoustics/audio-analysis/issues/196))
  Fix mistake in azure pipeliens yml
- 7da16240 Anthony Truskinger - ([#196](https://github.com/QutEcoacoustics/audio-analysis/issues/196))
  Ensure wavpack is installed on azure
- c8c8a09c Anthony Truskinger - ([#196](https://github.com/QutEcoacoustics/audio-analysis/issues/196))
  Fix missing rid arguments, add fallback for rid source
- 0358b8c6 Anthony Truskinger - ([#196](https://github.com/QutEcoacoustics/audio-analysis/issues/196))
  Add unit tests for missing wavpack binary
- 6d122983 Anthony Truskinger - ([#196](https://github.com/QutEcoacoustics/audio-analysis/issues/196))
  Fix bad runtime identifer source for CheckExecutePermission test
- 4c02f917 Anthony Truskinger - ([#196](https://github.com/QutEcoacoustics/audio-analysis/issues/196))
  Fixes issue with publish step
- 6673ed40 Anthony Truskinger - ([#196](https://github.com/QutEcoacoustics/audio-analysis/issues/196))
  Fixes stack overflow in AED data generator
- 7a5a13e1 Anthony Truskinger - ([#196](https://github.com/QutEcoacoustics/audio-analysis/issues/196))
  Fix missing arg on RuntimeIdentifierSpecificDataTestMethod
- d0f4d308 Anthony Truskinger - ([#196](https://github.com/QutEcoacoustics/audio-analysis/issues/196))
  Add SoX install script for linux CI
- 62b03206 Anthony Truskinger - ([#196](https://github.com/QutEcoacoustics/audio-analysis/issues/196))
  Fix style warning in SoX Audio utility
- 3c50c3cf Anthony Truskinger - ([#196](https://github.com/QutEcoacoustics/audio-analysis/issues/196))
  More cross-platform test fixes
- 9e78583c Anthony Truskinger - ([#196](https://github.com/QutEcoacoustics/audio-analysis/issues/196))
  Fix more tests
- 14859ffa Anthony Truskinger - ([#196](https://github.com/QutEcoacoustics/audio-analysis/issues/196))
  Fix template syntax
- 0bd8eca7 Anthony Truskinger - ([#196](https://github.com/QutEcoacoustics/audio-analysis/issues/196))
  Fix test, attempt different job factorization strategy
- d8cb6b4b Anthony Truskinger - ([#196](https://github.com/QutEcoacoustics/audio-analysis/issues/196))
  Clean up solution, remove some mono refs in docs
- 7534b47b Anthony Truskinger - ([#196](https://github.com/QutEcoacoustics/audio-analysis/issues/196))
  Fixes various cross platform issues
- 3269c785 Anthony Truskinger - ([#196](https://github.com/QutEcoacoustics/audio-analysis/issues/196))
  Fixes tests for SelfContained/platform-specific builds
- 0ba342ab Anthony Truskinger - ([#196](https://github.com/QutEcoacoustics/audio-analysis/issues/196))
  Attempt to fix CI
- b5a43e8c Anthony Truskinger - ([#196](https://github.com/QutEcoacoustics/audio-analysis/issues/196))
  Attempts to fix tests on linux
- 2b655643 Anthony Truskinger - ([#196](https://github.com/QutEcoacoustics/audio-analysis/issues/196))
  Fix AED tests
- 628d95ce Anthony Truskinger - ([#196](https://github.com/QutEcoacoustics/audio-analysis/issues/196))
  Fixes ribbon plot tests
- 310b38a0 Anthony Truskinger - ([#196](https://github.com/QutEcoacoustics/audio-analysis/issues/196))
  Fixes unit test with hardcoded windows path
- 0a58db50 Charles Alleman - ([#196](https://github.com/QutEcoacoustics/audio-analysis/issues/196))
  Solved AED.Test error on test
- fbe9a784 Anthony Truskinger - ([#196](https://github.com/QutEcoacoustics/audio-analysis/issues/196))
  Simplified path diagnostics test for directories with trailing spaces
- 894012b6 Charles Alleman - ([#304](https://github.com/QutEcoacoustics/audio-analysis/issues/304), [#196](https://github.com/QutEcoacoustics/audio-analysis/issues/196))
  Process extension (#304)
- 9b444ff0 Anthony Truskinger - ([#196](https://github.com/QutEcoacoustics/audio-analysis/issues/196))
  Fixes stack overflow exception in ReadSpectrogram
- 25a77b21 Anthony Truskinger - ([#196](https://github.com/QutEcoacoustics/audio-analysis/issues/196))
  Fixes process runner tests
- 11c9199a Anthony Truskinger - ([#196](https://github.com/QutEcoacoustics/audio-analysis/issues/196))
  Fixes tiler tests
- 75901282 Anthony Truskinger - ([#196](https://github.com/QutEcoacoustics/audio-analysis/issues/196))
  Fixes path diagnostics for Linux
- 71ee22eb Anthony Truskinger - ([#196](https://github.com/QutEcoacoustics/audio-analysis/issues/196))
  Fixes PathUtils tests for Linux
- f952de36 Anthony Truskinger - ([#196](https://github.com/QutEcoacoustics/audio-analysis/issues/196))
  Fixes font fallback option
- cf15a270 Anthony Truskinger - ([#196](https://github.com/QutEcoacoustics/audio-analysis/issues/196))
  Fix Assembly load bug resulting in CSV class maps not getting registered
- 85c5ec50 Anthony Truskinger - ([#196](https://github.com/QutEcoacoustics/audio-analysis/issues/196))
  [WIP] Add negatated platform specific test option
- 01405d0f Anthony Truskinger - ([#196](https://github.com/QutEcoacoustics/audio-analysis/issues/196))
  Removes Zio
- 12f0324d Anthony Truskinger - ([#196](https://github.com/QutEcoacoustics/audio-analysis/issues/196))
  Finishes adding support for Roboto font
- bd7db260 Anthony Truskinger - ([#196](https://github.com/QutEcoacoustics/audio-analysis/issues/196))
  Bump .NET SDK to 3.1.200
- 61be2eca Anthony Truskinger - ([#196](https://github.com/QutEcoacoustics/audio-analysis/issues/196))
  [WIP] Bundles a font to use with AP.exe
- ac009d81 Anthony Truskinger - ([#196](https://github.com/QutEcoacoustics/audio-analysis/issues/196))
  Updates namespaces on FSharp projects
- 5f08faa9 Anthony Truskinger - ([#196](https://github.com/QutEcoacoustics/audio-analysis/issues/196))
  Initial changes to get build working on Linux
- 6e806a40 Anthony Truskinger - ([#196](https://github.com/QutEcoacoustics/audio-analysis/issues/196))
  Adjusts timeouts for tests on CI
- 6876db51 Anthony Truskinger - ([#196](https://github.com/QutEcoacoustics/audio-analysis/issues/196))
  Adds executable check to exe reosolution
- 922027db Anthony Truskinger - ([#196](https://github.com/QutEcoacoustics/audio-analysis/issues/196))
  Cleaned up solution
- a6117a6c Anthony Truskinger - ([#196](https://github.com/QutEcoacoustics/audio-analysis/issues/196))
  Up execution time for test
- 822c3ae5 Anthony Truskinger - ([#196](https://github.com/QutEcoacoustics/audio-analysis/issues/196))
  Fix permissions of included binaries
- 1dca915b Anthony Truskinger - ([#196](https://github.com/QutEcoacoustics/audio-analysis/issues/196))
  Add path diagnostics method
- 671d93f3 Anthony Truskinger - ([#196](https://github.com/QutEcoacoustics/audio-analysis/issues/196))
  Battle hardening ProcessRunner for .NET Core
- ec4502fc Anthony Truskinger - ([#196](https://github.com/QutEcoacoustics/audio-analysis/issues/196))
  Fixing platform breaking unit tests
- b310984c Anthony Truskinger - ([#196](https://github.com/QutEcoacoustics/audio-analysis/issues/196))
  Fix failing unit tests
- 6dd705b8 Anthony Truskinger - ([#196](https://github.com/QutEcoacoustics/audio-analysis/issues/196))
  More hardening for solution root helper
- b01a1b0e Anthony Truskinger - ([#196](https://github.com/QutEcoacoustics/audio-analysis/issues/196))
  fix no build switch
- 1dad15c6 Anthony Truskinger - ([#196](https://github.com/QutEcoacoustics/audio-analysis/issues/196))
  Attempt to get coverlet working, add more diagnostics to test runtime
- 6d38388d Anthony Truskinger - ([#196](https://github.com/QutEcoacoustics/audio-analysis/issues/196))
  Changes unit test solution directory finder method
- b1bca4a0 Anthony Truskinger - ([#196](https://github.com/QutEcoacoustics/audio-analysis/issues/196))
  Fixes bad test invocation for azure CI
- 103ad4d5 Anthony Truskinger - ([#196](https://github.com/QutEcoacoustics/audio-analysis/issues/196))
  Removed and deprecated no longer used audio tools
- 433f4bfa Anthony Truskinger - ([#196](https://github.com/QutEcoacoustics/audio-analysis/issues/196))
  [WIP] refactoring audio utils
- 2735d443 Anthony Truskinger - ([#196](https://github.com/QutEcoacoustics/audio-analysis/issues/196))
  Flesh out azure build
- 69dc04da Anthony Truskinger - ([#196](https://github.com/QutEcoacoustics/audio-analysis/issues/196))
  Change runsettings configuration
- c7c9d28f Anthony Truskinger - ([#196](https://github.com/QutEcoacoustics/audio-analysis/issues/196))
  Vastly simplifies ap version metadata generation
- a3bc20ea Anthony Truskinger - ([#196](https://github.com/QutEcoacoustics/audio-analysis/issues/196))
  More work on CI
- 60dc9d33 Anthony Truskinger - ([#196](https://github.com/QutEcoacoustics/audio-analysis/issues/196))
  Removes auto attach feature
- a0e3d58a Anthony Truskinger - ([#196](https://github.com/QutEcoacoustics/audio-analysis/issues/196))
  Conntinued work on CI
- 02f4cae2 Anthony Truskinger - ([#196](https://github.com/QutEcoacoustics/audio-analysis/issues/196))
  More work on CI
- 5a328826 Anthony Truskinger - ([#196](https://github.com/QutEcoacoustics/audio-analysis/issues/196))
  First (failing attempt) to make Azure Pipelines work
- a0cddca0 Anthony Truskinger - Update contributing guide
- 752eaa46 Anthony Truskinger - Fix clean missing project import reference
- a0bc5ab0 Anthony Truskinger - Fixes all unit tests for .NET Core 😁
- 47ee8ff7 Anthony Truskinger - Fixed metadata generation
- 0d08e64a Anthony Truskinger - Updates ImageSharp libraries
- 9ca5467e Anthony Truskinger - Cleaned up solution file
- 64c2913e towsey - Mostly cosmetic changes.
- 7b4ef823 Anthony Truskinger - Refactors Audio2Sonogram
- 4d67dbf8 Anthony Truskinger - Improves image testing and test output directories
- 7670fe75 Anthony Truskinger - Adds textual pattern test image generation to TestImage
- 4ce35449 Anthony Truskinger - Added wrapper methods for no antialias code
- 4e7b4988 towsey - Fix merging of overlapped acoustic events
- ce503316 towsey - Fix spectrogram image generation from GenericRecognizers
- 5e51bb7a Anthony Truskinger - Wrote tests for combine images methods
- 1117d4e9 Anthony Truskinger - Refactored drawing methods and extensions
- 6c52072e towsey - Trying to get spectrogram images working
- 5c085d9e towsey - Update Audio2Sonogram.cs
- dadf0bfb towsey - Update ImageTools.cs
- df5606d7 Anthony Truskinger - Refactored solution build and tests
- 3f195bec towsey - Fixed various post merge issues
- 1f33214a Anthony Truskinger - Fixes a bug in ImageTools
- bb1c4c67 Anthony Truskinger - ([#241](https://github.com/QutEcoacoustics/audio-analysis/issues/241))
  Refactored .NET Core solution
- 248163f6 Anthony Truskinger - Removes outdated packages directory references
- 2c134cbd Anthony Truskinger - Fixes errors from previous merge commits
- d880a997 Anthony Truskinger - Adds download URL for .NET Core SDK
- 63e6299b Anthony Truskinger - ([#289](https://github.com/QutEcoacoustics/audio-analysis/issues/289), [#288](https://github.com/QutEcoacoustics/audio-analysis/issues/288), [#158](https://github.com/QutEcoacoustics/audio-analysis/issues/158), [#159](https://github.com/QutEcoacoustics/audio-analysis/issues/159))
  Almost completes port to .NET Core
- ecad1de1 Anthony Truskinger - ([#158](https://github.com/QutEcoacoustics/audio-analysis/issues/158), [#159](https://github.com/QutEcoacoustics/audio-analysis/issues/159))
  .NET Core and ImageSharp: fixing unit tests
- fee81bd6 Anthony Truskinger - ([#158](https://github.com/QutEcoacoustics/audio-analysis/issues/158), [#159](https://github.com/QutEcoacoustics/audio-analysis/issues/159))
  Converting to .NET Core and ImageSharp
- a4d25e9d Anthony Truskinger - ([#159](https://github.com/QutEcoacoustics/audio-analysis/issues/159), [#158](https://github.com/QutEcoacoustics/audio-analysis/issues/158))
  Fixing .NET Core incompatibilities
- 79d7ae03 Anthony Truskinger - ([#159](https://github.com/QutEcoacoustics/audio-analysis/issues/159))
  Converted Solution to PackageReferences format
## Version v20.2.0.99 - QUT Ecoacoustics Workshop 2020 Version (14/02/2020)

### Notes

This version of AP was bundled in the USB sticks used in the tutorial of the 2020 QUT Ecoacoustics Workshop.

**IMPORTANT**: This will be the last stable version of AP.exe to be released for some time. The project is changing its fundamental architecture (see #159) from the .NET Framework to .NET Core.

After this point, mono will no longer be required to use AP.exe on Mac and Linux and instead, standalone platform-independent packages will be produced.

This will be an exciting time for us but be wary, there will be dragons ahead.

### Details

[Compare v19.12.0.1...v20.2.0.99](https://github.com/QutBioacoustics/audio-analysis/compare/v19.12.0.1...v20.2.0.99)

- 8bc90089 Anthony Truskinger - Adds useful defaults to ConcatenateIndexFiles command
- ae7501e2 Anthony Truskinger - No not do appconfig name check on mono
- 398bd4f6 Michael Towsey - Update UseModel.cs
- c39bb95a Anthony Truskinger - Updated default BlueEnhanceParameter in Towsey.Acoustic.yml
- 08cb0fe6 Michael Towsey - Work on the harmonic recognizer
- 4571c2de Michael Towsey - a little work on harmonic recognizer
- 4955f30e Michael Towsey - Update ConcatenationTests.cs
- 1dfaa928 Michael Towsey - Get Generic recognizer tests working
- 39e6a38d Michael Towsey - Set up config for doing test of GenericRecognizers
- f9d4f9dc Michael Towsey - Work on Harmonic and Whistle recognizers
- 9511e918 Michael Towsey - Work on Whistle recognizer
- feb72a07 towsey - Change config files
- 5c99a195 towsey - Start on Oscillation recognizer
- 21f89d33 Anthony Truskinger - Fixes window overlap calculation for generic recognizer
- 5b1e4002 Anthony Truskinger - Improves method of registering yaml tag types
- 072bfcf5 towsey - Change two config file names
- 5a37019b Anthony Truskinger - Ensures GenericRecognizerConfig static constructor is invoked
- 4b6a453f towsey - Update LDSpectrogramRGB.cs
- 663b9ef3 Anthony Truskinger - Finished Generic recognizer and tests
- 00086b0b Anthony Truskinger - [WIP] Finished rewwriting generic recognizer
- a2db97af Anthony Truskinger - [WIP] Refactoring generic recognizer
- 262dde4e Michael Towsey - insert configs into generic recogniser classes
- 0d255d4a Michael Towsey - Set up classes
- 2bfa8895 Michael Towsey - Set up Generic recognizers
- c5ab40d3 Michael Towsey - Conflict resolution
- 701b9053 Michael Towsey - Updated image sharp packages
- 66d3caea towsey - Update ConcatenateIndexFiles.cs
- 1e0079e8 towsey - Attempt to fix bug in UseModel.cs
- 59672cc7 towsey - Read parameters from the config.yml file.
- 996d5648 towsey - Try versions of difference spectrogram
- b1ba0c8c towsey - Enable recording name to be passed to spectrograms.
- 664597ad towsey - Set up generation of Cepstral spectrogram.
- 0bf4f6c2 towsey - Create Towsey.SpectrogramGenerator.yml
- 3c544d93 towsey - Add TODO notes to two classes
- 5752c027 towsey - Replace config dictionary with typed dictionary
- de70d9b9 towsey - Add back second time scale to spectrograms.
- 85f32ce3 towsey - Trial removal of obsolete class
- 99c4bfce towsey - Get four spectrograms displaying correctly
- 8bbd8722 towsey - Further work to get more spectrograms
- 9e72c71f towsey - Get Waveform image working.
- d39e9820 towsey - Refactor the method to produce spectrograms
- 1c26d469 towsey - Fix unit tests
- 18c7bb6c towsey - Made correction of blue colour an optional parameter
- e69360fc towsey - Adjusted display of indices rendered in blue.
- 9e523266 towsey - Update LDSpectrogramRGB.cs
- 9ad5c900 towsey - Work done by Anthony to implement a new Results class for 6 indices.
- 376a5039 towsey - Update UseModel.cs
- 7a0364fa towsey - Update Sandpit.cs
- e095850a towsey - Add testing capcability to the Build/Make templates.
- d9db4ae0 towsey - Implement entry point for building content description models
- c15800b1 Michael Towsey - Create new SpectralIndexvalues class
- c342e0b8 Michael Towsey - Shift Content description analysis to main analysis loop
- 79f939b2 Michael Towsey - Update Towsey.TemplateDefinitions.json
- be0abf9b Anthony Truskinger - Code review for content description
- 89355ce8 Michael Towsey - add documentation
- c8f22515 Michael Towsey - Update TemplateManifest.cs
- e1465f57 Michael Towsey - Delete unnecesary files
- 129fb7f4 towsey - Fix up ContentSignatures.cs
- 51b29d88 towsey - derive more params from config file
- b8a84bcb towsey - Access params from the config.yml file
- 9c13b79e towsey - Finally get content description working in IAnalyzer2 environment.
- 516abf0b towsey - integrating Content Descirption into IAnalyzer2.
- a52c9b02 towsey - Content description now works using IAnalyzer2
- b3e568e9 towsey - Set up IAnalyzer2 system ready for Content description
- d765d942 towsey - More work on content description
- e7a4052b towsey - Set up class to calculate only six spectral indices
- 42b429ad towsey - Set up Content Description as IAnalyzer2.
- 17a56e35 towsey - Testing that manifest to tesmplates works
- 82f03295 towsey - resharper stuff
- a1fbbc9b towsey - Improve score normalisation
- d471feb8 towsey - Experiment with different score normalisations
- 7f1c5041 towsey - Complete refactoring of code to separate manifests from template definitions.
- d683f115 towsey - Set up Json reading and writing of template files
- 6bd20d29 towsey - Issue #252 Refactor code as per Anthony suggestions
- cb7f80a7 towsey - Work on template creation
- 418032d5 towsey - Issue #252 Start work on template editing
- 1b8e7764 towsey - Start work on Template Creation
- 1ac77f0f towsey - Issue #252 Finalise three content algoithms
- 0132e83a towsey - Issue #252  Set up new data structures for content description
- 6a319be1 towsey - Added visualisation methods to Plot.cs
- e5645d13 towsey - Created DataProcessing class plus two new test classes
- 7747fd56 towsey - Add another way of calculating distance
- 45aeb8be towsey - Set up two new content types, Morning Chorus and Silver Eye
- 2e4c4c1a towsey - Set up new classes to recognise acoustic types
- c2ec082a towsey - Set up wind and rain detection methods
- d2511f4c towsey - Building more of code structure for content descirption
- 4847c108 towsey - Continue setting up Content description framework.
- 99a27ff7 towsey - Set up methods for validating the output from content description classes
- d1d9f9f0 towsey - Write methods to read in acoustic index spectrograms.
- 857b9f72 towsey - Set up ConentDescription project
---

## Ecoacoustics Audio Analysis Software v19.12.0.1 (09/12/2019)
Version v19.12.0.1

[Compare v19.12.0.5...v19.12.0.1](https://github.com/QutBioacoustics/audio-analysis/compare/v19.12.0.5...v19.12.0.1)

- b93a667 towsey - Changes to standard spectrogram config file
---

## Ecoacoustics Audio Analysis Software v19.12.0.5 (02/12/2019)
Version v19.12.0.5

[Compare v19.11.1.1...v19.12.0.5](https://github.com/QutBioacoustics/audio-analysis/compare/v19.11.1.1...v19.12.0.5)

- b45652f towsey - Update TestAnalyzeLongRecording.cs
- 1907094 towsey - Fix five more broken tests
- 4dd38e0 towsey - Removed references to R3D identified by Anthony
- 96354c9 towsey - removed calculation of R3D
---

## Ecoacoustics Audio Analysis Software v19.11.1.1 (25/11/2019)
Version v19.11.1.1

[Compare v19.11.0.30...v19.11.1.1](https://github.com/QutBioacoustics/audio-analysis/compare/v19.11.0.30...v19.11.1.1)

- 781d0a3 towsey - Update Sandpit.cs
---

## Ecoacoustics Audio Analysis Software v19.11.0.30 (18/11/2019)
Version v19.11.0.30

[Compare v19.10.0.5...v19.11.0.30](https://github.com/QutBioacoustics/audio-analysis/compare/v19.10.0.5...v19.11.0.30)

- 33c5094 Michael Towsey - Issue-#238 Fix failed tests for flying fox
- cc8b834 Anthony Truskinger - Update CONTRIBUTING.md
- 856da7a Michael Towsey - Fixed unit test
- 0a5ec7d Michael Towsey - Update AcousticComplexityIndex.cs
- b31598b towsey - More work on Oscillation recognizer
- a0e643b towsey - Reworking the oscillation detection component of Flying Foxrecognizer
- e807743 towsey - Update PulseTrain.cs
- 1228ff3 towsey - Update SonogramTests.cs
- 1228b46 towsey - Finish Unit tests for Flying FOx class
- 70caaaf towsey - Update SonogramTests.cs
- 2f8947d towsey - More work on Pteropus recogniser tests
- 93cb93b towsey - More work on FlyingFox tests
- 67e023b towsey - Set up unit tests for the flying fox recogniser
- 03a304d towsey - More changes to FF code
- 1d1c552 towsey - Finished response to requests from Anthony
- 877703f towsey - Responding to comments from Anthony
- 10ad62a towsey - Fine adjustment of parameters
- 23429bd towsey - Update PteropusSpecies.cs
- 007d150 towsey - Attempt to implement pulase train detection
- 6e6cce8 towsey - Add wingbeat profile to Flying Fox
- 7a2c5e1 towsey - Penultimate work on FF recogniser
- 5e5016e towsey - Refactor acoustic events method
- a07ae44 towsey - cleaned up the PreopusSpecies class
- 3a5db0a towsey - New method to find events
- d8813c2 towsey - Further work on the flying fox recogniser.
- d7ac462 Michael Towsey - Begin work on flying fox recogniser
- 3f24758 towsey - Set up Flying Fox Recogniser classes
- 56c0725 towsey - Projects & Packages required to get recogniser working
---

## Ecoacoustics Audio Analysis Software v19.10.0.5 (21/10/2019)
Version v19.10.0.5

[Compare v19.10.0.3...v19.10.0.5](https://github.com/QutBioacoustics/audio-analysis/compare/v19.10.0.3...v19.10.0.5)

- e96fb1e dependabot-preview[bot] - Bump Microsoft.Extensions.FileProviders.Physical
- 733ef00 dependabot-preview[bot] - Bump System.Diagnostics.DiagnosticSource from 4.3.0 to 4.6.0
- 2d51909 dependabot-preview[bot] - Bump xunit.extensibility.core from 2.2.0 to 2.4.1
- 4164f40 dependabot-preview[bot] - Bump SixLabors.ImageSharp.Drawing from 1.0.0-beta0007 to 1.0.0-dev000893
- e6a208d dependabot-preview[bot] - Bump Zio from 0.3.6 to 0.7.4
---

## Ecoacoustics Audio Analysis Software v19.10.0.3 (07/10/2019)
Version v19.10.0.3

[Compare v19.9.2.18...v19.10.0.3](https://github.com/QutBioacoustics/audio-analysis/compare/v19.9.2.18...v19.10.0.3)

- bea4285 Anthony Truskinger - Allow fo detection of AudioMoth dates in result file names
- ce28732 Anthony Truskinger - Added FxCopAnalyzers package
- 3657e16 Anthony Truskinger - Added support for parsing AudioMoth V1 dates
---

## Ecoacoustics Audio Analysis Software v19.9.2.18 (30/09/2019)
Version v19.9.2.18

[Compare v19.9.1.3...v19.9.2.18](https://github.com/QutBioacoustics/audio-analysis/compare/v19.9.1.3...v19.9.2.18)

- ea59702 Anthony Truskinger - Fix syntax error from merge conflict
- ded36f9 towsey - Update ACI.bin
- 2c4599a towsey - Fix problem ACI value in top freq bin
- 53def22 towsey - Fix Unit tests for Concatenation
- 498c468 towsey - More experiments with rendering of zooming spectrograms
- 37cf217 towsey - Experiment with combined BGN index
- b8b39aa towsey - Check zooming spgm code is working OK
- 2c6578b Michael Towsey - Fix ColourMap weightings
- f9ed53d Michael Towsey - Work on ZoomTiledSpectrograms
- 3f9fe4f Michael Towsey - new method to load index matrices
- 4c55275 Michael Towsey - Shift directory SearchOption one level higher
- 1d78306 Michael Towsey - Edit code to adjust to config changes
- b6f64ca Michael Towsey - edit IndexProperties files
- 52cc26e Michael Towsey - Fix treatment of degenerate distributions
- d2e3377 Michael Towsey - Experiments with concatenate
- 9f3455c Michael Towsey - Deleted config files no longer relevant
---

## Ecoacoustics Audio Analysis Software v19.9.1.3 (23/09/2019)
Version v19.9.1.3

[Compare v19.9.0.3...v19.9.1.3](https://github.com/QutBioacoustics/audio-analysis/compare/v19.9.0.3...v19.9.1.3)

- 4462211 Anthony Truskinger - Fixed errant tilde
- e579aba Anthony Truskinger - Updated Plot.cs with changes from content-description branch
- b141a00 Anthony Truskinger - Updated image sharp and other dependencies
---

## Ecoacoustics Audio Analysis Software v19.9.0.3 (16/09/2019)
Version v19.9.0.3

[Compare v19.9.0.0...v19.9.0.3](https://github.com/QutBioacoustics/audio-analysis/compare/v19.9.0.0...v19.9.0.3)

- d4fe77f Anthony Truskinger - Cleaned up old string extension methods
- 92c371e Anthony Truskinger - Test and patch for short-name-app-config bug
---

## Ecoacoustics Audio Analysis Software v19.9.0.0 (02/09/2019)
Version v19.9.0.0

[Compare v19.8.2.5...v19.9.0.0](https://github.com/QutBioacoustics/audio-analysis/compare/v19.8.2.5...v19.9.0.0)

- 7542295 Anthony Truskinger - temporarily disable azure pipelines
- dd04350 Anthony Truskinger - minor docs change
- d2b94f0 towsey - Responding to comments from Anthony
- f6537bf towsey - Fine adjustment of parameters
- a1eb4e7 towsey - Update PteropusSpecies.cs
- f1a5a9f towsey - Attempt to implement pulase train detection
- 14a3214 towsey - Add wingbeat profile to Flying Fox
- 2cf3847 towsey - Penultimate work on FF recogniser
- f74098d towsey - Refactor acoustic events method
- aa85432 towsey - cleaned up the PreopusSpecies class
- c86caaa towsey - New method to find events
- d123d30 towsey - Further work on the flying fox recogniser.
- 414b392 Michael Towsey - Begin work on flying fox recogniser
- 6481e0e towsey - Set up Flying Fox Recogniser classes
- 4bb818c towsey - Projects & Packages required to get recogniser working
- bd00af0 Anthony Truskinger - Missing changes to previous fix
- efc4a44 Anthony Truskinger - Adds test for BARLT metadata parsing
---

## Ecoacoustics Audio Analysis Software v19.8.3.17 (29/08/2019)
Version v19.8.3.17

[Compare v19.8.2.5...v19.8.3.17](https://github.com/QutBioacoustics/audio-analysis/compare/v19.8.2.5...v19.8.3.17)

- 7542295 Anthony Truskinger - temporarily disable azure pipelines
- dd04350 Anthony Truskinger - minor docs change
- d2b94f0 towsey - Responding to comments from Anthony
- f6537bf towsey - Fine adjustment of parameters
- a1eb4e7 towsey - Update PteropusSpecies.cs
- f1a5a9f towsey - Attempt to implement pulase train detection
- 14a3214 towsey - Add wingbeat profile to Flying Fox
- 2cf3847 towsey - Penultimate work on FF recogniser
- f74098d towsey - Refactor acoustic events method
- aa85432 towsey - cleaned up the PreopusSpecies class
- c86caaa towsey - New method to find events
- d123d30 towsey - Further work on the flying fox recogniser.
- 414b392 Michael Towsey - Begin work on flying fox recogniser
- 6481e0e towsey - Set up Flying Fox Recogniser classes
- 4bb818c towsey - Projects & Packages required to get recogniser working
- bd00af0 Anthony Truskinger - Missing changes to previous fix
- efc4a44 Anthony Truskinger - Adds test for BARLT metadata parsing
---

## Ecoacoustics Audio Analysis Software v19.8.2.5 (26/08/2019)
Version v19.8.2.5

[Compare v19.8.1.1...v19.8.2.5](https://github.com/QutBioacoustics/audio-analysis/compare/v19.8.1.1...v19.8.2.5)

- 68d58fe towsey - Update LdSpectrogramRibbons.cs
- 70277d4 towsey - Clean up some build problems
- 734954b towsey - Set up methods for reading indices from Spectrogram ribbon images
- d0b228d towsey - Write methods to read ribbon images
- 9b40034 Anthony Truskinger - Update faq.md
---

## Ecoacoustics Audio Analysis Software v19.8.1.1 (12/08/2019)
Version v19.8.1.1

[Compare v19.8.0.1...v19.8.1.1](https://github.com/QutBioacoustics/audio-analysis/compare/v19.8.0.1...v19.8.1.1)

- abb00dd Anthony Truskinger - Added notes on chunk size choices
---

## Ecoacoustics Audio Analysis Software v19.8.0.1 (05/08/2019)
Version v19.8.0.1

[Compare v19.7.2.3...v19.8.0.1](https://github.com/QutBioacoustics/audio-analysis/compare/v19.7.2.3...v19.8.0.1)

- 0ba6fb0 Anthony Truskinger - Update installing.md
---

## Ecoacoustics Audio Analysis Software v19.7.2.3 (29/07/2019)
Version v19.7.2.3

[Compare v19.7.1.3...v19.7.2.3](https://github.com/QutBioacoustics/audio-analysis/compare/v19.7.1.3...v19.7.2.3)

- f47ea18 Anthony Truskinger - Fixes spelling mistakes in bug template ðŸ™„
- 6aa4089 Anthony Truskinger - Update bug_report.md
- 79a2f00 Anthony Truskinger - Set up CI with Azure Pipelines
---

## Ecoacoustics Audio Analysis Software v19.7.1.3 (22/07/2019)
Version v19.7.1.3

[Compare v19.7.0.1...v19.7.1.3](https://github.com/QutBioacoustics/audio-analysis/compare/v19.7.0.1...v19.7.1.3)

- 5f1d30f Anthony Truskinger - Updated bug report issue template
- 69ee220 Anthony Truskinger - Create a question issue template
- 089e148 Anthony Truskinger - Fixes System.Numerics.Vectors not loading on some systems and produces better errors for reflection loading exceptions (#244)
---

## Ecoacoustics Audio Analysis Software v19.7.0.1 (08/07/2019)
Version v19.7.0.1

[Compare v19.6.1.1...v19.7.0.1](https://github.com/QutBioacoustics/audio-analysis/compare/v19.6.1.1...v19.7.0.1)

- b1045dd Anthony Truskinger - Adds documentation to SaveIntermediateCsvFiles config option
---

## Ecoacoustics Audio Analysis Software v19.6.1.1 (01/07/2019)
Version v19.6.1.1

[Compare v19.6.0.1...v19.6.1.1](https://github.com/QutBioacoustics/audio-analysis/compare/v19.6.0.1...v19.6.1.1)

- 30eb5bd Anthony Truskinger - Fixes culture formatting bug for SoX commands (#243)
---

## Ecoacoustics Audio Analysis Software v19.6.0.1 (24/06/2019)
Version v19.6.0.1

[Compare v19.5.1.1...v19.6.0.1](https://github.com/QutBioacoustics/audio-analysis/compare/v19.5.1.1...v19.6.0.1)

- 484f922 Anthony Truskinger - Add notes on procuring Visual Studio
---

## Ecoacoustics Audio Analysis Software v19.5.1.1 (13/05/2019)
Version v19.5.1.1

[Compare v19.5.0.1...v19.5.1.1](https://github.com/QutBioacoustics/audio-analysis/compare/v19.5.0.1...v19.5.1.1)

- 6973fd5 Anthony Truskinger - More ap_download script updates
---

## Ecoacoustics Audio Analysis Software v19.5.0.1 (06/05/2019)
Version v19.5.0.1

[Compare v19.4.1.4...v19.5.0.1](https://github.com/QutBioacoustics/audio-analysis/compare/v19.4.1.4...v19.5.0.1)

- 7dc4f0e Anthony Truskinger - Updated download_ap.ps1 to accept github api token
---

## Ecoacoustics Audio Analysis Software v19.4.1.4 (29/04/2019)
Version v19.4.1.4

[Compare v19.4.0.1...v19.4.1.4](https://github.com/QutBioacoustics/audio-analysis/compare/v19.4.0.1...v19.4.1.4)

- ff0e123 Anthony Truskinger - Adds unit tests for ribbon plots
- 8f9e727 Anthony Truskinger - [WIP] Finished ribbon plots
- 2005bc8 Anthony Truskinger - [WIP] Almost got ribbons working
- 51f27bd Anthony Truskinger - [WIP] Initial work for drawing ribbon plots
---

## Ecoacoustics Audio Analysis Software v19.4.0.1 (08/04/2019)
Version v19.4.0.1

[Compare v19.3.3.39...v19.4.0.1](https://github.com/QutBioacoustics/audio-analysis/compare/v19.3.3.39...v19.4.0.1)

- 3c64b3c Anthony Truskinger - Updated fs compiler tools
---

## Ecoacoustics Audio Analysis Software v19.3.3.39 (01/04/2019)
Version v19.3.3.39

[Compare v19.3.2.1...v19.3.3.39](https://github.com/QutBioacoustics/audio-analysis/compare/v19.3.2.1...v19.3.3.39)

- 3bc6a90 Michael Towsey - Mostly resharper stuff
- 934f466 Michael Towsey - Change config file
- 5630edd Michael Towsey - Add new property to AcousticIndicesConfig class
- 03f7190 Anthony Truskinger - Upped timeout for log clearing test
- f25b053 Michael Towsey - Adjust tests affected by Issue #217
- 8c4a0fb Michael Towsey - Small changes to Sandpit
- c7c2a17 Michael Towsey - Update SummaryIndexValues.cs
- 910ff24 Michael Towsey - Final commit for issue #217
- 21d8925 Michael Towsey - remove option to include Sunrise data
- b9fe5db Michael Towsey - Remove translation dictionary
- 369d3a3 Michael Towsey - Removed unnecessary Indices & properties
- 3d949e0 Michael Towsey - Split the IndexProperties file in two
- e35260d Anthony Truskinger - Fixed minor concat bugs
- cedad4c Michael Towsey - Update ConcatenationTests.cs
- 252c58f Michael Towsey - Changed way mode of distribution calculated
- 57de07e Michael Towsey - Deal with case where histogram mode = 0.0
- d0228f1 Michael Towsey - Refactor code which draws histograms
- d690c35 Michael Towsey - Rework rendering of Concatenated Indices
- 047e36c Michael Towsey - Four minor changes for checking purposes
- 9691546 Michael Towsey - Revert "Update Sandpit.cs"
- c5fa3cb Michael Towsey - Update Sandpit.cs
- 668e48b Michael Towsey - Update GapsAndJoins.cs
---

## Ecoacoustics Audio Analysis Software v19.3.2.1 (25/03/2019)
Version v19.3.2.1

[Compare v19.3.1.9...v19.3.2.1](https://github.com/QutBioacoustics/audio-analysis/compare/v19.3.1.9...v19.3.2.1)

- 6cbf3da Anthony Truskinger - Fixes spelling mistakes in CLI help
---

## Ecoacoustics Audio Analysis Software v19.3.1.9 (18/03/2019)
Version v19.3.1.9

[Compare v19.3.0.5...v19.3.1.9](https://github.com/QutBioacoustics/audio-analysis/compare/v19.3.0.5...v19.3.1.9)

- 64e0f98 Anthony Truskinger - Adds checkes for critical DLLs
- e3317bd Anthony Truskinger - Use a nuget version of System.ComponentModel.Annotations
- 3d0d284 Anthony Truskinger - More fixes to download script
- 5463137 Anthony Truskinger - Fully disables CLIpager
- b89a5d5 Anthony Truskinger - Tried to improve check environment
- ee00f00 Anthony Truskinger - Fixes log naming bug
- 478f910 Anthony Truskinger - Disables CLI pager
- b6a34a5 Anthony Truskinger - Improves downloader script
---

## Ecoacoustics Audio Analysis Software v19.2.2.1 (25/02/2019)
Version v19.2.2.1

[Compare v19.2.1.10...v19.2.2.1](https://github.com/QutBioacoustics/audio-analysis/compare/v19.2.1.10...v19.2.2.1)

- e336ab8 Anthony Truskinger - Fixes a bug in CLI parsing
---

## Ecoacoustics Audio Analysis Software v19.2.1.10 (18/02/2019)
Version v19.2.1.10

[Compare v19.2.0.90...v19.2.1.10](https://github.com/QutBioacoustics/audio-analysis/compare/v19.2.0.90...v19.2.1.10)

- f998a4f Anthony Truskinger - Fixes extra bad new line in header
- 799a653 Anthony Truskinger - Splits up copyright text
- 9e9e56d Anthony Truskinger - More logging patches
- c96ea6e Anthony Truskinger - Fixes CLI duplicate value parser bug
- 3e1a67a Anthony Truskinger - Refactored static Logging class
- ce80cbc Michael Towsey - Fixes and closes Issue #170 concat joins
- 981696d Michael Towsey - Concat crashing Issue #170
- 2917519 Anthony Truskinger - Fixes CLI pager bug
---

## Ecoacoustics Audio Analysis Software v19.2.0.90 (11/02/2019)
Version v19.2.0.90

[Compare v19.2.0.3...v19.2.0.90](https://github.com/QutBioacoustics/audio-analysis/compare/v19.2.0.3...v19.2.0.90)

- e47eeff Anthony Truskinger - Fix unit tests from previous merge
- 1372bd6 Anthony Truskinger - Updates mstest and adapters
- 8259fee Anthony Truskinger - Improves perf for zoom tests
- 2ad4da1 Mahnoosh Kholghi - removed unnecessary references
- 557e5de Mahnoosh Kholghi - removing some unnecessary references
- d7e5a7c Mahnoosh Kholghi - added comments to the spectral peak tracking method
- 29af151 Mahnoosh Kholghi - fixed bugs in new peak tracking method
- 2af60bf Mahnoosh Kholghi - added a new method for peak tracking
- 01054f2 Mahnoosh Kholghi - fixed a bug in the config file
- 789e618 Mahnoosh Kholghi - added a method to draw spectral tracks
- 470283d Mahnoosh Kholghi - extended the drawing method to highlight bands bounadries
- e3577e1 Mahnoosh Kholghi - added methods to draw peak hits on spectrogram
- 36e234e Mahnoosh Kholghi - added energy spectrogram class
- 0c84f1b Mahnoosh Kholghi - cleaning the code
- b5f8125 Mahnoosh Kholghi - added FindLocalSpectralPeaks method and corresponding test
- 85c8f74 Mahnoosh Kholghi - added GetPeakBinsIndex method and corresponding test
- e7a59d9 Mahnoosh Kholghi - added spectral peak tracking for night parrot
- 6f194f2 Anthony Truskinger - Fixes dotsettings
- 71d6f14 mkholghi - cleaning feature learning and extraction process
- 30272e0 mkholghi - removing unnecessary comments and lines
- ec2e73e Anthony Truskinger - Unit test fixes for rendering all gray FCS
- 8043425 Michael Towsey - Reworked code to draw grey scale spectrograms
- 6ec16c6 Michael Towsey - Adds rendering of ALL grayscale LD spectrograms to standard indices generation
- 44ac920 Mahnoosh Kholghi - amended patch sampling approach in semi-supervised clustering process
- 8f9dac6 Mahnoosh Kholghi - amended ListOf2DArrayToOne2DArray method, so that it can merge matrices with different number of rows
- 9981af3 Mahnoosh Kholghi - debugged the semi-supervised feature learning method
- 73b2b5f Mahnoosh Kholghi - added semi-supervised fearture learning
- 0d64531 Mahnoosh Kholghi - fixed bug in frame window length and fixed window overlap
- b52daae Mahnoosh Kholghi - fixed bug in feature extraction
- 895396f Mahnoosh Kholghi - Fixed bug in audio segmentation
- 2eb488b Mahnoosh Kholghi - added audio segmentation and generate features for any desired resolution
- e9be1d4 Mahnoosh Kholghi - added downsampling step to feature extraction and feature learning process
- 7c915e6 Mahnoosh Kholghi - Fixed test methods due to build failed: commit 780a171 by @towsey
- e452e3a Mahnoosh Kholghi - Editted MahnooshSandpit
- e5eec5c Mahnoosh Kholghi - saved similarity vectors to a csv file
- ed2a488 Mahnoosh Kholghi - added random sampling without replacement
- c951ea4 Mahnoosh Kholghi - added ExtractClusteringFeatures and GenerateSpectrograms to MahnooshSandpit
- cf0c3b9 Mahnoosh Kholghi - added frame window length and step size
- ddeba37 Mahnoosh Kholghi - updated project files
- a2154b5 Mahnoosh Kholghi - added two parameters for making a window of group of frames
- 203e07e Mahnoosh Kholghi - more recording samples tested for different spectrogram classes
- bfe2369 Mahnoosh Kholghi - revised drawing method
- 7c27469 Mahnoosh Kholghi - revised noise reduction method
- 554dbd2 Mahnoosh Kholghi - added a method to calculate percentile noise profile
- 4527865 Mahnoosh Kholghi - adding a class for feature learning settings and configurations
- d27c09c Mahnoosh Kholghi - revised sandpit based on new classes
- ad0e209 Mahnoosh Kholghi - adding two parameters to config file
- f05446d Mahnoosh Kholghi - Adding two class for feature learning and feature extrcation processes
- 21ba023 Mahnoosh Kholghi - changed settings
- 9f1831e Michael Towsey - More work on Spectrogram clsases
- 98a948c Mahnoosh Kholghi - Fixed bug
- 719bf88 Michael Towsey - work on new standard spectrogram classes
- f7168dc Mahnoosh Kholghi - Added test for different spectrograms
- a79bbdc Mahnoosh Kholghi - cleaning the code and notes
- 9bf06be Mahnoosh Kholghi - Added a constructor to EnergySpectrogram
- 918b8b3 Mahnoosh Kholghi - Added get image methods
- 9cf361c Mahnoosh Kholghi - Added Amplitude and Decibel Spectrograms
- da2ea55 Mahnoosh Kholghi - changes to config file
- 780a171 Michael Towsey - Changes to EnergySpectogram class to remove dependence on Base class.
- 5bb02f2 Mahnoosh Kholghi - added ignore attribute to PSD test
- df76bf7 Mahnoosh Kholghi - specified path to feature learning config file
- 9ed6cae Mahnoosh Kholghi - modified PSD test
- 69a83c1 Mahnoosh Kholghi - modified config file
- 078ce16 Mahnoosh Kholghi - Added energy spectrogram
- c0e47a5 Mahnoosh Kholghi - added a test for PSD class
- 6e566bc Mahnoosh Kholghi - Added class to calculate power spectrum
- a2a8af3 Mahnoosh Kholghi - added class for PSD
- c70b164 Mahnoosh Kholghi - Read parameters from config file
- 433ff34 Mahnoosh Kholghi - added config file for feature learning
- 4757dc9 Mahnoosh Kholghi - fixed bug in feature pooling step
- db4d8d7 Mahnoosh Kholghi - added min pooling
- d6ad371 Mahnoosh Kholghi - fixed a bug in writing the features to file
- a58fa6e Mahnoosh Kholghi - added a method to get the min value of a vactor
- 1c05ae0 Mahnoosh Kholghi - Added a condition for vector normalization
- 991bdd1 Mahnoosh Kholghi - updated config for Acoustic Test packages
- f2b7649 Mahnoosh Kholghi - Updated MSTest
- 28f4be7 Mahnoosh Kholghi - Added skewness measure
- ba7d158 Mahnoosh Kholghi - updated McMaster
- 6cbe20e Mahnoosh Kholghi - update MSTest
- 8b19583 Mahnoosh Kholghi - writing all feature vectors to one file
- 15888da Mahnoosh Kholghi - cleaning the code
- 512f8b4 Mahnoosh Kholghi - generating features for a set of recordings
- 9b7a178 Mahnoosh Kholghi - fixing the error in directory
- 3fdb19c Mahnoosh Kholghi - Adding a method for arbitrary freq bins
