using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using System.Security;
using UnityEngine;

public class MultiUpgradeTurret : MonoBehaviour
{
    private SpriteRenderer sr;
    public int secondsToShowNegativeFeedback = 1;
    void Start()
    {
        sr = gameObject.GetComponent<SpriteRenderer>();
        sr.enabled = false;
    }

    private void OnMouseDown()
    {
        // Check: if player can afford && upgradeLevel is < 3
        // Cost is inherited from MainTurret parent object
        //  if yes: send rpc to upgrade
        //  if no : show negative feedback
        if (CrossSceneManager.instance.amIDefender) // only defender should upgrade turrets
        {
            if (CrossSceneManager.instance.CanPlayerAffordWithMoney(gameObject.transform.parent.gameObject.GetComponent<MainTurret>().upgradeCost)
                && gameObject.transform.parent.gameObject.GetComponent<MainTurret>().upgradeLevel < 3)
            {
                gameObject.transform.parent.gameObject.GetComponent<PhotonView>().RPC("LevelUp", RpcTarget.All);
            } else
            {
                ShowRedCrossForNSeconds(secondsToShowNegativeFeedback);
                // Those two ifs are just debug, safe to comment out
                if (!CrossSceneManager.instance.CanPlayerAffordWithMoney(gameObject.transform.parent.gameObject.GetComponent<MainTurret>().upgradeCost)) { print("Not enough money to upgrade"); }
                if (gameObject.transform.parent.gameObject.GetComponent<MainTurret>().upgradeLevel >= 3) { print("Turret already at max level"); }
            }
        }
    }

    private void OnMouseOver()
    {
        // We need to disable the gameObject with mask so other "ranges" dont get blocked
        transform.parent.transform.Find("Range").Find("RangeCenter").gameObject.SetActive(true);
        transform.parent.transform.Find("Range").gameObject.GetComponent<SpriteRenderer>().enabled = true;
    }

    private void OnMouseExit()
    {
        transform.parent.transform.Find("Range").Find("RangeCenter").gameObject.SetActive(false);
        transform.parent.transform.Find("Range").gameObject.GetComponent<SpriteRenderer>().enabled = false;
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
