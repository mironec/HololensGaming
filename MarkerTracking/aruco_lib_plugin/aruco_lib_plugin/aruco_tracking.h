#pragma once
#define API __declspec(dllexport)
//#include <opencv2/aruco.hpp>
#include <opencv2/imgproc.hpp>
#include "aruco.h"

typedef void(*PrintFunc)(const char *);

extern "C" {
	API void init(int _img_width, int _img_height);
	API int detect_markers(unsigned char *unity_img, int* out_ids_len, int** out_ids, float** out_corners, double** out_rvecs, double** out_tvecs);
	API void set_debug_cb(PrintFunc ptr);
	API void destroy();
}

