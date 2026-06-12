using UnityEngine;
using Photon.Pun;
using System.Collections;

public class EnemySpawner : MonoBehaviourPunCallbacks
{
    [Header("Spawner")]
    public string enemyPrefabName = "Dummy";

    public float spawnInterval = 5f;

    public int maxEnemies = 5;

    void Start()
    {
        StartCoroutine(SpawnEnemyRoutine());
    }

    IEnumerator SpawnEnemyRoutine()
    {
        while (true)
        {
            yield return new WaitForSeconds(spawnInterval);

            if (PhotonNetwork.InRoom &&
                PhotonNetwork.IsMasterClient)
            {
                GameObject[] currentEnemies =
                    GameObject.FindGameObjectsWithTag("Enemy");

                if (currentEnemies.Length < maxEnemies)
                {
                    SpawnEnemy();
                }
            }
        }
    }

    void SpawnEnemy()
    {
        Vector3 spawnPos =
            GroundPositionManager.GetRandomGroundPosition();

        PhotonNetwork.InstantiateRoomObject(
            enemyPrefabName,
            spawnPos,
            Quaternion.identity);
    }
}