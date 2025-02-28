
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class XPManager : MonoBehaviour
{
    public Slider xpBar; 
    public int maxXP = 100; 
    private int currentXP = 0,level;
    public GameObject newCardPanel;
    public TextMeshProUGUI xpLevel;
    void Start()
    {
        xpBar.maxValue = maxXP;
        xpBar.value = currentXP;
        newCardPanel.SetActive(true);
        level = 1;
        xpLevel.text = level.ToString();
    }

    public void AddXP(int amount)
    {
        currentXP += amount;
        xpBar.value = currentXP;

        if (currentXP >= maxXP)
        {
            ShowNewCards();
            currentXP = 0; // Reset XP
            xpBar.value = currentXP;
        }
    }

    void ShowNewCards()
    {

        newCardPanel.SetActive(true);
        level++;
        xpLevel.text = level.ToString();    
    }
    
}