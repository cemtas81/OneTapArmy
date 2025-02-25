using UnityEngine;
using UnityEngine.AI;

public class EnemyAI : MonoBehaviour
{
    public float waitTime = 2f, attackRange = 1f, moveSpeed = 3f;
    public int damage;
    public float attackInterval = 1f; // Interval between attacks
    public Transform target;
    public bool isArcher;
    private NavMeshAgent agent;
    private Animator anim;
    private readonly int _attack = Animator.StringToHash("Attack");
    private readonly int _velocity = Animator.StringToHash("Velocity");
    private readonly int _death = Animator.StringToHash("Death");
    [SerializeField] private Canvas canvas;
    private Camera mainCamera;
    private EnemySpawner spawner;
    public float distanceMultiplier;
    private bool isAttacking = false;

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
       
        // Wait for a few seconds before attacking
        //Invoke(nameof(StartAttacking), waitTime);
        spawner.charge.AddListener(StartAttacking);
    }
    private void OnDisable()
    {
        spawner.charge.RemoveListener(StartAttacking);
        agent.enabled = true;
    }
    void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
        anim = GetComponent<Animator>();
        mainCamera = Camera.main;
        spawner = FindFirstObjectByType<EnemySpawner>();
        
    }

    void StartAttacking()
    {
        isAttacking = true;
        FindTarget();
    }

    void Update()
    {
        if (isAttacking)
        {
            Movement();
        }
        canvas.transform.LookAt(mainCamera.transform);
        Animating(agent.velocity.magnitude);
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
        if (distanceToTarget <= attackRange * distanceMultiplier)
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
        else 
        {
            transform.LookAt(target);
            anim.SetBool(_attack, true);
        }
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

    public void Damaging()
    {
        if (target != null && target.gameObject.activeInHierarchy && target.TryGetComponent<UnitHealth>(out UnitHealth unitHealth))
        {
            unitHealth.TakeDamage(damage);
        }
        else if (target != null && target.gameObject.activeInHierarchy && target.TryGetComponent<BaseHealth>(out BaseHealth baseHealth))
        {
            baseHealth.TakeDamage(damage);
        }
    }

    public void FindTarget()
    {
        // Find all active player units and the player base
        GameObject[] playerUnits = GameObject.FindGameObjectsWithTag("PlayerUnit");
        GameObject playerBase = GameObject.FindGameObjectWithTag("PlayerBase");

        // Find the nearest target among player units and the player base
        Transform nearestTarget = FindNearestTarget(playerUnits, playerBase);

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

    private Transform FindNearestTarget(GameObject[] playerUnits, GameObject playerBase)
    {
        Transform nearestTarget = null;
        float shortestDistance = Mathf.Infinity;

        // Check player units
        nearestTarget = FindNearestInArray(playerUnits, ref shortestDistance);

        // Check player base
        if (playerBase != null && playerBase.activeInHierarchy)
        {
            float baseDistance = Vector3.Distance(transform.position, playerBase.transform.position);
            if (baseDistance < shortestDistance)
            {
                nearestTarget = playerBase.transform;
            }
        }

        return nearestTarget;
    }

    private Transform FindNearestInArray(GameObject[] targets, ref float shortestDistance)
    {
        Transform nearestTarget = null;

        foreach (GameObject target in targets)
        {
            // Skip null or inactive targets
            if (target == null || !target.activeInHierarchy)
            {
                continue;
            }

            // Calculate distance to the target
            float distance = Vector3.Distance(transform.position, target.transform.position);

            // Update nearest target if this target is closer
            if (distance < shortestDistance)
            {
                shortestDistance = distance;
                nearestTarget = target.transform;
            }
        }

        return nearestTarget;
    }
}