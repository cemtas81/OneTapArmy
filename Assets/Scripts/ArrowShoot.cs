using UnityEngine;
using DG.Tweening;

public class ArrowShoot : MonoBehaviour
{
    public Transform startPoint; // Point A
    public Transform endPoint;   // Point B
    public float height = 5f;   // Height of the arc
    public float duration = 2f; // Duration of the tween
    private ArrowSpawner arrowSpawner;
    private void Start()
    {
        arrowSpawner = FindFirstObjectByType<ArrowSpawner>();
    }
    public void Initialize()
    {
        gameObject.SetActive(true);
    }

    public void ResetUnit()
    {
        gameObject.SetActive(false);
    }
    //private void OnEnable()
    //{
    //    ShootArrow();
    //}

    public void ShootArrow()
    {
        // Calculate the midpoint for the arc
        Vector3 midPoint = Vector3.Lerp(startPoint.position, endPoint.position, 0.5f);
        midPoint.y += height;

        // Create a path for the arrow to follow
        Vector3[] path = new Vector3[] { startPoint.position, midPoint, endPoint.position };

        // Use DOTween to animate the arrow along the path
        transform.DOPath(path, duration, PathType.CatmullRom)
            .SetEase(Ease.OutQuad)
            .OnComplete(() => OnArrowLanded());
    }

    void OnArrowLanded()
    {
        arrowSpawner.UnitDefeated(this);
    }
    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("EnemyUnit"))
        {
            if (collision.gameObject.TryGetComponent<EnemyHealth>(out var enemyHealth))
            {
                enemyHealth.TakeDamage(100);
                arrowSpawner.UnitDefeated(this);
            }
        }
    }
}