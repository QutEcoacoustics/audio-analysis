---
uid: AnalysisPrograms.Recognizers.Base.HarmonicParameters
---

## Harmonic Event detection

The algorithm to find harmonic events uses a `discrete cosine transform` or *DCT*. Setting the correct DCT for the target syllable requires additional parameters.

The algorithm to find harmonic events can be visualized as similar to the
[oscillations algorithm]](xref:AnalysisPrograms.Recognizers.Base.OscillationParameters),
but rotated by 90 degrees. It uses a DCT oriented in a vertical direction and
requires similar additional parameters.

```yml
Profiles:
    Speech: !HarmonicParameters
        FrameSize: 512
        FrameStep: 512
        # The search band
        MinHertz: 500          
        MaxHertz: 5000
        # Min & max duration for a set of harmonics.
        MinDuration: 0.2
        MaxDuration: 1.0        
        DecibelThreshold: 2.0
        #  Min & max Hertz gap between harmonics
        MinFormantGap: 400        
        MaxFormantGap: 1200
        DctThreshold: 0.15         
        # Event threshold - use this to determine FP/FN trade-off.
        EventThreshold: 0.5
```

> [!NOTE]
> The first parameters are common to all events—see
> <xref:AnalysisPrograms.Recognizers.Base.CommonParameters>.
> These parameters determine the search band, the allowable event duration and
> the decibel threshold.
>
> The remaining parameters are unique to the harmonic algorithm and
> determine the search for harmonics.

There are only two parameters that are specific to `Harmonics`,
`MinFormantGap` and `MaxFormantGap`. These specify the minimum and maximum
allowed gap (measured in Hertz) between adjacent formants/harmonics. Note that
for these purposes the terms `harmonic` and `formant` are equivalent.
By default, the DCT is calculated over all bins in the search band.

The output from a DCT operation is an array of coefficients (taking values in
`[0, 1]`). The index into the array is the gap between formants and the value
at that index is the formant amplitude. The index with largest amplitude
indicates the likely formant gap, but `DctThreshold` sets the minimum
acceptable amplitude value. Lowering `DctThreshold` increases the likelihood
that random noise will be accepted as a true set of formants; increasing
`DctThreshold` increases the likelihood that a target set of formants is rejected.
