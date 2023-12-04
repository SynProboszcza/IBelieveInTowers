using Photon.Pun;
using Photon.Realtime;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UpdateRoomList : MonoBehaviourPunCallbacks
{
    [SerializeField]
    private GameObject asd;

    public override void OnRoomListUpdate(List<RoomInfo> roomList)
    {
        print(roomList);
        base.OnRoomListUpdate(roomList);
    }
}
