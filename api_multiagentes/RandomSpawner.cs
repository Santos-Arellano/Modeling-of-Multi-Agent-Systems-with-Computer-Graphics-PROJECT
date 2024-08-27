using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RandomSpawner : MonoBehaviour
{
    public GameObject cubePrefab;
    public GameObject robotPrefab;

    public CalculateFloorBounds boundsCalculator;

    // Arrays (Lists) to store the positions
    private List<Vector3> cubePositions = new List<Vector3>();
    private List<Vector3> robotPositions = new List<Vector3>();

    // Lists to store the instances
    private List<GameObject> cubeInstances = new List<GameObject>();
    private List<GameObject> robotInstances = new List<GameObject>();

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            Vector3 minBounds = boundsCalculator.minBounds;
            Vector3 maxBounds = boundsCalculator.maxBounds;

            // Clear previous positions and instances
            cubePositions.Clear();
            robotPositions.Clear();
            cubeInstances.Clear();
            robotInstances.Clear();

            // Spawn 10 valid cubes
            for (int i = 0; i < 10; i++)
            {
                Vector3 randomSpawnPosition = GetValidSpawnPosition(minBounds, maxBounds, 0f);
                Debug.Log("Spawning cube at: " + randomSpawnPosition);
                GameObject cubeInstance = Instantiate(cubePrefab, randomSpawnPosition, Quaternion.identity);

                // Assign a unique name to each cube
                cubeInstance.name = "Cube" + (i + 1);

                // Save cube position and instance, and debug print
                cubePositions.Add(randomSpawnPosition);
                cubeInstances.Add(cubeInstance);
                Debug.Log("Saved Cube Position: " + randomSpawnPosition);
            }

            // Spawn 5 valid robots
            for (int i = 0; i < 5; i++)
            {
                Vector3 randomSpawnPosition = GetValidSpawnPosition(minBounds, maxBounds, 0f);
                GameObject spawnedRobot = Instantiate(robotPrefab, randomSpawnPosition, Quaternion.identity);

                // Assign a unique name to each robot
                spawnedRobot.name = "Robot" + (i + 1);

                Rigidbody rb = spawnedRobot.GetComponent<Rigidbody>();
                Debug.Log($"Spawning robot at: {randomSpawnPosition}");

                // Save robot position and instance, and debug print
                robotPositions.Add(randomSpawnPosition);
                robotInstances.Add(spawnedRobot);
                Debug.Log("Saved Robot Position: " + randomSpawnPosition);
            }

            // Debug log final vectors for cubes
            Debug.Log("Final Cube Positions:");
            foreach (Vector3 pos in cubePositions)
            {
                Debug.Log(pos);
            }

            // Debug log final vectors for robots
            Debug.Log("Final Robot Positions:");
            foreach (Vector3 pos in robotPositions)
            {
                Debug.Log(pos);
            }

            // Debug log instances for cubes
            Debug.Log("Final Cube Instances:");
            foreach (GameObject cube in cubeInstances)
            {
                Debug.Log(cube.name + " at position: " + cube.transform.position);
            }

            // Debug log instances for robots
            Debug.Log("Final Robot Instances:");
            foreach (GameObject robot in robotInstances)
            {
                Debug.Log(robot.name + " at position: " + robot.transform.position);
            }
        }
    }

    Vector3 GetValidSpawnPosition(Vector3 minBounds, Vector3 maxBounds, float height)
    {
        while (true) // Loop until a valid position is found
        {
            Vector3 randomPosition = new Vector3(
                Random.Range(minBounds.x, maxBounds.x),
                height,
                Random.Range(minBounds.z, maxBounds.z)
            );

            // Raycast downward to check if there's a shelf (Estante) below
            RaycastHit hit;
            if (Physics.Raycast(randomPosition, Vector3.down, out hit, Mathf.Infinity))
            {
                Debug.DrawRay(randomPosition, Vector3.down * Mathf.Infinity, Color.red, 0f);

                if (hit.collider.CompareTag("Estante"))
                {
                    // If the raycast hits an object with the tag "Estante", skip this position
                    continue;
                }
                else
                {
                    // If no "Estante" is detected below, return this as a valid spawn position
                    Vector3 validPosition = hit.point + Vector3.up * 0.5f;
                    Debug.Log("Valid position found: " + validPosition);
                    return validPosition; // Adjust to just above the hit point
                }
            }
            else
            {
                // If the raycast hits nothing (i.e., open ground), return the position
                Debug.Log("Open ground position found: " + randomPosition);
                return randomPosition;
            }
        }
    }

    // Methods to access the stored positions
    public List<Vector3> GetCubePositions()
    {
        return new List<Vector3>(cubePositions);
    }

    public List<Vector3> GetRobotPositions()
    {
        return new List<Vector3>(robotPositions);
    }

    // Methods to access the stored instances
    public List<GameObject> GetCubeInstances()
    {
        return new List<GameObject>(cubeInstances);
    }

    public List<GameObject> GetRobotInstances()
    {
        return new List<GameObject>(robotInstances);
    }
}
