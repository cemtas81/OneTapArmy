using UnityEngine;

[CreateAssetMenu]
public class Card : ScriptableObject
{
    public int cardNumber;
    public string cardName;
    public Sprite cardImage;
    public Sprite background;
    public int level;
    public int multiplier, initialHealth;
    public int multiplier2, initialDamage;
    public string multiplierType;
    public string multiplierType2;
    public GameObject unitPrefab;

}