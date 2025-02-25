using UnityEngine;

public class ArrowSpawner : MonoBehaviour
{

    public Transform spawnPoint;
    public float spawnInterval = 5f;
    private float timer;
    private int currentUnits;
    public float attackRange;
    private Transform target;
    private int hitCount;
    public bool isEnemyArrow;
    private ArrowShoot arrow;
    private ArrowPoolManager arrowPool;
    private void Start()
    {
        arrow = ArrowPoolManager.Instance.unitPrefab;
        arrowPool = ArrowPoolManager.Instance;
    }
    void Update()
    {
        timer += Time.deltaTime;
        if (timer >= spawnInterval && currentUnits < arrowPool.maxUnits)
        {
            SpawnUnit();
            timer = 0f;
        }
    }

    public void SpawnUnit()
    {
        FindAttackTarget();
        if (currentUnits >= arrowPool.maxUnits || target == null) return;

        arrow = arrowPool.Get();
        arrow.isEnemyArrow = isEnemyArrow;
        arrow.transform.SetPositionAndRotation(spawnPoint.position, spawnPoint.rotation);
       
        arrow.startPoint = spawnPoint;
        arrow.endPoint = target;

        arrow.ShootArrow();
        currentUnits++;


    }

    public void UnitDefeated(ArrowShoot unit)
    {
        arrowPool.Release(unit);
        currentUnits--;
    }

    private void FindAttackTarget()
    {
        Collider[] hitColliders = new Collider[arrowPool.maxUnits];

        hitCount = isEnemyArrow
            ? Physics.OverlapSphereNonAlloc(transform.position, attackRange, hitColliders, LayerMask.GetMask("PlayerUnit"))
            : Physics.OverlapSphereNonAlloc(transform.position, attackRange, hitColliders, LayerMask.GetMask("EnemyUnit"));

        Transform nearestTarget = null;
        float shortestDistance = Mathf.Infinity;

        for (int i = 0; i < hitCount; i++)
        {
            if (hitColliders[i].gameObject.activeSelf)
            {
                float distance = Vector3.Distance(transform.position, hitColliders[i].transform.position);
                if (distance < shortestDistance)
                {
                    shortestDistance = distance;
                    nearestTarget = hitColliders[i].transform;
                }
            }
        }

        target = nearestTarget ?? null; // Set nearest target or clear target if none found
    }
}
