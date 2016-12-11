#include "apriltag.h"
#include "tag36h11.h"
extern "C" {
	apriltag_detector_t *detector;
	apriltag_family_t *tag_family;
	image_u8_t *grey_img;
	int img_width;
	int img_height;

	__declspec(dllexport) void init(int width, int height) {
		tag_family = tag36h11_create();
		detector = apriltag_detector_create();
		apriltag_detector_add_family(detector, tag_family);

		detector->quad_decimate = 1.0;
		detector->quad_sigma = 0.0;
		detector->nthreads = 1;
		detector->debug = 0;
		detector->refine_decode = 0;
		detector->refine_pose = 0;

		grey_img = image_u8_create(width, height);
		img_width = width;
		img_height = height;
	}

	__declspec(dllexport) void destroy() {
		apriltag_detector_destroy(detector);
		tag36h11_destroy(tag_family);
		image_u8_destroy(grey_img);
	}

	__declspec(dllexport) int find_tags(unsigned char* unity_img) {
		for (int y = 0; y < img_height; y++) {
			for (int x = 0; x < img_width; x++) {
				int offset = y * img_width * 4 + x * 4; //4 = bytes per pixel, assuming rgba format from unity here
				//int offset = y * width * 3 + x * 3;
				grey_img->buf[y*grey_img->stride + x] = (unity_img[offset] + unity_img[offset + 1] * 2 + unity_img[offset + 2]) / 4; //rgb averaging for greyscale
			}
		}

		zarray_t* detections = apriltag_detector_detect(detector, grey_img);
		
		int detection_count = zarray_size(detections);

		apriltag_detections_destroy(detections);
		
		return detection_count; //Hand back the detection count, simplest this way
	}
}