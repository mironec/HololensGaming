using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using HoloToolkit.Unity.InputModule;
using HoloToolkit.Unity.SpatialMapping;
using System;

public class PlayfieldPlacer : MonoBehaviour, IInputClickHandler
{
    [Flags]
    public enum PlayfieldOptions
    {
        MainGamePlane = 1,
        FloorGamePlane = 2
    }
    public event Action onPlayfieldSelected;

    private bool playfieldSelected = false;
    //private GameObject selectedSurfacePlane;
    private GameObject[] gamePlanes;
    public PlaneInfo[] abstractGamePlanes { get; private set; }

    public PlayfieldOptions playfieldOption = PlayfieldOptions.MainGamePlane;
    private bool planeFindingStarted = false;
    private List<GameObject> shownPlayspaces;

    public GameObject playSpaceAnchor;
    public float clearingSpaceHeight = 0.5f;

    void Start() {
        playfieldSelected = false;
        shownPlayspaces = new List<GameObject>();
        SurfaceMeshesToPlanes.Instance.MakePlanesComplete += OnPlanesComplete;
        // Assume the GameManager subscribed as a global listener
    }

    private bool hasFlag(PlayfieldOptions flag) {
        return (playfieldOption & flag) == flag;
    }

    private int calculateNumberOfGamePlanes() {
        int toRet = 0;
        if (hasFlag(PlayfieldOptions.MainGamePlane))
            toRet++;
        if (hasFlag(PlayfieldOptions.FloorGamePlane))
            toRet++;
        return toRet;
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
                    int gamePlanesCount = calculateNumberOfGamePlanes();
                    gamePlanes = new GameObject[gamePlanesCount];
                    abstractGamePlanes = new PlaneInfo[gamePlanesCount];
                    int index = 0;
                    if (hasFlag(PlayfieldOptions.MainGamePlane)) {
                        gamePlanes[index] = hitObject;
                        abstractGamePlanes[index] = createGamePlaneInfo(hitObject);
                        index++;
                    }
                    if (hasFlag(PlayfieldOptions.FloorGamePlane)) {
                        gamePlanes[index] = SurfaceMeshesToPlanes.Instance.GetActivePlanes(PlaneTypes.Floor)[0];
                        abstractGamePlanes[index] = createGamePlaneInfo(gamePlanes[index]);
                        index++;
                    }
                    playSpaceAnchor.transform.position = hitObject.transform.position;
                    Quaternion alignRot = Quaternion.FromToRotation(hitObject.transform.rotation * Vector3.up, Vector3.up);
                    if (hitObject.transform.localScale.x > hitObject.transform.localScale.y)
                        playSpaceAnchor.transform.rotation = alignRot * hitObject.transform.rotation * Quaternion.AngleAxis(0, Vector3.up);
                    else
                        playSpaceAnchor.transform.rotation = alignRot * hitObject.transform.rotation * Quaternion.AngleAxis(-90, Vector3.up);
                }
            }
        }
    }

    PlaneInfo createGamePlaneInfo(GameObject planeObj) {
        PlaneInfo newPlane = new PlaneInfo();
        newPlane.origin = planeObj.transform.position;
        newPlane.rotation = planeObj.transform.rotation * Quaternion.AngleAxis(-90, Vector2.right);
        newPlane.inverseRotation = Quaternion.Inverse(newPlane.rotation);
        newPlane.normal = newPlane.rotation * Vector3.up;

        Vector2 size = new Vector2();
        size.x = planeObj.transform.localScale.x;
        size.y = planeObj.transform.localScale.y;
        newPlane.size = size;
        return newPlane;
    }

    void OnPlanesComplete(object source, EventArgs args)
    {
        if (gamePlanes != null) {
            foreach (var plane in SurfaceMeshesToPlanes.Instance.ActivePlanes)
            {
                plane.SetActive(false);
            }
            var list = new List<GameObject>();
            GameObject cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
            cube.tag = "TemporaryRemoveVerticesObject";
            cube.GetComponent<MeshRenderer>().enabled = false;
            cube.transform.localScale = gamePlanes[0].transform.localScale;
            cube.transform.localScale = new Vector3(cube.transform.localScale.x, cube.transform.localScale.y, clearingSpaceHeight);
            cube.transform.position = gamePlanes[0].transform.position;
            cube.transform.localRotation = gamePlanes[0].transform.localRotation;
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
                bool inGamePlanes = false;
                foreach(var gplane in gamePlanes) {
                    if (gplane == plane)
                    {
                        plane.GetComponent<Renderer>().enabled = false;
                        inGamePlanes = true;
                        break;
                    }
                }
                if(!inGamePlanes)
                    Destroy(plane);
            }
            playfieldSelected = true;
            return;
        }
        planeFindingStarted = false;
        SpatialMappingManager.Instance.StartObserver();
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
        onPlayfieldSelected.Invoke();
    }
}

public struct PlaneInfo {
    public Vector3 origin;
    public Vector3 normal;
    public Quaternion rotation;
    public Quaternion inverseRotation;
    public Vector2 size;
}