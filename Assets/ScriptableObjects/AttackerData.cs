using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Attacker", order = 1, fileName = "AttackerDataInstance")]
public class AttackerData : ScriptableObject
{
    [SerializeField]
    public int moneyPerSecond = 15;
}
