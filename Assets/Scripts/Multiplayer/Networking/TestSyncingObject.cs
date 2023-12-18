using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class TestSyncingObject : MonoBehaviourPunCallbacks
{
    public override void OnEnable()
    {
        gameObject.transform.SetParent(GameObject.FindGameObjectWithTag("MainCanvas").transform);

    }
}
