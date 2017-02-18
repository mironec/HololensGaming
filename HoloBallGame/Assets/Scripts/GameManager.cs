using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
using UnityEngine.SceneManagement;
using HoloToolkit.Unity.InputModule;

public class GameManager : MonoBehaviour, IInputClickHandler {
    public static event Action<GameObject> onBallSet;
    public static event Action onGameReset;

    public GameObject ball;
    public GameObject victoryText;
    public bool pauseOnStart = true;

    Vector3 ballStartPos;
    Quaternion ballStartRot;

    public GoalTrigger goalTrigger;

    bool paused;
    bool level_complete;

	// Use this for initialization
	void Start () {
        ballStartPos = ball.transform.position;
        ballStartRot = ball.transform.rotation;

        StartCoroutine(emit_start_events());
        goalTrigger.onGoalReached += onGoalReached;

        InputManager.Instance.AddGlobalListener(gameObject);
        if(pauseOnStart) pauseGame();
	}

    private void OnDestroy() {
        goalTrigger.onGoalReached -= onGoalReached;
    }

    //Delaying events that are emitted on start by one frame so we can be sure everyone has subscribed during Start() calls
    private IEnumerator emit_start_events() {
        yield return null;
        onBallSet.Invoke(ball);
    }
	
	// Update is called once per frame
	void Update () {
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
