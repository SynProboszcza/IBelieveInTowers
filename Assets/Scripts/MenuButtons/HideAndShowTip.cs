using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HideAndShowTip : MonoBehaviour
{
    public GameObject tipGameObject;
    /// <summary>
    /// Look for any rooms and if there are any, show tip
    /// </summary>
    void Update()
    {
        if(transform.childCount > 0)
        {
            tipGameObject.SetActive(true);
        } else
        {
            tipGameObject.SetActive(false);
        }
    }
}
