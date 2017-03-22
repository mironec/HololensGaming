using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GoalTrigger : MonoBehaviour {
    public event Action onGoalReached;
    public bool allowBallToEscape = false;

    Collider ballCollider;
    Rigidbody ballRb;
    private bool ball_inside = false;
    private GameObject ball;

	// Use this for initialization
	void Start () {
        GameManager.onBallSet += onBallSet;
        GameManager.onGameReset += onGameReset;
	}

    private void OnDestroy() {
        GameManager.onBallSet -= onBallSet;
        GameManager.onGameReset -= onGameReset;
    }

    private void onBallSet(GameObject _ball) {
        ball = _ball;
        ballCollider = _ball.GetComponent<Collider>();
        ballRb = _ball.GetComponent<Rigidbody>();
    }

    private void onGameReset() {
        ball_inside = false;
    }

    // Update is called once per frame
    void FixedUpdate () {
        if (ball_inside) {
            Vector3 force = transform.position - ball.transform.position;
            ballRb.AddForce(-(ballRb.velocity / 10.0f), ForceMode.VelocityChange);
            ballRb.AddForce(force, ForceMode.VelocityChange);
        }
	}

    private void OnTriggerEnter(Collider other) {
        if(other.Equals(ballCollider)) {
            onGoalReached.Invoke();
            ball_inside = true;
            Rigidbody rigidbody = other.GetComponent<Rigidbody>();
            if (rigidbody != null) {
                rigidbody.useGravity = true;
            }
        }
    }

    private void OnTriggerExit(Collider other) {
        if (other.Equals(ballCollider))
        {
            if(allowBallToEscape)
                ball_inside = false;
        }
    }
}
