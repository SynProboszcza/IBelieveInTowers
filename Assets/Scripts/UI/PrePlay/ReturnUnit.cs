using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class ReturnUnit : MonoBehaviour
{
    public void ReturnBear()
    {
        int price = CrossSceneManager.instance.bearPrice;
        CrossSceneManager.instance.AddMoney(price);
        Destroy(gameObject);
    }
    public void ReturnBettle()
    {
        int price = CrossSceneManager.instance.bettlePrice;
        CrossSceneManager.instance.AddMoney(price);
        Destroy(gameObject);
    }
    public void ReturnDino()
    {
        int price = CrossSceneManager.instance.dinoPrice;
        CrossSceneManager.instance.AddMoney(price);
        Destroy(gameObject);
    }
    public void ReturnOpossum()
    {
        int price = CrossSceneManager.instance.opossumPrice;
        CrossSceneManager.instance.AddMoney(price);
        Destroy(gameObject);
    }
    public void ReturnSlimer()
    {
        int price = CrossSceneManager.instance.slimerPrice;
        CrossSceneManager.instance.AddMoney(price);
        Destroy(gameObject);
    }

}
