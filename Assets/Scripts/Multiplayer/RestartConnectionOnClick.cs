using Photon.Pun;
using Photon.Realtime;
using System.Collections;
using System.Collections.Generic;
using System.Net.Sockets;
using UnityEngine;
using UnityEngine.UI;

public class RestartConnectionOnClick : MonoBehaviourPunCallbacks
{
    [SerializeField]
    private Button createGameButton;
    [SerializeField]
    private GameObject roomList;
    public void RestartConnection()
    {
        PhotonNetwork.Disconnect();
        gameObject.GetComponent<Button>().interactable = false;
        createGameButton.GetComponent<Button>().interactable = false;
        print("Connecting to server...");
        PhotonNetwork.ConnectUsingSettings();
    }

    public override void OnConnectedToMaster()
    {
        gameObject.GetComponent<Button>().interactable = true;
        createGameButton.GetComponent<Button>().interactable = true;
        base.OnConnectedToMaster();
    }
}
