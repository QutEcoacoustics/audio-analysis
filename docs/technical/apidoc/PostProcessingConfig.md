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

The intuition of this filter is that an unambiguous event (representing a call or syllable) should have an "acoustic-free zone" above and below it.
This filter removes an event that has "excessive" acoustic activity spilling into its sidebands.
Such events are likely to be _broadband_ events unrelated to the target event.
Since this is a common occurrence, a sideband filter is useful.

Use the keyword `SidebandAcousticActivity` to enable sideband filtering. There are four parameters, the first two set the width of the sidebands and the second two set decibel thresholds for the amount acoustic noise/activity in those sidebands.

1. `LowerSidebandWidth` sets the width of the desired sideband "zone" below the target event.
2. `UpperSidebandWidth` sets the width of the desired sideband "zone" above the target event.

There are two tests for determining if the acoustic activity in a sideband is excessive, each having a single parameter:

1. `MaxBackgroundDecibels` sets a threshold value for the maximum permitted background or average decibel value in each
  sideband. The average is taken over all spectrogram cells included in a sideband, excluding those adjacent to the event.
2. `MaxActivityDecibels` sets a threshold value for the maximum permitted average decibel value in any one frequency bin
  or timeframe of a sideband. The averages are over all relevant spectrogram cells in a frame or bin, excluding the cell
  adjacent to the event.

This test covers the possibility that there is an acoustic event concentrated in a few frequency bins or timeframes
within a sideband. Only one sideband bin or frame is allowed to contain acoustic activity exceeding the threshold.

> [!TIP]
> To exclude a sideband or not perform a test, comment out its parameter with a `#`. In the above example config file for
> _Ninox boobook_, two of the four parameters are commented out.
>
> ```yaml
>  SidebandAcousticActivity:
>     LowerSidebandWidth: 150
>     #UpperSidebandWidth: 200
>     MaxBackgroundDecibels: 12
>     #MaxActivityDecibels: 12
> ```
>
> In this example, only one test (for background noise) will be performed on only one sideband (the lower).
> If no sideband tests are performed, all events will be accepted regardless of the acoustic activity in their sidebands.
