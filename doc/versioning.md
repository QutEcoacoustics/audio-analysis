# Versioning

## Obtaining the version of _AP.exe_

The software version number can be obtained by simply typing:

```
$ AnalysisPrograms.exe
```

The version number is also embedded in the:

- whenever the program runs
- in the log file every time the program is run
- metadata of the file
- the filename of the release
- the tag of the release on GitHub

## Interpreting the version

The output looks like:

```
 QUT Bioacoustic Analysis Program - version 16.10.123.456 (DEBUG build, 1/10/2016 11:07 AM)

 Git branch-version: master-e2a88694390d39216bfab3a88a77d21f96be2f4a

 Copyright QUT 2016
```

Our program uses an automatic version numbering system. A version number such as 16.06.3430.0 can be deciphered as:

&lt;2-digit-year&gt;.&lt;2-digit-month&gt;.&lt;build-server-count&gt;.&lt;commits-since-last-release&gt;.

The Git branch information can be deciphered as:

&lt;git-branch-when-built&gt;-&lt;latest-commit-hash-when-built&gt;