using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public struct PoseData {
    public Vector3 pos;
    public Quaternion rot;
}

public class ArucoTrackingUtil {

    public static Dictionary<int, PoseData> createUnityPoseData(int marker_count, int[] ids, double[] rvecs, double[] tvecs) {
        Dictionary<int, PoseData> out_dict = new Dictionary<int, PoseData>();
        if (marker_count == 0) return out_dict;

        Vector3 rvec = new Vector3();
        for(int i=0; i<marker_count; i++) {
            PoseData data = new PoseData();
            data.pos.Set((float)tvecs[i * 3], (float)tvecs[i * 3 + 1], (float)tvecs[i * 3 + 2]);
            

            rvec.Set((float)rvecs[i * 3], (float)rvecs[i * 3 + 1], (float)rvecs[i * 3 + 2]);

            float theta = rvec.magnitude;
            rvec.Normalize();

            //the rvec from OpenCV is a compact axis-angle format. The direction of the vector is the axis, and the length of it is the angle to rotate about (i.e. theta)
            //From this stackoverflow answer: http://stackoverflow.com/questions/12933284/rodrigues-into-eulerangles-and-vice-versa
            data.rot = Quaternion.AngleAxis(theta * Mathf.Rad2Deg, rvec);

            out_dict[ids[i]] = data;
        }

        return out_dict;
    }
}
