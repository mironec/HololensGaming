using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;

public class GameManager : MonoBehaviour {
    public static event Action<GameObject> on_ball_set;

    public GameObject ball;
    public GameObject victory_text;

    Vector3 ball_start_pos;
    Quaternion ball_start_rot;

    public GoalTrigger goal_trigger;

    bool paused;
    bool level_complete;

	// Use this for initialization
	void Start () {
        ball_start_pos = ball.transform.position;
        ball_start_rot = ball.transform.rotation;

        StartCoroutine(emit_start_events());
        goal_trigger.on_goal_reached += on_goal_reached;
	}

        //Delaying events that are emitted on start by one frame so we can be sure everyone has subscribed during Start() calls
    private IEnumerator emit_start_events() {
        yield return null;
        on_ball_set.Invoke(ball);
    }
	
	// Update is called once per frame
	void Update () {
		if(Input.GetKeyDown(KeyCode.Space)) {
            if(level_complete) {
                reset_game();
                unpause_game();
            }
            else {
                if (paused) {
                    unpause_game();
                }
                else {
                    pause_game();
                }
            }
        }

        if(Input.GetKeyDown(KeyCode.R)) {
            reset_game();
            pause_game();
        }
	}

    void on_goal_reached() {
        pause_game();
        level_complete = true;
        Debug.Log("Goal reached!");
        victory_text.SetActive(true);
    }

    void pause_game() {
        //Time.timeScale = 0.0f;
        paused = true;
    }

    void unpause_game() {
        Time.timeScale = 1.0f;
        paused = false;
    }

    void reset_game() {
        level_complete = false;
        ball.transform.position = ball_start_pos;
        ball.transform.rotation = ball_start_rot;
        Rigidbody ball_rb = ball.GetComponent<Rigidbody>();
        ball_rb.velocity = Vector3.zero;
        ball_rb.angularVelocity = Vector3.zero;
    }
}
