using UnityEngine;
using UnityEngine.Events;
using System.Collections.Generic;

public class EnemySpawner : MonoBehaviour
{
    public List<EnemyAI> enemyPrefabs; // List of enemy prefabs (different types)
    public Transform spawnPoint; // Where enemies will spawn
    public float spawnInterval = 5f; // Time between spawns
    public int maxEnemies = 10; // Maximum number of enemies allowed
    public UnityEvent charge;
    private float timer;
    public int currentEnemies = 0;
    [Range(3f, 15f)]
    public float attackTime;
    private ObjectPool<EnemyAI> enemyPool; // Object pool for enemies
    private float attacktimer;
    private int currentEnemyIndex = 0; // Tracks the current enemy prefab to spawn

    private void Awake()
    {
        // Initialize the object pool
        enemyPool = new ObjectPool<EnemyAI>(
            createFunc: CreateNextEnemy, // Create the next enemy in the list
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
    }

    void Update()
    {
        timer += Time.deltaTime;
        attacktimer += Time.deltaTime;

        if (timer >= spawnInterval && currentEnemies < maxEnemies)
        {
            SpawnEnemy();
            timer = 0f;
        }
        else if (attacktimer >= attackTime)
        {
            charge.Invoke();
            attacktimer = 0f;
        }
    }

    void SpawnEnemy()
    {
        // Get an enemy from the pool
        EnemyAI enemy = enemyPool.Get();
        enemy.transform.SetPositionAndRotation(spawnPoint.position, spawnPoint.rotation);
        currentEnemies++;
    }

    public void EnemyDefeated(EnemyAI enemy)
    {
        // Return the enemy to the pool
        enemyPool.Release(enemy);
        currentEnemies--;
    }

    private EnemyAI CreateNextEnemy()
    {
        // Check if the enemyPrefabs list is empty
        if (enemyPrefabs == null || enemyPrefabs.Count == 0)
        {
            Debug.LogError("No enemy prefabs assigned in the list!");
            return null;
        }

        // Get the next enemy prefab in the list
        EnemyAI enemyPrefab = enemyPrefabs[currentEnemyIndex];

        // Move to the next index (loop back to 0 if at the end of the list)
        currentEnemyIndex = (currentEnemyIndex + 1) % enemyPrefabs.Count;

        // Instantiate the selected enemy prefab
        return Instantiate(enemyPrefab);
    }
}