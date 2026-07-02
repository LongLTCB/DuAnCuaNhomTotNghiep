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

        if (miniMapFullMapAutoFit != null)
        {
            miniMapFullMapAutoFit.FitToGroundNow();
        }
    }
}