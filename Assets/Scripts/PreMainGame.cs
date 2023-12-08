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
            string _enemyNickName = "Connecting...";
            if (PhotonNetwork.IsMasterClient)
            {
                _enemyNickName = PhotonNetwork.CurrentRoom.CustomProperties["roomCreatorNickname"].ToString();
            } else
            {
                _enemyNickName = PhotonNetwork.CurrentRoom.CustomProperties["roomJoinedNickname"].ToString();
            }
            RefreshTextfields(PhotonNetwork.CurrentLobby.Type.ToString(), PhotonNetwork.CurrentRoom.Name.ToString(), PhotonNetwork.CloudRegion, PhotonNetwork.NickName, _enemyNickName);
        }
    }

    public void RefreshTextfields(string _lobbyName, string _roomName, string _regionName, string _nickName, string _enemyNickName)
    {
        textfieldLobby.GetComponent<TMP_Text>().text = "Lobby: " + _lobbyName;
        textfieldRoom.GetComponent<TMP_Text>().text = "Room: " + _roomName;
        textfieldRegion.GetComponent<TMP_Text>().text = "Region: " + _regionName;
        textfieldMyNickName.GetComponent<TMP_Text>().text = "Me: " + _nickName;
        textfieldEnemyNickName.GetComponent<TMP_Text>().text = "Enemy: " + _enemyNickName;
    }
}
