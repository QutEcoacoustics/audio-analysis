# Introduction
The matching pursuit package is implemented in MATLAB. To use the package, download MPTK-binary-0.6.1-x86_64-Windows.exe from [InriaForge](https://gforge.inria.fr/frs/?group_id=36).
The version used here is 0.6.1 2013-04-26 under Windows operation system

# Install the package
Double click the .exe file, follow the instructions to install the MPTK package.

# Configure the path
* Open the environment variable configuration panel: Start -> Config panel -> System -> Advanced -> Environment variables
* Add a new user variable with Name: MPTK_CONFIG_FILENAME  Value: path_to_MPTK/mptk/path.xml
* Make sure the installed MPTK folder is added into current working directory in MATLAB

# Create dictionary for matching pursuit algorithm
In MATLAB console, type dictcreate_gui to call the graphical user interface for dictionary creation. 
Save the dictionary in the folder ".../MPTK/mptk/reference/dictionary/"

# Examples
MP_features.m and multiscale_MP_features.m are two similar examples that use customised dictionaries.