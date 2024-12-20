using System.Linq;
using UnityEngine;

public class BackgroundController : MonoBehaviour {

    public float AsteroidSpawnProbability = 0.01f;
    public GameObject[] asteroidPrefabs;

    public float PlanetSpawnProbability = 0.001f;
    public GameObject[] planetPrefabs;

    private void FixedUpdate() {
        float rando = Random.Range(0f, 1f);

        if (rando < AsteroidSpawnProbability) { 
            GameObject asteroid = Instantiate(asteroidPrefabs[Random.Range(0, asteroidPrefabs.Count())], transform);
            asteroid.transform.position = new Vector3(Random.Range(-25f, 25f), Random.Range(-50, -8), -5f);

            float s = Random.Range(0.1f, 1f);
            asteroid.transform.localScale = new Vector3(s, s, s);
        }

        if (rando < PlanetSpawnProbability) { 
            GameObject planet = Instantiate(planetPrefabs[Random.Range(0, planetPrefabs.Count())], transform);
            planet.transform.position = new Vector3(Random.Range(-25f, 25f), Random.Range(-50, -8), -5f);
        }
    }
}
