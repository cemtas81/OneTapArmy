using System.Net;
using Unity.VisualScripting;
using UnityEngine;

public class ArrowSpawner : MonoBehaviour
{
    private ObjectPool<ArrowShoot> arrowPool; // Object pool for units
    public int maxUnits = 20;
    public ArrowShoot unitPrefab;
    public Transform spawnPoint;
    public float spawnInterval = 5f;
    private float timer;
    private int currentUnits;
    public float attackRange;
    private Transform target;

    private void Awake()
    {
        // Initialize the object pool
        arrowPool = new ObjectPool<ArrowShoot>(
            createFunc: () => Instantiate(unitPrefab), // Create a new unit
            actionOnGet: (unit) => unit.Initialize(), // Initialize the unit when taken from the pool
            actionOnRelease: (unit) => unit.ResetUnit(), // Reset the unit when returned to the pool
            actionOnDestroy: (unit) => Destroy(unit.gameObject), // Destroy the unit if the pool is cleared
            defaultCapacity: maxUnits // Initial pool size
        );

        // Pre-instantiate units
        for (int i = 0; i < maxUnits; i++)
        {
            ArrowShoot unit = arrowPool.Get();
            arrowPool.Release(unit);
        }
    }
    void Update()
    {
        timer += Time.deltaTime;
        if (timer >= spawnInterval && currentUnits < maxUnits)
        {
            SpawnUnit();
            
            timer = 0f;
        }
    }

    void SpawnUnit()
    {
        // Get a unit from the pool
        ArrowShoot unit = arrowPool.Get();
        unit.transform.SetPositionAndRotation(spawnPoint.position, spawnPoint.rotation);
        FindAttackTarget();
        unit.startPoint = spawnPoint;
        unit.endPoint = target;
        unit.ShootArrow();
        currentUnits++;
    }
    public void UnitDefeated(ArrowShoot unit)
    {
        // Return the unit to the pool
        arrowPool.Release(unit);
       
    }
    void FindAttackTarget()
    {
        // Define a sphere to check for enemies within attack range
        Collider[] hitColliders = new Collider[maxUnits];
        int hitCount = Physics.OverlapSphereNonAlloc(transform.position, attackRange, hitColliders, LayerMask.GetMask("EnemyUnit"));

        Transform nearestTarget = null;
        float shortestDistance = Mathf.Infinity;

        for (int i = 0; i < hitCount; i++)
        {
            if (hitColliders[i].gameObject.activeSelf) // Only consider active enemies
            {
                float distance = Vector3.Distance(transform.position, hitColliders[i].transform.position);
                if (distance < shortestDistance)
                {
                    shortestDistance = distance;
                    nearestTarget = hitColliders[i].transform;
                }
            }
        }

        // Set the nearest target as the attack target
        if (nearestTarget != null)
        {
            target = nearestTarget;
        }
        else
        {
            target = null; // Clear attack target if no enemy is found
           
        }
    }
}
