using UnityEngine;
using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class BossSpawner : MonoBehaviourPunCallbacks
{
    [Header("Boss Settings")]
    public string bossPrefabName = "GolemPhase3";
    public Vector3 bossSpawnPosition = new Vector3(0f, 0f, 0f);
    public float spawnDelay = 2f;
    public bool spawnOnStart = true;
    
    private bool bossSpawned = false;
    private GameObject bossInstance;

    void Start()
    {
        if (spawnOnStart)
        {
            StartCoroutine(SpawnBossWhenReady());
        }
    }

    private IEnumerator SpawnBossWhenReady()
    {
        while (GroundPositionManager.groundPositions == null || GroundPositionManager.groundPositions.Count == 0)
        {
            yield return null;
        }

        yield return new WaitForSeconds(spawnDelay);
        InitializeSpawnPositionFromMap();
        SpawnBoss();
    }

    public void InitializeSpawnPositionFromMap()
    {
        Vector3 mapSpawnPosition = GetBossSpawnPositionFromMap();
        if (mapSpawnPosition != Vector3.zero)
        {
            bossSpawnPosition = mapSpawnPosition;
        }
    }

    private Vector3 GetBossSpawnPositionFromMap()
    {
        GroundPositionManager groundManager = FindObjectOfType<GroundPositionManager>();
        if (groundManager != null && GroundPositionManager.groundPositions != null && GroundPositionManager.groundPositions.Count > 0)
        {
            return GetBossSpawnPositionFromGroundPositions(GroundPositionManager.groundPositions);
        }

        return GetBossSpawnPositionFromDungeon();
    }

   private Vector3 GetBossSpawnPositionFromGroundPositions(List<Vector3> groundPositions)
{
    if (groundPositions == null || groundPositions.Count == 0)
        return Vector3.zero;

    Vector3 bestPosition = groundPositions[0];
    int bestScore = -1;

    foreach (Vector3 pos in groundPositions)
    {
        int score = CountNearbyGround(pos, groundPositions);

        if (score > bestScore)
        {
            bestScore = score;
            bestPosition = pos;
        }
    }

    return new Vector3(bestPosition.x, bestPosition.y, 0f);
}
private int CountNearbyGround(Vector3 center, List<Vector3> grounds)
{
    int count = 0;

    foreach (Vector3 p in grounds)
    {
        if (Vector2.Distance(center, p) <= 4f)
        {
            count++;
        }
    }

    return count;
}

    private Vector3 GetBossSpawnPositionFromDungeon()
    {
        HashSet<Vector2Int> floorPositions = AbstractDungeonGenerator.FloorPositions;
        if (floorPositions == null || floorPositions.Count == 0)
        {
            return Vector3.zero;
        }

        Vector2 center = Vector2.zero;
        foreach (Vector2Int position in floorPositions)
        {
            center += new Vector2(position.x, position.y);
        }
        center /= floorPositions.Count;

        Vector2Int closestFloor = floorPositions
            .OrderBy(pos => (new Vector2(pos.x, pos.y) - center).sqrMagnitude)
            .First();

        return new Vector3(closestFloor.x + 0.5f, closestFloor.y + 0.5f, 0f);
    }

    IEnumerator SpawnBossAfterDelay()
    {
        yield return new WaitForSeconds(spawnDelay);
        InitializeSpawnPositionFromMap();
        SpawnBoss();
    }

    public void SpawnBoss()
    {
        InitializeSpawnPositionFromMap();

        if (bossSpawned)
        {
            Debug.LogWarning("Boss đã được spawn rồi!");
            return;
        }

        if (!IsPrefabAvailableInResources(bossPrefabName))
        {
            Debug.LogError($"BossSpawner: Prefab '{bossPrefabName}' không tìm thấy. Hãy đặt prefab vào Resources/bossPrefabName.prefab");
            return;
        }

        if (PhotonNetwork.InRoom)
        {
            if (!PhotonNetwork.IsMasterClient)
            {
                return;
            }

            bossInstance = PhotonNetwork.InstantiateRoomObject(
                bossPrefabName,
                bossSpawnPosition,
                Quaternion.identity
            );
        }
        else
        {
            bossInstance = Instantiate(
                Resources.Load<GameObject>(bossPrefabName),
                bossSpawnPosition,
                Quaternion.identity
            );
        }

        bossSpawned = true;
        Debug.Log($"Boss đã spawn tại vị trí: {bossSpawnPosition}");
    }

    /// <summary>
    /// Kiểm tra xem prefab có trong thư mục Resources không
    /// </summary>
    private bool IsPrefabAvailableInResources(string prefabName)
    {
        GameObject prefab = Resources.Load<GameObject>(prefabName);
        return prefab != null;
    }

    /// <summary>
    /// Lấy vị trí boss hiện tại (nếu đã spawn)
    /// </summary>
    public GameObject GetBossInstance()
    {
        return bossInstance;
    }

    /// <summary>
    /// Kiểm tra xem boss có còn sống không
    /// </summary>
    public bool IsBossAlive()
    {
        return bossInstance != null && bossSpawned;
    }
}
