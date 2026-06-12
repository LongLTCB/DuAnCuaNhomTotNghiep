using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class TrinhTaoTuong : MonoBehaviour
{
    public static void CreateWalls(HashSet<Vector2Int> floorPositions, TilemapVisualizer tilemapVisualizer)
    {
        PaintWallGroup(tilemapVisualizer, FindWallCandidates(floorPositions, Direction2D.cardinalDirectionsList), floorPositions, Direction2D.cardinalDirectionsList, tilemapVisualizer.PaintSingleBasicWall);
        PaintWallGroup(tilemapVisualizer, FindWallCandidates(floorPositions, Direction2D.diagonalDirectionsList), floorPositions, Direction2D.eightDirectionsList, tilemapVisualizer.PaintSingleCornerWall);
    }

    private static void PaintWallGroup(
        TilemapVisualizer tilemapVisualizer,
        HashSet<Vector2Int> wallPositions,
        HashSet<Vector2Int> floorPositions,
        IList<Vector2Int> neighbourDirections,
        Action<Vector2Int, string> painter)
    {
        foreach (var position in wallPositions)
        {
            painter(position, BuildNeighbourMask(position, floorPositions, neighbourDirections));
        }
    }

    private static string BuildNeighbourMask(Vector2Int centerPosition, HashSet<Vector2Int> floorPositions, IList<Vector2Int> directions)
    {
        char[] bits = new char[directions.Count];
        for (int index = 0; index < directions.Count; index++)
        {
            bits[index] = floorPositions.Contains(centerPosition + directions[index]) ? '1' : '0';
        }

        return new string(bits);
    }

    private static HashSet<Vector2Int> FindWallCandidates(HashSet<Vector2Int> floorPositions, IEnumerable<Vector2Int> directionList)
    {
        var wallPositions = new HashSet<Vector2Int>();
        foreach (var position in floorPositions)
        {
            foreach (var direction in directionList)
            {
                var neighbourPosition = position + direction;
                if (floorPositions.Contains(neighbourPosition) == false)
                    wallPositions.Add(neighbourPosition);
            }
        }
        return wallPositions;
    }
}

