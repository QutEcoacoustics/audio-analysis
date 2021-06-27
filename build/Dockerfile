# syntax=docker/dockerfile:1.2

FROM  debian:stretch-slim

ARG GIT_COMMIT=""
ARG AP_VERSION=""
ARG CREATION_DATE=""

LABEL \
    info.ecoacoustics.qut.ap.created=${CREATION_DATE} \
    info.ecoacoustics.qut.ap.authors="Anthony Truskinger <a.truskinger@qut.edu.au>" \
    info.ecoacoustics.qut.ap.url="https://github.com/QutEcoacoustics/audio-analysis" \
    info.ecoacoustics.qut.ap.documentation="https://ap.qut.ecoacoustics.info/" \
    info.ecoacoustics.qut.ap.source="https://github.com/QutEcoacoustics/audio-analysis/blob/master/build/Dockerfile" \
    info.ecoacoustics.qut.ap.version="${AP_VERSION}" \
    info.ecoacoustics.qut.ap.revision=${GIT_COMMIT} \
    info.ecoacoustics.qut.ap.vendor="QUT Ecoacoustics" \
    info.ecoacoustics.qut.ap.licenses="Apache-2.0" \
    info.ecoacoustics.qut.ap.title="AnalysisPrograms.exe" \
    info.ecoacoustics.qut.ap.description="Debian environment for running AnalysisPrograms.exe"

# Install system components (used by powershell. and AP as well)
RUN apt-get update && apt-get install -y curl gnupg apt-transport-https unzip \
    readline-common  software-properties-common \
    wavpack libsox-fmt-all sox shntool libav-tools ffmpeg \
    # link ffmpeg to /usr/bin/local
    && ln -s /usr/bin/ffmpeg /usr/local/bin/ffmpeg \
    && ln -s /usr/bin/ffprobe /usr/local/bin/ffprobe

# install mp3splt
RUN add-apt-repository "deb http://mp3splt.sourceforge.net/repository unstable main" \
    && apt-get update --allow-unauthenticated \
    && apt-get -y --allow-unauthenticated install libmp3splt0-mp3 libmp3splt0-ogg libmp3splt0-flac libmp3splt-doc libmp3splt-dev mp3splt mp3splt-gtk


# Powershell
RUN \
    # Import the public repository GPG keys
    curl https://packages.microsoft.com/keys/microsoft.asc | apt-key add - \
    # Register the Microsoft Product feed
    && echo "deb [arch=amd64] https://packages.microsoft.com/repos/microsoft-debian-stretch-prod stretch main" > /etc/apt/sources.list.d/microsoft.list \
    # Update the list of products
    && apt-get update \
    # Install PowerShell
    && apt-get install -y --no-install-recommends powershell

# Install AP.exe
ADD download_ap.ps1 /download_ap.ps1
RUN --mount=type=secret,id=GITHUB_AUTH_TOKEN  \
    /usr/bin/pwsh -NonInteractive -c \
    '/download_ap.ps1 -Install -version "${AP_VERSION}" -GithubApiToken "$(Get-Content -Raw /run/secrets/GITHUB_AUTH_TOKEN)"'

ENTRYPOINT [ "/bin/bash" ]
