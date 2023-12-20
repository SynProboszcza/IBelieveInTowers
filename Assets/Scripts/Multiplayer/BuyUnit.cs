using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class BuyUnit : MonoBehaviour
{
    [SerializeField]
    private GameObject enemyList;
    [SerializeField]
    private GameObject enemyPFPPrefab;
    [SerializeField]
    private int unitPrice = 200;

    private void Start()
    {
        SetUnitPrice(unitPrice);
    }

    public void AddEnemyToList()
    {
        Instantiate(enemyPFPPrefab, enemyList.transform);
    }

    public void SetUnitPrice(int _price)
    {
        transform.Find("Price").GetComponent<TMP_Text>().text = _price.ToString() + " G";
    }
}
