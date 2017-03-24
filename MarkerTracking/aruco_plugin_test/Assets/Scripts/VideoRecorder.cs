using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.VR.WSA.WebCam;
using System.Linq;

public class VideoRecorder : MonoBehaviour {

    VideoCapture videoCapture = null;

    public int width;
    public int height;
    public float fps;
    bool recordingStarted = false;
    // Use this for initialization
    void Start () {
        VideoCapture.CreateAsync(true, onVideoCaptureCreated);
        recordingStarted = true;
        Debug.Log("--- Trying to start recording!");
    }

    // Update is called once per frame
    void Update () {
		//if(Time.unscaledTime > 15 && !recordingStarted) {
            
  //          recordingStarted = true;
  //          Debug.Log("--- Trying to start recording!");
  //      }

        if(Time.unscaledTime > 30 && recordingStarted) {
            stopRecordingVideo();
        }
	}

    void onVideoCaptureCreated(VideoCapture _videoCapture) {
        if (videoCapture != null) {
            videoCapture = _videoCapture;
            
            CameraParameters cameraParameters = new CameraParameters();
            cameraParameters.hologramOpacity = 1.0f;
            cameraParameters.frameRate = fps;
            cameraParameters.cameraResolutionWidth = width;
            cameraParameters.cameraResolutionHeight = height;
            cameraParameters.pixelFormat = CapturePixelFormat.BGRA32;

            videoCapture.StartVideoModeAsync(cameraParameters,
                                                VideoCapture.AudioState.None,
                                                onStartedVideoCaptureMode);
        }
        else {
            Debug.LogError("Failed to create VideoCapture Instance!");
        }
    }

    void onStartedVideoCaptureMode(VideoCapture.VideoCaptureResult result) {
        if (result.success) {
            string filename = string.Format("MyVideo_{0}.mp4", Time.time);
            string filepath = System.IO.Path.Combine(Application.persistentDataPath, filename);

            videoCapture.StartRecordingAsync(filepath, onStartedRecordingVideo);
        }
    }

    void onStartedRecordingVideo(VideoCapture.VideoCaptureResult result) {
        Debug.Log("Started Recording Video!");
        // We will stop the video from recording via other input such as a timer or a tap, etc.
    }

    // The user has indicated to stop recording
    void stopRecordingVideo() {
        videoCapture.StopRecordingAsync(onStoppedRecordingVideo);
    }

    void onStoppedRecordingVideo(VideoCapture.VideoCaptureResult result) {
        Debug.Log("Stopped Recording Video!");
        videoCapture.StopVideoModeAsync(onStoppedVideoCaptureMode);
    }

    void onStoppedVideoCaptureMode(VideoCapture.VideoCaptureResult result) {
        videoCapture.Dispose();
        videoCapture = null;
    }
}
