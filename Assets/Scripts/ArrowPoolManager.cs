using UnityEngine;

public class ArrowPoolManager : MonoBehaviour
{
    public static ArrowPoolManager Instance { get; private set; } // Singleton instance

    private ObjectPool<ArrowShoot> arrowPool; // Object pool for arrows
    public int maxUnits = 20;
    public ArrowShoot unitPrefab;
    private void Awake()
    {
        // Singleton setup
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject); // Ensure only one instance exists
            return;
        }
        Instance = this;

        // Initialize the object pool
        arrowPool = new ObjectPool<ArrowShoot>(
            createFunc: () => Instantiate(unitPrefab),
            actionOnGet: (unit) => unit.Initialize(),
            actionOnRelease: (unit) => unit.ResetUnit(),
            actionOnDestroy: (unit) => Destroy(unit.gameObject),
            defaultCapacity: maxUnits
        );

        // Pre-instantiate arrows
        for (int i = 0; i < maxUnits; i++)
        {
            ArrowShoot unit = arrowPool.Get();
            arrowPool.Release(unit);
        }
    }
    // Public method to get an arrow from the pool
    public ArrowShoot Get()
    {
        return arrowPool.Get();
    }

    // Public method to release an arrow back to the pool
    public void Release(ArrowShoot unit)
    {
        arrowPool.Release(unit);
    }

}
