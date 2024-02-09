using UnityEngine;
using UnityEngine.SceneManagement;

public class ReturnUnit : MonoBehaviour
{
    public bool canReturnUnit = true; // This needs to be false during a round - not in premaingame

    private void Start()
    {
        // Bare-bones detection if instantiated during round or during
        if (SceneManager.GetActiveScene().name.Equals("PreparingToPlayAsAttacker") || SceneManager.GetActiveScene().name.Equals("InBetweenScene"))
        {
            canReturnUnit = true;
        } else
        {
            canReturnUnit = false;
        }
    }
    public void ReturnBear()
    {
        if (canReturnUnit)
        {
            int price = CrossSceneManager.instance.bearPrice;
            CrossSceneManager.instance.AddMoney(price);
            Destroy(gameObject);
        }
    }
    public void ReturnBettle()
    {
        if (canReturnUnit)
        {
            int price = CrossSceneManager.instance.bettlePrice;
            CrossSceneManager.instance.AddMoney(price);
            Destroy(gameObject);
        }
    }
    public void ReturnDino()
    {
        if (canReturnUnit)
        {
            int price = CrossSceneManager.instance.dinoPrice;
            CrossSceneManager.instance.AddMoney(price);
            Destroy(gameObject);
        }
    }
    public void ReturnOpossum()
    {
        if (canReturnUnit)
        {
            int price = CrossSceneManager.instance.opossumPrice;
            CrossSceneManager.instance.AddMoney(price);
            Destroy(gameObject);
        }
    }
    public void ReturnSlimer()
    {
        if (canReturnUnit)
        {
            int price = CrossSceneManager.instance.slimerPrice;
            CrossSceneManager.instance.AddMoney(price);
            Destroy(gameObject);
        }
    }

}
