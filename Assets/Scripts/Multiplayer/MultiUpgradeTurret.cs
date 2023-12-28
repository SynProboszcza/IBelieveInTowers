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
        // Check if player can bear the cost?? or do it better somehow
        // Now levelup already tries to pay, and returns bool
        // rpc is sent correctly from parent object, so i should check locally and 
        // send rpc that set the upgrade level for other player
        if (!gameObject.transform.parent.gameObject.GetComponent<MainTurret>().LevelUp())
        {
            ShowRedCrossForNSeconds(secondsToShowNegativeFeedback);
        } else
        {
            gameObject.transform.parent.gameObject.GetComponent<PhotonView>().RPC("LevelUp", RpcTarget.All);

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
