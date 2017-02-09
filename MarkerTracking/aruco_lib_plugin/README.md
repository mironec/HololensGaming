# Setup notes
The	`aruco` folder includes the current version of Aruco (2.0.16) - if this is outdated you can update it with a newer version, as long as there were no breaking changes in the API.
Build Aruco using cmake. The project should be generated into a folder called `builds`, such that the folder can be reached at `aruco/builds`. 
(If you decide to create a different folder structure, adjust the `aruco/builds` path in the Additional Library Directories in the Linker properties of the VS project to point to the different folder).
Build Aruco, which will produce a dll file and the corresponding .lib files for this project to read.
Then build this project, which will produce the plugin dll.