# Scripts

_AnalysisPrograms.exe_ works best when processing single audio files.
This has a few advantages:

**It simplifies the code**. We don't need to know about the particular 
way you store your data, or the way you want your data processed.

**It allows for greater customiszation**. By building a set of composable
tools, it allows you to choose what analysis done, and when. You wan't it
all? You got it. You only want the first bit done once, and the second done
100 times with parameter sweeps? Go for it.

**It is easier to parallelize**. You want to use your university HPC?
Write your own distributed analysis? Or just run it in sequence? That's all
your choice.

Despite these advantages, we recognize it is useful to have example of
how large analyses might be completed. Thus the scripts you find in this
folder contain various examples (in various languages) of composing 
workflows with _AnalysisPrograms.exe_.

## Contributions appreciated!

If you right a script that gets the job done for you, we'd be happy to
include it here as an example for others to use.

## PowerShell

You'll see a lot of scripts in this folder that are written in PowerShell.
If you're not familar with it, you can consider it as the Windows equivalent
of the Bash shell. @atruskie like's PowerShell because in their personal
opinion, the syntax is more reasonable than Bash, and the enhanced support
for dates, times, and regular expressions are well worth the investment.

As of [PowerShell 6.0](https://github.com/PowerShell/PowerShell#-powershell)
the shell is cross platform and well worth investigating. If you're not
convinced, the scripts should be easy enough to reimplment in your favourite
language (like Bash)--and we would of course appreciate any translated
scripts sent back to us as contributed examples.

## Example headers

We'd like to see each example script prefaced with a documentation header.
A suggested format:

```
# A simple loop to XXX for a folder of files.
# Jolene Blogs 2017
#
# For each audio file found,
# - It runs does XXX
# - It then also does YYY
# - And sometimes does ZZZ
# - ...
#
# Assumptions:
# - AnalysisPrograms.exe is in current directory
# - ...

... script starts here ...
```