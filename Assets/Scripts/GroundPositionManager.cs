using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class GroundPositionManager : MonoBehaviour
{
    public static List<Vector3> groundPositions =
        new List<Vector3>();

    private static readonly List<ProtectedGroundArea> protectedGroundAreas =
        new List<ProtectedGroundArea>();

    [SerializeField]
    private float roomDetectionRadius = 3.5f;

    private static float cachedRoomDetectionRadius = 3.5f;

    public Tilemap groundTilemap;

    private struct ProtectedGroundArea
    {
        public Vector3 position;
        public float radius;

        public ProtectedGroundArea(Vector3 position, float radius)
        {
            this.position = position;
            this.radius = radius;
        }
    }

    void Start()
    {
        cachedRoomDetectionRadius = roomDetectionRadius;
        RefreshGroundPositions();
    }

    public void RefreshGroundPositions()
    {
        groundPositions.Clear();

        if (groundTilemap == null)
        {
            return;
        }

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

    public static void RegisterProtectedGroundArea(Vector3 position, float radius)
    {
        protectedGroundAreas.Add(new ProtectedGroundArea(position, radius));
    }

    public static void ClearProtectedGroundAreas()
    {
        protectedGroundAreas.Clear();
    }

    public static Vector3 GetRandomGroundPosition()
    {
        return GetRandomGroundPosition(0, true);
    }

    public static Vector3 GetRandomGroundPosition(int minRoomScore, bool avoidProtectedAreas)
    {
        if (groundPositions.Count == 0)
        {
            GroundPositionManager manager = FindObjectOfType<GroundPositionManager>();
            if (manager != null)
            {
                cachedRoomDetectionRadius = manager.roomDetectionRadius;
                manager.RefreshGroundPositions();
            }
        }

        if (groundPositions.Count == 0)
            return Vector3.zero;

        List<Vector3> filteredPositions = new List<Vector3>();

        foreach (Vector3 position in groundPositions)
        {
            if (avoidProtectedAreas && IsInsideProtectedArea(position))
            {
                continue;
            }

            if (GetRoomScore(position) < minRoomScore)
            {
                continue;
            }

            filteredPositions.Add(position);
        }

        if (filteredPositions.Count == 0)
            return Vector3.zero;

        return filteredPositions[
            Random.Range(0, filteredPositions.Count)
        ];
    }

    private static bool IsInsideProtectedArea(Vector3 position)
    {
        foreach (ProtectedGroundArea protectedArea in protectedGroundAreas)
        {
            if (Vector3.Distance(position, protectedArea.position) <= protectedArea.radius)
            {
                return true;
            }
        }

        return false;
    }

    private static int GetRoomScore(Vector3 position)
    {
        int score = 0;
        float radiusSqr = cachedRoomDetectionRadius * cachedRoomDetectionRadius;

        foreach (Vector3 candidate in groundPositions)
        {
            if ((candidate - position).sqrMagnitude <= radiusSqr)
            {
                score++;
            }
        }

        return score;
    }
}