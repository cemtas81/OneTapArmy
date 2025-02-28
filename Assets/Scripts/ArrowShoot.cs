using UnityEngine;
using DG.Tweening;

public class ArrowShoot : MonoBehaviour
{
    public Transform startPoint; 
    public Transform endPoint;   
    public float height = 5f;  
    public float duration = 2f; 
    private ArrowPoolManager arrowSpawner;
    public int damage;
    public bool isEnemyArrow;
    public EnemyType shooterEnemyType; // The type of enemy that shot this arrow

    private void Start()
    {
        arrowSpawner = ArrowPoolManager.Instance;
    }

    public void Initialize()
    {
        gameObject.SetActive(true);
    }

    public void ResetUnit()
    {
        gameObject.SetActive(false);
    }

    private Vector3 previousPosition; 

    public void ShootArrow()
    {
        // Check if startPoint or endPoint is null
        if (startPoint == null || endPoint == null)
        {
            ResetUnit();
            return;
        }

        // Store the initial position
        previousPosition = startPoint.position;

        // Calculate the midpoint for the arc
        Vector3 midPoint = Vector3.Lerp(startPoint.position, endPoint.position, 0.5f);
        midPoint.y += height;

        // Create a path for the arrow to follow
        Vector3[] path = new Vector3[] { startPoint.position, midPoint, endPoint.position };

        // Use DOTween to animate the arrow along the path
        transform.DOPath(path, duration, PathType.CatmullRom)
            .SetEase(Ease.OutQuad)
            .OnUpdate(() => UpdateArrowRotation()) // Update rotation during the tween
            .OnComplete(() => OnArrowLanded());
    }

    private void UpdateArrowRotation()
    {
        // Calculate the direction the arrow is moving
        Vector3 direction = transform.position - previousPosition;

        // Only update rotation if the direction is not zero
        if (direction != Vector3.zero)
        {
            // Calculate the rotation to look in the direction of movement
            Quaternion targetRotation = Quaternion.LookRotation(direction);

            // Apply the rotation to the arrow
            transform.rotation = targetRotation;
        }

        // Update the previous position for the next frame
        previousPosition = transform.position;
    }

    void OnArrowLanded()
    {
        arrowSpawner.Release(this);
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (!isEnemyArrow)
        {
            // Arrow shot by player
            DamageEnemy(collision.collider);
        }
        else
        {
            // Arrow shot by enemy
            DamagePlayerOrEnemy(collision.collider);
        }
    }

    void DamageEnemy(Collider coll)
    {
        if (coll.gameObject.CompareTag("EnemyUnit"))
        {
            if (coll.gameObject.TryGetComponent<IHealth>(out var enemyHealth))
            {
                enemyHealth.TakeDamage(damage,true);
                ResetUnit();
            }
        }
    }

    void DamagePlayerOrEnemy(Collider coll)
    {
        // Check if the collider is a player unit
        if (coll.gameObject.CompareTag("PlayerUnit"))
        {
            if (coll.gameObject.TryGetComponent<IHealth>(out var playerHealth))
            {
                playerHealth.TakeDamage(damage, true);
                ResetUnit();
            }
        }
        // Check if the collider is an enemy unit of a different type
        else if (coll.gameObject.CompareTag("EnemyUnit"))
        {
            if (coll.gameObject.TryGetComponent<EnemyAI>(out var enemyAI))
            {
                // Only damage if the enemy types are different
                if (enemyAI.enemyType != shooterEnemyType)
                {
                    if (coll.gameObject.TryGetComponent<IHealth>(out var enemyHealth))
                    {
                        enemyHealth.TakeDamage(damage, false);
                        ResetUnit();
                    }
                }
            }
        }
    }
}