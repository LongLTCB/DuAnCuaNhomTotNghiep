using UnityEngine;
using Photon.Pun;
using System.Collections;

public class PlayerSpawner : MonoBehaviour
{
    [Header("Điểm Sinh Ra (Tùy chọn)")]
    public Transform spawnPoint; 

    IEnumerator Start()
    {
        // Chờ đến khi vào phòng xong
        while (!PhotonNetwork.InRoom)
        {
            yield return null; 
        }

        SpawnPlayer();
    }

    void SpawnPlayer()
    {
        Vector2 spawnPos = Vector2.zero;

        if (spawnPoint != null)
        {
            spawnPos = spawnPoint.position;
        }

        // Random nhẹ để không đè lên đầu nhau
        float randomX = Random.Range(-9f, -4f);
        float randomY = Random.Range(-9f, -4f);
        spawnPos += new Vector2(randomX, randomY);

        // Đọc tên Class
        string selectedClass = PlayerPrefs.GetString("MySelectedClass", "Class_Warrior");

        // ĐÃ SỬA: Dùng đúng biến spawnPos để đẻ nhân vật
        PhotonNetwork.Instantiate(selectedClass, spawnPos, Quaternion.identity); 
    }
}