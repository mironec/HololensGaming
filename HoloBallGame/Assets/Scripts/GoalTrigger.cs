using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GoalTrigger : MonoBehaviour {
    public event Action on_goal_reached;

    Collider ball_collider;

	// Use this for initialization
	void Start () {
        GameManager.on_ball_set += on_ball_set;
	}

    private void OnDestroy() {
        GameManager.on_ball_set -= on_ball_set;
    }

    private void on_ball_set(GameObject _ball) {
        ball_collider = _ball.GetComponent<Collider>();
    }

    // Update is called once per frame
    void Update () {
		
	}

    private void OnTriggerEnter(Collider other) {
        if(Collider.Equals(ball_collider, other)) {
            on_goal_reached.Invoke();
        }
    }
}
