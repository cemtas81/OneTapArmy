using UnityEngine;
using UnityEngine.Events;
using System.Collections.Generic;

public class CentralizedSpawnerManager : MonoBehaviour, IUpgrade
{
    public List<SpawnerData> spawnerDataList; // List of spawner configurations for each castle
    public UnityEvent charge;
    
    private Dictionary<EnemyType, float> attackTimers = new Dictionary<EnemyType, float>();
    private Dictionary<EnemyType, float> spawnTimers = new Dictionary<EnemyType, float>();
    public GameObject winPanel;
    private float timer;

    private Dictionary<EnemyType, ObjectPool<EnemyAI>> enemyPools;

    [Tooltip("Number of columns in the spawn grid")]
    public int spawnGridColumns = 3;
    [Tooltip("Number of rows in the spawn grid")]
    public int spawnGridRows = 2;
    [Tooltip("Spacing between spawned enemies")]
    public float spawnSpacing = 1.5f;

    // Keep track of the next available grid position for each enemy type
    private Dictionary<EnemyType, Vector2Int> nextSpawnGridPosition = new Dictionary<EnemyType, Vector2Int>();
    private void Awake()
    {
        InitializePools();
        InitializeAttackTimers();
        InitializeSpawnTimers();
        Time.timeScale = 1;
    }

    public void Upgrade(int level)
    {
        foreach (var spawnerData in spawnerDataList)
        {
            spawnerData.spawnInterval *= (1 - level / 100f); // Decrease spawnInterval by a percentage based on level
        }
    }

    private void InitializePools()
    {
        enemyPools = new Dictionary<EnemyType, ObjectPool<EnemyAI>>();

        // Initialize a pool for each enemy type
        foreach (var spawnerData in spawnerDataList)
        {
            foreach (var enemyPrefab in spawnerData.enemyPrefabs)
            {
                if (!enemyPools.ContainsKey(spawnerData.enemyType))
                {
                    enemyPools[spawnerData.enemyType] = new ObjectPool<EnemyAI>(
                        createFunc: () => CreateEnemy(spawnerData.enemyType),
                        actionOnGet: (enemy) => enemy.Initialize(),
                        actionOnRelease: (enemy) => enemy.ResetEnemy(),
                        actionOnDestroy: (enemy) => Destroy(enemy.gameObject),
                        defaultCapacity: spawnerData.maxEnemies
                    );

                    // Pre-instantiate enemies
                    for (int i = 0; i < spawnerData.maxEnemies; i++)
                    {
                        EnemyAI enemy = enemyPools[spawnerData.enemyType].Get();
                        enemyPools[spawnerData.enemyType].Release(enemy);
                    }
                }
            }
        }
    }

    private void InitializeAttackTimers()
    {
        // Initialize attack timers for each enemy type
        foreach (var spawnerData in spawnerDataList)
        {
            if (!attackTimers.ContainsKey(spawnerData.enemyType))
            {
                attackTimers[spawnerData.enemyType] = 0f; // Initialize timer to 0
            }
        }
    }

    private void InitializeSpawnTimers()
    {
        // Initialize spawn timers for each enemy type
        foreach (var spawnerData in spawnerDataList)
        {
            if (!spawnTimers.ContainsKey(spawnerData.enemyType))
            {
                spawnTimers[spawnerData.enemyType] = 0f; // Initialize timer to 0
                spawnerData.spawnCooldownSlider.maxValue = spawnerData.spawnInterval;
                // Initialize the grid position for this enemy type
                nextSpawnGridPosition[spawnerData.enemyType] = new Vector2Int(0, 0);
            }
        }
    }

    private EnemyAI CreateEnemy(EnemyType enemyType)
    {
        // Find the spawner data for the given enemy type
        SpawnerData spawnerData = spawnerDataList.Find(data => data.enemyType == enemyType);
        if (spawnerData == null || spawnerData.enemyPrefabs == null || spawnerData.enemyPrefabs.Count == 0)
        {
            Debug.LogError($"No enemy prefabs assigned for enemy type: {enemyType}!");
            return null;
        }

        EnemyAI enemyPrefab = spawnerData.enemyPrefabs[Random.Range(0, spawnerData.enemyPrefabs.Count)];
        EnemyAI newEnemy = Instantiate(enemyPrefab);
        newEnemy.enemyType = enemyType;
        return newEnemy;
    }

    void Update()
    {
        UpdateAttackTimers();
        UpdateSpawnTimers();
        UpdateSpawnCooldownSliders();
        WinCondition();
    }
    void WinCondition()
    {
        if (spawnerDataList.Count<1)
        {
            winPanel.SetActive(true);
            Time.timeScale = 0;
        }
    }
    private void UpdateSpawnTimers()
    {
        for (int i = spawnerDataList.Count - 1; i >= 0; i--) // Iterate backward to safely remove elements
        {
            var spawnerData = spawnerDataList[i];

            // Remove the spawner if its spawn point is destroyed
            if (spawnerData.spawnPoint == null)
            {
                Debug.LogWarning($"Spawn point for {spawnerData.enemyType} is destroyed. Removing spawner data.");
                spawnerDataList.RemoveAt(i);
                spawnTimers.Remove(spawnerData.enemyType);
                attackTimers.Remove(spawnerData.enemyType);
                enemyPools.Remove(spawnerData.enemyType);
                continue; // Skip processing for this spawner
            }

            // Increment spawn timer for each enemy type separately
            spawnTimers[spawnerData.enemyType] += Time.deltaTime;

            // Check if it's time to spawn and if we haven't reached max enemies for this type
            if (spawnTimers[spawnerData.enemyType] >= spawnerData.spawnInterval &&
                spawnerData.maxEnemies > GetActiveEnemyCount(spawnerData.enemyType))
            {
                // Spawn only this specific enemy type
                SpawnEnemy(spawnerData);
                spawnTimers[spawnerData.enemyType] = 0f; // Reset timer only for this enemy type
            }
        }
    }

    private void UpdateAttackTimers()
    {
        foreach (var spawnerData in spawnerDataList)
        {
            // Increment the attack timer for this enemy type
            attackTimers[spawnerData.enemyType] += Time.deltaTime;

            if (attackTimers[spawnerData.enemyType] >= spawnerData.attackInterval)
            {
                // Trigger the attack logic for this specific enemy type
                TriggerAttack(spawnerData.enemyType);

                // Reset the attack timer for this enemy type
                attackTimers[spawnerData.enemyType] = 0f;
            }
        }
    }

    private void UpdateSpawnCooldownSliders()
    {
        foreach (var spawnerData in spawnerDataList)
        {
            if (spawnerData.spawnCooldownSlider != null)
            {
              
                // Ensure the slider fills up to its maximum spawn interval
                spawnerData.spawnCooldownSlider.maxValue = spawnerData.spawnInterval;
                spawnerData.spawnCooldownSlider.value = Mathf.Clamp(spawnTimers[spawnerData.enemyType], 0f, spawnerData.spawnInterval);
            }
        }
    }

    private void TriggerAttack(EnemyType enemyType)
    {
        // Find all active enemies of this type
        if (enemyPools.ContainsKey(enemyType))
        {
            foreach (var enemy in enemyPools[enemyType].GetActiveObjects())
            {
                // Call the attack logic for each enemy
                enemy.StartAttacking();
            }
        }
    }
    private void SpawnEnemy(SpawnerData spawnerData)
    {
        if (spawnerData.spawnPoint == null)
            return;

        EnemyAI enemy = enemyPools[spawnerData.enemyType].Get();
        if (enemy != null)
        {
            // Get the next grid position for this enemy type
            Vector2Int gridPos = nextSpawnGridPosition[spawnerData.enemyType];

            // Calculate world position based on grid position
            Vector3 spawnPosition = spawnerData.spawnPoint.position;
            spawnPosition += spawnerData.spawnPoint.right * ((gridPos.x - (spawnGridColumns - 1) / 2.0f) * spawnSpacing);
            spawnPosition += spawnerData.spawnPoint.forward * ((gridPos.y - (spawnGridRows - 1) / 2.0f) * spawnSpacing);

            // Update position and rotation
            enemy.transform.position = spawnPosition;
            enemy.transform.rotation = spawnerData.spawnPoint.rotation;

            // Update the next grid position
            gridPos.x = (gridPos.x + 1) % spawnGridColumns;
            if (gridPos.x == 0)
            {
                gridPos.y = (gridPos.y + 1) % spawnGridRows;
            }
            nextSpawnGridPosition[spawnerData.enemyType] = gridPos;
        }
    }

    private int GetActiveEnemyCount(EnemyType enemyType)
    {
        if (enemyPools.ContainsKey(enemyType))
        {
            return enemyPools[enemyType].CountActive;
        }
        return 0;
    }

    public void EnemyDefeated(EnemyAI enemy)
    {
        if (enemyPools.ContainsKey(enemy.enemyType))
        {
            enemyPools[enemy.enemyType].Release(enemy);
        }
    }
}