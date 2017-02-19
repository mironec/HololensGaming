using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GoalTrigger : MonoBehaviour {
    public event Action onGoalReached;

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
            force *= force.magnitude * 10.0f;
            ballRb.velocity = ballRb.velocity * 0.90f;
            ballRb.velocity += force;
        }
	}

    private void OnTriggerEnter(Collider other) {
        if(other.Equals(ballCollider)) {
            onGoalReached.Invoke();
            ball_inside = true;
        }
    }

    private void OnTriggerExit(Collider other) {
        if (other.Equals(ballCollider))
        {
            ball_inside = false;
        }
    }
}
