using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class debugworkinprogress : MonoBehaviourPunCallbacks
{
    private int frames = 0;
    public TMP_Text showState;
    void Start()
    {
        print("State: " + PhotonNetwork.NetworkClientState);
    }

    // Update is called once per frame
    void Update()
    {
        frames++;
        //if (frames % 10 == 0)
        if (true)
        {
            //print("State: " + PhotonNetwork.NetworkClientState);
            showState.GetComponent<TMP_Text>().text = "State: " + PhotonNetwork.NetworkClientState;
            frames = 0;
        }
    }

    public void OnClickCheck()
    {
        PhotonNetwork.LeaveRoom();
    }

}
