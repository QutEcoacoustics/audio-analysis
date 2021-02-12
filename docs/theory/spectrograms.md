---
title: Spectrograms
uid: theory-spectrograms
---

# Spectrograms

A spectrogram is processed as a matrix of real values but visualized as a grey-scale image. Each row of pixels is a frequency bin and each column of pixels is a time-frame. The value in each spectrogram/matrix cell (represented visually by one image pixel) is the acoustic intensity in decibels with respect to the background noise baseline. Note that the decibel values in a noise-reduced spectrogram are always positive.

Throughout _AP_ you'll see references to spectrogram parameters, such as in the
[parameters for generic recognizer algorithms](xref:AnalysisPrograms.Recognizers.Base.CommonParameters).

`FrameSize` and `FrameStep` determine the time/frequency
resolution of the spectrogram. Typical values are 512 and 0 samples respectively. There is a trade-off between time
resolution and frequency resolution; finding the best compromise is really a matter of trial and error.
If your target syllable is of long duration with little temporal variation (e.g. a whistle) then `FrameSize` can be
increased to `1024` or even `2048`.

> [!NOTE]
> The value of `FrameSize` must be a power of 2.

To capture more temporal
variation in your target syllables, decrease `FrameSize` and/or decrease `FrameStep`. A typical `FrameStep` might be
half the `FrameSize` but does *not* need to be a power of 2.

The default value for *WindowFunction* is `HANNING`. There should never be a need to change this but you might like to
try a `HAMMING` window if you are not satisfied with the appearance of your spectrograms.

## Noise reduction

The "Bg" in `BgNoiseThreshold` means *background*. This parameter determines the degree of severity of noise removal
from the spectrogram. The units are decibels. Zero sets the least severe noise removal. It is the safest default value
and probably does not need to be changed. Increasing the value to say 3-4 decibels increases the likelihood that you
will lose some important components of your target calls. For more on the noise removal algorithm used by _AP_ see
[Towsey, Michael W. (2013) Noise removal from wave-forms and spectrograms derived from natural recordings of the environment.](https://eprints.qut.edu.au/61399/).