---
title: Download File
uid: command-download-file
---

# Download File

- **Command**: `download file`
- **Config file**: none (no config file required)

This command downloads one or more files from a remote Acoustic Workbench server.

The files to download are identified by their unique identifiers.
You can find the ID for any recording by looking at the URL of the recording,
or by looking on the details page of a recording.

## Usage

```shell
$ AnalysisPrograms.exe download [file_ids...] [options]
```

Here is an example of a command line:

```shell
$ AnalysisPrograms.exe download file 1 2 3 4 5 7 --repository="A2O" --auth-token "REDACTED" --output "D:\Temp\download"
```

![download files screenshot](~/images/download-files.png)

This command will download the files with the IDs `1`, `2`, `3`, `4`, `5`, and `7`.

You'll need to log in using an authentication token. You can get one by logging in to the website and clicking on the "My Account" link.

## Options

```shell
Download a single file from a remote repository

Usage: AnalysisPrograms.exe download file [options] <Ids>

Arguments:
  Ids                              One or more audio files to download

Options:
  -f|--flat                        If used will not place downloaded files into sub-folders
  -o|--output <OUTPUT>             A directory to write output to
  -repo|--repository <REPOSITORY>  Which repository to use to download audio from
  -a|--auth-token <AUTH_TOKEN>     Your personal access token for the repository
  ...<global options omitted>...
```

- `-f|--flat`: If used will not place downloaded files into sub-folders. Normally recordings are split into sub-folders by their site name.
- `-o|--output <OUTPUT>`: A directory to put the downloaded audio recordings into.
- `-repo|--repository <REPOSITORY>`: Which repository to use to download audio from. Either `A2O` or `Ecosounds`
- `-a|--auth-token <AUTH_TOKEN>`: Your personal access token for the repository.

All options except for `--flat` are required.