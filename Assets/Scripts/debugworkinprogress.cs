using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class debugworkinprogress : MonoBehaviourPunCallbacks
{
    private int frames = 0;
    void Start()
    {
        print("State: " + PhotonNetwork.NetworkClientState);
    }

    // Update is called once per frame
    void Update()
    {
        frames++;
        if (frames % 10 == 0)
        {
            print("State: " + PhotonNetwork.NetworkClientState);
            frames = 0;
        }
    }

    public void OnClickCheck()
    {
        PhotonNetwork.LeaveRoom();
    }

}
