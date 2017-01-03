using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Runtime.InteropServices;
using System;

public class aruco_tracker : MonoBehaviour {
    // Original Video parameters
    public int deviceNumber;
    public RawImage overlay;

    private WebCamTexture _webcamTexture;
    private int cam_width;
    private int cam_height;
    

    [DllImport("aruco_plugin")]
    public static extern void init(int width, int height);

    [DllImport("aruco_plugin")]
    public static extern int detect_markers(IntPtr unity_img, ref int marker_count, ref IntPtr out_ids, ref IntPtr out_corners, ref IntPtr out_rvecs, ref IntPtr out_tvecs);

    [DllImport("aruco_plugin")]
    public static extern void set_debug_cb(IntPtr ptr);

    [DllImport("aruco_plugin")]
    public static extern void destroy();

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    public delegate void PrintDelegate(string str);
    
    private bool dll_inited = false;

	// Use this for initialization
	void Start () {
        WebCamDevice[] devices = WebCamTexture.devices;
        if (devices.Length > 0)
        {
            Debug.Log("Got devices");
            _webcamTexture = new WebCamTexture(devices[deviceNumber].name, 1280, 720, 30);
            // Play the video source
            _webcamTexture.Play();
            cam_width = _webcamTexture.width;
            cam_height = _webcamTexture.height;
            
            init(cam_width, cam_height);
            dll_inited = true;

            overlay.texture = _webcamTexture;

        }
        else
        {
            Debug.Log("No webcam found!");
        }

        PrintDelegate plugin_delegate = new PrintDelegate(PluginDebugLog);
        IntPtr delegate_ptr = Marshal.GetFunctionPointerForDelegate(plugin_delegate);
        set_debug_cb(delegate_ptr);
    }

    void OnDestroy()
    {
        if(dll_inited) destroy();
    }

    // Update is called once per frame
    void Update () {
		if(Input.GetKeyDown(KeyCode.Space))
        {
            // Send video capture to ARUCO controller
            Color32[] colors = _webcamTexture.GetPixels32();
            IntPtr imageHandle = getImageHandle(colors);

            int marker_count = 0;
            IntPtr out_ids = IntPtr.Zero;
            IntPtr out_corners = IntPtr.Zero;
            IntPtr out_rvecs = IntPtr.Zero;
            IntPtr out_tvecs = IntPtr.Zero;

            detect_markers(imageHandle, ref marker_count, ref out_ids, ref out_corners, ref out_rvecs, ref out_tvecs);

            Debug.Log("Markers found: " + marker_count);

            if(marker_count > 0)
            {
                int[] ids = new int[marker_count];
                Marshal.Copy(out_ids, ids, 0, marker_count);

                float[] corners = new float[marker_count * 8];
                Marshal.Copy(out_corners, corners, 0, marker_count * 8);

                double[] rvecs = new double[marker_count * 3];
                Marshal.Copy(out_rvecs, rvecs, 0, marker_count * 3);
                
                double[] tvecs = new double[marker_count * 3];
                Marshal.Copy(out_tvecs, tvecs, 0, marker_count * 3);

                for(int i=0; i<marker_count; i++)
                {
                    Debug.Log("id: " + ids[i]);
                    Debug.Log("Corners: ");
                    for(int j=0; j<4; j++)
                    {
                        Debug.Log("" + j + ": " + corners[i * 8 + j * 2] + ", " + corners[i * 8 + j * 2 + 1]);
                    }

                    Debug.Log("rvec: " + rvecs[i * 3] + ", " + rvecs[i * 3 + 1] + ", " + rvecs[i * 3 + 2]);
                    Debug.Log("tvec: " + tvecs[i * 3] + ", " + tvecs[i * 3 + 1] + ", " + tvecs[i * 3 + 2]);
                }
            }

            //var stopwatch = System.Diagnostics.Stopwatch.StartNew();
            //int markers = find_tags(imageHandle);
            //stopwatch.Stop();
            //Debug.Log(stopwatch.ElapsedMilliseconds);

            //Debug.Log(markers);
        }
	}

    private static IntPtr getImageHandle(object colors)
    {
        IntPtr ptr;
        GCHandle handle = default(GCHandle);
        try
        {
            handle = GCHandle.Alloc(colors, GCHandleType.Pinned);
            ptr = handle.AddrOfPinnedObject();
        }
        finally
        {
            if (handle != default(GCHandle))
                handle.Free();
        }
        return ptr;
    }

    static void PluginDebugLog(string str)
    {
        Debug.Log("Aruco Plugin: " + str);
    }
}
