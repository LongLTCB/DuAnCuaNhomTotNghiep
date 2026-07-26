using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class WallGenerator
{
    public static void CreateWalls(HashSet<Vector2Int> floorPositions, TilemapVisualizer tilemapVisualizer)
    {
        var cardinalDirectionsList = new List<Vector2Int>
        {
            new Vector2Int(0, 1),  // Top
            new Vector2Int(1, 0),  // Right
            new Vector2Int(0, -1), // Bottom
            new Vector2Int(-1, 0)  // Left
        };

        var basicWallPositions = FindWallsInDirections(floorPositions, cardinalDirectionsList);
        foreach (var position in basicWallPositions)
        {
            var binaryType = BuildNeighbourBinary(position, floorPositions);
            tilemapVisualizer.PaintSingleBasicWall(position, binaryType);
        }
    }

    private static HashSet<Vector2Int> FindWallsInDirections(HashSet<Vector2Int> floorPositions, List<Vector2Int> directionList)
    {
        HashSet<Vector2Int> wallPositions = new HashSet<Vector2Int>();
        foreach (var position in floorPositions)
        {
            foreach (var direction in directionList)
            {
                var neighbourPosition = position + direction;
                if (floorPositions.Contains(neighbourPosition) == false)
                {
                    wallPositions.Add(neighbourPosition);
                }
            }
        }
        return wallPositions;
    }

    // Builds a 4-bit binary string representing neighbouring floor tiles
    // Order: Top, Right, Bottom, Left
    private static string BuildNeighbourBinary(Vector2Int position, HashSet<Vector2Int> floorPositions)
    {
        var top = floorPositions.Contains(position + new Vector2Int(0, 1)) ? '1' : '0';
        var right = floorPositions.Contains(position + new Vector2Int(1, 0)) ? '1' : '0';
        var bottom = floorPositions.Contains(position + new Vector2Int(0, -1)) ? '1' : '0';
        var left = floorPositions.Contains(position + new Vector2Int(-1, 0)) ? '1' : '0';

        return string.Concat(top, right, bottom, left);
    }
}