using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class UpdateMoneyAndMana : MonoBehaviour
{
    [SerializeField]
    private TMP_Text moneyText;
    [SerializeField]
    private TMP_Text manaText;

    void Update()
    {
        UpdateManaAndMoney();
    }

    public void UpdateManaAndMoney()
    {
        moneyText.text = "GOLD: " + CrossSceneManager.instance.playerMoney;
        manaText.text = "MANA: " + CrossSceneManager.instance.playerMana;
    }
}
