using UnityEngine;
using System.Collections;

public class Asteroid : MonoBehaviour {

    public GameObject playerRef;
    public float gazeDestroyTime;

    private float gazeTime;

	void Start () {
	    
	}

    void Update() {
        Renderer r = GetComponent<Renderer>();
        Color c;
        c.r = 0.8f;
        c.g = 0.8f - (gazeTime / gazeDestroyTime) * 0.8f;
        c.b = 0.8f - (gazeTime / gazeDestroyTime) * 0.8f;
        c.a = 1.0f;
        r.material.color = c;
    }
	
	void FixedUpdate () {
        if (Vector3.Distance(transform.position, playerRef.transform.position) > 25.0f) {
            Destroy(gameObject);
        }
        if (gazeTime > gazeDestroyTime)
            Destroy(gameObject);
        gazeTime -= Time.deltaTime / 5.0f;
        if (gazeTime < 0.0f) {
            gazeTime = 0.0f;
        }
	}

    public void Gaze(float time) {
        gazeTime += time;
    }
}
