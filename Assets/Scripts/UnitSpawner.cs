using UnityEngine;
using System.Collections.Generic;
using UnityEngine.UI;

public class UnitSpawner : MonoBehaviour, IUpgrade
{
    public List<UnitMovement> unitPrefabs;
    public Transform spawnPoint;
    public float spawnInterval = 5f;
    public int maxUnits = 50;
    public Slider healthSlider;
    private float timer;
    public int currentUnits = 0;
    public GameObject losePanel;
    // To keep track of the current spawn sequence
    private Queue<int> spawnQueue = new Queue<int>();
    private int nextSpawnIndex = 0;

    private List<UnitMovement> activeUnits = new List<UnitMovement>();
    private List<UnitMovement> pooledUnits = new List<UnitMovement>();

    private void Start()
    {
        PrePopulatePool();
        RefreshSpawnQueue();
    }
    private void OnDestroy()
    {
        Time.timeScale = 0;
        losePanel.SetActive(true);
    }
    public void OnSelect()
    {
        ClearPooledUnits();
        PrePopulatePool();
        RefreshSpawnQueue();
    }
    public void Upgrade(int level)
    {
        spawnInterval *= (1 - level / 100f); // Decrease spawnInterval by a percentage based on level
    }
    private void ClearPooledUnits()
    {
        foreach (var unit in pooledUnits)
        {
            if (unit != null)
                Destroy(unit.gameObject);
        }
        pooledUnits.Clear();
    }

    private void PrePopulatePool()
    {
        if (unitPrefabs == null || unitPrefabs.Count == 0)
            return;

        int unitsPerType = Mathf.Max(1, maxUnits / unitPrefabs.Count);

        for (int i = 0; i < unitPrefabs.Count; i++)
        {
            for (int j = 0; j < unitsPerType; j++)
            {
                if (pooledUnits.Count < maxUnits)
                {
                    UnitMovement newUnit = CreateUnitOfType(i);
                    newUnit.gameObject.SetActive(false);
                    pooledUnits.Add(newUnit);
                }
            }
        }
    }

    private void RefreshSpawnQueue()
    {
        spawnQueue.Clear();

        for (int i = 0; i < unitPrefabs.Count; i++)
        {
            spawnQueue.Enqueue(i);
        }
    }

    private void Update()
    {
        UpdateSpawnCooldownSlider();
    }
    private void UpdateSpawnCooldownSlider()
    {
        timer += Time.deltaTime;
        // Ensure the slider fills up to its maximum spawn interval
        healthSlider.maxValue = spawnInterval;
        healthSlider.value = Mathf.Clamp(timer, 0f, spawnInterval);
        if (timer >= spawnInterval && currentUnits < maxUnits)
        {
            SpawnNextUnit();
            timer = 0f;
        }
    }
    private void SpawnNextUnit()
    {
        if (unitPrefabs.Count == 0)
            return;

        if (spawnQueue.Count == 0)
        {
            RefreshSpawnQueue();
        }

        nextSpawnIndex = spawnQueue.Dequeue();

        UnitMovement unit = GetPooledUnitOfType(nextSpawnIndex);

        if (unit != null)
        {
            ActivateUnit(unit);
        }
        else
        {
            Debug.LogWarning($"Failed to find pooled unit of type index {nextSpawnIndex}");
        }
    }

    private void ActivateUnit(UnitMovement unit)
    {
        Vector3 spawnPosition = GetUniqueSpawnPosition();
        unit.transform.SetPositionAndRotation(spawnPosition, spawnPoint.rotation);
        unit.Initialize();

        pooledUnits.Remove(unit);
        activeUnits.Add(unit);
        currentUnits++;
    }
    private List<Vector3> occupiedPositions = new List<Vector3>(); // Track occupied positions

    private Vector3 GetUniqueSpawnPosition()
    {
        Vector3 spawnPosition = spawnPoint.position;
        float offset = .8f; // Adjust this value based on unit size
        int unitsPerRow = 5; // Number of units per row in the grid

        int maxAttempts = 100; // Prevent infinite loops
        int attempts = 0;

        while (attempts < maxAttempts)
        {
            // Calculate the row and column based on the currentUnits count
            int row = currentUnits / unitsPerRow;
            int column = currentUnits % unitsPerRow;

            // Calculate the spawn position based on the row and column
            Vector3 newPosition = spawnPosition + new Vector3(column * offset, 0, -row * offset);

            // Check if the position is already occupied
            if (!IsPositionOccupied(newPosition))
            {
                occupiedPositions.Add(newPosition); // Mark this position as occupied
                return newPosition;
            }

            // If the position is occupied, try the next position
            //currentUnits++;
            attempts++;
        }

        return spawnPosition;
    }

    private bool IsPositionOccupied(Vector3 position)
    {
        // Check if the position is already in the occupiedPositions list
        foreach (var occupiedPosition in occupiedPositions)
        {
            if (Vector3.Distance(occupiedPosition, position) < 0.1f) // Tolerance for floating-point precision
            {
                return true;
            }
        }
        return false;
    }

    public void UnitDefeated(UnitMovement unit)
    {
        if (IsUnitStillValid(unit))
        {
            ReturnUnitToPool(unit);
        }
        else
        {
            DestroyUnit(unit);
        }

        currentUnits--;
    }

    private bool IsUnitStillValid(UnitMovement unit)
    {
        foreach (var prefab in unitPrefabs)
        {
            if (prefab.unitID == unit.unitID)
            {
                return true;
            }
        }
        return false;
    }
    public void SpawnUnitOfType(int unitType, Vector3 spawnPosition, GameObject door)
    {

        // Find the index of the unit type in the unitPrefabs list
        int typeIndex = unitPrefabs.FindIndex(prefab => prefab.unitID == unitType);

        // Get a pooled unit of the specified type
        UnitMovement unit = GetPooledUnitOfType(typeIndex);
        if (unit != null)
        {
            // Activate the unit at the specified position
            unit.transform.SetPositionAndRotation(spawnPosition, spawnPoint.rotation);
            unit.Initialize();

            pooledUnits.Remove(unit);
            activeUnits.Add(unit);
            currentUnits++;
        }
        else
        {
            Destroy(door);
        }
    }
    private void ReturnUnitToPool(UnitMovement unit)
    {
        unit.ResetUnit();
        unit.gameObject.SetActive(false);
        activeUnits.Remove(unit);
        pooledUnits.Add(unit);
    }

    private void DestroyUnit(UnitMovement unit)
    {
        activeUnits.Remove(unit);
        Destroy(unit.gameObject);
    }

    private UnitMovement GetPooledUnitOfType(int typeIndex)
    {
        if (typeIndex >= unitPrefabs.Count)
            return null;

        foreach (UnitMovement unit in pooledUnits)
        {
            if (unit.unitID == unitPrefabs[typeIndex].unitID)
            {
                return unit;
            }
        }

        if (activeUnits.Count + pooledUnits.Count < maxUnits)
        {
            UnitMovement newUnit = CreateUnitOfType(typeIndex);
            newUnit.gameObject.SetActive(false);
            pooledUnits.Add(newUnit);
            return newUnit;
        }

        return null;
    }

    private UnitMovement CreateUnitOfType(int typeIndex)
    {
        if (typeIndex >= unitPrefabs.Count)
            return null;

        UnitMovement prefab = unitPrefabs[typeIndex];
        UnitMovement newUnit = Instantiate(prefab);
        newUnit.transform.SetParent(transform);

        return newUnit;
    }

    public void AddUnitPrefab(UnitMovement newPrefab, int cardNumber)
    {
        if (newPrefab == null)
            return;

        newPrefab.cardID = cardNumber;

        int index = unitPrefabs.FindIndex(prefab => prefab.cardID == cardNumber);
        if (index != -1)
        {
            int oldUnitID = unitPrefabs[index].unitID;
            unitPrefabs[index] = newPrefab;
            UpdatePooledUnits(oldUnitID, cardNumber);
        }
        else
        {
            unitPrefabs.Insert(0, newPrefab);
        }

        RefreshSpawnQueue();
        OnSelect();
    }

    private void UpdatePooledUnits(int oldUnitID, int newCardID)
    {
        foreach (UnitMovement unit in pooledUnits)
        {
            if (unit.unitID == oldUnitID)
            {
                unit.cardID = newCardID;
            }
        }
    }
}
