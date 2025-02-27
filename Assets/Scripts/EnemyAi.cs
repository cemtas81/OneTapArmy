using UnityEngine;
using UnityEngine.AI;

public class EnemyAI : MonoBehaviour
{
    public float waitTime = 2f, attackRange = 1f, moveSpeed = 3f;
    public int damage;
    public Transform target;
    public bool isArcher;
    private NavMeshAgent agent;
    private Animator anim;
    private readonly int _attack = Animator.StringToHash("Attack");
    private readonly int _velocity = Animator.StringToHash("Velocity");
    private readonly int _death = Animator.StringToHash("Death");
    [SerializeField] private Canvas canvas;
    private Camera mainCamera;
    public EnemySpawner spawner;
    public float searchRange;
    private bool isAttacking = false;
    public int maxUnits=15;
    public int enemyID;
    public float maxRange=15;
    private Collider[] hitColliders;
    [SerializeField] private LayerMask searchLayers;
    public EnemyType enemyType;

    public void Initialize()
    {
        gameObject.SetActive(true);
    }

    public void ResetEnemy()
    {
        gameObject.SetActive(false);
    }

    private void OnEnable()
    {
        isAttacking = false;
        target = null;
        spawner?.charge.AddListener(StartAttacking);
    }

    private void OnDisable()
    {
        spawner?.charge.RemoveListener(StartAttacking);
        agent.enabled = true;
    }

    void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
        anim = GetComponent<Animator>();
        mainCamera = Camera.main;
        //spawner = FindFirstObjectByType<EnemySpawner>();
    }
    
    void StartAttacking()
    {
        searchRange = maxRange;
        isAttacking = true; // Only set isAttacking to true, no movement logic here
    }

    void Update()
    {
        // Check for nearby targets even if isAttacking is false
        if (!isAttacking)
        {
            CheckForNearbyTargets();
        }

        // Only move and attack if isAttacking is true
        if (isAttacking)
        {
            Movement();
        }

        canvas.transform.LookAt(mainCamera.transform);
        Animating(agent.velocity.magnitude);
    }

    void CheckForNearbyTargets()
    {
        // Define a sphere to check for targets within search range
        Collider[] hitColliders = new Collider[maxUnits]; // Use a reasonable size for the array
        int hitCount = Physics.OverlapSphereNonAlloc(transform.position, searchRange, hitColliders, searchLayers);

        Transform nearestTarget = null;
        float shortestDistance = Mathf.Infinity;

        // Iterate through all the colliders found
        for (int i = 0; i < hitCount; i++)
        {
            // Skip null or inactive colliders
            if (hitColliders[i] == null || !hitColliders[i].gameObject.activeInHierarchy)
                continue;

            // Skip if the collider is this enemy itself
            if (hitColliders[i].transform == transform)
                continue;

            // Calculate the distance to the current collider
            float distance = Vector3.Distance(transform.position, hitColliders[i].transform.position);

            // Skip if the current collider is not closer than the previous nearest target
            if (distance >= shortestDistance)
                continue;

            // Check if the target is valid (player unit/base or another enemy type)
            if (IsValidTarget(hitColliders[i].transform))
            {
                // Update the nearest target and shortest distance
                shortestDistance = distance;
                nearestTarget = hitColliders[i].transform;
            }
        }

        // If a target is found within search range, set isAttacking to true
        if (nearestTarget != null)
        {
            target = nearestTarget;
            isAttacking = true; // Start attacking
        }
    }

    void Movement()
    {
        if (target == null || !target.gameObject.activeInHierarchy)
        {
            FindTarget(); // Find a new target if the current one is destroyed or inactive
        }

        if (target != null && target.gameObject.activeInHierarchy)
        {
            HandleAttackTarget();
        }
        else
        {
            StopAttacking(); // Stop attacking if no valid target is found
        }
    }

    void HandleAttackTarget()
    {
        float distanceToTarget = Vector3.Distance(transform.position, target.position);
        if (distanceToTarget <= attackRange)
        {
            AttackTarget();
        }
        else
        {
            ChaseTarget();
        }
    }

    void ChaseTarget()
    {
        anim.SetBool(_attack, false);
        if (agent.enabled)
        {
            agent.isStopped = false;
            agent.updateRotation = true;
            agent.SetDestination(target.position);
        }
    }

    void Animating(float velocity)
    {
        anim.SetFloat(_velocity, velocity);
    }

    private void AttackTarget()
    {
        if (target == null || !target.gameObject.activeInHierarchy)
        {
            StopAttacking(); // Exit if no target
            return;
        }

        transform.LookAt(target);
        anim.SetBool(_attack, true);

        if (isArcher)
        {
            anim.SetFloat("Speed", .6f);
        }

        if (agent.enabled)
        {
            agent.isStopped = true;
            agent.updateRotation = false;
        }
    }

    public void StopAttacking()
    {
        anim.SetBool(_attack, false);
        if (agent.enabled)
        {
            agent.isStopped = false;
            agent.updateRotation = true;
        }

        target = null; // Clear the target
    }

    public void Death()
    {
        isAttacking = false;
        anim.SetBool(_attack, false);
        agent.enabled = false;
        anim.SetBool(_death, true);
    }

    public void OnDeathAnimationEnd()
    {
        spawner.EnemyDefeated(this);
        ResetEnemy();
    }

    public int GetEnemyID()
    {
        return enemyID;
    }

    public void Damaging()
    {
        if (target == null || !target.gameObject.activeInHierarchy)
            return;

        // Damage player units or bases
        if (target.TryGetComponent<UnitHealth>(out UnitHealth unitHealth))
        {
            unitHealth.TakeDamage(damage);
        }
        else if (target.TryGetComponent<BaseHealth>(out BaseHealth baseHealth))
        {
            baseHealth.TakeDamage(damage);
        }
        // Damage other enemy types
        else if (target.TryGetComponent<EnemyAI>(out EnemyAI enemyAI) && enemyAI.enemyType != this.enemyType)
        {
            // Assuming EnemyAI has a Health component or similar
            if (enemyAI.TryGetComponent<EnemyHealth>(out EnemyHealth enemyHealth))
            {
                enemyHealth.TakeDamage(damage);
            }
        }
    }

    void FindTarget()
    {
        // Define a sphere to check for targets within search range
        hitColliders = new Collider[maxUnits]; // Use a reasonable size for the array
        int hitCount = Physics.OverlapSphereNonAlloc(transform.position, searchRange, hitColliders, searchLayers);

        Transform nearestTarget = null;
        float shortestDistance = Mathf.Infinity;

        // Iterate through all the colliders found
        for (int i = 0; i < hitCount; i++)
        {
            // Skip null or inactive colliders
            if (hitColliders[i] == null || !hitColliders[i].gameObject.activeInHierarchy)
                continue;

            // Skip if the collider is this enemy itself
            if (hitColliders[i].transform == transform)
                continue;

            // Calculate the distance to the current collider
            float distance = Vector3.Distance(transform.position, hitColliders[i].transform.position);

            // Skip if the current collider is not closer than the previous nearest target
            if (distance >= shortestDistance)
                continue;

            // Check if the target is valid (player unit/base or another enemy type)
            if (IsValidTarget(hitColliders[i].transform))
            {
                // Update the nearest target and shortest distance
                shortestDistance = distance;
                nearestTarget = hitColliders[i].transform;
            }
        }

        // Set the nearest target or clear it if no target is found
        if (nearestTarget != null)
        {
            target = nearestTarget;
        }
        else
        {
            StopAttacking(); // Stop attacking if no target is found
        }
    }

    bool IsValidTarget(Transform potentialTarget)
    {
        // Check if the target is a player unit or base
        if (potentialTarget.CompareTag("PlayerUnit") || potentialTarget.CompareTag("PlayerBase"))
        {
            return true;
        }

        // Check if the target is another enemy of a different type
        if (potentialTarget.TryGetComponent<EnemyAI>(out EnemyAI enemyAI))
        {
            // Attack other enemy types only if they are of a different type
            return enemyAI.enemyType != this.enemyType;
        }

        return false;
    }
}