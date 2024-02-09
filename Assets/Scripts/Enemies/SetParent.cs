using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class SetParent : MonoBehaviour
{
    public string tagToSetParenthoodTo;
    void Awake()
    {
        this.transform.SetParent(GameObject.FindGameObjectWithTag(tagToSetParenthoodTo).transform);
    }

    public void SetParentOfThisGO(GameObject parentGameObject)
    {
        this.transform.SetParent(parentGameObject.transform);
    }

}
