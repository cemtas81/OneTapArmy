using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[System.Serializable]
public class SpawnerData
{
    public Transform spawnPoint; 
    public List<EnemyAI> enemyPrefabs; 
    public float spawnInterval = 5f; 
    public float attackInterval = 3f; 
    public int maxEnemies = 10; 
    public EnemyType enemyType;
    public Slider spawnCooldownSlider;
}