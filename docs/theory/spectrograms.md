---
title: Spectrograms
uid: theory-spectrograms
---

# Spectrograms

A spectrogram is processed as a matrix of real values but visualized as a grey-scale image. Each row of pixels is a frequency bin and each column of pixels is a time-frame. The value in each spectrogram/matrix cell (represented visually by one image pixel) is the acoustic intensity in decibels with respect to the background noise baseline. Note that the decibel values in a noise-reduced spectrogram are always positive.

Throughout _AP_ you'll see references to four spectrogram parameters, such as in the
[parameters for generic recognizer algorithms](xref:AnalysisPrograms.Recognizers.Base.CommonParameters).
They are `FrameSize`, `FrameStep`, `WindowFunction` and `BgNoiseThreshold`.

## FrameSize
Sets the size of the FFT window used to make the spectrogram. A good default value for detecting typical animal calls is `512`. If your target syllable is of long duration with little temporal variation (e.g. a one-second long bird whistle) then `FrameSize` can be increased to `1024` or even `2048`.

> [!NOTE]
> The value of `FrameSize` must be a power of 2.

> [!NOTE]
> `FrameSize` determines the time and frequency resolutions along the x-axis and y-axis (respectively) of the spectrogram. There is a trade-off between these; that is, increasing the resolution of one will decrease the resolution of the other. Finding the best compromise is really a matter of trial and error.  

## FrameStep
Sets the number of samples between the start of one frame and the next. Therefore it controls frame overlap.  `FrameStep` must be less than `FrameSize` but need not be a power of 2. By default `FrameStep` equals `FrameSize` but it is frequently set to half the frame size.

> [!NOTE]
> To capture more temporal
variation in your target syllables, decrease `FrameSize` and/or decrease `FrameStep`.




## WindowFunction
Sets the FFT window function. It can be one of the values from <xref:TowseyLibrary.WindowFunctions>. `Hanning` is the default because we find it the most versatile.
There should never be a need to change this but you might like to try a `HAMMING` window if you are not satisfied with the appearance of your spectrograms.

## BgNoiseThreshold
 Sets the degree of severity of noise removal from the spectrogram.
 The "Bg" in `BgNoiseThreshold` means *background*.
 The units are decibels.
 Zero sets the least severe noise removal. This is the safest default value and probably does not need to be changed.
 Increasing the value to say 3-4 decibels increases the likelihood that you will lose some important components of your target calls. For more on the noise removal algorithm used by _AP_ see
[Towsey, Michael W. (2013) Noise removal from wave-forms and spectrograms derived from natural recordings of the environment.](https://eprints.qut.edu.au/61399/).
