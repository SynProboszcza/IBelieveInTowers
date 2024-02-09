using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LeaveRoom : MonoBehaviour
{
    public void ClickInterceptToLeaveRoom()
    {
        print("Disconnecting with room: " + PhotonNetwork.CurrentRoom.Name.ToString());
        PhotonNetwork.LeaveRoom();
        SceneManager.LoadScene("HostGame");
    }

}
