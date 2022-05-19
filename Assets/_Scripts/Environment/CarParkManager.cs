using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CarParkManager : MonoBehaviour
{
    // Car Park Settings
    [Header("Car Park Settings")]
    public List<Transform> parkingSlots;    // Stores all parking spaces.

    // Car Settings
    [Header("Car Settings")]
    public List<GameObject> cars;   // Stores all spawned cars.
    public GameObject carPrefab;    // NPC car to spawn. 
    public int carsToSpawn; // Number of cars to spawn within the car park. 

    // Target
    public Transform target;  // The chosen target for the agent.

    private void Start()
    {
        // Set the number of cars to spawn, 
        // based on what scenario was picked from the menu screen.
        carsToSpawn = SceneLoader.carsToSpawn;   
    }

    /// <summary>
    /// Destroys all of the cars within the car park, ready for the next episode.
    /// Clears the list of cars within the car park.
    /// </summary>
    public void ClearCarPark()
    {
        if (this.cars.Count > 0)
        {
            // Destroy all cars
            for (int i = 0; i < this.cars.Count; i++)
            {
                Destroy(this.cars[i].gameObject);
            }
        }

        // Clear the car list
        this.cars.Clear();
    }

    /// <summary>
    /// Finds and stores all the parking spaces within the car park.
    /// The chosen target is picked randomly from this list. 
    /// </summary>
    public void GetAllTargets()
    {
        // Find all potential targets
        for (int i = 0; i < this.transform.childCount; i++)
        {
            // Get all children tagged as 'target'
            if (this.transform.GetChild(i).CompareTag("target"))
            {
                this.parkingSlots.Add(this.transform.GetChild(i));
            }
        }
    }

    /// <summary>
    /// Choose a random target within the parking space list. 
    /// </summary>
    /// <returns>the chosen target.</returns>
    public Transform SetMainTarget()
    {
        // Choose main target
        return this.target = this.parkingSlots[Random.Range(0, parkingSlots.Count)]; // TODO: Choose based on closest one
    }


    /// <summary>
    /// Hides the mesh renderer and colliders of the non-chosen targets.
    /// Shows the mesh renderer and colliders of the chosen target.
    /// 
    /// Changes the layer based on if it is a target or non-target, 
    /// this is used for sensors. 
    /// </summary>
    public void SetupCarPark()
    {
        for (int i = 0; i < parkingSlots.Count; i++)
        {
            // Hide all other targets (red boxes)
            if (target != parkingSlots[i])
            {
                // Hide mesh renderer and box collider
                this.parkingSlots[i].GetComponent<MeshRenderer>().enabled = false;
                this.parkingSlots[i].GetComponent<BoxCollider>().enabled = false;

                //this.parkingSlots[i].Find("SpaceCP").GetComponent<MeshRenderer>().enabled = false;
                this.parkingSlots[i].Find("SpaceCP").GetComponent<BoxCollider>().enabled = false;

                // Remove from sensor layer
                this.parkingSlots[i].gameObject.layer = 0;  // 0 is default
            }
            else
            {
                // Show the main target
                target.GetComponent<MeshRenderer>().enabled = true;
                target.GetComponent<BoxCollider>().enabled = true;

                // Show the checkpoint 
                //target.Find("SpaceCP").GetComponent<MeshRenderer>().enabled = true;
                target.Find("SpaceCP").GetComponent<BoxCollider>().enabled = true;

                // Add to sensor layer 
                this.target.gameObject.layer = 6;   // 6 is target layer
            }
        }

        SpawnCars();
    }

    /// <summary>
    /// Once the target has been chosen, spawn cars in the remaining spaces.
    /// The number of cars spawned is based on what scenario was chosen, 
    /// or what was set in the inspector.
    /// </summary>
    private void SpawnCars()
    {
        List<int> randomNumbers = new List<int>();
        int carsSpawned = 0;

        while (carsSpawned != carsToSpawn)
        {
            int randomNumber = Random.Range(0, parkingSlots.Count);

            if (!randomNumbers.Contains(randomNumber) && !parkingSlots[randomNumber].GetComponent<MeshRenderer>().enabled)
            {
                randomNumbers.Add(randomNumber);
                GameObject spawnedCar = Instantiate(carPrefab, parkingSlots[randomNumber].GetChild(0));
                carsSpawned++;

                cars.Add(spawnedCar);

                //Debug.Log(carsSpawned);
            }
        }

        randomNumbers.Clear();

        // Stop spawned cars from moving
        for (int i = 0; i < cars.Count; i++)
        {
            Transform wheelColliders = cars[i].transform.Find("Wheel Colliders");
            for (int j = 0; j < wheelColliders.childCount; j++)
            {
                // Stops car from moving when spawned
                wheelColliders.GetChild(j).GetComponent<WheelCollider>().motorTorque = 0.0f;
                wheelColliders.GetChild(j).GetComponent<WheelCollider>().brakeTorque = 1000.0f;
            }
        }
    }
}
