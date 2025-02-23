using System.Collections;
using UnityEngine;

public class EnemySpawner : MonoBehaviour
{
    public EnemyAI enemyPrefab; // Enemy unit prefab (must be a Component, e.g., MonoBehaviour)
    public Transform spawnPoint; // Where enemies will spawn
    public float spawnInterval = 5f; // Time between spawns
    public int maxEnemies = 10; // Maximum number of enemies allowed

    private float timer;
    public int currentEnemies = 0;

    private ObjectPool<EnemyAI> enemyPool; // Object pool for enemies

    private void Awake()
    {
        // Initialize the object pool
        enemyPool = new ObjectPool<EnemyAI>(
            createFunc: () => Instantiate(enemyPrefab), // Create a new enemy
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
    private void Start()
    {
        StartCoroutine(CheckEnemiesRoutine());
    }

    IEnumerator CheckEnemiesRoutine()
    {
        while (true) // Infinite loop
        {
            yield return new WaitForSeconds(5f); // Wait for 5 seconds before checking again

            // Iterate through all current enemies
            for (int i = 0; i < currentEnemies; i++)
            {
                Transform enemyTransform = transform.GetChild(i); // Get the child transform
                if (enemyTransform.TryGetComponent<EnemyAI>(out EnemyAI enemy))
                {
                    if (enemy.target == null) // If the enemy has no target
                    {
                        // Pause the enemy's movement and attacking
                        enemy.isAttacking = false;
                        enemy.StopAttacking(); // Ensure the enemy stops attacking

                        yield return new WaitForSeconds(2f); // Wait for 2 seconds

                        // Resume the enemy's movement and attacking
                        enemy.isAttacking = true;
                        enemy.FindTarget(); // Start finding a target again
                    }
                }
            }
        }
    }
    void Update()
    {
        timer += Time.deltaTime;
        if (timer >= spawnInterval && currentEnemies < maxEnemies)
        {
            SpawnEnemy();
            timer = 0f;
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
        //currentEnemies--;
    }
}