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
    public int defaultManaAmount = 2000;
    public int defaultMatchDurationSeconds = 180;
    public int goldForDamageMultiplier = 20;
    public float delayFirstSpawnSeconds = 3;
    [Header("Messages")]
    public string roundWon = "You won the round, nice!";
    public string roundLost = "You lost the round, prepare for next one";
    public string matchWon = "You won the match, congratulations!";
    public string matchLost = "You lost the match, keep trying :)";
    [Header("Prefabs to use")]
    public GameObject showPriceCostPrefab;
    public GameObject showHealthChangePrefab;
    public GameObject bearPrefab;
    public GameObject bettlePrefab;
    public GameObject opossumPrefab;
    public GameObject dinoPrefab;
    public GameObject slimerPrefab;
    [Header("Units prices")]
    public int bearPrice = 500; 
    public int bettlePrice = 100; 
    public int opossumPrice = 350; 
    public int dinoPrice = 250; 
    public int slimerPrice = 200;
    [Header("Predetermined maps?")]
    public bool areMapsPredetermined = false;
    public string firstMiddleName = "";
    public string secondMiddleName = "";
    public string thirdMiddleName = "";


}
