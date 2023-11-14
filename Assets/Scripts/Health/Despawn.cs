using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class Despawn : MonoBehaviour
{
    public GameObject mainGame;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        mainGame.GetComponent<MainGameLoop>().TakePlayerDamage(collision.gameObject.GetComponent<Enemy>().GetDamage());
        //mainGame.GetComponent<MainGameLoop>().AddPlayerMoney(collision.gameObject.GetComponent<Enemy>().GetMoneyReward());
        Destroy(collision.gameObject);
    }
}
