using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using HoloToolkit.Unity.InputModule;
using HoloToolkit.Unity.SpatialMapping;
using System;
using UnityEditor;

[RequireComponent(typeof(GameManager))]
public class PlayfieldPlacer : MonoBehaviour, IInputClickHandler, IManipulationHandler
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

    [HideInInspector]
    public PlayfieldOptions playfieldOption = PlayfieldOptions.MainGamePlane;
    private bool planeFindingStarted = false;
    private List<GameObject> shownPlayspaces;
    private bool cleanupPlanes = false;
    private Quaternion playspaceRotation;

    public GameObject playSpaceAnchor;
    public GameObject playSpacePlane;
    public GameObject mainCamera;
    public float clearingSpaceHeight = 0.5f;
    public bool useManual = true;

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

    private void PlaceField(GameObject originalPlane, bool applyRotationCorrections = true) {
        int gamePlanesCount = calculateNumberOfGamePlanes();
        gamePlanes = new GameObject[gamePlanesCount];
        abstractGamePlanes = new PlaneInfo[gamePlanesCount];
        int index = 0;
        if (hasFlag(PlayfieldOptions.MainGamePlane))
        {
            gamePlanes[index] = originalPlane;
            abstractGamePlanes[index] = createGamePlaneInfo(originalPlane);
            index++;
        }
        if (hasFlag(PlayfieldOptions.FloorGamePlane))
        {
            gamePlanes[index] = shownPlayspaces.Find(x => x.GetComponent<SurfacePlane>().PlaneType == PlaneTypes.Floor);
            abstractGamePlanes[index] = createGamePlaneInfo(gamePlanes[index]);
            index++;
        }
        playSpaceAnchor.transform.position = originalPlane.transform.position;
        if (applyRotationCorrections)
        {
            Quaternion alignRot = Quaternion.FromToRotation(originalPlane.transform.rotation * Vector3.up, Vector3.up);
            if (originalPlane.transform.localScale.x > originalPlane.transform.localScale.y)
                playSpaceAnchor.transform.rotation = alignRot * originalPlane.transform.rotation * Quaternion.AngleAxis(0, Vector3.up);
            else
                playSpaceAnchor.transform.rotation = alignRot * originalPlane.transform.rotation * Quaternion.AngleAxis(-90, Vector3.up);
        }
    }

    public void OnInputClickedPlane(InputClickedEventData eventData) {
        Transform gazeTransform = GazeManager.Instance.GazeTransform;
        RaycastHit[] hits = Physics.RaycastAll(gazeTransform.position, gazeTransform.forward, 100.0f);
        foreach (var hit in hits)
        {
            GameObject hitObject = hit.collider.gameObject;
            if (hitObject.CompareTag("SurfacePlane"))
            {
                if (hitObject.GetComponent<SurfacePlane>().PlaneType == PlaneTypes.Table)
                {
                    PlaceField(hitObject);
                    cleanupPlanes = true;
                }
            }
        }
    }

    public void OnInputClickedSpatialMapping(InputClickedEventData eventData)
    {
        if (gamePlanes != null) {
            cleanupPlanes = true;
            return;
        }
        Transform gazeTransform = GazeManager.Instance.GazeTransform;
        RaycastHit hit;
        Physics.Raycast(gazeTransform.position, gazeTransform.forward, out hit, 100.0f, SpatialMappingManager.Instance.LayerMask);
        playSpacePlane.transform.position = hit.point;
        playSpacePlane.GetComponent<Renderer>().enabled = true;
        PlaceField(playSpacePlane, false);
        playSpacePlane.transform.position = hit.point;
        SurfaceMeshesToPlanes.Instance.drawPlanesMask = 0;
    }

    public void OnInputClicked(InputClickedEventData eventData)
    {
        if (useManual) {
            OnInputClickedSpatialMapping(eventData);
        }
        else {
            OnInputClickedPlane(eventData);
        }
    }

    public void OnManipulationStarted(ManipulationEventData eventData) {
        playspaceRotation = playSpaceAnchor.transform.rotation;
    }

    //This is magic, don't touch
    public void OnManipulationUpdated(ManipulationEventData eventData) {
        Debug.Log(GetComponent<GameManager>().isGamePaused());
        if (gamePlanes != null) {
            Vector3 v = new Vector3(eventData.CumulativeDelta.x, 0, eventData.CumulativeDelta.z);
            v = mainCamera.GetComponent<Camera>().worldToCameraMatrix * v;
            playSpaceAnchor.transform.rotation = playspaceRotation * Quaternion.Euler(0, v.x*360.0f, 0);
        }
    }

    public void OnManipulationCompleted(ManipulationEventData eventData) {}

    public void OnManipulationCanceled(ManipulationEventData eventData) {}

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

    private GameObject CreateBoundingObject(GameObject original) {
        GameObject cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
        cube.tag = "TemporaryRemoveVerticesObject";
        cube.GetComponent<MeshRenderer>().enabled = false;
        //cube.transform.localScale = original.transform.localScale;
        cube.transform.localScale = original.GetComponent<MeshCollider>().bounds.extents*2;
        cube.transform.localScale = new Vector3(cube.transform.localScale.x, clearingSpaceHeight, cube.transform.localScale.z);
        cube.transform.position = original.transform.position;
        cube.transform.localRotation = original.transform.localRotation;
        return cube;
    }

    void OnPlanesComplete(object source, EventArgs args)
    {
        if (cleanupPlanes) {
            foreach (var plane in SurfaceMeshesToPlanes.Instance.ActivePlanes)
            {
                plane.SetActive(false);
            }
            var list = new List<GameObject>();
            GameObject cube = CreateBoundingObject(gamePlanes[0]);
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
            //if (plane.GetComponent<SurfacePlane>().PlaneType != PlaneTypes.Table) continue;
            GameObject planeCopy = Instantiate(plane);
            planeCopy.GetComponent<SurfacePlane>().Plane = plane.GetComponent<SurfacePlane>().Plane;
            planeCopy.tag = "SurfacePlane";
            shownPlayspaces.Add(planeCopy);
        }
    }

    void OnRemoveVerticesComplete(object source, EventArgs args) {
        GameObject cube = GameObject.FindGameObjectWithTag("TemporaryRemoveVerticesObject");
        Destroy(cube);
        playSpacePlane.GetComponent<Renderer>().enabled = false;
        playfieldSelected = true;
        onPlayfieldSelected.Invoke();
    }
}

#if UNITY_EDITOR
/// <summary>
/// Editor extension class to enable multi-selection of the PlayfieldOptions mask.
/// Modified version of HoloToolkit's SurfaceMeshesToPlanes.
/// </summary>
[CustomEditor(typeof(PlayfieldPlacer))]
public class PlayfieldOptionsEnumEditor : Editor
{
    public SerializedProperty playfieldOptionsMask;

    void OnEnable()
    {
        playfieldOptionsMask = serializedObject.FindProperty("playfieldOption");
    }

    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();
        serializedObject.Update();

        playfieldOptionsMask.intValue = (int)((PlayfieldPlacer.PlayfieldOptions)EditorGUILayout.EnumMaskField
                ("Playfield Options", (PlayfieldPlacer.PlayfieldOptions)playfieldOptionsMask.intValue));

        serializedObject.ApplyModifiedProperties();
    }
}
#endif

public struct PlaneInfo {
    public Vector3 origin;
    public Vector3 normal;
    public Quaternion rotation;
    public Quaternion inverseRotation;
    public Vector2 size;
}