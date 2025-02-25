
using UnityEngine;
using UnityEngine.UI;

public class UnitHealth : MonoBehaviour,IHealth
{
    private int health = 100;
    public int maxHealth = 100;
    private UnitMovement unitMovement;
    [SerializeField] private Slider healthBar;
    private bool isDead;

    private void Start()
    {
        unitMovement = GetComponent<UnitMovement>();
    }

    public void TakeDamage(int damage)
    {
        health -= damage;
        healthBar.value = health;
        if (health <= 0&&!isDead)
        {
            isDead= true;
            unitMovement?.Death();
          
        }
    }
    private void OnDisable()
    {
        health = maxHealth;
        healthBar.value = health;
        isDead = false;
    }
}
