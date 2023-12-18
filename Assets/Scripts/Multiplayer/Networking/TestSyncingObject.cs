using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class TestSyncingObject : MonoBehaviourPunCallbacks
{
    public override void OnEnable()
    {
        // Set parent Canvas so you can display it
        gameObject.transform.SetParent(GameObject.FindWithTag("MainCanvas").transform);
    }
}
