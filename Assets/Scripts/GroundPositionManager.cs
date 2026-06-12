using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class GroundPositionManager : MonoBehaviour
{
    public static List<Vector3> groundPositions =
        new List<Vector3>();

    public Tilemap groundTilemap;

    void Start()
    {
        groundPositions.Clear();

        BoundsInt bounds = groundTilemap.cellBounds;

        foreach (Vector3Int pos in bounds.allPositionsWithin)
        {
            if (groundTilemap.HasTile(pos))
            {
                groundPositions.Add(
                    groundTilemap.GetCellCenterWorld(pos)
                );
            }
        }
    }

    public static Vector3 GetRandomGroundPosition()
    {
        if (groundPositions.Count == 0)
            return Vector3.zero;

        return groundPositions[
            Random.Range(0, groundPositions.Count)
        ];
    }
}