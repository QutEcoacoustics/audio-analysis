
# From http://www.mono-project.com/docs/getting-started/install/linux/
# http://software-development-toolbox.blogspot.com.au/2015/05/debian-jessie-install-mono-40.htmlvag

sudo apt-key adv --keyserver hkp://keyserver.ubuntu.com:80 --recv-keys 3FA7E0328081BFF6A14DA29AA6A19B38D3D831EF
echo "deb http://download.mono-project.com/repo/debian wheezy main" | sudo tee /etc/apt/sources.list.d/mono-xamarin.list

# libgdiplus (Debian 8.0 annd later)
echo "deb http://download.mono-project.com/repo/debian wheezy-libjpeg62-compat main" | sudo tee -a /etc/apt/sources.list.d/mono-xamarin.list

sudo apt-get update

DEBIAN_FRONTEND=noninteractive sudo apt-get install -y mono-complete