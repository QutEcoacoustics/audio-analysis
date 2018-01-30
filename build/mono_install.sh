#!/bin/bash

# http://www.mono-project.com/download/#download-lin-debian
sudo apt-key adv --keyserver hkp://keyserver.ubuntu.com:80 --recv-keys 3FA7E0328081BFF6A14DA29AA6A19B38D3D831EF
echo "deb http://download.mono-project.com/repo/debian stretch main" | sudo tee /etc/apt/sources.list.d/mono-official.list
sudo apt-get update
DEBIAN_FRONTEND=noninteractive sudo apt-get install -y mono-complete --allow-unauthenticated