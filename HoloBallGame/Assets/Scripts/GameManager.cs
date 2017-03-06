using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
using UnityEngine.SceneManagement;
using HoloToolkit.Unity.InputModule;
using HoloToolkit.Unity.SpatialMapping;

public class GameManager : MonoBehaviour, IInputClickHandler {
    public static event Action<GameObject> onBallSet;
    public static event Action onGameReset;

    public GameObject ball;
    public GameObject victoryText;
    public GameObject holoCamera;
    public GameObject playSpaceAnchor;
    public bool pauseOnStart = true;

    Vector3 ballStartPos;
    Quaternion ballStartRot;
    private bool planeFindingStarted = false;

    public GoalTrigger goalTrigger;

    private bool paused;
    bool level_complete;

	// Use this for initialization
	void Start () {
        ballStartPos = ball.transform.position;
        ballStartRot = ball.transform.rotation;

        StartCoroutine(emit_start_events());
        goalTrigger.onGoalReached += onGoalReached;

        InputManager.Instance.AddGlobalListener(gameObject);
        SurfaceMeshesToPlanes.Instance.MakePlanesComplete += OnPlanesComplete;
        if (pauseOnStart) pauseGame();
	}

    private void OnDestroy() {
        goalTrigger.onGoalReached -= onGoalReached;
    }

    public bool isGamePaused() {
        return paused;
    }

    //Delaying events that are emitted on start by one frame so we can be sure everyone has subscribed during Start() calls
    private IEnumerator emit_start_events() {
        yield return null;
        onBallSet.Invoke(ball);
    }
	
	// Update is called once per frame
	void Update () {
        if(!planeFindingStarted && SpatialMappingManager.Instance.GetMeshFilters().Count > 0){
            Debug.Log("Plane finding started.");
            SurfaceMeshesToPlanes.Instance.MakePlanes();
            planeFindingStarted = true;
        }
    }

    void OnPlanesComplete(object source, EventArgs args) {
        Debug.Log("On Planes Complete.");
        var tables = SurfaceMeshesToPlanes.Instance.GetActivePlanes(PlaneTypes.Table);
        if (tables.Count > 0) {
            float minDist = float.PositiveInfinity;
            GameObject closestTable = null;
            foreach (var t in tables) {
                float dist = Vector3.Distance(t.transform.position, holoCamera.transform.position);
                if (dist < minDist) {
                    minDist = dist;
                    closestTable = t;
                }
            }
            
            if(closestTable != null)
                playSpaceAnchor.transform.position = closestTable.transform.position;
        }
        planeFindingStarted = false;
        //SurfaceMeshesToPlanes.Instance.MakePlanes();
    }

    void onGoalReached() {
        level_complete = true;
        victoryText.SetActive(true);
    }

    void pauseGame() {
        Time.timeScale = 0.0f;
        paused = true;
    }

    void unpauseGame() {
        Time.timeScale = 1.0f;
        paused = false;
    }

    void resetGame() {
        level_complete = false;
        ball.transform.position = ballStartPos;
        ball.transform.rotation = ballStartRot;
        Rigidbody ballRb = ball.GetComponent<Rigidbody>();
        ballRb.velocity = Vector3.zero;
        ballRb.angularVelocity = Vector3.zero;
        victoryText.SetActive(false);

        onGameReset.Invoke();
    }

    public void OnInputClicked(InputClickedEventData eventData)
    {
        if (level_complete)
        {
            resetGame();
            pauseGame();
        }
        else
        {
            if (paused)
            {
                unpauseGame();
            }
            else
            {
                pauseGame();
            }
        }
    }
}
