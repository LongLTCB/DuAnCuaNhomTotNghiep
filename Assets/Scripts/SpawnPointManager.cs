using UnityEngine;
using System.Linq;

public class SpawnPointManager : MonoBehaviour
{
    public static Vector2 GetRandomFloorPosition()
    {
        if (AbstractDungeonGenerator.FloorPositions == null ||
            AbstractDungeonGenerator.FloorPositions.Count == 0)
        {
            return Vector2.zero;
        }

        var list = AbstractDungeonGenerator.FloorPositions.ToList();

        Vector2Int pos =
            list[Random.Range(0, list.Count)];

        return new Vector2(pos.x, pos.y);
    }
}