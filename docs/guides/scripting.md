# Scripting AP.exe

_AnalysisPrograms.exe_ works best when processing single audio files.
This has a few advantages:

**It simplifies the code**. We don't need to know about the particular
way you store your data, or the way you want your data processed.

**It allows for greater customization**. By building a set of composable
tools, it allows you to choose what analysis is done, and when. You want it
all? You got it. You only want the first bit done once, and the second bit done
100 times with parameter sweeps? Go for it.

**It is easier to parallelize**. You want to use your university HPC?
Write your own distributed analysis? Or just run it in sequence? That's all
your choice.

Despite these advantages, we recognize it is useful to have example of
how large analyses might be completed. Thus the scripts you find in this
folder contain various examples (in various languages) of composing
workflows with _AnalysisPrograms.exe_.

You can find a collection of example scripts here: https://github.com/QutEcoacoustics/audio-analysis/tree/master/scripts

## PowerShell

You'll see a lot of scripts in that folder that are written in PowerShell.
If you're not familiar with it, you can consider it as the Windows equivalent
of the Bash shell.

We like PowerShell because we think the syntax is more reasonable than Bash
and the enhanced support for dates, times, and regular expressions are well worth the investment.

As of [PowerShell 6.0](https://github.com/PowerShell/PowerShell#-powershell)
the shell is cross platform! If you're not
convinced, the scripts should be easy enough to reimplement in your favourite
language (like Bash)--and we would of course appreciate any translated
scripts sent back to us as contributed examples.

## Any other language

You can use any programming language to script AP.exe.

R is a popular choice. We have a short guide for [using AP.exe with R](./using_r.md)
