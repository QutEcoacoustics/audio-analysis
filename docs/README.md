# AP.exe docs

We use a tool called [DocFX](https://dotnet.github.io/docfx/tutorial/docfx_getting_started.html) to generate these docs.

You can use this tool locally to see what your documentation looks like.

To install:

Ensure you have [Chocolatey installed](https://chocolatey.org/install). Then:

```powershell
choco install docfx -y
```

To generate the docs:

```powershell
../build/generate_docs.ps1
```

Notes:

- If there are any errors or warnings they need to be fixed before your changes are committed.
- You **must rebuild** after changes to see the updated preview

To preview (from audio-analysis repo root), run the _serve_ command in a separate terminal:

```powershell
cd _site
docfx serve
```

Then visit the url in your browser, typically <http://localhost:8080>.

## Layout

The documentation is laid out into several areas:

- **basics**: include introductory topics, like downloading, installing, and general bit of information
- **theory**: is reserved for pages discussing theory like:
    - how audio algorithms work
    - how noise removal works
    - what the indices are
    - how indices are calculated
    - which event detection algorithms we have
- **guides**: short form workflows
    - if I have audio and I want a spectrogram I do ...
    - if I have audio and I want a FCS I do ...
    - if I have indices and I want FCS I do...
    - if I have segmented FCS/indices and I want them joined i do...
- **tutorials**: Reserved for detailed lessons
- **FAQ**: as you expect, duplicated in basics
- **Articles**: news/blog posts etc
- **Documentation**: is the _technical_ folder and hides anything that is too technical for general users

## Contributing guidelines

- the `docfx` build must produce no errors or warnings
- do not duplicate content
    - use cross references to refer to content in other parts of the site
    - if something is common you can reactor it out into it's own fragment and
      include the result in both places
- cross reference things as much as possible
    - the target document should have a `uid` entry at the top of the file (looks for other examples)
    - you can use `<xref:some-uid>` to reference the target
- use `docfx template export --all -o _exported` to see which internal templates affect the docs layout

## Publish docs

Use `../build/generate_docs.ps1` and then `../build/publish_docs.ps1.`

Note the `NETLIFY_AUTH_TOKEN` environment variable must be defined.

This file is not published with the docs.