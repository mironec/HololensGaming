using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GoalTrigger : MonoBehaviour {

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}

    private void OnTriggerEnter(Collider other)
    {
        if(Collider.Equals(GameManager.obj.ball.GetComponent<Collider>(), other))
        {
            Debug.Log("Ball entered goal");
        }
    }
}
