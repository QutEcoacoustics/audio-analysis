---
title: DIY Call Recognizer
uid: guides-generic-recognizers
---

# DIY Call Recognizers

A **DIY Call Recognizer** is a utility within [_Analysis Programs_](xref:basics-introduction) which allows you to write
your own call recognizers.

A **DIY call recognizer** uses our _generic recognizer_ tools. This guide will help you make your own
_generic recognizer_. The generic recognizer allows a user to generically reuse and parametrize our syllable detectors.
Once you can detect new syllables those syllables can be combined to form new call recognizers.  

> [!NOTE]
>
> - Incomplete parts of the manual are indicated by _**TODO**_.
> - Features not yet implemented are marked with a construction emoji (🚧).

## 1. Why make a DIY call recognizer?

There are three levels of sophistication in automated call recognizers:

- The simplest is the handcrafted template.
- More powerful is a _machine learned_ model.
- The current cutting edge of call recognizers is *deep-learning* using a convolutional neural network.

A comparison of these recognizer types is shown in the following table and explained further in the subsequent paragraph.

<figure>

|      Type of Recognizer     | Who does the feature extraction? |    Required dataset    | Skill level |    Accuracy    |
|:---------------------------:|:--------------------------------:|:----------------------:|:-----------:|:--------------:|
|      Template matching      |               User               |     Small (even 1!)    |    Least    | Sometimes good |
| Supervised machine learning |               User               |   Moderate (50-100s)   |     Some    |     Better     |
|             CNN             |       Part of CNN learning       | Very large (10k to 1M) |    A lot!   |      Best?     |

<figcaption>A comparison of three different kinds of call recognizer</figcaption>
</figure>

Hand-crafted, *rule-based* templates can be built using just one or a few examples of the target call. But like any
rule-based *AI* system, they are *brittle*, that is, they break easily if the target call falls even slightly outside
the bounds of the rules.

A supervised machine-learning model, for example an SVM or Random Forest, is far more resilient to slight changes in the
range of the target call but they require many more training examples, on the order of 100 training examples.

Finally, the convolutional neural network (CNN) is the most powerful learning machine available today (2021) but this
power is achieved only by supplying thousands of examples of the each target call.

> [!TIP]
> The following two rules apply to the preparation of training/test datasets, regardless of the recognizer type.
>
> - **Rule 1.** Rubbish in ➡ rubbish out!  
>    That is, think carefully about your chosen training/test examples.
> - **Rule 2.** Training and test sets should be representative (in some loose statistical sense) of the intended
>    operational environment.

To summarize (and at the risk of over-simplification):

- a hand-crafted template has low cost and low benefit
- a machine-learned model has medium cost and medium benefit
- while a deep-learned model has high cost and high benefit

The cost/benefit ratio in each case is similar but here is the catch - the cost must be paid _before_ you get the
benefit! Furthermore, in a typical ecological study, a bird species is of interest precisely because it is threatened or
cryptic. When not many calls are available, the more sophisticated approaches become untenable. Hence there is a place
for hand-crafted templates in call recognition.

These ideas are summarized in the following table:

| Type of Recognizer |  Cost  | Benefit | Cost/benefit ratio |        The catch !       |
|:------------------:|:------:|:-------:|:------------------:|:------------------------:|
|  Template matching |   Low  |   Low   |      A number      |     You must pay ...     |
|  Machine learning  | Medium |  Medium |  A similar number  |  ... the cost before ... |
|         CNN        |  High  |   High  |  A similar number  | ... you get the benefit! |

To summarize, the advantages of a hand-crafted DIY call recognizer are:

1. You can do it yourself!
2. You can start with just one or two calls.
3. Allows you to collect a larger dataset (and refine it) for machine learning purposes.
4. Exposes the variability of the target call as you go.

## 2. Calls, syllables, harmonics

The algorithmic approach of **DIY Call Recognizer** makes particular assumptions about animals calls and how they are structured.
A *call* is taken to be any sound of animal origin (whether for communication purposes or not) and include
bird songs/calls, animal vocalizations of any kind, the stridulation of insects, the wingbeats of birds and bats and the
various sounds produced by aquatic animals. Calls typically have temporal and spectral structure.
For example they may consist of a temporal sequence of two or more *syllables* (with "gaps" in between) or a set of simultaneous *harmonics*
or *formants*. (The distinction between harmonics and formants does not concern us here.)

## 3. Acoustic events

An [_acoustic event_](xref:theory-acoustic-events) is defined as a contiguous set of spectrogram cells/pixels whose
decibel values exceed some user
defined threshold. In the ideal case, an acoustic event should encompass a discrete component of acoustic energy within
a call, syllable or harmonic. It will be separated from other acoustic events by gaps having decibel values *below*
the user defined threshold.

**DIY Call Recognizer** contains algorithms to recognize seven different kinds of _generic_ acoustic events based on
their shape in the spectrogram.

There are seven types of acoustic events:

1. [Shrieks](xref:theory-acoustic-events#shrieks):
   diffuse events treated as "blobs" of acoustic energy. A typical example is a parrot shriek.
2. [Whistles](xref:theory-acoustic-events#whistles):
  "pure" tones (often imperfect) appearing as horizontal lines on a spectrogram.
3. [Chirps](xref:theory-acoustic-events#chirps):
  whistle like events that increases in frequency over time. Appears like a sloping line in a spectrogram.
4. [Whips](xref:theory-acoustic-events#whips):
  sound like a "whip crack". They appear as steeply ascending or descending *spectral track* in the spectrogram.
5. [Clicks](xref:theory-acoustic-events#clicks):
  appear as a single vertical line in a spectrogram and sounds, like the name suggests, as a very brief click.
6. [Oscillations](xref:theory-acoustic-events#oscillations):
  An oscillation is the same (or nearly the same) syllable (typically whips or clicks) repeated at a fixed periodicity over several to many time-frames.
7. [Harmonics](xref:theory-acoustic-events#harmonics):
  Harmonics are the same/similar shaped *whistle* or *chirp* repeated simultaneously at multiple intervals of frequency. Typically, the frequency intervals are similar as one ascends the stack of harmonics.

For more detail on event types see [_acoustic events_](xref:theory-acoustic-events).

<figure>

![Seven Kinds Of Acoustic Event](../images/SevenKindsAcousticEvent.jpg)

<figcaption>The seven kinds of generic acoustic event</figcaption>
</figure>

## 4. Detecting acoustic events

A **DIY Call Recognizer** attempts to recognize calls in a noise-reduced [spectrogram](xref:theory-spectrograms) using a sequence of steps:

1. Preprocessing—steps to prepare the recording for subsequent analysis.
    1. Input audio is broken up into 1-minute chunks
    2. Audio resampling
2. Processing—steps to identify target syllables as _"generic"_ acoustic events
    1. Spectrogram preparation
    2. Call syllable detection
3. Postprocessing—steps which simplify the output combining related acoustic events and filtering events to remove false-positives
    1. Combining syllable events into calls
    2. Syllable/call filtering

4. Saving Results

To execute these detection steps, suitable _parameter values_ must be placed into a [_configuration file_](xref:basics-config-files).

## 5. Configuration files

All analyses in _AP_ require a [_configuration file_](xref:basics-config-files) (henceforth, _config_ file) in order to tune the analysis.

It is no different for a generic recognizer. To find calls of interest in a recording _AP_ reads the config file
which contains _parameters_ and then executes the detection steps accordingly.

> [!IMPORTANT]
> If you're not familiar with AP's config files please review our <xref:basics-config-files> page.

### Naming

Configuration files must be named in a certain format. The basic format is:

[!include[config_naming](../basics/config_file_name.md)]

See [Naming in the Config Files](xref:basics-config-files#naming) document for more details and examples.

### Parameters

Config files contain a list of parameters, each of which is written as a name-value pair, for example:

```yml
ResampleRate: 22050
```

Changing these parameters allows for the construction of a generic recognizer. This guide will explain the various
parameters than can be changed and their typical values. However, this guide will not produce a functional recognizer;
each recognizer has to be "tuned" to the target syllables for species to be recognized. Only you can do that.

There are many parameters available. To make config files easier to read we order these parameters roughly in the
order that they are applied. This aligns with the [basic recognition](#4-detecting-acoustic-events) steps from above.

1. Parameters for preprocessing
2. Parameters for processing
3. Parameters for postprocessing
4. Parameters for  saving Results

### Profiles

[Profiles](xref:basics-config-files#profiles) are a list of acoustic event detection algorithms to use in our processing stage.

> [!TIP]
> For an introduction to profiles see the <xref:basics-config-files#profiles> page.

Each algorithm is designed to detect a syllable type. Thus to make a generic recognizer there should be at least one (1) profile in the `Profiles` list.
A config file may target more than one syllable or acoustic event, in which case there will be a profile for each target syllable or acoustic event.

The `Profiles` list contains one or more profile items, and each profile has several parameters. So we have a three level hierarchy:

1. The key-word `Profiles` that heads the list.
2. One or more _profile_ declarations.
    - There are two parts to each profile declaration:
        1. A user defined name
        2. And the algorithm type to use with this profile (prefixed with an exclamation mark (`!`))
3. The profile _parameters_ consisting of a list of name:value pairs

Here is an (abbreviated) example:

```yml
Profiles:  
    BoobookSyllable1: !ForwardTrackParameters
        # min and max of the freq band to search
        MinHertz: 400          
        MaxHertz: 1100
        # min and max time duration of call
        MinDuration: 0.1
        MaxDuration: 0.499
    BoobookSyllable2: !ForwardTrackParameters
        MinHertz: 400          
        MaxHertz: 1100
        MinDuration: 0.5
        MaxDuration: 0.899
    BoobookSyllable3: !ForwardTrackParameters
        MinHertz: 400          
        MaxHertz: 1100
        MinDuration: 0.9
        MaxDuration: 1.2
```

This artificial example illustrates three profiles (i.e. syllables or acoustic events) under the key word `Profiles`.

We can see one of the profiles has been given the name `BoobookSyllable3` and has the type `ForwardTrackParameters`.
This means for the `BoobookSyllable3` we want _AP_ to use the _forward track_ algorithm to look for a _chirp_.

Each profile in this example has four parameters. All three profiles have the same values for `MinHertz` and `MaxHertz`
but different values for their time duration. Each profile is processed separately by _AP_.

### Algorithm types

In the above example, the line `BoobookSyllable1: !ForwardTrackParameters` is to be read as:

> the name of the target syllable is _BoobookSyllable1_ and its type is _ForwardTrackParameters_

There are currently seven algorithm types, each designed to detect a different type of acoustic event.
The names of the acoustic events describe what they sound like, whereas,
the names of the algorithms (used to find those events) describe how the algorithms work.

 This table lists the "generic" events, the algorithm used to detect the event, and the name of the parameter list needed by the algorithm.

| Acoustic Event | Algorithm name    | Parameters name              |
|:--------------:|:-----------------:|:----------------------------:|
| Shriek         | `Blob`            | `!Blob`                      |
| Whistle        | `HorizontalTrack` | `!HorizontalTrackParameters` |
| Chirp          | `ForwardTrack`    | `!ForwardTrackParameters`    |
| Whip           | `UpwardsTrack`    | `!UpwardsTrackParameters`    |
| Click          | `VerticalTrack`   | `!VerticalTrackParameters`   |
| Oscillation    | `Oscillation`     | `!OscillationParameters`     |
| Harmonic       | `Harmonic`        | `!HarmonicParameters`        |

Each of these detection algorithms has some common parameters because all "generic" events are characterized by common
properties, such as their minimum and maximum temporal duration, their minimum and maximum frequencies, and their decibel intensity.
In fact, every acoustic event is bounded by an _implicit_ rectangle or marquee whose height represents the bandwidth of
the event and whose width represents the duration of the event.

Even a _chirp_ or _whip_ which consists only of a single sloping
*spectral track*, is enclosed by a rectangle, two of whose vertices sit at the start and end of the track.

See <xref:AnalysisPrograms.Recognizers.Base.CommonParameters> for more details.

## 6. Config parameters and values

This section describes how to set the parameters values for each of the seven call-detection steps. We use, as a concrete example, the config file for the Boobook Owl, *Ninox boobook*.

The `YAML` lines are followed by an explanation of each parameter.

### Audio segmentation and resampling

Analysis of long recordings is made tractable by [breaking them into shorter (typically 60-second) segments](xref:article-minute-chunks).
This is done with the <xref:command-analyze-long-recording> command.

The first part of a generic recognizer config file is as follows:

[!code-yaml[prep](./Ecosounds.NinoxBoobook.yml#L4-L9 "Audio segmentation")]

These parameters control:

- the size of the segments into which the audio file is split for analysis
- the amount of overlap between consecutive segments
- the sample rate at which the analysis is performed (22050 Hz)

For more information on these parameters see the <xref:AnalysisBase.AnalyzerConfig> page.

Segment size and and overlap have good defaults set and you should not need to change them. The best value for sample
rate will be analysis dependent, but will default to 22050 Hertz if not provided.

<figure>

![First Three Detection Steps](~/images/generic_recognizer/ParametersForSteps1-3.png)

<figcaption>Segmenting and resampling</figcaption>
</figure>

### Adding profiles

For each acoustic event you want to detect, you need to add a profile. Each profile uses one of the generic recognizer algorithms.

#### Common Parameters

[!code-yaml[profile](./Ecosounds.NinoxBoobook.yml#L11-L15 "Profiles")]

The key parts here are:

- the profile name (`BoobookSyllable`)
- the algorithm type (`!ForwardTrackParameters` which will detect a _chirp_)
- and an optional species name (`NinoxBoobook`)

Both the profile name and the species names can be any name you like. The names are stored in the results so you know
what algorithm generated an event.

We could have a profile name of `banana` and species name of `i_like_golf`—but neither of these names are useful
because they are not descriptive.

All algorithms have some [common parameters](xref:AnalysisPrograms.Recognizers.Base.CommonParameters). These include

- Spectrogram settings
- Noise removal settings
- Parameters that set basic limits to the allowed duration and bandwidth of an event

Each algorithm has its own spectrogram settings, so parameters such as `WindowSize` can be varied for _each_ type of
acoustic event you want to detect.

#### [Common Parameters](xref:AnalysisPrograms.Recognizers.Base.CommonParameters): Spectrogram preparation

By convention, we list the spectrogram parameters first (after the species name) in
each algorithm entry:

[!code-yaml[spectrogram](./Ecosounds.NinoxBoobook.yml#L11-L19 "Spectrogram parameters")]

- `FrameSize` sets the size of the FFT window.
- `FrameStep` sets the number of samples between frame starts.
- `WindowFunction` sets the FFT window function.
- `BgNoiseThreshold` sets the degree of background noise removal.

Since these parameters are so important for the success of call detection, you are strongly advised to refer to the
<xref:theory-spectrograms> document for more information about setting their values.

#### [Common Parameters](xref:AnalysisPrograms.Recognizers.Base.CommonParameters): Call syllable limits

A complete definition of the `BoobookSyllable` follows.

[!code-yaml[full_profile](./Ecosounds.NinoxBoobook.yml#L11-L30 "A complete profile")]

The additional parameters direct the actual search for target syllables in the spectrogram.

- `MinHertz` and `MaxHertz` set the frequency band in which to search for the target event.
  Note that these parameters define the bounds of the search band, _not_ the bounds of the event itself. These limits
  are hard bounds.
- `MinDuration` and `MaxDuration` set the minimum and maximum time duration (in seconds) of the target event. These
  limits are hard bounds.

<figure>

![Common Parameters](~/images/generic_recognizer/Fig2EventParameters.png)

<figcaption>Common parameters for all acoustic events, using an oscillation event as example.</figcaption>
</figure>

### Adding profiles with algorithms

For brevity, we've broken up the descriptions of each algorithm to their own pages.
Some of these algorithms have extra parameters, some do not, but all do have the
[common parameters](xref:AnalysisPrograms.Recognizers.Base.CommonParameters) we've previously described.

| I want to find a | I'll use this algorithm                                                                  |
|------------------|------------------------------------------------------------------------------------------|
| Shriek           | [!BlobParameters](xref:AnalysisPrograms.Recognizers.Base.BlobParameters)                 |
| Whistle          | 🚧 !HorizontalTrackParameters 🚧                                                        |
| Chirp            | [!ForwardTrackParameters](xref:AnalysisPrograms.Recognizers.Base.ForwardTrackParameters) |
| Whip             | 🚧!UpwardsTrackParameters 🚧                                                            |
| Click            | 🚧 !VerticalTrackParameters 🚧                                                          |
| Oscillation      | [!OscillationParameters](xref:AnalysisPrograms.Recognizers.Base.OscillationParameters)   |
| Harmonic         | [!HarmonicParameters](xref:AnalysisPrograms.Recognizers.Base.HarmonicParameters)         |

### [Post Processing](xref:AudioAnalysisTools.Events.Types.EventPostProcessing.PostProcessingConfig)

The post processing stage is run after event detection (the `Profiles`).
Note that these post-processing steps are performed on all acoustic events collectively, i.e. all those "discovered"
by all the *profiles* in the list of profiles.

Add a post processing section to you config file by adding the `PostProcessing` parameter and indenting the sub-parameters.

[!code-yaml[post_processing](./Ecosounds.NinoxBoobook.yml#L34-L34 "Post Processing")]

Post processing is optional - you may decide to combine or filter the "raw" events using code you have written yourself.
To add a post-processing section to your config file, insert the `PostProcessing` keyword and indent its parameters.

> [!NOTE]
> If you do not wish to include a post-processing step, delete or comment out the key word and all its parameters
> parameters using a `#` at the start of each relevant line in the config file.
> Disabling a post-processing filter means that all events are accepted for that step.

#### Order of operation

There are five post-processing steps, each of which is optional.
However the order in which these steps are performed _cannot_ be changed by the user. The post-processing sequence is:

1. Combine events having temporal _and_ spectral overlap.
2. Combine possible sequences of events that constitute a "call".
3. Remove (filter) events whose duration is outside an acceptable range.
4. Remove (filter) events whose bandwidth is outside an acceptable range.
5. Remove (filter) events having excessive acoustic activity in their sidebands.

Post-processing is performed once for each of the DecibelThresholds.

An example:

> Suppose you have three decibel thresholds (6, 9 and 12 dB is a typical set of values) in each of two profiles.
> There will be three rounds of post-processing:
>
> 1. All the events detected at threshold 6 dB (by both profiles) will be collected together and subjected to the
>   post-processing steps.
> 2. Next all the events detected at 9 dB will be collected and independently subjected to post-processing.
> 3. Next all events detected at the 12 dB threshold will be post-processed.

This sequence of multiple post-processing steps gives rise to one or more overlapping events.
The final post-processing step is to remove all but the longest duration event in any overlapping
set of events. This final step is **not configurable**.

#### Combine events having temporal _and_ spectral overlap

[!code-yaml[post_processing_combining](./Ecosounds.NinoxBoobook.yml#L34-L42 "Post Processing: Combining")]

The `CombineOverlappingEvents` parameter is typically set to `true`, but it depends on the target call. You would typically set this to true for two reasons:

- the target call is composed of two or more overlapping syllables that you want to join as one event.
- whistle events often require this step to unite whistle fragment detections into one event.

#### Combine possible sequences of events that constitute a "call"

Unlike overlapping events, if you want to combine a group of events (like syllables) that are near each other but not
overlapping, then make use of the `SyllableSequence` parameter. A typical example would be to join a sequence of chirps
in a honeyeater call.

[!code-yaml[post_processing_combining_syllables](./Ecosounds.NinoxBoobook.yml?start=34&end=51&highlight=10- "Post Processing: Combining syllables")]

`SyllableStartDifference` and `SyllableHertzGap` set the allowed tolerances when combining events into sequences

- `SyllableStartDifference` sets the maximum allowed time difference (in seconds) between the starts of two events.
- `SyllableHertzGap` sets the maximum allowed frequency difference (in Hertz) between the minimum frequencies of two events.

Once you have combined possible sequences, you may wish to remove sequences that do not satisfy the periodicity
constraints for your target call, that is, the maximum number of syllables permitted in a sequence and the average time
gap between syllables. To enable filtering on syllable periodicity, set `FilterSyllableSequence` to true and assign
values to `SyllableMaxCount` and `ExpectedPeriod`.

- `SyllableMaxCount` sets an upper limit on the number of events that constitute an allowed sequence.
- `ExpectedPeriod` sets an expectation value for the average period (in seconds) of an allowed combination of events.

> [!NOTE]
> This property interacts with `SyllableStartDifference`. Refer to the following documentation  for more information:
> <xref:AudioAnalysisTools.Events.Types.EventPostProcessing.SyllableSequenceConfig>.

#### Remove events whose duration is outside an acceptable range

[!code-yaml[post_processing_filtering](./Ecosounds.NinoxBoobook.yml?start=34&end=62&highlight=20- "Post Processing: filtering")]

Use the parameter `Duration` to filter out events that are too long or short. There are two parameters:

- `ExpectedDuration` defines the _expected_ or _average_ duration (in seconds) for the target events.
- `DurationStandardDeviation` defines _one_ SD of the assumed distribution.

Refer to the following documentation for more information: <xref:AudioAnalysisTools.Events.Types.EventPostProcessing.DurationConfig>.

#### Remove events whose bandwidth is outside an acceptable range

Use the parameter `Bandwidth` to filter out events whose bandwidth is too small or large. There are two parameters:

- `ExpectedBandwidth` defines the _expected_ or _average_ bandwidth (in Hertz) for the target events.
- `BandwidthStandardDeviation` defines one SD of the assumed distribution.

Refer to the following documentation for more information: <xref:AudioAnalysisTools.Events.Types.EventPostProcessing.BandwidthConfig>.

#### Remove events that have excessive noise or acoustic activity in their side-bands

[!code-yaml[post_processing_sideband](./Ecosounds.NinoxBoobook.yml?start=34&end=69&highlight=30- "Post Processing: sideband noise removal")]

The intuition of this filter is that an unambiguous event (representing a call or syllable) should have an "acoustic-free zone" above and below it.
This filter removes an event that has "excessive" acoustic activity spilling into its sidebands.
Such events are likely to be _broadband_ events unrelated to the target event.
Since this is a common occurrence, a sideband filter is useful.

For details on configuring this step see <xref:AudioAnalysisTools.Events.Types.EventPostProcessing.PostProcessingConfig#filtering-on-side-band-activity>.

### Parameters for saving results

The parameters in this final part of the config file determine what results are saved to file.

[!code-yaml[results](./Ecosounds.NinoxBoobook.yml#L70-L78 "Result output")]

Each of the parameters controls whether extra diagnostic files are saved while doing an analysis.

> [!IMPORTANT]
> If you are doing a lot of analysis **you'll want to disable** this extra diagnostic output. It will produce files
> that are in total larger than the input audio data—you will fill your harddrive quickly!

- `SaveSonogramImages` will save a spectrogram for each analysed segment (typically one-minute)
- `SaveIntermediateWavFiles` will save the converted WAVE file used to analyze each segment

Both parameters accept three values:

- `Never`: disables the output.
- `WhenEventsDetected`: only outputs the spectrogram/WAVE file when an event is found in the current segment.
  This choice is the most useful for debugging a new recognizer.
- `Always`: always save the diagnostic files. Don't use this option if you're going to analyze a lot of files

### The completed example

Here is a the completed config file for the hypothetical boobook recognizer we have been working with:

[!code-yaml[final](./Ecosounds.NinoxBoobook.yml "Final config")]

## 7. An efficient strategy to tune parameters

Tuning parameter values can be frustrating and time-consuming if a logical sequence is not followed. The idea is to
tune parameters in the sequence in which they appear in the config file, keeping all "downstream" parameters as "open"
or "unrestrictive" as possible. Here is a suggested tuning strategy:

1. Turn off all post-processing steps. That is, comment out all post-processing keywords/parameters.
2. Initially set all profile parameters so as to catch the maximum possible number of target calls/syllables.
    1. Set the array of decibel thresholds to cover the expected range of call amplitudes from minimum to maximum decibels.
    2. Set the minimum and maximum duration values to catch every target call by a wide margin. At this stage, do not
      worry that you are also catching a lot of false-positive events.
    3. Set the minimum and maximum frequency bounds to catch every target call by a wide margin. Once again, do not
      worry that you are also catching a lot of false-positive events.
    4. Set other parameters to their least "restrictive" values in order to catch maximum possible target events.

At this point you should have "captured" all the target calls/syllables (i.e. there should be minimal false-negatives), _but_ you are likely to have many false-positives.

3. Gradually constrain the parameter bounds (i.e. increase minimum values and decrease maximum values) until you start
  to lose obvious target calls/syllables. Then back off so that once again you just capture all the target events—but
  you will still have several to many false-positives.
4. Event combining: You are now ready to set parameters that determine the *post-processing* of events.
  The first post-processing steps combine events that are likely to be *syllables* that are part of the same *call*.
5. Event Filtering: Now add in the event filters in the same sequence as they appear in the config file.
  This sequence cannot currently be changed because it is determined by the underlying code. There are event filters for duration, bandwidth, periodicity of component syllables within a call and finally acoustic activity in the sidebands of an event.
    1. Set the `periodicity` parameters for filtering events based on syllable sequences.
    2. Set the `duration` parameters for filtering events on their time duration.
    3. Set the `bandwidth` parameters for filtering events on their bandwidth.
    4. Set the `SidebandAcousticActivity` parameters for filtering based on sideband  _acoustic activity_.

> [!NOTE]
> You are unlikely to want to use all filters. Some may be irrelevant to your target call.

At the end of this process, you are likely to have a mixture of true-positives, false-positives and false-negatives.
The goal is to set the parameter values so that the combined FP+FN total is minimized. You should adjust parameter
values so that the final FN/FP ratio reflects the relative costs of FN and FP errors.
For example, lowering a decibel threshold may pick up more TPs but almost certainly at the cost of more FPs.

> [!NOTE]
> A working DIY Call Recognizer can be built with just one example or training call. A machine learning algorithm typically requires 100 true and false examples.
> The price that you (the ecologist) pays for this simplicity is the
need to exercise some of the "intelligence" that would otherwise be exercised by the machine learning algorithm.
> That is, you must select calls and set parameter values that reflect the variability of the target calls and the relative costs of FN and FP errors.

## 8. Eight steps to building a DIY Call Recognizer

We described above the steps required to tune parameter values in a recognizer config file. We now step back from this detail and take an overview of all the steps required to obtain an operational recognizer for one or more target calls.

1. Select one or more one-minute recordings that contain typical examples of your target call. It is also desirable
  that the background acoustic events in your chosen recordings are representative of the intended operational
  environment. If this is difficult, one trick to try is to play examples of your target call through a loud speaker in
  a location that is similar to your intended operational environment. You can then record these calls using your
  intended Acoustic Recording Unit (ARU).
2. Assign parameter values into your config.yml file for the target call(s).
3. Run the recognizer, using the command line described in the next section.
4. Review the detection accuracy and try to determine reasons for FP and FN detections.
5. Tune or refine parameter values in order to increase the detection accuracy.
6. Repeat steps 3, 4 and 5 until you appear to have achieved the best possible accuracy. In order to minimize the
  number of iterations of stages 3 to 5, it is best to tune the configuration parameters in the sequence described in
  the previous section.
7. At this point you should have a recognizer that performs "as accurately as possible" on your training examples.
  The next step is to test your recognizer on one or a few examples that it has not seen before.
  That is, repeat steps 3, 4, 5 and 6 adding in a new example each time as they become available.
  It is also useful at this stage to accumulate a set of recordings that do *not* contain the target call.
  See Section 10 for more suggestions on building datasets.
8. At some point you are ready to use your recognizer on recordings obtained from the operational environment.

## 9. Running a generic recognizer

_AP_ performs several functions. Each function is selected by altering the command used to run _AP_.

For running a generic recognizer we need to to use the [`audio2csv`](xref:command-analyze-long-recording) command.

- For an introduction to running commands see <xref:cli>
- For detailed help on the audio2csv command see <xref:command-analyze-long-recording>

The basic form of the command line is:

```bash
AnalysisPrograms.exe audio2csv <input_file> <config_file> <output_folder> --analysis-identifier "Ecosounds.GenericRecognizer"
```

When you run the command, swap out `<input_file>`, `<config_file>`, and `<output_folder>` for the actual paths to your audio,
your config file, and your desired output folder respectively.
For example, if the files `birds.wav` and `NinoxBoobook.yml` were in the current folder and you want to save output to the folder `/BoobookResults`, one could run:

```bash
AnalysisPrograms.exe audio2csv birds.wav NinoxBoobook.yml BoobookResults --analysis-identifier "Ecosounds.GenericRecognizer"
```

> [!NOTE]
> The analysis-identifier (`--analysis-identifier` followed by the `"Ecosounds.GenericRecognizer"`) is required for
> generic recognizers.  Using `--analysis-identifier` informs _AP_ that this is generic recognition task and enables it to perform the correct analysis.

If you want to run your generic recognizer more than once, you might want to use
[powershell](xref:guides-scripting-pwsh) or [R](xref:guides-scripting-r) to script _AP_.

## 10. Building a larger data set

As indicated above, it is useful to accumulate a set of recordings, some of which contain the target call and some of
which *do not*. The *negative* examples should include acoustic events that have previously been detected as FPs.

You now have two sets of recordings, one set containing the target call(s) and one set containing previous FPs and
other possibly confusing acoustic events. The idea is to tune parameter values, while carefully watching for what
effect the changes have on both data sets.

Eventually, these two labelled data sets can be used for

- validating the efficacy of your recognizer
- or for machine learning purposes.

_Egret_ is software designed to assess large datasets for recognizer performance, in an **automated** fashion.
_Egret_ can greatly speed up the development of a recognizer because it is easier to repeatedly test small changes to
your recognizer parameters.

_Egret_ is available from [https://github.com/QutEcoacoustics/egret](https://github.com/QutEcoacoustics/egret).
