# Colour Spectrogram

Action      | colourSpectrogram
Config file | SpectrogramFalseColourConfig.yml
Config file2 | IndexPropertiesConfig.yml

This action produces a single false-colour (FC) spectrogram, taking as input the spectral indices produced by the _Acoustic Indices_ analysis on a single audio file.

One FC spectrogram is produced per audio file. To learn more about long-duration, false-colour spectrograms, check out our tutorial at <https://research.ecosounds.org/research/eadm-towsey/long-duration-audio-recordings-of-the-environment>.

## Usage

This section describes the command line arguments required to draw a false-colour spectrogram derived from matrices of spectral acoustic indices extracted from a single long-duration audio recording.

To run the analysis, type:

```
$ AnalysisPrograms.exe colourSpectrogram <options...>;
```

The output of this analysis is a set of false-colour and greyscale long-duration index spectrograms.

Here is an example of a command line with abbreviated path names:

```
$ AnalysisPrograms.exe colourSpectrogram –inputdatadirectory "path\directoryName1" –outputdirectory "path\directoryName2" - spectrogramconfigpath "path\fileName3.yml" -indexpropertiesconfig "path\fileName4.yml"
```

The above four paths are obligatory arguments (the program will return a fatal error if they are not found on the command line). 

## Options

-   `-inputdatadirectory`: Directory where the input matrices of spectral acoustic indices are located.
-   `-outputdirectory`: Directory where the output spectrogram images are to go.
-   `-spectrogramconfigpath`: Path to the config file specifying other parameters.
-   `-indexpropertiesconfig`: Path to the config file containing a list of indices and their display properties, including information about the brightness and contrast of the RGB channels in the FC spectrograms. These values are extremely important in determining the amount of detail that can be seen in FC spectrograms.

## Config file parameters (SpectrogramFalseColourConfig.yml)

The colourSpectrogram config file gives you access to various parameters that control drawing of long-duration false-colour spectrograms. Here are the available parameters with default values. 


Here is some additional information about the more important parameters:

-   `ColorMap1`: "ACI-ENT-EVN". You can experiment with various combinations of indices but we find these to work well. See the tutorial at <https://eprints.qut.edu.au/110634> for more information about the indices to which abbreviations refer.
-   `ColorMap2`: "BGN-PMN-R3D"
-   `ColourFilter`: This parameter determines the extent to which low index values are emphasized or de-emphasized in their colour representation. Its purpose is either to give emphasis to low intensity features or to de-emphasise them. This parameter applies a function that lies between y=x^-2 and y=x^2, i.e. between the square-root and the square.
    -   When filterCoeff = 1.0, small values are maximally emphasized, i.e. y=sqrt(x).
    -   When filterCoeff = 0.0, the matrix remains unchanged, that is, y=x.
    -   When filterCoeff =-1.0, small values are maximally de-emphasized, i.e. y=x^2.
    - Generally, usage suggests that a value of -0.25 is suitable. i.e. a slight de-emphasis.
-   `FreqScale`: "Linear". This sets the type of y-axis or Hertz scale. 
    - Eventual options will be:
        -   Linear
        -   Mel
        -   Linear62Octaves31Nyquist11025
        -   Linear125Octaves30Nyquist11025
        -   Octaves24Nyquist32000
        -   Linear125Octaves28Nyquist32000
    - [TODO: Update] Only "Linear", "Linear125Octaves7Tones28Nyquist32000" work at present.
-   `YAxisTicInterval`: 1000. Horizontal grid lines will be placed every 1kHz interval.