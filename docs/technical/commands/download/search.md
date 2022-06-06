---
title: Download Search
uid: command-download-search
---

# Download File

- **Command**: `download search`
- **Config file**: none (no config file required)

This command searches a repository for recordings.

You can use this command to see which recordings would be downloaded by the <xref:command-download-batch> command.

## Usage

```shell
$ AnalysisPrograms.exe download search [options]
```

Here is an example of a command line:

```shell
$ AnalysisPrograms.exe download search -s 1 -s 2 --start '2021-01-24' --end '2021-12-25' --repository="A2O" --auth-token "REDACTED"
```

![download search screenshot](~/images/download-search.png)

This command shows all the recordings from sites `1` and `2` which were recorded between `2021-01-24` and `2021-12-25`.

You'll need to log in using an authentication token. You can get one by logging in to the website and clicking on the "My Account" link.

## Options

```shell
Preview which files would be downloaded by the batch command

Usage: AnalysisPrograms.exe download search [options]

Options:
  -p|--project-ids <PROJECT_IDS>   Project IDs to filter recordings by
  -r|--region-ids <REGION_IDS>     Region IDs to filter recordings by
  -s|--site-ids <SITE_IDS>         Site IDs to filter recordings by
  --start <START>                  A date (inclusive) to filter out recordings. Can parse an ISO8601 date.
  --end <END>                      A date (exclusive) to filter out recordings. Can parse an ISO8601 date.
  -repo|--repository <REPOSITORY>  Which repository to use to download audio from
  -a|--auth-token <AUTH_TOKEN>     Your personal access token for the repository
  ...<global options omitted>...
```

- `-p|--project-ids <PROJECT_IDS>`: Project IDs to filter recordings by.
- `-r|--region-ids <REGION_IDS>`: Region IDs to filter recordings by.
- `-s|--site-ids <SITE_IDS>`: Site IDs to filter recordings by.
- `--start <START>`: A date (inclusive) to filter out recordings. Can parse an ISO8601 date.
- `--end <END>`: A date (exclusive) to filter out recordings. Can parse an ISO8601 date.
- `-repo|--repository <REPOSITORY>`: Which repository to use to download audio from. Either `A2O` or `Ecosounds`
- `-a|--auth-token <AUTH_TOKEN>`: Your personal access token for the repository.

The options `--repository` and `--auth-token` are required.

You can only choose one of `--project-ids`, `--region-ids`, or `--site-ids` per command.
But for each you specify the option multiple times. For example, to search multiple sites, you can do this:

```
... -p 123 -p 456 -p 789 ...
```

If you specify a start date (`--start`) then you must also include an end date.
