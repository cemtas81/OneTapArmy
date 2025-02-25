
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.EventSystems;

public class UnitMovement : MonoBehaviour
{
    public float speed = 3f;
    public float attackRange = 2f;
    public int attackDamage = 10, maxUnit;
    private Vector3 targetPosition;
    private TapController tapController;
    private Transform attackTarget;
    private NavMeshAgent agent;
    private Animator anim;
    private readonly int _attack = Animator.StringToHash("Attack");
    private readonly int _velocity = Animator.StringToHash("Velocity");
    private readonly int _death = Animator.StringToHash("Death");
    public LayerMask layerMask;
    [SerializeField] private Canvas canvas;
    [SerializeField] private float distanceMultiplier;
    private Camera mainCamera;
    private UnitSpawner spawner;
    private bool isDead;
    private float distanceToTarget;

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
        tapController = FindFirstObjectByType<TapController>();
        tapController?.move.AddListener(SetTarget);

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
        isDead =true;
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
        anim.SetFloat(_velocity, velocity);
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
        if (distanceToTarget <= attackRange*distanceMultiplier)
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
        agent.SetDestination(attackTarget.position);
    }
    private void AttackTarget()
    {
        if (attackTarget == null || !attackTarget.gameObject.activeInHierarchy) return; // Exit if no target
        agent.updateRotation = false;
        agent.isStopped = true;
        transform.LookAt(attackTarget);      
        anim.SetBool(_attack, true);    
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
        Collider[] hitColliders = new Collider[maxUnit];
        int hitCount = Physics.OverlapSphereNonAlloc(transform.position, attackRange, hitColliders, layerMask);

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