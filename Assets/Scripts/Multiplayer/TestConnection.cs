using Photon.Pun;
using Photon.Pun.Demo.Cockpit;
using Photon.Realtime;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestConnection : MonoBehaviourPunCallbacks
{
    public string nickNamePrefix = "defaultNickname";
    public string gameVersion = "0.1";
    void Start()
    {
        print("Connecting to server...");
        nickNamePrefix += Random.Range(0, 55555).ToString();
        PhotonNetwork.NickName = nickNamePrefix;
        PhotonNetwork.GameVersion = gameVersion;
        PhotonNetwork.ConnectUsingSettings();
    }

    public override void OnConnectedToMaster()
    {
        print("Rejoice! We connected!");
        print(PhotonNetwork.LocalPlayer.NickName);
    }

    public override void OnDisconnected(DisconnectCause cause)
    {
        print("Disconnected because: " + cause.ToString());
        base.OnDisconnected(cause);
    }
}
