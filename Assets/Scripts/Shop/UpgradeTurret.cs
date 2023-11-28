using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UpgradeTurret : MonoBehaviour
{
    private GameObject mainGame;
    private SpriteRenderer sr;
    private int upgradeCost = 100;
    public int secondsToShowNegativeFeedback = 1;
    void Start()
    {
        mainGame = GameObject.FindWithTag("SingleTagForMainGameLoop");
        sr = gameObject.GetComponent<SpriteRenderer>();
        sr.enabled = false;
    }
    public void SetUpgradeCost(int cost)
    {
        this.upgradeCost = cost;
    }

    private void OnMouseDown()
    {
        // Added check because in main menu there is no mainGame gameobject
        // and this generated errors for free; this way we disable upgrading
        // turrets in main menu
        if(mainGame != null)
        {
            // If player can afford 
            // If turret can be upgraded (3 is max upgrade)
            // Try to pay
            // Check if upgraded and log (for now)
            if (mainGame.GetComponent<MainGameLoop>().CanPlayerBearCost(upgradeCost)) 
            {
                if (transform.parent.gameObject.GetComponent<MainTurret>().GetUpgradeLevel() < 3)
                {
                    if (mainGame.GetComponent<MainGameLoop>().PayWithPlayerMoney(upgradeCost))
                    {
                        if(transform.parent.gameObject.GetComponent<MainTurret>().LevelUp())
                        {
                            Debug.Log("Upgraded");
                        } else
                        {
                            Debug.Log("NOT Upgraded");
                            ShowRedCrossForNSeconds(secondsToShowNegativeFeedback);
                        }
                    } else
                    {
                        Debug.Log("We checked and player could afford, and then not");
                        ShowRedCrossForNSeconds(secondsToShowNegativeFeedback);
                    }
                } else
                {
                    Debug.Log("Turret already at max level");
                    ShowRedCrossForNSeconds(secondsToShowNegativeFeedback);
                }
            } else
            {
                Debug.Log("Player can't afford to upgrade");
                ShowRedCrossForNSeconds(secondsToShowNegativeFeedback);
            }
        }
    }

    private void ShowRedCrossForNSeconds(int seconds)
    {
        sr.enabled = true;
        StartCoroutine(DisableRedCrossAfterNSeconds(seconds));
    }

    IEnumerator DisableRedCrossAfterNSeconds(int seconds)
    {
        yield return new WaitForSeconds(seconds);
        sr.enabled = false;
    }

    public void SetSecondsToShowNotUpgrading(int seconds)
    {
        this.secondsToShowNegativeFeedback = seconds;
    }
}
