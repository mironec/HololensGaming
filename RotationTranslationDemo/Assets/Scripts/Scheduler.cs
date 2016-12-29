using HoloToolkit.Unity;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Scheduler : MonoBehaviour {

    void Awake()
    {
        StartCoroutine(MakePlanes());
    }

    private IEnumerator MakePlanes() {
        yield return new WaitForSeconds(3);
        while (SpatialMappingManager.Instance.GetMeshFilters().Count < 1) {
            yield return new WaitForSeconds(2);
            SurfaceMeshesToPlanes.Instance.MakePlanes();
        }
    }
}
