---
uid: AnalysisPrograms.Recognizers.Base.CommonParameters
---

Each generic acoustic event algorithm shares these common parameters.

Don't use `CommonParameters` directly, use one of the algorithm parameter types.

## Spectrogram Parameters

For an explanation of these parameters see <xref:theory-spectrograms>.

## Bounding parameters

`MinHertz` and `MaxHertz` define the frequency band in which a search is to be made for the target event. Note that
these parameters define the bounds of the search band _not_ the bounds of the event itself.

`MinDuration` and `MaxDuration` set the minimum and maximum time duration (in seconds) of the target event.

Each of these limits are are hard bounds.

## Decibel thresholds

`DecibelThresholds` is an array of numbers that represent activity thresholds.
If a candidate event is above a threshold it be reported as an event.

Multiple thresholds can be used to cater for similar events that vary in intensity.

```yml
# Scan the frequency band at these thresholds
DecibelThresholds:
    - 6.0
    - 9.0
    - 12.0
```

<figure>

![Common Parameters](~/images/generic_recognizer/Fig2EventParameters.png)

<figcaption>Common parameters for all acoustic events, using an oscillation event as example.</figcaption>
</figure>
