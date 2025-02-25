using UnityEngine;
using UnityEngine.UI;

public class EnemyHealth : MonoBehaviour,IHealth
{
    public int health ;
    public int maxHealth ;
    public int xp; // Damage taken by the base
    private XPManager xpManager; // Reference to the XPManager
    private EnemyAI enemyAI;
    [SerializeField] private Slider healthBar;
    private bool isDead;

    private void Start()
    {
        xpManager = FindFirstObjectByType<XPManager>(); 
        enemyAI = GetComponent<EnemyAI>();
       
    }

    public void TakeDamage(int damage)
    {
        healthBar.value = health;
        health -= damage;
        if (health <= 0&&!isDead)
        {
            isDead= true;
            enemyAI?.Death();
            xpManager?.AddXP(xp); // Add XP when an enemy is destroyed
          
        }
    }
    private void OnDisable()
    {
        health = maxHealth;
        healthBar.value = health;
        isDead = false;
    }
}
