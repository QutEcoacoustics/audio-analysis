---
uid: basics-installing
---
# Installing

If you're new to using _AP.exe_ we recommend following the instructions
in the <xref:tutorial-01> practical.

You can choose to use our automated installer script or do a [manual install](./advanced/manual_install.md)

## Supported Platforms

- Any of the following platforms:
    - Windows 7 or newer
    - Mac OSX 10.9 or later
    - Any Debian based linux
    - Any Debian based linux container

- As well as the following CPu architectures
    - Intel x86 (64-bit only)
    - ARM (32 bit and 64-bit)

## Automatic install

The automatic install will download AP.exe and may install required perquisites.

1. Prerequisite: install Powershell 7+
    - Go to <https://docs.microsoft.com/en-us/powershell/scripting/install/installing-powershell>
      to find instructions for installing PowerShell
2. Then:

<!-- https://git.io/JtOo3 created with git.io and should point to-->
<!-- https://raw.githubusercontent.com/QutEcoacoustics/audio-analysis/master/build/download_ap.ps1 -->

### [Windows install](#tab/windows)

Run the following command in an elevated (_Run as Administrator_) prompt:

```powershell
 pwsh -nop -ex B -c '$function:i=irm "https://git.io/JtOo3";i'
```

Or, to install the prerelease version:

```powershell
 pwsh -nop -ex B -c '$function:i=irm "https://git.io/JtOo3";i -Pre'
```

### [Linux install](#tab/linux)

Run the following command:

```bash
 pwsh -nop -c '$function:i=irm "https://git.io/JtOo3";i'
```

Or, to install the prerelease version:

```bash
 pwsh -nop -c '$function:i=irm "https://git.io/JtOo3";i -Pre'
```

### [MacOSX install](#tab/osx)

Run the following command in _Terminal_:

```bash
 pwsh -nop -c '$function:i=irm "https://git.io/JtOo3";i'
```

Or, to install the prerelease version:

```bash
 pwsh -nop -c '$function:i=irm "https://git.io/JtOo3";i -Pre'
```

***

<br/>

> [!NOTE]
> Please inspect <https://git.io/JtOo3> which should point to
> <https://github.com/QutEcoacoustics/audio-analysis/blob/master/build/download_ap.ps1>
> prior to running these commands.
>
> We already know the script is safe, but you should verify
> the security and contents of any script from the internet you are not familiar
> with. The above command downloads a remote PowerShell script and executes it on
> your machine.

> [!WARNING]
> The installer script is brand new. There may be bugs. No warranties provided.

3. The script should install or upgrade AP.exe.
    - If it is upgrading it will ask you if you want to overwrite the old installation.
        - Choose _yes_ unless you have NOT stored data files or config files in the AP folder
        - Choose _no_ if you have stored data files or config files in the AP folder. 
          Copy out your files and then try installing again.
    ![AP installer screenshot](../images/installer_screenshot.png)
4. If everything went well AP should be ready to go. Try running a command:

    ```bash
    AP --version
    ```

> [!TIP]
> _AP_ is an alias for_AnalysisPrograms.exe_ that we made to make AP easier to use.
>
> The alias does exactly the same thing as calling _AnalysisPrograms.exe_ by its full name.
> To learn more about how it works see <xref:basics-path>.


## Uninstall

If you used our automatic install you can use the same script to uninstall:

### [Windows uninstall](#tab/windows)

Run the following command in an elevated (_Run as Administrator_) prompt:

```powershell
 pwsh -nop -ex B -c '$function:i=irm "https://git.io/JtOo3";i -Un'
```

### [Linux uninstall](#tab/linux)

Run the following command:

```bash
 pwsh -nop -c '$function:i=irm "https://git.io/JtOo3";i  -Un'
```

### [MacOSX uninstall](#tab/osx)

Run the following command in _Terminal_:

```bash
 pwsh -nop -c '$function:i=irm "https://git.io/JtOo3";i  -Un'
```
