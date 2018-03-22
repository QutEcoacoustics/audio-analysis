# Commands

## Types

There are, in broad terms, these types of sub-programs (commands):

-   Main commands
    -   Process large amounts of information (like `audio2csv`)
-   Development / small scale commands
    -   Small data / development entry points
    -   `eventrecognizer` which is a generic program for testing different recognizers
-   Utility commands
    -   `DummyAnalyser` - uses CPU for a while and does nothing useful
    -   `audiocutter` - cuts and converts long audio files up
-   Meta commands
    -   help and documentation usage (`help` & `list`)
    -   `analysesavailable`


## Important Commands

### Analyze Long Recordings

Processes large audio recording with the specified analysis. Can run recognizers,
calculate indices, or do other things, for very long recordings.

See [details on Analyze Long Recordings](./commands/analyze_long_recording.md)

### Colour Spectrogram

This command produces a single false-colour (FC) spectrogram, taking as input
the spectral indices produced by the _Acoustic Indices_ analysis on a single
audio file.

See [details on Colour Spectrogram](./commands/colour_spectrogram.md)

### Concatenate Index Files

This command joins together the results of several _Acoustic Indices_ analysis
result sets to produce data and images for 24-hour blocks of data.

See [details on Concatenate Index Files](./commands/concatenate_index_files.md)

