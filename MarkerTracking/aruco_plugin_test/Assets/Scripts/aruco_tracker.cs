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
    public Camera cam;
    public Texture2D test_img;

    public bool apply_rotation = true;

    public GameObject marker_quad_prefab;
    public GameObject id_text_prefab;

    private List<GameObject> quad_instances;
    private List<GameObject> id_instances;

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

    private Vector2 resolution;
    private Vector2 cparams;

    private Matrix4x4 transformMatrix = new Matrix4x4();

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
            
            //init(cam_width, cam_height);
            //dll_inited = true;

            overlay.texture = _webcamTexture;

        }
        else
        {
            Debug.Log("No webcam found!");
        }

        init(1280, 720);
        dll_inited = true;

        PrintDelegate plugin_delegate = new PrintDelegate(PluginDebugLog);
        IntPtr delegate_ptr = Marshal.GetFunctionPointerForDelegate(plugin_delegate);
        set_debug_cb(delegate_ptr);

        quad_instances = new List<GameObject>();
        id_instances = new List<GameObject>();

        //todo: fix the duplicate of cam_width/cam_height and resolution
        resolution = new Vector2(1280, 720);
        cparams = new Vector2(6.3218846628075391e+02f, 3.6227541578720428e+02f); //Extracted from the camera matrix used in the plugin

        //This and the code in assigning the marker object a new position taken from this stackoverflow answer:
        // http://stackoverflow.com/questions/36561593/opencv-rotation-rodrigues-and-translation-vectors-for-positioning-3d-object-in
        float focal_y = 1.0240612805194348e+03f;
        float vfov = 2.0f * Mathf.Atan(0.5f * resolution.y / focal_y) * Mathf.Rad2Deg;
        cam.fieldOfView = vfov;
        cam.aspect = resolution.x / resolution.y;

    }

    GameObject make_marker_quad()
    {
        GameObject quad = GameObject.Instantiate(marker_quad_prefab);
        quad.transform.localScale = new Vector3(0.088f, 0.088f, 0.088f); //Matches the real world marker scale, 1 unit = 1m
        if(!apply_rotation)
        {
            quad.transform.localScale = new Vector3(0.02f, 0.005f, 0.01f);
        }
        return quad;
    }

    void OnDestroy()
    {
        if(dll_inited) destroy();
    }

    // Update is called once per frame
    void Update () {
        // Send video capture to ARUCO controller
        Color32[] colors = _webcamTexture.GetPixels32();
        //Color32[] colors = test_img.GetPixels32();
        IntPtr imageHandle = getImageHandle(colors);

        int marker_count = 0;
        IntPtr out_ids = IntPtr.Zero;
        IntPtr out_corners = IntPtr.Zero;
        IntPtr out_rvecs = IntPtr.Zero;
        IntPtr out_tvecs = IntPtr.Zero;

        detect_markers(imageHandle, ref marker_count, ref out_ids, ref out_corners, ref out_rvecs, ref out_tvecs);
            
        //Debug.Log("Markers found: " + marker_count);

            //Add/remove quads to match how many we saw
        if (quad_instances.Count > marker_count)
        {
            //Clear out any instances we don't need anymore
            for (int i = quad_instances.Count - 1; i >= marker_count; i--)
            {
                GameObject.Destroy(quad_instances[i]);
                quad_instances.RemoveAt(i);
                GameObject.Destroy(id_instances[i]);
                id_instances.RemoveAt(i);
            }
        }
        else if (marker_count > quad_instances.Count)
        {
            int to_add = marker_count - quad_instances.Count;
            for (int i = 0; i < to_add; i++)
            {
                quad_instances.Add(make_marker_quad());
                GameObject id_text = GameObject.Instantiate(id_text_prefab);
                id_instances.Add(id_text);
            }
        }

        if (marker_count > 0)
        {
                //Copy over data from plugin side to c# managed arrays
            int[] ids = new int[marker_count];
            Marshal.Copy(out_ids, ids, 0, marker_count);

            float[] corners = new float[marker_count * 8];
            Marshal.Copy(out_corners, corners, 0, marker_count * 8);

            double[] rvecs = new double[marker_count * 3];
            Marshal.Copy(out_rvecs, rvecs, 0, marker_count * 3);
                
            double[] tvecs = new double[marker_count * 3];
            Marshal.Copy(out_tvecs, tvecs, 0, marker_count * 3);

                
            Vector3 image_center = new Vector3(0.5f, 0.5f, 0);
            Vector3 optical_center = new Vector3(0.5f + cparams.x / resolution.x, 0.5f + cparams.y / resolution.y, 0);

            Debug.Log("Found id: " + ids[0]);


            //Print out all information on each marker, for debugging
            for (int i = 0; i < marker_count; i++)
            {
                Vector3 tvec = new Vector3((float)tvecs[i * 3], (float)tvecs[i * 3 + 1], (float)tvecs[i * 3 + 2]);
                image_center.z = tvec.z;
                optical_center.z = tvec.z;
                //tvec += cam.ViewportToWorldPoint(image_center) - cam.ViewportToWorldPoint(optical_center);
                quad_instances[i].transform.position = tvec;

                if (apply_rotation)
                {
                    Vector3 rvec = new Vector3((float)rvecs[i * 3], (float)rvecs[i * 3 + 1], (float)rvecs[i * 3 + 2]);
                    transformMatrix = makePointTransform(tvec, rvec);

                    Vector4 up = transformMatrix.GetColumn(1);
                    Vector4 forward = transformMatrix.GetColumn(2);

                    Quaternion look = Quaternion.LookRotation(new Vector3(forward.x, -forward.y, forward.z), new Vector3(up.x, -up.y, up.z));

                    drawRotationDebug(transformMatrix);

                    float theta = rvec.magnitude;
                    rvec.Normalize();

                    //the rvec from OpenCV is a compact axis-angle format. The direction of the vector is the axis, and the length of it is the angle to rotate about (i.e. theta)
                    //From this stackoverflow answer: http://stackoverflow.com/questions/12933284/rodrigues-into-eulerangles-and-vice-versa
                    Quaternion new_rot = Quaternion.AngleAxis(theta * Mathf.Rad2Deg, rvec);
                    quad_instances[i].transform.rotation = new_rot;

                    if (Input.GetKeyDown(KeyCode.Space))
                    {
                        Matrix4x4 mat = Matrix4x4.TRS(Vector3.zero, quad_instances[i].transform.rotation, Vector3.one);
                        Debug.Log("rot mat: " + mat);
                    }
                }
                else
                {
                    quad_instances[i].transform.rotation = Quaternion.Euler(0, 180, 0);
                }

                GameObject text_object = id_instances[i];
                text_object.transform.position = tvec;
                TextMesh text_mesh = text_object.GetComponent<TextMesh>();
                text_mesh.text = " ID: " + ids[i];


                /*
                Debug.Log("id: " + ids[i]);
                Debug.Log("Corners: ");
                for (int j = 0; j < 4; j++)
                {
                    Debug.Log("" + j + ": " + corners[i * 8 + j * 2] + ", " + corners[i * 8 + j * 2 + 1]);
                }

                Debug.Log("rvec: " + rvecs[i * 3] + ", " + rvecs[i * 3 + 1] + ", " + rvecs[i * 3 + 2]);
                Debug.Log("tvec: " + tvecs[i * 3] + ", " + tvecs[i * 3 + 1] + ", " + tvecs[i * 3 + 2]);
                */
            }


            if (Input.GetKeyDown(KeyCode.Space))
            {

                
                String rvecs_out = "";
                for(int i=0; i<rvecs.Length; i++)
                {
                    rvecs_out += rvecs[i] + " ";
                }
                Debug.Log("rvecs: " + rvecs_out);

                Debug.Log("rad2deg: " + Mathf.Rad2Deg);
                printDebugValues(new Vector3((float)rvecs[0], (float)rvecs[1], (float)rvecs[2]));

                Debug.Log("0: " + quatPrint( Quaternion.AngleAxis(0, new Vector3(0, 1, 0))));
                Debug.Log("45: " + quatPrint(Quaternion.AngleAxis(45, new Vector3(0, 1, 0))));
                Debug.Log("90: " + quatPrint(Quaternion.AngleAxis(90, new Vector3(0, 1, 0))));
            }

            

        //Debug.DrawLine(tvec, tvec + rvec * 0.09f, Color.yellow, 3);
        //Debug.Log(theta * Mathf.Rad2Deg);
        }
	}

    String vec3Print(Vector3 vec)
    {
        String output = "(";
        output += vec.x + ", ";
        output += vec.y + ", ";
        output += vec.z + ")";
        return output;
    }

    String quatPrint(Quaternion quat)
    {
        String output = "(";
        output += quat.x + ", ";
        output += quat.y + ", ";
        output += quat.z + ", ";
        output += quat.w + ")";
        return output;
    }

    void printDebugValues(Vector3 rvec)
    {
        Debug.Log("mag: " + rvec.magnitude);
        float theta = rvec.magnitude;
        rvec.Normalize();
        Debug.Log("normalized: " + vec3Print(rvec));
        Debug.Log("theta * rad2deg: " + theta * Mathf.Rad2Deg);

        Quaternion new_rot = Quaternion.AngleAxis(theta * Mathf.Rad2Deg, rvec);
        Debug.Log("new_rot: " + quatPrint(new_rot));
    }
    
    /*
    Quaternion drawRotationDebug(Vector3 tvec, Vector3 rvec)
    {
        float theta = rvec.magnitude;

        rvec.Normalize();

        //The following is debug code to visualize the rvec information from opencv better
        //Re-building the rotation matrix based on the rodrigues vector - see this docs page for the formula: http://docs.opencv.org/3.1.0/d9/d0c/group__calib3d.html#ga61585db663d9da06b68e70cfbf6a1eac
        Vector3 f = new Vector3();
        Vector3 u = new Vector3();
        Vector3 r = new Vector3();

        float cos_theta = Mathf.Cos(theta);
        float one_minus_cos = 1.0f - cos_theta;
        float sin_theta = Mathf.Sin(theta);

        u.x = one_minus_cos * rvec.x * rvec.y + (sin_theta * -rvec.z);
        u.y = cos_theta + one_minus_cos * rvec.y * rvec.y;
        u.z = one_minus_cos * rvec.y * rvec.z + sin_theta * rvec.x;

        f.x = one_minus_cos * rvec.x * rvec.z + sin_theta * rvec.y;
        f.y = one_minus_cos * rvec.y * rvec.z + sin_theta * -rvec.x;
        f.z = cos_theta + one_minus_cos * rvec.z * rvec.z;

        r.x = cos_theta + one_minus_cos * rvec.x * rvec.x;
        r.y = one_minus_cos * rvec.x * rvec.y + sin_theta * rvec.z;
        r.z = one_minus_cos * rvec.x * rvec.z + sin_theta * -rvec.y;

        //String mat_string = "";
        //mat_string += r.x + " " + u.x + " " + f.x + "\n";
        //mat_string += r.y + " " + u.y + " " + f.y + "\n";
        //mat_string += r.z + " " + u.z + " " + f.z;

        //Debug.Log(mat_string);


        //According to this link (http://stackoverflow.com/questions/36561593/opencv-rotation-rodrigues-and-translation-vectors-for-positioning-3d-object-in)
        //opencv and unity coordinate systems differ and y-axis should be flipped - from my own testing, this is not the case, so it is not done here
        //f.y = -f.y; //reverse for opencv/unity coordinate system difference
        //u.y = -u.y;
        //r.y = -r.y;

        f.z = -f.z; //reverse for opencv/unity coordinate system difference
        u.z = -u.z;
        r.z = -r.z;

        //f.x = -f.x; //reverse for opencv/unity coordinate system difference
        //u.x = -u.x;
        //r.x = -r.x;

        //Quaternion new_rot = Quaternion.LookRotation(f, u);

        //Draw the rotation matrix vectors (which are up/right/foward of the given rotation) and rotation axis for debugging
        Debug.DrawLine(tvec, tvec + f * 0.044f, Color.blue, 0.01f);
        Debug.DrawLine(tvec, tvec + u * 0.044f, Color.green, 0.01f);
        Debug.DrawLine(tvec, tvec + r * 0.044f, Color.red, 0.01f);

        return Quaternion.LookRotation(f, u);
    }
    */

    void drawRotationDebug(Matrix4x4 markerTransformMatrix)
    {

        Vector4 right = markerTransformMatrix.GetColumn(0);
        Vector4 up = markerTransformMatrix.GetColumn(1);
        Vector4 forward = markerTransformMatrix.GetColumn(2);
        Vector4 tvec = markerTransformMatrix.GetColumn(3);

        //According to this link (http://stackoverflow.com/questions/36561593/opencv-rotation-rodrigues-and-translation-vectors-for-positioning-3d-object-in)
        //opencv and unity coordinate systems differ and y-axis should be flipped - from my own testing, this is not the case, so it is not done here
        //f.y = -f.y; //reverse for opencv/unity coordinate system difference
        //u.y = -u.y;
        //r.y = -r.y;

        forward.z = -forward.z; //reverse for opencv/unity coordinate system difference
        up.z = -up.z;
        right.z = -right.z;

        //f.x = -f.x; //reverse for opencv/unity coordinate system difference
        //u.x = -u.x;
        //r.x = -r.x;

        //Quaternion new_rot = Quaternion.LookRotation(f, u);

        //Draw the rotation matrix vectors (which are up/right/foward of the given rotation) and rotation axis for debugging
        Debug.DrawLine(tvec, tvec + forward * 0.044f, Color.blue, 0.01f);
        Debug.DrawLine(tvec, tvec + up * 0.044f, Color.green, 0.01f);
        Debug.DrawLine(tvec, tvec + right * 0.044f, Color.red, 0.01f);
    }

    Matrix4x4 makePointTransform(Vector3 tvec, Vector3 rvec)
    {
        //The following is debug code to visualize the rvec information from opencv better
        //Re-building the rotation matrix based on the rodrigues vector - see this docs page for the formula: http://docs.opencv.org/3.1.0/d9/d0c/group__calib3d.html#ga61585db663d9da06b68e70cfbf6a1eac

        rvec = new Vector3(rvec.x, rvec.y, rvec.z);
        float theta = rvec.magnitude;

        rvec = rvec.normalized;

        float cos_theta = Mathf.Cos(theta);
        float one_minus_cos = 1.0f - cos_theta;
        float sin_theta = Mathf.Sin(theta);

        Matrix4x4 result = new Matrix4x4();

        result[0, 0] = cos_theta + one_minus_cos * rvec.x * rvec.x;
        result[1, 0] = one_minus_cos * rvec.x * rvec.y + sin_theta * rvec.z;
        result[2, 0] = one_minus_cos * rvec.x * rvec.z + sin_theta * -rvec.y;
        result[3, 0] = 0;

        result[0, 1] = one_minus_cos * rvec.x * rvec.y + (sin_theta * -rvec.z);
        result[1, 1] = cos_theta + one_minus_cos * rvec.y * rvec.y;
        result[2, 1] = one_minus_cos * rvec.y * rvec.z + sin_theta * rvec.x;
        result[3, 1] = 0;

        result[0, 2] = one_minus_cos * rvec.x * rvec.z + sin_theta * rvec.y;
        result[1, 2] = one_minus_cos * rvec.y * rvec.z + sin_theta * -rvec.x;
        result[2, 2] = cos_theta + one_minus_cos * rvec.z * rvec.z;
        result[3, 2] = 0;

        //result = result.transpose;

        //float tmp;
        //tmp = result[0, 0];
        //result[0, 0] = result[0, 1];
        //result[0, 1] = tmp;

        //tmp = result[1, 0];
        //result[1, 0] = result[1, 1];
        //result[1, 1] = tmp;

        //tmp = result[2, 0];
        //result[2, 0] = result[2, 1];
        //result[2, 1] = tmp;

        //result[0, 0] = -result[0, 0];
        //result[1, 0] = -result[1, 0];
        //result[2, 0] = -result[2, 0];

        //result[0, 2] = -result[0, 2];
        //result[1, 2] = -result[1, 2];
        //result[2, 2] = -result[2, 2];

        //result[0, 0] = -result[0, 0];
        //result[0, 1] = -result[0, 1];
        //result[0, 2] = -result[0, 2];

        //result[1, 0] = -result[1, 0];
        //result[1, 1] = -result[1, 1];
        //result[1, 2] = -result[1, 2];

        //result[2, 0] = -result[2, 0];
        //result[2, 1] = -result[2, 1];
        //result[2, 2] = -result[2, 2];

        //// for z axis, we use cross product
        //result[0, 2] = result[1, 0] * result[2, 1] - result[2, 0] * result[1, 1];
        //result[1, 2] = -result[0, 0] * result[2, 1] + result[2, 0] * result[0, 1];
        //result[2, 2] = result[0, 0] * result[1, 1] - result[1, 0] * result[0, 1];

        result[0, 3] = tvec.x;
        result[1, 3] = tvec.y;
        result[2, 3] = tvec.z;
        result[3, 3] = 1;

        return result;
    }

    void drawTransformedPoint(Vector2 marker_point, Matrix4x4 transform)
    {
        Vector3 point = marker_point;
        Vector3 transformed = transform.MultiplyPoint(point);
        Gizmos.DrawSphere(transformed, 0.01f);
    }

    public void OnDrawGizmos()
    {
        drawTransformedPoint(new Vector2(0, 0), transformMatrix);
        drawTransformedPoint(new Vector2(0.04f, 0.04f), transformMatrix);
        drawTransformedPoint(new Vector2(-0.04f, 0.04f), transformMatrix);
        drawTransformedPoint(new Vector2(0.04f, -0.04f), transformMatrix);
        drawTransformedPoint(new Vector2(-0.04f, -0.04f), transformMatrix);
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
