using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "GameInfo", order = 2, fileName = "GameInformation")]
public class GameInfo : ScriptableObject
{
    [TextArea]
    [Tooltip("Doesn't do anything. Just comments shown in inspector")]
    public string Notes = "To modify enemies or turrets change individual prefabs in Assets/Resources";
    [Header("General settings")]
    public int amountOfMaps = 5;
    public int defaultDefenderHealth = 300;
    public int defaultMoneyAmount = 2000;
    public int defaultMatchDurationSeconds = 180;
    public int goldForDamageMultiplier = 20;
    public float delayFirstSpawnSeconds = 3;
    [Header("Messages")]
    public string roundWon = "You won the round, nice!";
    public string roundLost = "You lost the round, prepare for next one";
    public string matchWon = "You won the match, congratulations!";
    public string matchLost = "You lost the match, keep trying :)";

}
