using UnityEngine;

public class EnemyBaseHealth : MonoBehaviour, IHealth
{
    private int health;
    public int xp; 
    private XPManager xpManager; 
    public int maxHealth;

    private void Start()
    {
        xpManager = FindFirstObjectByType<XPManager>(); 
        health = maxHealth;
        xpManager = FindFirstObjectByType<XPManager>();
    }
    public void TakeDamage(int damage)
    {
        health -= damage;
        if (health <= 0)
        {
            xpManager?.AddXP(xp); // Add XP when an enemy is destroyed
            Destroy(gameObject);
        }
    }
  
}


