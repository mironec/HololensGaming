#include "aruco_tracking.h"

using namespace std;
using namespace aruco;
using namespace cv;

extern "C" {
	void getCameraParameters(CameraParameters *params, int width, int height); //Forward-declaring this function here, doesn't seem to work in the header..

	PrintFunc debug;

	int img_width, img_height;

		//Using pointers everywhere may get messy, making a class to hold all of this and just having a pointer to one instance at the global level may be useful in the future

	CameraParameters *params;
	MarkerDetector *detector;
	vector<Marker> *markers;

	vector<int> *ids;
	vector<vector<Point2f>> *corners;
	vector<float> *corners_flat;

		//Rotation and translation vectors from pose estimation
	vector<Vec3d> *tvecs;
	vector<double> *tvecs_flat;
	vector<Vec3d> *rvecs;
	vector<double> *rvecs_flat;


	void init(int _img_width, int _img_height) {
		img_width = _img_width;
		img_height = _img_height;

		detector = new MarkerDetector();
		detector->setDictionary(Dictionary::getTypeString(Dictionary::ARUCO));

		detector->setThresholdParams(7, 7);
		detector->setThresholdParamRange(2, 0);
		//detector->setParams(MarkerDetector::Params());
		markers = new vector<Marker>();

		ids = new vector<int>();
		corners_flat = new vector<float>();
		tvecs_flat = new vector<double>();
		rvecs_flat = new vector<double>();

		params = new CameraParameters();

			//Init camera parameters
		getCameraParameters(params, img_width, img_height);
		//params->resize(Size(img_width, img_height));
	}

	int detect_markers(unsigned char *unity_img, int* out_ids_len, int** out_ids, float** out_corners, double** out_rvecs, double** out_tvecs) { //pointer to array (int*) of ids, pointer to variable that we'll write array length into
		Mat img = Mat(img_height, img_width, CV_8UC4, unity_img, img_width * 4);

		Mat rgb;

		cvtColor(img, rgb, CV_RGBA2RGB);

		//flip(gray, flipped, 1); //Flip vertically
		
		//detector->detect(gray, *markers, *params, 0.088);

		vector<Marker> local_markers = detector->detect(rgb, *params, 0.088f);//, *params, 0.088);

		debug(to_string((int)local_markers.size()).c_str());

		//write address of elem 0 in ids vector to out_ids (*out_ids = address, since out_ids is just our copy of the c# variable address), write array length to *out_ids_len.
		//Also, ids has to be allocated non-local, otherwise it goes out of scope after this function ends..

		int marker_count = markers->size();
		*out_ids_len = marker_count;
		*out_ids = NULL;
		*out_corners = NULL;
		if (*out_ids_len > 0) {

			
			ids->resize(marker_count);
			corners_flat->resize(marker_count * 8); //For each marker, we have 4 corner points, each of which are 2 floats. So we need markers * 8 floats overall;
			rvecs_flat->resize(marker_count * 3); // 1 rvec per marker, 3 doubles per Vec3d
			tvecs_flat->resize(marker_count * 3); // Same as for rvecs
			for (int i = 0; i < marker_count; i++) {

				(*ids)[i] = (*markers)[i].id;
				
				//	//corners has an array of 4 `Point2f`s for each marker. Since they're continuous, and each Point2f is just 2 floats, we can copy 4 points into 8 floats in our flat array
				//memcpy(corners_flat->data() + (i * 8), (*corners)[i].data(), 4 * sizeof(Point2f)); 

				//	//Copy over rvec and tvec doubles into corresponding flat arrays
				//for (int j = 0; j < 3; j++) {
				//	(*rvecs_flat)[i * 3 + j] = (*rvecs)[i][j];
				//	(*tvecs_flat)[i * 3 + j] = (*tvecs)[i][j];
				//}

			}

			*out_ids = ids->data();
			*out_corners = corners_flat->data();
			*out_rvecs = rvecs_flat->data();
			*out_tvecs = tvecs_flat->data();
		}

		int result = ids->size();
		
		return result;
	}

	void getCameraParameters(CameraParameters *params, int width, int height) {
		Mat cam_matrix = Mat(3, 3, CV_64F);
		cam_matrix.at<double>(0, 0) = 1.0240612805194348e+03;
		cam_matrix.at<double>(0, 1) = 0.0;
		cam_matrix.at<double>(0, 2) = 6.3218846628075391e+02;
		cam_matrix.at<double>(1, 0) = 0.0;
		cam_matrix.at<double>(1, 1) = 1.0240612805194348e+03;
		cam_matrix.at<double>(1, 2) = 3.6227541578720428e+02;
		cam_matrix.at<double>(2, 0) = 0.0;
		cam_matrix.at<double>(2, 1) = 0.0;
		cam_matrix.at<double>(2, 2) = 1.0;

		Mat dist_coeffs = Mat(5, 1, CV_64F);
		dist_coeffs.at<double>(0, 0) = 7.9272342555005190e-02;
		dist_coeffs.at<double>(0, 1) = -1.7557543937376724e-01;
		dist_coeffs.at<double>(0, 2) = 6.0915748810957840e-04;
		dist_coeffs.at<double>(0, 3) = -2.9391344753009105e-03;
		dist_coeffs.at<double>(0, 4) = 1.0650125708199540e-01;

		params->setParams(cam_matrix, dist_coeffs, Size(width, height));
	}

	void destroy() {
		delete ids;
		delete corners;
		delete corners_flat;
	}

	void set_debug_cb(PrintFunc ptr) {
		debug = ptr;
	}
}