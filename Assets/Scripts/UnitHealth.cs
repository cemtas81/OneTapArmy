
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UnitHealth : MonoBehaviour, IHealth, IUpgrade
{

    public int maxHealth = 100, health = 100;
    public List<Renderer> Renderers = new List<Renderer>();
    public Material deadMaterial;
    public Material aliveMaterial;
    private UnitMovement unitMovement;
    [SerializeField] private Slider healthBar;
    private bool isDead;

    private void Start()
    {
        unitMovement = GetComponent<UnitMovement>();

    }

    public void TakeDamage(int damage, bool isEnemy)
    {
        health -= damage;
        healthBar.value = health;
        if (health <= 0 && !isDead)
        {
            isDead = true;
            unitMovement?.Death();
            Renderers.ForEach(renderer => renderer.sharedMaterial = deadMaterial);
        }
    }
    private void OnDisable()
    {
        health = maxHealth;
        healthBar.value = health;
        isDead = false;
        Renderers.ForEach(renderer => renderer.sharedMaterial = aliveMaterial);
    }
    public void Upgrade(int level)
    {
        maxHealth = (int)(maxHealth * (1 + level / 100f));
    }
}
