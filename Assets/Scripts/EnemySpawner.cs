using UnityEngine;
using Photon.Pun;
using System.Collections;

public class EnemySpawner : MonoBehaviourPunCallbacks
{
    [Header("Cài đặt Spawner")]
    public string enemyPrefabName = "Dummy"; // Tên chính xác của Quái trong thư mục Resources
    public float spawnInterval = 5f;         
    public int maxEnemies = 5;               
    
    [Header("Khu vực sinh quái (Hình chữ nhật)")]
    // X là Chiều rộng, Y là Chiều cao của khu vực
    public Vector2 spawnArea = new Vector2(6f, 4f); 

    void Start()
    {
        // Khởi động máy đếm thời gian ngay lập tức
        StartCoroutine(SpawnEnemyRoutine());
    }

    IEnumerator SpawnEnemyRoutine()
    {
        while (true)
        {
            // Chờ đủ thời gian (Ví dụ 5 giây)
            yield return new WaitForSeconds(spawnInterval);

            // Chỉ khi nào ĐÃ VÀO PHÒNG và ĐANG LÀ CHỦ PHÒNG thì mới được mở cửa thả quái
            if (PhotonNetwork.InRoom && PhotonNetwork.IsMasterClient)
            {
                // Đếm số quái hiện tại (Đảm bảo Prefab quái phải được gắn Tag "Enemy")
                GameObject[] currentEnemies = GameObject.FindGameObjectsWithTag("Enemy");
                
                if (currentEnemies.Length < maxEnemies)
                {
                    SpawnEnemy();
                }
            }
        }
    }

    void SpawnEnemy()
    {
        // Tung xúc xắc tìm một điểm ngẫu nhiên trong phạm vi Rộng và Cao của hình chữ nhật
        float randomX = Random.Range(-spawnArea.x / 2f, spawnArea.x / 2f);
        float randomY = Random.Range(-spawnArea.y / 2f, spawnArea.y / 2f);
        
        Vector3 spawnPos = transform.position + new Vector3(randomX, randomY, 0);

        // Gọi con quái từ thư mục Resources ra
        PhotonNetwork.InstantiateRoomObject(enemyPrefabName, spawnPos, Quaternion.identity);
    }

    void OnDrawGizmos()
    {
        // Vẽ một cái khung chữ nhật màu đỏ ngoài Scene để bro dễ căn chỉnh map
        Gizmos.color = Color.red;
        Gizmos.DrawWireCube(transform.position, new Vector3(spawnArea.x, spawnArea.y, 0));
    }
}