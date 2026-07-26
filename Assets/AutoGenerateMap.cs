using UnityEngine;

public class AutoGenerateMap : MonoBehaviour
{
    public AbstractDungeonGenerator dungeon;
    public GroundPositionManager groundPositionManager;
    public MiniMapFullMapAutoFit miniMapFullMapAutoFit;

    void Start()
    {
        dungeon.GenerateDungeon();

        if (groundPositionManager != null)
        {
            groundPositionManager.RefreshGroundPositions();
        }

        BossSpawner bossSpawner = FindObjectOfType<BossSpawner>();
        if (bossSpawner != null)
        {
            bossSpawner.InitializeSpawnPositionFromMap();
        }

        if (miniMapFullMapAutoFit != null)
        {
            miniMapFullMapAutoFit.FitToGroundNow();
        }
    }
}