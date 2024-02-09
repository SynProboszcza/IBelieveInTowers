using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MainMenuLoop : MonoBehaviour
{
    void Awake()
    {
        // Doing this because difference in framerate between Editor and 
        // an application causes differences in projectile speed
        //
        // This should be copied inside MainMenuLoop and MainGameLoop
        // (i think(?)(and maybe every scene?))
        QualitySettings.vSyncCount = 0;  // VSync must be disabled
        Application.targetFrameRate = 60;
    }
    private void Start()
    {
        CrossSceneManager.instance.FullReset();
        if (PhotonNetwork.IsConnectedAndReady)
        {
            if (PhotonNetwork.NetworkClientState == Photon.Realtime.ClientState.Joined || PhotonNetwork.NetworkClientState == Photon.Realtime.ClientState.JoinedLobby)
            {
                PhotonNetwork.LeaveRoom();
            }
        }
    } 
}
