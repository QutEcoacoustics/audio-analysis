---
title: Download Batch
uid: command-download-batch
---

# Download File

- **Command**: `download file`
- **Config file**: none (no config file required)

This command downloads one or more files from a remote Acoustic Workbench server.
You can use the <xref:command-download-search> command to preview which files would be downloaded by the batch command.


## Usage

```shell
$ AnalysisPrograms.exe  download batch [options]
```

Here is an example of a command line with abbreviated path names:

```shell
$ AnalysisPrograms.exe  download batch -s 1 -s 2 --start '2021-01-24' --end '2021-12-25' --repository="A2O" --auth-token "REDACTED" --output "D:\Temp\downloads"
```

![download batch screenshot](~/images/download-batch.png)

This command downloads all the recordings from sites `1` and `2` which were recorded between `2021-01-24` and `2021-12-25`.

You'll need to log in using an authentication token. You can get one by logging in to the website and clicking on the "My Account" link.

## Options

```shell
Downloads one or more files from a remote repository

Usage: AnalysisPrograms.exe download batch [options] <Ids>

Arguments:
  Ids                              One or more audio files to download

Options:
  -p|--project-ids <PROJECT_IDS>   Project IDs to filter recordings by
  -r|--region-ids <REGION_IDS>     Region IDs to filter recordings by
  -s|--site-ids <SITE_IDS>         Site IDs to filter recordings by
  --start <START>                  A date (inclusive) to filter out recordings. Can parse an ISO8601 date.
  --end <END>                      A date (exclusive) to filter out recordings. Can parse an ISO8601 date.
  -f|--flat                        If used will not place downloaded files into sub-folders
  -o|--output <OUTPUT>             A directory to write output to
  -repo|--repository <REPOSITORY>  Which repository to use to download audio from
  -a|--auth-token <AUTH_TOKEN>     Your personal access token for the repository
  ...<global options omitted>...
```

- `-p|--project-ids <PROJECT_IDS>`: Project IDs to filter recordings by.
- `-r|--region-ids <REGION_IDS>`: Region IDs to filter recordings by.
- `-s|--site-ids <SITE_IDS>`: Site IDs to filter recordings by.
- `--start <START>`: A date (inclusive) to filter out recordings. Can parse an ISO8601 date.
- `--end <END>`: A date (exclusive) to filter out recordings. Can parse an ISO8601 date.
- `-f|--flat`: If used will not place downloaded files into sub-folders. Normally recordings are split into sub-folders by their site name.
- `-o|--output <OUTPUT>`: A directory to put the downloaded audio recordings into.
- `-repo|--repository <REPOSITORY>`: Which repository to use to download audio from. Either `A2O` or `Ecosounds`
- `-a|--auth-token <AUTH_TOKEN>`: Your personal access token for the repository.

The options `--repo` and `--auth-token` are required.

You can only choose one of `--project-ids`, `--region-ids`, or `--site-ids` per command.
But for each you specify the option multiple times. For example, to search multiple sites, you can do this:

```
... -p 123 -p 456 -p 789 ...
```

If you specify a start date (`--start`) then you must also include an end date.
