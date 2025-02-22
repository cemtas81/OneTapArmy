using UnityEngine;
using UnityEngine.UI;

public class UnitCardManager : MonoBehaviour
{
    public GameObject[] unitPrefabs; // Array of unit prefabs
    public Button[] unitCards; // Array of unit card buttons
   

    private void Start()
    {
       
    }
    private void OnEnable()
    {
        Time.timeScale = 0f; // Pause the game
    }
    private void OnDisable()
    {
        Time.timeScale = 1f; // Resume the game 
    }
    public void OnUnitCardSelected(int cardIndex)
    {
       
    }
}