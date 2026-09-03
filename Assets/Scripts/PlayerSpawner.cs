using UnityEngine;
using Photon.Pun;
using System.Collections;
using System.Collections.Generic;

public class PlayerSpawner : MonoBehaviour
{
    [Header("Spawn Settings")]
    [SerializeField]
    private int minRoomScore = 8;

    [SerializeField]
    private float retryDelay = 0.15f;

    [SerializeField]
    private float npcSearchRadius = 8f;

    [Header("Tag Settings")]
    [SerializeField]
    private string groundTag = "Ground";

    IEnumerator Start()
    {
        // 1. Chờ kết nối Photon Room thành công
        while (!PhotonNetwork.InRoom)
        {
            yield return null;
        }

        // 2. Chờ hệ thống sinh Map (Dungeon/Ground Generator) hoàn tất việc tạo Tilemap
        GroundPositionManager manager = FindObjectOfType<GroundPositionManager>();
        
        while (manager == null || GroundPositionManager.groundPositions == null || GroundPositionManager.groundPositions.Count == 0)
        {
            manager = FindObjectOfType<GroundPositionManager>();
            if (manager != null)
            {
                GroundPositionManager.ClearProtectedGroundAreas();
                manager.RefreshGroundPositions();
            }

            yield return new WaitForSeconds(retryDelay);
        }
        // Chờ Dungeon Generate hoàn toàn
while (!AbstractDungeonGenerator.GenerationFinished)
{
    yield return null;
}

// Chờ thêm 1 frame để Tilemap cập nhật xong
yield return null;

manager.RefreshGroundPositions();

        // 3. Tiến hành spawn Player khi Map đã sẵn sàng
        SpawnPlayer();
    }

    void SpawnPlayer()
    {
        GroundPositionManager manager = FindObjectOfType<GroundPositionManager>();
        if (manager != null)
        {
            manager.RefreshGroundPositions();
        }

        // --- Tìm vị trí Spawn ưu tiên ---
        Vector3 spawnPos = GetPreferredSpawnPosition();
        Debug.Log($"PlayerSpawner: Preferred spawn candidate = {spawnPos}");

        // Kiểm tra Fallback 1: Inner Ground
        if (spawnPos == Vector3.zero || !IsPositionOnGround(spawnPos))
        {
            Debug.LogWarning("PlayerSpawner: Preferred invalid. Chuyển sang inner ground hợp lệ.");
            spawnPos = GroundPositionManager.GetInnerGroundPosition(minRoomScore, 3);
        }

        // Kiểm tra Fallback 2: Random Ground
        if (spawnPos == Vector3.zero || !IsPositionOnGround(spawnPos))
        {
            Debug.LogWarning("PlayerSpawner: Inner ground không tìm được. Dùng random ground hợp lệ.");
            spawnPos = GetRandomGroundTilePosition();
        }

        // Kiểm tra Fallback 3: Any Ground từ Manager
        if (spawnPos == Vector3.zero || !IsPositionOnGround(spawnPos))
        {
            Debug.LogWarning("PlayerSpawner: Random ground hợp lệ không tìm được. Dùng ground bất kỳ.");
            if (GroundPositionManager.groundPositions != null && GroundPositionManager.groundPositions.Count > 0)
            {
                spawnPos = GroundPositionManager.groundPositions[Random.Range(0, GroundPositionManager.groundPositions.Count)];
            }
        }

        // Kiểm tra Fallback 4: Trung tâm Map
        if (spawnPos == Vector3.zero || !IsPositionOnGround(spawnPos))
        {
            Debug.LogWarning("PlayerSpawner: Fallback sang trung tâm map do spawn không hợp lệ.");
            spawnPos = GetGroundCenterPosition();
        }

        // Nếu vẫn thất bại -> Báo lỗi
        if (spawnPos == Vector3.zero || !IsPositionOnGround(spawnPos))
        {
            Debug.LogError("PlayerSpawner: Khong tim duoc vi tri spawn hop le tren Tilemap Ground.");
            return;
        }

        // Snap về tâm ô Tile và đặt Z = 0
        spawnPos = SnapToGroundTileCenter(spawnPos);
        spawnPos.z = 0f;

        // Đăng ký vùng an toàn không spawn quái/bẫy đè lên Player
        GroundPositionManager.RegisterProtectedGroundArea(spawnPos, 7f);
        Debug.Log($"PlayerSpawner: Spawn player thành công tại {spawnPos} trên ground tile.");

        // Lấy Class đã chọn và Spawn qua Photon Network
        string selectedClass = PlayerPrefs.GetString("MySelectedClass", "Class_Warrior");

GameObject player = PhotonNetwork.Instantiate(
    selectedClass,
    spawnPos,
    Quaternion.identity
);

// Đánh dấu đây là Player của máy hiện tại
PhotonNetwork.LocalPlayer.TagObject = player;

// Báo cho Camera theo Player luôn
CameraFollowLimited cam = Camera.main.GetComponent<CameraFollowLimited>();

if (cam != null)
{
    cam.targetPlayer = player.transform;
}

Debug.Log("<color=green>PlayerSpawner: Camera đã theo Player.</color>");
Debug.Log("Ground Count = " + GroundPositionManager.groundPositions.Count);}

    #region Position Checks & Finders

    /// <summary>
    /// Kiểm tra vị trí World Position đó có nằm trên Tilemap có Tag "Ground" hay không
    /// </summary>
    private bool IsPositionOnGround(Vector3 position)
    {
        GroundPositionManager manager = FindObjectOfType<GroundPositionManager>();

        if (manager == null || manager.groundTilemap == null)
        {
            return false;
        }

        // 1. Kiểm tra GameObject của Tilemap có đúng Tag "Ground" hay không
        if (!manager.groundTilemap.gameObject.CompareTag(groundTag))
        {
            Debug.LogWarning($"Tilemap GameObject chưa được gắn Tag '{groundTag}'!");
            return false;
        }

        // 2. Chuyển World Position sang Cell Position của Tilemap
        Vector3Int cellPosition = manager.groundTilemap.WorldToCell(position);

        // 3. Kiểm tra xem ô Tile này có dữ liệu đất không
        return manager.groundTilemap.HasTile(cellPosition);
    }

    private Vector3 GetPreferredSpawnPosition()
    {
        GroundPositionManager manager = FindObjectOfType<GroundPositionManager>();
        if (manager != null)
        {
            manager.RefreshGroundPositions();
        }

        if (GroundPositionManager.groundPositions == null || GroundPositionManager.groundPositions.Count == 0)
        {
            return Vector3.zero;
        }

        Vector3 randomGround = GetRandomGroundTilePosition();
        if (randomGround != Vector3.zero)
        {
            return randomGround;
        }

        if (GroundPositionManager.groundPositions.Count > 0)
        {
            return GroundPositionManager.groundPositions[Random.Range(0, GroundPositionManager.groundPositions.Count)];
        }

        return Vector3.zero;
    }

    private Vector3 GetRandomGroundTilePosition()
    {
        return GroundPositionManager.GetRandomGroundPosition(minRoomScore, true);
    }

    private Vector3 GetGroundCenterPosition()
    {
        if (GroundPositionManager.groundPositions == null || GroundPositionManager.groundPositions.Count == 0)
        {
            return Vector3.zero;
        }

        Vector3 sum = Vector3.zero;
        foreach (Vector3 position in GroundPositionManager.groundPositions)
        {
            sum += position;
        }

        return sum / GroundPositionManager.groundPositions.Count;
    }

    private Vector3 SnapToGroundTileCenter(Vector3 worldPosition)
    {
        GroundPositionManager manager = FindObjectOfType<GroundPositionManager>();
        if (manager == null || manager.groundTilemap == null)
        {
            return worldPosition;
        }

        Vector3Int cellPosition = manager.groundTilemap.WorldToCell(worldPosition);
        if (manager.groundTilemap.HasTile(cellPosition))
        {
            return manager.groundTilemap.GetCellCenterWorld(cellPosition);
        }

        return worldPosition;
    }

    #endregion
}