using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class fruitSpawner : MonoBehaviour
{
    public GameObject fruitPrefab; // Assign your fruit prefab in the Inspector.
    public float spawnInterval = 10.0f; // Spawn a new fruit every 10 seconds
    public float minX = -80.8f;
    public float maxX = 56.3f;
    public float minY = -34.4f;
    public float maxY = 17.2f;

    private void Start()
    {
        // Start a repeating function to spawn fruit at a given interval.
        
    }
    private void Update()
    {
        InvokeRepeating("SpawnFruit", 0, spawnInterval);
    }
    private void SpawnFruit()
    {
        

        // Generate a random position within the specified range.
        float randomX = Random.Range(minX, maxX);
        float randomY = Random.Range(minY, maxY);

        // Create a Vector3 representing the random position.
        Vector3 spawnPosition = new Vector3(randomX, randomY, 0);

        // Instantiate a fruit at the random position.
        Instantiate(fruitPrefab, spawnPosition, Quaternion.identity);
    }

}
