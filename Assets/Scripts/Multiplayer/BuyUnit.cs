using System.Collections;
using TMPro;
using UnityEngine;

public class BuyUnit : MonoBehaviour
{
    [SerializeField]
    private GameObject spawnerMultiplayer;
    [SerializeField]
    private GameObject enemyList;
    [SerializeField]
    private GameObject enemyPFPPrefab;
    [SerializeField]
    public int unitPrice
    { get; private set; }
    [SerializeField]
    private float timeNegativeFeedbackSeconds = 0.35f;
    private Color firstPriceColor;

    void Start()
    {
        unitPrice = CrossSceneManager.instance.enemyPrices[gameObject.name];
        SetUnitPrice(unitPrice);
        firstPriceColor = transform.Find("Price").GetComponent<TMP_Text>().color;
    }

    public void AddEnemyToList()
    {
        if (CrossSceneManager.instance.PayWithMoney(unitPrice))
        {
            GameObject tmp = Instantiate(enemyPFPPrefab, enemyList.transform);
            //tmp.GetComponent<UnitStatistics>().spawnTime = enemyPFPPrefab.GetComponent<MultiplayerEnemy>().spawnTime;
            if (spawnerMultiplayer != null) // For PreMainGame
            {
                spawnerMultiplayer.GetComponent<SpawnerMultiplayer>().AddEnemyToSpawnQueue(tmp);
            }
            //GameObject PFP = Instantiate(enemyPFPPrefab, enemyList.transform);
        }
        else
        {
            SetPriceToRedForNSeconds(timeNegativeFeedbackSeconds);
        }

    }

    public void SetUnitPrice(int _price)
    {
        transform.Find("Price").GetComponent<TMP_Text>().text = _price.ToString() + " G";
    }

    private void SetPriceToRedForNSeconds(float seconds)
    {
        transform.Find("Price").GetComponent<TMP_Text>().color = Color.red;
        StartCoroutine(SetPriceColorBack(seconds));
    }

    IEnumerator SetPriceColorBack(float seconds)
    {
        yield return new WaitForSeconds(seconds);
        transform.Find("Price").GetComponent<TMP_Text>().color = firstPriceColor;
    }


}
