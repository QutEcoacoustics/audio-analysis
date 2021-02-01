# Commands

## Types

The following four categories of sub-programs (commands) are available:

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

See details here <xref:command-analyze-long-recording>

### Colour Spectrogram

This command produces a single false-colour (FC) spectrogram, taking as input
the spectral indices produced by the _Acoustic Indices_ analysis on a single
audio file.

See details here <xref:command-draw-long-duration-spectrogram>

### Concatenate Index Files

This command joins together the results of several _Acoustic Indices_ analysis
result sets to produce data and images for 24-hour blocks of data.

See details here <xref:command-concatenate-index-files>
