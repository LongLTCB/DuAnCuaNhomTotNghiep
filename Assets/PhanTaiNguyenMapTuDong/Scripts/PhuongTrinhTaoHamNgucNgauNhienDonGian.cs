using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Random = UnityEngine.Random;

public class PhuongTrinhTaoHamNgucNgauNhienDonGian : AbstractDungeonGenerator
{
   

    [SerializeField]
    protected SimpleRandomWalkSO randomWalkParameters;


    protected override void RunProceduralGeneration()
    {
        HashSet<Vector2Int> floorPositions = RunRandomWalk(randomWalkParameters, startPosition);
        tilemapVisualizer.Clear();
        tilemapVisualizer.PaintFloorTiles(floorPositions);
        WallGenerator.CreateWalls(floorPositions, tilemapVisualizer);
    }
    protected HashSet<Vector2Int> RunRandomWalk(SimpleRandomWalkSO parameters, Vector2Int position)
    {
        var currentPosition = position;
        HashSet<Vector2Int> floorPositions = new HashSet<Vector2Int>();
        List<Vector2Int> floorList = new List<Vector2Int>();
        for (int i = 0; i < parameters.iterations; i++)
        {
            var path = ThuatToanTaoMap.SimpleRandomWalk(currentPosition, parameters.walkLength);
            foreach (var pos in path)
            {
                if (floorPositions.Add(pos))
                    floorList.Add(pos);
            }

            if (parameters.startRandomlyEachIteration)
            {
                currentPosition = floorPositions.ElementAt(Random.Range(0, floorPositions.Count));
            }
        }

        return floorPositions;
    }

    
}