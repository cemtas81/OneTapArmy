using UnityEngine;
using UnityEngine.AI;

public class EnemyAI : MonoBehaviour
{
    public float waitTime = 2f, attackRange = 1f, moveSpeed = 3f;
    public int damage;
    public float attackInterval = 1f; // Interval between attacks
    private Transform target;
    private bool isAttacking = false;
    private NavMeshAgent agent;
    private Animator anim;
    private readonly int _attack = Animator.StringToHash("Attack");
    private readonly int _velocity = Animator.StringToHash("Velocity");

    public void Initialize()
    {
       
        gameObject.SetActive(true);
    }

    public void ResetEnemy()
    {
        
        gameObject.SetActive(false);
    }

    void Start()
    {
        // Wait for a few seconds before attacking
        Invoke(nameof(StartAttacking), waitTime);
        agent = GetComponent<NavMeshAgent>();
        anim = GetComponent<Animator>();
    }

    void StartAttacking()
    {
        isAttacking = true;
        FindTarget();
    }

    void Update()
    {
        if (isAttacking && target != null)
        {
            float distanceToTarget = Vector3.Distance(transform.position, target.position);
            if (distanceToTarget <= attackRange)
            {
                // Stop moving and start attacking
                agent.isStopped = true;
                AttackTarget();
            }
            else
            {
                // Move toward the target
                agent.isStopped = false;
                agent.SetDestination(target.position);
                StopAttacking(); // Stop attacking if out of range
            }
        }
        else if (isAttacking)
        {
            FindTarget(); // Find a new target if the current one is destroyed
        }

        Animating(agent.velocity.magnitude);
    }

    void Animating(float velocity)
    {
        anim.SetFloat(_velocity, velocity);
    }

    private void AttackTarget()
    {
        if (target == null) return; // Exit if no target

        // Look at the target
        Vector3 direction = (target.position - transform.position).normalized;
        Quaternion lookRotation = Quaternion.LookRotation(direction);
        transform.rotation = Quaternion.Slerp(transform.rotation, lookRotation, Time.deltaTime * 10f);

        if (!anim.GetBool(_attack))
        {
            anim.SetBool(_attack, true); 
        }
    }

    private void StopAttacking()
    {
        if (anim.GetBool(_attack))
        {
            anim.SetBool(_attack, false); 
        }
    }

    // Called by animation event
    public void Damaging()
    {
        if (target != null && target.TryGetComponent<UnitHealth>(out UnitHealth unitHealth))
        {
            unitHealth.TakeDamage(damage);
            Debug.Log($"Dealing {damage} damage to {target.name}");
        }
    }

    void FindTarget()
    {
        // Find the nearest player unit or base
        GameObject[] playerUnits = GameObject.FindGameObjectsWithTag("PlayerUnit");
        GameObject playerBase = GameObject.FindGameObjectWithTag("PlayerBase");

        float shortestDistance = Mathf.Infinity;
        Transform nearestTarget = null;

        // Check player units
        foreach (GameObject unit in playerUnits)
        {
            if (unit != null) 
            {
                float distance = Vector3.Distance(transform.position, unit.transform.position);
                if (distance < shortestDistance)
                {
                    shortestDistance = distance;
                    nearestTarget = unit.transform;
                }
            }
        }

        // Check player base
        if (playerBase != null) // Null check for player base
        {
            float baseDistance = Vector3.Distance(transform.position, playerBase.transform.position);
            if (baseDistance < shortestDistance)
            {
                nearestTarget = playerBase.transform;
            }
        }

        // Set the nearest target as the attack target
        if (nearestTarget != null)
        {
            target = nearestTarget;
        }
        else
        {
            target = null; // Clear attack target if no target is found
            StopAttacking(); 
        }
    }
}