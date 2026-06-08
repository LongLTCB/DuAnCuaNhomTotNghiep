using UnityEngine;
using Photon.Pun;
using System.Collections;

public class PointSpawner : MonoBehaviourPun
{
    [Header("Cài đặt Hồi sinh")]
    public string enemyPrefabName = "Dummy"; // Tên quái trong Resources
    public float respawnTime = 10f;          // Thời gian hồi sinh (10 giây)
    
    private GameObject currentEnemy;         // Theo dõi con quái hiện tại
    private bool isRespawning = false;

    void Start()
    {
        // Vừa vào game, Chủ phòng lập tức gọi quái ra
        if (PhotonNetwork.IsMasterClient)
        {
            SpawnEnemy();
        }
    }

    void Update()
    {
        if (!PhotonNetwork.IsMasterClient) return;

        // Nếu phát hiện con quái đã biến mất (bị giết) và chưa bắt đầu đếm giờ
        if (currentEnemy == null && !isRespawning)
        {
            StartCoroutine(RespawnRoutine());
        }
    }

    IEnumerator RespawnRoutine()
    {
        isRespawning = true;
        yield return new WaitForSeconds(respawnTime); // Đợi 10 giây
        
        SpawnEnemy(); // Gọi con mới
        isRespawning = false;
    }

    void SpawnEnemy()
    {
        currentEnemy = PhotonNetwork.InstantiateRoomObject(enemyPrefabName, transform.position, Quaternion.identity);
    }

    // Vẽ 1 cục xanh lá cây ngoài Scene để bạn biết chỗ nào quái sẽ mọc ra
    void OnDrawGizmos()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, 0.5f);
    }
}