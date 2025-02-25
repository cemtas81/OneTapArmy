
using UnityEngine;
using UnityEngine.Events;

public class EnemySpawner : MonoBehaviour
{
    public EnemyAI enemyPrefab; // Enemy unit prefab (must be a Component, e.g., MonoBehaviour)
    public Transform spawnPoint; // Where enemies will spawn
    public float spawnInterval = 5f; // Time between spawns
    public int maxEnemies = 10; // Maximum number of enemies allowed
    public UnityEvent charge;
    private float timer;
    public int currentEnemies = 0;
    [Range(3f, 10f)]
    public float attackTime;
    private ObjectPool<EnemyAI> enemyPool; // Object pool for enemies
    
    private float attacktimer;

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

    void Update()
    {
        timer += Time.deltaTime;
        attacktimer+= Time.deltaTime;
        if (timer >= spawnInterval && currentEnemies < maxEnemies)
        {
            SpawnEnemy();
            timer = 0f;
        }
        else if (attacktimer >= attackTime)
        {           
            //attackTime = Random.Range(5, 10);
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
        //currentEnemies--;
    }
}