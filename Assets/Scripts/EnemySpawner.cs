using UnityEngine;
using UnityEngine.Events;
using System.Collections.Generic;

public class EnemySpawner : MonoBehaviour,IUpgrade
{
    public List<EnemyAI> enemyPrefabs; // List of enemy prefabs (different types)
    public Transform spawnPoint; // Where enemies will spawn
    public float spawnInterval = 5f; // Time between spawns
    public int maxEnemies = 10; // Maximum number of enemies allowed
    public UnityEvent charge;

    [Range(3f, 15f)]
    public float attackTime;

    private float timer;
    private float attacktimer;
    public int currentEnemies = 0;

    // For ensuring balanced spawning
    private Queue<int> spawnOrder = new Queue<int>();
    private int currentEnemyIndex = 0; // Tracks the current enemy prefab to spawn
    public EnemyType enemyType;
    // Object pool for enemies
    private ObjectPool<EnemyAI> enemyPool;

    private void Awake()
    {
        InitializePool();
    }
    public void Upgrade(int level)
    {
        spawnInterval *= (1 - level / 100f); // Decrease spawnInterval by a percentage based on level
    }
    public void InitializePool()
    {
        // Clear the existing pool if it exists
        if (enemyPool != null)
        {
            enemyPool.Clear();
        }

        // Initialize the object pool
        enemyPool = new ObjectPool<EnemyAI>(
            createFunc: CreateEnemy, // Create an enemy based on the current index
            actionOnGet: (enemy) => enemy.Initialize(), // Initialize the enemy when taken from the pool
            actionOnRelease: (enemy) => enemy.ResetEnemy(), // Reset the enemy when returned to the pool
            actionOnDestroy: (enemy) => Destroy(enemy.gameObject), // Destroy the enemy if the pool is cleared
            defaultCapacity: maxEnemies // Initial pool size
        );

        // Pre-instantiate enemies
        for (int i = 0; i < maxEnemies; i++)
        {
            EnemyAI enemy = enemyPool.Get();
            enemyPool.Release(enemy);
        }

        // Initialize spawn order to ensure balanced distribution
        RefreshSpawnOrder();
    }

    private void RefreshSpawnOrder()
    {
        spawnOrder.Clear();
        for (int i = 0; i < enemyPrefabs.Count; i++)
        {
            spawnOrder.Enqueue(i);
        }
    }

    public void OnUpgrade()

    {
        InitializePool();
    }

    void Update()
    {
        // Spawn timer
        timer += Time.deltaTime;
        if (timer >= spawnInterval && currentEnemies < maxEnemies)
        {
            SpawnEnemy();
            timer = 0f;
        }

        // Attack timer
        attacktimer += Time.deltaTime;
        if (attacktimer >= attackTime)
        {
            charge.Invoke();
            attacktimer = 0f;
        }
    }

    void SpawnEnemy()
    {
        // Check if we need to refresh the spawn order
        if (spawnOrder.Count == 0 && enemyPrefabs.Count > 0)
        {
            RefreshSpawnOrder();
        }

        // Get the next enemy type to spawn
        if (spawnOrder.Count > 0)
        {
            currentEnemyIndex = spawnOrder.Dequeue();
        }

        // Get an enemy from the pool
        EnemyAI enemy = enemyPool.Get();
        if (enemy != null)
        {
            // Set position and increment counter
            enemy.transform.SetPositionAndRotation(spawnPoint.position, spawnPoint.rotation);
            currentEnemies++;

        }
    }

    public void EnemyDefeated(EnemyAI enemy)
    {
        // Return the enemy to the pool
        enemyPool.Release(enemy);
        currentEnemies--;
    }

    private EnemyAI CreateEnemy()
    {
        // Check if the enemyPrefabs list is empty
        if (enemyPrefabs == null || enemyPrefabs.Count == 0)
        {
            Debug.LogError("No enemy prefabs assigned in the list!");
            return null;
        }

        // Get the enemy prefab at the current index
        EnemyAI enemyPrefab = enemyPrefabs[currentEnemyIndex];

        // Instantiate the selected enemy prefab
        EnemyAI newEnemy = Instantiate(enemyPrefab);
        newEnemy.enemyType = enemyType;
        newEnemy.spawner = this;
        return newEnemy;
    }

    public void AddEnemyType(EnemyAI newEnemyPrefab, int enemyId)
    {
        if (newEnemyPrefab == null)
            return;

        // Search for an existing enemy with the same ID
        int index = enemyPrefabs.FindIndex(prefab => prefab.GetEnemyID() == enemyId);
        if (index != -1)
        {
            // Replace existing enemy type
            enemyPrefabs[index] = newEnemyPrefab;
        }
        else
        {
            // Add as a new enemy type
            enemyPrefabs.Add(newEnemyPrefab);
        }
        // Reinitialize the pool with updated prefabs
        InitializePool();
    }
}