using System.Linq;
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
    private bool isAttacking = false;
    public LayerMask layerMask;

    private void Start()
    {
        targetPosition = transform.position;
        tapController = FindFirstObjectByType<TapController>();
        tapController.units = tapController.units.Append(this).ToArray(); // Add this unit to the TapController's list
        agent = GetComponent<NavMeshAgent>();
        anim = GetComponent<Animator>();
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

    void Update()
    {
        if (attackTarget == null || !attackTarget.gameObject.activeInHierarchy)
        {
            FindAttackTarget();
        }

        if (attackTarget != null && attackTarget.gameObject.activeInHierarchy)
        {
            float distanceToTarget = Vector3.Distance(transform.position, attackTarget.position);
            if (distanceToTarget <= attackRange)
            {
                AttackTarget();
            }
            else
            {
                StopAttacking(); // Stop attacking if out of range
                //MoveToTarget(); // Move towards the target if out of range
            }
        }
        else
        {
            StopAttacking(); // Stop attacking if no target is found
            //MoveToTarget(); // Move towards the target position
        }

        Animating(agent.velocity.magnitude);
    }

    void Animating(float velocity)
    {
        anim.SetFloat(_velocity, velocity);
    }

    private void AttackTarget()
    {
        if (attackTarget == null || !attackTarget.gameObject.activeInHierarchy) return; // Exit if no target
        agent.updateRotation = false;
        agent.isStopped = true;
        transform.LookAt(attackTarget);

        if (!isAttacking)
        {
            isAttacking = true;
            anim.SetBool(_attack, true);
        }
    }

    private void StopAttacking()
    {
        isAttacking = false;
        anim.SetBool(_attack, false);
        agent.isStopped = false;
        agent.updateRotation = true;
        agent.SetDestination(targetPosition);
    }

    private void MoveToTarget()
    {
        agent.SetDestination(targetPosition);
    }

    public void Damaging()
    {
        if (attackTarget != null && attackTarget.gameObject.activeInHierarchy && attackTarget.TryGetComponent<IHealth>(out IHealth healthComponent))
        {
            healthComponent.TakeDamage(attackDamage);
            Debug.Log($"Dealing {attackDamage} damage to {attackTarget.name}");
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
            attackTarget = nearestTarget;
        }
        else
        {
            attackTarget = null; // Clear attack target if no enemy is found
        }
    }
}