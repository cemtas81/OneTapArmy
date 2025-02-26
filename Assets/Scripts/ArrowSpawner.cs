using UnityEngine;

public class ArrowSpawner : MonoBehaviour
{

    public Transform spawnPoint;
    public float spawnInterval = 5f, shootHeight;
    private float timer;
    private int currentUnits;
    public float attackRange;
    private Transform target;
    private int hitCount;
    public bool isEnemyArrow;
    private ArrowShoot arrow;
    private ArrowPoolManager arrowPool;
    public int damage;
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
        arrow.height = shootHeight;
        arrow.startPoint = spawnPoint;
        arrow.endPoint = target;
        arrow.damage = damage;
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
        // Determine the layer mask based on whether the arrow is an enemy arrow
        int layerMask = isEnemyArrow
            ? LayerMask.GetMask("PlayerUnit")
            : LayerMask.GetMask("EnemyUnit");

        // Create an array to store colliders within the attack range
        Collider[] hitColliders = new Collider[arrowPool.maxUnits];

        // Perform the overlap sphere to find colliders within the attack range
        hitCount = Physics.OverlapSphereNonAlloc(transform.position, attackRange, hitColliders, layerMask);

        Transform nearestTarget = null;
        float shortestDistance = Mathf.Infinity;

        // Iterate through all the colliders found
        for (int i = 0; i < hitCount; i++)
        {
            // Skip inactive game objects
            if (!hitColliders[i].gameObject.activeSelf)
                continue;

            // Calculate the distance to the current collider
            float distance = Vector3.Distance(transform.position, hitColliders[i].transform.position);

            // Skip if the current collider is not closer than the previous nearest target
            if (distance >= shortestDistance)
                continue;

            // Update the nearest target and shortest distance
            shortestDistance = distance;
            nearestTarget = hitColliders[i].transform;
        }

        // Set the target to the nearest target found, or null if no target was found
        target = nearestTarget;
    }
}
