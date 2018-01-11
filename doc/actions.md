# Actions

## Types

There are, in broad terms, these types of sub-programs:

-   Main actions
    -   Process large amounts of information (like `audio2csv`)
-   Development / small scale actions
    -   Small data / development entry points
    -   `eventrecognizer` which is a generic program for testing different recognizers
-   Utility actions
    -   `DummyAnalyser` - uses CPU for a while and does nothing useful
    -   `audiocutter` - cuts and converts long audio files up
-   Meta actions
    -   help and documentation usage (`help` & `list`)
    -   `analysesavailable`


## Actions


### Analyze Long Recordings

Processes large audio recording with the specified analysis. Can run recognizers, calculate indices, or do other things, for very long recordings.

See [details on Analyze Long Recordings](./actions/analyze_long_recordings.md)

### Colour Spectrogram

This action produces a single false-colour (FC) spectrogram, taking as input the spectral indices produced by the _Acoustic Indices_ analysis on a single audio file.

See [details on Colour Spectrogram](./actions/colour_spectrogram.md)

### Concatenate Index Files

This action joins together the results of several _Acoustic Indices_ analysis
result sets to produce data and images for 24-hour blocks of data.

See [details on Concatenate Index Files](./actions/concatenate_index_files.md)

