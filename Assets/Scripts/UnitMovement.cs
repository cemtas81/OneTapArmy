using UnityEngine;
using UnityEngine.AI;
using UnityEngine.EventSystems;

public class UnitMovement : MonoBehaviour, IUpgrade
{
    public float attackRange = 2f;
    public int attackDamage = 10, maxUnit = 10;
    private Vector3 targetPosition;
    private TapController tapController;
    public Transform attackTarget;
    private NavMeshAgent agent;
    private Animator anim;
    private readonly int _attack = Animator.StringToHash("Attack");
    private readonly int _velocity = Animator.StringToHash("Velocity");
    private readonly int _death = Animator.StringToHash("Death");
    public LayerMask layerMask;
    [SerializeField] private Canvas canvas;
    [SerializeField] private float searchDistance = 10f;
    private Camera mainCamera;
    private UnitSpawner spawner;
    private bool isDead;
    private float distanceToTarget;
    public bool isArcher, isGiant;
    public int unitID; // Unique identifier for the unit
    public int cardID; // Unique identifier for the card    
    private Collider[] hitColliders;
    
    // Add a timer to periodically refresh target search
    private float targetRefreshTimer = 0f;
    private float targetRefreshInterval = 0.5f;

    private bool isAttacking = false;
    public bool isHorse;

    private void Start()
    {
        targetPosition = transform.position;
        agent = GetComponent<NavMeshAgent>();
        anim = GetComponent<Animator>();
        mainCamera = Camera.main;
        spawner = FindFirstObjectByType<UnitSpawner>();

        // Initialize hitColliders array
        hitColliders = new Collider[maxUnit];
    }

    private void OnEnable()
    {
        attackTarget = null;
        isDead = false;
        isAttacking = false;
        tapController = FindFirstObjectByType<TapController>();
        tapController?.move.AddListener(SetTarget);

        if (agent != null)
        {
            agent.isStopped = false;
            agent.updateRotation = true;
        }
    }

    private void OnDisable()
    {
        tapController?.move.RemoveListener(SetTarget);
    }

    public void Upgrade(int level)
    {
        attackDamage = (int)(attackDamage * (1 + level / 100f));
    }

    public void Initialize()
    {
        gameObject.SetActive(true);
    }

    public void ResetUnit()
    {
        gameObject.SetActive(false);
    }

    public void SetTarget(Vector3 newTarget)
    {
        // Don't change target if the unit is currently attacking
        if (isAttacking || EventSystem.current.IsPointerOverGameObject())
            return;

        targetPosition = newTarget;
        targetPosition.y = transform.position.y; // Keep the same Y position
        attackTarget = null; // Reset attack target when a new target is set

        if (agent != null && agent.enabled)
        {
            StopAttacking();
            agent.SetDestination(targetPosition);
        }
    }

    public void Death()
    {
        tapController?.move.RemoveListener(SetTarget);
        isDead = true;
        isAttacking = false;
        anim.SetBool(_attack, false);

        if (agent != null && agent.enabled)
        {
            agent.isStopped = true;
            agent.updateRotation = false;
        }

        anim.SetBool(_death, true);
    }

    public void OnDeathEnd()
    {
        spawner.UnitDefeated(this);
        ResetUnit();
    }

    void Update()
    {
        if (isDead)
        {
            return; // Don't do anything else if dead
        }

        // Only continue if agent is valid
        if (agent == null || !agent.enabled)
        {
            return;
        }

        // Refresh search for targets periodically rather than every frame
        targetRefreshTimer += Time.deltaTime;
        if (targetRefreshTimer >= targetRefreshInterval)
        {
            targetRefreshTimer = 0f;

            if (attackTarget == null || !attackTarget.gameObject.activeInHierarchy)
            {
                FindAttackTarget();
            }
        }

        Movement();

        if (canvas != null && mainCamera != null)
        {
            canvas.transform.LookAt(mainCamera.transform);
        }

        if (agent != null)
        {
            Animating(agent.velocity.magnitude);
        }
    }

    void Animating(float velocity)
    {
        if (anim == null) return;

        if (!isGiant)
        {
            anim.SetFloat(_velocity, velocity);
        }
        else
        {
            anim.SetFloat(_velocity, velocity * .35f);
        }
    }

    void Movement()
    {
        // Check if target is still valid
        if (attackTarget != null && !attackTarget.gameObject.activeInHierarchy)
        {
            attackTarget = null;
            isAttacking = false;
        }

        if (attackTarget != null)
        {
            HandleAttackTarget();
        }
        else
        {
            // Make sure we're moving to target position if no enemy
            if (!agent.pathPending && agent.remainingDistance < 0.1f)
            {
                // We've reached our destination and have no target
                agent.isStopped = true;
            }
            else
            {
                StopAttacking();
            }
        }
    }

    private void HandleAttackTarget()
    {
        if (attackTarget == null) return;

        distanceToTarget = Vector3.Distance(transform.position, attackTarget.position);

        if (distanceToTarget <= attackRange)
        {
            AttackTarget();
        }
        else
        {
            ChaseTarget();
        }
    }

    private void ChaseTarget()
    {
        isAttacking = false;

        if (anim != null)
        {
            anim.SetBool(_attack, false);
        }

        if (agent != null && agent.enabled && attackTarget != null)
        {
            agent.isStopped = false;
            agent.updateRotation = true;
            agent.SetDestination(attackTarget.position);
        }
    }

    private void AttackTarget()
    {
        if (attackTarget == null || !attackTarget.gameObject.activeInHierarchy)
        {
            StopAttacking();
            return;
        }

        isAttacking = true;
       
        anim.SetBool(_attack, true);

        if (isArcher)
        {
            anim.SetFloat("Speed", .6f);
        }
        else
        {
            transform.LookAt(attackTarget);
        }
        if (isHorse)
        {
            // Turn the unit's direction to face away from the target
            transform.Rotate(0, 180, 0); 
            
        }

        if (agent != null && agent.enabled)
        {
            agent.isStopped = true;
            agent.updateRotation = false;
        }
    }

    private void StopAttacking()
    {
        isAttacking = false;

        if (anim != null)
        {
            anim.SetBool(_attack, false);
        }

        if (agent != null && agent.enabled)
        {
            agent.isStopped = false;
            agent.updateRotation = true;
            agent.SetDestination(targetPosition);
        }
    }

    public void Damaging()
    {
        if (attackTarget != null && attackTarget.gameObject.activeInHierarchy)
        {
            IHealth healthComponent = attackTarget.GetComponent<IHealth>();
            if (healthComponent != null)
            {
                healthComponent.TakeDamage(attackDamage, true);
            }
        }
    }

    void FindAttackTarget()
    {
        // Don't search for targets if already attacking
        if (isAttacking)
            return;

        if (hitColliders == null)
        {
            hitColliders = new Collider[maxUnit];
        }

        int hitCount = Physics.OverlapSphereNonAlloc(transform.position, searchDistance, hitColliders, layerMask);

        Transform nearestTarget = null;
        float shortestDistance = Mathf.Infinity;

        for (int i = 0; i < hitCount; i++)
        {
            if (hitColliders[i] != null && hitColliders[i].gameObject.activeSelf) // Check for null and active enemies
            {
                float distance = Vector3.Distance(transform.position, hitColliders[i].transform.position);
                UpdateNearestTarget(ref nearestTarget, ref shortestDistance, hitColliders[i].transform, distance);
            }
        }

        SetAttackTarget(nearestTarget);

        // Clear the array after use to avoid stale references
        for (int i = 0; i < hitColliders.Length; i++)
        {
            hitColliders[i] = null;
        }
    }

    private void UpdateNearestTarget(ref Transform nearestTarget, ref float shortestDistance, Transform currentTarget, float distance)
    {
        if (distance < shortestDistance)
        {
            shortestDistance = distance;
            nearestTarget = currentTarget;
        }
    }

    private void SetAttackTarget(Transform target)
    {
        attackTarget = target;

        // Immediately update our movement if we found a target and we're not attacking
        if (attackTarget != null && agent != null && agent.enabled && !isAttacking)
        {
            agent.SetDestination(attackTarget.position);
        }
    }

}