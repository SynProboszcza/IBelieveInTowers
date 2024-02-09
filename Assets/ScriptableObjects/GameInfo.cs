using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "GameInfo", order = 2, fileName = "GameInformation")]
public class GameInfo : ScriptableObject
{
    [SerializeField]
    public int amountOfMaps = 5; 
}
