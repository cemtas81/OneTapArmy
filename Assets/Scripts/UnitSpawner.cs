using UnityEngine;

public class UnitSpawner : MonoBehaviour
{
    public UnitMovement unitPrefab; // Unit prefab to pool (must be a Component, e.g., MonoBehaviour)
    public Transform spawnPoint;
    public float spawnInterval = 5f; 
    public int maxUnits = 10; 

    private float timer;
    public int currentUnits = 0;

    private ObjectPool<UnitMovement> unitPool; // Object pool for units

    private void Awake()
    {
        // Initialize the object pool
        unitPool = new ObjectPool<UnitMovement>(
            createFunc: () => Instantiate(unitPrefab), // Create a new unit
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
        unit.transform.SetPositionAndRotation(spawnPoint.position, spawnPoint.rotation);
        currentUnits++;
    }

    public void UnitDefeated(UnitMovement unit)
    {
        // Return the unit to the pool
        unitPool.Release(unit);
        //currentUnits--;
    }
}