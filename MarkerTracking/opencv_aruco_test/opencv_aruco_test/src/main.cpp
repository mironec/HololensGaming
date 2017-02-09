/**
* @file videocapture_starter.cpp
* @brief A starter sample for using OpenCV VideoCapture with capture devices, video files or image sequences
* easy as CV_PI right?
*
*  Created on: Nov 23, 2010
*      Author: Ethan Rublee
*
*  Modified on: April 17, 2013
*      Author: Kevin Hughes
*/

#include <opencv2/imgcodecs.hpp>
#include <opencv2/videoio.hpp>
#include <opencv2/highgui.hpp>
#include <opencv2/aruco.hpp>

#include <iostream>
#include <stdio.h>

using namespace cv;
using namespace std;

void getCameraParameters(Mat &camera_matrix, Mat &dist_coeffs) {
	camera_matrix.create(3, 3, CV_64F);
	camera_matrix.at<double>(0, 0) = 1.0240612805194348e+03;
	camera_matrix.at<double>(0, 1) = 0.0;
	camera_matrix.at<double>(0, 2) = 6.3218846628075391e+02;
	camera_matrix.at<double>(1, 0) = 0.0;
	camera_matrix.at<double>(1, 1) = 1.0240612805194348e+03;
	camera_matrix.at<double>(1, 2) = 3.6227541578720428e+02;
	camera_matrix.at<double>(2, 0) = 0.0;
	camera_matrix.at<double>(2, 1) = 0.0;
	camera_matrix.at<double>(2, 2) = 1.0;

	dist_coeffs.create(5, 1, CV_64F);
	dist_coeffs.at<double>(0, 0) = 7.9272342555005190e-02;
	dist_coeffs.at<double>(0, 1) = -1.7557543937376724e-01;
	dist_coeffs.at<double>(0, 2) = 6.0915748810957840e-04;
	dist_coeffs.at<double>(0, 3) = -2.9391344753009105e-03;
	dist_coeffs.at<double>(0, 4) = 1.0650125708199540e-01;
}

int main(int ac, char** av) {
	//VideoCapture capture(0); //try to open string, this will attempt to open it as a video file or image sequence
	VideoCapture capture("test_vid.mp4"); //try to open string, this will attempt to open it as a video file or image sequence
	capture.set(CV_CAP_PROP_FRAME_WIDTH, 1280/2);
	capture.set(CV_CAP_PROP_FRAME_HEIGHT, 720/2);
	string window_name = "video | q or esc to quit";
	cout << "press space to save a picture. q or esc to quit" << endl;
	namedWindow(window_name, WINDOW_KEEPRATIO); //resizable window;
	Mat frame;
	Ptr<aruco::Dictionary> dictionary = Ptr<aruco::Dictionary> (aruco::getPredefinedDictionary(aruco::DICT_ARUCO_ORIGINAL));

	Mat cameraMatrix, distCoeffs; // camera parameters are read from somewhere 
	getCameraParameters(cameraMatrix, distCoeffs);


	int frame_count = 0;

	for (;;) {
		capture >> frame;
		if (frame.empty())
			break;

		vector<int> ids;
		vector<vector<Point2f>> corners;
		aruco::detectMarkers(frame, dictionary, corners, ids);

		cout << "frame ";
		cout << frame_count;
		cout << ": ";
		frame_count++;

		if (ids.size() > 0) {
			aruco::drawDetectedMarkers(frame, corners, ids);

			vector<Vec3d> rvecs, tvecs;
			aruco::estimatePoseSingleMarkers(corners, 0.088, cameraMatrix, distCoeffs, rvecs, tvecs); 
			// draw axis for each marker 
			for (int i = 0; i < ids.size(); i++) {
				aruco::drawAxis(frame, cameraMatrix, distCoeffs, rvecs[i], tvecs[i], 0.044);

				cout << "Txyz=";
				for (int j = 0; j < 3; j++)
					cout << tvecs[i](j) << " ";

				for(int j=0; j<3; j++) 
					cout << rvecs[i](j) << " ";

				
					//This is to inspect the translation vectors and validate calculated distance from camera compared to actual distance.`
				//Vec3d tvec = tvecs[i];
				//double len = sqrt(tvec(0) * tvec(0) + tvec(1) * tvec(1) + tvec(2) * tvec(2));
				//cout << tvec << " " << len << endl;
				
			}
			
		}
		cout << "\n";

		imshow(window_name, frame);
		char key = (char)waitKey(30); //delay N millis, usually long enough to display and capture input

		switch (key) {
		case 'q':
		case 'Q':
		case 27: //escape key
			return 0;
		default:
			break;
		}
	}
	return 0;
}
