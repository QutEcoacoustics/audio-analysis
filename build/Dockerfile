FROM  debian:stretch-slim

ARG GIT_COMMIT
ARG AP_SOURCE="github"
ARG AP_VERSION="latest"

LABEL maintainer="Anthony Truskinger <a.truskinger@qut.edu.au>" \
      description="Debian environment for running AnalysisPrograms.exe" \
      version="1.0" \
      org.ecosounds.name="AnalysisPrograms.exe" \
      org.ecosounds.version=${AP_VERSION} \
      org.ecosounds.vendor="QUT Ecoacoustics" \
      org.ecosounds.url="https://github.com/QutEcoacoustics/audio-analysis" \
      org.ecosounds.vcs-url="https://github.com/QutEcoacoustics/audio-analysis" \
      org.ecosounds.vcs-ref=${GIT_COMMIT} \
      org.ecosounds.schema-version="1.0"



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

# Mono
RUN \
    # Add mono key server
    curl https://origin-download.mono-project.com/repo/xamarin.gpg | apt-key add - \
    # install mono
    echo "deb http://download.mono-project.com/repo/debian stable-stretch main" | tee /etc/apt/sources.list.d/mono-official-stable.list \
    && apt-get update \
    && apt-get install -y mono-complete \
    && rm -rf /var/lib/apt/lists/* /tmp/*

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
RUN /usr/bin/pwsh -NonInteractive -c "/download_ap.ps1 ${AP_SOURCE} -version ${AP_VERSION}"

ENTRYPOINT [ "/bin/bash" ]
