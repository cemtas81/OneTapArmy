using UnityEngine;
using UnityEngine.UI;
public enum EnemyType
{
    Red,
    Yellow,
    Purple
}
public class EnemyBaseHealth : MonoBehaviour, IHealth, IUpgrade
{
    public int health;
    public int xp;
    private XPManager xpManager;
    public int maxHealth;
    public Slider healthBar;
    private void Start()
    {
        health = maxHealth;
        healthBar.maxValue = maxHealth;
        xpManager = FindFirstObjectByType<XPManager>();
    }
    public void Upgrade(int level)
    {
        maxHealth = (int)(maxHealth * (1 + level / 100f));
    }
    public void TakeDamage(int damage, bool isEnemy)
    {
        health -= damage;
        healthBar.value = health;
        if (health <= 0)
        {
            xpManager?.AddXP(xp); 
            Destroy(gameObject);
        }
    }

}


