using Photon.Pun;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PreMainGame : MonoBehaviourPunCallbacks
{
    public Toggle readyToggle;
    public TMP_Text textfieldLobby;
    public TMP_Text textfieldRoom;
    public TMP_Text textfieldRegion;
    public TMP_Text textfieldMyNickName;
    public TMP_Text textfieldEnemyNickName;
    void Start()
    {
        
    }

    void Update()
    {
        // Check for readiness
        if(readyToggle.isOn)
        {
            readyToggle.transform.Find("Label").GetComponent<Text>().color = Color.green;
        } else
        {
            readyToggle.transform.Find("Label").GetComponent<Text>().color = Color.red;
        }
        if (PhotonNetwork.IsConnectedAndReady)
        {
            // print(PhotonNetwork.CurrentLobby.Type.ToString());
            // print(PhotonNetwork.CurrentRoom);
            // print(PhotonNetwork.CloudRegion);
            // print(PhotonNetwork.NickName);
            // print(PhotonNetwork.IsMasterClient);
            RefreshTextfields(PhotonNetwork.CurrentLobby.Type.ToString(), PhotonNetwork.CurrentRoom.Name.ToString(), PhotonNetwork.CloudRegion, PhotonNetwork.NickName, "XxX_Crusher_XxX"); // Nick is to be pulled from room.specificinfo or sth
        }
    }

    // PhotonNetwork is not set(?) 
    public void RefreshTextfields(string _lobbyName, string _roomName, string _regionName, string _nickName, string _enemyNickName)
    {
        textfieldLobby.GetComponent<TMP_Text>().text = "Lobby: " + _lobbyName;
        textfieldRoom.GetComponent<TMP_Text>().text = "Room: " + _roomName;
        textfieldRegion.GetComponent<TMP_Text>().text = "Region: " + _regionName;
        textfieldMyNickName.GetComponent<TMP_Text>().text = "Me: " + _nickName;
        textfieldEnemyNickName.GetComponent<TMP_Text>().text = "Enemy: " + _enemyNickName;
    }
}
