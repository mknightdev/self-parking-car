using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Sensors;
using Unity.MLAgentsExamples;


public class CarParkArea : Area
{
    [Header("Car Park Settings")]
    public List<GameObject> parkingSlots;
    public int numOfCars;

    [Header("NPC Car Settings")]
    public List<GameObject> cars;


    public void Start()
    {
        ResetArea();
    }

    public override void ResetArea()
    {
        SetupCarPark();
    }

    private void SetupCarPark()
    {
        Transform m_parkingSlots = transform.GetChild(0);

        // Iterate and add all parking slots
        for (int i = 0; i < m_parkingSlots.childCount; i++)
        {
            parkingSlots.Add(m_parkingSlots.GetChild(i).gameObject);
        }

        // TODO: Test functionality
        //if (numOfCars > m_parkingSlots.childCount) { return; }

        numOfCars = Random.Range(m_parkingSlots.childCount / 2, m_parkingSlots.childCount - 1);

        int randomValue = 0;
        List<int> randomValueList = new List<int>();

        // Loop until we have equal amount of values to cars
        while (randomValueList.Count != numOfCars)
        {
            // Generate random value
            randomValue = Random.Range(0, parkingSlots.Count);

            // If we don't have it, add it to the list
            if (!randomValueList.Contains(randomValue))
            {
                randomValueList.Add(randomValue);
            }
        }

        // Iterate and turn off target object based on how many cars already parked
        for (int i = 0; i < numOfCars; i++)
        {
            // Remove target object, as it's occupied
            parkingSlots[randomValueList[i]].transform.GetChild(0).gameObject.SetActive(false);

            // Spawn car in position
            Instantiate(cars[Random.Range(0, cars.Count)], parkingSlots[randomValueList[i]].transform);

            // Debug to show value
            Debug.Log($"List #{i}: {randomValueList[i]}");
        }

        // Clear the list when all cars have spawned
        randomValueList.Clear();
    }
}
