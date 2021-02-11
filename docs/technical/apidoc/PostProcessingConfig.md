---
uid: AudioAnalysisTools.Events.Types.EventPostProcessing.PostProcessingConfig
---

An example configuration:

[!code-yaml[post_processing](../../guides/Ecosounds.NinoxBoobook.yml#L34-L69 "Post Processing")]

## Combine events

The `CombineOverlappingEvents` parameter is typically set to `true`, but it depends on the target call. You may wish to
set this true for two reasons:

- the target call is composed of two or more overlapping syllables that you want to join as one event.
- whistle events often require this step to unite whistle fragments detections into one event.

## Combine syllables

Add a <xref:AudioAnalysisTools.Events.Types.EventPostProcessing.SyllableSequenceConfig> object to combine syllables.

## Filtering

Use the parameter `Duration` to filter out events that are too long or short.
This filter removes events whose duration lies outside three standard deviations (SDs) of an expected value.

- `ExpectedDuration` defines the _expected_ or _average_ duration (in seconds) for the target events
- `DurationStandardDeviation` defines _one_ SD of the assumed distribution. Assuming the duration is normally distributed, three SDs sets hard upper and lower duration bounds that includes 99.7% of instances. The filtering algorithm calculates these hard bounds and removes acoustic events that fall outside the bounds.

Use the parameter `Bandwidth` to filter out events whose bandwidth is too small or large.
This filter removes events whose bandwidth lies outside three standard deviations (SDs) of an expected value.

- `ExpectedBandwidth` defines the _expected_ or _average_ bandwidth (in Hertz) for the target events
- `BandwidthStandardDeviation` defines one SD of the assumed distribution. Assuming the bandwidth is normally
  distributed, three SDs sets hard upper and lower bandwidth bounds that includes 99.7% of instances. The filtering
  algorithm calculates these hard bounds and removes acoustic events that fall outside the bounds.

## Filtering on side band activity

The intuition of this filter is that an unambiguous event should have an "acoustic-free zone" above and below it.
This filter removes an event that has "excessive" acoustic activity spilling into its sidebands (i.e. upper and lower
"buffer" zones). These events are likely to be _broadband_ events unrelated to the target event. Since this is a common
occurrence, this filter is useful.

Use the parameter `SidebandActivity` to enable side band filtering.

`LowerHertzBuffer` and `UpperHertzBuffer` set the width of the sidebands required below and above the target event.
(These can be also be understood as buffer zones, hence the names assigned to the parameters.)

There are two tests for determining if the sideband activity is excessive:

1. The average decibel value in each sideband should be below the threshold value given by `MaxAverageSidebandDecibels`.
  The average is taken over all spectrogram cells included in a sideband.
2. There should be no more than one sideband frequency bin and one sideband timeframe whose average acoustic activity
  lies within 3 dB of the average acoustic activity in the event. (The averages are over all relevant spectrogram cells.)
  This covers the possibility that there is an acoustic event concentrated in a few frequency bins or timeframes within
  a sideband. The 3 dB threshold is a small arbitrary value which seems to work well. It cannot be changed by the user.

> [!TIP]
> If you do not wish to apply these sideband filters, set `LowerHertzBuffer` and `UpperHertzBuffer` equal to zero.
>Both sideband tests are applied where the buffer zones are non-zero.