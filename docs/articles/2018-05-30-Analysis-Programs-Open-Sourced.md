---
title: Open sourcing our analysis code
uid: article-open-source
---

We have wanted to open source our analysis code for some time but various
complications made this a difficult prospect. We [committed to open sourcing
back in October](http://research.ecosounds.org/2017/10/18/about-our-platform)
last year and we are now finally ready to announce it.

We have released the *QUT Ecoacoustics Analysis Programs* software package. The
code is on GitHub and can be found here
<https://github.com/QutEcoacoustics/audio-analysis>. Analysis Programs is our
(somewhat unimaginative) name for the collection of code that runs all our
production grade analysis. The Analysis Programs software package can perform a
suite of analyses on audio recordings of the environment. Although the analyses
are intended for long-duration recordings (1-24 hours), in fact they can be
performed on any duration file provided the format is supported by the software.
Analysis Programs can:

-   calculate summary and spectral acoustic indices at variable resolutions

-   produce long-duration, false-colour, multi-index spectrograms

-   calculate critical statistics of annotations downloaded from an Acoustic
    Workbench

-   run various acoustic event recognisers

In the past, we have traditionally run this program over large datasets for
other scientists. They would send us their data, we use our compute
infrastructure to generate the results, and then we send the results back to the
scientist. The good news is we're still offering that service! Even though it
possible for anyone to run these analyses now, there are still challenges to
running large scale analyses. As an eScience group our goal is to scale
traditional science and we intend to keep on doing that.

## The nitty-gritty details

All the analyses are performed by a single executable
file, *AnalysisPrograms.exe*, which can be run on Windows, Linux, or MacOS. The
Mono runtime is used to run the program on non-Windows platforms.

We’ve also begun writing a manual for Analysis Programs. The manual can be found
here <https://github.com/QutEcoacoustics/audio-analysis/tree/master/docs>. It is
currently limited in scope, but we intend to expand it in the future. We welcome
any feedback or contributions you might have for the documentation.

It has been a long process to open source the code. An important factor was the
need to determine ownership and then remove intellectual property that we don’t
own. However, the most significant delay for us was the nature of the software:
this software is a critical tool for our workflows and is an active, changing,
and always in development project. We wanted to do more than satisfy the
bare-minimum journal requirements—more than just releasing a static,
non-functional copy of our code from three years ago. Releasing an active
product meant we needed to pay off our technical debt and that required changes,
clean-up, and investments into updated tooling. For those interested you can see
the work involved by viewing our tracking issue for the open sourcing of the
project: <https://github.com/QutEcoacoustics/audio-analysis/issues/140>.

To reflect the changing nature of the software, this repository has been setup
to produce a weekly release that will contain new functionality and new bug
fixes. We've also got a slew of performance improvements, new features, and bug
fixes in the backlog.

We’re keen to gather feedback. If you have any issues using our software you can
file an [issue](https://github.com/QutEcoacoustics/audio-analysis/issues) or
contact us on Twitter (either [@atruskie](https://twitter.com/atruskie) or
[@QUTEcoacoustics](https://twitter.com/QUTEcoacoustics)).
