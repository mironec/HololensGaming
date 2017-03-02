using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GeneralCameraProvider : CameraProvider {
    public int webcamDeviceNumber;
    public int webcamDesiredWidth;
    public int webcamDesiredHeight;
    public int webcamDesiredFPS;

    public bool useTestImg;
    public Texture2D testImg;

    public float focalX;
    public float focalY;
    public float centerX;
    public float centerY;

    public float[] distortion;

    private WebCamTexture webcamTexture;
    private Color32[] imgData;

    override public void init(out int _width, out int _height, out float[] _cam_params) {
            //Default values to signify failed init if that should happen
        _width = 0;
        _height = 0;
        if (useTestImg) {
            _width = testImg.width;
            _height = testImg.height;
            imgData = testImg.GetPixels32();
        }
        else {
            WebCamDevice[] devices = WebCamTexture.devices;
            if(devices.Length > 0) {
                webcamTexture = new WebCamTexture(devices[webcamDeviceNumber].name, webcamDesiredWidth, webcamDesiredHeight, webcamDesiredFPS);
                    //We have to play the webcam to get actual width/height information from it
                webcamTexture.Play();
                _width = webcamTexture.width;
                _height = webcamTexture.height;

                imgData = new Color32[_width * _height];
            }
            else {
                Debug.Log("No webcam found!");
            }
        }

        _cam_params = initCameraParams();
    }

    float[] initCameraParams() {
        float[]cameraParams = new float[4 + 5];

        cameraParams[0] = focalX;
        cameraParams[1] = focalY;
        cameraParams[2] = centerX;
        cameraParams[3] = centerY;

        if(distortion.Length != 5) {
            Debug.LogAssertion("Camera parameters expect 5 distorion values. Will continue with all set to 0 for now.");
        }
        else {
            for (int i = 0; i < 5; i++) {
                cameraParams[4 + i] = distortion[i];
            }
        }
        
        //if (hololens) {
        //    //Hololens camera parameters based on camera photos, which are 1408x792, at 48 horizontal FOV
        //    //cameraParams[0] = 1.6226756644523603e+03f;
        //    //cameraParams[1] = 1.6226756644523603e+03f;
        //    //cameraParams[0] = 1581.20988f;
        //    //cameraParams[1] = 1581.20988f;
        //    cameraParams[0] = 1006.2244747090887922651680044105f;
        //    cameraParams[1] = 1006.2244747090887922651680044105f;
        //    //cameraParams[0] = 
        //    //camera_params[2] = 6.2516688711209542e+02f;
        //    //camera_params[3] = 3.8018373700505418e+02f;
        //    //The opencv calibration program didn't emit useful values for some reason, but normally these are simply half of the image width/height
        //    //cameraParams[2] = 1408 / 2;
        //    //cameraParams[3] = 792 / 2;
        //    cameraParams[2] = 896 / 2;
        //    cameraParams[3] = 504 / 2;
        //    //1006.2244747090887922651680044105
        //    cameraParams[4] = -5.6781211352631726e-03f;
        //    cameraParams[5] = -1.1566538973188603e+00f;
        //    cameraParams[6] = -1.3849725342370161e-03f;
        //    cameraParams[7] = -3.9288657236615111e-03f;
        //    cameraParams[8] = 9.4499768251174778e+00f;
        //}
        //else {
        //    //Parameters for a Macbook 15" webcam from 1280x720 image
        //    cameraParams[0] = 1.0240612805194348e+03f;
        //    cameraParams[1] = 1.0240612805194348e+03f;
        //    cameraParams[2] = 1280 / 2;
        //    cameraParams[3] = 720 / 2;
        //    //camera_params[2] = 6.3218846628075391e+02f;
        //    //camera_params[3] = 3.6227541578720428e+02f;
        //    cameraParams[4] = 7.9272342555005190e-02f;
        //    cameraParams[5] = -1.7557543937376724e-01f;
        //    cameraParams[6] = 6.0915748810957840e-04f;
        //    cameraParams[7] = -2.9391344753009105e-03f;
        //    cameraParams[8] = 1.0650125708199540e-01f;
        //}
        return cameraParams;
    }

    override public Color32[] getImage() {
        if(!useTestImg) {
            webcamTexture.GetPixels32(imgData);
        }
        return imgData;
    }

    public Texture getTexture() {
        if(useTestImg) {
            return testImg;
        }
        else {
            return webcamTexture;
        }
    }
}
