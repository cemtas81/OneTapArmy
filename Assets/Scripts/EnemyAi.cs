using UnityEngine;
using UnityEngine.AI;

public class EnemyAI : MonoBehaviour
{
    public float waitTime = 2f, attackRange = 1f, moveSpeed = 3f;
    public int damage;
    public float attackInterval = 1f; // Interval between attacks
    public Transform target;
    public bool isAttacking = false;
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
        if (isAttacking)
        {
            if (target == null || !target.gameObject.activeInHierarchy)
            {
                FindTarget(); // Find a new target if the current one is destroyed or inactive
            }

            if (target != null && target.gameObject.activeInHierarchy)
            {
                float distanceToTarget = Vector3.Distance(transform.position, target.position);
                if (distanceToTarget <= attackRange)
                {
                    AttackTarget();
                }
                else
                {
                    StopAttacking(); // Stop attacking if out of range

                }
            }
            else
            {
                StopAttacking(); // Stop attacking if no valid target is found
            }
        }

        Animating(agent.velocity.magnitude);
    }

    void Animating(float velocity)
    {
        anim.SetFloat(_velocity, velocity);
    }

    private void AttackTarget()
    {
        if (target == null || !target.gameObject.activeInHierarchy) return; // Exit if no target

        // Stop moving and start attacking
        agent.isStopped = true;
        agent.updateRotation = false;

        // Look at the target
        transform.LookAt(target);

        anim.SetBool(_attack, true);
    }

    public void StopAttacking()
    {
        anim.SetBool(_attack, false);

        // Resume movement
        agent.isStopped = false;
        agent.updateRotation = true;

        if (target != null && target.gameObject.activeInHierarchy)
        {
            agent.SetDestination(target.position); // Move towards the target
        }
    }


    public void Damaging()
    {
        if (target != null && target.gameObject.activeInHierarchy && target.TryGetComponent<UnitHealth>(out UnitHealth unitHealth))
        {
            unitHealth.TakeDamage(damage);
            Debug.Log($"Dealing {damage} damage to {target.name}");
        }
        else if (target != null && target.gameObject.activeInHierarchy && target.TryGetComponent<BaseHealth>(out BaseHealth baseHealth))
        {
            baseHealth.TakeDamage(damage);
            Debug.Log($"Dealing {damage} damage to {target.name}");
        }
    }

    public void FindTarget()
    {
        // Find the nearest player unit or base
        GameObject[] playerUnits = GameObject.FindGameObjectsWithTag("PlayerUnit");
        GameObject playerBase = GameObject.FindGameObjectWithTag("PlayerBase");

        float shortestDistance = Mathf.Infinity;
        Transform nearestTarget = null;

        // Check player units
        foreach (GameObject unit in playerUnits)
        {
            if (unit != null && unit.activeInHierarchy) // Only consider active units
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
        if (playerBase != null && playerBase.activeInHierarchy) // Null check for player base
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
            StopAttacking(); // Stop attacking if no target is found
        }
    }
}