# Acoustic Indices

Action      | audio2csv
Config file | Towsey.Acoustic.yaml

Generates acoustic indices (AKA summary indices) and spectral indices for given audio recording. To learn more about the spectral and summary indices calculated by this action, download “The calculation of acoustic indices derived from long-duration recordings of the natural environment”, Michael Towsey, 2017. at <https://eprints.qut.edu.au/110634>.


```yml
IndexCalculationDuration: 60.0
# BGNoiseNeighbourhood: 5
ResampleRate: 22050
FrameLength: 512
LowFreqBound: 1000
MidFreqBound: 8000
HighFreqBound: 11000
FrequencyScale: Linear
# SaveIntermediateWavFiles: Never
# SaveIntermediateCsvFiles: false
SaveSonogramImages: Never
# SaveSonogramData: false
ParallelProcessing: false
# TileImageOutput: false
# RequireDateInFilename: false
# IndexPropertiesConfig: './IndexPropertiesConfig.yml'
# EventThreshold: 0.2
```

Here is some additional information about the more important parameters:

-   IndexCalculationDuration: Typically, a long duration audio recordings will be divided into 60 second segments and indices calculated for each segment. However, this timespan can be shortened. A minimum value is 0.1 seconds. Note however, that you would not do this for very long-duration recordings.

-   ResampleRate: You should only increase this, from the default of 22050, if you have high sample rate recording and you are interested in acoustic content above 11kHz. Conversely, you would only decrease this value if you have very long recordings (thereby decreasing computation time) and are only interested in low-frequency content.

-   FrameLength: Typically, we find 512 or 1024 to be good values for producing FC spectrograms. However, where the sample rate &gt; 44Khz, frame size can be increased to 2048.

-   LowFreqBound, MidFreqBound, HighFreqBound: Birds calls are typically found in the 1- 8 kHz frequency band. It can be useful to obtain indices for this band and to compare the acoustic energy in the three bands. If your bands of interest are different, you can reset the bounds here.

-   FrequencyScale: Currently the only options are Linear or Octave.

-   SaveSonogramImages: Best to keep the default value of *Never*. However, it may be useful to save spectrograms for debugging purposes. The other (case-sensitive) options are: `[True/Always | WhenEventsDetected]`