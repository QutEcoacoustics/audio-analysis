# Formats

_AP.exe_ supports:

- WAVE (`.wav`)
    - any sample rate
    - integer PCM only
    - bit depths of 8, 16, 24, and 32
    - multiple channels
- MP3 (`.mp3`)
    - any sample rate
    - VBR or Fixed
    - **NOTE**: MP3 compression creates artefacts in the reconstituted audio file to which some indices (especially ACI) are highly sensitive. For this reason, we discourage the use of MP3 recordings.
- Ogg Vorbis (`.ogg`)    
- FLAC (`.flac`)
- Wave Pack (`.wv`)
- WebM (`.webm`)
- WMA (`.wma`)