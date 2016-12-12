using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class GameSystem : MonoBehaviour {

    public Asteroid originalAsteroid;
    public GameObject player;
    public float inverseAsteroidChance;
    public Text scoreText;

    private float lastAsteroid;
    private ArrayList asteroids;
    private float score;
    private float elapsedTime;

    void Start()
    {
        lastAsteroid = Time.fixedTime;
        asteroids = new ArrayList();
        score = 0.0f;
        elapsedTime = 0.0f;
    }

    void UpdateScoreText() {
        scoreText.text = "Score: " + ((int)score).ToString();
    }

    void Update() {
        UpdateScoreText();
    }

    void FixedUpdate()
    {
        score += Time.deltaTime * elapsedTime;
        elapsedTime += Time.deltaTime;
        float currentTime = Time.fixedTime;
        if (Random.Range(0.0f, inverseAsteroidChance) * Time.deltaTime < currentTime - lastAsteroid)
        {
            Vector3 pos = player.transform.forward * 10.0f;
            pos = Vector3.RotateTowards(pos, Random.rotation * player.transform.forward, 0.25f, 10000.0f);
            Asteroid newAsteroid = Instantiate(originalAsteroid, pos, Quaternion.Inverse(player.transform.rotation)) as Asteroid;
            lastAsteroid = currentTime;
            asteroids.Add(newAsteroid);
            newAsteroid.transform.localScale *= Random.Range(0.8f, 1.2f);
            newAsteroid.playerRef = player;
            Rigidbody body = newAsteroid.GetComponent<Rigidbody>();
            body.AddForce( (player.transform.position - pos) * 10.0f );
        }
    }

    void SetScore(float score) {
        this.score = score;
    }

    public void PlayerHitByAsteroid() {
        score = 0.0f;
        elapsedTime = 0.0f;

        foreach (Asteroid a in asteroids) {
            if (a == null) continue;
            Destroy(a.gameObject);
        }
        asteroids = new ArrayList();
        lastAsteroid = Time.fixedTime;
    }
}
