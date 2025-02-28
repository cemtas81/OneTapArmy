using UnityEngine;

public class Archer : MonoBehaviour
{

    public ArrowShoot arrowPrefab; // Assign different arrow prefabs in the Inspector
    private EnemyAI enemyAI;
    public Transform spawnPoint;
    private ArrowPoolManager arrowPool;
    public bool isEnemyArrow;
    private UnitMovement unitMovement;
    public int dam = 10;

    private void Start()
    {
    
        if (isEnemyArrow)
        {
            enemyAI = GetComponent<EnemyAI>();
            dam = enemyAI.damage;
            //arrow.shooterEnemyType = enemyAI.enemyType; // This will now be set on the spawned arrow
        }
        else
        {
            unitMovement = GetComponent<UnitMovement>();
            dam = unitMovement.attackDamage;
        }

        arrowPool = ArrowPoolManager.Instance;
    }

    public void SpawnArrow()
    {
        ArrowShoot arrow = arrowPool.Get(); // Get an arrow from the pool, based on the assigned prefab
        if (arrow == null)
        {
            Debug.LogError("Failed to spawn arrow.  Ensure the prefab is assigned and the pool is properly initialized.");
            return;
        }

        arrow.isEnemyArrow = isEnemyArrow;
        arrow.transform.SetPositionAndRotation(spawnPoint.position, spawnPoint.rotation);
        arrow.startPoint = spawnPoint;

        if (isEnemyArrow)
        {
            arrow.endPoint = enemyAI.target;
            arrow.shooterEnemyType = enemyAI.enemyType; // Set the enemy type for this specific arrow
        }
        else
        {
            arrow.endPoint = unitMovement.attackTarget;
        }

        arrow.height = 1;
        arrow.damage = dam;
        arrow.ShootArrow();
    }

    public void UnitDefeated(ArrowShoot unit)
    {
        arrowPool.Release(unit);
    }
}