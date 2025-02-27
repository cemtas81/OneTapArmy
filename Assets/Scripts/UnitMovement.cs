
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.EventSystems;

public class UnitMovement : MonoBehaviour, IUpgrade
{
  
    public float attackRange = 2f;
    public int attackDamage = 10, maxUnit;
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
    [SerializeField] private float searchDistance;
    private Camera mainCamera;
    private UnitSpawner spawner;
    private bool isDead;
    private float distanceToTarget;
    public bool isArcher,isGiant;
    public int unitID; // Unique identifier for the unit
    public int cardID; // Unique identifier for the card    
    private Collider[] hitColliders;

    private void Start()
    {
        targetPosition = transform.position;
        agent = GetComponent<NavMeshAgent>();
        anim = GetComponent<Animator>();
        mainCamera = Camera.main;
        spawner = FindFirstObjectByType<UnitSpawner>();

    }
    private void OnEnable()
    {
        attackTarget = null;
        isDead = false;
        tapController = FindFirstObjectByType<TapController>();
        tapController?.move.AddListener(SetTarget);

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
        if (!EventSystem.current.IsPointerOverGameObject())
        {
            targetPosition = newTarget;
            targetPosition.y = transform.position.y; // Keep the same Y position
            attackTarget = null; // Reset attack target when a new target is set
            StopAttacking(); // Stop attacking when relocating
        }
    }
    public void Death()
    {
        tapController?.move.RemoveListener(SetTarget);
        isDead = true;
        anim.SetBool(_attack, false);
        agent.isStopped = true;
        agent.updateRotation = false;
        anim.SetBool(_death, true);
    }
    public void OnDeathEnd()
    {

        spawner.UnitDefeated(this);
        ResetUnit();
    }
    void Update()
    {
        if (!isDead)
        {
            Movement();
        }

        canvas.transform.LookAt(mainCamera.transform);
        Animating(agent.velocity.magnitude);
    }

    void Animating(float velocity)
    {
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
        if (attackTarget == null || !attackTarget.gameObject.activeInHierarchy)
        {
            FindAttackTarget();
        }

        if (attackTarget != null && attackTarget.gameObject.activeInHierarchy)
        {
            HandleAttackTarget();
        }
        else
        {
            StopAttacking(); // Stop attacking if no target is found
        }
    }

    private void HandleAttackTarget()
    {
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
        anim.SetBool(_attack, false);
        agent.SetDestination(attackTarget.position);
    }
    private void AttackTarget()
    {
        if (attackTarget == null || !attackTarget.gameObject.activeInHierarchy)
        {
            StopAttacking(); // Exit if no target
            return;
        }

        transform.LookAt(attackTarget);
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

    private void StopAttacking()
    {
        anim.SetBool(_attack, false);
        agent.isStopped = false;
        agent.updateRotation = true;
        agent.SetDestination(targetPosition);
    }

    public void Damaging()
    {
        if (attackTarget != null && attackTarget.gameObject.activeInHierarchy && attackTarget.TryGetComponent<IHealth>(out IHealth healthComponent))
        {
            healthComponent.TakeDamage(attackDamage);
        }
    }

    void FindAttackTarget()
    {
        // Define a sphere to check for enemies within attack range
        hitColliders = new Collider[maxUnit];
        int hitCount = Physics.OverlapSphereNonAlloc(transform.position, searchDistance, hitColliders, layerMask);

        Transform nearestTarget = null;
        float shortestDistance = Mathf.Infinity;

        for (int i = 0; i < hitCount; i++)
        {
            if (hitColliders[i].gameObject.activeSelf) // Only consider active enemies
            {
                float distance = Vector3.Distance(transform.position, hitColliders[i].transform.position);
                UpdateNearestTarget(ref nearestTarget, ref shortestDistance, hitColliders[i].transform, distance);
            }
        }

        SetAttackTarget(nearestTarget);
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
        attackTarget = target != null ? target : null; // Clear attack target if no enemy is found
    }
}