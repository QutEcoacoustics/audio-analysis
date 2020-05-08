# Versioning

## Obtaining the version of _AP.exe_

The software version number can be obtained by simply typing:

```
$ AnalysisPrograms.exe
```

The version number is also shown:

- whenever the program runs
- in the [log file](./logs.md) every time the program is run
- metadata of the file
- the filename of the release on GitHub
- the tag of the release on GitHub

## Interpreting the version

The output looks like:

```
QUT Ecoacoustics Analysis Programs - version 18.03.3.5 (DEBUG build, 2018-03-19 12:23)
Git branch-version: master-517b65bca92f1ed6ce3ea207a5660ff473222424-DIRTY-CI:000
Copyright QUT 2018
```

Our program uses an automatic version numbering system.
A version number such as `18.03.3.5` can be deciphered as:

&lt;2-digit-year&gt;.&lt;2-digit-month&gt;.&lt;number-of-releases-this-month&gt;.&lt;commits-since-last-release&gt;.

Thus, version `18.03.3.5` was created in 2018, in March, and is the third release
made that month, and there were five changes (commits) since the last release.

The Git branch information can be deciphered as:

&lt;git-branch-when-built&gt;-&lt;latest-commit-hash-when-built&gt;