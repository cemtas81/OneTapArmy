
using UnityEngine;
using UnityEngine.UI;

public class XPManager : MonoBehaviour
{
    public Slider xpBar; 
    public int maxXP = 100; 
    private int currentXP = 0;
    public GameObject newCardPanel; 
    void Start()
    {
        xpBar.maxValue = maxXP;
        xpBar.value = currentXP;
    }

    public void AddXP(int amount)
    {
        currentXP += amount;
        xpBar.value = currentXP;

        if (currentXP >= maxXP)
        {
            ShowNewCards(); // Call this method to display new cards
            currentXP = 0; // Reset XP
        }
    }

    void ShowNewCards()
    {
        // Logic to display new unit cards
        Debug.Log("New cards available!");
        newCardPanel.SetActive(true);
    }
    
}