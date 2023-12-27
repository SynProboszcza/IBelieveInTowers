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
        if (!gameObject.transform.parent.gameObject.GetComponent<MainTurret>().LevelUp())
        {
            ShowRedCrossForNSeconds(secondsToShowNegativeFeedback);
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
