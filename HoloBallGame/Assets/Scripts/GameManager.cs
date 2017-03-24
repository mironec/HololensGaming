using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
using UnityEngine.SceneManagement;
using HoloToolkit.Unity.InputModule;
using HoloToolkit.Unity.SpatialMapping;

[RequireComponent(typeof (PlayfieldPlacer))]
public class GameManager : MonoBehaviour, IInputClickHandler {
    public static event Action<GameObject> onBallSet;
    public static event Action onGameReset;
    private float startTime;

    public GameObject ball;
    public GameObject victoryText;
    public GameObject holoCamera;
    public bool pauseOnStart = true;

    Vector3 ballStartPos;
    Quaternion ballStartRot;

    public GoalTrigger goalTrigger;

    private bool paused;
    private PlayfieldPlacer playfieldPlacer;
    bool level_complete;

    private float lastClickTime;

	// Use this for initialization
	void Start () {
        ballStartPos = ball.transform.position;
        ballStartRot = ball.transform.rotation;

        StartCoroutine(emit_start_events());
        goalTrigger.onGoalReached += onGoalReached;

        InputManager.Instance.AddGlobalListener(gameObject);
        playfieldPlacer = GetComponent<PlayfieldPlacer>();
        playfieldPlacer.onPlayfieldSelected += OnPlayfieldSelected;
        startTime = Time.unscaledTime;
        if (pauseOnStart) pauseGame();
	}

    private void OnDestroy() {
        goalTrigger.onGoalReached -= onGoalReached;
        if(playfieldPlacer != null) playfieldPlacer.onPlayfieldSelected -= OnPlayfieldSelected;
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
        if(Time.unscaledTime - startTime > 10 && SpatialMappingManager.Instance.IsObserverRunning()) {
            SpatialMappingManager.Instance.StopObserver();
            startTime = Time.unscaledTime;
        }
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

    private void OnPlayfieldSelected() {
        playfieldPlacer.onPlayfieldSelected -= OnPlayfieldSelected;
        playfieldPlacer.enabled = false;
        playfieldPlacer = null;
        ballStartPos = ball.transform.position;
        ballStartRot = ball.transform.rotation;
    } 

    public void OnInputClicked(InputClickedEventData eventData)
    {
        if (playfieldPlacer != null && !playfieldPlacer.isPlayfieldSelected()) { return; }
        if (level_complete)
        {
            resetGame();
            pauseGame();
        }
        else
        {
            if (Time.unscaledTime - lastClickTime < 2.5f)
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
        lastClickTime = Time.unscaledTime;
    }
}
