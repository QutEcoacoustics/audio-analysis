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