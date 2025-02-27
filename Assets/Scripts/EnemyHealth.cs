using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class EnemyHealth : MonoBehaviour, IHealth
{
    public int health;
    public int maxHealth;
    public int xp; // Damage taken by the base
    private XPManager xpManager; // Reference to the XPManager
    private EnemyAI enemyAI;
    [SerializeField] private Slider healthBar;
    private bool isDead;
    public List<Renderer> Renderers = new List<Renderer>();
    public Material deadMaterial;
    public Material aliveMaterial;
    public List<Material> materials;
    private EnemyType enemyType;
    private void Start()
    {
        xpManager = FindFirstObjectByType<XPManager>();
        enemyAI = GetComponent<EnemyAI>();
        enemyType=enemyAI.enemyType;
        InitializeColor();
    }
    void InitializeColor()
    {
       
        switch (enemyType)
        {
            case EnemyType.Red:
                Debug.Log("Red");
                aliveMaterial = materials[0];
                break;
            case EnemyType.Yellow:
                aliveMaterial = materials[1];
                Debug.Log("Yellow");
                break;
            case EnemyType.Purple:
                aliveMaterial = materials[2];
                Debug.Log("Purple");
                break;
        }
        Renderers.ForEach(renderer => renderer.sharedMaterial = aliveMaterial);
    }
    public void TakeDamage(int damage)
    {
        healthBar.value = health;
        health -= damage;
        if (health <= 0 && !isDead)
        {
            isDead = true;
            enemyAI?.Death();
            xpManager?.AddXP(xp); // Add XP when an enemy is destroyed
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
}
