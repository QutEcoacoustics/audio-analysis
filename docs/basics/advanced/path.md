---
title: Adding to PATH
uid: basics-path
---


## Adding AP.exe to `PATH`

The `PATH` variable is a system wide variable (known as en environment variable)
that any program can access. The `PATH` variable contains a list of folders that
your computer can check when it searches for programs to run.

When AP's folder is added to `PATH` you or any program on your computer can
run _AnalysisPrograms.exe_ without knowing where AP is actually installed.

So instead of needing this:

```powershell
> C:\Users\Anthony\AP\AnalysisPrograms.exe
```

You can instead write:

```powershell
> AnalysisPrograms.exe
```

### Setup using the installer script

The [automatic installer](../installing.md) will automatically add AP to `PATH` for you.

If you don't want this to happen use the `-DontAddToPath` switch when installing.

### Add to `PATH` manually

### [Windows](#tab/windows)

1. Find where AP is installed on your computer. This will be a directory (folder)
  where _AnalysisPrograms.exe_ resides.
2. Open your _System Environment Variables_. You can type some of
_Edit environment variables for your account_ in the Start Menu search box
to find the settings.
3. Choose _Environment Variables..._ in the windows that popped up
4. In the _user variables_ section, find the _Path_ variable, select it, and then hit the _Edit_ button
5. Add the directory from step 1 to the end
    - Ensure a semi-colon (`;`) delimits the new directory from the previous ones, if you're using an older version of Windows
6. Then click _OK_ or close all windows.
7. You will have to restart any programs for which you want to see the new value

### [Linux](#tab/linux)

1. Find where AP is installed on your computer. This will be a directory (folder)
  where _AnalysisPrograms.exe_ resides.
2. Open or create your `~/.profile` file
3. Add the following line to the end:

    ```bash
    PATH=$PATH:<REPLACE-ME>
    ```

   where you replace the `<REPLACE-ME>` with the directory from step 1.
4. Close and save the file
5. Run the same command in your current shell (`PATH=$PATH:<REPLACE-ME>`) to
  see the change take effect immediately (or restart your shell).

### [MacOSX](#tab/osx)

1. Find where AP is installed on your computer. This will be a directory (folder)
  where _AnalysisPrograms.exe_ resides.
2. Open or create your `~/.profile` file
3. Add the following line to the end:

    ```bash
    PATH=$PATH:<REPLACE-ME>
    ```

   where you replace the `<REPLACE-ME>` with the directory from step 1.
4. Close and save the file
5. Run the same command in your current shell (`PATH=$PATH:<REPLACE-ME>`) to
  see the change take effect immediately (or restart your shell).

***

## Aliasing _AnalysisPrograms.exe_ to _AP_

_AnalysisPrograms.exe_ is a long name. Tiring to type, prone to errors.
It also isn't a good name for a cross-platform program; on Linux and Mac OS it
is simply _AnalysisPrograms_.

To make it easier for people to experiment with and use _AP_ we aliased (gave
another name) to _AnalysisPrograms.exe_. We chose `AP`.

So instead of needing this:

```powershell
> C:\Users\Anthony\AP\AnalysisPrograms.exe
```

You can instead write (assuming _AnalysisPrograms.exe_ is on PATH):

```powershell
> AP
```

### Setup an alias using the installer script

If you have installed AP using the [automatic installer](../installing.md) then
this alias has already been set up for you!

### Setup an alias manually

This is advanced content.

1. Find the _AP_ installation directory
2. Create a symbolic link 
    - Windows: `<INSTALL-DIR>\AP.exe` and `<INSTALL-DIR>\_AnalysisPrograms.exe`
    - Linux/Mac: `<INSTALL-DIR>/AP` and `<INSTALL-DIR>/_AnalysisPrograms`

You'll need to ensure `<INSTALL-DIR>` in on `PATH`.
