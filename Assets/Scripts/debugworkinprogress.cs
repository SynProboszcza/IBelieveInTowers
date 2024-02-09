using Photon.Pun;
using Photon.Realtime;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class debugworkinprogress : MonoBehaviourPunCallbacks
{
    public TMP_Text showState;

    // Update is called once per frame
    void Update()
    {
        if (PhotonNetwork.NetworkClientState.Equals(ClientState.PeerCreated) ||
            PhotonNetwork.NetworkClientState.Equals(ClientState.Authenticating) ||
            PhotonNetwork.NetworkClientState.Equals(ClientState.ConnectingToNameServer) ||
            PhotonNetwork.NetworkClientState.Equals(ClientState.ConnectedToNameServer))
        {
            showState.GetComponent<TMP_Text>().text = "State: Connecting...";
        } 
        else if 
           (PhotonNetwork.NetworkClientState.Equals(ClientState.JoinedLobby))
        {
            showState.GetComponent<TMP_Text>().text = "State: Ready";
        }
        else if 
           (PhotonNetwork.NetworkClientState.Equals(ClientState.ConnectWithFallbackProtocol) ||
            PhotonNetwork.NetworkClientState.Equals(ClientState.ConnectWithoutAuthOnceWss))
        {
            showState.GetComponent<TMP_Text>().text = "State: Connecting failed, retrying...";
        }
        else
        {
            showState.GetComponent<TMP_Text>().text = "State: " + PhotonNetwork.NetworkClientState;
        }
    }

    public void OnClickCheck()
    {
        PhotonNetwork.LeaveRoom();
    }


    /*
    Connecting...
    {
        PeerCreated,
        Authenticating,
        JoiningLobby,
        ConnectingToNameServer,
        ConnectedToNameServer,
    }
    Conected and ready
    {
        JoinedLobby,
    }
    Connecting failed, retrying...
    {
        ConnectWithFallbackProtocol,
        ConnectWithoutAuthOnceWss
    }
    Disconnecting
    {
        Disconnecting,
        DisconnectingFromNameServer,
    }
    Disconnected
    {
        Disconnected,
    }
    Joining...
    {
        Joining,
        Joined,  
    }
    Leaving
    {
        Leaving,
    }     
     */

}
