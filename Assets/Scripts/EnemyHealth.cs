using UnityEngine;

public class EnemyHealth : MonoBehaviour,IHealth
{
    private int health ;
    public int maxHealth ;
    public int xp; // Damage taken by the base
    private XPManager xpManager; // Reference to the XPManager
    private EnemySpawner spawner;
    private EnemyAI enemyAI;

    private void Start()
    {
        xpManager = FindFirstObjectByType<XPManager>(); 
        spawner = FindFirstObjectByType<EnemySpawner>(); 
    }
    private void OnEnable()
    {
        health = maxHealth;
    }
    public void TakeDamage(int damage)
    {
        health -= damage;
        if (health <= 0)
        {
            spawner?.EnemyDefeated(this.GetComponent<EnemyAI>());
            enemyAI?.ResetEnemy();
            xpManager?.AddXP(xp); // Add XP when an enemy is destroyed
            health = maxHealth;
        }
    }
}
