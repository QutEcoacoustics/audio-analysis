

MQUTeR Data
=================
This folder contains original and converted audio for the MQUTeR Sensors Project. Please do not change or delete anything in this folder.


Contacts
-----------------
Paul Roe: p.roe@qut.edu.au


Folders
=================

AudioDataStorage 
-----------------
Contains the original, unmodified data. 
This directory should be backed up. 
Once audio files are placed in this folder, they should not be moved or modified.

 - Structure:
   Max 256 sub folders named using first 2 characters of a Globally Unique Identifier (GUID/UUID).
   File names are constructed from <audio reading id>_<yyMMdd-hhmm>.<extension from audio mime type>
   eg. 4fccfda1-c725-4699-99ea-0a4a64cafc42_110330-0006.wav
   
Converted
-----------------
Contains audio files converted and/or segmented from audio data stored in AudioDataStorage.
The files in this folder are temporary files.
They should not be backed up.
They can be deleted is necessary, and will be re-created when required.

 - Structure:
   Max 256 sub folders named using first 2 characters of a Globally Unique Identifier (GUID/UUID).
   File names are constructed from <audio reading id>_<yyMMdd-hhmm>_<relative start time in ms>_<relative end time in ms>.<extension from audio mime type>
   eg. 4fccfda1-c725-4699-99ea-0a4a64cafc42_110330-0006_86280000_86400000.wav / mp3