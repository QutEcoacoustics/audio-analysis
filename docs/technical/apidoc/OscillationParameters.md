---
uid: AnalysisPrograms.Recognizers.Base.OscillationParameters
---

## Oscillation Event detection

The algorithm to find oscillation events uses a `discrete cosine transform` or *DCT*. Setting the correct DCT for the
target syllable requires additional parameters. Here is the `Profiles` declaration in the config file for the
_flying fox_. It contains two profiles, the first for a vocalization and the second to detect the rhythmic sound of wing
beats as a flying fox takes off or comes in to land.

```yml
Profiles:
    Territorial: !BlobParameters
        ComponentName: TerritorialScreech
        MinHertz: 800          
        MaxHertz: 8000
        MinDuration: 0.15
        MaxDuration: 0.8
        DecibelThresholds:
            - 9.0
    Wingbeats: !OscillationParameters
        ComponentName: Wingbeats
        # The search band
        MinHertz: 200          
        MaxHertz: 2000
        # Min & max duration for sequence of wingbeats.
        MinDuration: 1.0
        MaxDuration: 10.0        
        DecibelThresholds:
            - 6.0
        # Wingbeat bounds - oscillations per second       
        MinOscillationFrequency: 4        
        MaxOscillationFrequency: 6    
        # DCT duration in seconds 
        DctDuration: 0.5
        # minimum acceptable value of a DCT coefficient
        DctThreshold: 0.5
        
        # Event threshold - use this to determine FP/FN trade-off.
        EventThreshold: 0.5
```

> [!NOTE]
> The first parameters are common to all events—see
> <xref:AnalysisPrograms.Recognizers.Base.CommonParameters>.
> These parameters determine the search band, the allowable event duration and
> the decibel threshold.
>
> The remaining parameters are unique to the oscillation algorithm and
> determine the search for oscillations.

`MinOscilFreq` and `MaxOscilFreq` specify the oscillation bounds in beats or
oscillations per second. These values were established by measuring a sample of
flying fox wingbeats.

The next two parameters, the DCT duration in seconds and
the DCT threshold can be tricky to establish but are critical for success.
The DCT is computationally expensive but for accuracy it needs to span at least
two or three oscillations. In this case a duration of `0.5` seconds is just enough
to span at least two oscillations. The output from a DCT operation is an array
of coefficients (taking values in `[0, 1]`). The index into the array is the
oscillation rate and the value at that index is the amplitude. The index with
largest amplitude indicates the likely oscillation rate, but `DctThreshold` sets
the minimum acceptable amplitude value. Lowering `DctThreshold` increases the
likelihood that random noise will be accepted as a true oscillation;
increasing `DctThreshold` increases the likelihood that a target oscillation is
rejected.

The optimum values for `DctDuration` and `DctThreshold` interact. It requires
some experimentation to find the best values for your target syllable.
Experiment with `DctDuration` first while keeping the `DctThreshold` value low.
Once you have a reliable value for `DctDuration`, gradually increase the value
for `DctThreshold` until you're no longer detecting target events.

<figure>

![DCT parameters](~/images/generic_recognizer/DCTparameters.jpg)

<figcaption>Figure. Parameters required for using a DCT to detect an oscillation event.</figcaption>
</figure>
