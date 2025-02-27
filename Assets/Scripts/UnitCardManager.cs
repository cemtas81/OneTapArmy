using System.Collections.Generic;
using UnityEngine;

public class UnitCardManager : MonoBehaviour
{
    public List<GameObject> Cards;
    private bool canShowCastleCard=false;
    public int numberOfObjectsToActivate = 3;
    public GameObject castleCard;

    private void OnEnable()
    {

        Time.timeScale = 0f;

        if (Cards == null || Cards.Count == 0)
        {
            Debug.LogError("No GameObjects assigned in the list!");
            return;
        }

        // Deactivate all GameObjects in the list first
        DeactivateAllGameObjects();

        // Shuffle the list to randomize the selection
        ShuffleList(Cards);

        for (int i = 0; i < numberOfObjectsToActivate; i++)
        {
            if (Cards[i] != null)
            {
                Cards[i].SetActive(true);
            }
        }
    }

    private void OnDisable()
    {

        Time.timeScale = 1f;
        if (!canShowCastleCard)
        {
            canShowCastleCard = true;
            OpenCastleCard();
        }
    }
    void OpenCastleCard()
    {
        Cards.Add(castleCard);
    }
    // Helper method to shuffle the list using Fisher-Yates algorithm
    private void ShuffleList(List<GameObject> list)
    {
        for (int i = list.Count - 1; i > 0; i--)
        {
            int randomIndex = Random.Range(0, i + 1);
            GameObject temp = list[i];
            list[i] = list[randomIndex];
            list[randomIndex] = temp;
        }
    }

    // Helper method to deactivate all GameObjects in the list
    private void DeactivateAllGameObjects()
    {
        foreach (GameObject obj in Cards)
        {
            if (obj != null)
            {
                obj.SetActive(false);
            }
        }
    }
}