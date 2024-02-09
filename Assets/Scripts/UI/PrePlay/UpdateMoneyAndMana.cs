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
        if (CrossSceneManager.instance.isMoneyInfinite)
        {
            moneyText.text = "GOLD: inf";
            if (CrossSceneManager.instance.isManaInfinite)
            {
                //manaText.text = "MANA: inf";
                manaText.text = "";
            } else
            {
                //manaText.text = "MANA: " + CrossSceneManager.instance.playerMana;
                manaText.text = "";
            }
        }
        else
        {
            moneyText.text = "GOLD: " + CrossSceneManager.instance.playerMoney;
            if (CrossSceneManager.instance.isManaInfinite)
            {
                //manaText.text = "MANA: inf";
                manaText.text = "";
            } else
            {
                //manaText.text = "MANA: " + CrossSceneManager.instance.playerMana;
                manaText.text = "";
            }
        }
    }
}
