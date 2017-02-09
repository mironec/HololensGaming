# Setup notes

This project requires you to install the OpenCV-Hololens NuGet package to work correctly.
The package can be found here: [https://www.nuget.org/packages/OpenCV-Hololens/](https://www.nuget.org/packages/OpenCV-Hololens/) (github repository here: [https://github.com/sylvain-prevost/opencv-hololens](https://github.com/sylvain-prevost/opencv-hololens)).
To install, right click on the aruco_hololens_plugin project in the solution, select "Manage NuGet packages", then search for the OpenCV-Hololens package and install it.
Once installed, it should have been added in the `packages` folder in the solution, and an "includes" folder should have been added in the project folder. This folder should include the OpenCV headers, and is being referenced as a C++ include folder in the project.
After this, you should be able to build the project just fine. The output folder will contain the `aruco_plugin.dll` as well as all OpenCV dlls - not all of them are required by the plugin. Currently, only opencv_core, opencv_imgproc and opencv_aruco are required.