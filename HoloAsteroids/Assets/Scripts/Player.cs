using UnityEngine;
using System.Collections;

public class Player : MonoBehaviour {

    public GameSystem gameSystem;

	void Start () {
	
	}
	
	void FixedUpdate () {
        RaycastHit hit;

        if (Physics.Raycast(transform.position, transform.forward, out hit, 25.0f) && hit.collider.gameObject.CompareTag("Asteroid")) {
            Asteroid a =  hit.collider.gameObject.GetComponent<Asteroid>();
            a.Gaze(Time.deltaTime);
        }
	}

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Asteroid"))
        {
            gameSystem.PlayerHitByAsteroid();
        }
    }
}
