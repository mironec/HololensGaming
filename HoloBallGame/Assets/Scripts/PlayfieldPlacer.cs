using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using HoloToolkit.Unity.InputModule;
using HoloToolkit.Unity.SpatialMapping;
using System;

public class PlayfieldPlacer : MonoBehaviour, IInputClickHandler
{
    private bool playfieldSelected = false;
    private GameObject selectedSurfacePlane;
    private bool planeFindingStarted = false;
    private List<GameObject> shownPlayspaces;

    public GameObject playSpaceAnchor;

    void Start () {
        playfieldSelected = false;
        selectedSurfacePlane = null;
        shownPlayspaces = new List<GameObject>();
        SurfaceMeshesToPlanes.Instance.MakePlanesComplete += OnPlanesComplete;
        InputManager.Instance.AddGlobalListener(gameObject);
	}

    public bool isPlayfieldSelected() {
        return playfieldSelected;
    }
	
	void Update () {
        if (!planeFindingStarted && !SpatialMappingManager.Instance.IsObserverRunning() && SpatialMappingManager.Instance.GetMeshFilters().Count > 0)
        {
            Debug.Log("Plane finding started.");
            SurfaceMeshesToPlanes.Instance.MakePlanes();
            planeFindingStarted = true;
        }
    }

    public void OnInputClicked(InputClickedEventData eventData)
    {
        Transform gazeTransform = GazeManager.Instance.GazeTransform;
        RaycastHit[] hits = Physics.RaycastAll(gazeTransform.position, gazeTransform.forward, 100.0f);
        foreach (var hit in hits) {
            GameObject hitObject = hit.collider.gameObject;
            if (hitObject.CompareTag("SurfacePlane")) {
                if (hitObject.GetComponent<SurfacePlane>().PlaneType == PlaneTypes.Table) {
                    selectedSurfacePlane = hitObject;
                    playSpaceAnchor.transform.position = selectedSurfacePlane.transform.position;
                }
            }
        }
    }

    void OnPlanesComplete(object source, EventArgs args)
    {
        if (selectedSurfacePlane != null) {
            foreach (var plane in SurfaceMeshesToPlanes.Instance.ActivePlanes)
            {
                plane.SetActive(false);
            }
            var list = new List<GameObject>();
            GameObject cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
            cube.tag = "TemporaryRemoveVerticesObject";
            cube.GetComponent<MeshRenderer>().enabled = false;
            cube.transform.localScale = selectedSurfacePlane.transform.localScale;
            cube.transform.localScale = new Vector3(cube.transform.localScale.x, cube.transform.localScale.y, 0.5f);
            cube.transform.position = selectedSurfacePlane.transform.position;
            cube.transform.localRotation = selectedSurfacePlane.transform.localRotation;
            list.Add(cube);
            RemoveSurfaceVertices.Instance.RemoveVerticesComplete += OnRemoveVerticesComplete;
            RemoveSurfaceVertices.Instance.RemoveSurfaceVerticesWithinBounds(list);
            cube.GetComponent<BoxCollider>().enabled = false;
            SurfaceMeshesToPlanes.Instance.MakePlanesComplete -= OnPlanesComplete;
            foreach (var item in SurfaceMeshesToPlanes.Instance.ActivePlanes)
            {
                Destroy(item);
            }
            foreach (var plane in shownPlayspaces) {
                if (plane != selectedSurfacePlane)
                    Destroy(plane);
                else
                    plane.GetComponent<Renderer>().enabled = false;
            }
            playfieldSelected = true;
            return;
        }
        planeFindingStarted = false;
        foreach (var plane in shownPlayspaces) {
            Destroy(plane);
        }
        shownPlayspaces.Clear();
        foreach (var plane in SurfaceMeshesToPlanes.Instance.ActivePlanes)
        {
            if (plane.GetComponent<SurfacePlane>().PlaneType != PlaneTypes.Table) continue;
            GameObject planeCopy = Instantiate(plane);
            planeCopy.tag = "SurfacePlane";
            shownPlayspaces.Add(planeCopy);
        }
    }

    void OnRemoveVerticesComplete(object source, EventArgs args) {
        GameObject cube = GameObject.FindGameObjectWithTag("TemporaryRemoveVerticesObject");
        Destroy(cube);
        playfieldSelected = true;
    }
}
