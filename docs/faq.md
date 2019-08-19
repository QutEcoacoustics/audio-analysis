# FAQ

## How do I reproduce the results I'm given?

You need four things to reproduce a result set:

- The input data
- The config file
- The command used to run AP.exe
- And the same version of AP.exe

Getting the same input data is up to you. 

The config file (and it's values), the command used, and the version of AP.exe
are all recorded in the log file. Once you open the log file you can find and 
extract all of these values. For more information on log files see the document
on [log files](./logs.md).

## Who is this program designed for?

Short answer: computers.

_AP.exe_ is designed primarily to be used by people who have significant computing
expertise. It designed specifically to be used by **other programs** for any real workloads.
It is not designed to be human friendly.

We encourage anyone to give it a go--don't be daunted by these docs--but keep in
mind the target audience. You're in the right ballpark if:

- your workload involves hundreds/thousands of files; or
- you need to use a script just to use _AP.exe_; or
- you have more RAM or CPU than you know what to do with!

More than likely if you're stuck we can help 😊.

## Why do we analyze data in 1-minute chunks?

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

## What effect does chunk size have on data?

For acoustic event recognition, typically only boundary effects are 
affected by chunk-size choice.
That is, if an acoustic event occurs and is clipped by
by either the start or end of the one-minute chunk, then it is now only a "partial vocalisation".
A typical event recognizer may not detect such "partial vocalisations".

For acoustic indices, from a theoretical point of view, chunk-size has the same
kinds of issues as the choice of FFT frame length in speech processing. Because an FFT
assumes signal stationarity, one chooses a frame length over which the spectral
content in the signal of interest is approximately constant. In the case of
acoustic indices, one chooses an index calculation duration which captures
a sufficient amount of data for the acoustic feature that you are interested in.
The longer the analysis duration the more you blur or average out features of
interest. However, if you choose too short an interval in then the calculated
index may be dominated by "noise"---that is, features that are not of interest.
We find this is particularly the case with the ACI index; one-minute seems to be
an appropriate duration for the indices that are typically calculated.

## Can I change the chunk size?

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


## We collect metrics/statistics; what information is collected and how is it used?

**NOTE: this is an upcoming feature and has not been released yet**

_AP.exe_ collects metrics (statistics) so that we can measure important parts
of how our program is run. It lets us keep a track of how much computer
resources we use, how much audio we analyze, and which commands and analyses
are used the most. Metrics help us prioritize new features or important bugs
and they also justify continued investment into this software.

We collect mostly anonymous information. All information collected is also
embedded into the [log file](./logs) of each run so you can inspect it
yourself. AP.exe sends a payload to our metric collector similar
to the following (entirely anonymous):

```
{
  "Platform": "Microsoft Windows NT 6.2.9200.0",
  "ProcessorCount": 32,
  "ExecutionTime": 3441.2261135999997,
  "PeakWorkingSet": 239583232,
  “DurationAnalysed”: 86399.75
}
```

When the metrics are collected we also record which public IP address they were
sent from. 

Metrics can be disabled with an an environment variable. If you're concerned
with the data collection then please contact us so we can work out a compromise.

## What is a _binary_? What is an _executable_? What does _compiling_ mean?

Unlike R, Python, Ruby, or JavaScript, some programming languages can not just be run straight from source code.

We call such languages _compiled_ programming languages because a special program, called a _compiler_
is required to transform the text-based programming source code to a low-level set of instructions that a computer understands.

Some programming languages that need to be compiled include C++, C, Java, and C#.

This compilation step is discrete and happens before the code is run. Compiling is often referred to as _building_.

The result of compilation is one or more _binary_ files. We call these files binaries because the code in them
is no longer readable text--rather it is just blobs of binary instructions that the computer will use.

Binaries that can be run as programs are often called _executables_.

## What is a _command_?

Our program is a monolith--a very large structure. To support doing different
things we have various sub-programs that can be run by themselves. We call each
of these sub-programs a _command_. If you run _AP.exe_ with no arguments, it will
list the available commands (sub-programs) that you can run.