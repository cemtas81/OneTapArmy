using UnityEngine;

public class BaseHealth : MonoBehaviour, IHealth
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
}
