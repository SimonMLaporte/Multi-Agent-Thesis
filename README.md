# Thesis repo
This repos contains my work for my master thesis. All code is solely written by me.

Each folder contains the .gha file for each experiment. To install, add the file to your grasshopper "components" folder and restart rhino.  
Each folder contains a grasshopper script with the modules setup.  
Each folder contain an environment that must be downloaded and the filepath of the bitmap must refer to the corresponding bitmap.  
New environments can be created by using a 200x200 .bmp. Experiment 3 uses the lightness value per pixel for each agent type, while experiment 2 uses an RGB image with the value of each color representing attraction value.

Note that only experiment .gha file can be present in the components folder in grasshopper at a time as the components share guid.

Source code in c# as well as visual studio project is available in the "CellsThesis" subfolder.
The code is organized such that each file with the prefix "ghc" contains the modules within grasshopper, while the rest of the code contains classes and methods used in the code.

DEPENDENCIES:  
Squid for image file generation: https://www.food4rhino.com/en/app/squid  
Karamba3d for bridge fitness evaluation: https://www.karamba3d.com/ (Note payed liscense is required)
