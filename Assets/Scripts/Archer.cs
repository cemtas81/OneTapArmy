
using UnityEngine;

public class Archer : MonoBehaviour
{

    private ArrowShoot arrow;
    private EnemyAI enemyAI;
    public Transform spawnPoint;
    private ArrowPoolManager arrowPool;
    public bool isEnemyArrow;
    private UnitMovement unitMovement;
    public int dam =10;
    private void Start()
    {
        arrow = ArrowPoolManager.Instance.unitPrefab;
        if (isEnemyArrow)
        {
            enemyAI = GetComponent<EnemyAI>();
            dam = enemyAI.damage;
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

        arrow = arrowPool.Get();
        arrow.isEnemyArrow = isEnemyArrow;
        arrow.transform.SetPositionAndRotation(spawnPoint.position, spawnPoint.rotation);
        arrow.startPoint = spawnPoint;
        if (isEnemyArrow)
        {
            arrow.endPoint = enemyAI.target;
        }
        else
        {
            arrow.endPoint = unitMovement.attackTarget;
        }
        arrow.height=1;
        arrow.damage = dam;
        arrow.ShootArrow();
 
    }

    public void UnitDefeated(ArrowShoot unit)
    {
        arrowPool.Release(unit);

    }
  
}
