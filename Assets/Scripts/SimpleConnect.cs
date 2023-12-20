using Photon.Pun;
using Photon.Realtime;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

// Simple script to add to a random object and set up 
// connection to a room, so everything about PhotonNetwork works
public class SimpleConnect : MonoBehaviourPunCallbacks
{
    public string backupNickNamePrefix = "defaultNickname";
    public string gameVersion = "0.1";
    public string roomName = "SIMPLEroom";
    public string nickName = "SIMPLEnick";

    private void Start()
    {
        BeginConnecting();
    }

    private void BeginConnecting()
    {
        print("SIMPLE Trying to connect");
        PhotonNetwork.GameVersion = gameVersion;
        PhotonNetwork.NickName = nickName;
        PhotonNetwork.ConnectUsingSettings();
    }

    public override void OnConnectedToMaster()
    {
        print("SIMPLE Connected to master, trying to join lobby");
        PhotonNetwork.JoinLobby(TypedLobby.Default);
        base.OnConnectedToMaster();
    }

    public override void OnJoinedLobby()
    {
        print("SIMPLE Joined Lobby: " + PhotonNetwork.CurrentLobby.ToString() + "; trying to create and join room: " + roomName);
        PhotonNetwork.CreateRoom(roomName, new RoomOptions(), TypedLobby.Default);
        base.OnJoinedLobby();
    }

    public override void OnJoinedRoom()
    {
        print("SIMPLE Joined room: " + roomName);
        base.OnJoinedRoom();
    }

    public override void OnCreateRoomFailed(short returnCode, string message)
    {
        print("Failed to create room!");
        base.OnCreateRoomFailed(returnCode, message);
    }

    public override void OnDisconnected(DisconnectCause cause)
    {
        print("Disconnected because: " + cause.ToString());
        base.OnDisconnected(cause);
    }



}
