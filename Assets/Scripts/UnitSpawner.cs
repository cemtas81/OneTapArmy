using UnityEngine;
using System.Collections.Generic;

public class UnitSpawner : MonoBehaviour
{
    public List<UnitMovement> unitPrefabs; // List of unit prefabs (different types)
    public Transform spawnPoint; // Where units will spawn
    public float spawnInterval = 5f; // Time between spawns
    public int maxUnits = 10; // Maximum number of units allowed

    private float timer;
    public int currentUnits = 0;

    private ObjectPool<UnitMovement> unitPool; // Object pool for units
    private int currentUnitIndex = 0; // Tracks the current unit prefab to spawn

    public void OnSelect()
    {
        // Clear the existing pool if it exists
        if (unitPool != null)
        {
            unitPool.Clear();
        }

        // Initialize the object pool
        unitPool = new ObjectPool<UnitMovement>(
            createFunc: CreateNextUnit, // Create the next unit in the list
            actionOnGet: (unit) => unit.Initialize(), // Initialize the unit when taken from the pool
            actionOnRelease: (unit) => unit.ResetUnit(), // Reset the unit when returned to the pool
            actionOnDestroy: (unit) => Destroy(unit.gameObject), // Destroy the unit if the pool is cleared
            defaultCapacity: maxUnits // Initial pool size
        );

        // Pre-instantiate units
        for (int i = 0; i < maxUnits; i++)
        {
            UnitMovement unit = unitPool.Get();
            unitPool.Release(unit);
        }
    }

    void Update()
    {
        timer += Time.deltaTime;

        if (timer >= spawnInterval && currentUnits < maxUnits)
        {
            SpawnUnit();
            timer = 0f;
        }
    }

    void SpawnUnit()
    {
        // Get a unit from the pool
        UnitMovement unit = unitPool.Get();
        if (unit != null)
        {
            unit.transform.SetPositionAndRotation(spawnPoint.position, spawnPoint.rotation);
            currentUnits++;
        }
    }

    public void UnitDefeated(UnitMovement unit)
    {
        // Check if the unit's prefab is still in the list
        if (unitPrefabs.Exists(prefab => prefab.unitID == unit.unitID))
        {
            // Return the unit to the pool
            unitPool.Release(unit);
        }
        else
        {
            // Destroy the unit if its prefab is no longer in the list
            Destroy(unit.gameObject);
        }

        currentUnits--;
    }
    private UnitMovement CreateNextUnit()
    {
        // Check if the unitPrefabs list is empty
        if (unitPrefabs == null || unitPrefabs.Count == 0)
        {
            Debug.LogError("No unit prefabs assigned in the list!");
            return null;
        }

        // Generate a random index within the range of the list
        int randomIndex = Random.Range(0, unitPrefabs.Count);

        // Get the unit prefab at the random index
        UnitMovement unitPrefab = unitPrefabs[randomIndex];

        // Instantiate the selected unit prefab
        return Instantiate(unitPrefab);
    }

    public void AddUnitPrefab(UnitMovement newPrefab, int cardNumber)
    {
        if (newPrefab != null)
        {
            // Set the unitID of the new prefab
            newPrefab.unitID = cardNumber;

            // Search for an existing prefab with the same unitID
            int index = unitPrefabs.FindIndex(prefab => prefab.unitID == cardNumber);
            if (index != -1)
            {
                unitPrefabs[index] = newPrefab; // Replace the existing prefab
            }
            else
            {
                unitPrefabs.Add(newPrefab); // Add the new prefab if it doesn't exist
            }

            // Reset the currentUnitIndex to ensure it points to a valid prefab
            currentUnitIndex = 0;

            // Reinitialize the object pool with the updated prefabs
            OnSelect();
        }
    }
}