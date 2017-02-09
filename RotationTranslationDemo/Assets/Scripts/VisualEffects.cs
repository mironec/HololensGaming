using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class VisualEffects : MonoBehaviour {

    /*public float ballsPerSecond = 2.5f;
    public GameObject ballPrefab;
    private float timeSinceLastBall = 0.0f;
    private float secondsPerBall;*/

	void Start () {
        //secondsPerBall = 1.0f / ballsPerSecond;
    }
	
	void FixedUpdate () {
        /*timeSinceLastBall += Time.fixedDeltaTime;
        while (timeSinceLastBall > secondsPerBall) {
            GameObject ball = Instantiate(ballPrefab);
            List<GameObject> ceilings = HoloToolkit.Unity.SurfaceMeshesToPlanes.Instance.GetActivePlanes(HoloToolkit.Unity.PlaneTypes.Ceiling);
            GameObject ceiling = ceilings[Random.Range(0, ceilings.Count)];
            if (ceilings.Count <= 0)
            {
                Debug.Log("No ceiling, returning.");
                timeSinceLastBall = 0.0f;
                return;
            }
            Mesh planeMesh = ceiling.GetComponent<MeshFilter>().mesh;
            Bounds bounds = planeMesh.bounds;
            float minX = ceiling.transform.position.x - ceiling.transform.localScale.x * bounds.size.x * 0.5f;
            float minZ = ceiling.transform.position.z - ceiling.transform.localScale.z * bounds.size.z * 0.5f;
            ball.transform.position = new Vector3(Random.Range(minX, -minX),
                HoloToolkit.Unity.SurfaceMeshesToPlanes.Instance.CeilingYPosition - ball.GetComponent<MeshFilter>().mesh.bounds.size.y * ball.transform.localScale.y * 0.5f,
                Random.Range(minZ, -minZ));
            timeSinceLastBall -= secondsPerBall;
        }*/

        
	}
}
