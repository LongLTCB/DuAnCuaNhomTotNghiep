using UnityEngine;
using Photon.Pun;
using Photon.Realtime; 

public class NetworkManager : MonoBehaviourPunCallbacks 
{
    void Start()
    {
        Debug.Log("1. Đang kết nối tới Server Photon...");
        PhotonNetwork.ConnectUsingSettings(); 
    }

    public override void OnConnectedToMaster()
    {
        Debug.Log("2. Đã vào Server! Đang tìm phòng...");
        PhotonNetwork.JoinOrCreateRoom("PhongTest", new RoomOptions { MaxPlayers = 4 }, TypedLobby.Default);
    }

    public override void OnJoinedRoom()
    {
        // XÓA DÒNG ĐẺ NHÂN VẬT Ở ĐÂY ĐI, NHƯỜNG VIỆC ĐÓ CHO PLAYER SPAWNER
        Debug.Log("3. Đã vào phòng thành công! Đang chờ PlayerSpawner gọi nhân vật ra...");
    }
}