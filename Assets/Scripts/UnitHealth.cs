using System.Linq;
using UnityEngine;

public class UnitHealth : MonoBehaviour,IHealth
{
    private int health = 100;
    public int maxHealth = 100;
    private TapController tapController;
    private UnitSpawner spawner;
    private UnitMovement unitMovement;

    private void Start()
    {
        tapController = FindFirstObjectByType<TapController>();
        unitMovement = GetComponent<UnitMovement>();
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
            UnitMovement unit = this.GetComponent<UnitMovement>();
            unitMovement?.ResetUnit();
            tapController?.RemoveUnit(unit);
            spawner?.UnitDefeated(unit);
        }
    }
    
}
