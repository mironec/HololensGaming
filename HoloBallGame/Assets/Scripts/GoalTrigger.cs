using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GoalTrigger : MonoBehaviour {
    public event Action on_goal_reached;

    Collider ball_collider;
    private bool ball_inside = false;
    private GameObject ball;

	// Use this for initialization
	void Start () {
        GameManager.on_ball_set += on_ball_set;
	}

    private void OnDestroy() {
        GameManager.on_ball_set -= on_ball_set;
    }

    private void on_ball_set(GameObject _ball) {
        ball = _ball;
        ball_collider = _ball.GetComponent<Collider>();
    }

    // Update is called once per frame
    void FixedUpdate () {
        if (ball_inside) {
            Rigidbody ball_rigidbody = ball.GetComponent<Rigidbody>();
            Vector3 force = transform.position - ball.transform.position;
            force *= force.magnitude * 10.0f;
            ball_rigidbody.velocity = ball_rigidbody.velocity * 0.90f;
            ball_rigidbody.velocity += force;
        }
	}

    private void OnTriggerEnter(Collider other) {
        if(Collider.Equals(ball_collider, other)) {
            on_goal_reached.Invoke();
        }

        ball_inside = true;
    }
}
