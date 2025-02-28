using TMPro;
using Unity.VisualScripting;
using UnityEngine;

public class XDoorMultiplier : MonoBehaviour
{
    private int multiplier; 
    public TextMeshProUGUI countText; 
    private UnitSpawner spawner; 
    [SerializeField] private Transform spawnPos;
    private bool isTriggered = false;
    Collider coll;


    private void OnEnable()
    {
        
        multiplier = Random.Range(1, 4);
        countText.text = "+" + multiplier.ToString(); 
        spawner = FindFirstObjectByType<UnitSpawner>();
        coll = GetComponent<Collider>();
    }

    private void OnTriggerEnter(Collider other)
    {
        
        if (other.TryGetComponent(out UnitMovement unitMovement)&&!isTriggered)
        {
            coll.enabled = false;
            isTriggered = true;
            // Get the unit's type (unitID or cardID)
            int unitType = unitMovement.unitID; 

            // Trigger the spawner to spawn the unit `multiplier` times at the trigger position
            for (int i = 0; i < multiplier; i++)
            {
                spawner.SpawnUnitOfType(unitType, other.transform.position,this.gameObject); // Pass the unit type and spawn position
            }

            Destroy(gameObject);
        }
    }
}