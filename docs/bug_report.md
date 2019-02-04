# Reporting a bug

If you think you have found a bug, please follow these steps:

## 1. Try and run the command again.

Often transient problems (like a bad network connection) can cause AP.exe to
crash. 

Make sure the output directory is empty. Sometimes when AP.exe crashes it leaves
behind files that can cause trouble.

## 2. Double check the arguments

Make sure the arguments, options, and config files you have supplied to AP.exe
are valid. Has something changed?

## 3. Try updating AP.exe

AP.exe is updated regularly. There is a good chance your problem might be solved
in a newer version. Try downloading the latest weekly version (
see [installing](./installing.md)  for instructions).

## 4. Check the documentation, and check for issues

Check the documentation on this site for any problems that may relate to the
problem you are experiencing.

Additionally, check for any open or closed issues in the AP.exe issue tracker:
<https://github.com/QutEcoacoustics/audio-analysis/issues?q=is:issue>.
These issues may contain the answers you need. If someone else has the same issue
you can report your problems in the same thread.

## 5. File a bug report

If all else fails, we'll open a new issue.

To prepare for this, one last time, try running your command again, **but** this
time run the command with very verbose logging enabled. Doing so will add much
more diagnostic information to the standard logging file that is produced by
AP.exe. For more information on the log file see the [logs](./logs.md) document.

To enable very verbose logging you should add the `--log-level 7` option to the
end of your command. When the command has completed collect the [log](./logs.md)
file.

Then open a [new issue](https://github.com/QutEcoacoustics/audio-analysis/issues/new/choose)
and fill out the form.

Happy bug hunting!
