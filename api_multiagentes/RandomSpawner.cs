using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RandomSpawner : MonoBehaviour
{
    public GameObject cubePrefab;
    public GameObject robotPrefab;
    public CalculateFloorBounds boundsCalculator;
    public Posicion_Inicial positionSender; // Reference to the Posicion_Inicial script

    private List<GameObject> cubeInstances = new List<GameObject>();
    private List<GameObject> robotInstances = new List<GameObject>();

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            Vector3 minBounds = boundsCalculator.minBounds;
            Vector3 maxBounds = boundsCalculator.maxBounds;

            cubeInstances.Clear();
            robotInstances.Clear();

            for (int i = 0; i < 10; i++)
            {
                Vector3 randomSpawnPosition = GetValidSpawnPosition(minBounds, maxBounds, 0f);
                GameObject cubeInstance = Instantiate(cubePrefab, randomSpawnPosition, Quaternion.identity);
                cubeInstance.name = "Cube" + (i + 1);  // Name includes index
                cubeInstances.Add(cubeInstance);
            }

            for (int i = 0; i < 5; i++)
            {
                Vector3 randomSpawnPosition = GetValidSpawnPosition(minBounds, maxBounds, 0f);
                GameObject spawnedRobot = Instantiate(robotPrefab, randomSpawnPosition, Quaternion.identity);
                spawnedRobot.name = "Robot" + (i + 1);  // Name includes index
                robotInstances.Add(spawnedRobot);
            }

            // Combine the lists if you want to send all objects together
            List<GameObject> allObjects = new List<GameObject>();
            allObjects.AddRange(cubeInstances);
            allObjects.AddRange(robotInstances);

            // Send all object positions and names
            positionSender.SendPositions(allObjects);
        }
    }

    Vector3 GetValidSpawnPosition(Vector3 minBounds, Vector3 maxBounds, float height)
    {
        while (true)
        {
            Vector3 randomPosition = new Vector3(
                Random.Range(minBounds.x, maxBounds.x),
                height,
                Random.Range(minBounds.z, maxBounds.z)
            );

            RaycastHit hit;
            if (Physics.Raycast(randomPosition, Vector3.down, out hit, Mathf.Infinity))
            {
                Debug.DrawRay(randomPosition, Vector3.down * Mathf.Infinity, Color.red, 0f);

                if (hit.collider.CompareTag("Estante"))
                {
                    continue;
                }
                else
                {
                    Vector3 validPosition = hit.point + Vector3.up * 0.5f;
                    return validPosition;
                }
            }
            else
            {
                return randomPosition;
            }
        }
    }
}
