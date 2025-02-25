
using UnityEngine;

public class Archer : MonoBehaviour
{

    private ArrowShoot arrow;
    private EnemyAI enemyAI;
    public Transform spawnPoint;
    private ArrowPoolManager arrowPool;
    public bool isEnemyArrow;

    private void Start()
    {
        arrow = ArrowPoolManager.Instance.unitPrefab;
        enemyAI = GetComponent<EnemyAI>();
        arrowPool = ArrowPoolManager.Instance;
    }

    public void SpawnArrow()
    {

        arrow = arrowPool.Get();
        arrow.isEnemyArrow = isEnemyArrow;
        arrow.transform.SetPositionAndRotation(spawnPoint.position, spawnPoint.rotation);
        arrow.startPoint = spawnPoint;
        arrow.endPoint = enemyAI.target;
        arrow.height=1;
        arrow.damage = 10;
        arrow.ShootArrow();
 
    }

    public void UnitDefeated(ArrowShoot unit)
    {
        arrowPool.Release(unit);

    }
  
}
