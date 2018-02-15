# F# test fixtures

This folder contains the test fixtures needed for the F# tests.
Two of the directories have been zipped to save space.
All of the assets should be in git-lfs.

The `Common` module in `AED.Test` automatically unzips the two
folders when module loads. The content of these two folders should
not be checked in and are ignored by the `.gitignore` in this folder.