using UnityEngine;
using Photon.Pun;
using System.Collections;
using System.Collections.Generic;

public class EnemySpawner : MonoBehaviourPunCallbacks
{
    [Header("Spawner")]
    public string enemyPrefabName = "Dummy";

    [System.Serializable]
    private class EnemySpawnEntry
    {
        public string prefabName = "Dummy";
        public float weight = 1f;
        public int minRoomScore = 0;
    }

    [SerializeField]
    private List<EnemySpawnEntry> enemySpawnEntries = new List<EnemySpawnEntry>();

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
        EnemySpawnEntry selectedEntry = GetRandomEnemySpawnEntry();
        if (selectedEntry == null)
        {
            return;
        }

        if (!IsPrefabAvailableInResources(selectedEntry.prefabName))
        {
            Debug.LogError($"EnemySpawner: Prefab '{selectedEntry.prefabName}' khong load duoc. Hay dat prefab trong thu muc Resources va dung dung ten prefab.");
            return;
        }

        Vector3 spawnPos =
            GroundPositionManager.GetRandomGroundPosition(selectedEntry.minRoomScore, true);

        if (spawnPos == Vector3.zero && GroundPositionManager.groundPositions.Count == 0)
        {
            return;
        }

        if (spawnPos == Vector3.zero)
        {
            return;
        }

        PhotonNetwork.InstantiateRoomObject(
            selectedEntry.prefabName,
            spawnPos,
            Quaternion.identity);
    }

    private EnemySpawnEntry GetRandomEnemySpawnEntry()
    {
        List<EnemySpawnEntry> entries = new List<EnemySpawnEntry>();

        if (enemySpawnEntries != null)
        {
            foreach (EnemySpawnEntry entry in enemySpawnEntries)
            {
                if (entry != null && !string.IsNullOrEmpty(entry.prefabName))
                {
                    entries.Add(entry);
                }
            }
        }

        if (entries.Count == 0 && !string.IsNullOrEmpty(enemyPrefabName))
        {
            entries.Add(new EnemySpawnEntry
            {
                prefabName = enemyPrefabName,
                weight = 1f,
                minRoomScore = 0
            });
        }

        if (entries.Count == 0)
        {
            return null;
        }

        float totalWeight = 0f;
        foreach (EnemySpawnEntry entry in entries)
        {
            totalWeight += Mathf.Max(0.01f, entry.weight);
        }

        float randomValue = Random.Range(0f, totalWeight);
        float cumulativeWeight = 0f;

        foreach (EnemySpawnEntry entry in entries)
        {
            cumulativeWeight += Mathf.Max(0.01f, entry.weight);
            if (randomValue <= cumulativeWeight)
            {
                return entry;
            }
        }

        return entries[entries.Count - 1];
    }

    private bool IsPrefabAvailableInResources(string prefabName)
    {
        if (string.IsNullOrEmpty(prefabName))
        {
            return false;
        }

        return Resources.Load<GameObject>(prefabName) != null;
    }
}