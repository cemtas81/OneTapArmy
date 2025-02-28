using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CardView : MonoBehaviour
{
    public List<Card> cards; // List of cards
    public Image cardImage;
    public Image background;
    public TextMeshProUGUI cardName;
    public TextMeshProUGUI level;
    public TextMeshProUGUI multiplier;

    private UnitSpawner unitSpawner;
    private UnitCardManager unitCardManager;
    private int currentCardIndex = 0; // Tracks the current card index

    private void Awake()
    {
        unitSpawner = FindFirstObjectByType<UnitSpawner>();
        unitCardManager = FindFirstObjectByType<UnitCardManager>();

        // Initialize the UI with the first card (if the list is not empty)
        if (cards.Count > 0)
        {
            UpdateCardUI(cards[currentCardIndex]);
        }
        else
        {
            Debug.LogWarning("No cards available!");
        }
    }

    public void OnCardSelected()
    {
        // Check if there are no more cards to select
        if (currentCardIndex >= cards.Count)
        {
            Debug.LogWarning("No more cards to select!");
            return;
        }

        // Get the current card
        Card currentCard = cards[currentCardIndex];
        UnitMovement unit = currentCard.unitPrefab.GetComponent<UnitMovement>();
        // Apply the card's multiplier to the unit spawner
        //currentCard.ApplyMultiplier(unitSpawner);

        // Toggle the unit card manager
        unitCardManager.gameObject.SetActive(!unitCardManager.gameObject.activeSelf);

        // Apply upgrades to the unitPrefab 
        //IUpgrade[] upgrades = unit.GetComponents<IUpgrade>();
        //if (upgrades != null && upgrades.Length > 0)
        //{
        //    foreach (var upgrade in upgrades)
        //    {
        //        upgrade.Upgrade(currentCard.multiplier);
        //    }
        //}
        // Notify the unit spawner that a card has been selected
        unitSpawner.AddUnitPrefab(unit,currentCard.cardNumber);
        unitSpawner.OnSelect();
        // Move to the next card
        currentCardIndex++;

        // Update the UI with the next card (if available)
        if (currentCardIndex < cards.Count)
        {
            UpdateCardUI(cards[currentCardIndex]);
        }
        else
        {
            Debug.Log("All cards have been used.");
        }
    }

    private void UpdateCardUI(Card card)
    {
     
        cardImage.sprite = card.cardImage;
        background.sprite = card.background;
        cardName.text = card.cardName;
        level.text = card.multiplierType.ToString();
        multiplier.text = card.multiplierType2.ToString();
    }
   
}