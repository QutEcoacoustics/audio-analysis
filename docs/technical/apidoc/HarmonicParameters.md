---
uid: AnalysisPrograms.Recognizers.Base.HarmonicParameters
---

## Harmonic Event detection

The algorithm to find harmonic events uses a `discrete cosine transform` or *DCT* to find a stack of harmonics or formants.  Setting the correct DCT for the target syllable requires additional parameters. Note that for our purposes here, the terms `harmonic` and `formant` are taken as equivalent.

The algorithm to find harmonic events can be visualized as similar to the
[oscillations algorithm]](xref:AnalysisPrograms.Recognizers.Base.OscillationParameters),
but rotated by 90 degrees. It uses a DCT oriented in a vertical direction and
requires similar additional parameters.

```yml
Profiles:
    Speech: !HarmonicParameters
        FrameSize: 512
        FrameStep: 512
        SmoothingWindow: 3
        # The search band
        MinHertz: 500          
        MaxHertz: 5000
        # Min & max duration for a set of harmonics.
        MinDuration: 0.2
        MaxDuration: 1.0        
        DecibelThreshold: 2.0
        #  Min & max Hertz interval between harmonics.
        MinFormantGap: 400        
        MaxFormantGap: 1200
        DctThreshold: 0.15         
        # Event threshold - use this to determine FP/FN trade-off.
        EventThreshold: 0.5
```

> [!NOTE]
> Some of these parameters are common to all events, that is, those that determine the search band, the allowable event duration and
> the decibel threshold —see
> <xref:AnalysisPrograms.Recognizers.Base.CommonParameters>.
>
> The remaining parameters are unique to the harmonic algorithm and
> determine the search for harmonics.

There are four parameters specific to `Harmonics`: `SmoothingWindow`,
`MinFormantGap`, `MaxFormantGap` and `DctThreshold`. `SmoothingWindow` sets the window size of a moving average filter that smoothes the frequency bin values in the spectrogram prior to running the DCT. This can be useful when the formants of interest are broken by noise or interrupted. `MinFormantGap` and `MaxFormantGap` specify the minimum and maximum
allowed interval (measured in Hertz) between adjacent formants/harmonics.
By default, the DCT is calculated over all frequency bins in the search band.
`DctThreshold` is a value between 0.0 and 1.0 which sets the sensitivity of the search. A lower value of `DctThreshold` will detect more harmonic events.

The output from a DCT operation is an array of coefficients (taking values in
`[0, 1]`). The index into the array indicates a particular harmonic interval and the array value at that index indicates magnitude of that interval. The index with largest amplitude
indicates the likely interval between each of the formants. However, if the maximum coefficient is less than the `DctThreshold`, a stack of formants is consider not to be present. Lowering the `DctThreshold` increases the likelihood that random noise will be accepted as a true set of formants; increasing the `DctThreshold` increases the likelihood that a target set of formants is rejected.

Note that to reduce the chances of the DCT algorithm producing an erroneous result, a minimum of three harmonics/formants is required, that is, the fundamental and two higher harmonics. Another way to think of this is that at least two harmonic intervals are required to constitute a stack of harmonics. Despite this precaution, the DCT algorithm is sensitive to noise and you made need to experiment to get the optimum parameter values.
