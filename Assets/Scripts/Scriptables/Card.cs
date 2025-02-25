using UnityEngine;

[CreateAssetMenu]
public class Card : ScriptableObject
{
    public string cardName;
    public Sprite cardImage;
    public int level;
    public float multiplier;
    public float multiplier2;
    public string multiplierType;
    public string multiplierType2;
}
