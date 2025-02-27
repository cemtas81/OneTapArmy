using UnityEngine;

public class BaseHealth : MonoBehaviour, IHealth, IUpgrade
{
    public int health;

    public int maxHealth;
    private void Start()
    {

        health = maxHealth;

    }
    public void TakeDamage(int damage)
    {
        health -= damage;
        if (health <= 0)
        {

            Destroy(gameObject);
        }
    }
    public void Upgrade(int level)
    {
        maxHealth = (int)(maxHealth * (1 + level / 100f));
    }
}

