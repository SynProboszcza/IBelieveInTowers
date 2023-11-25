using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Manager/GameSettings")]

public class GameSettings : ScriptableObject
{

    [SerializeField]
    private string _gameVersion = "0.1";
    public string gameVersion { get { return _gameVersion; } }
    [SerializeField]
    private string _nickname = "defaultNickname";
    public string nickname
    {
        get
        {
            int randomValue = Random.Range(0, 55555);
            return _nickname + randomValue.ToString();
        }
    }
}
