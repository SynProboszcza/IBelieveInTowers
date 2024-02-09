using ExitGames.Client.Photon;
using Photon.Pun;
using Photon.Realtime;
//using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

// Simple script to add to a random object and set up 
// connection to a room, so everything about PhotonNetwork works
public class SimpleConnect : MonoBehaviourPunCallbacks
{
    [SerializeField]
    [TextArea]
    [Tooltip("Doesn't do anything. Just comments shown in inspector")]
    public string Notes = "This components disables MainGame(maingame does it also), sets choosen defender/attacker state, then activates MainGame. MainGame activates proper part on its own. AttackerPart and DefenderPart should always be disabled in editor and only enabled by MainGame.";
    public string backupNickNamePrefix = "defaultNickname";
    public string gameVersion = "0.1";
    public string roomName = "SIMPLEroom";
    public string nickName = "SIMPLEnick";
    public int matchDurationSeconds = 1800; // 30mins
    [Header("Setup first, then read-only; for gameplay change tick it in CrossSceneManager")]
    public bool amIDefending = false;
    public bool isMoneyInfinite = false;
    public bool isManaInfinite = false;
    public bool invincibleTurrets = false;
    //private bool specialRules = false;
    public GameObject mainGameScript;

    private void Start()
    {
        mainGameScript.SetActive(false);
        CrossSceneManager.instance.FullReset();
        CrossSceneManager.instance.currentMatchMaxTime = matchDurationSeconds;
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
        CrossSceneManager.instance.amIMaster = PhotonNetwork.IsMasterClient;
        CrossSceneManager.instance.amIDefender = amIDefending;
        CrossSceneManager.instance.isMoneyInfinite = isMoneyInfinite;
        CrossSceneManager.instance.isManaInfinite = isManaInfinite;
        CrossSceneManager.instance.invincibleTurrets = invincibleTurrets;
        mainGameScript.SetActive(true);
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
