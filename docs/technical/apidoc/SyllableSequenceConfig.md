---
uid: AudioAnalysisTools.Events.Types.EventPostProcessing.SyllableSequenceConfig
---


Use `SyllableSequence`  where you want to combine possible syllable sequences. A typical example is
a sequence of chirps in a honeyeater call.

`SyllableStartDifference` and `SyllableHertzGap` set the allowed tolerances when combining events into sequences

- `SyllableStartDifference` sets the maximum allowed time difference (in seconds) between the starts of two events
- `SyllableHertzGap` sets the maximum allowed frequency difference (in Hertz) between the minimum frequencies of two events.

Once you have combined possible sequences, you may wish to remove sequences that do not satisfy the parameters for your
target call. Set `FilterSyllableSequence` true if you want to filter (remove) sequences that do not fall within the
constraints defined by `SyllableMaxCount` and `ExpectedPeriod`.

- `SyllableMaxCount` sets an upper limit of the number of events that are combined to form a sequence
-`ExpectedPeriod` sets a limit on the average period (in seconds) of the combined events.