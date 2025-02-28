using UnityEngine;
using UnityEngine.UI;

public class BaseHealth : MonoBehaviour, IHealth, IUpgrade
{
    public int health;
    public Slider healthSlider;
    public int maxHealth;
    private void Start()
    {

        health = maxHealth;
        healthSlider.maxValue = maxHealth;
    }
    public void TakeDamage(int damage, bool isEnemy)
    {
        health -= damage;
        healthSlider.value = health;
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

