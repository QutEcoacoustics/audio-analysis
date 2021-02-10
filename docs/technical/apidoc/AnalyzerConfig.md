---
uid: AnalysisBase.AnalyzerConfig
---

Any analysis that works with the <xref:command-analyze-long-recording> command will use this class as a base.

## SegmentDuration & SegmentOverlap

The default values are 60 and 0 seconds respectively and these seldom need to be changed.
You may wish to work at finer resolution by reducing _SegmentDuration_ to 20 or 30 seconds.

If your target call is comparatively long (such as a koala bellow, e.g. greater than 10 - 15 seconds), you could
increase _SegmentOverlap_ to 10 seconds. This actually increases the segment duration to 70 seconds (60+10)
so reducing the probability that a call will be split across segments. It also maintains a 60-second interval
between segment-starts, which helps to identify where you are in a recording.

## Audio resampling

Specifies the sample rate at which the recording will be processed.

```yaml
ResampleRate: 22050
```

If this parameter is not specified in the config file, the default is to _resample_ each recording segment (up or down)
to 22050 samples per second. This has the effect of limiting the maximum frequency (the Nyquist) to 11025 Hertz.
*ResampleRate* must be twice the desired Nyquist. Specify the resample rate that gives the best result for your target
call. If the target call is in a low frequency band (e.g. < 2kHz), then lower the resample rate to somewhat more than
twice the maximum frequency of interest. This will reduce processing time and produce better focused spectrograms.
If you down-sample, you will lose high frequency content. If you up-sample, there will be undefined "noise" in
spectrograms above the original Nyquist.