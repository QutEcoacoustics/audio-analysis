---
title: Acoustic Events
uid: theory-acoustic-events
---

# Acoustic Events

An *acoustic event* is defined as an interval of acoustic energy, above background noise level, emitted from a single source.

For recognition purposes, it can also be defined as a contiguous set of spectrogram cells/pixels whose decibel values
exceed some user defined threshold.

In the ideal case, an acoustic event should encompass a discrete component of acoustic energy within a call, syllable
or harmonic. It will be separated from other acoustic events by intervening pixels having decibel values *below* the user defined threshold.

![Seven Kinds Of Acoustic Event](../images/SevenKindsAcousticEvent.jpg)

## Simple Events

### Shrieks

This is a diffuse acoustic event that is extended in both time and frequency. While a shriek may have some internal
structure, it is often treated by as a "blob" of acoustic energy. A typical example is a parrot shriek.

### Whistles

This is a narrow band, "pure" tone having duration over several to many time frames but having very restricted bandwidth.
In theory a pure tone occupies a single frequency bin, but in practice bird whistles can occupy several frequency bins
and appear as a horizontal *spectral track* in the spectrogram.

### Chirps

This sounds like a whistle whose frequency increases or decreases over time. A chirp is said to be a *frequency modulated*
tone. It appears in the spectrogram as a gently ascending or descending *spectral track*.

### Whips

A *whip* is like a *chirp* except that the frequency modulation can be extremely rapid so that it sounds like a
"whip crack". It has the appearance of a steeply ascending or descending *spectral track* in the spectrogram.
An archetypal whip is the final component in the whistle-whip of the Australian whip-bird.

In _AP_, the distinction between a chirp and a whip is not sharp. That is, a *spectral track* that is ascending
diagonally (cell-wise) at 45 degrees in the spectrogram will be detected by both the *chirp* and the *whip* algorithms.

### Clicks

The *click* appears as a single vertical line in a spectrogram and sounds, like the name suggests, as a very brief click.
In practice, depending on spectrogram configuration settings, a *click* may occupy two or more adjacent time-frames.

Note that each of the above five acoustic events are "simple" events. The remaining two kinds of acoustic event are said
to be composite, that is, they are composed of more than one acoustic event but the detection algorithm is designed to
pick them up as a single event.

## Complex Events

### Oscillations

An oscillation is the same (or nearly the same) syllable (typically whips or clicks) repeated at a fixed periodicity over
several to many time-frames.

### Harmonics

Harmonics are the same/similar shaped *whistle* or *chirp* repeated simultaneously at multiple intervals of frequency.
Typically, the frequency intervals are similar as one ascends the stack of harmonics.
