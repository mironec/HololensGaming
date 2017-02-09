using HoloToolkit.Unity;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;

public class GameManager : MonoBehaviour {

    public GameObject something;

    private bool done = false;
    private bool scanDone = false;

    void Start() {
        SpatialUnderstanding.Instance.RequestBeginScanning();
    }
	
	void Update () {
        if (SpatialUnderstanding.Instance.ScanState == SpatialUnderstanding.ScanStates.Done && !done)
        {
            SpatialUnderstandingDllTopology.TopologyResult[] result = new SpatialUnderstandingDllTopology.TopologyResult[1];
            System.IntPtr intptr = SpatialUnderstanding.Instance.UnderstandingDLL.PinObject(result);
            int locationCount = SpatialUnderstandingDllTopology.QueryTopology_FindLargePositionsOnWalls(0.1f, 0.1f, 0.0f, 0.2f, result.Length, intptr);

            if (locationCount > 0)
            {

                something.transform.position = result[0].position;
                Debug.Log(result[0].position);
                Debug.Log(result[0].length);
                Debug.Log(result[0].width);
                Debug.Log(result[0].normal);
                SpatialUnderstanding.Instance.UnderstandingDLL.UnpinAllObjects();

                done = true;
                Debug.Log("Retrieved spatial understanding position.");
            }
        }

        if (!scanDone && Time.fixedTime > 20.0f)
        {
            SpatialUnderstanding.Instance.RequestFinishScan();
            scanDone = true;
        }
    }
}
