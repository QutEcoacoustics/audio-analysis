---
title: "Why do we analyze data in 1-minute chunks?"
uid: article-minute-chunks
tags: ["AnalysisPrograms", "Analysis"]
---

We received an email earlier this week about why we choose one-minute sized
audio chunks as our standard for analysis:

>Most research uses one-minute segments for the analysis of acoustic indices. We believe that the reason is solely for simplicity and to standardize the analysis with respect to other investigations, but we are not sure if there is another reason to use one-minute segments. That is,  it would not cause any problem to use segments of 3, 5 or 10 minutes of recording to calculate the indexes? Or is there something related to the calculation of the indices that it needs to be adjusted in segments of one minute?

It's a great question and we thought it deserved a public response (since 
[email silos information](https://blog.codinghorror.com/is-email-efail/)!).

## Why we analyze data in one-minute chunks

There are a couple reasons, but mainly, because when we first started doing this,
computers were far less efficient than they are now. Computers are fundamentally
limited by the amount of data they can work with at one time; in technical terms,
the amount of data that can fit into main memory (RAM). By breaking the data into 
smaller chunks, we could _stream_ the data through the analysis and our overall
analysis speed was greatly improved.

We could have chosen any size from 30-seconds to 5-minutes, but
one-minute blocks also had nice temporal features (they compose well with data
at different resolutions) and are still detailed enough to answer questions in
our multi-day analyses.

Today it seems to be the de facto standard to analyze data in one-minute blocks.
We suggest that it is still a good default for most use cases:

- Computers don't have the same limitations as they did when we started, but small
  blocks of data allow for parallel analysis that effectively utilizes all CPU cores
- While computer's are getting better, we're also doing more complex analyses. In
  parallel we can use a large amount of RAM and most of the computer's CPU(s) for
  the quickest analysis
- One-minute blocks still retain the nice temporally composable attributes detailed
  above.
- And since one-minute blocks seem to be a defacto standard it does (by happenstance)
  provide common ground for comparing data

## What about the effect on data?

For acoustic event recognition, typically only oundary effects are 
affected by chunk-size choice.
That is, if an acoustic event occurs and is clipped by
by either the start or end of the one-minute chunk, then it is now only a "partial vocalisation".
A typical event recognizer may not detect such "partial vocalisations".

For acoustic indices, from a theoretical point of view, chunk-size has the same kinds of issues
as the choice of FFT frame length in speech processing. Because an FFT
assumes signal stationarity, one chooses a frame length over which the spectral
content in the signal of interest is approximately constant. In the case of
acoustic indices, one chooses an index calculation duration which captures
a sufficient amount of data for the acoustic feature that you are interested in.
The longer the analysis duration the more you blur or average out features of
interest. However, if you choose too short an interval in then the calculated
index may be dominated by "noise"---that is, features that are not of interest.
We find this is particularly the case with the ACI index; one-minute seems to be
an appropriate duration for the indices that are typically calculated.

## ...but my situation is different, can I change the chunk size?

Good news, for our tools 
([AnalysisPrograms.exe](https://github.com/QutEcoacoustics/audio-analysis)), you
can change the defaults!

The [config files](https://github.com/QutEcoacoustics/audio-analysis/tree/master/src/AnalysisConfigFiles) for most of our analyses contain these common settings:

### Chunk-size:

```yaml
# SegmentDuration: units=seconds;
# Long duration recordings are cut into short segments for more 
# efficient processing. Default segment length = 60 seconds.
SegmentDuration: 60
```
Here, `SegmentDuration` is our name for the chunk-size. If, for example, you wanted
to process data in 5 minute chunks, you could change the configuration to
`SegmentDuration: 300`.

### Chunk-overlap:

```yaml
# SegmentOverlap: units=seconds;
SegmentOverlap: 0
```

If you're doing event recognition and you're concerned about boundary effects,
you could change the overlap to `SegmentOverlap: 10`, which would ensure every
`SegmentDuration`-sized-chunk (typically one-minute in size) would be cut with
an extra, trailing, 10-second buffer.

Note: we rarely change this setting and setting too much overlap may produce
duplicate events.

### Index Calculation Duration (for the indices analysis only):

For acoustic indices in particular, their calculation resolution depends on a
second setting that is limited by chunk-size (`SegmentDuration`):

```yaml
# IndexCalculationDuration: units=seconds (default=60 seconds)
# The Timespan (in seconds) over which summary and spectral indices are calculated
# This value MUST not exceed value of SegmentDuration.
# Default value = 60 seconds, however can be reduced down to 0.1 seconds for higher resolution.
# IndexCalculationDuration should divide SegmentDuration with MODULO zero
IndexCalculationDuration: 60.0
```

If you wanted indices calculated over a duration longer than one-minute, you
could change the `SegmentDuration` and the `IndexCalculation` duration to higher
values: 

```yaml
SegmentDuration: 300
IndexCalculationDuration: 300
```

However, we suggest that there are better methods for calculating low-resolution
indices. A method we often use is to calculate indices at a 60.0 seconds resolution
and aggregate the values into lower resolution chunks. The aggregation method
can provide some interesting choices:

- We've seen the maximum, median, or minimum value for a block of indices
    chosen (and sometimes all 3).
    - though be cautious when using a mean function, it can skew the value of
        logarithmic indices
- And we've seen a block of indices flattened into a larger feature vector and
    fed to a machine learning or clustering algorithm

## Conclusion

We hope we've provided clarity on our reasoning for using one-minute sized chunks
for audio analysis. We'd love to here any follow up feedback via
[twitter](https://twitter.com/QUTEcoacoustics), or through our
[audio-analysis](https://github.com/QutEcoacoustics/audio-analysis) GitHub
repository.
